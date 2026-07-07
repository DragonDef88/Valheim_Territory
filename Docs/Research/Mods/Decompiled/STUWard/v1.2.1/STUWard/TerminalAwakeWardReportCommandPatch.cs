using HarmonyLib;

namespace STUWard;

[HarmonyPatch(typeof(Terminal), "Awake")]
internal static class TerminalAwakeWardReportCommandPatch
{
	private static void Postfix(Terminal __instance)
	{
		ManagedWardReportService.EnsureConsoleCommandRegistered(__instance);
	}
}
