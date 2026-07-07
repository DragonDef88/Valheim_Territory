using System;
using UnityEngine;

public class Pickable : MonoBehaviour, Hoverable, Interactable
{
	public delegate bool SpawnCheck(Pickable p);

	public GameObject m_hideWhenPicked;

	public GameObject m_itemPrefab;

	public int m_amount = 1;

	public int m_minAmountScaled = 1;

	public bool m_dontScale;

	public DropTable m_extraDrops = new DropTable();

	public string m_overrideName = "";

	public float m_respawnTimeMinutes;

	public float m_respawnTimeInitMin;

	public float m_respawnTimeInitMax;

	public float m_spawnOffset = 0.5f;

	public EffectList m_pickEffector = new EffectList();

	public bool m_pickEffectAtSpawnPoint;

	public bool m_useInteractAnimation;

	public bool m_tarPreventsPicking;

	public float m_aggravateRange;

	public bool m_defaultPicked;

	public bool m_defaultEnabled = true;

	public SpawnCheck m_spawnCheck;

	public bool m_harvestable;

	public Skills.SkillType m_pickRaiseSkill;

	public float m_maxLevelBonusChance = 0.25f;

	public int m_bonusYieldAmount = 1;

	public EffectList m_bonusEffect;

	private ZNetView m_nview;

	private Floating m_floating;

	private bool m_picked;

	private int m_enabled = 2;

	private long m_pickedTime;

