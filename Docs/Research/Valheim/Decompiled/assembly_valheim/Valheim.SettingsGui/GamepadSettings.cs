using System;
using System.Collections.Generic;
using System.Linq;
using GUIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Valheim.SettingsGui;

public class GamepadSettings : MonoBehaviour, ISettingsTab
{
	[SerializeField]
	private UIGroupHandler m_groupHandler;

	[Header("Gamepad")]
	[SerializeField]
	private Toggle m_gamepadEnabled;

	[SerializeField]
	private Slider m_gamepadSensitivitySlider;

	[SerializeField]
	private TMP_Text m_cameraSensitivityText;

	[SerializeField]
	private Button m_leftLayoutButton;

	[SerializeField]
	private Button m_rightLayoutButton;

	[SerializeField]
	private GamepadMapController m_gamepadMapController;

	[SerializeField]
	private TMP_Text m_layoutText;

	[SerializeField]
	private Toggle m_swapTriggers;

	[SerializeField]
	private GuiDropdown m_glyphs;

	[SerializeField]
	private Toggle m_invertCameraY;

	[SerializeField]
	private Toggle m_invertCameraX;

	[SerializeField]
	private GameObject m_emptyToggleShift;

	private const string GlyphsXbox = "Xbox";

	private const string GlyphsPlaystation = "Playstation";

	private List<string> m_glyphOptions = new List<string> { "Xbox", "Playstation" };

	private GamepadGlyphs m_initialGlyph;

	private InputLayout m_initialLayout;

	private InputLayout m_currentLayout;

	private bool m_initialAlternativeGlyphs;

	private bool m_initialSwapTriggers;

	public event Action<string, int> SharedSettingChanged;

	public void OnTabOpen(Button backButton, Button okButton)
	{
		GuiUtils.SetNavigationDown((Selectable)(object)m_gamepadSensitivitySlider, (Selectable)(object)backButton);
		GuiUtils.SetNavigationUp((Selectable)(object)backButton, (Selectable)(object)m_gamepadSensitivitySlider);
		GuiUtils.SetNavigationUp((Selectable)(object)okButton, (Selectable)(object)m_gamepadSensitivitySlider);
	}

