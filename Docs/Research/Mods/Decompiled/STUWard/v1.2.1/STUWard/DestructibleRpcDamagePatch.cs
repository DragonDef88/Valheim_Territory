using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Destructible), "RPC_Damage")]
internal static class DestructibleRpcDamagePatch
{
	private static bool Prefix(Destructible __instance, long sender, HitData hit)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return WardPatchHelpers.GetBuildingDamageBlockReason(((Component)__instance).transform.position, WardPatchHelpers.GetProtectedBuildingPiece((Component?)(object)__instance), hit, sender) == BuildingDamageBlockReason.None;
	}
}
