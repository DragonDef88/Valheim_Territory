using System;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(ZNetScene), "CreateObject", new Type[] { typeof(ZDO) })]
internal static class ZNetSceneCreateObjectManagedWardPatch
{
	private static void Postfix(ZDO zdo, GameObject __result)
	{
		if (!((Object)(object)__result == (Object)null) && zdo != null && zdo.IsValid())
		{
			PrivateArea val = __result.GetComponent<PrivateArea>() ?? __result.GetComponentInChildren<PrivateArea>();
			if (!((Object)(object)val == (Object)null) && ManagedWardIdentity.TryResolve(val, zdo, repairComponent: true, out var matchedByComponent, out var matchedByZdo))
			{
				Plugin.LogWardDiagnosticVerbose("Placement.CreateObject", $"ZNetScene.CreateObject postfix hit. matchedByComponent={matchedByComponent}, matchedByZdo={matchedByZdo}, isServer={(Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer()}, objectName='{((Object)__result).name}', {WardDiagnosticInfo.DescribeWard(val)}");
				ManagedWardInitializationCoordinator.EnsureLocalInitialization(val);
				ManagedWardInitializationCoordinator.EnsureNetworkInitialization(val, matchedByComponent, matchedByZdo);
			}
		}
	}
}
