using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "RemovePermitted")]
internal static class PrivateAreaRemovePermittedSnapshotPatch
{
	private static void Postfix(PrivateArea __instance, long playerID)
	{
		ManagedWardRef ward = ManagedWardRef.FromArea(__instance);
		if (WardAccess.IsManagedWard(ward, requireEnabled: false))
		{
			WardPermittedSnapshots.Remove(__instance, playerID);
			ManagedWardPresenceService.Invalidate();
			ManagedWardMapStateService.NotifyLiveWardMutation(__instance, ManagedWardMapMutationKind.IndexAndPins, "ward permitted player removed");
			WardOwnership.ForceSyncManagedWardZdoToServer(ward, "TogglePermitted.Sync");
		}
	}
}
