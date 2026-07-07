namespace STUWard;

internal readonly struct ManagedWardReportBuildResult
{
	internal string Contents { get; }

	internal int TrackedAccounts { get; }

	internal int TotalWards { get; }

	internal int UnresolvedOwners { get; }

	internal ManagedWardReportBuildResult(string contents, int trackedAccounts, int totalWards, int unresolvedOwners)
	{
		Contents = contents ?? string.Empty;
		TrackedAccounts = trackedAccounts;
		TotalWards = totalWards;
		UnresolvedOwners = unresolvedOwners;
	}
}
