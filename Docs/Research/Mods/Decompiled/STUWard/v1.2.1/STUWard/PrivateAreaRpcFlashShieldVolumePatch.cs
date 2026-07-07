using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "RPC_FlashShield")]
internal static class PrivateAreaRpcFlashShieldVolumePatch
{
	private static bool Prefix(PrivateArea __instance)
	{
		return WardSettings.HandleManagedFlashEffect(__instance);
	}
}
