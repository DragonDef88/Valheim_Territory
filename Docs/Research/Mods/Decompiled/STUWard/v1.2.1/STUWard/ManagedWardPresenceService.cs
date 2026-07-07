using System.Collections.Generic;
using UnityEngine;

namespace STUWard;

internal static class ManagedWardPresenceService
{
	private const float TrustedPresenceSweepActiveDurationSeconds = 15f;

	private const float TrustedPlayerRangeBuffer = 8f;

	private const float TrustedPresenceGraceSeconds = 10f;

	private const float PresenceRefreshIntervalSeconds = 1f;

	private static readonly List<PrivateArea> TrustedPresenceCandidateBuffer = new List<PrivateArea>();

	private static readonly List<PrivateArea> TrustedPresenceCoverageBuffer = new List<PrivateArea>();

	private static bool _trustedPresenceSweepWasActive;

	private static float _trustedPresenceSweepActiveUntilTime = float.NegativeInfinity;

	private static float _nextTrustedPresenceSweepTime = float.NegativeInfinity;

	internal static void Invalidate()
	{
		ManagedWardRuntimeContexts.ResetPresenceStates();
		_trustedPresenceSweepActiveUntilTime = float.NegativeInfinity;
		_nextTrustedPresenceSweepTime = float.NegativeInfinity;
		_trustedPresenceSweepWasActive = false;
	}

	internal static void ResetRuntimeState()
	{
		ManagedWardRuntimeContexts.ResetPresenceStates();
		TrustedPresenceCandidateBuffer.Clear();
		TrustedPresenceCoverageBuffer.Clear();
		_trustedPresenceSweepWasActive = false;
		_trustedPresenceSweepActiveUntilTime = float.NegativeInfinity;
		_nextTrustedPresenceSweepTime = float.NegativeInfinity;
	}

