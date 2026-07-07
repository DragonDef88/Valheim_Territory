using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(Terminal), "TryRunCommand")]
internal static class TerminalTryRunCommandWardReportPatch
{
	private static bool Prefix(Terminal __instance, string text)
	{
		return !ManagedWardReportService.TryHandleConsoleCommand(__instance, text);
	}
}
