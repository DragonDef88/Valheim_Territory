using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using JetBrains.Annotations;
using UnityEngine;

namespace Guilds;

[PublicAPI]
public static class API
{
	public delegate void AchievementCompleted(PlayerReference player, string achievment);

	public delegate void GuildJoined(Guild guild, PlayerReference player);

	public delegate void GuildLeft(Guild guild, PlayerReference player);

	public delegate void GuildCreated(Guild guild);

	public delegate void GuildDeleted(Guild guild);

	public static List<GuildJoined> OnGuildJoined = new List<GuildJoined>();

	public static List<GuildLeft> OnGuildLeft = new List<GuildLeft>();

	public static List<GuildCreated> OnGuildCreated = new List<GuildCreated>();

	public static List<GuildDeleted> OnGuildDeleted = new List<GuildDeleted>();

	public static bool IsLoaded()
	{
		return true;
	}

	public static Guild? GetPlayerGuild(PlayerReference player)
	{
		KeyValuePair<string, Guild> keyValuePair = GuildList.guildList.FirstOrDefault<KeyValuePair<string, Guild>>((KeyValuePair<string, Guild> s) => s.Value.Members.ContainsKey(player));
		if (!Utility.IsNullOrWhiteSpace(keyValuePair.Key))
		{
			return keyValuePair.Value;
		}
		return null;
	}

	public static Guild? GetPlayerGuild(Player player)
	{
		return GetPlayerGuild(PlayerReference.fromPlayer(player));
	}

	public static Guild? GetOwnGuild()
	{
		return GetPlayerGuild(PlayerReference.forOwnPlayer());
	}

	public static Guild? GetGuild(string name)
	{
		if (GuildList.guildList.TryGetValue(name, out Guild value))
		{
			return value;
		}
		return null;
	}

	public static Guild? GetGuild(int id)
	{
		if (GuildList.guildsById.TryGetValue(id, out Guild value))
		{
			return value;
		}
		return null;
	}

	public static List<Guild> GetGuilds()
	{
		return GuildList.guildList.Values.ToList();
	}

	public static PlayerReference GetGuildLeader(Guild guild)
	{
		return guild.Members.FirstOrDefault<KeyValuePair<PlayerReference, GuildMember>>((KeyValuePair<PlayerReference, GuildMember> r) => r.Value.rank == Ranks.Leader).Key;
	}

	public static Guild? CreateGuild(string name, PlayerReference leader)
	{
		if (GuildList.guildList.TryGetValue(name, out Guild _))
		{
			return null;
		}
		int id = GuildList.guildsById.Keys.DefaultIfEmpty().Max() + 1;
		Guild guild = new Guild
		{
			General = new GuildGeneral
			{
				id = id
			},
			Name = name
		};
		guild.Members.Add(leader, new GuildMember
		{
			rank = Ranks.Leader
		});
		GuildList.guildList[name] = guild;
		GuildList.updateGuild(name);
		return guild;
	}

	public static bool DeleteGuild(Guild guild)
	{
		if (GuildList.guildList.ContainsKey(guild.Name))
		{
			GuildList.removeGuild(guild.Name);
			return true;
		}
		return false;
	}

	public static bool RenameGuild(Guild guild, string newName)
	{
		if (GuildList.guildList.ContainsKey(newName))
		{
			return false;
		}
		GuildList.guildList[newName] = GuildList.guildList[guild.Name];
		GuildList.guildList.Remove(guild.Name);
		GuildList.renameGuild(guild.Name, newName);
		return true;
	}

	public static bool SaveGuild(Guild guild)
	{
		if (GuildList.guildList.ContainsKey(guild.Name))
		{
			GuildList.guildList[guild.Name] = guild;
			GuildList.updateGuild(guild.Name);
			return true;
		}
		return false;
	}

	public static bool AddPlayerToGuild(PlayerReference player, Guild guild)
	{
		if (GetPlayerGuild(player) == null)
		{
			guild.Members.Add(player, new GuildMember());
			SaveGuild(guild);
			InvokeGuildJoined(guild, player);
			return true;
		}
		return false;
	}

	public static bool RemovePlayerFromGuild(PlayerReference player)
	{
		Guild playerGuild = GetPlayerGuild(player);
		if (playerGuild != null)
		{
			playerGuild.Members.Remove(player);
			SaveGuild(playerGuild);
			InvokeGuildLeft(playerGuild, player);
			return true;
		}
		return false;
	}

	public static Ranks GetPlayerRank(PlayerReference player)
	{
		return GetPlayerGuild(player)?.Members[player].rank ?? Ranks.Leader;
	}

	public static bool UpdatePlayerRank(PlayerReference player, Ranks newRank)
	{
		Guild playerGuild = GetPlayerGuild(player);
		if (playerGuild != null)
		{
			GuildMember guildMember = playerGuild.Members[player];
			guildMember.rank = newRank;
			playerGuild.Members[player] = guildMember;
			SaveGuild(playerGuild);
			return true;
		}
		return false;
	}

