using UnityEngine;

namespace STUWard;

internal static class ManagedWardAccessEvaluator
{
	internal static bool HasPlayerAccess(PrivateArea area, ManagedWardAccessActor actor, bool includeDiagnosticData, bool logDiagnostic = true)
	{
		ManagedWardAccessSubject subject = BuildManagedWardAccessSubjectFromArea(area, actor, includeDiagnosticData);
		ManagedWardAccessEvaluation evaluation = ManagedWardAccessPolicy.Evaluate(actor, subject);
		if (logDiagnostic)
		{
			LogResolutionVerbose(actor, subject, evaluation);
		}
		return evaluation.Allowed;
	}

	internal static bool TryCreateActorForAccessCheck(long playerId, out ManagedWardAccessActor actor)
	{
		if (playerId != 0L)
		{
			actor = CreateActor(playerId);
			return true;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			actor = default(ManagedWardAccessActor);
			return false;
		}
		actor = CreateActor(localPlayer.GetPlayerID(), GuildsCompat.GetPlayerGuildIdentity(localPlayer));
		return true;
	}

	internal static ManagedWardAccessActor CreateActor(long playerId)
	{
		return CreateActor(playerId, GuildsCompat.GetPlayerGuildIdentity(playerId));
	}

	internal static ManagedWardAccessActor CreateActor(long playerId, WardGuildIdentity playerGuild)
	{
		return new ManagedWardAccessActor(playerId, playerGuild, WardAdminDebugAccess.IsPlayerAdminDebugController(playerId));
	}

	internal static bool HasPlayerAccessToManagedWardZdo(ZDO? zdo, long playerId)
	{
		return HasPlayerAccessToManagedWardZdo(zdo, playerId, GuildsCompat.GetPlayerGuildIdentity(playerId));
	}

	internal static bool HasPlayerAccessToManagedWardZdo(ZDO? zdo, long playerId, WardGuildIdentity playerGuild)
	{
		if (zdo == null || !zdo.IsValid() || playerId == 0L)
		{
			return false;
		}
		ManagedWardAccessActor actor = CreateActor(playerId, playerGuild);
		ManagedWardAccessSubject subject = BuildManagedWardAccessSubjectFromZdo(zdo, actor, Plugin.ShouldLogWardDiagnosticVerbose());
		ManagedWardAccessEvaluation evaluation = ManagedWardAccessPolicy.Evaluate(actor, subject);
		LogResolutionVerbose(actor, subject, evaluation);
		return evaluation.Allowed;
	}

	internal static bool HasPlayerAccessToManagedWardIndexEntry(WardMinimapVisibilityIndexedEntry entry, long playerId, WardGuildIdentity playerGuild)
	{
		if (playerId == 0L)
		{
			return false;
		}
		ManagedWardAccessActor actor = CreateActor(playerId, playerGuild);
		ManagedWardAccessSubject subject = BuildManagedWardAccessSubjectFromIndexEntry(entry, actor, Plugin.ShouldLogWardDiagnosticVerbose());
		ManagedWardAccessEvaluation evaluation = ManagedWardAccessPolicy.Evaluate(actor, subject);
		LogResolutionVerbose(actor, subject, evaluation);
		return evaluation.Allowed;
	}

	internal static bool HasMatchingGuild(WardGuildIdentity playerGuild, WardGuildIdentity wardGuild)
	{
		return ManagedWardAccessPolicy.HasMatchingGuild(playerGuild, wardGuild);
	}

	private static void LogResolutionVerbose(ManagedWardAccessActor actor, ManagedWardAccessSubject subject, ManagedWardAccessEvaluation evaluation)
	{
		if (Plugin.ShouldLogWardDiagnosticVerbose())
		{
			string playerName = WardOwnership.GetPlayerName(actor.PlayerId);
			string playerAccountId = WardOwnership.GetPlayerAccountId(actor.PlayerId);
			Plugin.LogWardDiagnosticVerbose("Access.Resolve", $"Resolved managed ward access. allowed={evaluation.Allowed}, reason={evaluation.Reason}, playerId={actor.PlayerId}, playerName='{playerName}', accountId='{playerAccountId}', playerGuildId={actor.PlayerGuild.Id}, playerGuildName='{actor.PlayerGuild.Name ?? string.Empty}', permitted={evaluation.Permitted}, sameGuild={evaluation.SameGuild}, wardGuildId={subject.WardGuild.Id}, wardGuildName='{subject.WardGuild.Name ?? string.Empty}', wardOwnerPlayerId={subject.OwnerPlayerId}, wardSteamAccountId='{subject.WardSteamAccountId}', wardZdo={subject.WardZdoLabel}");
		}
	}

	private static ManagedWardAccessSubject BuildManagedWardAccessSubjectFromArea(PrivateArea area, ManagedWardAccessActor actor, bool includeDiagnosticData)
	{
		return BuildManagedWardAccessSubjectCore(WardPrivateAreaSafeAccess.GetZdo(area), WardAccess.GetCanonicalCreatorPlayerId(area), GuildsCompat.GetWardGuildId(area), WardPrivateAreaSafeAccess.IsPlayerPermitted(area, actor.PlayerId), includeDiagnosticData);
	}

	private static ManagedWardAccessSubject BuildManagedWardAccessSubjectFromZdo(ZDO zdo, ManagedWardAccessActor actor, bool includeDiagnosticData)
	{
		return BuildManagedWardAccessSubjectCore(zdo, zdo.GetLong(ZDOVars.s_creator, 0L), GuildsCompat.GetWardGuildId(zdo), WardPrivateAreaSafeAccess.IsPlayerPermitted(zdo, actor.PlayerId), includeDiagnosticData);
	}

	private static ManagedWardAccessSubject BuildManagedWardAccessSubjectFromIndexEntry(WardMinimapVisibilityIndexedEntry entry, ManagedWardAccessActor actor, bool includeDiagnosticData)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		long ownerPlayerId = entry.OwnerPlayerId;
		WardGuildIdentity wardGuild = new WardGuildIdentity(entry.WardGuildId, string.Empty);
		bool permitted = IsPlayerPermitted(entry, actor.PlayerId);
		string empty = string.Empty;
		string? wardZdoLabel;
		if (!includeDiagnosticData)
		{
			wardZdoLabel = string.Empty;
		}
		else
		{
			ZDOID zdoId = entry.ZdoId;
			wardZdoLabel = ((object)(ZDOID)(ref zdoId)).ToString();
		}
		return new ManagedWardAccessSubject(ownerPlayerId, wardGuild, permitted, empty, wardZdoLabel);
	}

	private static ManagedWardAccessSubject BuildManagedWardAccessSubjectCore(ZDO? zdo, long ownerPlayerId, int wardGuildId, bool permitted, bool includeDiagnosticData)
	{
		return new ManagedWardAccessSubject(ownerPlayerId, new WardGuildIdentity(wardGuildId, (includeDiagnosticData && wardGuildId != 0) ? GuildsCompat.GetWardGuildName(zdo) : string.Empty), permitted, includeDiagnosticData ? WardOwnership.GetWardSteamAccountId(zdo) : string.Empty, (!includeDiagnosticData) ? string.Empty : (((zdo != null) ? ((object)(ZDOID)(ref zdo.m_uid)).ToString() : null) ?? "none"));
	}

	private static bool IsPlayerPermitted(WardMinimapVisibilityIndexedEntry entry, long playerId)
	{
		for (int i = 0; i < entry.PermittedPlayerIds.Length; i++)
		{
			if (entry.PermittedPlayerIds[i] == playerId)
			{
				return true;
			}
		}
		return false;
	}
}
