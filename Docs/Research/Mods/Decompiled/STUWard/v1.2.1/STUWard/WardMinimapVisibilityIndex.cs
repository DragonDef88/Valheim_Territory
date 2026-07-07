using System;
using System.Collections.Generic;
using UnityEngine;

namespace STUWard;

internal static class WardMinimapVisibilityIndex
{
	private readonly struct ViewerCacheKey : IEquatable<ViewerCacheKey>
	{
		internal long PlayerId { get; }

		internal int GuildId { get; }

		internal bool CanSeeAllWards { get; }

		internal ViewerCacheKey(long playerId, int guildId, bool canSeeAllWards)
		{
			PlayerId = playerId;
			GuildId = guildId;
			CanSeeAllWards = canSeeAllWards;
		}

		public bool Equals(ViewerCacheKey other)
		{
			if (PlayerId == other.PlayerId && GuildId == other.GuildId)
			{
				return CanSeeAllWards == other.CanSeeAllWards;
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is ViewerCacheKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((PlayerId.GetHashCode() * 397) ^ GuildId) * 397) ^ CanSeeAllWards.GetHashCode();
		}
	}

	private sealed class ViewerCacheState
	{
		internal int IndexedRevision;

		internal int ViewerRevisionToken;

		internal ZDOID[] VisibleWardIds = Array.Empty<ZDOID>();
	}

	private static readonly Dictionary<ZDOID, WardMinimapVisibilityIndexedEntry> IndexedWards = new Dictionary<ZDOID, WardMinimapVisibilityIndexedEntry>();

	private static readonly Dictionary<ViewerCacheKey, ViewerCacheState> ViewerCaches = new Dictionary<ViewerCacheKey, ViewerCacheState>();

	private static readonly List<ZDO> PrepareBuffer = new List<ZDO>();

	private static readonly Dictionary<ZDOID, uint> IndexedWardDataRevisions = new Dictionary<ZDOID, uint>();

	private static bool _prepared;

	private static int _indexRevision;

	private static int _nextViewerRevisionToken;

	internal static void ResetRuntimeState()
	{
		IndexedWards.Clear();
		IndexedWardDataRevisions.Clear();
		ViewerCaches.Clear();
		PrepareBuffer.Clear();
		_prepared = false;
		_indexRevision = 0;
		_nextViewerRevisionToken = 0;
	}

	internal static void OnZNetAwake()
	{
		ResetRuntimeState();
	}

