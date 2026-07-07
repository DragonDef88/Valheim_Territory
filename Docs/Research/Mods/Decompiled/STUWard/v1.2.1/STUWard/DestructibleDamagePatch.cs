using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Destructible), "Damage")]
internal static class DestructibleDamagePatch
{
	private static bool Prefix(Destructible __instance, HitData hit)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Character attacker = hit.GetAttacker();
		BuildingDamageBlockReason buildingDamageBlockReason = WardPatchHelpers.GetBuildingDamageBlockReason(((Component)__instance).transform.position, WardPatchHelpers.GetProtectedBuildingPiece((Component?)(object)__instance), attacker);
		if (buildingDamageBlockReason == BuildingDamageBlockReason.None)
		{
			return true;
		}
		Player localPlayerForCharacter = WardPatchHelpers.GetLocalPlayerForCharacter(attacker);
		if (buildingDamageBlockReason == BuildingDamageBlockReason.FriendlyWardProtection)
		{
			WardAccess.ShowProtectedBuildingDamageMessage(localPlayerForCharacter);
		}
		else
		{
			WardAccess.ShowNoAccessMessage(localPlayerForCharacter);
		}
		return false;
	}
}
