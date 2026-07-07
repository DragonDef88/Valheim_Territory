using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(Minimap), "SetMapMode")]
internal static class MinimapSetMapModeWardPinsPatch
{
	private static void Postfix(Minimap __instance, MapMode mode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		WardMinimapPinsManager.HandleMapModeChanged(__instance, mode);
	}
}
