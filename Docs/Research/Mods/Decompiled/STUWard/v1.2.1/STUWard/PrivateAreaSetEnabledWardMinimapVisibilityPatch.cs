using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "SetEnabled")]
internal static class PrivateAreaSetEnabledWardMinimapVisibilityPatch
{
	private static void Prefix(PrivateArea __instance)
	{
		ManagedWardRuntimeContexts.GetOrCreate(__instance).SetEnabledInvocationDepth++;
	}

	private static void Postfix(PrivateArea __instance)
	{
		if (ManagedWardRuntimeContexts.TryGet(__instance, out ManagedWardRuntimeContext context) && context.SetEnabledInvocationDepth > 0)
		{
			context.SetEnabledInvocationDepth--;
		}
		if (ManagedWardInteractionRpc.IsManagedWardForHooks(__instance))
		{
			ManagedWardRef ward = ManagedWardRef.FromArea(__instance);
			ManagedWardMapStateService.NotifyLiveWardMutation(__instance, ManagedWardMapMutationKind.IndexAndPins, "ward set enabled state changed");
			WardOwnership.ForceSyncManagedWardZdoToServer(ward, "ToggleEnabled.Sync");
		}
	}
}
