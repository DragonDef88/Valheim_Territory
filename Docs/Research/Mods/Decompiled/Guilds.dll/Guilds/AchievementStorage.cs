using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Guilds;

public class AchievementStorage
{
	public readonly Dictionary<string, DateTime> lastBossKillTimes = new Dictionary<string, DateTime>();

	public readonly List<string> guildBossKills = new List<string>();

	[_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlIgnore]
	private Guild? guild;

	public static AchievementStorage get(Guild? guild = null)
	{
		if (guild == null)
		{
			guild = API.GetOwnGuild();
		}
		if (guild != null)
		{
			AchievementStorage customData = API.GetCustomData<AchievementStorage>(guild);
			if (customData != null)
			{
				customData.guild = guild;
				return customData;
			}
			customData = new AchievementStorage
			{
				guild = guild
			};
			API.SetCustomData(guild, customData);
			return customData;
		}
		return new AchievementStorage();
	}

	public void Save()
	{
		if (guild != null)
		{
			API.SetCustomData(guild, this);
		}
	}
}
