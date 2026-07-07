using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "AddPermitted")]
internal static class PrivateAreaAddPermittedSnapshotPatch
{
	private static void Postfix(PrivateArea __instance, long playerID)
	{
		ManagedWardRef ward = ManagedWardRef.FromArea(__instance);
		if (WardAccess.IsManagedWard(ward, requireEnabled: false))
		{
			WardPermittedSnapshots.Capture(__instance, playerID);
			ManagedWardPresenceService.Invalidate();
			ManagedWardMapStateService.NotifyLiveWardMutation(__instance, ManagedWardMapMutationKind.IndexAndPins, "ward permitted player added");
			WardOwnership.ForceSyncManagedWardZdoToServer(ward, "TogglePermitted.Sync");
		}
	}
}
