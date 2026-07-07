using System;

namespace STUWard;

internal static class ManagedWardHoverTextCache
{
	internal static void Store(PrivateArea area, uint dataRevision, string originalText, long playerId, bool canConfigure, bool canAttemptAnyWardControl, bool hasSettingsShortcutBinding, string shortcutLabel, string guildName, string finalText)
	{
		ManagedWardRuntimeContext orCreate = ManagedWardRuntimeContexts.GetOrCreate(area);
		orCreate.HoverText = new ManagedWardHoverTextCacheEntry(dataRevision, originalText, playerId, canConfigure, canAttemptAnyWardControl, hasSettingsShortcutBinding, shortcutLabel, guildName, finalText);
		orCreate.HasHoverText = true;
	}

	internal static void Forget(PrivateArea? area)
	{
		ManagedWardRuntimeContexts.ClearHoverText(area);
	}

	internal static void Reset()
	{
		ManagedWardRuntimeContexts.ClearHoverTexts();
	}

	internal static bool TryGet(PrivateArea area, uint dataRevision, string originalText, long playerId, bool canConfigure, bool canAttemptAnyWardControl, bool hasSettingsShortcutBinding, string shortcutLabel, string guildName, out string cachedHoverText)
	{
		cachedHoverText = string.Empty;
		if (!ManagedWardRuntimeContexts.TryGet(area, out ManagedWardRuntimeContext context) || !context.HasHoverText)
		{
			return false;
		}
		ManagedWardHoverTextCacheEntry hoverText = context.HoverText;
		if (hoverText.DataRevision != dataRevision || !string.Equals(hoverText.OriginalText, originalText, StringComparison.Ordinal) || hoverText.PlayerId != playerId || hoverText.CanConfigure != canConfigure || hoverText.CanAttemptAnyWardControl != canAttemptAnyWardControl || hoverText.HasSettingsShortcutBinding != hasSettingsShortcutBinding || !string.Equals(hoverText.ShortcutLabel, shortcutLabel, StringComparison.Ordinal) || !string.Equals(hoverText.GuildName, guildName, StringComparison.Ordinal))
		{
			return false;
		}
		cachedHoverText = hoverText.FinalText;
		return true;
	}
}
