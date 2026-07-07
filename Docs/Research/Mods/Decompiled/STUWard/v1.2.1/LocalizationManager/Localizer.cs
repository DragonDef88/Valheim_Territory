using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Jotunn.Entities;
using Jotunn.Managers;
using STUWard;
using UnityEngine;
using YamlDotNet.Serialization;

namespace LocalizationManager;

public static class Localizer
{
	private static readonly string[] FileExtensions = new string[2] { ".json", ".yml" };

	private static readonly Dictionary<string, Dictionary<string, string>> CachedTranslations = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

	private static readonly IDeserializer Deserializer = new DeserializerBuilder().IgnoreFields().Build();

	private static BaseUnityPlugin? _plugin;

	private static bool _registeredWithJotunn;

	private static bool _hookedJotunn;

	private static string? _lastLoggedAppliedLanguage;

	private static int _lastLoggedAppliedCount = -1;

	private static BaseUnityPlugin Plugin
	{
		get
		{
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Expected O, but got Unknown
			if ((Object)(object)_plugin != (Object)null)
			{
				return _plugin;
			}
			IEnumerable<TypeInfo> source;
			try
			{
				source = Assembly.GetExecutingAssembly().DefinedTypes.ToList();
			}
			catch (ReflectionTypeLoadException ex)
			{
				source = from type in ex.Types
					where type != null
					select type.GetTypeInfo();
			}
			_plugin = (BaseUnityPlugin)Chainloader.ManagerObject.GetComponent((Type)source.First((TypeInfo type) => type.IsClass && typeof(BaseUnityPlugin).IsAssignableFrom(type)));
			return _plugin;
		}
	}

	private static CustomLocalization CustomLocalization => LocalizationManager.Instance.GetLocalization();

	public static event Action? OnLocalizationComplete;

	public static void Load()
	{
		_ = Plugin;
		LoadTranslations();
		RegisterWithJotunn();
		HookJotunn();
		ReloadCurrentLanguageIfAvailable();
		SafeCallLocalizeComplete();
	}

	public static void Unload()
	{
		if (_hookedJotunn)
		{
			LocalizationManager.OnLocalizationAdded -= ReloadCurrentLanguageIfAvailable;
			_hookedJotunn = false;
		}
		_plugin = null;
		_registeredWithJotunn = false;
		_lastLoggedAppliedLanguage = null;
		_lastLoggedAppliedCount = -1;
	}

	public static void ReloadCurrentLanguageIfAvailable()
	{
		if (Localization.instance != null)
		{
			LoadTranslations();
			RegisterWithJotunn();
			ApplyCurrentLanguage(Localization.instance);
		}
	}

	public static void LoadLocalizationLater()
	{
		ReloadCurrentLanguageIfAvailable();
	}

	public static void SafeCallLocalizeComplete()
	{
		Localizer.OnLocalizationComplete?.Invoke();
	}

	public static void AddText(string key, string text)
	{
		if (!CachedTranslations.TryGetValue("English", out Dictionary<string, string> value))
		{
			value = new Dictionary<string, string>(StringComparer.Ordinal);
			CachedTranslations["English"] = value;
		}
		value[key] = text;
		string text2 = "English";
		string text3 = key;
		CustomLocalization.ClearToken(ref text2, ref text3);
		CustomLocalization.AddTranslation(ref text2, ref text3, text);
		if (Localization.instance != null)
		{
			Localization.instance.AddWord(key, text);
		}
	}

	private static void LoadTranslations()
	{
		if (CachedTranslations.Count > 0)
		{
			return;
		}
		HashSet<string> availableLanguages = GetAvailableLanguages();
		Dictionary<string, string> dictionary = ReadMergedLanguage("English", null);
		if (dictionary == null || dictionary.Count == 0)
		{
			throw new InvalidOperationException("Found no English localizations in mod " + Plugin.Info.Metadata.Name + ". Expected translations/English.json or translations/English.yml.");
		}
		CachedTranslations["English"] = dictionary;
		foreach (string item in availableLanguages)
		{
			if (!item.Equals("English", StringComparison.OrdinalIgnoreCase))
			{
				Dictionary<string, string> dictionary2 = ReadMergedLanguage(item, dictionary);
				if (dictionary2 != null && dictionary2.Count > 0)
				{
					CachedTranslations[item] = dictionary2;
				}
			}
		}
		ManualLogSource log = STUWard.Plugin.Log;
		if (log != null)
		{
			log.LogInfo((object)("Loaded STUWard localizations: " + string.Join(", ", CachedTranslations.Select<KeyValuePair<string, Dictionary<string, string>>, string>((KeyValuePair<string, Dictionary<string, string>> kv) => $"{kv.Key}={kv.Value.Count}"))));
		}
	}

	private static void RegisterWithJotunn()
	{
		if (_registeredWithJotunn)
		{
			return;
		}
		_registeredWithJotunn = true;
		foreach (KeyValuePair<string, Dictionary<string, string>> cachedTranslation in CachedTranslations)
		{
			cachedTranslation.Deconstruct(out var key, out var value);
			string text = key;
			Dictionary<string, string> dictionary = new Dictionary<string, string>(value, StringComparer.Ordinal);
			CustomLocalization.AddTranslation(ref text, dictionary);
		}
		ManualLogSource log = STUWard.Plugin.Log;
		if (log != null)
		{
			log.LogInfo((object)("Registered STUWard localizations with Jotunn: " + string.Join(", ", CachedTranslations.Keys)));
		}
	}

	private static void HookJotunn()
	{
		if (!_hookedJotunn)
		{
			LocalizationManager.OnLocalizationAdded += ReloadCurrentLanguageIfAvailable;
			_hookedJotunn = true;
		}
	}