	public void Initialize()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		PlayerController.m_gamepadSens = PlatformPrefs.GetFloat("GamepadSensitivity", PlayerController.m_gamepadSens);
		PlayerController.m_invertCameraY = PlatformPrefs.GetInt("InvertCameraY", PlatformPrefs.GetInt("InvertMouse", 0)) == 1;
		PlayerController.m_invertCameraX = PlatformPrefs.GetInt("InvertCameraX", 0) == 1;
		m_initialLayout = ZInput.InputLayout;
		m_currentLayout = m_initialLayout;
		if (PlatformPrefs.GetInt("AltGlyphs", 99) != 99)
		{
			m_initialGlyph = (GamepadGlyphs)((PlatformPrefs.GetInt("AltGlyphs", 0) == 1) ? 2 : 0);
			PlatformPrefs.DeleteKey("AltGlyphs");
		}
		else
		{
			string[] names = Enum.GetNames(typeof(GamepadGlyphs));
			m_initialGlyph = (GamepadGlyphs)Array.IndexOf(names, PlatformPrefs.GetString("gamepad_glyphs", "Auto"));
		}
		ZInput.CurrentGlyph = m_initialGlyph;
		m_initialSwapTriggers = ZInput.SwapTriggers;
		m_gamepadEnabled.isOn = ZInput.IsGamepadEnabled();
		m_gamepadSensitivitySlider.value = PlayerController.m_gamepadSens;
		m_invertCameraY.isOn = PlayerController.m_invertCameraY;
		m_invertCameraX.isOn = PlayerController.m_invertCameraX;
		m_swapTriggers.isOn = m_initialSwapTriggers;
		((TMP_Dropdown)m_glyphs).ClearOptions();
		m_glyphOptions = Enum.GetNames(typeof(GamepadGlyphs)).ToList();
		((TMP_Dropdown)m_glyphs).AddOptions(m_glyphOptions);
		((TMP_Dropdown)m_glyphs).value = m_glyphOptions.IndexOf(((object)(GamepadGlyphs)(ref m_initialGlyph)).ToString());
		((UnityEvent<int>)(object)((TMP_Dropdown)m_glyphs).onValueChanged).RemoveListener((UnityAction<int>)OnGamepadGlyphChanged);
		((UnityEvent<int>)(object)((TMP_Dropdown)m_glyphs).onValueChanged).AddListener((UnityAction<int>)OnGamepadGlyphChanged);
		m_gamepadMapController.Show(m_initialLayout, (GamepadMapType)0);
		OnLayoutChanged();
		OnZInputLayoutChanged();
	}

	public void OnBack()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Dropdown)m_glyphs).value = Enum.GetNames(typeof(GamepadGlyphs)).ToList().IndexOf(((object)(GamepadGlyphs)(ref m_initialGlyph)).ToString());
		m_currentLayout = m_initialLayout;
		m_swapTriggers.isOn = m_initialSwapTriggers;
		OnLayoutChanged();
	}

	public void OnOkAsync(OkActionCompletedHandler okActionCompletedCallback)
	{
		PlatformPrefs.SetFloat("GamepadSensitivity", m_gamepadSensitivitySlider.value);
		PlatformPrefs.SetInt("InvertCameraY", m_invertCameraY.isOn ? 1 : 0);
		PlatformPrefs.SetInt("InvertCameraX", m_invertCameraX.isOn ? 1 : 0);
		PlatformPrefs.SetInt("SwapTriggers", m_swapTriggers.isOn ? 1 : 0);
		PlatformPrefs.SetString("gamepad_glyphs", ((TMP_Dropdown)m_glyphs).options[((TMP_Dropdown)m_glyphs).value].text);
		PlayerController.m_gamepadSens = m_gamepadSensitivitySlider.value;
		PlayerController.m_invertCameraY = m_invertCameraY.isOn;
		PlayerController.m_invertCameraX = m_invertCameraX.isOn;
		ZInput.SwapTriggers = m_swapTriggers.isOn;
		ZInput.SetGamepadEnabled(m_gamepadEnabled.isOn);
		okActionCompletedCallback?.Invoke();
	}

	public void OnGamepadSensitivityChanged()
	{
		m_cameraSensitivityText.text = Mathf.Round(m_gamepadSensitivitySlider.value * 100f) + "%";
	}

	public void OnLayoutLeft()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_currentLayout = GamepadMapController.PrevLayout(m_gamepadMapController.VisibleLayout);
		OnLayoutChanged();
	}

	public void OnLayoutRight()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_currentLayout = GamepadMapController.NextLayout(m_gamepadMapController.VisibleLayout);
		OnLayoutChanged();
	}

	public void OnLayoutChanged()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (ZInput.instance != null)
		{
			ZInput.SwapTriggers = m_swapTriggers.isOn;
			ZInput.instance.ChangeLayout(m_currentLayout);
		}
	}

	public void OnGamepadGlyphChanged(int newValue)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		ZInput.CurrentGlyph = (GamepadGlyphs)Enum.Parse(typeof(GamepadGlyphs), ((TMP_Dropdown)m_glyphs).options[((TMP_Dropdown)m_glyphs).value].text);
		OnLayoutChanged();
	}

	private void OnZInputLayoutChanged()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		m_gamepadMapController.Show(m_currentLayout, GamepadMapController.GetType(ZInput.CurrentGlyph, Settings.IsSteamRunningOnSteamDeck()));
		m_layoutText.text = Localization.instance.Localize(GamepadMapController.GetLayoutStringId(m_currentLayout));
	}

	private void OnEnable()
	{
		ZInput.OnInputLayoutChanged += OnZInputLayoutChanged;
	}

	private void OnDisable()
	{
		ZInput.OnInputLayoutChanged -= OnZInputLayoutChanged;
	}
}
