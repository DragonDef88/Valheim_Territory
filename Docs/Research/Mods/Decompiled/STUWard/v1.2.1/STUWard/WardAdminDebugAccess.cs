using System;
using System.Collections.Generic;
using UnityEngine;

namespace STUWard;

internal static class WardAdminDebugAccess
{
	private const string RpcSetAdminDebugState = "STUWard_SetAdminDebugState";

	private const string RpcReceiveAdminDebugState = "STUWard_ReceiveAdminDebugState";

	private static readonly TimeSpan DebugStateResendInterval = TimeSpan.FromSeconds(3.0);

	private static readonly HashSet<long> ServerDebugAdminPlayerIds = new HashSet<long>();

	private static bool _rpcsRegistered;

	private static bool? _lastLocalDebugAdminState;

	private static bool _serverApprovedLocalDebugState;

	private static DateTime _lastLocalDebugAdminSyncUtc = DateTime.MinValue;

	internal static void ResetRuntimeState()
	{
		_rpcsRegistered = false;
		_lastLocalDebugAdminState = null;
		_serverApprovedLocalDebugState = false;
		_lastLocalDebugAdminSyncUtc = DateTime.MinValue;
		ServerDebugAdminPlayerIds.Clear();
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

	internal static void RegisterRpcs()
	{
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (!_rpcsRegistered && instance != null)
		{
			instance.Register<bool>("STUWard_SetAdminDebugState", (Action<long, bool>)HandleSetAdminDebugState);
			instance.Register<bool>("STUWard_ReceiveAdminDebugState", (Action<long, bool>)HandleReceiveAdminDebugState);
			_rpcsRegistered = true;
		}
	}

	internal static void UpdateLocalState(Player? player, bool force = false)
	{
		if ((Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)Player.m_localPlayer || (Object)(object)ZNet.instance == (Object)null)
		{
			return;
		}
		bool flag = IsLocalAdminDebugRequested(player);
		DateTime utcNow = DateTime.UtcNow;
		if (ZNet.instance.IsServer())
		{
			if (force || !_lastLocalDebugAdminState.HasValue || _lastLocalDebugAdminState.Value != flag)
			{
				_lastLocalDebugAdminState = flag;
				_lastLocalDebugAdminSyncUtc = utcNow;
				SetServerAdminDebugState(player.GetPlayerID(), flag);
			}
			return;
		}
		bool flag2 = !_lastLocalDebugAdminState.HasValue || _lastLocalDebugAdminState.Value != flag;
		bool flag3 = flag && !_serverApprovedLocalDebugState && utcNow - _lastLocalDebugAdminSyncUtc >= DebugStateResendInterval;
		if (force || flag2 || flag3)
		{
			_lastLocalDebugAdminState = flag;
			_lastLocalDebugAdminSyncUtc = utcNow;
			RegisterRpcs();
			ZRoutedRpc instance = ZRoutedRpc.instance;
			if (instance != null)
			{
				instance.InvokeRoutedRPC("STUWard_SetAdminDebugState", new object[1] { flag });
			}
		}
	}

	internal static bool CanLocallyControlAnyWard(PrivateArea? area, Player? player)
	{
		if ((Object)(object)area != (Object)null && (Object)(object)player != (Object)null && (Object)(object)player == (Object)(object)Player.m_localPlayer && WardAccess.IsManagedWard(area, requireEnabled: false))
		{
			return IsLocalAdminDebugController(player);
		}
		return false;
	}

	internal static bool CanLocallyAttemptAnyWardControl(PrivateArea? area, Player? player)
	{
		if ((Object)(object)area != (Object)null && (Object)(object)player != (Object)null && (Object)(object)player == (Object)(object)Player.m_localPlayer && WardAccess.IsManagedWard(area, requireEnabled: false))
		{
			return Player.m_debugMode;
		}
		return false;
	}

	internal static bool IsPlayerAdminDebugController(long playerId)
	{
		if (playerId == 0L)
		{
			return false;
		}
		if ((Object)(object)Player.m_localPlayer != (Object)null && Player.m_localPlayer.GetPlayerID() == playerId && IsLocalAdminDebugController(Player.m_localPlayer))
		{
			return true;
		}
		return ServerDebugAdminPlayerIds.Contains(playerId);
	}

	internal static void ForgetServerPlayer(long playerId)
	{
		if (playerId != 0L && ServerDebugAdminPlayerIds.Remove(playerId))
		{
			Plugin.LogWardDiagnosticVerbose("AdminDebug.Sync", $"Removed stale admin debug control state for disconnected playerId={playerId}.");
		}
	}

	private static bool IsLocalAdminDebugController(Player? player)
	{
		if ((Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)Player.m_localPlayer || !Player.m_debugMode || (Object)(object)ZNet.instance == (Object)null)
		{
			return false;
		}
		if (ZNet.instance.IsServer())
		{
			return true;
		}
		string playerAccountId = WardOwnership.GetPlayerAccountId(player.GetPlayerID());
		if ((Object)(object)player != (Object)null)
		{
			if (!_serverApprovedLocalDebugState && !IsAdminAccountId(playerAccountId))
			{
				return ZNet.instance.LocalPlayerIsAdminOrHost();
			}
			return true;
		}
		return false;
	}

	private static bool IsLocalAdminDebugRequested(Player? player)
	{
		if ((Object)(object)player != (Object)null && (Object)(object)player == (Object)(object)Player.m_localPlayer && Player.m_debugMode)
		{
			return (Object)(object)ZNet.instance != (Object)null;
		}
		return false;
	}

	private static void HandleSetAdminDebugState(long sender, bool enabled)
	{
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer() || !WardOwnership.TryResolveAuthoritativePlayerIdFromSender(sender, "AdminDebug.Sync", out var playerId))
		{
			return;
		}
		if (!enabled)
		{
			ServerDebugAdminPlayerIds.Remove(playerId);
			SendAdminDebugStateResponse(sender, enabled: false);
			return;
		}
		string authoritativeAccountIdFromSender = WardOwnership.GetAuthoritativeAccountIdFromSender(sender, playerId);
		if (!IsAdminAccountId(authoritativeAccountIdFromSender))
		{
			ServerDebugAdminPlayerIds.Remove(playerId);
			Plugin.LogWardDiagnosticFailure("AdminDebug.Sync", $"Rejected admin debug enable because the requesting account is not a server admin. sender={sender}, playerId={playerId}, accountId='{authoritativeAccountIdFromSender}'");
			SendAdminDebugStateResponse(sender, enabled: false);
		}
		else
		{
			ServerDebugAdminPlayerIds.Add(playerId);
			Plugin.LogWardDiagnosticVerbose("AdminDebug.Sync", $"Enabled admin debug control for playerId={playerId}, sender={sender}, accountId='{authoritativeAccountIdFromSender}'.");
			SendAdminDebugStateResponse(sender, enabled: true);
		}
	}