	internal static void Update()
	{
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return;
		}
		if (GetHostileCreatureStructureProtectionMode() != Plugin.HostileCreatureStructureProtectionMode.UnattendedOnly || !WardAccess.HasEnabledManagedWards())
		{
			if (_trustedPresenceSweepWasActive)
			{
				ManagedWardRuntimeContexts.ResetPresenceStates();
				_trustedPresenceSweepWasActive = false;
				_trustedPresenceSweepActiveUntilTime = float.NegativeInfinity;
				_nextTrustedPresenceSweepTime = float.NegativeInfinity;
			}
			return;
		}
		float time = Time.time;
		if (!_trustedPresenceSweepWasActive || time > _trustedPresenceSweepActiveUntilTime)
		{
			_trustedPresenceSweepWasActive = false;
			_trustedPresenceSweepActiveUntilTime = float.NegativeInfinity;
			_nextTrustedPresenceSweepTime = float.NegativeInfinity;
		}
		else if (!(time < _nextTrustedPresenceSweepTime))
		{
			SweepTrustedPlayerPresence(time);
			_nextTrustedPresenceSweepTime = time + 1f;
		}
	}

	internal static bool ShouldBlockHostileCreatureDamageToBuilding(Vector3 point)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return GetHostileCreatureStructureProtectionMode() switch
		{
			Plugin.HostileCreatureStructureProtectionMode.Off => false, 
			Plugin.HostileCreatureStructureProtectionMode.Always => IsInsideEnabledWard(point), 
			_ => ShouldBlockUnattendedHostileCreatureDamageToBuilding(point), 
		};
	}

	private static bool ShouldBlockUnattendedHostileCreatureDamageToBuilding(Vector3 point)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		WardAccess.FillCandidateManagedWards(point, 0f, requireEnabled: true, TrustedPresenceCandidateBuffer);
		if (TrustedPresenceCandidateBuffer.Count == 0)
		{
			return false;
		}
		TrustedPresenceCoverageBuffer.Clear();
		for (int i = 0; i < TrustedPresenceCandidateBuffer.Count; i++)
		{
			PrivateArea val = TrustedPresenceCandidateBuffer[i];
			if (!((Object)(object)val == (Object)null) && val.IsInside(point, 0f))
			{
				TrustedPresenceCoverageBuffer.Add(val);
			}
		}
		if (TrustedPresenceCoverageBuffer.Count == 0)
		{
			return false;
		}
		EnsureTrustedPlayerPresenceSweepActivity(time, TrustedPresenceCoverageBuffer, 10f);
		for (int j = 0; j < TrustedPresenceCoverageBuffer.Count; j++)
		{
			if (!IsWardConsideredAttended(TrustedPresenceCoverageBuffer[j], time, 10f))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsInsideEnabledWard(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		WardAccess.FillCandidateManagedWards(point, 0f, requireEnabled: true, TrustedPresenceCandidateBuffer);
		if (TrustedPresenceCandidateBuffer.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < TrustedPresenceCandidateBuffer.Count; i++)
		{
			PrivateArea val = TrustedPresenceCandidateBuffer[i];
			if ((Object)(object)val != (Object)null && val.IsInside(point, 0f))
			{
				return true;
			}
		}
		return false;
	}

	private static Plugin.HostileCreatureStructureProtectionMode GetHostileCreatureStructureProtectionMode()
	{
		if (Plugin.HostileCreatureStructureProtection == null)
		{
			return Plugin.HostileCreatureStructureProtectionMode.UnattendedOnly;
		}
		return Plugin.HostileCreatureStructureProtection.Value;
	}

	private static void EnsureTrustedPlayerPresenceSweepActivity(float now, IReadOnlyList<PrivateArea> coveringAreas, float graceSeconds)
	{
		if (!_trustedPresenceSweepWasActive || !(now <= _trustedPresenceSweepActiveUntilTime) || HasUnattendedPresenceState(coveringAreas, now, graceSeconds))
		{
			RefreshTrustedPlayerPresenceForAreas(now, coveringAreas);
			_nextTrustedPresenceSweepTime = now + 1f;
		}
		_trustedPresenceSweepWasActive = true;
		_trustedPresenceSweepActiveUntilTime = now + 15f;
	}

	private static void SweepTrustedPlayerPresence(float now)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		List<Player> allPlayers = Player.GetAllPlayers();
		if (allPlayers == null || allPlayers.Count == 0)
		{
			return;
		}
		float radius = Mathf.Max(0f, WardSettings.MaxRadius + 8f);
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player val = allPlayers[i];
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			long playerID = val.GetPlayerID();
			if (playerID == 0L)
			{
				continue;
			}
			Vector3 position = ((Component)val).transform.position;
			ManagedWardAccessActor actor = ManagedWardAccessEvaluator.CreateActor(playerID);
			WardAccess.FillCandidateManagedWards(position, radius, requireEnabled: true, TrustedPresenceCandidateBuffer);
			for (int j = 0; j < TrustedPresenceCandidateBuffer.Count; j++)
			{
				PrivateArea val2 = TrustedPresenceCandidateBuffer[j];
				if (!((Object)(object)val2 == (Object)null) && val2.IsInside(position, 8f) && ManagedWardAccessEvaluator.HasPlayerAccess(val2, actor, includeDiagnosticData: false, logDiagnostic: false))
				{
					ManagedWardRuntimeContexts.GetOrCreate(val2).PresenceLastTrustedNearbyTime = now;
				}
			}
		}
	}

	private static void RefreshTrustedPlayerPresenceForAreas(float now, IReadOnlyList<PrivateArea> areas)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (areas.Count == 0)
		{
			return;
		}
		List<Player> allPlayers = Player.GetAllPlayers();
		if (allPlayers == null || allPlayers.Count == 0)
		{
			return;
		}
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player val = allPlayers[i];
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			long playerID = val.GetPlayerID();
			if (playerID == 0L)
			{
				continue;
			}
			Vector3 position = ((Component)val).transform.position;
			ManagedWardAccessActor actor = ManagedWardAccessEvaluator.CreateActor(playerID);
			for (int j = 0; j < areas.Count; j++)
			{
				PrivateArea val2 = areas[j];
				if (!((Object)(object)val2 == (Object)null) && val2.IsInside(position, 8f) && ManagedWardAccessEvaluator.HasPlayerAccess(val2, actor, includeDiagnosticData: false, logDiagnostic: false))
				{
					ManagedWardRuntimeContexts.GetOrCreate(val2).PresenceLastTrustedNearbyTime = now;
				}
			}
		}
	}

	private static bool IsWardConsideredAttended(PrivateArea area, float now, float graceSeconds)
	{
		if (ManagedWardRuntimeContexts.TryGet(area, out ManagedWardRuntimeContext context))
		{
			return now - context.PresenceLastTrustedNearbyTime <= graceSeconds;
		}
		return false;
	}

	private static bool HasUnattendedPresenceState(IReadOnlyList<PrivateArea> coveringAreas, float now, float graceSeconds)
	{
		for (int i = 0; i < coveringAreas.Count; i++)
		{
			PrivateArea val = coveringAreas[i];
			if (!((Object)(object)val == (Object)null) && !IsWardConsideredAttended(val, now, graceSeconds))
			{
				return true;
			}
		}
		return false;
	}
}
