namespace STUWard;

internal readonly struct CachedWardConfiguration
{
	internal uint DataRevision { get; }

	internal float MaxRadius { get; }

	internal WardRestrictionOptions ForcedRestrictions { get; }

	internal WardConfiguration Configuration { get; }

	internal CachedWardConfiguration(uint dataRevision, float maxRadius, WardRestrictionOptions forcedRestrictions, WardConfiguration configuration)
	{
		DataRevision = dataRevision;
		MaxRadius = maxRadius;
		ForcedRestrictions = forcedRestrictions;
		Configuration = configuration;
	}
}
