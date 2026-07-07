using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(Humanoid), "UseItem")]
internal static class HumanoidUseItemPatch
{
	private static bool Prefix(Humanoid __instance, ItemData item)
	{
		return WardAccess.TryBlockItemUse(WardAccess.GetPlayer(__instance), item);
	}
}
