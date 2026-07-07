using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(Player), "Start")]
internal static class PlayerStartWardOwnershipPatch
{
	private static void Postfix(Player __instance)
	{
		WardGuiController.Instance?.CloseWardUi();
		WardOwnership.RefreshServerPlayerAccountIdForPlayer(__instance);
		WardAdminDebugAccess.UpdateLocalState(__instance, force: true);
		WardMinimapPinsManager.UpdateLocalState(__instance, force: true);
		GuildsCompat.OnLocalPlayerStarted(__instance);
	}
}
