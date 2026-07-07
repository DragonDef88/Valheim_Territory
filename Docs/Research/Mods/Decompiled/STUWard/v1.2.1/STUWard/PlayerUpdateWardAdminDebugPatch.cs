using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(Player), "Update")]
internal static class PlayerUpdateWardAdminDebugPatch
{
	private static void Postfix(Player __instance)
	{
		WardAdminDebugAccess.UpdateLocalState(__instance);
	}
}
