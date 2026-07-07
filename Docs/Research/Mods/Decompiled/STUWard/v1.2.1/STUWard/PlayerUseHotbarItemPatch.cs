using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "UseHotbarItem")]
[HarmonyPriority(800)]
[HarmonyBefore(new string[] { "kg.TameableCollector" })]
internal static class PlayerUseHotbarItemPatch
{
	private static bool Prefix(Player __instance, int index)
	{
		if ((Object)(object)__instance != (Object)(object)Player.m_localPlayer || ((Humanoid)__instance).m_inventory == null)
		{
			return true;
		}
		ItemData itemAt = ((Humanoid)__instance).m_inventory.GetItemAt(index - 1, 0);
		return WardAccess.TryBlockItemUse(__instance, itemAt);
	}
}
