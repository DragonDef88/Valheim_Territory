using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "OnDestroy")]
internal static class PrivateAreaOnDestroyPatch
{
	private static void Prefix(PrivateArea __instance)
	{
		ManagedWardLifecycle.NotifyAreaDestroyed(__instance);
	}
}
