using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Guilds;

public static class Achievements
{
	[HarmonyPatch(typeof(PlayerProfile), "IncrementStat")]
	private static class IncreaseGuildAchievementProgress
	{
		private static void Postfix(PlayerStatType stat, float amount)
		{
			Guild ownGuild = API.GetOwnGuild();
			if (ownGuild != null)
			{
				API.IncreaseAchievementProgress(ownGuild, ((object)(PlayerStatType)(ref stat)).ToString().ToLowerInvariant(), amount);
			}
		}
	}

	private static readonly CustomSyncedValue<string> achievementConfigData = new CustomSyncedValue<string>(Guilds.configSync, "achievementConfig", "");

	private static Dictionary<string, AchievementConfig> achievementConfigs = new Dictionary<string, AchievementConfig>();

	internal static readonly Dictionary<string, AchievementConfig> dynamicAchievementConfigs = new Dictionary<string, AchievementConfig>();

	private static string achievementConfigPath = null;

	internal static readonly List<API.AchievementCompleted> achievementCompletedCallbacks = new List<API.AchievementCompleted>();

	internal static void Init()
	{
		string? directoryName = Path.GetDirectoryName(((BaseUnityPlugin)Guilds.self).Config.ConfigFilePath);
		char directorySeparatorChar = Path.DirectorySeparatorChar;
		achievementConfigPath = directoryName + directorySeparatorChar + "AchievementConfig.yml";
		achievementConfigData.ValueChanged += ConfigChanged;
		Guilds.guildAchievementConfig.SettingChanged += delegate
		{
			ConfigChanged();
		};
		readAchievementConfigFile();
		Guilds.addFileWatchEvent(new FileSystemWatcher(Path.GetDirectoryName(achievementConfigPath), Path.GetFileName(achievementConfigPath)), delegate
		{
			readAchievementConfigFile();
		});
		API.RegisterOnAchievementCompleted(delegate(PlayerReference player, string achievement)
		{
			AchievementConfig achievementConfig = GetAchievementConfig(achievement);
			if (achievementConfig != null)
			{
				Guild ownGuild = API.GetOwnGuild();
				if (ownGuild != null && ownGuild.Members.ContainsKey(player) && ownGuild.Achievements.TryGetValue(achievement, out AchievementData value))
				{
					AchievementPopup.Queue(player, achievementConfig, value.completed.Count);
				}
			}
		});
		static void ConfigChanged()
		{
			if (Guilds.guildAchievementConfig.Value == Guilds.GuildAchievements.Disabled)
			{
				achievementConfigs = new Dictionary<string, AchievementConfig>();
				return;
			}
			try
			{
				Dictionary<string, AchievementConfig> dictionary = deserializeConfig(achievementConfigData.Value);
				if (Guilds.guildAchievementConfig.Value != Guilds.GuildAchievements.External)
				{
					achievementConfigs = deserializeConfig(Encoding.UTF8.GetString(Tools.ReadEmbeddedFileBytes("AchievementConfig.yml")));
					{
						foreach (KeyValuePair<string, AchievementConfig> item in dictionary)
						{
							achievementConfigs[item.Key] = item.Value;
						}
						return;
					}
				}
				achievementConfigs = dictionary;
			}
			catch (Exception arg)
			{
				Debug.LogError((object)$"Failed to deserialize achievementConfig: {arg}");
			}
		}
	}

	private static Dictionary<string, AchievementConfig> deserializeConfig(string data)
	{
		Dictionary<string, AchievementConfig> dictionary = new Dictionary<string, AchievementConfig>(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder().WithNamingConvention(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECamelCaseNamingConvention.Instance).Build().Deserialize<Dictionary<string, AchievementConfig>>(data) ?? new Dictionary<string, AchievementConfig>(), StringComparer.InvariantCultureIgnoreCase);
		foreach (AchievementConfig value in dictionary.Values)
		{
			AchievementConfig current;
			AchievementConfig achievementConfig = (current = value);
			if (current.progress == null)
			{
				current.progress = new List<float> { 1f };
			}
			current = achievementConfig;
			if (current.level == null)
			{
				current.level = new List<int>();
			}
			current = achievementConfig;
			if (current.config == null)
			{
				current.config = new Dictionary<string, string>();
			}
		}
		return dictionary;
	}

	internal static AchievementConfig? GetAchievementConfig(string achievement)
	{
		if (!achievementConfigs.TryGetValue(achievement, out AchievementConfig value))
		{
			dynamicAchievementConfigs.TryGetValue(achievement, out value);
		}
		return value;
	}

	internal static IEnumerable<KeyValuePair<string, AchievementConfig>> AllAchievementConfigs()
	{
		return achievementConfigs.Concat(dynamicAchievementConfigs);
	}

	private static void readAchievementConfigFile()
	{
		achievementConfigData.AssignLocalValue(File.Exists(achievementConfigPath) ? File.ReadAllText(achievementConfigPath) : "");
	}
}
