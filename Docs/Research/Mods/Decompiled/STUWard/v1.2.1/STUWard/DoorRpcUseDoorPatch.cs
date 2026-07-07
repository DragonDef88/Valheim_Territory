using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Door), "RPC_UseDoor")]
internal static class DoorRpcUseDoorPatch
{
	private static void Postfix(Door __instance)
	{
		ZNetView val = (((Object)(object)__instance.m_nview != (Object)null) ? __instance.m_nview : ((Component)__instance).GetComponent<ZNetView>());
		if (!((Object)(object)val == (Object)null) && val.IsValid())
		{
			ZDO zDO = val.GetZDO();
			if (zDO == null || zDO.GetInt(ZDOVars.s_state, 0) == 0)
			{
				WardGuiController.Instance?.CancelDoorAutoClose(__instance);
			}
			else
			{
				WardGuiController.Instance?.ScheduleDoorAutoClose(__instance);
			}
		}
	}
}
