using System.Collections.Generic;
using UnityEngine;

namespace STUWard;

internal static class ManagedWardInteractionRpc
{
	private readonly struct PendingLocalEnabledToggleRequest
	{
		internal bool ExpectedEnabled { get; }

		internal float ExpiresAt { get; }

		internal PendingLocalEnabledToggleRequest(bool expectedEnabled, float expiresAt)
		{
			ExpectedEnabled = expectedEnabled;
			ExpiresAt = expiresAt;
		}
	}

	private readonly struct PendingLocalPermittedToggleRequest
	{
		internal bool ExpectedPermitted { get; }

		internal float ExpiresAt { get; }

		internal PendingLocalPermittedToggleRequest(bool expectedPermitted, float expiresAt)
		{
			ExpectedPermitted = expectedPermitted;
			ExpiresAt = expiresAt;
		}
	}

	private static readonly Dictionary<ZDOID, PendingLocalEnabledToggleRequest> PendingLocalEnabledToggleRequests = new Dictionary<ZDOID, PendingLocalEnabledToggleRequest>();

	private static readonly Dictionary<ZDOID, PendingLocalPermittedToggleRequest> PendingLocalPermittedToggleRequests = new Dictionary<ZDOID, PendingLocalPermittedToggleRequest>();

	private const float PendingLocalEnabledToggleTimeoutSeconds = 1.5f;

	private const float PendingLocalPermittedToggleTimeoutSeconds = 1.5f;

	internal static bool IsManagedWardForHooks(PrivateArea? area)
	{
		if ((Object)(object)area == (Object)null)
		{
			return false;
		}
		return ManagedWardIdentity.EnsureManagedComponent(area);
	}

	internal static void ResetLocalInteractionState()
	{
		PendingLocalEnabledToggleRequests.Clear();
		PendingLocalPermittedToggleRequests.Clear();
	}

