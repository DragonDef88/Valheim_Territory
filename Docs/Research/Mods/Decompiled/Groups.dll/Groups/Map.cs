using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Groups;

public static class Map
{
	[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
	public class ColorNames
	{
		private static void Postfix(Character c, Dictionary<Character, HudData> ___m_huds)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			Player val = (Player)(object)((c is Player) ? c : null);
			if (val != null)
			{
				((Graphic)((Component)___m_huds[c].m_gui.transform.Find("Name")).GetComponent<TextMeshProUGUI>()).color = ((Groups.ownGroup != null && Groups.ownGroup.playerStates.ContainsKey(PlayerReference.fromPlayer(val))) ? Groups.friendlyNameColor.Value : defaultColor);
			}
		}
	}

	[HarmonyPatch(typeof(Chat), "SendPing")]
	private static class RestrictPingsToGroupOnModifierHeld
	{
		[UsedImplicitly]
		private static bool RestrictBroadcast(ZRoutedRpc instance, long targetPeerId, string methodName, params object[] parameters)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (Groups.ownGroup != null && targetPeerId == ZRoutedRpc.Everybody)
			{
				KeyboardShortcut value = Groups.groupPingHotkey.Value;
				if (((KeyboardShortcut)(ref value)).IsPressed())
				{
					foreach (PlayerReference key in Groups.ownGroup.playerStates.Keys)
					{
						instance.InvokeRoutedRPC(key.peerId, "Groups MapPing", parameters);
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
				new CodeInstruction(OpCodes.Call, (object)AccessTools.DeclaredMethod(typeof(RestrictPingsToGroupOnModifierHeld), "RestrictBroadcast", (Type[])null, (Type[])null)),
				new CodeInstruction(OpCodes.Brtrue, (object)label)
			})
				.ToArray());
			return list;
		}
	}

	[HarmonyPatch(typeof(Chat), "RPC_ChatMessage")]
	private class ClearGroupPing
	{
		public static void Prefix(Chat __instance, long sender)
		{
			WorldTextInstance val = __instance.FindExistingWorldText(sender);
			if (val == null || !groupPingTexts.Remove(val) || !Object.op_Implicit((Object)(object)Minimap.instance))
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
	private class ChangeGroupMemberPin
	{
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
					Group? ownGroup = Groups.ownGroup;
					if (ownGroup != null && ownGroup.playerStates.ContainsKey(PlayerReference.fromPlayerInfo(val2)))
					{
						val.m_icon = groupMapPlayerIcon;
						flag = true;
					}
					else if ((Object)(object)val.m_icon == (Object)(object)groupMapPlayerIcon)
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
	private class ChangeGroupMemberPing
	{
		private static void Postfix(Minimap __instance)
		{
			for (int i = 0; i < __instance.m_tempShouts.Count; i++)
			{
				PinData val = __instance.m_pingPins[i];
				WorldTextInstance key = __instance.m_tempShouts[i];
				if (groupPingTexts.TryGetValue(key, out object _))
				{
					val.m_icon = groupMapPingIcon;
					if (Object.op_Implicit((Object)(object)val.m_iconElement))
					{
						val.m_iconElement.sprite = val.m_icon;
					}
				}
			}
		}
	}

	private static Sprite groupMapPlayerIcon = null;

	private static Sprite groupMapPingIcon = null;

	private static readonly ConditionalWeakTable<WorldTextInstance, object> groupPingTexts = new ConditionalWeakTable<WorldTextInstance, object>();

	private static readonly Color defaultColor = new Color(1f, 61f / 85f, 49f / 136f);

	public static void Init()
	{
		groupMapPlayerIcon = Helper.loadSprite("groupPlayerIcon.png", 64, 64);
		groupMapPingIcon = Helper.loadSprite("groupMapPingIcon.png", 64, 64);
		UpdateMapPinColor();
	}

	public static void onMapPing(long senderId, Vector3 position, int type, UserInfo name, string text)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Chat.instance.RPC_ChatMessage(senderId, position, type, name, text);
		WorldTextInstance val = Chat.instance.FindExistingWorldText(senderId);
		((Graphic)val.m_textMeshField).color = Groups.friendlyNameColor.Value;
		groupPingTexts.Add(val, Array.Empty<object>());
	}

	public static void UpdateMapPinColor()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		Color[] pixels = Helper.loadTexture("groupPlayerIcon.png").GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			if ((double)pixels[i].r > 0.5 && (double)pixels[i].b < 0.5 && (double)pixels[i].g < 0.5)
			{
				pixels[i] = Groups.friendlyNameColor.Value;
			}
		}
		groupMapPlayerIcon.texture.SetPixels(pixels);
		groupMapPlayerIcon.texture.Apply();
		pixels = Helper.loadTexture("groupMapPingIcon.png").GetPixels();
		for (int j = 0; j < pixels.Length; j++)
		{
			if ((double)pixels[j].r > 0.5 && (double)pixels[j].b < 0.5 && (double)pixels[j].g < 0.5)
			{
				pixels[j].b = Groups.friendlyNameColor.Value.b;
				pixels[j].g = Groups.friendlyNameColor.Value.g;
				pixels[j].r = Groups.friendlyNameColor.Value.r;
			}
		}
		groupMapPingIcon.texture.SetPixels(pixels);
		groupMapPingIcon.texture.Apply();
	}
}
