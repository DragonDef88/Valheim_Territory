using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class CookingStation : MonoBehaviour, Interactable, Hoverable, IHasHoverMenuExtended
{
	[Serializable]
	public class ItemConversion
	{
		public ItemDrop m_from;

		public ItemDrop m_to;

		public float m_cookTime = 10f;
	}

	[Serializable]
	public class ItemMessage
	{
		public ItemDrop m_item;

		public string m_message;
	}

	private enum Status
	{
		NotDone,
		Done,
		Burnt
	}

	public Switch m_addFoodSwitch;

	public Switch m_addFuelSwitch;

	public EffectList m_addEffect = new EffectList();

	public EffectList m_doneEffect = new EffectList();

	public EffectList m_overcookedEffect = new EffectList();

	public EffectList m_pickEffector = new EffectList();

	public string m_addItemTooltip = "$piece_cstand_cook";

	public Transform m_spawnPoint;

	public float m_spawnForce = 5f;

	public ItemDrop m_overCookedItem;

	public List<ItemConversion> m_conversion = new List<ItemConversion>();

	public List<ItemMessage> m_incompatibleItems = new List<ItemMessage>();

	public Transform[] m_slots;

	public ParticleSystem[] m_donePS;

	public ParticleSystem[] m_burntPS;

	public string m_name = "";

	public bool m_requireFire = true;

	public Transform[] m_fireCheckPoints;

	public float m_fireCheckRadius = 0.25f;

	public bool m_useFuel;

	public ItemDrop m_fuelItem;

	public int m_maxFuel = 10;

	public int m_secPerFuel = 5000;

	public EffectList m_fuelAddedEffects = new EffectList();

	public GameObject m_haveFuelObject;

	public GameObject m_haveFireObject;

	private bool m_cheapFireCheck;

	private ZNetView m_nview;

	private ParticleSystem[] m_ps;

	private AudioSource[] m_as;

	private void Awake()
	{
		m_nview = ((Component)this).gameObject.GetComponent<ZNetView>();
		if (m_nview.GetZDO() != null)
		{
			m_ps = (ParticleSystem[])(object)new ParticleSystem[m_slots.Length];
			m_as = (AudioSource[])(object)new AudioSource[m_slots.Length];
			for (int i = 0; i < m_slots.Length; i++)
			{
				m_ps[i] = ((Component)m_slots[i]).GetComponent<ParticleSystem>();
				m_as[i] = ((Component)m_slots[i]).GetComponent<AudioSource>();
			}
			m_nview.Register<Vector3, int>("RPC_RemoveDoneItem", RPC_RemoveDoneItem);
			m_nview.Register<string>("RPC_AddItem", RPC_AddItem);
			m_nview.Register("RPC_AddFuel", RPC_AddFuel);
			m_nview.Register<int, string>("RPC_SetSlotVisual", RPC_SetSlotVisual);
			if (Object.op_Implicit((Object)(object)m_addFoodSwitch))
			{
				m_addFoodSwitch.m_onUse = OnAddFoodSwitch;
				m_addFoodSwitch.m_hoverText = HoverText();
			}
			if (Object.op_Implicit((Object)(object)m_addFuelSwitch))
			{
				m_addFuelSwitch.m_onUse = OnAddFuelSwitch;
				m_addFuelSwitch.m_onHover = OnHoverFuelSwitch;
			}
			WearNTear component = ((Component)this).GetComponent<WearNTear>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.m_onDestroyed = (Action)Delegate.Combine(component.m_onDestroyed, new Action(OnDestroyed));
			}
			m_cheapFireCheck = m_fireCheckRadius == 0.25f;
			((MonoBehaviour)this).InvokeRepeating("UpdateCooking", 0f, 1f);
		}
	}

	private void DropAllItems()
	{
		if ((Object)(object)m_fuelItem != (Object)null)
		{
			float fuel = GetFuel();
			for (int i = 0; i < (int)fuel; i++)
			{
				drop(m_fuelItem);
			}
			SetFuel(0f);
		}
		for (int j = 0; j < m_slots.Length; j++)
		{
			GetSlot(j, out var itemName, out var _, out var status);
			if (!(itemName != ""))
			{
				continue;
			}
			switch (status)
			{
			case Status.Done:
				drop(GetItemConversion(itemName).m_to);
				break;
			case Status.Burnt:
				drop(m_overCookedItem);
				break;
			case Status.NotDone:
			{
				GameObject prefab = ZNetScene.instance.GetPrefab(itemName);
				if ((Object)(object)prefab != (Object)null)
				{
					ItemDrop component = prefab.GetComponent<ItemDrop>();
					if (Object.op_Implicit((Object)(object)component))
					{
						drop(component);
					}
				}
				break;
			}
			}
			SetSlot(j, "", 0f, Status.NotDone);
		}
		void drop(ItemDrop item)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = ((Component)this).transform.position + Vector3.up + Random.insideUnitSphere * 0.3f;
			Quaternion val2 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
			ItemDrop.OnCreateNew(Object.Instantiate<GameObject>(((Component)item).gameObject, val, val2));
		}
	}

	private void OnDestroyed()
	{
		if (m_nview.IsOwner())
		{
			DropAllItems();
		}
	}

	private void UpdateCooking()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return;
		}
		bool flag = (m_requireFire && IsFireLit()) || (m_useFuel && GetFuel() > 0f);
		if (m_nview.IsOwner())
		{
			float deltaTime = GetDeltaTime();
			if (flag)
			{
				UpdateFuel(deltaTime);
				for (int i = 0; i < m_slots.Length; i++)
				{
					GetSlot(i, out var itemName, out var cookedTime, out var status);
					if (!(itemName != "") || status == Status.Burnt)
					{
						continue;
					}
					ItemConversion itemConversion = GetItemConversion(itemName);
					if (itemConversion == null)
					{
						SetSlot(i, "", 0f, Status.NotDone);
						continue;
					}
					cookedTime += deltaTime;
					if (cookedTime > itemConversion.m_cookTime * 2f)
					{
						m_overcookedEffect.Create(m_slots[i].position, Quaternion.identity);
						SetSlot(i, ((Object)m_overCookedItem).name, cookedTime, Status.Burnt);
					}
					else if (cookedTime > itemConversion.m_cookTime && itemName == ((Object)itemConversion.m_from).name)
					{
						m_doneEffect.Create(m_slots[i].position, Quaternion.identity);
						SetSlot(i, ((Object)itemConversion.m_to).name, cookedTime, Status.Done);
					}
					else
					{
						SetSlot(i, itemName, cookedTime, status);
					}
				}
			}
		}
		UpdateVisual(flag);
	}

	private float GetDeltaTime()
	{
		DateTime time = ZNet.instance.GetTime();
		DateTime dateTime = new DateTime(m_nview.GetZDO().GetLong(ZDOVars.s_startTime, time.Ticks));
		double totalSeconds = (time - dateTime).TotalSeconds;
		m_nview.GetZDO().Set(ZDOVars.s_startTime, time.Ticks);
		return (float)totalSeconds;
	}

	private void UpdateFuel(float dt)
	{
		if (m_useFuel)
		{
			float num = dt / (float)m_secPerFuel;
			float fuel = GetFuel();
			fuel -= num;
			if (fuel < 0f)
			{
				fuel = 0f;
			}
			SetFuel(fuel);
		}
	}

	private void UpdateVisual(bool fireLit)
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			GetSlot(i, out var itemName, out var _, out var status);
			SetSlotVisual(i, itemName, fireLit, status);
		}
		if (m_useFuel)
		{
			bool active = GetFuel() > 0f;
			if (Object.op_Implicit((Object)(object)m_haveFireObject))
			{
				m_haveFireObject.SetActive(fireLit);
			}
			if (Object.op_Implicit((Object)(object)m_haveFuelObject))
			{
				m_haveFuelObject.SetActive(active);
			}
		}
	}

	private void RPC_SetSlotVisual(long sender, int slot, string item)
	{
		SetSlotVisual(slot, item, fireLit: false, Status.NotDone);
	}

	private void SetSlotVisual(int i, string item, bool fireLit, Status status)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		if (item == "")
		{
			EmissionModule emission = m_ps[i].emission;
			((EmissionModule)(ref emission)).enabled = false;
			if (m_burntPS.Length != 0)
			{
				EmissionModule emission2 = m_burntPS[i].emission;
				((EmissionModule)(ref emission2)).enabled = false;
			}
			if (m_donePS.Length != 0)
			{
				EmissionModule emission3 = m_donePS[i].emission;
				((EmissionModule)(ref emission3)).enabled = false;
			}
			m_as[i].mute = true;
			if (m_slots[i].childCount > 0)
			{
				Object.Destroy((Object)(object)((Component)m_slots[i].GetChild(0)).gameObject);
			}
			return;
		}
		EmissionModule emission4 = m_ps[i].emission;
		((EmissionModule)(ref emission4)).enabled = fireLit && status != Status.Burnt;
		if (m_burntPS.Length != 0)
		{
			EmissionModule emission5 = m_burntPS[i].emission;
			((EmissionModule)(ref emission5)).enabled = fireLit && status == Status.Burnt;
		}
		if (m_donePS.Length != 0)
		{
			EmissionModule emission6 = m_donePS[i].emission;
			((EmissionModule)(ref emission6)).enabled = fireLit && status == Status.Done;
		}
		m_as[i].mute = !fireLit;
		if (m_slots[i].childCount == 0 || ((Object)m_slots[i].GetChild(0)).name != item)
		{
			if (m_slots[i].childCount > 0)
			{
				Object.Destroy((Object)(object)((Component)m_slots[i].GetChild(0)).gameObject);
			}
			Transform obj = ObjectDB.instance.GetItemPrefab(item).transform.Find("attach");
			Transform val = m_slots[i];
			GameObject obj2 = Object.Instantiate<GameObject>(((Component)obj).gameObject, val.position, val.rotation, val);
			((Object)obj2).name = item;
			Renderer[] componentsInChildren = obj2.GetComponentsInChildren<Renderer>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].shadowCastingMode = (ShadowCastingMode)0;
			}
		}
	}

	private void RPC_RemoveDoneItem(long sender, Vector3 userPoint, int amount)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_slots.Length; i++)
		{
			GetSlot(i, out var itemName, out var _, out var _);
			if (itemName != "" && IsItemDone(itemName))
			{
				for (int j = 0; j < amount; j++)
				{
					SpawnItem(itemName, i, userPoint);
				}
				SetSlot(i, "", 0f, Status.NotDone);
				m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetSlotVisual", i, "");
				break;
			}
		}
	}

	private bool HaveDoneItem()
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			GetSlot(i, out var itemName, out var _, out var _);
			if (itemName != "" && IsItemDone(itemName))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsItemDone(string itemName)
	{
		if (itemName == ((Object)m_overCookedItem).name)
		{
			return true;
		}
		ItemConversion itemConversion = GetItemConversion(itemName);
		if (itemConversion == null)
		{
			return false;
		}
		if (itemName == ((Object)itemConversion.m_to).name)
		{
			return true;
		}
		return false;
	}

	private void SpawnItem(string name, int slot, Vector3 userPoint)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
		Vector3 val;
		Vector3 val2;
		if ((Object)(object)m_spawnPoint != (Object)null)
		{
			val = m_spawnPoint.position;
			val2 = m_spawnPoint.forward;
		}
		else
		{
			Vector3 position = m_slots[slot].position;
			Vector3 val3 = userPoint - position;
			val3.y = 0f;
			((Vector3)(ref val3)).Normalize();
			val = position + val3 * 0.5f;
			val2 = val3;
		}
		Quaternion val4 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
		GameObject obj = Object.Instantiate<GameObject>(itemPrefab, val, val4);
		ItemDrop.OnCreateNew(obj);
		obj.GetComponent<Rigidbody>().linearVelocity = val2 * m_spawnForce;
		m_pickEffector.Create(val, Quaternion.identity);
	}

	public string GetHoverText()
	{
		if ((Object)(object)m_addFoodSwitch != (Object)null)
		{
			return "";
		}
		return Localization.instance.Localize(HoverText());
	}

	private string HoverText()
	{
		return m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] " + m_addItemTooltip + (ZInput.GamepadActive ? "" : ("\n[<color=yellow><b>1-8</b></color>] " + m_addItemTooltip));
	}

	public string GetHoverName()
	{
		return m_name;
	}

	private bool OnAddFuelSwitch(Switch sw, Humanoid user, ItemDrop.ItemData item)
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
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			ZLog.Log((object)"Add fuel");
			float fuel = GetFuel();
			SetFuel(fuel + 1f);
			m_fuelAddedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		}
	}

	private string OnHoverFuelSwitch()
	{
		float fuel = GetFuel();
		Localization instance = Localization.instance;
		string[] obj = new string[9]
		{
			m_name,
			" (",
			m_fuelItem.m_itemData.m_shared.m_name,
			" ",
			Mathf.Ceil(fuel).ToString(),
			"/",
			null,
			null,
			null
		};
		int maxFuel = m_maxFuel;
		obj[6] = maxFuel.ToString();
		obj[7] = ")\n[<color=yellow><b>$KEY_Use</b></color>] $piece_smelter_add ";
		obj[8] = m_fuelItem.m_itemData.m_shared.m_name;
		return instance.Localize(string.Concat(obj));
	}

	private bool OnAddFoodSwitch(Switch caller, Humanoid user, ItemDrop.ItemData item)
	{
		ZLog.Log((object)"add food switch");
		if (item != null)
		{
			return OnUseItem(user, item);
		}
		return OnInteract(user);
	}

	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if ((Object)(object)m_addFoodSwitch != (Object)null)
		{
			return false;
		}
		return OnInteract(user);
	}

	private bool OnInteract(Humanoid user)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (HaveDoneItem())
		{
			Player.m_localPlayer.RaiseSkill(Skills.SkillType.Cooking, 0.6f);
			int num = 1;
			float skillFactor = Player.m_localPlayer.GetSkillFactor(Skills.SkillType.Cooking);
			if (Random.value < skillFactor * InventoryGui.instance.m_craftBonusChance)
			{
				num += InventoryGui.instance.m_craftBonusAmount;
				DamageText.instance.ShowText(DamageText.TextType.Bonus, ((Component)this).transform.position + Vector3.up, "+" + InventoryGui.instance.m_craftBonusAmount, player: true);
				InventoryGui.instance.m_craftBonusEffect.Create(((Component)this).transform.position, Quaternion.identity);
				ZLog.Log((object)"Bonus food cooking station!");
			}
			m_nview.InvokeRPC("RPC_RemoveDoneItem", ((Component)user).transform.position, num);
			return true;
		}
		ItemDrop.ItemData itemData = FindCookableItem(user.GetInventory());
		if (itemData == null)
		{
			ItemMessage itemMessage = FindIncompatibleItem(user.GetInventory());
			if (itemMessage != null)
			{
				user.Message(MessageHud.MessageType.Center, itemMessage.m_message + " " + itemMessage.m_item.m_itemData.m_shared.m_name);
			}
			else
			{
				user.Message(MessageHud.MessageType.Center, "$msg_nocookitems");
			}
			return false;
		}
		if (OnUseItem(user, itemData))
		{
			Player.m_localPlayer.RaiseSkill(Skills.SkillType.Cooking, 0.4f);
			return true;
		}
		return false;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if ((Object)(object)m_addFoodSwitch != (Object)null)
		{
			return false;
		}
		return OnUseItem(user, item);
	}

	private bool OnUseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (m_requireFire && !IsFireLit())
		{
			user.Message(MessageHud.MessageType.Center, "$msg_needfire");
			user.UseIemBlockkMessage();
			return false;
		}
		if (GetFreeSlot() == -1)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_nocookroom");
			user.UseIemBlockkMessage();
			return false;
		}
		return CookItem(user, item);
	}

	private bool IsFireLit()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (m_fireCheckPoints != null && m_fireCheckPoints.Length != 0)
		{
			Transform[] fireCheckPoints = m_fireCheckPoints;
			foreach (Transform val in fireCheckPoints)
			{
				if (m_cheapFireCheck)
				{
					if (!EffectArea.IsPointPlus025InsideBurningArea(val.position))
					{
						return false;
					}
				}
				else if (!Object.op_Implicit((Object)(object)EffectArea.IsPointInsideArea(val.position, EffectArea.Type.Burning, m_fireCheckRadius)))
				{
					return false;
				}
			}
			return true;
		}
		if (m_cheapFireCheck)
		{
			return EffectArea.IsPointPlus025InsideBurningArea(((Component)this).transform.position);
		}
		return Object.op_Implicit((Object)(object)EffectArea.IsPointInsideArea(((Component)this).transform.position, EffectArea.Type.Burning, m_fireCheckRadius));
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

	private ItemMessage FindIncompatibleItem(Inventory inventory)
	{
		foreach (ItemMessage incompatibleItem in m_incompatibleItems)
		{
			if (inventory.GetItem(incompatibleItem.m_item.m_itemData.m_shared.m_name) != null)
			{
				return incompatibleItem;
			}
		}
		return null;
	}

	private bool CookItem(Humanoid user, ItemDrop.ItemData item)
	{
		string name = ((Object)item.m_dropPrefab).name;
		if (!m_nview.HasOwner())
		{
			m_nview.ClaimOwnership();
		}
		foreach (ItemMessage incompatibleItem in m_incompatibleItems)
		{
			if (incompatibleItem.m_item.m_itemData.m_shared.m_name == item.m_shared.m_name)
			{
				user.Message(MessageHud.MessageType.Center, incompatibleItem.m_message + " " + incompatibleItem.m_item.m_itemData.m_shared.m_name);
				return true;
			}
		}
		if (!IsItemAllowed(item))
		{
			return false;
		}
		if (GetFreeSlot() == -1)
		{
			return false;
		}
		user.GetInventory().RemoveOneItem(item);
		m_nview.InvokeRPC("RPC_AddItem", name);
		return true;
	}

	private void RPC_AddItem(long sender, string itemName)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (IsItemAllowed(itemName))
		{
			int freeSlot = GetFreeSlot();
			if (freeSlot != -1)
			{
				SetSlot(freeSlot, itemName, 0f, Status.NotDone);
				m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetSlotVisual", freeSlot, itemName);
				m_addEffect.Create(m_slots[freeSlot].position, Quaternion.identity);
			}
		}
	}

	private void SetSlot(int slot, string itemName, float cookedTime, Status status)
	{
		if (m_nview.IsValid())
		{
			m_nview.GetZDO().Set("slot" + slot, itemName);
			m_nview.GetZDO().Set("slot" + slot, cookedTime);
			m_nview.GetZDO().Set("slotstatus" + slot, (int)status);
		}
	}

	private void GetSlot(int slot, out string itemName, out float cookedTime, out Status status)
	{
		if (!m_nview.IsValid())
		{
			itemName = "";
			status = Status.NotDone;
			cookedTime = 0f;
		}
		else
		{
			itemName = m_nview.GetZDO().GetString("slot" + slot);
			cookedTime = m_nview.GetZDO().GetFloat("slot" + slot);
			status = (Status)m_nview.GetZDO().GetInt("slotstatus" + slot);
		}
	}

	private bool IsEmpty()
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			if (m_nview.GetZDO().GetString("slot" + i) != "")
			{
				return false;
			}
		}
		return true;
	}

	private int GetFreeSlot()
	{
		for (int i = 0; i < m_slots.Length; i++)
		{
			if (m_nview.GetZDO().GetString("slot" + i) == "")
			{
				return i;
			}
		}
		return -1;
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

	private ItemConversion GetItemConversion(string itemName)
	{
		foreach (ItemConversion item in m_conversion)
		{
			if (((Object)((Component)item.m_from).gameObject).name == itemName || ((Object)((Component)item.m_to).gameObject).name == itemName)
			{
				return item;
			}
		}
		return null;
	}

	private void SetFuel(float fuel)
	{
		m_nview.GetZDO().Set(ZDOVars.s_fuel, fuel);
	}

	private float GetFuel()
	{
		return m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
	}

	public bool TryGetItems(Player player, Switch switchRef, out List<string> items)
	{
		items = new List<string>();
		if (!Object.op_Implicit((Object)(object)switchRef) && Object.op_Implicit((Object)(object)m_addFoodSwitch))
		{
			return false;
		}
		if (!CanUseItems(player, switchRef))
		{
			return true;
		}
		if ((Object)(object)switchRef == (Object)null || (Object)(object)switchRef == (Object)(object)m_addFoodSwitch)
		{
			items.AddRange(m_conversion.Select((ItemConversion conversion) => conversion.m_from.m_itemData.m_shared.m_name));
		}
		else if ((Object)(object)switchRef == (Object)(object)m_addFuelSwitch)
		{
			items.Add(m_fuelItem.m_itemData.m_shared.m_name);
		}
		return true;
	}

	public bool CanUseItems(Player player, Switch switchRef, bool sendErrorMessage = true)
	{
		if ((Object)(object)switchRef == (Object)null || (Object)(object)switchRef == (Object)(object)m_addFoodSwitch)
		{
			if (m_requireFire && !IsFireLit())
			{
				if (sendErrorMessage)
				{
					player.Message(MessageHud.MessageType.Center, "$msg_needfire");
				}
				return false;
			}
			if (GetFreeSlot() == -1)
			{
				if (sendErrorMessage)
				{
					player.Message(MessageHud.MessageType.Center, "$msg_nocookroom");
				}
				return false;
			}
			if (FindCookableItem(player.GetInventory()) != null)
			{
				return true;
			}
			if (sendErrorMessage)
			{
				player.Message(MessageHud.MessageType.Center, "$msg_nocookitems");
			}
			return false;
		}
		if ((Object)(object)switchRef != (Object)(object)m_addFuelSwitch)
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

	private void OnDrawGizmosSelected()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!m_requireFire)
		{
			return;
		}
		if (m_fireCheckPoints != null && m_fireCheckPoints.Length != 0)
		{
			Transform[] fireCheckPoints = m_fireCheckPoints;
			foreach (Transform obj in fireCheckPoints)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(obj.position, m_fireCheckRadius);
			}
		}
		else
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(((Component)this).transform.position, m_fireCheckRadius);
		}
	}
}
