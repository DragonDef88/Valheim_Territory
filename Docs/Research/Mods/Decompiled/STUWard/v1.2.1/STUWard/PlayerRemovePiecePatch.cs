using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Player), "RemovePiece")]
internal static class PlayerRemovePiecePatch
{
	private static bool Prefix(Player __instance, ref bool __result, out bool __state)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Piece val = WardPatchHelpers.FindRemoveTarget(__instance);
		__state = ManagedWardIdentity.IsManaged(((Object)(object)val != (Object)null) ? ((Component)val).GetComponent<PrivateArea>() : null);
		if ((Object)(object)val == (Object)null || !WardAccess.ShouldBlock(((Component)val).transform.position, 0f, __instance))
		{
			return true;
		}
		__state = false;
		WardAccess.ShowNoAccessMessage(__instance);
		__result = false;
		return false;
	}

	private static void Postfix(Player __instance, bool __state)
	{
	}
}
