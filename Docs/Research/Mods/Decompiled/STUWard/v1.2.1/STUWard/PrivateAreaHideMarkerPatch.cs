using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "HideMarker")]
internal static class PrivateAreaHideMarkerPatch
{
	private static bool Prefix(PrivateArea __instance)
	{
		if (!WardAccess.IsManagedWard(__instance, requireEnabled: false))
		{
			return true;
		}
		return !WardSettings.ShouldShowAreaMarker(__instance);
	}
}
