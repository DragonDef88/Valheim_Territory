using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(TeleportWorld), "Teleport")]
internal static class TeleportWorldTeleportPatch
{
	private static bool Prefix(TeleportWorld __instance, Player player, out WardCheckScopeState __state)
	{
		__state = default(WardCheckScopeState);
		bool num = WardAccess.TryBlockVoid(WardRestrictionOptions.Portals, (Component)(object)__instance, player);
		if (num)
		{
			__state.EnterRestriction(WardRestrictionOptions.Portals);
		}
		return num;
	}

	private static void Postfix(WardCheckScopeState __state)
	{
		__state.Dispose();
	}
}
