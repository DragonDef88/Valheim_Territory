using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "RPC_ToggleEnabled")]
internal static class PrivateAreaRpcToggleEnabledAdminDebugPatch
{
	private static bool Prefix(PrivateArea __instance, long uid, long playerID)
	{
		if (!ManagedWardInteractionRpc.IsManagedWardForHooks(__instance))
		{
			return true;
		}
		ManagedWardInteractionRpc.TryHandleVanillaToggleEnabled(__instance, uid, playerID);
		return false;
	}
}
