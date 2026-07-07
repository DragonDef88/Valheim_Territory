using System.Collections.Generic;
using UnityEngine;

namespace STUWard;

internal static class WardPermittedSnapshots
{
	private readonly struct BackfillRequest
	{
		internal PrivateArea? Area { get; }

		internal int InstanceId { get; }

		internal BackfillRequest(PrivateArea area)
		{
			Area = area;
			InstanceId = ((Object)area).GetInstanceID();
		}
	}

	private readonly struct SnapshotEntry
	{
		internal long PlayerId { get; }

		internal string GuildName { get; }

		internal string PlatformId { get; }

		internal SnapshotEntry(long playerId, string guildName, string platformId)
		{
			PlayerId = playerId;
			GuildName = guildName;
			PlatformId = platformId;
		}
	}

	private sealed class CachedSnapshot
	{
		internal uint Revision { get; }

		internal Dictionary<long, SnapshotEntry> Entries { get; }

		internal CachedSnapshot(uint revision, Dictionary<long, SnapshotEntry> entries)
		{
			Revision = revision;
			Entries = entries;
		}
	}

	private const int BackfillBatchSize = 4;

	private const int SnapshotFormatVersion = 1;

	private static readonly int SnapshotVersionKey = StringExtensionMethods.GetStableHashCode("stuw_perm_snapshot_version");

	private static readonly int SnapshotDataKey = StringExtensionMethods.GetStableHashCode("stuw_perm_snapshot");

	private static readonly int SnapshotRevisionKey = StringExtensionMethods.GetStableHashCode("stuw_perm_snapshot_revision");

	private static readonly Dictionary<ZDOID, CachedSnapshot> SnapshotCache = new Dictionary<ZDOID, CachedSnapshot>();

	private static readonly List<BackfillRequest> PendingBackfillRequests = new List<BackfillRequest>();

	private static readonly HashSet<int> PendingBackfillAreaIds = new HashSet<int>();

	internal static void Capture(PrivateArea? area, long playerId)
	{
		Refresh(area);
	}

	internal static void Remove(PrivateArea? area, long playerId)
	{
		Refresh(area);
	}

	internal static void Backfill(PrivateArea? area)
	{
		Backfill(ManagedWardRef.FromArea(area));
	}

	internal static void Backfill(ManagedWardRef ward)
	{
		if (TryGetOwnedSnapshotZdo(ward, out ZDO zdo, out ZNetView _) && !HasCurrentSnapshot(zdo))
		{
			EnqueueBackfill(ward.Area);
		}
	}

