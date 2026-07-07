using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Attack), "SpawnOnHitTerrain")]
internal static class AttackSpawnOnHitTerrainPatch
{
	private static bool Prefix(Vector3 hitPoint, GameObject prefab, Character character, ref GameObject __result)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Player player = (Player)(object)((character is Player) ? character : null);
		if (!WardAccess.ShouldBlock(hitPoint, WardAccess.GetTerrainRadius(prefab), player))
		{
			return true;
		}
		WardAccess.ShowNoAccessMessage(player);
		__result = null;
		return false;
	}
}
