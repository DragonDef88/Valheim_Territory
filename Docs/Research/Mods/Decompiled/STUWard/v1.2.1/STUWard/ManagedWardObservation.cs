namespace STUWard;

internal readonly struct ManagedWardObservation
{
	internal ZDO Zdo { get; }

	internal ZDOID ZdoId { get; }

	internal long PlayerId { get; }

	internal string AccountId { get; }

	internal ManagedWardObservation(ZDO zdo, ZDOID zdoId, long playerId, string accountId)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Zdo = zdo;
		ZdoId = zdoId;
		PlayerId = playerId;
		AccountId = accountId ?? string.Empty;
	}
}
