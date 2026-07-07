namespace STUWard;

internal readonly struct ServerSessionIdentity
{
	internal long SenderUid { get; }

	internal ZDOID CharacterZdoId { get; }

	internal long PlayerId { get; }

	internal string AccountId { get; }

	internal string PlayerName { get; }

	internal ServerSessionIdentity(long senderUid, ZDOID characterZdoId, long playerId, string accountId, string playerName)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		SenderUid = senderUid;
		CharacterZdoId = characterZdoId;
		PlayerId = playerId;
		AccountId = accountId ?? string.Empty;
		PlayerName = playerName ?? string.Empty;
	}
}
