using System;
using System.Collections.Generic;
using Splatform;
using UnityEngine;
using UnityEngine.UI;

namespace STUWard;

internal static class WardMinimapPinsManager
{
	private enum ClientSnapshotState
	{
		Uninitialized,
		AwaitingFullSnapshot,
		Ready
	}

	private sealed class ServerViewerSyncState
	{
		internal readonly Dictionary<ZDOID, uint> VisibleWardDataRevisions = new Dictionary<ZDOID, uint>();

		internal int ViewerRevisionToken;

		internal bool HasSentFullSnapshot;
	}

	private enum WardPinsResponseKind
	{
		Unavailable,
		FullSnapshot,
		Unchanged
	}

	private const float DefaultSmallMapZoom = 0.01f;

	private static PinType WardIconPinType = (PinType)6;

	private static PinType WardRangePinType = (PinType)13;

	private static readonly TimeSpan RemoteSnapshotRequestInterval = TimeSpan.FromSeconds(0.5);

	private static readonly TimeSpan PendingSnapshotRetryInterval = TimeSpan.FromSeconds(2.0);

	private static readonly Dictionary<ZDOID, WardMinimapSnapshotEntry> LocalSnapshot = new Dictionary<ZDOID, WardMinimapSnapshotEntry>();

	private static readonly Dictionary<ZDOID, PinData> IconPins = new Dictionary<ZDOID, PinData>();

	private static readonly Dictionary<ZDOID, PinData> ActiveRangePins = new Dictionary<ZDOID, PinData>();

	private static bool _pendingForceRefresh;

	private static bool _loggedMissingWardIcon;

	private static bool _loggedHiddenWardIconPinType;

	private static bool _loggedHiddenWardRangePinType;

	private static bool _hasLastLoggedMapMode;

	private static bool? _lastCanSeeAllWards;

	private static ClientSnapshotState _snapshotState = ClientSnapshotState.Uninitialized;

	private static int _lastViewerRevisionToken;

	private static int _nextSnapshotRequestId;

	private static int _pendingSnapshotRequestId;

	private static DateTime _lastRemoteSnapshotRequestUtc = DateTime.MinValue;

	private static Minimap? _boundMinimap;

	private static Minimap? _customPinTypesMinimap;

	private static MapMode _lastLoggedMapMode;

	private static string? _lastDisplayDecision;

	private static string? _lastApplySummary;

	private static string? _lastPendingRefreshReason;

	private static string? _lastRemoteSnapshotBootstrapReason;

	private static string? _lastScanSummary;

	private static readonly TimeSpan ServerViewerRefreshDebounce = TimeSpan.FromMilliseconds(150.0);

	private static readonly Dictionary<long, ServerViewerSyncState> ServerViewerSyncStatesByPeerUid = new Dictionary<long, ServerViewerSyncState>();

	private static readonly HashSet<long> PendingServerViewerRefreshPeerUids = new HashSet<long>();

	private static bool _pendingServerViewerRefreshForAll;

	private static string? _lastServerViewerRefreshReason;

	private static DateTime _serverViewerRefreshFlushAtUtc = DateTime.MinValue;

	private const string RequestWardPinsRpc = "STUWard_RequestWardPins";

	private const string ReceiveWardPinsRpc = "STUWard_ReceiveWardPins";

	private const string PushWardPinsRpc = "STUWard_PushWardPins";

	private static bool _rpcsRegistered;

	internal static void ResetRuntimeState()
	{
		ResetClientRuntimeState();
		_rpcsRegistered = false;
		_pendingServerViewerRefreshForAll = false;
		_lastServerViewerRefreshReason = null;
		ServerViewerSyncStatesByPeerUid.Clear();
		PendingServerViewerRefreshPeerUids.Clear();
		_serverViewerRefreshFlushAtUtc = DateTime.MinValue;
		Plugin.LogWardDiagnosticVerbose("WardPins.State", "Reset ward minimap pin manager state after ZNet awake.");
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

	internal static void Update()
	{
		ProcessPendingServerViewerRefreshes();
	}

	internal static bool HasPendingRuntimeWork()
	{
		if (!_pendingServerViewerRefreshForAll)
		{
			return PendingServerViewerRefreshPeerUids.Count > 0;
		}
		return true;
	}

	internal static void NotifyWardDataMayHaveChanged(string reason, bool refreshImmediatelyIfVisible = false)
	{
		NotifyLocalWardDataMayHaveChanged(reason, refreshImmediatelyIfVisible);
		QueueServerViewerRefreshRecipients(null, reason);
	}

	private static void ResetClientRuntimeState()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		WardMapRangeSprites.Reset();
		ClearLocalPins(clearSnapshot: true);
		_pendingForceRefresh = false;
		_snapshotState = ClientSnapshotState.AwaitingFullSnapshot;
		_loggedMissingWardIcon = false;
		_loggedHiddenWardIconPinType = false;
		_loggedHiddenWardRangePinType = false;
		_hasLastLoggedMapMode = false;
		_lastCanSeeAllWards = null;
		_lastViewerRevisionToken = 0;
		_nextSnapshotRequestId = 0;
		_pendingSnapshotRequestId = 0;
		_lastRemoteSnapshotRequestUtc = DateTime.MinValue;
		_boundMinimap = null;
		_customPinTypesMinimap = null;
		WardIconPinType = (PinType)6;
		WardRangePinType = (PinType)13;
		_lastDisplayDecision = null;
		_lastApplySummary = null;
		_lastPendingRefreshReason = null;
		_lastRemoteSnapshotBootstrapReason = "znet awake";
		_lastScanSummary = null;
		LocalSnapshot.Clear();
		IconPins.Clear();
		ActiveRangePins.Clear();
	}

	internal static void HandleLocalConfigChanged()
	{
		WardMapRangeSprites.Reset();
		UpdateLocalState(Player.m_localPlayer, force: false, allowClosedMapRefresh: true);
		Plugin.LogWardDiagnosticVerbose("WardPins.State", $"Local ward pin config changed. pinScale={Plugin.WardMinimapPinScale?.Value}, ranges={Plugin.WardMinimapActiveRanges?.Value}");
	}

	internal static void NotifyLocalWardDataMayHaveChanged(string reason, bool refreshImmediatelyIfVisible = false)
	{
		QueueForceRefresh(reason);
		if (refreshImmediatelyIfVisible)
		{
			UpdateLocalState(Player.m_localPlayer, force: true, allowClosedMapRefresh: true);
		}
	}

