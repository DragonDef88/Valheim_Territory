namespace STUWard;

internal readonly struct WardRestrictionDefinition
{
	internal WardRestrictionOptions Restriction { get; }

	internal string ConfigName { get; }

	internal string ConfigDescription { get; }

	internal string LocalizationToken { get; }

	internal string LocalizationFallback { get; }

	internal WardRestrictionDefinition(WardRestrictionOptions restriction, string configName, string configDescription, string localizationToken, string localizationFallback)
	{
		Restriction = restriction;
		ConfigName = configName;
		ConfigDescription = configDescription;
		LocalizationToken = localizationToken;
		LocalizationFallback = localizationFallback;
	}
}
