namespace STUWard;

internal static class GuildsSaveGuildPatch
{
	internal static void Postfix(object[] __args)
	{
		if (GuildsCompat.IsGuildHooksActive() && __args.Length != 0)
		{
			Plugin.LogWardDiagnosticVerbose("GuildsCompat.Event", "Observed SaveGuild postfix, guild=" + GuildsCompat.DescribeGuildObject(__args[0]) + ".");
			GuildsCompat.HandleGuildSaved(__args[0]);
		}
	}
}
