using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Jotunn.Managers;
using LocalizationManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace STUWard;

internal sealed class WardGuiController : MonoBehaviour
{
	private sealed class PermittedRowView
	{
		internal GameObject Root { get; }

		internal Text NameText { get; }

		internal int LastSeenGeneration { get; set; }

		internal PermittedRowView(GameObject root, Text nameText)
		{
			Root = root;
			NameText = nameText;
		}
	}

	private enum WardSettingsPage
	{
		General,
		Restrictions
	}

	private sealed class RestrictionRowView
	{
		internal GameObject Root { get; }

		internal Toggle Toggle { get; }

		internal Text Label { get; }

		internal Text StateText { get; }

		internal RestrictionRowView(GameObject root, Toggle toggle, Text label, Text stateText)
		{
			Root = root;
			Toggle = toggle;
			Label = label;
			StateText = stateText;
		}
	}

	private sealed class SliderCommitHandler : MonoBehaviour, IEndDragHandler, IEventSystemHandler, IPointerUpHandler
	{
		internal Action? OnCommit { get; set; }

		public void OnEndDrag(PointerEventData eventData)
		{
			OnCommit?.Invoke();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			OnCommit?.Invoke();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCloseDoorAfterDelay_003Ed__60 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Door door;

		public float delay;

		public WardGuiController _003C_003E4__this;

		private int _003Ckey_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCloseDoorAfterDelay_003Ed__60(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			WardGuiController wardGuiController = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ckey_003E5__2 = ((Object)door).GetInstanceID();
				_003C_003E2__current = (object)new WaitForSeconds(delay);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				wardGuiController._doorCloseCoroutines.Remove(_003Ckey_003E5__2);
				if ((Object)(object)door == (Object)null || door.m_canNotBeClosed)
				{
					return false;
				}
				ZNetView val = (((Object)(object)door.m_nview != (Object)null) ? door.m_nview : ((Component)door).GetComponent<ZNetView>());
				if ((Object)(object)val == (Object)null || !val.IsValid())
				{
					return false;
				}
				if (!WardSettings.TryGetAutoCloseDoorDelay(((Component)door).transform.position, out var _))
				{
					return false;
				}
				ZDO zDO = val.GetZDO();
				if (zDO == null || zDO.GetInt(ZDOVars.s_state, 0) == 0)
				{
					return false;
				}
				val.InvokeRPC("UseDoor", new object[1] { true });
				return false;
			}
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private const float ConfigurationPushDebounceSeconds = 0.15f;

	private const float ConfigurationRequestTimeoutSeconds = 5f;

	private readonly Dictionary<int, Coroutine> _doorCloseCoroutines = new Dictionary<int, Coroutine>();

	private readonly Dictionary<long, PermittedRowView> _permittedRows = new Dictionary<long, PermittedRowView>();

	private readonly Dictionary<WardRestrictionOptions, RestrictionRowView> _restrictionRows = new Dictionary<WardRestrictionOptions, RestrictionRowView>();

	private readonly List<long> _permittedRowsToRemove = new List<long>();

	private PrivateArea? _currentWard;

	private WardConfiguration _currentConfiguration;

	private WardConfiguration _authoritativeConfiguration;

	private WardConfiguration _pendingConfiguration;

	private GameObject? _root;

	private GameObject? _hintRoot;

	private GameObject? _panel;

	private GameObject? _generalPageRoot;

	private GameObject? _restrictionsPageRoot;

	private RectTransform? _permittedContent;

	private RectTransform? _restrictionsContent;

	private Text? _ownerValueText;

	private Text? _guildValueText;

	private Text? _shortcutHintText;

	private Text? _areaMarkerSpeedValueText;

	private Text? _areaMarkerAlphaValueText;

	private Text? _radiusValueText;

	private Text? _delayValueText;

	private Slider? _areaMarkerSpeedSlider;

	private Slider? _areaMarkerAlphaSlider;

	private Slider? _autoCloseDelaySlider;

	private Slider? _radiusSlider;

	private Toggle? _warningSoundToggle;

	private Toggle? _warningFlashToggle;

	private Button? _previousPageButton;

	private Button? _nextPageButton;

	private Image? _radiusLimitMarker;

	private Transform? _buildParent;

	private WardSettingsPage _currentPage;

	private bool _visible;

	private bool _suppressUiEvents;

	private bool _configurationCommitPending;

	private bool _configurationPushPending;

	private bool _layoutRebuildPending;

	private float _nextConfigurationPushTime;

	private float _pendingConfigurationRequestedAt;

	private int _lastPermittedRevision = int.MinValue;

	private int _permittedRefreshGeneration;

	private long _pendingConfigurationRequestId;

	private PermittedRowView? _emptyPermittedRow;

	internal static WardGuiController? Instance { get; private set; }

	internal bool IsVisible => _visible;

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null && (Object)(object)Instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)this);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		GUIManager.OnCustomGUIAvailable += BuildGui;
		BuildGui();
	}

	private void OnDestroy()
	{
		GUIManager.OnCustomGUIAvailable -= BuildGui;
		GUIManager.BlockInput(false);
		_doorCloseCoroutines.Clear();
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	private void Update()
	{
		if (_layoutRebuildPending && !IsTextInputFocused())
		{
			_layoutRebuildPending = false;
			RebuildLayout();
		}
		if (!_visible)
		{
			SetShortcutHintVisible(visible: false);
			TryOpenHoveredWardUi();
			return;
		}
		SetShortcutHintVisible(visible: false);
		if (Input.GetKeyDown((KeyCode)27))
		{
			CloseWardUi();
			return;
		}
		if ((Object)(object)_currentWard == (Object)null || !WardAccess.IsManagedWard(ManagedWardRef.FromArea(_currentWard), requireEnabled: false))
		{
			CloseWardUi();
			return;
		}
		if (HasPendingConfigurationRequest() && Time.unscaledTime - _pendingConfigurationRequestedAt >= 5f)
		{
			HandlePendingConfigurationRequestTimeout();
		}
		if (!HasPendingConfigurationRequest())
		{
			RefreshAuthoritativeConfigurationFromWard();
		}
		if (!HasPendingConfigurationRequest() && (_configurationCommitPending || _configurationPushPending) && Time.unscaledTime >= _nextConfigurationPushTime)
		{
			CommitPendingConfiguration();
		}
		if (WardPermittedSnapshots.GetRevision(_currentWard) != _lastPermittedRevision)
		{
			RefreshPermittedPlayers(force: false);
		}
	}

	internal bool TryOpenHoveredWardUi()
	{
		if (!Plugin.IsWardSettingsShortcutDown())
		{
			return false;
		}
		Player localPlayer = Player.m_localPlayer;
		GameObject val = (((Object)(object)localPlayer != (Object)null) ? localPlayer.m_hovering : null);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		ManagedWardRef ward = ManagedWardRef.FromArea(val.GetComponentInParent<PrivateArea>());
		if (!WardAccess.CanConfigureWard(ward, localPlayer) && !WardAdminDebugAccess.CanLocallyAttemptAnyWardControl(ward.Area, localPlayer))
		{
			return false;
		}
		OpenWardUi(ward.Area);
		return true;
	}

	internal void OpenWardUi(PrivateArea ward)
	{
		BuildGui();
		if (!((Object)(object)_root == (Object)null))
		{
			_currentWard = ward;
			_configurationCommitPending = false;
			_configurationPushPending = false;
			_lastPermittedRevision = int.MinValue;
			_currentPage = WardSettingsPage.General;
			ClearPendingConfigurationRequest();
			_authoritativeConfiguration = WardSettings.GetConfiguration(ward);
			_currentConfiguration = _authoritativeConfiguration;
			RefreshStaticTexts();
			RefreshControls();
			RefreshPermittedPlayers(force: true);
			SetVisible(visible: true);
		}
	}

	internal void CloseWardUi()
	{
		FlushPendingConfigurationPush();
		_currentWard = null;
		_configurationCommitPending = false;
		_configurationPushPending = false;
		_lastPermittedRevision = int.MinValue;
		ClearPendingConfigurationRequest();
		SetVisible(visible: false);
	}

	internal void ScheduleDoorAutoClose(Door door)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)door == (Object)null || door.m_canNotBeClosed)
		{
			return;
		}
		if (!WardSettings.TryGetAutoCloseDoorDelay(((Component)door).transform.position, out var delay))
		{
			CancelDoorAutoClose(door);
			return;
		}
		int instanceID = ((Object)door).GetInstanceID();
		if (_doorCloseCoroutines.TryGetValue(instanceID, out Coroutine value))
		{
			((MonoBehaviour)this).StopCoroutine(value);
		}
		_doorCloseCoroutines[instanceID] = ((MonoBehaviour)this).StartCoroutine(CloseDoorAfterDelay(door, delay));
	}

	internal void CancelDoorAutoClose(Door door)
	{
		if (!((Object)(object)door == (Object)null))
		{
			int instanceID = ((Object)door).GetInstanceID();
			if (_doorCloseCoroutines.TryGetValue(instanceID, out Coroutine value))
			{
				((MonoBehaviour)this).StopCoroutine(value);
				_doorCloseCoroutines.Remove(instanceID);
			}
		}
	}

	[IteratorStateMachine(typeof(_003CCloseDoorAfterDelay_003Ed__60))]
	private IEnumerator CloseDoorAfterDelay(Door door, float delay)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCloseDoorAfterDelay_003Ed__60(0)
		{
			_003C_003E4__this = this,
			door = door,
			delay = delay
		};
	}

	private void BuildGui()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Expected O, but got Unknown
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Expected O, but got Unknown
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Expected O, but got Unknown
		if (!((Object)(object)GUIManager.CustomGUIFront == (Object)null))
		{
			Localizer.ReloadCurrentLanguageIfAvailable();
			if ((Object)(object)_root != (Object)null)
			{
				Object.Destroy((Object)(object)_root);
			}
			if ((Object)(object)_hintRoot != (Object)null)
			{
				Object.Destroy((Object)(object)_hintRoot);
			}
			ClearPermittedRows();
			_emptyPermittedRow = null;
			_lastPermittedRevision = int.MinValue;
			_restrictionRows.Clear();
			_restrictionsContent = null;
			_generalPageRoot = null;
			_restrictionsPageRoot = null;
			_previousPageButton = null;
			_nextPageButton = null;
			_buildParent = null;
			GUIManager instance = GUIManager.Instance;
			Vector2 panelSize = WardGuiLayoutSettings.GetPanelSize();
			_root = new GameObject("STUWardGUIRoot", new Type[2]
			{
				typeof(RectTransform),
				typeof(Image)
			});
			_root.transform.SetParent(GUIManager.CustomGUIFront.transform, false);
			RectTransform component = _root.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = Vector2.zero;
			component.offsetMax = Vector2.zero;
			component.anchoredPosition = Vector2.zero;
			Image component2 = _root.GetComponent<Image>();
			((Graphic)component2).color = new Color(0f, 0f, 0f, 0.6f);
			((Graphic)component2).raycastTarget = true;
			_panel = instance.CreateWoodpanel(_root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), WardGuiLayoutSettings.GetPanelOffset(), panelSize.x, panelSize.y, false);
			((Object)_panel).name = "STUWardPanel";
			_generalPageRoot = CreatePageRoot("STUWardGeneralPage", panelSize);
			_restrictionsPageRoot = CreatePageRoot("STUWardRestrictionsPage", panelSize);
			GameObject val = instance.CreateText(string.Empty, GUIManager.CustomGUIFront.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), instance.AveriaSerifBold, 18, instance.ValheimBeige, true, Color.black, 460f, 84f, false);
			((Object)val).name = "STUWardShortcutHint";
			_hintRoot = val;
			_shortcutHintText = val.GetComponent<Text>();
			if ((Object)(object)_shortcutHintText != (Object)null)
			{
				_shortcutHintText.alignment = (TextAnchor)4;
			}
			SetShortcutHintVisible(visible: false);
			CreateLabel(WardLocalization.Localize("$stuw_ui_title", "Ward Settings"), WardGuiLayoutSettings.GetTitlePosition(), 34, 560f, 56f, (TextAnchor)4, instance.AveriaSerifBold, instance.ValheimOrange);
			_ownerValueText = CreateLabel(string.Empty, WardGuiLayoutSettings.GetOwnerPosition(), 22, 800f, 36f, (TextAnchor)3, instance.AveriaSerifBold, instance.ValheimBeige);
			_guildValueText = CreateLabel(string.Empty, WardGuiLayoutSettings.GetGuildPosition(), 20, 800f, 32f, (TextAnchor)3, instance.AveriaSerif, instance.ValheimBeige);
			((UnityEvent)CreateButton(WardLocalization.Localize("$stuw_ui_close", "Close"), WardGuiLayoutSettings.GetCloseButtonPosition(), 170f, 42f).onClick).AddListener(new UnityAction(CloseWardUi));
			_previousPageButton = CreateButton("<", WardGuiLayoutSettings.GetPageArrowButtonPosition(), 54f, 42f);
			((UnityEvent)_previousPageButton.onClick).AddListener((UnityAction)delegate
			{
				SetActivePage(WardSettingsPage.General);
			});
			StylePageArrowButton(_previousPageButton);
			_nextPageButton = CreateButton(">", WardGuiLayoutSettings.GetPageArrowButtonPosition(), 54f, 42f);
			((UnityEvent)_nextPageButton.onClick).AddListener((UnityAction)delegate
			{
				SetActivePage(WardSettingsPage.Restrictions);
			});
			StylePageArrowButton(_nextPageButton);
			BuildGeneralPage(instance);
			_buildParent = _restrictionsPageRoot.transform;
			BuildRestrictionsPage();
			_buildParent = null;
			SetActivePage(_currentPage);
			SetVisible(_visible);
			if (_visible && (Object)(object)_currentWard != (Object)null)
			{
				RefreshStaticTexts();
				RefreshControls();
				RefreshPermittedPlayers(force: true);
			}
		}
	}

	private void BuildGeneralPage(GUIManager gui)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_0513: Expected O, but got Unknown
		if ((Object)(object)_generalPageRoot == (Object)null)
		{
			return;
		}
		Vector2 permittedListSize = WardGuiLayoutSettings.GetPermittedListSize();
		Vector2 permittedListPosition = WardGuiLayoutSettings.GetPermittedListPosition();
		Vector2 registeredPlayersHeaderPosition = WardGuiLayoutSettings.GetRegisteredPlayersHeaderPosition();
		_buildParent = _generalPageRoot.transform;
		CreateLabel(WardLocalization.Localize("$stuw_ui_radius", "Ward radius"), WardGuiLayoutSettings.GetRadiusLabelPosition(), 21, 240f, 36f, (TextAnchor)3, gui.AveriaSerifBold, gui.ValheimBeige);
		_radiusSlider = CreateSlider(WardGuiLayoutSettings.GetRadiusSliderPosition(), 520f, 8f, WardSettings.MaxRadius, wholeNumbers: true, commitOnRelease: true);
		((UnityEvent<float>)(object)_radiusSlider.onValueChanged).AddListener((UnityAction<float>)OnRadiusSliderChanged);
		_radiusLimitMarker = CreateSliderLimitMarker(_radiusSlider, new Color(0.82f, 0.22f, 0.18f, 0.95f));
		_radiusValueText = CreateLabel(string.Empty, WardGuiLayoutSettings.GetRadiusValuePosition(), 21, 120f, 36f, (TextAnchor)4, gui.AveriaSerifBold, gui.ValheimYellow);
		CreateLabel(WardLocalization.Localize("$stuw_ui_range_speed", "Range speed"), WardGuiLayoutSettings.GetAreaMarkerSpeedLabelPosition(), 21, 240f, 36f, (TextAnchor)3, gui.AveriaSerifBold, gui.ValheimBeige);
		_areaMarkerSpeedSlider = CreateSlider(WardGuiLayoutSettings.GetAreaMarkerSpeedSliderPosition(), 520f, 0f, 1f, wholeNumbers: false, commitOnRelease: true);
		((UnityEvent<float>)(object)_areaMarkerSpeedSlider.onValueChanged).AddListener((UnityAction<float>)OnAreaMarkerSpeedSliderChanged);
		_areaMarkerSpeedValueText = CreateLabel(string.Empty, WardGuiLayoutSettings.GetAreaMarkerSpeedValuePosition(), 21, 120f, 36f, (TextAnchor)4, gui.AveriaSerifBold, gui.ValheimYellow);
		CreateLabel(WardLocalization.Localize("$stuw_ui_range_brightness", "Range brightness"), WardGuiLayoutSettings.GetAreaMarkerAlphaLabelPosition(), 21, 240f, 36f, (TextAnchor)3, gui.AveriaSerifBold, gui.ValheimBeige);
		_areaMarkerAlphaSlider = CreateSlider(WardGuiLayoutSettings.GetAreaMarkerAlphaSliderPosition(), 520f, 0f, 1f, wholeNumbers: false, commitOnRelease: true);
		((UnityEvent<float>)(object)_areaMarkerAlphaSlider.onValueChanged).AddListener((UnityAction<float>)OnAreaMarkerAlphaSliderChanged);
		_areaMarkerAlphaValueText = CreateLabel(string.Empty, WardGuiLayoutSettings.GetAreaMarkerAlphaValuePosition(), 21, 120f, 36f, (TextAnchor)4, gui.AveriaSerifBold, gui.ValheimYellow);
		CreateLabel(WardLocalization.Localize("$stuw_ui_door_close_delay", "Door close delay"), WardGuiLayoutSettings.GetAutoCloseDelayLabelPosition(), 21, 240f, 36f, (TextAnchor)3, gui.AveriaSerifBold, gui.ValheimBeige);
		_autoCloseDelaySlider = CreateSlider(WardGuiLayoutSettings.GetAutoCloseDelaySliderPosition(), 520f, 0f, 10f, wholeNumbers: true, commitOnRelease: true);
		((UnityEvent<float>)(object)_autoCloseDelaySlider.onValueChanged).AddListener((UnityAction<float>)OnAutoCloseDelaySliderChanged);
		_delayValueText = CreateLabel(string.Empty, WardGuiLayoutSettings.GetAutoCloseDelayValuePosition(), 21, 120f, 36f, (TextAnchor)4, gui.AveriaSerifBold, gui.ValheimYellow);
		CreateLabel(WardLocalization.Localize("$stuw_ui_warning_effects", "Warning effects"), WardGuiLayoutSettings.GetWarningEffectsLabelPosition(), 21, 240f, 36f, (TextAnchor)3, gui.AveriaSerifBold, gui.ValheimBeige);
		float sliderHandleHeight = GetSliderHandleHeight(_radiusSlider);
		CreateLabel(WardLocalization.Localize("$stuw_ui_warning_sound", "Sound"), WardGuiLayoutSettings.GetWarningSoundLabelPosition(sliderHandleHeight), 21, 120f, 36f, (TextAnchor)3, gui.AveriaSerifBold, gui.ValheimBeige);
		_warningSoundToggle = CreateCenteredToggle(GetBuildParent(), WardGuiLayoutSettings.GetWarningSoundTogglePosition(sliderHandleHeight), sliderHandleHeight);
		((UnityEvent<bool>)(object)_warningSoundToggle.onValueChanged).AddListener((UnityAction<bool>)OnWarningSoundToggleChanged);
		CreateLabel(WardLocalization.Localize("$stuw_ui_warning_flash", "Flash"), WardGuiLayoutSettings.GetWarningFlashLabelPosition(sliderHandleHeight), 21, 120f, 36f, (TextAnchor)3, gui.AveriaSerifBold, gui.ValheimBeige);
		_warningFlashToggle = CreateCenteredToggle(GetBuildParent(), WardGuiLayoutSettings.GetWarningFlashTogglePosition(sliderHandleHeight), sliderHandleHeight);
		((UnityEvent<bool>)(object)_warningFlashToggle.onValueChanged).AddListener((UnityAction<bool>)OnWarningFlashToggleChanged);
		CreateLabel(WardLocalization.Localize("$stuw_ui_registered_players", "Registered players"), registeredPlayersHeaderPosition, 24, permittedListSize.x, 40f, (TextAnchor)4, gui.AveriaSerifBold, gui.ValheimOrange);
		GameObject val = gui.CreateScrollView(_generalPageRoot.transform, false, true, 20f, 6f, gui.ValheimScrollbarHandleColorBlock, new Color(0f, 0f, 0f, 0.35f), permittedListSize.x, permittedListSize.y);
		ConfigureRect(val.GetComponent<RectTransform>(), permittedListPosition, permittedListSize.x, permittedListSize.y);
		((Object)val).name = "STUWardPermittedPlayers";
		ref RectTransform? permittedContent = ref _permittedContent;
		Transform obj = val.transform.Find("Scroll View/Viewport/Content");
		permittedContent = (RectTransform?)(object)((obj is RectTransform) ? obj : null);
		if (!((Object)(object)_permittedContent == (Object)null))
		{
			VerticalLayoutGroup component = ((Component)_permittedContent).GetComponent<VerticalLayoutGroup>();
			if (!((Object)(object)component == (Object)null))
			{
				((LayoutGroup)component).childAlignment = (TextAnchor)0;
				((HorizontalOrVerticalLayoutGroup)component).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)component).childForceExpandWidth = true;
				((HorizontalOrVerticalLayoutGroup)component).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)component).spacing = 6f;
				((LayoutGroup)component).padding = new RectOffset(8, 8, 8, 8);
			}
		}
	}

	internal void RebuildLayout()
	{
		BuildGui();
	}

	internal void ScheduleLayoutRebuild()
	{
		_layoutRebuildPending = true;
	}

	private void SetVisible(bool visible)
	{
		_visible = visible;
		if ((Object)(object)_root != (Object)null)
		{
			_root.SetActive(visible);
		}
		if (visible)
		{
			SetShortcutHintVisible(visible: false);
		}
		GUIManager.BlockInput(visible);
	}

	private void SetShortcutHintVisible(bool visible)
	{
		if ((Object)(object)_hintRoot != (Object)null)
		{
			_hintRoot.SetActive(visible);
		}
	}

	private void RefreshStaticTexts()
	{
		if (!((Object)(object)_currentWard == (Object)null) && !((Object)(object)_ownerValueText == (Object)null) && !((Object)(object)_guildValueText == (Object)null))
		{
			_ownerValueText.text = WardLocalization.LocalizeFormat("$stuw_ui_owner", "Owner: {0}", WardPrivateAreaSafeAccess.GetCreatorName(_currentWard));
			string wardGuildName = GuildsCompat.GetWardGuildName(_currentWard);
			_guildValueText.text = WardLocalization.LocalizeFormat("$stuw_ui_guild", "Guild: {0}", string.IsNullOrWhiteSpace(wardGuildName) ? "-" : wardGuildName);
		}
	}

	private void RefreshControls()
	{
		if (!((Object)(object)_areaMarkerSpeedSlider == (Object)null) && !((Object)(object)_areaMarkerSpeedValueText == (Object)null) && !((Object)(object)_areaMarkerAlphaSlider == (Object)null) && !((Object)(object)_areaMarkerAlphaValueText == (Object)null) && !((Object)(object)_autoCloseDelaySlider == (Object)null) && !((Object)(object)_radiusSlider == (Object)null) && !((Object)(object)_radiusValueText == (Object)null) && !((Object)(object)_delayValueText == (Object)null) && !((Object)(object)_warningSoundToggle == (Object)null) && !((Object)(object)_warningFlashToggle == (Object)null))
		{
			float maxRadius = (((Object)(object)_currentWard != (Object)null) ? WardSettings.GetMaxNonOverlappingRadius(_currentWard) : WardSettings.MaxRadius);
			float num = Mathf.Clamp(_currentConfiguration.Radius, 8f, WardSettings.MaxRadius);
			_suppressUiEvents = true;
			_areaMarkerSpeedSlider.value = _currentConfiguration.AreaMarkerSpeedMultiplier;
			_areaMarkerAlphaSlider.value = _currentConfiguration.AreaMarkerAlpha;
			_autoCloseDelaySlider.value = _currentConfiguration.AutoCloseDelay;
			_warningSoundToggle.isOn = _currentConfiguration.WarningSoundEnabled;
			_warningFlashToggle.isOn = _currentConfiguration.WarningFlashEnabled;
			_radiusSlider.maxValue = WardSettings.MaxRadius;
			_radiusSlider.value = num;
			_areaMarkerSpeedValueText.text = $"{Mathf.RoundToInt(_currentConfiguration.AreaMarkerSpeedMultiplier * 100f)}%";
			_areaMarkerAlphaValueText.text = $"{Mathf.RoundToInt(_currentConfiguration.AreaMarkerAlpha * 100f)}%";
			_radiusValueText.text = WardLocalization.LocalizeFormat("$stuw_ui_radius_value", "{0} m", Mathf.RoundToInt(num));
			_delayValueText.text = (Mathf.Approximately(_currentConfiguration.AutoCloseDelay, 0f) ? WardLocalization.Localize("$stuw_ui_off", "Off") : WardLocalization.LocalizeFormat("$stuw_ui_delay_value", "{0} s", Mathf.RoundToInt(_currentConfiguration.AutoCloseDelay)));
			RefreshRestrictionRows();
			_suppressUiEvents = false;
			UpdateRadiusLimitMarker(maxRadius);
			UpdateRadiusValueVisuals(maxRadius);
		}
	}

	private void RefreshPermittedPlayers(bool force)
	{
		if ((Object)(object)_currentWard == (Object)null || (Object)(object)_permittedContent == (Object)null)
		{
			return;
		}
		int revision = WardPermittedSnapshots.GetRevision(_currentWard);
		if (!force && revision == _lastPermittedRevision)
		{
			return;
		}
		_lastPermittedRevision = revision;
		List<KeyValuePair<long, string>> permittedPlayers = WardPrivateAreaSafeAccess.GetPermittedPlayers(_currentWard);
		if (permittedPlayers.Count == 0)
		{
			ClearPermittedRows();
			EnsureEmptyPermittedRow();
			UpdatePermittedRowText(_emptyPermittedRow, WardLocalization.Localize("$stuw_ui_no_registered_players", "No registered players."));
			_emptyPermittedRow.Root.SetActive(true);
			_emptyPermittedRow.Root.transform.SetSiblingIndex(0);
			return;
		}
		if (_emptyPermittedRow != null)
		{
			_emptyPermittedRow.Root.SetActive(false);
		}
		permittedPlayers.Sort((KeyValuePair<long, string> left, KeyValuePair<long, string> right) => string.Compare(left.Value, right.Value, StringComparison.OrdinalIgnoreCase));
		_permittedRefreshGeneration++;
		for (int i = 0; i < permittedPlayers.Count; i++)
		{
			KeyValuePair<long, string> keyValuePair = permittedPlayers[i];
			if (!_permittedRows.TryGetValue(keyValuePair.Key, out PermittedRowView value))
			{
				value = CreatePermittedRow(keyValuePair.Key);
				_permittedRows[keyValuePair.Key] = value;
			}
			value.LastSeenGeneration = _permittedRefreshGeneration;
			UpdatePermittedRowText(value, BuildPermittedPlayerDisplayText(_currentWard, keyValuePair.Key, keyValuePair.Value));
			value.Root.transform.SetSiblingIndex(i);
			value.Root.SetActive(true);
		}
		_permittedRowsToRemove.Clear();
		foreach (KeyValuePair<long, PermittedRowView> permittedRow in _permittedRows)
		{
			if (permittedRow.Value.LastSeenGeneration != _permittedRefreshGeneration)
			{
				_permittedRowsToRemove.Add(permittedRow.Key);
			}
		}
		for (int j = 0; j < _permittedRowsToRemove.Count; j++)
		{
			long key = _permittedRowsToRemove[j];
			if (_permittedRows.TryGetValue(key, out PermittedRowView value2))
			{
				Object.Destroy((Object)(object)value2.Root);
				_permittedRows.Remove(key);
			}
		}
	}

	private PermittedRowView CreatePermittedRow(long playerId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Expected O, but got Unknown
		Vector2 permittedListSize = WardGuiLayoutSettings.GetPermittedListSize();
		float num = Mathf.Max(560f, permittedListSize.x - 72f);
		float num2 = 46f;
		float num3 = 130f;
		GameObject val = new GameObject("PermittedPlayerRow", new Type[3]
		{
			typeof(RectTransform),
			typeof(Image),
			typeof(LayoutElement)
		});
		val.transform.SetParent((Transform)(object)_permittedContent, false);
		RectTransform component = val.GetComponent<RectTransform>();
		component.SetSizeWithCurrentAnchors((Axis)0, num);
		component.SetSizeWithCurrentAnchors((Axis)1, num2);
		((Graphic)val.GetComponent<Image>()).color = new Color(0f, 0f, 0f, 0.18f);
		LayoutElement component2 = val.GetComponent<LayoutElement>();
		component2.preferredHeight = num2;
		component2.preferredWidth = num;
		GameObject val2 = new GameObject("PlayerName", new Type[3]
		{
			typeof(RectTransform),
			typeof(Text),
			typeof(LayoutElement)
		});
		val2.transform.SetParent(val.transform, false);
		RectTransform component3 = val2.GetComponent<RectTransform>();
		component3.anchorMin = new Vector2(0.5f, 0.5f);
		component3.anchorMax = new Vector2(0.5f, 0.5f);
		component3.pivot = new Vector2(0f, 0.5f);
		float num4 = 10f;
		Vector2 registeredPlayersRemoveButtonPosition = WardGuiLayoutSettings.GetRegisteredPlayersRemoveButtonPosition();
		float num5 = Mathf.Clamp(registeredPlayersRemoveButtonPosition.x, (0f - num) * 0.5f + num3 * 0.5f + 10f, num * 0.5f - num3 * 0.5f - 4f);
		float num6 = num5 - num3 * 0.5f - 12f;
		float num7 = Mathf.Max(340f, num6 - ((0f - num) * 0.5f + num4));
		component3.anchoredPosition = new Vector2((0f - num) * 0.5f + num4, 0f);
		component3.SetSizeWithCurrentAnchors((Axis)0, num7);
		component3.SetSizeWithCurrentAnchors((Axis)1, num2 - 8f);
		Text component4 = val2.GetComponent<Text>();
		GUIManager instance = GUIManager.Instance;
		instance.ApplyTextStyle(component4, instance.AveriaSerifBold, instance.ValheimBeige, 18, false);
		component4.text = string.Empty;
		component4.alignment = (TextAnchor)3;
		component4.horizontalOverflow = (HorizontalWrapMode)1;
		component4.verticalOverflow = (VerticalWrapMode)0;
		((UnityEvent)CreateAnchoredButton(val.transform, WardLocalization.Localize("$stuw_ui_remove", "Remove"), new Vector2(num5, registeredPlayersRemoveButtonPosition.y), num3, 32f).onClick).AddListener((UnityAction)delegate
		{
			if (!((Object)(object)_currentWard == (Object)null))
			{
				WardSettings.RequestRemovePermitted(_currentWard, playerId);
			}
		});
		return new PermittedRowView(val, component4);
	}

	private void EnsureEmptyPermittedRow()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		if (_emptyPermittedRow == null)
		{
			GameObject val = new GameObject("PermittedPlayerRowEmpty", new Type[3]
			{
				typeof(RectTransform),
				typeof(Image),
				typeof(LayoutElement)
			});
			val.transform.SetParent((Transform)(object)_permittedContent, false);
			RectTransform component = val.GetComponent<RectTransform>();
			Vector2 permittedListSize = WardGuiLayoutSettings.GetPermittedListSize();
			float num = Mathf.Max(560f, permittedListSize.x - 72f);
			float num2 = 46f;
			component.SetSizeWithCurrentAnchors((Axis)0, num);
			component.SetSizeWithCurrentAnchors((Axis)1, num2);
			((Graphic)val.GetComponent<Image>()).color = new Color(0f, 0f, 0f, 0.18f);
			LayoutElement component2 = val.GetComponent<LayoutElement>();
			component2.preferredHeight = num2;
			component2.preferredWidth = num;
			GameObject val2 = new GameObject("PlayerName", new Type[3]
			{
				typeof(RectTransform),
				typeof(Text),
				typeof(LayoutElement)
			});
			val2.transform.SetParent(val.transform, false);
			RectTransform component3 = val2.GetComponent<RectTransform>();
			component3.anchorMin = new Vector2(0.5f, 0.5f);
			component3.anchorMax = new Vector2(0.5f, 0.5f);
			component3.pivot = new Vector2(0f, 0.5f);
			component3.anchoredPosition = new Vector2((0f - num) * 0.5f + 10f, 0f);
			component3.SetSizeWithCurrentAnchors((Axis)0, num - 24f);
			component3.SetSizeWithCurrentAnchors((Axis)1, num2 - 8f);
			Text component4 = val2.GetComponent<Text>();
			GUIManager instance = GUIManager.Instance;
			instance.ApplyTextStyle(component4, instance.AveriaSerifBold, instance.ValheimBeige, 18, false);
			component4.alignment = (TextAnchor)3;
			component4.horizontalOverflow = (HorizontalWrapMode)1;
			component4.verticalOverflow = (VerticalWrapMode)0;
			_emptyPermittedRow = new PermittedRowView(val, component4);
		}
	}

	private void UpdatePermittedRowText(PermittedRowView row, string text)
	{
		if (!string.Equals(row.NameText.text, text, StringComparison.Ordinal))
		{
			row.NameText.text = text;
		}
	}

	private void ClearPermittedRows()
	{
		foreach (PermittedRowView value in _permittedRows.Values)
		{
			Object.Destroy((Object)(object)value.Root);
		}
		_permittedRows.Clear();
		_permittedRowsToRemove.Clear();
		_permittedRefreshGeneration = 0;
		if (_emptyPermittedRow != null)
		{
			Object.Destroy((Object)(object)_emptyPermittedRow.Root);
			_emptyPermittedRow = null;
		}
	}

	private void BuildRestrictionsPage()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		if ((Object)(object)_restrictionsPageRoot == (Object)null)
		{
			return;
		}
		GUIManager instance = GUIManager.Instance;
		Vector2 restrictionListSize = WardGuiLayoutSettings.GetRestrictionListSize();
		CreateLabel(WardLocalization.Localize("$stuw_ui_restrictions", "Restrictions"), WardGuiLayoutSettings.GetRestrictionsHeaderPosition(), 24, restrictionListSize.x, 40f, (TextAnchor)4, instance.AveriaSerifBold, instance.ValheimOrange);
		GameObject val = instance.CreateScrollView(_restrictionsPageRoot.transform, false, true, 20f, 6f, instance.ValheimScrollbarHandleColorBlock, new Color(0f, 0f, 0f, 0.35f), restrictionListSize.x, restrictionListSize.y);
		ConfigureRect(val.GetComponent<RectTransform>(), WardGuiLayoutSettings.GetRestrictionListPosition(), restrictionListSize.x, restrictionListSize.y);
		((Object)val).name = "STUWardRestrictions";
		ref RectTransform? restrictionsContent = ref _restrictionsContent;
		Transform obj = val.transform.Find("Scroll View/Viewport/Content");
		restrictionsContent = (RectTransform?)(object)((obj is RectTransform) ? obj : null);
		if (!((Object)(object)_restrictionsContent == (Object)null))
		{
			VerticalLayoutGroup component = ((Component)_restrictionsContent).GetComponent<VerticalLayoutGroup>();
			if ((Object)(object)component != (Object)null)
			{
				((LayoutGroup)component).childAlignment = (TextAnchor)0;
				((HorizontalOrVerticalLayoutGroup)component).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)component).childForceExpandWidth = true;
				((HorizontalOrVerticalLayoutGroup)component).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)component).spacing = 6f;
				((LayoutGroup)component).padding = new RectOffset(8, 8, 8, 8);
			}
			IReadOnlyList<WardRestrictionDefinition> restrictionDefinitions = WardSettings.RestrictionDefinitions;
			for (int i = 0; i < restrictionDefinitions.Count; i++)
			{
				WardRestrictionDefinition definition = restrictionDefinitions[i];
				_restrictionRows[definition.Restriction] = CreateRestrictionRow(definition);
			}
		}
	}

	private RestrictionRowView CreateRestrictionRow(WardRestrictionDefinition definition)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		Vector2 restrictionListSize = WardGuiLayoutSettings.GetRestrictionListSize();
		float num = Mathf.Max(560f, restrictionListSize.x - 72f);
		float num2 = 48f;
		GameObject val = new GameObject("RestrictionRow", new Type[3]
		{
			typeof(RectTransform),
			typeof(Image),
			typeof(LayoutElement)
		});
		val.transform.SetParent((Transform)(object)_restrictionsContent, false);
		RectTransform component = val.GetComponent<RectTransform>();
		component.SetSizeWithCurrentAnchors((Axis)0, num);
		component.SetSizeWithCurrentAnchors((Axis)1, num2);
		((Graphic)val.GetComponent<Image>()).color = new Color(0f, 0f, 0f, 0.18f);
		LayoutElement component2 = val.GetComponent<LayoutElement>();
		component2.preferredHeight = num2;
		component2.preferredWidth = num;
		Toggle val2 = CreateCenteredToggle(val.transform, new Vector2((0f - num) * 0.5f + 28f, 0f), 30f);
		WardRestrictionOptions restriction = definition.Restriction;
		((UnityEvent<bool>)(object)val2.onValueChanged).AddListener((UnityAction<bool>)delegate(bool enabled)
		{
			OnRestrictionToggleChanged(restriction, enabled);
		});
		GameObject val3 = new GameObject("RestrictionName", new Type[3]
		{
			typeof(RectTransform),
			typeof(Text),
			typeof(LayoutElement)
		});
		val3.transform.SetParent(val.transform, false);
		RectTransform component3 = val3.GetComponent<RectTransform>();
		component3.anchorMin = new Vector2(0.5f, 0.5f);
		component3.anchorMax = new Vector2(0.5f, 0.5f);
		component3.pivot = new Vector2(0f, 0.5f);
		component3.anchoredPosition = new Vector2((0f - num) * 0.5f + 58f, 0f);
		component3.SetSizeWithCurrentAnchors((Axis)0, num - 220f);
		component3.SetSizeWithCurrentAnchors((Axis)1, num2 - 8f);
		Text component4 = val3.GetComponent<Text>();
		GUIManager instance = GUIManager.Instance;
		instance.ApplyTextStyle(component4, instance.AveriaSerifBold, instance.ValheimBeige, 20, false);
		component4.text = WardLocalization.Localize(definition.LocalizationToken, definition.LocalizationFallback);
		component4.alignment = (TextAnchor)3;
		component4.horizontalOverflow = (HorizontalWrapMode)0;
		component4.verticalOverflow = (VerticalWrapMode)0;
		GameObject val4 = new GameObject("RestrictionState", new Type[3]
		{
			typeof(RectTransform),
			typeof(Text),
			typeof(LayoutElement)
		});
		val4.transform.SetParent(val.transform, false);
		RectTransform component5 = val4.GetComponent<RectTransform>();
		component5.anchorMin = new Vector2(0.5f, 0.5f);
		component5.anchorMax = new Vector2(0.5f, 0.5f);
		component5.pivot = new Vector2(1f, 0.5f);
		component5.anchoredPosition = new Vector2(num * 0.5f - 14f, 0f);
		component5.SetSizeWithCurrentAnchors((Axis)0, 130f);
		component5.SetSizeWithCurrentAnchors((Axis)1, num2 - 8f);
		Text component6 = val4.GetComponent<Text>();
		instance.ApplyTextStyle(component6, instance.AveriaSerifBold, instance.ValheimYellow, 18, false);
		component6.alignment = (TextAnchor)5;
		component6.horizontalOverflow = (HorizontalWrapMode)1;
		component6.verticalOverflow = (VerticalWrapMode)0;
		return new RestrictionRowView(val, val2, component4, component6);
	}

	private void RefreshRestrictionRows()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		GUIManager instance = GUIManager.Instance;
		IReadOnlyList<WardRestrictionDefinition> restrictionDefinitions = WardSettings.RestrictionDefinitions;
		for (int i = 0; i < restrictionDefinitions.Count; i++)
		{
			WardRestrictionDefinition wardRestrictionDefinition = restrictionDefinitions[i];
			if (_restrictionRows.TryGetValue(wardRestrictionDefinition.Restriction, out RestrictionRowView value))
			{
				bool flag = WardSettings.IsRestrictionForced(wardRestrictionDefinition.Restriction);
				value.Toggle.isOn = WardSettings.HasRestriction(_currentConfiguration, wardRestrictionDefinition.Restriction);
				((Selectable)value.Toggle).interactable = !flag;
				((Graphic)value.Label).color = (Color)(flag ? new Color(0.65f, 0.62f, 0.55f) : instance.ValheimBeige);
				value.StateText.text = (flag ? WardLocalization.Localize("$stuw_ui_restriction_forced", "Forced") : string.Empty);
			}
		}
	}

	private void OnRestrictionToggleChanged(WardRestrictionOptions restriction, bool enabled)
	{
		if (!_suppressUiEvents)
		{
			if (WardSettings.IsRestrictionForced(restriction))
			{
				RefreshControls();
				return;
			}
			_currentConfiguration = WardSettings.WithRestriction(_currentConfiguration, restriction, enabled);
			RefreshControls();
			ScheduleConfigurationPush();
		}
	}

	private void OnAreaMarkerSpeedSliderChanged(float value)
	{
		if (!_suppressUiEvents)
		{
			_currentConfiguration = WardSettings.WithAreaMarkerSpeedMultiplier(_currentConfiguration, value);
			RefreshControls();
			ScheduleConfigurationCommit();
		}
	}

	private void OnAreaMarkerAlphaSliderChanged(float value)
	{
		if (!_suppressUiEvents)
		{
			_currentConfiguration = WardSettings.WithAreaMarkerAlpha(_currentConfiguration, value);
			RefreshControls();
			ScheduleConfigurationCommit();
		}
	}

	private void OnRadiusSliderChanged(float value)
	{
		if (!_suppressUiEvents)
		{
			_currentConfiguration = WardSettings.WithRadius(_currentConfiguration, value);
			ScheduleConfigurationCommit();
			UpdateRadiusTexts();
		}
	}

	private void OnAutoCloseDelaySliderChanged(float value)
	{
		if (!_suppressUiEvents)
		{
			_currentConfiguration = WardSettings.WithAutoCloseDelay(_currentConfiguration, value);
			RefreshControls();
			ScheduleConfigurationCommit();
		}
	}

	private void OnWarningSoundToggleChanged(bool enabled)
	{
		if (!_suppressUiEvents)
		{
			_currentConfiguration = WardSettings.WithWarningSoundEnabled(_currentConfiguration, enabled);
			RefreshControls();
			ScheduleConfigurationPush();
		}
	}

	private void OnWarningFlashToggleChanged(bool enabled)
	{
		if (!_suppressUiEvents)
		{
			_currentConfiguration = WardSettings.WithWarningFlashEnabled(_currentConfiguration, enabled);
			RefreshControls();
			ScheduleConfigurationPush();
		}
	}

	private void PushConfiguration()
	{
		if (!((Object)(object)_currentWard == (Object)null))
		{
			WardConfiguration currentConfiguration = _currentConfiguration;
			_configurationCommitPending = false;
			_configurationPushPending = false;
			WardConfigurationRequestSubmission wardConfigurationRequestSubmission = WardSettings.RequestUpdateConfiguration(_currentWard, currentConfiguration);
			if (wardConfigurationRequestSubmission.IsPending)
			{
				BeginPendingConfigurationRequest(wardConfigurationRequestSubmission.RequestId, currentConfiguration);
				return;
			}
			WardSettings.ShowConfigurationRequestFeedback(wardConfigurationRequestSubmission.ResultCode, wardConfigurationRequestSubmission.ShowOverlapMessage);
			ApplyConfigurationResponse(0L, wardConfigurationRequestSubmission.ResultCode, wardConfigurationRequestSubmission.Configuration);
		}
	}

	private void ScheduleConfigurationCommit()
	{
		if (!_suppressUiEvents && !((Object)(object)_currentWard == (Object)null))
		{
			_configurationCommitPending = true;
			_configurationPushPending = false;
			_nextConfigurationPushTime = float.PositiveInfinity;
		}
	}

	private void ScheduleConfigurationPush()
	{
		if (!_suppressUiEvents && !((Object)(object)_currentWard == (Object)null))
		{
			_configurationPushPending = true;
			_nextConfigurationPushTime = Time.unscaledTime + 0.15f;
		}
	}

	private void FlushPendingConfigurationPush()
	{
		if (_configurationCommitPending || _configurationPushPending)
		{
			CommitPendingConfiguration();
		}
	}

	private void CommitPendingConfiguration()
	{
		if (!_suppressUiEvents && !((Object)(object)_currentWard == (Object)null) && (_configurationCommitPending || _configurationPushPending) && !HasPendingConfigurationRequest())
		{
			PushConfiguration();
		}
	}

	internal void HandleWardConfigurationResponse(PrivateArea ward, long requestId, WardConfigurationRequestResultCode resultCode, WardConfiguration configuration)
	{
		if (!((Object)(object)_currentWard == (Object)null) && !((Object)(object)ward != (Object)(object)_currentWard))
		{
			ApplyConfigurationResponse(requestId, resultCode, configuration);
		}
	}

	private bool HasPendingConfigurationRequest()
	{
		return _pendingConfigurationRequestId != 0;
	}

	private void BeginPendingConfigurationRequest(long requestId, WardConfiguration submittedConfiguration)
	{
		_pendingConfigurationRequestId = requestId;
		_pendingConfiguration = submittedConfiguration;
		_pendingConfigurationRequestedAt = Time.unscaledTime;
	}

	private void ClearPendingConfigurationRequest()
	{
		_pendingConfigurationRequestId = 0L;
		_pendingConfiguration = default(WardConfiguration);
		_pendingConfigurationRequestedAt = 0f;
	}

	private void ApplyConfigurationResponse(long requestId, WardConfigurationRequestResultCode resultCode, WardConfiguration configuration)
	{
		bool flag = HasPendingConfigurationRequest();
		if (requestId == 0L || (flag && requestId == _pendingConfigurationRequestId))
		{
			bool flag2 = flag && !WardSettings.ConfigurationsMatch(_currentConfiguration, _pendingConfiguration);
			_authoritativeConfiguration = configuration;
			if (flag)
			{
				ClearPendingConfigurationRequest();
			}
			bool flag3 = (uint)(resultCode - 2) <= 2u;
			if (flag3 || !flag2)
			{
				_currentConfiguration = configuration;
			}
			if (flag3)
			{
				_configurationCommitPending = false;
				_configurationPushPending = false;
			}
			RefreshControls();
			TryFlushDeferredConfigurationAfterRequestResolution();
		}
	}

	private void RefreshAuthoritativeConfigurationFromWard()
	{
		if ((Object)(object)_currentWard == (Object)null)
		{
			return;
		}
		WardConfiguration configuration = WardSettings.GetConfiguration(_currentWard);
		if (!WardSettings.ConfigurationsMatch(_authoritativeConfiguration, configuration))
		{
			_authoritativeConfiguration = configuration;
			if (!_configurationCommitPending && !_configurationPushPending && !WardSettings.ConfigurationsMatch(_currentConfiguration, configuration))
			{
				_currentConfiguration = configuration;
				RefreshControls();
			}
		}
	}

	private void HandlePendingConfigurationRequestTimeout()
	{
		if (!((Object)(object)_currentWard == (Object)null) && HasPendingConfigurationRequest())
		{
			Plugin.Log.LogWarning((object)$"Timed out waiting for ward configuration response for ward instanceId={((Object)_currentWard).GetInstanceID()} requestId={_pendingConfigurationRequestId}.");
			bool num = !WardSettings.ConfigurationsMatch(_currentConfiguration, _pendingConfiguration);
			_authoritativeConfiguration = WardSettings.GetConfiguration(_currentWard);
			ClearPendingConfigurationRequest();
			if (!num)
			{
				_currentConfiguration = _authoritativeConfiguration;
				RefreshControls();
			}
			TryFlushDeferredConfigurationAfterRequestResolution();
		}
	}

	private void TryFlushDeferredConfigurationAfterRequestResolution()
	{
		if (!HasPendingConfigurationRequest())
		{
			if (_configurationCommitPending)
			{
				CommitPendingConfiguration();
			}
			else if (_configurationPushPending && Time.unscaledTime >= _nextConfigurationPushTime)
			{
				CommitPendingConfiguration();
			}
		}
	}

	private void UpdateRadiusTexts()
	{
		if (!((Object)(object)_radiusValueText == (Object)null))
		{
			float maxRadius = (((Object)(object)_currentWard != (Object)null) ? WardSettings.GetMaxNonOverlappingRadius(_currentWard) : WardSettings.MaxRadius);
			_radiusValueText.text = WardLocalization.LocalizeFormat("$stuw_ui_radius_value", "{0} m", Mathf.RoundToInt(_currentConfiguration.Radius));
			UpdateRadiusLimitMarker(maxRadius);
			UpdateRadiusValueVisuals(maxRadius);
		}
	}

	private GameObject CreatePageRoot(string name, Vector2 panelSize)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		GameObject val = new GameObject(name, new Type[1] { typeof(RectTransform) });
		val.transform.SetParent(_panel.transform, false);
		ConfigureRect(val.GetComponent<RectTransform>(), Vector2.zero, panelSize.x, panelSize.y);
		return val;
	}

	private Transform GetBuildParent()
	{
		if (!((Object)(object)_buildParent != (Object)null))
		{
			return _panel.transform;
		}
		return _buildParent;
	}

	private Button CreateButton(string text, Vector2 position, float width, float height)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		return GUIManager.Instance.CreateButton(text, _panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, width, height).GetComponent<Button>();
	}

	private Button CreateAnchoredButton(Transform parent, string text, Vector2 position, float width, float height)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		return GUIManager.Instance.CreateButton(text, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, width, height).GetComponent<Button>();
	}

	private Slider CreateSlider(Vector2 position, float width, float minValue, float maxValue, bool wholeNumbers, bool commitOnRelease = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = DefaultControls.CreateSlider(default(Resources));
		val.transform.SetParent(GetBuildParent(), false);
		((Object)val).name = "STUWardSlider";
		ConfigureRect(val.GetComponent<RectTransform>(), position, width, 34f);
		Slider component = val.GetComponent<Slider>();
		component.direction = (Direction)0;
		component.minValue = minValue;
		component.maxValue = maxValue;
		component.wholeNumbers = wholeNumbers;
		GUIManager.Instance.ApplySliderStyle(component);
		ShrinkSliderHandle(val.transform);
		if (commitOnRelease)
		{
			val.AddComponent<SliderCommitHandler>().OnCommit = CommitPendingConfiguration;
		}
		return component;
	}

	private Toggle CreateToggle(Vector2 position, float boxSize)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return CreateAnchoredToggle(GetBuildParent(), position, boxSize);
	}

	private Toggle CreateCenteredToggle(Transform parent, Vector2 position, float boxSize)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return CreateAnchoredToggle(parent, position, boxSize, centerGraphic: true);
	}

	private Toggle CreateAnchoredToggle(Transform parent, Vector2 position, float boxSize, bool centerGraphic = false, float graphicYOffset = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = DefaultControls.CreateToggle(default(Resources));
		obj.transform.SetParent(parent, false);
		((Object)obj).name = "STUWardToggle";
		ConfigureRect(obj.GetComponent<RectTransform>(), position, boxSize, boxSize);
		Toggle component = obj.GetComponent<Toggle>();
		Transform obj2 = obj.transform.Find("Background");
		Image val = ((obj2 != null) ? ((Component)obj2).GetComponent<Image>() : null);
		if ((Object)(object)val != (Object)null)
		{
			((Graphic)val).color = new Color(0f, 0f, 0f, 0.6f);
			Transform transform = ((Component)val).transform;
			RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if (val2 != null)
			{
				if (centerGraphic)
				{
					val2.anchorMin = new Vector2(0.5f, 0.5f);
					val2.anchorMax = new Vector2(0.5f, 0.5f);
					val2.pivot = new Vector2(0.5f, 0.5f);
				}
				val2.SetSizeWithCurrentAnchors((Axis)0, boxSize);
				val2.SetSizeWithCurrentAnchors((Axis)1, boxSize);
				val2.anchoredPosition = (Vector2)(centerGraphic ? new Vector2(0f, graphicYOffset) : Vector2.zero);
			}
		}
		Transform obj3 = obj.transform.Find("Background/Checkmark");
		Image val3 = ((obj3 != null) ? ((Component)obj3).GetComponent<Image>() : null);
		if ((Object)(object)val3 != (Object)null)
		{
			((Graphic)val3).color = GUIManager.Instance.ValheimOrange;
			Transform transform2 = ((Component)val3).transform;
			RectTransform val4 = (RectTransform)(object)((transform2 is RectTransform) ? transform2 : null);
			if (val4 != null)
			{
				float num = Mathf.Max(4f, boxSize - 6f);
				if (centerGraphic)
				{
					val4.anchorMin = new Vector2(0.5f, 0.5f);
					val4.anchorMax = new Vector2(0.5f, 0.5f);
					val4.pivot = new Vector2(0.5f, 0.5f);
				}
				val4.SetSizeWithCurrentAnchors((Axis)0, num);
				val4.SetSizeWithCurrentAnchors((Axis)1, num);
				val4.anchoredPosition = Vector2.zero;
			}
		}
		Transform val5 = obj.transform.Find("Label");
		if ((Object)(object)val5 != (Object)null)
		{
			((Component)val5).gameObject.SetActive(false);
		}
		return component;
	}

	private static Image? CreateSliderLimitMarker(Slider slider, Color color)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)slider).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		GameObject val2 = new GameObject("STUWardLimitMarker", new Type[2]
		{
			typeof(RectTransform),
			typeof(Image)
		});
		val2.transform.SetParent((Transform)(object)val, false);
		val2.transform.SetAsLastSibling();
		RectTransform component = val2.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(1f, 0.5f);
		component.anchorMax = new Vector2(1f, 0.5f);
		component.pivot = new Vector2(0.5f, 0.5f);
		component.anchoredPosition = Vector2.zero;
		component.sizeDelta = new Vector2(4f, GetSliderTrackHeight(slider));
		Transform val3 = ((Transform)val).Find("Handle Slide Area");
		if ((Object)(object)val3 != (Object)null)
		{
			val2.transform.SetSiblingIndex(val3.GetSiblingIndex());
		}
		else
		{
			val2.transform.SetAsLastSibling();
		}
		Image component2 = val2.GetComponent<Image>();
		((Graphic)component2).color = color;
		((Graphic)component2).raycastTarget = false;
		return component2;
	}

	private void UpdateRadiusLimitMarker(float maxRadius)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_radiusSlider == (Object)null) && !((Object)(object)_radiusLimitMarker == (Object)null))
		{
			float num = Mathf.Clamp(maxRadius, _radiusSlider.minValue, _radiusSlider.maxValue);
			bool flag = num < _radiusSlider.maxValue - 0.01f;
			((Component)_radiusLimitMarker).gameObject.SetActive(flag);
			if (flag)
			{
				float num2 = Mathf.InverseLerp(_radiusSlider.minValue, _radiusSlider.maxValue, num);
				RectTransform rectTransform = ((Graphic)_radiusLimitMarker).rectTransform;
				rectTransform.anchorMin = new Vector2(num2, 0.5f);
				rectTransform.anchorMax = new Vector2(num2, 0.5f);
				rectTransform.anchoredPosition = Vector2.zero;
				rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, GetSliderTrackHeight(_radiusSlider));
			}
		}
	}

	private void UpdateRadiusValueVisuals(float maxRadius)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_radiusValueText == (Object)null))
		{
			((Graphic)_radiusValueText).color = (Color)((_currentConfiguration.Radius > maxRadius + 0.01f) ? new Color(0.85f, 0.2f, 0.2f) : GUIManager.Instance.ValheimYellow);
		}
	}

	private Text CreateLabel(string text, Vector2 position, int fontSize, float width, float height, TextAnchor alignment, Font font, Color color)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Text component = GUIManager.Instance.CreateText(text, GetBuildParent(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, font, fontSize, color, true, Color.black, width, height, false).GetComponent<Text>();
		component.alignment = alignment;
		return component;
	}

	private void SetActivePage(WardSettingsPage page)
	{
		_currentPage = page;
		if ((Object)(object)_generalPageRoot != (Object)null)
		{
			_generalPageRoot.SetActive(page == WardSettingsPage.General);
		}
		if ((Object)(object)_restrictionsPageRoot != (Object)null)
		{
			_restrictionsPageRoot.SetActive(page == WardSettingsPage.Restrictions);
		}
		UpdatePageButtonVisuals();
	}

	private void UpdatePageButtonVisuals()
	{
		if ((Object)(object)_previousPageButton != (Object)null)
		{
			((Component)_previousPageButton).gameObject.SetActive(_currentPage == WardSettingsPage.Restrictions);
		}
		if ((Object)(object)_nextPageButton != (Object)null)
		{
			((Component)_nextPageButton).gameObject.SetActive(_currentPage == WardSettingsPage.General);
		}
	}

	private static void StylePageArrowButton(Button? button)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Text val = (((Object)(object)button != (Object)null) ? ((Component)button).GetComponentInChildren<Text>() : null);
		if ((Object)(object)val != (Object)null)
		{
			val.text = val.text.Trim();
			val.fontSize = 34;
			((Graphic)val).color = GUIManager.Instance.ValheimYellow;
			val.alignment = (TextAnchor)4;
			RectTransform rectTransform = ((Graphic)val).rectTransform;
			rectTransform.anchoredPosition += new Vector2(0f, 1f);
		}
	}

	private static void ConfigureRect(RectTransform? rectTransform, Vector2 position, float width, float height)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rectTransform == (Object)null))
		{
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = position;
			rectTransform.SetSizeWithCurrentAnchors((Axis)0, width);
			rectTransform.SetSizeWithCurrentAnchors((Axis)1, height);
		}
	}

	private static void ShrinkSliderHandle(Transform sliderTransform)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Transform obj = sliderTransform.Find("Handle Slide Area/Handle");
		RectTransform val = (RectTransform)(object)((obj is RectTransform) ? obj : null);
		if (!((Object)(object)val == (Object)null))
		{
			((Transform)val).localScale = new Vector3(0.5f, 0.8f, 1f);
		}
	}

	private static float GetSliderTrackHeight(Slider slider)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Transform obj = ((Component)slider).transform.Find("Background");
		RectTransform val = (RectTransform)(object)((obj is RectTransform) ? obj : null);
		if ((Object)(object)val == (Object)null)
		{
			return 14f;
		}
		Rect rect = val.rect;
		if (((Rect)(ref rect)).height > 0.01f)
		{
			rect = val.rect;
			return ((Rect)(ref rect)).height;
		}
		if (!(val.sizeDelta.y > 0.01f))
		{
			return 14f;
		}
		return val.sizeDelta.y;
	}

	private static float GetSliderHandleHeight(Slider? slider)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)slider == (Object)null)
		{
			return 18f;
		}
		Transform obj = ((Component)slider).transform.Find("Handle Slide Area/Handle");
		RectTransform val = (RectTransform)(object)((obj is RectTransform) ? obj : null);
		if ((Object)(object)val == (Object)null)
		{
			return 18f;
		}
		Rect rect = val.rect;
		float num;
		if (!(((Rect)(ref rect)).height > 0.01f))
		{
			num = val.sizeDelta.y;
		}
		else
		{
			rect = val.rect;
			num = ((Rect)(ref rect)).height;
		}
		float num2 = num;
		if (num2 <= 0.01f)
		{
			num2 = 18f;
		}
		float num3 = num2 * Mathf.Abs(((Transform)val).localScale.y);
		return Mathf.Max(12f, num3);
	}

	private static string BuildPermittedPlayerDisplayText(PrivateArea? area, long playerId, string playerName)
	{
		string permittedPlayerGuildName = GetPermittedPlayerGuildName(area, playerId);
		string permittedPlayerPlatformId = GetPermittedPlayerPlatformId(area, playerId);
		string text = (string.IsNullOrWhiteSpace(permittedPlayerGuildName) ? "-" : permittedPlayerGuildName);
		string text2 = (string.IsNullOrWhiteSpace(permittedPlayerPlatformId) ? "-" : permittedPlayerPlatformId);
		return WardLocalization.LocalizeFormat("$stuw_ui_registered_player_format", "{0} / {1} / {2}", playerName, text, text2);
	}

	private static string GetPermittedPlayerGuildName(PrivateArea? area, long playerId)
	{
		if (WardPermittedSnapshots.TryGet(area, playerId, out string guildName, out string _))
		{
			return guildName;
		}
		return GuildsCompat.GetPlayerGuildName(playerId);
	}

	private static string GetPermittedPlayerPlatformId(PrivateArea? area, long playerId)
	{
		if (WardPermittedSnapshots.TryGet(area, playerId, out string _, out string platformId))
		{
			return platformId;
		}
		return WardOwnership.GetPlayerSteamIdDisplay(playerId);
	}

	private static bool IsTextInputFocused()
	{
		GameObject val = (((Object)(object)EventSystem.current != (Object)null) ? EventSystem.current.currentSelectedGameObject : null);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		if ((Object)(object)val.GetComponent<InputField>() != (Object)null)
		{
			return true;
		}
		Component[] components = val.GetComponents<Component>();
		foreach (Component val2 in components)
		{
			if ((Object)(object)val2 != (Object)null && ((object)val2).GetType().Name.Contains("InputField", StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}
}
