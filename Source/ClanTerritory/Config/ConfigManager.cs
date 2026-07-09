using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using ClanTerritory.Utils;
using UnityEngine;

namespace ClanTerritory.Config
{
    internal static class ConfigManager
    {
        public static ConfigEntry<float> TerritoryRadius;
        public static ConfigEntry<bool> AllowOverlap;
        public static ConfigEntry<int> DoorAutoCloseSeconds;
        public static ConfigEntry<bool> DebugMode;
        public static ConfigEntry<string> LocalizationLanguage;

        public static void Initialize(ConfigFile config)
        {
            TerritoryRadius = config.Bind(
                "Territory",
                "Radius",
                100f,
                new ConfigDescription(
                    "Territory radius in meters.",
                    new AcceptableValueRange<float>(50f, 200f)));

            AllowOverlap = config.Bind(
                "Territory",
                "AllowOverlap",
                false,
                "Allow territories to overlap.");

            DoorAutoCloseSeconds = config.Bind(
                "Territory",
                "DoorAutoCloseSeconds",
                5,
                new ConfigDescription(
                    "Seconds before locked territory doors close automatically after being opened. Used only when territory door lock is enabled.",
                    new AcceptableValueRange<int>(3, 10)));

            LocalizationLanguage = config.Bind(
                "Localization",
                "Language",
                "auto",
                "Language code for Clan Territory UI. Use 'auto', 'en', 'ru', or any custom language pack file name from BepInEx/plugins/ClanTerritory/Localization/<code>.txt.");

            DebugMode = config.Bind(
                "Debug",
                "Enabled",
                false,
                "Enable debug logging.");

            ClanTerritory.Localization.CtLocalization.Reset();
        }
    }
}

namespace ClanTerritory.Localization
{
    internal static class CtLocalization
    {
        private const string DefaultLanguage = "en";
        private const string RussianLanguage = "ru";

        private static readonly object SyncRoot = new object();

        private static Dictionary<string, string> _texts;
        private static string _loadedLanguage;

        public static string CurrentLanguage
        {
            get
            {
                EnsureLoaded();
                return _loadedLanguage ?? DefaultLanguage;
            }
        }

        public static void Reset()
        {
            lock (SyncRoot)
            {
                _texts = null;
                _loadedLanguage = null;
            }
        }

        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            EnsureLoaded();

            string value;

            if (_texts != null && _texts.TryGetValue(key, out value))
                return value;

            return key;
        }

        public static string Format(string key, params object[] args)
        {
            string template = Get(key);

            if (args == null || args.Length == 0)
                return template;

            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }

