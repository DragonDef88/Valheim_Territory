using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace Guilds;

[PublicAPI]
public class GuildMember
{
	public Ranks rank = Ranks.Member;

	[_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMember(Alias = "last online", ApplyNamingConventions = false)]
	public DateTime lastOnline = DateTime.Now;

	public Dictionary<int, Dictionary<string, int>> contribution = new Dictionary<int, Dictionary<string, int>>();
}
