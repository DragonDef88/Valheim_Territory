namespace STUWard;

internal readonly struct BuildingDamagePolicyInput
{
	internal bool IsBuildingTarget { get; }

	internal DamageSourceKind SourceKind { get; }

	internal long PlayerId { get; }

	internal bool InsideEnabledWard { get; }

	internal bool BlocksHostileCreatureDamage { get; }

	internal bool PlayerHasAccess { get; }

	internal BuildingDamagePolicyInput(bool isBuildingTarget, DamageSourceKind sourceKind, long playerId, bool insideEnabledWard, bool blocksHostileCreatureDamage, bool playerHasAccess)
	{
		IsBuildingTarget = isBuildingTarget;
		SourceKind = sourceKind;
		PlayerId = playerId;
		InsideEnabledWard = insideEnabledWard;
		BlocksHostileCreatureDamage = blocksHostileCreatureDamage;
		PlayerHasAccess = playerHasAccess;
	}
}
