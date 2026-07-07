using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Splatform;
using UnityEngine;

namespace STUWard;

internal static class WardOwnership
{
	private sealed class OverrideState
	{
		internal readonly Dictionary<string, int> WardLimitOverrides = new Dictionary<string, int>(StringComparer.Ordinal);
	}

	private sealed class PlacementLifecycleState
	{
		internal readonly Dictionary<ZDOID, PendingManagedWardPlacementObserve> PendingManagedWardPlacementObserves = new Dictionary<ZDOID, PendingManagedWardPlacementObserve>();

		internal ZDOMan? TrackedZdoMan;

		internal bool ManagedWardObservationInitialized;
	}

	private sealed class LocalState
	{
		internal bool RpcsRegistered;
	}

	private sealed class IdentityAuthState
	{
		internal readonly Dictionary<long, string> ServerPlayerAccountIdsByPlayerId = new Dictionary<long, string>();

		internal readonly Dictionary<long, ServerSessionIdentity> ServerSessionIdentitiesBySender = new Dictionary<long, ServerSessionIdentity>();

		internal readonly Dictionary<long, DateTime> SenderResolveFailureFirstSeenUtcBySender = new Dictionary<long, DateTime>();
	}

	private const string ManagedWardMarkerKey = "stuw_is_managed_ward";

	private const string SteamAccountIdKey = "stuw_owner_account_id";

	private const string LimitRefundProcessedKey = "stuw_limit_refund_processed";

	private const string ReceiveWardPlacementRejectedRpc = "STUWard_ReceiveWardPlacementRejected";

	private const string NotifyManagedWardPlacedRpc = "STUWard_NotifyManagedWardPlaced";

	private const string NotifyManagedWardMapStateChangedRpc = "STUWard_NotifyManagedWardMapStateChanged";

	private const string ReportFileName = "STUWard.WardCountReport.yml";

	private static readonly OverrideState OverrideData = new OverrideState();

	private static readonly PlacementLifecycleState PlacementData = new PlacementLifecycleState();

	private static readonly LocalState LocalData = new LocalState();

	private static readonly List<ZDO> ManagedWardPrefabScanBuffer = new List<ZDO>();

	private static readonly int ManagedWardPrefabHash = StringExtensionMethods.GetStableHashCode("piece_stuward");

	private static readonly TimeSpan WardDataRequestInterval = TimeSpan.FromSeconds(0.5);

	private static readonly TimeSpan WardDataRefreshInterval = TimeSpan.FromSeconds(5.0);

	private static readonly TimeSpan PendingManagedWardPlacementObserveLifetime = TimeSpan.FromSeconds(10.0);

	private static readonly TimeSpan ProtectedRpcSenderResolveGraceWindow = TimeSpan.FromSeconds(5.0);

	private static readonly IdentityAuthState IdentityAuthData = new IdentityAuthState();

	private static Dictionary<string, int> WardLimitOverrides => OverrideData.WardLimitOverrides;

	private static Dictionary<ZDOID, PendingManagedWardPlacementObserve> PendingManagedWardPlacementObserves => PlacementData.PendingManagedWardPlacementObserves;

	private static ZDOMan? _trackedZdoMan
	{
		get
		{
			return PlacementData.TrackedZdoMan;
		}
		set
		{
			PlacementData.TrackedZdoMan = value;
		}
	}

	private static bool _managedWardObservationInitialized
	{
		get
		{
			return PlacementData.ManagedWardObservationInitialized;
		}
		set
		{
			PlacementData.ManagedWardObservationInitialized = value;
		}
	}

	private static bool _rpcsRegistered
	{
		get
		{
			return LocalData.RpcsRegistered;
		}
		set
		{
			LocalData.RpcsRegistered = value;
		}
	}

	private static Dictionary<long, string> ServerPlayerAccountIdsByPlayerId => IdentityAuthData.ServerPlayerAccountIdsByPlayerId;

	private static Dictionary<long, ServerSessionIdentity> ServerSessionIdentitiesBySender => IdentityAuthData.ServerSessionIdentitiesBySender;

	private static Dictionary<long, DateTime> SenderResolveFailureFirstSeenUtcBySender => IdentityAuthData.SenderResolveFailureFirstSeenUtcBySender;

	internal static void Initialize()
	{
		ReloadOverrides(force: true);
	}

	internal static void Update()
	{
		ProcessPendingManagedWardPlacementObserves();
	}

	internal static bool HasPendingRuntimeWork()
	{
		return PendingManagedWardPlacementObserves.Count > 0;
	}

	internal static void HandleWardLimitPolicyChanged()
	{
		ReloadOverrides(force: true);
	}

	internal static void RegisterRpcs()
	{
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (!_rpcsRegistered && instance != null)
		{
			instance.Register<ZPackage>("STUWard_ReceiveWardPlacementRejected", (Action<long, ZPackage>)HandleReceiveWardPlacementRejected);
			instance.Register<ZPackage>("STUWard_NotifyManagedWardPlaced", (Action<long, ZPackage>)HandleNotifyManagedWardPlaced);
			instance.Register<ZPackage>("STUWard_NotifyManagedWardMapStateChanged", (Action<long, ZPackage>)HandleNotifyManagedWardMapStateChanged);
			ManagedWardReportService.RegisterRpcs();
			_rpcsRegistered = true;
			Plugin.LogWardDiagnosticVerbose("Rpcs.Register", $"Registered managed ward routed RPCs. isServer={(Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer()}, " + "placementReject='STUWard_ReceiveWardPlacementRejected', placementNotify='STUWard_NotifyManagedWardPlaced', mapStateNotify='STUWard_NotifyManagedWardMapStateChanged'");
			EnsureManagedWardObservationInitialized();
		}
	}

	internal static void ResetRuntimeState()
	{
		Plugin.LogWardDiagnosticVerbose("Rpcs.ZNetAwake", $"WardOwnership.OnZNetAwake invoked. isServer={(Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer()}, hadRegisteredRpcs={_rpcsRegistered}");
		_rpcsRegistered = false;
		ManagedWardReportService.OnZNetAwake();
		ResetServerRuntimeState();
	}

	internal static void EnsureRuntimeBindings()
	{
		RegisterRpcs();
	}

	internal static void OnZNetAwake()
	{
		ResetRuntimeState();
		EnsureRuntimeBindings();
	}

	internal static void RegisterManagedWardRpcHandlers(PrivateArea area)
	{
	}

	internal static bool WardMatchesAccountId(PrivateArea? area, string accountId)
	{
		string text = NormalizeAccountId(accountId);
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		return MatchesAccountId(GetWardSteamAccountId(area), text);
	}

	internal static void ObserveManagedWard(PrivateArea? area)
	{
		ObserveManagedWard(ManagedWardRef.FromArea(area));
	}

	internal static void ObserveManagedWard(ManagedWardRef ward)
	{
		if (!((Object)(object)ward.Area == (Object)null) && !((Object)(object)ZNet.instance == (Object)null) && ZNet.instance.IsServer())
		{
			ZDO zdo = ward.Zdo;
			if (zdo != null)
			{
				PromoteRuntimeManagedWardZdo(zdo, "runtime area observe");
				EnsureManagedWardObservationInitialized();
				ObserveManagedWard(zdo);
			}
		}
	}

	internal static bool TryStampLocalManagedWardOwnerAccount(PrivateArea? area)
	{
		return TryStampLocalManagedWardOwnerAccount(ManagedWardRef.FromArea(area));
	}

