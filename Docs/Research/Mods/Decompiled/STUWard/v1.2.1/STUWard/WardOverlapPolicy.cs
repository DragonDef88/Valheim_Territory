using System;
using System.Collections.Generic;

namespace STUWard;

internal static class WardOverlapPolicy
{
	internal static bool WouldOverlapForeignWard(WardOverlapQuery query, IEnumerable<WardOverlapArea> areas)
	{
		foreach (WardOverlapArea area in areas)
		{
			if (!ShouldIgnoreArea(query, area) && !SharesTrustedWardGroup(area, query) && Overlaps(query, area))
			{
				return true;
			}
		}
		return false;
	}

	internal static float GetMaxNonOverlappingRadius(float fallbackRadius, WardOverlapQuery query, IEnumerable<WardOverlapArea> areas)
	{
		float num = fallbackRadius;
		foreach (WardOverlapArea area in areas)
		{
			if (!ShouldIgnoreArea(query, area) && !SharesTrustedWardGroup(area, query))
			{
				float num2 = DistanceXZ(query.X, query.Z, area.X, area.Z) - area.Radius;
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return Clamp(num, 0f, fallbackRadius);
	}

	internal static bool Overlaps(WardOverlapQuery query, WardOverlapArea area)
	{
		return DistanceXZ(query.X, query.Z, area.X, area.Z) < area.Radius + query.Radius;
	}

	internal static bool SharesTrustedWardGroup(WardOverlapArea area, WardOverlapQuery query)
	{
		if (SharesDirectOwnerGroup(area.OwnerPlayerId, query.OwnerPlayerId))
		{
			return true;
		}
		if (area.GuildId != 0 && query.GuildId != 0)
		{
			return area.GuildId == query.GuildId;
		}
		return false;
	}

	private static bool ShouldIgnoreArea(WardOverlapQuery query, WardOverlapArea area)
	{
		if (query.IgnoredAreaId != 0)
		{
			return area.Id == query.IgnoredAreaId;
		}
		return false;
	}

	private static bool SharesDirectOwnerGroup(long leftCreatorPlayerId, long rightCreatorPlayerId)
	{
		if (leftCreatorPlayerId != 0L && rightCreatorPlayerId != 0L)
		{
			return leftCreatorPlayerId == rightCreatorPlayerId;
		}
		return false;
	}

	private static float DistanceXZ(float leftX, float leftZ, float rightX, float rightZ)
	{
		float num = leftX - rightX;
		float num2 = leftZ - rightZ;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	private static float Clamp(float value, float min, float max)
	{
		if (value < min)
		{
			return min;
		}
		if (!(value > max))
		{
			return value;
		}
		return max;
	}
}
