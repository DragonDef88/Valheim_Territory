using System.Collections.Generic;

namespace STUWard;

internal static class WardLimitPolicy
{
	internal static int GetEffectiveLimit(string accountId, IReadOnlyDictionary<string, int> overrides, int defaultLimit)
	{
		if (!string.IsNullOrWhiteSpace(accountId) && overrides != null && overrides.TryGetValue(accountId, out var value))
		{
			return value;
		}
		return defaultLimit;
	}

	internal static WardLimitEvaluation EvaluatePlacement(int limit, int currentCount)
	{
		if (limit < 0)
		{
			return new WardLimitEvaluation(allowed: true, "unlimited", limit, currentCount);
		}
		if (currentCount >= limit)
		{
			return new WardLimitEvaluation(allowed: false, "limit_reached", limit, currentCount);
		}
		return new WardLimitEvaluation(allowed: true, "under_limit", limit, currentCount);
	}
}
