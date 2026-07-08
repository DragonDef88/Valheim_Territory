using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Groups;

public static class Interface
{
	[HarmonyPatch(typeof(Hud), "Awake")]
	public class AddGroupDisplay
	{
		private static void Postfix(Hud __instance)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("Groups", new Type[1] { typeof(RectTransform) });
			val.AddComponent<DragNDrop>();
			val.transform.SetParent(__instance.m_rootObject.transform);
			val.transform.localPosition = Vector2.op_Implicit(Groups.groupInterfaceAnchor.Value);
			groupMemberFirst = Object.Instantiate<GameObject>(EnemyHud.instance.m_baseHudPlayer, val.transform);
			groupMemberFirst.transform.localPosition = Vector3.zero;
			Transform obj = groupMemberFirst.transform.Find("Health");
			obj.localScale = new Vector3(1f, 3.5f, 1f);
			((Component)((Component)obj).transform.Find("darken")).GetComponent<RectTransform>().sizeDelta = new Vector2(8f, 3f);
			GameObject val2 = new GameObject("Leader Icon");
			val2.transform.SetParent(groupMemberFirst.transform);
			val2.AddComponent<Image>().sprite = groupLeaderIcon;
			RectTransform component = val2.GetComponent<RectTransform>();
			component.sizeDelta = new Vector2(32f, 32f);
			((Transform)component).localPosition = Vector2.op_Implicit(new Vector2(0f, 46f));
			val2.SetActive(false);
			GameObject val3 = new GameObject("Life Display Text", new Type[1] { typeof(RectTransform) });
			val3.transform.SetParent(groupMemberFirst.transform, false);
			((RectTransform)val3.transform).sizeDelta = new Vector2(300f, 50f);
			val3.transform.localPosition = new Vector3(0f, 5f);
			Text obj2 = val3.AddComponent<Text>();
			obj2.font = ((IEnumerable<Font>)Resources.FindObjectsOfTypeAll<Font>()).FirstOrDefault((Func<Font, bool>)((Font x) => ((Object)x).name == "AveriaSerifLibre-Bold"));
			obj2.fontSize = 14;
			obj2.alignment = (TextAnchor)4;
			Outline obj3 = val3.AddComponent<Outline>();
			((Shadow)obj3).effectColor = Color.black;
			((Shadow)obj3).effectDistance = new Vector2(1f, -1f);
		}
	}

	[HarmonyPatch(typeof(Hud), "Update")]
	public class UpdateGroupDisplay
	{
		private static void Postfix()
		{
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0303: Unknown result type (might be due to invalid IL or missing references)
			Transform parent = groupMemberFirst.transform.parent;
			bool flag = ((Component)parent).gameObject.activeSelf;
			((Component)parent).gameObject.SetActive(Groups.ownGroup != null);
			if (Groups.ownGroup == null)
			{
				return;
			}
			Vector2 sizeDelta = default(Vector2);
			while (parent.childCount < Groups.ownGroup.playerStates.Count)
			{
				GameObject obj = Object.Instantiate<GameObject>(groupMemberFirst, parent, false);
				obj.transform.localPosition = ((Groups.horizontalGroupInterface.Value == Groups.Toggle.On) ? new Vector3((float)(parent.childCount - 1) * Groups.spaceBetweenGroupMembers.Value, 0f) : new Vector3(0f, (float)(-(parent.childCount - 1)) * Groups.spaceBetweenGroupMembers.Value));
				GuiBar component = ((Component)groupMemberFirst.transform.Find("Health/health_slow")).GetComponent<GuiBar>();
				((Vector2)(ref sizeDelta))._002Ector(Mathf.Max(component.m_bar.sizeDelta.x, component.m_width), component.m_bar.sizeDelta.y);
				((Component)obj.transform.Find("Health/health_slow")).GetComponent<GuiBar>().m_firstSet = true;
				((Component)obj.transform.Find("Health/health_slow")).GetComponent<GuiBar>().m_bar.sizeDelta = sizeDelta;
				((Component)obj.transform.Find("Health/health_fast")).GetComponent<GuiBar>().m_firstSet = true;
				((Component)obj.transform.Find("Health/health_fast")).GetComponent<GuiBar>().m_bar.sizeDelta = sizeDelta;
				flag = false;
			}
			if (!flag)
			{
				((Component)parent).GetComponent<DragNDrop>().SetPosition(((Component)parent).transform.position);
			}
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				List<KeyValuePair<PlayerReference, Group.PlayerState>> list = Groups.ownGroup.playerStates.ToList();
				bool flag2 = i < Groups.ownGroup.playerStates.Count;
				((Component)child).gameObject.SetActive(flag2);
				if (flag2)
				{
					PlayerReference key = list[i].Key;
					Group.PlayerState value = list[i].Value;
					((Component)child.Find("Health/health_slow")).GetComponent<GuiBar>().SetValue(value.health / value.maxHealth);
					((Component)child.Find("Health/health_fast")).GetComponent<GuiBar>().SetValue(value.health / value.maxHealth);
					((Component)child.Find("Life Display Text")).GetComponent<Text>().text = ((value.health <= 0f) ? "DEAD" : (Mathf.Ceil(value.health) + " / " + Mathf.Ceil(value.maxHealth)));
					TextMeshProUGUI component2 = ((Component)child.Find("Name")).GetComponent<TextMeshProUGUI>();
					((TMP_Text)component2).text = key.name;
					((Graphic)component2).color = ((key == Groups.ownGroup.leader && Groups.groupLeaderDisplay.Value == Groups.GroupLeaderDisplayOption.Color) ? Groups.groupLeaderColor.Value : Groups.friendlyNameColor.Value);
					((Component)child.Find("Leader Icon")).gameObject.SetActive(key == Groups.ownGroup.leader && Groups.groupLeaderDisplay.Value == Groups.GroupLeaderDisplayOption.Icon);
					API.InvokeUIUpdate(key, ((Component)child).gameObject);
				}
			}
		}
	}

	[HarmonyPatch]
	public class BroadcastHealth
	{
		[CompilerGenerated]
		private sealed class _003CTargetMethods_003Ed__0 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IDisposable, IEnumerator
		{
			private int _003C_003E1__state;

			private MethodBase _003C_003E2__current;

			private int _003C_003El__initialThreadId;

			MethodBase IEnumerator<MethodBase>.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			[DebuggerHidden]
			public _003CTargetMethods_003Ed__0(int _003C_003E1__state)
			{
				this._003C_003E1__state = _003C_003E1__state;
				_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				_003C_003E1__state = -2;
			}

			private bool MoveNext()
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E2__current = AccessTools.Method(typeof(Character), "SetHealth", (Type[])null, (Type[])null);
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E2__current = AccessTools.Method(typeof(Character), "SetMaxHealth", (Type[])null, (Type[])null);
					_003C_003E1__state = 2;
					return true;
				case 2:
					_003C_003E1__state = -1;
					return false;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
			{
				if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
				{
					_003C_003E1__state = 0;
					return this;
				}
				return new _003CTargetMethods_003Ed__0(0);
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<MethodBase>)this).GetEnumerator();
			}
		}

		[IteratorStateMachine(typeof(_003CTargetMethods_003Ed__0))]
		private static IEnumerable<MethodBase> TargetMethods()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003CTargetMethods_003Ed__0(-2);
		}

		private static void Postfix(Character __instance)
		{
			if (Groups.ownGroup == null || !((Object)(object)__instance == (Object)(object)Player.m_localPlayer))
			{
				return;
			}
			foreach (PlayerReference key in Groups.ownGroup.playerStates.Keys)
			{
				ZRoutedRpc.instance.InvokeRoutedRPC(key.peerId, "Groups UpdateHealth", new object[2]
				{
					__instance.GetHealth(),
					__instance.GetMaxHealth()
				});
			}
		}
	}

	private static Sprite groupLeaderIcon;

	private static GameObject? groupMemberFirst;

	public static void Init()
	{
		groupLeaderIcon = Helper.loadSprite("groupLeaderIcon.png", 32, 32);
	}

	public static void AnchorGroupInterface(object sender, EventArgs e)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (groupMemberFirst != null)
		{
			Transform parent = groupMemberFirst.transform.parent;
			Vector2 val = Vector2.op_Implicit(parent.localPosition) - Groups.groupInterfaceAnchor.Value;
			if ((double)((Vector2)(ref val)).magnitude > 0.001)
			{
				parent.localPosition = Vector2.op_Implicit(Groups.groupInterfaceAnchor.Value);
				((Component)parent).GetComponent<DragNDrop>().SetPosition(parent.position);
			}
		}
	}

	public static void UpdateGroupInterfaceSpacing(object sender, EventArgs e)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (groupMemberFirst != null)
		{
			Transform parent = groupMemberFirst.transform.parent;
			for (int i = 0; i < parent.childCount; i++)
			{
				parent.GetChild(i).localPosition = ((Groups.horizontalGroupInterface.Value == Groups.Toggle.On) ? new Vector3((float)i * Groups.spaceBetweenGroupMembers.Value, 0f) : new Vector3(0f, (float)(-i) * Groups.spaceBetweenGroupMembers.Value));
			}
		}
	}

	public static void onUpdateHealth(long senderId, float health, float maxHealth)
	{
		if (Groups.ownGroup != null && Groups.ownGroup.playerStates.TryGetValue(PlayerReference.fromPlayerId(senderId), out Group.PlayerState value))
		{
			value.health = health;
			value.maxHealth = maxHealth;
		}
	}
}
