using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "Awake")]
internal static class PrivateAreaAwakePatch
{
	private static void Postfix(PrivateArea __instance)
	{
		ManagedWardRef ward = ManagedWardRef.FromArea(__instance);
		ZDO zdo = ward.Zdo;
		bool matchedByComponent = ward.HasManagedComponent;
		bool matchedByZdo = ward.IsManagedZdo;
		if (!ShouldSkipPlacementGhostAwake(__instance, matchedByComponent, matchedByZdo, zdo) && (matchedByComponent || matchedByZdo))
		{
			ManagedWardIdentity.TryResolve(ward, repairComponent: true, out matchedByComponent, out matchedByZdo);
			Plugin.LogWardDiagnosticVerbose("Placement.Awake", $"PrivateArea.Awake postfix hit. matchedByComponent={matchedByComponent}, matchedByZdo={matchedByZdo}, {WardDiagnosticInfo.DescribeWard(__instance)}");
			ManagedWardInitializationCoordinator.EnsureLocalInitialization(__instance);
		}
	}

	private static bool ShouldSkipPlacementGhostAwake(PrivateArea area, bool matchedByComponent, bool matchedByZdo, ZDO? zdo)
	{
		if (Player.IsPlacementGhost(((Component)area).gameObject))
		{
			return true;
		}
		if (matchedByComponent && !matchedByZdo && zdo == null && ZNetView.m_forceDisableInit)
		{
			return true;
		}
		return false;
	}
}
