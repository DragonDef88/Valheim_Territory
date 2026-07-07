using UnityEngine;

namespace STUWard;

internal static class ManagedWardInitializationCoordinator
{
	internal static void EnsureLocalInitialization(PrivateArea area)
	{
		if (!((Object)(object)area == (Object)null) && ManagedWardIdentity.EnsureManagedComponent(area))
		{
			ManagedWardLocalInitializationState orAddState = GetOrAddState(area);
			if (!orAddState.LocalInitializationComplete)
			{
				WardSettings.CaptureAreaDefaults(area);
				orAddState.LocalInitializationComplete = true;
			}
		}
	}

	internal static void EnsureNetworkInitialization(PrivateArea area, bool matchedByComponent, bool matchedByZdo)
	{
		ManagedWardLifecycle.NotifyAreaReady(area, matchedByComponent, matchedByZdo);
	}

	internal static bool TryGetValidZdo(PrivateArea area, out ZDO zdo)
	{
		return TryGetValidZdo(ManagedWardRef.FromArea(area), out zdo);
	}

	internal static bool TryGetValidZdo(ManagedWardRef ward, out ZDO zdo)
	{
		zdo = null;
		if (!ward.HasValidNetworkIdentity)
		{
			return false;
		}
		zdo = ward.Zdo;
		return true;
	}

	internal static ManagedWardLocalInitializationState GetOrAddState(PrivateArea area)
	{
		ManagedWardLocalInitializationState component = ((Component)area).GetComponent<ManagedWardLocalInitializationState>();
		if ((Object)(object)component != (Object)null)
		{
			return component;
		}
		return ((Component)area).gameObject.AddComponent<ManagedWardLocalInitializationState>();
	}
}
