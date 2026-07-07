namespace STUWard;

internal readonly struct ManagedWardObservedState
{
	internal bool Enabled { get; }

	internal uint DataRevision { get; }

	internal ManagedWardObservedState(bool enabled, uint dataRevision)
	{
		Enabled = enabled;
		DataRevision = dataRevision;
	}
}
