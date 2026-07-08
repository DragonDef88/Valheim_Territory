using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Splatform;
using UnityEngine;

namespace Groups;

public static class RPC
{
	[HarmonyPatch(typeof(Game), "Start")]
	private class AddRPCs
	{
		private static void Postfix()
		{
			ZRoutedRpc.instance.Register<string>("Groups AddMessage", (Action<long, string>)delegate(long _, string message)
			{
				((Terminal)Chat.instance).AddString(message);
				Chat.instance.m_hideTimer = 0f;
			});
			ZRoutedRpc.instance.Register<UserInfo, string>("Groups ChatMessage", (Action<long, UserInfo, string>)onChatMessageReceived);
			ZRoutedRpc.instance.Register<string>("Groups InvitePlayer", (Action<long, string>)onInvitationReceived);
			ZRoutedRpc.instance.Register("Groups ForcedInvitation", (Action<long>)onForcedInvitationReceived);
			ZRoutedRpc.instance.Register<ZPackage>("Groups AcceptInvitation", (Action<long, ZPackage>)onInvitationAccepted);
			ZRoutedRpc.instance.Register<ZPackage>("Groups AcceptInvitationResponse", (Action<long, ZPackage>)onInvitationAcceptedResponse);
			ZRoutedRpc.instance.Register<string, string>("Groups UpdateGroup", (Action<long, string, string>)onUpdateGroup);
			ZRoutedRpc.instance.Register<string, ZPackage>("Groups AddMember", (Action<long, string, ZPackage>)onNewGroupMember);
			ZRoutedRpc.instance.Register<float, float>("Groups UpdateHealth", (Action<long, float, float>)Interface.onUpdateHealth);
			ZRoutedRpc.instance.Register<Vector3>("Groups UpdatePosition", (Action<long, Vector3>)onUpdatePosition);
			ZRoutedRpc.instance.Register<Vector3, int, UserInfo, string>("Groups MapPing", (Method<Vector3, int, UserInfo, string>)Map.onMapPing);
		}
	}

