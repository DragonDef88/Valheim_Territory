using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace STUWard;

internal static class WardPluginConfigBindings
{
	private static bool _handlersBound;

	private const int GeneralOrderStart = 700;

	private const int RestrictionOrderStart = 900;

	private const int ClientOrderStart = 300;

	private const int DebugOrderStart = 100;

	private const int OrderStep = 10;

	internal static void BindAll()
	{
		UnbindAll();
		BindGeneral();
		BindRestrictions();
		BindClient();
		BindDebug();
		BindHandlers();
	}

	internal static void UnbindAll()
	{
		if (_handlersBound)
		{
			UnbindHandler<float>(Plugin.MaxWardRadius, HandleMaxWardRadiusChanged);
			UnbindHandler<int>(Plugin.MaxWardsPerSteamId, HandleMaxWardLimitChanged);
			UnbindHandler<Plugin.HostileCreatureStructureProtectionMode>(Plugin.HostileCreatureStructureProtection, HandleWardPresenceConfigChanged);
			UnbindHandler<Plugin.Toggle>(Plugin.DisableVanillaGuardStoneRecipe, HandleRecipeSettingsChanged);
			UnbindHandler<string>(Plugin.StuWardRecipe, HandleRecipeSettingsChanged);
			UnbindHandler<int>(Plugin.WardMinimapPinScale, HandleLocalWardPinConfigChanged);
			UnbindHandler<Plugin.Toggle>(Plugin.WardMinimapActiveRanges, HandleLocalWardPinConfigChanged);
			_handlersBound = false;
		}
	}

	private static void BindGeneral()
	{
		Plugin.ServerConfigLocked = Plugin.BindConfigEntry("1 - General", "Lock Configuration", Plugin.Toggle.On, "If on, the configuration is locked and can be changed by server admins only.", synchronizedSetting: true, 700);
		Plugin.ConfigSync.AddLockingConfigEntry<Plugin.Toggle>(Plugin.ServerConfigLocked);
		Plugin.MaxWardRadius = Plugin.BindConfigEntry("1 - General", "Max Ward Radius", 32f, "Maximum configurable Ward radius. Valid range: 8 to 64.", synchronizedSetting: true, 690);
		Plugin.MaxWardsPerSteamId = Plugin.BindConfigEntry("1 - General", "Max Wards Per Steam ID", 3, "Maximum number of managed Wards allowed per Steam/platform account. Set to -1 for unlimited.", synchronizedSetting: true, 680);
		Plugin.PickupBlockMode = Plugin.BindConfigEntry("1 - General", "Pickup Block Mode", Plugin.PickupBlockRule.BlockAllExceptWhitelist, "Pickup rule inside a foreign enabled ward. BlockAllExceptWhitelist blocks every item pickup except pickup_whitelist. AllowAllExceptBlacklist allows item pickup except pickup_blacklist.", synchronizedSetting: true, 670);
		Plugin.HostileCreatureStructureProtection = Plugin.BindConfigEntry("1 - General", "Hostile Creature Structure Protection Mode", Plugin.HostileCreatureStructureProtectionMode.UnattendedOnly, "Controls whether building pieces inside an enabled ward ignore damage from MonsterAI-controlled attackers. Off disables this extra protection. UnattendedOnly protects while no trusted player is nearby. Always protects regardless of trusted player presence. UnattendedOnly uses an 8 m trusted-player range buffer, 10 s grace time, and 1 s presence refresh.", synchronizedSetting: true, 660);
		Plugin.DisableVanillaGuardStoneRecipe = Plugin.BindConfigEntry("1 - General", "Disable Vanilla Guard Stone Recipe", Plugin.Toggle.On, "If on, the vanilla guard_stone build recipe is removed from the Hammer piece table while STUWard remains available.", synchronizedSetting: true, 650);
		Plugin.StuWardRecipe = Plugin.BindConfigEntry("1 - General", "STUWard Recipe", "GreydwarfEye:1,BoneFragments:3,Flint:5,Wood:7", "STUWard recipe override. Format: ItemPrefab:Amount[:Recover], ...", synchronizedSetting: true, 640);
	}

	private static void BindRestrictions()
	{
		IReadOnlyList<WardRestrictionDefinition> restrictionDefinitions = WardSettings.RestrictionDefinitions;
		for (int i = 0; i < restrictionDefinitions.Count; i++)
		{
			WardRestrictionDefinition wardRestrictionDefinition = restrictionDefinitions[i];
			SetRestrictionConfigEntry(wardRestrictionDefinition.Restriction, BindRestrictionMode(wardRestrictionDefinition.ConfigName, wardRestrictionDefinition.ConfigDescription, 900 - i * 10));
		}
	}

	private static ConfigEntry<Plugin.RestrictionServerMode> BindRestrictionMode(string name, string description, int configManagerOrder)
	{
		return Plugin.BindConfigEntry("2 - Ward Restrictions", name + " Restriction", Plugin.RestrictionServerMode.ForcedOn, description + " ForcedOn preserves the server rule. NotForced lets each ward owner toggle this restriction in the ward settings UI.", synchronizedSetting: true, configManagerOrder);
	}

