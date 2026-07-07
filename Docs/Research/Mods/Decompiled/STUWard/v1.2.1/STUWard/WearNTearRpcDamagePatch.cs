using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
internal static class WearNTearRpcDamagePatch
{
	private static bool Prefix(WearNTear __instance, long sender, HitData hit)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (ManagedWardIdentity.EnsureManagedComponent(((Component)__instance).GetComponent<PrivateArea>()))
		{
			return false;
		}
		return WardPatchHelpers.GetBuildingDamageBlockReason(((Component)__instance).transform.position, ((Component)__instance).GetComponent<Piece>(), hit, sender) == BuildingDamageBlockReason.None;
	}
}
