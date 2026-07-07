using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "CheckAccess")]
internal static class PrivateAreaCheckAccessManagedPatch
{
	private static bool Prefix(Vector3 point, float radius, bool flash, bool wardCheck, ref bool __result)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null || !WardAccess.HasEnabledManagedWards())
		{
			return true;
		}
		float num = radius;
		GameObject placementGhost = localPlayer.m_placementGhost;
		PrivateArea area = (((Object)(object)placementGhost != (Object)null) ? placementGhost.GetComponent<PrivateArea>() : null);
		if ((Object)(object)placementGhost != (Object)null && StuWardArea.IsManaged(area) && Vector3.Distance(placementGhost.transform.position, point) <= 0.1f)
		{
			num = 8f;
		}
		IReadOnlyList<PrivateArea> candidateManagedWards = WardAccess.GetCandidateManagedWards(point, num, requireEnabled: true);
		if (WardAccess.IsManagedWardAllowScopeActive && WardAccess.IsInsideAnyManagedWard(point, num, candidateManagedWards))
		{
			__result = true;
			return false;
		}
		WardRestrictionOptions restriction;
		bool flag = WardAccess.TryGetRestrictionScope(out restriction);
		WardAccess.AccessResult accessResult = (flag ? WardAccess.EvaluateRestrictionAccessAgainstCandidates(restriction, point, num, localPlayer.GetPlayerID(), candidateManagedWards, flash, wardCheck) : WardAccess.EvaluateAccessAgainstCandidates(point, num, localPlayer.GetPlayerID(), candidateManagedWards, flash, wardCheck));
		if (accessResult.Decision == WardAccess.AccessDecision.NoWard)
		{
			if (flag && WardAccess.IsInsideAnyManagedWard(point, num, candidateManagedWards))
			{
				__result = true;
				return false;
			}
			return true;
		}
		if (Plugin.ShouldLogWardDiagnosticVerbose())
		{
			Plugin.LogWardDiagnosticVerbose("Access.PrivateArea", string.Format("Evaluated PrivateArea.CheckAccess. playerId={0}, decision={1}, restriction={2}, flash={3}, wardCheck={4}, radius={5}, effectiveRadius={6}, point={7}", localPlayer.GetPlayerID(), accessResult.Decision, flag ? restriction.ToString() : "None", flash, wardCheck, radius, num, point));
		}
		__result = !accessResult.IsDenied;
		return false;
	}
}