	internal static void HandleMapModeChanged(Minimap? minimap, MapMode mode)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		if (!((Object)(object)minimap == (Object)null))
		{
			if (_boundMinimap != minimap)
			{
				_boundMinimap = minimap;
			}
			if (!_hasLastLoggedMapMode || _lastLoggedMapMode != mode)
			{
				_hasLastLoggedMapMode = true;
				_lastLoggedMapMode = mode;
				Plugin.LogWardDiagnosticVerbose("WardPins.State", $"Minimap map mode changed. mode={mode}");
			}
			if ((int)mode == 2)
			{
				UpdateLocalState(Player.m_localPlayer, force: true);
			}
		}
	}

	internal static void UpdateLocalState(Player? player, bool force = false, bool allowClosedMapRefresh = false)
	{
		if ((Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)Player.m_localPlayer)
		{
			return;
		}
		Minimap instance = Minimap.instance;
		if (_boundMinimap != instance)
		{
			Plugin.LogWardDiagnosticVerbose("WardPins.State", $"Minimap binding changed. hadBoundMinimap={(Object)(object)_boundMinimap != (Object)null}, hasMinimap={(Object)(object)instance != (Object)null}");
			ClearLocalPins(clearSnapshot: false);
			_boundMinimap = instance;
		}
		bool flag = WardAdminDebugAccess.IsPlayerAdminDebugController(player.GetPlayerID());
		if (!_lastCanSeeAllWards.HasValue || _lastCanSeeAllWards.Value != flag)
		{
			if (_lastCanSeeAllWards.GetValueOrDefault() && !flag)
			{
				ClearLocalPins(clearSnapshot: true);
			}
			_lastCanSeeAllWards = flag;
			Plugin.LogWardDiagnosticVerbose("WardPins.State", $"Admin debug visibility changed. playerId={player.GetPlayerID()}, canSeeAllWards={flag}");
			QueueRemoteSnapshotBootstrapRequest("admin debug visibility changed");
			_pendingForceRefresh = true;
			if (_lastPendingRefreshReason == null)
			{
				_lastPendingRefreshReason = "admin debug visibility changed";
			}
			force = force || IsLargeMapOpen(instance);
		}
		_ = _pendingForceRefresh;
		if (!ShouldDisplayPins(player, flag, out string reason))
		{
			LogDisplayDecision(player, instance, flag, shouldDisplay: false, reason);
			ClearLocalPins(clearSnapshot: false);
		}
		else
		{
			LogDisplayDecision(player, instance, flag, shouldDisplay: true, "display-enabled");
			RefreshLocalSnapshotIfNeeded(player, instance, flag, force, allowClosedMapRefresh);
			ApplySnapshotToMinimap(instance);
		}
	}

	internal static void UpdatePendingRemoteState(Player? player)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)Player.m_localPlayer) && IsLargeMapOpen(Minimap.instance) && (_pendingForceRefresh || _snapshotState != ClientSnapshotState.Ready || _pendingSnapshotRequestId != 0))
		{
			UpdateLocalState(player);
		}
	}

	private static bool ShouldDisplayPins(Player player, bool canSeeAllWards, out string reason)
	{
		bool num = GetWardPinScale() > 0;
		bool flag = Plugin.WardMinimapActiveRanges != null && Plugin.WardMinimapActiveRanges.Value == Plugin.Toggle.On;
		if (!num && !flag)
		{
			reason = "pins-and-ranges-config-off";
			return false;
		}
		if (Game.m_noMap && !canSeeAllWards)
		{
			reason = "nomap-blocked";
			return false;
		}
		if ((Object)(object)ZNet.instance == (Object)null)
		{
			reason = "znet-unavailable";
			return false;
		}
		if (ZDOMan.instance == null)
		{
			reason = "zdoman-unavailable";
			return false;
		}
		if ((Object)(object)player != (Object)(object)Player.m_localPlayer)
		{
			reason = "not-local-player";
			return false;
		}
		reason = "display-enabled";
		return true;
	}

	private static void RefreshLocalSnapshotIfNeeded(Player player, Minimap? minimap, bool canSeeAllWards, bool force, bool allowClosedMapRefresh)
	{
		if (allowClosedMapRefresh || IsLargeMapOpen(minimap))
		{
			if ((Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer())
			{
				RefreshLocalSnapshotFromLocalIndex(player, canSeeAllWards, force);
			}
			else
			{
				RequestRemoteSnapshotIfNeeded(player, canSeeAllWards, force);
			}
		}
	}

	private static void RefreshLocalSnapshotFromLocalIndex(Player player, bool canSeeAllWards, bool force)
	{
		if (!WardMinimapVisibilityIndex.TryPrepare(ZDOMan.instance, "ward minimap refresh"))
		{
			LocalSnapshot.Clear();
			_lastViewerRevisionToken = 0;
			QueueRemoteSnapshotBootstrapRequest("visibility index unavailable during local refresh");
			LogScanSummary("visibility-index-unavailable");
			return;
		}
		long playerID = player.GetPlayerID();
		int playerGuildId = GuildsCompat.GetPlayerGuildId(playerID);
		int viewerRevisionToken = WardMinimapVisibilityIndex.GetViewerRevisionToken(playerID, playerGuildId, canSeeAllWards);
		if (force || _pendingForceRefresh || _snapshotState != ClientSnapshotState.Ready || viewerRevisionToken != _lastViewerRevisionToken)
		{
			ClearPendingForceRefresh();
			_lastViewerRevisionToken = viewerRevisionToken;
			RebuildLocalSnapshot(playerID, playerGuildId, canSeeAllWards);
		}
	}

	private static void RequestRemoteSnapshotIfNeeded(Player player, bool canSeeAllWards, bool force)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		RegisterRpcs();
		DateTime utcNow = DateTime.UtcNow;
		bool flag = _pendingSnapshotRequestId != 0 && utcNow - _lastRemoteSnapshotRequestUtc >= PendingSnapshotRetryInterval;
		bool flag2 = force || _pendingForceRefresh || _snapshotState != ClientSnapshotState.Ready || _lastViewerRevisionToken == 0;
		if (!(flag2 || flag))
		{
			return;
		}
		TimeSpan timeSpan = (force ? RemoteSnapshotRequestInterval : ((_pendingSnapshotRequestId != 0) ? PendingSnapshotRetryInterval : RemoteSnapshotRequestInterval));
		if (utcNow - _lastRemoteSnapshotRequestUtc < timeSpan)
		{
			return;
		}
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (instance != null)
		{
			int num = (_pendingSnapshotRequestId = (_nextSnapshotRequestId = ((_nextSnapshotRequestId == int.MaxValue) ? 1 : (_nextSnapshotRequestId + 1))));
			_lastRemoteSnapshotRequestUtc = utcNow;
			ZPackage val = new ZPackage();
			val.Write(num);
			int num2 = ((!flag2) ? _lastViewerRevisionToken : 0);
			val.Write(num2);
			val.Write(flag2);
			if (flag2)
			{
				QueueRemoteSnapshotBootstrapRequest(force ? "forced remote snapshot request" : "remote snapshot request needs full snapshot");
			}
			instance.InvokeRoutedRPC("STUWard_RequestWardPins", new object[1] { val });
			if (Plugin.ShouldLogWardDiagnosticVerbose())
			{
				Plugin.LogWardDiagnosticVerbose("WardPins.Request", $"Requested remote ward minimap snapshot. requestId={num}, requestFullSnapshot={flag2}, snapshotState={_snapshotState}, knownViewerRevisionToken={num2}, localViewerRevisionToken={_lastViewerRevisionToken}, playerId={player.GetPlayerID()}, canSeeAllWards={canSeeAllWards}, force={force}, cachedSnapshotCount={LocalSnapshot.Count}, bootstrapReason='{_lastRemoteSnapshotBootstrapReason ?? string.Empty}'");
			}
		}
	}

	private static void RebuildLocalSnapshot(long playerId, int playerGuildId, bool canSeeAllWards)
	{
		WardMinimapViewerSnapshot wardMinimapViewerSnapshot = WardMinimapViewerSnapshotBuilder.Build(playerId, playerGuildId, canSeeAllWards, _lastViewerRevisionToken, includeEntries: true, includeVisibleWardDataRevisions: false);
		ReplaceLocalSnapshot(wardMinimapViewerSnapshot.Entries);
		_pendingSnapshotRequestId = 0;
		ClearPendingForceRefresh();
		ClearPendingRemoteSnapshotBootstrapRequest();
		if (Plugin.ShouldLogWardDiagnosticVerbose())
		{
			LogScanSummary($"playerId={playerId}, canSeeAllWards={canSeeAllWards}, indexedWardCount={wardMinimapViewerSnapshot.IndexedWardCount}, candidateWardCount={wardMinimapViewerSnapshot.CandidateWardCount}, visibleWardCount={wardMinimapViewerSnapshot.VisibleWardCount}, enabledWardCount={wardMinimapViewerSnapshot.EnabledWardCount}{DescribeFirstEntry(wardMinimapViewerSnapshot.FirstEntry)}");
		}
	}

	private static void ApplySnapshotToMinimap(Minimap? minimap)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)minimap == (Object)null)
		{
			LogApplySummary("minimap=null");
			return;
		}
		bool flag = GetWardPinScale() > 0;
		bool flag2 = Plugin.WardMinimapActiveRanges != null && Plugin.WardMinimapActiveRanges.Value == Plugin.Toggle.On;
		Sprite pieceIcon = StuWardPrefab.GetPieceIcon();
		Sprite rangeIcon = (flag2 ? GetRepresentativeRangeSprite() : null);
		EnsureCustomPinTypes(minimap, pieceIcon, rangeIcon);
		if (flag && (Object)(object)pieceIcon == (Object)null)
		{
			if (!_loggedMissingWardIcon)
			{
				_loggedMissingWardIcon = true;
				Plugin.LogWardDiagnosticFailure("WardPins.Icon", "Could not resolve piece_stuward icon sprite for minimap pins.");
			}
		}
		else
		{
			_loggedMissingWardIcon = false;
		}
		bool? pinTypeVisibility = GetPinTypeVisibility(minimap, WardIconPinType);
		LogHiddenPinTypeIfNeeded("Ward icon", WardIconPinType, flag && LocalSnapshot.Count > 0, pinTypeVisibility, ref _loggedHiddenWardIconPinType);
		bool? pinTypeVisibility2 = GetPinTypeVisibility(minimap, WardRangePinType);
		LogHiddenPinTypeIfNeeded("Ward active range", WardRangePinType, flag2 && LocalSnapshot.Count > 0, pinTypeVisibility2, ref _loggedHiddenWardRangePinType);
		HashSet<ZDOID> hashSet = new HashSet<ZDOID>();
		int num = 0;
		foreach (WardMinimapSnapshotEntry value in LocalSnapshot.Values)
		{
			hashSet.Add(value.ZdoId);
			if (flag)
			{
				UpsertIconPin(minimap, value, pieceIcon);
			}
			else
			{
				RemoveTrackedPin(minimap, IconPins, value.ZdoId);
			}
			if (flag2 && value.IsEnabled)
			{
				num++;
				UpsertActiveRangePin(minimap, value);
			}
			else
			{
				RemoveTrackedPin(minimap, ActiveRangePins, value.ZdoId);
			}
		}
		RemoveMissingPins(minimap, IconPins, hashSet);
		RemoveMissingPins(minimap, ActiveRangePins, hashSet);
		if (Plugin.ShouldLogWardDiagnosticVerbose())
		{
			LogApplySummary($"snapshotCount={LocalSnapshot.Count}, enabledWardCount={num}, iconPins={IconPins.Count}, rangePins={ActiveRangePins.Count}, showIconPins={flag}, showActiveRanges={flag2}, iconResolved={(Object)(object)pieceIcon != (Object)null}, iconPinTypeVisible={FormatNullableBool(pinTypeVisibility)}, rangePinTypeVisible={FormatNullableBool(pinTypeVisibility2)}");
		}
	}

	private static void UpsertIconPin(Minimap minimap, WardMinimapSnapshotEntry entry, Sprite? wardIcon)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		PinData value;
		bool flag2 = !IconPins.TryGetValue(entry.ZdoId, out value);
		if (!flag2 && !IsTrackedPinOnMinimap(minimap, value))
		{
			flag2 = true;
		}
		if (flag2)
		{
			value = minimap.AddPin(entry.Position, WardIconPinType, string.Empty, false, false, 0L, default(PlatformUserID));
			IconPins[entry.ZdoId] = value;
			flag = true;
		}
		if (value.m_pos != entry.Position)
		{
			value.m_pos = entry.Position;
			flag = true;
		}
		if (!value.m_doubleSize)
		{
			value.m_doubleSize = true;
			flag = true;
		}
		float iconWorldSize = GetIconWorldSize(minimap);
		if (iconWorldSize > 0f && !Mathf.Approximately(value.m_worldSize, iconWorldSize))
		{
			value.m_worldSize = iconWorldSize;
			flag = true;
		}
		if ((Object)(object)wardIcon != (Object)null && (Object)(object)value.m_icon != (Object)(object)wardIcon)
		{
			value.m_icon = wardIcon;
			flag = true;
		}
		if ((Object)(object)value.m_iconElement != (Object)null && (Object)(object)value.m_iconElement.sprite != (Object)(object)value.m_icon)
		{
			value.m_iconElement.sprite = value.m_icon;
			flag = true;
		}
		if (flag)
		{
			minimap.m_pinUpdateRequired = true;
		}
	}

	private static void UpsertActiveRangePin(Minimap minimap, WardMinimapSnapshotEntry entry)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(0f, entry.Radius * 2f);
		Sprite rangeSprite = WardMapRangeSprites.GetRangeSprite(entry.Radius);
		bool flag = false;
		PinData value;
		bool flag2 = !ActiveRangePins.TryGetValue(entry.ZdoId, out value);
		if (!flag2 && !IsTrackedPinOnMinimap(minimap, value))
		{
			flag2 = true;
		}
		if (flag2)
		{
			value = minimap.AddPin(entry.Position, WardRangePinType, string.Empty, false, false, 0L, default(PlatformUserID));
			ActiveRangePins[entry.ZdoId] = value;
			flag = true;
		}
		else if (!Mathf.Approximately(value.m_worldSize, num))
		{
			minimap.RemovePin(value);
			value = minimap.AddPin(entry.Position, WardRangePinType, string.Empty, false, false, 0L, default(PlatformUserID));
			ActiveRangePins[entry.ZdoId] = value;
			flag = true;
		}
		if (value.m_pos != entry.Position)
		{
			value.m_pos = entry.Position;
			flag = true;
		}
		if (!Mathf.Approximately(value.m_worldSize, num))
		{
			value.m_worldSize = num;
			flag = true;
		}
		if ((Object)(object)rangeSprite != (Object)null && (Object)(object)value.m_icon != (Object)(object)rangeSprite)
		{
			value.m_icon = rangeSprite;
			flag = true;
		}
		if ((Object)(object)value.m_iconElement != (Object)null && (Object)(object)value.m_iconElement.sprite != (Object)(object)value.m_icon)
		{
			value.m_iconElement.sprite = value.m_icon;
			flag = true;
		}
		if (flag)
		{
			minimap.m_pinUpdateRequired = true;
		}
	}

	private static bool IsTrackedPinOnMinimap(Minimap minimap, PinData pin)
	{
		if (pin != null && minimap.m_pins != null)
		{
			return minimap.m_pins.Contains(pin);
		}
		return false;
	}

	private static void RemoveMissingPins(Minimap minimap, Dictionary<ZDOID, PinData> pins, HashSet<ZDOID> seenWardIds)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		List<ZDOID> list = null;
		foreach (KeyValuePair<ZDOID, PinData> pin in pins)
		{
			if (!seenWardIds.Contains(pin.Key))
			{
				if (list == null)
				{
					list = new List<ZDOID>();
				}
				list.Add(pin.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				RemoveTrackedPin(minimap, pins, list[i]);
			}
		}
	}

	private static void RemoveTrackedPin(Minimap minimap, Dictionary<ZDOID, PinData> pins, ZDOID wardId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (pins.Remove(wardId, out PinData value))
		{
			minimap.RemovePin(value);
		}
	}

	private static void ClearLocalPins(bool clearSnapshot)
	{
		int count = IconPins.Count;
		int count2 = ActiveRangePins.Count;
		int num = (clearSnapshot ? LocalSnapshot.Count : 0);
		if ((Object)(object)_boundMinimap != (Object)null)
		{
			foreach (PinData value in IconPins.Values)
			{
				_boundMinimap.RemovePin(value);
			}
			foreach (PinData value2 in ActiveRangePins.Values)
			{
				_boundMinimap.RemovePin(value2);
			}
		}
		IconPins.Clear();
		ActiveRangePins.Clear();
		if (count > 0 || count2 > 0 || num > 0)
		{
			Plugin.LogWardDiagnosticVerbose("WardPins.State", $"Cleared local ward pins. clearSnapshot={clearSnapshot}, removedIconPins={count}, removedRangePins={count2}, clearedSnapshotCount={num}");
		}
		if (clearSnapshot)
		{
			_lastViewerRevisionToken = 0;
			_pendingSnapshotRequestId = 0;
			LocalSnapshot.Clear();
			QueueRemoteSnapshotBootstrapRequest("local snapshot cleared");
		}
	}

	private static void ReplaceLocalSnapshot(IReadOnlyList<WardMinimapSnapshotEntry> snapshotEntries)
	{
		LocalSnapshot.Clear();
		UpsertLocalSnapshotEntries(snapshotEntries);
	}

	private static void UpsertLocalSnapshotEntries(IReadOnlyList<WardMinimapSnapshotEntry> snapshotEntries)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < snapshotEntries.Count; i++)
		{
			WardMinimapSnapshotEntry value = snapshotEntries[i];
			LocalSnapshot[value.ZdoId] = value;
		}
	}

	private static void ApplyLocalSnapshotDelta(IReadOnlyList<WardMinimapSnapshotEntry> snapshotEntries, IReadOnlyList<ZDOID> removedWardIds)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < removedWardIds.Count; i++)
		{
			LocalSnapshot.Remove(removedWardIds[i]);
		}
		UpsertLocalSnapshotEntries(snapshotEntries);
	}

	private static void QueueForceRefresh(string reason)
	{
		if (_pendingForceRefresh)
		{
			if (_lastPendingRefreshReason == null)
			{
				_lastPendingRefreshReason = reason;
			}
			return;
		}
		_pendingForceRefresh = true;
		_lastPendingRefreshReason = reason;
		if (Plugin.ShouldLogWardDiagnosticVerbose())
		{
			Plugin.LogWardDiagnosticVerbose("WardPins.State", "Queued ward minimap rescan for next large map open. reason='" + reason + "'");
		}
	}

	private static void QueueRemoteSnapshotBootstrapRequest(string reason)
	{
		_snapshotState = ClientSnapshotState.AwaitingFullSnapshot;
		if (!string.IsNullOrWhiteSpace(reason))
		{
			_lastRemoteSnapshotBootstrapReason = reason;
		}
	}

	private static void ClearPendingRemoteSnapshotBootstrapRequest()
	{
		_snapshotState = ClientSnapshotState.Ready;
		_lastRemoteSnapshotBootstrapReason = null;
	}

	private static void ClearPendingForceRefresh()
	{
		_pendingForceRefresh = false;
		_lastPendingRefreshReason = null;
	}

	private static void LogDisplayDecision(Player player, Minimap? minimap, bool canSeeAllWards, bool shouldDisplay, string reason)
	{
		if (Plugin.ShouldLogWardDiagnosticVerbose())
		{
			string text = $"shouldDisplay={shouldDisplay}, reason={reason}, playerId={player.GetPlayerID()}, canSeeAllWards={canSeeAllWards}, noMap={Game.m_noMap}, pinScale={Plugin.WardMinimapPinScale?.Value}, rangesConfig={Plugin.WardMinimapActiveRanges?.Value}, hasMinimap={(Object)(object)minimap != (Object)null}, hasZNet={(Object)(object)ZNet.instance != (Object)null}, hasZdoMan={ZDOMan.instance != null}";
			if (!(text == _lastDisplayDecision))
			{
				_lastDisplayDecision = text;
				Plugin.LogWardDiagnosticVerbose("WardPins.Display", text);
			}
		}
	}

	private static void LogApplySummary(string summary)
	{
		if (Plugin.ShouldLogWardDiagnosticVerbose() && !(summary == _lastApplySummary))
		{
			_lastApplySummary = summary;
			Plugin.LogWardDiagnosticVerbose("WardPins.Apply", summary);
		}
	}

	private static void LogScanSummary(string summary)
	{
		if (Plugin.ShouldLogWardDiagnosticVerbose() && !(summary == _lastScanSummary))
		{
			_lastScanSummary = summary;
			Plugin.LogWardDiagnosticVerbose("WardPins.Scan", summary);
		}
	}

	private static bool? GetPinTypeVisibility(Minimap minimap, PinType pinType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		int num = (int)pinType;
		bool[] visibleIconTypes = minimap.m_visibleIconTypes;
		if (visibleIconTypes == null || num < 0 || num >= visibleIconTypes.Length)
		{
			return null;
		}
		return visibleIconTypes[num];
	}

	private static void EnsureCustomPinTypes(Minimap minimap, Sprite? wardIcon, Sprite? rangeIcon)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (minimap.m_visibleIconTypes != null && minimap.m_icons != null)
		{
			if (_customPinTypesMinimap != minimap || !IsValidPinType(minimap, WardIconPinType) || !IsValidPinType(minimap, WardRangePinType) || !HasSpriteData(minimap, WardIconPinType) || !HasSpriteData(minimap, WardRangePinType))
			{
				WardIconPinType = AddCustomPinType(minimap, wardIcon);
				WardRangePinType = AddCustomPinType(minimap, rangeIcon ?? wardIcon);
				_customPinTypesMinimap = minimap;
				Plugin.LogWardDiagnosticVerbose("WardPins.Icon", $"Registered STUWard minimap pin types. iconPinType={WardIconPinType}, rangePinType={WardRangePinType}, visibleIconTypeCount={minimap.m_visibleIconTypes.Length}");
			}
			else
			{
				UpdateCustomPinTypeSprite(minimap, WardIconPinType, wardIcon);
				UpdateCustomPinTypeSprite(minimap, WardRangePinType, rangeIcon ?? wardIcon);
			}
		}
	}

	private static PinType AddCustomPinType(Minimap minimap, Sprite? icon)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		int num = minimap.m_visibleIconTypes.Length;
		PinType val = (PinType)num;
		ExpandVisibleIconTypes(minimap, num + 1);
		minimap.m_visibleIconTypes[num] = true;
		UpdateCustomPinTypeSprite(minimap, val, icon);
		return val;
	}

	private static void ExpandVisibleIconTypes(Minimap minimap, int requiredLength)
	{
		bool[] visibleIconTypes = minimap.m_visibleIconTypes;
		if (visibleIconTypes.Length < requiredLength)
		{
			bool[] array = new bool[requiredLength];
			Array.Copy(visibleIconTypes, array, visibleIconTypes.Length);
			for (int i = visibleIconTypes.Length; i < array.Length; i++)
			{
				array[i] = true;
			}
			minimap.m_visibleIconTypes = array;
		}
	}

	private static void UpdateCustomPinTypeSprite(Minimap minimap, PinType pinType, Sprite? icon)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		List<SpriteData> icons = minimap.m_icons;
		if (icons == null)
		{
			return;
		}
		for (int i = 0; i < icons.Count; i++)
		{
			SpriteData val = icons[i];
			if (val.m_name == pinType)
			{
				if ((Object)(object)icon != (Object)null && (Object)(object)val.m_icon != (Object)(object)icon)
				{
					val.m_icon = icon;
					icons[i] = val;
				}
				return;
			}
		}
		icons.Add(new SpriteData
		{
			m_name = pinType,
			m_icon = icon
		});
	}

	private static bool IsValidPinType(Minimap minimap, PinType pinType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		int num = (int)pinType;
		if (minimap.m_visibleIconTypes != null && num >= 0)
		{
			return num < minimap.m_visibleIconTypes.Length;
		}
		return false;
	}

	private static bool HasSpriteData(Minimap minimap, PinType pinType)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		List<SpriteData> icons = minimap.m_icons;
		if (icons == null)
		{
			return false;
		}
		for (int i = 0; i < icons.Count; i++)
		{
			if (icons[i].m_name == pinType)
			{
				return true;
			}
		}
		return false;
	}

	private static void LogHiddenPinTypeIfNeeded(string label, PinType pinType, bool shouldBeVisible, bool? visible, ref bool loggedHidden)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!shouldBeVisible || visible != false)
		{
			loggedHidden = false;
		}
		else if (!loggedHidden)
		{
			loggedHidden = true;
			Plugin.LogWardDiagnosticFailure("WardPins.Icon", $"{label} pin type {pinType} is currently hidden by minimap icon filters.");
		}
	}

	private static Sprite? GetRepresentativeRangeSprite()
	{
		foreach (WardMinimapSnapshotEntry value in LocalSnapshot.Values)
		{
			if (value.IsEnabled)
			{
				return WardMapRangeSprites.GetRangeSprite(value.Radius);
			}
		}
		return null;
	}

	private static string DescribeFirstEntry(WardMinimapSnapshotEntry? firstEntry)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!firstEntry.HasValue)
		{
			return string.Empty;
		}
		WardMinimapSnapshotEntry value = firstEntry.Value;
		return $", firstWard=zdoId={value.ZdoId}, position={value.Position}, radius={value.Radius:F1}, enabled={value.IsEnabled}";
	}

	private static string FormatNullableBool(bool? value)
	{
		if (!value.HasValue)
		{
			return "n/a";
		}
		return value.Value.ToString();
	}

	private static float GetIconWorldSize(Minimap minimap)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		float num = minimap.m_pinSizeSmall * 2f;
		RawImage val = (((Object)(object)minimap.m_mapImageSmall != (Object)null) ? minimap.m_mapImageSmall : minimap.m_mapImageLarge);
		if (num <= 0f || (Object)(object)val == (Object)null)
		{
			return 0f;
		}
		float num2 = 0.01f;
		Rect rect = ((Graphic)val).rectTransform.rect;
		float height = ((Rect)(ref rect)).height;
		if (num2 <= 0f || height <= 0f)
		{
			return 0f;
		}
		int num3 = Mathf.Clamp(GetWardPinScale(), 0, 100);
		return num * (float)num3 * minimap.m_pixelSize * (float)minimap.m_textureSize * num2 / height;
	}

	private static int GetWardPinScale()
	{
		if (Plugin.WardMinimapPinScale == null)
		{
			return 1;
		}
		return Plugin.WardMinimapPinScale.Value;
	}

	private static bool IsLargeMapOpen(Minimap? minimap)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((Object)(object)minimap != (Object)null)
		{
			return (int)minimap.m_mode == 2;
		}
		return false;
	}

	internal static void QueueServerViewerRefreshRecipients(HashSet<long>? recipientPeerUids, string reason)
	{
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return;
		}
		if (recipientPeerUids == null)
		{
			_pendingServerViewerRefreshForAll = true;
			PendingServerViewerRefreshPeerUids.Clear();
		}
		else if (!_pendingServerViewerRefreshForAll)
		{
			foreach (long recipientPeerUid in recipientPeerUids)
			{
				if (recipientPeerUid != 0L)
				{
					PendingServerViewerRefreshPeerUids.Add(recipientPeerUid);
				}
			}
		}
		_lastServerViewerRefreshReason = (string.IsNullOrWhiteSpace(reason) ? _lastServerViewerRefreshReason : reason);
		if (_serverViewerRefreshFlushAtUtc == DateTime.MinValue)
		{
			_serverViewerRefreshFlushAtUtc = DateTime.UtcNow + ServerViewerRefreshDebounce;
		}
	}

	private static void ProcessPendingServerViewerRefreshes()
	{
		if ((!_pendingServerViewerRefreshForAll && PendingServerViewerRefreshPeerUids.Count == 0) || (Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer() || _serverViewerRefreshFlushAtUtc > DateTime.UtcNow)
		{
			return;
		}
		List<ZNetPeer> peers = ZNet.instance.GetPeers();
		HashSet<long> hashSet = new HashSet<long>();
		HashSet<long> hashSet2 = new HashSet<long>();
		if (peers != null)
		{
			for (int i = 0; i < peers.Count; i++)
			{
				ZNetPeer val = peers[i];
				if (val != null && val.m_uid != 0L)
				{
					hashSet.Add(val.m_uid);
					long playerIdFromSender = WardOwnership.GetPlayerIdFromSender(val.m_uid);
					if (playerIdFromSender != 0L)
					{
						hashSet2.Add(playerIdFromSender);
					}
				}
			}
		}
		AddActivePlayerIds(hashSet2);
		PruneServerViewerSyncStates(hashSet);
		if (hashSet2.Count > 0 || hashSet.Count == 0)
		{
			WardMinimapVisibilityIndex.PruneViewerCaches(hashSet2);
		}
		List<long> list = new List<long>();
		if (_pendingServerViewerRefreshForAll)
		{
			list.AddRange(hashSet);
		}
		else
		{
			foreach (long pendingServerViewerRefreshPeerUid in PendingServerViewerRefreshPeerUids)
			{
				if (hashSet.Contains(pendingServerViewerRefreshPeerUid))
				{
					list.Add(pendingServerViewerRefreshPeerUid);
				}
			}
		}
		string reason = (string.IsNullOrWhiteSpace(_lastServerViewerRefreshReason) ? "ward minimap server refresh" : _lastServerViewerRefreshReason);
		_pendingServerViewerRefreshForAll = false;
		PendingServerViewerRefreshPeerUids.Clear();
		_lastServerViewerRefreshReason = null;
		_serverViewerRefreshFlushAtUtc = DateTime.MinValue;
		if (list.Count != 0 && WardMinimapVisibilityIndex.TryPrepare(ZDOMan.instance, reason))
		{
			for (int j = 0; j < list.Count; j++)
			{
				PushWardPinsUpdateToPeer(list[j]);
			}
		}
	}

	private static void AddActivePlayerIds(HashSet<long> activePlayerIds)
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		if (allPlayers == null || allPlayers.Count == 0)
		{
			return;
		}
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player val = allPlayers[i];
			if (!((Object)(object)val == (Object)null))
			{
				long playerID = val.GetPlayerID();
				if (playerID != 0L)
				{
					activePlayerIds.Add(playerID);
				}
			}
		}
	}

	private static void PruneServerViewerSyncStates(HashSet<long> livePeerUids)
	{
		List<long> list = null;
		foreach (KeyValuePair<long, ServerViewerSyncState> item in ServerViewerSyncStatesByPeerUid)
		{
			if (!livePeerUids.Contains(item.Key))
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
			for (int i = 0; i < list.Count; i++)
			{
				ServerViewerSyncStatesByPeerUid.Remove(list[i]);
			}
		}
	}

	private static void PushWardPinsUpdateToPeer(long receiverUid)
	{
		if (TryBuildServerViewerSyncUpdate(receiverUid, out bool fullSnapshot, out int viewerRevisionToken, out long playerId, out bool canSeeAllWards, out int indexedWardCount, out int candidateWardCount, out int visibleWardCount, out int enabledWardCount, out IReadOnlyList<WardMinimapSnapshotEntry> snapshotEntries, out IReadOnlyList<ZDOID> removedWardIds, out WardMinimapSnapshotEntry? firstEntry))
		{
			SendWardPinsPush(receiverUid, fullSnapshot, viewerRevisionToken, playerId, canSeeAllWards, indexedWardCount, candidateWardCount, visibleWardCount, enabledWardCount, snapshotEntries, removedWardIds, firstEntry);
		}
	}

	private static bool TryBuildServerViewerSyncUpdate(long receiverUid, out bool fullSnapshot, out int viewerRevisionToken, out long playerId, out bool canSeeAllWards, out int indexedWardCount, out int candidateWardCount, out int visibleWardCount, out int enabledWardCount, out IReadOnlyList<WardMinimapSnapshotEntry> snapshotEntries, out IReadOnlyList<ZDOID> removedWardIds, out WardMinimapSnapshotEntry? firstEntry)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		fullSnapshot = false;
		viewerRevisionToken = 0;
		playerId = 0L;
		canSeeAllWards = false;
		indexedWardCount = 0;
		candidateWardCount = 0;
		visibleWardCount = 0;
		enabledWardCount = 0;
		snapshotEntries = Array.Empty<WardMinimapSnapshotEntry>();
		removedWardIds = Array.Empty<ZDOID>();
		firstEntry = null;
		playerId = WardOwnership.GetPlayerIdFromSender(receiverUid);
		if (playerId == 0L)
		{
			Plugin.LogWardDiagnosticFailure("WardPins.Push", $"Skipped ward minimap push because the receiver player id could not be resolved. receiverUid={receiverUid}");
			return false;
		}
		canSeeAllWards = playerId != 0L && WardAdminDebugAccess.IsPlayerAdminDebugController(playerId);
		int playerGuildId = GuildsCompat.GetPlayerGuildId(playerId);
		viewerRevisionToken = WardMinimapVisibilityIndex.GetViewerRevisionToken(playerId, playerGuildId, canSeeAllWards);
		WardMinimapViewerSnapshot wardMinimapViewerSnapshot = WardMinimapViewerSnapshotBuilder.Build(playerId, playerGuildId, canSeeAllWards, viewerRevisionToken, includeEntries: true, includeVisibleWardDataRevisions: true);
		indexedWardCount = wardMinimapViewerSnapshot.IndexedWardCount;
		candidateWardCount = wardMinimapViewerSnapshot.CandidateWardCount;
		visibleWardCount = wardMinimapViewerSnapshot.VisibleWardCount;
		enabledWardCount = wardMinimapViewerSnapshot.EnabledWardCount;
		firstEntry = wardMinimapViewerSnapshot.FirstEntry;
		ServerViewerSyncState orCreateServerViewerSyncState = GetOrCreateServerViewerSyncState(receiverUid);
		if (orCreateServerViewerSyncState.HasSentFullSnapshot && orCreateServerViewerSyncState.ViewerRevisionToken == viewerRevisionToken)
		{
			return false;
		}
		List<WardMinimapSnapshotEntry> list = null;
		for (int i = 0; i < wardMinimapViewerSnapshot.Entries.Count; i++)
		{
			WardMinimapSnapshotEntry item = wardMinimapViewerSnapshot.Entries[i];
			uint value;
			uint num = (wardMinimapViewerSnapshot.VisibleWardDataRevisions.TryGetValue(item.ZdoId, out value) ? value : 0u);
			if (!orCreateServerViewerSyncState.HasSentFullSnapshot || !orCreateServerViewerSyncState.VisibleWardDataRevisions.TryGetValue(item.ZdoId, out var value2) || value2 != num)
			{
				if (list == null)
				{
					list = new List<WardMinimapSnapshotEntry>();
				}
				list.Add(item);
			}
		}
		List<ZDOID> list2 = null;
		if (orCreateServerViewerSyncState.HasSentFullSnapshot && orCreateServerViewerSyncState.VisibleWardDataRevisions.Count > 0)
		{
			foreach (KeyValuePair<ZDOID, uint> visibleWardDataRevision in orCreateServerViewerSyncState.VisibleWardDataRevisions)
			{
				if (!wardMinimapViewerSnapshot.VisibleWardDataRevisions.ContainsKey(visibleWardDataRevision.Key))
				{
					if (list2 == null)
					{
						list2 = new List<ZDOID>();
					}
					list2.Add(visibleWardDataRevision.Key);
				}
			}
		}
		int num2 = list?.Count ?? 0;
		int num3 = list2?.Count ?? 0;
		fullSnapshot = !orCreateServerViewerSyncState.HasSentFullSnapshot || ShouldSendFullSnapshot(num2, num3, wardMinimapViewerSnapshot.VisibleWardCount);
		if (!fullSnapshot && num2 == 0 && num3 == 0)
		{
			UpdateServerViewerSyncState(orCreateServerViewerSyncState, viewerRevisionToken, wardMinimapViewerSnapshot.VisibleWardDataRevisions);
			return false;
		}
		IReadOnlyList<WardMinimapSnapshotEntry> readOnlyList2;
		if (!fullSnapshot)
		{
			if (list != null && list.Count != 0)
			{
				IReadOnlyList<WardMinimapSnapshotEntry> readOnlyList = list;
				readOnlyList2 = readOnlyList;
			}
			else
			{
				IReadOnlyList<WardMinimapSnapshotEntry> readOnlyList = Array.Empty<WardMinimapSnapshotEntry>();
				readOnlyList2 = readOnlyList;
			}
		}
		else
		{
			readOnlyList2 = wardMinimapViewerSnapshot.Entries;
		}
		snapshotEntries = readOnlyList2;
		IReadOnlyList<ZDOID> readOnlyList4;
		if (!fullSnapshot && list2 != null && list2.Count != 0)
		{
			IReadOnlyList<ZDOID> readOnlyList3 = list2;
			readOnlyList4 = readOnlyList3;
		}
		else
		{
			IReadOnlyList<ZDOID> readOnlyList3 = Array.Empty<ZDOID>();
			readOnlyList4 = readOnlyList3;
		}
		removedWardIds = readOnlyList4;
		UpdateServerViewerSyncState(orCreateServerViewerSyncState, viewerRevisionToken, wardMinimapViewerSnapshot.VisibleWardDataRevisions);
		return true;
	}

	private static ServerViewerSyncState GetOrCreateServerViewerSyncState(long receiverUid)
	{
		if (!ServerViewerSyncStatesByPeerUid.TryGetValue(receiverUid, out ServerViewerSyncState value))
		{
			value = new ServerViewerSyncState();
			ServerViewerSyncStatesByPeerUid[receiverUid] = value;
		}
		return value;
	}

	private static void UpdateServerViewerSyncState(ServerViewerSyncState syncState, int viewerRevisionToken, IReadOnlyDictionary<ZDOID, uint> currentVisibleWardRevisions)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		syncState.VisibleWardDataRevisions.Clear();
		foreach (KeyValuePair<ZDOID, uint> currentVisibleWardRevision in currentVisibleWardRevisions)
		{
			syncState.VisibleWardDataRevisions[currentVisibleWardRevision.Key] = currentVisibleWardRevision.Value;
		}
		syncState.ViewerRevisionToken = viewerRevisionToken;
		syncState.HasSentFullSnapshot = true;
	}

	private static bool ShouldSendFullSnapshot(int changedEntryCount, int removedEntryCount, int visibleWardCount)
	{
		if (visibleWardCount <= 0)
		{
			return removedEntryCount > 0;
		}
		return changedEntryCount + removedEntryCount >= visibleWardCount;
	}

	private static void TrackServerViewerSyncState(long receiverUid, WardMinimapViewerSnapshot snapshot)
	{
		if (receiverUid != 0L && snapshot.ViewerRevisionToken != 0)
		{
			UpdateServerViewerSyncState(GetOrCreateServerViewerSyncState(receiverUid), snapshot.ViewerRevisionToken, snapshot.VisibleWardDataRevisions);
		}
	}

	internal static void RegisterRpcs()
	{
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (!_rpcsRegistered && instance != null)
		{
			instance.Register<ZPackage>("STUWard_RequestWardPins", (Action<long, ZPackage>)HandleRequestWardPins);
			instance.Register<ZPackage>("STUWard_ReceiveWardPins", (Action<long, ZPackage>)HandleReceiveWardPins);
			instance.Register<ZPackage>("STUWard_PushWardPins", (Action<long, ZPackage>)HandlePushWardPins);
			_rpcsRegistered = true;
		}
	}

	private static void HandleRequestWardPins(long sender, ZPackage pkg)
	{
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		bool flag = true;
		try
		{
			num = ((pkg != null) ? pkg.ReadInt() : 0);
		}
		catch
		{
			num = 0;
		}
		if (num > 0 && pkg != null)
		{
			try
			{
				num2 = pkg.ReadInt();
			}
			catch
			{
				num2 = 0;
			}
			flag = num2 == 0;
			try
			{
				flag = pkg.ReadBool();
			}
			catch
			{
			}
		}
		if (WardOwnership.TryResolveAuthoritativePlayerIdFromSender(sender, "WardPins.Request", out var playerId))
		{
			bool canSeeAllWards = WardAdminDebugAccess.IsPlayerAdminDebugController(playerId);
			int playerGuildId = GuildsCompat.GetPlayerGuildId(playerId);
			bool num3 = WardMinimapVisibilityIndex.TryPrepare(ZDOMan.instance, "ward minimap remote request");
			WardPinsResponseKind responseKind = WardPinsResponseKind.Unavailable;
			WardMinimapViewerSnapshot snapshot = WardMinimapViewerSnapshot.Empty;
			if (num3)
			{
				int viewerRevisionToken = WardMinimapVisibilityIndex.GetViewerRevisionToken(playerId, playerGuildId, canSeeAllWards);
				bool flag2 = flag || num2 == 0 || viewerRevisionToken != num2;
				snapshot = WardMinimapViewerSnapshotBuilder.Build(playerId, playerGuildId, canSeeAllWards, viewerRevisionToken, flag2, includeVisibleWardDataRevisions: true);
				responseKind = (flag2 ? WardPinsResponseKind.FullSnapshot : WardPinsResponseKind.Unchanged);
				TrackServerViewerSyncState(sender, snapshot);
			}
			SendWardPinsResponse(sender, num, responseKind, playerId, canSeeAllWards, snapshot);
		}
	}

	private static void SendWardPinsResponse(long receiverUid, int requestId, WardPinsResponseKind responseKind, long playerId, bool canSeeAllWards, WardMinimapViewerSnapshot snapshot)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (instance != null && receiverUid != 0L)
		{
			ZPackage val = new ZPackage();
			val.Write(requestId);
			val.Write((int)responseKind);
			val.Write(snapshot.ViewerRevisionToken);
			val.Write(playerId);
			val.Write(canSeeAllWards);
			val.Write(snapshot.IndexedWardCount);
			val.Write(snapshot.CandidateWardCount);
			val.Write(snapshot.VisibleWardCount);
			val.Write(snapshot.EnabledWardCount);
			val.Write(snapshot.Entries.Count);
			WriteSnapshotEntries(val, snapshot.Entries);
			instance.InvokeRoutedRPC(receiverUid, "STUWard_ReceiveWardPins", new object[1] { val });
			Plugin.LogWardDiagnosticVerbose("WardPins.Response", $"Sent ward minimap snapshot response. requestId={requestId}, receiverUid={receiverUid}, responseKind={responseKind}, viewerRevisionToken={snapshot.ViewerRevisionToken}, playerId={playerId}, canSeeAllWards={canSeeAllWards}, indexedWardCount={snapshot.IndexedWardCount}, candidateWardCount={snapshot.CandidateWardCount}, visibleWardCount={snapshot.VisibleWardCount}, enabledWardCount={snapshot.EnabledWardCount}{DescribeFirstEntry(snapshot.FirstEntry)}");
		}
	}

	private static void SendWardPinsPush(long receiverUid, bool fullSnapshot, int viewerRevisionToken, long playerId, bool canSeeAllWards, int indexedWardCount, int candidateWardCount, int visibleWardCount, int enabledWardCount, IReadOnlyList<WardMinimapSnapshotEntry> snapshotEntries, IReadOnlyList<ZDOID> removedWardIds, WardMinimapSnapshotEntry? firstEntry)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (instance != null && receiverUid != 0L)
		{
			ZPackage val = new ZPackage();
			val.Write(fullSnapshot);
			val.Write(viewerRevisionToken);
			val.Write(playerId);
			val.Write(canSeeAllWards);
			val.Write(indexedWardCount);
			val.Write(candidateWardCount);
			val.Write(visibleWardCount);
			val.Write(enabledWardCount);
			val.Write(snapshotEntries.Count);
			WriteSnapshotEntries(val, snapshotEntries);
			val.Write(removedWardIds.Count);
			WriteRemovedWardIds(val, removedWardIds);
			instance.InvokeRoutedRPC(receiverUid, "STUWard_PushWardPins", new object[1] { val });
			Plugin.LogWardDiagnosticVerbose("WardPins.Push", string.Format("Pushed ward minimap {0}. receiverUid={1}, viewerRevisionToken={2}, playerId={3}, canSeeAllWards={4}, indexedWardCount={5}, candidateWardCount={6}, visibleWardCount={7}, enabledWardCount={8}, upsertedWardCount={9}, removedWardCount={10}{11}", fullSnapshot ? "full snapshot" : "delta", receiverUid, viewerRevisionToken, playerId, canSeeAllWards, indexedWardCount, candidateWardCount, visibleWardCount, enabledWardCount, snapshotEntries.Count, removedWardIds.Count, DescribeFirstEntry(firstEntry)));
		}
	}

	private static void HandleReceiveWardPins(long _, ZPackage pkg)
	{
		if (pkg == null)
		{
			return;
		}
		int num;
		WardPinsResponseKind wardPinsResponseKind;
		int num2;
		long num3;
		bool flag;
		int num4;
		int num5;
		int num6;
		int num7;
		int num8;
		try
		{
			num = pkg.ReadInt();
			wardPinsResponseKind = ReadWardPinsResponseKind(pkg.ReadInt());
			num2 = pkg.ReadInt();
			num3 = pkg.ReadLong();
			flag = pkg.ReadBool();
			num4 = pkg.ReadInt();
			num5 = pkg.ReadInt();
			num6 = pkg.ReadInt();
			num7 = pkg.ReadInt();
			num8 = pkg.ReadInt();
		}
		catch
		{
			Plugin.LogWardDiagnosticFailure("WardPins.Response", "Failed to deserialize remote ward minimap snapshot header.");
			return;
		}
		if (num <= 0 || (_lastViewerRevisionToken != 0 && num2 != 0 && num2 < _lastViewerRevisionToken))
		{
			return;
		}
		if (_pendingSnapshotRequestId == 0 || num != _pendingSnapshotRequestId)
		{
			Plugin.LogWardDiagnosticVerbose("WardPins.Response", $"Ignored remote ward minimap snapshot response because it does not match the current pending request. requestId={num}, pendingRequestId={_pendingSnapshotRequestId}");
			return;
		}
		switch (wardPinsResponseKind)
		{
		case WardPinsResponseKind.Unavailable:
			Plugin.LogWardDiagnosticVerbose("WardPins.Response", $"Deferred applying remote ward minimap snapshot because the server could not prepare it yet. requestId={num}, playerId={num3}, canSeeAllWards={flag}");
			break;
		case WardPinsResponseKind.Unchanged:
			if (_snapshotState != ClientSnapshotState.Ready)
			{
				_pendingSnapshotRequestId = 0;
				_lastViewerRevisionToken = 0;
				QueueRemoteSnapshotBootstrapRequest("server reported unchanged before full snapshot was acknowledged");
				Plugin.LogWardDiagnosticVerbose("WardPins.Response", $"Rejected unchanged remote ward minimap response because the client has not acknowledged a full snapshot yet. requestId={num}, snapshotState={_snapshotState}, playerId={num3}, canSeeAllWards={flag}, serverViewerRevisionToken={num2}");
				UpdateLocalState(Player.m_localPlayer);
			}
			else
			{
				ClearPendingForceRefresh();
				ClearPendingRemoteSnapshotBootstrapRequest();
				_pendingSnapshotRequestId = 0;
				_lastViewerRevisionToken = num2;
				LogScanSummary($"playerId={num3}, canSeeAllWards={flag}, indexedWardCount={num4}, candidateWardCount={num5}, visibleWardCount={num6}, enabledWardCount={num7}, source=server-unchanged");
				UpdateLocalState(Player.m_localPlayer);
			}
			break;
		default:
		{
			if (!TryReadSnapshotEntries(pkg, num8, out WardMinimapSnapshotEntry[] snapshotEntries, out WardMinimapSnapshotEntry? firstEntry))
			{
				Plugin.LogWardDiagnosticFailure("WardPins.Response", $"Failed to deserialize remote ward minimap snapshot body. requestId={num}, declaredSnapshotCount={num8}");
				QueueRemoteSnapshotBootstrapRequest("failed to deserialize remote ward minimap snapshot body");
				break;
			}
			ReplaceLocalSnapshot(snapshotEntries);
			ClearPendingForceRefresh();
			ClearPendingRemoteSnapshotBootstrapRequest();
			_pendingSnapshotRequestId = 0;
			_lastViewerRevisionToken = num2;
			LogScanSummary($"playerId={num3}, canSeeAllWards={flag}, indexedWardCount={num4}, candidateWardCount={num5}, visibleWardCount={num6}, enabledWardCount={num7}, source=server{DescribeFirstEntry(firstEntry)}");
			UpdateLocalState(Player.m_localPlayer);
			break;
		}
		}
	}

	private static WardPinsResponseKind ReadWardPinsResponseKind(int rawValue)
	{
		return rawValue switch
		{
			0 => WardPinsResponseKind.Unavailable, 
			1 => WardPinsResponseKind.FullSnapshot, 
			2 => WardPinsResponseKind.Unchanged, 
			_ => WardPinsResponseKind.Unavailable, 
		};
	}

	private static void HandlePushWardPins(long _, ZPackage pkg)
	{
		if (pkg == null)
		{
			return;
		}
		bool flag;
		int num;
		long num2;
		bool flag2;
		int num3;
		int num4;
		int num5;
		int num6;
		int num7;
		try
		{
			flag = pkg.ReadBool();
			num = pkg.ReadInt();
			num2 = pkg.ReadLong();
			flag2 = pkg.ReadBool();
			num3 = pkg.ReadInt();
			num4 = pkg.ReadInt();
			num5 = pkg.ReadInt();
			num6 = pkg.ReadInt();
			num7 = pkg.ReadInt();
		}
		catch
		{
			Plugin.LogWardDiagnosticFailure("WardPins.Push", "Failed to deserialize pushed ward minimap snapshot header.");
			QueueRemoteSnapshotBootstrapRequest("failed to deserialize pushed ward minimap snapshot header");
			return;
		}
		if (_lastViewerRevisionToken != 0 && num != 0 && num < _lastViewerRevisionToken)
		{
			return;
		}
		if (!TryReadSnapshotEntries(pkg, num7, out WardMinimapSnapshotEntry[] snapshotEntries, out WardMinimapSnapshotEntry? firstEntry))
		{
			Plugin.LogWardDiagnosticFailure("WardPins.Push", $"Failed to deserialize pushed ward minimap snapshot body. viewerRevisionToken={num}, declaredSnapshotCount={num7}");
			QueueRemoteSnapshotBootstrapRequest("failed to deserialize pushed ward minimap snapshot body");
			return;
		}
		int num8;
		try
		{
			num8 = pkg.ReadInt();
		}
		catch
		{
			Plugin.LogWardDiagnosticFailure("WardPins.Push", $"Failed to deserialize pushed ward minimap removed-id count. viewerRevisionToken={num}");
			QueueRemoteSnapshotBootstrapRequest("failed to deserialize pushed ward minimap removed-id count");
			return;
		}
		if (!TryReadRemovedWardIds(pkg, num8, out ZDOID[] removedWardIds))
		{
			Plugin.LogWardDiagnosticFailure("WardPins.Push", $"Failed to deserialize pushed ward minimap removed-id body. viewerRevisionToken={num}, declaredRemovedCount={num8}");
			QueueRemoteSnapshotBootstrapRequest("failed to deserialize pushed ward minimap removed-id body");
			return;
		}
		if (!flag && _snapshotState != ClientSnapshotState.Ready)
		{
			Plugin.LogWardDiagnosticVerbose("WardPins.Push", $"Ignored pushed ward minimap delta because the client has not acknowledged a full snapshot yet. snapshotState={_snapshotState}, viewerRevisionToken={num}, upsertedWardCount={snapshotEntries.Length}, removedWardCount={removedWardIds.Length}");
			QueueRemoteSnapshotBootstrapRequest("ignored pushed delta before full snapshot was acknowledged");
			return;
		}
		if (flag)
		{
			ReplaceLocalSnapshot(snapshotEntries);
		}
		else
		{
			ApplyLocalSnapshotDelta(snapshotEntries, removedWardIds);
		}
		ClearPendingForceRefresh();
		ClearPendingRemoteSnapshotBootstrapRequest();
		_pendingSnapshotRequestId = 0;
		_lastViewerRevisionToken = num;
		LogScanSummary(string.Format("playerId={0}, canSeeAllWards={1}, indexedWardCount={2}, candidateWardCount={3}, visibleWardCount={4}, enabledWardCount={5}, source={6}, upsertedWardCount={7}, removedWardCount={8}{9}", num2, flag2, num3, num4, num5, num6, flag ? "server-push-full" : "server-push-delta", snapshotEntries.Length, removedWardIds.Length, DescribeFirstEntry(firstEntry)));
		UpdateLocalState(Player.m_localPlayer);
	}

	private static void WriteSnapshotEntries(ZPackage pkg, IReadOnlyList<WardMinimapSnapshotEntry> snapshotEntries)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < snapshotEntries.Count; i++)
		{
			WardMinimapSnapshotEntry wardMinimapSnapshotEntry = snapshotEntries[i];
			pkg.Write(wardMinimapSnapshotEntry.ZdoId);
			pkg.Write(wardMinimapSnapshotEntry.Position);
			pkg.Write(wardMinimapSnapshotEntry.Radius);
			pkg.Write(wardMinimapSnapshotEntry.IsEnabled);
		}
	}

	private static void WriteRemovedWardIds(ZPackage pkg, IReadOnlyList<ZDOID> removedWardIds)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < removedWardIds.Count; i++)
		{
			pkg.Write(removedWardIds[i]);
		}
	}

	private static bool TryReadSnapshotEntries(ZPackage pkg, int snapshotCount, out WardMinimapSnapshotEntry[] snapshotEntries, out WardMinimapSnapshotEntry? firstEntry)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		snapshotEntries = ((snapshotCount <= 0) ? Array.Empty<WardMinimapSnapshotEntry>() : new WardMinimapSnapshotEntry[snapshotCount]);
		firstEntry = null;
		try
		{
			for (int i = 0; i < snapshotEntries.Length; i++)
			{
				snapshotEntries[i] = new WardMinimapSnapshotEntry(pkg.ReadZDOID(), pkg.ReadVector3(), pkg.ReadSingle(), pkg.ReadBool());
				WardMinimapSnapshotEntry valueOrDefault = firstEntry.GetValueOrDefault();
				if (!firstEntry.HasValue)
				{
					valueOrDefault = snapshotEntries[i];
					firstEntry = valueOrDefault;
				}
			}
			return true;
		}
		catch
		{
			snapshotEntries = Array.Empty<WardMinimapSnapshotEntry>();
			firstEntry = null;
			return false;
		}
	}

	private static bool TryReadRemovedWardIds(ZPackage pkg, int removedWardCount, out ZDOID[] removedWardIds)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		removedWardIds = (ZDOID[])((removedWardCount <= 0) ? ((Array)Array.Empty<ZDOID>()) : ((Array)new ZDOID[removedWardCount]));
		try
		{
			for (int i = 0; i < removedWardIds.Length; i++)
			{
				removedWardIds[i] = pkg.ReadZDOID();
			}
			return true;
		}
		catch
		{
			removedWardIds = Array.Empty<ZDOID>();
			return false;
		}
	}
}
