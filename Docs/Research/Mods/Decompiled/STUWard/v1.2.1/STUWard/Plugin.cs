using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace STUWard;

[BepInPlugin("sighsorry.STUWard", "STUWard", "1.2.1")]
[BepInDependency(/*Could not decode attribute arguments.*/)]
[BepInDependency(/*Could not decode attribute arguments.*/)]
public sealed class Plugin : BaseUnityPlugin
{
	internal enum Toggle
	{
		Off,
		On
	}

	internal enum PickupBlockRule
	{
		BlockAllExceptWhitelist,
		AllowAllExceptBlacklist
	}

	internal enum HostileCreatureStructureProtectionMode
	{
		Off,
		UnattendedOnly,
		Always
	}

	internal enum RestrictionServerMode
	{
		NotForced,
		ForcedOn
	}

	internal enum DiagnosticLogMode
	{
		Off,
		Failures,
		Verbose
	}

	internal const string ModName = "STUWard";

	internal const string ModVersion = "1.2.1";

	internal const string Author = "sighsorry";

	internal const string ModGuid = "sighsorry.STUWard";

	internal static readonly ConfigSync ConfigSync = new ConfigSync("sighsorry.STUWard")
	{
		DisplayName = "STUWard",
		CurrentVersion = "1.2.1",
		MinimumRequiredVersion = "1.2.1"
	};

	private Harmony _harmony;

	internal static ManualLogSource Log = null;

	internal static Plugin Instance = null;

	internal static WardGuiController WardGui = null;

	internal static ConfigEntry<Toggle> ServerConfigLocked = null;

	internal static ConfigEntry<int> MaxWardsPerSteamId = null;

	internal static ConfigEntry<float> MaxWardRadius = null;

	internal static ConfigEntry<PickupBlockRule> PickupBlockMode = null;

	internal static ConfigEntry<HostileCreatureStructureProtectionMode> HostileCreatureStructureProtection = null;

	internal static ConfigEntry<RestrictionServerMode> DoorsRestriction = null;

	internal static ConfigEntry<RestrictionServerMode> PortalsRestriction = null;

	internal static ConfigEntry<RestrictionServerMode> PickupRestriction = null;

	internal static ConfigEntry<RestrictionServerMode> PlacedConsumablesRestriction = null;

	internal static ConfigEntry<RestrictionServerMode> ItemStandsRestriction = null;

	internal static ConfigEntry<RestrictionServerMode> ArmorStandsRestriction = null;

	internal static ConfigEntry<RestrictionServerMode> ContainersRestriction = null;

	internal static ConfigEntry<RestrictionServerMode> CraftingStationsRestriction = null;

	internal static ConfigEntry<RestrictionServerMode> TameablesAndSaddlesRestriction = null;

	internal static ConfigEntry<Toggle> DisableVanillaGuardStoneRecipe = null;

	internal static ConfigEntry<string> StuWardRecipe = null;

	internal static ConfigEntry<KeyboardShortcut> WardSettingsShortcut = null;

	internal static ConfigEntry<int> WardMinimapPinScale = null;

	internal static ConfigEntry<Toggle> WardMinimapActiveRanges = null;

	internal static ConfigEntry<DiagnosticLogMode> WardDiagnosticLogging = null;

