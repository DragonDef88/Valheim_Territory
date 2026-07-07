using System;
using System.Collections.Generic;
using System.Linq;

public static class ZDOHelper
{
	public static readonly HashSet<int> s_stripOldData = new HashSet<int>
	{
		StringExtensionMethods.GetStableHashCode("generated"),
		StringExtensionMethods.GetStableHashCode("patrolSpawnPoint"),
		StringExtensionMethods.GetStableHashCode("autoDespawn"),
		StringExtensionMethods.GetStableHashCode("targetHear"),
		StringExtensionMethods.GetStableHashCode("targetSee"),
		StringExtensionMethods.GetStableHashCode("burnt0"),
		StringExtensionMethods.GetStableHashCode("burnt1"),
		StringExtensionMethods.GetStableHashCode("burnt2"),
		StringExtensionMethods.GetStableHashCode("burnt3"),
		StringExtensionMethods.GetStableHashCode("burnt4"),
		StringExtensionMethods.GetStableHashCode("burnt5"),
		StringExtensionMethods.GetStableHashCode("burnt6"),
		StringExtensionMethods.GetStableHashCode("burnt7"),
		StringExtensionMethods.GetStableHashCode("burnt8"),
		StringExtensionMethods.GetStableHashCode("burnt9"),
		StringExtensionMethods.GetStableHashCode("burnt10"),
		StringExtensionMethods.GetStableHashCode("LookDir"),
		StringExtensionMethods.GetStableHashCode("RideSpeed")
	};

	public static readonly List<int> s_stripOldLongData;

	public static readonly List<int> s_stripOldDataByteArray;

	public static string ToStringFast(this ZDOExtraData.ConnectionType value)
	{
		return (value & ~ZDOExtraData.ConnectionType.Target) switch
		{
			ZDOExtraData.ConnectionType.Portal => "Portal", 
			ZDOExtraData.ConnectionType.SyncTransform => "SyncTransform", 
			ZDOExtraData.ConnectionType.Spawned => "Spawned", 
			_ => value.ToString(), 
		};
	}

	public static TValue GetValueOrDefaultPiktiv<TKey, TValue>(this IDictionary<TKey, TValue> container, TKey zid, TValue defaultValue)
	{
		if (!container.ContainsKey(zid))
		{
			return defaultValue;
		}
		return container[zid];
	}

	public static bool InitAndSet<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid, int hash, TType value)
	{
		container.Init(zid);
		return container[zid].SetValue(hash, value);
	}

	public static bool Update<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid, int hash, TType value)
	{
		return container[zid].SetValue(hash, value);
	}

	public static void InitAndReserve<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid, int size)
	{
		container.Init(zid);
		container[zid].Reserve(size);
	}

	public static List<ZDOID> GetAllZDOIDsWithHash<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, int hash)
	{
		List<ZDOID> list = new List<ZDOID>();
		foreach (KeyValuePair<ZDOID, BinarySearchDictionary<int, TType>> item in container)
		{
			foreach (KeyValuePair<int, TType> item2 in item.Value)
			{
				if (item2.Key == hash)
				{
					list.Add(item.Key);
					break;
				}
			}
		}
		return list;
	}

	public static List<KeyValuePair<int, TType>> GetValuesOrEmpty<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid)
	{
		if (!container.ContainsKey(zid))
		{
			return Array.Empty<KeyValuePair<int, TType>>().ToList();
		}
		return ((IEnumerable<KeyValuePair<int, TType>>)container[zid]).ToList();
	}

	public static bool GetValue<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid, int hash, out TType value)
	{
		if (!container.ContainsKey(zid))
		{
			value = default(TType);
			return false;
		}
		return container[zid].TryGetValue(hash, ref value);
	}

	public static TType GetValueOrDefault<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid, int hash, TType defaultValue)
	{
		if (!container.ContainsKey(zid))
		{
			return defaultValue;
		}
		return container[zid].GetValueOrDefault(hash, defaultValue);
	}

	public static void Release<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid)
	{
		if (container.ContainsKey(zid))
		{
			container[zid].Clear();
			Pool<BinarySearchDictionary<int, TType>>.Release(container[zid]);
			container[zid] = null;
			container.Remove(zid);
		}
	}

	private static void Init<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID zid)
	{
		if (!container.ContainsKey(zid))
		{
			container.Add(zid, Pool<BinarySearchDictionary<int, TType>>.Create());
		}
	}

	public static bool Remove<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container, ZDOID id, int hash)
	{
		if (!container.ContainsKey(id) || !container[id].ContainsKey(hash))
		{
			return false;
		}
		container[id].Remove(hash);
		if (container[id].Count == 0)
		{
			Pool<BinarySearchDictionary<int, TType>>.Release(container[id]);
			container[id] = null;
			container.Remove(id);
		}
		return true;
	}

	public static Dictionary<ZDOID, BinarySearchDictionary<int, TType>> Clone<TType>(this Dictionary<ZDOID, BinarySearchDictionary<int, TType>> container)
	{
		return container.ToDictionary((KeyValuePair<ZDOID, BinarySearchDictionary<int, TType>> entry) => entry.Key, (KeyValuePair<ZDOID, BinarySearchDictionary<int, TType>> entry) => (BinarySearchDictionary<int, TType>)entry.Value.Clone());
	}

	public static Dictionary<ZDOID, ZDOConnectionHashData> Clone(this Dictionary<ZDOID, ZDOConnectionHashData> container)
	{
		return container.ToDictionary((KeyValuePair<ZDOID, ZDOConnectionHashData> entry) => entry.Key, (KeyValuePair<ZDOID, ZDOConnectionHashData> entry) => entry.Value);
	}

	static ZDOHelper()
	{
		List<int> list = new List<int>();
		KeyValuePair<int, int> s_zdoidUser = ZDOVars.s_zdoidUser;
		list.Add(s_zdoidUser.Key);
		s_zdoidUser = ZDOVars.s_zdoidUser;
		list.Add(s_zdoidUser.Value);
		s_zdoidUser = ZDOVars.s_zdoidRodOwner;
		list.Add(s_zdoidUser.Key);
		s_zdoidUser = ZDOVars.s_zdoidRodOwner;
		list.Add(s_zdoidUser.Value);
		s_zdoidUser = ZDOVars.s_sessionCatchID;
		list.Add(s_zdoidUser.Key);
		s_zdoidUser = ZDOVars.s_sessionCatchID;
		list.Add(s_zdoidUser.Value);
		s_stripOldLongData = list;
		s_stripOldDataByteArray = new List<int> { StringExtensionMethods.GetStableHashCode("health") };
	}
}
