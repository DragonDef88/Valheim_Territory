namespace STUWard;

internal readonly struct WardConfiguration
{
	internal bool ShowAreaMarker { get; }

	internal float AreaMarkerSpeedMultiplier { get; }

	internal float AreaMarkerAlpha { get; }

	internal float Radius { get; }

	internal float AutoCloseDelay { get; }

	internal bool WarningSoundEnabled { get; }

	internal bool WarningFlashEnabled { get; }

	internal WardRestrictionOptions Restrictions { get; }

	internal bool AutoCloseDoors => AutoCloseDelay > 0f;

	internal WardConfiguration(bool showAreaMarker, float areaMarkerSpeedMultiplier, float areaMarkerAlpha, float radius, float autoCloseDelay, bool warningSoundEnabled, bool warningFlashEnabled, WardRestrictionOptions restrictions = WardRestrictionOptions.All)
	{
		ShowAreaMarker = showAreaMarker;
		AreaMarkerSpeedMultiplier = areaMarkerSpeedMultiplier;
		AreaMarkerAlpha = areaMarkerAlpha;
		Radius = radius;
		AutoCloseDelay = autoCloseDelay;
		WarningSoundEnabled = warningSoundEnabled;
		WarningFlashEnabled = warningFlashEnabled;
		Restrictions = restrictions;
	}
}
