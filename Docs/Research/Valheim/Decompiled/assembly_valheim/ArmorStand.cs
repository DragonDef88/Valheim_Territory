using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmorStand : MonoBehaviour, IHasHoverMenuExtended
{
	[Serializable]
	public class ArmorStandSlot
	{
		public Switch m_switch;

		public VisSlot m_slot;

		public List<ItemDrop.ItemData.ItemType> m_supportedTypes = new List<ItemDrop.ItemData.ItemType>();

		[HideInInspector]
		public ItemDrop.ItemData m_item;

		[HideInInspector]
		public string m_visualName = "";

		[HideInInspector]
		public int m_visualVariant;

		[HideInInspector]
		public string m_currentItemName = "";
	}

	[Serializable]
	public class ArmorStandSupport
	{
		public List<ItemDrop> m_items = new List<ItemDrop>();

		public List<GameObject> m_supports = new List<GameObject>();
	}

	public ZNetView m_netViewOverride;

	private ZNetView m_nview;

	public List<ArmorStandSlot> m_slots = new List<ArmorStandSlot>();

	public List<ArmorStandSupport> m_supports = new List<ArmorStandSupport>();

	public Switch m_changePoseSwitch;

	public Animator m_poseAnimator;

	public string m_name = "";

	public Transform m_dropSpawnPoint;

	public VisEquipment m_visEquipment;

	public EffectList m_effects = new EffectList();

	public EffectList m_destroyEffects = new EffectList();

	public int m_poseCount = 3;

	public int m_startPose;

	private int m_pose;

	public float m_clothSimLodDistance = 10f;

	private bool m_clothLodded;

	private Cloth[] m_cloths;

	private ItemDrop.ItemData m_queuedItem;

	private int m_queuedSlot;

	private static int[] s_itemKeyHashes;

	private static int[] s_variantKeyHashes;

	private void Awake()
	{
		m_nview = (Object.op_Implicit((Object)(object)m_netViewOverride) ? m_netViewOverride : ((Component)this).gameObject.GetComponent<ZNetView>());
		if (m_nview.GetZDO() == null)
		{
			return;
		}
		WearNTear component = ((Component)this).GetComponent<WearNTear>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.m_onDestroyed = (Action)Delegate.Combine(component.m_onDestroyed, new Action(OnDestroyed));
		}
		m_nview.Register<int>("RPC_DropItem", RPC_DropItem);
		m_nview.Register<string>("RPC_DropItemByName", RPC_DropItemByName);
		m_nview.Register("RPC_RequestOwn", RPC_RequestOwn);
		m_nview.Register<int>("RPC_DestroyAttachment", RPC_DestroyAttachment);
		m_nview.Register<int, string, int>("RPC_SetVisualItem", RPC_SetVisualItem);
		m_nview.Register<int>("RPC_SetPose", RPC_SetPose);
		InitKeys();
		((MonoBehaviour)this).InvokeRepeating("UpdateVisual", 1f, 4f);
		SetPose(m_nview.GetZDO().GetInt(ZDOVars.s_pose, m_pose), effect: false);
		foreach (ArmorStandSlot item2 in m_slots)
		{
			if (item2.m_switch.m_onUse != null)
			{
				continue;
			}
			Switch @switch = item2.m_switch;
			@switch.m_onUse = (Switch.Callback)Delegate.Combine(@switch.m_onUse, new Switch.Callback(UseItem));
			Switch switch2 = item2.m_switch;
			switch2.m_onHover = (Switch.TooltipCallback)Delegate.Combine(switch2.m_onHover, (Switch.TooltipCallback)delegate
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				if (!PrivateArea.CheckAccess(((Component)this).transform.position, 0f, flash: false))
				{
					return Localization.instance.Localize(m_name + "\n$piece_noaccess");
				}
				string text = ((GetNrOfAttachedItems() > 0) ? "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_itemstand_take" : "");
				return Localization.instance.Localize(item2.m_switch.m_hoverText + "\n[<color=yellow><b>1-8</b></color>] $piece_itemstand_attach" + text);
			});
		}
		if (!((Object)(object)m_changePoseSwitch != (Object)null) || !((Component)m_changePoseSwitch).gameObject.activeInHierarchy)
		{
			return;
		}
		Switch changePoseSwitch = m_changePoseSwitch;
		changePoseSwitch.m_onUse = (Switch.Callback)Delegate.Combine(changePoseSwitch.m_onUse, (Switch.Callback)delegate
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (!m_nview.IsOwner())
			{
				m_nview.InvokeRPC("RPC_RequestOwn");
			}
			if (!PrivateArea.CheckAccess(((Component)this).transform.position, 0f, flash: false))
			{
				return false;
			}
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetPose", (m_pose + 1 < m_poseCount) ? (m_pose + 1) : 0);
			return true;
		});
		Switch changePoseSwitch2 = m_changePoseSwitch;
		changePoseSwitch2.m_onHover = (Switch.TooltipCallback)Delegate.Combine(changePoseSwitch2.m_onHover, (Switch.TooltipCallback)(() => (!PrivateArea.CheckAccess(((Component)this).transform.position, 0f, flash: false)) ? Localization.instance.Localize(m_name + "\n$piece_noaccess") : Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] Change pose ")));
	}

	private void Update()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Player.m_localPlayer != (Object)null) || m_cloths == null || m_cloths.Length == 0)
		{
			return;
		}
		bool flag = Vector3.Distance(((Component)Player.m_localPlayer).transform.position, ((Component)this).transform.position) > m_clothSimLodDistance * QualitySettings.lodBias;
		if (m_clothLodded == flag)
		{
			return;
		}
		m_clothLodded = flag;
		Cloth[] cloths = m_cloths;
		foreach (Cloth val in cloths)
		{
			if (Object.op_Implicit((Object)(object)val))
			{
				val.enabled = !flag;
			}
		}
	}

	private void OnDestroyed()
	{
		if (m_nview.IsOwner())
		{
			for (int i = 0; i < m_slots.Count; i++)
			{
				DropItem(i);
			}
		}
	}

	private void SetPose(int index, bool effect = true)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		m_pose = index;
		m_poseAnimator.SetInteger("Pose", m_pose);
		if (effect)
		{
			m_effects.Create(((Component)this).transform.position, Quaternion.identity);
		}
		if (m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_pose, m_pose);
		}
	}

	public void RPC_SetPose(long sender, int index)
	{
		SetPose(index);
	}

	private bool UseItem(Switch caller, Humanoid user, ItemDrop.ItemData item)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!PrivateArea.CheckAccess(((Component)this).transform.position, 0f, flash: false))
		{
			return true;
		}
		ArmorStandSlot armorStandSlot = null;
		int num = -1;
		for (int i = 0; i < m_slots.Count; i++)
		{
			if ((Object)(object)m_slots[i].m_switch == (Object)(object)caller && ((item == null && !string.IsNullOrEmpty(m_slots[i].m_visualName)) || (item != null && CanAttach(m_slots[i], item))))
			{
				armorStandSlot = m_slots[i];
				num = i;
				break;
			}
		}
		if (item == null)
		{
			if (armorStandSlot == null || num < 0)
			{
				return false;
			}
			if (HaveAttachment(num))
			{
				m_nview.InvokeRPC("RPC_DropItemByName", ((Object)m_slots[num].m_switch).name);
				return true;
			}
			return false;
		}
		if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Legs && item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Chest)
		{
			int childCount = item.m_dropPrefab.transform.childCount;
			bool flag = false;
			for (int j = 0; j < childCount; j++)
			{
				Transform child = item.m_dropPrefab.transform.GetChild(j);
				if (((Object)((Component)child).gameObject).name == "attach" || ((Object)((Component)child).gameObject).name == "attach_skin")
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (num < 0)
		{
			user.Message(MessageHud.MessageType.Center, "$piece_armorstand_cantattach");
			return true;
		}
		if (HaveAttachment(num))
		{
			return false;
		}
		if (!m_nview.IsOwner())
		{
			m_nview.InvokeRPC("RPC_RequestOwn");
		}
		m_queuedItem = item;
		m_queuedSlot = num;
		((MonoBehaviour)this).CancelInvoke("UpdateAttach");
		((MonoBehaviour)this).InvokeRepeating("UpdateAttach", 0f, 0.1f);
		return true;
	}

	public void DestroyAttachment(int index)
	{
		m_nview.InvokeRPC("RPC_DestroyAttachment", index);
	}

	public void RPC_DestroyAttachment(long sender, int index)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner() && HaveAttachment(index))
		{
			m_nview.GetZDO().Set(s_itemKeyHashes[index], "");
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetVisualItem", index, "", 0);
			m_destroyEffects.Create(m_dropSpawnPoint.position, Quaternion.identity);
		}
	}

	private void RPC_DropItemByName(long sender, string name)
	{
		if (!m_nview.IsOwner())
		{
			return;
		}
		for (int i = 0; i < m_slots.Count; i++)
		{
			if (((Object)m_slots[i].m_switch).name == name)
			{
				DropItem(i);
			}
		}
	}

	private void RPC_DropItem(long sender, int index)
	{
		if (m_nview.IsOwner())
		{
			DropItem(index);
		}
	}

	private void DropItem(int index)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (HaveAttachment(index))
		{
			string @string = m_nview.GetZDO().GetString(s_itemKeyHashes[index]);
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(@string);
			if (Object.op_Implicit((Object)(object)itemPrefab))
			{
				GameObject obj = Object.Instantiate<GameObject>(itemPrefab, m_dropSpawnPoint.position, m_dropSpawnPoint.rotation);
				ItemDrop component = obj.GetComponent<ItemDrop>();
				ItemDrop.LoadFromZDO(index, component.m_itemData, m_nview.GetZDO());
				obj.GetComponent<Rigidbody>().linearVelocity = Vector3.up * 4f;
				m_destroyEffects.Create(m_dropSpawnPoint.position, Quaternion.identity);
			}
			m_nview.GetZDO().Set(s_itemKeyHashes[index], "");
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetVisualItem", index, "", 0);
			UpdateSupports();
			m_cloths = ((Component)this).GetComponentsInChildren<Cloth>();
		}
	}

	private void UpdateAttach()
	{
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			((MonoBehaviour)this).CancelInvoke("UpdateAttach");
			Player localPlayer = Player.m_localPlayer;
			if (m_queuedItem != null && (Object)(object)localPlayer != (Object)null && localPlayer.GetInventory().ContainsItem(m_queuedItem) && !HaveAttachment(m_queuedSlot))
			{
				ItemDrop.ItemData itemData = m_queuedItem.Clone();
				itemData.m_stack = 1;
				m_nview.GetZDO().Set(s_itemKeyHashes[m_queuedSlot], ((Object)m_queuedItem.m_dropPrefab).name);
				ItemDrop.SaveToZDO(m_queuedSlot, itemData, m_nview.GetZDO());
				localPlayer.UnequipItem(m_queuedItem);
				localPlayer.GetInventory().RemoveOneItem(m_queuedItem);
				m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetVisualItem", m_queuedSlot, ((Object)itemData.m_dropPrefab).name, itemData.m_variant);
				m_effects.Create(((Component)this).transform.position, Quaternion.identity);
				Game.instance.IncrementPlayerStat(PlayerStatType.ArmorStandUses);
			}
			m_queuedItem = null;
		}
	}

	private void RPC_RequestOwn(long sender)
	{
		if (m_nview.IsOwner())
		{
			m_nview.GetZDO().SetOwner(sender);
		}
	}

	private void InitKeys()
	{
		SetKeyHashes(ref s_variantKeyHashes, "_variant");
		SetKeyHashes(ref s_itemKeyHashes, "_item");
	}

	private void SetKeyHashes(ref int[] hashArray, string name)
	{
		if (hashArray == null || hashArray.Length < m_slots.Count)
		{
			hashArray = new int[m_slots.Count];
			for (int i = 0; i < m_slots.Count; i++)
			{
				hashArray[i] = StringExtensionMethods.GetStableHashCode(i + name);
			}
		}
	}

	private void UpdateVisual()
	{
		if (!((Object)(object)m_nview == (Object)null) && m_nview.IsValid())
		{
			for (int i = 0; i < m_slots.Count; i++)
			{
				string @string = m_nview.GetZDO().GetString(s_itemKeyHashes[i]);
				int @int = m_nview.GetZDO().GetInt(s_variantKeyHashes[i]);
				SetVisualItem(i, @string, @int);
			}
		}
	}

	private void RPC_SetVisualItem(long sender, int index, string itemName, int variant)
	{
		SetVisualItem(index, itemName, variant);
	}

	private void SetVisualItem(int index, string itemName, int variant)
	{
		ArmorStandSlot armorStandSlot = m_slots[index];
		if (armorStandSlot.m_visualName == itemName && armorStandSlot.m_visualVariant == variant)
		{
			return;
		}
		armorStandSlot.m_visualName = itemName;
		armorStandSlot.m_visualVariant = variant;
		armorStandSlot.m_currentItemName = "";
		if (armorStandSlot.m_visualName == "")
		{
			m_visEquipment.SetItem(armorStandSlot.m_slot, "");
			return;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemName);
		if ((Object)(object)itemPrefab == (Object)null)
		{
			ZLog.LogWarning((object)("Missing item prefab " + itemName));
			return;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		armorStandSlot.m_currentItemName = component.m_itemData.m_shared.m_name;
		ItemDrop component2 = itemPrefab.GetComponent<ItemDrop>();
		if (component2 != null)
		{
			if ((Object)(object)component2.m_itemData.m_dropPrefab == (Object)null)
			{
				component2.m_itemData.m_dropPrefab = itemPrefab.gameObject;
			}
			m_visEquipment.SetItem(armorStandSlot.m_slot, ((Object)component2.m_itemData.m_dropPrefab).name, armorStandSlot.m_visualVariant);
			UpdateSupports();
			m_cloths = ((Component)this).GetComponentsInChildren<Cloth>();
		}
	}

	private void UpdateSupports()
	{
		foreach (ArmorStandSupport support in m_supports)
		{
			foreach (GameObject support2 in support.m_supports)
			{
				support2.SetActive(false);
			}
		}
		foreach (ArmorStandSlot slot in m_slots)
		{
			if (slot.m_item == null)
			{
				continue;
			}
			foreach (ArmorStandSupport support3 in m_supports)
			{
				foreach (ItemDrop item in support3.m_items)
				{
					if (!(item.m_itemData.m_shared.m_name == slot.m_currentItemName))
					{
						continue;
					}
					foreach (GameObject support4 in support3.m_supports)
					{
						support4.SetActive(true);
					}
				}
			}
		}
	}

	private GameObject GetAttachPrefab(GameObject item)
	{
		Transform val = item.transform.Find("attach_skin");
		if (Object.op_Implicit((Object)(object)val))
		{
			return ((Component)val).gameObject;
		}
		val = item.transform.Find("attach");
		if (Object.op_Implicit((Object)(object)val))
		{
			return ((Component)val).gameObject;
		}
		return null;
	}

	private bool CanAttach(ArmorStandSlot slot, ItemDrop.ItemData item)
	{
		if (slot.m_supportedTypes.Count == 0)
		{
			return true;
		}
		return slot.m_supportedTypes.Contains((item.m_shared.m_attachOverride != 0) ? item.m_shared.m_attachOverride : item.m_shared.m_itemType);
	}

	public bool HaveAttachment(int index)
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		return m_nview.GetZDO().GetString(s_itemKeyHashes[index]) != "";
	}

	public string GetAttachedItem(int index)
	{
		if (!m_nview.IsValid())
		{
			return "";
		}
		return m_nview.GetZDO().GetString(s_itemKeyHashes[index]);
	}

	public int GetNrOfAttachedItems()
	{
		int num = 0;
		foreach (ArmorStandSlot slot in m_slots)
		{
			if (slot.m_currentItemName.Length > 0)
			{
				num++;
			}
		}
		return num;
	}

	public bool TryGetItems(Player player, Switch switchRef, out List<string> items)
	{
		items = new List<string>();
		ArmorStandSlot armorStandSlot = m_slots.FirstOrDefault((ArmorStandSlot t) => (Object)(object)t.m_switch == (Object)(object)switchRef);
		if (armorStandSlot == null)
		{
			return false;
		}
		items.Add("type");
		items.AddRange(armorStandSlot.m_supportedTypes.Select((ItemDrop.ItemData.ItemType type) => type.ToString()));
		return true;
	}

	public bool CanUseItems(Player player, Switch switchRef, bool sendErrorMessage = true)
	{
		return m_slots.FirstOrDefault((ArmorStandSlot t) => (Object)(object)t.m_switch == (Object)(object)switchRef) != null;
	}
}