        private static void EnsureLoaded()
        {
            string language = ResolveLanguageCode();

            if (_texts != null &&
                string.Equals(_loadedLanguage, language, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            lock (SyncRoot)
            {
                language = ResolveLanguageCode();

                if (_texts != null &&
                    string.Equals(_loadedLanguage, language, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                Dictionary<string, string> texts = BuildEnglishDefaults();

                if (!string.Equals(language, DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    Merge(
                        texts,
                        BuildLanguageDefaults(language));
                }

                EnsureDefaultLanguageFiles();

                Merge(
                    texts,
                    ReadLanguageFile(DefaultLanguage));

                if (!string.Equals(language, DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    Merge(
                        texts,
                        ReadLanguageFile(language));
                }

                _texts = texts;
                _loadedLanguage = language;

                ModLog.Info("[Localization] Loaded language: " + language + ", keys: " + texts.Count);
            }
        }

        private static string ResolveLanguageCode()
        {
            string configured =
                ClanTerritory.Config.ConfigManager.LocalizationLanguage != null
                    ? ClanTerritory.Config.ConfigManager.LocalizationLanguage.Value
                    : "auto";

            configured = (configured ?? "auto").Trim();

            if (!string.IsNullOrEmpty(configured) &&
                !string.Equals(configured, "auto", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeLanguageCode(configured);
            }

            string detectedLanguage;
            string normalizedLanguage;

            if (TryResolveValheimLanguage(out detectedLanguage))
            {
                normalizedLanguage = NormalizeLanguageCode(detectedLanguage);

                if (!string.IsNullOrEmpty(normalizedLanguage))
                    return normalizedLanguage;
            }

            if (TryResolvePlayerPrefsLanguage(out detectedLanguage))
            {
                normalizedLanguage = NormalizeLanguageCode(detectedLanguage);

                if (!string.IsNullOrEmpty(normalizedLanguage))
                    return normalizedLanguage;
            }

            normalizedLanguage = NormalizeLanguageCode(
                Application.systemLanguage.ToString());

            return !string.IsNullOrEmpty(normalizedLanguage)
                ? normalizedLanguage
                : DefaultLanguage;
        }

        private static bool TryResolveValheimLanguage(out string language)
        {
            language = null;

            try
            {
                Type localizationType = FindLoadedType("Localization");

                if (localizationType == null)
                    return false;

                object instance = GetStaticMemberValue(
                    localizationType,
                    "instance");

                if (TryInvokeLanguageMethod(
                        localizationType,
                        instance,
                        "GetSelectedLanguage",
                        out language))
                {
                    return true;
                }

                if (TryInvokeLanguageMethod(
                        localizationType,
                        instance,
                        "GetCurrentLanguage",
                        out language))
                {
                    return true;
                }

                if (TryInvokeLanguageMethod(
                        localizationType,
                        instance,
                        "GetLanguage",
                        out language))
                {
                    return true;
                }

                if (TryReadLanguageMember(
                        localizationType,
                        instance,
                        "m_selectedLanguage",
                        out language))
                {
                    return true;
                }

                if (TryReadLanguageMember(
                        localizationType,
                        instance,
                        "m_currentLanguage",
                        out language))
                {
                    return true;
                }

                if (TryReadLanguageMember(
                        localizationType,
                        instance,
                        "m_language",
                        out language))
                {
                    return true;
                }

                if (TryReadLanguageMember(
                        localizationType,
                        instance,
                        "m_languageName",
                        out language))
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                ModLog.Debug("[Localization] Valheim language detection failed: " + exception.Message);
            }

            return false;
        }

        private static bool TryResolvePlayerPrefsLanguage(out string language)
        {
            language = null;

            try
            {
                string[] keys =
                {
                    "language",
                    "Language",
                    "selected_language",
                    "SelectedLanguage",
                    "localization_language",
                    "LocalizationLanguage"
                };

                for (int i = 0; i < keys.Length; i++)
                {
                    string key = keys[i];

                    if (!PlayerPrefs.HasKey(key))
                        continue;

                    string value = PlayerPrefs.GetString(key, "");

                    if (!string.IsNullOrEmpty(value))
                    {
                        language = value;
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                ModLog.Debug("[Localization] PlayerPrefs language detection failed: " + exception.Message);
            }

            return false;
        }

        private static Type FindLoadedType(string typeName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];

                if (assembly == null)
                    continue;

                Type type = assembly.GetType(typeName);

                if (type != null)
                    return type;
            }

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];

                if (assembly == null)
                    continue;

                Type[] types = GetSafeTypes(assembly);

                for (int t = 0; t < types.Length; t++)
                {
                    Type type = types[t];

                    if (type == null)
                        continue;

                    if (string.Equals(
                            type.Name,
                            typeName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return type;
                    }
                }
            }

            return null;
        }

        private static Type[] GetSafeTypes(Assembly assembly)
        {
            if (assembly == null)
                return new Type[0];

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                List<Type> types = new List<Type>();

                if (exception.Types != null)
                {
                    for (int i = 0; i < exception.Types.Length; i++)
                    {
                        if (exception.Types[i] != null)
                            types.Add(exception.Types[i]);
                    }
                }

                return types.ToArray();
            }
            catch
            {
                return new Type[0];
            }
        }

        private static object GetStaticMemberValue(
            Type type,
            string name)
        {
            if (type == null || string.IsNullOrEmpty(name))
                return null;

            BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static;

            PropertyInfo property = type.GetProperty(
                name,
                flags);

            if (property != null)
            {
                try
                {
                    return property.GetValue(null, null);
                }
                catch
                {
                }
            }

            FieldInfo field = type.GetField(
                name,
                flags);

            if (field != null)
            {
                try
                {
                    return field.GetValue(null);
                }
                catch
                {
                }
            }

            return null;
        }

        private static bool TryInvokeLanguageMethod(
            Type type,
            object instance,
            string methodName,
            out string language)
        {
            language = null;

            if (type == null || string.IsNullOrEmpty(methodName))
                return false;

            BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.Instance;

            MethodInfo method = type.GetMethod(
                methodName,
                flags,
                null,
                Type.EmptyTypes,
                null);

            if (method == null)
                return false;

            object target = method.IsStatic
                ? null
                : instance;

            if (target == null && !method.IsStatic)
                return false;

            try
            {
                object raw = method.Invoke(
                    target,
                    null);

                return TryExtractLanguageString(
                    raw,
                    out language);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadLanguageMember(
            Type type,
            object instance,
            string memberName,
            out string language)
        {
            language = null;

            if (type == null || string.IsNullOrEmpty(memberName))
                return false;

            BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.Instance;

            PropertyInfo property = type.GetProperty(
                memberName,
                flags);

            if (property != null)
            {
                try
                {
                    MethodInfo getter = property.GetGetMethod(true);
                    object target = getter != null && getter.IsStatic
                        ? null
                        : instance;

                    if (target != null || getter == null || getter.IsStatic)
                    {
                        object raw = property.GetValue(
                            target,
                            null);

                        if (TryExtractLanguageString(
                                raw,
                                out language))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                }
            }

            FieldInfo field = type.GetField(
                memberName,
                flags);

            if (field != null)
            {
                try
                {
                    object target = field.IsStatic
                        ? null
                        : instance;

                    if (target != null || field.IsStatic)
                    {
                        object raw = field.GetValue(target);

                        if (TryExtractLanguageString(
                                raw,
                                out language))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        private static bool TryExtractLanguageString(
            object value,
            out string language)
        {
            language = null;

            if (value == null)
                return false;

            string direct = value as string;

            if (!string.IsNullOrEmpty(direct))
            {
                language = direct;
                return true;
            }

            Type type = value.GetType();

            string[] memberNames =
            {
                "Name",
                "name",
                "m_name",
                "Language",
                "language",
                "m_language",
                "Id",
                "id",
                "m_id"
            };

            for (int i = 0; i < memberNames.Length; i++)
            {
                if (TryReadSimpleStringMember(
                        type,
                        value,
                        memberNames[i],
                        out language))
                {
                    return true;
                }
            }

            language = value.ToString();
            return !string.IsNullOrEmpty(language);
        }

        private static bool TryReadSimpleStringMember(
            Type type,
            object instance,
            string memberName,
            out string value)
        {
            value = null;

            if (type == null || instance == null || string.IsNullOrEmpty(memberName))
                return false;

            BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance;

            PropertyInfo property = type.GetProperty(
                memberName,
                flags);

            if (property != null)
            {
                try
                {
                    object raw = property.GetValue(instance, null);

                    if (raw != null)
                    {
                        value = raw.ToString();
                        return !string.IsNullOrEmpty(value);
                    }
                }
                catch
                {
                }
            }

            FieldInfo field = type.GetField(
                memberName,
                flags);

            if (field != null)
            {
                try
                {
                    object raw = field.GetValue(instance);

                    if (raw != null)
                    {
                        value = raw.ToString();
                        return !string.IsNullOrEmpty(value);
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        private static string NormalizeLanguageCode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return DefaultLanguage;

            string normalized = value
                .Trim()
                .Trim('$')
                .Replace('_', '-')
                .ToLowerInvariant();

            if (normalized.StartsWith("settings-language-", StringComparison.Ordinal))
                normalized = normalized.Substring("settings-language-".Length);

            if (normalized.StartsWith("language-", StringComparison.Ordinal))
                normalized = normalized.Substring("language-".Length);

            if (normalized == "ru" ||
                normalized.StartsWith("ru-", StringComparison.Ordinal) ||
                normalized.Contains("russian") ||
                normalized.Contains("рус") ||
                normalized.Contains("russki"))
            {
                return RussianLanguage;
            }

            if (normalized == "en" ||
                normalized.StartsWith("en-", StringComparison.Ordinal) ||
                normalized.Contains("english") ||
                normalized.Contains("англ"))
            {
                return DefaultLanguage;
            }

            if (normalized.Contains("german") || normalized.Contains("deutsch"))
                return "de";

            if (normalized.Contains("french") || normalized.Contains("franc"))
                return "fr";

            if (normalized.Contains("spanish") || normalized.Contains("espan") || normalized.Contains("españ"))
                return "es";

            if (normalized.Contains("portuguese") || normalized.Contains("portug"))
                return "pt";

            if (normalized.Contains("polish") || normalized.Contains("polski"))
                return "pl";

            if (normalized.Contains("czech") || normalized.Contains("cesk") || normalized.Contains("česk"))
                return "cs";

            if (normalized.Contains("turkish") || normalized.Contains("turk"))
                return "tr";

            if (normalized.Contains("chinese") || normalized.Contains("zh"))
                return "zh";

            if (normalized.Contains("japanese") || normalized.Contains("japan"))
                return "ja";

            if (normalized.Contains("korean") || normalized.Contains("korea"))
                return "ko";

            int separator = normalized.IndexOf('-');

            if (separator > 0)
                normalized = normalized.Substring(0, separator);

            return SanitizeLanguageCode(normalized);
        }

        private static string SanitizeLanguageCode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return DefaultLanguage;

            char[] buffer = value.ToCharArray();
            int length = 0;

            for (int i = 0; i < buffer.Length; i++)
            {
                char c = buffer[i];

                if ((c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9'))
                {
                    buffer[length] = c;
                    length++;
                }
            }

            if (length <= 0)
                return DefaultLanguage;

            return new string(buffer, 0, length);
        }

        private static void EnsureDefaultLanguageFiles()
        {
            try
            {
                string directory = GetLocalizationDirectory();

                if (string.IsNullOrEmpty(directory))
                    return;

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                WriteLanguageFileIfMissing(
                    Path.Combine(directory, "en.txt"),
                    BuildEnglishDefaults());

                WriteLanguageFileIfMissing(
                    Path.Combine(directory, "ru.txt"),
                    BuildRussianDefaults());

                string readme = Path.Combine(directory, "README.txt");

                if (!File.Exists(readme))
                {
                    File.WriteAllText(
                        readme,
                        "Clan Territory language packs\n" +
                        "\n" +
                        "Format: key=value\n" +
                        "Use \\n for line breaks.\n" +
                        "Add a new <code>.txt file here and set Localization.Language=<code> in the BepInEx config.\n",
                        System.Text.Encoding.UTF8);
                }
            }
            catch (Exception exception)
            {
                ModLog.Warning("[Localization] Failed to create default language files: " + exception.Message);
            }
        }

        private static void WriteLanguageFileIfMissing(
            string path,
            Dictionary<string, string> values)
        {
            if (File.Exists(path))
                return;

            List<string> lines = new List<string>();
            lines.Add("# Clan Territory language pack");
            lines.Add("# Format: key=value");
            lines.Add("");

            foreach (KeyValuePair<string, string> pair in values)
            {
                lines.Add(pair.Key + "=" + Escape(pair.Value));
            }

            File.WriteAllLines(
                path,
                lines.ToArray(),
                System.Text.Encoding.UTF8);
        }

        private static Dictionary<string, string> ReadLanguageFile(string language)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            try
            {
                string path = Path.Combine(
                    GetLocalizationDirectory(),
                    language + ".txt");

                if (!File.Exists(path))
                    return values;

                string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    line = line.Trim();

                    if (line.StartsWith("#", StringComparison.Ordinal) ||
                        line.StartsWith("//", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int separator = line.IndexOf('=');

                    if (separator <= 0)
                        continue;

                    string key = line.Substring(0, separator).Trim();
                    string value = line.Substring(separator + 1);

                    if (string.IsNullOrEmpty(key))
                        continue;

                    values[key] = Unescape(value);
                }
            }
            catch (Exception exception)
            {
                ModLog.Warning("[Localization] Failed to read language file '" + language + "': " + exception.Message);
            }

            return values;
        }

        private static string GetLocalizationDirectory()
        {
            return Path.Combine(
                Paths.PluginPath,
                "ClanTerritory",
                "Localization");
        }

        private static void Merge(
            Dictionary<string, string> target,
            Dictionary<string, string> source)
        {
            if (target == null || source == null)
                return;

            foreach (KeyValuePair<string, string> pair in source)
                target[pair.Key] = pair.Value;
        }

        private static string Escape(string value)
        {
            if (value == null)
                return "";

            return value
                .Replace("\\", "\\\\")
                .Replace("\r", "")
                .Replace("\n", "\\n");
        }

        private static string Unescape(string value)
        {
            if (value == null)
                return "";

            return value
                .Replace("\\n", "\n")
                .Replace("\\\\", "\\");
        }

        private static Dictionary<string, string> BuildLanguageDefaults(string language)
        {
            return string.Equals(language, RussianLanguage, StringComparison.OrdinalIgnoreCase)
                ? BuildRussianDefaults()
                : new Dictionary<string, string>();
        }

        private static Dictionary<string, string> BuildEnglishDefaults()
        {
            return new Dictionary<string, string>
            {
                { "ct.menu.title.default", "Territory" },
                { "ct.menu.title.guild", "{0} Territory" },
                { "ct.menu.subtitle", "Territory radius: {0} m   |   Protection: {1}" },
                { "ct.menu.tab.overview", "Overview" },
                { "ct.menu.tab.ward", "Ward" },
                { "ct.menu.tab.territory", "Territory" },
                { "ct.menu.tab.terraforming", "Terraforming" },
                { "ct.menu.button.treasury", "Treasury" },
                { "ct.menu.button.close", "Close" },
                { "ct.menu.button.back", "Back" },
                { "ct.menu.button.enable_protection", "Enable Protection" },
                { "ct.menu.button.disable_protection", "Disable Protection" },
                { "ct.menu.button.add_me", "Add Me" },
                { "ct.menu.button.remove_me", "Remove Me" },
                { "ct.menu.button.lock_doors", "Lock Doors" },
                { "ct.menu.button.unlock_doors", "Unlock Doors" },
                { "ct.menu.button.enable_structure_protection", "Enable Structure Protection" },
                { "ct.menu.button.disable_structure_protection", "Disable Structure Protection" },
                { "ct.menu.button.enable_leveling", "Enable Leveling" },
                { "ct.menu.button.disable_leveling", "Disable Leveling" },
                { "ct.menu.button.start_leveling", "Start Leveling" },
                { "ct.menu.button.stop_leveling", "Stop Leveling" },
                { "ct.menu.button.open_preparation", "Open Preparation Chest" },
                { "ct.menu.button.rename_territory", "Rename Territory" },
                { "ct.menu.button.toggle_protection", "Toggle Protection" },
                { "ct.menu.button.hoe_slot", "Hoe Slot" },
                { "ct.menu.button.hoe_set", "Hoe: Set" },
                { "ct.menu.button.pickaxe_slot", "Pickaxe Slot" },
                { "ct.menu.button.pickaxe_set", "Pickaxe: Set" },
                { "ct.menu.button.remove", "Remove" },
                { "ct.menu.overview.title", "Overview" },
                { "ct.menu.ward.title", "Ward Access" },
                { "ct.menu.territory.title", "Territory Settings" },
                { "ct.menu.leveling.title", "Territory Leveling" },
                { "ct.menu.field.territory", "Territory" },
                { "ct.menu.field.ward_id", "Ward ID" },
                { "ct.menu.field.owner", "Owner" },
                { "ct.menu.field.radius", "Territory radius" },
                { "ct.menu.field.protection", "Protection" },
                { "ct.menu.field.your_access", "Your access" },
                { "ct.menu.field.doors", "Doors" },
                { "ct.menu.field.structures", "Structures" },
                { "ct.menu.field.name", "Name" },
                { "ct.menu.field.guild_access", "Guild access" },
                { "ct.menu.field.group_access", "Group access" },
                { "ct.menu.field.permitted_players", "Permitted players" },
                { "ct.menu.field.status", "Status" },
                { "ct.menu.field.target", "Target" },
                { "ct.menu.field.work_radius", "Work radius" },
                { "ct.menu.field.tools", "Tools" },
                { "ct.menu.field.fuel", "Fuel" },
                { "ct.menu.field.stone", "Stone" },
                { "ct.menu.field.scan", "Scan" },
                { "ct.menu.value.enabled", "Enabled" },
                { "ct.menu.value.disabled", "Disabled" },
                { "ct.menu.value.locked", "Locked" },
                { "ct.menu.value.unlocked", "Unlocked" },
                { "ct.menu.value.locked_auto_close", "Locked, auto-close {0}s" },
                { "ct.menu.value.protected", "Protected" },
                { "ct.menu.value.vulnerable", "Vulnerable" },
                { "ct.menu.value.owner", "Owner" },
                { "ct.menu.value.permitted", "Permitted" },
                { "ct.menu.value.guest", "Guest" },
                { "ct.menu.value.empty", "Empty" },
                { "ct.menu.value.ready", "Ready" },
                { "ct.menu.value.running", "Running" },
                { "ct.menu.value.ward_height", "ward height ({0})" },
                { "ct.menu.tool.hoe", "Hoe" },
                { "ct.menu.tool.pickaxe", "Pickaxe" },
                { "ct.menu.tool.axe", "Axe" },
                { "ct.menu.preparation.note", "Preparation opens a real piece_chest_wood container." },
                { "ct.menu.preparation.title", "Leveling Preparation Chest" },
                { "ct.menu.preparation.tools", "Tools" },
                { "ct.menu.preparation.fuel_slots", "Fuel slots, 500 each" },
                { "ct.menu.preparation.stone_slots", "Stone slots, 500 each" },
                { "ct.menu.storage.fuel.short", "Fuel {0}" },
                { "ct.menu.storage.stone.short", "Stone {0}" },
                { "ct.menu.storage.fuel.value", "Fuel {0}\\n{1}/500" },
                { "ct.menu.storage.stone.value", "Stone {0}\\n{1}/500" },
                { "ct.menu.more_players", "... and {0} more" },
                { "ct.message.doors_locked", "Territory doors are locked." },
                { "ct.message.entered_territory", "Entered territory: {0}" },
                { "ct.message.left_territory", "Left territory: {0}" },
                { "ct.territory.unnamed", "Unnamed Territory" },
                { "ct.status.terraforming_unavailable", "Terraforming service unavailable" }
            };
        }

        private static Dictionary<string, string> BuildRussianDefaults()
        {
            return new Dictionary<string, string>
            {
                { "ct.menu.title.default", "Территория" },
                { "ct.menu.title.guild", "Территория {0}" },
                { "ct.menu.subtitle", "Радиус территории: {0} м   |   Защита: {1}" },
                { "ct.menu.tab.overview", "Обзор" },
                { "ct.menu.tab.ward", "Вард" },
                { "ct.menu.tab.territory", "Территория" },
                { "ct.menu.tab.terraforming", "Земля" },
                { "ct.menu.button.treasury", "Казна" },
                { "ct.menu.button.close", "Закрыть" },
                { "ct.menu.button.back", "Назад" },
                { "ct.menu.button.enable_protection", "Включить защиту" },
                { "ct.menu.button.disable_protection", "Выключить защиту" },
                { "ct.menu.button.add_me", "Добавить себя" },
                { "ct.menu.button.remove_me", "Убрать себя" },
                { "ct.menu.button.lock_doors", "Запереть двери" },
                { "ct.menu.button.unlock_doors", "Отпереть двери" },
                { "ct.menu.button.enable_structure_protection", "Включить защиту построек" },
                { "ct.menu.button.disable_structure_protection", "Выключить защиту построек" },
                { "ct.menu.button.enable_leveling", "Включить выравнивание" },
                { "ct.menu.button.disable_leveling", "Выключить выравнивание" },
                { "ct.menu.button.start_leveling", "Запустить выравнивание" },
                { "ct.menu.button.stop_leveling", "Остановить выравнивание" },
                { "ct.menu.button.open_preparation", "Открыть сундук подготовки" },
                { "ct.menu.button.rename_territory", "Переименовать территорию" },
                { "ct.menu.button.toggle_protection", "Переключить защиту" },
                { "ct.menu.button.hoe_slot", "Слот мотыги" },
                { "ct.menu.button.hoe_set", "Мотыга: есть" },
                { "ct.menu.button.pickaxe_slot", "Слот кирки" },
                { "ct.menu.button.pickaxe_set", "Кирка: есть" },
                { "ct.menu.button.remove", "Удалить" },
                { "ct.menu.overview.title", "Обзор" },
                { "ct.menu.ward.title", "Доступ к варду" },
                { "ct.menu.territory.title", "Настройки территории" },
                { "ct.menu.leveling.title", "Выравнивание территории" },
                { "ct.menu.field.territory", "Территория" },
                { "ct.menu.field.ward_id", "ID варда" },
                { "ct.menu.field.owner", "Владелец" },
                { "ct.menu.field.radius", "Радиус территории" },
                { "ct.menu.field.protection", "Защита" },
                { "ct.menu.field.your_access", "Ваш доступ" },
                { "ct.menu.field.doors", "Двери" },
                { "ct.menu.field.structures", "Постройки" },
                { "ct.menu.field.name", "Название" },
                { "ct.menu.field.guild_access", "Доступ гильдии" },
                { "ct.menu.field.group_access", "Доступ группы" },
                { "ct.menu.field.permitted_players", "Разрешённые игроки" },
                { "ct.menu.field.status", "Статус" },
                { "ct.menu.field.target", "Цель" },
                { "ct.menu.field.work_radius", "Радиус работ" },
                { "ct.menu.field.tools", "Инструменты" },
                { "ct.menu.field.fuel", "Топливо" },
                { "ct.menu.field.stone", "Камень" },
                { "ct.menu.field.scan", "Скан" },
                { "ct.menu.value.enabled", "Включено" },
                { "ct.menu.value.disabled", "Выключено" },
                { "ct.menu.value.locked", "Заперто" },
                { "ct.menu.value.unlocked", "Открыто" },
                { "ct.menu.value.locked_auto_close", "Заперто, автозакрытие {0}с" },
                { "ct.menu.value.protected", "Защищены" },
                { "ct.menu.value.vulnerable", "Уязвимы" },
                { "ct.menu.value.owner", "Владелец" },
                { "ct.menu.value.permitted", "Разрешён" },
                { "ct.menu.value.guest", "Гость" },
                { "ct.menu.value.empty", "Пусто" },
                { "ct.menu.value.ready", "Готово" },
                { "ct.menu.value.running", "Работает" },
                { "ct.menu.value.ward_height", "высота варда ({0})" },
                { "ct.menu.tool.hoe", "Мотыга" },
                { "ct.menu.tool.pickaxe", "Кирка" },
                { "ct.menu.tool.axe", "Топор" },
                { "ct.menu.preparation.note", "Подготовка открывает настоящий контейнер piece_chest_wood." },
                { "ct.menu.preparation.title", "Сундук подготовки выравнивания" },
                { "ct.menu.preparation.tools", "Инструменты" },
                { "ct.menu.preparation.fuel_slots", "Слоты топлива, по 500" },
                { "ct.menu.preparation.stone_slots", "Слоты камня, по 500" },
                { "ct.menu.storage.fuel.short", "Топливо {0}" },
                { "ct.menu.storage.stone.short", "Камень {0}" },
                { "ct.menu.storage.fuel.value", "Топливо {0}\\n{1}/500" },
                { "ct.menu.storage.stone.value", "Камень {0}\\n{1}/500" },
                { "ct.menu.more_players", "... и ещё {0}" },
                { "ct.message.doors_locked", "Двери территории заперты." },
                { "ct.message.entered_territory", "Вы вошли на территорию: {0}" },
                { "ct.message.left_territory", "Вы покинули территорию: {0}" },
                { "ct.territory.unnamed", "Безымянная территория" },
                { "ct.status.terraforming_unavailable", "Сервис выравнивания недоступен" }
            };
        }
    }
}
