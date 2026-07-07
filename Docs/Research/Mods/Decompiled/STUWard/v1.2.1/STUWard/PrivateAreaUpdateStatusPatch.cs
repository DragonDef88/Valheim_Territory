using HarmonyLib;
using UnityEngine;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "UpdateStatus")]
internal static class PrivateAreaUpdateStatusPatch
{
	private static void Postfix(PrivateArea __instance)
	{
		ManagedWardRef ward = ManagedWardRef.FromArea(__instance);
		if (ward.IsPlacementGhost || !WardAccess.IsManagedWard(ward, requireEnabled: false))
		{
			WardAccess.UnregisterManagedWard(ward);
			WardRuntimeStateTracker.Forget(__instance);
			return;
		}
		ManagedWardRuntimeContext context;
		bool flag = ManagedWardRuntimeContexts.TryGet(__instance, out context) && context.HasObservedState;
		bool flag2 = context != null && context.SetEnabledInvocationDepth > 0;
		if (!WardRuntimeStateTracker.TryConsumeChanges(__instance, out var enabledChanged, out var dataRevisionChanged))
		{
			return;
		}
		bool flag3 = false;
		bool flag4 = false;
		if (ManagedWardRuntimeContexts.TryGet(__instance, out ManagedWardRuntimeContext context2))
		{
			flag3 = enabledChanged && ManagedWardRuntimeContexts.TryConsumeEnabledFanOutSuppression(__instance, context2.ObservedState.Enabled);
			flag4 = dataRevisionChanged && ManagedWardRuntimeContexts.TryConsumeDataRevisionFanOutSuppression(__instance, context2.ObservedState.DataRevision);
		}
		if (enabledChanged)
		{
			ManagedWardInteractionRpc.NotifyLocalEnabledStateObserved(__instance);
			WardAccess.RefreshManagedWardState(ward);
			if (flag && !flag2)
			{
				ReplaySynchronizedToggleEffects(__instance);
			}
		}
		if (dataRevisionChanged)
		{
			ManagedWardInteractionRpc.NotifyLocalPermittedStateObserved(__instance);
			WardSettings.ApplyAreaState(ward);
		}
		bool flag5 = enabledChanged && !flag3;
		bool flag6 = dataRevisionChanged && !flag4;
		if (flag5 || flag6)
		{
			ManagedWardMapStateService.NotifyLiveWardMutation(__instance, ManagedWardMapMutationKind.IndexAndPins, (flag5 && flag6) ? "ward update status changed enabled state and data revision" : (flag5 ? "ward update status changed enabled state" : "ward update status changed data revision"));
		}
	}

	private static void ReplaySynchronizedToggleEffects(PrivateArea area)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Player.m_localPlayer == (Object)null))
		{
			EffectList val = (area.IsEnabled() ? area.m_activateEffect : area.m_deactivateEffect);
			if (val != null)
			{
				Transform transform = ((Component)area).transform;
				val.Create(transform.position, transform.rotation, (Transform)null, 1f, -1);
				Plugin.LogWardDiagnosticVerbose("ToggleEnabled.Effects", $"Replayed synchronized managed ward toggle effects after UpdateStatus. enabled={area.IsEnabled()}, {WardDiagnosticInfo.DescribeWard(area)}");
			}
		}
	}
}
