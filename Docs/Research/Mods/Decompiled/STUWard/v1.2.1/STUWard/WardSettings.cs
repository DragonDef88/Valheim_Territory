using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace STUWard;

internal static class WardSettings
{
	internal const int ManagedAreaMarkerSegments = 36;

	private const float ManagedAreaMarkerSegmentLengthMultiplier = 2f;

	internal const float MinAreaMarkerSpeedMultiplier = 0f;

	internal const float MaxAreaMarkerSpeedMultiplier = 1f;

	internal const float DefaultAreaMarkerSpeedMultiplier = 0.5f;

	internal const float MinAreaMarkerAlpha = 0f;

	internal const float MaxAreaMarkerAlpha = 1f;

	internal const float DefaultAreaMarkerAlpha = 0.5f;

	internal const float MinRadius = 8f;

	internal const float MaxRadiusLimit = 64f;

	internal const float DefaultMaxRadius = 32f;

	internal const float MinAutoCloseDelay = 0f;

	internal const float MaxAutoCloseDelay = 10f;

	internal const float DefaultAutoCloseDelay = 4f;

	internal const bool DefaultWarningSoundEnabled = true;

	internal const bool DefaultWarningFlashEnabled = true;

	private const string RpcUpdateSettings = "STUWard_UpdateSettings";

	private const string RpcUpdateSettingsResponse = "STUWard_UpdateSettingsResponse";

	private const string RpcRemovePermitted = "STUWard_RemovePermitted";

	private const string ShowAreaMarkerKey = "stuw_show_area_marker";

	private const string AreaMarkerSpeedMultiplierKey = "stuw_area_marker_speed_multiplier";

	private const string AreaMarkerAlphaKey = "stuw_area_marker_alpha";

	private const string RadiusKey = "stuw_radius";

	private const string AutoCloseDoorsKey = "stuw_auto_close_doors";

	private const string AutoCloseDelayKey = "stuw_auto_close_delay";

	private const string WarningSoundEnabledKey = "stuw_warning_sound_enabled";

	private const string WarningFlashEnabledKey = "stuw_warning_flash_enabled";

	private const string RestrictionOptionsKey = "stuw_restriction_options";

	private const float FallbackAreaMarkerSpeed = 0.1f;

	private const float MinimumAreaMarkerBrightness = 0.35f;

	private const float AreaMarkerBrightnessGamma = 1.8f;

	private const float MinimumAreaMarkerBrightnessInput = 0.5f;

	private static readonly string[] AreaMarkerColorProperties = new string[3] { "_Color", "_BaseColor", "_TintColor" };

	private static readonly MaterialPropertyBlock AreaMarkerPropertyBlock = new MaterialPropertyBlock();

	private static readonly WardRestrictionDefinition[] RestrictionDefinitionValues = new WardRestrictionDefinition[9]
	{
		new WardRestrictionDefinition(WardRestrictionOptions.Doors, "Doors", "Controls whether door interaction is always blocked by foreign enabled wards or can be turned off per ward.", "$stuw_ui_restriction_doors", "Doors"),
		new WardRestrictionDefinition(WardRestrictionOptions.Portals, "Portals", "Controls whether portal entry and TargetPortal routing are always blocked by foreign enabled wards or can be turned off per ward.", "$stuw_ui_restriction_portals", "Portals"),
		new WardRestrictionDefinition(WardRestrictionOptions.Pickup, "Pickup", "Controls whether normal item pickup is always blocked by foreign enabled wards or can be turned off per ward.", "$stuw_ui_restriction_pickup", "Pickup"),
		new WardRestrictionDefinition(WardRestrictionOptions.PlacedConsumables, "Placed Consumables", "Controls whether eating hammer-placed consumables and feasts is always blocked by foreign enabled wards or can be turned off per ward.", "$stuw_ui_restriction_placed_consumables", "Consumables"),
		new WardRestrictionDefinition(WardRestrictionOptions.ItemStands, "Item Stands", "Controls whether item stand interaction is always blocked by foreign enabled wards or can be turned off per ward.", "$stuw_ui_restriction_item_stands", "Item stands"),
		new WardRestrictionDefinition(WardRestrictionOptions.ArmorStands, "Armor Stands", "Controls whether armor stand item placement is always blocked by foreign enabled wards or can be turned off per ward.", "$stuw_ui_restriction_armor_stands", "Armor stands"),
		new WardRestrictionDefinition(WardRestrictionOptions.Containers, "Containers", "Controls whether container interaction and remote container access are always blocked by foreign enabled wards or can be turned off per ward.", "$stuw_ui_restriction_containers", "Containers"),
		new WardRestrictionDefinition(WardRestrictionOptions.CraftingStations, "Crafting Stations", "Controls whether crafting station interaction is always blocked by foreign enabled wards or can be turned off per ward.", "$stuw_ui_restriction_crafting_stations", "Crafting stations"),
		new WardRestrictionDefinition(WardRestrictionOptions.TameablesAndSaddles, "Tameables And Saddles", "Controls whether tameable and saddle interaction is always blocked by foreign enabled wards or can be turned off per ward.", "$stuw_ui_restriction_tameables_and_saddles", "Tames")
	};

	private static long _nextConfigurationRequestId = 1L;

	internal static IReadOnlyList<WardRestrictionDefinition> RestrictionDefinitions => RestrictionDefinitionValues;

	internal static float MaxRadius => Mathf.Clamp(Plugin.MaxWardRadius?.Value ?? 32f, 8f, 64f);

	internal static WardRestrictionOptions ForcedRestrictions
	{
		get
		{
			WardRestrictionOptions restrictions = WardRestrictionOptions.None;
			for (int i = 0; i < RestrictionDefinitionValues.Length; i++)
			{
				WardRestrictionDefinition wardRestrictionDefinition = RestrictionDefinitionValues[i];
				AddForcedRestriction(ref restrictions, wardRestrictionDefinition.Restriction, GetRestrictionConfigEntry(wardRestrictionDefinition.Restriction));
			}
			return restrictions;
		}
	}

	internal static bool IsRestrictionForced(WardRestrictionOptions restriction)
	{
		return (ForcedRestrictions & restriction) != 0;
	}

