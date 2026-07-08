using System;
using JetBrains.Annotations;

namespace Guilds;

[PublicAPI]
public class ApplicationClass
{
	public PlayerReference player;

	public DateTime applied = DateTime.Now;

	public string description = "";
}
