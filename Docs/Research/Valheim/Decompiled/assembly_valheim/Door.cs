using UnityEngine;

public class Door : MonoBehaviour, Hoverable, Interactable
{
	public string m_name = "door";

	public ItemDrop m_keyItem;

	public bool m_canNotBeClosed;

	public bool m_invertedOpenClosedText;

	public bool m_checkGuardStone = true;

	public GameObject m_openEnable;

	public EffectList m_openEffects = new EffectList();

	public EffectList m_closeEffects = new EffectList();

	public EffectList m_lockedEffects = new EffectList();

	private ZNetView m_nview;

	private Animator m_animator;

	private uint m_lastDataRevision = uint.MaxValue;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_nview.GetZDO() != null)
		{
			m_animator = ((Component)this).GetComponentInChildren<Animator>();
			if (Object.op_Implicit((Object)(object)m_nview))
			{
				m_nview.Register<bool>("UseDoor", RPC_UseDoor);
			}
			((MonoBehaviour)this).InvokeRepeating("UpdateState", 0f, 0.2f);
		}
	}

	private void UpdateState()
	{
		if (m_nview.IsValid())
		{
			uint dataRevision = m_nview.GetZDO().DataRevision;
			if (m_lastDataRevision != dataRevision)
			{
				m_lastDataRevision = dataRevision;
				int @int = m_nview.GetZDO().GetInt(ZDOVars.s_state);
				SetState(@int);
			}
		}
	}

	private void SetState(int state)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (m_animator.GetInteger("state") != state)
		{
			if (state != 0)
			{
				m_openEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			}
			else
			{
				m_closeEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			}
			m_animator.SetInteger("state", state);
		}
		if (Object.op_Implicit((Object)(object)m_openEnable))
		{
			m_openEnable.SetActive(state != 0);
		}
	}

	private bool CanInteract()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (((Object)(object)m_keyItem != (Object)null || m_canNotBeClosed) && m_nview.GetZDO().GetInt(ZDOVars.s_state) != 0)
		{
			return false;
		}
		AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
		if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsTag("open"))
		{
			currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
			return ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsTag("closed");
		}
		return true;
	}

	public string GetHoverText()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return "";
		}
		if (m_canNotBeClosed && !CanInteract())
		{
			return "";
		}
		if (m_checkGuardStone && !PrivateArea.CheckAccess(((Component)this).transform.position, 0f, flash: false))
		{
			return Localization.instance.Localize(m_name + "\n$piece_noaccess");
		}
		if (CanInteract())
		{
			if (m_nview.GetZDO().GetInt(ZDOVars.s_state) != 0)
			{
				return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] " + (m_invertedOpenClosedText ? "$piece_door_open" : "$piece_door_close"));
			}
			return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] " + (m_invertedOpenClosedText ? "$piece_door_close" : "$piece_door_open"));
		}
		return Localization.instance.Localize(m_name);
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		if (hold)
		{
			return false;
		}
		if (!CanInteract())
		{
			return false;
		}
		if (m_checkGuardStone && !PrivateArea.CheckAccess(((Component)this).transform.position))
		{
			return true;
		}
		if ((Object)(object)m_keyItem != (Object)null)
		{
			if (!HaveKey(character))
			{
				m_lockedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
				if (Game.m_worldLevel > 0 && HaveKey(character, matchWorldLevel: false))
				{
					character.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_ng_the_x") + m_keyItem.m_itemData.m_shared.m_name + Localization.instance.Localize("$msg_ng_x_is_too_low"));
				}
				else
				{
					character.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_door_needkey", new string[1] { m_keyItem.m_itemData.m_shared.m_name }));
				}
				return true;
			}
			character.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_door_usingkey", new string[1] { m_keyItem.m_itemData.m_shared.m_name }));
		}
		Vector3 val = ((Component)character).transform.position - ((Component)this).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Game.instance.IncrementPlayerStat((m_nview.GetZDO().GetInt(ZDOVars.s_state) == 0) ? PlayerStatType.DoorsOpened : PlayerStatType.DoorsClosed);
		Open(normalized);
		return true;
	}

	private void Open(Vector3 userDir)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Vector3.Dot(((Component)this).transform.forward, userDir) < 0f;
		m_nview.InvokeRPC("UseDoor", flag);
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_keyItem != (Object)null && m_keyItem.m_itemData.m_shared.m_name == item.m_shared.m_name)
		{
			if (!CanInteract())
			{
				return false;
			}
			if (m_checkGuardStone && !PrivateArea.CheckAccess(((Component)this).transform.position))
			{
				return true;
			}
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_door_usingkey", new string[1] { m_keyItem.m_itemData.m_shared.m_name }));
			Vector3 val = ((Component)user).transform.position - ((Component)this).transform.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			Open(normalized);
			return true;
		}
		return false;
	}

	private bool HaveKey(Humanoid player, bool matchWorldLevel = true)
	{
		if ((Object)(object)m_keyItem == (Object)null)
		{
			return true;
		}
		return player.GetInventory().HaveItem(m_keyItem.m_itemData.m_shared.m_name, matchWorldLevel);
	}

	private void RPC_UseDoor(long uid, bool forward)
	{
		if (!CanInteract())
		{
			return;
		}
		if (m_nview.GetZDO().GetInt(ZDOVars.s_state) == 0)
		{
			if (forward)
			{
				m_nview.GetZDO().Set(ZDOVars.s_state, 1);
			}
			else
			{
				m_nview.GetZDO().Set(ZDOVars.s_state, -1);
			}
		}
		else
		{
			m_nview.GetZDO().Set(ZDOVars.s_state, 0);
		}
		UpdateState();
	}
}
