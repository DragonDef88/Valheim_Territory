using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(TeleportWorldTrigger), "OnTriggerEnter")]
[HarmonyPriority(800)]
[HarmonyBefore(new string[] { "org.bepinex.plugins.targetportal" })]
internal static class TeleportWorldTriggerPatch
{
	private static bool Prefix(TeleportWorldTrigger __instance, Collider colliderIn, out WardCheckScopeState __state)
	{
		__state = default(WardCheckScopeState);
		Player player = WardAccess.GetPlayer(colliderIn);
		if ((Object)(object)player == (Object)null)
		{
			return true;
		}
		TeleportWorld componentInParent = ((Component)__instance).GetComponentInParent<TeleportWorld>();
		if ((Object)(object)componentInParent == (Object)null)
		{
			TargetPortalCompat.ClearBlockedPortalEntry(player);
			return true;
		}
		if (WardAccess.TryBlockVoid(WardRestrictionOptions.Portals, (Component)(object)componentInParent, player))
		{
			__state.EnterRestriction(WardRestrictionOptions.Portals);
			TargetPortalCompat.ClearBlockedPortalEntry(player);
			return true;
		}
		TargetPortalCompat.MarkBlockedPortalEntry(player);
		TargetPortalCompat.ClosePortalSelection();
		return false;
	}

	private static void Postfix(WardCheckScopeState __state)
	{
		__state.Dispose();
	}
}
