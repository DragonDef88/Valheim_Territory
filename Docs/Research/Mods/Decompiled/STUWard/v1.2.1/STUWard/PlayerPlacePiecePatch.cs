using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "PlacePiece")]
internal static class PlayerPlacePiecePatch
{
	private static bool Prefix(Player __instance, Piece piece, Vector3 pos)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (!WardAccess.TryBlockManagedWardPlacement(__instance, (Component?)(object)piece, pos))
		{
			return false;
		}
		return WardAccess.TryBlockPlacement(__instance, pos, 0f);
	}
}
