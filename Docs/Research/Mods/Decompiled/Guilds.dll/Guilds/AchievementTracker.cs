using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Guilds;

public static class AchievementTracker
{
	[HarmonyPatch(typeof(Character), "OnDeath")]
	public static class TrackBossKills
	{
		private static void Prefix(Character __instance)
		{
			Guild ownGuild = API.GetOwnGuild();
			if (ownGuild == null || !bosses.Contains(Utils.GetPrefabName(((Component)__instance).gameObject)))
			{
				return;
			}
			AchievementStorage achievementStorage = AchievementStorage.get(ownGuild);
			if (!achievementStorage.guildBossKills.Contains(Utils.GetPrefabName(((Component)__instance).gameObject)))
			{
				AchievementConfig achievementConfig = API.GetAchievementConfig("Guild Boss Kills");
				if (achievementConfig != null && Tools.GetNearbyGuildMembers(Player.m_localPlayer, 40f, includeSelf: true).Count >= achievementConfig.getConfigValue("required members", 3))
				{
					achievementStorage.guildBossKills.Add(((Object)__instance).name);
					achievementStorage.Save();
					if (achievementStorage.guildBossKills.Count == bosses.Count)
					{
						API.IncreaseAchievementProgress(ownGuild, "Guild Boss Kills");
					}
				}
			}
			AchievementConfig timedKillsConfig = API.GetAchievementConfig("Guild Boss Kills Timed");
			if (timedKillsConfig != null)
			{
				if (achievementStorage.lastBossKillTimes.All<KeyValuePair<string, DateTime>>((KeyValuePair<string, DateTime> k) => k.Value < DateTime.Now.Subtract(new TimeSpan(0, 0, timedKillsConfig.getConfigValue("maximum time between kills", 10)))))
				{
					achievementStorage.lastBossKillTimes.Clear();
				}
				achievementStorage.lastBossKillTimes[((Object)__instance).name] = DateTime.Now;
				if (achievementStorage.lastBossKillTimes.Count == bosses.Count)
				{
					API.IncreaseAchievementProgress(ownGuild, "Guild Boss Kills Timed");
				}
				achievementStorage.Save();
			}
		}
	}

	[HarmonyPatch(typeof(Trader), "Interact")]
	private static class TrackTraderDiscovery
	{
		private static void Postfix(Trader __instance)
		{
			Guild ownGuild = API.GetOwnGuild();
			if (ownGuild != null)
			{
				if (Utils.GetPrefabName(((Component)__instance).gameObject) == "Haldor")
				{
					API.IncreaseAchievementProgress(ownGuild, "Found Haldor");
				}
				if (Utils.GetPrefabName(((Component)__instance).gameObject) == "Hildir")
				{
					API.IncreaseAchievementProgress(ownGuild, "Found Hildir");
				}
			}
		}
	}

	[HarmonyPatch(typeof(Trader), "OnBought")]
	private static class TrackCoinsSpend
	{
		private static void Postfix(TradeItem item)
		{
			Guild ownGuild = API.GetOwnGuild();
			if (ownGuild != null)
			{
				API.IncreaseAchievementProgress(ownGuild, "Spend Coins", item.m_price);
			}
		}
	}

	private static readonly List<string> bosses = new List<string> { "Eikthyr", "gd_king", "Bonemass", "Dragon", "GoblinKing", "SeekerQueen", "Fader" };
}
