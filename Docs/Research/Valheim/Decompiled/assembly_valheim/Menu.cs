using System;
using System.Collections;
using System.Collections.Generic;
using SoftReferenceableAssets.SceneManagement;
using Splatform;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valheim.SettingsGui;

public class Menu : MonoBehaviour
{
	public delegate void CloudStorageFullOkCallback();

	private enum CloseMenuState : byte
	{
		SettingsOpen,
		Blocked,
		CanBeClosed
	}

	private bool m_cloudStorageWarningShown;

	private List<CloudStorageFullOkCallback> cloudStorageFullOkCallbackList = new List<CloudStorageFullOkCallback>();

	[SerializeField]
	private GameObject CurrentPlayersPrefab;

	private GameObject m_currentPlayersInstance;

	public Button menuCurrentPlayersListButton;

	public Button menuInviteFriendsButton;

	private GameObject m_settingsInstance;

	public Button saveButton;

	public TMP_Text lastSaveText;

	private DateTime m_lastSavedDate = DateTime.MinValue;

	public RectTransform menuEntriesParent;

	private static Menu m_instance;

	public Transform m_root;

	public Transform m_menuDialog;

	public Transform m_quitDialog;

	public Transform m_logoutDialog;

	public GameObject m_cloudStorageWarning;

	public GameObject m_cloudStorageWarningNextSave;

	public GameObject m_settingsPrefab;

	public GameObject m_feedbackPrefab;

	public GameObject m_gamepadRoot;

	public GamepadMapController m_gamepadMapController;

	public SceneReference m_startScene;

	private int m_hiddenFrames;

	public GameObject m_skipButton;

	private int m_updateLocalizationTimer;

	private int m_manualSaveCooldownUntil;

	private const int ManualSavingCooldownTime = 60;

	private bool m_rebuildLayout;

	private bool m_saveOnLogout = true;

	private bool m_loadStartSceneOnLogout = true;

	private CloseMenuState m_closeMenuState = CloseMenuState.CanBeClosed;

	private Button m_firstMenuButton;

	public static bool ExceedCloudStorageTest;

	public static Menu instance => m_instance;

	public bool PlayerListActive
	{
		get
		{
			if ((Object)(object)m_currentPlayersInstance != (Object)null)
			{
				return m_currentPlayersInstance.activeSelf;
			}
			return false;
		}
	}

	private void Start()
	{
		m_instance = this;
		Hide();
		UpdateNavigation();
		m_rebuildLayout = true;
		if (ZNet.GetWorldIfIsHost() == null)
		{
			PlayerProfile.SavingFinished = (Action)Delegate.Combine(PlayerProfile.SavingFinished, new Action(SaveFinished));
		}
		else
		{
			ZNet.WorldSaveFinished = (Action)Delegate.Combine(ZNet.WorldSaveFinished, new Action(SaveFinished));
		}
	}

