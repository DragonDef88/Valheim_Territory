using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(ItemDrop), "Pickup")]
internal static class ItemDropPickupPatch
{
	private static bool Prefix(ItemDrop __instance, Humanoid character, out WardCheckScopeState __state)
	{
		__state = default(WardCheckScopeState);
		Player player = WardAccess.GetPlayer(character);
		if (!WardItemPrefabPolicy.CanAnyPickupBeBlocked() || !WardItemPrefabPolicy.ShouldBlockPickup(__instance))
		{
			__state.EnterManagedWardAllow();
			return true;
		}
		if (!WardAccess.ShouldBlockPickup(__instance, player))
		{
			__state.EnterRestriction(WardRestrictionOptions.Pickup);
			return true;
		}
		WardAccess.ShowNoAccessMessage(player);
		return false;
	}

	private static void Postfix(WardCheckScopeState __state)
	{
		__state.Dispose();
	}
}
