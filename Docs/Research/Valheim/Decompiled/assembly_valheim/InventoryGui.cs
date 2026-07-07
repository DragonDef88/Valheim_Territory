using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryGui : MonoBehaviour
{
	public enum SortMethod
	{
		Original,
		Name,
		Type,
		Weight,
		Count
	}

	private struct RecipeDataPair
	{
		public Recipe Recipe { get; private set; }

		public ItemDrop.ItemData ItemData { get; private set; }

		public GameObject InterfaceElement { get; private set; }

		public bool CanCraft { get; private set; }

		public RecipeDataPair(Recipe recipe, ItemDrop.ItemData data, GameObject element, bool canCraft)
		{
			Recipe = recipe;
			ItemData = data;
			InterfaceElement = element;
			CanCraft = canCraft;
		}
	}

	private List<Recipe> m_tempRecipes = new List<Recipe>();

	private List<ItemDrop.ItemData> m_tempItemList = new List<ItemDrop.ItemData>();

	private List<ItemDrop.ItemData> m_tempWornItems = new List<ItemDrop.ItemData>();

	private static InventoryGui m_instance;

	[Header("Gamepad")]
	public UIGroupHandler m_inventoryGroup;

	public UIGroupHandler[] m_uiGroups = (UIGroupHandler[])(object)new UIGroupHandler[0];

	private int m_activeGroup = 1;

	[SerializeField]
	private bool m_inventoryGroupCycling;

	[Header("Other")]
	public Transform m_inventoryRoot;

	public RectTransform m_player;

	public RectTransform m_crafting;

	public RectTransform m_info;

	public RectTransform m_container;

	public GameObject m_dragItemPrefab;

	public TMP_Text m_containerName;

	public Button m_dropButton;

	public Button m_takeAllButton;

	public Button m_stackAllButton;

	public float m_autoCloseDistance = 4f;

	[Header("Crafting dialog")]
	public Button m_tabCraft;

	public Button m_tabUpgrade;

	public GameObject m_recipeElementPrefab;

	public RectTransform m_recipeListRoot;

	public Scrollbar m_recipeListScroll;

	public float m_recipeListSpace = 30f;

	public float m_craftBonusChance = 0.25f;

	public int m_craftBonusAmount = 1;

	public EffectList m_craftBonusEffect;

	public float m_craftDurationSkillMaxDecrease = 0.6f;

	public float m_craftDuration = 2f;

	public int m_multiCraftAmount = 5;

	public float m_multiCraftDuration = 6f;

	public TMP_Text m_craftingStationName;

	public Image m_craftingStationIcon;

	public RectTransform m_craftingStationLevelRoot;

	public TMP_Text m_craftingStationLevel;

	public TMP_Text m_recipeName;

	public TMP_Text m_recipeDecription;

	public Image m_recipeIcon;

	public GameObject[] m_recipeRequirementList = (GameObject[])(object)new GameObject[0];

	public Button m_variantButton;

	public Button m_craftButton;

	public Button m_craftCancelButton;

	public Transform m_craftProgressPanel;

	public GuiBar m_craftProgressBar;

	[Header("Repair")]
	public Button m_repairButton;

	public Transform m_repairPanel;

	public Image m_repairButtonGlow;

	public Transform m_repairPanelSelection;

	[Header("Upgrade")]
	public Image m_upgradeItemIcon;

	public GuiBar m_upgradeItemDurability;

	public TMP_Text m_upgradeItemName;

	public TMP_Text m_upgradeItemQuality;

	public GameObject m_upgradeItemQualityArrow;

	public TMP_Text m_upgradeItemNextQuality;

	public TMP_Text m_upgradeItemIndex;

	public TMP_Text m_itemCraftType;

	public RectTransform m_qualityPanel;

	public Button m_qualityLevelDown;

	public Button m_qualityLevelUp;

	public TMP_Text m_qualityLevel;

	public Image m_minStationLevelIcon;

	private Color m_minStationLevelBasecolor;

	public TMP_Text m_minStationLevelText;

	public ScrollRectEnsureVisible m_recipeEnsureVisible;

	[Header("Variants dialog")]
	public VariantDialog m_variantDialog;

	[Header("Skills dialog")]
	public SkillsDialog m_skillsDialog;

	[Header("Texts dialog")]
	public TextsDialog m_textsDialog;

	[Header("Split dialog")]
	public Transform m_splitPanel;

	public Slider m_splitSlider;

	public TMP_Text m_splitAmount;

	public Button m_splitCancelButton;

	public Button m_splitOkButton;

	public Image m_splitIcon;

	public TMP_Text m_splitIconName;

	[Header("Character stats")]
	public Transform m_infoPanel;

	public TMP_Text m_playerName;

	public TMP_Text m_armor;

	public TMP_Text m_weight;

	public TMP_Text m_containerWeight;

	public Toggle m_pvp;

	[Header("Trophies")]
	public GameObject m_trophiesPanel;

	public RectTransform m_trophieListRoot;

	public float m_trophieListSpace = 30f;

	public GameObject m_trophieElementPrefab;

	public Scrollbar m_trophyListScroll;

	[Header("Effects")]
	public EffectList m_moveItemEffects = new EffectList();

	public EffectList m_craftItemEffects = new EffectList();

	public EffectList m_craftItemDoneEffects = new EffectList();

	public EffectList m_openInventoryEffects = new EffectList();

	public EffectList m_closeInventoryEffects = new EffectList();

	public EffectList m_setActiveGroupEffects = new EffectList();

	[HideInInspector]
	public InventoryGrid m_playerGrid;

	private InventoryGrid m_containerGrid;

	private Animator m_animator;

	private Container m_currentContainer;

	private bool m_firstContainerUpdate = true;

	private float m_containerHoldTime;

	private float m_containerHoldPlaceStackDelay = 0.5f;

	private float m_containerHoldExitDelay = 0.5f;

	private int m_containerHoldState;

	private RecipeDataPair m_selectedRecipe;

	private List<ItemDrop.ItemData> m_upgradeItems = new List<ItemDrop.ItemData>();

	private List<Piece.Requirement> m_reqList = new List<Piece.Requirement>();

	private int m_selectedVariant;

	private Recipe m_craftRecipe;

	private ItemDrop.ItemData m_craftUpgradeItem;

	private int m_craftVariant;

	private List<RecipeDataPair> m_availableRecipes = new List<RecipeDataPair>();

	private GameObject m_dragGo;

	private ItemDrop.ItemData m_dragItem;

	private Inventory m_dragInventory;

	private int m_dragAmount = 1;

	private ItemDrop.ItemData m_splitItem;

	private Inventory m_splitInventory;

	private float m_craftTimer = -1f;

	private bool m_multiCrafting;

	private float m_recipeListBaseSize;

	private int m_hiddenFrames = 9999;

	private string m_splitInput = "";

	private DateTime m_lastSplitInput;

	public float m_splitNumInputTimeoutSec = 0.5f;

	private List<GameObject> m_trophyList = new List<GameObject>();

	private float m_trophieListBaseSize;

	public static InventoryGui instance => m_instance;

	public int ActiveGroup => m_activeGroup;

	public bool IsSkillsPanelOpen => ((Component)m_skillsDialog).gameObject.activeInHierarchy;

	public bool IsTextPanelOpen => ((Component)m_textsDialog).gameObject.activeInHierarchy;

	public bool IsTrophisPanelOpen => m_trophiesPanel.activeInHierarchy;

	public InventoryGrid ContainerGrid => m_containerGrid;

	private void Awake()
	{
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Expected O, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Expected O, but got Unknown
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Expected O, but got Unknown
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Expected O, but got Unknown
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Expected O, but got Unknown
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Expected O, but got Unknown
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Expected O, but got Unknown
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		m_instance = this;
		m_animator = ((Component)this).GetComponent<Animator>();
		((Component)m_inventoryRoot).gameObject.SetActive(true);
		((Component)m_player).gameObject.SetActive(true);
		((Component)m_crafting).gameObject.SetActive(true);
		((Component)m_info).gameObject.SetActive(true);
		((Component)m_container).gameObject.SetActive(false);
		((Component)m_splitPanel).gameObject.SetActive(false);
		m_trophiesPanel.SetActive(false);
		((Component)m_variantDialog).gameObject.SetActive(false);
		((Component)m_skillsDialog).gameObject.SetActive(false);
		((Component)m_textsDialog).gameObject.SetActive(false);
		m_playerGrid = ((Component)m_player).GetComponentInChildren<InventoryGrid>();
		m_containerGrid = ((Component)m_container).GetComponentInChildren<InventoryGrid>();
		InventoryGrid playerGrid = m_playerGrid;
		playerGrid.m_onSelected = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier>)Delegate.Combine(playerGrid.m_onSelected, new Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier>(OnSelectedItem));
		InventoryGrid playerGrid2 = m_playerGrid;
		playerGrid2.m_onRightClick = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i>)Delegate.Combine(playerGrid2.m_onRightClick, new Action<InventoryGrid, ItemDrop.ItemData, Vector2i>(OnRightClickItem));
		InventoryGrid containerGrid = m_containerGrid;
		containerGrid.m_onSelected = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier>)Delegate.Combine(containerGrid.m_onSelected, new Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier>(OnSelectedItem));
		InventoryGrid containerGrid2 = m_containerGrid;
		containerGrid2.m_onRightClick = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i>)Delegate.Combine(containerGrid2.m_onRightClick, new Action<InventoryGrid, ItemDrop.ItemData, Vector2i>(OnRightClickItem));
		InventoryGrid playerGrid3 = m_playerGrid;
		playerGrid3.OnMoveToLowerInventoryGrid = (Action<Vector2i>)Delegate.Combine(playerGrid3.OnMoveToLowerInventoryGrid, new Action<Vector2i>(MoveToLowerInventoryGrid));
		InventoryGrid containerGrid3 = m_containerGrid;
		containerGrid3.OnMoveToUpperInventoryGrid = (Action<Vector2i>)Delegate.Combine(containerGrid3.OnMoveToUpperInventoryGrid, new Action<Vector2i>(MoveToUpperInventoryGrid));
		((UnityEvent)m_craftButton.onClick).AddListener(new UnityAction(OnCraftPressed));
		((UnityEvent)m_craftCancelButton.onClick).AddListener(new UnityAction(OnCraftCancelPressed));
		((UnityEvent)m_dropButton.onClick).AddListener(new UnityAction(OnDropOutside));
		((UnityEvent)m_takeAllButton.onClick).AddListener(new UnityAction(OnTakeAll));
		((UnityEvent)m_stackAllButton.onClick).AddListener(new UnityAction(OnStackAll));
		((UnityEvent)m_repairButton.onClick).AddListener(new UnityAction(OnRepairPressed));
		((UnityEvent<float>)(object)m_splitSlider.onValueChanged).AddListener((UnityAction<float>)OnSplitSliderChanged);
		((UnityEvent)m_splitCancelButton.onClick).AddListener(new UnityAction(OnSplitCancel));
		((UnityEvent)m_splitOkButton.onClick).AddListener(new UnityAction(OnSplitOk));
		VariantDialog variantDialog = m_variantDialog;
		variantDialog.m_selected = (Action<int>)Delegate.Combine(variantDialog.m_selected, new Action<int>(OnVariantSelected));
		Rect rect = m_recipeListRoot.rect;
		m_recipeListBaseSize = ((Rect)(ref rect)).height;
		rect = m_trophieListRoot.rect;
		m_trophieListBaseSize = ((Rect)(ref rect)).height;
		m_minStationLevelBasecolor = ((Graphic)m_minStationLevelText).color;
		((Selectable)m_tabCraft).interactable = false;
		((Selectable)m_tabUpgrade).interactable = true;
	}

	private void MoveToLowerInventoryGrid(Vector2i previousGridPosition)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (m_inventoryGroup.IsActive && IsContainerOpen())
		{
			int num = (int)Math.Ceiling((float)(m_playerGrid.GridWidth - m_containerGrid.GridWidth) / 2f);
			Vector2i selectionGridPosition = m_containerGrid.SelectionGridPosition;
			int num2 = Mathf.Max(0, previousGridPosition.x - num);
			selectionGridPosition.x = Mathf.Min(num2, m_containerGrid.GridWidth - 1);
			m_containerGrid.SetSelection(selectionGridPosition);
			SetActiveGroup(m_activeGroup - 1);
		}
	}

	private void MoveToUpperInventoryGrid(Vector2i previousGridPosition)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (m_inventoryGroup.IsActive)
		{
			int num = (int)Math.Ceiling((float)(m_playerGrid.GridWidth - m_containerGrid.GridWidth) / 2f);
			Vector2i selectionGridPosition = m_playerGrid.SelectionGridPosition;
			int num2 = Mathf.Max(0, previousGridPosition.x + num);
			int num3 = Mathf.Min(m_playerGrid.GridWidth - 1, previousGridPosition.x);
			selectionGridPosition.x = Mathf.Max(num2, num3);
			m_playerGrid.SetSelection(selectionGridPosition);
			SetActiveGroup(m_activeGroup + 1);
		}
	}

	private void OnDestroy()
	{
		m_instance = null;
	}

	private void Update()
	{
		bool @bool = m_animator.GetBool("visible");
		if (!@bool)
		{
			m_hiddenFrames++;
		}
		Player localPlayer = Player.m_localPlayer;
		if ((Object)(object)localPlayer == (Object)null || localPlayer.IsDead() || localPlayer.InCutscene() || localPlayer.IsTeleporting())
		{
			Hide();
			return;
		}
		if (m_craftTimer < 0f && ((Object)(object)Chat.instance == (Object)null || !Chat.instance.HasFocus()) && !Console.IsVisible() && !Menu.IsVisible() && Object.op_Implicit((Object)(object)TextViewer.instance) && !TextViewer.instance.IsVisible() && !localPlayer.InCutscene() && !GameCamera.InFreeFly() && !Minimap.IsOpen())
		{
			if (m_trophiesPanel.activeSelf && (ZInput.GetButtonDown("JoyButtonB") || ZInput.GetKeyDown((KeyCode)27, true)))
			{
				m_trophiesPanel.SetActive(false);
			}
			else if (((Component)m_skillsDialog).gameObject.activeSelf && (ZInput.GetButtonDown("JoyButtonB") || ZInput.GetKeyDown((KeyCode)27, true)))
			{
				m_skillsDialog.OnClose();
			}
			else if (((Component)m_textsDialog).gameObject.activeSelf && (ZInput.GetButtonDown("JoyButtonB") || ZInput.GetKeyDown((KeyCode)27, true)))
			{
				((Component)m_textsDialog).gameObject.SetActive(false);
			}
			else if (((Component)m_splitPanel).gameObject.activeSelf && (ZInput.GetButtonDown("JoyButtonB") || ZInput.GetKeyDown((KeyCode)27, true)))
			{
				((Component)m_splitPanel).gameObject.SetActive(false);
			}
			else if (((Component)m_variantDialog).gameObject.activeSelf && (ZInput.GetButtonDown("JoyButtonB") || ZInput.GetKeyDown((KeyCode)27, true)))
			{
				((Component)m_variantDialog).gameObject.SetActive(false);
			}
			else if (@bool)
			{
				if (ZInput.GetButtonDown("Inventory") || ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("JoyButtonY") || ZInput.GetKeyDown((KeyCode)27, true) || ZInput.GetButtonDown("Use"))
				{
					ZInput.ResetButtonStatus("Inventory");
					ZInput.ResetButtonStatus("JoyButtonB");
					ZInput.ResetButtonStatus("JoyButtonY");
					ZInput.ResetButtonStatus("Use");
					Hide();
				}
			}
			else if ((ZInput.GetButtonDown("Inventory") || ZInput.GetButtonDown("JoyButtonY")) && !Hud.InRadial())
			{
				ZInput.ResetButtonStatus("Inventory");
				ZInput.ResetButtonStatus("JoyButtonY");
				localPlayer.ShowTutorial("inventory", force: true);
				Show(null);
			}
		}
		if (@bool)
		{
			m_hiddenFrames = 0;
			UpdateGamepad();
			UpdateInventory(localPlayer);
			UpdateContainer(localPlayer);
			UpdateItemDrag();
			UpdateCharacterStats(localPlayer);
			UpdateInventoryWeight(localPlayer);
			UpdateContainerWeight();
			UpdateSplitDialog();
			UpdateRecipe(localPlayer, Time.deltaTime);
			UpdateRepair();
		}
	}

	private void UpdateGamepad()
	{
		if (m_inventoryGroup.IsActive)
		{
			if (ZInput.GetButtonDown("JoyTabLeft"))
			{
				SetActiveGroup(m_activeGroup - 1);
			}
			if (ZInput.GetButtonDown("JoyTabRight"))
			{
				SetActiveGroup(m_activeGroup + 1);
			}
			if (m_activeGroup == 0 && !IsContainerOpen())
			{
				SetActiveGroup(1);
			}
			if (m_activeGroup == 3)
			{
				UpdateRecipeGamepadInput();
			}
		}
	}

	private void SetActiveGroup(int index, bool playSound = true)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (!m_inventoryGroupCycling)
		{
			index = Mathf.Clamp(index, 0, m_uiGroups.Length - 1);
		}
		else
		{
			if (index == 0 && !IsContainerOpen())
			{
				index = m_uiGroups.Length - 1;
			}
			index = (index + m_uiGroups.Length) % m_uiGroups.Length;
		}
		m_activeGroup = index;
		for (int i = 0; i < m_uiGroups.Length; i++)
		{
			m_uiGroups[i].SetActive(i == m_activeGroup);
		}
		if (Object.op_Implicit((Object)(object)Player.m_localPlayer) && playSound)
		{
			m_setActiveGroupEffects?.Create(((Component)Player.m_localPlayer).transform.position, Quaternion.identity);
		}
	}

	private void UpdateCharacterStats(Player player)
	{
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		m_playerName.text = playerProfile.GetName();
		float bodyArmor = player.GetBodyArmor();
		m_armor.text = bodyArmor.ToString();
		((Selectable)m_pvp).interactable = player.CanSwitchPVP();
		player.SetPVP(m_pvp.isOn);
	}

	private void UpdateInventoryWeight(Player player)
	{
		int num = Mathf.CeilToInt(player.GetInventory().GetTotalWeight());
		int num2 = Mathf.CeilToInt(player.GetMaxCarryWeight());
		if (num > num2)
		{
			if (Mathf.Sin(Time.time * 10f) > 0f)
			{
				m_weight.text = $"<color=red>{num}</color>/{num2}";
			}
			else
			{
				m_weight.text = $"{num}/{num2}";
			}
		}
		else
		{
			m_weight.text = $"{num}/{num2}";
		}
	}

	private void UpdateContainerWeight()
	{
		if (!((Object)(object)m_currentContainer == (Object)null))
		{
			int num = Mathf.CeilToInt(m_currentContainer.GetInventory().GetTotalWeight());
			m_containerWeight.text = num.ToString();
		}
	}

	private void UpdateInventory(Player player)
	{
		Inventory inventory = player.GetInventory();
		m_playerGrid.UpdateInventory(inventory, player, m_dragItem);
	}

	private void UpdateContainer(Player player)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (!m_animator.GetBool("visible"))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)m_currentContainer) && m_currentContainer.IsOwner())
		{
			m_currentContainer.SetInUse(inUse: true);
			((Component)m_container).gameObject.SetActive(true);
			m_containerGrid.UpdateInventory(m_currentContainer.GetInventory(), null, m_dragItem);
			m_containerName.text = Localization.instance.Localize(m_currentContainer.GetInventory().GetName());
			if (m_firstContainerUpdate)
			{
				m_containerGrid.ResetView();
				m_firstContainerUpdate = false;
				m_containerHoldTime = 0f;
				m_containerHoldState = 0;
			}
			if (Vector3.Distance(((Component)m_currentContainer).transform.position, ((Component)player).transform.position) > m_autoCloseDistance)
			{
				CloseContainer();
			}
			if (ZInput.GetButton("Use") || ZInput.GetButton("JoyUse"))
			{
				m_containerHoldTime += Time.deltaTime;
				if (m_containerHoldTime > m_containerHoldPlaceStackDelay && m_containerHoldState == 0)
				{
					m_currentContainer.StackAll();
					m_containerHoldState = 1;
				}
				else if (m_containerHoldTime > m_containerHoldPlaceStackDelay + m_containerHoldExitDelay && m_containerHoldState == 1)
				{
					Hide();
				}
			}
			else if (m_containerHoldState >= 0)
			{
				m_containerHoldState = -1;
			}
		}
		else
		{
			((Component)m_container).gameObject.SetActive(false);
			if (m_dragInventory != null && m_dragInventory != Player.m_localPlayer.GetInventory())
			{
				SetupDragItem(null, null, 1);
			}
		}
	}

	private RectTransform GetSelectedGamepadElement()
	{
		RectTransform gamepadSelectedElement = m_playerGrid.GetGamepadSelectedElement();
		if (Object.op_Implicit((Object)(object)gamepadSelectedElement))
		{
			return gamepadSelectedElement;
		}
		if (((Component)m_container).gameObject.activeSelf)
		{
			return m_containerGrid.GetGamepadSelectedElement();
		}
		return null;
	}

	private void UpdateItemDrag()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_dragGo))
		{
			return;
		}
		if (ZInput.IsGamepadActive() && !ZInput.IsMouseActive())
		{
			RectTransform selectedGamepadElement = GetSelectedGamepadElement();
			if (Object.op_Implicit((Object)(object)selectedGamepadElement))
			{
				Vector3[] array = (Vector3[])(object)new Vector3[4];
				selectedGamepadElement.GetWorldCorners(array);
				m_dragGo.transform.position = array[2] + new Vector3(0f, 32f, 0f);
			}
			else
			{
				m_dragGo.transform.position = new Vector3(-99999f, 0f, 0f);
			}
		}
		else
		{
			m_dragGo.transform.position = ZInput.mousePosition;
		}
		Image component = ((Component)m_dragGo.transform.Find("icon")).GetComponent<Image>();
		TMP_Text component2 = ((Component)m_dragGo.transform.Find("name")).GetComponent<TMP_Text>();
		TMP_Text component3 = ((Component)m_dragGo.transform.Find("amount")).GetComponent<TMP_Text>();
		component.sprite = m_dragItem.GetIcon();
		component2.text = m_dragItem.m_shared.m_name;
		component3.text = ((m_dragAmount > 1) ? m_dragAmount.ToString() : "");
		if (ZInput.GetMouseButton(1) || ZInput.GetButton("JoyButtonB"))
		{
			SetupDragItem(null, null, 1);
		}
	}

	private void OnTakeAll()
	{
		if (!Player.m_localPlayer.IsTeleporting() && Object.op_Implicit((Object)(object)m_currentContainer))
		{
			SetupDragItem(null, null, 1);
			Inventory inventory = m_currentContainer.GetInventory();
			Player.m_localPlayer.GetInventory().MoveAll(inventory);
		}
	}

	private void OnStackAll()
	{
		if (!Player.m_localPlayer.IsTeleporting() && Object.op_Implicit((Object)(object)m_currentContainer))
		{
			SetupDragItem(null, null, 1);
			m_currentContainer.GetInventory().StackAll(Player.m_localPlayer.GetInventory());
		}
	}

	private void OnDropOutside()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_dragGo))
		{
			ZLog.Log((object)("Drop item " + m_dragItem.m_shared.m_name));
			if (!m_dragInventory.ContainsItem(m_dragItem))
			{
				SetupDragItem(null, null, 1);
			}
			else if (Player.m_localPlayer.DropItem(m_dragInventory, m_dragItem, m_dragAmount))
			{
				m_moveItemEffects.Create(((Component)this).transform.position, Quaternion.identity);
				SetupDragItem(null, null, 1);
				UpdateCraftingPanel();
			}
		}
	}

	private void OnRightClickItem(InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos)
	{
		if (item != null && Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			Player.m_localPlayer.UseItem(grid.GetInventory(), item, fromInventoryGui: true);
		}
	}

	private void OnSelectedItem(InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer.IsTeleporting())
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)m_dragGo))
		{
			m_moveItemEffects.Create(((Component)this).transform.position, Quaternion.identity);
			bool flag = localPlayer.IsItemEquiped(m_dragItem);
			bool flag2 = item != null && localPlayer.IsItemEquiped(item);
			Vector2i gridPos = m_dragItem.m_gridPos;
			if ((m_dragItem.m_shared.m_questItem || (item != null && item.m_shared.m_questItem)) && m_dragInventory != grid.GetInventory())
			{
				return;
			}
			if (!m_dragInventory.ContainsItem(m_dragItem))
			{
				SetupDragItem(null, null, 1);
				return;
			}
			localPlayer.RemoveEquipAction(item);
			localPlayer.RemoveEquipAction(m_dragItem);
			localPlayer.UnequipItem(m_dragItem, triggerEquipEffects: false);
			localPlayer.UnequipItem(item, triggerEquipEffects: false);
			bool num = grid.DropItem(m_dragInventory, m_dragItem, m_dragAmount, pos);
			if (m_dragItem.m_stack < m_dragAmount)
			{
				m_dragAmount = m_dragItem.m_stack;
			}
			if (flag)
			{
				ItemDrop.ItemData itemAt = grid.GetInventory().GetItemAt(pos.x, pos.y);
				if (itemAt != null)
				{
					localPlayer.EquipItem(itemAt, triggerEquipEffects: false);
				}
				if (localPlayer.GetInventory().ContainsItem(m_dragItem))
				{
					localPlayer.EquipItem(m_dragItem, triggerEquipEffects: false);
				}
			}
			if (flag2)
			{
				ItemDrop.ItemData itemAt2 = m_dragInventory.GetItemAt(gridPos.x, gridPos.y);
				if (itemAt2 != null)
				{
					localPlayer.EquipItem(itemAt2, triggerEquipEffects: false);
				}
				if (localPlayer.GetInventory().ContainsItem(item))
				{
					localPlayer.EquipItem(item, triggerEquipEffects: false);
				}
			}
			if (num)
			{
				SetupDragItem(null, null, 1);
				UpdateCraftingPanel();
			}
		}
		else
		{
			if (item == null)
			{
				return;
			}
			switch (mod)
			{
			case InventoryGrid.Modifier.Move:
				if (item.m_shared.m_questItem)
				{
					return;
				}
				if ((Object)(object)m_currentContainer != (Object)null)
				{
					localPlayer.RemoveEquipAction(item);
					localPlayer.UnequipItem(item);
					if (grid.GetInventory() == m_currentContainer.GetInventory())
					{
						localPlayer.GetInventory().MoveItemToThis(grid.GetInventory(), item);
					}
					else
					{
						m_currentContainer.GetInventory().MoveItemToThis(localPlayer.GetInventory(), item);
					}
					m_moveItemEffects.Create(((Component)this).transform.position, Quaternion.identity);
				}
				else if (Player.m_localPlayer.DropItem(grid.GetInventory(), item, item.m_stack))
				{
					m_moveItemEffects.Create(((Component)this).transform.position, Quaternion.identity);
				}
				return;
			case InventoryGrid.Modifier.Drop:
				if (Player.m_localPlayer.DropItem(grid.GetInventory(), item, item.m_stack))
				{
					m_moveItemEffects.Create(((Component)this).transform.position, Quaternion.identity);
				}
				return;
			case InventoryGrid.Modifier.Split:
				if (item.m_stack > 1)
				{
					ShowSplitDialog(item, grid.GetInventory());
					return;
				}
				break;
			}
			SetupDragItem(item, grid.GetInventory(), item.m_stack);
		}
	}

	public static bool IsVisible()
	{
		if (Object.op_Implicit((Object)(object)m_instance))
		{
			return m_instance.m_hiddenFrames <= 1;
		}
		return false;
	}

	public bool IsContainerOpen()
	{
		return (Object)(object)m_currentContainer != (Object)null;
	}

	public void Show(Container container, int activeGroup = 1)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Hud.HidePieceSelection();
		m_animator.SetBool("visible", true);
		SetActiveGroup(activeGroup, playSound: false);
		Player localPlayer = Player.m_localPlayer;
		if (Object.op_Implicit((Object)(object)localPlayer))
		{
			SetupCrafting();
		}
		m_currentContainer = container;
		m_hiddenFrames = 0;
		if (Object.op_Implicit((Object)(object)localPlayer))
		{
			m_openInventoryEffects.Create(((Component)localPlayer).transform.position, Quaternion.identity);
		}
		Gogan.LogEvent("Screen", "Enter", "Inventory", 0L);
	}

	public void Hide()
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (m_animator.GetBool("visible"))
		{
			m_craftTimer = -1f;
			m_animator.SetBool("visible", false);
			m_trophiesPanel.SetActive(false);
			((Component)m_variantDialog).gameObject.SetActive(false);
			((Component)m_skillsDialog).gameObject.SetActive(false);
			((Component)m_textsDialog).gameObject.SetActive(false);
			((Component)m_splitPanel).gameObject.SetActive(false);
			SetupDragItem(null, null, 1);
			if (Object.op_Implicit((Object)(object)m_currentContainer))
			{
				m_currentContainer.SetInUse(inUse: false);
				m_currentContainer = null;
			}
			if (Object.op_Implicit((Object)(object)Player.m_localPlayer))
			{
				m_closeInventoryEffects.Create(((Component)Player.m_localPlayer).transform.position, Quaternion.identity);
			}
			m_containerHoldTime = 0f;
			m_containerHoldState = 0;
			Gogan.LogEvent("Screen", "Exit", "Inventory", 0L);
		}
	}

	private void CloseContainer()
	{
		if (m_dragInventory != null && m_dragInventory != Player.m_localPlayer.GetInventory())
		{
			SetupDragItem(null, null, 1);
		}
		if (Object.op_Implicit((Object)(object)m_currentContainer))
		{
			m_currentContainer.SetInUse(inUse: false);
			m_currentContainer = null;
		}
		((Component)m_splitPanel).gameObject.SetActive(false);
		m_firstContainerUpdate = true;
		((Component)m_container).gameObject.SetActive(false);
	}

	private void SetupCrafting()
	{
		UpdateCraftingPanel(focusView: true);
	}

	private void UpdateCraftingPanel(bool focusView = false)
	{
		Player localPlayer = Player.m_localPlayer;
		if (!Object.op_Implicit((Object)(object)localPlayer.GetCurrentCraftingStation()) && !localPlayer.NoCostCheat() && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost))
		{
			((Selectable)m_tabCraft).interactable = false;
			((Selectable)m_tabUpgrade).interactable = true;
			((Component)m_tabUpgrade).gameObject.SetActive(false);
		}
		else
		{
			((Component)m_tabUpgrade).gameObject.SetActive(true);
		}
		m_tempRecipes.Clear();
		localPlayer.GetAvailableRecipes(ref m_tempRecipes);
		UpdateRecipeList(m_tempRecipes);
		if (m_availableRecipes.Count > 0)
		{
			if ((Object)(object)m_selectedRecipe.Recipe != (Object)null)
			{
				int selectedRecipeIndex = GetSelectedRecipeIndex(acceptOneLevelHigher: true);
				SetRecipe(selectedRecipeIndex, focusView);
			}
			else
			{
				SetRecipe(0, focusView);
			}
		}
		else
		{
			SetRecipe(-1, focusView);
		}
	}

	private void UpdateRecipeList(List<Recipe> recipes)
	{
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		Player localPlayer = Player.m_localPlayer;
		foreach (RecipeDataPair availableRecipe in m_availableRecipes)
		{
			Object.Destroy((Object)(object)availableRecipe.InterfaceElement);
		}
		m_availableRecipes.Clear();
		bool globalKey = ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost);
		if (InCraftTab())
		{
			foreach (Recipe recipe2 in recipes)
			{
				AddRecipeToList(localPlayer, recipe2, null, localPlayer.HaveRequirements(recipe2, discover: false, 1) || globalKey);
			}
		}
		else
		{
			for (int i = 0; i < recipes.Count; i++)
			{
				Recipe recipe = recipes[i];
				if (recipe.m_item.m_itemData.m_shared.m_maxQuality <= 1)
				{
					continue;
				}
				m_tempItemList.Clear();
				localPlayer.GetInventory().GetAllItems(recipe.m_item.m_itemData.m_shared.m_name, m_tempItemList);
				foreach (ItemDrop.ItemData tempItem in m_tempItemList)
				{
					bool canCraft = tempItem.m_quality < tempItem.m_shared.m_maxQuality && (localPlayer.HaveRequirements(recipe, discover: false, tempItem.m_quality + 1) || globalKey);
					AddRecipeToList(localPlayer, recipe, tempItem, canCraft);
				}
			}
		}
		float num = (float)m_availableRecipes.Count * m_recipeListSpace;
		num = Mathf.Max(m_recipeListBaseSize, num);
		m_recipeListRoot.SetSizeWithCurrentAnchors((Axis)1, num);
		if (!Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			return;
		}
		SortMethod sortMethod = SortMethod.Original;
		if (Player.m_localPlayer.TryGetUniqueKeyValue("sortcraft", out var value) && Enum.TryParse<SortMethod>(value, ignoreCase: true, out var result))
		{
			sortMethod = result;
		}
		switch (sortMethod)
		{
		case SortMethod.Original:
			m_availableRecipes.Sort(delegate(RecipeDataPair a, RecipeDataPair b)
			{
				int num3 = byCraftable(a, b);
				if (num3 == 0)
				{
					num3 = bySortWeight(a, b);
				}
				if (num3 == 0)
				{
					num3 = a.Recipe.m_item.m_itemData.m_shared.m_name.CompareTo(b.Recipe.m_item.m_itemData.m_shared.m_name);
				}
				if (num3 == 0)
				{
					num3 = byLevel(a, b);
				}
				return num3;
			});
			break;
		case SortMethod.Name:
			m_availableRecipes.Sort(delegate(RecipeDataPair a, RecipeDataPair b)
			{
				int num4 = byCraftable(a, b);
				if (num4 == 0)
				{
					num4 = bySortWeight(a, b);
				}
				if (num4 == 0)
				{
					num4 = byName(a, b);
				}
				if (num4 == 0)
				{
					num4 = byLevel(a, b);
				}
				return num4;
			});
			break;
		case SortMethod.Type:
			m_availableRecipes.Sort(delegate(RecipeDataPair a, RecipeDataPair b)
			{
				int num2 = byCraftable(a, b);
				if (num2 == 0)
				{
					num2 = bySortWeight(a, b);
				}
				if (num2 == 0)
				{
					num2 = a.Recipe.m_item.m_itemData.m_shared.m_itemType.CompareTo(b.Recipe.m_item.m_itemData.m_shared.m_itemType);
				}
				if (num2 == 0)
				{
					num2 = byName(a, b);
				}
				if (num2 == 0)
				{
					num2 = byLevel(a, b);
				}
				return num2;
			});
			break;
		case SortMethod.Weight:
			m_availableRecipes.Sort(delegate(RecipeDataPair a, RecipeDataPair b)
			{
				int num5 = byCraftable(a, b);
				if (num5 == 0)
				{
					num5 = bySortWeight(a, b);
				}
				if (num5 == 0)
				{
					num5 = a.Recipe.m_item.m_itemData.m_shared.m_weight.CompareTo(b.Recipe.m_item.m_itemData.m_shared.m_weight);
				}
				if (num5 == 0)
				{
					num5 = byName(a, b);
				}
				if (num5 == 0)
				{
					num5 = byLevel(a, b);
				}
				return num5;
			});
			break;
		}
		for (int j = 0; j < m_availableRecipes.Count; j++)
		{
			Transform transform = m_availableRecipes[j].InterfaceElement.transform;
			((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(0f, (float)j * (0f - m_recipeListSpace));
		}
		static int byCraftable(RecipeDataPair a, RecipeDataPair b)
		{
			return b.CanCraft.CompareTo(a.CanCraft);
		}
		static int byLevel(RecipeDataPair a, RecipeDataPair b)
		{
			if (b.ItemData != null && a.ItemData != null)
			{
				return b.ItemData.m_quality.CompareTo(a.ItemData.m_quality);
			}
			return 0;
		}
		static int byName(RecipeDataPair a, RecipeDataPair b)
		{
			return Localization.instance.Localize(a.Recipe.m_item.m_itemData.m_shared.m_name).CompareTo(Localization.instance.Localize(b.Recipe.m_item.m_itemData.m_shared.m_name));
		}
		static int bySortWeight(RecipeDataPair a, RecipeDataPair b)
		{
			return a.Recipe.m_listSortWeight.CompareTo(b.Recipe.m_listSortWeight);
		}
	}

	private void AddRecipeToList(Player player, Recipe recipe, ItemDrop.ItemData item, bool canCraft)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Expected O, but got Unknown
		int count = m_availableRecipes.Count;
		GameObject element = Object.Instantiate<GameObject>(m_recipeElementPrefab, (Transform)(object)m_recipeListRoot);
		element.SetActive(true);
		Transform transform = element.transform;
		((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(0f, (float)count * (0f - m_recipeListSpace));
		Image component = ((Component)element.transform.Find("icon")).GetComponent<Image>();
		component.sprite = recipe.m_item.m_itemData.GetIcon();
		((Graphic)component).color = (Color)(canCraft ? Color.white : new Color(1f, 0f, 1f, 0f));
		TMP_Text component2 = ((Component)element.transform.Find("name")).GetComponent<TMP_Text>();
		string text = Localization.instance.Localize(recipe.m_item.m_itemData.m_shared.m_name);
		if (recipe.m_amount > 1)
		{
			text += $" x{recipe.m_amount}";
		}
		component2.text = text;
		((Graphic)component2).color = (Color)(canCraft ? Color.white : new Color(0.66f, 0.66f, 0.66f, 1f));
		GuiBar component3 = ((Component)element.transform.Find("Durability")).GetComponent<GuiBar>();
		if (item != null && item.m_shared.m_useDurability && item.m_durability < item.GetMaxDurability())
		{
			((Component)component3).gameObject.SetActive(true);
			component3.SetValue(item.GetDurabilityPercentage());
		}
		else
		{
			((Component)component3).gameObject.SetActive(false);
		}
		TMP_Text component4 = ((Component)element.transform.Find("QualityLevel")).GetComponent<TMP_Text>();
		if (item != null)
		{
			((Component)component4).gameObject.SetActive(true);
			component4.text = item.m_quality.ToString();
		}
		else
		{
			((Component)component4).gameObject.SetActive(false);
		}
		((UnityEvent)element.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
		{
			OnSelectedRecipe(element);
		});
		m_availableRecipes.Add(new RecipeDataPair(recipe, item, element, canCraft));
	}

	private void OnSelectedRecipe(GameObject button)
	{
		int index = FindSelectedRecipe(button);
		SetRecipe(index, center: false);
	}

	private void UpdateRecipeGamepadInput()
	{
		if (m_availableRecipes.Count > 0)
		{
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				SetRecipe(Mathf.Min(m_availableRecipes.Count - 1, GetSelectedRecipeIndex() + 1), center: true);
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				SetRecipe(Mathf.Max(0, GetSelectedRecipeIndex() - 1), center: true);
			}
		}
	}

	private int GetSelectedRecipeIndex(bool acceptOneLevelHigher = false)
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_availableRecipes.Count; i++)
		{
			if ((Object)(object)m_availableRecipes[i].Recipe == (Object)(object)m_selectedRecipe.Recipe && m_availableRecipes[i].ItemData == m_selectedRecipe.ItemData)
			{
				return i;
			}
		}
		if (acceptOneLevelHigher && m_selectedRecipe.ItemData != null)
		{
			for (int j = 0; j < m_availableRecipes.Count; j++)
			{
				if ((Object)(object)m_availableRecipes[j].Recipe == (Object)(object)m_selectedRecipe.Recipe && isOneLevelHigher(m_availableRecipes[j].ItemData, m_selectedRecipe.ItemData) && m_availableRecipes[j].ItemData.m_gridPos == m_selectedRecipe.ItemData.m_gridPos)
				{
					return j;
				}
			}
			for (int k = 0; k < m_availableRecipes.Count; k++)
			{
				if ((Object)(object)m_availableRecipes[k].Recipe == (Object)(object)m_selectedRecipe.Recipe && isOneLevelHigher(m_availableRecipes[k].ItemData, m_selectedRecipe.ItemData))
				{
					return k;
				}
			}
		}
		return 0;
		bool isOneLevelHigher(ItemDrop.ItemData available, ItemDrop.ItemData selected)
		{
			if (available != null && available.m_quality == m_selectedRecipe.ItemData.m_quality + 1 && (Object)(object)available.m_dropPrefab == (Object)(object)m_selectedRecipe.ItemData.m_dropPrefab && available.m_variant == m_selectedRecipe.ItemData.m_variant)
			{
				return available.m_stack == m_selectedRecipe.ItemData.m_stack;
			}
			return false;
		}
	}

	private void SetRecipe(int index, bool center)
	{
		ZLog.Log((object)("Setting selected recipe " + index));
		for (int i = 0; i < m_availableRecipes.Count; i++)
		{
			bool active = i == index;
			((Component)m_availableRecipes[i].InterfaceElement.transform.Find("selected")).gameObject.SetActive(active);
		}
		if (center && index >= 0)
		{
			ScrollRectEnsureVisible recipeEnsureVisible = m_recipeEnsureVisible;
			Transform transform = m_availableRecipes[index].InterfaceElement.transform;
			recipeEnsureVisible.CenterOnItem((RectTransform)(object)((transform is RectTransform) ? transform : null));
		}
		if (index < 0)
		{
			m_selectedRecipe = default(RecipeDataPair);
			m_selectedVariant = 0;
			return;
		}
		RecipeDataPair selectedRecipe = m_availableRecipes[index];
		if ((Object)(object)selectedRecipe.Recipe != (Object)(object)m_selectedRecipe.Recipe || selectedRecipe.ItemData != m_selectedRecipe.ItemData)
		{
			m_selectedRecipe = selectedRecipe;
			m_selectedVariant = 0;
		}
	}

	private void UpdateRecipe(Player player, float dt)
	{
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		CraftingStation currentCraftingStation = player.GetCurrentCraftingStation();
		if (Object.op_Implicit((Object)(object)currentCraftingStation))
		{
			m_craftingStationName.text = Localization.instance.Localize(currentCraftingStation.m_name);
			((Component)m_craftingStationIcon).gameObject.SetActive(true);
			m_craftingStationIcon.sprite = currentCraftingStation.m_icon;
			int level = currentCraftingStation.GetLevel();
			m_craftingStationLevel.text = level.ToString();
			((Component)m_craftingStationLevelRoot).gameObject.SetActive(true);
		}
		else
		{
			m_craftingStationName.text = Localization.instance.Localize("$hud_crafting");
			((Component)m_craftingStationIcon).gameObject.SetActive(false);
			((Component)m_craftingStationLevelRoot).gameObject.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)m_selectedRecipe.Recipe))
		{
			((Behaviour)m_recipeIcon).enabled = true;
			((Behaviour)m_recipeName).enabled = true;
			((Behaviour)m_recipeDecription).enabled = true;
			ItemDrop.ItemData itemData = m_selectedRecipe.ItemData;
			int num = ((itemData == null) ? 1 : (itemData.m_quality + 1));
			bool flag = num <= m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_maxQuality;
			bool flag2 = itemData == null && (ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyLStick"));
			int num2 = itemData?.m_variant ?? m_selectedVariant;
			m_recipeIcon.sprite = m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_icons[num2];
			string text = Localization.instance.Localize(m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_name);
			int num3 = ((!flag2) ? 1 : m_multiCraftAmount);
			int num4 = m_selectedRecipe.Recipe.m_amount * num3;
			if (m_selectedRecipe.Recipe.m_amount > 1)
			{
				text = $"{text} x{num4}";
			}
			m_recipeName.text = text;
			m_recipeDecription.text = Localization.instance.Localize(ItemDrop.ItemData.GetTooltip(m_selectedRecipe.Recipe.m_item.m_itemData, num, crafting: true, Game.m_worldLevel, num4));
			if (m_selectedRecipe.Recipe.m_requireOnlyOneIngredient)
			{
				TMP_Text recipeDecription = m_recipeDecription;
				recipeDecription.text += Localization.instance.Localize("\n\n<color=orange>$inventory_onlyoneingredient</color>");
			}
			if (itemData != null)
			{
				((Component)m_itemCraftType).gameObject.SetActive(true);
				if (itemData.m_quality >= itemData.m_shared.m_maxQuality)
				{
					m_itemCraftType.text = Localization.instance.Localize("$inventory_maxquality");
				}
				else
				{
					string text2 = Localization.instance.Localize(itemData.m_shared.m_name);
					m_itemCraftType.text = Localization.instance.Localize("$inventory_upgrade", new string[2]
					{
						text2,
						(itemData.m_quality + 1).ToString()
					});
				}
			}
			else
			{
				((Component)m_itemCraftType).gameObject.SetActive(false);
			}
			((Component)m_variantButton).gameObject.SetActive(m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_variants > 1 && m_selectedRecipe.ItemData == null);
			SetupRequirementList(num, player, flag, num3);
			int requiredStationLevel = m_selectedRecipe.Recipe.GetRequiredStationLevel(num);
			CraftingStation requiredStation = m_selectedRecipe.Recipe.GetRequiredStation(num);
			if ((Object)(object)requiredStation != (Object)null && flag)
			{
				((Component)m_minStationLevelIcon).gameObject.SetActive(true);
				m_minStationLevelText.text = requiredStationLevel.ToString();
				if ((Object)(object)currentCraftingStation == (Object)null || currentCraftingStation.GetLevel() < requiredStationLevel)
				{
					((Graphic)m_minStationLevelText).color = ((Mathf.Sin(Time.time * 10f) > 0f && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost)) ? Color.red : m_minStationLevelBasecolor);
				}
				else
				{
					((Graphic)m_minStationLevelText).color = m_minStationLevelBasecolor;
				}
			}
			else
			{
				((Component)m_minStationLevelIcon).gameObject.SetActive(false);
			}
			bool flag3 = player.HaveRequirements(m_selectedRecipe.Recipe, discover: false, num, (!flag2) ? 1 : m_multiCraftAmount);
			bool flag4 = true;
			bool flag5 = !Object.op_Implicit((Object)(object)requiredStation) || (Object.op_Implicit((Object)(object)currentCraftingStation) && currentCraftingStation.CheckUsable(player, showMessage: false));
			((Selectable)m_craftButton).interactable = ((flag3 && flag5) || player.NoCostCheat() || (ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost) && flag5)) && flag4 && flag;
			TMP_Text componentInChildren = ((Component)m_craftButton).GetComponentInChildren<TMP_Text>();
			if (num > 1)
			{
				componentInChildren.text = Localization.instance.Localize("$inventory_upgradebutton");
			}
			else
			{
				componentInChildren.text = Localization.instance.Localize("$inventory_craftbutton");
				if (flag2)
				{
					componentInChildren.text = componentInChildren.text + " x " + m_multiCraftAmount;
				}
			}
			UITooltip component = ((Component)m_craftButton).GetComponent<UITooltip>();
			if (!flag4)
			{
				component.m_text = Localization.instance.Localize("$inventory_full");
			}
			else if (!flag3 && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost))
			{
				component.m_text = Localization.instance.Localize("$msg_missingrequirement");
			}
			else if (!flag5)
			{
				component.m_text = Localization.instance.Localize("$msg_missingstation");
			}
			else
			{
				component.m_text = "";
			}
		}
		else
		{
			((Behaviour)m_recipeIcon).enabled = false;
			((Behaviour)m_recipeName).enabled = false;
			((Behaviour)m_recipeDecription).enabled = false;
			((Component)m_qualityPanel).gameObject.SetActive(false);
			((Component)m_minStationLevelIcon).gameObject.SetActive(false);
			((Component)m_craftButton).GetComponent<UITooltip>().m_text = "";
			((Component)m_variantButton).gameObject.SetActive(false);
			((Component)m_itemCraftType).gameObject.SetActive(false);
			for (int i = 0; i < m_recipeRequirementList.Length; i++)
			{
				HideRequirement(m_recipeRequirementList[i].transform);
			}
			((Selectable)m_craftButton).interactable = false;
		}
		if (m_craftTimer < 0f)
		{
			((Component)m_craftProgressPanel).gameObject.SetActive(false);
			((Component)m_craftButton).gameObject.SetActive(true);
			return;
		}
		float num5 = (m_multiCrafting ? m_multiCraftDuration : m_craftDuration);
		if ((Object)(object)currentCraftingStation != (Object)null && currentCraftingStation.m_craftingSkill != 0)
		{
			num5 *= 1f - Player.m_localPlayer.GetSkillFactor(currentCraftingStation.m_craftingSkill) * m_craftDurationSkillMaxDecrease;
		}
		((Component)m_craftButton).gameObject.SetActive(false);
		((Component)m_craftProgressPanel).gameObject.SetActive(true);
		m_craftProgressBar.SetMaxValue(num5);
		m_craftProgressBar.SetValue(m_craftTimer);
		m_craftTimer += dt;
		if (m_craftTimer >= num5)
		{
			DoCrafting(player);
			m_craftTimer = -1f;
		}
	}

	private void SetupRequirementList(int quality, Player player, bool allowedQuality, int amount)
	{
		int i = 0;
		int num = m_recipeRequirementList.Length;
		Piece.Requirement[] resources = m_selectedRecipe.Recipe.m_resources;
		m_reqList.Clear();
		if (m_selectedRecipe.Recipe.m_requireOnlyOneIngredient)
		{
			m_reqList.Clear();
			Piece.Requirement[] array = resources;
			foreach (Piece.Requirement requirement in array)
			{
				if (player.IsKnownMaterial(requirement.m_resItem.m_itemData.m_shared.m_name) && requirement.GetAmount(quality) > 0)
				{
					m_reqList.Add(requirement);
				}
			}
		}
		else
		{
			Piece.Requirement[] array = resources;
			foreach (Piece.Requirement requirement2 in array)
			{
				if (requirement2.GetAmount(quality) > 0)
				{
					m_reqList.Add(requirement2);
				}
			}
		}
		int num2 = 0;
		if (m_reqList.Count > 4)
		{
			int num3 = (int)Mathf.Ceil((float)m_reqList.Count / (float)num);
			num2 = (int)Time.fixedTime % num3 * num;
		}
		if (allowedQuality)
		{
			for (int k = num2; k < m_reqList.Count; k++)
			{
				if (SetupRequirement(m_recipeRequirementList[i].transform, m_reqList[k], player, craft: true, quality, amount))
				{
					i++;
				}
				if (i >= m_recipeRequirementList.Length)
				{
					break;
				}
			}
		}
		for (; i < num; i++)
		{
			HideRequirement(m_recipeRequirementList[i].transform);
		}
	}

	private void SetupUpgradeItem(Recipe recipe, ItemDrop.ItemData item)
	{
		if (item == null)
		{
			m_upgradeItemIcon.sprite = recipe.m_item.m_itemData.m_shared.m_icons[m_selectedVariant];
			m_upgradeItemName.text = Localization.instance.Localize(recipe.m_item.m_itemData.m_shared.m_name);
			m_upgradeItemNextQuality.text = ((recipe.m_item.m_itemData.m_shared.m_maxQuality > 1) ? "1" : "");
			m_itemCraftType.text = Localization.instance.Localize("$inventory_new");
			((Component)m_upgradeItemDurability).gameObject.SetActive(recipe.m_item.m_itemData.m_shared.m_useDurability);
			if (recipe.m_item.m_itemData.m_shared.m_useDurability)
			{
				m_upgradeItemDurability.SetValue(1f);
			}
			return;
		}
		m_upgradeItemIcon.sprite = item.GetIcon();
		m_upgradeItemName.text = Localization.instance.Localize(item.m_shared.m_name);
		m_upgradeItemNextQuality.text = item.m_quality.ToString();
		((Component)m_upgradeItemDurability).gameObject.SetActive(item.m_shared.m_useDurability);
		if (item.m_shared.m_useDurability)
		{
			m_upgradeItemDurability.SetValue(item.GetDurabilityPercentage());
		}
		if (item.m_quality >= item.m_shared.m_maxQuality)
		{
			m_itemCraftType.text = Localization.instance.Localize("$inventory_maxquality");
		}
		else
		{
			m_itemCraftType.text = Localization.instance.Localize("$inventory_upgrade");
		}
	}

	public static bool SetupRequirement(Transform elementRoot, Piece.Requirement req, Player player, bool craft, int quality, int craftMultiplier = 1)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		Image component = ((Component)((Component)elementRoot).transform.Find("res_icon")).GetComponent<Image>();
		TMP_Text component2 = ((Component)((Component)elementRoot).transform.Find("res_name")).GetComponent<TMP_Text>();
		TMP_Text component3 = ((Component)((Component)elementRoot).transform.Find("res_amount")).GetComponent<TMP_Text>();
		UITooltip component4 = ((Component)elementRoot).GetComponent<UITooltip>();
		if ((Object)(object)req.m_resItem != (Object)null)
		{
			((Component)component).gameObject.SetActive(true);
			((Component)component2).gameObject.SetActive(true);
			((Component)component3).gameObject.SetActive(true);
			component.sprite = req.m_resItem.m_itemData.GetIcon();
			((Graphic)component).color = Color.white;
			component4.m_text = Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name);
			component2.text = Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name);
			int num = player.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name);
			int num2 = req.GetAmount(quality) * craftMultiplier;
			if (num2 <= 0)
			{
				HideRequirement(elementRoot);
				return false;
			}
			component3.text = num2.ToString();
			if (num < num2 && ((!craft && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoBuildCost)) || (craft && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost))))
			{
				((Graphic)component3).color = ((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : Color.white);
			}
			else
			{
				((Graphic)component3).color = Color.white;
			}
		}
		return true;
	}

	public static void HideRequirement(Transform elementRoot)
	{
		Image component = ((Component)((Component)elementRoot).transform.Find("res_icon")).GetComponent<Image>();
		TMP_Text component2 = ((Component)((Component)elementRoot).transform.Find("res_name")).GetComponent<TMP_Text>();
		TMP_Text component3 = ((Component)((Component)elementRoot).transform.Find("res_amount")).GetComponent<TMP_Text>();
		((Component)elementRoot).GetComponent<UITooltip>().m_text = "";
		((Component)component).gameObject.SetActive(false);
		((Component)component2).gameObject.SetActive(false);
		((Component)component3).gameObject.SetActive(false);
	}

	private void DoCrafting(Player player)
	{
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_craftRecipe == (Object)null)
		{
			return;
		}
		int num = ((m_craftUpgradeItem == null) ? 1 : (m_craftUpgradeItem.m_quality + 1));
		if (num > m_craftRecipe.m_item.m_itemData.m_shared.m_maxQuality)
		{
			return;
		}
		int num2 = ((!m_multiCrafting) ? 1 : m_multiCraftAmount);
		int need;
		ItemDrop.ItemData singleReqItem;
		int num3 = m_craftRecipe.GetAmount(num, out need, out singleReqItem, num2);
		if ((!player.HaveRequirements(m_craftRecipe, discover: false, num, num2) && !player.NoCostCheat() && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost)) || (m_craftUpgradeItem != null && !player.GetInventory().ContainsItem(m_craftUpgradeItem)) || (m_craftRecipe.m_requireOnlyOneIngredient && singleReqItem == null))
		{
			return;
		}
		CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
		int num4 = 0;
		if ((Object)(object)currentCraftingStation != (Object)null && currentCraftingStation.m_craftingSkill != 0 && m_craftRecipe.m_item.m_itemData.m_shared.m_maxStackSize > 1)
		{
			float skillFactor = Player.m_localPlayer.GetSkillFactor(currentCraftingStation.m_craftingSkill);
			for (int i = 0; i < num2; i++)
			{
				if (Random.value < skillFactor * m_craftBonusChance)
				{
					num4 += m_craftBonusAmount;
					num3 += num4;
				}
			}
		}
		if (m_craftUpgradeItem == null && !player.GetInventory().CanAddItem(((Component)m_craftRecipe.m_item).gameObject, num3))
		{
			return;
		}
		if (m_craftRecipe.m_item.m_itemData.m_shared.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(m_craftRecipe.m_item.m_itemData.m_shared.m_dlc))
		{
			player.Message(MessageHud.MessageType.Center, "$msg_dlcrequired");
			return;
		}
		int variant = m_craftVariant;
		Vector2i gridPos = default(Vector2i);
		((Vector2i)(ref gridPos))._002Ector(-1, -1);
		if (m_craftUpgradeItem != null)
		{
			gridPos = m_craftUpgradeItem.m_gridPos;
			variant = m_craftUpgradeItem.m_variant;
			player.UnequipItem(m_craftUpgradeItem);
			player.GetInventory().RemoveItem(m_craftUpgradeItem);
		}
		long playerID = player.GetPlayerID();
		string playerName = player.GetPlayerName();
		if (player.GetInventory().AddItem(((Object)((Component)m_craftRecipe.m_item).gameObject).name, num3, num, variant, playerID, playerName, gridPos) != null)
		{
			if (!player.NoCostCheat() && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost))
			{
				int multiplier = ((!m_multiCrafting) ? 1 : m_multiCraftAmount);
				if (singleReqItem != null)
				{
					player.GetInventory().RemoveItem(singleReqItem.m_shared.m_name, need, singleReqItem.m_quality);
				}
				else
				{
					player.ConsumeResources(m_craftRecipe.m_resources, num, -1, multiplier);
				}
			}
			UpdateCraftingPanel();
			if ((Object)(object)m_craftRecipe.m_craftingStation != (Object)null && m_craftRecipe.m_craftingStation.m_craftingSkill != 0)
			{
				Player.m_localPlayer.RaiseSkill(m_craftRecipe.m_craftingStation.m_craftingSkill, (!m_multiCrafting) ? 1 : m_multiCraftAmount);
			}
			if (num4 > 0)
			{
				Vector3 val = (Object.op_Implicit((Object)(object)currentCraftingStation) ? ((Component)currentCraftingStation).transform.position : ((Component)Player.m_localPlayer).transform.position) + Vector3.up;
				DamageText.instance.ShowText(DamageText.TextType.Bonus, val, $"+{num4}", player: true);
				m_craftBonusEffect.Create(val, Quaternion.identity);
				ZLog.Log((object)$"Bonus craft x{num4}!");
			}
		}
		if (Object.op_Implicit((Object)(object)currentCraftingStation))
		{
			currentCraftingStation.m_craftItemDoneEffects.Create(((Component)player).transform.position, Quaternion.identity);
		}
		else
		{
			m_craftItemDoneEffects.Create(((Component)player).transform.position, Quaternion.identity);
		}
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		playerProfile.IncrementStat(PlayerStatType.CraftsOrUpgrades);
		if (m_craftUpgradeItem == null)
		{
			playerProfile.IncrementStat(PlayerStatType.Crafts);
			Utils.IncrementOrSet<string>(playerProfile.m_itemCraftStats, m_craftRecipe.m_item.m_itemData.m_shared.m_name, 1f);
		}
		else
		{
			playerProfile.IncrementStat(PlayerStatType.Upgrades);
		}
		Gogan.LogEvent("Game", "Crafted", m_craftRecipe.m_item.m_itemData.m_shared.m_name, num);
	}

	private int FindSelectedRecipe(GameObject button)
	{
		for (int i = 0; i < m_availableRecipes.Count; i++)
		{
			if ((Object)(object)m_availableRecipes[i].InterfaceElement == (Object)(object)button)
			{
				return i;
			}
		}
		return -1;
	}

	private void OnCraftCancelPressed()
	{
		if (m_craftTimer >= 0f)
		{
			m_craftTimer = -1f;
		}
	}

	private void OnCraftPressed()
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_selectedRecipe.Recipe))
		{
			return;
		}
		m_craftRecipe = m_selectedRecipe.Recipe;
		m_craftUpgradeItem = m_selectedRecipe.ItemData;
		m_craftVariant = m_selectedVariant;
		m_multiCrafting = m_craftUpgradeItem == null && (ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyLStick"));
		int quality = ((m_craftUpgradeItem == null) ? 1 : (m_craftUpgradeItem.m_quality + 1));
		int need;
		ItemDrop.ItemData singleReqItem;
		int amount = m_craftRecipe.GetAmount(quality, out need, out singleReqItem, (!m_multiCrafting) ? 1 : m_multiCraftAmount);
		if (m_craftUpgradeItem == null && !Player.m_localPlayer.GetInventory().CanAddItem(((Component)m_craftRecipe.m_item).gameObject, amount))
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$inventory_full");
			return;
		}
		m_craftTimer = 0f;
		if (Object.op_Implicit((Object)(object)m_craftRecipe.m_craftingStation))
		{
			CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
			if (Object.op_Implicit((Object)(object)currentCraftingStation))
			{
				currentCraftingStation.m_craftItemEffects.Create(((Component)Player.m_localPlayer).transform.position, Quaternion.identity);
			}
		}
		else
		{
			m_craftItemEffects.Create(((Component)Player.m_localPlayer).transform.position, Quaternion.identity);
		}
	}

	private void OnRepairPressed()
	{
		RepairOneItem();
		UpdateRepair();
		UpdateCraftingPanel();
	}

	private void UpdateRepair()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.m_localPlayer.GetCurrentCraftingStation() == (Object)null && !Player.m_localPlayer.NoCostCheat())
		{
			((Component)m_repairPanel).gameObject.SetActive(false);
			((Component)m_repairPanelSelection).gameObject.SetActive(false);
			((Component)m_repairButton).gameObject.SetActive(false);
			return;
		}
		((Component)m_repairButton).gameObject.SetActive(true);
		((Component)m_repairPanel).gameObject.SetActive(true);
		((Component)m_repairPanelSelection).gameObject.SetActive(true);
		if (HaveRepairableItems())
		{
			((Selectable)m_repairButton).interactable = true;
			((Component)m_repairButtonGlow).gameObject.SetActive(true);
			Color color = ((Graphic)m_repairButtonGlow).color;
			color.a = 0.5f + Mathf.Sin(Time.time * 5f) * 0.5f;
			((Graphic)m_repairButtonGlow).color = color;
		}
		else
		{
			((Selectable)m_repairButton).interactable = false;
			((Component)m_repairButtonGlow).gameObject.SetActive(false);
		}
	}

	private void RepairOneItem()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			return;
		}
		CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
		if (((Object)(object)currentCraftingStation == (Object)null && !Player.m_localPlayer.NoCostCheat()) || (Object.op_Implicit((Object)(object)currentCraftingStation) && !currentCraftingStation.CheckUsable(Player.m_localPlayer, showMessage: false)))
		{
			return;
		}
		m_tempWornItems.Clear();
		Player.m_localPlayer.GetInventory().GetWornItems(m_tempWornItems);
		foreach (ItemDrop.ItemData tempWornItem in m_tempWornItems)
		{
			if (CanRepair(tempWornItem))
			{
				Player.m_localPlayer.RaiseSkill(Skills.SkillType.Crafting, 1f - tempWornItem.m_durability / tempWornItem.GetMaxDurability());
				tempWornItem.m_durability = tempWornItem.GetMaxDurability();
				if (Object.op_Implicit((Object)(object)currentCraftingStation))
				{
					currentCraftingStation.m_repairItemDoneEffects.Create(((Component)currentCraftingStation).transform.position, Quaternion.identity);
				}
				Player.m_localPlayer.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_repaired", new string[1] { tempWornItem.m_shared.m_name }));
				return;
			}
		}
		Player.m_localPlayer.Message(MessageHud.MessageType.Center, "No more item to repair");
	}

	private bool HaveRepairableItems()
	{
		if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			return false;
		}
		CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
		if ((Object)(object)currentCraftingStation == (Object)null && !Player.m_localPlayer.NoCostCheat())
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)currentCraftingStation) && !currentCraftingStation.CheckUsable(Player.m_localPlayer, showMessage: false))
		{
			return false;
		}
		m_tempWornItems.Clear();
		Player.m_localPlayer.GetInventory().GetWornItems(m_tempWornItems);
		foreach (ItemDrop.ItemData tempWornItem in m_tempWornItems)
		{
			if (CanRepair(tempWornItem))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanRepair(ItemDrop.ItemData item)
	{
		if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			return false;
		}
		if (!item.m_shared.m_canBeReparied)
		{
			return false;
		}
		if (Player.m_localPlayer.NoCostCheat())
		{
			return true;
		}
		CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
		if ((Object)(object)currentCraftingStation == (Object)null)
		{
			return false;
		}
		Recipe recipe = ObjectDB.instance.GetRecipe(item);
		if ((Object)(object)recipe == (Object)null)
		{
			return false;
		}
		if ((Object)(object)recipe.m_craftingStation == (Object)null && (Object)(object)recipe.m_repairStation == (Object)null)
		{
			return false;
		}
		if (((Object)(object)recipe.m_repairStation != (Object)null && recipe.m_repairStation.m_name == currentCraftingStation.m_name) || ((Object)(object)recipe.m_craftingStation != (Object)null && recipe.m_craftingStation.m_name == currentCraftingStation.m_name) || item.m_worldLevel < Game.m_worldLevel)
		{
			if (Mathf.Min(currentCraftingStation.GetLevel(), 4) < recipe.m_minStationLevel)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private void SetupDragItem(ItemDrop.ItemData item, Inventory inventory, int amount)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_dragGo))
		{
			Object.Destroy((Object)(object)m_dragGo);
			m_dragGo = null;
			m_dragItem = null;
			m_dragInventory = null;
			m_dragAmount = 0;
		}
		if (item != null)
		{
			m_dragGo = Object.Instantiate<GameObject>(m_dragItemPrefab, ((Component)this).transform);
			m_dragItem = item;
			m_dragInventory = inventory;
			m_dragAmount = amount;
			m_moveItemEffects.Create(((Component)this).transform.position, Quaternion.identity);
			UITooltip.HideTooltip();
		}
	}

	private void ShowSplitDialog(ItemDrop.ItemData item, Inventory fromIventory)
	{
		bool num = ZInput.GetKey((KeyCode)306, true) || ZInput.GetKey((KeyCode)305, true);
		m_splitSlider.minValue = 1f;
		m_splitSlider.maxValue = item.m_stack;
		if (!num)
		{
			m_splitSlider.value = Mathf.CeilToInt((float)item.m_stack / 2f);
		}
		else if (m_splitSlider.value / (float)item.m_stack > 0.5f)
		{
			m_splitSlider.value = Mathf.Min(m_splitSlider.value, (float)item.m_stack);
		}
		m_splitIcon.sprite = item.GetIcon();
		m_splitIconName.text = Localization.instance.Localize(item.m_shared.m_name);
		((Component)m_splitPanel).gameObject.SetActive(true);
		m_splitItem = item;
		m_splitInventory = fromIventory;
		OnSplitSliderChanged(m_splitSlider.value);
	}

	private void OnSplitSliderChanged(float value)
	{
		m_splitAmount.text = (int)value + "/" + (int)m_splitSlider.maxValue;
	}

	private void UpdateSplitDialog()
	{
		if (!((Component)m_splitSlider).gameObject.activeInHierarchy)
		{
			return;
		}
		for (int i = 0; i < 10; i++)
		{
			if (ZInput.GetKeyDown((KeyCode)(256 + i), true) || ZInput.GetKeyDown((KeyCode)(48 + i), true))
			{
				if (m_lastSplitInput + TimeSpan.FromSeconds(m_splitNumInputTimeoutSec) < DateTime.Now)
				{
					m_splitInput = "";
				}
				m_lastSplitInput = DateTime.Now;
				m_splitInput += i;
				if (int.TryParse(m_splitInput, out var result))
				{
					m_splitSlider.value = Mathf.Clamp((float)result, 1f, m_splitSlider.maxValue);
					OnSplitSliderChanged(m_splitSlider.value);
				}
			}
		}
		if (ZInput.GetKeyDown((KeyCode)276, true) && m_splitSlider.value > 1f)
		{
			Slider splitSlider = m_splitSlider;
			splitSlider.value -= 1f;
			OnSplitSliderChanged(m_splitSlider.value);
		}
		if (ZInput.GetKeyDown((KeyCode)275, true) && m_splitSlider.value < m_splitSlider.maxValue)
		{
			Slider splitSlider2 = m_splitSlider;
			splitSlider2.value += 1f;
			OnSplitSliderChanged(m_splitSlider.value);
		}
		if (ZInput.GetKeyDown((KeyCode)271, true) || ZInput.GetKeyDown((KeyCode)13, true))
		{
			OnSplitOk();
		}
	}

	private void OnSplitCancel()
	{
		m_splitItem = null;
		m_splitInventory = null;
		((Component)m_splitPanel).gameObject.SetActive(false);
	}

	private void OnSplitOk()
	{
		SetupDragItem(m_splitItem, m_splitInventory, (int)m_splitSlider.value);
		m_splitItem = null;
		m_splitInventory = null;
		((Component)m_splitPanel).gameObject.SetActive(false);
	}

	public void OnOpenSkills()
	{
		if (Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			m_skillsDialog.Setup(Player.m_localPlayer);
			Gogan.LogEvent("Screen", "Enter", "Skills", 0L);
		}
	}

	public void OnOpenTexts()
	{
		if (Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			m_textsDialog.Setup(Player.m_localPlayer);
			Gogan.LogEvent("Screen", "Enter", "Texts", 0L);
		}
	}

	public void OnOpenTrophies()
	{
		m_trophiesPanel.SetActive(true);
		UpdateTrophyList();
		Gogan.LogEvent("Screen", "Enter", "Trophies", 0L);
	}

	public void OnCloseTrophies()
	{
		m_trophiesPanel.SetActive(false);
	}

	private void UpdateTrophyList()
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.m_localPlayer == (Object)null)
		{
			return;
		}
		foreach (GameObject trophy in m_trophyList)
		{
			Object.Destroy((Object)(object)trophy);
		}
		m_trophyList.Clear();
		List<string> trophies = Player.m_localPlayer.GetTrophies();
		float num = 0f;
		for (int i = 0; i < trophies.Count; i++)
		{
			string text = trophies[i];
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(text);
			if ((Object)(object)itemPrefab == (Object)null)
			{
				ZLog.LogWarning((object)("Missing trophy prefab:" + text));
				continue;
			}
			ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
			GameObject val = Object.Instantiate<GameObject>(m_trophieElementPrefab, (Transform)(object)m_trophieListRoot);
			val.SetActive(true);
			Transform transform = val.transform;
			RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			val2.anchoredPosition = new Vector2((float)((Vector2Int)(ref component.m_itemData.m_shared.m_trophyPos)).x * m_trophieListSpace, (float)((Vector2Int)(ref component.m_itemData.m_shared.m_trophyPos)).y * (0f - m_trophieListSpace));
			num = Mathf.Min(num, val2.anchoredPosition.y - m_trophieListSpace);
			string text2 = Localization.instance.Localize(component.m_itemData.m_shared.m_name);
			if (Utils.CustomEndsWith(text2, " trophy"))
			{
				text2 = text2.Remove(text2.Length - 7);
			}
			((Component)((Transform)val2).Find("icon_bkg/icon")).GetComponent<Image>().sprite = component.m_itemData.GetIcon();
			((Component)((Transform)val2).Find("name")).GetComponent<TMP_Text>().text = text2;
			((Component)((Transform)val2).Find("description")).GetComponent<TMP_Text>().text = Localization.instance.Localize(component.m_itemData.m_shared.m_name + "_lore");
			m_trophyList.Add(val);
		}
		ZLog.Log((object)("SIZE " + num));
		float num2 = Mathf.Max(m_trophieListBaseSize, 0f - num);
		m_trophieListRoot.SetSizeWithCurrentAnchors((Axis)1, num2);
		m_trophyListScroll.value = 1f;
	}

	public void OnShowVariantSelection()
	{
		m_variantDialog.Setup(m_selectedRecipe.Recipe.m_item.m_itemData);
		Gogan.LogEvent("Screen", "Enter", "VariantSelection", 0L);
	}

	private void OnVariantSelected(int index)
	{
		ZLog.Log((object)("Item variant selected " + index));
		m_selectedVariant = index;
	}

	public bool InUpradeTab()
	{
		return !((Selectable)m_tabUpgrade).interactable;
	}

	public bool InCraftTab()
	{
		return !((Selectable)m_tabCraft).interactable;
	}

	public void OnTabCraftPressed()
	{
		((Selectable)m_tabCraft).interactable = false;
		((Selectable)m_tabUpgrade).interactable = true;
		UpdateCraftingPanel();
	}

	public void OnTabUpgradePressed()
	{
		((Selectable)m_tabCraft).interactable = true;
		((Selectable)m_tabUpgrade).interactable = false;
		UpdateCraftingPanel();
	}
}
