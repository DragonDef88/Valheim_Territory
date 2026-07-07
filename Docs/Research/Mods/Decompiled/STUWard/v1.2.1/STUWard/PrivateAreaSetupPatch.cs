using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(PrivateArea), "Setup")]
internal static class PrivateAreaSetupPatch
{
	private static void Postfix(PrivateArea __instance)
	{
		if (ManagedWardIdentity.TryResolve(ManagedWardRef.FromArea(__instance), repairComponent: true, out var matchedByComponent, out var matchedByZdo))
		{
			Plugin.LogWardDiagnosticVerbose("Placement.Setup", $"PrivateArea.Setup postfix hit. matchedByComponent={matchedByComponent}, matchedByZdo={matchedByZdo}, {WardDiagnosticInfo.DescribeWard(__instance)}");
			ManagedWardInitializationCoordinator.EnsureNetworkInitialization(__instance, matchedByComponent, matchedByZdo);
		}
	}
}
