using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Humanoid), "UpdateEquipment")]
internal static class HumanoidUpdateEquipmentPatch
{
	private const float ForceUnequipCheckIntervalSeconds = 0.15f;

	private static float _nextForceUnequipCheckTime;

	private static void Prefix(Humanoid __instance)
	{
		if (!((Object)(object)__instance != (Object)(object)Player.m_localPlayer) && WardAccess.HasEnabledManagedWards() && Plugin.HasBlockedItems() && !(Time.unscaledTime < _nextForceUnequipCheckTime))
		{
			_nextForceUnequipCheckTime = Time.unscaledTime + 0.15f;
			WardAccess.TryForceUnequipBlockedItems(Player.m_localPlayer);
		}
	}
}
