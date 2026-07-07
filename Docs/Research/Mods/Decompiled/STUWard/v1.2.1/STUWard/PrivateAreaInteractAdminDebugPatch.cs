using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "Interact")]
internal static class PrivateAreaInteractAdminDebugPatch
{
	private static bool Prefix(PrivateArea __instance, Humanoid human, bool hold, bool alt, ref bool __result)
	{
		Player val = (Player)(object)((human is Player) ? human : null);
		if (val == null || (Object)(object)val != (Object)(object)Player.m_localPlayer)
		{
			return true;
		}
		if (!WardAccess.IsManagedWard(__instance, requireEnabled: false))
		{
			return true;
		}
		return ManagedWardInteractionRpc.TryHandleInteract(__instance, val, hold, ref __result);
	}
}