	private static void HandleReceiveAdminDebugState(long _, bool enabled)
	{
		_serverApprovedLocalDebugState = enabled;
		_lastLocalDebugAdminSyncUtc = DateTime.UtcNow;
		Plugin.LogWardDiagnosticVerbose("AdminDebug.Sync", $"Received admin debug approval state from server. enabled={enabled}");
	}

	private static void SendAdminDebugStateResponse(long receiverUid, bool enabled)
	{
		if (receiverUid != 0L)
		{
			ZRoutedRpc instance = ZRoutedRpc.instance;
			if (instance != null)
			{
				instance.InvokeRoutedRPC(receiverUid, "STUWard_ReceiveAdminDebugState", new object[1] { enabled });
			}
		}
	}

	private static void SetServerAdminDebugState(long playerId, bool enabled)
	{
		if (playerId != 0L)
		{
			if (enabled)
			{
				ServerDebugAdminPlayerIds.Add(playerId);
			}
			else
			{
				ServerDebugAdminPlayerIds.Remove(playerId);
			}
		}
	}

	internal static bool IsAdminAccountId(string accountId)
	{
		ZNet instance = ZNet.instance;
		List<string> list = ((instance != null) ? instance.GetAdminList() : null);
		if (list == null || string.IsNullOrWhiteSpace(accountId))
		{
			return false;
		}
		string text = NormalizeAccountId(accountId);
		string accountIdCore = GetAccountIdCore(text);
		for (int i = 0; i < list.Count; i++)
		{
			string text2 = NormalizeAccountId(list[i]);
			if (string.Equals(text2, text, StringComparison.Ordinal))
			{
				return true;
			}
			if (!string.IsNullOrWhiteSpace(accountIdCore) && string.Equals(GetAccountIdCore(text2), accountIdCore, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	private static string NormalizeAccountId(string? rawAccountId)
	{
		if (string.IsNullOrWhiteSpace(rawAccountId))
		{
			return string.Empty;
		}
		return WardOwnership.NormalizeAccountIdValue(rawAccountId);
	}

	private static string GetAccountIdCore(string accountId)
	{
		if (string.IsNullOrWhiteSpace(accountId))
		{
			return string.Empty;
		}
		int num = accountId.IndexOf('_');
		if (num < 0 || num >= accountId.Length - 1)
		{
			return accountId;
		}
		return accountId.Substring(num + 1);
	}
}
