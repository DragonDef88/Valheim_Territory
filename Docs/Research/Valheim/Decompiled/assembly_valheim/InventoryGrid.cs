using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryGrid : MonoBehaviour
{
	private class Element
	{
		public Vector2i m_pos;

		public GameObject m_go;

		public Image m_icon;

		public TMP_Text m_amount;

		public TMP_Text m_quality;

		public Image m_equiped;

		public Image m_queued;

		public GameObject m_selected;

		public Image m_noteleport;

		public Image m_food;

		public UITooltip m_tooltip;

		public GuiBar m_durability;

		public bool m_used;
	}

	public enum Modifier
	{
		Select,
		Split,
		Move,
		Drop
	}

	public Action<InventoryGrid, ItemDrop.ItemData, Vector2i, Modifier> m_onSelected;

	public Action<InventoryGrid, ItemDrop.ItemData, Vector2i> m_onRightClick;

	public RectTransform m_tooltipAnchor;

	public Action<Vector2i> OnMoveToUpperInventoryGrid;

	public Action<Vector2i> OnMoveToLowerInventoryGrid;

	public GameObject m_elementPrefab;

	public RectTransform m_gridRoot;

	public Scrollbar m_scrollbar;

	public UIGroupHandler m_uiGroup;

	public float m_elementSpace = 10f;

	private int m_width = 4;

	private int m_height = 4;

	private Vector2i m_selected = new Vector2i(0, 0);

	private Inventory m_inventory;

	private List<Element> m_elements = new List<Element>();

	private bool jumpToNextContainer;

	private readonly Color m_foodEitrColor = new Color(0.6f, 0.6f, 1f, 1f);

	private readonly Color m_foodHealthColor = new Color(1f, 0.5f, 0.5f, 1f);

	private readonly Color m_foodStaminaColor = new Color(1f, 1f, 0.5f, 1f);

	internal int GridWidth => m_width;

	internal Vector2i SelectionGridPosition => m_selected;

	protected void Awake()
	{
	}

	public void ResetView()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		Rect rect = m_gridRoot.rect;
		float height = ((Rect)(ref rect)).height;
		rect = val.rect;
		if (height > ((Rect)(ref rect)).height)
		{
			m_gridRoot.pivot = new Vector2(m_gridRoot.pivot.x, 1f);
		}
		else
		{
			m_gridRoot.pivot = new Vector2(m_gridRoot.pivot.x, 0.5f);
		}
		m_gridRoot.anchoredPosition = new Vector2(0f, 0f);
	}

	public void UpdateInventory(Inventory inventory, Player player, ItemDrop.ItemData dragItem)
	{
		m_inventory = inventory;
		UpdateGamepad();
		UpdateGui(player, dragItem);
	}

	private void UpdateGamepad()
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		if (!m_uiGroup.IsActive || Console.IsVisible())
		{
			return;
		}
		if (ZInput.GetButtonDown("JoyDPadLeft") || ZInput.GetButtonDown("JoyLStickLeft"))
		{
			m_selected.x = Mathf.Max(0, m_selected.x - 1);
		}
		if (ZInput.GetButtonDown("JoyDPadRight") || ZInput.GetButtonDown("JoyLStickRight"))
		{
			m_selected.x = Mathf.Min(m_width - 1, m_selected.x + 1);
		}
		if (ZInput.GetButtonDown("JoyDPadUp") || ZInput.GetButtonDown("JoyLStickUp"))
		{
			if (m_selected.y - 1 < 0)
			{
				if (!jumpToNextContainer)
				{
					return;
				}
				OnMoveToUpperInventoryGrid?.Invoke(m_selected);
			}
			else
			{
				m_selected.y = Mathf.Max(0, m_selected.y - 1);
				jumpToNextContainer = false;
			}
		}
		if (!ZInput.GetButton("JoyDPadUp") && !ZInput.GetButton("JoyLStickUp") && m_selected.y - 1 <= 0)
		{
			jumpToNextContainer = true;
		}
		if (ZInput.GetButtonDown("JoyDPadDown") || ZInput.GetButtonDown("JoyLStickDown"))
		{
			if (m_selected.y + 1 > m_height - 1)
			{
				if (!jumpToNextContainer)
				{
					return;
				}
				OnMoveToLowerInventoryGrid?.Invoke(m_selected);
			}
			else
			{
				m_selected.y = Mathf.Min(m_width - 1, m_selected.y + 1);
				jumpToNextContainer = false;
			}
		}
		if (!ZInput.GetButton("JoyDPadDown") && !ZInput.GetButton("JoyLStickDown") && m_selected.y + 1 >= m_height - 1)
		{
			jumpToNextContainer = true;
		}
		if (ZInput.GetButtonDown("JoyButtonA"))
		{
			Modifier arg = Modifier.Select;
			if (ZInput.GetButton("JoyLTrigger"))
			{
				arg = Modifier.Split;
			}
			if (ZInput.GetButton("JoyRTrigger"))
			{
				arg = Modifier.Drop;
			}
			ItemDrop.ItemData gamepadSelectedItem = GetGamepadSelectedItem();
			m_onSelected(this, gamepadSelectedItem, m_selected, arg);
		}
		if (ZInput.GetButtonDown("JoyButtonX"))
		{
			ItemDrop.ItemData gamepadSelectedItem2 = GetGamepadSelectedItem();
			if (ZInput.GetButton("JoyLTrigger"))
			{
				m_onSelected(this, gamepadSelectedItem2, m_selected, Modifier.Move);
			}
			else
			{
				m_onRightClick(this, gamepadSelectedItem2, m_selected);
			}
		}
	}

	private void UpdateGui(Player player, ItemDrop.ItemData dragItem)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0654: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_069b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0688: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		int width = m_inventory.GetWidth();
		int height = m_inventory.GetHeight();
		if (m_selected.x >= width - 1)
		{
			m_selected.x = width - 1;
		}
		if (m_selected.y >= height - 1)
		{
			m_selected.y = height - 1;
		}
		if (m_width != width || m_height != height)
		{
			m_width = width;
			m_height = height;
			foreach (Element element4 in m_elements)
			{
				Object.Destroy((Object)(object)element4.m_go);
			}
			m_elements.Clear();
			Vector2 widgetSize = GetWidgetSize();
			Rect rect = val.rect;
			Vector2 val2 = new Vector2(((Rect)(ref rect)).width / 2f, 0f) - new Vector2(widgetSize.x, 0f) * 0.5f;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					Vector2 val3 = Vector2.op_Implicit(new Vector3((float)j * m_elementSpace, (float)i * (0f - m_elementSpace)));
					GameObject val4 = Object.Instantiate<GameObject>(m_elementPrefab, (Transform)(object)m_gridRoot);
					Transform transform2 = val4.transform;
					((RectTransform)((transform2 is RectTransform) ? transform2 : null)).anchoredPosition = val2 + val3;
					UIInputHandler componentInChildren = val4.GetComponentInChildren<UIInputHandler>();
					componentInChildren.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(componentInChildren.m_onRightDown, new Action<UIInputHandler>(OnRightClick));
					componentInChildren.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(componentInChildren.m_onLeftDown, new Action<UIInputHandler>(OnLeftClick));
					TMP_Text component = ((Component)val4.transform.Find("binding")).GetComponent<TMP_Text>();
					if (Object.op_Implicit((Object)(object)player) && i == 0)
					{
						component.text = (j + 1).ToString();
					}
					else
					{
						((Behaviour)component).enabled = false;
					}
					Element element = new Element();
					element.m_pos = new Vector2i(j, i);
					element.m_go = val4;
					element.m_icon = ((Component)val4.transform.Find("icon")).GetComponent<Image>();
					element.m_amount = ((Component)val4.transform.Find("amount")).GetComponent<TMP_Text>();
					element.m_quality = ((Component)val4.transform.Find("quality")).GetComponent<TMP_Text>();
					element.m_equiped = ((Component)val4.transform.Find("equiped")).GetComponent<Image>();
					element.m_queued = ((Component)val4.transform.Find("queued")).GetComponent<Image>();
					element.m_noteleport = ((Component)val4.transform.Find("noteleport")).GetComponent<Image>();
					element.m_food = ((Component)val4.transform.Find("foodicon")).GetComponent<Image>();
					element.m_selected = ((Component)val4.transform.Find("selected")).gameObject;
					element.m_tooltip = val4.GetComponent<UITooltip>();
					element.m_durability = ((Component)val4.transform.Find("durability")).GetComponent<GuiBar>();
					m_elements.Add(element);
				}
			}
		}
		foreach (Element element5 in m_elements)
		{
			element5.m_used = false;
		}
		bool flag = m_uiGroup.IsActive && ZInput.IsGamepadActive();
		List<ItemDrop.ItemData> allItems = m_inventory.GetAllItems();
		Element element2 = (flag ? GetElement(m_selected.x, m_selected.y, width) : GetHoveredElement());
		foreach (ItemDrop.ItemData item in allItems)
		{
			Element element3 = GetElement(item.m_gridPos.x, item.m_gridPos.y, width);
			element3.m_used = true;
			((Behaviour)element3.m_icon).enabled = true;
			element3.m_icon.sprite = item.GetIcon();
			((Graphic)element3.m_icon).color = ((item == dragItem) ? Color.grey : Color.white);
			bool flag2 = item.m_shared.m_useDurability && item.m_durability < item.GetMaxDurability();
			((Component)element3.m_durability).gameObject.SetActive(flag2);
			if (flag2)
			{
				if (item.m_durability <= 0f)
				{
					element3.m_durability.SetValue(1f);
					element3.m_durability.SetColor((Color)((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : new Color(0f, 0f, 0f, 0f)));
				}
				else
				{
					element3.m_durability.SetValue(item.GetDurabilityPercentage());
					element3.m_durability.ResetColor();
				}
			}
			((Behaviour)element3.m_equiped).enabled = Object.op_Implicit((Object)(object)player) && item.m_equipped;
			((Behaviour)element3.m_queued).enabled = Object.op_Implicit((Object)(object)player) && player.IsEquipActionQueued(item);
			((Behaviour)element3.m_noteleport).enabled = !item.m_shared.m_teleportable && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.TeleportAll);
			if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable && (item.m_shared.m_food > 0f || item.m_shared.m_foodStamina > 0f || item.m_shared.m_foodEitr > 0f))
			{
				((Behaviour)element3.m_food).enabled = true;
				if (item.m_shared.m_food < item.m_shared.m_foodEitr / 2f && item.m_shared.m_foodStamina < item.m_shared.m_foodEitr / 2f)
				{
					((Graphic)element3.m_food).color = m_foodEitrColor;
				}
				else if (item.m_shared.m_foodStamina < item.m_shared.m_food / 2f)
				{
					((Graphic)element3.m_food).color = m_foodHealthColor;
				}
				else if (item.m_shared.m_food < item.m_shared.m_foodStamina / 2f)
				{
					((Graphic)element3.m_food).color = m_foodStaminaColor;
				}
				else
				{
					((Graphic)element3.m_food).color = Color.white;
				}
			}
			else
			{
				((Behaviour)element3.m_food).enabled = false;
			}
			if (element2 == element3)
			{
				CreateItemTooltip(item, element3.m_tooltip);
			}
			((Behaviour)element3.m_quality).enabled = item.m_shared.m_maxQuality > 1;
			if (item.m_shared.m_maxQuality > 1)
			{
				element3.m_quality.text = item.m_quality.ToString();
			}
			((Behaviour)element3.m_amount).enabled = item.m_shared.m_maxStackSize > 1;
			if (item.m_shared.m_maxStackSize > 1)
			{
				element3.m_amount.text = $"{item.m_stack}/{item.m_shared.m_maxStackSize}";
			}
		}
		foreach (Element element6 in m_elements)
		{
			element6.m_selected.SetActive(flag && element6.m_pos == m_selected);
			if (!element6.m_used)
			{
				((Component)element6.m_durability).gameObject.SetActive(false);
				((Behaviour)element6.m_icon).enabled = false;
				((Behaviour)element6.m_amount).enabled = false;
				((Behaviour)element6.m_quality).enabled = false;
				((Behaviour)element6.m_equiped).enabled = false;
				((Behaviour)element6.m_queued).enabled = false;
				((Behaviour)element6.m_noteleport).enabled = false;
				((Behaviour)element6.m_food).enabled = false;
				element6.m_tooltip.m_text = "";
				element6.m_tooltip.m_topic = "";
			}
		}
		float num = (float)height * m_elementSpace;
		m_gridRoot.SetSizeWithCurrentAnchors((Axis)1, num);
	}

	private void CreateItemTooltip(ItemDrop.ItemData item, UITooltip tooltip)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		tooltip.Set(item.m_shared.m_name, item.GetTooltip(), m_tooltipAnchor, default(Vector2));
	}

	public Vector2 GetWidgetSize()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((float)m_width * m_elementSpace, (float)m_height * m_elementSpace);
	}

	private void OnRightClick(UIInputHandler element)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = ((Component)element).gameObject;
		Vector2i buttonPos = GetButtonPos(gameObject);
		ItemDrop.ItemData itemAt = m_inventory.GetItemAt(buttonPos.x, buttonPos.y);
		if (m_onRightClick != null)
		{
			m_onRightClick(this, itemAt, buttonPos);
		}
	}

	private void OnLeftClick(UIInputHandler clickHandler)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = ((Component)clickHandler).gameObject;
		Vector2i buttonPos = GetButtonPos(gameObject);
		ItemDrop.ItemData itemAt = m_inventory.GetItemAt(buttonPos.x, buttonPos.y);
		Modifier arg = Modifier.Select;
		if (ZInput.GetKey((KeyCode)304, true) || ZInput.GetKey((KeyCode)303, true))
		{
			arg = Modifier.Split;
		}
		else if (ZInput.GetKey((KeyCode)306, true) || ZInput.GetKey((KeyCode)305, true))
		{
			arg = Modifier.Move;
		}
		if (m_onSelected != null)
		{
			m_onSelected(this, itemAt, buttonPos, arg);
		}
	}

	private Element GetElement(int x, int y, int width)
	{
		int index = y * width + x;
		return m_elements[index];
	}

	private Element GetHoveredElement()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		foreach (Element element in m_elements)
		{
			Transform transform = element.m_go.transform;
			Transform obj = ((transform is RectTransform) ? transform : null);
			Vector2 val = Vector2.op_Implicit(obj.InverseTransformPoint(ZInput.mousePosition));
			Rect rect = ((RectTransform)obj).rect;
			if (((Rect)(ref rect)).Contains(val))
			{
				return element;
			}
		}
		return null;
	}

	private Vector2i GetButtonPos(GameObject go)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_elements.Count; i++)
		{
			if ((Object)(object)m_elements[i].m_go == (Object)(object)go)
			{
				int num = i / m_width;
				return new Vector2i(i - num * m_width, num);
			}
		}
		return new Vector2i(-1, -1);
	}

	public bool DropItem(Inventory fromInventory, ItemDrop.ItemData item, int amount, Vector2i pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		ItemDrop.ItemData itemAt = m_inventory.GetItemAt(pos.x, pos.y);
		if (itemAt == item)
		{
			return true;
		}
		if (itemAt != null && (itemAt.m_shared.m_name != item.m_shared.m_name || (item.m_shared.m_maxQuality > 1 && itemAt.m_quality != item.m_quality) || itemAt.m_shared.m_maxStackSize == 1) && item.m_stack == amount)
		{
			fromInventory.RemoveItem(item);
			fromInventory.MoveItemToThis(m_inventory, itemAt, itemAt.m_stack, item.m_gridPos.x, item.m_gridPos.y);
			m_inventory.MoveItemToThis(fromInventory, item, amount, pos.x, pos.y);
			return true;
		}
		return m_inventory.MoveItemToThis(fromInventory, item, amount, pos.x, pos.y);
	}

	public ItemDrop.ItemData GetItem(Vector2i cursorPosition)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		foreach (Element element in m_elements)
		{
			Transform transform = element.m_go.transform;
			if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)(object)((transform is RectTransform) ? transform : null), ((Vector2i)(ref cursorPosition)).ToVector2()))
			{
				Vector2i buttonPos = GetButtonPos(element.m_go);
				return m_inventory.GetItemAt(buttonPos.x, buttonPos.y);
			}
		}
		return null;
	}

	public Inventory GetInventory()
	{
		return m_inventory;
	}

	public void SetSelection(Vector2i pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		m_selected = pos;
	}

	public ItemDrop.ItemData GetGamepadSelectedItem()
	{
		if (!m_uiGroup.IsActive)
		{
			return null;
		}
		if (m_inventory == null)
		{
			return null;
		}
		return m_inventory.GetItemAt(m_selected.x, m_selected.y);
	}

	public RectTransform GetGamepadSelectedElement()
	{
		if (!m_uiGroup.IsActive)
		{
			return null;
		}
		if (m_selected.x < 0 || m_selected.x >= m_width || m_selected.y < 0 || m_selected.y >= m_height)
		{
			return null;
		}
		Transform transform = GetElement(m_selected.x, m_selected.y, m_width).m_go.transform;
		return (RectTransform)(object)((transform is RectTransform) ? transform : null);
	}
}
