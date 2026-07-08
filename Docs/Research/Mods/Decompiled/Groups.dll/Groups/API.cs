using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Groups;

[PublicAPI]
public class API
{
	public static event Action? joinedGroup;

	public static event Action? leftGroup;

	public static event Action<PlayerReference>? memberJoined;

	public static event Action<PlayerReference>? memberLeft;

	public static event Action<PlayerReference>? leaderChanged;

	public static event Action<PlayerReference, GameObject>? invitationReceived;

	public static event Action<PlayerReference, GameObject>? uiUpdate;

	internal static void InvokeGroupJoined()
	{
		API.joinedGroup?.Invoke();
	}

	internal static void InvokeGroupLeft()
	{
		API.leftGroup?.Invoke();
	}

	internal static void InvokeMemberJoined(PlayerReference player)
	{
		API.memberJoined?.Invoke(player);
	}

	internal static void InvokeMemberLeft(PlayerReference player)
	{
		API.memberLeft?.Invoke(player);
	}

	internal static void InvokeLeaderChanged(PlayerReference player)
	{
		API.leaderChanged?.Invoke(player);
	}

	internal static void InvokeInvitationReceived(PlayerReference player, GameObject invitationDialog)
	{
		API.invitationReceived?.Invoke(player, invitationDialog);
	}

	internal static void InvokeUIUpdate(PlayerReference player, GameObject playerRoot)
	{
		API.uiUpdate?.Invoke(player, playerRoot);
	}

	public static bool IsLoaded()
	{
		return true;
	}

	public static int GetMaxGroupSize()
	{
		return Groups.maximumGroupSize.Value;
	}

	public static List<PlayerReference> GroupPlayers()
	{
		if (Groups.ownGroup == null)
		{
			return new List<PlayerReference>();
		}
		return Groups.ownGroup.playerStates.Keys.ToList();
	}

	public static PlayerReference? GetLeader()
	{
		return Groups.ownGroup?.leader;
	}

	public static bool CreateNewGroup()
	{
		LeaveGroup();
		Groups.ownGroup = new Group(PlayerReference.fromPlayer(Player.m_localPlayer), Group.PlayerState.fromLocal());
		InvokeGroupJoined();
		return true;
	}

	public static bool WriteToGroup(string message)
	{
		if (Groups.ownGroup != null)
		{
			foreach (PlayerReference key in Groups.ownGroup.playerStates.Keys)
			{
				ZRoutedRpc.instance.InvokeRoutedRPC(key.peerId, "Groups AddMessage", new object[1] { message });
			}
			return true;
		}
		return false;
	}

	public static bool LeaveGroup()
	{
		Groups.ownGroup?.Leave();
		Groups.ownGroup = null;
		return true;
	}

	public static bool JoinGroup(PlayerReference targetPlayer)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		if (Groups.ownGroup != null)
		{
			LeaveGroup();
		}
		ZPackage val = new ZPackage();
		Group.PlayerState.fromLocal().write(val);
		ZRoutedRpc.instance.InvokeRoutedRPC(targetPlayer.peerId, "Groups AcceptInvitation", new object[1] { val });
		return true;
	}

	public static bool PromoteToLeader(PlayerReference groupMember)
	{
		if (Groups.ownGroup != null)
		{
			Groups.ownGroup.PromoteMember(groupMember, sendToLeader: true);
			return true;
		}
		return false;
	}

	public static bool ForcePlayerIntoOwnGroup(PlayerReference targetPlayer)
	{
		if (Groups.ownGroup == null)
		{
			CreateNewGroup();
		}
		ZRoutedRpc.instance.InvokeRoutedRPC(targetPlayer.peerId, "Groups ForcedInvitation", Array.Empty<object>());
		return true;
	}

	public static PlayerReference? FindGroupMemberByPlayerId(long playerId)
	{
		if (playerId == Game.instance.m_playerProfile.GetPlayerID())
		{
			return PlayerReference.fromPlayerId(ZDOMan.GetSessionID());
		}
		Group ownGroup = Groups.ownGroup;
		if (ownGroup != null)
		{
			foreach (KeyValuePair<PlayerReference, Group.PlayerState> playerState in ownGroup.playerStates)
			{
				if (playerState.Value.playerId == playerId)
				{
					return playerState.Key;
				}
			}
		}
		return null;
	}
}
