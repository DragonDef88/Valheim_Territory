using System;
using System.Collections.Generic;
using UnityEngine;

namespace STUWard;

internal static class WardPrivateAreaSafeAccess
{
	private sealed class CachedPermittedPlayerIds
	{
		internal uint Revision { get; }

		internal long[] PlayerIds { get; }

		internal CachedPermittedPlayerIds(uint revision, long[] playerIds)
		{
			Revision = revision;
			PlayerIds = playerIds ?? Array.Empty<long>();
		}
	}

	private static readonly Dictionary<ZDOID, CachedPermittedPlayerIds> PermittedPlayerIdsCache = new Dictionary<ZDOID, CachedPermittedPlayerIds>();

	private static readonly List<int> PermittedPlayerIdKeys = new List<int>();

	private static readonly List<int> PermittedPlayerNameKeys = new List<int>();

	internal static ZNetView? GetNView(PrivateArea? area)
	{
		if ((Object)(object)area == (Object)null)
		{
			return null;
		}
		if (!((Object)(object)area.m_nview != (Object)null))
		{
			return ((Component)area).GetComponent<ZNetView>();
		}
		return area.m_nview;
	}

	internal static ZDO? GetZdo(PrivateArea? area)
	{
		return GetZdo(GetNView(area));
	}

	internal static ZDO? GetZdo(ZNetView? nview)
	{
		if ((Object)(object)nview == (Object)null || !nview.IsValid())
		{
			return null;
		}
		return nview.GetZDO();
	}

	internal static string GetCreatorName(PrivateArea? area)
	{
		return GetCreatorName(GetZdo(area));
	}

	internal static string GetCreatorName(ZDO? zdo)
	{
		if (zdo != null)
		{
			return (zdo.GetString(ZDOVars.s_creatorName, string.Empty) ?? string.Empty).Trim();
		}
		return string.Empty;
	}

	internal static List<KeyValuePair<long, string>> GetPermittedPlayers(PrivateArea? area)
	{
		return GetPermittedPlayers(GetZdo(area));
	}

	internal static List<KeyValuePair<long, string>> GetPermittedPlayers(ZDO? zdo)
	{
		if (zdo == null)
		{
			return new List<KeyValuePair<long, string>>();
		}
		int @int = zdo.GetInt(ZDOVars.s_permitted, 0);
		if (@int <= 0)
		{
			return new List<KeyValuePair<long, string>>();
		}
		List<KeyValuePair<long, string>> list = new List<KeyValuePair<long, string>>(@int);
		for (int i = 0; i < @int; i++)
		{
			long @long = zdo.GetLong(GetPermittedPlayerIdKey(i), 0L);
			if (@long != 0L)
			{
				list.Add(new KeyValuePair<long, string>(@long, zdo.GetString(GetPermittedPlayerNameKey(i), string.Empty) ?? string.Empty));
			}
		}
		return list;
	}

	internal static long[] GetPermittedPlayerIds(PrivateArea? area)
	{
		return GetPermittedPlayerIds(GetZdo(area));
	}

	internal static long[] GetPermittedPlayerIds(ZDO? zdo)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (zdo == null || !zdo.IsValid())
		{
			return Array.Empty<long>();
		}
		if (PermittedPlayerIdsCache.TryGetValue(zdo.m_uid, out CachedPermittedPlayerIds value) && value.Revision == zdo.DataRevision)
		{
			return value.PlayerIds;
		}
		long[] array = ReadPermittedPlayerIds(zdo);
		PermittedPlayerIdsCache[zdo.m_uid] = new CachedPermittedPlayerIds(zdo.DataRevision, array);
		return array;
	}

	internal static bool IsPlayerPermitted(PrivateArea? area, long playerId)
	{
		return IsPlayerPermitted(GetZdo(area), playerId);
	}

	internal static bool IsPlayerPermitted(ZDO? zdo, long playerId)
	{
		if (playerId == 0L)
		{
			return false;
		}
		if (zdo == null)
		{
			return false;
		}
		long[] permittedPlayerIds = GetPermittedPlayerIds(zdo);
		for (int i = 0; i < permittedPlayerIds.Length; i++)
		{
			if (permittedPlayerIds[i] == playerId)
			{
				return true;
			}
		}
		return false;
	}

	internal static void SetPermittedPlayers(ZDO? zdo, IReadOnlyList<KeyValuePair<long, string>> permittedPlayers)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (zdo != null)
		{
			zdo.Set(ZDOVars.s_permitted, permittedPlayers.Count, false);
			long[] array = ((permittedPlayers.Count == 0) ? Array.Empty<long>() : new long[permittedPlayers.Count]);
			for (int i = 0; i < permittedPlayers.Count; i++)
			{
				KeyValuePair<long, string> keyValuePair = permittedPlayers[i];
				array[i] = keyValuePair.Key;
				zdo.Set(GetPermittedPlayerIdKey(i), keyValuePair.Key);
				zdo.Set(GetPermittedPlayerNameKey(i), keyValuePair.Value ?? string.Empty);
			}
			PermittedPlayerIdsCache[zdo.m_uid] = new CachedPermittedPlayerIds(zdo.DataRevision, array);
		}
	}

	internal static bool HasLocalAccess(PrivateArea? area)
	{
		if ((Object)(object)area == (Object)null)
		{
			return false;
		}
		Piece val = (((Object)(object)area.m_piece != (Object)null) ? area.m_piece : ((Component)area).GetComponent<Piece>());
		if ((Object)(object)val != (Object)null && val.IsCreator())
		{
			return true;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer != (Object)null)
		{
			return IsPlayerPermitted(area, localPlayer.GetPlayerID());
		}
		return false;
	}

	internal static void ForgetPermittedPlayerIds(ZDOID zdoId)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!((ZDOID)(ref zdoId)).IsNone())
		{
			PermittedPlayerIdsCache.Remove(zdoId);
		}
	}

	internal static void ResetRuntimeState()
	{
		PermittedPlayerIdsCache.Clear();
	}

	internal static void OnZNetAwake()
	{
		ResetRuntimeState();
	}

	private static long[] ReadPermittedPlayerIds(ZDO zdo)
	{
		int @int = zdo.GetInt(ZDOVars.s_permitted, 0);
		if (@int <= 0)
		{
			return Array.Empty<long>();
		}
		long[] array = new long[@int];
		int num = 0;
		for (int i = 0; i < @int; i++)
		{
			long @long = zdo.GetLong(GetPermittedPlayerIdKey(i), 0L);
			if (@long != 0L)
			{
				array[num] = @long;
				num++;
			}
		}
		if (num == 0)
		{
			return Array.Empty<long>();
		}
		if (num == array.Length)
		{
			return array;
		}
		long[] array2 = new long[num];
		Array.Copy(array, array2, num);
		return array2;
	}

	private static int GetPermittedPlayerIdKey(int index)
	{
		EnsurePermittedPlayerKeys(index);
		return PermittedPlayerIdKeys[index];
	}

	private static int GetPermittedPlayerNameKey(int index)
	{
		EnsurePermittedPlayerKeys(index);
		return PermittedPlayerNameKeys[index];
	}

	private static void EnsurePermittedPlayerKeys(int index)
	{
		while (PermittedPlayerIdKeys.Count <= index)
		{
			int count = PermittedPlayerIdKeys.Count;
			PermittedPlayerIdKeys.Add(StringExtensionMethods.GetStableHashCode($"pu_id{count}"));
			PermittedPlayerNameKeys.Add(StringExtensionMethods.GetStableHashCode($"pu_name{count}"));
		}
	}
}