	internal static bool HasRestriction(WardConfiguration configuration, WardRestrictionOptions restriction)
	{
		return (configuration.Restrictions & restriction) != 0;
	}

	internal static WardConfiguration WithRestriction(WardConfiguration configuration, WardRestrictionOptions restriction, bool enabled)
	{
		WardRestrictionOptions restrictions = (enabled ? (configuration.Restrictions | restriction) : (configuration.Restrictions & ~restriction));
		return WithRestrictions(configuration, restrictions);
	}

	internal static WardConfiguration WithAreaMarkerSpeedMultiplier(WardConfiguration configuration, float value)
	{
		return new WardConfiguration(configuration.ShowAreaMarker, Mathf.Clamp(value, 0f, 1f), configuration.AreaMarkerAlpha, configuration.Radius, configuration.AutoCloseDelay, configuration.WarningSoundEnabled, configuration.WarningFlashEnabled, configuration.Restrictions);
	}

	internal static WardConfiguration WithAreaMarkerAlpha(WardConfiguration configuration, float value)
	{
		return new WardConfiguration(configuration.ShowAreaMarker, configuration.AreaMarkerSpeedMultiplier, Mathf.Clamp(value, 0f, 1f), configuration.Radius, configuration.AutoCloseDelay, configuration.WarningSoundEnabled, configuration.WarningFlashEnabled, configuration.Restrictions);
	}

	internal static WardConfiguration WithRadius(WardConfiguration configuration, float value)
	{
		return new WardConfiguration(configuration.ShowAreaMarker, configuration.AreaMarkerSpeedMultiplier, configuration.AreaMarkerAlpha, Mathf.Clamp(value, 8f, MaxRadius), configuration.AutoCloseDelay, configuration.WarningSoundEnabled, configuration.WarningFlashEnabled, configuration.Restrictions);
	}

	internal static WardConfiguration WithAutoCloseDelay(WardConfiguration configuration, float value)
	{
		return new WardConfiguration(configuration.ShowAreaMarker, configuration.AreaMarkerSpeedMultiplier, configuration.AreaMarkerAlpha, configuration.Radius, Mathf.Clamp(value, 0f, 10f), configuration.WarningSoundEnabled, configuration.WarningFlashEnabled, configuration.Restrictions);
	}

	internal static WardConfiguration WithRestrictions(WardConfiguration configuration, WardRestrictionOptions restrictions)
	{
		return new WardConfiguration(configuration.ShowAreaMarker, configuration.AreaMarkerSpeedMultiplier, configuration.AreaMarkerAlpha, configuration.Radius, configuration.AutoCloseDelay, configuration.WarningSoundEnabled, configuration.WarningFlashEnabled, NormalizeRestrictions(restrictions));
	}

	private static void AddForcedRestriction(ref WardRestrictionOptions restrictions, WardRestrictionOptions restriction, ConfigEntry<Plugin.RestrictionServerMode>? config)
	{
		if (config != null && config.Value == Plugin.RestrictionServerMode.ForcedOn)
		{
			restrictions |= restriction;
		}
	}

	private static ConfigEntry<Plugin.RestrictionServerMode>? GetRestrictionConfigEntry(WardRestrictionOptions restriction)
	{
		return (ConfigEntry<Plugin.RestrictionServerMode>?)(restriction switch
		{
			WardRestrictionOptions.Doors => Plugin.DoorsRestriction, 
			WardRestrictionOptions.Portals => Plugin.PortalsRestriction, 
			WardRestrictionOptions.Pickup => Plugin.PickupRestriction, 
			WardRestrictionOptions.PlacedConsumables => Plugin.PlacedConsumablesRestriction, 
			WardRestrictionOptions.ItemStands => Plugin.ItemStandsRestriction, 
			WardRestrictionOptions.ArmorStands => Plugin.ArmorStandsRestriction, 
			WardRestrictionOptions.Containers => Plugin.ContainersRestriction, 
			WardRestrictionOptions.CraftingStations => Plugin.CraftingStationsRestriction, 
			WardRestrictionOptions.TameablesAndSaddles => Plugin.TameablesAndSaddlesRestriction, 
			_ => null, 
		});
	}

	internal static void CaptureAreaDefaults(PrivateArea area)
	{
		CircleProjector areaMarker = area.m_areaMarker;
		ManagedWardRuntimeContext orCreate = ManagedWardRuntimeContexts.GetOrCreate(area);
		orCreate.DefaultAreaMarkerSpeed = (((Object)(object)areaMarker != (Object)null) ? Mathf.Max(areaMarker.m_speed, 0f) : 0.1f);
		orCreate.HasDefaultAreaMarkerSpeed = true;
	}

	internal static void InitializeArea(PrivateArea area)
	{
		if (!ManagedWardRuntimeContexts.GetOrCreate(area).HasDefaultAreaMarkerSpeed)
		{
			CaptureAreaDefaults(area);
		}
	}

	internal static void HandleMaxRadiusChanged()
	{
		ManagedWardRuntimeContexts.ClearConfigurationCaches();
		List<PrivateArea> allAreas = PrivateArea.m_allAreas;
		if (allAreas == null)
		{
			return;
		}
		for (int i = 0; i < allAreas.Count; i++)
		{
			ManagedWardRef ward = ManagedWardRef.FromArea(allAreas[i]);
			if (WardAccess.IsManagedWard(ward, requireEnabled: false))
			{
				ApplyAreaState(ward);
			}
		}
		ManagedWardMapStateService.InvalidateProjection("max ward radius config changed");
	}