	[HarmonyPatch(typeof(Player), "SetLocalPlayer")]
	private class SetCharacterId
	{
		private static void Postfix(Player __instance)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			ZNet.instance.m_characterID = ((Character)__instance).GetZDOID();
		}
	}

	[HarmonyPatch]
	private class RemoveFromGroupOnLogoutAndPreservePosition
	{
		private static IEnumerable<MethodInfo> TargetMethods()
		{
			return new MethodInfo[2]
			{
				AccessTools.DeclaredMethod(typeof(ZNet), "RPC_PlayerList", (Type[])null, (Type[])null),
				AccessTools.DeclaredMethod(typeof(ZNet), "UpdatePlayerList", (Type[])null, (Type[])null)
			};
		}

		private static void Prefix(ZNet __instance, out Dictionary<long, Vector3> __state)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			__state = new Dictionary<long, Vector3>();
			if (Groups.ownGroup == null)
			{
				return;
			}
			foreach (PlayerInfo item in __instance.m_players.Where((PlayerInfo p) => Groups.ownGroup.playerStates.ContainsKey(PlayerReference.fromPlayerId(((ZDOID)(ref p.m_characterID)).UserID))))
			{
				Dictionary<long, Vector3> obj = __state;
				ZDOID characterID = item.m_characterID;
				obj[((ZDOID)(ref characterID)).UserID] = item.m_position;
			}
		}

		private static void Postfix(ZNet __instance, Dictionary<long, Vector3> __state)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			ZNet __instance2 = __instance;
			foreach (PlayerInfo player2 in __instance2.m_players)
			{
				if (player2.m_characterID != ZDOID.None)
				{
					characterIdCache[player2.m_userInfo.m_id] = PlayerReference.fromPlayerInfo(player2);
				}
			}
			PlatformUserID[] array = characterIdCache.Keys.Where((PlatformUserID host) => __instance2.m_players.All((PlayerInfo p) => p.m_userInfo.m_id != host)).ToArray();
			foreach (PlatformUserID key in array)
			{
				characterIdCache.Remove(key);
			}
			ChatCommands.UpdateAutoCompletion();
			if (Groups.ownGroup == null)
			{
				return;
			}
			List<PlayerInfo> list = new List<PlayerInfo>();
			foreach (PlayerInfo player3 in __instance2.m_players)
			{
				PlayerInfo val = player3;
				characterIdCache.TryGetValue(val.m_userInfo.m_id, out var value);
				if (Groups.ownGroup.playerStates.ContainsKey(value) && value.peerId != ZDOMan.GetSessionID())
				{
					ZDOID characterID = player3.m_characterID;
					if (__state.TryGetValue(((ZDOID)(ref characterID)).UserID, out var value2) && !player3.m_publicPosition)
					{
						val.m_position = value2;
					}
					val.m_publicPosition = true;
				}
				list.Add(val);
			}
			__instance2.m_players = list;
			if (Groups.ownGroup.leader.peerId != ZDOMan.GetSessionID())
			{
				if (characterIdCache.ContainsValue(Groups.ownGroup.leader) || Groups.ownGroup.playerStates.Keys.OrderBy((PlayerReference p) => p.peerId).First(((IEnumerable<PlayerReference>)characterIdCache.Values).Contains<PlayerReference>).peerId != ZDOMan.GetSessionID())
				{
					return;
				}
				Groups.ownGroup.PromoteMember(PlayerReference.fromPlayerId(ZDOMan.GetSessionID()));
			}
			PlayerReference[] array2 = Groups.ownGroup.playerStates.Keys.Except(characterIdCache.Values).ToArray();
			foreach (PlayerReference player in array2)
			{
				Groups.ownGroup.RemoveMember(player, self: true);
			}
		}
	}

	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static PopupButtonCallback _003C0_003E__Pop;
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<Player, bool> _003C_003E9__8_1;

		public static PopupButtonCallback _003C_003E9__8_2;

		public static Func<PlayerReference, bool> _003C_003E9__10_1;

		internal bool _003ConInvitationReceived_003Eb__8_1(Player p)
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)p != (Object)(object)Player.m_localPlayer && ((Character)p).IsPVPEnabled())
			{
				if (Groups.friendlyFire.Value != Groups.Toggle.On)
				{
					Group? ownGroup = Groups.ownGroup;
					if (ownGroup != null && ownGroup.playerStates.ContainsKey(PlayerReference.fromPlayer(p)))
					{
						goto IL_006a;
					}
				}
				return Vector3.Distance(((Component)Player.m_localPlayer).transform.position, ((Component)p).transform.position) < 30f;
			}
			goto IL_006a;
			IL_006a:
			return false;
		}

		internal void _003ConInvitationReceived_003Eb__8_2()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Groups.ownGroup?.Leave();
			UnifiedPopup.Pop();
			ZPackage val = new ZPackage();
			Group.PlayerState.fromLocal().write(val);
			ZRoutedRpc.instance.InvokeRoutedRPC(pendingInvitationSenderId, "Groups AcceptInvitation", new object[1] { val });
		}

		internal bool _003ConInvitationAccepted_003Eb__10_1(PlayerReference p)
		{
			return p != Groups.ownGroup.leader;
		}
	}

	private static long pendingInvitationSenderId;

	private static readonly Dictionary<PlatformUserID, PlayerReference> characterIdCache = new Dictionary<PlatformUserID, PlayerReference>();

	private static void onUpdateGroup(long senderId, string playerReference, string action)
	{
		if (Groups.ownGroup == null)
		{
			return;
		}
		if (!(action == "Member Removed"))
		{
			if (action == "Member Promoted")
			{
				Groups.ownGroup.leader = PlayerReference.fromString(playerReference);
				ChatCommands.UpdateAutoCompletion();
			}
			return;
		}
		PlayerReference playerReference2 = PlayerReference.fromString(playerReference);
		if (playerReference2 == PlayerReference.fromPlayer(Player.m_localPlayer))
		{
			API.InvokeGroupLeft();
			ChatCommands.ToggleGroupsChat(active: false);
			Groups.ownGroup = null;
		}
		else
		{
			Groups.ownGroup.playerStates.Remove(playerReference2);
			API.InvokeMemberLeft(playerReference2);
		}
		ChatCommands.UpdateAutoCompletion();
	}

	private static void onNewGroupMember(long senderId, string playerReference, ZPackage playerState)
	{
		if (Groups.ownGroup != null)
		{
			PlayerReference playerReference2 = PlayerReference.fromString(playerReference);
			Groups.ownGroup.playerStates.Add(playerReference2, Group.PlayerState.read(playerState));
			API.InvokeMemberJoined(playerReference2);
			ChatCommands.UpdateAutoCompletion();
		}
	}

	private static void onChatMessageReceived(long senderId, UserInfo name, string message)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		((Terminal)Chat.instance).AddString("<color=orange>" + name.Name + "</color>: <color=#" + ColorUtility.ToHtmlStringRGBA(Groups.groupChatColor.Value) + ">" + message + "</color>");
		Chat.instance.m_hideTimer = 0f;
		ZDOID characterID = ((IEnumerable<PlayerInfo>)ZNet.instance.m_players).FirstOrDefault((Func<PlayerInfo, bool>)((PlayerInfo p) => ((ZDOID)(ref p.m_characterID)).UserID == senderId)).m_characterID;
		if (!(characterID != ZDOID.None))
		{
			return;
		}
		GameObject val = ZNetScene.instance.FindInstance(characterID);
		if (val != null)
		{
			Player component = val.GetComponent<Player>();
			if (component != null && (!Object.op_Implicit((Object)(object)Minimap.instance) || !Object.op_Implicit((Object)(object)Player.m_localPlayer) || (int)Minimap.instance.m_mode != 0 || !(Vector3.Distance(((Component)Player.m_localPlayer).transform.position, ((Character)component).GetHeadPoint()) > Minimap.instance.m_nomapPingDistance)))
			{
				Chat.instance.AddInworldText(val, senderId, ((Character)component).GetHeadPoint(), (Type)1, name, "<color=#" + ColorUtility.ToHtmlStringRGBA(Groups.groupChatColor.Value) + ">" + message + "</color>");
			}
		}
	}

	private static void onInvitationReceived(long senderId, string name)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		string name2 = name;
		if (Groups.ignoreList.Value.Split(new char[1] { ',' }).Any((string s) => string.Compare(s.Trim(), name2, StringComparison.OrdinalIgnoreCase) == 0) || Groups.blockInvitations.Value == Groups.BlockInvitation.Always)
		{
			return;
		}
		if (Groups.blockInvitations.Value == Groups.BlockInvitation.PvP)
		{
			Player localPlayer = Player.m_localPlayer;
			if (localPlayer != null && ((Character)localPlayer).IsPVPEnabled())
			{
				return;
			}
		}
		if (Groups.blockInvitations.Value == Groups.BlockInvitation.Enemy)
		{
			Player localPlayer2 = Player.m_localPlayer;
			if (localPlayer2 != null && ((Character)localPlayer2).IsPVPEnabled() && Player.s_players.Any(delegate(Player p)
			{
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)p != (Object)(object)Player.m_localPlayer && ((Character)p).IsPVPEnabled())
				{
					if (Groups.friendlyFire.Value != Groups.Toggle.On)
					{
						Group? ownGroup = Groups.ownGroup;
						if (ownGroup != null && ownGroup.playerStates.ContainsKey(PlayerReference.fromPlayer(p)))
						{
							goto IL_006a;
						}
					}
					return Vector3.Distance(((Component)Player.m_localPlayer).transform.position, ((Component)p).transform.position) < 30f;
				}
				goto IL_006a;
				IL_006a:
				return false;
			}))
			{
				return;
			}
		}
		pendingInvitationSenderId = senderId;
		string text = Localization.instance.Localize("$groups_invitation_received_description", new string[1] { name2 });
		object obj = _003C_003Ec._003C_003E9__8_2;
		if (obj == null)
		{
			PopupButtonCallback val = delegate
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Expected O, but got Unknown
				Groups.ownGroup?.Leave();
				UnifiedPopup.Pop();
				ZPackage val3 = new ZPackage();
				Group.PlayerState.fromLocal().write(val3);
				ZRoutedRpc.instance.InvokeRoutedRPC(pendingInvitationSenderId, "Groups AcceptInvitation", new object[1] { val3 });
			};
			_003C_003Ec._003C_003E9__8_2 = val;
			obj = (object)val;
		}
		object obj2 = _003C_003EO._003C0_003E__Pop;
		if (obj2 == null)
		{
			PopupButtonCallback val2 = UnifiedPopup.Pop;
			_003C_003EO._003C0_003E__Pop = val2;
			obj2 = (object)val2;
		}
		UnifiedPopup.Push((PopupBase)new YesNoPopup("$groups_invitation_received_title", text, (PopupButtonCallback)obj, (PopupButtonCallback)obj2, true));
		API.InvokeInvitationReceived(PlayerReference.fromPlayerId(senderId), UnifiedPopup.instance.popupUIParent);
	}

	private static void onForcedInvitationReceived(long senderId)
	{
		API.JoinGroup(PlayerReference.fromPlayerId(senderId));
	}

	private static void onInvitationAccepted(long senderId, ZPackage playerState)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		ZPackage group = new ZPackage();
		if (Groups.ownGroup != null && Groups.ownGroup.AddMember(PlayerReference.fromPlayerId(senderId), Group.PlayerState.read(playerState)))
		{
			AddPlayer(Groups.ownGroup.leader);
			group.Write(Groups.ownGroup.playerStates.Count - 1);
			foreach (PlayerReference item in Groups.ownGroup.playerStates.Keys.Where((PlayerReference p) => p != Groups.ownGroup.leader))
			{
				AddPlayer(item);
			}
			ZRoutedRpc.instance.InvokeRoutedRPC(senderId, "Groups AcceptInvitationResponse", new object[1] { group });
		}
		else
		{
			ZRoutedRpc.instance.InvokeRoutedRPC(senderId, "Groups AcceptInvitationResponse", new object[1] { group });
		}
		void AddPlayer(PlayerReference player)
		{
			group.Write(player.ToString());
			Groups.ownGroup.playerStates[player].write(group);
		}
	}

	private static void onInvitationAcceptedResponse(long senderId, ZPackage group)
	{
		if (group.Size() == 0)
		{
			((Terminal)Chat.instance).AddString(Localization.instance.Localize("$groups_joining_failed"));
			return;
		}
		Group group2 = new Group(PlayerReference.fromString(group.ReadString()), Group.PlayerState.read(group));
		int num = group.ReadInt();
		for (int i = 0; i < num; i++)
		{
			group2.playerStates.Add(PlayerReference.fromString(group.ReadString()), Group.PlayerState.read(group));
		}
		Groups.ownGroup = group2;
		API.InvokeGroupJoined();
		ChatCommands.UpdateAutoCompletion();
	}

	private static void onUpdatePosition(long senderId, Vector3 position)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		List<PlayerInfo> list = new List<PlayerInfo>();
		foreach (PlayerInfo player in ZNet.instance.m_players)
		{
			PlayerInfo current = player;
			if (((ZDOID)(ref current.m_characterID)).UserID == senderId)
			{
				current.m_position = position;
			}
			list.Add(current);
		}
		ZNet.instance.m_players = list;
	}
}