	public static IEnumerable<PlayerReference> GetOnlinePlayers(Guild? guild = null)
	{
		HashSet<PlayerReference> hashSet = new HashSet<PlayerReference>(ZNet.instance.m_players.Select(PlayerReference.fromPlayerInfo));
		if (guild != null)
		{
			return guild.Members.Keys.Where(hashSet.Contains);
		}
		foreach (PlayerReference item in GetGuilds().SelectMany((Guild g) => g.Members.Keys))
		{
			hashSet.Remove(item);
		}
		return hashSet;
	}

	public static Guild? GetOwnAppliedGuild()
	{
		return GetPlayerAppliedGuild(PlayerReference.forOwnPlayer());
	}

	public static Guild? GetPlayerAppliedGuild(PlayerReference player)
	{
		return GuildList.guildList.FirstOrDefault<KeyValuePair<string, Guild>>((KeyValuePair<string, Guild> s) => s.Value.Applications.ContainsKey(player)).Value;
	}

	public static bool ApplyToGuild(PlayerReference player, string description, Guild guild)
	{
		RemovePlayerApplication(player);
		guild.Applications[player] = new Application
		{
			description = description
		};
		return SaveGuild(guild);
	}

	public static bool RemovePlayerApplication(PlayerReference player, Guild? guild = null)
	{
		Guild playerAppliedGuild = GetPlayerAppliedGuild(player);
		if (playerAppliedGuild != null && (guild == null || playerAppliedGuild == guild))
		{
			playerAppliedGuild.Applications.Remove(player);
			return SaveGuild(playerAppliedGuild);
		}
		return true;
	}

	public static void RegisterCustomData(Type type)
	{
		CustomDataConverter.RegisteredCustomTypes.Add(type.FullName, type);
	}

	public static T? GetCustomData<T>(Guild guild) where T : class
	{
		guild.customData.data.TryGetValue(typeof(T), out object value);
		return (T)value;
	}

	public static void SetCustomData<T>(Guild guild, T customData) where T : class
	{
		guild.customData.data[typeof(T)] = customData;
	}

	public static void IncreaseAchievementProgress(Guild guild, string achievement, float increment = 1f)
	{
		if (guild.Achievements.TryGetValue(achievement, out AchievementData value))
		{
			float? progress = value.progress;
			if (!progress.HasValue)
			{
				return;
			}
		}
		GuildList.increaseAchievement(guild.General.id, achievement, increment);
	}

	public static void RegisterOnAchievementCompleted(AchievementCompleted callback)
	{
		Achievements.achievementCompletedCallbacks.Add(callback);
	}

	public static void RegisterAchievement(string name, AchievementConfig config)
	{
		Achievements.dynamicAchievementConfigs.Add(name, config);
	}

	internal static AchievementConfig? GetAchievementConfig(string achievement)
	{
		return Achievements.GetAchievementConfig(achievement);
	}

	internal static IEnumerable<KeyValuePair<string, AchievementConfig>> AllAchievementConfigs()
	{
		return Achievements.AllAchievementConfigs();
	}

	public static Sprite? GetGuildIcon(Guild guild)
	{
		if (!Interface.GuildIcons.TryGetValue(guild.General.icon, out Sprite value))
		{
			return Interface.GuildIcons[1];
		}
		return value;
	}

	public static Sprite? GetGuildIconById(int iconId)
	{
		if (!Interface.GuildIcons.TryGetValue(iconId, out Sprite value))
		{
			return Interface.GuildIcons[1];
		}
		return value;
	}

	public static int GetAchievementStage(Guild guild, AchievementConfig achievement)
	{
		if (guild.Achievements.TryGetValue(achievement.name, out AchievementData value))
		{
			return value.completed.Count;
		}
		return 0;
	}

	public static void RegisterOnGuildJoined(GuildJoined callback)
	{
		OnGuildJoined.Add(callback);
	}

	public static void RegisterOnGuildLeft(GuildLeft callback)
	{
		OnGuildLeft.Add(callback);
	}

	public static void RegisterOnGuildCreated(GuildCreated callback)
	{
		OnGuildCreated.Add(callback);
	}

	public static void RegisterOnGuildDeleted(GuildDeleted callback)
	{
		OnGuildDeleted.Add(callback);
	}

	internal static void InvokeGuildJoined(Guild guild, PlayerReference player)
	{
		Guild guild2 = guild;
		OnGuildJoined.ForEach(delegate(GuildJoined cb)
		{
			cb(guild2, player);
		});
	}

	internal static void InvokeGuildLeft(Guild guild, PlayerReference player)
	{
		Guild guild2 = guild;
		OnGuildLeft.ForEach(delegate(GuildLeft cb)
		{
			cb(guild2, player);
		});
	}

	internal static void InvokeGuildCreated(Guild guild)
	{
		Guild guild2 = guild;
		OnGuildCreated.ForEach(delegate(GuildCreated cb)
		{
			cb(guild2);
		});
	}

	internal static void InvokeGuildDeleted(Guild guild)
	{
		Guild guild2 = guild;
		OnGuildDeleted.ForEach(delegate(GuildDeleted cb)
		{
			cb(guild2);
		});
	}
}