	internal static bool TryStampLocalManagedWardOwnerAccount(ManagedWardRef ward)
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		PrivateArea area = ward.Area;
		if ((Object)(object)area == (Object)null || !ManagedWardIdentity.EnsureManagedComponent(ward))
		{
			return false;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return false;
		}
		if (!WardAccess.IsDirectWardOwner(ward, localPlayer.GetPlayerID()))
		{
			return false;
		}
		string playerAccountId = GetPlayerAccountId(localPlayer);
		if (string.IsNullOrWhiteSpace(playerAccountId))
		{
			return false;
		}
		if (!ward.HasValidNetworkIdentity || !ward.IsOwner)
		{
			return false;
		}
		ZDO zdo = ward.Zdo;
		if (zdo == null)
		{
			return false;
		}
		bool flag = false;
		if (!zdo.GetBool("stuw_is_managed_ward", false))
		{
			zdo.Set("stuw_is_managed_ward", true);
			flag = true;
		}
		if (!SameAccountId(NormalizeAccountId(zdo.GetString("stuw_owner_account_id", string.Empty)), playerAccountId))
		{
			zdo.Set("stuw_owner_account_id", playerAccountId);
			flag = true;
		}
		if (GuildsCompat.TryStampLocalWardGuildMetadata(ward))
		{
			flag = true;
		}
		if (!flag)
		{
			return false;
		}
		ManagedWardMetadataMutationService.SynchronizeRegistryEntry(zdo);
		ZDOMan instance = ZDOMan.instance;
		if (instance != null)
		{
			instance.ForceSendZDO(zdo.m_uid);
		}
		Plugin.LogWardDiagnosticVerbose("Placement.LocalStamp", "Stamped managed ward steamAccountId='" + playerAccountId + "' locally before replication. " + WardDiagnosticInfo.DescribeWard(area));
		return true;
	}

	internal static void RefreshServerPlayerAccountIdForPlayer(Player? player)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)ZNet.instance == (Object)null) && ZNet.instance.IsServer())
		{
			string playerAccountId = GetPlayerAccountId(player);
			if (!string.IsNullOrWhiteSpace(playerAccountId))
			{
				RememberServerPlayerAccountId(player.GetPlayerID(), playerAccountId);
			}
		}
	}

	internal static void RefreshServerPlayerAccountIdForResolvedPlayer(long playerId, string accountId)
	{
		RememberServerPlayerAccountId(playerId, accountId);
	}

	private static void SendWardPlacementRejectedResponse(long receiverUid, int limit, bool showLimitMessage)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (instance == null)
		{
			return;
		}
		if (IsLocalReceiver(receiverUid))
		{
			ManagedWardMapStateService.RequestLocalDisplayRefresh("local managed ward placement rejected", refreshImmediatelyIfVisible: true);
			if (showLimitMessage)
			{
				WardAccess.ShowWardLimitMessage(Player.m_localPlayer, limit);
			}
		}
		else
		{
			ZPackage val = new ZPackage();
			val.Write(showLimitMessage);
			val.Write(limit);
			instance.InvokeRoutedRPC(receiverUid, "STUWard_ReceiveWardPlacementRejected", new object[1] { val });
		}
	}

	private static void RejectManagedWardPlacement(ZDO? zdo, long receiverUid, int limit, bool showLimitMessage, string context, string reason, string wardDescription)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		Plugin.LogWardDiagnosticFailure(context, "Rejected managed ward placement because " + reason + ". " + wardDescription);
		SendWardPlacementRejectedResponse(receiverUid, limit, showLimitMessage);
		if (zdo == null || !zdo.IsValid())
		{
			return;
		}
		DropManagedWardPlacementRefundOnce(zdo);
		ManagedWardRegistry.RemoveEntry(zdo.m_uid);
		WardPrivateAreaSafeAccess.ForgetPermittedPlayerIds(zdo.m_uid);
		ManagedWardMapStateService.NotifyWardRemoved(zdo.m_uid, "managed ward placement rejected");
		TryClaimManagedWardMutationOwnership(zdo, context);
		ZNetScene instance = ZNetScene.instance;
		GameObject val = ((instance != null) ? instance.FindInstance(zdo.m_uid) : null);
		if ((Object)(object)val != (Object)null && (Object)(object)ZNetScene.instance != (Object)null)
		{
			ZNetScene.instance.Destroy(val);
			return;
		}
		ZDOMan instance2 = ZDOMan.instance;
		if (instance2 != null)
		{
			instance2.DestroyZDO(zdo);
		}
	}

	private static void DropManagedWardPlacementRefundOnce(ZDO zdo)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (zdo != null && zdo.IsValid() && !zdo.GetBool("stuw_limit_refund_processed", false))
		{
			zdo.Set("stuw_limit_refund_processed", true);
			DropManagedWardPlacementRefund(zdo.GetPosition());
		}
	}

	private static void DropManagedWardPlacementRefund(Vector3 position)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		Requirement[] currentStuWardRequirements = StuWardPrefab.GetCurrentStuWardRequirements();
		if (currentStuWardRequirements.Length == 0)
		{
			return;
		}
		Vector3 val = position + Vector3.up;
		foreach (Requirement val2 in currentStuWardRequirements)
		{
			ItemDrop resItem = val2.m_resItem;
			if ((Object)(object)resItem == (Object)null || !val2.m_recover)
			{
				continue;
			}
			int amount = val2.GetAmount(1);
			if (amount > 0)
			{
				GameObject gameObject = ((Component)resItem).gameObject;
				int val3 = Math.Max(1, resItem.m_itemData.m_shared.m_maxStackSize);
				int num = amount;
				while (num > 0)
				{
					int num2 = Math.Min(num, val3);
					num -= num2;
					ItemData obj = resItem.m_itemData.Clone();
					obj.m_dropPrefab = gameObject;
					obj.m_stack = num2;
					ItemDrop.DropItem(obj, num2, val, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
				}
			}
		}
	}

	private static bool IsLocalReceiver(long receiverUid)
	{
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer != (Object)null && receiverUid != 0L)
		{
			return ((Character)localPlayer).GetOwner() == receiverUid;
		}
		return false;
	}

	private static void HandleReceiveWardPlacementRejected(long _, ZPackage pkg)
	{
		bool num = pkg.ReadBool();
		int limit = pkg.ReadInt();
		ManagedWardMapStateService.RequestDisplayRefresh("managed ward placement rejected", liveDisplayRefresh: true);
		if (num)
		{
			WardAccess.ShowWardLimitMessage(Player.m_localPlayer, limit);
		}
	}

	private static int GetEffectiveWardLimitForAccount(string accountId)
	{
		ReloadOverrides(force: false);
		return WardLimitPolicy.GetEffectiveLimit(NormalizeOverrideAccountId(accountId), WardLimitOverrides, Plugin.MaxWardsPerSteamId?.Value ?? 3);
	}

	private static void ResetServerRuntimeState()
	{
		ResetPlacementLifecycleState();
		ManagedWardRegistry.Reset();
		ResetIdentityAuthState();
	}

	private static void ResetPlacementLifecycleState()
	{
		if (_trackedZdoMan != null)
		{
			ZDOMan? trackedZdoMan = _trackedZdoMan;
			trackedZdoMan.m_onZDODestroyed = (Action<ZDO>)Delegate.Remove(trackedZdoMan.m_onZDODestroyed, new Action<ZDO>(HandleTrackedWardDestroyed));
			_trackedZdoMan = null;
		}
		PendingManagedWardPlacementObserves.Clear();
		_managedWardObservationInitialized = false;
	}

	private static void EnsureManagedWardObservationInitialized()
	{
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return;
		}
		ZDOMan instance = ZDOMan.instance;
		if (instance != null)
		{
			EnsureTrackedZdoManHooked(instance);
			if (!_managedWardObservationInitialized)
			{
				RunInitialManagedWardObservationPass(instance, "initialization");
			}
		}
	}

	private static void EnsureTrackedZdoManHooked(ZDOMan zdoMan)
	{
		if (_trackedZdoMan != zdoMan)
		{
			if (_trackedZdoMan != null)
			{
				ZDOMan? trackedZdoMan = _trackedZdoMan;
				trackedZdoMan.m_onZDODestroyed = (Action<ZDO>)Delegate.Remove(trackedZdoMan.m_onZDODestroyed, new Action<ZDO>(HandleTrackedWardDestroyed));
			}
			zdoMan.m_onZDODestroyed = (Action<ZDO>)Delegate.Combine(zdoMan.m_onZDODestroyed, new Action<ZDO>(HandleTrackedWardDestroyed));
			_trackedZdoMan = zdoMan;
		}
	}

	private static void RunInitialManagedWardObservationPass(ZDOMan zdoMan, string reason)
	{
		EnsureTrackedZdoManHooked(zdoMan);
		bool managedWardObservationInitialized = _managedWardObservationInitialized;
		_managedWardObservationInitialized = false;
		ManagedWardRegistry.Reset();
		int num = PrepareManagedWardPrefabScan(zdoMan);
		for (int i = 0; i < num; i++)
		{
			ObserveManagedWard(ManagedWardPrefabScanBuffer[i]);
		}
		_managedWardObservationInitialized = true;
		Plugin.LogWardDiagnosticVerbose("Placement.ServerObserve", $"Completed initial managed ward observation pass. reason='{reason}', previousInitialized={managedWardObservationInitialized}, scannedZdos={num}, objectsByIdCount={zdoMan.m_objectsByID.Count}, managedWardPrefabHash={ManagedWardPrefabHash}");
	}

	private static int PrepareManagedWardPrefabScan(ZDOMan zdoMan)
	{
		ManagedWardPrefabScanBuffer.Clear();
		int num = 0;
		while (!zdoMan.GetAllZDOsWithPrefabIterative("piece_stuward", ManagedWardPrefabScanBuffer, ref num))
		{
		}
		return ManagedWardPrefabScanBuffer.Count;
	}

	private static void ObserveManagedWard(ZDO? zdo)
	{
		if (TryPrepareManagedWardObservation(zdo, out var observation, out var authoritativeMetadataChanged))
		{
			Plugin.LogWardDiagnosticVerbose("WardObserve", $"Observing managed ward. ownerPlayerId={observation.PlayerId}, accountId='{observation.AccountId}', {DescribeManagedWardZdo(observation.Zdo)}");
			ApplyManagedWardObservationEffects(observation, authoritativeMetadataChanged);
		}
	}

	private static bool TryPrepareManagedWardObservation(ZDO? zdo, out ManagedWardObservation observation, out bool authoritativeMetadataChanged)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		observation = default(ManagedWardObservation);
		authoritativeMetadataChanged = false;
		if (!IsManagedWardZdo(zdo))
		{
			return false;
		}
		long @long = zdo.GetLong(ZDOVars.s_creator, 0L);
		if ((Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer())
		{
			authoritativeMetadataChanged = TryCanonicalizeWardSteamAccountIdFromCreator(zdo, @long);
		}
		if ((Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer() && _managedWardObservationInitialized)
		{
			if (!TryFinalizeAuthoritativeManagedWardPlacement(zdo, out var metadataChanged))
			{
				return false;
			}
			authoritativeMetadataChanged |= metadataChanged;
		}
		observation = new ManagedWardObservation(zdo, zdo.m_uid, @long, ResolveWardSteamAccountId(zdo, @long));
		return true;
	}

	private static void ApplyManagedWardObservationEffects(ManagedWardObservation observation, bool authoritativeMetadataChanged)
	{
		ManagedWardMetadataMutationService.ObserveAuthoritativeWard(observation.Zdo, observation.PlayerId, observation.AccountId, authoritativeMetadataChanged, "managed ward observed");
	}

	private static bool TryCanonicalizeWardSteamAccountIdFromCreator(ZDO zdo, long ownerPlayerId)
	{
		if (zdo == null || ownerPlayerId == 0L || (Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return false;
		}
		string playerAccountId = GetPlayerAccountId(ownerPlayerId);
		if (string.IsNullOrWhiteSpace(playerAccountId))
		{
			return false;
		}
		string text = NormalizeAccountId(zdo.GetString("stuw_owner_account_id", string.Empty));
		if (SameAccountId(text, playerAccountId))
		{
			return false;
		}
		zdo.Set("stuw_owner_account_id", playerAccountId);
		Plugin.LogWardDiagnosticVerbose("Placement.Server", $"Canonicalized managed ward steamAccountId from creator mapping. ownerPlayerId={ownerPlayerId}, oldSteamAccountId='{text}', newSteamAccountId='{playerAccountId}', {DescribeManagedWardZdo(zdo)}");
		return true;
	}

	private static bool TryFinalizeAuthoritativeManagedWardPlacement(ZDO zdo, out bool metadataChanged)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		metadataChanged = false;
		if (!IsManagedWardZdo(zdo))
		{
			return false;
		}
		string text = NormalizeAccountId(zdo.GetString("stuw_owner_account_id", string.Empty));
		long @long = zdo.GetLong(ZDOVars.s_creator, 0L);
		if (@long == 0L)
		{
			return true;
		}
		long senderUid;
		long num = (TryGetServerSessionSenderUid(@long, out senderUid) ? senderUid : 0);
		string text2 = text;
		if (string.IsNullOrWhiteSpace(text2))
		{
			text2 = GetPlayerAccountId(@long);
		}
		if (string.IsNullOrWhiteSpace(text2))
		{
			text2 = GetAuthoritativeAccountIdFromSender(num, @long);
		}
		Plugin.LogWardDiagnosticVerbose("WardLimit.Server", $"Authoritative placement validation resolved owner identity. ownerPlayerId={@long}, senderUid={num}, storedAccountId='{text}', resolvedAccountId='{text2}', {DescribeManagedWardZdo(zdo)}");
		if (string.IsNullOrWhiteSpace(text2))
		{
			RejectManagedWardPlacement(zdo, num, 0, showLimitMessage: false, "Placement.Server", $"the owner's authoritative steamAccountId could not be resolved. ownerPlayerId={@long}", DescribeManagedWardZdo(zdo));
			return false;
		}
		int effectiveWardLimitForAccount = GetEffectiveWardLimitForAccount(text2);
		int num2 = CountAuthoritativeManagedWardsForAccount(text2, zdo.m_uid);
		WardLimitEvaluation wardLimitEvaluation = WardLimitPolicy.EvaluatePlacement(effectiveWardLimitForAccount, num2);
		Plugin.LogWardDiagnosticVerbose("WardLimit.Server", $"Authoritative placement validation counted direct managed wards. ownerPlayerId={@long}, accountId='{text2}', limit={effectiveWardLimitForAccount}, currentCount={num2}, {DescribeManagedWardZdo(zdo)}");
		if (!wardLimitEvaluation.Allowed)
		{
			RejectManagedWardPlacement(zdo, num, wardLimitEvaluation.Limit, showLimitMessage: true, "Placement.Server", $"the authoritative ward limit was reached. ownerPlayerId={@long}, accountId='{text2}', limit={wardLimitEvaluation.Limit}, currentCount={wardLimitEvaluation.CurrentCount}", DescribeManagedWardZdo(zdo));
			return false;
		}
		if (!SameAccountId(text, text2))
		{
			zdo.Set("stuw_owner_account_id", text2);
			metadataChanged = true;
		}
		Plugin.LogWardDiagnosticVerbose("Placement.Server", $"Validated managed ward steamAccountId='{text2}' using direct managed ward count. ownerPlayerId={@long}, limit={effectiveWardLimitForAccount}, currentCountBeforePlacement={num2}, metadataChanged={metadataChanged}, {DescribeManagedWardZdo(zdo)}");
		return true;
	}

	private static int CountAuthoritativeManagedWardsForAccount(string accountId, ZDOID ignoredZdoId = default(ZDOID))
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ManagedWardRegistry.CountForAccount(accountId, ignoredZdoId);
	}

	private static string DescribeManagedWardZdo(ZDO? zdo)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (zdo == null)
		{
			return "wardZdo=null";
		}
		string text = NormalizeAccountId(zdo.GetString("stuw_owner_account_id", string.Empty));
		return string.Format("wardZdo={0}, zdoValid={1}, prefab={2}, prefabMatchesManaged={3}, managedMarker={4}, zdoCreator={5}, steamAccountId='{6}'", zdo.m_uid, zdo.IsValid(), zdo.GetPrefab(), zdo.GetPrefab() == ManagedWardPrefabHash, zdo.GetBool("stuw_is_managed_ward", false), zdo.GetLong(ZDOVars.s_creator, 0L), text);
	}

	private static void HandleTrackedWardDestroyed(ZDO zdo)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (zdo != null)
		{
			ManagedWardRegistry.RemoveEntry(zdo.m_uid);
			WardPrivateAreaSafeAccess.ForgetPermittedPlayerIds(zdo.m_uid);
			ManagedWardMapStateService.NotifyWardRemoved(zdo.m_uid, "managed ward destroyed");
		}
	}

	internal static bool IsManagedWardZdo(ZDO? zdo)
	{
		if (zdo == null || !zdo.IsValid())
		{
			return false;
		}
		if (zdo.GetPrefab() == ManagedWardPrefabHash)
		{
			return true;
		}
		if (zdo.GetBool("stuw_is_managed_ward", false))
		{
			return true;
		}
		if (!string.IsNullOrWhiteSpace(NormalizeAccountId(zdo.GetString("stuw_owner_account_id", string.Empty))))
		{
			return true;
		}
		return false;
	}

	private static bool PromoteRuntimeManagedWardZdo(ZDO zdo, string reason)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (zdo == null || !zdo.IsValid())
		{
			return false;
		}
		bool flag = false;
		if (!zdo.GetBool("stuw_is_managed_ward", false))
		{
			zdo.Set("stuw_is_managed_ward", true);
			flag = true;
		}
		string playerAccountId = GetPlayerAccountId(zdo.GetLong(ZDOVars.s_creator, 0L));
		string left = NormalizeAccountId(zdo.GetString("stuw_owner_account_id", string.Empty));
		if (!string.IsNullOrWhiteSpace(playerAccountId) && !SameAccountId(left, playerAccountId))
		{
			zdo.Set("stuw_owner_account_id", playerAccountId);
			flag = true;
		}
		if (zdo.GetPrefab() != ManagedWardPrefabHash)
		{
			Plugin.LogWardDiagnosticVerbose("Placement.ServerObserve", $"Observed runtime managed ward with unexpected prefab hash during {reason}. expected={ManagedWardPrefabHash}, actual={zdo.GetPrefab()}, {DescribeManagedWardZdo(zdo)}");
		}
		if (!flag)
		{
			return false;
		}
		ZDOMan instance = ZDOMan.instance;
		if (instance != null)
		{
			instance.ForceSendZDO(zdo.m_uid);
		}
		Plugin.LogWardDiagnosticVerbose("Placement.ServerObserve", "Promoted runtime managed ward ZDO during " + reason + ". " + DescribeManagedWardZdo(zdo));
		return true;
	}

	internal static void ForceSyncManagedWardZdoToServer(PrivateArea? area, string context)
	{
		ForceSyncManagedWardZdoToServer(ManagedWardRef.FromArea(area), context);
	}

	internal static void ForceSyncManagedWardZdoToServer(ManagedWardRef ward, string context)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		PrivateArea area = ward.Area;
		if ((Object)(object)area == (Object)null || !ManagedWardIdentity.EnsureManagedComponent(ward) || (Object)(object)ZNet.instance == (Object)null || ZNet.instance.IsServer() || !ward.HasValidNetworkIdentity || !ward.IsOwner)
		{
			return;
		}
		ZDO zdo = ward.Zdo;
		if (zdo == null || !zdo.IsValid() || !TryGetServerPeerId(out var serverPeerId))
		{
			return;
		}
		long[] permittedPlayerIds = WardPrivateAreaSafeAccess.GetPermittedPlayerIds(area);
		long @long = zdo.GetLong(ZDOVars.s_creator, 0L);
		int wardGuildId = GuildsCompat.GetWardGuildId(zdo);
		Vector3 position = ((Component)area).transform.position;
		float storedRadiusOrMin = WardSettings.GetStoredRadiusOrMin(area);
		bool flag = area.IsEnabled();
		ZDOMan instance = ZDOMan.instance;
		if (instance != null)
		{
			instance.ForceSendZDO(serverPeerId, zdo.m_uid);
		}
		ZRoutedRpc instance2 = ZRoutedRpc.instance;
		if (instance2 != null)
		{
			ZPackage val = new ZPackage();
			val.Write(zdo.m_uid);
			val.Write(zdo.DataRevision);
			val.Write(position);
			val.Write(storedRadiusOrMin);
			val.Write(flag);
			val.Write(@long);
			val.Write(wardGuildId);
			val.Write(permittedPlayerIds.Length);
			for (int i = 0; i < permittedPlayerIds.Length; i++)
			{
				val.Write(permittedPlayerIds[i]);
			}
			instance2.InvokeRoutedRPC(serverPeerId, "STUWard_NotifyManagedWardMapStateChanged", new object[1] { val });
		}
		Plugin.LogWardDiagnosticVerbose(context, $"Force-sent managed ward ZDO to server after local mutation and notified server map-state refresh. serverPeerId={serverPeerId}, dataRevision={zdo.DataRevision}, radius={storedRadiusOrMin:F1}, enabled={flag}, permittedCount={permittedPlayerIds.Length}, {WardDiagnosticInfo.DescribeWard(area)}");
	}

	internal static bool TryClaimManagedWardMutationOwnership(PrivateArea? area, string context)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		ZNetView nView = WardPrivateAreaSafeAccess.GetNView(area);
		if ((Object)(object)nView == (Object)null || !nView.IsValid())
		{
			return false;
		}
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer() || nView.IsOwner())
		{
			return true;
		}
		nView.ClaimOwnership();
		if (!nView.IsOwner())
		{
			ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(nView);
			if (zdo != null && zdo.IsValid())
			{
				zdo.SetOwner(ZDOMan.GetSessionID());
				ZDOMan instance = ZDOMan.instance;
				if (instance != null)
				{
					instance.ForceSendZDO(zdo.m_uid);
				}
			}
		}
		if (!nView.IsOwner())
		{
			Plugin.LogWardDiagnosticFailure(context, "Failed to claim managed ward ownership for mutation. " + WardDiagnosticInfo.DescribeWard(area));
			return false;
		}
		Plugin.LogWardDiagnosticVerbose(context, "Claimed managed ward ownership for mutation. " + WardDiagnosticInfo.DescribeWard(area));
		return true;
	}

	private static bool TryClaimManagedWardMutationOwnership(ZDO? zdo, string context)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (zdo == null || !zdo.IsValid())
		{
			return false;
		}
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return true;
		}
		ZNetScene instance = ZNetScene.instance;
		GameObject val = ((instance != null) ? instance.FindInstance(zdo.m_uid) : null);
		ZNetView val2 = (((Object)(object)val != (Object)null) ? val.GetComponent<ZNetView>() : null);
		if ((Object)(object)val2 != (Object)null && val2.IsValid())
		{
			PrivateArea component = val.GetComponent<PrivateArea>();
			if (!((Object)(object)component != (Object)null))
			{
				return val2.IsOwner();
			}
			return TryClaimManagedWardMutationOwnership(component, context);
		}
		if (zdo.GetOwner() != ZDOMan.GetSessionID())
		{
			zdo.SetOwner(ZDOMan.GetSessionID());
			ZDOMan instance2 = ZDOMan.instance;
			if (instance2 != null)
			{
				instance2.ForceSendZDO(zdo.m_uid);
			}
		}
		if (zdo.GetOwner() != ZDOMan.GetSessionID())
		{
			Plugin.LogWardDiagnosticFailure(context, "Failed to claim managed ward ZDO ownership for mutation. " + DescribeManagedWardZdo(zdo));
			return false;
		}
		Plugin.LogWardDiagnosticVerbose(context, "Claimed managed ward ZDO ownership for mutation. " + DescribeManagedWardZdo(zdo));
		return true;
	}

	internal static void NotifyServerManagedWardPlaced(PrivateArea? area)
	{
		NotifyServerManagedWardPlaced(ManagedWardRef.FromArea(area));
	}

	internal static void NotifyServerManagedWardPlaced(ManagedWardRef ward)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ward.Area == (Object)null || !ManagedWardIdentity.EnsureManagedComponent(ward) || !ward.HasValidNetworkIdentity)
		{
			return;
		}
		ZDO zdo = ward.Zdo;
		if (zdo == null)
		{
			return;
		}
		if ((Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer())
		{
			ObserveManagedWard(ward);
			return;
		}
		Player localPlayer = Player.m_localPlayer;
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (!((Object)(object)localPlayer == (Object)null) && instance != null)
		{
			long serverPeerID = instance.GetServerPeerID();
			if (serverPeerID != 0L)
			{
				ZPackage val = new ZPackage();
				val.Write(localPlayer.GetPlayerID());
				val.Write(zdo.m_uid);
				Plugin.LogWardDiagnosticVerbose("Placement.Notify", $"Notifying server about locally placed managed ward. senderPlayerId={localPlayer.GetPlayerID()}, wardZdo={zdo.m_uid}");
				instance.InvokeRoutedRPC(serverPeerID, "STUWard_NotifyManagedWardPlaced", new object[1] { val });
			}
		}
	}

	internal static bool CanApplyManagedWardStateLocally(ZNetView? nview)
	{
		if ((Object)(object)nview != (Object)null && nview.IsValid() && nview.IsOwner() && (Object)(object)ZNet.instance != (Object)null)
		{
			return ZNet.instance.IsServer();
		}
		return false;
	}

	internal static bool CanHandleManagedWardStateRpc(ZNetView? nview)
	{
		if ((Object)(object)nview != (Object)null && nview.IsValid())
		{
			if (!nview.IsOwner())
			{
				if ((Object)(object)ZNet.instance != (Object)null)
				{
					return ZNet.instance.IsServer();
				}
				return false;
			}
			return true;
		}
		return false;
	}

	internal static bool TryGetServerPeerId(out long serverPeerId)
	{
		serverPeerId = 0L;
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (instance == null)
		{
			return false;
		}
		serverPeerId = instance.GetServerPeerID();
		return serverPeerId != 0;
	}

	private static void HandleNotifyManagedWardPlaced(long sender, ZPackage pkg)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer() || pkg == null)
		{
			return;
		}
		long claimedPlayerId = pkg.ReadLong();
		ZDOID val = pkg.ReadZDOID();
		if (!TryResolveClaimedPlayerIdFromSender(sender, claimedPlayerId, "Placement.Notify", out var playerId))
		{
			return;
		}
		if (((ZDOID)(ref val)).IsNone())
		{
			Plugin.LogWardDiagnosticFailure("Placement.Notify", $"Rejected managed ward placement notify because ward ZDO id was empty. sender={sender}, requesterId={playerId}.");
			return;
		}
		ZDOMan instance = ZDOMan.instance;
		ZDO val2 = ((instance != null) ? instance.GetZDO(val) : null);
		if (val2 == null || !val2.IsValid())
		{
			EnqueuePendingManagedWardPlacementObserve(sender, playerId, val);
			return;
		}
		long @long = val2.GetLong(ZDOVars.s_creator, 0L);
		if (@long != 0L && @long != playerId)
		{
			Plugin.LogWardDiagnosticFailure("Placement.Notify", $"Rejected managed ward placement notify because requester did not match ward creator. sender={sender}, requesterId={playerId}, wardCreator={@long}, wardZdo={val}");
		}
		else
		{
			ObserveManagedWard(val2);
			Plugin.LogWardDiagnosticVerbose("Placement.Notify", $"Observed managed ward from placement notify. sender={sender}, requesterId={playerId}, {DescribeManagedWardZdo(val2)}");
		}
	}

	private static void EnqueuePendingManagedWardPlacementObserve(long sender, long requesterId, ZDOID wardZdoId)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!((ZDOID)(ref wardZdoId)).IsNone())
		{
			PendingManagedWardPlacementObserves[wardZdoId] = new PendingManagedWardPlacementObserve(wardZdoId, sender, requesterId, DateTime.UtcNow);
			Plugin.LogWardDiagnosticVerbose("Placement.Notify", $"Deferred managed ward placement observe because ward ZDO was not available yet. sender={sender}, requesterId={requesterId}, wardZdo={wardZdoId}");
		}
	}

	private static void ProcessPendingManagedWardPlacementObserves()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		if (PendingManagedWardPlacementObserves.Count == 0 || (Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return;
		}
		ZDOMan instance = ZDOMan.instance;
		if (instance == null)
		{
			return;
		}
		List<ZDOID> list = null;
		DateTime utcNow = DateTime.UtcNow;
		foreach (KeyValuePair<ZDOID, PendingManagedWardPlacementObserve> pendingManagedWardPlacementObserf in PendingManagedWardPlacementObserves)
		{
			PendingManagedWardPlacementObserve value = pendingManagedWardPlacementObserf.Value;
			ZDO zDO = instance.GetZDO(value.WardZdoId);
			if (zDO == null || !zDO.IsValid())
			{
				if (!(utcNow - value.FirstSeenUtc < PendingManagedWardPlacementObserveLifetime))
				{
					if (list == null)
					{
						list = new List<ZDOID>();
					}
					list.Add(pendingManagedWardPlacementObserf.Key);
					Plugin.LogWardDiagnosticFailure("Placement.Notify", $"Dropped deferred managed ward placement observe because the ward ZDO never became available. sender={value.SenderUid}, requesterId={value.RequesterId}, wardZdo={value.WardZdoId}");
				}
				continue;
			}
			long @long = zDO.GetLong(ZDOVars.s_creator, 0L);
			if (@long != 0L && @long != value.RequesterId)
			{
				if (list == null)
				{
					list = new List<ZDOID>();
				}
				list.Add(pendingManagedWardPlacementObserf.Key);
				Plugin.LogWardDiagnosticFailure("Placement.Notify", $"Dropped deferred managed ward placement observe because requester did not match ward creator. sender={value.SenderUid}, requesterId={value.RequesterId}, wardCreator={@long}, wardZdo={value.WardZdoId}");
			}
			else
			{
				ObserveManagedWard(zDO);
				if (list == null)
				{
					list = new List<ZDOID>();
				}
				list.Add(pendingManagedWardPlacementObserf.Key);
				Plugin.LogWardDiagnosticVerbose("Placement.Notify", $"Observed managed ward from deferred placement notify. sender={value.SenderUid}, requesterId={value.RequesterId}, {DescribeManagedWardZdo(zDO)}");
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				PendingManagedWardPlacementObserves.Remove(list[i]);
			}
		}
	}

	private static void HandleNotifyManagedWardMapStateChanged(long sender, ZPackage pkg)
	{
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer() || pkg == null)
		{
			return;
		}
		ZDOID val;
		uint num;
		Vector3 val2;
		float num2;
		bool flag;
		long num3;
		int num4;
		int num5;
		try
		{
			val = pkg.ReadZDOID();
			num = pkg.ReadUInt();
			val2 = pkg.ReadVector3();
			num2 = pkg.ReadSingle();
			flag = pkg.ReadBool();
			num3 = pkg.ReadLong();
			num4 = pkg.ReadInt();
			num5 = pkg.ReadInt();
		}
		catch
		{
			Plugin.LogWardDiagnosticFailure("WardPins.Sync", $"Failed to deserialize managed ward map-state sync header. sender={sender}");
			return;
		}
		if (((ZDOID)(ref val)).IsNone())
		{
			return;
		}
		if (num5 < 0)
		{
			Plugin.LogWardDiagnosticFailure("WardPins.Sync", $"Rejected managed ward map-state sync with invalid permitted count. sender={sender}, wardZdo={val}, permittedCount={num5}");
			return;
		}
		long[] array = ((num5 == 0) ? Array.Empty<long>() : new long[num5]);
		try
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = pkg.ReadLong();
			}
		}
		catch
		{
			Plugin.LogWardDiagnosticFailure("WardPins.Sync", $"Failed to deserialize managed ward map-state sync permitted players. sender={sender}, wardZdo={val}, permittedCount={num5}");
			return;
		}
		ZDOMan instance = ZDOMan.instance;
		ZDO val3 = ((instance != null) ? instance.GetZDO(val) : null);
		if (val3 != null && val3.IsValid() && !IsManagedWardZdo(val3))
		{
			Plugin.LogWardDiagnosticVerbose("WardPins.Sync", $"Ignored managed ward map-state sync because the ward ZDO was available but not managed. sender={sender}, wardZdo={val}");
			return;
		}
		ManagedWardMapStateService.NotifySyncedWardState(val, num, num3, num4, val2, num2, flag, array, "server applied managed ward map-state sync");
		Plugin.LogWardDiagnosticVerbose("WardPins.Sync", $"Applied managed ward map-state sync on server. sender={sender}, wardZdo={val}, dataRevision={num}, position={val2}, radius={num2:F1}, enabled={flag}, ownerPlayerId={num3}, guildId={num4}, permittedCount={array.Length}, zdoPresent={val3 != null && val3.IsValid()}");
	}

	private static void ResetIdentityAuthState()
	{
		ServerPlayerAccountIdsByPlayerId.Clear();
		ServerSessionIdentitiesBySender.Clear();
		SenderResolveFailureFirstSeenUtcBySender.Clear();
	}

	internal static string GetWardSteamAccountId(PrivateArea? area)
	{
		return GetWardSteamAccountId(WardPrivateAreaSafeAccess.GetZdo(area));
	}

	internal static string GetWardSteamAccountId(ZDO? zdo)
	{
		return NormalizeAccountId(((zdo != null) ? zdo.GetString("stuw_owner_account_id", string.Empty) : null) ?? string.Empty);
	}

	internal static string ResolveWardSteamAccountId(ZDO? zdo, long playerId = 0L, string fallbackSteamAccountId = "")
	{
		string text = NormalizeAccountId(((zdo != null) ? zdo.GetString("stuw_owner_account_id", string.Empty) : null) ?? string.Empty);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		string text2 = NormalizeAccountId(fallbackSteamAccountId);
		if (!string.IsNullOrWhiteSpace(text2))
		{
			return text2;
		}
		if (playerId != 0L)
		{
			return GetPlayerAccountId(playerId);
		}
		return string.Empty;
	}

	internal static string GetPlayerAccountId(Player? player)
	{
		if (!((Object)(object)player == (Object)null))
		{
			return GetPlayerAccountId(player.GetPlayerID());
		}
		return string.Empty;
	}

	internal static string GetPlayerAccountId(long playerId)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (playerId == 0L)
		{
			return string.Empty;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer != (Object)null && localPlayer.GetPlayerID() == playerId)
		{
			string localPlayerAccountId = GetLocalPlayerAccountId();
			if (!string.IsNullOrWhiteSpace(localPlayerAccountId))
			{
				RememberServerPlayerAccountId(playerId, localPlayerAccountId);
				return localPlayerAccountId;
			}
		}
		string cachedServerPlayerAccountId = GetCachedServerPlayerAccountId(playerId);
		if (!string.IsNullOrWhiteSpace(cachedServerPlayerAccountId))
		{
			return cachedServerPlayerAccountId;
		}
		string serverSessionAccountId = GetServerSessionAccountId(playerId);
		if (!string.IsNullOrWhiteSpace(serverSessionAccountId))
		{
			return serverSessionAccountId;
		}
		PlayerInfo? val = FindPlayerInfo(playerId);
		if (!val.HasValue)
		{
			return string.Empty;
		}
		try
		{
			PlayerInfo value = val.Value;
			string text = NormalizeAccountId(((object)(PlatformUserID)(ref value.m_userInfo.m_id)).ToString());
			RememberServerPlayerAccountId(playerId, text);
			return text;
		}
		catch
		{
			return string.Empty;
		}
	}

	internal static string GetPlayerSteamIdDisplay(long playerId)
	{
		return GetPlayerAccountId(playerId);
	}

	internal static string GetPlayerName(long playerId)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (playerId == 0L)
		{
			return string.Empty;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer != (Object)null && localPlayer.GetPlayerID() == playerId)
		{
			return localPlayer.GetPlayerName();
		}
		PlayerInfo? val = FindPlayerInfo(playerId);
		if (val.HasValue)
		{
			return val.Value.m_name ?? string.Empty;
		}
		return GetServerSessionPlayerName(playerId);
	}

	internal static string NormalizeAccountIdValue(string? rawAccountId)
	{
		return NormalizeAccountId(rawAccountId);
	}

	internal static string NormalizeOverrideAccountIdValue(string? rawAccountId)
	{
		return NormalizeOverrideAccountId(rawAccountId);
	}

	internal static long ResolvePlayerIdFromSender(long sender)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (sender == 0L)
		{
			return 0L;
		}
		if (TryResolvePlayerIdFromSessionId(sender, out var playerId))
		{
			return playerId;
		}
		if ((Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer())
		{
			if (TryGetServerSessionIdentity(sender, out var sessionIdentity))
			{
				if (sessionIdentity.PlayerId != 0L)
				{
					return sessionIdentity.PlayerId;
				}
				RefreshServerSessionIdentity(ZNet.instance.GetPeer(sender));
				if (TryGetServerSessionIdentity(sender, out sessionIdentity) && sessionIdentity.PlayerId != 0L)
				{
					return sessionIdentity.PlayerId;
				}
			}
			else
			{
				RefreshServerSessionIdentity(ZNet.instance.GetPeer(sender));
				if (TryGetServerSessionIdentity(sender, out sessionIdentity) && sessionIdentity.PlayerId != 0L)
				{
					return sessionIdentity.PlayerId;
				}
			}
		}
		ZNet instance = ZNet.instance;
		long playerId2 = GetPlayerId((instance != null) ? instance.GetPeer(sender) : null);
		if (playerId2 == 0L)
		{
			return GetLocalHostPlayerId(sender);
		}
		return playerId2;
	}

	internal static bool TryResolveAuthoritativePlayerIdFromSender(long sender, string context, out long playerId)
	{
		playerId = ResolvePlayerIdFromSender(sender);
		if (playerId != 0L)
		{
			SenderResolveFailureFirstSeenUtcBySender.Remove(sender);
			return true;
		}
		LogProtectedRpcSenderResolveFailure(context, sender);
		return false;
	}

	internal static string GetAuthoritativeAccountIdFromSender(long sender, long playerId = 0L)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (sender != 0L)
		{
			if (TryGetServerSessionIdentity(sender, out var sessionIdentity) && !string.IsNullOrWhiteSpace(sessionIdentity.AccountId))
			{
				return sessionIdentity.AccountId;
			}
			if ((Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer())
			{
				RefreshServerSessionIdentity(ZNet.instance.GetPeer(sender));
				if (TryGetServerSessionIdentity(sender, out sessionIdentity) && !string.IsNullOrWhiteSpace(sessionIdentity.AccountId))
				{
					return sessionIdentity.AccountId;
				}
			}
		}
		if (playerId != 0L)
		{
			return GetPlayerAccountId(playerId);
		}
		return string.Empty;
	}

	internal static bool TryResolveClaimedPlayerIdFromSender(long sender, long claimedPlayerId, string context, out long playerId)
	{
		playerId = 0L;
		if (claimedPlayerId == 0L)
		{
			Plugin.LogWardDiagnosticFailure(context, $"Rejected protected RPC because claimed player id was empty. sender={sender}.");
			return false;
		}
		Player player = Player.GetPlayer(claimedPlayerId);
		if ((Object)(object)player != (Object)null && ((Character)player).GetOwner() == sender)
		{
			playerId = claimedPlayerId;
			return true;
		}
		long num = ResolvePlayerIdFromSender(sender);
		if (num == claimedPlayerId && num != 0L)
		{
			playerId = num;
			return true;
		}
		Plugin.LogWardDiagnosticFailure(context, $"Rejected protected RPC because sender/player could not be validated. sender={sender}, claimedPlayerId={claimedPlayerId}, resolvedPlayerId={num}.");
		return false;
	}

	internal static long GetPlayerIdFromSender(long sender)
	{
		return ResolvePlayerIdFromSender(sender);
	}

	internal static void RefreshServerSessionIdentity(ZNetPeer? peer, ZDOID characterIdOverride = default(ZDOID))
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (peer != null && peer.m_uid != 0L && !((Object)(object)ZNet.instance == (Object)null) && ZNet.instance.IsServer())
		{
			ZDOID val = ((!((ZDOID)(ref characterIdOverride)).IsNone()) ? characterIdOverride : peer.m_characterID);
			long playerId = GetPlayerId(val);
			string accountId = ResolveServerSessionAccountId(playerId, val);
			string playerName = ResolveServerSessionPlayerName(peer, val, playerId);
			ServerSessionIdentitiesBySender[peer.m_uid] = new ServerSessionIdentity(peer.m_uid, val, playerId, accountId, playerName);
			SenderResolveFailureFirstSeenUtcBySender.Remove(peer.m_uid);
		}
	}

	internal static void ForgetServerSessionIdentity(ZNetPeer? peer)
	{
		if (peer != null && peer.m_uid != 0L)
		{
			if (ServerSessionIdentitiesBySender.TryGetValue(peer.m_uid, out var value) && value.PlayerId != 0L)
			{
				WardAdminDebugAccess.ForgetServerPlayer(value.PlayerId);
			}
			ServerSessionIdentitiesBySender.Remove(peer.m_uid);
			SenderResolveFailureFirstSeenUtcBySender.Remove(peer.m_uid);
		}
	}

	private static void LogProtectedRpcSenderResolveFailure(string context, long sender)
	{
		if (!ShouldUseProtectedRpcSenderResolveGraceWindow(context))
		{
			Plugin.LogWardDiagnosticFailure(context, $"Rejected protected RPC because sender could not be resolved authoritatively. sender={sender}.");
			return;
		}
		DateTime utcNow = DateTime.UtcNow;
		if (!SenderResolveFailureFirstSeenUtcBySender.TryGetValue(sender, out var value))
		{
			value = utcNow;
			SenderResolveFailureFirstSeenUtcBySender[sender] = value;
		}
		if (utcNow - value < ProtectedRpcSenderResolveGraceWindow)
		{
			Plugin.LogWardDiagnosticVerbose(context, $"Deferred protected RPC during join grace window because sender could not yet be resolved authoritatively. sender={sender}.");
		}
		else
		{
			Plugin.LogWardDiagnosticFailure(context, $"Rejected protected RPC because sender could not be resolved authoritatively. sender={sender}.");
		}
	}

	private static bool ShouldUseProtectedRpcSenderResolveGraceWindow(string context)
	{
		if (!(context == "AdminDebug.Sync") && !(context == "WardPins.Request"))
		{
			return context == "GuildsCompat.Sync";
		}
		return true;
	}

	private static bool TryResolvePlayerIdFromSessionId(long sender, out long playerId)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		playerId = GetLocalHostPlayerId(sender);
		if (playerId != 0L)
		{
			return true;
		}
		PlayerInfo? val = FindPlayerInfoBySessionId(sender);
		if (val.HasValue)
		{
			playerId = GetPlayerId(val.Value.m_characterID);
			if (playerId != 0L)
			{
				return true;
			}
		}
		ZRoutedRpc instance = ZRoutedRpc.instance;
		object obj = ((instance != null) ? instance.GetPeer(sender) : null);
		if (obj == null)
		{
			ZNet instance2 = ZNet.instance;
			obj = ((instance2 != null) ? instance2.GetPeer(sender) : null);
		}
		ZNetPeer val2 = (ZNetPeer)obj;
		if (val2 != null)
		{
			playerId = GetPlayerId(val2.m_characterID);
			if (playerId != 0L)
			{
				return true;
			}
		}
		return false;
	}

	private static long GetLocalHostPlayerId(long sender)
	{
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return 0L;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return 0L;
		}
		long playerID = localPlayer.GetPlayerID();
		if (playerID == 0L)
		{
			return 0L;
		}
		if (((Character)localPlayer).GetOwner() != sender)
		{
			return 0L;
		}
		return playerID;
	}

	private static long GetPlayerId(ZNetPeer? peer)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (peer != null)
		{
			return GetPlayerId(peer.m_characterID);
		}
		return 0L;
	}

	private static long GetPlayerId(ZDOID characterId)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (((ZDOID)(ref characterId)).IsNone())
		{
			return 0L;
		}
		ZDOMan instance = ZDOMan.instance;
		ZDO obj = ((instance != null) ? instance.GetZDO(characterId) : null);
		if (obj == null)
		{
			return 0L;
		}
		return obj.GetLong(ZDOVars.s_playerID, 0L);
	}

	private static PlayerInfo? FindPlayerInfo(long playerId)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		List<PlayerInfo> list = ZNet.instance?.m_players;
		if (list == null)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			PlayerInfo val = list[i];
			ZDOMan instance = ZDOMan.instance;
			ZDO obj = ((instance != null) ? instance.GetZDO(val.m_characterID) : null);
			if (((obj != null) ? obj.GetLong(ZDOVars.s_playerID, 0L) : 0) == playerId)
			{
				return val;
			}
		}
		return null;
	}

	private static PlayerInfo? FindPlayerInfoByCharacterId(ZDOID characterId)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (((ZDOID)(ref characterId)).IsNone())
		{
			return null;
		}
		List<PlayerInfo> list = ZNet.instance?.m_players;
		if (list == null)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			PlayerInfo value = list[i];
			if (((ZDOID)(ref value.m_characterID)).Equals(characterId))
			{
				return value;
			}
		}
		return null;
	}

	private static PlayerInfo? FindPlayerInfoBySessionId(long sessionId)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (sessionId == 0L)
		{
			return null;
		}
		List<PlayerInfo> list = ZNet.instance?.m_players;
		if (list == null)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			PlayerInfo value = list[i];
			if (((ZDOID)(ref value.m_characterID)).UserID == sessionId)
			{
				return value;
			}
		}
		return null;
	}

	private static bool TryGetServerSessionIdentity(long sender, out ServerSessionIdentity sessionIdentity)
	{
		return ServerSessionIdentitiesBySender.TryGetValue(sender, out sessionIdentity);
	}

	private static string ResolveServerSessionAccountId(long playerId, ZDOID characterId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (playerId != 0L)
		{
			string cachedServerPlayerAccountId = GetCachedServerPlayerAccountId(playerId);
			if (!string.IsNullOrWhiteSpace(cachedServerPlayerAccountId))
			{
				return cachedServerPlayerAccountId;
			}
		}
		PlayerInfo? val = FindPlayerInfoByCharacterId(characterId);
		if (!val.HasValue && playerId != 0L)
		{
			val = FindPlayerInfo(playerId);
		}
		if (!val.HasValue)
		{
			return string.Empty;
		}
		try
		{
			PlayerInfo value = val.Value;
			string text = NormalizeAccountId(((object)(PlatformUserID)(ref value.m_userInfo.m_id)).ToString());
			if (playerId != 0L && !string.IsNullOrWhiteSpace(text))
			{
				RememberServerPlayerAccountId(playerId, text);
			}
			return text;
		}
		catch
		{
			return string.Empty;
		}
	}

	private static string ResolveServerSessionPlayerName(ZNetPeer peer, ZDOID characterId, long playerId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrWhiteSpace(peer.m_playerName))
		{
			return peer.m_playerName;
		}
		PlayerInfo? val = FindPlayerInfoByCharacterId(characterId);
		if (val.HasValue)
		{
			return val.Value.m_name ?? string.Empty;
		}
		if (playerId != 0L)
		{
			return GetPlayerName(playerId);
		}
		return string.Empty;
	}

	private static string GetServerSessionAccountId(long playerId)
	{
		if (playerId == 0L)
		{
			return string.Empty;
		}
		foreach (ServerSessionIdentity value in ServerSessionIdentitiesBySender.Values)
		{
			if (value.PlayerId == playerId && !string.IsNullOrWhiteSpace(value.AccountId))
			{
				return value.AccountId;
			}
		}
		return string.Empty;
	}

	private static string GetServerSessionPlayerName(long playerId)
	{
		if (playerId == 0L)
		{
			return string.Empty;
		}
		foreach (ServerSessionIdentity value in ServerSessionIdentitiesBySender.Values)
		{
			if (value.PlayerId == playerId && !string.IsNullOrWhiteSpace(value.PlayerName))
			{
				return value.PlayerName;
			}
		}
		return string.Empty;
	}

	private static bool TryGetServerSessionSenderUid(long playerId, out long senderUid)
	{
		senderUid = 0L;
		if (playerId == 0L)
		{
			return false;
		}
		foreach (ServerSessionIdentity value in ServerSessionIdentitiesBySender.Values)
		{
			if (value.PlayerId == playerId && value.SenderUid != 0L)
			{
				senderUid = value.SenderUid;
				return true;
			}
		}
		return false;
	}

	private static void RefreshServerSessionAccountIds(long playerId, string accountId)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (playerId == 0L || string.IsNullOrWhiteSpace(accountId))
		{
			return;
		}
		List<long> list = null;
		foreach (KeyValuePair<long, ServerSessionIdentity> item in ServerSessionIdentitiesBySender)
		{
			if (item.Value.PlayerId == playerId)
			{
				if (list == null)
				{
					list = new List<long>();
				}
				list.Add(item.Key);
			}
		}
		if (list != null)
		{
			string accountId2 = NormalizeAccountId(accountId);
			for (int i = 0; i < list.Count; i++)
			{
				long key = list[i];
				ServerSessionIdentity serverSessionIdentity = ServerSessionIdentitiesBySender[key];
				ServerSessionIdentitiesBySender[key] = new ServerSessionIdentity(serverSessionIdentity.SenderUid, serverSessionIdentity.CharacterZdoId, serverSessionIdentity.PlayerId, accountId2, serverSessionIdentity.PlayerName);
			}
		}
	}

	private static string GetCachedServerPlayerAccountId(long playerId)
	{
		if (playerId == 0L || (Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return string.Empty;
		}
		if (!ServerPlayerAccountIdsByPlayerId.TryGetValue(playerId, out string value))
		{
			return string.Empty;
		}
		return value;
	}

	private static void RememberServerPlayerAccountId(long playerId, string accountId)
	{
		if (playerId != 0L && !((Object)(object)ZNet.instance == (Object)null) && ZNet.instance.IsServer())
		{
			string text = NormalizeAccountId(accountId);
			if (!string.IsNullOrWhiteSpace(text) && (!ServerPlayerAccountIdsByPlayerId.TryGetValue(playerId, out string value) || !SameAccountId(value, text)))
			{
				ServerPlayerAccountIdsByPlayerId[playerId] = text;
				RefreshServerSessionAccountIds(playerId, text);
			}
		}
	}

	private static string GetLocalPlayerAccountId()
	{
		try
		{
			return NormalizeAccountId(((object)(PlatformUserID)(ref UserInfo.GetLocalUser().UserId)).ToString());
		}
		catch
		{
			return string.Empty;
		}
	}

	private static string NormalizeAccountId(string? rawAccountId)
	{
		string text = rawAccountId?.Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}
		string text2 = text;
		if (!text2.StartsWith("Steam_", StringComparison.Ordinal))
		{
			return text2;
		}
		return text2.Substring("Steam_".Length);
	}

	private static string NormalizeOverrideAccountId(string? rawAccountId)
	{
		string text = NormalizeAccountId(rawAccountId);
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}
		if (!text.StartsWith("Steam_", StringComparison.Ordinal))
		{
			return text;
		}
		return text.Substring("Steam_".Length);
	}

	private static bool MatchesAccountId(string left, string right)
	{
		if (!string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right))
		{
			return string.Equals(NormalizeAccountId(left), NormalizeAccountId(right), StringComparison.Ordinal);
		}
		return false;
	}

	private static bool SameAccountId(string left, string right)
	{
		return string.Equals(NormalizeAccountId(left), NormalizeAccountId(right), StringComparison.Ordinal);
	}

	internal static bool TryPrepareManagedWardPrefabScan(out int scannedZdoCount)
	{
		scannedZdoCount = 0;
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return false;
		}
		ZDOMan instance = ZDOMan.instance;
		if (instance == null)
		{
			return false;
		}
		scannedZdoCount = PrepareManagedWardPrefabScan(instance);
		return true;
	}

	internal static ZDO? GetPreparedManagedWardPrefabScanEntry(int index)
	{
		if (index < 0 || index >= ManagedWardPrefabScanBuffer.Count)
		{
			return null;
		}
		return ManagedWardPrefabScanBuffer[index];
	}

	internal static bool TryBuildManagedWardScanEntries(List<ManagedWardScanEntry> scanEntries, out int scannedZdoCount)
	{
		scanEntries.Clear();
		if (!TryPrepareManagedWardPrefabScan(out scannedZdoCount))
		{
			return false;
		}
		for (int i = 0; i < scannedZdoCount; i++)
		{
			if (TryBuildManagedWardScanEntry(GetPreparedManagedWardPrefabScanEntry(i), out var scanEntry))
			{
				scanEntries.Add(scanEntry);
			}
		}
		return true;
	}

	private static bool TryBuildManagedWardScanEntry(ZDO? zdo, out ManagedWardScanEntry scanEntry)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		scanEntry = default(ManagedWardScanEntry);
		if (!IsManagedWardZdo(zdo))
		{
			return false;
		}
		long @long = zdo.GetLong(ZDOVars.s_creator, 0L);
		string accountId = NormalizeAccountIdValue(ResolveWardSteamAccountId(zdo, @long, GetWardSteamAccountId(zdo)));
		string ownerName = (WardPrivateAreaSafeAccess.GetCreatorName(zdo) ?? string.Empty).Trim();
		scanEntry = new ManagedWardScanEntry(zdo, zdo.m_uid, @long, accountId, ownerName, zdo.GetBool(ZDOVars.s_enabled, false), WardSettings.GetStoredRadius(zdo));
		return true;
	}

	private static bool ReloadOverrides(bool force)
	{
		if ((Object)(object)ZNet.instance != (Object)null && !ZNet.instance.IsServer())
		{
			return false;
		}
		IReadOnlyDictionary<string, int> wardLimitOverrides = ManagedWardConfigFileService.CurrentSnapshot.WardLimitOverrides;
		bool flag = force || WardLimitOverrides.Count != wardLimitOverrides.Count;
		if (!flag)
		{
			foreach (KeyValuePair<string, int> item in wardLimitOverrides)
			{
				if (!WardLimitOverrides.TryGetValue(item.Key, out var value) || value != item.Value)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		WardLimitOverrides.Clear();
		foreach (KeyValuePair<string, int> item2 in wardLimitOverrides)
		{
			WardLimitOverrides[item2.Key] = item2.Value;
		}
		return true;
	}

	internal static string GetReportFilePath()
	{
		return Path.Combine(Paths.ConfigPath, "STUWard.WardCountReport.yml");
	}

	private static string GetCurrentWorldName()
	{
		ZNet instance = ZNet.instance;
		string text = ((instance != null) ? instance.GetWorldName() : null);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text.Trim();
		}
		return "unknown_world";
	}

	private static long GetCurrentWorldUid()
	{
		ZNet instance = ZNet.instance;
		if (instance == null)
		{
			return 0L;
		}
		return instance.GetWorldUID();
	}

	internal static bool TryWriteWardCountReport(out string reportPath, out int trackedAccounts, out int totalWards, out int unresolvedOwners)
	{
		reportPath = GetReportFilePath();
		if (!TryBuildWardCountReport(out string reportContents, out trackedAccounts, out totalWards, out unresolvedOwners))
		{
			return false;
		}
		try
		{
			File.WriteAllText(reportPath, reportContents);
			return true;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Failed to write ward report file '" + reportPath + "': " + ex.Message));
			return false;
		}
	}

	internal static bool TryBuildWardCountReport(out string reportContents, out int trackedAccounts, out int totalWards, out int unresolvedOwners)
	{
		reportContents = string.Empty;
		trackedAccounts = 0;
		totalWards = 0;
		unresolvedOwners = 0;
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return false;
		}
		try
		{
			ReloadOverrides(force: false);
			List<ManagedWardScanEntry> list = new List<ManagedWardScanEntry>();
			if (!TryBuildManagedWardScanEntries(list, out var _))
			{
				return false;
			}
			List<KeyValuePair<string, int>> list2 = CollectReportWardCountsByAccount(list);
			list2.Sort(delegate(KeyValuePair<string, int> left, KeyValuePair<string, int> right)
			{
				int num = right.Value.CompareTo(left.Value);
				return (num == 0) ? string.CompareOrdinal(left.Key, right.Key) : num;
			});
			List<ManagedWardReportAccountEntry> list3 = new List<ManagedWardReportAccountEntry>(list2.Count);
			for (int i = 0; i < list2.Count; i++)
			{
				KeyValuePair<string, int> keyValuePair = list2[i];
				list3.Add(new ManagedWardReportAccountEntry(keyValuePair.Key, keyValuePair.Value, GetEffectiveWardLimitForAccount(keyValuePair.Key), WardLimitOverrides.ContainsKey(keyValuePair.Key)));
			}
			List<KeyValuePair<long, int>> unresolvedWardOwnerCounts = CollectUnresolvedWardOwnerCounts(list);
			List<KeyValuePair<long, int>> playerAccountMapGapCounts = CollectPlayerAccountMapGapCounts(list);
			if (!ManagedWardReportBuilder.TryBuild(new ManagedWardReportSnapshot("STUWard", DateTime.UtcNow, GetCurrentWorldName(), GetCurrentWorldUid(), list.Count, list3, unresolvedWardOwnerCounts, playerAccountMapGapCounts), out var result))
			{
				return false;
			}
			reportContents = result.Contents;
			trackedAccounts = result.TrackedAccounts;
			totalWards = result.TotalWards;
			unresolvedOwners = result.UnresolvedOwners;
			return true;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Failed to build ward report: " + ex.Message));
			return false;
		}
	}

	private static List<KeyValuePair<string, int>> CollectReportWardCountsByAccount(IReadOnlyList<ManagedWardScanEntry> scanEntries)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		for (int i = 0; i < scanEntries.Count; i++)
		{
			string accountId = scanEntries[i].AccountId;
			if (!string.IsNullOrWhiteSpace(accountId))
			{
				dictionary[accountId] = ((!dictionary.TryGetValue(accountId, out var value)) ? 1 : (value + 1));
			}
		}
		return new List<KeyValuePair<string, int>>(dictionary);
	}

	private static List<KeyValuePair<long, int>> CollectUnresolvedWardOwnerCounts(IReadOnlyList<ManagedWardScanEntry> scanEntries)
	{
		Dictionary<long, int> dictionary = new Dictionary<long, int>();
		for (int i = 0; i < scanEntries.Count; i++)
		{
			ManagedWardScanEntry managedWardScanEntry = scanEntries[i];
			if (string.IsNullOrWhiteSpace(managedWardScanEntry.AccountId))
			{
				long ownerPlayerId = managedWardScanEntry.OwnerPlayerId;
				dictionary[ownerPlayerId] = ((!dictionary.TryGetValue(ownerPlayerId, out var value)) ? 1 : (value + 1));
			}
		}
		return SortWardCountEntries(dictionary);
	}

	private static List<KeyValuePair<long, int>> CollectPlayerAccountMapGapCounts(IReadOnlyList<ManagedWardScanEntry> scanEntries)
	{
		Dictionary<long, int> dictionary = new Dictionary<long, int>();
		Dictionary<long, string> cachedPlayerAccountIds = new Dictionary<long, string>();
		for (int i = 0; i < scanEntries.Count; i++)
		{
			long ownerPlayerId = scanEntries[i].OwnerPlayerId;
			if (ownerPlayerId != 0L && string.IsNullOrWhiteSpace(GetCachedReportPlayerAccountId(ownerPlayerId, cachedPlayerAccountIds)))
			{
				dictionary[ownerPlayerId] = ((!dictionary.TryGetValue(ownerPlayerId, out var value)) ? 1 : (value + 1));
			}
		}
		return SortWardCountEntries(dictionary);
	}

	private static string GetCachedReportPlayerAccountId(long playerId, Dictionary<long, string> cachedPlayerAccountIds)
	{
		if (playerId == 0L)
		{
			return string.Empty;
		}
		if (cachedPlayerAccountIds.TryGetValue(playerId, out string value))
		{
			return value;
		}
		return cachedPlayerAccountIds[playerId] = GetPlayerAccountId(playerId);
	}

	private static List<KeyValuePair<long, int>> SortWardCountEntries(Dictionary<long, int> countsByCreatorId)
	{
		List<KeyValuePair<long, int>> list = new List<KeyValuePair<long, int>>(countsByCreatorId);
		list.Sort(delegate(KeyValuePair<long, int> left, KeyValuePair<long, int> right)
		{
			int num = right.Value.CompareTo(left.Value);
			return (num == 0) ? left.Key.CompareTo(right.Key) : num;
		});
		return list;
	}
}
