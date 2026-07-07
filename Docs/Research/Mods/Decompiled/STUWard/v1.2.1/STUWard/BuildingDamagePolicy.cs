namespace STUWard;

internal static class BuildingDamagePolicy
{
	internal static BuildingDamageBlockReason Evaluate(BuildingDamagePolicyInput input)
	{
		if (input.IsBuildingTarget && input.SourceKind == DamageSourceKind.MonsterAI && input.BlocksHostileCreatureDamage)
		{
			return BuildingDamageBlockReason.FriendlyWardProtection;
		}
		if (input.IsBuildingTarget && IsFriendlyBuildingDamageSource(input.SourceKind) && input.InsideEnabledWard)
		{
			return BuildingDamageBlockReason.FriendlyWardProtection;
		}
		if (input.PlayerId != 0L && !input.PlayerHasAccess)
		{
			return BuildingDamageBlockReason.NoAccess;
		}
		return BuildingDamageBlockReason.None;
	}

	private static bool IsFriendlyBuildingDamageSource(DamageSourceKind sourceKind)
	{
		if (sourceKind != DamageSourceKind.Player)
		{
			return sourceKind == DamageSourceKind.TamedCreature;
		}
		return true;
	}
}
