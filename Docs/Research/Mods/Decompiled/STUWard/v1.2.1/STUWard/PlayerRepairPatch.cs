using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "Repair")]
internal static class PlayerRepairPatch
{
	private static bool Prefix(Player __instance, Piece __1)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Piece val = __instance.GetHoveringPiece() ?? __1;
		if ((Object)(object)val == (Object)null || !WardAccess.ShouldBlock(((Component)val).transform.position, 0f, __instance))
		{
			return true;
		}
		WardAccess.ShowNoAccessMessage(__instance);
		return false;
	}
}
