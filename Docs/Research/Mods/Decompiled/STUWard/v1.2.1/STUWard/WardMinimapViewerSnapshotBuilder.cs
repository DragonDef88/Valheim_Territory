using System;
using System.Collections.Generic;

namespace STUWard;

internal static class WardMinimapViewerSnapshotBuilder
{
	internal static WardMinimapViewerSnapshot Build(long playerId, int playerGuildId, bool canSeeAllWards, int viewerRevisionToken, bool includeEntries, bool includeVisibleWardDataRevisions)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		ZDOID[] visibleCandidateWardIds = WardMinimapVisibilityIndex.GetVisibleCandidateWardIds(playerId, playerGuildId, canSeeAllWards);
		int indexedWardCount = WardMinimapVisibilityIndex.GetIndexedWardCount();
		int candidateWardCount = visibleCandidateWardIds.Length;
		int num = 0;
		int num2 = 0;
		WardMinimapSnapshotEntry? firstEntry = null;
		List<WardMinimapSnapshotEntry> list = ((includeEntries && visibleCandidateWardIds.Length != 0) ? new List<WardMinimapSnapshotEntry>(visibleCandidateWardIds.Length) : null);
		Dictionary<ZDOID, uint> dictionary = ((includeVisibleWardDataRevisions && visibleCandidateWardIds.Length != 0) ? new Dictionary<ZDOID, uint>(visibleCandidateWardIds.Length) : null);
		for (int i = 0; i < visibleCandidateWardIds.Length; i++)
		{
			if (WardMinimapVisibilityIndex.TryGetEntry(visibleCandidateWardIds[i], out var entry))
			{
				WardMinimapSnapshotEntry wardMinimapSnapshotEntry = new WardMinimapSnapshotEntry(entry.ZdoId, entry.Position, entry.Radius, entry.IsEnabled);
				num++;
				if (wardMinimapSnapshotEntry.IsEnabled)
				{
					num2++;
				}
				WardMinimapSnapshotEntry valueOrDefault = firstEntry.GetValueOrDefault();
				if (!firstEntry.HasValue)
				{
					valueOrDefault = wardMinimapSnapshotEntry;
					firstEntry = valueOrDefault;
				}
				list?.Add(wardMinimapSnapshotEntry);
				if (dictionary != null)
				{
					dictionary[wardMinimapSnapshotEntry.ZdoId] = (WardMinimapVisibilityIndex.TryGetDataRevision(wardMinimapSnapshotEntry.ZdoId, out var dataRevision) ? dataRevision : 0u);
				}
			}
		}
		int visibleWardCount = num;
		int enabledWardCount = num2;
		IReadOnlyList<WardMinimapSnapshotEntry> entries;
		if (list != null && list.Count != 0)
		{
			IReadOnlyList<WardMinimapSnapshotEntry> readOnlyList = list;
			entries = readOnlyList;
		}
		else
		{
			IReadOnlyList<WardMinimapSnapshotEntry> readOnlyList = Array.Empty<WardMinimapSnapshotEntry>();
			entries = readOnlyList;
		}
		return new WardMinimapViewerSnapshot(viewerRevisionToken, indexedWardCount, candidateWardCount, visibleWardCount, enabledWardCount, entries, dictionary, firstEntry);
	}
}
