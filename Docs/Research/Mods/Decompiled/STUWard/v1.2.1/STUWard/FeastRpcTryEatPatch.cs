using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Feast), "RPC_TryEat")]
internal static class FeastRpcTryEatPatch
{
	private static bool Prefix(Feast __instance, long sender)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		switch (WardPatchHelpers.EvaluateInteractionBySender(((Component)__instance).transform.position, sender, WardRestrictionOptions.PlacedConsumables))
		{
		case WardPatchHelpers.ProtectedRpcDecision.Allow:
			return true;
		case WardPatchHelpers.ProtectedRpcDecision.Deny:
			WardAccess.ShowNoAccessMessage(WardPatchHelpers.GetLocalPlayerForSender(sender));
			break;
		}
		return false;
	}
}
