using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(WearNTear), "Remove")]
internal static class WearNTearRemovePatch
{
	private static bool Prefix(WearNTear __instance)
	{
		if (!WardPatchHelpers.ShouldBlockLocalRemoval(((Component)__instance).GetComponent<Piece>()))
		{
			return true;
		}
		WardAccess.ShowNoAccessMessage(Player.m_localPlayer);
		return false;
	}
}
