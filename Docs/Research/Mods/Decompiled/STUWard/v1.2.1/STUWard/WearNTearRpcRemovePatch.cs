using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(WearNTear), "RPC_Remove")]
internal static class WearNTearRpcRemovePatch
{
	private static bool Prefix(WearNTear __instance, long sender, bool blockDrop)
	{
		Piece component = ((Component)__instance).GetComponent<Piece>();
		if (blockDrop && WardPatchHelpers.IsPlacedConsumablePiece(component))
		{
			switch (WardPatchHelpers.EvaluatePlacedConsumableRemovalBySender(component, sender))
			{
			case WardPatchHelpers.ProtectedRpcDecision.Allow:
				return true;
			case WardPatchHelpers.ProtectedRpcDecision.Deny:
				WardAccess.ShowNoAccessMessage(WardPatchHelpers.GetLocalPlayerForSender(sender));
				break;
			}
			return false;
		}
		switch (WardPatchHelpers.EvaluateRemovalBySender(component, sender))
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