	internal static void Update()
	{
		if (PendingBackfillRequests.Count == 0 || (Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return;
		}
		int num = 0;
		while (num < 4 && PendingBackfillRequests.Count > 0)
		{
			int index = PendingBackfillRequests.Count - 1;
			BackfillRequest backfillRequest = PendingBackfillRequests[index];
			PendingBackfillRequests.RemoveAt(index);
			PendingBackfillAreaIds.Remove(backfillRequest.InstanceId);
			if (!((Object)(object)backfillRequest.Area == (Object)null) && TryGetOwnedSnapshotZdo(backfillRequest.Area, out ZDO zdo, out ZNetView _) && !HasCurrentSnapshot(zdo))
			{
				Refresh(backfillRequest.Area);
				num++;
			}
		}
	}

	internal static bool HasPendingRuntimeWork()
	{
		return PendingBackfillRequests.Count > 0;
	}

	internal static bool TryGet(PrivateArea? area, long playerId, out string guildName, out string platformId)
	{
		guildName = string.Empty;
		platformId = string.Empty;
		if ((Object)(object)area == (Object)null || playerId == 0L)
		{
			return false;
		}
		ZNetView nView = WardPrivateAreaSafeAccess.GetNView(area);
		if ((Object)(object)nView == (Object)null || !nView.IsValid())
		{
			return false;
		}
		if (!TryGetSnapshot(area, nView, out Dictionary<long, SnapshotEntry> entries))
		{
			return false;
		}
		if (!entries.TryGetValue(playerId, out var value))
		{
			return false;
		}
		guildName = value.GuildName;
		platformId = value.PlatformId;
		if (string.IsNullOrWhiteSpace(guildName))
		{
			return !string.IsNullOrWhiteSpace(platformId);
		}
		return true;
	}

	internal static int GetRevision(PrivateArea? area)
	{
		if ((Object)(object)area == (Object)null)
		{
			return 0;
		}
		ZDO? zdo = WardPrivateAreaSafeAccess.GetZdo(area);
		if (zdo == null)
		{
			return 0;
		}
		return zdo.GetInt(SnapshotRevisionKey, 0);
	}

	internal static void RefreshFromZdo(ZDO? zdo)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (zdo != null && zdo.IsValid() && !((Object)(object)ZNet.instance == (Object)null) && ZNet.instance.IsServer())
		{
			List<SnapshotEntry> list = BuildEntries(zdo);
			ZPackage val = new ZPackage();
			val.Write(1);
			val.Write(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				SnapshotEntry snapshotEntry = list[i];
				val.Write(snapshotEntry.PlayerId);
				val.Write(snapshotEntry.GuildName);
				val.Write(snapshotEntry.PlatformId);
			}
			byte[] array = val.GetArray();
			int @int = zdo.GetInt(SnapshotVersionKey, 0);
			byte[] byteArray = zdo.GetByteArray(SnapshotDataKey, (byte[])null);
			int int2 = zdo.GetInt(SnapshotRevisionKey, 0);
			if (@int != 1 || int2 <= 0 || !ByteArraysEqual(byteArray, array))
			{
				zdo.Set(SnapshotVersionKey, 1, false);
				zdo.Set(SnapshotDataKey, array);
				zdo.Set(SnapshotRevisionKey, (int2 == int.MaxValue) ? 1 : (int2 + 1), false);
			}
			SnapshotCache[zdo.m_uid] = new CachedSnapshot(zdo.DataRevision, ToLookup(list));
		}
	}

