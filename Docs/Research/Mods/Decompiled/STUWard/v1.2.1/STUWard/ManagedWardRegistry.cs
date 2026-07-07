using System;
using System.Collections.Generic;
using UnityEngine;

namespace STUWard;

internal static class ManagedWardRegistry
{
	private sealed class ManagedWardRegistryState
	{
		internal readonly Dictionary<ZDOID, ManagedWardRegistryEntry> EntriesByZdoId = new Dictionary<ZDOID, ManagedWardRegistryEntry>();

		internal readonly Dictionary<string, int> CountByAccountId = new Dictionary<string, int>(StringComparer.Ordinal);

		internal readonly Dictionary<string, HashSet<ZDOID>> WardIdsByAccountId = new Dictionary<string, HashSet<ZDOID>>(StringComparer.Ordinal);

		internal readonly Dictionary<long, HashSet<ZDOID>> WardIdsByOwnerPlayerId = new Dictionary<long, HashSet<ZDOID>>();

		internal readonly Dictionary<int, HashSet<ZDOID>> WardIdsByGuildId = new Dictionary<int, HashSet<ZDOID>>();

		internal readonly Dictionary<string, HashSet<ZDOID>> WardIdsByCharacterKey = new Dictionary<string, HashSet<ZDOID>>(StringComparer.Ordinal);
	}

	private static readonly ManagedWardRegistryState ManagedWardRegistryData = new ManagedWardRegistryState();

	private static Dictionary<ZDOID, ManagedWardRegistryEntry> ManagedWardRegistryEntriesByZdoId => ManagedWardRegistryData.EntriesByZdoId;

	private static Dictionary<string, int> ManagedWardCountsByAccountId => ManagedWardRegistryData.CountByAccountId;

	private static Dictionary<string, HashSet<ZDOID>> ManagedWardIdsByAccountId => ManagedWardRegistryData.WardIdsByAccountId;

	private static Dictionary<long, HashSet<ZDOID>> ManagedWardIdsByOwnerPlayerId => ManagedWardRegistryData.WardIdsByOwnerPlayerId;

	private static Dictionary<int, HashSet<ZDOID>> ManagedWardIdsByGuildId => ManagedWardRegistryData.WardIdsByGuildId;

	private static Dictionary<string, HashSet<ZDOID>> ManagedWardIdsByCharacterKey => ManagedWardRegistryData.WardIdsByCharacterKey;

	internal static void Reset()
	{
		ManagedWardRegistryEntriesByZdoId.Clear();
		ManagedWardCountsByAccountId.Clear();
		ManagedWardIdsByAccountId.Clear();
		ManagedWardIdsByOwnerPlayerId.Clear();
		ManagedWardIdsByGuildId.Clear();
		ManagedWardIdsByCharacterKey.Clear();
	}

	internal static void UpsertEntry(ZDO? zdo)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (zdo == null || !zdo.IsValid() || (Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return;
		}
		if (!WardOwnership.IsManagedWardZdo(zdo))
		{
			RemoveEntry(zdo.m_uid);
			return;
		}
		ManagedWardRegistryEntry managedWardRegistryEntry = BuildManagedWardRegistryEntry(zdo);
		if (ManagedWardRegistryEntriesByZdoId.TryGetValue(managedWardRegistryEntry.ZdoId, out var value))
		{
			RemoveManagedWardRegistryIndexes(value);
		}
		ManagedWardRegistryEntriesByZdoId[managedWardRegistryEntry.ZdoId] = managedWardRegistryEntry;
		AddManagedWardRegistryIndexes(managedWardRegistryEntry);
	}