	private void Awake()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		Instance = this;
		Log = ((BaseUnityPlugin)this).Logger;
		WardPluginBootstrap.InitializeCore();
		bool saveOnConfigSet = ((BaseUnityPlugin)this).Config.SaveOnConfigSet;
		((BaseUnityPlugin)this).Config.SaveOnConfigSet = false;
		try
		{
			WardPluginConfigBindings.BindAll();
			WardPluginBootstrap.InitializeFeatures();
			_harmony = new Harmony("sighsorry.STUWard");
			WardPatchRegistry.ApplyAll(_harmony);
			WardGui = CreateOrReuseWardGuiController();
			((BaseUnityPlugin)this).Config.Save();
		}
		finally
		{
			((BaseUnityPlugin)this).Config.SaveOnConfigSet = saveOnConfigSet;
		}
	}

	private void Update()
	{
		WardPluginBootstrap.Update();
	}

	private void OnDestroy()
	{
		WardPluginBootstrap.Shutdown();
		Harmony harmony = _harmony;
		if (harmony != null)
		{
			harmony.UnpatchSelf();
		}
		((BaseUnityPlugin)this).Config.Save();
	}

	internal static bool IsBlockedItem(string prefabName)
	{
		return WardItemPrefabPolicy.IsBlockedItem(prefabName);
	}

	internal static bool HasBlockedItems()
	{
		return WardItemPrefabPolicy.HasBlockedItems();
	}

	internal static bool IsWardSettingsShortcutDown()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (WardSettingsShortcut != null)
		{
			KeyboardShortcut value = WardSettingsShortcut.Value;
			if ((int)((KeyboardShortcut)(ref value)).MainKey != 0)
			{
				value = WardSettingsShortcut.Value;
				return ((KeyboardShortcut)(ref value)).IsDown();
			}
		}
		return false;
	}

	internal static bool HasWardSettingsShortcutBinding()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if (WardSettingsShortcut != null)
		{
			KeyboardShortcut value = WardSettingsShortcut.Value;
			return (int)((KeyboardShortcut)(ref value)).MainKey > 0;
		}
		return false;
	}

	internal static string GetWardSettingsShortcutLabel()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Invalid comparison between Unknown and I4
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (WardSettingsShortcut != null)
		{
			KeyboardShortcut value = WardSettingsShortcut.Value;
			if ((int)((KeyboardShortcut)(ref value)).MainKey != 0)
			{
				KeyboardShortcut value2 = WardSettingsShortcut.Value;
				List<string> list = new List<string>();
				AddModifierLabel(list, ((KeyboardShortcut)(ref value2)).Modifiers, (KeyCode)306, (KeyCode)305, "Ctrl");
				AddModifierLabel(list, ((KeyboardShortcut)(ref value2)).Modifiers, (KeyCode)308, (KeyCode)307, "Alt");
				AddModifierLabel(list, ((KeyboardShortcut)(ref value2)).Modifiers, (KeyCode)304, (KeyCode)303, "Shift");
				foreach (KeyCode modifier in ((KeyboardShortcut)(ref value2)).Modifiers)
				{
					if (modifier - 303 > 5)
					{
						list.Add(GetKeyLabel(modifier));
					}
				}
				list.Add(GetKeyLabel(((KeyboardShortcut)(ref value2)).MainKey));
				return string.Join("+", list);
			}
		}
		return WardLocalization.Localize("$stuw_shortcut_unbound", "Unbound");
	}

	private static void AddModifierLabel(List<string> parts, IEnumerable<KeyCode> modifiers, KeyCode left, KeyCode right, string label)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyCode modifier in modifiers)
		{
			if (modifier == left || modifier == right)
			{
				parts.Add(label);
				break;
			}
		}
	}

	private static string GetKeyLabel(KeyCode keyCode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected I4, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected I4, but got Unknown
		switch (keyCode - 48)
		{
		default:
			switch (keyCode - 303)
			{
			case 4:
			case 5:
				return "Alt";
			case 2:
			case 3:
				return "Ctrl";
			case 0:
			case 1:
				return "Shift";
			default:
				return ((object)(KeyCode)(ref keyCode)).ToString();
			}
		case 0:
			return "0";
		case 1:
			return "1";
		case 2:
			return "2";
		case 3:
			return "3";
		case 4:
			return "4";
		case 5:
			return "5";
		case 6:
			return "6";
		case 7:
			return "7";
		case 8:
			return "8";
		case 9:
			return "9";
		}
	}

	internal static ConfigEntry<T> BindConfigEntry<T>(string group, string name, T value, string description, bool synchronizedSetting = true, int? configManagerOrder = null)
	{
		return Instance.BindConfig(group, name, value, description, synchronizedSetting, configManagerOrder);
	}

	internal static bool ShouldLogWardDiagnosticFailures()
	{
		if (WardDiagnosticLogging != null)
		{
			return WardDiagnosticLogging.Value != DiagnosticLogMode.Off;
		}
		return false;
	}

	internal static bool ShouldLogWardDiagnosticVerbose()
	{
		if (WardDiagnosticLogging != null)
		{
			return WardDiagnosticLogging.Value == DiagnosticLogMode.Verbose;
		}
		return false;
	}

	internal static void LogWardDiagnosticFailure(string context, string message)
	{
		if (ShouldLogWardDiagnosticFailures() && Log != null)
		{
			Log.LogWarning((object)("[WardDiag:" + context + "] " + message));
		}
	}

	internal static void LogWardDiagnosticVerbose(string context, string message)
	{
		if (ShouldLogWardDiagnosticVerbose() && Log != null)
		{
			Log.LogInfo((object)("[WardDiag:" + context + "] " + message));
		}
	}

	private static WardGuiController CreateOrReuseWardGuiController()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		if ((Object)(object)WardGuiController.Instance != (Object)null)
		{
			return WardGuiController.Instance;
		}
		GameObject val = new GameObject("STUWard.WardGui");
		Object.DontDestroyOnLoad((Object)val);
		return val.AddComponent<WardGuiController>();
	}

	private ConfigEntry<T> BindConfig<T>(string group, string name, T value, string description, bool synchronizedSetting = true, int? configManagerOrder = null)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		string text = (synchronizedSetting ? "Synced with server." : "Not synced with server.");
		string text2 = (string.IsNullOrWhiteSpace(description) ? text : (description.TrimEnd() + " " + text));
		ConfigDescription val = (configManagerOrder.HasValue ? new ConfigDescription(text2, (AcceptableValueBase)null, new object[1]
		{
			new ConfigurationManagerAttributes
			{
				Order = configManagerOrder.Value
			}
		}) : new ConfigDescription(text2, (AcceptableValueBase)null, Array.Empty<object>()));
		ConfigEntry<T> val2 = ((BaseUnityPlugin)this).Config.Bind<T>(group, name, value, val);
		if (synchronizedSetting)
		{
			ConfigSync.AddConfigEntry<T>(val2).SynchronizedConfig = true;
		}
		return val2;
	}
}
