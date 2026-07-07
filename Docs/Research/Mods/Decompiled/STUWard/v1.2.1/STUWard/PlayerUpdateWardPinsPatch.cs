using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(Player), "Update")]
internal static class PlayerUpdateWardPinsPatch
{
	private static void Postfix(Player __instance)
	{
		WardMinimapPinsManager.UpdatePendingRemoteState(__instance);
	}
}
