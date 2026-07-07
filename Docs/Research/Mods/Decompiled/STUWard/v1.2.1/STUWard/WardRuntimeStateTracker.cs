using UnityEngine;

namespace STUWard;

internal static class WardRuntimeStateTracker
{
	internal static void Forget(PrivateArea? area)
	{
		ManagedWardRuntimeContexts.ClearObservedState(area);
	}

	internal static void Reset()
	{
		ManagedWardRuntimeContexts.ClearObservedStates();
	}

	internal static bool TryConsumeChanges(PrivateArea? area, out bool enabledChanged, out bool dataRevisionChanged)
	{
		enabledChanged = false;
		dataRevisionChanged = false;
		if (!TryBuildObservedState(area, out var state))
		{
			ManagedWardRuntimeContexts.ClearObservedState(area);
			return false;
		}
		ManagedWardRuntimeContext orCreate = ManagedWardRuntimeContexts.GetOrCreate(area);
		if (!orCreate.HasObservedState)
		{
			orCreate.ObservedState = state;
			orCreate.HasObservedState = true;
			return false;
		}
		ManagedWardObservedState observedState = orCreate.ObservedState;
		enabledChanged = observedState.Enabled != state.Enabled;
		dataRevisionChanged = observedState.DataRevision != state.DataRevision;
		if (!enabledChanged && !dataRevisionChanged)
		{
			return false;
		}
		orCreate.ObservedState = state;
		return true;
	}

	private static bool TryBuildObservedState(PrivateArea? area, out ManagedWardObservedState state)
	{
		state = default(ManagedWardObservedState);
		if ((Object)(object)area == (Object)null)
		{
			return false;
		}
		ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(area);
		if (zdo == null)
		{
			return false;
		}
		state = new ManagedWardObservedState(area.IsEnabled(), zdo.DataRevision);
		return true;
	}
}
