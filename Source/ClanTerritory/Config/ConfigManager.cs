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
                { "ct.menu.tab.biome", "Biome" },
                { "ct.menu.tab.economy", "Economy" },
                { "ct.menu.tab.terraforming", "Terraforming" },
                { "ct.menu.button.treasury", "Treasury" },
                { "ct.menu.button.close", "Close" },
                { "ct.menu.button.clan", "Clan" },
                { "ct.menu.button.overview", "Overview" },
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
                { "ct.menu.button.claim_biome", "Claim Biome" },
                { "ct.menu.button.release_biome", "Release Biome" },
                { "ct.menu.button.economy_deposit", "Deposit" },
                { "ct.menu.button.economy_withdraw", "Withdraw" },
                { "ct.menu.button.economy_upkeep", "Upkeep" },
                { "ct.menu.button.economy_tax", "Tax" },
                { "ct.menu.button.economy_transfer", "Transfer" },
                { "ct.menu.button.lock_biome_doors", "Lock Biome Doors" },
                { "ct.menu.button.unlock_biome_doors", "Unlock Biome Doors" },
                { "ct.menu.button.enable_biome_structure_protection", "Enable Biome Structure Protection" },
                { "ct.menu.button.disable_biome_structure_protection", "Disable Biome Structure Protection" },
                { "ct.menu.button.toggle_protection", "Toggle Protection" },
                { "ct.menu.button.hoe_slot", "Hoe Slot" },
                { "ct.menu.button.hoe_set", "Hoe: Set" },
                { "ct.menu.button.pickaxe_slot", "Pickaxe Slot" },
                { "ct.menu.button.pickaxe_set", "Pickaxe: Set" },
                { "ct.menu.button.remove", "Remove" },
                { "ct.menu.overview.title", "Overview" },
                { "ct.menu.clan.description_title", "Clan" },
                { "ct.menu.clan.description_unavailable", "Clan description is not available." },
                { "ct.menu.clan.no_clan", "This ward is not bound to a clan." },
                { "ct.menu.ward.title", "Ward Access" },
                { "ct.menu.territory.title", "Territory Settings" },
                { "ct.menu.biome.title", "Biome Dominion" },
                { "ct.menu.leveling.title", "Territory Leveling" },
                { "ct.menu.field.territory", "Territory" },
                { "ct.menu.field.biome", "Biome" },
                { "ct.menu.field.ward_id", "Ward ID" },
                { "ct.menu.field.owner", "Owner" },
                { "ct.menu.field.clan", "Clan" },
                { "ct.menu.field.radius", "Territory radius" },
                { "ct.menu.field.protection", "Protection" },
                { "ct.menu.field.your_access", "Your access" },
                { "ct.menu.field.doors", "Doors" },
                { "ct.menu.field.structures", "Structures" },
                { "ct.menu.field.name", "Name" },
                { "ct.menu.field.guild_access", "Guild access" },
                { "ct.menu.field.owner_guild", "Owner guild" },
                { "ct.menu.field.vassal_status", "Vassal status" },
                { "ct.menu.field.guild", "Guild" },
                { "ct.menu.field.balance", "Balance" },
                { "ct.menu.field.territory_guild", "Territory guild" },
                { "ct.menu.field.deposited", "Deposited" },
                { "ct.menu.field.withdrawn", "Withdrawn" },
                { "ct.menu.field.upkeep_paid", "Upkeep paid" },
                { "ct.menu.field.tribute_received", "Tribute received" },
                { "ct.menu.field.taxes", "Taxes paid / received" },
                { "ct.menu.field.transfers", "Transfers sent / received" },
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
                { "ct.menu.value.none", "None" },
                { "ct.menu.value.unknown", "Unknown" },
                { "ct.menu.value.unavailable", "Unavailable" },
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
                { "ct.menu.biome.status.free", "Free" },
                { "ct.menu.biome.status.claimed", "Claimed" },
                { "ct.menu.biome.vassal.yes", "This territory is vassal to the biome ruler." },
                { "ct.menu.biome.vassal.no", "This territory belongs to the biome ruler." },
                { "ct.menu.biome.overview.free", "{0} is unclaimed" },
                { "ct.menu.biome.overview.claimed", "{0} ruled by {1}" },
                { "ct.message.doors_locked", "Territory doors are locked." },
                { "ct.message.entered_territory", "Entered territory: {0}" },
                { "ct.message.left_territory", "Left territory: {0}" },
                { "ct.message.entered_vassal_territory", "Entered vassal territory: {0} ({1}, ruled by {2})" },
                { "ct.menu.economy.title", "Guild Economy" },
                { "ct.menu.economy.deposit_prompt", "Deposit coins amount" },
                { "ct.menu.economy.withdraw_prompt", "Withdraw coins amount" },
                { "ct.menu.economy.upkeep_prompt", "Territory upkeep amount" },
                { "ct.menu.economy.tax_prompt", "Territory tax amount" },
                { "ct.menu.economy.transfer_prompt", "Target guild and amount" },
                { "ct.economy.command.help", "Economy commands: /cteco status, /cteco deposit <coins>, /cteco withdraw <coins>, /cteco upkeep [coins], /cteco tax <coins>, /cteco transfer <guild> <coins>. Alias: /cteconomy" },
                { "ct.economy.command.server_only", "Economy changes can be made only on the server/host." },
                { "ct.economy.command.no_player", "Player is not ready." },
                { "ct.economy.command.no_guilds", "Guilds integration is not available." },
                { "ct.economy.command.no_guild", "You are not in a Guilds guild." },
                { "ct.economy.command.leader_only", "Only the guild leader can withdraw from the guild treasury." },
                { "ct.economy.command.invalid_amount", "Amount must be a positive number." },
                { "ct.economy.command.not_enough_coins", "You need {0} coins in your inventory." },
                { "ct.economy.command.not_enough_balance", "Guild treasury balance is {0} coins, but {1} coins were requested." },
                { "ct.economy.command.withdraw_failed", "Could not create coin payout." },
                { "ct.economy.command.deposit_success", "{0} treasury: deposited {1} coins. Balance: {2}." },
                { "ct.economy.command.withdraw_success", "{0} treasury: withdrew {1} coins. Balance: {2}." },
                { "ct.economy.command.status", "{0} treasury balance: {1} coins. Deposited: {2}. Withdrawn: {3}. Upkeep paid: {4}. Tribute received: {5}. Tax paid: {6}. Tax received: {7}. Transfers sent: {8}. Transfers received: {9}." },
                { "ct.economy.command.no_current_territory", "You are not inside a Clan Territory ward territory." },
                { "ct.economy.command.no_territory_guild", "This territory is not linked to a Guilds guild." },
                { "ct.economy.command.not_territory_guild_member", "Only a member of the territory guild can pay this territory upkeep." },
                { "ct.economy.command.not_enough_upkeep_balance", "{0} treasury has {1} coins, but upkeep requires {2}." },
                { "ct.economy.command.upkeep_success", "{0} paid territory upkeep: {1} coins. Treasury balance: {2}." },
                { "ct.economy.command.upkeep_vassal_success", "{0} paid territory upkeep: {1} coins. Tribute sent: {2} coins to {3}. Treasury balance: {4}. Overlord balance: {5}." },
                { "ct.economy.command.transfer_usage", "Usage: /cteco transfer <guild name> <coins>" },
                { "ct.economy.command.unknown_target_guild", "Unknown target guild treasury: {0}. The target guild must have an economy account first." },
                { "ct.economy.command.transfer_self", "Cannot transfer guild treasury coins to the same guild." },
                { "ct.economy.command.transfer_success", "{0} transferred {2} coins to {1}. Sender balance: {3}. Receiver balance: {4}." },
                { "ct.economy.command.tax_failed", "Territory tax payment failed." },
                { "ct.economy.command.tax_success", "Territory tax paid to {0}: {1} coins. Treasury balance: {2}." },
                { "ct.economy.command.tax_vassal_success", "Territory tax paid to {0}: {1} coins. Territory share: {2}. Tribute: {3} coins to {4}. Treasury balance: {5}. Overlord balance: {6}." },
                { "ct.biome.command.help", "Biome dominion commands: /ctbiome status, /ctbiome claim, /ctbiome release, /ctbiome list, /ctbiome set doorlock on|off, /ctbiome set protection on|off, /ctbiome set autoclose 3-10" },
                { "ct.biome.command.server_only", "Biome dominion can be changed only on the server/host." },
                { "ct.biome.command.no_player", "Player is not ready." },
                { "ct.biome.command.no_guilds", "Guilds integration is not available." },
                { "ct.biome.command.no_guild", "You are not in a Guilds guild." },
                { "ct.biome.command.leader_only", "Only the guild leader can claim or change biome dominion." },
                { "ct.biome.command.no_biome", "Current biome could not be detected." },
                { "ct.biome.command.already_claimed", "Biome {0} is already claimed by {1}." },
                { "ct.biome.command.claimed", "Biome {0} is now claimed by {1}. Existing territories in this biome are vassal territories." },
                { "ct.biome.command.not_claimed", "This biome is not claimed." },
                { "ct.biome.command.not_owner", "Your guild does not rule this biome, or you are not the guild leader." },
                { "ct.biome.command.released", "Biome {0} has been released." },
                { "ct.biome.command.status_free", "Biome {0} is not claimed." },
                { "ct.biome.command.status_claimed", "Biome {0} is ruled by {1}. Door lock: {2}. Structure protection: {3}. Auto-close: {4}s." },
                { "ct.biome.command.list_empty", "No claimed biomes." },
                { "ct.biome.command.list_header", "Claimed biomes:" },
                { "ct.biome.command.set_help", "Usage: /ctbiome set doorlock on|off, /ctbiome set protection on|off, /ctbiome set autoclose 3-10" },
                { "ct.biome.command.invalid_rule", "Unknown biome rule. Use doorlock, protection, or autoclose." },
                { "ct.biome.command.invalid_value", "Invalid value." },
                { "ct.biome.command.rule_saved", "Biome {0} rule saved: {1} = {2}." },
                { "ct.biome.command.autoclose_saved", "Biome {0} door auto-close saved: {1}s." },
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
                { "ct.menu.tab.biome", "Биом" },
                { "ct.menu.tab.economy", "Экономика" },
                { "ct.menu.tab.terraforming", "Земля" },
                { "ct.menu.button.treasury", "Казна" },
                { "ct.menu.button.close", "Закрыть" },
                { "ct.menu.button.clan", "Клан" },
                { "ct.menu.button.overview", "Обзор" },
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
                { "ct.menu.button.claim_biome", "Объявить биом" },
                { "ct.menu.button.release_biome", "Освободить биом" },
                { "ct.menu.button.economy_deposit", "Внести" },
                { "ct.menu.button.economy_withdraw", "Снять" },
                { "ct.menu.button.economy_upkeep", "Содержание" },
                { "ct.menu.button.economy_tax", "Налог" },
                { "ct.menu.button.economy_transfer", "Перевод" },
                { "ct.menu.button.lock_biome_doors", "Запереть двери биома" },
                { "ct.menu.button.unlock_biome_doors", "Отпереть двери биома" },
                { "ct.menu.button.enable_biome_structure_protection", "Включить защиту построек биома" },
                { "ct.menu.button.disable_biome_structure_protection", "Выключить защиту построек биома" },
                { "ct.menu.button.toggle_protection", "Переключить защиту" },
                { "ct.menu.button.hoe_slot", "Слот мотыги" },
                { "ct.menu.button.hoe_set", "Мотыга: есть" },
                { "ct.menu.button.pickaxe_slot", "Слот кирки" },
                { "ct.menu.button.pickaxe_set", "Кирка: есть" },
                { "ct.menu.button.remove", "Удалить" },
                { "ct.menu.overview.title", "Обзор" },
                { "ct.menu.clan.description_title", "Клан" },
                { "ct.menu.clan.description_unavailable", "Описание клана недоступно." },
                { "ct.menu.clan.no_clan", "Этот ward не привязан к клану." },
                { "ct.menu.ward.title", "Доступ к варду" },
                { "ct.menu.territory.title", "Настройки территории" },
                { "ct.menu.biome.title", "Владение биомом" },
                { "ct.menu.leveling.title", "Выравнивание территории" },
                { "ct.menu.field.territory", "Территория" },
                { "ct.menu.field.biome", "Биом" },
                { "ct.menu.field.ward_id", "ID варда" },
                { "ct.menu.field.owner", "Владелец" },
                { "ct.menu.field.clan", "Клан" },
                { "ct.menu.field.radius", "Радиус территории" },
                { "ct.menu.field.protection", "Защита" },
                { "ct.menu.field.your_access", "Ваш доступ" },
                { "ct.menu.field.doors", "Двери" },
                { "ct.menu.field.structures", "Постройки" },
                { "ct.menu.field.name", "Название" },
                { "ct.menu.field.guild_access", "Доступ гильдии" },
                { "ct.menu.field.owner_guild", "Гильдия-владелец" },
                { "ct.menu.field.vassal_status", "Вассальный статус" },
                { "ct.menu.field.guild", "Гильдия" },
                { "ct.menu.field.balance", "Баланс" },
                { "ct.menu.field.territory_guild", "Гильдия территории" },
                { "ct.menu.field.deposited", "Внесено" },
                { "ct.menu.field.withdrawn", "Снято" },
                { "ct.menu.field.upkeep_paid", "Уплачено содержания" },
                { "ct.menu.field.tribute_received", "Получено дани" },
                { "ct.menu.field.taxes", "Налоги уплачено / получено" },
                { "ct.menu.field.transfers", "Переводы отправлено / получено" },
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
                { "ct.menu.value.none", "Нет" },
                { "ct.menu.value.unknown", "Неизвестно" },
                { "ct.menu.value.unavailable", "Недоступно" },
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
                { "ct.menu.biome.status.free", "Свободен" },
                { "ct.menu.biome.status.claimed", "Захвачен" },
                { "ct.menu.biome.vassal.yes", "Эта территория вассальна правителю биома." },
                { "ct.menu.biome.vassal.no", "Эта территория принадлежит правителю биома." },
                { "ct.menu.biome.overview.free", "{0} свободен" },
                { "ct.menu.biome.overview.claimed", "{0} под властью {1}" },
                { "ct.message.doors_locked", "Двери территории заперты." },
                { "ct.message.entered_territory", "Вы вошли на территорию: {0}" },
                { "ct.message.left_territory", "Вы покинули территорию: {0}" },
                { "ct.message.entered_vassal_territory", "Вы вошли на вассальную территорию: {0} ({1}, владелец биома: {2})" },
                { "ct.menu.economy.title", "Экономика гильдии" },
                { "ct.menu.economy.deposit_prompt", "Количество монет для взноса" },
                { "ct.menu.economy.withdraw_prompt", "Количество монет для снятия" },
                { "ct.menu.economy.upkeep_prompt", "Количество монет на содержание территории" },
                { "ct.menu.economy.tax_prompt", "Количество монет на налог территории" },
                { "ct.menu.economy.transfer_prompt", "Целевая гильдия и количество" },
                { "ct.economy.command.help", "Команды экономики: /cteco status, /cteco deposit <монеты>, /cteco withdraw <монеты>, /cteco upkeep [монеты], /cteco tax <монеты>, /cteco transfer <гильдия> <монеты>. Алиас: /cteconomy" },
                { "ct.economy.command.server_only", "Экономику можно менять только на сервере/хосте." },
                { "ct.economy.command.no_player", "Игрок ещё не готов." },
                { "ct.economy.command.no_guilds", "Интеграция Guilds недоступна." },
                { "ct.economy.command.no_guild", "Вы не состоите в Guilds-гильдии." },
                { "ct.economy.command.leader_only", "Только лидер гильдии может снимать монеты из казны." },
                { "ct.economy.command.invalid_amount", "Количество должно быть положительным числом." },
                { "ct.economy.command.not_enough_coins", "В инвентаре нужно {0} монет." },
                { "ct.economy.command.not_enough_balance", "В казне гильдии {0} монет, а запрошено {1}." },
                { "ct.economy.command.withdraw_failed", "Не удалось создать выплату монетами." },
                { "ct.economy.command.deposit_success", "Казна {0}: внесено {1} монет. Баланс: {2}." },
                { "ct.economy.command.withdraw_success", "Казна {0}: снято {1} монет. Баланс: {2}." },
                { "ct.economy.command.status", "Баланс казны {0}: {1} монет. Внесено: {2}. Снято: {3}. Уплачено содержания: {4}. Получено дани: {5}. Уплачено налогов: {6}. Получено налогов: {7}. Переведено: {8}. Получено переводов: {9}." },
                { "ct.economy.command.no_current_territory", "Вы не находитесь внутри территории ward Clan Territory." },
                { "ct.economy.command.no_territory_guild", "Эта территория не привязана к Guilds-гильдии." },
                { "ct.economy.command.not_territory_guild_member", "Содержание этой территории может оплатить только участник её гильдии." },
                { "ct.economy.command.not_enough_upkeep_balance", "В казне {0} есть {1} монет, а для содержания нужно {2}." },
                { "ct.economy.command.upkeep_success", "{0} оплатила содержание территории: {1} монет. Баланс казны: {2}." },
                { "ct.economy.command.upkeep_vassal_success", "{0} оплатила содержание территории: {1} монет. Дань отправлена: {2} монет для {3}. Баланс казны: {4}. Баланс сюзерена: {5}." },
                { "ct.economy.command.transfer_usage", "Использование: /cteco transfer <название гильдии> <монеты>" },
                { "ct.economy.command.unknown_target_guild", "Неизвестная казна целевой гильдии: {0}. У целевой гильдии сначала должен появиться economy account." },
                { "ct.economy.command.transfer_self", "Нельзя перевести монеты из казны гильдии в эту же гильдию." },
                { "ct.economy.command.transfer_success", "{0} перевела {2} монет для {1}. Баланс отправителя: {3}. Баланс получателя: {4}." },
                { "ct.economy.command.tax_failed", "Оплата налога территории не удалась." },
                { "ct.economy.command.tax_success", "Налог территории уплачен для {0}: {1} монет. Баланс казны: {2}." },
                { "ct.economy.command.tax_vassal_success", "Налог территории уплачен для {0}: {1} монет. Доля территории: {2}. Дань: {3} монет для {4}. Баланс казны: {5}. Баланс сюзерена: {6}." },
                { "ct.biome.command.help", "Команды владения биомом: /ctbiome status, /ctbiome claim, /ctbiome release, /ctbiome list, /ctbiome set doorlock on|off, /ctbiome set protection on|off, /ctbiome set autoclose 3-10" },
                { "ct.biome.command.server_only", "Владение биомом можно менять только на сервере/хосте." },
                { "ct.biome.command.no_player", "Игрок ещё не готов." },
                { "ct.biome.command.no_guilds", "Интеграция Guilds недоступна." },
                { "ct.biome.command.no_guild", "Вы не состоите в Guilds-гильдии." },
                { "ct.biome.command.leader_only", "Только лидер гильдии может объявлять владение биомом или менять его правила." },
                { "ct.biome.command.no_biome", "Текущий биом не удалось определить." },
                { "ct.biome.command.already_claimed", "Биом {0} уже принадлежит {1}." },
                { "ct.biome.command.claimed", "Биом {0} теперь принадлежит {1}. Существующие территории в этом биоме стали вассальными." },
                { "ct.biome.command.not_claimed", "Этот биом никому не принадлежит." },
                { "ct.biome.command.not_owner", "Ваша гильдия не владеет этим биомом, или вы не лидер гильдии." },
                { "ct.biome.command.released", "Биом {0} освобождён." },
                { "ct.biome.command.status_free", "Биом {0} никому не принадлежит." },
                { "ct.biome.command.status_claimed", "Биом {0} принадлежит {1}. Замки дверей: {2}. Защита построек: {3}. Автозакрытие: {4}с." },
                { "ct.biome.command.list_empty", "Захваченных биомов нет." },
                { "ct.biome.command.list_header", "Захваченные биомы:" },
                { "ct.biome.command.set_help", "Использование: /ctbiome set doorlock on|off, /ctbiome set protection on|off, /ctbiome set autoclose 3-10" },
                { "ct.biome.command.invalid_rule", "Неизвестное правило биома. Используйте doorlock, protection или autoclose." },
                { "ct.biome.command.invalid_value", "Неверное значение." },
                { "ct.biome.command.rule_saved", "Правило биома {0} сохранено: {1} = {2}." },
                { "ct.biome.command.autoclose_saved", "Автозакрытие дверей биома {0} сохранено: {1}с." },
                { "ct.territory.unnamed", "Безымянная территория" },
                { "ct.status.terraforming_unavailable", "Сервис выравнивания недоступен" }
            };
        }
    }
}