	private static void Refresh(PrivateArea? area)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetOwnedSnapshotZdo(area, out ZDO zdo, out ZNetView _))
		{
			List<SnapshotEntry> list = BuildEntries(area);
			ZPackage val = new ZPackage();
			val.Write(1);
			val.Write(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				SnapshotEntry snapshotEntry = list[i];
				val.Write(snapshotEntry.PlayerId);
				val.Write(snapshotEntry.GuildName);
				val.Write(snapshotEntry.PlatformId);
			}
			byte[] array = val.GetArray();
			int @int = zdo.GetInt(SnapshotVersionKey, 0);
			byte[] byteArray = zdo.GetByteArray(SnapshotDataKey, (byte[])null);
			int int2 = zdo.GetInt(SnapshotRevisionKey, 0);
			if (@int != 1 || int2 <= 0 || !ByteArraysEqual(byteArray, array))
			{
				zdo.Set(SnapshotVersionKey, 1, false);
				zdo.Set(SnapshotDataKey, array);
				zdo.Set(SnapshotRevisionKey, (int2 == int.MaxValue) ? 1 : (int2 + 1), false);
			}
			SnapshotCache[zdo.m_uid] = new CachedSnapshot(zdo.DataRevision, ToLookup(list));
		}
	}

	internal static void ClearCache()
	{
		SnapshotCache.Clear();
		PendingBackfillRequests.Clear();
		PendingBackfillAreaIds.Clear();
	}

	private static bool HasCurrentSnapshot(ZDO zdo)
	{
		if (zdo.GetInt(SnapshotVersionKey, 0) == 1 && zdo.GetInt(SnapshotRevisionKey, 0) > 0)
		{
			byte[] byteArray = zdo.GetByteArray(SnapshotDataKey, (byte[])null);
			return ((byteArray != null && byteArray.Length != 0) ? 1 : 0) > (false ? 1 : 0);
		}
		return false;
	}

	private static void EnqueueBackfill(PrivateArea area)
	{
		int instanceID = ((Object)area).GetInstanceID();
		if (PendingBackfillAreaIds.Add(instanceID))
		{
			PendingBackfillRequests.Add(new BackfillRequest(area));
		}
	}

	private static bool TryGetOwnedSnapshotZdo(PrivateArea? area, out ZDO zdo, out ZNetView nview)
	{
		return TryGetOwnedSnapshotZdo(ManagedWardRef.FromArea(area), out zdo, out nview);
	}

	private static bool TryGetOwnedSnapshotZdo(ManagedWardRef ward, out ZDO zdo, out ZNetView nview)
	{
		zdo = null;
		nview = null;
		PrivateArea area = ward.Area;
		if ((Object)(object)area == (Object)null || Player.IsPlacementGhost(((Component)area).gameObject) || (Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return false;
		}
		nview = ward.NView;
		if (!ward.HasValidNetworkIdentity || !ward.IsOwner)
		{
			return false;
		}
		zdo = ward.Zdo;
		return true;
	}

	private static bool TryGetSnapshot(PrivateArea area, ZNetView nview, out Dictionary<long, SnapshotEntry> entries)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		entries = null;
		ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(nview);
		if (zdo == null)
		{
			return false;
		}
		if (SnapshotCache.TryGetValue(zdo.m_uid, out CachedSnapshot value) && value.Revision == zdo.DataRevision)
		{
			entries = value.Entries;
			return true;
		}
		entries = Deserialize(zdo);
		SnapshotCache[zdo.m_uid] = new CachedSnapshot(zdo.DataRevision, entries);
		return entries.Count > 0;
	}

	private static List<SnapshotEntry> BuildEntries(PrivateArea area)
	{
		return BuildEntries(WardPrivateAreaSafeAccess.GetZdo(area));
	}

	private static List<SnapshotEntry> BuildEntries(ZDO? zdo)
	{
		long[] permittedPlayerIds = WardPrivateAreaSafeAccess.GetPermittedPlayerIds(zdo);
		List<SnapshotEntry> list = new List<SnapshotEntry>(permittedPlayerIds.Length);
		foreach (long num in permittedPlayerIds)
		{
			if (num != 0L)
			{
				list.Add(new SnapshotEntry(num, GuildsCompat.GetPlayerGuildName(num) ?? string.Empty, WardOwnership.GetPlayerSteamIdDisplay(num) ?? string.Empty));
			}
		}
		return list;
	}

	private static Dictionary<long, SnapshotEntry> Deserialize(ZDO zdo)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		Dictionary<long, SnapshotEntry> dictionary = new Dictionary<long, SnapshotEntry>();
		if (zdo.GetInt(SnapshotVersionKey, 0) != 1)
		{
			return dictionary;
		}
		byte[] byteArray = zdo.GetByteArray(SnapshotDataKey, (byte[])null);
		if (byteArray == null || byteArray.Length == 0)
		{
			return dictionary;
		}
		ZPackage val = new ZPackage(byteArray);
		if (val.ReadInt() != 1)
		{
			return dictionary;
		}
		int num = val.ReadInt();
		for (int i = 0; i < num; i++)
		{
			long num2 = val.ReadLong();
			if (num2 == 0L)
			{
				val.ReadString();
				val.ReadString();
			}
			else
			{
				dictionary[num2] = new SnapshotEntry(num2, val.ReadString(), val.ReadString());
			}
		}
		return dictionary;
	}

	private static Dictionary<long, SnapshotEntry> ToLookup(List<SnapshotEntry> entries)
	{
		Dictionary<long, SnapshotEntry> dictionary = new Dictionary<long, SnapshotEntry>(entries.Count);
		for (int i = 0; i < entries.Count; i++)
		{
			SnapshotEntry value = entries[i];
			dictionary[value.PlayerId] = value;
		}
		return dictionary;
	}

	private static bool ByteArraysEqual(byte[]? left, byte[] right)
	{
		if (left == null || left.Length != right.Length)
		{
			return false;
		}
		for (int i = 0; i < right.Length; i++)
		{
			if (left[i] != right[i])
			{
				return false;
			}
		}
		return true;
	}
}
