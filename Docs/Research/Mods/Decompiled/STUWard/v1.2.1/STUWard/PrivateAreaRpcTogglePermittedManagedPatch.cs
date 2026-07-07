using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "RPC_TogglePermitted")]
internal static class PrivateAreaRpcTogglePermittedManagedPatch
{
	private static bool Prefix(PrivateArea __instance, long uid, long playerID, string name)
	{
		if (!ManagedWardInteractionRpc.IsManagedWardForHooks(__instance))
		{
			return true;
		}
		ManagedWardInteractionRpc.TryHandleVanillaTogglePermitted(__instance, uid, playerID, name);
		return false;
	}
}
