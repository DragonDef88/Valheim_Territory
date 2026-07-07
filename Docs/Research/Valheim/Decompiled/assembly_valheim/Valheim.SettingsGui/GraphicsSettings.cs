using System;
using System.Collections.Generic;
using GUIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Valheim.SettingsGui;

public class GraphicsSettings : MonoBehaviour, ISettingsTab
{
	[SerializeField]
	private UIGroupHandler m_groupHandler;

	[SerializeField]
	private VerticalLayoutGroup m_verticalLayoutGroup;

	[SerializeField]
	private GridLayoutGroup m_toggleGridLayoutGroup;

	[SerializeField]
	private TMP_Text m_devBuildSettingsText;

	[SerializeField]
	private GameObject m_devBuildSettingFramePrefab;

	[SerializeField]
	private TMP_Text m_devGraphicsModeValuesText;

	[SerializeField]
	private TMP_Text m_devPlayerPrefsValuesText;

	[Header("Present settings")]
	[SerializeField]
	private GameObject m_resolutionRoot;

	[SerializeField]
	private GuiDropdown m_resolutionDropdown;

	[SerializeField]
	private GameObject m_upscalingOptionsRoot;

	[SerializeField]
	private GuiDropdown m_renderScaleDropdown;

	[SerializeField]
	private GuiDropdown m_upscalingAlgorithmDropdown;

	[SerializeField]
	private Toggle m_fullscreenToggle;

	[SerializeField]
	private GuiButton m_testResolutionButton;

	[SerializeField]
	private GameObject m_resolutionDialog;

	[SerializeField]
	private GameObject m_resolutionListElement;

	[SerializeField]
	private RectTransform m_resolutionListRoot;

	[SerializeField]
	private Scrollbar m_resolutionListScroll;

	[SerializeField]
	private GameObject m_resolutionSwitchDialog;

	[SerializeField]
	private GuiButton m_resolutionOk;

	[SerializeField]
	private Slider m_fpsLimitSlider;

	[SerializeField]
	private TMP_Text m_fpsLimitText;

	[SerializeField]
	private Toggle m_vsyncToggle;

	[SerializeField]
	private int m_minResWidth = 1280;

	[SerializeField]
	private int m_minResHeight = 720;

	[Header("Graphics presets")]
	[SerializeField]
	private GameObject m_graphicPresetsRoot;

	[SerializeField]
	private TMP_Text m_graphicsMode;

	[SerializeField]
	private Button m_graphicPresetLeft;

	[SerializeField]
	private Button m_graphicPresetRight;

	[SerializeField]
	private TMP_Text m_graphicsModeDescr;

	[Header("Quality settings")]
	[SerializeField]
	private GameObject m_qualitySliderPrefab;

	[SerializeField]
	private GameObject m_qualityTogglePrefab;

	private List<Resolution> m_resolutions = new List<Resolution>();

	private List<Resolution> m_resolutionOptions = new List<Resolution>();

	private ScrollRectEnsureVisible m_dropdownScrollRectEnsureVisible;

	private bool m_resolutionOptionModified;

	private bool m_oldFullscreen;

	private Resolution m_oldResolution;

	private OkActionCompletedHandler m_okActionCompletedCallback;

	private GraphicsSettingsState m_currentSettingsRaw;

	private int m_currentPresetID = 100;

	private bool m_currentPresetModified;

	private List<QualitySliderData> m_qualitySliders = new List<QualitySliderData>(Enum.GetValues(typeof(GraphicsSettingInt)).Length);

	private List<QualityToggleData> m_qualityToggles = new List<QualityToggleData>(Enum.GetValues(typeof(GraphicsSettingBool)).Length);

	private List<Slider> m_dynamicQualitySliders = new List<Slider>(Enum.GetValues(typeof(GraphicsSettingInt)).Length);

	private List<Toggle> m_dynamicQualityToggles = new List<Toggle>(Enum.GetValues(typeof(GraphicsSettingBool)).Length);

	private List<QualityDropdownData> m_qualityDropdowns = new List<QualityDropdownData>(3);

	private List<GameObject> m_devBuildSettingFrames = new List<GameObject>();

	public event Action<string, int> SharedSettingChanged;

	public void Initialize()
	{
		InitializeUI();
		GraphicsModeConfiguration currentGraphicsModeConfiguration = GraphicsSettingsManager.Instance.GetCurrentGraphicsModeConfiguration();
		GraphicsSettingsState state = m_currentSettingsRaw;
		GraphicsSettingsPreset currentPreset = GetCurrentPreset(currentGraphicsModeConfiguration, excludeUnsupported: true);
		if (currentPreset != null)
		{
			GraphicsSettingsManager.Instance.SetGraphicsSettingsFromPreset(currentGraphicsModeConfiguration, ref state, currentPreset);
		}
		SetSettingsFromState(state);
		SubscribeEvents();
	}

	private void UpdateUI()
	{
		GraphicsModeConfiguration currentGraphicsModeConfiguration = GraphicsSettingsManager.Instance.GetCurrentGraphicsModeConfiguration();
		UpdateAvailableResolutions();
		UpdateSettingAvailability(currentGraphicsModeConfiguration);
		m_currentSettingsRaw = GraphicsSettingsManager.Instance.CurrentSettingsRaw;
		m_currentPresetID = GraphicsSettingsManager.Instance.CurrentPresetID;
	}

	public void Terminate()
	{
		UnsubscribeEvents();
	}

	public void OnTabOpen(Button backButton, Button okButton)
	{
		UpdateNavigation(backButton, okButton);
	}

