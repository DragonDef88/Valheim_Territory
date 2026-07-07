namespace STUWard;

internal readonly struct ManagedWardRegistryEntry
{
	internal ZDOID ZdoId { get; }

	internal long OwnerPlayerId { get; }

	internal string AccountId { get; }

	internal string OwnerName { get; }

	internal string CharacterKey { get; }

	internal int GuildId { get; }

	internal ManagedWardRegistryEntry(ZDOID zdoId, long ownerPlayerId, string accountId, string ownerName, string characterKey, int guildId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		ZdoId = zdoId;
		OwnerPlayerId = ownerPlayerId;
		AccountId = accountId ?? string.Empty;
		OwnerName = ownerName ?? string.Empty;
		CharacterKey = characterKey ?? string.Empty;
		GuildId = guildId;
	}
}