	private void HandleInputLayoutChanged()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		UpdateCursor();
		if (!ZInput.IsGamepadActive())
		{
			m_gamepadRoot.gameObject.SetActive(false);
			return;
		}
		m_gamepadRoot.gameObject.SetActive(true);
		m_gamepadMapController.Show(ZInput.InputLayout, GamepadMapController.GetType(ZInput.CurrentGlyph, Settings.IsSteamRunningOnSteamDeck()));
	}

	private void UpdateNavigation()
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		Button component = ((Component)m_menuDialog.Find("MenuEntries/Logout")).GetComponent<Button>();
		Button component2 = ((Component)m_menuDialog.Find("MenuEntries/Exit")).GetComponent<Button>();
		Button component3 = ((Component)m_menuDialog.Find("MenuEntries/Continue")).GetComponent<Button>();
		Button component4 = ((Component)m_menuDialog.Find("MenuEntries/Settings")).GetComponent<Button>();
		Button component5 = ((Component)m_menuDialog.Find("MenuEntries/SkipIntro")).GetComponent<Button>();
		m_firstMenuButton = component3;
		List<Button> list = new List<Button>();
		list.Add(component3);
		if (((Component)component5).gameObject.activeSelf)
		{
			list.Add(component5);
		}
		if (((Selectable)saveButton).interactable)
		{
			list.Add(saveButton);
		}
		if (((Component)menuCurrentPlayersListButton).gameObject.activeSelf)
		{
			list.Add(menuCurrentPlayersListButton);
		}
		if (((Component)menuInviteFriendsButton).gameObject.activeSelf)
		{
			list.Add(menuInviteFriendsButton);
		}
		list.Add(component4);
		list.Add(component);
		if (((Component)component2).gameObject.activeSelf)
		{
			list.Add(component2);
		}
		for (int i = 0; i < list.Count; i++)
		{
			Navigation navigation = ((Selectable)list[i]).navigation;
			if (i > 0)
			{
				((Navigation)(ref navigation)).selectOnUp = (Selectable)(object)list[i - 1];
			}
			else
			{
				((Navigation)(ref navigation)).selectOnUp = (Selectable)(object)list[list.Count - 1];
			}
			if (i < list.Count - 1)
			{
				((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)list[i + 1];
			}
			else
			{
				((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)list[0];
			}
			((Navigation)(ref navigation)).mode = (Mode)4;
			((Selectable)list[i]).navigation = navigation;
		}
	}

	private void OnDestroy()
	{
		PlayerProfile.SavingFinished = (Action)Delegate.Remove(PlayerProfile.SavingFinished, new Action(SaveFinished));
		ZNet.WorldSaveFinished = (Action)Delegate.Remove(ZNet.WorldSaveFinished, new Action(SaveFinished));
		ZInput.OnInputLayoutChanged -= HandleInputLayoutChanged;
	}

	private void SaveFinished()
	{
		m_lastSavedDate = DateTime.Now;
		m_rebuildLayout = true;
		if ((Object)(object)ZNet.instance != (Object)null && !ZNet.instance.IsSaving() && (!CanSaveToCloudStorage() || ExceedCloudStorageTest))
		{
			ShowCloudStorageLowNextSaveWarning();
		}
	}

	private static bool CanSaveToCloudStorage()
	{
		return SaveSystem.CanSaveToCloudStorage(ZNet.GetWorldIfIsHost(), Game.instance.GetPlayerProfile());
	}

	public void Show()
	{
		Gogan.LogEvent("Screen", "Enter", "Menu", 0L);
		((Component)m_root).gameObject.SetActive(true);
		((Component)m_menuDialog).gameObject.SetActive(true);
		m_skipButton.gameObject.SetActive(Game.instance.InIntro(includeQueued: true) || (Object.op_Implicit((Object)(object)Player.m_localPlayer) && Player.m_localPlayer.InIntro()));
		((Component)m_logoutDialog).gameObject.SetActive(false);
		((Component)m_quitDialog).gameObject.SetActive(false);
		bool active = !ZNet.IsSinglePlayer && ZNet.instance.IsServer() && PlatformManager.DistributionPlatform.UIProvider.InviteUsers != null && PlatformManager.DistributionPlatform.UIProvider.InviteUsers.CanInviteUsers;
		((Component)menuCurrentPlayersListButton).gameObject.SetActive(!ZNet.IsSinglePlayer);
		((Component)menuInviteFriendsButton).gameObject.SetActive(active);
		((Component)saveButton).gameObject.SetActive(true);
		((Component)lastSaveText).gameObject.SetActive(m_lastSavedDate > DateTime.MinValue);
		if ((Object)(object)Player.m_localPlayer != (Object)null)
		{
			Game.Pause();
		}
		if (Chat.instance.IsChatDialogWindowVisible())
		{
			Chat.instance.Hide();
		}
		JoinCode.Show();
		UpdateNavigation();
		m_rebuildLayout = true;
		m_saveOnLogout = true;
		m_loadStartSceneOnLogout = true;
		ZInput.WorkaroundEnabled = false;
		ZInput.OnInputLayoutChanged -= HandleInputLayoutChanged;
		ZInput.OnInputLayoutChanged += HandleInputLayoutChanged;
		HandleInputLayoutChanged();
	}

	private IEnumerator SelectEntry(GameObject entry)
	{
		yield return null;
		yield return null;
		EventSystem.current.SetSelectedGameObject(entry);
		UpdateCursor();
	}

	public void Hide()
	{
		((Component)m_root).gameObject.SetActive(false);
		JoinCode.Hide();
		Game.Unpause();
		if (ZInput.IsGamepadActive())
		{
			PlayerController.SetTakeInputDelay(0.1f);
		}
		ZInput.OnInputLayoutChanged -= UpdateCursor;
		ZInput.WorkaroundEnabled = true;
	}

	private void UpdateCursor()
	{
		Cursor.lockState = (CursorLockMode)(!ZInput.IsMouseActive());
		Cursor.visible = ZInput.IsMouseActive();
	}

	public static bool IsVisible()
	{
		if ((Object)(object)m_instance == (Object)null)
		{
			return false;
		}
		if (m_instance.m_hiddenFrames > 2)
		{
			return UnifiedPopup.WasVisibleThisFrame();
		}
		return true;
	}

	public static bool IsActive()
	{
		if ((Object)(object)m_instance == (Object)null)
		{
			return false;
		}
		if (!((Component)m_instance.m_root).gameObject.activeSelf)
		{
			return UnifiedPopup.WasVisibleThisFrame();
		}
		return true;
	}

	private void Update()
	{
		if (Game.instance.IsShuttingDown())
		{
			Hide();
			return;
		}
		if (((Component)m_root).gameObject.activeSelf)
		{
			m_hiddenFrames = 0;
			if ((ZInput.GetKeyDown((KeyCode)27, true) || (ZInput.GetButtonDown("JoyMenu") && (!ZInput.GetButton("JoyLTrigger") || !ZInput.GetButton("JoyLBumper")) && !ZInput.GetButton("JoyAltKeys")) || ZInput.GetButtonDown("JoyButtonB")) && !Object.op_Implicit((Object)(object)m_settingsInstance) && !Object.op_Implicit((Object)(object)m_currentPlayersInstance) && !Feedback.IsVisible() && !UnifiedPopup.IsVisible())
			{
				if (((Component)m_quitDialog).gameObject.activeSelf)
				{
					OnQuitNo();
				}
				else if (((Component)m_logoutDialog).gameObject.activeSelf)
				{
					OnLogoutNo();
				}
				else
				{
					if (m_closeMenuState == CloseMenuState.SettingsOpen && ZInput.GetButtonDown("JoyButtonB"))
					{
						m_closeMenuState = CloseMenuState.Blocked;
					}
					if (m_closeMenuState != CloseMenuState.Blocked)
					{
						Hide();
					}
				}
			}
			if (m_closeMenuState == CloseMenuState.Blocked && ZInput.GetButtonUp("JoyButtonB"))
			{
				m_closeMenuState = CloseMenuState.CanBeClosed;
			}
			if (ZInput.IsGamepadActive() && ((Component)this).gameObject.activeInHierarchy && (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)null && (Object)(object)m_firstMenuButton != (Object)null)
			{
				((MonoBehaviour)this).StartCoroutine(SelectEntry(((Component)m_firstMenuButton).gameObject));
			}
			if (m_lastSavedDate > DateTime.MinValue)
			{
				int minutes = (DateTime.Now - m_lastSavedDate).Minutes;
				string text = minutes.ToString();
				if (minutes < 1)
				{
					text = "<1";
				}
				lastSaveText.text = Localization.instance.Localize("$menu_manualsavetime", new string[1] { text });
			}
			if ((((Selectable)saveButton).interactable && (float)m_manualSaveCooldownUntil > Time.unscaledTime) || (!((Selectable)saveButton).interactable && (float)m_manualSaveCooldownUntil < Time.unscaledTime))
			{
				((Selectable)saveButton).interactable = (float)m_manualSaveCooldownUntil < Time.unscaledTime;
				UpdateNavigation();
			}
			if (m_rebuildLayout)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(menuEntriesParent);
				((Component)lastSaveText).gameObject.SetActive(m_lastSavedDate > DateTime.MinValue);
				m_rebuildLayout = false;
				((MonoBehaviour)this).StartCoroutine(SelectEntry(((Component)m_firstMenuButton).gameObject));
			}
		}
		else
		{
			m_hiddenFrames++;
			bool flag = !InventoryGui.IsVisible() && !Minimap.IsOpen() && !Console.IsVisible() && !TextInput.IsVisible() && !ZNet.instance.InPasswordDialog() && !ZNet.instance.InConnectingScreen() && !StoreGui.IsVisible() && !Hud.IsPieceSelectionVisible() && !UnifiedPopup.IsVisible() && !PlayerCustomizaton.IsBarberGuiVisible() && !Hud.InRadial();
			if ((ZInput.GetKeyDown((KeyCode)27, true) || (ZInput.GetButtonDown("JoyMenu") && (!ZInput.GetButton("JoyLTrigger") || !ZInput.GetButton("JoyLBumper")) && !ZInput.GetButton("JoyAltKeys"))) && flag && !Chat.instance.m_wasFocused)
			{
				Show();
			}
		}
		if (m_updateLocalizationTimer > 30)
		{
			Localization.instance.ReLocalizeVisible(((Component)this).transform);
			m_updateLocalizationTimer = 0;
		}
		else
		{
			m_updateLocalizationTimer++;
		}
	}

	public void OnSkip()
	{
		Game.instance.SkipIntro();
		Hide();
	}

	public void OnSettings()
	{
		Gogan.LogEvent("Screen", "Enter", "Settings", 0L);
		m_settingsInstance = Object.Instantiate<GameObject>(m_settingsPrefab, ((Component)this).transform);
		m_closeMenuState = CloseMenuState.SettingsOpen;
	}

	public void OnQuit()
	{
		((Component)m_quitDialog).gameObject.SetActive(true);
		((Component)m_menuDialog).gameObject.SetActive(false);
	}

	public void OnCurrentPlayers()
	{
		if ((Object)(object)m_currentPlayersInstance == (Object)null)
		{
			m_currentPlayersInstance = Object.Instantiate<GameObject>(CurrentPlayersPrefab, ((Component)this).transform);
		}
		else
		{
			m_currentPlayersInstance.SetActive(true);
		}
	}

	public void InviteFriends()
	{
		InviteUsersUI inviteUsers = PlatformManager.DistributionPlatform.UIProvider.InviteUsers;
		if (inviteUsers != null && inviteUsers.CanInviteUsers)
		{
			inviteUsers.Open();
		}
	}

	public void OnManualSave()
	{
		if ((float)m_manualSaveCooldownUntil >= Time.unscaledTime)
		{
			return;
		}
		if (!CanSaveToCloudStorage())
		{
			((Component)m_logoutDialog).gameObject.SetActive(false);
			ShowCloudStorageFullWarning(Logout);
		}
		else
		{
			if (!((Object)(object)ZNet.instance != (Object)null))
			{
				return;
			}
			if (ZNet.IsSinglePlayer || ZNet.instance.GetPeerConnections() < 1)
			{
				if (!ZNet.instance.EnoughDiskSpaceAvailable(out var _))
				{
					return;
				}
				Game.instance.SavePlayerProfile(setLogoutPoint: true);
				ZNet.instance.Save(sync: true, saveOtherPlayerProfiles: false, waitForNextFrame: true);
			}
			else
			{
				ZNet.instance.SaveWorldAndPlayerProfiles();
			}
			m_manualSaveCooldownUntil = (int)Time.unscaledTime + 60;
			m_saveOnLogout = (Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer();
		}
	}

	public void OnQuitYes()
	{
		ZNet.instance.EnoughDiskSpaceAvailable(out var exitGamePopupShown, exitGamePrompt: true, delegate(bool exit)
		{
			if (exit)
			{
				QuitGame();
			}
		});
		if (exitGamePopupShown)
		{
			return;
		}
		if (!CanSaveToCloudStorage())
		{
			((Component)m_quitDialog).gameObject.SetActive(false);
			if (!FileHelpers.LocalStorageSupported)
			{
				m_saveOnLogout = false;
			}
			ShowCloudStorageFullWarning(QuitGame);
		}
		else
		{
			QuitGame();
		}
	}

	private void QuitGame()
	{
		Gogan.LogEvent("Game", "Quit", "", 0L);
		Application.Quit();
	}

	public void OnQuitNo()
	{
		((Component)m_quitDialog).gameObject.SetActive(false);
		((Component)m_menuDialog).gameObject.SetActive(true);
	}

	public void OnLogout()
	{
		((Component)m_menuDialog).gameObject.SetActive(false);
		((Component)m_logoutDialog).gameObject.SetActive(true);
	}

	public void OnLogoutYes()
	{
		if (m_saveOnLogout && !CanSaveToCloudStorage())
		{
			((Component)m_logoutDialog).gameObject.SetActive(false);
			if (!FileHelpers.LocalStorageSupported)
			{
				m_saveOnLogout = false;
			}
			ShowCloudStorageFullWarning(Logout);
		}
		else
		{
			Logout();
		}
	}

	public void Logout()
	{
		Gogan.LogEvent("Game", "LogOut", "", 0L);
		Game.instance.Logout(m_saveOnLogout, m_loadStartSceneOnLogout);
	}

	public void OnLogoutNo()
	{
		((Component)m_logoutDialog).gameObject.SetActive(false);
		((Component)m_menuDialog).gameObject.SetActive(true);
	}

	public void OnClose()
	{
		Gogan.LogEvent("Screen", "Exit", "Menu", 0L);
		Hide();
	}

	public void OnButtonFeedback()
	{
		Object.Instantiate<GameObject>(m_feedbackPrefab, ((Component)this).transform);
	}

	public void ShowCloudStorageFullWarning(CloudStorageFullOkCallback okCallback)
	{
		if (m_cloudStorageWarningShown)
		{
			okCallback?.Invoke();
			return;
		}
		if (okCallback != null)
		{
			cloudStorageFullOkCallbackList.Add(okCallback);
		}
		m_cloudStorageWarning.SetActive(true);
	}

	public void OnCloudStorageFullWarningOk()
	{
		int count = cloudStorageFullOkCallbackList.Count;
		while (count-- > 0)
		{
			cloudStorageFullOkCallbackList[count]();
		}
		cloudStorageFullOkCallbackList.Clear();
		m_cloudStorageWarningShown = true;
		m_cloudStorageWarning.SetActive(false);
	}

	public void ShowCloudStorageLowNextSaveWarning()
	{
		m_saveOnLogout = false;
		m_loadStartSceneOnLogout = false;
		Logout();
		m_cloudStorageWarningNextSave.SetActive(true);
	}

	public void OnCloudStorageLowNextSaveWarningOk()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SceneManager.LoadScene(m_startScene, (LoadSceneMode)0);
	}
}
