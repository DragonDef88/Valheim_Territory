using System.Collections.Generic;
using JetBrains.Annotations;

namespace Guilds;

[PublicAPI]
public class Guild
{
	public string Name = "";

	public GuildGeneral General = new GuildGeneral();

	public Dictionary<PlayerReference, GuildMember> Members = new Dictionary<PlayerReference, GuildMember>();

	public Dictionary<PlayerReference, Application> Applications = new Dictionary<PlayerReference, Application>();

	public Dictionary<string, AchievementData> Achievements = new Dictionary<string, AchievementData>();

	public CustomData customData = new CustomData();
}
