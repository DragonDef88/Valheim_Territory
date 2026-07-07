namespace STUWard;

internal readonly struct ManagedWardScanEntry
{
	internal ZDO Zdo { get; }

	internal ZDOID ZdoId { get; }

	internal long OwnerPlayerId { get; }

	internal string AccountId { get; }

	internal string OwnerName { get; }

	internal bool IsEnabled { get; }

	internal float Radius { get; }

	internal ManagedWardScanEntry(ZDO zdo, ZDOID zdoId, long ownerPlayerId, string accountId, string ownerName, bool isEnabled, float radius)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Zdo = zdo;
		ZdoId = zdoId;
		OwnerPlayerId = ownerPlayerId;
		AccountId = accountId ?? string.Empty;
		OwnerName = ownerName ?? string.Empty;
		IsEnabled = isEnabled;
		Radius = radius;
	}
}
