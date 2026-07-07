namespace STUWard;

internal readonly struct ManagedWardAccessActor
{
	internal long PlayerId { get; }

	internal WardGuildIdentity PlayerGuild { get; }

	internal bool IsAdminDebug { get; }

	internal ManagedWardAccessActor(long playerId, WardGuildIdentity playerGuild, bool isAdminDebug)
	{
		PlayerId = playerId;
		PlayerGuild = playerGuild;
		IsAdminDebug = isAdminDebug;
	}
}
