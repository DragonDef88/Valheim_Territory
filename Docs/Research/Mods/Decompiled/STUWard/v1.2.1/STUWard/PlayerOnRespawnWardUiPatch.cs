using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "OnRespawn")]
internal static class PlayerOnRespawnWardUiPatch
{
	private static void Postfix(Player __instance)
	{
		if (!((Object)(object)__instance != (Object)(object)Player.m_localPlayer))
		{
			WardGuiController.Instance?.CloseWardUi();
		}
	}
}
