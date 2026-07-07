using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch]
internal static class DirectInteractionPatches
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return WardInteractionPatchTargets.GetDirectInteractTargets();
	}

	private static bool Prefix(Component __instance, Humanoid __0, ref bool __result, out WardCheckScopeState __state)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		__state = default(WardCheckScopeState);
		Player player = WardAccess.GetPlayer(__0);
		bool flag = TryHandleDirectInteraction(__instance, player, ref __result, ref __state);
		if (Plugin.ShouldLogWardDiagnosticVerbose() && (Object)(object)__instance != (Object)null && (Object)(object)player != (Object)null)
		{
			Plugin.LogWardDiagnosticVerbose("Access.DirectInteract", $"Intercepted direct interact. targetType={((object)__instance).GetType().Name}, targetName='{((Object)__instance).name}', playerId={player.GetPlayerID()}, continueOriginal={flag}, result={__result}, position={__instance.transform.position}");
		}
		return flag;
	}

	private static void Postfix(Component __instance, Humanoid __0, ref bool __result, WardCheckScopeState __state)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		__state.Dispose();
		Player player = WardAccess.GetPlayer(__0);
		if (Plugin.ShouldLogWardDiagnosticVerbose() && !((Object)(object)__instance == (Object)null) && !((Object)(object)player == (Object)null))
		{
			Plugin.LogWardDiagnosticVerbose("Access.DirectInteract.Result", $"Completed direct interact. targetType={((object)__instance).GetType().Name}, targetName='{((Object)__instance).name}', playerId={player.GetPlayerID()}, result={__result}, position={__instance.transform.position}");
		}
	}

	private static bool TryHandleDirectInteraction(Component target, Player? player, ref bool result, ref WardCheckScopeState scopeState)
	{
		ItemDrop val = (ItemDrop)(object)((target is ItemDrop) ? target : null);
		if (val != null)
		{
			if (WardAccess.IsPlacedConsumable(val))
			{
				return TryBlockWithRestriction(WardRestrictionOptions.PlacedConsumables, (Component)(object)val, player, ref result, ref scopeState);
			}
			if (!WardItemPrefabPolicy.CanAnyPickupBeBlocked() || !WardItemPrefabPolicy.ShouldBlockPickup(val))
			{
				scopeState.EnterManagedWardAllow();
				return true;
			}
			if (!WardAccess.ShouldBlockPickup(val, player))
			{
				scopeState.EnterRestriction(WardRestrictionOptions.Pickup);
				return true;
			}
			WardAccess.ShowNoAccessMessage(player);
			result = true;
			return false;
		}
		if (WardInteractionPatchTargets.TryGetRestriction(target, out var restriction))
		{
			return TryBlockWithRestriction(restriction, target, player, ref result, ref scopeState);
		}
		return WardAccess.TryBlockInteraction(target, player, ref result);
	}

	private static bool TryBlockWithRestriction(WardRestrictionOptions restriction, Component target, Player? player, ref bool result, ref WardCheckScopeState scopeState)
	{
		bool num = WardAccess.TryBlockInteraction(restriction, target, player, ref result);
		if (num)
		{
			scopeState.EnterRestriction(restriction);
		}
		return num;
	}
}
