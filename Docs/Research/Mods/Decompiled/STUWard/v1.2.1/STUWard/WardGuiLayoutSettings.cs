using UnityEngine;

namespace STUWard;

internal static class WardGuiLayoutSettings
{
	private const float SettingsSliderWidth = 520f;

	private const float WarningToggleLabelGap = 12f;

	private const float WarningToggleLabelWidth = 120f;

	private const float WarningToggleY = 57f;

	internal static Vector2 GetPanelOffset()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(0f, 0f);
	}

	internal static Vector2 GetPanelSize()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(1080f, 900f);
	}

	internal static Vector2 GetTitlePosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(0f, 382f);
	}

	internal static Vector2 GetOwnerPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(-100f, 400f);
	}

	internal static Vector2 GetGuildPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(-100f, 360f);
	}

	internal static Vector2 GetCloseButtonPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(440f, 400f);
	}

	internal static Vector2 GetPageArrowButtonPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(315f, 382f);
	}

	internal static Vector2 GetRadiusLabelPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(-360f, 300f);
	}

	internal static Vector2 GetRadiusSliderPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(20f, 300f);
	}

	internal static Vector2 GetRadiusValuePosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(360f, 300f);
	}

	internal static Vector2 GetAreaMarkerSpeedLabelPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(-360f, 240f);
	}

	internal static Vector2 GetAreaMarkerSpeedSliderPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(20f, 240f);
	}

	internal static Vector2 GetAreaMarkerSpeedValuePosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(360f, 240f);
	}

	internal static Vector2 GetAreaMarkerAlphaLabelPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(-360f, 180f);
	}

	internal static Vector2 GetAreaMarkerAlphaSliderPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(20f, 180f);
	}

	internal static Vector2 GetAreaMarkerAlphaValuePosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(360f, 180f);
	}

	internal static Vector2 GetAutoCloseDelayLabelPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(-360f, 120f);
	}

	internal static Vector2 GetAutoCloseDelaySliderPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(20f, 120f);
	}

	internal static Vector2 GetAutoCloseDelayValuePosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(360f, 120f);
	}

	internal static Vector2 GetWarningEffectsLabelPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(-360f, 60f);
	}

	internal static Vector2 GetWarningSoundLabelPosition(float toggleSize)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GetWarningToggleLabelPosition(GetSettingsSliderLeftEdge(), toggleSize);
	}

	internal static Vector2 GetWarningSoundTogglePosition(float toggleSize)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GetWarningTogglePosition(GetSettingsSliderLeftEdge(), toggleSize);
	}

	internal static Vector2 GetWarningFlashLabelPosition(float toggleSize)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GetWarningToggleLabelPosition(GetSettingsSliderCenterX(), toggleSize);
	}

	internal static Vector2 GetWarningFlashTogglePosition(float toggleSize)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GetWarningTogglePosition(GetSettingsSliderCenterX(), toggleSize);
	}

	internal static Vector2 GetRegisteredPlayersRemoveButtonPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(395f, 0f);
	}

	internal static Vector2 GetRegisteredPlayersHeaderPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(0f, 10f);
	}

	internal static Vector2 GetRegisteredPlayersHelpPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(0f, -128f);
	}

	internal static Vector2 GetPermittedListPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(-15f, -190f);
	}

	internal static Vector2 GetPermittedListSize()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(960f, 360f);
	}

	internal static Vector2 GetRestrictionsHeaderPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(0f, 285f);
	}

	internal static Vector2 GetRestrictionListPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(-15f, -75f);
	}

	internal static Vector2 GetRestrictionListSize()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(960f, 620f);
	}

	private static float GetSettingsSliderLeftEdge()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetRadiusSliderPosition().x - 260f;
	}

	private static float GetSettingsSliderCenterX()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetRadiusSliderPosition().x;
	}

	private static Vector2 GetWarningTogglePosition(float leftEdge, float toggleSize)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(leftEdge + toggleSize * 0.5f, 57f);
	}

	private static Vector2 GetWarningToggleLabelPosition(float leftEdge, float toggleSize)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(leftEdge + toggleSize + 12f + 60f, 57f);
	}
}
