using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Groups;

public static class ChatCommands
{
	[HarmonyPatch(typeof(Terminal), "InitTerminal")]
	public class AddChatCommands
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static ConsoleEvent _003C_003E9__0_0;

			public static Func<PlayerInfo, string> _003C_003E9__0_9;

			public static ConsoleOptionsFetcher _003C_003E9__0_1;

			public static ConsoleEvent _003C_003E9__0_2;

			public static Func<PlayerReference, string> _003C_003E9__0_11;

			public static Func<string, bool> _003C_003E9__0_12;

			public static ConsoleOptionsFetcher _003C_003E9__0_3;

			public static ConsoleEvent _003C_003E9__0_4;

			public static Func<PlayerReference, string> _003C_003E9__0_14;

			public static Func<string, bool> _003C_003E9__0_15;

			public static ConsoleOptionsFetcher _003C_003E9__0_5;

			public static ConsoleEvent _003C_003E9__0_6;

			public static ConsoleEvent _003C_003E9__0_7;

			internal void _003CPostfix_003Eb__0_0(ConsoleEventArgs args)
			{
				//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
				_003C_003Ec__DisplayClass0_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass0_0();
				if (args.FullLine.Length <= "invite".Length || (Object)(object)Chat.instance == (Object)null)
				{
					return;
				}
				Player localPlayer = Player.m_localPlayer;
				if (localPlayer == null || ((Character)localPlayer).m_nview.GetZDO().GetBool("dead", false))
				{
					args.Context.AddString(Localization.instance.Localize("$groups_dead"));
					return;
				}
				CS_0024_003C_003E8__locals0.playerName = args.FullLine.Substring(7);
				if (string.Compare(CS_0024_003C_003E8__locals0.playerName, ((Character)Player.m_localPlayer).GetHoverName(), StringComparison.OrdinalIgnoreCase) == 0)
				{
					args.Context.AddString(Localization.instance.Localize("$groups_cannot_invite_self"));
					return;
				}
				PlayerInfo val = ((IEnumerable<PlayerInfo>)ZNet.instance.m_players).FirstOrDefault((Func<PlayerInfo, bool>)((PlayerInfo p) => string.Compare(CS_0024_003C_003E8__locals0.playerName, p.m_name, StringComparison.OrdinalIgnoreCase) == 0));
				long userID = ((ZDOID)(ref val.m_characterID)).UserID;
				if (userID == 0L)
				{
					args.Context.AddString(Localization.instance.Localize("$groups_player_not_online", new string[1] { CS_0024_003C_003E8__locals0.playerName }));
					return;
				}
				if (Groups.ownGroup == null)
				{
					Groups.ownGroup = new Group(PlayerReference.fromPlayer(Player.m_localPlayer), Group.PlayerState.fromLocal());
					API.InvokeGroupJoined();
				}
				if (Groups.ownGroup.leader == PlayerReference.fromPlayer(Player.m_localPlayer))
				{
					ZRoutedRpc.instance.InvokeRoutedRPC(userID, "Groups InvitePlayer", new object[1] { ((Character)Player.m_localPlayer).GetHoverName() });
					args.Context.AddString(Localization.instance.Localize("$groups_invitation_sent", new string[1] { CS_0024_003C_003E8__locals0.playerName }));
				}
				else
				{
					args.Context.AddString(Localization.instance.Localize("$groups_leader_can_invite"));
				}
			}

			internal List<string> _003CPostfix_003Eb__0_1()
			{
				return ZNet.instance.m_players.Select((PlayerInfo p) => p.m_name).ToList();
			}

