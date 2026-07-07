using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "TryPlacePiece")]
internal static class PlayerTryPlacePiecePatch
{
	private static bool Prefix(Player __instance, ref bool __result)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		GameObject placementGhost = __instance.m_placementGhost;
		if ((Object)(object)placementGhost == (Object)null)
		{
			return true;
		}
		if (!WardAccess.TryBlockManagedWardPlacement(__instance, (Component?)(object)placementGhost.transform, placementGhost.transform.position, ref __result))
		{
			return false;
		}
		return true;
	}
}
