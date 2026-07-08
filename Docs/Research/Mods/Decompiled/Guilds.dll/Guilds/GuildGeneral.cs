using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace Guilds;

[PublicAPI]
public class GuildGeneral
{
	public int id;

	public string description = "";

	[_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMember(Alias = "icon id", ApplyNamingConventions = false)]
	public int icon = 1;

	public int level;

	[_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMember(Alias = "guild color", ApplyNamingConventions = false)]
	public string color = "";
}
