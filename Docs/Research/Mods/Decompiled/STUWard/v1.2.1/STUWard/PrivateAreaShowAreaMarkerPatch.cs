using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "ShowAreaMarker")]
internal static class PrivateAreaShowAreaMarkerPatch
{
	private static bool Prefix(PrivateArea __instance)
	{
		if (!WardAccess.IsManagedWard(__instance, requireEnabled: false))
		{
			return true;
		}
		if (!WardSettings.ShouldShowAreaMarker(__instance))
		{
			return true;
		}
		WardSettings.ShowManagedAreaMarker(__instance);
		return false;
	}
}