	public void OnOkAsync(OkActionCompletedHandler okActionCompletedCallback)
	{
		if (okActionCompletedCallback == null)
		{
			throw new ArgumentNullException("okActionCompletedCallback");
		}
		GraphicsModeConfiguration currentGraphicsModeConfiguration = GraphicsSettingsManager.Instance.GetCurrentGraphicsModeConfiguration();
		GraphicsSettingsState settings = m_currentSettingsRaw;
		GraphicsSettingsPreset graphicsSettingsPreset = (m_currentPresetModified ? null : GetCurrentPreset(currentGraphicsModeConfiguration, excludeUnsupported: true));
		if (graphicsSettingsPreset != null)
		{
			GraphicsSettingsManager.Instance.SaveAndApplyGraphicsSettingsWithPreset(ref settings, graphicsSettingsPreset.m_type);
		}
		else
		{
			GraphicsSettingsManager.Instance.SaveAndApplyGraphicsSettingsCustom(ref settings);
		}
		if (GraphicsSettingsManager.CanChangePresentSettings() && ResolutionSettingsChanged())
		{
			m_okActionCompletedCallback = okActionCompletedCallback;
			OnTestResolution();
		}
		else
		{
			okActionCompletedCallback();
		}
	}

	public void OnBack()
	{
	}

	public void OnSharedSettingChanged(string setting, int value)
	{
		GraphicsSettingBool graphicsSettingBool;
		if (!(setting == "MotionBlur"))
		{
			if (!(setting == "DepthOfField"))
			{
				return;
			}
			graphicsSettingBool = GraphicsSettingBool.DepthOfField;
		}
		else
		{
			graphicsSettingBool = GraphicsSettingBool.MotionBlur;
		}
		bool isOn = value == 1;
		int i;
		for (i = 0; i < m_qualityToggles.Count && m_qualityToggles[i].m_setting != graphicsSettingBool; i++)
		{
		}
		m_qualityToggles[i].m_toggle.isOn = isOn;
	}

	private void InitializeUI()
	{
		InitializePresentSettings();
		InitializeUpscalingAlgorithmsDropdown();
		CreateDynamicGraphicsQualitySettings();
		UpdateUI();
	}

	private void InitializePresentSettings()
	{
		InitializePresentSettingsState();
		m_resolutionDialog.SetActive(false);
		m_qualitySliders.Add(new QualitySliderData(GraphicsSettingInt.FpsLimit, m_fpsLimitSlider, m_fpsLimitText));
		m_qualityToggles.Add(new QualityToggleData(GraphicsSettingBool.Vsync, m_vsyncToggle));
	}

	private void InitializePresentSettingsState()
	{
		m_fpsLimitSlider.minValue = 30f;
		m_fpsLimitSlider.maxValue = 361f;
	}

