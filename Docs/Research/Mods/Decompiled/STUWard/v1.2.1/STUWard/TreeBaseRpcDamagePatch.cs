using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(TreeBase), "RPC_Damage")]
internal static class TreeBaseRpcDamagePatch
{
	private static bool Prefix(TreeBase __instance, long sender)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return WardPatchHelpers.EvaluateDamageBySender(((Component)__instance).transform.position, sender) == WardPatchHelpers.ProtectedRpcDecision.Allow;
	}
}