	public int GetEnabled => m_enabled;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		ZDO zDO = m_nview.GetZDO();
		if (zDO != null)
		{
			m_nview.Register<bool>("RPC_SetPicked", RPC_SetPicked);
			m_nview.Register<int>("RPC_Pick", RPC_Pick);
			m_picked = zDO.GetBool(ZDOVars.s_picked, m_defaultPicked);
			m_pickedTime = m_nview.GetZDO().GetLong(ZDOVars.s_pickedTime, 0L);
			if (m_enabled == 2)
			{
				m_enabled = (zDO.GetBool(ZDOVars.s_enabled, defaultValue: true) ? 1 : 0);
			}
			else if (m_nview.IsOwner())
			{
				zDO.Set(ZDOVars.s_enabled, m_enabled == 1);
			}
			if (Object.op_Implicit((Object)(object)m_hideWhenPicked))
			{
				m_hideWhenPicked.SetActive(!m_picked && m_enabled == 1);
			}
			float num = 60f;
			if (m_respawnTimeMinutes > 0f)
			{
				((MonoBehaviour)this).InvokeRepeating("UpdateRespawn", Random.Range(1f, 5f), num);
			}
			if (m_respawnTimeMinutes <= 0f && (Object)(object)m_hideWhenPicked == (Object)null && m_nview.GetZDO().GetBool(ZDOVars.s_picked))
			{
				m_nview.ClaimOwnership();
				m_nview.Destroy();
				ZLog.Log((object)("Destroying old picked " + ((Object)this).name));
			}
		}
	}

	public string GetHoverText()
	{
		if (m_picked || m_enabled == 0)
		{
			return "";
		}
		return Localization.instance.Localize(GetHoverName() + "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
	}

	public string GetHoverName()
	{
		if (!string.IsNullOrEmpty(m_overrideName))
		{
			return m_overrideName;
		}
		return m_itemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
	}

	private void UpdateRespawn()
	{
		if (!m_nview.IsValid() || !m_nview.IsOwner() || !m_picked)
		{
			return;
		}
		if (m_pickedTime == 0L)
		{
			m_pickedTime = ZNet.instance.GetTime().Ticks - TimeSpan.FromMinutes(Random.Range(m_respawnTimeInitMin * 100f, m_respawnTimeInitMax * 100f)).Ticks;
			if (m_pickedTime < 1)
			{
				m_pickedTime = 1L;
			}
			m_nview.GetZDO().Set(ZDOVars.s_pickedTime, m_pickedTime);
		}
		if (m_enabled == 0)
		{
			if (Object.op_Implicit((Object)(object)m_hideWhenPicked))
			{
				m_hideWhenPicked.SetActive(false);
			}
		}
		else if (ShouldRespawn())
		{
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetPicked", false);
		}
	}

	private bool ShouldRespawn()
	{
		if (!Object.op_Implicit((Object)(object)m_nview))
		{
			return false;
		}
		long @long = m_nview.GetZDO().GetLong(ZDOVars.s_pickedTime, 0L);
		DateTime dateTime = new DateTime(@long);
		TimeSpan timeSpan = ZNet.instance.GetTime() - dateTime;
		if (@long > 1 && timeSpan.TotalMinutes <= (double)m_respawnTimeMinutes)
		{
			return false;
		}
		if (m_spawnCheck != null)
		{
			return m_spawnCheck(this);
		}
		return true;
	}

	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || m_enabled == 0)
		{
			return false;
		}
		if (m_tarPreventsPicking)
		{
			if ((Object)(object)m_floating == (Object)null)
			{
				m_floating = ((Component)this).GetComponent<Floating>();
			}
			if (Object.op_Implicit((Object)(object)m_floating) && m_floating.IsInTar())
			{
				character.Message(MessageHud.MessageType.Center, "$hud_itemstucktar");
				return m_useInteractAnimation;
			}
		}
		int num = 0;
		if (!m_picked && m_pickRaiseSkill != 0 && character is Player player)
		{
			player.RaiseSkill(m_pickRaiseSkill);
			float skillFactor = player.GetSkillFactor(m_pickRaiseSkill);
			if (Random.value < skillFactor * m_maxLevelBonusChance)
			{
				num = m_bonusYieldAmount;
				DamageText.instance.ShowText(DamageText.TextType.Bonus, ((Component)this).transform.position + Vector3.up * m_spawnOffset, $"+{num}", player: true);
				m_bonusEffect.Create(((Component)this).transform.position, Quaternion.identity);
				ZLog.Log((object)"Bonus food picked!");
			}
		}
		m_nview.InvokeRPC("RPC_Pick", num);
		return m_useInteractAnimation;
	}

	private void RPC_Pick(long sender, int bonus)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner() || m_picked)
		{
			return;
		}
		Vector3 basePos = (m_pickEffectAtSpawnPoint ? (((Component)this).transform.position + Vector3.up * m_spawnOffset) : ((Component)this).transform.position);
		m_pickEffector.Create(basePos, Quaternion.identity);
		int num = (m_dontScale ? m_amount : Mathf.Max(m_minAmountScaled, Game.instance.ScaleDrops(m_itemPrefab, m_amount)));
		num += bonus;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			Drop(m_itemPrefab, num2++, 1);
		}
		if (!m_extraDrops.IsEmpty())
		{
			foreach (ItemDrop.ItemData dropListItem in m_extraDrops.GetDropListItems())
			{
				Drop(dropListItem.m_dropPrefab, num2++, dropListItem.m_stack);
			}
		}
		if (m_aggravateRange > 0f)
		{
			BaseAI.AggravateAllInArea(((Component)this).transform.position, m_aggravateRange, BaseAI.AggravatedReason.Theif);
		}
		m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetPicked", true);
	}

	private void RPC_SetPicked(long sender, bool picked)
	{
		SetPicked(picked);
	}

	public void SetPicked(bool picked)
	{
		m_picked = picked;
		if (Object.op_Implicit((Object)(object)m_hideWhenPicked))
		{
			m_hideWhenPicked.SetActive(!picked);
		}
		if (!Object.op_Implicit((Object)(object)m_nview) || !m_nview.IsOwner())
		{
			return;
		}
		if (m_respawnTimeMinutes > 0f || (Object)(object)m_hideWhenPicked != (Object)null)
		{
			m_nview.GetZDO().Set(ZDOVars.s_picked, m_picked);
			if (picked && m_respawnTimeMinutes > 0f)
			{
				DateTime time = ZNet.instance.GetTime();
				m_nview.GetZDO().Set(ZDOVars.s_pickedTime, time.Ticks);
			}
		}
		else if (picked)
		{
			m_nview.Destroy();
		}
	}

	public bool GetPicked()
	{
		return m_picked;
	}

	public void SetEnabled(bool value)
	{
		SetEnabled(value ? 1 : 0);
	}

	public void SetEnabled(int value)
	{
		m_enabled = value;
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsOwner() && m_nview.GetZDO() != null)
		{
			m_nview.GetZDO().Set(ZDOVars.s_enabled, ((Behaviour)this).enabled);
		}
		if (Object.op_Implicit((Object)(object)m_hideWhenPicked))
		{
			m_hideWhenPicked.SetActive(((Behaviour)this).enabled && ShouldRespawn());
		}
	}

	public bool CanBePicked()
	{
		if (!Object.op_Implicit((Object)(object)m_hideWhenPicked) || !m_hideWhenPicked.activeInHierarchy)
		{
			if (!m_picked)
			{
				return m_enabled == 1;
			}
			return false;
		}
		return true;
	}

	private void Drop(GameObject prefab, int offset, int stack)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Random.insideUnitCircle * 0.2f;
		Vector3 val2 = ((Component)this).transform.position + Vector3.up * m_spawnOffset + new Vector3(val.x, 0.5f * (float)offset, val.y);
		Quaternion val3 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
		GameObject obj = Object.Instantiate<GameObject>(prefab, val2, val3);
		ItemDrop component = obj.GetComponent<ItemDrop>();
		if (component != null)
		{
			component.SetStack(stack);
			ItemDrop.OnCreateNew(component);
		}
		obj.GetComponent<Rigidbody>().linearVelocity = Vector3.up * 4f;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}
}
