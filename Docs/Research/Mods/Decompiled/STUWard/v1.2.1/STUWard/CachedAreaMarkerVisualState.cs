namespace STUWard;

internal readonly struct CachedAreaMarkerVisualState
{
	internal int MarkerInstanceId { get; }

	internal int SegmentCount { get; }

	internal int FirstSegmentInstanceId { get; }

	internal int LastSegmentInstanceId { get; }

	internal float MaxRadius { get; }

	internal float Radius { get; }

	internal float AreaMarkerAlpha { get; }

	internal CachedAreaMarkerVisualState(int markerInstanceId, int segmentCount, int firstSegmentInstanceId, int lastSegmentInstanceId, float maxRadius, float radius, float areaMarkerAlpha)
	{
		MarkerInstanceId = markerInstanceId;
		SegmentCount = segmentCount;
		FirstSegmentInstanceId = firstSegmentInstanceId;
		LastSegmentInstanceId = lastSegmentInstanceId;
		MaxRadius = maxRadius;
		Radius = radius;
		AreaMarkerAlpha = areaMarkerAlpha;
	}
}
