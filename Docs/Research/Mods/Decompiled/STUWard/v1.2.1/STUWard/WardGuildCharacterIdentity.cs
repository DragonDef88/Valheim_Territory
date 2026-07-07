namespace STUWard;

internal readonly struct WardGuildCharacterIdentity
{
	internal long PlayerId { get; }

	internal string AccountId { get; }

	internal string PlayerName { get; }

	internal bool HasPlayerId => PlayerId != 0;

	internal bool HasAccountAndName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(AccountId))
			{
				return !string.IsNullOrWhiteSpace(PlayerName);
			}
			return false;
		}
	}

	internal WardGuildCharacterIdentity(long playerId, string accountId, string playerName)
	{
		PlayerId = playerId;
		AccountId = WardOwnership.NormalizeAccountIdValue(accountId);
		PlayerName = playerName?.Trim() ?? string.Empty;
	}
}
