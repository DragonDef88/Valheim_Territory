using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Guilds;

public static class GuildChat
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

			internal void _003CPostfix_003Eb__0_0(ConsoleEventArgs args)
			{
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Unknown result type (might be due to invalid IL or missing references)
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_0077: Unknown result type (might be due to invalid IL or missing references)
				//IL_0089: Unknown result type (might be due to invalid IL or missing references)
				//IL_008a: Unknown result type (might be due to invalid IL or missing references)
				//IL_008f: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)Chat.instance == (Object)null)
				{
					return;
				}
				Guild ownGuild = API.GetOwnGuild();
				if (ownGuild == null)
				{
					args.Context.AddString("You are not in a guild.");
					return;
				}
				if (args.FullLine.Length > 2)
				{
					string text = args.FullLine.Substring(2);
					{
						foreach (PlayerInfo player in ZNet.instance.m_players)
						{
							ZDOID characterID = player.m_characterID;
							if (((ZDOID)(ref characterID)).UserID != 0L && ownGuild.Members.ContainsKey(PlayerReference.fromPlayerInfo(player)))
							{
								ZRoutedRpc instance = ZRoutedRpc.instance;
								characterID = player.m_characterID;
								instance.InvokeRoutedRPC(((ZDOID)(ref characterID)).UserID, "Guilds ChatMessage", new object[2]
								{
									UserInfo.GetLocalUser(),
									text
								});
							}
						}
						return;
					}
				}
				ToggleGuildsChat(!guildChatActive);
			}
		}

		private static void Postfix()
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			guildChatPlaceholder = Localization.instance.Localize("$guilds_guild_chat_placeholder");
			object obj = _003C_003Ec._003C_003E9__0_0;
			if (obj == null)
			{
				ConsoleEvent val = delegate(ConsoleEventArgs args)
				{
					//IL_005a: Unknown result type (might be due to invalid IL or missing references)
					//IL_005f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0060: Unknown result type (might be due to invalid IL or missing references)
					//IL_0061: Unknown result type (might be due to invalid IL or missing references)
					//IL_0066: Unknown result type (might be due to invalid IL or missing references)
					//IL_0077: Unknown result type (might be due to invalid IL or missing references)
					//IL_0089: Unknown result type (might be due to invalid IL or missing references)
					//IL_008a: Unknown result type (might be due to invalid IL or missing references)
					//IL_008f: Unknown result type (might be due to invalid IL or missing references)
					if (!((Object)(object)Chat.instance == (Object)null))
					{
						Guild ownGuild = API.GetOwnGuild();
						if (ownGuild == null)
						{
							args.Context.AddString("You are not in a guild.");
						}
						else
						{
							if (args.FullLine.Length > 2)
							{
								string text = args.FullLine.Substring(2);
								{
									foreach (PlayerInfo player in ZNet.instance.m_players)
									{
										ZDOID characterID = player.m_characterID;
										if (((ZDOID)(ref characterID)).UserID != 0L && ownGuild.Members.ContainsKey(PlayerReference.fromPlayerInfo(player)))
										{
											ZRoutedRpc instance = ZRoutedRpc.instance;
											characterID = player.m_characterID;
											instance.InvokeRoutedRPC(((ZDOID)(ref characterID)).UserID, "Guilds ChatMessage", new object[2]
											{
												UserInfo.GetLocalUser(),
												text
											});
										}
									}
									return;
								}
							}
							ToggleGuildsChat(!guildChatActive);
						}
					}
				};
				_003C_003Ec._003C_003E9__0_0 = val;
				obj = (object)val;
			}
			new ConsoleCommand("g", "toggles the guild chat on", (ConsoleEvent)obj, false, false, false, false, false, (ConsoleOptionsFetcher)null, false, false, false);
		}
	}

	[HarmonyPatch(typeof(Chat), "Awake")]
	public class AddGuildChat
	{
		private static void Postfix(Chat __instance)
		{
			int index = Math.Max(0, ((Terminal)__instance).m_chatBuffer.Count - 5);
			((Terminal)__instance).m_chatBuffer.Insert(index, "/g [text] Guild chat");
			((Terminal)__instance).m_chatBuffer.Insert(index, "/g Toggle guild chat");
			((Terminal)__instance).UpdateChat();
		}
	}

	[HarmonyPatch(typeof(Chat), "InputText")]
	public class SendMessageToGuild
	{
		private static void Prefix(Chat __instance)
		{
			if (((TMP_InputField)((Terminal)__instance).m_input).text.Length != 0 && guildChatActive && ((TMP_InputField)((Terminal)__instance).m_input).text[0] != '/')
			{
				((TMP_InputField)((Terminal)__instance).m_input).text = "/g " + ((TMP_InputField)((Terminal)__instance).m_input).text;
			}
		}
	}

	[HarmonyPatch(typeof(Game), "Start")]
	private static class AddRPCs
	{
		private static void Postfix()
		{
			ZRoutedRpc.instance.Register<UserInfo, string>("Guilds ChatMessage", (Action<long, UserInfo, string>)onChatMessageReceived);
			ZRoutedRpc.instance.Register<Vector3, int, UserInfo, string>("Guilds MapPing", (Method<Vector3, int, UserInfo, string>)Map.onMapPing);
			ZRoutedRpc.instance.Register<Vector3>("Guilds UpdatePosition", (Action<long, Vector3>)Map.onUpdatePosition);
		}
	}

	private static string guildChatPlaceholder;

	private static bool guildChatActive
	{
		get
		{
			if (Object.op_Implicit((Object)(object)Chat.instance))
			{
				return ((TMP_Text)((Component)((Component)((Terminal)Chat.instance).m_input).transform.Find("Text Area/Placeholder")).GetComponent<TextMeshProUGUI>()).text == guildChatPlaceholder;
			}
			return false;
		}
	}

	public static void ToggleGuildsChat(bool active)
	{
		if (Object.op_Implicit((Object)(object)Chat.instance))
		{
			TextMeshProUGUI component = ((Component)((Component)((Terminal)Chat.instance).m_input).transform.Find("Text Area/Placeholder")).GetComponent<TextMeshProUGUI>();
			if (active)
			{
				((TMP_Text)component).text = guildChatPlaceholder;
				Localization.instance.textMeshStrings[(TMP_Text)(object)component] = guildChatPlaceholder;
			}
			else if (((TMP_Text)component).text == guildChatPlaceholder)
			{
				((TMP_Text)component).text = Localization.instance.Localize("$chat_entertext");
				Localization.instance.textMeshStrings[(TMP_Text)(object)component] = "$chat_entertext";
			}
		}
	}

	private static void onChatMessageReceived(long senderId, UserInfo name, string message)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		string text = ((Guilds.guildColors.Value == Toggle.Off) ? ("#" + ColorUtility.ToHtmlStringRGBA(Guilds.guildChatColor.Value)) : API.GetOwnGuild().General.color);
		((Terminal)Chat.instance).AddString("<color=orange>" + name.Name + "</color>: <color=" + text + ">" + message + "</color>");
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
				Chat.instance.AddInworldText(val, senderId, ((Character)component).GetHeadPoint(), (Type)1, name, "<color=" + text + ">" + message + "</color>");
			}
		}
	}
}
