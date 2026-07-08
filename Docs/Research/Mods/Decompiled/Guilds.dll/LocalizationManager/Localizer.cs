using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace LocalizationManager;

[_003C1bfc4759_002Dc5a6_002D4eb1_002Dbee9_002Da1d0c5c59449_003ENullable(0)]
[_003C79594173_002De941_002D4ad9_002D973e_002D713c0d41eb0b_003ENullableContext(1)]
[PublicAPI]
internal class Localizer
{
	private static readonly Dictionary<string, Dictionary<string, Func<string>>> PlaceholderProcessors;

	private static readonly Dictionary<string, Dictionary<string, string>> loadedTexts;

	private static readonly ConditionalWeakTable<Localization, string> localizationLanguage;

	private static readonly List<WeakReference<Localization>> localizationObjects;

	[_003C1bfc4759_002Dc5a6_002D4eb1_002Dbee9_002Da1d0c5c59449_003ENullable(2)]
	private static BaseUnityPlugin _plugin;

	private static readonly List<string> fileExtensions;

	private static BaseUnityPlugin plugin
	{
		get
		{
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Expected O, but got Unknown
			if (_plugin == null)
			{
				IEnumerable<TypeInfo> source;
				try
				{
					source = Assembly.GetExecutingAssembly().DefinedTypes.ToList();
				}
				catch (ReflectionTypeLoadException ex)
				{
					source = from t in ex.Types
						where t != null
						select t.GetTypeInfo();
				}
				_plugin = (BaseUnityPlugin)Chainloader.ManagerObject.GetComponent((Type)source.First([_003C79594173_002De941_002D4ad9_002D973e_002D713c0d41eb0b_003ENullableContext(0)] (TypeInfo t) => t.IsClass && typeof(BaseUnityPlugin).IsAssignableFrom(t)));
			}
			return _plugin;
		}
	}

	[_003C1bfc4759_002Dc5a6_002D4eb1_002Dbee9_002Da1d0c5c59449_003ENullable(2)]
	[method: _003C79594173_002De941_002D4ad9_002D973e_002D713c0d41eb0b_003ENullableContext(2)]
	[field: _003C1bfc4759_002Dc5a6_002D4eb1_002Dbee9_002Da1d0c5c59449_003ENullable(2)]
	public static event Action OnLocalizationComplete;

	private static void UpdatePlaceholderText(Localization localization, string key)
	{
		localizationLanguage.TryGetValue(localization, out var value);
		string text = loadedTexts[value][key];
		if (PlaceholderProcessors.TryGetValue(key, out var value2))
		{
			text = value2.Aggregate(text, [_003C79594173_002De941_002D4ad9_002D973e_002D713c0d41eb0b_003ENullableContext(0)] (string current, KeyValuePair<string, Func<string>> kv) => current.Replace("{" + kv.Key + "}", kv.Value()));
		}
		localization.AddWord(key, text);
	}

	public static void AddPlaceholder<T>(string key, string placeholder, ConfigEntry<T> config, [_003C1bfc4759_002Dc5a6_002D4eb1_002Dbee9_002Da1d0c5c59449_003ENullable(new byte[] { 2, 1, 1 })] Func<T, string> convertConfigValue = null)
	{
		if (convertConfigValue == null)
		{
			convertConfigValue = (T val) => val.ToString();
		}
		if (!PlaceholderProcessors.ContainsKey(key))
		{
			PlaceholderProcessors[key] = new Dictionary<string, Func<string>>();
		}
		config.SettingChanged += [_003C79594173_002De941_002D4ad9_002D973e_002D713c0d41eb0b_003ENullableContext(0)] (object _, EventArgs _) =>
		{
			UpdatePlaceholder();
		};
		if (loadedTexts.ContainsKey(Localization.instance.GetSelectedLanguage()))
		{
			UpdatePlaceholder();
		}
		void UpdatePlaceholder()
		{
			PlaceholderProcessors[key][placeholder] = () => convertConfigValue(config.Value);
			UpdatePlaceholderText(Localization.instance, key);
		}
	}

	public static void AddText(string key, string text)
	{
		List<WeakReference<Localization>> list = new List<WeakReference<Localization>>();
		foreach (WeakReference<Localization> localizationObject in localizationObjects)
		{
			if (localizationObject.TryGetTarget(out var target))
			{
				Dictionary<string, string> dictionary = loadedTexts[localizationLanguage.GetOrCreateValue(target)];
				if (!target.m_translations.ContainsKey(key))
				{
					dictionary[key] = text;
					target.AddWord(key, text);
				}
			}
			else
			{
				list.Add(localizationObject);
			}
		}
		foreach (WeakReference<Localization> item in list)
		{
			localizationObjects.Remove(item);
		}
	}

	public static void Load()
	{
		_ = plugin;
	}

	public static void LoadLocalizationLater(Localization __instance)
	{
		LoadLocalization(Localization.instance, __instance.GetSelectedLanguage());
	}

