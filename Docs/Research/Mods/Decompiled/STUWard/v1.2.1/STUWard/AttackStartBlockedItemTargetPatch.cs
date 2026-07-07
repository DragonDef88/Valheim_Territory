using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(Attack), "Start")]
[HarmonyPriority(800)]
[HarmonyBefore(new string[] { "meldurson.valheim.PortablePals" })]
internal static class AttackStartBlockedItemTargetPatch
{
	private static bool Prefix(Humanoid character, ItemData weapon, ref bool __result)
	{
		return WardAccess.TryBlockAttack(WardAccess.GetPlayer(character), weapon, ref __result);
	}
}