	private void SaveRevertableResolutionSetting()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		m_oldFullscreen = Screen.fullScreen;
		m_oldResolution = PresentManager.GetCurrentPresentResolution();
		Debug.Log((object)m_oldResolution);
	}

	private void InitializeUpscalingAlgorithmsDropdown()
	{
		List<string> list = new List<string>();
		for (int i = 0; Enum.IsDefined(typeof(UpscalingAlgorithm), i); i++)
		{
			list.Add(((UpscalingAlgorithm)i).ToDisplayName());
		}
		((TMP_Dropdown)m_upscalingAlgorithmDropdown).ClearOptions();
		((TMP_Dropdown)m_upscalingAlgorithmDropdown).AddOptions(list);
		m_qualityDropdowns.Add(new QualityDropdownData(GraphicsSettingInt.UpscalingAlgorithm, m_upscalingAlgorithmDropdown, null));
	}

	private void CreateDynamicGraphicsQualitySettings()
	{
		GraphicsSettingsManager.Instance.GetCurrentGraphicsModeConfiguration();
		int num = m_qualitySliderPrefab.transform.GetSiblingIndex() + 1;
		for (int i = 0; i < 32; i++)
		{
			GraphicsSettingInt graphicsSettingInt = (GraphicsSettingInt)(1 << i);
			if (Enum.IsDefined(typeof(GraphicsSettingInt), graphicsSettingInt) && graphicsSettingInt.IsShownToUser() && !graphicsSettingInt.IsPresentSetting() && graphicsSettingInt != GraphicsSettingInt.Target3DResolutionVertical && graphicsSettingInt != GraphicsSettingInt.UpscalingAlgorithm)
			{
				GameObject obj = Object.Instantiate<GameObject>(m_qualitySliderPrefab, m_qualitySliderPrefab.transform.parent);
				obj.transform.SetSiblingIndex(num++);
				Slider component = obj.GetComponent<Slider>();
				TMP_Text component2 = ((Component)obj.transform.Find("Label")).GetComponent<TMP_Text>();
				TMP_Text component3 = ((Component)obj.transform.Find("Value")).GetComponent<TMP_Text>();
				RangeIntInclusive range = graphicsSettingInt.GetRange();
				component.minValue = range.m_minValue;
				component.maxValue = range.m_maxValue;
				component2.text = graphicsSettingInt.ToDisplayName();
				obj.SetActive(true);
				QualitySliderData item = new QualitySliderData(graphicsSettingInt, component, component3);
				m_qualitySliders.Add(item);
				m_dynamicQualitySliders.Add(component);
			}
		}
		num = m_qualityTogglePrefab.transform.GetSiblingIndex();
		for (int j = 0; j < 32; j++)
		{
			GraphicsSettingBool graphicsSettingBool = (GraphicsSettingBool)(1 << j);
			if (Enum.IsDefined(typeof(GraphicsSettingBool), graphicsSettingBool) && graphicsSettingBool.IsShownToUser() && !graphicsSettingBool.IsPresentSetting())
			{
				GameObject obj2 = Object.Instantiate<GameObject>(m_qualityTogglePrefab, m_qualityTogglePrefab.transform.parent);
				obj2.transform.SetSiblingIndex(num++);
				GuiToggle componentInChildren = obj2.GetComponentInChildren<GuiToggle>();
				((Component)((Component)componentInChildren).transform.Find("Label")).GetComponent<TMP_Text>().text = graphicsSettingBool.ToDisplayName();
				obj2.SetActive(true);
				QualityToggleData item2 = new QualityToggleData(graphicsSettingBool, (Toggle)(object)componentInChildren);
				m_qualityToggles.Add(item2);
				m_dynamicQualityToggles.Add((Toggle)(object)componentInChildren);
			}
		}
	}

	private void SubscribeEvents()
	{
		GraphicsSettingsManager.GraphicsSettingsChanged += UpdateUI;
		((UnityEvent<int>)(object)((TMP_Dropdown)m_resolutionDropdown).onValueChanged).AddListener((UnityAction<int>)delegate
		{
			OnResolutionOptionModified();
		});
		((UnityEvent<bool>)(object)m_fullscreenToggle.onValueChanged).AddListener((UnityAction<bool>)delegate
		{
			OnResolutionOptionModified();
		});
		m_resolutionDropdown.OnExpandedStateChange += OnDropdownExpanded;
		for (int i = 0; i < m_qualityDropdowns.Count; i++)
		{
			QualityDropdownData ui = m_qualityDropdowns[i];
			ui.m_dropdown.OnExpandedStateChange += OnDropdownExpanded;
			((UnityEvent<int>)(object)((TMP_Dropdown)ui.m_dropdown).onValueChanged).AddListener((UnityAction<int>)delegate
			{
				OnDropdownValueUpdated(ui.m_setting);
			});
		}
		for (int j = 0; j < m_qualitySliders.Count; j++)
		{
			QualitySliderData ui2 = m_qualitySliders[j];
			((UnityEvent<float>)(object)ui2.m_slider.onValueChanged).AddListener((UnityAction<float>)delegate
			{
				OnSliderValueUpdated(ui2.m_setting);
			});
		}
		for (int k = 0; k < m_qualityToggles.Count; k++)
		{
			QualityToggleData ui3 = m_qualityToggles[k];
			((UnityEvent<bool>)(object)ui3.m_toggle.onValueChanged).AddListener((UnityAction<bool>)delegate
			{
				OnToggleValueUpdated(ui3.m_setting);
			});
		}
	}

	private void UnsubscribeEvents()
	{
		GraphicsSettingsManager.GraphicsSettingsChanged -= UpdateUI;
		((UnityEventBase)((TMP_Dropdown)m_resolutionDropdown).onValueChanged).RemoveAllListeners();
		((UnityEventBase)m_fullscreenToggle.onValueChanged).RemoveAllListeners();
		m_resolutionDropdown.OnExpandedStateChange -= OnDropdownExpanded;
		for (int i = 0; i < m_qualityDropdowns.Count; i++)
		{
			_ = m_qualityDropdowns[i];
			m_renderScaleDropdown.OnExpandedStateChange -= OnDropdownExpanded;
			((UnityEventBase)((TMP_Dropdown)m_renderScaleDropdown).onValueChanged).RemoveAllListeners();
		}
		for (int j = 0; j < m_qualitySliders.Count; j++)
		{
			((UnityEventBase)m_qualitySliders[j].m_slider.onValueChanged).RemoveAllListeners();
		}
		for (int k = 0; k < m_qualityToggles.Count; k++)
		{
			((UnityEventBase)m_qualityToggles[k].m_toggle.onValueChanged).RemoveAllListeners();
		}
	}

	private void Update()
	{
		CenterScrollRectOnCurrenlySelected();
	}

	public void OnResSwitchOK()
	{
		m_resolutionSwitchDialog.SetActive(false);
		m_resolutionOptionModified = false;
		UpdateTestResolutionButton();
		Settings.instance.BlockNavigation(block: false);
		InvokeCallbackIfSet();
	}

	public void OnResSwitchCancel()
	{
		RevertMode();
		m_resolutionSwitchDialog.SetActive(false);
	}

	public void OnTestResolution()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		SaveRevertableResolutionSetting();
		ApplyResolution(m_resolutionOptions[((TMP_Dropdown)m_resolutionDropdown).value]);
		m_resolutionSwitchDialog.SetActive(true);
		if ((Object)(object)m_resolutionSwitchDialog.transform.parent == (Object)(object)((Component)this).transform)
		{
			m_resolutionSwitchDialog.transform.parent = m_resolutionSwitchDialog.transform.parent.parent;
		}
		m_resolutionSwitchDialog.GetComponent<ResolutionSwitchDialogTimedRemoval>().ResCountdownTimer = 5f;
		EventSystem.current.SetSelectedGameObject(((Component)m_resolutionOk).gameObject);
		Settings.instance.BlockNavigation(block: true);
	}

	public void OnGraphicPresetRight()
	{
		ChangePreset(1);
	}

	public void OnGraphicPresetLeft()
	{
		ChangePreset(-1);
	}

	private void OnDropdownValueUpdated(GraphicsSettingInt setting)
	{
		int i;
		for (i = 0; i < m_qualityDropdowns.Count && m_qualityDropdowns[i].m_setting != setting; i++)
		{
		}
		QualityDropdownData qualityDropdownData = m_qualityDropdowns[i];
		int value = qualityDropdownData.GetValue(((TMP_Dropdown)qualityDropdownData.m_dropdown).value);
		ModifySetting(setting, value);
	}

	private void OnSliderValueUpdated(GraphicsSettingInt setting)
	{
		int i;
		for (i = 0; i < m_qualitySliders.Count && m_qualitySliders[i].m_setting != setting; i++)
		{
		}
		QualitySliderData qualitySliderData = m_qualitySliders[i];
		int value = Mathf.RoundToInt(qualitySliderData.m_slider.value);
		qualitySliderData.m_valueText.text = GetDisplayValue(setting, value);
		ModifySetting(setting, value);
	}

	private void OnToggleValueUpdated(GraphicsSettingBool setting)
	{
		int i;
		for (i = 0; i < m_qualityToggles.Count && m_qualityToggles[i].m_setting != setting; i++)
		{
		}
		bool isOn = m_qualityToggles[i].m_toggle.isOn;
		ModifySetting(setting, isOn);
		switch (setting)
		{
		case GraphicsSettingBool.DepthOfField:
			this.SharedSettingChanged?.Invoke("DepthOfField", isOn ? 1 : 0);
			break;
		case GraphicsSettingBool.MotionBlur:
			this.SharedSettingChanged?.Invoke("MotionBlur", isOn ? 1 : 0);
			break;
		}
	}

	private void OnDropdownExpanded(bool expanded)
	{
		Settings.instance.BlockNavigation(expanded);
		if ((Object)(object)m_dropdownScrollRectEnsureVisible == (Object)null && expanded)
		{
			FindDropdownScrollRect();
		}
	}

	private void UpdateAvailableResolutions()
	{
		PopulateResolutions();
		PopulateRenderScales();
	}

	private void PopulateResolutions()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		UpdateValidResolutions();
		Resolution? val = null;
		if (((TMP_Dropdown)m_resolutionDropdown).value < m_resolutionOptions.Count)
		{
			val = m_resolutionOptions[((TMP_Dropdown)m_resolutionDropdown).value];
		}
		((TMP_Dropdown)m_resolutionDropdown).ClearOptions();
		m_resolutionOptions.Clear();
		List<string> list = new List<string>();
		int num = -1;
		foreach (Resolution resolution in m_resolutions)
		{
			Resolution current = resolution;
			string text = $"{((Resolution)(ref current)).width}x{((Resolution)(ref current)).height}";
			if ((int)Screen.fullScreenMode == 0)
			{
				RefreshRate refreshRateRatio = ((Resolution)(ref current)).refreshRateRatio;
				list.Add($"{text} {Mathf.Round((float)((RefreshRate)(ref refreshRateRatio)).value)}hz");
			}
			else
			{
				if (list.Contains(text))
				{
					continue;
				}
				list.Add(text);
			}
			if (val.HasValue)
			{
				Resolution value = val.Value;
				if (((object)(Resolution)(ref value)).Equals((object?)current))
				{
					num = m_resolutionOptions.Count;
				}
			}
			else if (Screen.width == ((Resolution)(ref current)).width && Screen.height == ((Resolution)(ref current)).height)
			{
				num = m_resolutionOptions.Count;
			}
			m_resolutionOptions.Add(current);
		}
		((TMP_Dropdown)m_resolutionDropdown).AddOptions(list);
		if (num >= 0)
		{
			((TMP_Dropdown)m_resolutionDropdown).SetValueWithoutNotify(num);
		}
	}

	private void PopulateRenderScales()
	{
		int target3DResolutionVertical = GraphicsSettingsManager.Instance.GetCurrentSettingsWithCurrentPresetApplied(includeBackground: false).m_target3DResolutionVertical;
		List<int> list = new List<int>
		{
			int.MaxValue,
			2160,
			1800,
			1600,
			1440,
			1200,
			1080,
			900,
			800,
			720,
			600,
			480,
			360,
			240,
			192,
			160
		};
		if (UpscaledFrameBuffer.AutomaticRenderScaleSupported())
		{
			list.Add(0);
		}
		int height = Screen.height;
		int num = (height - 1) / 240;
		for (int i = 2; i <= num; i++)
		{
			int num2 = height / i;
			if (num2 * i == height && !list.Contains(num2))
			{
				list.Add(num2);
			}
		}
		if (!list.Contains(target3DResolutionVertical))
		{
			list.Add(target3DResolutionVertical);
		}
		list.Sort(delegate(int a, int b)
		{
			if (a == 0)
			{
				a = 2147483646;
			}
			if (b == 0)
			{
				b = 2147483646;
			}
			return -a.CompareTo(b);
		});
		int value = list.IndexOf(target3DResolutionVertical);
		List<string> list2 = new List<string>(list.Count);
		for (int j = 0; j < list.Count; j++)
		{
			int num3 = list[j];
			switch (num3)
			{
			case int.MaxValue:
				list2.Add(Localization.instance.Localize("$settings_native"));
				continue;
			case 0:
				list2.Add(Localization.instance.Localize("$settings_automatic"));
				continue;
			}
			int num4 = height / num3;
			if (num4 * num3 == height && num4 > 1)
			{
				list2.Add($"{num3}p (1/{num4})");
			}
			else
			{
				list2.Add($"{num3}p");
			}
		}
		((TMP_Dropdown)m_renderScaleDropdown).ClearOptions();
		((TMP_Dropdown)m_renderScaleDropdown).AddOptions(list2);
		((TMP_Dropdown)m_renderScaleDropdown).value = value;
		m_qualityDropdowns.Add(new QualityDropdownData(GraphicsSettingInt.Target3DResolutionVertical, m_renderScaleDropdown, null, list));
	}

	private void UpdateSettingAvailability(GraphicsModeConfiguration config)
	{
		bool isChangeable = GraphicsSettingsManager.CanChangePresentSettings();
		bool flag = SetChangeable(isChangeable, (Selectable)(object)m_resolutionDropdown);
		bool flag2 = SetChangeable(isChangeable, (Selectable)(object)m_fullscreenToggle);
		m_resolutionRoot.SetActive(flag || flag2);
		bool flag3 = SetChangeable(config.HasCustomPreset || config.CanCustomizeGraphicsSetting(GraphicsSettingInt.Target3DResolutionVertical), (Selectable)(object)m_renderScaleDropdown);
		bool flag4 = SetChangeable(config.HasCustomPreset || config.CanCustomizeGraphicsSetting(GraphicsSettingInt.UpscalingAlgorithm), (Selectable)(object)m_upscalingAlgorithmDropdown);
		m_upscalingOptionsRoot.SetActive(flag3 || flag4);
		int num = config.Presets.Count;
		if (config.HasCustomPreset)
		{
			num++;
		}
		SetChangeable(num > 1, m_graphicPresetsRoot, (Selectable)m_graphicPresetLeft, (Selectable)m_graphicPresetRight);
		for (int i = 0; i < m_qualitySliders.Count; i++)
		{
			QualitySliderData qualitySliderData = m_qualitySliders[i];
			SetChangeable(config.HasCustomPreset || config.CanCustomizeGraphicsSetting(qualitySliderData.m_setting), (Selectable)(object)qualitySliderData.m_slider);
		}
		for (int j = 0; j < m_qualityToggles.Count; j++)
		{
			QualityToggleData qualityToggleData = m_qualityToggles[j];
			SetChangeable(config.HasCustomPreset || config.CanCustomizeGraphicsSetting(qualityToggleData.m_setting), (Selectable)(object)qualityToggleData.m_toggle);
		}
	}

	private bool SetChangeable(bool isChangeable, GameObject root, params Selectable[] uis)
	{
		bool flag = isChangeable;
		if (root != null)
		{
			root.SetActive(flag);
		}
		if (flag && !isChangeable)
		{
			for (int i = 0; i < uis.Length; i++)
			{
				AddFrame(((Component)uis[i]).gameObject);
			}
			return true;
		}
		return flag;
	}

	private bool SetChangeable(bool isChangeable, Selectable ui)
	{
		return SetChangeable(isChangeable, ((Component)ui).gameObject, ui);
	}

	private void UpdateNavigation(Button backButton, Button okButton)
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		List<Selectable> list = new List<Selectable>(m_dynamicQualitySliders.Count);
		for (int i = 0; i < m_dynamicQualitySliders.Count; i++)
		{
			if (((Behaviour)m_dynamicQualitySliders[i]).isActiveAndEnabled)
			{
				list.Add((Selectable)(object)m_dynamicQualitySliders[i]);
			}
		}
		List<Selectable> list2 = new List<Selectable>(m_dynamicQualityToggles.Count);
		for (int j = 0; j < m_dynamicQualityToggles.Count; j++)
		{
			if (((Behaviour)m_dynamicQualityToggles[j]).isActiveAndEnabled)
			{
				list2.Add((Selectable)(object)m_dynamicQualityToggles[j]);
			}
		}
		Selectable val = (Selectable)(object)m_graphicPresetLeft;
		Selectable val2 = (Selectable)((list.Count > 0) ? list[0] : ((list2.Count <= 0) ? ((object)backButton) : ((object)list2[0])));
		GuiUtils.SetNavigationDown((Selectable)(object)m_graphicPresetLeft, val2);
		GuiUtils.SetNavigationDown((Selectable)(object)m_graphicPresetRight, val2);
		val2 = (Selectable)((list2.Count <= 0) ? ((object)backButton) : ((object)list2[0]));
		for (int k = 0; k < list.Count; k++)
		{
			Selectable obj = list[k];
			Navigation navigation = obj.navigation;
			((Navigation)(ref navigation)).selectOnUp = ((k > 0) ? list[k - 1] : val);
			((Navigation)(ref navigation)).selectOnDown = ((k < list.Count - 1) ? list[k + 1] : val2);
			obj.navigation = navigation;
		}
		if (list.Count > 0)
		{
			val = list[list.Count - 1];
		}
		val2 = (Selectable)(object)backButton;
		int constraintCount = m_toggleGridLayoutGroup.constraintCount;
		for (int l = 0; l < list2.Count; l++)
		{
			int num = l / constraintCount;
			int num2 = l - constraintCount * num;
			Selectable obj2 = list2[l];
			Navigation navigation2 = obj2.navigation;
			((Navigation)(ref navigation2)).selectOnLeft = ((num2 > 0) ? list2[l - 1] : null);
			((Navigation)(ref navigation2)).selectOnRight = ((num2 < constraintCount - 1 && l + 1 < list2.Count) ? list2[l + 1] : null);
			((Navigation)(ref navigation2)).selectOnUp = ((l - constraintCount >= 0) ? list2[l - constraintCount] : val);
			((Navigation)(ref navigation2)).selectOnDown = ((l + constraintCount < list2.Count) ? list2[l + constraintCount] : val2);
			obj2.navigation = navigation2;
		}
		if (list2.Count > 0)
		{
			val = list2[list2.Count - 1];
		}
		GuiUtils.SetNavigationUp((Selectable)(object)backButton, val);
		GuiUtils.SetNavigationUp((Selectable)(object)okButton, val);
	}

	private void CenterScrollRectOnCurrenlySelected()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (Object.op_Implicit((Object)(object)m_dropdownScrollRectEnsureVisible) && Object.op_Implicit((Object)(object)currentSelectedGameObject) && ZInput.GamepadActive)
		{
			m_dropdownScrollRectEnsureVisible.CenterOnItem(currentSelectedGameObject.GetComponent<RectTransform>());
		}
	}

	private void UpdateDisplayValues()
	{
		for (int i = 0; i < m_qualitySliders.Count; i++)
		{
			m_qualitySliders[i].m_valueText.text = GetDisplayValue(m_qualitySliders[i].m_setting, Mathf.RoundToInt(m_qualitySliders[i].m_slider.value));
		}
		UpdateModeStepperInfo();
		UpdateTestResolutionButton();
	}

	private void OnResolutionOptionModified()
	{
		m_resolutionOptionModified = true;
		UpdateTestResolutionButton();
	}

	private void UpdateTestResolutionButton()
	{
		((Selectable)m_testResolutionButton).interactable = ResolutionSettingsChanged();
		((Component)m_testResolutionButton).gameObject.SetActive(((Selectable)m_testResolutionButton).interactable);
	}

	private void UpdateModeStepperInfo()
	{
		GraphicsModeConfiguration currentGraphicsModeConfiguration = GraphicsSettingsManager.Instance.GetCurrentGraphicsModeConfiguration();
		GraphicsSettingsPreset currentPreset = GetCurrentPreset(currentGraphicsModeConfiguration, excludeUnsupported: false);
		if (currentPreset == null)
		{
			m_graphicsMode.alpha = 1f;
			m_graphicsMode.text = Localization.instance.Localize("$settings_quality_mode_custom");
			m_graphicsModeDescr.text = "";
		}
		else if (GraphicsSettingsManager.Instance.IsPresetSupported(currentPreset))
		{
			m_graphicsMode.alpha = 1f;
			m_graphicsMode.text = Localization.instance.Localize(currentPreset.m_type.NameTextId) + (m_currentPresetModified ? "*" : "");
			m_graphicsModeDescr.text = Localization.instance.Localize(m_currentPresetModified ? "$settings_quality_mode_customized" : currentPreset.m_type.DescriptionTextId);
		}
		else
		{
			m_graphicsMode.alpha = 0.25f;
			m_graphicsMode.text = Localization.instance.Localize(currentPreset.m_type.NameTextId) + "*";
			m_graphicsModeDescr.text = Localization.instance.Localize("$settings_quality_mode_not_supported");
		}
	}

	private void FindDropdownScrollRect()
	{
		ScrollRectEnsureVisible[] componentsInChildren = ((Component)this).GetComponentsInChildren<ScrollRectEnsureVisible>(false);
		if (componentsInChildren.Length == 0)
		{
			ZLog.LogError((object)"Missing ScrollRectEnsureVisible component on Resolution dropdown list!");
		}
		else if (componentsInChildren.Length == 1)
		{
			m_dropdownScrollRectEnsureVisible = componentsInChildren[0];
		}
		else
		{
			ZLog.LogError((object)"More than one enabled component with ScrollRectEnsureVisible active within graphics tab at a time - not supported!");
		}
	}

	private void RemoveAllFrames()
	{
		foreach (GameObject devBuildSettingFrame in m_devBuildSettingFrames)
		{
			Object.Destroy((Object)(object)devBuildSettingFrame);
		}
		m_devBuildSettingFrames.Clear();
	}

	private void AddFrame(GameObject target)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(m_devBuildSettingFramePrefab, target.transform);
		((Object)val).name = ((Object)target).name + " [Dev Frame]";
		val.SetActive(true);
		m_devBuildSettingFrames.Add(val);
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		RectTransform component = target.GetComponent<RectTransform>();
		component.GetWorldCorners(array);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(array[3].x - array[0].x, array[1].y - array[0].y);
		Rect rect = component.rect;
		float num = ((Rect)(ref rect)).width / val2.x;
		val.transform.position = Vector2.op_Implicit(new Vector2(array[0].x + val2.x / 2f, array[2].y - val2.y / 2f));
		val.GetComponent<RectTransform>().sizeDelta = new Vector2((val2.x + 10f) * num, (val2.y + 10f) * num);
	}

	private GraphicsSettingsPreset GetCurrentPreset(GraphicsModeConfiguration config, bool excludeUnsupported)
	{
		GraphicsSettingsPreset presetByID = config.GetPresetByID(m_currentPresetID);
		if (config.HasCustomPreset)
		{
			return presetByID;
		}
		if (presetByID == null)
		{
			return config.DefaultPreset;
		}
		if (excludeUnsupported && presetByID != null && !GraphicsSettingsManager.Instance.IsPresetSupported(presetByID))
		{
			presetByID = config.GetPresetByID(GraphicsSettingsManager.Instance.CurrentPresetID);
		}
		return presetByID;
	}

	private GraphicsSettingsState GetStateFromSettingsMenu()
	{
		GraphicsSettingsState result = default(GraphicsSettingsState);
		result.m_presentSettings = new PresentSettingsState
		{
			m_fpsLimit = Mathf.RoundToInt(m_fpsLimitSlider.value),
			m_vsync = m_vsyncToggle.isOn
		};
		for (int i = 0; i < m_qualityDropdowns.Count; i++)
		{
			GraphicsSettingInt setting = m_qualityDropdowns[i].m_setting;
			if (!setting.IsPresentSetting())
			{
				int value = m_qualityDropdowns[i].GetValue(Mathf.RoundToInt((float)((TMP_Dropdown)m_qualityDropdowns[i].m_dropdown).value));
				result.SetValue(setting, value);
			}
		}
		for (int j = 0; j < m_qualitySliders.Count; j++)
		{
			GraphicsSettingInt setting2 = m_qualitySliders[j].m_setting;
			if (!setting2.IsPresentSetting())
			{
				result.SetValue(setting2, Mathf.RoundToInt(m_qualitySliders[j].m_slider.value));
			}
		}
		for (int k = 0; k < m_qualityToggles.Count; k++)
		{
			GraphicsSettingBool setting3 = m_qualityToggles[k].m_setting;
			if (!setting3.IsPresentSetting())
			{
				result.SetValue(setting3, m_qualityToggles[k].m_toggle.isOn);
			}
		}
		return result;
	}

	private void ModifySetting(GraphicsSettingInt setting, int value)
	{
		GraphicsModeConfiguration currentGraphicsModeConfiguration = GraphicsSettingsManager.Instance.GetCurrentGraphicsModeConfiguration();
		if (currentGraphicsModeConfiguration.HasCustomPreset && m_currentPresetID != 100)
		{
			GraphicsSettingsPreset currentPreset = GetCurrentPreset(currentGraphicsModeConfiguration, excludeUnsupported: true);
			if (!currentGraphicsModeConfiguration.CanCustomizeGraphicsSetting(setting) && currentPreset.TryGetQualitySetting(setting, out int _))
			{
				m_currentPresetModified = true;
				m_currentSettingsRaw = GetStateFromSettingsMenu();
			}
		}
		m_currentSettingsRaw.SetValue(setting, value);
		UpdateModeStepperInfo();
	}

	private void ModifySetting(GraphicsSettingBool setting, bool value)
	{
		GraphicsModeConfiguration currentGraphicsModeConfiguration = GraphicsSettingsManager.Instance.GetCurrentGraphicsModeConfiguration();
		if (currentGraphicsModeConfiguration.HasCustomPreset && m_currentPresetID != 100)
		{
			GraphicsSettingsPreset currentPreset = GetCurrentPreset(currentGraphicsModeConfiguration, excludeUnsupported: true);
			if (!currentGraphicsModeConfiguration.CanCustomizeGraphicsSetting(setting) && currentPreset.TryGetQualitySetting(setting, out var value2) && (!GraphicsSettingsManager.Instance.IsAccessibilitySetting(setting) || (!value2 && value)))
			{
				m_currentPresetModified = true;
				m_currentSettingsRaw = GetStateFromSettingsMenu();
			}
		}
		m_currentSettingsRaw.SetValue(setting, value);
		UpdateModeStepperInfo();
	}

	private void InvokeCallbackIfSet()
	{
		if (m_okActionCompletedCallback != null)
		{
			OkActionCompletedHandler okActionCompletedCallback = m_okActionCompletedCallback;
			m_okActionCompletedCallback = null;
			okActionCompletedCallback();
		}
	}

	private bool ResolutionSettingsChanged()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!m_resolutionOptionModified)
		{
			return false;
		}
		Resolution val = m_resolutionOptions[((TMP_Dropdown)m_resolutionDropdown).value];
		bool fullScreen = Screen.fullScreen;
		Resolution currentPresentResolution = PresentManager.GetCurrentPresentResolution();
		if (((Resolution)(ref currentPresentResolution)).width == ((Resolution)(ref val)).width && ((Resolution)(ref currentPresentResolution)).height == ((Resolution)(ref val)).height)
		{
			RefreshRate refreshRateRatio = ((Resolution)(ref currentPresentResolution)).refreshRateRatio;
			double value = ((RefreshRate)(ref refreshRateRatio)).value;
			refreshRateRatio = ((Resolution)(ref val)).refreshRateRatio;
			if (value == ((RefreshRate)(ref refreshRateRatio)).value)
			{
				return fullScreen != m_fullscreenToggle.isOn;
			}
		}
		return true;
	}

	private void SetSettingsFromState(GraphicsSettingsState state)
	{
		m_fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
		for (int i = 0; i < m_qualityDropdowns.Count; i++)
		{
			QualityDropdownData qualityDropdownData = m_qualityDropdowns[i];
			int value = state.GetValue(qualityDropdownData.m_setting);
			for (int j = 0; j < qualityDropdownData.OptionCount; j++)
			{
				if (qualityDropdownData.GetValue(j) == value)
				{
					((TMP_Dropdown)qualityDropdownData.m_dropdown).SetValueWithoutNotify(j);
					break;
				}
			}
		}
		for (int k = 0; k < m_qualitySliders.Count; k++)
		{
			QualitySliderData qualitySliderData = m_qualitySliders[k];
			int num = state.GetValue(m_qualitySliders[k].m_setting);
			if (qualitySliderData.m_setting == GraphicsSettingInt.FpsLimit && num < 30)
			{
				num = 361;
			}
			m_qualitySliders[k].m_slider.SetValueWithoutNotify((float)num);
		}
		for (int l = 0; l < m_qualityToggles.Count; l++)
		{
			m_qualityToggles[l].m_toggle.SetIsOnWithoutNotify(state.GetValue(m_qualityToggles[l].m_setting));
		}
		UpdateDisplayValues();
	}

	public void ChangePreset(int relativeIndex)
	{
		GraphicsModeConfiguration currentGraphicsModeConfiguration = GraphicsSettingsManager.Instance.GetCurrentGraphicsModeConfiguration();
		int num = (currentGraphicsModeConfiguration.HasCustomPreset ? 1 : 0);
		int presetIndexByID = currentGraphicsModeConfiguration.GetPresetIndexByID(m_currentPresetID);
		presetIndexByID = Utils.Mod(presetIndexByID + relativeIndex + num, currentGraphicsModeConfiguration.Presets.Count + num) - num;
		m_currentPresetID = currentGraphicsModeConfiguration.GetPresetIDByIndex(presetIndexByID);
		m_currentPresetModified = false;
		GraphicsSettingsState state = m_currentSettingsRaw;
		GraphicsSettingsPreset currentPreset = GetCurrentPreset(currentGraphicsModeConfiguration, excludeUnsupported: true);
		if (currentPreset != null)
		{
			GraphicsSettingsManager.Instance.SetGraphicsSettingsFromPreset(currentGraphicsModeConfiguration, ref state, currentPreset);
		}
		SetSettingsFromState(state);
	}

	private void UpdateValidResolutions()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Resolution[] resolutions = Screen.resolutions;
		m_resolutions.Clear();
		Resolution[] array = resolutions;
		for (int i = 0; i < array.Length; i++)
		{
			Resolution item = array[i];
			if ((((Resolution)(ref item)).width >= m_minResWidth && ((Resolution)(ref item)).height >= m_minResHeight) || ((Resolution)(ref item)).width == ((Resolution)(ref m_oldResolution)).width || ((Resolution)(ref item)).height == ((Resolution)(ref m_oldResolution)).height)
			{
				m_resolutions.Add(item);
			}
		}
		if (m_resolutions.Count == 0)
		{
			m_resolutions.Add(m_oldResolution);
		}
		m_resolutions.Sort(delegate(Resolution a, Resolution b)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			if (((Resolution)(ref a)).width != ((Resolution)(ref b)).width)
			{
				return -((Resolution)(ref a)).width.CompareTo(((Resolution)(ref b)).width);
			}
			if (((Resolution)(ref a)).height != ((Resolution)(ref b)).height)
			{
				return -((Resolution)(ref a)).height.CompareTo(((Resolution)(ref b)).height);
			}
			RefreshRate refreshRateRatio = ((Resolution)(ref a)).refreshRateRatio;
			double value = ((RefreshRate)(ref refreshRateRatio)).value;
			refreshRateRatio = ((Resolution)(ref b)).refreshRateRatio;
			double value2 = ((RefreshRate)(ref refreshRateRatio)).value;
			return (value != value2) ? (-value.CompareTo(value2)) : 0;
		});
	}

	private void ApplyResolution(Resolution resolution)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (Screen.width != ((Resolution)(ref resolution)).width || Screen.height != ((Resolution)(ref resolution)).height || m_fullscreenToggle.isOn != Screen.fullScreen)
		{
			Screen.SetResolution(((Resolution)(ref resolution)).width, ((Resolution)(ref resolution)).height, (FullScreenMode)(m_fullscreenToggle.isOn ? 1 : 3), ((Resolution)(ref resolution)).refreshRateRatio);
		}
	}

	public void RevertMode()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		m_fullscreenToggle.isOn = m_oldFullscreen;
		ApplyResolution(m_oldResolution);
		UpdateTestResolutionButton();
		Settings.instance.BlockNavigation(block: false);
		InvokeCallbackIfSet();
	}

	private static string GetQualityText(int level)
	{
		return level switch
		{
			1 => Localization.instance.Localize("[$settings_medium]"), 
			2 => Localization.instance.Localize("[$settings_high]"), 
			3 => Localization.instance.Localize("[$settings_veryhigh]"), 
			_ => Localization.instance.Localize("[$settings_low]"), 
		};
	}

	private static string GetDisplayValue(GraphicsSettingInt setting, int value)
	{
		switch (setting)
		{
		case GraphicsSettingInt.FpsLimit:
			if (value > 360)
			{
				return Localization.instance.Localize("$settings_unlimited");
			}
			break;
		case GraphicsSettingInt.Vegetation:
			return GetQualityText(Math.Max(0, value - 1));
		case GraphicsSettingInt.LOD:
		case GraphicsSettingInt.Lights:
		case GraphicsSettingInt.ShadowQuality:
			return GetQualityText(value);
		case GraphicsSettingInt.PointLights:
		{
			int pointLightLimit = GraphicsSettingsManager.GetPointLightLimit(value);
			return GetQualityText(value) + " (" + ((pointLightLimit < 0) ? Localization.instance.Localize("$settings_infinite") : pointLightLimit.ToString()) + ")";
		}
		case GraphicsSettingInt.PointLightShadows:
		{
			int pointLightShadowLimit = GraphicsSettingsManager.GetPointLightShadowLimit(value);
			return GetQualityText(value) + " (" + ((pointLightShadowLimit < 0) ? Localization.instance.Localize("$settings_infinite") : pointLightShadowLimit.ToString()) + ")";
		}
		case GraphicsSettingInt.SSAO:
			return value switch
			{
				0 => Localization.instance.Localize("[$hud_off]"), 
				1 => GetQualityText(0), 
				_ => GetQualityText(2), 
			};
		}
		return value.ToString();
	}
}