	internal static void RemoveEntry(ZDOID zdoId)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!((ZDOID)(ref zdoId)).IsNone() && ManagedWardRegistryEntriesByZdoId.Remove(zdoId, out var value))
		{
			RemoveManagedWardRegistryIndexes(value);
		}
	}

	internal static int CountForAccount(string accountId, ZDOID ignoredZdoId = default(ZDOID))
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		string text = WardOwnership.NormalizeAccountIdValue(accountId);
		if (string.IsNullOrWhiteSpace(text))
		{
			return 0;
		}
		int value;
		int num = (ManagedWardCountsByAccountId.TryGetValue(text, out value) ? value : 0);
		if (!((ZDOID)(ref ignoredZdoId)).IsNone() && ManagedWardRegistryEntriesByZdoId.TryGetValue(ignoredZdoId, out var value2) && string.Equals(value2.AccountId, text, StringComparison.Ordinal))
		{
			num--;
		}
		return Math.Max(0, num);
	}

	internal static int GetIndexedCount()
	{
		return ManagedWardRegistryEntriesByZdoId.Count;
	}

	internal static int CollectCandidateIds(HashSet<ZDOID> candidateWardIds, HashSet<long>? targetPlayerIds, HashSet<string>? targetCharacterKeys, HashSet<int>? affectedGuildIds, bool fullRefresh)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		candidateWardIds.Clear();
		if (fullRefresh)
		{
			foreach (KeyValuePair<ZDOID, ManagedWardRegistryEntry> item in ManagedWardRegistryEntriesByZdoId)
			{
				candidateWardIds.Add(item.Key);
			}
			return candidateWardIds.Count;
		}
		if (targetPlayerIds != null)
		{
			foreach (long targetPlayerId in targetPlayerIds)
			{
				UnionManagedWardRegistryIds(candidateWardIds, ManagedWardIdsByOwnerPlayerId, targetPlayerId);
			}
		}
		if (targetCharacterKeys != null)
		{
			foreach (string targetCharacterKey in targetCharacterKeys)
			{
				if (!string.IsNullOrWhiteSpace(targetCharacterKey))
				{
					UnionManagedWardRegistryIds(candidateWardIds, ManagedWardIdsByCharacterKey, targetCharacterKey);
				}
			}
		}
		if (affectedGuildIds != null)
		{
			foreach (int affectedGuildId in affectedGuildIds)
			{
				if (affectedGuildId != 0)
				{
					UnionManagedWardRegistryIds(candidateWardIds, ManagedWardIdsByGuildId, affectedGuildId);
				}
			}
		}
		return candidateWardIds.Count;
	}

	private static ManagedWardRegistryEntry BuildManagedWardRegistryEntry(ZDO zdo)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		long @long = zdo.GetLong(ZDOVars.s_creator, 0L);
		string accountId = WardOwnership.NormalizeAccountIdValue(WardOwnership.ResolveWardSteamAccountId(zdo, @long, WardOwnership.GetWardSteamAccountId(zdo)));
		string text = (WardPrivateAreaSafeAccess.GetCreatorName(zdo) ?? string.Empty).Trim();
		string characterKey = GuildsCompat.BuildCharacterIdentityKey(accountId, text);
		int wardGuildId = GuildsCompat.GetWardGuildId(zdo);
		return new ManagedWardRegistryEntry(zdo.m_uid, @long, accountId, text, characterKey, wardGuildId);
	}

	private static void AddManagedWardRegistryIndexes(ManagedWardRegistryEntry entry)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrWhiteSpace(entry.AccountId))
		{
			ManagedWardCountsByAccountId[entry.AccountId] = ((!ManagedWardCountsByAccountId.TryGetValue(entry.AccountId, out var value)) ? 1 : (value + 1));
			AddManagedWardRegistryId(ManagedWardIdsByAccountId, entry.AccountId, entry.ZdoId);
		}
		if (entry.OwnerPlayerId != 0L)
		{
			AddManagedWardRegistryId(ManagedWardIdsByOwnerPlayerId, entry.OwnerPlayerId, entry.ZdoId);
		}
		if (entry.GuildId != 0)
		{
			AddManagedWardRegistryId(ManagedWardIdsByGuildId, entry.GuildId, entry.ZdoId);
		}
		if (!string.IsNullOrWhiteSpace(entry.CharacterKey))
		{
			AddManagedWardRegistryId(ManagedWardIdsByCharacterKey, entry.CharacterKey, entry.ZdoId);
		}
	}

	private static void RemoveManagedWardRegistryIndexes(ManagedWardRegistryEntry entry)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrWhiteSpace(entry.AccountId))
		{
			if (ManagedWardCountsByAccountId.TryGetValue(entry.AccountId, out var value))
			{
				if (value <= 1)
				{
					ManagedWardCountsByAccountId.Remove(entry.AccountId);
				}
				else
				{
					ManagedWardCountsByAccountId[entry.AccountId] = value - 1;
				}
			}
			RemoveManagedWardRegistryId(ManagedWardIdsByAccountId, entry.AccountId, entry.ZdoId);
		}
		if (entry.OwnerPlayerId != 0L)
		{
			RemoveManagedWardRegistryId(ManagedWardIdsByOwnerPlayerId, entry.OwnerPlayerId, entry.ZdoId);
		}
		if (entry.GuildId != 0)
		{
			RemoveManagedWardRegistryId(ManagedWardIdsByGuildId, entry.GuildId, entry.ZdoId);
		}
		if (!string.IsNullOrWhiteSpace(entry.CharacterKey))
		{
			RemoveManagedWardRegistryId(ManagedWardIdsByCharacterKey, entry.CharacterKey, entry.ZdoId);
		}
	}

	private static void AddManagedWardRegistryId<TKey>(Dictionary<TKey, HashSet<ZDOID>> indexedWardIds, TKey key, ZDOID zdoId) where TKey : notnull
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!indexedWardIds.TryGetValue(key, out HashSet<ZDOID> value))
		{
			value = (indexedWardIds[key] = new HashSet<ZDOID>());
		}
		value.Add(zdoId);
	}

	private static void RemoveManagedWardRegistryId<TKey>(Dictionary<TKey, HashSet<ZDOID>> indexedWardIds, TKey key, ZDOID zdoId) where TKey : notnull
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (indexedWardIds.TryGetValue(key, out HashSet<ZDOID> value))
		{
			value.Remove(zdoId);
			if (value.Count == 0)
			{
				indexedWardIds.Remove(key);
			}
		}
	}

	private static void UnionManagedWardRegistryIds<TKey>(HashSet<ZDOID> candidateWardIds, Dictionary<TKey, HashSet<ZDOID>> indexedWardIds, TKey key) where TKey : notnull
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (!indexedWardIds.TryGetValue(key, out HashSet<ZDOID> value))
		{
			return;
		}
		foreach (ZDOID item in value)
		{
			candidateWardIds.Add(item);
		}
	}
}