	internal static WardConfiguration GetConfiguration(PrivateArea area)
	{
		InitializeArea(area);
		ZDO zdo = GetZdo(area);
		float maxRadius = MaxRadius;
		WardRestrictionOptions forcedRestrictions = ForcedRestrictions;
		if (zdo != null)
		{
			uint dataRevision = zdo.DataRevision;
			ManagedWardRuntimeContext orCreate = ManagedWardRuntimeContexts.GetOrCreate(area);
			if (orCreate.HasCachedConfiguration)
			{
				CachedWardConfiguration cachedConfiguration = orCreate.CachedConfiguration;
				if (cachedConfiguration.DataRevision == dataRevision && Mathf.Approximately(cachedConfiguration.MaxRadius, maxRadius) && cachedConfiguration.ForcedRestrictions == forcedRestrictions)
				{
					return cachedConfiguration.Configuration;
				}
			}
		}
		float defaultRadius = GetDefaultRadius(area);
		bool showAreaMarker = zdo == null || zdo.GetBool("stuw_show_area_marker", true);
		float areaMarkerSpeedMultiplier = Mathf.Clamp01((zdo != null) ? zdo.GetFloat("stuw_area_marker_speed_multiplier", 0.5f) : 0.5f);
		float areaMarkerAlpha = Mathf.Clamp01((zdo != null) ? zdo.GetFloat("stuw_area_marker_alpha", 0.5f) : 0.5f);
		float radius = Mathf.Clamp((zdo != null) ? zdo.GetFloat("stuw_radius", defaultRadius) : defaultRadius, 8f, maxRadius);
		float autoCloseDelay = Mathf.Clamp((zdo != null) ? zdo.GetFloat("stuw_auto_close_delay", 4f) : 4f, 0f, 10f);
		bool warningSoundEnabled = zdo == null || zdo.GetBool("stuw_warning_sound_enabled", true);
		bool warningFlashEnabled = zdo == null || zdo.GetBool("stuw_warning_flash_enabled", true);
		WardRestrictionOptions restrictions = ApplyForcedRestrictions(NormalizeRestrictions((zdo != null) ? ((WardRestrictionOptions)zdo.GetInt("stuw_restriction_options", 511)) : WardRestrictionOptions.All), forcedRestrictions);
		WardConfiguration wardConfiguration = new WardConfiguration(showAreaMarker, areaMarkerSpeedMultiplier, areaMarkerAlpha, radius, autoCloseDelay, warningSoundEnabled, warningFlashEnabled, restrictions);
		if (zdo != null)
		{
			ManagedWardRuntimeContext orCreate2 = ManagedWardRuntimeContexts.GetOrCreate(area);
			orCreate2.CachedConfiguration = new CachedWardConfiguration(zdo.DataRevision, maxRadius, forcedRestrictions, wardConfiguration);
			orCreate2.HasCachedConfiguration = true;
		}
		return wardConfiguration;
	}

	internal static void ApplyAreaState(PrivateArea area)
	{
		ApplyAreaState(ManagedWardRef.FromArea(area));
	}

	internal static void ApplyAreaState(PrivateArea area, WardConfiguration configuration)
	{
		ApplyAreaState(ManagedWardRef.FromArea(area), configuration);
	}

	internal static void ApplyAreaState(ManagedWardRef ward)
	{
		PrivateArea area = ward.Area;
		if (!((Object)(object)area == (Object)null))
		{
			ApplyAreaState(ward, GetConfiguration(area));
		}
	}

	internal static void ApplyAreaState(ManagedWardRef ward, WardConfiguration configuration)
	{
		PrivateArea area = ward.Area;
		if ((Object)(object)area == (Object)null)
		{
			return;
		}
		InitializeArea(area);
		bool num = !Mathf.Approximately(area.m_radius, configuration.Radius);
		if (num)
		{
			area.m_radius = configuration.Radius;
		}
		CircleProjector areaMarker = area.m_areaMarker;
		if ((Object)(object)areaMarker == (Object)null)
		{
			InvalidateAreaMarkerVisuals(area);
		}
		else
		{
			bool visible = ShouldShowAreaMarker(area, configuration);
			float num2 = GetDefaultAreaMarkerSpeed(area) * configuration.AreaMarkerSpeedMultiplier;
			if (areaMarker.m_nrOfSegments != 36)
			{
				areaMarker.m_nrOfSegments = 36;
			}
			if (!Mathf.Approximately(areaMarker.m_radius, configuration.Radius))
			{
				areaMarker.m_radius = configuration.Radius;
			}
			if (!Mathf.Approximately(areaMarker.m_speed, num2))
			{
				areaMarker.m_speed = num2;
			}
			ApplyManagedAreaMarkerVisibility(area, visible);
			if (ShouldRefreshAreaMarkerVisuals(area, areaMarker, configuration))
			{
				ApplyAreaMarkerVisuals(areaMarker, configuration);
				CacheAreaMarkerVisualState(area, areaMarker, configuration);
			}
		}
		if (num)
		{
			ManagedWardPresenceService.Invalidate();
			ManagedWardPlacementPreviewService.Invalidate();
			WardAccess.RefreshManagedWardSpatialIndexEntry(ward);
		}
	}

	internal static void ApplyPlacementGhostPreviewRadius(PrivateArea area)
	{
		if ((Object)(object)area == (Object)null || !Player.IsPlacementGhost(((Component)area).gameObject))
		{
			return;
		}
		InitializeArea(area);
		float maxRadius = MaxRadius;
		if (!Mathf.Approximately(area.m_radius, maxRadius))
		{
			area.m_radius = maxRadius;
		}
		if (Object.op_Implicit((Object)(object)area.m_areaMarker))
		{
			if (area.m_areaMarker.m_nrOfSegments != 36)
			{
				area.m_areaMarker.m_nrOfSegments = 36;
			}
			if (!Mathf.Approximately(area.m_areaMarker.m_radius, maxRadius))
			{
				area.m_areaMarker.m_radius = maxRadius;
			}
		}
	}

	internal static bool ShouldShowAreaMarker(PrivateArea area)
	{
		return ShouldShowAreaMarker(area, GetConfiguration(area));
	}

	internal static bool ShouldShowAreaMarker(PrivateArea area, WardConfiguration configuration)
	{
		if (!Player.IsPlacementGhost(((Component)area).gameObject))
		{
			if (area.IsEnabled())
			{
				return configuration.ShowAreaMarker;
			}
			return false;
		}
		return true;
	}

	internal static void ShowManagedAreaMarker(PrivateArea area)
	{
		if (!((Object)(object)area == (Object)null) && !((Object)(object)area.m_areaMarker == (Object)null))
		{
			((MonoBehaviour)area).CancelInvoke("HideMarker");
			((Component)area.m_areaMarker).gameObject.SetActive(true);
		}
	}

