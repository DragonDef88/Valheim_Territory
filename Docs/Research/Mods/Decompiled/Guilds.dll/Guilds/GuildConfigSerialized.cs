using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace Guilds;

[PublicAPI]
public class GuildConfigSerialized
{
	public GuildGeneral general = new GuildGeneral();

	public List<GuildMemberClass> members = new List<GuildMemberClass>();

	public List<ApplicationClass> applications = new List<ApplicationClass>();

	public Dictionary<string, AchievementData> achievements = new Dictionary<string, AchievementData>();

	[_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMember(Alias = "custom data", ApplyNamingConventions = false)]
	public CustomData customData = new CustomData();

	public Guild toGuild(string name)
	{
		return new Guild
		{
			Name = name,
			Members = new Dictionary<PlayerReference, GuildMember>(members.ToDictionary((GuildMemberClass m) => m.player, (GuildMemberClass m) => new GuildMember
			{
				rank = m.rank,
				lastOnline = m.lastOnline
			})),
			General = general,
			Applications = new Dictionary<PlayerReference, Application>(applications.ToDictionary((ApplicationClass a) => a.player, (ApplicationClass a) => new Application
			{
				applied = a.applied,
				description = a.description
			})),
			Achievements = achievements,
			customData = customData
		};
	}

	public static GuildConfigSerialized fromGuildConfig(Guild from)
	{
		return new GuildConfigSerialized
		{
			members = from.Members.Select<KeyValuePair<PlayerReference, GuildMember>, GuildMemberClass>((KeyValuePair<PlayerReference, GuildMember> kv) => new GuildMemberClass
			{
				player = kv.Key,
				rank = kv.Value.rank,
				lastOnline = kv.Value.lastOnline
			}).ToList(),
			applications = from.Applications.Select<KeyValuePair<PlayerReference, Application>, ApplicationClass>((KeyValuePair<PlayerReference, Application> kv) => new ApplicationClass
			{
				player = kv.Key,
				applied = kv.Value.applied,
				description = kv.Value.description
			}).ToList(),
			general = from.General,
			customData = from.customData,
			achievements = from.Achievements
		};
	}
}