	internal static bool TryHandleInteract(PrivateArea area, Player player, bool hold, ref bool result)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		result = false;
		if (hold)
		{
			return false;
		}
		if ((int)area.m_ownerFaction != 0)
		{
			return false;
		}
		ZNetView nView = GetNView(area);
		if ((Object)(object)nView == (Object)null || !nView.IsValid())
		{
			Plugin.LogWardDiagnosticFailure("Interaction", "Blocked managed ward interact because ward nview is invalid. " + WardDiagnosticInfo.DescribeWard(area));
			return false;
		}
		if (Plugin.IsWardSettingsShortcutDown())
		{
			if (WardAccess.CanConfigureWard(area, player) || WardAdminDebugAccess.CanLocallyAttemptAnyWardControl(area, player))
			{
				Plugin.LogWardDiagnosticVerbose("Interaction.Local", "Skipped managed ward interact because ward settings shortcut is active. " + WardDiagnosticInfo.DescribeLocalPlayer(player) + ", " + WardDiagnosticInfo.DescribeInteractionState(area, player.GetPlayerID()));
			}
			result = false;
			return false;
		}
		long playerID = player.GetPlayerID();
		bool flag = WardAccess.CanControlManagedWard(area, playerID) || WardAdminDebugAccess.CanLocallyAttemptAnyWardControl(area, player);
		Plugin.LogWardDiagnosticVerbose("Interaction.Local", string.Format("Local interact hold={0}, action={1}, ", hold, flag ? "toggle_enabled" : (area.IsEnabled() ? "blocked_enabled_foreign" : "toggle_permitted")) + WardDiagnosticInfo.DescribeLocalPlayer(player) + ", " + WardDiagnosticInfo.DescribeInteractionState(area, playerID));
		if (flag)
		{
			result = RequestToggleEnabled(area, player);
			return false;
		}
		if (area.IsEnabled())
		{
			return false;
		}
		result = RequestTogglePermitted(area, player);
		return false;
	}

	internal static string? GetHoverActionLine(PrivateArea area, Player? player)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || (int)area.m_ownerFaction != 0)
		{
			return null;
		}
		if (WardAccess.CanControlManagedWard(area, player.GetPlayerID()) || WardAdminDebugAccess.CanLocallyAttemptAnyWardControl(area, player))
		{
			return LocalizeUseAction(area.IsEnabled() ? "$piece_guardstone_deactivate" : "$piece_guardstone_activate", area.IsEnabled() ? "Deactivate" : "Activate");
		}
		if (area.IsEnabled())
		{
			return null;
		}
		bool flag = WardPrivateAreaSafeAccess.IsPlayerPermitted(area, player.GetPlayerID());
		return LocalizeUseAction(flag ? "$piece_guardstone_remove" : "$piece_guardstone_add", flag ? "Remove" : "Add");
	}

	private static bool RequestToggleEnabled(PrivateArea area, Player player)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		ZNetView nView = GetNView(area);
		if ((Object)(object)nView == (Object)null || !nView.IsValid())
		{
			return false;
		}
		ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(area);
		if (zdo == null || !zdo.IsValid())
		{
			return false;
		}
		if (IsLocalEnabledToggleRequestInFlight(zdo.m_uid, area.IsEnabled()))
		{
			Plugin.LogWardDiagnosticVerbose("ToggleEnabled.Send", $"Suppressed duplicate ToggleEnabled request while a local request is still in flight. playerId={player.GetPlayerID()}, wardZdo={zdo.m_uid}, enabled={area.IsEnabled()}");
			return true;
		}
		Plugin.LogWardDiagnosticVerbose("ToggleEnabled.Send", $"Sending vanilla ToggleEnabled request for playerId={player.GetPlayerID()}, {WardDiagnosticInfo.DescribeInteractionState(area, player.GetPlayerID())}");
		bool expectedEnabled = !area.IsEnabled();
		nView.InvokeRPC("ToggleEnabled", new object[1] { player.GetPlayerID() });
		PendingLocalEnabledToggleRequests[zdo.m_uid] = new PendingLocalEnabledToggleRequest(expectedEnabled, Time.time + 1.5f);
		return true;
	}

	private static bool RequestTogglePermitted(PrivateArea area, Player player)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		ZNetView nView = GetNView(area);
		if ((Object)(object)nView == (Object)null || !nView.IsValid())
		{
			return false;
		}
		ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(area);
		if (zdo == null || !zdo.IsValid())
		{
			return false;
		}
		bool flag = WardPrivateAreaSafeAccess.IsPlayerPermitted(area, player.GetPlayerID());
		if (IsLocalPermittedToggleRequestInFlight(zdo.m_uid, flag))
		{
			Plugin.LogWardDiagnosticVerbose("TogglePermitted.Send", $"Suppressed duplicate TogglePermitted request while a local request is still in flight. playerId={player.GetPlayerID()}, wardZdo={zdo.m_uid}, permitted={flag}");
			return true;
		}
		Plugin.LogWardDiagnosticVerbose("TogglePermitted.Send", $"Sending vanilla TogglePermitted request for playerId={player.GetPlayerID()}, {WardDiagnosticInfo.DescribeInteractionState(area, player.GetPlayerID())}");
		bool expectedPermitted = !flag;
		nView.InvokeRPC("TogglePermitted", new object[2]
		{
			player.GetPlayerID(),
			player.GetPlayerName()
		});
		PendingLocalPermittedToggleRequests[zdo.m_uid] = new PendingLocalPermittedToggleRequest(expectedPermitted, Time.time + 1.5f);
		return true;
	}

	internal static bool TryHandleVanillaToggleEnabled(PrivateArea area, long sender, long claimedPlayerId)
	{
		if (!WardOwnership.CanHandleManagedWardStateRpc(GetNView(area)))
		{
			return false;
		}
		if (!WardOwnership.TryResolveClaimedPlayerIdFromSender(sender, claimedPlayerId, "ToggleEnabled.Vanilla", out var playerId))
		{
			return false;
		}
		return TryApplyResolvedToggleEnabled(area, playerId, "ToggleEnabled.Vanilla");
	}

	internal static bool TryHandleVanillaTogglePermitted(PrivateArea area, long sender, long claimedPlayerId, string name)
	{
		if (!WardOwnership.CanHandleManagedWardStateRpc(GetNView(area)))
		{
			return false;
		}
		if (!WardOwnership.TryResolveClaimedPlayerIdFromSender(sender, claimedPlayerId, "TogglePermitted.Vanilla", out var playerId))
		{
			return false;
		}
		if (playerId == 0L)
		{
			return false;
		}
		if (WardAccess.CanControlManagedWard(area, playerId))
		{
			Plugin.LogWardDiagnosticFailure("TogglePermitted.Vanilla", "Rejected TogglePermitted because requester can control this ward and should use ToggleEnabled instead. " + WardDiagnosticInfo.DescribeInteractionState(area, playerId));
			return false;
		}
		if (area.IsEnabled())
		{
			Plugin.LogWardDiagnosticFailure("TogglePermitted.Vanilla", "Rejected TogglePermitted because ward is enabled. " + WardDiagnosticInfo.DescribeInteractionState(area, playerId));
			return false;
		}
		if (!WardOwnership.TryClaimManagedWardMutationOwnership(area, "TogglePermitted.Vanilla"))
		{
			return false;
		}
		if (WardPrivateAreaSafeAccess.IsPlayerPermitted(area, playerId))
		{
			uint dataRevision;
			bool num = ManagedWardRuntimeContexts.TryGetCurrentDataRevision(area, out dataRevision);
			area.RemovePermitted(playerId);
			if (num)
			{
				ManagedWardRuntimeContexts.ArmNextDataRevisionFanOutSuppressionIfChanged(area, dataRevision);
			}
			Plugin.LogWardDiagnosticVerbose("TogglePermitted.Vanilla", "Removed permitted player. " + WardDiagnosticInfo.DescribeInteractionState(area, playerId));
			return true;
		}
		string text = ((!string.IsNullOrWhiteSpace(name)) ? name : WardOwnership.GetPlayerName(playerId));
		if (string.IsNullOrWhiteSpace(text))
		{
			text = playerId.ToString();
		}
		ManagedWardRuntimeContexts.ArmNextDataRevisionFanOutSuppression(area);
		area.AddPermitted(playerId, text);
		Plugin.LogWardDiagnosticVerbose("TogglePermitted.Vanilla", "Added permitted player requesterName='" + text + "'. " + WardDiagnosticInfo.DescribeInteractionState(area, playerId));
		return true;
	}

	internal static bool TryApplyResolvedToggleEnabled(PrivateArea area, long requesterId, string context)
	{
		return ApplyToggleEnabled(area, requesterId, context);
	}

	private static bool ApplyToggleEnabled(PrivateArea area, long requesterId, string context)
	{
		if (!WardAccess.CanControlManagedWard(area, requesterId))
		{
			Plugin.LogWardDiagnosticFailure(context, "Rejected ToggleEnabled because requester cannot control this ward. " + WardDiagnosticInfo.DescribeInteractionState(area, requesterId));
			return false;
		}
		if (!WardOwnership.TryClaimManagedWardMutationOwnership(area, context))
		{
			return false;
		}
		bool flag = !area.IsEnabled();
		ManagedWardRuntimeContexts.ArmNextEnabledFanOutSuppression(area, flag);
		ManagedWardRuntimeContexts.ArmNextDataRevisionFanOutSuppression(area);
		area.SetEnabled(flag);
		Plugin.LogWardDiagnosticVerbose(context, "Applied ToggleEnabled. " + WardDiagnosticInfo.DescribeInteractionState(area, requesterId));
		return true;
	}

	private static ZNetView? GetNView(PrivateArea area)
	{
		return WardPrivateAreaSafeAccess.GetNView(area);
	}

	internal static void NotifyLocalEnabledStateObserved(PrivateArea area)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(area);
		if (zdo != null && zdo.IsValid())
		{
			PendingLocalEnabledToggleRequests.Remove(zdo.m_uid);
		}
	}

	internal static void NotifyLocalPermittedStateObserved(PrivateArea area)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		ZDO zdo = WardPrivateAreaSafeAccess.GetZdo(area);
		if (!((Object)(object)localPlayer == (Object)null) && zdo != null && zdo.IsValid())
		{
			bool flag = WardPrivateAreaSafeAccess.IsPlayerPermitted(area, localPlayer.GetPlayerID());
			if (PendingLocalPermittedToggleRequests.TryGetValue(zdo.m_uid, out var value) && (Time.time >= value.ExpiresAt || flag == value.ExpectedPermitted))
			{
				PendingLocalPermittedToggleRequests.Remove(zdo.m_uid);
			}
		}
	}

	private static bool IsLocalEnabledToggleRequestInFlight(ZDOID wardZdoId, bool currentEnabled)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!PendingLocalEnabledToggleRequests.TryGetValue(wardZdoId, out var value))
		{
			return false;
		}
		if (Time.time >= value.ExpiresAt || currentEnabled == value.ExpectedEnabled)
		{
			PendingLocalEnabledToggleRequests.Remove(wardZdoId);
			return false;
		}
		return true;
	}

	private static bool IsLocalPermittedToggleRequestInFlight(ZDOID wardZdoId, bool currentPermitted)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!PendingLocalPermittedToggleRequests.TryGetValue(wardZdoId, out var value))
		{
			return false;
		}
		if (Time.time >= value.ExpiresAt || currentPermitted == value.ExpectedPermitted)
		{
			PendingLocalPermittedToggleRequests.Remove(wardZdoId);
			return false;
		}
		return true;
	}

	private static string LocalizeUseAction(string token, string fallback)
	{
		if (Localization.instance == null)
		{
			return "[E] " + fallback;
		}
		return Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] " + token);
	}
}
