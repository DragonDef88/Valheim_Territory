using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(WearNTear), "Damage")]
internal static class WearNTearDamagePatch
{
	private static bool Prefix(WearNTear __instance, HitData hit)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Character attacker = hit.GetAttacker();
		Piece component = ((Component)__instance).GetComponent<Piece>();
		PrivateArea component2 = ((Component)__instance).GetComponent<PrivateArea>();
		if (ManagedWardIdentity.EnsureManagedComponent(component2))
		{
			Player localPlayerForCharacter = WardPatchHelpers.GetLocalPlayerForCharacter(attacker);
			if ((Object)(object)localPlayerForCharacter != (Object)null && !WardAccess.CanControlManagedWard(component2, localPlayerForCharacter.GetPlayerID()))
			{
				WardAccess.ShowNoAccessMessage(localPlayerForCharacter);
			}
			return false;
		}
		BuildingDamageBlockReason buildingDamageBlockReason = WardPatchHelpers.GetBuildingDamageBlockReason(((Component)__instance).transform.position, component, attacker);
		if (buildingDamageBlockReason == BuildingDamageBlockReason.None)
		{
			return true;
		}
		Player localPlayerForCharacter2 = WardPatchHelpers.GetLocalPlayerForCharacter(attacker);
		if (buildingDamageBlockReason == BuildingDamageBlockReason.FriendlyWardProtection)
		{
			WardAccess.ShowProtectedBuildingDamageMessage(localPlayerForCharacter2);
		}
		else
		{
			WardAccess.ShowNoAccessMessage(localPlayerForCharacter2);
		}
		return false;
	}
}
