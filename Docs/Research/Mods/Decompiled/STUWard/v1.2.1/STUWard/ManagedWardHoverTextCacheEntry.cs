namespace STUWard;

internal readonly struct ManagedWardHoverTextCacheEntry
{
	internal uint DataRevision { get; }

	internal string OriginalText { get; }

	internal long PlayerId { get; }

	internal bool CanConfigure { get; }

	internal bool CanAttemptAnyWardControl { get; }

	internal bool HasSettingsShortcutBinding { get; }

	internal string ShortcutLabel { get; }

	internal string GuildName { get; }

	internal string FinalText { get; }

	internal ManagedWardHoverTextCacheEntry(uint dataRevision, string originalText, long playerId, bool canConfigure, bool canAttemptAnyWardControl, bool hasSettingsShortcutBinding, string shortcutLabel, string guildName, string finalText)
	{
		DataRevision = dataRevision;
		OriginalText = originalText;
		PlayerId = playerId;
		CanConfigure = canConfigure;
		CanAttemptAnyWardControl = canAttemptAnyWardControl;
		HasSettingsShortcutBinding = hasSettingsShortcutBinding;
		ShortcutLabel = shortcutLabel;
		GuildName = guildName;
		FinalText = finalText;
	}
}
