using System;
using System.Collections;
using System.Collections.Generic;
using GUIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Valheim.SettingsGui;

public class KeyboardMouseSettings : MonoBehaviour, ISettingsTab
{
	[SerializeField]
	private UIGroupHandler m_groupHandler;

	[Header("Controls")]
	[SerializeField]
	private Slider m_mouseSensitivitySlider;

	[SerializeField]
	private TMP_Text m_mouseSensitivityText;

	[SerializeField]
	private Toggle m_invertMouse;

	[SerializeField]
	private Toggle m_quickPieceSelect;

	[SerializeField]
	private GameObject m_bindDialog;

	[SerializeField]
	private List<KeySetting> m_keys = new List<KeySetting>();

	[SerializeField]
	private Button m_consoleKeyButton;

	[SerializeField]
	private Button m_bottomLeftKeyButton;

	[SerializeField]
	private Button m_bottomRightKeyButton;

	[SerializeField]
	private int m_keyRows = 13;

	[SerializeField]
	private int m_keyCols = 2;

	private GameObject m_selectedGameObject;

	private ScrollRectEnsureVisible m_scrollRectVisibilityManager;

	private float m_blockInputDelay;

	private KeySetting m_selectedKey;

	private static int m_mouseSensModifier = 1;

	public event Action<string, int> SharedSettingChanged;

