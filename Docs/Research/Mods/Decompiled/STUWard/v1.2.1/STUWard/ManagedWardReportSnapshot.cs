using System;
using System.Collections.Generic;

namespace STUWard;

internal readonly struct ManagedWardReportSnapshot
{
	internal string ModName { get; }

	internal DateTime GeneratedAtUtc { get; }

	internal string WorldName { get; }

	internal long WorldUid { get; }

	internal int TotalTrackedWards { get; }

	internal List<ManagedWardReportAccountEntry> Accounts { get; }

	internal List<KeyValuePair<long, int>> UnresolvedWardOwnerCounts { get; }

	internal List<KeyValuePair<long, int>> PlayerAccountMapGapCounts { get; }

	internal ManagedWardReportSnapshot(string modName, DateTime generatedAtUtc, string worldName, long worldUid, int totalTrackedWards, List<ManagedWardReportAccountEntry> accounts, List<KeyValuePair<long, int>> unresolvedWardOwnerCounts, List<KeyValuePair<long, int>> playerAccountMapGapCounts)
	{
		ModName = modName ?? string.Empty;
		GeneratedAtUtc = generatedAtUtc;
		WorldName = worldName ?? string.Empty;
		WorldUid = worldUid;
		TotalTrackedWards = totalTrackedWards;
		Accounts = accounts ?? new List<ManagedWardReportAccountEntry>();
		UnresolvedWardOwnerCounts = unresolvedWardOwnerCounts ?? new List<KeyValuePair<long, int>>();
		PlayerAccountMapGapCounts = playerAccountMapGapCounts ?? new List<KeyValuePair<long, int>>();
	}
}