	internal static void InvalidateAreaMarkerVisuals(PrivateArea? area)
	{
		ManagedWardRuntimeContexts.ClearAreaMarkerVisualState(area);
	}

	internal static float GetRadius(PrivateArea area)
	{
		return GetConfiguration(area).Radius;
	}

	internal static float GetStoredRadiusOrMin(PrivateArea area)
	{
		if (!((Object)(object)area == (Object)null))
		{
			return GetStoredRadius(GetZdo(area));
		}
		return 8f;
	}

	internal static float GetStoredRadius(ZDO? zdo, float defaultRadius = 8f)
	{
		return Mathf.Clamp((zdo != null) ? zdo.GetFloat("stuw_radius", defaultRadius) : defaultRadius, 8f, MaxRadius);
	}

	internal static void ApplyPlacementPreviewMarker(CircleProjector marker, float radius)
	{
		if (!((Object)(object)marker == (Object)null))
		{
			marker.m_radius = radius;
			ApplyAreaMarkerVisuals(marker, new WardConfiguration(showAreaMarker: true, 0f, 0.5f, radius, 0f, warningSoundEnabled: true, warningFlashEnabled: true));
		}
	}

	internal static bool TryGetAutoCloseDoorDelay(Vector3 point, out float delay)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<PrivateArea> candidateManagedWards = WardAccess.GetCandidateManagedWards(point, 0f, requireEnabled: true);
		if (candidateManagedWards.Count == 0)
		{
			delay = 0f;
			return false;
		}
		bool flag = false;
		float num = float.MaxValue;
		foreach (PrivateArea item in candidateManagedWards)
		{
			if ((Object)(object)item == (Object)null || !item.IsInside(point, 0f))
			{
				continue;
			}
			WardConfiguration configuration = GetConfiguration(item);
			if (!(configuration.AutoCloseDelay <= 0f))
			{
				flag = true;
				if (configuration.AutoCloseDelay < num)
				{
					num = configuration.AutoCloseDelay;
				}
			}
		}
		delay = (flag ? num : 0f);
		return flag;
	}

	internal static float GetDefaultRadius(PrivateArea area)
	{
		return 8f;
	}

	internal static bool HandleManagedFlashEffect(PrivateArea area)
	{
		if (!WardAccess.IsManagedWard(area, requireEnabled: false) || area.m_flashEffect == null)
		{
			return true;
		}
		WardConfiguration configuration = GetConfiguration(area);
		if (configuration.WarningFlashEnabled && configuration.WarningSoundEnabled)
		{
			return true;
		}
		if (!configuration.WarningFlashEnabled && !configuration.WarningSoundEnabled)
		{
			return false;
		}
		PlayManagedWarningEffect(area, configuration.WarningSoundEnabled, configuration.WarningFlashEnabled);
		return false;
	}

	private static void PlayManagedWarningEffect(PrivateArea area, bool warningSoundEnabled, bool warningFlashEnabled)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!warningFlashEnabled && !warningSoundEnabled)
		{
			return;
		}
		GameObject[] array = area.m_flashEffect.Create(((Component)area).transform.position, Quaternion.identity, (Transform)null, 1f, -1);
		if (array == null)
		{
			return;
		}
		foreach (GameObject val in array)
		{
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			if (!warningSoundEnabled)
			{
				AudioSource[] componentsInChildren = val.GetComponentsInChildren<AudioSource>(true);
				foreach (AudioSource val2 in componentsInChildren)
				{
					if (!((Object)(object)val2 == (Object)null))
					{
						val2.mute = true;
						val2.volume = 0f;
						if (val2.isPlaying)
						{
							val2.Stop();
						}
					}
				}
			}
			if (warningFlashEnabled)
			{
				continue;
			}
			Renderer[] componentsInChildren2 = val.GetComponentsInChildren<Renderer>(true);
			foreach (Renderer val3 in componentsInChildren2)
			{
				if ((Object)(object)val3 != (Object)null)
				{
					val3.enabled = false;
				}
			}
			Light[] componentsInChildren3 = val.GetComponentsInChildren<Light>(true);
			foreach (Light val4 in componentsInChildren3)
			{
				if ((Object)(object)val4 != (Object)null)
				{
					((Behaviour)val4).enabled = false;
				}
			}
		}
	}

	internal static WardConfiguration WithWarningSoundEnabled(WardConfiguration configuration, bool enabled)
	{
		return new WardConfiguration(configuration.ShowAreaMarker, configuration.AreaMarkerSpeedMultiplier, configuration.AreaMarkerAlpha, configuration.Radius, configuration.AutoCloseDelay, enabled, configuration.WarningFlashEnabled, configuration.Restrictions);
	}

	internal static WardConfiguration WithWarningFlashEnabled(WardConfiguration configuration, bool enabled)
	{
		return new WardConfiguration(configuration.ShowAreaMarker, configuration.AreaMarkerSpeedMultiplier, configuration.AreaMarkerAlpha, configuration.Radius, configuration.AutoCloseDelay, configuration.WarningSoundEnabled, enabled, configuration.Restrictions);
	}

	internal static float GetMaxNonOverlappingRadius(PrivateArea area)
	{
		return Mathf.Max(8f, WardAccess.GetMaxNonOverlappingRadius(area, MaxRadius));
	}

	internal static WardConfigurationRequestSubmission RequestUpdateConfiguration(PrivateArea area, WardConfiguration configuration)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		ZNetView nView = GetNView(area);
		Player localPlayer = Player.m_localPlayer;
		WardConfiguration configuration2 = GetConfiguration(area);
		if ((Object)(object)nView == (Object)null || (Object)(object)localPlayer == (Object)null || !nView.IsValid())
		{
			return new WardConfigurationRequestSubmission(isPending: false, 0L, WardConfigurationRequestResultCode.InvalidState, configuration2, showOverlapMessage: false);
		}
		if (WardOwnership.CanApplyManagedWardStateLocally(nView))
		{
			if (!CanControlWard(area, localPlayer.GetPlayerID()))
			{
				return new WardConfigurationRequestSubmission(isPending: false, 0L, WardConfigurationRequestResultCode.Denied, configuration2, showOverlapMessage: false);
			}
			WardConfigurationUpdateResult wardConfigurationUpdateResult = ProcessConfigurationUpdate(area, configuration, configuration2);
			return new WardConfigurationRequestSubmission(isPending: false, 0L, wardConfigurationUpdateResult.ResultCode, wardConfigurationUpdateResult.Configuration, wardConfigurationUpdateResult.ShowOverlapMessage);
		}
		long num = AllocateConfigurationRequestId();
		ZPackage val = new ZPackage();
		val.Write(num);
		WriteConfiguration(val, configuration);
		Plugin.LogWardDiagnosticVerbose("UpdateSettings.Send", $"Sending per-ward UpdateSettings RPC for playerId={localPlayer.GetPlayerID()}, requestId={num}, {WardDiagnosticInfo.DescribeWard(area)}");
		nView.InvokeRPC("STUWard_UpdateSettings", new object[1] { val });
		return new WardConfigurationRequestSubmission(isPending: true, num, WardConfigurationRequestResultCode.Applied, configuration2, showOverlapMessage: false);
	}

	internal static void RequestRemovePermitted(PrivateArea area, long targetPlayerId)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		ZNetView nView = GetNView(area);
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)nView == (Object)null || (Object)(object)localPlayer == (Object)null || !nView.IsValid())
		{
			return;
		}
		if (WardOwnership.CanApplyManagedWardStateLocally(nView))
		{
			if (CanControlWard(area, localPlayer.GetPlayerID()))
			{
				uint dataRevision;
				bool num = ManagedWardRuntimeContexts.TryGetCurrentDataRevision(area, out dataRevision);
				area.RemovePermitted(targetPlayerId);
				if (num)
				{
					ManagedWardRuntimeContexts.ArmNextDataRevisionFanOutSuppressionIfChanged(area, dataRevision);
				}
			}
		}
		else
		{
			ZPackage val = new ZPackage();
			val.Write(targetPlayerId);
			Plugin.LogWardDiagnosticVerbose("RemovePermitted.Send", $"Sending per-ward RemovePermitted RPC for playerId={localPlayer.GetPlayerID()}, targetPlayerId={targetPlayerId}, {WardDiagnosticInfo.DescribeWard(area)}");
			nView.InvokeRPC("STUWard_RemovePermitted", new object[1] { val });
		}
	}

	internal static void RegisterRpcHandlers(PrivateArea area)
	{
		RegisterRpcHandlers(ManagedWardRef.FromArea(area));
	}

	internal static void RegisterRpcHandlers(ManagedWardRef ward)
	{
		PrivateArea area = ward.Area;
		if ((Object)(object)area == (Object)null)
		{
			return;
		}
		ZNetView val = ward.NView ?? GetNView(area);
		if (!((Object)(object)val == (Object)null) && val.IsValid() && !((Object)(object)((Component)area).GetComponent<WardSettingsRpcRegistrationState>() != (Object)null))
		{
			((Component)area).gameObject.AddComponent<WardSettingsRpcRegistrationState>();
			val.Register<ZPackage>("STUWard_UpdateSettings", (Action<long, ZPackage>)delegate(long sender, ZPackage pkg)
			{
				HandleUpdateConfiguration(area, sender, pkg);
			});
			val.Register<ZPackage>("STUWard_UpdateSettingsResponse", (Action<long, ZPackage>)delegate(long sender, ZPackage pkg)
			{
				HandleUpdateConfigurationResponse(area, sender, pkg);
			});
			val.Register<ZPackage>("STUWard_RemovePermitted", (Action<long, ZPackage>)delegate(long sender, ZPackage pkg)
			{
				HandleRemovePermitted(area, sender, pkg);
			});
		}
	}

	private static void HandleUpdateConfiguration(PrivateArea area, long sender, ZPackage pkg)
	{
		ZNetView nView = GetNView(area);
		WardConfiguration configuration = GetConfiguration(area);
		if ((Object)(object)nView == (Object)null || !WardOwnership.CanHandleManagedWardStateRpc(nView))
		{
			SendUpdateConfigurationResponse(nView, sender, 0L, new WardConfigurationUpdateResult(WardConfigurationRequestResultCode.InvalidState, configuration, showOverlapMessage: false));
			return;
		}
		if (!TryReadConfigurationRequest(pkg, out var requestId, out var configuration2))
		{
			SendUpdateConfigurationResponse(nView, sender, 0L, new WardConfigurationUpdateResult(WardConfigurationRequestResultCode.InvalidPayload, configuration, showOverlapMessage: false));
			return;
		}
		if (!WardOwnership.TryResolveAuthoritativePlayerIdFromSender(sender, "UpdateSettings.Request", out var playerId) || !CanControlWard(area, playerId))
		{
			SendUpdateConfigurationResponse(nView, sender, requestId, new WardConfigurationUpdateResult(WardConfigurationRequestResultCode.Denied, configuration, showOverlapMessage: false));
			return;
		}
		if (!WardOwnership.TryClaimManagedWardMutationOwnership(area, "UpdateSettings.Request"))
		{
			SendUpdateConfigurationResponse(nView, sender, requestId, new WardConfigurationUpdateResult(WardConfigurationRequestResultCode.InvalidState, configuration, showOverlapMessage: false));
			return;
		}
		WardConfigurationUpdateResult result = ProcessConfigurationUpdate(area, configuration2, configuration);
		SendUpdateConfigurationResponse(nView, sender, requestId, result);
	}

	private static void HandleUpdateConfigurationResponse(PrivateArea area, long _, ZPackage pkg)
	{
		if (TryReadConfigurationResponse(area, pkg, out var requestId, out var resultCode, out var configuration, out var showOverlapMessage))
		{
			ShowConfigurationRequestFeedback(resultCode, showOverlapMessage);
			WardGuiController.Instance?.HandleWardConfigurationResponse(area, requestId, resultCode, configuration);
		}
	}

	private static void HandleRemovePermitted(PrivateArea area, long sender, ZPackage? pkg)
	{
		ZNetView nView = GetNView(area);
		if (TryReadRemovePermittedRequest(pkg, out var targetPlayerId) && !((Object)(object)nView == (Object)null) && WardOwnership.CanHandleManagedWardStateRpc(nView) && WardOwnership.TryResolveAuthoritativePlayerIdFromSender(sender, "RemovePermitted.Request", out var playerId) && CanControlWard(area, playerId) && WardOwnership.TryClaimManagedWardMutationOwnership(area, "RemovePermitted.Request"))
		{
			uint dataRevision;
			bool num = ManagedWardRuntimeContexts.TryGetCurrentDataRevision(area, out dataRevision);
			area.RemovePermitted(targetPlayerId);
			if (num)
			{
				ManagedWardRuntimeContexts.ArmNextDataRevisionFanOutSuppressionIfChanged(area, dataRevision);
			}
		}
	}

	private static bool TryCreateConfiguration(bool showAreaMarker, float areaMarkerSpeedMultiplier, float areaMarkerAlpha, float radius, float autoCloseDelay, bool warningSoundEnabled, bool warningFlashEnabled, WardRestrictionOptions restrictions, out WardConfiguration configuration)
	{
		configuration = default(WardConfiguration);
		if (float.IsNaN(areaMarkerSpeedMultiplier) || float.IsInfinity(areaMarkerSpeedMultiplier) || float.IsNaN(areaMarkerAlpha) || float.IsInfinity(areaMarkerAlpha) || float.IsNaN(radius) || float.IsInfinity(radius) || float.IsNaN(autoCloseDelay) || float.IsInfinity(autoCloseDelay))
		{
			return false;
		}
		configuration = new WardConfiguration(showAreaMarker, Mathf.Clamp01(areaMarkerSpeedMultiplier), Mathf.Clamp01(areaMarkerAlpha), Mathf.Clamp(radius, 8f, MaxRadius), Mathf.Clamp(autoCloseDelay, 0f, 10f), warningSoundEnabled, warningFlashEnabled, NormalizeRestrictions(restrictions));
		return true;
	}

	private static void WriteConfiguration(ZPackage pkg, WardConfiguration configuration)
	{
		pkg.Write(configuration.ShowAreaMarker);
		pkg.Write(configuration.AreaMarkerSpeedMultiplier);
		pkg.Write(configuration.AreaMarkerAlpha);
		pkg.Write(configuration.Radius);
		pkg.Write(configuration.AutoCloseDelay);
		pkg.Write(configuration.WarningSoundEnabled);
		pkg.Write(configuration.WarningFlashEnabled);
		pkg.Write((int)configuration.Restrictions);
	}

	private static void SaveConfiguration(PrivateArea area, WardConfiguration currentConfiguration, WardConfiguration configuration)
	{
		ZDO zdo = GetZdo(area);
		if (zdo != null)
		{
			if (currentConfiguration.ShowAreaMarker != configuration.ShowAreaMarker)
			{
				zdo.Set("stuw_show_area_marker", configuration.ShowAreaMarker);
			}
			if (!Mathf.Approximately(currentConfiguration.AreaMarkerSpeedMultiplier, configuration.AreaMarkerSpeedMultiplier))
			{
				zdo.Set("stuw_area_marker_speed_multiplier", configuration.AreaMarkerSpeedMultiplier);
			}
			if (!Mathf.Approximately(currentConfiguration.AreaMarkerAlpha, configuration.AreaMarkerAlpha))
			{
				zdo.Set("stuw_area_marker_alpha", configuration.AreaMarkerAlpha);
			}
			if (!Mathf.Approximately(currentConfiguration.Radius, configuration.Radius))
			{
				zdo.Set("stuw_radius", configuration.Radius);
			}
			if (currentConfiguration.AutoCloseDoors != configuration.AutoCloseDoors)
			{
				zdo.Set("stuw_auto_close_doors", configuration.AutoCloseDoors);
			}
			if (!Mathf.Approximately(currentConfiguration.AutoCloseDelay, configuration.AutoCloseDelay))
			{
				zdo.Set("stuw_auto_close_delay", configuration.AutoCloseDelay);
			}
			if (currentConfiguration.WarningSoundEnabled != configuration.WarningSoundEnabled)
			{
				zdo.Set("stuw_warning_sound_enabled", configuration.WarningSoundEnabled);
			}
			if (currentConfiguration.WarningFlashEnabled != configuration.WarningFlashEnabled)
			{
				zdo.Set("stuw_warning_flash_enabled", configuration.WarningFlashEnabled);
			}
			if (currentConfiguration.Restrictions != configuration.Restrictions)
			{
				zdo.Set("stuw_restriction_options", (int)configuration.Restrictions);
			}
			ManagedWardRuntimeContext orCreate = ManagedWardRuntimeContexts.GetOrCreate(area);
			orCreate.CachedConfiguration = new CachedWardConfiguration(zdo.DataRevision, MaxRadius, ForcedRestrictions, configuration);
			orCreate.HasCachedConfiguration = true;
		}
	}

	private static WardConfigurationUpdateResult ProcessConfigurationUpdate(PrivateArea area, WardConfiguration requestedConfiguration, WardConfiguration currentConfiguration)
	{
		WardConfiguration wardConfiguration = ClampConfiguration(area, requestedConfiguration);
		bool flag = !Mathf.Approximately(currentConfiguration.Radius, wardConfiguration.Radius);
		bool showOverlapMessage = wardConfiguration.Radius < requestedConfiguration.Radius;
		if (ConfigurationsMatch(currentConfiguration, wardConfiguration))
		{
			return new WardConfigurationUpdateResult(WardConfigurationRequestResultCode.Unchanged, currentConfiguration, showOverlapMessage);
		}
		ManagedWardRuntimeContexts.ArmNextDataRevisionFanOutSuppression(area);
		SaveConfiguration(area, currentConfiguration, wardConfiguration);
		ManagedWardRef ward = ManagedWardRef.FromArea(area);
		ApplyAreaState(ward, wardConfiguration);
		if (flag)
		{
			ManagedWardMapStateService.NotifyLiveWardMutation(area, ManagedWardMapMutationKind.IndexAndPins, "ward radius updated");
			WardOwnership.ForceSyncManagedWardZdoToServer(ward, "UpdateSettings.Sync");
		}
		return new WardConfigurationUpdateResult(WardConfigurationRequestResultCode.Applied, wardConfiguration, showOverlapMessage);
	}

	private static WardConfiguration ClampConfiguration(PrivateArea area, WardConfiguration configuration)
	{
		float maxNonOverlappingRadius = WardAccess.GetMaxNonOverlappingRadius(area, MaxRadius);
		float radius = Mathf.Clamp(Mathf.Min(configuration.Radius, maxNonOverlappingRadius), 8f, MaxRadius);
		return new WardConfiguration(configuration.ShowAreaMarker, configuration.AreaMarkerSpeedMultiplier, configuration.AreaMarkerAlpha, radius, configuration.AutoCloseDelay, configuration.WarningSoundEnabled, configuration.WarningFlashEnabled, ApplyForcedRestrictions(configuration.Restrictions));
	}

	private static void ApplyAreaMarkerVisuals(CircleProjector marker, WardConfiguration configuration)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		List<GameObject> segments = marker.m_segments;
		if (segments == null || segments.Count == 0)
		{
			return;
		}
		Vector3 baseScale = (((Object)(object)marker.m_prefab != (Object)null) ? marker.m_prefab.transform.localScale : Vector3.one);
		float lengthScale = Mathf.Clamp(configuration.Radius / MaxRadius * 2f, 0f, 2f);
		for (int i = 0; i < segments.Count; i++)
		{
			GameObject val = segments[i];
			if (!((Object)(object)val == (Object)null))
			{
				val.transform.localScale = ScaleMarkerSegment(baseScale, lengthScale);
				ApplyAreaMarkerAlpha(val, configuration.AreaMarkerAlpha);
			}
		}
	}

	private static Vector3 ScaleMarkerSegment(Vector3 baseScale, float lengthScale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (baseScale.x > baseScale.z)
		{
			return new Vector3(baseScale.x * lengthScale, baseScale.y, baseScale.z);
		}
		return new Vector3(baseScale.x, baseScale.y, baseScale.z * lengthScale);
	}

	private static void ApplyAreaMarkerAlpha(GameObject segment, float alpha)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(alpha);
		float num2 = Mathf.Lerp(0.5f, 1f, num);
		float num3 = Mathf.Lerp(0.35f, 1f, Mathf.Pow(num2, 1.8f));
		Renderer[] componentsInChildren = segment.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer val in componentsInChildren)
		{
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			Material sharedMaterial = val.sharedMaterial;
			if ((Object)(object)sharedMaterial == (Object)null)
			{
				continue;
			}
			AreaMarkerPropertyBlock.Clear();
			bool flag = false;
			for (int j = 0; j < AreaMarkerColorProperties.Length; j++)
			{
				string text = AreaMarkerColorProperties[j];
				if (sharedMaterial.HasProperty(text))
				{
					Color color = sharedMaterial.GetColor(text);
					color.r *= num3;
					color.g *= num3;
					color.b *= num3;
					AreaMarkerPropertyBlock.SetColor(text, color);
					flag = true;
				}
			}
			if (flag)
			{
				val.SetPropertyBlock(AreaMarkerPropertyBlock);
			}
		}
	}

	private static void ApplyManagedAreaMarkerVisibility(PrivateArea area, bool visible)
	{
		if (visible)
		{
			((MonoBehaviour)area).CancelInvoke("HideMarker");
		}
		GameObject gameObject = ((Component)area.m_areaMarker).gameObject;
		if (gameObject.activeSelf != visible)
		{
			gameObject.SetActive(visible);
		}
	}

	private static bool ShouldRefreshAreaMarkerVisuals(PrivateArea area, CircleProjector marker, WardConfiguration configuration)
	{
		if (!TryBuildAreaMarkerVisualState(marker, configuration, out var visualState))
		{
			ManagedWardRuntimeContexts.ClearAreaMarkerVisualState(area);
			return false;
		}
		ManagedWardRuntimeContext orCreate = ManagedWardRuntimeContexts.GetOrCreate(area);
		if (orCreate.HasAreaMarkerVisualState)
		{
			return !AreaMarkerVisualStatesMatch(orCreate.AreaMarkerVisualState, visualState);
		}
		return true;
	}

	private static void CacheAreaMarkerVisualState(PrivateArea area, CircleProjector marker, WardConfiguration configuration)
	{
		if (!TryBuildAreaMarkerVisualState(marker, configuration, out var visualState))
		{
			ManagedWardRuntimeContexts.ClearAreaMarkerVisualState(area);
			return;
		}
		ManagedWardRuntimeContext orCreate = ManagedWardRuntimeContexts.GetOrCreate(area);
		orCreate.AreaMarkerVisualState = visualState;
		orCreate.HasAreaMarkerVisualState = true;
	}

	private static bool TryBuildAreaMarkerVisualState(CircleProjector marker, WardConfiguration configuration, out CachedAreaMarkerVisualState visualState)
	{
		visualState = default(CachedAreaMarkerVisualState);
		List<GameObject> segments = marker.m_segments;
		if (segments == null || segments.Count == 0)
		{
			return false;
		}
		GameObject val = segments[0];
		GameObject val2 = segments[segments.Count - 1];
		visualState = new CachedAreaMarkerVisualState(((Object)marker).GetInstanceID(), segments.Count, ((Object)(object)val != (Object)null) ? ((Object)val).GetInstanceID() : 0, ((Object)(object)val2 != (Object)null) ? ((Object)val2).GetInstanceID() : 0, MaxRadius, configuration.Radius, configuration.AreaMarkerAlpha);
		return true;
	}

	private static bool AreaMarkerVisualStatesMatch(CachedAreaMarkerVisualState left, CachedAreaMarkerVisualState right)
	{
		if (left.MarkerInstanceId == right.MarkerInstanceId && left.SegmentCount == right.SegmentCount && left.FirstSegmentInstanceId == right.FirstSegmentInstanceId && left.LastSegmentInstanceId == right.LastSegmentInstanceId && Mathf.Approximately(left.MaxRadius, right.MaxRadius) && Mathf.Approximately(left.Radius, right.Radius))
		{
			return Mathf.Approximately(left.AreaMarkerAlpha, right.AreaMarkerAlpha);
		}
		return false;
	}

	private static float GetDefaultAreaMarkerSpeed(PrivateArea area)
	{
		if (!ManagedWardRuntimeContexts.TryGet(area, out ManagedWardRuntimeContext context) || !context.HasDefaultAreaMarkerSpeed)
		{
			return 0.1f;
		}
		return context.DefaultAreaMarkerSpeed;
	}

	private static bool CanControlWard(PrivateArea area, long playerId)
	{
		return WardAccess.CanControlManagedWard(area, playerId);
	}

	private static long AllocateConfigurationRequestId()
	{
		if (_nextConfigurationRequestId == long.MaxValue)
		{
			_nextConfigurationRequestId = 1L;
		}
		return _nextConfigurationRequestId++;
	}

	private static bool TryReadConfigurationRequest(ZPackage? pkg, out long requestId, out WardConfiguration configuration)
	{
		requestId = 0L;
		configuration = default(WardConfiguration);
		if (pkg == null)
		{
			return false;
		}
		try
		{
			requestId = pkg.ReadLong();
			return requestId != 0L && TryReadConfigurationPayload(pkg, out configuration);
		}
		catch
		{
			requestId = 0L;
			configuration = default(WardConfiguration);
			return false;
		}
	}

	private static bool TryReadRemovePermittedRequest(ZPackage? pkg, out long targetPlayerId)
	{
		targetPlayerId = 0L;
		if (pkg == null)
		{
			return false;
		}
		try
		{
			targetPlayerId = pkg.ReadLong();
			return targetPlayerId != 0;
		}
		catch
		{
			targetPlayerId = 0L;
			return false;
		}
	}

	private static void SendUpdateConfigurationResponse(ZNetView? nview, long receiverUid, long requestId, WardConfigurationUpdateResult result)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		if (!((Object)(object)nview == (Object)null) && receiverUid != 0L)
		{
			ZPackage val = new ZPackage();
			val.Write(requestId);
			val.Write((int)result.ResultCode);
			val.Write(result.ShowOverlapMessage);
			WriteConfiguration(val, result.Configuration);
			nview.InvokeRPC(receiverUid, "STUWard_UpdateSettingsResponse", new object[1] { val });
		}
	}

	private static bool TryReadConfigurationResponse(PrivateArea area, ZPackage? pkg, out long requestId, out WardConfigurationRequestResultCode resultCode, out WardConfiguration configuration, out bool showOverlapMessage)
	{
		requestId = 0L;
		resultCode = WardConfigurationRequestResultCode.InvalidState;
		configuration = GetConfiguration(area);
		showOverlapMessage = false;
		if (pkg == null)
		{
			return false;
		}
		try
		{
			requestId = pkg.ReadLong();
			resultCode = (WardConfigurationRequestResultCode)pkg.ReadInt();
			showOverlapMessage = pkg.ReadBool();
			return TryReadConfigurationPayload(pkg, out configuration);
		}
		catch
		{
			requestId = 0L;
			resultCode = WardConfigurationRequestResultCode.InvalidState;
			configuration = GetConfiguration(area);
			showOverlapMessage = false;
			return false;
		}
	}

	private static bool TryReadConfigurationPayload(ZPackage pkg, out WardConfiguration configuration)
	{
		configuration = default(WardConfiguration);
		if (pkg == null)
		{
			return false;
		}
		try
		{
			bool showAreaMarker = pkg.ReadBool();
			float areaMarkerSpeedMultiplier = pkg.ReadSingle();
			float areaMarkerAlpha = pkg.ReadSingle();
			float radius = pkg.ReadSingle();
			float autoCloseDelay = pkg.ReadSingle();
			bool warningSoundEnabled = pkg.ReadBool();
			bool warningFlashEnabled = pkg.ReadBool();
			WardRestrictionOptions restrictions = (WardRestrictionOptions)pkg.ReadInt();
			return TryCreateConfiguration(showAreaMarker, areaMarkerSpeedMultiplier, areaMarkerAlpha, radius, autoCloseDelay, warningSoundEnabled, warningFlashEnabled, restrictions, out configuration);
		}
		catch
		{
			configuration = default(WardConfiguration);
			return false;
		}
	}

	internal static void ShowConfigurationRequestFeedback(WardConfigurationRequestResultCode resultCode, bool showOverlapMessage)
	{
		Player localPlayer = Player.m_localPlayer;
		if (showOverlapMessage)
		{
			WardAccess.ShowWardOverlapMessage(localPlayer);
		}
		if (resultCode == WardConfigurationRequestResultCode.Denied)
		{
			WardAccess.ShowNoAccessMessage(localPlayer);
		}
	}

	internal static bool ConfigurationsMatch(WardConfiguration left, WardConfiguration right)
	{
		if (left.ShowAreaMarker == right.ShowAreaMarker && Mathf.Approximately(left.AreaMarkerSpeedMultiplier, right.AreaMarkerSpeedMultiplier) && Mathf.Approximately(left.AreaMarkerAlpha, right.AreaMarkerAlpha) && Mathf.Approximately(left.Radius, right.Radius) && Mathf.Approximately(left.AutoCloseDelay, right.AutoCloseDelay) && left.WarningSoundEnabled == right.WarningSoundEnabled && left.WarningFlashEnabled == right.WarningFlashEnabled)
		{
			return left.Restrictions == right.Restrictions;
		}
		return false;
	}

	private static WardRestrictionOptions ApplyForcedRestrictions(WardRestrictionOptions restrictions)
	{
		return ApplyForcedRestrictions(restrictions, ForcedRestrictions);
	}

	private static WardRestrictionOptions ApplyForcedRestrictions(WardRestrictionOptions restrictions, WardRestrictionOptions forcedRestrictions)
	{
		return NormalizeRestrictions(restrictions) | forcedRestrictions;
	}

	private static WardRestrictionOptions NormalizeRestrictions(WardRestrictionOptions restrictions)
	{
		return restrictions & WardRestrictionOptions.All;
	}

	private static ZNetView? GetNView(PrivateArea area)
	{
		return WardPrivateAreaSafeAccess.GetNView(area);
	}

	private static ZDO? GetZdo(PrivateArea area)
	{
		return WardPrivateAreaSafeAccess.GetZdo(area);
	}
}
