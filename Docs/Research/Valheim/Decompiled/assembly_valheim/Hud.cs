using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valheim.UI;

public class Hud : MonoBehaviour
{
	private class PieceIconData
	{
		public GameObject m_go;

		public Image m_icon;

		public GameObject m_marker;

		public GameObject m_upgrade;

		public UITooltip m_tooltip;
	}

	private float m_lastMaxAdrenaline;

	private static Hud m_instance;

	public GameObject m_rootObject;

	public TMP_Text m_buildSelection;

	public TMP_Text m_pieceDescription;

	public Image m_buildIcon;

	[SerializeField]
	private Image m_snappingIcon;

	[SerializeField]
	private Sprite m_buildSnappingIcon;

	[SerializeField]
	private Sprite m_shipSnappingIcon;

	[SerializeField]
	private Sprite m_hoeSnappingIcon;

	public GameObject m_buildHud;

	public GameObject m_saveIcon;

	public Image m_saveIconImage;

	public GameObject m_badConnectionIcon;

	public GameObject m_betaText;

	[Header("Piece")]
	public GameObject[] m_requirementItems = (GameObject[])(object)new GameObject[0];

	public GameObject[] m_pieceCategoryTabs = (GameObject[])(object)new GameObject[0];

	public GameObject m_pieceSelectionWindow;

	public GameObject m_pieceCategoryRoot;

	public RectTransform m_pieceListRoot;

	public RectTransform m_pieceListMask;

	public GameObject m_pieceIconPrefab;

	public UIInputHandler m_closePieceSelectionButton;

	public EffectList m_selectItemEffect = new EffectList();

	public float m_pieceIconSpacing = 64f;

	private float m_pieceBarTargetPosX;

	private Piece.PieceCategory m_lastPieceCategory = Piece.PieceCategory.Max;

	[Header("Health")]
	public RectTransform m_healthBarRoot;

	public RectTransform m_healthPanel;

	private const float m_healthPanelBuffer = 56f;

	private const float m_healthPanelMinSize = 138f;

	public Animator m_healthAnimator;

	public GuiBar m_healthBarFast;

	public GuiBar m_healthBarSlow;

	public TMP_Text m_healthText;

	[Header("Food")]
	public Image[] m_foodBars;

	public Image[] m_foodIcons;

	public TMP_Text[] m_foodTime;

	public RectTransform m_foodBarRoot;

	public RectTransform m_foodBaseBar;

	public Image m_foodIcon;

	public Color m_foodColorHungry = Color.white;

	public Color m_foodColorFull = Color.white;

	public TMP_Text m_foodText;

	[Header("Action bar")]
	public GameObject m_actionBarRoot;

	public GuiBar m_actionProgress;

	public TMP_Text m_actionName;

	[Header("Stagger bar")]
	public Animator m_staggerAnimator;

	public GuiBar m_staggerProgress;

	[Header("Guardian power")]
	public RectTransform m_gpRoot;

	public TMP_Text m_gpName;

	public TMP_Text m_gpCooldown;

	public Image m_gpIcon;

	[Header("Stamina")]
	public RectTransform m_staminaBar2Root;

	public Animator m_staminaAnimator;

	public GuiBar m_staminaBar2Fast;

	public GuiBar m_staminaBar2Slow;

	public TMP_Text m_staminaText;

	private float m_staminaBarBorderBuffer = 16f;

	[Header("Adrenaline")]
	public RectTransform m_adrenalineBarRoot;

	public Animator m_adrenalineAnimator;

	public GuiBar m_adrenalineBarFast;

	public GuiBar m_adrenalineBarSlow;

	public TMP_Text m_adrenalineText;

	[Header("Eitr")]
	public RectTransform m_eitrBarRoot;

	public Animator m_eitrAnimator;

	public GuiBar m_eitrBarFast;

	public GuiBar m_eitrBarSlow;

	public TMP_Text m_eitrText;

	[Header("Mount")]
	public GameObject m_mountPanel;

	public Image m_mountIcon;

	public GuiBar m_mountHealthBarFast;

	public GuiBar m_mountHealthBarSlow;

	public TextMeshProUGUI m_mountHealthText;

	public GuiBar m_mountStaminaBar;

	public TextMeshProUGUI m_mountStaminaText;

	public TextMeshProUGUI m_mountNameText;

	[Header("Loading")]
	public CanvasGroup m_loadingScreen;

	public GameObject m_loadingProgress;

	public LoadingIndicator m_loadingIndicator;

	public GameObject m_sleepingProgress;

	public GameObject m_teleportingProgress;

	public Image m_loadingImage;

	public TMP_Text m_loadingTip;

	public List<string> m_loadingTips = new List<string>();

	private int m_currentLoadingTipIndex;

	private bool m_progressIndicatorShown;

	[Header("Crosshair")]
	public Image m_crosshair;

	public Image m_crosshairBow;

	public TextMeshProUGUI m_hoverName;

	public RectTransform m_pieceHealthRoot;

	public GuiBar m_pieceHealthBar;

	public Image m_damageScreen;

	public Image m_lavaWarningScreen;

	[Header("Radial Menus")]
	public RadialBase m_radialMenu;

	public OpenRadialConfig m_config;

	[Header("Target")]
	public GameObject m_targetedAlert;

	public GameObject m_targeted;

	public GameObject m_hidden;

	public GuiBar m_stealthBar;

	[Header("Status effect")]
	public RectTransform m_statusEffectListRoot;

	public RectTransform m_statusEffectTemplate;

	public float m_statusEffectSpacing = 55f;

	public int m_effectsPerRow = 7;

	private List<RectTransform> m_statusEffects = new List<RectTransform>();

	[Header("Ship hud")]
	public GameObject m_shipHudRoot;

	public GameObject m_shipControlsRoot;

	public GameObject m_rudderLeft;

	public GameObject m_rudderRight;

	public GameObject m_rudderSlow;

	public GameObject m_rudderForward;

	public GameObject m_rudderFastForward;

	public GameObject m_rudderBackward;

	public GameObject m_halfSail;

	public GameObject m_fullSail;

	public GameObject m_rudder;

	public RectTransform m_shipWindIndicatorRoot;

	public Image m_shipWindIcon;

	public RectTransform m_shipWindIconRoot;

	public Image m_shipRudderIndicator;

	public Image m_shipRudderIcon;

	[Header("Event")]
	public GameObject m_eventBar;

	public TMP_Text m_eventName;

	[NonSerialized]
	public bool m_userHidden;

	private float m_hudPressed;

	private CraftingStation m_currentCraftingStation;

	private List<StatusEffect> m_tempStatusEffects = new List<StatusEffect>();

	private List<PieceIconData> m_pieceIcons = new List<PieceIconData>();

	private int m_pieceIconUpdateIndex;

	private bool m_haveSetupLoadScreen;

	private float m_staggerHideTimer = 99999f;

	private float m_staminaHideTimer = 99999f;

	private float m_adrenalineHideTimer = 99999f;

	private float m_eitrHideTimer = 99999f;

	private int m_closePieceSelection;

	private Piece m_hoveredPiece;

