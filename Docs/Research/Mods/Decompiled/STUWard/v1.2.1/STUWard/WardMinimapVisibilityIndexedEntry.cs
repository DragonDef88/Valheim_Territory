using System;
using UnityEngine;

namespace STUWard;

internal readonly struct WardMinimapVisibilityIndexedEntry
{
	internal ZDOID ZdoId { get; }

	internal long OwnerPlayerId { get; }

	internal int WardGuildId { get; }

	internal Vector3 Position { get; }

	internal float Radius { get; }

	internal bool IsEnabled { get; }

	internal long[] PermittedPlayerIds { get; }

	internal WardMinimapVisibilityIndexedEntry(ZDOID zdoId, long ownerPlayerId, int wardGuildId, Vector3 position, float radius, bool isEnabled, long[] permittedPlayerIds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		ZdoId = zdoId;
		OwnerPlayerId = ownerPlayerId;
		WardGuildId = wardGuildId;
		Position = position;
		Radius = radius;
		IsEnabled = isEnabled;
		PermittedPlayerIds = permittedPlayerIds ?? Array.Empty<long>();
	}
}
