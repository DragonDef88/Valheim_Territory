using System;
using System.Collections.Generic;

namespace STUWard;

internal readonly struct WardMinimapViewerSnapshot
{
	private static readonly IReadOnlyDictionary<ZDOID, uint> EmptyVisibleWardDataRevisions = new Dictionary<ZDOID, uint>(0);

	internal static WardMinimapViewerSnapshot Empty { get; } = new WardMinimapViewerSnapshot(0, 0, 0, 0, 0, Array.Empty<WardMinimapSnapshotEntry>(), EmptyVisibleWardDataRevisions, null);


	internal int ViewerRevisionToken { get; }

	internal int IndexedWardCount { get; }

	internal int CandidateWardCount { get; }

	internal int VisibleWardCount { get; }

	internal int EnabledWardCount { get; }

	internal IReadOnlyList<WardMinimapSnapshotEntry> Entries { get; }

	internal IReadOnlyDictionary<ZDOID, uint> VisibleWardDataRevisions { get; }

	internal WardMinimapSnapshotEntry? FirstEntry { get; }

	internal WardMinimapViewerSnapshot(int viewerRevisionToken, int indexedWardCount, int candidateWardCount, int visibleWardCount, int enabledWardCount, IReadOnlyList<WardMinimapSnapshotEntry>? entries, IReadOnlyDictionary<ZDOID, uint>? visibleWardDataRevisions, WardMinimapSnapshotEntry? firstEntry)
	{
		ViewerRevisionToken = viewerRevisionToken;
		IndexedWardCount = indexedWardCount;
		CandidateWardCount = candidateWardCount;
		VisibleWardCount = visibleWardCount;
		EnabledWardCount = enabledWardCount;
		Entries = entries ?? Array.Empty<WardMinimapSnapshotEntry>();
		VisibleWardDataRevisions = visibleWardDataRevisions ?? EmptyVisibleWardDataRevisions;
		FirstEntry = firstEntry;
	}
}
