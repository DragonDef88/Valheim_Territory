namespace STUWard;

internal readonly struct ManagedWardAccessSubject
{
	internal long OwnerPlayerId { get; }

	internal WardGuildIdentity WardGuild { get; }

	internal bool Permitted { get; }

	internal string WardSteamAccountId { get; }

	internal string WardZdoLabel { get; }

	internal ManagedWardAccessSubject(long ownerPlayerId, WardGuildIdentity wardGuild, bool permitted, string wardSteamAccountId, string wardZdoLabel)
	{
		OwnerPlayerId = ownerPlayerId;
		WardGuild = wardGuild;
		Permitted = permitted;
		WardSteamAccountId = wardSteamAccountId ?? string.Empty;
		WardZdoLabel = wardZdoLabel ?? "none";
	}
}
