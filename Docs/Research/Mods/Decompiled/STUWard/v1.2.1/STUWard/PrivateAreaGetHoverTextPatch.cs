using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "GetHoverText")]
internal static class PrivateAreaGetHoverTextPatch
{
	private static void Postfix(PrivateArea __instance, ref string __result)
	{
		if (ManagedWardHoverTextService.TryRewriteHoverText(__instance, __result, out string rewrittenText))
		{
			__result = rewrittenText;
		}
	}
}
