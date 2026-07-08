using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace StatManager;

[PublicAPI]
internal class Stat
{
	private static readonly Dictionary<string, PlayerStatType> stats;

	private static bool patched;

	public readonly PlayerStatType StatType;

	public Stat(string englishName)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected I4, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		StatType = fromName(englishName);
		stats.Add(englishName, StatType);
		if (patched)
		{
			AddStats(new string[1] { englishName }, (IEnumerable<PlayerStatType>)(object)new PlayerStatType[1] { (PlayerStatType)(int)StatType });
			PlayerProfile playerProfile = Game.instance.m_playerProfile;
			if (playerProfile != null)
			{
				playerProfile.m_playerStats[StatType] = 0f;
			}
		}
	}

	public static PlayerStatType fromName(string englishName)
	{
		return (PlayerStatType)Math.Abs(StringExtensionMethods.GetStableHashCode(englishName));
	}

	public void Increment(float amount = 1f)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Game.instance))
		{
			Game.instance.IncrementPlayerStat(StatType, amount);
		}
	}

	public float Get()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Game.instance))
		{
			return Game.instance.GetPlayerStat(StatType);
		}
		return 0f;
	}

	static Stat()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected O, but got Unknown
		//IL_011d: Expected O, but got Unknown
		stats = new Dictionary<string, PlayerStatType>();
		patched = false;
		Harmony val = new Harmony("org.bepinex.helpers.statmanager");
		val.Patch((MethodBase)AccessTools.DeclaredConstructor(typeof(PlayerStats), (Type[])null, false), (HarmonyMethod)null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Stat), "Patch_PlayerStats", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		val.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(FejdStartup), "Awake", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Stat), "Patch_FejdStartup", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		val.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(PlayerProfile), "LoadPlayerFromDisk", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Stat), "Patch_LoadPlayer", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		val.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(PlayerProfile), "SavePlayerToDisk", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Stat), "Patch_SavePlayer", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Stat), "Patch_SavePlayerCleanup", (Type[])null, (Type[])null)), (HarmonyMethod)null);
	}

	private static void Patch_PlayerStats(PlayerStats __instance)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (PlayerStatType value in stats.Values)
		{
			__instance.m_stats[value] = 0f;
		}
	}

	private static void AddStats(IEnumerable<string> names, IEnumerable<PlayerStatType> values)
	{
		object obj = AccessTools.DeclaredMethod(typeof(Enum), "GetCachedValuesAndNames", (Type[])null, (Type[])null).Invoke(null, new object[2]
		{
			typeof(PlayerStatType),
			true
		});
		FieldInfo fieldInfo = AccessTools.DeclaredField(obj.GetType(), "Values");
		FieldInfo fieldInfo2 = AccessTools.DeclaredField(obj.GetType(), "Names");
		fieldInfo.SetValue(obj, ((ulong[])fieldInfo.GetValue(obj)).Concat(values.Select((PlayerStatType v) => (ulong)(long)v)).ToArray());
		fieldInfo2.SetValue(obj, ((string[])fieldInfo2.GetValue(obj)).Concat(names).ToArray());
	}

	private static void Patch_FejdStartup()
	{
		if (!patched)
		{
			patched = true;
			AddStats(stats.Keys, stats.Values);
		}
	}

	private static void Patch_LoadPlayer(PlayerProfile __instance)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<string, PlayerStatType> stat in stats)
		{
			string key = "StatManager " + stat.Key;
			if (__instance.m_knownCommands.TryGetValue(key, out var value))
			{
				__instance.m_playerStats.m_stats[stat.Value] = value;
				__instance.m_knownCommands.Remove(key);
			}
		}
	}

	private static void Patch_SavePlayer(PlayerProfile __instance)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<string, PlayerStatType> stat in stats)
		{
			__instance.m_knownCommands["StatManager " + stat.Key] = __instance.m_playerStats[stat.Value];
		}
	}

	private static void Patch_SavePlayerCleanup(PlayerProfile __instance)
	{
		foreach (KeyValuePair<string, PlayerStatType> stat in stats)
		{
			__instance.m_knownCommands.Remove("StatManager " + stat.Key);
		}
	}
}
