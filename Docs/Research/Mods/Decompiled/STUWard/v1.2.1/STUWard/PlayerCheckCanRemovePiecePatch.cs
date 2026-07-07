using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "CheckCanRemovePiece")]
internal static class PlayerCheckCanRemovePiecePatch
{
	[HarmonyPriority(0)]
	private static void Postfix(Player __instance, Piece __0, ref bool __result)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)__0 == (Object)null) && WardAccess.ShouldBlock(((Component)__0).transform.position, 0f, __instance))
		{
			WardAccess.ShowNoAccessMessage(__instance);
			__result = false;
		}
	}
}
