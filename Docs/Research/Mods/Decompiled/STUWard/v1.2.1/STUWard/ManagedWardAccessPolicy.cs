namespace STUWard;

internal static class ManagedWardAccessPolicy
{
	internal static ManagedWardAccessEvaluation Evaluate(ManagedWardAccessActor actor, ManagedWardAccessSubject subject)
	{
		if (subject.OwnerPlayerId != 0L && subject.OwnerPlayerId == actor.PlayerId)
		{
			return new ManagedWardAccessEvaluation(allowed: true, "owner", subject.Permitted, sameGuild: false);
		}
		if (actor.IsAdminDebug)
		{
			return new ManagedWardAccessEvaluation(allowed: true, "admin_debug", subject.Permitted, sameGuild: false);
		}
		if (HasMatchingGuild(actor.PlayerGuild, subject.WardGuild))
		{
			return new ManagedWardAccessEvaluation(allowed: true, "guild", subject.Permitted, sameGuild: true);
		}
		if (subject.Permitted)
		{
			return new ManagedWardAccessEvaluation(allowed: true, "permitted", permitted: true, sameGuild: false);
		}
		return new ManagedWardAccessEvaluation(allowed: false, "denied", subject.Permitted, sameGuild: false);
	}

	internal static bool HasMatchingGuild(WardGuildIdentity playerGuild, WardGuildIdentity wardGuild)
	{
		if (playerGuild.Id != 0 && wardGuild.Id != 0)
		{
			return playerGuild.Id == wardGuild.Id;
		}
		return false;
	}
}
