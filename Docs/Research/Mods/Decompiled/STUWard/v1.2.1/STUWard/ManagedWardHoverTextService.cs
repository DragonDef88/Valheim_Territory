using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace STUWard;

internal static class ManagedWardHoverTextService
{
	private readonly struct HoverTextLine
	{
		internal int Start { get; }

		internal int Length { get; }

		internal string? Text { get; }

		internal bool IsInserted => Text != null;

		internal HoverTextLine(int start, int length)
		{
			Start = start;
			Length = length;
			Text = null;
		}

		internal HoverTextLine(string text)
		{
			Start = 0;
			Length = 0;
			Text = text;
		}
	}

	internal static bool TryRewriteHoverText(PrivateArea? area, string originalText, out string rewrittenText)
	{
		rewrittenText = originalText;
		if ((Object)(object)area == (Object)null || !WardAccess.IsManagedWard(area, requireEnabled: false) || string.IsNullOrEmpty(originalText))
		{
			return false;
		}
		Player localPlayer = Player.m_localPlayer;
		ZDO? zdo = WardPrivateAreaSafeAccess.GetZdo(area);
		uint dataRevision = ((zdo != null) ? zdo.DataRevision : 0u);
		long playerId = (((Object)(object)localPlayer != (Object)null) ? localPlayer.GetPlayerID() : 0);
		string text = GuildsCompat.GetWardGuildName(area) ?? string.Empty;
		string text2 = (string.IsNullOrWhiteSpace(text) ? null : WardLocalization.LocalizeFormat("$stuw_ui_guild", "Guild: {0}", text));
		bool flag = WardAccess.CanConfigureWard(area, localPlayer);
		bool flag2 = WardAdminDebugAccess.CanLocallyAttemptAnyWardControl(area, localPlayer);
		string hoverActionLine = ManagedWardInteractionRpc.GetHoverActionLine(area, localPlayer);
		string text3 = null;
		bool flag3 = Plugin.HasWardSettingsShortcutBinding();
		string text4 = (flag3 ? Plugin.GetWardSettingsShortcutLabel() : string.Empty);
		if ((Object)(object)localPlayer != (Object)null && flag3 && (flag || flag2))
		{
			text3 = WardLocalization.LocalizeFormat("$stuw_hover_settings", "[<color=yellow><b>{0}</b></color>] Ward settings", text4);
		}
		if (text2 == null && hoverActionLine == null && text3 == null)
		{
			ManagedWardHoverTextCache.Forget(area);
			return false;
		}
		if (ManagedWardHoverTextCache.TryGet(area, dataRevision, originalText, playerId, flag, flag2, flag3, text4, text, out string cachedHoverText))
		{
			rewrittenText = cachedHoverText;
			return true;
		}
		int actionLineIndex;
		List<HoverTextLine> list = CollectHoverTextLines(originalText, out actionLineIndex);
		if (list.Count < 2)
		{
			return false;
		}
		if (text2 != null)
		{
			list.Insert(2, new HoverTextLine(text2));
		}
		if (hoverActionLine != null)
		{
			if (actionLineIndex >= 0)
			{
				if (text2 != null && actionLineIndex >= 2)
				{
					actionLineIndex++;
				}
				list[actionLineIndex] = new HoverTextLine(hoverActionLine);
			}
			else
			{
				list.Insert(Mathf.Max(2, list.Count - 1), new HoverTextLine(hoverActionLine));
			}
		}
		if (text3 != null && list.Count >= 3)
		{
			list.Insert(list.Count - 1, new HoverTextLine(text3));
		}
		rewrittenText = BuildHoverText(originalText, list, text2, hoverActionLine, text3);
		ManagedWardHoverTextCache.Store(area, dataRevision, originalText, playerId, flag, flag2, flag3, text4, text, rewrittenText);
		return true;
	}

	private static List<HoverTextLine> CollectHoverTextLines(string text, out int actionLineIndex)
	{
		List<HoverTextLine> list = new List<HoverTextLine>(4);
		actionLineIndex = -1;
		int num = 0;
		while (num <= text.Length)
		{
			int num2 = text.IndexOf('\n', num);
			if (num2 < 0)
			{
				num2 = text.Length;
			}
			int num3 = num2 - num;
			if (actionLineIndex < 0 && num3 > 0 && text[num] == '[')
			{
				actionLineIndex = list.Count;
			}
			list.Add(new HoverTextLine(num, num3));
			if (num2 >= text.Length)
			{
				break;
			}
			num = num2 + 1;
		}
		return list;
	}

	private static string BuildHoverText(string originalText, List<HoverTextLine> lines, string? guildLine, string? toggleLine, string? settingsLine)
	{
		StringBuilder stringBuilder = new StringBuilder(originalText.Length + (guildLine?.Length ?? 0) + (toggleLine?.Length ?? 0) + (settingsLine?.Length ?? 0) + 4);
		for (int i = 0; i < lines.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append('\n');
			}
			HoverTextLine hoverTextLine = lines[i];
			if (hoverTextLine.IsInserted)
			{
				stringBuilder.Append(hoverTextLine.Text);
			}
			else
			{
				stringBuilder.Append(originalText, hoverTextLine.Start, hoverTextLine.Length);
			}
		}
		return stringBuilder.ToString();
	}
}
