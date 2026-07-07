using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(Humanoid), "EquipItem")]
internal static class HumanoidEquipItemPatch
{
	private static bool Prefix(Humanoid __instance, ItemData item, ref bool __result)
	{
		return WardAccess.TryBlockItemUse(WardAccess.GetPlayer(__instance), item, ref __result);
	}
}
