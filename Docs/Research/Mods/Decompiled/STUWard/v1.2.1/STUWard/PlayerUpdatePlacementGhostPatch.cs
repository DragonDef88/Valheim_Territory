using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
internal static class PlayerUpdatePlacementGhostPatch
{
	private static void Postfix(Player __instance)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		GameObject placementGhost = __instance.m_placementGhost;
		PrivateArea area = (((Object)(object)placementGhost != (Object)null) ? placementGhost.GetComponent<PrivateArea>() : null);
		if ((Object)(object)placementGhost != (Object)null && StuWardArea.IsManaged(area))
		{
			WardSettings.ApplyPlacementGhostPreviewRadius(area);
			if ((int)__instance.GetPlacementStatus() == 0 && ManagedWardPlacementPreviewService.ShouldShowAsInvalid(__instance, (Component?)(object)placementGhost.transform, placementGhost.transform.position))
			{
				__instance.m_placementStatus = (PlacementStatus)5;
				__instance.SetPlacementGhostValid(false);
			}
		}
	}
}
