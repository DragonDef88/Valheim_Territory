using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(TreeBase), "Damage")]
internal static class TreeBaseDamagePatch
{
	private static bool Prefix(TreeBase __instance, HitData hit)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return !WardPatchHelpers.ShouldBlockDamageByCharacter(((Component)__instance).transform.position, hit.GetAttacker());
	}
}
