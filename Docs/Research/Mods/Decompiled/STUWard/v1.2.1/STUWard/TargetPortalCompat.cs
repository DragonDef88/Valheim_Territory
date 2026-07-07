using System;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

internal static class TargetPortalCompat
{
	private const string PluginGuid = "org.bepinex.plugins.targetportal";

	private static readonly Assembly? TargetPortalAssembly = GetPluginAssembly();

	private static readonly Type? MapType = TargetPortalAssembly?.GetType("TargetPortal.Map");

	private static readonly Type? OpenMapPatchType = TargetPortalAssembly?.GetType("TargetPortal.Map+OpenMapOnPortalEnter");

	private static readonly Type? TogglePortalModePatchType = TargetPortalAssembly?.GetType("TargetPortal.TargetPortal+TogglePortalMode");

	private static readonly Type? StartPortalFetchingPatchType = TargetPortalAssembly?.GetType("TargetPortal.TargetPortal+StartPortalFetching");

	private static readonly MethodInfo? OpenMapPrefixMethod = ((OpenMapPatchType != null) ? AccessTools.Method(OpenMapPatchType, "Prefix", new Type[2]
	{
		typeof(TeleportWorldTrigger),
		typeof(Collider)
	}, (Type[])null) : null);

	private static readonly MethodInfo? TogglePortalModePrefixMethod = ((TogglePortalModePatchType != null) ? AccessTools.Method(TogglePortalModePatchType, "Prefix", new Type[2]
	{
		typeof(TeleportWorld),
		typeof(bool)
	}, (Type[])null) : null);

	private static readonly MethodInfo? OnPortalModeChangeMethod = ((StartPortalFetchingPatchType != null) ? AccessTools.Method(StartPortalFetchingPatchType, "OnPortalModeChange", new Type[5]
	{
		typeof(long),
		typeof(ZDOID),
		typeof(int),
		typeof(string),
		typeof(string)
	}, (Type[])null) : null);

	private static readonly MethodInfo? HandlePortalClickMethod = ((MapType != null) ? AccessTools.Method(MapType, "HandlePortalClick", (Type[])null, (Type[])null) : null);

	private static readonly MethodInfo? CancelTeleportMethod = ((MapType != null) ? AccessTools.Method(MapType, "CancelTeleport", Type.EmptyTypes, (Type[])null) : null);

	private static bool _blockedPortalEntry;

	private static Assembly? GetPluginAssembly()
	{
		if (!Chainloader.PluginInfos.TryGetValue("org.bepinex.plugins.targetportal", out var value))
		{
			return null;
		}
		return ((object)value.Instance)?.GetType().Assembly;
	}

	internal static void TryPatch(Harmony harmony)
	{
		if (!(TargetPortalAssembly == null) && harmony != null)
		{
			PatchMethod(harmony, OpenMapPrefixMethod, "TargetPortalOpenMapPrefix");
			PatchMethod(harmony, TogglePortalModePrefixMethod, "TargetPortalTogglePortalModePrefix");
			PatchMethod(harmony, OnPortalModeChangeMethod, "TargetPortalOnPortalModeChangePrefix");
			PatchMethod(harmony, HandlePortalClickMethod, "TargetPortalHandlePortalClickPrefix");
		}
	}

	internal static void MarkBlockedPortalEntry(Player? player)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)Player.m_localPlayer))
		{
			_blockedPortalEntry = true;
		}
	}

	internal static void ClearBlockedPortalEntry(Player? player)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)Player.m_localPlayer))
		{
			_blockedPortalEntry = false;
		}
	}

	internal static void ClosePortalSelection()
	{
		_blockedPortalEntry = false;
		try
		{
			if ((Object)(object)Minimap.instance != (Object)null)
			{
				Minimap.instance.SetMapMode((MapMode)1);
			}
		}
		catch
		{
		}
		try
		{
			CancelTeleportMethod?.Invoke(null, Array.Empty<object>());
		}
		catch
		{
		}
	}

	private static void PatchMethod(Harmony harmony, MethodInfo? originalMethod, string prefixName)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		if (!(originalMethod == null))
		{
			MethodInfo methodInfo = AccessTools.DeclaredMethod(typeof(TargetPortalCompat), prefixName, (Type[])null, (Type[])null);
			if (!(methodInfo == null))
			{
				harmony.Patch((MethodBase)originalMethod, new HarmonyMethod(methodInfo), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
	}

	private static bool TargetPortalOpenMapPrefix(TeleportWorldTrigger __0, Collider __1)
	{
		Player player = WardAccess.GetPlayer(__1);
		if ((Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)Player.m_localPlayer)
		{
			return true;
		}
		TeleportWorld val = (((Object)(object)__0 != (Object)null) ? ((Component)__0).GetComponentInParent<TeleportWorld>() : null);
		if ((Object)(object)val == (Object)null)
		{
			ClearBlockedPortalEntry(player);
			return true;
		}
		if (WardAccess.TryBlockVoid(WardRestrictionOptions.Portals, (Component)(object)val, player))
		{
			ClearBlockedPortalEntry(player);
			return true;
		}
		MarkBlockedPortalEntry(player);
		ClosePortalSelection();
		return false;
	}

	private static bool TargetPortalHandlePortalClickPrefix()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			_blockedPortalEntry = false;
			return true;
		}
		if (!_blockedPortalEntry && !WardAccess.ShouldBlockRestriction(WardRestrictionOptions.Portals, ((Component)localPlayer).transform.position, 0f, localPlayer, flash: false))
		{
			return true;
		}
		WardAccess.ShowNoAccessMessage(localPlayer);
		ClosePortalSelection();
		return false;
	}

	private static bool TargetPortalTogglePortalModePrefix(TeleportWorld __0, bool __1)
	{
		if (__1)
		{
			return true;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null || (Object)(object)__0 == (Object)null)
		{
			return true;
		}
		return !WardAccess.ShouldBlockRestriction(WardRestrictionOptions.Portals, (Component?)(object)__0, localPlayer, 0f, flash: false);
	}

	private static bool TargetPortalOnPortalModeChangePrefix(long __0, ZDOID __1)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!WardOwnership.TryResolveAuthoritativePlayerIdFromSender(__0, "TargetPortal.OnPortalModeChange", out var playerId))
		{
			return false;
		}
		ZDOMan instance = ZDOMan.instance;
		ZDO val = ((instance != null) ? instance.GetZDO(__1) : null);
		if (val == null)
		{
			return true;
		}
		return WardAccess.CheckRestrictionAccess(WardRestrictionOptions.Portals, val.GetPosition(), 0f, playerId, flash: false);
	}
}
