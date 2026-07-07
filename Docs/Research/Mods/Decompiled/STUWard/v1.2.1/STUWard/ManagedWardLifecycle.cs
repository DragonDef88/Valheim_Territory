using UnityEngine;

namespace STUWard;

internal static class ManagedWardLifecycle
{
	internal static void NotifyAreaReady(PrivateArea? area, bool matchedByComponent, bool matchedByZdo)
	{
		ManagedWardRef ward = ManagedWardRef.FromArea(area);
		if (!((Object)(object)area == (Object)null) && (matchedByComponent || matchedByZdo) && ManagedWardInitializationCoordinator.TryGetValidZdo(ward, out ZDO _) && ManagedWardIdentity.EnsureManagedComponent(ward))
		{
			ManagedWardRuntimeContext orCreate = ManagedWardRuntimeContexts.GetOrCreate(area);
			if (!orCreate.NetworkInitializationComplete)
			{
				WardOwnership.TryStampLocalManagedWardOwnerAccount(ward);
				WardAccess.RegisterManagedWard(ward);
				WardPermittedSnapshots.Backfill(ward);
				WardSettings.RegisterRpcHandlers(ward);
				WardOwnership.RegisterManagedWardRpcHandlers(area);
				WardSettings.ApplyAreaState(ward);
				orCreate.NetworkInitializationComplete = true;
			}
			if (!orCreate.OwnershipObserved)
			{
				WardOwnership.ObserveManagedWard(ward);
				orCreate.OwnershipObserved = true;
			}
		}
	}

	internal static void NotifyAreaDestroyed(PrivateArea? area)
	{
		if (!((Object)(object)area == (Object)null))
		{
			WardAccess.UnregisterManagedWard(area);
			ManagedWardRuntimeContexts.Forget(area);
		}
	}

	internal static void NotifySessionReset()
	{
		ManagedWardRuntimeLifecycle.ResetSession();
		ManagedWardRuntimeLifecycle.BindNetwork();
	}
}
