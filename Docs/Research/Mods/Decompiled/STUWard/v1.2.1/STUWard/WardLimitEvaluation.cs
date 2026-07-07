namespace STUWard;

internal readonly struct WardLimitEvaluation
{
	internal bool Allowed { get; }

	internal string Reason { get; }

	internal int Limit { get; }

	internal int CurrentCount { get; }

	internal bool IsUnlimited => Limit < 0;

	internal WardLimitEvaluation(bool allowed, string reason, int limit, int currentCount)
	{
		Allowed = allowed;
		Reason = reason;
		Limit = limit;
		CurrentCount = currentCount;
	}
}
