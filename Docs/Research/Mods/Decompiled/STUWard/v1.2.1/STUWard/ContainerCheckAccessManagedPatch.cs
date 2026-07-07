using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(Container), "CheckAccess")]
internal static class ContainerCheckAccessManagedPatch
{
	private static bool Prefix(Container __instance, long playerID, ref bool __result)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)__instance == (Object)null || playerID == 0L || !WardAccess.HasEnabledManagedWards())
		{
			return true;
		}
		IReadOnlyList<PrivateArea> candidateManagedWards = WardAccess.GetCandidateManagedWards(((Component)__instance).transform.position, 0f, requireEnabled: true);
		WardAccess.AccessResult accessResult = WardAccess.EvaluateRestrictionAccessAgainstCandidates(WardRestrictionOptions.Containers, ((Component)__instance).transform.position, 0f, playerID, candidateManagedWards, flash: false);
		if (accessResult.Decision == WardAccess.AccessDecision.NoWard)
		{
			if (WardAccess.IsInsideAnyManagedWard(((Component)__instance).transform.position, 0f, candidateManagedWards))
			{
				__result = true;
				return false;
			}
			return true;
		}
		if (Plugin.ShouldLogWardDiagnosticVerbose())
		{
			Plugin.LogWardDiagnosticVerbose("Access.Container", $"Evaluated Container.CheckAccess. playerId={playerID}, decision={accessResult.Decision}, position={((Component)__instance).transform.position}");
		}
		__result = !accessResult.IsDenied;
		return false;
	}
}
