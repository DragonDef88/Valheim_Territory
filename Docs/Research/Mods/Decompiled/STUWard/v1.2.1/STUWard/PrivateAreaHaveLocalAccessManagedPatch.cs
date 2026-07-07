using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "HaveLocalAccess")]
internal static class PrivateAreaHaveLocalAccessManagedPatch
{
	private static void Postfix(PrivateArea __instance, ref bool __result)
	{
		if (__result || !WardAccess.IsManagedWard(__instance, requireEnabled: false))
		{
			return;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return;
		}
		if (WardAdminDebugAccess.CanLocallyControlAnyWard(__instance, localPlayer))
		{
			__result = true;
			if (Plugin.ShouldLogWardDiagnosticVerbose())
			{
				object arg = localPlayer.GetPlayerID();
				ZDO? zdo = WardPrivateAreaSafeAccess.GetZdo(__instance);
				Plugin.LogWardDiagnosticVerbose("Access.HaveLocalAccess", string.Format("Granted HaveLocalAccess through admin debug control. playerId={0}, wardZdo={1}", arg, ((zdo != null) ? ((object)(ZDOID)(ref zdo.m_uid)).ToString() : null) ?? "none"));
			}
		}
		else if (WardAccess.IsPlayerInWardGuild(localPlayer, __instance))
		{
			__result = true;
			if (Plugin.ShouldLogWardDiagnosticVerbose())
			{
				object[] obj = new object[4]
				{
					localPlayer.GetPlayerID(),
					GuildsCompat.GetWardGuildId(__instance),
					GuildsCompat.GetWardGuildName(__instance),
					null
				};
				ZDO? zdo2 = WardPrivateAreaSafeAccess.GetZdo(__instance);
				obj[3] = ((zdo2 != null) ? ((object)(ZDOID)(ref zdo2.m_uid)).ToString() : null) ?? "none";
				Plugin.LogWardDiagnosticVerbose("Access.HaveLocalAccess", string.Format("Granted HaveLocalAccess through guild match. playerId={0}, wardGuildId={1}, wardGuildName='{2}', wardZdo={3}", obj));
			}
		}
	}
}
