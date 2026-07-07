using UnityEngine;

namespace STUWard;

internal static class ManagedWardPlacementPreviewService
{
	private sealed class PlacementPreviewOverlapState
	{
		internal int CandidateInstanceId;

		internal Vector3 Point;

		internal long PlayerId;

		internal int GuildId;

		internal int SpatialRevision = -1;

		internal bool BlocksPlacement;

		internal bool HasValue;
	}

	private static readonly PlacementPreviewOverlapState OverlapCache = new PlacementPreviewOverlapState();

	internal static bool ShouldShowAsInvalid(Player? player, Component? candidate, Vector3 point)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (!WardAccess.IsManagedWardPlacementCandidate(candidate) || (Object)(object)candidate == (Object)null)
		{
			return false;
		}
		long num = (((Object)(object)player != (Object)null) ? player.GetPlayerID() : 0);
		int num2 = (((Object)(object)player != (Object)null) ? GuildsCompat.GetPlayerGuildId(player) : 0);
		int instanceID = ((Object)candidate).GetInstanceID();
		int managedWardSpatialIndexRevision = WardAccess.GetManagedWardSpatialIndexRevision();
		if (OverlapCache.HasValue && OverlapCache.CandidateInstanceId == instanceID && OverlapCache.PlayerId == num && OverlapCache.GuildId == num2 && OverlapCache.SpatialRevision == managedWardSpatialIndexRevision && PointsMatch(OverlapCache.Point, point))
		{
			return OverlapCache.BlocksPlacement;
		}
		bool flag = WardAccess.WouldBlockManagedWardPlacement(player, candidate, point, flash: false);
		OverlapCache.CandidateInstanceId = instanceID;
		OverlapCache.Point = point;
		OverlapCache.PlayerId = num;
		OverlapCache.GuildId = num2;
		OverlapCache.SpatialRevision = managedWardSpatialIndexRevision;
		OverlapCache.BlocksPlacement = flag;
		OverlapCache.HasValue = true;
		return flag;
	}

	internal static void Invalidate()
	{
		OverlapCache.HasValue = false;
	}

	private static bool PointsMatch(Vector3 left, Vector3 right)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Approximately(left.x, right.x))
		{
			return Mathf.Approximately(left.z, right.z);
		}
		return false;
	}
}
