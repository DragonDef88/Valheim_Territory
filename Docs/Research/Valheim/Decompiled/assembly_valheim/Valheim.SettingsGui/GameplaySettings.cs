using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Valheim.SettingsGui;

public class GameplaySettings : MonoBehaviour, ISettingsTab
{
	[Header("Gameplay")]
	[SerializeField]
	private TMP_Text m_language;

	[SerializeField]
	private TMP_Text m_communityTranslation;

	[SerializeField]
	private TMP_Text m_cloudStorageWarning;

	[SerializeField]
	private Toggle m_toggleRun;

	[SerializeField]
	private Toggle m_toggleAttackTowardsPlayerLookDir;

	[SerializeField]
	private Toggle m_showKeyHints;

	[SerializeField]
	private Toggle m_tutorialsEnabled;

	[SerializeField]
	private Button m_resetTutorial;

	[SerializeField]
	private Toggle m_reduceBGUsage;

	[SerializeField]
	private Toggle m_enableConsole;

	[SerializeField]
	private Slider m_autoBackups;

	[SerializeField]
	private TMP_Text m_autoBackupsText;

	[SerializeField]
	private Button m_deleteAccount;

	private string m_languageKey = "";

	private int m_showCloudWarningBackupThreshold = 4;

	private const string c_AttackTowardsPlayerLookDirString = "AttackTowardsPlayerLookDir";

	public event Action<string, int> SharedSettingChanged;

	public void OnTabOpen(Button backButton, Button okButton)
	{
		GuiUtils.SetNavigationDown((Selectable)(object)m_deleteAccount, (Selectable)(object)backButton);
		GuiUtils.SetNavigationUp((Selectable)(object)backButton, (Selectable)(object)m_deleteAccount);
		GuiUtils.SetNavigationUp((Selectable)(object)okButton, (Selectable)(object)m_deleteAccount);
	}

	public static void SetControllerSpecificFirstTimeSettings()
	{
		if (!PlatformPrefs.HasKey("AttackTowardsPlayerLookDir"))
		{
			PlatformPrefs.SetInt("AttackTowardsPlayerLookDir", ZInput.GamepadActive ? 1 : 0);
		}
	}

	public void Initialize()
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		SetControllerSpecificFirstTimeSettings();
		((Component)m_communityTranslation).gameObject.SetActive(false);
		m_languageKey = Localization.instance.GetSelectedLanguage();
		m_toggleRun.isOn = PlatformPrefs.GetInt("ToggleRun", ZInput.IsGamepadActive() ? 1 : 0) == 1;
		m_toggleAttackTowardsPlayerLookDir.isOn = PlatformPrefs.GetInt("AttackTowardsPlayerLookDir", 0) == 1;
		m_showKeyHints.isOn = PlatformPrefs.GetInt("KeyHints", 1) == 1;
		m_tutorialsEnabled.isOn = PlatformPrefs.GetInt("TutorialsEnabled", 1) == 1;
		m_reduceBGUsage.isOn = PlatformPrefs.GetInt("ReduceBackgroundUsage", 0) == 1;
		m_enableConsole.isOn = PlatformPrefs.GetInt("EnableConsole", Console.instance.IsConsoleEnabled() ? 1 : 0) == 1;
		m_autoBackups.value = PlatformPrefs.GetInt("AutoBackups", 4);
		UpdateLanguageText();
		OnAutoBackupsChanged();
		ZInput.instance.ChangeLayout(ZInput.InputLayout);
	}

	public void OnOkAsync(OkActionCompletedHandler okActionCompletedCallback)
	{
		PlatformPrefs.SetInt("KeyHints", m_showKeyHints.isOn ? 1 : 0);
		PlatformPrefs.SetInt("ToggleRun", m_toggleRun.isOn ? 1 : 0);
		PlatformPrefs.SetInt("AttackTowardsPlayerLookDir", m_toggleAttackTowardsPlayerLookDir.isOn ? 1 : 0);
		PlatformPrefs.SetInt("TutorialsEnabled", m_tutorialsEnabled.isOn ? 1 : 0);
		PlatformPrefs.SetInt("ReduceBackgroundUsage", m_reduceBGUsage.isOn ? 1 : 0);
		PlatformPrefs.SetInt("AutoBackups", (int)m_autoBackups.value);
		ZInput.ToggleRun = m_toggleRun.isOn;
		Raven.m_tutorialsEnabled = m_tutorialsEnabled.isOn;
		Settings.ReduceBackgroundUsage = m_reduceBGUsage.isOn;
		if ((Object)(object)Player.m_localPlayer != (Object)null)
		{
			Player.m_localPlayer.AttackTowardsPlayerLookDir = m_toggleAttackTowardsPlayerLookDir.isOn;
		}
		Localization.instance.SetLanguage(m_languageKey);
		okActionCompletedCallback?.Invoke();
	}

	private void SharedSettingsChanged(string setting, int value)
	{
		if (setting == "ToggleRun" && m_toggleRun.isOn != (value == 1))
		{
			m_toggleRun.isOn = value == 1;
		}
	}

	public void OnLanguageLeft()
	{
		m_languageKey = Localization.instance.GetPrevLanguage(m_languageKey);
		UpdateLanguageText();
	}

	public void OnLanguageRight()
	{
		m_languageKey = Localization.instance.GetNextLanguage(m_languageKey);
		UpdateLanguageText();
	}

	public void OnConsoleToggle()
	{
		PlatformPrefs.SetInt("EnableConsole", m_enableConsole.isOn ? 1 : 0);
		Object.FindAnyObjectByType<KeyboardMouseSettings>((FindObjectsInactive)1).SetConsoleEnabled(m_enableConsole.isOn);
		Console.SetConsoleEnabled(m_enableConsole.isOn);
	}

	private void UpdateLanguageText()
	{
		m_language.text = Localization.instance.Localize("$language_" + m_languageKey.ToLower());
		((Component)m_communityTranslation).gameObject.SetActive(m_language.text.Contains("*"));
	}

	public void OnResetTutorial()
	{
		Player.ResetSeenTutorials();
	}

	public void OnDeleteAccount()
	{
		if (!PlayFabManager.IsLoggedIn)
		{
			UnifiedPopup.Push(new WarningPopup("", "$settings_deleteplayfabaccount_not_logged_in", UnifiedPopup.Pop));
			return;
		}
		if ((Object)(object)ZNet.instance != (Object)null)
		{
			UnifiedPopup.Push(new WarningPopup("", "$settings_deleteplayfabaccount_ingamewarning", UnifiedPopup.Pop));
			return;
		}
		UnifiedPopup.Push(new YesNoPopup("$settings_deleteplayfabaccount", "$settings_deleteplayfabaccount_text", delegate
		{
			UnifiedPopup.Pop();
			PlayFabManager.instance.DeletePlayerTitleAccount();
		}, UnifiedPopup.Pop));
	}

	public void OnAutoBackupsChanged()
	{
		m_autoBackupsText.text = ((m_autoBackups.value == 1f) ? "0" : m_autoBackups.value.ToString());
		((Component)m_cloudStorageWarning).gameObject.SetActive(m_autoBackups.value > (float)m_showCloudWarningBackupThreshold);
	}

	public void OnToggleRunChanged()
	{
		this.SharedSettingChanged?.Invoke("ToggleRun", m_toggleRun.isOn ? 1 : 0);
	}
}
