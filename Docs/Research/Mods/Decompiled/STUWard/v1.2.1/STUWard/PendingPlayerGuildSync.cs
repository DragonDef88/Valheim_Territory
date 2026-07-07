using System;

namespace STUWard;

internal readonly struct PendingPlayerGuildSync
{
	internal long SenderUid { get; }

	internal int GuildId { get; }

	internal string GuildName { get; }

	internal DateTime FirstSeenUtc { get; }

	internal PendingPlayerGuildSync(long senderUid, int guildId, string guildName, DateTime firstSeenUtc)
	{
		SenderUid = senderUid;
		GuildId = guildId;
		GuildName = guildName ?? string.Empty;
		FirstSeenUtc = firstSeenUtc;
	}
}