	private const float minimumSaveIconDisplayTime = 3f;

	private float m_saveIconTimer;

	private bool m_worldSaving;

	private bool m_fullyOpaqueSaveIcon;

	private bool m_profileSaving;

	private static Vector3 s_notVisiblePosition = new Vector3(10000f, 0f, 0f);

	private static Color s_colorRedBlueZeroAlpha = new Color(1f, 0f, 1f, 0f);

	private static Color s_colorRedish = new Color(1f, 0.5f, 0.5f, 1f);

	private static Color s_shipWindIconColor = new Color(0.2f, 0.2f, 0.2f, 1f);

	private static Color s_whiteHalfAlpha = new Color(1f, 1f, 1f, 0.5f);

	public static Hud instance => m_instance;

	private void OnDestroy()
	{
		m_instance = null;
		PlayerProfile.SavingStarted = (Action)Delegate.Remove(PlayerProfile.SavingStarted, new Action(ProfileSaveStarted));
		PlayerProfile.SavingFinished = (Action)Delegate.Remove(PlayerProfile.SavingFinished, new Action(ProfileSaveFinished));
		ZNet.WorldSaveStarted = (Action)Delegate.Remove(ZNet.WorldSaveStarted, new Action(WorldSaveStarted));
		ZNet.WorldSaveFinished = (Action)Delegate.Remove(ZNet.WorldSaveFinished, new Action(WorldSaveFinished));
	}

