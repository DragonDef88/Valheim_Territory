using System;
using JetBrains.Annotations;

namespace Guilds;

[PublicAPI]
public class Application
{
	public DateTime applied = DateTime.Now;

	public string description = "";
}
