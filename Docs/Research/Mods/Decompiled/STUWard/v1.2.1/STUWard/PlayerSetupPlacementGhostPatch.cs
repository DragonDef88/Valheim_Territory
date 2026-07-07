using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "SetupPlacementGhost")]
internal static class PlayerSetupPlacementGhostPatch
{
	private static void Postfix(Player __instance)
	{
		PrivateArea area = (((Object)(object)__instance.m_placementGhost != (Object)null) ? __instance.m_placementGhost.GetComponent<PrivateArea>() : null);
		if (StuWardArea.IsManaged(area))
		{
			WardSettings.ApplyPlacementGhostPreviewRadius(area);
		}
	}
}