	private void Awake()
	{
		m_instance = this;
		m_pieceSelectionWindow.SetActive(false);
		((Component)m_loadingScreen).gameObject.SetActive(false);
		((Component)m_statusEffectTemplate).gameObject.SetActive(false);
		m_eventBar.SetActive(false);
		((Component)m_gpRoot).gameObject.SetActive(false);
		m_betaText.SetActive(false);
		UIInputHandler closePieceSelectionButton = m_closePieceSelectionButton;
		closePieceSelectionButton.m_onLeftClick = (Action<UIInputHandler>)Delegate.Combine(closePieceSelectionButton.m_onLeftClick, new Action<UIInputHandler>(OnClosePieceSelection));
		UIInputHandler closePieceSelectionButton2 = m_closePieceSelectionButton;
		closePieceSelectionButton2.m_onRightClick = (Action<UIInputHandler>)Delegate.Combine(closePieceSelectionButton2.m_onRightClick, new Action<UIInputHandler>(OnClosePieceSelection));
		if (SteamManager.APP_ID == 1223920)
		{
			m_betaText.SetActive(true);
		}
		GameObject[] pieceCategoryTabs = m_pieceCategoryTabs;
		for (int i = 0; i < pieceCategoryTabs.Length; i++)
		{
			UIInputHandler component = pieceCategoryTabs[i].GetComponent<UIInputHandler>();
			component.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onLeftDown, new Action<UIInputHandler>(OnLeftClickCategory));
		}
		PlayerProfile.SavingStarted = (Action)Delegate.Combine(PlayerProfile.SavingStarted, new Action(ProfileSaveStarted));
		PlayerProfile.SavingFinished = (Action)Delegate.Combine(PlayerProfile.SavingFinished, new Action(ProfileSaveFinished));
		ZNet.WorldSaveStarted = (Action)Delegate.Combine(ZNet.WorldSaveStarted, new Action(WorldSaveStarted));
		ZNet.WorldSaveFinished = (Action)Delegate.Combine(ZNet.WorldSaveFinished, new Action(WorldSaveFinished));
	}

	private void ProfileSaveStarted()
	{
		m_profileSaving = true;
		m_fullyOpaqueSaveIcon = true;
		m_saveIconTimer = 3f;
	}

	private void ProfileSaveFinished()
	{
		m_profileSaving = false;
	}

	private void WorldSaveStarted()
	{
		m_worldSaving = true;
		m_fullyOpaqueSaveIcon = true;
		m_saveIconTimer = 3f;
	}

	private void WorldSaveFinished()
	{
		m_worldSaving = false;
	}

	private void SetVisible(bool visible)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		if (visible == IsVisible())
		{
			return;
		}
		if (visible)
		{
			m_rootObject.transform.localPosition = Vector3.zero;
		}
		else
		{
			m_rootObject.transform.localPosition = s_notVisiblePosition;
			if (ZInput.IsGamepadActive() && !Player.m_localPlayer.InCutscene())
			{
				string text = "$hud_hidden_messagehud_notification_gamepad " + ZInput.instance.GetBoundKeyString("JoyAltKeys", false) + " + " + ZInput.instance.GetBoundKeyString("JoyToggleHUD", false);
				MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, text, 0, null, showDespiteHiddenHUD: true);
			}
		}
		if (Object.op_Implicit((Object)(object)Menu.instance) && (visible || (Object.op_Implicit((Object)(object)Player.m_localPlayer) && !Player.m_localPlayer.InCutscene())))
		{
			JoinCode.m_instance.m_root.transform.localPosition = m_rootObject.transform.localPosition;
			((Component)Menu.instance.m_root).transform.localPosition = m_rootObject.transform.localPosition;
		}
	}

	public bool IsVisible()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return m_rootObject.transform.localPosition.x < 1000f;
	}

	private void Update()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Invalid comparison between Unknown and I4
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Invalid comparison between Unknown and I4
		float deltaTime = Time.deltaTime;
		if (m_worldSaving || m_profileSaving || m_saveIconTimer > 0f)
		{
			m_saveIcon.SetActive(true);
			if ((double)Time.unscaledDeltaTime < 0.5)
			{
				m_saveIconTimer -= Time.unscaledDeltaTime;
			}
			Color color = ((Graphic)m_saveIconImage).color;
			float num;
			if (m_fullyOpaqueSaveIcon)
			{
				num = 1f;
				m_fullyOpaqueSaveIcon = false;
			}
			else
			{
				num = 0.3f + Mathf.PingPong(m_saveIconTimer * 2f, 0.7f);
			}
			((Graphic)m_saveIconImage).color = new Color(color.r, color.g, color.b, num);
			m_badConnectionIcon.SetActive(false);
		}
		else
		{
			m_saveIcon.SetActive(false);
			m_badConnectionIcon.SetActive((Object)(object)ZNet.instance != (Object)null && ZNet.instance.HasBadConnection() && Mathf.Sin(Time.time * 10f) > 0f);
		}
		Player localPlayer = Player.m_localPlayer;
		UpdateDamageFlash(deltaTime);
		if (Object.op_Implicit((Object)(object)localPlayer))
		{
			bool flag = ((int)ZInput.InputLayout == 1 && ZInput.GetButtonDown("JoyToggleHUD") && ZInput.GetButton("JoyAltKeys") && !ZInput.GetButton("JoyLTrigger")) || ((int)ZInput.InputLayout == 0 && ZInput.GetButtonDown("JoyToggleHUD") && ZInput.GetButton("JoyAltKeys") && !ZInput.GetButton("JoyLBumper")) || ((int)ZInput.InputLayout == 2 && ZInput.GetButtonDown("JoyToggleHUD") && ZInput.GetButton("JoyAltKeys") && !ZInput.GetButton("JoyLBumper"));
			if ((ZInput.GetKeyDown((KeyCode)284, true) && ZInput.GetKey((KeyCode)306, true)) || flag)
			{
				m_userHidden = !m_userHidden;
				m_hudPressed = 0f;
			}
			if (ZInput.GetButtonDown("JoyToggleHUD") && !ZInput.GetButton("JoyLTrigger"))
			{
				m_hudPressed += 1f;
			}
			if (m_hudPressed > 0f)
			{
				m_hudPressed -= Time.deltaTime;
			}
			if (m_hudPressed > 3f && m_userHidden)
			{
				m_userHidden = false;
				m_hudPressed = 0f;
			}
			SetVisible(!m_userHidden && !localPlayer.InCutscene());
			UpdateBuild(localPlayer, forceUpdateAllBuildStatuses: false);
			m_tempStatusEffects.Clear();
			localPlayer.GetSEMan().GetHUDStatusEffects(m_tempStatusEffects);
			UpdateStatusEffects(m_tempStatusEffects);
			UpdateGuardianPower(localPlayer);
			float attackDrawPercentage = localPlayer.GetAttackDrawPercentage();
			UpdateFood(localPlayer);
			UpdateHealth(localPlayer);
			UpdateStamina(localPlayer, deltaTime);
			UpdateAdrenaline(localPlayer, deltaTime);
			UpdateEitr(localPlayer, deltaTime);
			UpdateStealth(localPlayer, attackDrawPercentage);
			UpdateCrosshair(localPlayer, attackDrawPercentage);
			UpdateEvent(localPlayer);
			UpdateActionProgress(localPlayer);
			UpdateStagger(localPlayer, deltaTime);
			UpdateMount(localPlayer, deltaTime);
		}
	}

	private void LateUpdate()
	{
		UpdateBlackScreen(Player.m_localPlayer, Time.deltaTime);
		Player localPlayer = Player.m_localPlayer;
		if (Object.op_Implicit((Object)(object)localPlayer))
		{
			UpdateShipHud(localPlayer, Time.deltaTime);
		}
	}

	private float GetFadeDuration(Player player)
	{
		if ((Object)(object)player != (Object)null)
		{
			if (player.IsDead())
			{
				return Game.instance.m_fadeTimeDeath;
			}
			if (player.IsSleeping())
			{
				return Game.instance.m_fadeTimeSleep;
			}
		}
		return 1f;
	}

	private void UpdateBlackScreen(Player player, float dt)
	{
		if ((Object)(object)player == (Object)null || player.IsDead() || player.IsTeleporting() || Game.instance.IsShuttingDown() || player.IsSleeping())
		{
			((Component)m_loadingScreen).gameObject.SetActive(true);
			float alpha = m_loadingScreen.alpha;
			float fadeDuration = GetFadeDuration(player);
			alpha = Mathf.MoveTowards(alpha, 1f, dt / fadeDuration);
			if (Game.instance.IsShuttingDown())
			{
				alpha = 1f;
			}
			m_loadingScreen.alpha = alpha;
			if ((Object)(object)player != (Object)null && player.IsSleeping())
			{
				m_sleepingProgress.SetActive(true);
				m_loadingProgress.SetActive(false);
				m_teleportingProgress.SetActive(false);
			}
			else if ((Object)(object)player != (Object)null && player.ShowTeleportAnimation())
			{
				m_loadingProgress.SetActive(false);
				m_sleepingProgress.SetActive(false);
				m_teleportingProgress.SetActive(true);
			}
			else if (Object.op_Implicit((Object)(object)Game.instance) && Game.instance.WaitingForRespawn())
			{
				if (!m_haveSetupLoadScreen)
				{
					m_haveSetupLoadScreen = true;
					ShuffleTips();
					UpdateShownTip(forceUpdateText: true);
				}
				else
				{
					UpdateShownTip(forceUpdateText: false);
				}
				m_loadingProgress.SetActive(true);
				m_sleepingProgress.SetActive(false);
				m_teleportingProgress.SetActive(false);
				UpdateProgressIndicator();
			}
			else
			{
				m_loadingProgress.SetActive(false);
				m_sleepingProgress.SetActive(false);
				m_teleportingProgress.SetActive(false);
			}
		}
		else
		{
			m_haveSetupLoadScreen = false;
			float fadeDuration2 = GetFadeDuration(player);
			float alpha2 = m_loadingScreen.alpha;
			alpha2 = Mathf.MoveTowards(alpha2, 0f, dt / fadeDuration2);
			m_loadingScreen.alpha = alpha2;
			if (m_loadingScreen.alpha <= 0f)
			{
				((Component)m_loadingScreen).gameObject.SetActive(false);
			}
		}
	}

	private void ShuffleTips()
	{
		Utils.Shuffle<string>((IList<string>)m_loadingTips);
		m_currentLoadingTipIndex = 0;
	}

	private void UpdateShownTip(bool forceUpdateText)
	{
		int num = m_currentLoadingTipIndex;
		if (ZInput.GetButtonDown("JoyButtonA") || ZInput.GetKeyDown((KeyCode)32, true) || ZInput.GetMouseButtonDown(0) || ZInput.GetButtonDown("JoyDPadRight") || ZInput.GetKeyDown((KeyCode)275, true))
		{
			num++;
		}
		if (ZInput.GetButtonDown("JoyDPadLeft") || ZInput.GetKeyDown((KeyCode)276, true))
		{
			num--;
		}
		if (num >= m_loadingTips.Count)
		{
			num = 0;
		}
		else if (num < 0)
		{
			num = m_loadingTips.Count - 1;
		}
		if (num != m_currentLoadingTipIndex)
		{
			m_currentLoadingTipIndex = num;
			forceUpdateText = true;
		}
		if (forceUpdateText)
		{
			string text = m_loadingTips[m_currentLoadingTipIndex];
			ZLog.Log((object)("tip:" + text));
			m_loadingTip.text = Localization.instance.Localize(text);
		}
	}

	private void UpdateProgressIndicator()
	{
		if (!ZNet.instance.IsServer())
		{
			return;
		}
		bool flag = !ZoneSystem.instance.LocationsGenerated;
		if (m_progressIndicatorShown || flag)
		{
			m_loadingIndicator.SetProgress(ZoneSystem.instance.GenerateLocationsProgress);
		}
		if (m_progressIndicatorShown != flag)
		{
			m_loadingIndicator.SetShowProgress(flag);
			if (flag)
			{
				m_loadingIndicator.SetText("$menu_generating");
			}
			m_progressIndicatorShown = flag;
		}
	}

	private void UpdateShipHud(Player player, float dt)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		Ship controlledShip = player.GetControlledShip();
		if ((Object)(object)controlledShip == (Object)null)
		{
			m_shipHudRoot.gameObject.SetActive(false);
		}
		else
		{
			if (!IsVisible())
			{
				return;
			}
			Ship.Speed speedSetting = controlledShip.GetSpeedSetting();
			float rudder = controlledShip.GetRudder();
			float rudderValue = controlledShip.GetRudderValue();
			m_shipHudRoot.SetActive(true);
			m_rudderSlow.SetActive(speedSetting == Ship.Speed.Slow);
			m_rudderForward.SetActive(speedSetting == Ship.Speed.Half);
			m_rudderFastForward.SetActive(speedSetting == Ship.Speed.Full);
			m_rudderBackward.SetActive(speedSetting == Ship.Speed.Back);
			m_rudderLeft.SetActive(false);
			m_rudderRight.SetActive(false);
			m_fullSail.SetActive(speedSetting == Ship.Speed.Full);
			m_halfSail.SetActive(speedSetting == Ship.Speed.Half);
			GameObject rudder2 = m_rudder;
			int active;
			switch (speedSetting)
			{
			case Ship.Speed.Stop:
				active = ((Mathf.Abs(rudderValue) > 0.2f) ? 1 : 0);
				break;
			default:
				active = 0;
				break;
			case Ship.Speed.Back:
			case Ship.Speed.Slow:
				active = 1;
				break;
			}
			rudder2.SetActive((byte)active != 0);
			if ((rudder > 0f && rudderValue < 1f) || (rudder < 0f && rudderValue > -1f))
			{
				((Component)m_shipRudderIcon).transform.Rotate(new Vector3(0f, 0f, 200f * (0f - rudder) * dt));
			}
			if (Mathf.Abs(rudderValue) < 0.02f)
			{
				((Component)m_shipRudderIndicator).gameObject.SetActive(false);
			}
			else
			{
				((Component)m_shipRudderIndicator).gameObject.SetActive(true);
				if (rudderValue > 0f)
				{
					m_shipRudderIndicator.fillClockwise = true;
					m_shipRudderIndicator.fillAmount = rudderValue * 0.25f;
				}
				else
				{
					m_shipRudderIndicator.fillClockwise = false;
					m_shipRudderIndicator.fillAmount = (0f - rudderValue) * 0.25f;
				}
			}
			float shipYawAngle = controlledShip.GetShipYawAngle();
			((Transform)m_shipWindIndicatorRoot).localRotation = Quaternion.Euler(0f, 0f, shipYawAngle);
			float windAngle = controlledShip.GetWindAngle();
			((Transform)m_shipWindIconRoot).localRotation = Quaternion.Euler(0f, 0f, windAngle);
			float windAngleFactor = controlledShip.GetWindAngleFactor();
			((Graphic)m_shipWindIcon).color = Color.Lerp(s_shipWindIconColor, Color.white, windAngleFactor);
			Camera mainCamera = Utils.GetMainCamera();
			if (!((Object)(object)mainCamera == (Object)null))
			{
				m_shipControlsRoot.transform.position = Utils.WorldToScreenPointScaled(mainCamera, controlledShip.m_controlGuiPos.position);
			}
		}
	}

	private void UpdateStagger(Player player, float dt)
	{
		float staggerPercentage = player.GetStaggerPercentage();
		m_staggerProgress.SetValue(staggerPercentage);
		if (staggerPercentage > 0f)
		{
			m_staggerHideTimer = 0f;
		}
		else
		{
			m_staggerHideTimer += dt;
		}
		m_staggerAnimator.SetBool("Visible", m_staggerHideTimer < 1f);
	}

	public void StaggerBarFlash()
	{
		m_staggerAnimator.SetTrigger("Flash");
	}

	private void UpdateActionProgress(Player player)
	{
		player.GetActionProgress(out var name, out var progress, out var data);
		if (!string.IsNullOrEmpty(name) && data.m_duration > 0.5f)
		{
			m_actionBarRoot.SetActive(true);
			m_actionProgress.SetValue(progress);
			m_actionName.text = Localization.instance.Localize(name);
		}
		else
		{
			m_actionBarRoot.SetActive(false);
		}
	}

	private void UpdateCrosshair(Player player, float bowDrawPercentage)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		if (player.IsAttached() && (Object)(object)player.GetAttachCameraPoint() != (Object)null)
		{
			((Component)m_crosshair).gameObject.SetActive(false);
		}
		else if (!((Component)m_crosshair).gameObject.activeSelf)
		{
			((Component)m_crosshair).gameObject.SetActive(true);
		}
		GameObject hoverObject = player.GetHoverObject();
		Hoverable hoverable = (Object.op_Implicit((Object)(object)hoverObject) ? hoverObject.GetComponentInParent<Hoverable>() : null);
		if (hoverable != null && !TextViewer.instance.IsVisible())
		{
			string text = hoverable.GetHoverText();
			if (ZInput.IsGamepadActive())
			{
				text = text.Replace("[<color=yellow><b><sprite=", "<sprite=");
				text = text.Replace("\"></b></color>]", "\">");
			}
			((TMP_Text)m_hoverName).text = text;
			((Graphic)m_crosshair).color = ((((TMP_Text)m_hoverName).text.Length > 0) ? Color.yellow : s_whiteHalfAlpha);
		}
		else
		{
			((Graphic)m_crosshair).color = s_whiteHalfAlpha;
			((TMP_Text)m_hoverName).text = "";
		}
		Piece hoveringPiece = player.GetHoveringPiece();
		if (Object.op_Implicit((Object)(object)hoveringPiece))
		{
			WearNTear component = ((Component)hoveringPiece).GetComponent<WearNTear>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Component)m_pieceHealthRoot).gameObject.SetActive(true);
				m_pieceHealthBar.SetValue(component.GetHealthPercentage());
			}
			else
			{
				((Component)m_pieceHealthRoot).gameObject.SetActive(false);
			}
		}
		else
		{
			((Component)m_pieceHealthRoot).gameObject.SetActive(false);
		}
		if (bowDrawPercentage > 0f)
		{
			float num = Mathf.Lerp(1f, 0.15f, bowDrawPercentage);
			((Component)m_crosshairBow).gameObject.SetActive(true);
			((Component)m_crosshairBow).transform.localScale = new Vector3(num, num, num);
			((Graphic)m_crosshairBow).color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.yellow, bowDrawPercentage);
		}
		else
		{
			((Component)m_crosshairBow).gameObject.SetActive(false);
		}
	}

	private void UpdateStealth(Player player, float bowDrawPercentage)
	{
		float stealthFactor = player.GetStealthFactor();
		if ((player.IsCrouching() || stealthFactor < 1f) && bowDrawPercentage == 0f)
		{
			if (player.IsSensed())
			{
				m_targetedAlert.SetActive(true);
				m_targeted.SetActive(false);
				m_hidden.SetActive(false);
			}
			else if (player.IsTargeted())
			{
				m_targetedAlert.SetActive(false);
				m_targeted.SetActive(true);
				m_hidden.SetActive(false);
			}
			else
			{
				m_targetedAlert.SetActive(false);
				m_targeted.SetActive(false);
				m_hidden.SetActive(true);
			}
			((Component)m_stealthBar).gameObject.SetActive(true);
			m_stealthBar.SetValue(stealthFactor);
		}
		else
		{
			m_targetedAlert.SetActive(false);
			m_hidden.SetActive(false);
			m_targeted.SetActive(false);
			((Component)m_stealthBar).gameObject.SetActive(false);
		}
	}

	private void SetHealthBarSize(float size)
	{
		size = Mathf.Ceil(size);
		Mathf.Max(size + 56f, 138f);
		m_healthBarRoot.SetSizeWithCurrentAnchors((Axis)0, size);
		m_healthBarSlow.SetWidth(size);
		m_healthBarFast.SetWidth(size);
	}

	private void SetStaminaBarSize(float size)
	{
		m_staminaBar2Root.SetSizeWithCurrentAnchors((Axis)0, size + m_staminaBarBorderBuffer);
		m_staminaBar2Slow.SetWidth(size);
		m_staminaBar2Fast.SetWidth(size);
	}

	private void SetAdrenalineBarSize(float size)
	{
		m_adrenalineBarRoot.SetSizeWithCurrentAnchors((Axis)0, size + m_staminaBarBorderBuffer);
		m_adrenalineBarSlow.SetWidth(size);
		m_adrenalineBarFast.SetWidth(size);
	}

	private void SetEitrBarSize(float size)
	{
		m_eitrBarRoot.SetSizeWithCurrentAnchors((Axis)0, size + m_staminaBarBorderBuffer);
		m_eitrBarSlow.SetWidth(size);
		m_eitrBarFast.SetWidth(size);
	}

	private void UpdateFood(Player player)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		List<Player.Food> foods = player.GetFoods();
		float num = player.GetBaseFoodHP() / 25f * 32f;
		m_foodBaseBar.SetSizeWithCurrentAnchors((Axis)0, num);
		for (int i = 0; i < m_foodBars.Length; i++)
		{
			Image val = m_foodBars[i];
			Image val2 = m_foodIcons[i];
			TMP_Text val3 = m_foodTime[i];
			if (i < foods.Count)
			{
				((Component)val).gameObject.SetActive(true);
				Player.Food food = foods[i];
				((Component)val2).gameObject.SetActive(true);
				val2.sprite = food.m_item.GetIcon();
				if (food.CanEatAgain())
				{
					((Graphic)val2).color = new Color(1f, 1f, 1f, 0.7f + Mathf.Sin(Time.time * 5f) * 0.3f);
				}
				else
				{
					((Graphic)val2).color = Color.white;
				}
				((Component)val3).gameObject.SetActive(true);
				if (food.m_time >= 60f)
				{
					val3.text = Mathf.CeilToInt(food.m_time / 60f) + "m";
					((Graphic)val3).color = Color.white;
				}
				else
				{
					val3.text = Mathf.FloorToInt(food.m_time) + "s";
					((Graphic)val3).color = new Color(1f, 1f, 1f, 0.4f + Mathf.Sin(Time.time * 10f) * 0.6f);
				}
			}
			else
			{
				((Component)val).gameObject.SetActive(false);
				((Component)val2).gameObject.SetActive(false);
				((Component)val3).gameObject.SetActive(false);
			}
		}
		float num2 = Mathf.Ceil(player.GetMaxHealth() / 25f * 32f);
		m_foodBarRoot.SetSizeWithCurrentAnchors((Axis)0, num2);
	}

	private void UpdateMount(Player player, float dt)
	{
		Sadle sadle = player.GetDoodadController() as Sadle;
		if ((Object)(object)sadle == (Object)null)
		{
			m_mountPanel.SetActive(false);
			return;
		}
		Character character = sadle.GetCharacter();
		m_mountPanel.SetActive(true);
		m_mountIcon.overrideSprite = sadle.m_mountIcon;
		m_mountHealthBarSlow.SetValue(character.GetHealthPercentage());
		m_mountHealthBarFast.SetValue(character.GetHealthPercentage());
		((TMP_Text)m_mountHealthText).text = StringExtensionMethods.ToFastString(Mathf.CeilToInt(character.GetHealth()));
		float stamina = sadle.GetStamina();
		float maxStamina = sadle.GetMaxStamina();
		m_mountStaminaBar.SetValue(stamina / maxStamina);
		((TMP_Text)m_mountStaminaText).text = StringExtensionMethods.ToFastString(Mathf.CeilToInt(stamina));
		((TMP_Text)m_mountNameText).text = character.GetHoverName() + " (" + Localization.instance.Localize(sadle.GetTameable().GetStatusString()) + " )";
	}

	private void UpdateHealth(Player player)
	{
		float maxHealth = player.GetMaxHealth();
		SetHealthBarSize(maxHealth / 25f * 32f);
		float health = player.GetHealth();
		m_healthBarFast.SetMaxValue(maxHealth);
		m_healthBarFast.SetValue(health);
		m_healthBarSlow.SetMaxValue(maxHealth);
		m_healthBarSlow.SetValue(health);
		string text = StringExtensionMethods.ToFastString(Mathf.CeilToInt(player.GetHealth()));
		m_healthText.text = text.ToString();
	}

	private void UpdateStamina(Player player, float dt)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		float stamina = player.GetStamina();
		float maxStamina = player.GetMaxStamina();
		if (stamina < maxStamina)
		{
			m_staminaHideTimer = 0f;
		}
		else
		{
			m_staminaHideTimer += dt;
		}
		m_staminaAnimator.SetBool("Visible", m_staminaHideTimer < 1f);
		m_staminaText.text = StringExtensionMethods.ToFastString(Mathf.CeilToInt(stamina));
		SetStaminaBarSize(maxStamina / 25f * 32f);
		Transform transform = ((Component)m_staminaBar2Root).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if (m_buildHud.activeSelf || m_shipHudRoot.activeSelf)
		{
			val.anchoredPosition = new Vector2(0f, 320f);
		}
		else
		{
			val.anchoredPosition = new Vector2(0f, 130f);
		}
		m_staminaBar2Slow.SetValue(stamina / maxStamina);
		m_staminaBar2Fast.SetValue(stamina / maxStamina);
	}

	private void UpdateAdrenaline(Player player, float dt)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		float maxAdrenaline = player.GetMaxAdrenaline();
		if (maxAdrenaline > 0f)
		{
			m_lastMaxAdrenaline = maxAdrenaline;
		}
		float adrenaline = player.GetAdrenaline();
		if (adrenaline <= 0f)
		{
			m_adrenalineAnimator.SetBool("Visible", false);
			return;
		}
		if (adrenaline > 0f)
		{
			m_adrenalineHideTimer = 0f;
		}
		else
		{
			m_adrenalineHideTimer += dt;
		}
		m_adrenalineAnimator.SetBool("Visible", m_adrenalineHideTimer < 1f);
		m_adrenalineText.text = StringExtensionMethods.ToFastString(Mathf.FloorToInt(adrenaline));
		if (maxAdrenaline > 0f)
		{
			SetAdrenalineBarSize(maxAdrenaline / 25f * 32f * 2f);
		}
		Transform transform = ((Component)m_adrenalineBarRoot).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if (m_buildHud.activeSelf || m_shipHudRoot.activeSelf)
		{
			val.anchoredPosition = new Vector2(0f, 320f);
		}
		else
		{
			val.anchoredPosition = new Vector2(0f, 130f);
		}
		float value = adrenaline / m_lastMaxAdrenaline;
		m_adrenalineBarSlow.SetValue(value);
		m_adrenalineBarFast.SetValue(value);
	}

	private void UpdateEitr(Player player, float dt)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		float eitr = player.GetEitr();
		float maxEitr = player.GetMaxEitr();
		if (eitr < maxEitr)
		{
			m_eitrHideTimer = 0f;
		}
		else
		{
			m_eitrHideTimer += dt;
		}
		m_eitrAnimator.SetBool("Visible", m_eitrHideTimer < 1f);
		m_eitrText.text = StringExtensionMethods.ToFastString(Mathf.CeilToInt(eitr));
		SetEitrBarSize(maxEitr / 25f * 32f);
		Transform transform = ((Component)m_eitrBarRoot).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if (m_buildHud.activeSelf || m_shipHudRoot.activeSelf)
		{
			val.anchoredPosition = new Vector2(0f, 285f);
		}
		else
		{
			val.anchoredPosition = new Vector2(0f, 130f);
		}
		m_eitrBarSlow.SetValue(eitr / maxEitr);
		m_eitrBarFast.SetValue(eitr / maxEitr);
	}

	public void DamageFlash()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((Graphic)m_damageScreen).color;
		color.a = 1f;
		((Graphic)m_damageScreen).color = color;
		((Component)m_damageScreen).gameObject.SetActive(true);
	}

	private void UpdateDamageFlash(float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((Graphic)m_damageScreen).color;
		color.a = Mathf.MoveTowards(color.a, 0f, dt * 4f);
		((Graphic)m_damageScreen).color = color;
		if (color.a <= 0f)
		{
			((Component)m_damageScreen).gameObject.SetActive(false);
		}
	}

	private void UpdatePieceList(Player player, Vector2Int selectedNr, Piece.PieceCategory category, bool updateAllBuildStatuses)
	{
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		List<Piece> buildPieces = player.GetBuildPieces();
		int num = 15;
		int num2 = 6;
		if (buildPieces.Count <= 1)
		{
			num = 1;
			num2 = 1;
		}
		if (m_pieceIcons.Count != num * num2)
		{
			foreach (PieceIconData pieceIcon in m_pieceIcons)
			{
				Object.Destroy((Object)(object)pieceIcon.m_go);
			}
			m_pieceIcons.Clear();
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					GameObject val = Object.Instantiate<GameObject>(m_pieceIconPrefab, (Transform)(object)m_pieceListRoot);
					Transform transform = val.transform;
					((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2((float)j * m_pieceIconSpacing, (float)(-i) * m_pieceIconSpacing);
					PieceIconData pieceIconData = new PieceIconData();
					pieceIconData.m_go = val;
					pieceIconData.m_tooltip = val.GetComponent<UITooltip>();
					pieceIconData.m_icon = ((Component)val.transform.Find("icon")).GetComponent<Image>();
					pieceIconData.m_marker = ((Component)val.transform.Find("selected")).gameObject;
					pieceIconData.m_upgrade = ((Component)val.transform.Find("upgrade")).gameObject;
					((Graphic)pieceIconData.m_icon).color = s_colorRedBlueZeroAlpha;
					UIInputHandler component = val.GetComponent<UIInputHandler>();
					component.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onLeftDown, new Action<UIInputHandler>(OnLeftClickPiece));
					component.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onRightDown, new Action<UIInputHandler>(OnRightClickPiece));
					component.m_onPointerEnter = (Action<UIInputHandler>)Delegate.Combine(component.m_onPointerEnter, new Action<UIInputHandler>(OnHoverPiece));
					component.m_onPointerExit = (Action<UIInputHandler>)Delegate.Combine(component.m_onPointerExit, new Action<UIInputHandler>(OnHoverPieceExit));
					m_pieceIcons.Add(pieceIconData);
				}
			}
		}
		for (int k = 0; k < num2; k++)
		{
			for (int l = 0; l < num; l++)
			{
				int num3 = k * num + l;
				PieceIconData pieceIconData2 = m_pieceIcons[num3];
				pieceIconData2.m_marker.SetActive(new Vector2Int(l, k) == selectedNr);
				if (num3 < buildPieces.Count)
				{
					Piece piece = buildPieces[num3];
					pieceIconData2.m_icon.sprite = piece.m_icon;
					((Behaviour)pieceIconData2.m_icon).enabled = true;
					pieceIconData2.m_tooltip.m_text = piece.m_name;
					pieceIconData2.m_upgrade.SetActive(piece.m_isUpgrade);
				}
				else
				{
					((Behaviour)pieceIconData2.m_icon).enabled = false;
					pieceIconData2.m_tooltip.m_text = "";
					pieceIconData2.m_upgrade.SetActive(false);
				}
			}
		}
		UpdatePieceBuildStatus(buildPieces, player);
		if (updateAllBuildStatuses)
		{
			UpdatePieceBuildStatusAll(buildPieces, player);
		}
		if (m_lastPieceCategory != category)
		{
			m_lastPieceCategory = category;
			UpdatePieceBuildStatusAll(buildPieces, player);
		}
	}

	private void OnLeftClickCategory(UIInputHandler ih)
	{
		for (int i = 0; i < m_pieceCategoryTabs.Length; i++)
		{
			if ((Object)(object)m_pieceCategoryTabs[i] == (Object)(object)((Component)ih).gameObject)
			{
				Player.m_localPlayer.SetBuildCategory(i);
				break;
			}
		}
	}

	private void OnLeftClickPiece(UIInputHandler ih)
	{
		SelectPiece(ih);
		HidePieceSelection();
	}

	private void OnRightClickPiece(UIInputHandler ih)
	{
		if (IsQuickPieceSelectEnabled())
		{
			SelectPiece(ih);
			HidePieceSelection();
		}
	}

	private void OnHoverPiece(UIInputHandler ih)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int selectedGrid = GetSelectedGrid(ih);
		if (((Vector2Int)(ref selectedGrid)).x != -1)
		{
			m_hoveredPiece = Player.m_localPlayer.GetPiece(selectedGrid);
		}
	}

	private void OnHoverPieceExit(UIInputHandler ih)
	{
		m_hoveredPiece = null;
	}

	public bool IsQuickPieceSelectEnabled()
	{
		return PlatformPrefs.GetInt("QuickPieceSelect", 0) == 1;
	}

	private Vector2Int GetSelectedGrid(UIInputHandler ih)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		int num = 15;
		int num2 = 6;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int index = i * num + j;
				if ((Object)(object)m_pieceIcons[index].m_go == (Object)(object)((Component)ih).gameObject)
				{
					return new Vector2Int(j, i);
				}
			}
		}
		return new Vector2Int(-1, -1);
	}

	private void SelectPiece(UIInputHandler ih)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int selectedGrid = GetSelectedGrid(ih);
		if (((Vector2Int)(ref selectedGrid)).x != -1)
		{
			Player.m_localPlayer.SetSelectedPiece(selectedGrid);
			m_selectItemEffect.Create(((Component)this).transform.position, Quaternion.identity);
		}
	}

	private void UpdatePieceBuildStatus(List<Piece> pieces, Player player)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (m_pieceIcons.Count != 0)
		{
			if (m_pieceIconUpdateIndex >= m_pieceIcons.Count)
			{
				m_pieceIconUpdateIndex = 0;
			}
			PieceIconData pieceIconData = m_pieceIcons[m_pieceIconUpdateIndex];
			if (m_pieceIconUpdateIndex < pieces.Count)
			{
				Piece piece = pieces[m_pieceIconUpdateIndex];
				bool flag = player.HaveRequirements(piece, Player.RequirementMode.CanBuild);
				((Graphic)pieceIconData.m_icon).color = (flag ? Color.white : s_colorRedBlueZeroAlpha);
			}
			m_pieceIconUpdateIndex++;
		}
	}

	private void UpdatePieceBuildStatusAll(List<Piece> pieces, Player player)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_pieceIcons.Count; i++)
		{
			PieceIconData pieceIconData = m_pieceIcons[i];
			if (i < pieces.Count)
			{
				Piece piece = pieces[i];
				bool flag = player.HaveRequirements(piece, Player.RequirementMode.CanBuild);
				((Graphic)pieceIconData.m_icon).color = (flag ? Color.white : s_colorRedBlueZeroAlpha);
			}
			else
			{
				((Graphic)pieceIconData.m_icon).color = Color.white;
			}
		}
		m_pieceIconUpdateIndex = 0;
	}

	public void TogglePieceSelection()
	{
		m_hoveredPiece = null;
		if (m_pieceSelectionWindow.activeSelf)
		{
			PlayerController.SetTakeInputDelay(0.2f);
			m_pieceSelectionWindow.SetActive(false);
		}
		else
		{
			m_pieceSelectionWindow.SetActive(true);
			UpdateBuild(Player.m_localPlayer, forceUpdateAllBuildStatuses: true);
		}
	}

	private void OnClosePieceSelection(UIInputHandler ih)
	{
		HidePieceSelection();
	}

	public static void HidePieceSelection()
	{
		if (!((Object)(object)m_instance == (Object)null))
		{
			m_instance.m_closePieceSelection = 2;
		}
	}

	public static bool IsPieceSelectionVisible()
	{
		if ((Object)(object)m_instance == (Object)null)
		{
			return false;
		}
		if (m_instance.m_buildHud.activeSelf)
		{
			return m_instance.m_pieceSelectionWindow.activeSelf;
		}
		return false;
	}

	private void UpdateBuild(Player player, bool forceUpdateAllBuildStatuses)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (player.InPlaceMode())
		{
			if (m_closePieceSelection > 0)
			{
				m_closePieceSelection--;
				if (m_closePieceSelection <= 0 && m_pieceSelectionWindow.activeSelf)
				{
					m_hoveredPiece = null;
					m_pieceSelectionWindow.SetActive(false);
					Character.SetTakeInputDelay(0.2f);
					PlayerController.SetTakeInputDelay(0.2f);
				}
			}
			player.GetBuildSelection(out var go, out var id, out var _, out var category, out var pieceTable);
			m_buildHud.SetActive(!m_radialMenu.Active);
			if (m_pieceSelectionWindow.activeSelf)
			{
				UpdatePieceList(player, id, category, forceUpdateAllBuildStatuses);
				m_pieceCategoryRoot.SetActive(pieceTable.m_categories.Count > 0);
				if (pieceTable.m_categories.Count > 0)
				{
					for (int i = 0; i < m_pieceCategoryTabs.Length; i++)
					{
						GameObject val = m_pieceCategoryTabs[i];
						Transform val2 = val.transform.Find("Selected");
						bool flag = i < pieceTable.m_categories.Count;
						val.SetActive(flag);
						if (flag)
						{
							string text = $"{pieceTable.m_categoryLabels[i]} [<color=yellow>{player.GetAvailableBuildPiecesInCategory(pieceTable.m_categories[i])}</color>]";
							if (pieceTable.m_categories[i] == category)
							{
								((Component)val2).gameObject.SetActive(true);
								((Component)val2).GetComponentInChildren<TMP_Text>().text = text;
							}
							else
							{
								((Component)val2).gameObject.SetActive(false);
								val.GetComponentInChildren<TMP_Text>().text = text;
							}
						}
					}
				}
				Localization.instance.Localize(m_buildHud.transform);
			}
			if (Object.op_Implicit((Object)(object)m_hoveredPiece) && (ZInput.IsGamepadActive() || !player.IsPieceAvailable(m_hoveredPiece)))
			{
				m_hoveredPiece = null;
			}
			if (Object.op_Implicit((Object)(object)m_hoveredPiece))
			{
				SetupPieceInfo(m_hoveredPiece);
			}
			else
			{
				SetupPieceInfo(go);
			}
		}
		else
		{
			m_hoveredPiece = null;
			m_buildHud.SetActive(false);
			m_pieceSelectionWindow.SetActive(false);
		}
	}

	private void SetupPieceInfo(Piece piece)
	{
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)piece == (Object)null)
		{
			m_buildSelection.text = Localization.instance.Localize("$hud_nothingtobuild");
			m_pieceDescription.text = "";
			((Behaviour)m_buildIcon).enabled = false;
			((Behaviour)m_snappingIcon).enabled = false;
			for (int i = 0; i < m_requirementItems.Length; i++)
			{
				m_requirementItems[i].SetActive(false);
			}
			return;
		}
		Player localPlayer = Player.m_localPlayer;
		m_buildSelection.text = Localization.instance.Localize(piece.m_name);
		m_pieceDescription.text = Localization.instance.Localize(piece.m_description);
		((Behaviour)m_buildIcon).enabled = true;
		m_buildIcon.sprite = piece.m_icon;
		Sprite snappingIconForPiece = GetSnappingIconForPiece(piece);
		m_snappingIcon.sprite = snappingIconForPiece;
		((Behaviour)m_snappingIcon).enabled = (Object)(object)snappingIconForPiece != (Object)null && (piece.m_category == Piece.PieceCategory.BuildingWorkbench || piece.m_groundPiece || piece.m_waterPiece);
		for (int j = 0; j < m_requirementItems.Length; j++)
		{
			if (j < piece.m_resources.Length)
			{
				Piece.Requirement req = piece.m_resources[j];
				m_requirementItems[j].SetActive(true);
				InventoryGui.SetupRequirement(m_requirementItems[j].transform, req, localPlayer, piece.FreeBuildKey() == GlobalKeys.NoCraftCost, 0);
			}
			else
			{
				m_requirementItems[j].SetActive(false);
			}
		}
		if (Object.op_Implicit((Object)(object)piece.m_craftingStation))
		{
			CraftingStation craftingStation = CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, ((Component)localPlayer).transform.position);
			GameObject obj = m_requirementItems[piece.m_resources.Length];
			obj.SetActive(true);
			Image component = ((Component)obj.transform.Find("res_icon")).GetComponent<Image>();
			TMP_Text component2 = ((Component)obj.transform.Find("res_name")).GetComponent<TMP_Text>();
			TMP_Text component3 = ((Component)obj.transform.Find("res_amount")).GetComponent<TMP_Text>();
			UITooltip component4 = obj.GetComponent<UITooltip>();
			component.sprite = piece.m_craftingStation.m_icon;
			component2.text = Localization.instance.Localize(piece.m_craftingStation.m_name);
			component4.m_text = piece.m_craftingStation.m_name;
			if ((Object)(object)craftingStation != (Object)null)
			{
				craftingStation.ShowAreaMarker();
				((Graphic)component).color = Color.white;
				component3.text = "";
				((Graphic)component3).color = Color.white;
			}
			else
			{
				((Graphic)component).color = Color.gray;
				component3.text = Localization.instance.Localize("$menu_none");
				((Graphic)component3).color = ((Mathf.Sin(Time.time * 10f) > 0f && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost)) ? Color.red : Color.white);
			}
		}
	}

	private Sprite GetSnappingIconForPiece(Piece piece)
	{
		if (piece.m_groundPiece)
		{
			if (!Player.m_localPlayer.AlternativePlacementActive)
			{
				return null;
			}
			return m_hoeSnappingIcon;
		}
		if (piece.m_waterPiece)
		{
			if (!Player.m_localPlayer.AlternativePlacementActive)
			{
				return null;
			}
			return m_shipSnappingIcon;
		}
		if (!Player.m_localPlayer.AlternativePlacementActive)
		{
			return null;
		}
		return m_buildSnappingIcon;
	}

	private void UpdateGuardianPower(Player player)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		player.GetGuardianPowerHUD(out var se, out var cooldown);
		if (Object.op_Implicit((Object)(object)se))
		{
			if (!((Component)m_gpRoot).gameObject.activeSelf)
			{
				((Component)m_gpRoot).gameObject.SetActive(true);
			}
			m_gpIcon.sprite = se.m_icon;
			((Graphic)m_gpIcon).color = ((cooldown <= 0f) ? Color.white : s_colorRedBlueZeroAlpha);
			m_gpName.text = Localization.instance.Localize(se.m_name);
			if (cooldown > 0f)
			{
				m_gpCooldown.text = StatusEffect.GetTimeString(cooldown);
			}
			else
			{
				m_gpCooldown.text = Localization.instance.Localize("$hud_ready");
			}
		}
		else if (((Component)m_gpRoot).gameObject.activeSelf)
		{
			((Component)m_gpRoot).gameObject.SetActive(false);
		}
	}

	private void UpdateStatusEffects(List<StatusEffect> statusEffects)
	{
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		if (m_statusEffects.Count != statusEffects.Count)
		{
			foreach (RectTransform statusEffect2 in m_statusEffects)
			{
				Object.Destroy((Object)(object)((Component)statusEffect2).gameObject);
			}
			m_statusEffects.Clear();
			for (int i = 0; i < statusEffects.Count; i++)
			{
				num = Mathf.FloorToInt((float)(i / m_effectsPerRow));
				num2 = i - num * m_effectsPerRow;
				RectTransform val = Object.Instantiate<RectTransform>(m_statusEffectTemplate, (Transform)(object)m_statusEffectListRoot);
				((Component)val).gameObject.SetActive(true);
				val.anchoredPosition = Vector2.op_Implicit(new Vector3(-4f - (float)num2 * m_statusEffectSpacing, (float)(-num) * m_statusEffectSpacing, 0f));
				m_statusEffects.Add(val);
			}
		}
		for (int j = 0; j < statusEffects.Count; j++)
		{
			StatusEffect statusEffect = statusEffects[j];
			RectTransform val2 = m_statusEffects[j];
			Image component = ((Component)((Transform)val2).Find("Icon")).GetComponent<Image>();
			component.sprite = statusEffect.m_icon;
			if (statusEffect.m_flashIcon)
			{
				((Graphic)component).color = ((Mathf.Sin(Time.time * 10f) > 0f) ? s_colorRedish : Color.white);
			}
			else
			{
				((Graphic)component).color = Color.white;
			}
			((Component)((Transform)val2).Find("Cooldown")).gameObject.SetActive(statusEffect.m_cooldownIcon);
			((Component)val2).GetComponentInChildren<TMP_Text>().text = Localization.instance.Localize(statusEffect.m_name);
			TMP_Text component2 = ((Component)((Transform)val2).Find("TimeText")).GetComponent<TMP_Text>();
			string iconText = statusEffect.GetIconText();
			if (!string.IsNullOrEmpty(iconText))
			{
				((Component)component2).gameObject.SetActive(true);
				component2.text = iconText;
			}
			else
			{
				((Component)component2).gameObject.SetActive(false);
			}
			if (statusEffect.m_isNew)
			{
				statusEffect.m_isNew = false;
				((Component)val2).GetComponentInChildren<Animator>().SetTrigger("flash");
			}
		}
	}

	private void UpdateEvent(Player player)
	{
		RandomEvent activeEvent = RandEventSystem.instance.GetActiveEvent();
		if (activeEvent != null && !EnemyHud.instance.ShowingBossHud() && activeEvent.GetTime() > 3f)
		{
			m_eventBar.SetActive(true);
			m_eventName.text = Localization.instance.Localize(activeEvent.GetHudText());
		}
		else
		{
			m_eventBar.SetActive(false);
		}
	}

	public void ToggleBetaTextVisible()
	{
		m_betaText.SetActive(!m_betaText.activeSelf);
	}

	public void FlashHealthBar()
	{
		m_healthAnimator.SetTrigger("Flash");
	}

	public void StaminaBarUppgradeFlash()
	{
		m_staminaAnimator.SetTrigger("Flash");
	}

	public void AdrenalineBarFlash()
	{
		m_adrenalineAnimator.SetTrigger("Flash");
	}

	public void StaminaBarEmptyFlash()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		m_staminaHideTimer = 0f;
		AnimatorStateInfo currentAnimatorStateInfo = m_staminaAnimator.GetCurrentAnimatorStateInfo(0);
		if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsTag("nostamina"))
		{
			m_staminaAnimator.SetTrigger("NoStamina");
		}
	}

	public void EitrBarEmptyFlash()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		m_eitrHideTimer = 0f;
		AnimatorStateInfo currentAnimatorStateInfo = m_eitrAnimator.GetCurrentAnimatorStateInfo(0);
		if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsTag("nostamina"))
		{
			m_eitrAnimator.SetTrigger("NoStamina");
		}
	}

	public void EitrBarUppgradeFlash()
	{
		m_eitrAnimator.SetTrigger("Flash");
	}

	public static bool IsUserHidden()
	{
		if (Object.op_Implicit((Object)(object)m_instance))
		{
			return m_instance.m_userHidden;
		}
		return false;
	}

	public static bool InRadial()
	{
		if (Object.op_Implicit((Object)(object)m_instance) && Object.op_Implicit((Object)(object)instance.m_radialMenu))
		{
			return ((Component)instance.m_radialMenu).gameObject.activeSelf;
		}
		return false;
	}
}
