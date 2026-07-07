using System;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(ZNetScene), "Destroy", new Type[] { typeof(GameObject) })]
internal static class ZNetSceneDestroyPatch
{
	private static bool Prefix(GameObject go)
	{
		if (!WardPatchHelpers.ShouldBlockLocalRemoval(((Object)(object)go != (Object)null) ? (go.GetComponent<Piece>() ?? go.GetComponentInParent<Piece>()) : null))
		{
			return true;
		}
		WardAccess.ShowNoAccessMessage(Player.m_localPlayer);
		return false;
	}
}
