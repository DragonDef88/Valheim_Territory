using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Smelter : MonoBehaviour, IHasHoverMenuExtended
{
	[Serializable]
	public class ItemConversion
	{
		public ItemDrop m_from;

		public ItemDrop m_to;
	}

	public string m_name = "Smelter";

	public string m_addOreTooltip = "$piece_smelter_additem";

	public string m_emptyOreTooltip = "$piece_smelter_empty";

	public Switch m_addWoodSwitch;

	public Switch m_addOreSwitch;

	public Switch m_emptyOreSwitch;

	public Transform m_outputPoint;

	public Transform m_roofCheckPoint;

	public GameObject m_enabledObject;

	public GameObject m_disabledObject;

	public GameObject m_haveFuelObject;

	public GameObject m_haveOreObject;

	public GameObject m_noOreObject;

	public Animator[] m_animators;

	public ItemDrop m_fuelItem;

	public int m_maxOre = 10;

	public int m_maxFuel = 10;

	public int m_fuelPerProduct = 4;

	public float m_secPerProduct = 10f;

	public bool m_spawnStack;

	public bool m_requiresRoof;

	public Windmill m_windmill;

	public SmokeSpawner m_smokeSpawner;

	public float m_addOreAnimationDuration;

	public List<ItemConversion> m_conversion = new List<ItemConversion>();

	public EffectList m_oreAddedEffects = new EffectList();

	public EffectList m_fuelAddedEffects = new EffectList();

	public EffectList m_produceEffects = new EffectList();

	private ZNetView m_nview;

	private bool m_haveRoof;

	private bool m_blockedSmoke;

	private float m_addedOreTime = -1000f;

	private StringBuilder m_sb = new StringBuilder();

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if ((Object)(object)m_nview == (Object)null)
		{
			m_nview = ((Component)this).GetComponentInParent<ZNetView>();
		}
		if (!((Object)(object)m_nview == (Object)null) && m_nview.GetZDO() != null)
		{
			if (Object.op_Implicit((Object)(object)m_addOreSwitch))
			{
				Switch addOreSwitch = m_addOreSwitch;
				addOreSwitch.m_onUse = (Switch.Callback)Delegate.Combine(addOreSwitch.m_onUse, new Switch.Callback(OnAddOre));
				m_addOreSwitch.m_onHover = OnHoverAddOre;
			}
			if (Object.op_Implicit((Object)(object)m_addWoodSwitch))
			{
				Switch addWoodSwitch = m_addWoodSwitch;
				addWoodSwitch.m_onUse = (Switch.Callback)Delegate.Combine(addWoodSwitch.m_onUse, new Switch.Callback(OnAddFuel));
				m_addWoodSwitch.m_onHover = OnHoverAddFuel;
			}
			if (Object.op_Implicit((Object)(object)m_emptyOreSwitch))
			{
				Switch emptyOreSwitch = m_emptyOreSwitch;
				emptyOreSwitch.m_onUse = (Switch.Callback)Delegate.Combine(emptyOreSwitch.m_onUse, new Switch.Callback(OnEmpty));
				Switch emptyOreSwitch2 = m_emptyOreSwitch;
				emptyOreSwitch2.m_onHover = (Switch.TooltipCallback)Delegate.Combine(emptyOreSwitch2.m_onHover, new Switch.TooltipCallback(OnHoverEmptyOre));
			}
			m_nview.Register<string>("RPC_AddOre", RPC_AddOre);
			m_nview.Register("RPC_AddFuel", RPC_AddFuel);
			m_nview.Register("RPC_EmptyProcessed", RPC_EmptyProcessed);
			WearNTear component = ((Component)this).GetComponent<WearNTear>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.m_onDestroyed = (Action)Delegate.Combine(component.m_onDestroyed, new Action(OnDestroyed));
			}
			((MonoBehaviour)this).InvokeRepeating("UpdateSmelter", 1f, 1f);
		}
	}

	private void DropAllItems()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		SpawnProcessed();
		if ((Object)(object)m_fuelItem != (Object)null)
		{
			float num = ((m_nview.GetZDO() == null) ? 0f : m_nview.GetZDO().GetFloat(ZDOVars.s_fuel));
			for (int i = 0; i < (int)num; i++)
			{
				Vector3 val = ((Component)this).transform.position + Vector3.up + Random.insideUnitSphere * 0.3f;
				Quaternion val2 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
				ItemDrop.OnCreateNew(Object.Instantiate<GameObject>(((Component)m_fuelItem).gameObject, val, val2));
			}
		}
		while (GetQueueSize() > 0)
		{
			string queuedOre = GetQueuedOre();
			RemoveOneOre();
			ItemConversion itemConversion = GetItemConversion(queuedOre);
			if (itemConversion != null)
			{
				Vector3 val3 = ((Component)this).transform.position + Vector3.up + Random.insideUnitSphere * 0.3f;
				Quaternion val4 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
				ItemDrop.OnCreateNew(Object.Instantiate<GameObject>(((Component)itemConversion.m_from).gameObject, val3, val4));
			}
		}
	}

	private void OnDestroyed()
	{
		if (m_nview.IsOwner())
		{
			DropAllItems();
		}
	}

	private bool IsItemAllowed(ItemDrop.ItemData item)
	{
		return IsItemAllowed(((Object)item.m_dropPrefab).name);
	}

	private bool IsItemAllowed(string itemName)
	{
		foreach (ItemConversion item in m_conversion)
		{
			if (((Object)((Component)item.m_from).gameObject).name == itemName)
			{
				return true;
			}
		}
		return false;
	}

	private ItemDrop.ItemData FindCookableItem(Inventory inventory)
	{
		foreach (ItemConversion item2 in m_conversion)
		{
			ItemDrop.ItemData item = inventory.GetItem(item2.m_from.m_itemData.m_shared.m_name);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	private bool OnAddOre(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (item == null)
		{
			item = FindCookableItem(user.GetInventory());
			if (item == null)
			{
				user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems");
				return false;
			}
		}
		if (!IsItemAllowed(((Object)item.m_dropPrefab).name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_wontwork");
			return false;
		}
		ZLog.Log((object)("trying to add " + item.m_shared.m_name));
		if (GetQueueSize() >= m_maxOre)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
			return false;
		}
		user.Message(MessageHud.MessageType.Center, "$msg_added " + item.m_shared.m_name);
		user.GetInventory().RemoveItem(item, 1);
		m_nview.InvokeRPC("RPC_AddOre", ((Object)item.m_dropPrefab).name);
		m_addedOreTime = Time.time;
		if (m_addOreAnimationDuration > 0f)
		{
			SetAnimation(active: true);
		}
		return true;
	}

	private float GetBakeTimer()
	{
		if (!m_nview.IsValid())
		{
			return 0f;
		}
		return m_nview.GetZDO().GetFloat(ZDOVars.s_bakeTimer);
	}

	private void SetBakeTimer(float t)
	{
		if (m_nview.IsValid())
		{
			m_nview.GetZDO().Set(ZDOVars.s_bakeTimer, t);
		}
	}

	private float GetFuel()
	{
		if (!m_nview.IsValid())
		{
			return 0f;
		}
		return m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
	}

	private void SetFuel(float fuel)
	{
		if (m_nview.IsValid())
		{
			m_nview.GetZDO().Set(ZDOVars.s_fuel, fuel);
		}
	}

	private int GetQueueSize()
	{
		if (!m_nview.IsValid())
		{
			return 0;
		}
		return m_nview.GetZDO().GetInt(ZDOVars.s_queued);
	}

	private void RPC_AddOre(long sender, string name)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			if (!IsItemAllowed(name))
			{
				ZLog.Log((object)("Item not allowed " + name));
				return;
			}
			QueueOre(name);
			m_oreAddedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			ZLog.Log((object)("Added ore " + name));
		}
	}

	private void QueueOre(string name)
	{
		int queueSize = GetQueueSize();
		m_nview.GetZDO().Set("item" + queueSize, name);
		m_nview.GetZDO().Set(ZDOVars.s_queued, queueSize + 1);
	}

	private string GetQueuedOre()
	{
		if (GetQueueSize() == 0)
		{
			return "";
		}
		return m_nview.GetZDO().GetString(ZDOVars.s_item0);
	}

	private void RemoveOneOre()
	{
		int queueSize = GetQueueSize();
		if (queueSize != 0)
		{
			for (int i = 0; i < queueSize; i++)
			{
				string @string = m_nview.GetZDO().GetString("item" + (i + 1));
				m_nview.GetZDO().Set("item" + i, @string);
			}
			m_nview.GetZDO().Set(ZDOVars.s_queued, queueSize - 1);
		}
	}

	private bool OnEmpty(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (GetProcessedQueueSize() <= 0)
		{
			return false;
		}
		m_nview.InvokeRPC("RPC_EmptyProcessed");
		return true;
	}

	private void RPC_EmptyProcessed(long sender)
	{
		if (m_nview.IsOwner())
		{
			SpawnProcessed();
		}
	}

	private bool OnAddFuel(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (item != null && item.m_shared.m_name != m_fuelItem.m_itemData.m_shared.m_name)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_wrongitem");
			return false;
		}
		if (GetFuel() > (float)(m_maxFuel - 1))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
			return false;
		}
		if (!user.GetInventory().HaveItem(m_fuelItem.m_itemData.m_shared.m_name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_donthaveany " + m_fuelItem.m_itemData.m_shared.m_name);
			return false;
		}
		user.Message(MessageHud.MessageType.Center, "$msg_added " + m_fuelItem.m_itemData.m_shared.m_name);
		user.GetInventory().RemoveItem(m_fuelItem.m_itemData.m_shared.m_name, 1);
		m_nview.InvokeRPC("RPC_AddFuel");
		return true;
	}

	private void RPC_AddFuel(long sender)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			float fuel = GetFuel();
			SetFuel(fuel + 1f);
			m_fuelAddedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		}
	}

	private double GetDeltaTime()
	{
		DateTime time = ZNet.instance.GetTime();
		DateTime dateTime = new DateTime(m_nview.GetZDO().GetLong(ZDOVars.s_startTime, time.Ticks));
		double totalSeconds = (time - dateTime).TotalSeconds;
		m_nview.GetZDO().Set(ZDOVars.s_startTime, time.Ticks);
		return totalSeconds;
	}

	private float GetAccumulator()
	{
		if (!m_nview.IsValid())
		{
			return 0f;
		}
		return m_nview.GetZDO().GetFloat(ZDOVars.s_accTime);
	}

	private void SetAccumulator(float t)
	{
		if (m_nview.IsValid())
		{
			m_nview.GetZDO().Set(ZDOVars.s_accTime, t);
		}
	}

	private void UpdateRoof()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (m_requiresRoof)
		{
			m_haveRoof = Cover.IsUnderRoof(m_roofCheckPoint.position);
		}
	}

	private void UpdateSmoke()
	{
		if ((Object)(object)m_smokeSpawner != (Object)null)
		{
			m_blockedSmoke = m_smokeSpawner.IsBlocked();
		}
		else
		{
			m_blockedSmoke = false;
		}
	}

	private void UpdateSmelter()
	{
		if (!m_nview.IsValid())
		{
			return;
		}
		UpdateRoof();
		UpdateSmoke();
		UpdateState();
		if (!m_nview.IsOwner())
		{
			return;
		}
		double deltaTime = GetDeltaTime();
		float accumulator = GetAccumulator();
		accumulator += (float)deltaTime;
		if (accumulator > 3600f)
		{
			accumulator = 3600f;
		}
		float num = (Object.op_Implicit((Object)(object)m_windmill) ? m_windmill.GetPowerOutput() : 1f);
		while (accumulator >= 1f)
		{
			accumulator -= 1f;
			float fuel = GetFuel();
			string queuedOre = GetQueuedOre();
			if ((m_maxFuel != 0 && !(fuel > 0f)) || (m_maxOre != 0 && !(queuedOre != "")) || !(m_secPerProduct > 0f) || (m_requiresRoof && !m_haveRoof) || m_blockedSmoke)
			{
				continue;
			}
			float num2 = 1f * num;
			if (m_maxFuel > 0)
			{
				float num3 = m_secPerProduct / (float)m_fuelPerProduct;
				fuel -= num2 / num3;
				if (fuel < 0.0001f)
				{
					fuel = 0f;
				}
				SetFuel(fuel);
			}
			if (queuedOre != "")
			{
				float bakeTimer = GetBakeTimer();
				bakeTimer += num2;
				SetBakeTimer(bakeTimer);
				if (bakeTimer >= m_secPerProduct)
				{
					SetBakeTimer(0f);
					RemoveOneOre();
					QueueProcessed(queuedOre);
				}
			}
		}
		if (GetQueuedOre() == "" || ((float)m_maxFuel > 0f && GetFuel() == 0f))
		{
			SpawnProcessed();
		}
		SetAccumulator(accumulator);
	}

	private void QueueProcessed(string ore)
	{
		if (!m_spawnStack)
		{
			Spawn(ore, 1);
		}
		else
		{
			if (!m_nview.IsValid())
			{
				return;
			}
			string @string = m_nview.GetZDO().GetString(ZDOVars.s_spawnOre);
			int @int = m_nview.GetZDO().GetInt(ZDOVars.s_spawnAmount);
			if (@string.Length > 0)
			{
				if (@string != ore)
				{
					SpawnProcessed();
					m_nview.GetZDO().Set(ZDOVars.s_spawnOre, ore);
					m_nview.GetZDO().Set(ZDOVars.s_spawnAmount, 1);
					return;
				}
				@int++;
				ItemConversion itemConversion = GetItemConversion(ore);
				if (itemConversion == null || @int >= itemConversion.m_to.m_itemData.m_shared.m_maxStackSize)
				{
					Spawn(ore, @int);
					m_nview.GetZDO().Set(ZDOVars.s_spawnOre, "");
					m_nview.GetZDO().Set(ZDOVars.s_spawnAmount, 0);
				}
				else
				{
					m_nview.GetZDO().Set(ZDOVars.s_spawnAmount, @int);
				}
			}
			else
			{
				m_nview.GetZDO().Set(ZDOVars.s_spawnOre, ore);
				m_nview.GetZDO().Set(ZDOVars.s_spawnAmount, 1);
			}
		}
	}

	private void SpawnProcessed()
	{
		if (m_nview.IsValid())
		{
			int @int = m_nview.GetZDO().GetInt(ZDOVars.s_spawnAmount);
			if (@int > 0)
			{
				string @string = m_nview.GetZDO().GetString(ZDOVars.s_spawnOre);
				Spawn(@string, @int);
				m_nview.GetZDO().Set(ZDOVars.s_spawnOre, "");
				m_nview.GetZDO().Set(ZDOVars.s_spawnAmount, 0);
			}
		}
	}

	private int GetProcessedQueueSize()
	{
		if (!m_nview.IsValid())
		{
			return 0;
		}
		return m_nview.GetZDO().GetInt(ZDOVars.s_spawnAmount);
	}

	private void Spawn(string ore, int stack)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		ItemConversion itemConversion = GetItemConversion(ore);
		if (itemConversion != null && (Object)(object)itemConversion.m_to != (Object)null)
		{
			m_produceEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			ItemDrop component = Object.Instantiate<GameObject>(((Component)itemConversion.m_to).gameObject, m_outputPoint.position, m_outputPoint.rotation).GetComponent<ItemDrop>();
			component.m_itemData.m_stack = stack;
			ItemDrop.OnCreateNew(component);
		}
	}

	private ItemConversion GetItemConversion(string itemName)
	{
		foreach (ItemConversion item in m_conversion)
		{
			if (((Object)((Component)item.m_from).gameObject).name == itemName)
			{
				return item;
			}
		}
		return null;
	}

	private void UpdateState()
	{
		bool flag = IsActive();
		m_enabledObject.SetActive(flag);
		if (Object.op_Implicit((Object)(object)m_disabledObject))
		{
			m_disabledObject.SetActive(!flag);
		}
		if (Object.op_Implicit((Object)(object)m_haveFuelObject))
		{
			m_haveFuelObject.SetActive(GetFuel() > 0f);
		}
		if (Object.op_Implicit((Object)(object)m_haveOreObject))
		{
			m_haveOreObject.SetActive(GetQueueSize() > 0);
		}
		if (Object.op_Implicit((Object)(object)m_noOreObject))
		{
			m_noOreObject.SetActive(GetQueueSize() == 0);
		}
		if (m_addOreAnimationDuration > 0f && Time.time - m_addedOreTime < m_addOreAnimationDuration)
		{
			flag = true;
		}
		SetAnimation(flag);
	}

	private void SetAnimation(bool active)
	{
		Animator[] animators = m_animators;
		foreach (Animator val in animators)
		{
			if (((Component)val).gameObject.activeInHierarchy)
			{
				val.SetBool("active", active);
				val.SetFloat("activef", active ? 1f : 0f);
			}
		}
	}

	public bool IsActive()
	{
		if ((m_maxFuel == 0 || GetFuel() > 0f) && (m_maxOre == 0 || GetQueueSize() > 0) && (!m_requiresRoof || m_haveRoof))
		{
			return !m_blockedSmoke;
		}
		return false;
	}

	private string OnHoverAddFuel()
	{
		float fuel = GetFuel();
		return Localization.instance.Localize($"{m_name} ({m_fuelItem.m_itemData.m_shared.m_name} {Mathf.Ceil(fuel)}/{m_maxFuel})\n[<color=yellow><b>$KEY_Use</b></color>] $piece_smelter_add {m_fuelItem.m_itemData.m_shared.m_name}");
	}

	private string OnHoverEmptyOre()
	{
		int processedQueueSize = GetProcessedQueueSize();
		return Localization.instance.Localize($"{m_name} ({processedQueueSize} $piece_smelter_ready) \n[<color=yellow><b>$KEY_Use</b></color>] {m_emptyOreTooltip}");
	}

	private string OnHoverAddOre()
	{
		m_sb.Clear();
		int queueSize = GetQueueSize();
		m_sb.Append($"{m_name} ({queueSize}/{m_maxOre}) ");
		if (m_requiresRoof && !m_haveRoof && Mathf.Sin(Time.time * 10f) > 0f)
		{
			m_sb.Append(" <color=yellow>$piece_smelter_reqroof</color>");
		}
		m_sb.Append("\n[<color=yellow><b>$KEY_Use</b></color>] " + m_addOreTooltip);
		return Localization.instance.Localize(m_sb.ToString());
	}

	public bool TryGetItems(Player player, Switch switchRef, out List<string> items)
	{
		items = new List<string>();
		if (!CanUseItems(player, switchRef))
		{
			return true;
		}
		if ((Object)(object)switchRef == (Object)(object)m_addOreSwitch)
		{
			items = m_conversion.Select((ItemConversion conversion) => conversion.m_from.m_itemData.m_shared.m_name).ToList();
		}
		else if ((Object)(object)switchRef == (Object)(object)m_addWoodSwitch)
		{
			items.Add(m_fuelItem.m_itemData.m_shared.m_name);
		}
		return true;
	}

	public bool CanUseItems(Player player, Switch switchRef, bool sendErrorMessage = true)
	{
		if ((Object)(object)switchRef == (Object)(object)m_emptyOreSwitch)
		{
			return false;
		}
		if ((Object)(object)switchRef == (Object)(object)m_addOreSwitch)
		{
			if (GetQueueSize() >= m_maxOre)
			{
				if (sendErrorMessage)
				{
					player.Message(MessageHud.MessageType.Center, "$msg_itsfull");
				}
				return false;
			}
			if (player.GetInventory().CountItemsByName(m_conversion.Select((ItemConversion conversion) => conversion.m_from.m_itemData.m_shared.m_name).ToArray()) > 0)
			{
				return true;
			}
			if (sendErrorMessage)
			{
				player.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems");
			}
			return false;
		}
		if ((Object)(object)switchRef != (Object)(object)m_addWoodSwitch)
		{
			return false;
		}
		if (GetFuel() > (float)(m_maxFuel - 1))
		{
			if (sendErrorMessage)
			{
				player.Message(MessageHud.MessageType.Center, "$msg_itsfull");
			}
			return false;
		}
		if (player.GetInventory().HaveItem(m_fuelItem.m_itemData.m_shared.m_name))
		{
			return true;
		}
		if (sendErrorMessage)
		{
			player.Message(MessageHud.MessageType.Center, "$msg_donthaveany " + m_fuelItem.m_itemData.m_shared.m_name);
		}
		return false;
	}
}
