using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

public static class Map
{
	[HarmonyPatch(typeof(Game), "RequestRespawn")]
	private static class UpdateGuildIcon
	{
		private static void Postfix()
		{
			UpdateMapPinColor();
		}
	}

	[HarmonyPatch(typeof(Chat), "SendPing")]
	private static class RestrictPingsToGuildOnModifierHeld
	{
		[UsedImplicitly]
		private static bool RestrictBroadcast(ZRoutedRpc instance, long targetPeerId, string methodName, params object[] parameters)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			Guild ownGuild = API.GetOwnGuild();
			if (ownGuild != null && targetPeerId == ZRoutedRpc.Everybody)
			{
				KeyboardShortcut value = Guilds.guildPingHotkey.Value;
				if (((KeyboardShortcut)(ref value)).IsPressed())
				{
					foreach (PlayerInfo player in ZNet.instance.m_players)
					{
						if (ownGuild.Members.ContainsKey(PlayerReference.fromPlayerInfo(player)))
						{
							ZDOID characterID = player.m_characterID;
							instance.InvokeRoutedRPC(((ZDOID)(ref characterID)).UserID, "Guilds MapPing", parameters);
						}
					}
					return true;
				}
			}
			return false;
		}

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructionsEnumerable, ILGenerator ilg)
		{
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Expected O, but got Unknown
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Expected O, but got Unknown
			MethodInfo routedRPC = AccessTools.DeclaredMethod(typeof(ZRoutedRpc), "InvokeRoutedRPC", new Type[3]
			{
				typeof(long),
				typeof(string),
				typeof(object[])
			}, (Type[])null);
			MethodInfo routedRPCInstance = AccessTools.DeclaredPropertyGetter(typeof(ZRoutedRpc), "instance");
			List<CodeInstruction> list = instructionsEnumerable.ToList();
			int num = list.FindIndex((CodeInstruction i) => CodeInstructionExtensions.Calls(i, routedRPC));
			int num2 = list.FindLastIndex(num, (CodeInstruction i) => CodeInstructionExtensions.Calls(i, routedRPCInstance));
			Label label = ilg.DefineLabel();
			list[num + 1].labels.Add(label);
			list.InsertRange(num2, list.Skip(num2).Take(num - num2).Concat((IEnumerable<CodeInstruction>)(object)new CodeInstruction[2]
			{
				new CodeInstruction(OpCodes.Call, (object)AccessTools.DeclaredMethod(typeof(RestrictPingsToGuildOnModifierHeld), "RestrictBroadcast", (Type[])null, (Type[])null)),
				new CodeInstruction(OpCodes.Brtrue, (object)label)
			})
				.ToArray());
			return list;
		}
	}

	[HarmonyPatch(typeof(Chat), "RPC_ChatMessage")]
	private class ClearGuildPing
	{
		public static void Prefix(Chat __instance, long sender)
		{
			WorldTextInstance val = __instance.FindExistingWorldText(sender);
			if (val == null || !guildPingTexts.Remove(val) || !Object.op_Implicit((Object)(object)Minimap.instance))
			{
				return;
			}
			for (int i = 0; i < Minimap.instance.m_tempShouts.Count; i++)
			{
				PinData val2 = Minimap.instance.m_pingPins[i];
				if (Minimap.instance.m_tempShouts[i] == val)
				{
					val2.m_icon = Minimap.instance.GetSprite((PinType)12);
					if (Object.op_Implicit((Object)(object)val2.m_iconElement))
					{
						val2.m_iconElement.sprite = val2.m_icon;
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(Minimap), "UpdatePlayerPins")]
	private class ChangeGuildMemberPin
	{
		[HarmonyPriority(500)]
		private static void Postfix(Minimap __instance)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < __instance.m_tempPlayerInfo.Count; i++)
			{
				PinData val = __instance.m_playerPins[i];
				PlayerInfo val2 = __instance.m_tempPlayerInfo[i];
				if (val.m_name == val2.m_name)
				{
					bool flag = false;
					Guild? ownGuild = API.GetOwnGuild();
					if (ownGuild != null && ownGuild.Members.ContainsKey(PlayerReference.fromPlayerInfo(val2)))
					{
						val.m_icon = guildMapPlayerIcon;
						flag = true;
					}
					else if ((Object)(object)val.m_icon == (Object)(object)guildMapPlayerIcon)
					{
						val.m_icon = __instance.GetSprite((PinType)10);
						flag = true;
					}
					if (flag && Object.op_Implicit((Object)(object)val.m_iconElement))
					{
						val.m_iconElement.sprite = val.m_icon;
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(Minimap), "UpdatePingPins")]
	private class ChangeGuildMemberPing
	{
		private static void Postfix(Minimap __instance)
		{
			for (int i = 0; i < __instance.m_tempShouts.Count; i++)
			{
				PinData val = __instance.m_pingPins[i];
				WorldTextInstance key = __instance.m_tempShouts[i];
				if (guildPingTexts.TryGetValue(key, out object _))
				{
					val.m_icon = guildMapPingIcon;
					if (Object.op_Implicit((Object)(object)val.m_iconElement))
					{
						val.m_iconElement.sprite = val.m_icon;
					}
				}
			}
		}
	}

	[HarmonyPatch]
	private class PreservePlayerPosition
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
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			__state = new Dictionary<long, Vector3>();
			Guild guild = API.GetOwnGuild();
			if (guild == null)
			{
				return;
			}
			foreach (PlayerInfo item in __instance.m_players.Where((PlayerInfo p) => guild.Members.ContainsKey(PlayerReference.fromPlayerInfo(p))))
			{
				Dictionary<long, Vector3> obj = __state;
				ZDOID characterID = item.m_characterID;
				obj[((ZDOID)(ref characterID)).UserID] = item.m_position;
			}
		}

		private static void Postfix(ZNet __instance, Dictionary<long, Vector3> __state)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			Guild ownGuild = API.GetOwnGuild();
			if (ownGuild == null)
			{
				return;
			}
			List<PlayerInfo> list = new List<PlayerInfo>();
			foreach (PlayerInfo player in __instance.m_players)
			{
				PlayerInfo val = player;
				if (ownGuild.Members.ContainsKey(PlayerReference.fromPlayerInfo(val)))
				{
					ZDOID characterID = val.m_characterID;
					Player localPlayer = Player.m_localPlayer;
					ZDOID? val2 = ((localPlayer != null) ? new ZDOID?(((Character)localPlayer).GetZDOID()) : null);
					if (!val2.HasValue || characterID != val2.GetValueOrDefault())
					{
						characterID = player.m_characterID;
						if (__state.TryGetValue(((ZDOID)(ref characterID)).UserID, out var value) && !player.m_publicPosition)
						{
							val.m_position = value;
						}
						val.m_publicPosition = true;
					}
				}
				list.Add(val);
			}
			__instance.m_players = list;
		}
	}

	private static Sprite guildMapPlayerIcon = null;

	private static Sprite guildMapPingIcon = null;

	private static readonly ConditionalWeakTable<WorldTextInstance, object> guildPingTexts = new ConditionalWeakTable<WorldTextInstance, object>();

	private static readonly Color defaultColor = new Color(1f, 61f / 85f, 49f / 136f);

	public static void Init()
	{
		guildMapPlayerIcon = Tools.loadSprite("guildPlayerIcon.png", 64, 64);
		guildMapPingIcon = Tools.loadSprite("guildMapPingIcon.png", 64, 64);
	}

	public static void onMapPing(long senderId, Vector3 position, int type, UserInfo name, string text)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Guild ownGuild = API.GetOwnGuild();
		if (ownGuild != null)
		{
			Color color = default(Color);
			ColorUtility.TryParseHtmlString(ownGuild.General.color, ref color);
			Chat.instance.RPC_ChatMessage(senderId, position, type, name, text);
			WorldTextInstance val = Chat.instance.FindExistingWorldText(senderId);
			((Graphic)val.m_textMeshField).color = color;
			guildPingTexts.Add(val, Array.Empty<object>());
		}
	}

	internal static void UpdateMapPinColor()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		Guild ownGuild = API.GetOwnGuild();
		if (ownGuild == null)
		{
			return;
		}
		Color val = default(Color);
		ColorUtility.TryParseHtmlString(ownGuild.General.color, ref val);
		Color[] pixels = Tools.loadTexture("guildPlayerIcon.png").GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			if ((double)pixels[i].r > 0.5 && (double)pixels[i].b < 0.5 && (double)pixels[i].g < 0.5)
			{
				pixels[i] = val;
			}
		}
		guildMapPlayerIcon.texture.SetPixels(pixels);
		guildMapPlayerIcon.texture.Apply();
		pixels = Tools.loadTexture("guildMapPingIcon.png").GetPixels();
		for (int j = 0; j < pixels.Length; j++)
		{
			if ((double)pixels[j].r > 0.5 && (double)pixels[j].b < 0.5 && (double)pixels[j].g < 0.5)
			{
				pixels[j].b = val.b;
				pixels[j].g = val.g;
				pixels[j].r = val.r;
			}
		}
		guildMapPingIcon.texture.SetPixels(pixels);
		guildMapPingIcon.texture.Apply();
	}

	internal static void onUpdatePosition(long senderId, Vector3 position)
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