			internal string _003CPostfix_003Eb__0_9(PlayerInfo p)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				return p.m_name;
			}

			internal void _003CPostfix_003Eb__0_2(ConsoleEventArgs args)
			{
				_003C_003Ec__DisplayClass0_1 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass0_1();
				if (args.FullLine.Length <= "remove".Length || (Object)(object)Chat.instance == (Object)null)
				{
					return;
				}
				if (Groups.ownGroup == null)
				{
					args.Context.AddString(Localization.instance.Localize("$groups_not_in_group"));
					return;
				}
				if (Groups.ownGroup.leader != PlayerReference.fromPlayer(Player.m_localPlayer))
				{
					args.Context.AddString(Localization.instance.Localize("$groups_leader_can_remove"));
					return;
				}
				CS_0024_003C_003E8__locals0.playerName = args.FullLine.Substring(7);
				if (string.Compare(CS_0024_003C_003E8__locals0.playerName, ((Character)Player.m_localPlayer).GetHoverName(), StringComparison.OrdinalIgnoreCase) == 0)
				{
					args.Context.AddString(Localization.instance.Localize("$groups_removed_self"));
				}
				else if (!Groups.ownGroup.RemoveMember(Groups.ownGroup.playerStates.Keys.FirstOrDefault((PlayerReference p) => string.Compare(p.name, CS_0024_003C_003E8__locals0.playerName, StringComparison.OrdinalIgnoreCase) == 0)))
				{
					args.Context.AddString(Localization.instance.Localize("$groups_target_not_in_group", new string[1] { CS_0024_003C_003E8__locals0.playerName }));
				}
			}

			internal List<string> _003CPostfix_003Eb__0_3()
			{
				return (from p in Groups.ownGroup?.playerStates.Keys
					select p.name into n
					where n != ((Character)Player.m_localPlayer).GetHoverName()
					select n).ToList() ?? new List<string>();
			}

			internal string _003CPostfix_003Eb__0_11(PlayerReference p)
			{
				return p.name;
			}

			internal bool _003CPostfix_003Eb__0_12(string n)
			{
				return n != ((Character)Player.m_localPlayer).GetHoverName();
			}

			internal void _003CPostfix_003Eb__0_4(ConsoleEventArgs args)
			{
				_003C_003Ec__DisplayClass0_2 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass0_2();
				if (args.FullLine.Length <= "promote".Length || (Object)(object)Chat.instance == (Object)null)
				{
					return;
				}
				if (Groups.ownGroup == null)
				{
					args.Context.AddString(Localization.instance.Localize("$groups_not_in_group"));
					return;
				}
				if (Groups.ownGroup.leader != PlayerReference.fromPlayer(Player.m_localPlayer))
				{
					args.Context.AddString(Localization.instance.Localize("$groups_leader_can_promote"));
					return;
				}
				CS_0024_003C_003E8__locals0.playerName = args.FullLine.Substring(8);
				if (!Groups.ownGroup.PromoteMember(Groups.ownGroup.playerStates.Keys.FirstOrDefault((PlayerReference p) => string.Compare(p.name, CS_0024_003C_003E8__locals0.playerName, StringComparison.OrdinalIgnoreCase) == 0)))
				{
					args.Context.AddString(Localization.instance.Localize("$groups_target_not_in_group", new string[1] { CS_0024_003C_003E8__locals0.playerName }));
				}
			}

			internal List<string> _003CPostfix_003Eb__0_5()
			{
				return (from p in Groups.ownGroup?.playerStates.Keys
					select p.name into n
					where n != ((Character)Player.m_localPlayer).GetHoverName()
					select n).ToList() ?? new List<string>();
			}

			internal string _003CPostfix_003Eb__0_14(PlayerReference p)
			{
				return p.name;
			}

			internal bool _003CPostfix_003Eb__0_15(string n)
			{
				return n != ((Character)Player.m_localPlayer).GetHoverName();
			}

			internal void _003CPostfix_003Eb__0_6(ConsoleEventArgs args)
			{
				if (args.FullLine.Length >= "leave".Length && !((Object)(object)Chat.instance == (Object)null))
				{
					if (Groups.ownGroup == null)
					{
						args.Context.AddString(Localization.instance.Localize("$groups_not_in_group"));
					}
					else
					{
						Groups.ownGroup.Leave();
					}
				}
			}

			internal void _003CPostfix_003Eb__0_7(ConsoleEventArgs args)
			{
				if ((Object)(object)Chat.instance == (Object)null)
				{
					return;
				}
				if (Groups.ownGroup == null)
				{
					args.Context.AddString(Localization.instance.Localize("$groups_not_in_group"));
					return;
				}
				if (args.FullLine.Length > 2)
				{
					string text = args.FullLine.Substring(2);
					{
						foreach (PlayerReference key in Groups.ownGroup.playerStates.Keys)
						{
							ZRoutedRpc.instance.InvokeRoutedRPC(key.peerId, "Groups ChatMessage", new object[2]
							{
								UserInfo.GetLocalUser(),
								text
							});
						}
						return;
					}
				}
				ToggleGroupsChat(!groupChatActive);
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass0_0
		{
			public string playerName;

			internal bool _003CPostfix_003Eb__8(PlayerInfo p)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return string.Compare(playerName, p.m_name, StringComparison.OrdinalIgnoreCase) == 0;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass0_1
		{
			public string playerName;

			internal bool _003CPostfix_003Eb__10(PlayerReference p)
			{
				return string.Compare(p.name, playerName, StringComparison.OrdinalIgnoreCase) == 0;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass0_2
		{
			public string playerName;

			internal bool _003CPostfix_003Eb__13(PlayerReference p)
			{
				return string.Compare(p.name, playerName, StringComparison.OrdinalIgnoreCase) == 0;
			}
		}

		private static void Postfix()
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Expected O, but got Unknown
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Expected O, but got Unknown
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Expected O, but got Unknown
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Expected O, but got Unknown
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Expected O, but got Unknown
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Expected O, but got Unknown
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Expected O, but got Unknown
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Expected O, but got Unknown
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Expected O, but got Unknown
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Expected O, but got Unknown
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Expected O, but got Unknown
			groupChatPlaceholder = Localization.instance.Localize("$groups_chat_placeholder");
			ChatCommands.terminalCommands.Clear();
			List<ConsoleCommand> terminalCommands = ChatCommands.terminalCommands;
			object obj = _003C_003Ec._003C_003E9__0_0;
			if (obj == null)
			{
				ConsoleEvent val = delegate(ConsoleEventArgs args)
				{
					//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
					//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
					if (args.FullLine.Length > "invite".Length && !((Object)(object)Chat.instance == (Object)null))
					{
						Player localPlayer = Player.m_localPlayer;
						if (localPlayer == null || ((Character)localPlayer).m_nview.GetZDO().GetBool("dead", false))
						{
							args.Context.AddString(Localization.instance.Localize("$groups_dead"));
						}
						else
						{
							string playerName3 = args.FullLine.Substring(7);
							if (string.Compare(playerName3, ((Character)Player.m_localPlayer).GetHoverName(), StringComparison.OrdinalIgnoreCase) == 0)
							{
								args.Context.AddString(Localization.instance.Localize("$groups_cannot_invite_self"));
							}
							else
							{
								PlayerInfo val9 = ((IEnumerable<PlayerInfo>)ZNet.instance.m_players).FirstOrDefault((Func<PlayerInfo, bool>)((PlayerInfo p) => string.Compare(playerName3, p.m_name, StringComparison.OrdinalIgnoreCase) == 0));
								long userID = ((ZDOID)(ref val9.m_characterID)).UserID;
								if (userID == 0L)
								{
									args.Context.AddString(Localization.instance.Localize("$groups_player_not_online", new string[1] { playerName3 }));
								}
								else
								{
									if (Groups.ownGroup == null)
									{
										Groups.ownGroup = new Group(PlayerReference.fromPlayer(Player.m_localPlayer), Group.PlayerState.fromLocal());
										API.InvokeGroupJoined();
									}
									if (Groups.ownGroup.leader == PlayerReference.fromPlayer(Player.m_localPlayer))
									{
										ZRoutedRpc.instance.InvokeRoutedRPC(userID, "Groups InvitePlayer", new object[1] { ((Character)Player.m_localPlayer).GetHoverName() });
										args.Context.AddString(Localization.instance.Localize("$groups_invitation_sent", new string[1] { playerName3 }));
									}
									else
									{
										args.Context.AddString(Localization.instance.Localize("$groups_leader_can_invite"));
									}
								}
							}
						}
					}
				};
				_003C_003Ec._003C_003E9__0_0 = val;
				obj = (object)val;
			}
			object obj2 = _003C_003Ec._003C_003E9__0_1;
			if (obj2 == null)
			{
				ConsoleOptionsFetcher val2 = () => ZNet.instance.m_players.Select((PlayerInfo p) => p.m_name).ToList();
				_003C_003Ec._003C_003E9__0_1 = val2;
				obj2 = (object)val2;
			}
			terminalCommands.Add(new ConsoleCommand("invite", "invite someone to your group", (ConsoleEvent)obj, false, false, false, false, false, (ConsoleOptionsFetcher)obj2, false, false, false));
			List<ConsoleCommand> terminalCommands2 = ChatCommands.terminalCommands;
			object obj3 = _003C_003Ec._003C_003E9__0_2;
			if (obj3 == null)
			{
				ConsoleEvent val3 = delegate(ConsoleEventArgs args)
				{
					if (args.FullLine.Length > "remove".Length && !((Object)(object)Chat.instance == (Object)null))
					{
						if (Groups.ownGroup == null)
						{
							args.Context.AddString(Localization.instance.Localize("$groups_not_in_group"));
						}
						else if (Groups.ownGroup.leader != PlayerReference.fromPlayer(Player.m_localPlayer))
						{
							args.Context.AddString(Localization.instance.Localize("$groups_leader_can_remove"));
						}
						else
						{
							string playerName2 = args.FullLine.Substring(7);
							if (string.Compare(playerName2, ((Character)Player.m_localPlayer).GetHoverName(), StringComparison.OrdinalIgnoreCase) == 0)
							{
								args.Context.AddString(Localization.instance.Localize("$groups_removed_self"));
							}
							else if (!Groups.ownGroup.RemoveMember(Groups.ownGroup.playerStates.Keys.FirstOrDefault((PlayerReference p) => string.Compare(p.name, playerName2, StringComparison.OrdinalIgnoreCase) == 0)))
							{
								args.Context.AddString(Localization.instance.Localize("$groups_target_not_in_group", new string[1] { playerName2 }));
							}
						}
					}
				};
				_003C_003Ec._003C_003E9__0_2 = val3;
				obj3 = (object)val3;
			}
			object obj4 = _003C_003Ec._003C_003E9__0_3;
			if (obj4 == null)
			{
				ConsoleOptionsFetcher val4 = () => (from p in Groups.ownGroup?.playerStates.Keys
					select p.name into n
					where n != ((Character)Player.m_localPlayer).GetHoverName()
					select n).ToList() ?? new List<string>();
				_003C_003Ec._003C_003E9__0_3 = val4;
				obj4 = (object)val4;
			}
			terminalCommands2.Add(new ConsoleCommand("remove", "removes someone from your group", (ConsoleEvent)obj3, false, false, false, false, false, (ConsoleOptionsFetcher)obj4, false, false, false));
			List<ConsoleCommand> terminalCommands3 = ChatCommands.terminalCommands;
			object obj5 = _003C_003Ec._003C_003E9__0_4;
			if (obj5 == null)
			{
				ConsoleEvent val5 = delegate(ConsoleEventArgs args)
				{
					if (args.FullLine.Length > "promote".Length && !((Object)(object)Chat.instance == (Object)null))
					{
						if (Groups.ownGroup == null)
						{
							args.Context.AddString(Localization.instance.Localize("$groups_not_in_group"));
						}
						else if (Groups.ownGroup.leader != PlayerReference.fromPlayer(Player.m_localPlayer))
						{
							args.Context.AddString(Localization.instance.Localize("$groups_leader_can_promote"));
						}
						else
						{
							string playerName = args.FullLine.Substring(8);
							if (!Groups.ownGroup.PromoteMember(Groups.ownGroup.playerStates.Keys.FirstOrDefault((PlayerReference p) => string.Compare(p.name, playerName, StringComparison.OrdinalIgnoreCase) == 0)))
							{
								args.Context.AddString(Localization.instance.Localize("$groups_target_not_in_group", new string[1] { playerName }));
							}
						}
					}
				};
				_003C_003Ec._003C_003E9__0_4 = val5;
				obj5 = (object)val5;
			}
			object obj6 = _003C_003Ec._003C_003E9__0_5;
			if (obj6 == null)
			{
				ConsoleOptionsFetcher val6 = () => (from p in Groups.ownGroup?.playerStates.Keys
					select p.name into n
					where n != ((Character)Player.m_localPlayer).GetHoverName()
					select n).ToList() ?? new List<string>();
				_003C_003Ec._003C_003E9__0_5 = val6;
				obj6 = (object)val6;
			}
			terminalCommands3.Add(new ConsoleCommand("promote", "promotes someone to group leader", (ConsoleEvent)obj5, false, false, false, false, false, (ConsoleOptionsFetcher)obj6, false, false, false));
			object obj7 = _003C_003Ec._003C_003E9__0_6;
			if (obj7 == null)
			{
				ConsoleEvent val7 = delegate(ConsoleEventArgs args)
				{
					if (args.FullLine.Length >= "leave".Length && !((Object)(object)Chat.instance == (Object)null))
					{
						if (Groups.ownGroup == null)
						{
							args.Context.AddString(Localization.instance.Localize("$groups_not_in_group"));
						}
						else
						{
							Groups.ownGroup.Leave();
						}
					}
				};
				_003C_003Ec._003C_003E9__0_6 = val7;
				obj7 = (object)val7;
			}
			new ConsoleCommand("leave", "leaves your current group", (ConsoleEvent)obj7, false, false, false, false, false, (ConsoleOptionsFetcher)null, false, false, false);
			object obj8 = _003C_003Ec._003C_003E9__0_7;
			if (obj8 == null)
			{
				ConsoleEvent val8 = delegate(ConsoleEventArgs args)
				{
					if (!((Object)(object)Chat.instance == (Object)null))
					{
						if (Groups.ownGroup == null)
						{
							args.Context.AddString(Localization.instance.Localize("$groups_not_in_group"));
						}
						else
						{
							if (args.FullLine.Length > 2)
							{
								string text = args.FullLine.Substring(2);
								{
									foreach (PlayerReference key in Groups.ownGroup.playerStates.Keys)
									{
										ZRoutedRpc.instance.InvokeRoutedRPC(key.peerId, "Groups ChatMessage", new object[2]
										{
											UserInfo.GetLocalUser(),
											text
										});
									}
									return;
								}
							}
							ToggleGroupsChat(!groupChatActive);
						}
					}
				};
				_003C_003Ec._003C_003E9__0_7 = val8;
				obj8 = (object)val8;
			}
			new ConsoleCommand("p", "toggles the group chat on", (ConsoleEvent)obj8, false, false, false, false, false, (ConsoleOptionsFetcher)null, false, false, false);
		}
	}

	[HarmonyPatch(typeof(Chat), "Awake")]
	public class AddGroupChat
	{
		private static void Postfix(Chat __instance)
		{
			int index = Math.Max(0, ((Terminal)__instance).m_chatBuffer.Count - 5);
			((Terminal)__instance).m_chatBuffer.Insert(index, Localization.instance.Localize("$groups_group_chat_message_hint"));
			((Terminal)__instance).m_chatBuffer.Insert(index, Localization.instance.Localize("$groups_group_chat_toggle_hint"));
			((Terminal)__instance).UpdateChat();
		}
	}

	[HarmonyPatch(typeof(Chat), "InputText")]
	public class SendMessageToGroup
	{
		private static void Prefix(Chat __instance)
		{
			if (((TMP_InputField)((Terminal)__instance).m_input).text.Length != 0 && groupChatActive && ((TMP_InputField)((Terminal)__instance).m_input).text[0] != '/')
			{
				((TMP_InputField)((Terminal)__instance).m_input).text = "/p " + ((TMP_InputField)((Terminal)__instance).m_input).text;
			}
		}
	}

	private static string groupChatPlaceholder = null;

	private static readonly List<ConsoleCommand> terminalCommands = new List<ConsoleCommand>();

	private static bool groupChatActive
	{
		get
		{
			if (Object.op_Implicit((Object)(object)Chat.instance))
			{
				return ((TMP_Text)((Component)((Component)((Terminal)Chat.instance).m_input).transform.Find("Text Area/Placeholder")).GetComponent<TextMeshProUGUI>()).text == groupChatPlaceholder;
			}
			return false;
		}
	}

	public static void ToggleGroupsChat(bool active)
	{
		if (Object.op_Implicit((Object)(object)Chat.instance))
		{
			TextMeshProUGUI component = ((Component)((Component)((Terminal)Chat.instance).m_input).transform.Find("Text Area/Placeholder")).GetComponent<TextMeshProUGUI>();
			if (active)
			{
				((TMP_Text)component).text = groupChatPlaceholder;
				Localization.instance.textMeshStrings[(TMP_Text)(object)component] = groupChatPlaceholder;
			}
			else if (((TMP_Text)component).text == groupChatPlaceholder)
			{
				((TMP_Text)component).text = Localization.instance.Localize("$chat_entertext");
				Localization.instance.textMeshStrings[(TMP_Text)(object)component] = "$chat_entertext";
			}
		}
	}

	public static void UpdateAutoCompletion()
	{
		foreach (ConsoleCommand terminalCommand in terminalCommands)
		{
			terminalCommand.m_tabOptions = null;
		}
	}
}