	internal static bool TryPrepare(ZDOMan? zdoMan, string reason)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (_prepared)
		{
			return true;
		}
		if (zdoMan == null)
		{
			return false;
		}
		PrepareBuffer.Clear();
		int num = 0;
		while (!zdoMan.GetAllZDOsWithPrefabIterative("piece_stuward", PrepareBuffer, ref num))
		{
		}
		IndexedWards.Clear();
		int num2 = 0;
		for (int i = 0; i < PrepareBuffer.Count; i++)
		{
			if (TryBuildEntry(PrepareBuffer[i], out var entry))
			{
				IndexedWards[entry.ZdoId] = entry;
				num2++;
			}
		}
		_prepared = true;
		BumpIndexRevision();
		Plugin.LogWardDiagnosticVerbose("WardPins.Index", $"Prepared ward minimap visibility index. reason='{reason}', scannedWardCount={PrepareBuffer.Count}, indexedWardCount={num2}");
		return true;
	}

	internal static void ObserveManagedWard(ZDO? zdo)
	{
		UpdateEntry(zdo);
	}

	internal static void NotifyWardStateChanged(PrivateArea? area)
	{
		if (!((Object)(object)area == (Object)null))
		{
			UpdateEntry(area);
		}
	}

	internal static void NotifyWardStateChanged(ZDO? zdo)
	{
		UpdateEntry(zdo);
	}

	internal static bool ForgetWard(ZDOID zdoId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		IndexedWardDataRevisions.Remove(zdoId);
		if (!IndexedWards.Remove(zdoId))
		{
			return false;
		}
		BumpIndexRevision();
		return true;
	}

	internal static void InvalidateAll(string reason)
	{
		IndexedWards.Clear();
		IndexedWardDataRevisions.Clear();
		ViewerCaches.Clear();
		PrepareBuffer.Clear();
		_prepared = false;
		BumpIndexRevision();
		Plugin.LogWardDiagnosticVerbose("WardPins.Index", "Invalidated ward minimap visibility index. reason='" + reason + "'");
	}

	internal static void ObserveSyncedWardState(ZDOID zdoId, uint dataRevision, long ownerPlayerId, int wardGuildId, Vector3 position, float radius, bool isEnabled, long[] permittedPlayerIds)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		ApplyEntry(new WardMinimapVisibilityIndexedEntry(zdoId, ownerPlayerId, wardGuildId, position, radius, isEnabled, permittedPlayerIds ?? Array.Empty<long>()), dataRevision);
	}

	internal static int GetViewerRevisionToken(long playerId, int guildId, bool canSeeAllWards)
	{
		return GetOrBuildViewerCache(playerId, guildId, canSeeAllWards).ViewerRevisionToken;
	}

	internal static ZDOID[] GetVisibleCandidateWardIds(long playerId, int guildId, bool canSeeAllWards)
	{
		return GetOrBuildViewerCache(playerId, guildId, canSeeAllWards).VisibleWardIds;
	}

	internal static bool TryGetEntry(ZDOID zdoId, out WardMinimapVisibilityIndexedEntry entry)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return IndexedWards.TryGetValue(zdoId, out entry);
	}

	internal static bool TryGetDataRevision(ZDOID zdoId, out uint dataRevision)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return IndexedWardDataRevisions.TryGetValue(zdoId, out dataRevision);
	}

	internal static int GetIndexedWardCount()
	{
		return IndexedWards.Count;
	}

	internal static void PruneViewerCaches(HashSet<long> activePlayerIds)
	{
		if (ViewerCaches.Count == 0)
		{
			return;
		}
		if (activePlayerIds.Count == 0)
		{
			int count = ViewerCaches.Count;
			ViewerCaches.Clear();
			Plugin.LogWardDiagnosticVerbose("WardPins.Index", $"Pruned ward minimap viewer caches. removedCacheCount={count}, activePlayerCount=0");
			return;
		}
		List<ViewerCacheKey> list = null;
		foreach (KeyValuePair<ViewerCacheKey, ViewerCacheState> viewerCache in ViewerCaches)
		{
			if (!activePlayerIds.Contains(viewerCache.Key.PlayerId))
			{
				if (list == null)
				{
					list = new List<ViewerCacheKey>();
				}
				list.Add(viewerCache.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ViewerCaches.Remove(list[i]);
			}
			Plugin.LogWardDiagnosticVerbose("WardPins.Index", $"Pruned ward minimap viewer caches. removedCacheCount={list.Count}, activePlayerCount={activePlayerIds.Count}");
		}
	}

	private static ViewerCacheState GetOrBuildViewerCache(long playerId, int guildId, bool canSeeAllWards)
	{
		ViewerCacheKey key = new ViewerCacheKey(playerId, guildId, canSeeAllWards);
		if (!ViewerCaches.TryGetValue(key, out ViewerCacheState value))
		{
			value = new ViewerCacheState();
			ViewerCaches[key] = value;
		}
		if (value.IndexedRevision == _indexRevision)
		{
			return value;
		}
		value.VisibleWardIds = BuildVisibleWardIds(playerId, guildId, canSeeAllWards);
		value.IndexedRevision = _indexRevision;
		value.ViewerRevisionToken = NextViewerRevisionToken();
		return value;
	}

	private static ZDOID[] BuildVisibleWardIds(long playerId, int guildId, bool canSeeAllWards)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (IndexedWards.Count == 0)
		{
			return Array.Empty<ZDOID>();
		}
		List<ZDOID> list = new List<ZDOID>(IndexedWards.Count);
		WardGuildIdentity playerGuild = new WardGuildIdentity(guildId, string.Empty);
		foreach (WardMinimapVisibilityIndexedEntry value in IndexedWards.Values)
		{
			if (canSeeAllWards || ManagedWardAccessEvaluator.HasPlayerAccessToManagedWardIndexEntry(value, playerId, playerGuild))
			{
				list.Add(value.ZdoId);
			}
		}
		if (list.Count != 0)
		{
			return list.ToArray();
		}
		return Array.Empty<ZDOID>();
	}

	private static void UpdateEntry(ZDO? zdo)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!TryBuildEntry(zdo, out var entry))
		{
			if (zdo != null)
			{
				ForgetWard(zdo.m_uid);
			}
		}
		else
		{
			ApplyEntry(entry, zdo.DataRevision);
		}
	}

	private static void UpdateEntry(PrivateArea? area)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!TryBuildEntry(area, out var entry))
		{
			ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(area);
			if (zdo != null)
			{
				ForgetWard(zdo.m_uid);
			}
		}
		else
		{
			ZDO zdo2 = WardPrivateAreaSafeAccess.GetZdo(area);
			ApplyEntry(entry, (zdo2 != null) ? zdo2.DataRevision : 0u);
		}
	}

	private static void ApplyEntry(WardMinimapVisibilityIndexedEntry entry, uint dataRevision)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if ((!IndexedWardDataRevisions.TryGetValue(entry.ZdoId, out var value) || value <= dataRevision) && (!IndexedWards.TryGetValue(entry.ZdoId, out var value2) || !EntriesEqual(value2, entry) || value != dataRevision))
		{
			IndexedWards[entry.ZdoId] = entry;
			IndexedWardDataRevisions[entry.ZdoId] = dataRevision;
			BumpIndexRevision();
		}
	}

	private static bool TryBuildEntry(ZDO? zdo, out WardMinimapVisibilityIndexedEntry entry)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!WardOwnership.IsManagedWardZdo(zdo))
		{
			entry = default(WardMinimapVisibilityIndexedEntry);
			return false;
		}
		entry = new WardMinimapVisibilityIndexedEntry(zdo.m_uid, zdo.GetLong(ZDOVars.s_creator, 0L), GuildsCompat.ResolveWardGuildIdentityReadOnly(zdo).Id, zdo.GetPosition(), WardSettings.GetStoredRadius(zdo), zdo.GetBool(ZDOVars.s_enabled, false), WardPrivateAreaSafeAccess.GetPermittedPlayerIds(zdo));
		return true;
	}

	private static bool TryBuildEntry(PrivateArea? area, out WardMinimapVisibilityIndexedEntry entry)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(area);
		if ((Object)(object)area == (Object)null || zdo == null || !WardOwnership.IsManagedWardZdo(zdo))
		{
			entry = default(WardMinimapVisibilityIndexedEntry);
			return false;
		}
		entry = new WardMinimapVisibilityIndexedEntry(zdo.m_uid, zdo.GetLong(ZDOVars.s_creator, 0L), GuildsCompat.ResolveWardGuildIdentityReadOnly(zdo).Id, ((Component)area).transform.position, WardSettings.GetStoredRadiusOrMin(area), area.IsEnabled(), WardPrivateAreaSafeAccess.GetPermittedPlayerIds(zdo));
		return true;
	}

	private static bool EntriesEqual(WardMinimapVisibilityIndexedEntry left, WardMinimapVisibilityIndexedEntry right)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (left.OwnerPlayerId != right.OwnerPlayerId || left.WardGuildId != right.WardGuildId || left.Position != right.Position || !Mathf.Approximately(left.Radius, right.Radius) || left.IsEnabled != right.IsEnabled || left.PermittedPlayerIds.Length != right.PermittedPlayerIds.Length)
		{
			return false;
		}
		for (int i = 0; i < left.PermittedPlayerIds.Length; i++)
		{
			if (left.PermittedPlayerIds[i] != right.PermittedPlayerIds[i])
			{
				return false;
			}
		}
		return true;
	}

	private static void BumpIndexRevision()
	{
		if (_indexRevision == int.MaxValue)
		{
			_indexRevision = 1;
			ViewerCaches.Clear();
		}
		else
		{
			_indexRevision++;
		}
	}

	private static int NextViewerRevisionToken()
	{
		if (_nextViewerRevisionToken == int.MaxValue)
		{
			_nextViewerRevisionToken = 1;
			return _nextViewerRevisionToken;
		}
		_nextViewerRevisionToken++;
		return _nextViewerRevisionToken;
	}
}
