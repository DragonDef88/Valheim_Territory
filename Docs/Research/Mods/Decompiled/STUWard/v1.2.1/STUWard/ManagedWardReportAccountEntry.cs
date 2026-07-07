namespace STUWard;

internal readonly struct ManagedWardReportAccountEntry
{
	internal string AccountId { get; }

	internal int WardCount { get; }

	internal int EffectiveLimit { get; }

	internal bool HasOverride { get; }

	internal ManagedWardReportAccountEntry(string accountId, int wardCount, int effectiveLimit, bool hasOverride)
	{
		AccountId = accountId ?? string.Empty;
		WardCount = wardCount;
		EffectiveLimit = effectiveLimit;
		HasOverride = hasOverride;
	}
}
