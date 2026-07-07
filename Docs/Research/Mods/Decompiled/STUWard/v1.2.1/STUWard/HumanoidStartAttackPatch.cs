using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(Humanoid), "StartAttack")]
internal static class HumanoidStartAttackPatch
{
	private static bool Prefix(Humanoid __instance, ref bool __result)
	{
		return WardAccess.TryBlockAttack(WardAccess.GetPlayer(__instance), ref __result);
	}
}
