using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Humanoid), "Pickup")]
internal static class HumanoidPickupPatch
{
	private static bool Prefix(Humanoid __instance, GameObject go, ref bool __result, out WardCheckScopeState __state)
	{
		__state = default(WardCheckScopeState);
		Player player = WardAccess.GetPlayer(__instance);
		if (!WardItemPrefabPolicy.CanAnyPickupBeBlocked() || !WardItemPrefabPolicy.ShouldBlockPickup(go))
		{
			__state.EnterManagedWardAllow();
			return true;
		}
		if (!WardAccess.ShouldBlockPickup(go, player))
		{
			__state.EnterRestriction(WardRestrictionOptions.Pickup);
			return true;
		}
		__result = false;
		return false;
	}

	private static void Postfix(WardCheckScopeState __state)
	{
		__state.Dispose();
	}
}
