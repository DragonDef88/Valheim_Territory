using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(ZNet), "Awake")]
internal static class ZNetAwakeWardOwnershipPatch
{
	private static void Postfix()
	{
		ManagedWardLifecycle.NotifySessionReset();
	}
}