	public void OnTabOpen(Button backButton, Button okButton)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Button val = ((((Component)m_consoleKeyButton).transform.parent.localScale.x > 0f) ? ((Component)m_consoleKeyButton).GetComponentInChildren<Button>() : ((Component)m_bottomLeftKeyButton).GetComponentInChildren<Button>());
		GuiUtils.SetNavigationDown((Selectable)(object)val, (Selectable)(object)backButton);
		GuiUtils.SetNavigationUp((Selectable)(object)backButton, (Selectable)(object)val);
		val = ((Component)m_bottomRightKeyButton).GetComponentInChildren<Button>();
		GuiUtils.SetNavigationDown((Selectable)(object)val, (Selectable)(object)okButton);
		GuiUtils.SetNavigationUp((Selectable)(object)okButton, (Selectable)(object)val);
	}

	public void Initialize()
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		PlayerController.m_mouseSens = PlatformPrefs.GetFloat("MouseSensitivity", PlayerController.m_mouseSens);
		m_mouseSensitivitySlider.value = PlatformPrefs.GetFloat("MouseSensitivity", PlayerController.m_mouseSens) / (float)m_mouseSensModifier;
		PlayerController.m_invertMouse = PlatformPrefs.GetInt("InvertMouse", 0) == 1;
		m_invertMouse.isOn = PlayerController.m_invertMouse;
		m_quickPieceSelect.isOn = PlatformPrefs.GetInt("QuickPieceSelect", 0) == 1;
		OnMouseSensitivityChanged();
		m_bindDialog.SetActive(false);
		SetupKeys();
		m_scrollRectVisibilityManager = ((Component)this).GetComponentInChildren<ScrollRectEnsureVisible>();
		m_selectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (((Component)m_consoleKeyButton).transform.parent.localScale.x > 0f)
		{
			SetConsoleEnabled(enabled: true);
		}
	}

	public void OnBack()
	{
		ZInput.instance.Load();
	}

	public void OnOkAsync(OkActionCompletedHandler okActionCompletedCallback)
	{
		PlatformPrefs.SetFloat("MouseSensitivity", m_mouseSensitivitySlider.value * (float)m_mouseSensModifier);
		PlatformPrefs.SetInt("InvertMouse", m_invertMouse.isOn ? 1 : 0);
		PlatformPrefs.SetInt("QuickPieceSelect", m_quickPieceSelect.isOn ? 1 : 0);
		PlayerController.m_mouseSens = m_mouseSensitivitySlider.value * (float)m_mouseSensModifier;
		PlayerController.m_invertMouse = m_invertMouse.isOn;
		okActionCompletedCallback?.Invoke();
	}

	private void Update()
	{
		if (!m_bindDialog.activeSelf)
		{
			if (ZInput.IsGamepadActive() && !((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)m_selectedGameObject))
			{
				m_selectedGameObject = EventSystem.current.currentSelectedGameObject;
				m_scrollRectVisibilityManager?.CenterOnItem((RectTransform)/*isinst with value type is only supported in some contexts*/);
			}
			return;
		}
		m_blockInputDelay -= Time.unscaledDeltaTime;
		if (!(m_blockInputDelay >= 0f))
		{
			if (InvalidKeyBind())
			{
				m_bindDialog.SetActive(false);
				InvalidKeybindPopup();
			}
			else if (!ZInput.s_IsRebindActive && m_bindDialog.activeSelf)
			{
				m_bindDialog.SetActive(false);
				UpdateBindings();
				((MonoBehaviour)this).StartCoroutine(DelayedKeyEnable());
			}
		}
	}

	private bool InvalidKeyBind()
	{
		KeyCode[] blockedButtons = m_selectedKey.m_blockedButtons;
		for (int i = 0; i < blockedButtons.Length; i++)
		{
			if (ZInput.GetKeyDown(blockedButtons[i], true))
			{
				return true;
			}
		}
		return false;
	}

	private void InvalidKeybindPopup()
	{
		string text = "$invalid_keybind_text";
		UnifiedPopup.Push(new WarningPopup("$invalid_keybind_header", text, delegate
		{
			UnifiedPopup.Pop();
			((MonoBehaviour)this).StartCoroutine(DelayedKeyEnable());
		}));
	}

	private IEnumerator DelayedKeyEnable()
	{
		if (!((Object)(object)((Component)this).gameObject == (Object)null))
		{
			yield return null;
			EnableKeys(enable: true);
			m_groupHandler.m_defaultElement = ((Component)m_mouseSensitivitySlider).gameObject;
			Settings.instance.BlockNavigation(block: false);
		}
	}

	private void OnDestroy()
	{
		foreach (KeySetting key in m_keys)
		{
			((UnityEventBase)((Button)((Component)key.m_keyTransform).GetComponentInChildren<GuiButton>()).onClick).RemoveAllListeners();
		}
		m_keys.Clear();
	}

	public void OnMouseSensitivityChanged()
	{
		m_mouseSensitivityText.text = Mathf.Round(m_mouseSensitivitySlider.value * 100f) + "%";
	}

	public void SetConsoleEnabled(bool enabled)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		int num = (enabled ? 1 : 0);
		((Component)((Component)m_consoleKeyButton).transform.parent).transform.localScale = new Vector3((float)num, (float)num, 1f);
		if (enabled)
		{
			GuiUtils.SetNavigationUp((Selectable)(object)m_consoleKeyButton, (Selectable)(object)m_bottomLeftKeyButton);
			GuiUtils.SetNavigationLeft((Selectable)(object)m_consoleKeyButton, (Selectable)null);
			GuiUtils.SetNavigationDown((Selectable)(object)m_bottomLeftKeyButton, (Selectable)(object)m_consoleKeyButton);
		}
	}

	private void SetupKeys()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (KeySetting key in m_keys)
		{
			GuiButton componentInChildren = ((Component)key.m_keyTransform).GetComponentInChildren<GuiButton>();
			((UnityEvent)((Button)componentInChildren).onClick).AddListener((UnityAction)delegate
			{
				OpenBindDialog(key);
			});
			if (num < m_keyRows - 1)
			{
				num3 = num2 * m_keyRows + num + 1;
				if (num3 < m_keys.Count)
				{
					GuiButton componentInChildren2 = ((Component)m_keys[num3].m_keyTransform).GetComponentInChildren<GuiButton>();
					GuiUtils.SetNavigationDown((Selectable)(object)componentInChildren, (Selectable)(object)componentInChildren2);
				}
			}
			if (num > 0)
			{
				num3 = num2 * m_keyRows + num - 1;
				GuiButton componentInChildren2 = ((Component)m_keys[num3].m_keyTransform).GetComponentInChildren<GuiButton>();
				GuiUtils.SetNavigationUp((Selectable)(object)componentInChildren, (Selectable)(object)componentInChildren2);
			}
			if (num2 > 0)
			{
				num3 = (num2 - 1) * m_keyRows + num;
				GuiButton componentInChildren2 = ((Component)m_keys[num3].m_keyTransform).GetComponentInChildren<GuiButton>();
				GuiUtils.SetNavigationLeft((Selectable)(object)componentInChildren, (Selectable)(object)componentInChildren2);
			}
			if (num2 < m_keyCols - 1)
			{
				num3 = (num2 + 1) * m_keyRows + num;
				if (num3 < m_keys.Count)
				{
					GuiButton componentInChildren2 = ((Component)m_keys[num3].m_keyTransform).GetComponentInChildren<GuiButton>();
					GuiUtils.SetNavigationRight((Selectable)(object)componentInChildren, (Selectable)(object)componentInChildren2);
				}
			}
			num++;
			if (num % m_keyRows == 0)
			{
				num = 0;
				num2++;
			}
		}
		UpdateBindings();
	}

	private void EnableKeys(bool enable)
	{
		foreach (KeySetting key in m_keys)
		{
			((Selectable)((Component)key.m_keyTransform).GetComponentInChildren<GuiButton>()).interactable = enable;
		}
	}

	private void OpenBindDialog(KeySetting key)
	{
		ZLog.Log((object)("Binding key " + key.m_keyName));
		m_selectedKey = key;
		Settings.instance.BlockNavigation(block: true);
		m_bindDialog.SetActive(true);
		m_blockInputDelay = 0.2f;
		m_groupHandler.m_defaultElement = EventSystem.current.currentSelectedGameObject;
		EventSystem.current.SetSelectedGameObject(m_bindDialog.gameObject);
		EnableKeys(enable: false);
		ZInput.instance.StartBindKey(key.m_keyName);
	}

	private void UpdateBindings()
	{
		foreach (KeySetting key in m_keys)
		{
			((Component)((Component)key.m_keyTransform).GetComponentInChildren<Button>()).GetComponentInChildren<TMP_Text>().text = Localization.instance.GetBoundKeyString(key.m_keyName, true);
		}
	}

	public void ResetBindings()
	{
		ZInput.instance.ResetToDefault("all");
		UpdateBindings();
	}

	public static void SetPlatformSpecificFirstTimeSettings()
	{
		if (!PlatformPrefs.HasKey("MouseSensitivity"))
		{
			PlatformPrefs.SetFloat("MouseSensitivity", (float)m_mouseSensModifier);
		}
	}
}
