using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(TerrainOp), "Awake")]
internal static class TerrainOpAwakePatch
{
	private static bool Prefix(TerrainOp __instance)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ZNetView component = ((Component)__instance).GetComponent<ZNetView>();
		if ((Object)(object)component != (Object)null && component.IsValid() && !component.IsOwner())
		{
			return true;
		}
		Player localPlayer = Player.m_localPlayer;
		if (!WardAccess.ShouldBlock(((Component)__instance).transform.position, __instance.GetRadius(), localPlayer))
		{
			return true;
		}
		WardAccess.ShowNoAccessMessage(localPlayer);
		Object.Destroy((Object)(object)((Component)__instance).gameObject);
		return false;
	}
}
