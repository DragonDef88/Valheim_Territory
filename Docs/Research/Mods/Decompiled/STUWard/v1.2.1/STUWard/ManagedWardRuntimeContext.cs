namespace STUWard;

internal sealed class ManagedWardRuntimeContext
{
	internal bool NetworkInitializationComplete;

	internal bool OwnershipObserved;

	internal bool HasDefaultAreaMarkerSpeed;

	internal float DefaultAreaMarkerSpeed;

	internal bool HasCachedConfiguration;

	internal CachedWardConfiguration CachedConfiguration;

	internal bool HasAreaMarkerVisualState;

	internal CachedAreaMarkerVisualState AreaMarkerVisualState;

	internal bool HasObservedState;

	internal ManagedWardObservedState ObservedState;

	internal int SetEnabledInvocationDepth;

	internal bool HasPendingEnabledFanOutSuppression;

	internal bool PendingEnabledFanOutState;

	internal bool HasPendingDataRevisionFanOutSuppression;

	internal uint PendingDataRevisionFanOutBaseline;

	internal bool HasHoverText;

	internal ManagedWardHoverTextCacheEntry HoverText;

	internal float PresenceLastTrustedNearbyTime = float.NegativeInfinity;

	internal void ClearConfigurationCaches()
	{
		HasCachedConfiguration = false;
		HasAreaMarkerVisualState = false;
	}

	internal void ClearAreaMarkerVisualState()
	{
		HasAreaMarkerVisualState = false;
	}

	internal void ClearObservedState()
	{
		HasObservedState = false;
		SetEnabledInvocationDepth = 0;
		HasPendingEnabledFanOutSuppression = false;
		HasPendingDataRevisionFanOutSuppression = false;
	}

	internal void ClearHoverText()
	{
		HasHoverText = false;
	}

	internal void ResetPresenceState()
	{
		PresenceLastTrustedNearbyTime = float.NegativeInfinity;
	}
}
