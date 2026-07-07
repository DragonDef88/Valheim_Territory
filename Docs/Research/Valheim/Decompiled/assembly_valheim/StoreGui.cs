using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StoreGui : MonoBehaviour
{
	private static StoreGui m_instance;

	public GameObject m_rootPanel;

	public Button m_buyButton;

	public Button m_sellButton;

	public RectTransform m_listRoot;

	public GameObject m_listElement;

	public Scrollbar m_listScroll;

	public ScrollRectEnsureVisible m_itemEnsureVisible;

	public TMP_Text m_coinText;

	public EffectList m_buyEffects = new EffectList();

	public EffectList m_sellEffects = new EffectList();

	public float m_hideDistance = 5f;

	public float m_itemSpacing = 64f;

	public ItemDrop m_coinPrefab;

	private List<GameObject> m_itemList = new List<GameObject>();

	private Trader.TradeItem m_selectedItem;

	private Trader m_trader;

	private float m_itemlistBaseSize;

	private int m_hiddenFrames;

	private List<ItemDrop.ItemData> m_tempItems = new List<ItemDrop.ItemData>();

	public RectTransform m_tooltipAnchor;

	public static StoreGui instance => m_instance;

	private void Awake()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		m_instance = this;
		m_rootPanel.SetActive(false);
		Rect rect = m_listRoot.rect;
		m_itemlistBaseSize = ((Rect)(ref rect)).height;
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_instance == (Object)(object)this)
		{
			m_instance = null;
		}
	}

	private void Update()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (!m_rootPanel.activeSelf)
		{
			m_hiddenFrames++;
			return;
		}
		m_hiddenFrames = 0;
		if (!Object.op_Implicit((Object)(object)m_trader))
		{
			Hide();
			return;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null || localPlayer.IsDead() || localPlayer.InCutscene())
		{
			Hide();
			return;
		}
		if (Vector3.Distance(((Component)m_trader).transform.position, ((Component)Player.m_localPlayer).transform.position) > m_hideDistance)
		{
			Hide();
			return;
		}
		if (InventoryGui.IsVisible() || Minimap.IsOpen())
		{
			Hide();
			return;
		}
		if (((Object)(object)Chat.instance == (Object)null || !Chat.instance.HasFocus()) && !Console.IsVisible() && !Menu.IsVisible() && Object.op_Implicit((Object)(object)TextViewer.instance) && !TextViewer.instance.IsVisible() && !localPlayer.InCutscene() && (ZInput.GetButtonDown("JoyButtonB") || ZInput.GetKeyDown((KeyCode)27, true) || ZInput.GetButtonDown("Use")))
		{
			ZInput.ResetButtonStatus("JoyButtonB");
			Hide();
		}
		UpdateBuyButton();
		UpdateSellButton();
		UpdateRecipeGamepadInput();
		m_coinText.text = GetPlayerCoins().ToString();
	}

	public void Show(Trader trader)
	{
		if (!((Object)(object)m_trader == (Object)(object)trader) || !IsVisible())
		{
			m_trader = trader;
			m_rootPanel.SetActive(true);
			FillList();
		}
	}

	public void Hide()
	{
		m_trader = null;
		m_rootPanel.SetActive(false);
	}

	public static bool IsVisible()
	{
		if (Object.op_Implicit((Object)(object)m_instance))
		{
			return m_instance.m_hiddenFrames <= 1;
		}
		return false;
	}

	public void OnBuyItem()
	{
		BuySelectedItem();
	}

	private void BuySelectedItem()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		if (m_selectedItem != null && CanAfford(m_selectedItem))
		{
			int stack = Mathf.Min(m_selectedItem.m_stack, m_selectedItem.m_prefab.m_itemData.m_shared.m_maxStackSize);
			int quality = m_selectedItem.m_prefab.m_itemData.m_quality;
			int variant = m_selectedItem.m_prefab.m_itemData.m_variant;
			if (Player.m_localPlayer.GetInventory().AddItem(((Object)m_selectedItem.m_prefab).name, stack, quality, variant, 0L, "") != null)
			{
				Player.m_localPlayer.GetInventory().RemoveItem(m_coinPrefab.m_itemData.m_shared.m_name, m_selectedItem.m_price);
				m_trader.OnBought(m_selectedItem);
				m_buyEffects.Create(((Component)this).transform.position, Quaternion.identity);
				Player.m_localPlayer.ShowPickupMessage(m_selectedItem.m_prefab.m_itemData, m_selectedItem.m_prefab.m_itemData.m_stack);
				FillList();
				Gogan.LogEvent("Game", "BoughtItem", ((Object)m_selectedItem.m_prefab).name, 0L);
			}
		}
	}

	public void OnSellItem()
	{
		SellItem();
	}

	private void SellItem()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		ItemDrop.ItemData sellableItem = GetSellableItem();
		if (sellableItem != null)
		{
			int stack = sellableItem.m_shared.m_value * sellableItem.m_stack;
			Player.m_localPlayer.GetInventory().RemoveItem(sellableItem);
			Player.m_localPlayer.GetInventory().AddItem(((Object)((Component)m_coinPrefab).gameObject).name, stack, m_coinPrefab.m_itemData.m_quality, m_coinPrefab.m_itemData.m_variant, 0L, "");
			string text = "";
			text = ((sellableItem.m_stack <= 1) ? sellableItem.m_shared.m_name : (sellableItem.m_stack + "x" + sellableItem.m_shared.m_name));
			m_sellEffects.Create(((Component)this).transform.position, Quaternion.identity);
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_sold", new string[2]
			{
				text,
				stack.ToString()
			}), 0, sellableItem.m_shared.m_icons[0]);
			m_trader.OnSold();
			FillList();
			Gogan.LogEvent("Game", "SoldItem", text, 0L);
		}
	}

	private int GetPlayerCoins()
	{
		return Player.m_localPlayer.GetInventory().CountItems(m_coinPrefab.m_itemData.m_shared.m_name);
	}

	private bool CanAfford(Trader.TradeItem item)
	{
		int playerCoins = GetPlayerCoins();
		return item.m_price <= playerCoins;
	}

	private void FillList()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Expected O, but got Unknown
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		int playerCoins = GetPlayerCoins();
		int num = GetSelectedItemIndex();
		List<Trader.TradeItem> availableItems = m_trader.GetAvailableItems();
		foreach (GameObject item in m_itemList)
		{
			Object.Destroy((Object)(object)item);
		}
		m_itemList.Clear();
		float num2 = (float)availableItems.Count * m_itemSpacing;
		num2 = Mathf.Max(m_itemlistBaseSize, num2);
		m_listRoot.SetSizeWithCurrentAnchors((Axis)1, num2);
		for (int i = 0; i < availableItems.Count; i++)
		{
			Trader.TradeItem tradeItem = availableItems[i];
			GameObject element = Object.Instantiate<GameObject>(m_listElement, (Transform)(object)m_listRoot);
			element.SetActive(true);
			Transform transform = element.transform;
			RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			Rect rect = m_listRoot.rect;
			float width = ((Rect)(ref rect)).width;
			rect = val.rect;
			float num3 = (width - ((Rect)(ref rect)).width) / 2f;
			val.anchoredPosition = new Vector2(num3, (float)i * (0f - m_itemSpacing) - num3);
			bool flag = tradeItem.m_price <= playerCoins;
			Image component = ((Component)element.transform.Find("icon")).GetComponent<Image>();
			component.sprite = tradeItem.m_prefab.m_itemData.m_shared.m_icons[0];
			((Graphic)component).color = (Color)(flag ? Color.white : new Color(1f, 0f, 1f, 0f));
			string text = Localization.instance.Localize(tradeItem.m_prefab.m_itemData.m_shared.m_name);
			if (tradeItem.m_stack > 1)
			{
				text = text + " x" + tradeItem.m_stack;
			}
			TMP_Text component2 = ((Component)element.transform.Find("name")).GetComponent<TMP_Text>();
			component2.text = text;
			((Graphic)component2).color = (flag ? Color.white : Color.grey);
			element.GetComponent<UITooltip>().Set(tradeItem.m_prefab.m_itemData.m_shared.m_name, tradeItem.m_prefab.m_itemData.GetTooltip(tradeItem.m_stack), m_tooltipAnchor, default(Vector2));
			TMP_Text component3 = ((Component)Utils.FindChild(element.transform, "price", (IterativeSearchType)0)).GetComponent<TMP_Text>();
			component3.text = tradeItem.m_price.ToString();
			if (!flag)
			{
				((Graphic)component3).color = Color.grey;
			}
			((UnityEvent)element.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				OnSelectedItem(element);
			});
			m_itemList.Add(element);
		}
		if (num < 0)
		{
			num = 0;
		}
		SelectItem(num, center: false);
	}

	private void OnSelectedItem(GameObject button)
	{
		int index = FindSelectedRecipe(button);
		SelectItem(index, center: false);
	}

	private int FindSelectedRecipe(GameObject button)
	{
		for (int i = 0; i < m_itemList.Count; i++)
		{
			if ((Object)(object)m_itemList[i] == (Object)(object)button)
			{
				return i;
			}
		}
		return -1;
	}

	private void SelectItem(int index, bool center)
	{
		ZLog.Log((object)("Setting selected recipe " + index));
		for (int i = 0; i < m_itemList.Count; i++)
		{
			bool active = i == index;
			((Component)m_itemList[i].transform.Find("selected")).gameObject.SetActive(active);
		}
		if (center && index >= 0)
		{
			ScrollRectEnsureVisible itemEnsureVisible = m_itemEnsureVisible;
			Transform transform = m_itemList[index].transform;
			itemEnsureVisible.CenterOnItem((RectTransform)(object)((transform is RectTransform) ? transform : null));
		}
		if (index < 0)
		{
			m_selectedItem = null;
		}
		else
		{
			m_selectedItem = m_trader.GetAvailableItems()[index];
		}
	}

	private void UpdateSellButton()
	{
		((Selectable)m_sellButton).interactable = GetSellableItem() != null;
	}

	private ItemDrop.ItemData GetSellableItem()
	{
		m_tempItems.Clear();
		Player.m_localPlayer.GetInventory().GetValuableItems(m_tempItems);
		foreach (ItemDrop.ItemData tempItem in m_tempItems)
		{
			if (tempItem.m_shared.m_name != m_coinPrefab.m_itemData.m_shared.m_name)
			{
				return tempItem;
			}
		}
		return null;
	}

	private int GetSelectedItemIndex()
	{
		int result = 0;
		List<Trader.TradeItem> availableItems = m_trader.GetAvailableItems();
		for (int i = 0; i < availableItems.Count; i++)
		{
			if (availableItems[i] == m_selectedItem)
			{
				result = i;
			}
		}
		return result;
	}

	private void UpdateBuyButton()
	{
		UITooltip component = ((Component)m_buyButton).GetComponent<UITooltip>();
		if (m_selectedItem != null)
		{
			bool flag = CanAfford(m_selectedItem);
			bool flag2 = Player.m_localPlayer.GetInventory().HaveEmptySlot();
			((Selectable)m_buyButton).interactable = flag && flag2;
			if (!flag)
			{
				component.m_text = Localization.instance.Localize("$msg_missingrequirement");
			}
			else if (!flag2)
			{
				component.m_text = Localization.instance.Localize("$inventory_full");
			}
			else
			{
				component.m_text = "";
			}
		}
		else
		{
			((Selectable)m_buyButton).interactable = false;
			component.m_text = "";
		}
	}

	private void UpdateRecipeGamepadInput()
	{
		if (m_itemList.Count > 0)
		{
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				SelectItem(Mathf.Min(m_itemList.Count - 1, GetSelectedItemIndex() + 1), center: true);
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				SelectItem(Mathf.Max(0, GetSelectedItemIndex() - 1), center: true);
			}
		}
	}
}
