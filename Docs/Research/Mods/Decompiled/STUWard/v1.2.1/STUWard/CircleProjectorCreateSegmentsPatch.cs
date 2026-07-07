using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(CircleProjector), "CreateSegments")]
internal static class CircleProjectorCreateSegmentsPatch
{
	private static void Prefix(CircleProjector __instance)
	{
		PrivateArea componentInParent = ((Component)__instance).GetComponentInParent<PrivateArea>();
		if (!((Object)(object)componentInParent == (Object)null) && !((Object)(object)componentInParent.m_areaMarker != (Object)(object)__instance) && WardAccess.IsManagedWard(componentInParent, requireEnabled: false))
		{
			__instance.m_nrOfSegments = 36;
		}
	}

	private static void Postfix(CircleProjector __instance)
	{
		PrivateArea componentInParent = ((Component)__instance).GetComponentInParent<PrivateArea>();
		ManagedWardRef ward = ManagedWardRef.FromArea(componentInParent);
		if (!((Object)(object)componentInParent == (Object)null) && !((Object)(object)componentInParent.m_areaMarker != (Object)(object)__instance) && WardAccess.IsManagedWard(ward, requireEnabled: false))
		{
			WardSettings.InvalidateAreaMarkerVisuals(componentInParent);
			WardSettings.ApplyAreaState(ward);
		}
	}
}
