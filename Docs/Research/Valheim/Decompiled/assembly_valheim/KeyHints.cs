using TMPro;
using UnityEngine;
using Valheim.SettingsGui;

public class KeyHints : MonoBehaviour
{
	private static KeyHints m_instance;

	[Header("Key hints")]
	public GameObject m_buildHints;

	public GameObject m_combatHints;

	public GameObject m_inventoryHints;

	public GameObject m_inventoryWithContainerHints;

	public GameObject m_fishingHints;

	public GameObject m_barberHints;

	public GameObject m_radialHints;

	public GameObject[] m_equipButtons;

	public GameObject m_primaryAttackGP;

	public GameObject m_primaryAttackKB;

	public GameObject m_secondaryAttackGP;

	public GameObject m_secondaryAttackKB;

	public GameObject m_closeMenuHintKB;

	public GameObject m_closeMenuHintGP;

	public GameObject m_bowDrawGP;

	public GameObject m_bowDrawKB;

	private bool m_keyHintsEnabled = true;

	public TextMeshProUGUI m_buildMenuKey;

	public TextMeshProUGUI m_buildRotateKey;

	public TextMeshProUGUI m_buildAlternativePlacingKey;

	public TextMeshProUGUI m_dodgeKey;

	public TextMeshProUGUI m_cycleSnapKey;

	public KeyHintsRadial m_radialKeyHints;

	public static KeyHints instance => m_instance;

	private void OnDestroy()
	{
		m_instance = null;
	}

	private void Awake()
	{
		m_instance = this;
		ApplySettings();
	}

