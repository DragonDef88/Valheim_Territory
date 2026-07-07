using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "OnDeath")]
internal static class PlayerOnDeathWardUiPatch
{
	private static void Prefix(Player __instance)
	{
		if (!((Object)(object)__instance != (Object)(object)Player.m_localPlayer))
		{
			WardGuiController.Instance?.CloseWardUi();
		}
	}
}