	private static void ApplyCurrentLanguage(Localization localization)
	{
		string selectedLanguage = localization.GetSelectedLanguage();
		Dictionary<string, string> translationsForLanguage = GetTranslationsForLanguage(selectedLanguage);
		foreach (var (text3, text4) in translationsForLanguage)
		{
			localization.AddWord(text3, text4);
		}
		if (!string.Equals(_lastLoggedAppliedLanguage, selectedLanguage, StringComparison.Ordinal) || _lastLoggedAppliedCount != translationsForLanguage.Count)
		{
			_lastLoggedAppliedLanguage = selectedLanguage;
			_lastLoggedAppliedCount = translationsForLanguage.Count;
			ManualLogSource log = STUWard.Plugin.Log;
			if (log != null)
			{
				log.LogInfo((object)$"Applied STUWard localization for language '{selectedLanguage}' with {translationsForLanguage.Count} entries.");
			}
		}
	}

	private static Dictionary<string, string> GetTranslationsForLanguage(string language)
	{
		if (CachedTranslations.TryGetValue(language, out Dictionary<string, string> value))
		{
			return value;
		}
		return CachedTranslations["English"];
	}

	private static HashSet<string> GetAvailableLanguages()
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "English" };
		string[] manifestResourceNames = typeof(Localizer).Assembly.GetManifestResourceNames();
		foreach (string text in manifestResourceNames)
		{
			string[] fileExtensions = FileExtensions;
			foreach (string text2 in fileExtensions)
			{
				_ = "translations." + text2;
				if (!text.Contains(".translations.", StringComparison.OrdinalIgnoreCase) || !text.EndsWith(text2, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				int num = text.LastIndexOf(".translations.", StringComparison.OrdinalIgnoreCase);
				if (num >= 0)
				{
					int num2 = num + ".translations.".Length;
					int num3 = text.Length - num2 - text2.Length;
					if (num3 > 0)
					{
						hashSet.Add(text.Substring(num2, num3));
					}
				}
			}
		}
		foreach (string item in from path in Directory.GetFiles(Paths.PluginPath, Plugin.Info.Metadata.Name + ".*", SearchOption.AllDirectories)
			where FileExtensions.Contains<string>(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)
			select path)
		{
			string[] array = Path.GetFileNameWithoutExtension(item).Split('.');
			if (array.Length >= 2 && !string.IsNullOrWhiteSpace(array[1]))
			{
				hashSet.Add(array[1]);
			}
		}
		return hashSet;
	}

	private static Dictionary<string, string>? ReadMergedLanguage(string language, IReadOnlyDictionary<string, string>? englishFallback)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		if (englishFallback != null)
		{
			MergeInto(dictionary, englishFallback);
		}
		Dictionary<string, string> dictionary2 = ReadEmbeddedLanguage(language);
		if (dictionary2 != null)
		{
			MergeInto(dictionary, dictionary2);
		}
		Dictionary<string, string> dictionary3 = ReadExternalLanguage(language);
		if (dictionary3 != null)
		{
			MergeInto(dictionary, dictionary3);
		}
		if (dictionary.Count != 0)
		{
			return dictionary;
		}
		return null;
	}

	private static Dictionary<string, string>? ReadEmbeddedLanguage(string language)
	{
		string[] fileExtensions = FileExtensions;
		foreach (string text in fileExtensions)
		{
			byte[] array = ReadEmbeddedFileBytes("translations." + language + text, typeof(Localizer).Assembly);
			if (array != null)
			{
				return DeserializeTranslations(Encoding.UTF8.GetString(array));
			}
		}
		return null;
	}

	private static Dictionary<string, string>? ReadExternalLanguage(string language)
	{
		string text = FindExternalLanguageFile(language);
		if (text != null)
		{
			return DeserializeTranslations(File.ReadAllText(text, Encoding.UTF8));
		}
		return null;
	}

	private static string? FindExternalLanguageFile(string language)
	{
		string[] fileExtensions = FileExtensions;
		foreach (string text in fileExtensions)
		{
			string searchPattern = Plugin.Info.Metadata.Name + "." + language + text;
			string text2 = Directory.GetFiles(Paths.PluginPath, searchPattern, SearchOption.AllDirectories).FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(text2))
			{
				return text2;
			}
		}
		return null;
	}

	private static Dictionary<string, string> DeserializeTranslations(string rawText)
	{
		return Deserializer.Deserialize<Dictionary<string, string>>(rawText) ?? new Dictionary<string, string>(StringComparer.Ordinal);
	}

	private static void MergeInto(IDictionary<string, string> target, IReadOnlyDictionary<string, string> source)
	{
		foreach (KeyValuePair<string, string> item in source)
		{
			item.Deconstruct(out var key, out var value);
			string key2 = key;
			string value2 = value;
			target[key2] = value2;
		}
	}

	public static byte[]? ReadEmbeddedFileBytes(string resourceFileName, Assembly? containingAssembly = null)
	{
		string resourceFileName2 = resourceFileName;
		using MemoryStream memoryStream = new MemoryStream();
		if ((object)containingAssembly == null)
		{
			containingAssembly = Assembly.GetCallingAssembly();
		}
		string text = containingAssembly.GetManifestResourceNames().FirstOrDefault((string name) => name.EndsWith(resourceFileName2, StringComparison.OrdinalIgnoreCase));
		if (text == null)
		{
			return null;
		}
		containingAssembly.GetManifestResourceStream(text)?.CopyTo(memoryStream);
		return (memoryStream.Length == 0L) ? null : memoryStream.ToArray();
	}
}
