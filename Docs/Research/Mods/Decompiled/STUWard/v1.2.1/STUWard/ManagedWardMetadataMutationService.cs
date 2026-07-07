using UnityEngine;

namespace STUWard;

internal static class ManagedWardMetadataMutationService
{
	internal static ManagedWardMetadataMutationResult ObserveAuthoritativeWard(ZDO? zdo, long ownerPlayerId, string wardSteamAccountId, bool authoritativeMetadataChanged, string reason, bool liveDisplayRefresh = false)
	{
		ManagedWardProjectionApplyResult projectionResult = ManagedWardProjectionService.RefreshProjection(zdo, ownerPlayerId, wardSteamAccountId);
		return FinalizeMutation(zdo, projectionResult, authoritativeMetadataChanged, reason, forceSendWhenMetadataChanged: true, notifyObserved: true, null, liveDisplayRefresh);
	}

	internal static ManagedWardMetadataMutationResult RefreshProjectedMetadata(ZDO? zdo, long ownerPlayerId, string wardSteamAccountId, ManagedWardMapMutationKind mutationKind, string reason, bool forceSendWhenMetadataChanged = false, bool liveDisplayRefresh = false)
	{
		ManagedWardProjectionApplyResult projectionResult = ManagedWardProjectionService.RefreshProjection(zdo, ownerPlayerId, wardSteamAccountId);
		return FinalizeMutation(zdo, projectionResult, authoritativeMetadataChanged: false, reason, forceSendWhenMetadataChanged, notifyObserved: false, mutationKind, liveDisplayRefresh);
	}

	internal static ManagedWardMetadataMutationResult ApplyExplicitProjection(ZDO? zdo, ManagedWardProjection projection, ManagedWardMapMutationKind mutationKind, string reason, bool forceSendWhenMetadataChanged = true, bool liveDisplayRefresh = false)
	{
		ManagedWardProjectionApplyResult projectionResult = ManagedWardProjectionService.ApplyProjection(zdo, projection);
		return FinalizeMutation(zdo, projectionResult, authoritativeMetadataChanged: false, reason, forceSendWhenMetadataChanged, notifyObserved: false, mutationKind, liveDisplayRefresh);
	}

	internal static ManagedWardMetadataMutationResult ApplyOwnedLocalProjection(ZDO? zdo, ManagedWardProjection projection, ManagedWardMapMutationKind mutationKind, string reason, bool forceSendWhenMetadataChanged = true, bool liveDisplayRefresh = false)
	{
		ManagedWardProjectionApplyResult projectionResult = ManagedWardProjectionService.ApplyProjection(zdo, projection, requireServer: false);
		return FinalizeMutation(zdo, projectionResult, authoritativeMetadataChanged: false, reason, forceSendWhenMetadataChanged, notifyObserved: false, mutationKind, liveDisplayRefresh);
	}

	internal static void SynchronizeRegistryEntry(ZDO? zdo)
	{
		if (CanSynchronizeRegistry(zdo))
		{
			ManagedWardRegistry.UpsertEntry(zdo);
		}
	}

	private static ManagedWardMetadataMutationResult FinalizeMutation(ZDO? zdo, ManagedWardProjectionApplyResult projectionResult, bool authoritativeMetadataChanged, string reason, bool forceSendWhenMetadataChanged, bool notifyObserved, ManagedWardMapMutationKind? mutationKind, bool liveDisplayRefresh)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		bool registrySynchronized = false;
		if (CanSynchronizeRegistry(zdo))
		{
			ManagedWardRegistry.UpsertEntry(zdo);
			registrySynchronized = true;
		}
		bool fastSendTriggered = false;
		if (zdo != null && zdo.IsValid() && forceSendWhenMetadataChanged && (authoritativeMetadataChanged || projectionResult.AnyChanged))
		{
			ZDOMan instance = ZDOMan.instance;
			if (instance != null)
			{
				instance.ForceSendZDO(zdo.m_uid);
			}
			fastSendTriggered = true;
		}
		if (notifyObserved)
		{
			ManagedWardMapStateService.NotifyWardObserved(zdo, reason, liveDisplayRefresh);
		}
		else if (mutationKind.HasValue && projectionResult.AnyChanged)
		{
			ManagedWardMapStateService.NotifyZdoWardMutation(zdo, mutationKind.Value, reason, liveDisplayRefresh);
		}
		return new ManagedWardMetadataMutationResult(projectionResult, authoritativeMetadataChanged, registrySynchronized, fastSendTriggered);
	}

	private static bool CanSynchronizeRegistry(ZDO? zdo)
	{
		if (zdo != null && zdo.IsValid() && (Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer())
		{
			return WardOwnership.IsManagedWardZdo(zdo);
		}
		return false;
	}
}
