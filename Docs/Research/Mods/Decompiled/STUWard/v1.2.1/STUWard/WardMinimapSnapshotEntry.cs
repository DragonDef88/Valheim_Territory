using UnityEngine;

namespace STUWard;

internal readonly struct WardMinimapSnapshotEntry
{
	internal ZDOID ZdoId { get; }

	internal Vector3 Position { get; }

	internal float Radius { get; }

	internal bool IsEnabled { get; }

	internal WardMinimapSnapshotEntry(ZDOID zdoId, Vector3 position, float radius, bool isEnabled)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		ZdoId = zdoId;
		Position = position;
		Radius = radius;
		IsEnabled = isEnabled;
	}
}
