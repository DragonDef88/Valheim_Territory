namespace STUWard;

internal readonly struct ManagedWardProjection
{
	internal string AccountId { get; }

	internal bool HasResolvedGuild { get; }

	internal WardGuildIdentity Guild { get; }

	internal ManagedWardProjection(string accountId, bool hasResolvedGuild, WardGuildIdentity guild)
	{
		AccountId = accountId ?? string.Empty;
		HasResolvedGuild = hasResolvedGuild;
		Guild = guild;
	}
}