	private static void SetRestrictionConfigEntry(WardRestrictionOptions restriction, ConfigEntry<Plugin.RestrictionServerMode> entry)
	{
		switch (restriction)
		{
		case WardRestrictionOptions.Doors:
			Plugin.DoorsRestriction = entry;
			break;
		case WardRestrictionOptions.Portals:
			Plugin.PortalsRestriction = entry;
			break;
		case WardRestrictionOptions.Pickup:
			Plugin.PickupRestriction = entry;
			break;
		case WardRestrictionOptions.PlacedConsumables:
			Plugin.PlacedConsumablesRestriction = entry;
			break;
		case WardRestrictionOptions.ItemStands:
			Plugin.ItemStandsRestriction = entry;
			break;
		case WardRestrictionOptions.ArmorStands:
			Plugin.ArmorStandsRestriction = entry;
			break;
		case WardRestrictionOptions.Containers:
			Plugin.ContainersRestriction = entry;
			break;
		case WardRestrictionOptions.CraftingStations:
			Plugin.CraftingStationsRestriction = entry;
			break;
		case WardRestrictionOptions.TameablesAndSaddles:
			Plugin.TameablesAndSaddlesRestriction = entry;
			break;
		}
	}

	private static void BindClient()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Plugin.WardSettingsShortcut = Plugin.BindConfigEntry<KeyboardShortcut>("3 - Client", "Ward Settings Shortcut", new KeyboardShortcut((KeyCode)101, (KeyCode[])(object)new KeyCode[1] { (KeyCode)308 }), "Shortcut used to open the ward settings UI while looking at your ward. Example values: LeftAlt + E, F7", synchronizedSetting: false, 300);
		Plugin.WardMinimapPinScale = Plugin.BindConfigEntry("3 - Client", "Ward Minimap Pin Scale", 1, "0 disables ward icon pins. 1 is the default icon size. 100 means x100 icon size.", synchronizedSetting: false, 290);
		Plugin.WardMinimapActiveRanges = Plugin.BindConfigEntry("3 - Client", "Ward Minimap Active Ranges", Plugin.Toggle.On, "If on, enabled managed wards also show their active radius on the minimap and map.", synchronizedSetting: false, 280);
	}

	private static void BindDebug()
	{
		Plugin.WardDiagnosticLogging = Plugin.BindConfigEntry("4 - Debug", "Ward Diagnostic Logging", Plugin.DiagnosticLogMode.Off, "Local-only scalar diagnostic logging for ward ownership/toggle flows. Use Failures for rejection paths only, or Verbose for request and state tracing. Enable separately on each client/server instance you want logs from.", synchronizedSetting: false, 100);
	}

	private static void BindHandlers()
	{
		BindHandler<float>(Plugin.MaxWardRadius, HandleMaxWardRadiusChanged);
		BindHandler<int>(Plugin.MaxWardsPerSteamId, HandleMaxWardLimitChanged);
		BindHandler<Plugin.HostileCreatureStructureProtectionMode>(Plugin.HostileCreatureStructureProtection, HandleWardPresenceConfigChanged);
		BindHandler<Plugin.Toggle>(Plugin.DisableVanillaGuardStoneRecipe, HandleRecipeSettingsChanged);
		BindHandler<string>(Plugin.StuWardRecipe, HandleRecipeSettingsChanged);
		BindHandler<int>(Plugin.WardMinimapPinScale, HandleLocalWardPinConfigChanged);
		BindHandler<Plugin.Toggle>(Plugin.WardMinimapActiveRanges, HandleLocalWardPinConfigChanged);
		_handlersBound = true;
	}

	private static void HandleMaxWardRadiusChanged(object? _, EventArgs __)
	{
		WardSettings.HandleMaxRadiusChanged();
	}

	private static void HandleMaxWardLimitChanged(object? _, EventArgs __)
	{
		WardOwnership.HandleWardLimitPolicyChanged();
	}

	private static void HandleWardPresenceConfigChanged(object? _, EventArgs __)
	{
		ManagedWardPresenceService.Invalidate();
	}

	private static void HandleRecipeSettingsChanged(object? _, EventArgs __)
	{
		WardPluginBootstrap.ApplyRecipeSettings();
	}

	private static void HandleLocalWardPinConfigChanged(object? _, EventArgs __)
	{
		WardMinimapPinsManager.HandleLocalConfigChanged();
	}

	private static void BindHandler<T>(ConfigEntry<T>? entry, EventHandler handler)
	{
		if (entry != null)
		{
			entry.SettingChanged += handler;
		}
	}

	private static void UnbindHandler<T>(ConfigEntry<T>? entry, EventHandler handler)
	{
		if (entry != null)
		{
			entry.SettingChanged -= handler;
		}
	}
}
