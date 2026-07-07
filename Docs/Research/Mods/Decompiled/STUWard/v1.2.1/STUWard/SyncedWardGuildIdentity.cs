namespace STUWard;

internal readonly struct SyncedWardGuildIdentity
{
	internal bool HasGuild { get; }

	internal int GuildId { get; }

	internal string GuildName { get; }

	internal SyncedWardGuildIdentity(bool hasGuild, int guildId, string guildName)
	{
		HasGuild = hasGuild;
		GuildId = guildId;
		GuildName = guildName ?? string.Empty;
	}
}
