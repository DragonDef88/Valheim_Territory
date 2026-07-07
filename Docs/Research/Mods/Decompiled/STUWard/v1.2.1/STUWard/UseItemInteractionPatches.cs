using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch]
internal static class UseItemInteractionPatches
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return WardInteractionPatchTargets.GetCommonTargets("UseItem");
	}

	private static bool Prefix(Component __instance, Humanoid __0, ref bool __result, out WardCheckScopeState __state)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		__state = default(WardCheckScopeState);
		Player player = WardAccess.GetPlayer(__0);
		WardRestrictionOptions restriction;
		bool flag = (WardInteractionPatchTargets.TryGetRestriction(__instance, out restriction) ? WardAccess.TryBlockInteraction(restriction, __instance, player, ref __result) : WardAccess.TryBlockInteraction(__instance, player, ref __result));
		if (flag && restriction != 0)
		{
			__state.EnterRestriction(restriction);
		}
		if (Plugin.ShouldLogWardDiagnosticVerbose() && (Object)(object)__instance != (Object)null && (Object)(object)player != (Object)null)
		{
			Plugin.LogWardDiagnosticVerbose("Access.UseItem", $"Intercepted use-item interaction. targetType={((object)__instance).GetType().Name}, targetName='{((Object)__instance).name}', playerId={player.GetPlayerID()}, continueOriginal={flag}, result={__result}, position={__instance.transform.position}");
		}
		return flag;
	}

	private static void Postfix(WardCheckScopeState __state)
	{
		__state.Dispose();
	}
}
