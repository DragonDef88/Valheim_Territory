namespace STUWard;

internal readonly struct ManagedWardAccessEvaluation
{
	internal bool Allowed { get; }

	internal string Reason { get; }

	internal bool Permitted { get; }

	internal bool SameGuild { get; }

	internal ManagedWardAccessEvaluation(bool allowed, string reason, bool permitted, bool sameGuild)
	{
		Allowed = allowed;
		Reason = reason;
		Permitted = permitted;
		SameGuild = sameGuild;
	}
}
