using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(InventoryGui), "OnRightClickItem")]
[HarmonyPriority(800)]
[HarmonyBefore(new string[] { "kg.TameableCollector" })]
internal static class InventoryGuiOnRightClickItemPatch
{
	private static bool Prefix(ItemData item)
	{
		return WardAccess.TryBlockItemUse(Player.m_localPlayer, item);
	}
}