	public void SetGamePadBindings()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Invalid comparison between Unknown and I4
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Invalid comparison between Unknown and I4
		if ((Object)(object)m_cycleSnapKey != (Object)null)
		{
			((TMP_Text)m_cycleSnapKey).text = "$hud_cyclesnap  <mspace=0.6em>$KEY_PrevSnap / $KEY_NextSnap</mspace>";
			Localization.instance.Localize(((TMP_Text)m_cycleSnapKey).transform);
		}
		if ((Object)(object)m_buildMenuKey != (Object)null)
		{
			Localization.instance.RemoveTextFromCache((TMP_Text)(object)m_buildMenuKey);
			InputLayout inputLayout = ZInput.InputLayout;
			if ((int)inputLayout != 0)
			{
				if (inputLayout - 1 <= 1)
				{
					((TMP_Text)m_buildMenuKey).text = "$hud_buildmenu  <mspace=0.6em>$KEY_BuildMenu</mspace>";
				}
			}
			else
			{
				((TMP_Text)m_buildMenuKey).text = "$hud_buildmenu  <mspace=0.6em>$KEY_Use</mspace>";
			}
			Localization.instance.Localize(((TMP_Text)m_buildMenuKey).transform);
		}
		if ((Object)(object)m_buildRotateKey != (Object)null)
		{
			Localization.instance.RemoveTextFromCache((TMP_Text)(object)m_buildRotateKey);
			InputLayout inputLayout = ZInput.InputLayout;
			if ((int)inputLayout != 0)
			{
				if (inputLayout - 1 <= 1)
				{
					((TMP_Text)m_buildRotateKey).text = "$hud_rotate  <mspace=0.6em>$KEY_LTrigger / $KEY_RTrigger</mspace>";
				}
			}
			else
			{
				((TMP_Text)m_buildRotateKey).text = "$hud_rotate  <mspace=0.6em>$KEY_Block + $KEY_RStick</mspace>";
			}
			Localization.instance.Localize(((TMP_Text)m_buildRotateKey).transform);
		}
		if ((Object)(object)m_dodgeKey != (Object)null)
		{
			Localization.instance.RemoveTextFromCache((TMP_Text)(object)m_dodgeKey);
			InputLayout inputLayout = ZInput.InputLayout;
			if ((int)inputLayout != 0)
			{
				if (inputLayout - 1 <= 1)
				{
					((TMP_Text)m_dodgeKey).text = "$settings_dodge  <mspace=0.6em>$KEY_Block + $KEY_Dodge</mspace>";
				}
			}
			else
			{
				((TMP_Text)m_dodgeKey).text = "$settings_dodge  <mspace=0.6em>$KEY_Block + $KEY_Jump</mspace>";
			}
			Localization.instance.Localize(((TMP_Text)m_dodgeKey).transform);
		}
		m_radialKeyHints.UpdateGamepadHints();
	}

	private void Start()
	{
	}

	public void ApplySettings()
	{
		m_keyHintsEnabled = PlatformPrefs.GetInt("KeyHints", 1) == 1;
		SetGamePadBindings();
	}

	private void Update()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		UpdateHints();
		if (ZInput.GetKeyDown((KeyCode)290, true))
		{
			ZInput.instance.ChangeLayout(GamepadMapController.NextLayout(ZInput.InputLayout));
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("Changed controller layout to: " + GamepadMapController.GetLayoutStringId(ZInput.InputLayout)));
			ApplySettings();
		}
	}

	private void UpdateHints()
	{
		Player localPlayer = Player.m_localPlayer;
		if (!m_keyHintsEnabled || (Object)(object)localPlayer == (Object)null || localPlayer.IsDead() || Chat.instance.IsChatDialogWindowVisible() || Game.IsPaused() || ((Object)(object)InventoryGui.instance != (Object)null && (InventoryGui.instance.IsSkillsPanelOpen || InventoryGui.instance.IsTrophisPanelOpen || InventoryGui.instance.IsTextPanelOpen)))
		{
			m_buildHints.SetActive(false);
			m_combatHints.SetActive(false);
			m_inventoryHints.SetActive(false);
			m_inventoryWithContainerHints.SetActive(false);
			m_fishingHints.SetActive(false);
			m_barberHints.SetActive(false);
			m_radialHints.SetActive(false);
			return;
		}
		_ = m_buildHints.activeSelf;
		_ = m_buildHints.activeSelf;
		ItemDrop.ItemData currentWeapon = localPlayer.GetCurrentWeapon();
		if (InventoryGui.IsVisible())
		{
			bool flag = InventoryGui.instance.IsContainerOpen();
			bool flag2 = InventoryGui.instance.ActiveGroup == 0;
			ItemDrop.ItemData itemData = (flag2 ? InventoryGui.instance.ContainerGrid.GetGamepadSelectedItem() : InventoryGui.instance.m_playerGrid.GetGamepadSelectedItem());
			bool flag3 = itemData?.IsEquipable() ?? false;
			bool flag4 = itemData != null && itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable;
			m_buildHints.SetActive(false);
			m_combatHints.SetActive(false);
			m_inventoryHints.SetActive(!flag);
			m_inventoryWithContainerHints.SetActive(flag);
			for (int i = 0; i < m_equipButtons.Length; i++)
			{
				m_equipButtons[i].SetActive(flag4 || (flag3 && !flag2));
			}
			m_fishingHints.SetActive(false);
			m_barberHints.SetActive(false);
			m_radialHints.SetActive(false);
		}
		else if (Hud.instance.m_radialMenu.Active)
		{
			m_buildHints.SetActive(false);
			m_combatHints.SetActive(false);
			m_inventoryHints.SetActive(false);
			m_inventoryWithContainerHints.SetActive(false);
			m_fishingHints.SetActive(false);
			m_barberHints.SetActive(false);
			m_radialHints.SetActive(true);
		}
		else if (localPlayer.InPlaceMode())
		{
			if (ZInput.IsNonClassicFunctionality())
			{
				string text = Localization.instance.Localize("$hud_altplacement  <mspace=0.6em>$KEY_AltKeys + $KEY_AltPlace</mspace>");
				string text2 = (localPlayer.AlternativePlacementActive ? Localization.instance.Localize("$hud_off") : Localization.instance.Localize("$hud_on"));
				((TMP_Text)m_buildAlternativePlacingKey).text = text + " " + text2;
			}
			if (Hud.IsPieceSelectionVisible())
			{
				if (ZInput.IsGamepadActive())
				{
					m_closeMenuHintGP.SetActive(true);
				}
				else
				{
					m_closeMenuHintKB.SetActive(true);
				}
			}
			else
			{
				m_closeMenuHintGP.SetActive(false);
				m_closeMenuHintKB.SetActive(false);
			}
			m_buildHints.SetActive(true);
			m_combatHints.SetActive(false);
			m_inventoryHints.SetActive(false);
			m_inventoryWithContainerHints.SetActive(false);
			m_fishingHints.SetActive(false);
			m_barberHints.SetActive(false);
			m_radialHints.SetActive(false);
		}
		else if (PlayerCustomizaton.IsBarberGuiVisible())
		{
			m_buildHints.SetActive(false);
			m_combatHints.SetActive(false);
			m_inventoryHints.SetActive(false);
			m_inventoryWithContainerHints.SetActive(false);
			m_fishingHints.SetActive(false);
			m_barberHints.SetActive(true);
			m_radialHints.SetActive(false);
		}
		else if (localPlayer.GetDoodadController() != null)
		{
			m_buildHints.SetActive(false);
			m_combatHints.SetActive(false);
			m_inventoryHints.SetActive(false);
			m_inventoryWithContainerHints.SetActive(false);
			m_fishingHints.SetActive(false);
			m_barberHints.SetActive(false);
			m_radialHints.SetActive(false);
		}
		else if (currentWeapon != null && currentWeapon.m_shared.m_animationState == ItemDrop.ItemData.AnimationState.FishingRod)
		{
			m_buildHints.SetActive(false);
			m_combatHints.SetActive(false);
			m_inventoryHints.SetActive(false);
			m_inventoryWithContainerHints.SetActive(false);
			m_fishingHints.SetActive(true);
			m_radialHints.SetActive(false);
		}
		else if (currentWeapon != null && (currentWeapon != localPlayer.m_unarmedWeapon.m_itemData || localPlayer.IsTargeted()))
		{
			m_buildHints.SetActive(false);
			m_combatHints.SetActive(true);
			m_inventoryHints.SetActive(false);
			m_inventoryWithContainerHints.SetActive(false);
			m_fishingHints.SetActive(false);
			m_barberHints.SetActive(false);
			m_radialHints.SetActive(false);
			bool flag5 = currentWeapon.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow && currentWeapon.m_shared.m_skillType != Skills.SkillType.Crossbows;
			bool active = !flag5 && currentWeapon.HavePrimaryAttack();
			bool active2 = !flag5 && currentWeapon.HaveSecondaryAttack();
			m_bowDrawGP.SetActive(flag5);
			m_bowDrawKB.SetActive(flag5);
			m_primaryAttackGP.SetActive(active);
			m_primaryAttackKB.SetActive(active);
			m_secondaryAttackGP.SetActive(active2);
			m_secondaryAttackKB.SetActive(active2);
		}
		else
		{
			m_buildHints.SetActive(false);
			m_combatHints.SetActive(false);
			m_inventoryHints.SetActive(false);
			m_inventoryWithContainerHints.SetActive(false);
			m_fishingHints.SetActive(false);
			m_barberHints.SetActive(false);
			m_radialHints.SetActive(false);
		}
		if (((Behaviour)m_radialKeyHints).isActiveAndEnabled)
		{
			m_radialKeyHints.UpdateRadialHints(Hud.instance.m_radialMenu);
		}
	}
}
