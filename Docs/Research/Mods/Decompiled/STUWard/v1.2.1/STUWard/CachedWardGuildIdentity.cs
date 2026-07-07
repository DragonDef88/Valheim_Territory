using System;

namespace STUWard;

internal readonly struct CachedWardGuildIdentity
{
	internal bool HasGuild { get; }

	internal int GuildId { get; }

	internal string GuildName { get; }

	internal DateTime ExpiresAtUtc { get; }

	internal CachedWardGuildIdentity(bool hasGuild, int guildId, string guildName, DateTime expiresAtUtc)
	{
		HasGuild = hasGuild;
		GuildId = guildId;
		GuildName = guildName;
		ExpiresAtUtc = expiresAtUtc;
	}
}