	public static void SafeCallLocalizeComplete()
	{
		Localizer.OnLocalizationComplete?.Invoke();
	}

	private static void LoadLocalization(Localization __instance, string language)
	{
		if (!localizationLanguage.Remove(__instance))
		{
			localizationObjects.Add(new WeakReference<Localization>(__instance));
		}
		localizationLanguage.Add(__instance, language);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string item in from f in Directory.GetFiles(Path.GetDirectoryName(Paths.PluginPath), plugin.Info.Metadata.Name + ".*", SearchOption.AllDirectories)
			where fileExtensions.IndexOf(Path.GetExtension(f)) >= 0
			select f)
		{
			string text = Path.GetFileNameWithoutExtension(item).Split(new char[1] { '.' })[1];
			if (dictionary.ContainsKey(text))
			{
				Debug.LogWarning((object)("Duplicate key " + text + " found for " + plugin.Info.Metadata.Name + ". The duplicate file found at " + item + " will be skipped."));
			}
			else
			{
				dictionary[text] = item;
			}
		}
		byte[] array = LoadTranslationFromAssembly("English");
		if (array == null)
		{
			throw new Exception("Found no English localizations in mod " + plugin.Info.Metadata.Name + ". Expected an embedded resource translations/English.json or translations/English.yml.");
		}
		Dictionary<string, string> dictionary2 = new DeserializerBuilder().IgnoreFields().Build().Deserialize<Dictionary<string, string>>(Encoding.UTF8.GetString(array));
		if (dictionary2 == null)
		{
			throw new Exception("Localization for mod " + plugin.Info.Metadata.Name + " failed: Localization file was empty.");
		}
		string text2 = null;
		if (language != "English")
		{
			if (dictionary.TryGetValue(language, out var value))
			{
				text2 = File.ReadAllText(value);
			}
			else
			{
				byte[] array2 = LoadTranslationFromAssembly(language);
				if (array2 != null)
				{
					text2 = Encoding.UTF8.GetString(array2);
				}
			}
		}
		if (text2 == null && dictionary.TryGetValue("English", out var value2))
		{
			text2 = File.ReadAllText(value2);
		}
		if (text2 != null)
		{
			foreach (KeyValuePair<string, string> item2 in new DeserializerBuilder().IgnoreFields().Build().Deserialize<Dictionary<string, string>>(text2) ?? new Dictionary<string, string>())
			{
				dictionary2[item2.Key] = item2.Value;
			}
		}
		loadedTexts[language] = dictionary2;
		foreach (KeyValuePair<string, string> item3 in dictionary2)
		{
			UpdatePlaceholderText(__instance, item3.Key);
		}
	}

	static Localizer()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		PlaceholderProcessors = new Dictionary<string, Dictionary<string, Func<string>>>();
		loadedTexts = new Dictionary<string, Dictionary<string, string>>();
		localizationLanguage = new ConditionalWeakTable<Localization, string>();
		localizationObjects = new List<WeakReference<Localization>>();
		fileExtensions = new List<string>(2) { ".json", ".yml" };
		Harmony val = new Harmony("org.bepinex.helpers.LocalizationManager");
		val.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(Localization), "SetupLanguage", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Localizer), "LoadLocalization", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		val.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(FejdStartup), "SetupGui", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Localizer), "LoadLocalizationLater", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		val.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(FejdStartup), "Start", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Localizer), "SafeCallLocalizeComplete", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
	}

	[return: _003C1bfc4759_002Dc5a6_002D4eb1_002Dbee9_002Da1d0c5c59449_003ENullable(2)]
	private static byte[] LoadTranslationFromAssembly(string language)
	{
		foreach (string fileExtension in fileExtensions)
		{
			byte[] array = ReadEmbeddedFileBytes("translations." + language + fileExtension);
			if (array != null)
			{
				return array;
			}
		}
		return null;
	}

	[_003C79594173_002De941_002D4ad9_002D973e_002D713c0d41eb0b_003ENullableContext(2)]
	public static byte[] ReadEmbeddedFileBytes([_003C1bfc4759_002Dc5a6_002D4eb1_002Dbee9_002Da1d0c5c59449_003ENullable(1)] string resourceFileName, Assembly containingAssembly = null)
	{
		using MemoryStream memoryStream = new MemoryStream();
		if ((object)containingAssembly == null)
		{
			containingAssembly = Assembly.GetCallingAssembly();
		}
		string text = containingAssembly.GetManifestResourceNames().FirstOrDefault([_003C79594173_002De941_002D4ad9_002D973e_002D713c0d41eb0b_003ENullableContext(0)] (string str) => str.EndsWith(resourceFileName, StringComparison.Ordinal));
		if (text != null)
		{
			containingAssembly.GetManifestResourceStream(text)?.CopyTo(memoryStream);
		}
		return (memoryStream.Length == 0L) ? null : memoryStream.ToArray();
	}
}
