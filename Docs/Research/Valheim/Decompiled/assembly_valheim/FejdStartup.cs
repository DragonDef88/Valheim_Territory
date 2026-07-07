using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using GUIFramework;
using NetworkingUtils;
using SoftReferenceableAssets.SceneManagement;
using Splatform;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class FejdStartup : MonoBehaviour
{
	private delegate void ContinueAction();

	private Vector3 camSpeed = Vector3.zero;

	private Vector3 camRotSpeed = Vector3.zero;

	private const int maxRetries = 50;

	private static int retries = 0;

	private static FejdStartup m_instance;

	[Header("Start")]
	public Animator m_menuAnimator;

	public GameObject m_worldVersionPanel;

	public GameObject m_playerVersionPanel;

	public GameObject m_newGameVersionPanel;

	public GameObject m_connectionFailedPanel;

	public TMP_Text m_connectionFailedError;

	public TMP_Text m_newVersionName;

	public GameObject m_loading;

	public GameObject m_pleaseWait;

	public TMP_Text m_versionLabel;

	public GameObject m_mainMenu;

	public GameObject m_ndaPanel;

	public GameObject m_betaText;

	public GameObject m_moddedText;

	public Scrollbar m_patchLogScroll;

	public GameObject m_characterSelectScreen;

	public GameObject m_selectCharacterPanel;

	public GameObject m_newCharacterPanel;

	public GameObject m_creditsPanel;

	public GameObject m_startGamePanel;

	public GameObject m_createWorldPanel;

	public ServerOptionsGUI m_serverOptions;

	public Button m_serverOptionsButton;

	public GameObject m_menuList;

	private Button[] m_menuButtons;

	private Button m_menuSelectedButton;

	public RectTransform m_creditsList;

	public float m_creditsSpeed = 100f;

	public SceneReference m_startScene;

	public SceneReference m_mainScene;

	[Header("Camera")]
	public GameObject m_mainCamera;

	public Transform m_cameraMarkerStart;

	public Transform m_cameraMarkerMain;

	public Transform m_cameraMarkerCharacter;

	public Transform m_cameraMarkerCredits;

	public Transform m_cameraMarkerGame;

	public Transform m_cameraMarkerSaves;

	public float m_cameraMoveSpeed = 1.5f;

	public float m_cameraMoveSpeedStart = 1.5f;

	[Header("Join")]
	public GameObject m_serverListPanel;

	public Toggle m_publicServerToggle;

	public Toggle m_openServerToggle;

	public Toggle m_crossplayServerToggle;

	public Color m_toggleColor = new Color(1f, 0.6308316f, 0.2352941f);

	public GuiInputField m_serverPassword;

	public TMP_Text m_passwordError;

	public int m_minimumPasswordLength = 5;

	public float m_characterRotateSpeed = 4f;

	public float m_characterRotateSpeedGamepad = 200f;

	public int m_joinHostPort = 2456;

	[Header("World")]
	public GameObject m_worldListPanel;

	public RectTransform m_worldListRoot;

	public GameObject m_worldListElement;

	public ScrollRectEnsureVisible m_worldListEnsureVisible;

	public float m_worldListElementStep = 28f;

	public TextMeshProUGUI m_worldSourceInfo;

	public GameObject m_worldSourceInfoPanel;

	public GuiInputField m_newWorldName;

	public GuiInputField m_newWorldSeed;

	public Button m_newWorldDone;

	public Button m_worldStart;

	public Button m_worldRemove;

	public GameObject m_removeWorldDialog;

	public TMP_Text m_removeWorldName;

	public GameObject m_removeCharacterDialog;

	public TMP_Text m_removeCharacterName;

	public RectTransform m_tooltipAnchor;

	public RectTransform m_tooltipSecondaryAnchor;

	[Header("Character selection")]
	public Button m_csStartButton;

	public Button m_csNewBigButton;

	public Button m_csNewButton;

	public Button m_csRemoveButton;

	public Button m_csLeftButton;

	public Button m_csRightButton;

	public Button m_csNewCharacterDone;

	public Button m_csNewCharacterCancel;

	public GameObject m_newCharacterError;

	public TMP_Text m_csName;

	public TMP_Text m_csFileSource;

	public TMP_Text m_csSourceInfo;

	public GuiInputField m_csNewCharacterName;

	[Header("Misc")]
	public Transform m_characterPreviewPoint;

	public GameObject m_playerPrefab;

	public GameObject m_objectDBPrefab;

	public GameObject m_settingsPrefab;

	public GameObject m_consolePrefab;

	public GameObject m_feedbackPrefab;

	public GameObject m_changeEffectPrefab;

	public ManageSavesMenu m_manageSavesMenu;

	public GameObject m_cloudStorageWarningNextSave;

	private GameObject m_settingsPopup;

	private string m_downloadUrl = "";

	[TextArea]
	public string m_versionXmlUrl = "https://dl.dropboxusercontent.com/s/5ibm05oelbqt8zq/fejdversion.xml?dl=0";

	private World m_world;

	private bool m_startingWorld;

	private ServerJoinData m_joinServer = ServerJoinData.None;

	private ServerJoinData m_queuedJoinServer = ServerJoinData.None;

	private float m_worldListBaseSize;

	private List<PlayerProfile> m_profiles;

	private int m_profileIndex;

	private string m_tempRemoveCharacterName = "";

	private FileSource m_tempRemoveCharacterSource;

	private int m_tempRemoveCharacterIndex = -1;

	private BackgroundWorker m_moveFileWorker;

	private List<GameObject> m_worldListElements = new List<GameObject>();

	private List<World> m_worlds;

	private GameObject m_playerInstance;

	private static bool m_firstStartup = true;

	private bool m_autoConnectionInProgress;

	public static Action HandlePendingInvite;

	public static Action ResetPendingInvite;

	public static Action<Privilege> ResolvePrivilege;

	private static GameObject s_monoUpdaters = null;

	public static FejdStartup instance => m_instance;

	public static string InstanceId { get; private set; } = null;


	public static string ServerPassword { get; private set; } = null;


	private event Action m_cliUpdateAction;

	private void Awake()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		m_instance = this;
		ParseArguments();
		((Component)m_crossplayServerToggle).gameObject.SetActive(true);
		if (!AwakePlatforms())
		{
			return;
		}
		ZLog.Log((object)("Valheim version: " + Version.GetVersionString() + " (network version " + 36u + ")"));
		Settings.ApplyStartupSettings();
		WorldGenerator.Initialize(World.GetMenuWorld());
		if (!Object.op_Implicit((Object)(object)Console.instance))
		{
			Object.Instantiate<GameObject>(m_consolePrefab);
		}
		m_mainCamera.transform.position = ((Component)m_cameraMarkerMain).transform.position;
		m_mainCamera.transform.rotation = ((Component)m_cameraMarkerMain).transform.rotation;
		RenderingThreadingMode renderingThreadingMode = SystemInfo.renderingThreadingMode;
		ZLog.Log((object)("Render threading mode:" + ((object)(RenderingThreadingMode)(ref renderingThreadingMode)).ToString()));
		Gogan.StartSession();
		Gogan.LogEvent("Game", "Version", Version.GetVersionString(), 0L);
		Gogan.LogEvent("Game", "SteamID", SteamManager.APP_ID.ToString(), 0L);
		Gogan.LogEvent("Screen", "Enter", "StartMenu", 0L);
		if (Settings.IsSteamRunningOnSteamDeck())
		{
			Transform val = m_mainMenu.transform.Find("showlog");
			if (val != null)
			{
				((Component)val).gameObject.SetActive(false);
			}
		}
		m_menuButtons = m_menuList.GetComponentsInChildren<Button>();
		TabHandler[] enabledComponentsInChildren = Utils.GetEnabledComponentsInChildren<TabHandler>(m_startGamePanel.gameObject);
		TabHandler[] array = enabledComponentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			((Behaviour)array[i]).enabled = false;
		}
		m_startGamePanel.gameObject.SetActive(true);
		((Component)m_serverOptions).gameObject.SetActive(true);
		((Component)m_serverOptions).gameObject.SetActive(false);
		m_startGamePanel.gameObject.SetActive(false);
		array = enabledComponentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			((Behaviour)array[i]).enabled = true;
		}
		MultiBackendMatchmaking.Hold();
		Game.Unpause();
		Time.timeScale = 1f;
		ZInput.Initialize();
		ZInput.WorkaroundEnabled = false;
		ZInput.OnInputLayoutChanged += UpdateCursor;
		UpdateCursor();
	}

	public static bool AwakePlatforms()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		if ((Object)(object)s_monoUpdaters == (Object)null)
		{
			s_monoUpdaters = new GameObject();
			s_monoUpdaters.AddComponent<MonoUpdaters>();
			Object.DontDestroyOnLoad((Object)(object)s_monoUpdaters);
		}
		if (!AwakeSteam() || !AwakePlayFab())
		{
			ZLog.LogError((object)"Awake of network backend failed");
			return false;
		}
		return true;
	}

	private static bool AwakePlayFab()
	{
		PlayFabManager.Initialize();
		return true;
	}

	private static bool AwakeSteam()
	{
		if (!InitializeSteam())
		{
			return false;
		}
		return true;
	}

	private void OnDestroy()
	{
		SaveSystem.ClearWorldListCache(reload: false);
		m_instance = null;
		ZInput.OnInputLayoutChanged -= UpdateCursor;
		Localization.OnLanguageChange = (Action)Delegate.Remove(Localization.OnLanguageChange, new Action(OnLanguageChange));
		MultiBackendMatchmaking.Release();
	}

	private void OnApplicationQuit()
	{
		HeightmapBuilder.instance.Dispose();
	}

	private void Start()
	{
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Invalid comparison between Unknown and I4
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		SetupGui();
		SetupObjectDB();
		((UnityEvent<bool>)(object)m_openServerToggle.onValueChanged).AddListener((UnityAction<bool>)OnOpenServerToggleClicked);
		MusicMan.instance.Reset();
		MusicMan.instance.TriggerMusic("menu");
		ShowConnectError();
		ZSteamMatchmaking.Initialize();
		if (m_firstStartup)
		{
			HandleStartupJoin();
		}
		m_menuAnimator.SetBool("FirstStartup", m_firstStartup);
		m_firstStartup = false;
		string @string = PlatformPrefs.GetString("profile", "");
		if (@string.Length > 0)
		{
			SetSelectedProfile(@string);
		}
		else
		{
			m_profiles = SaveSystem.GetAllPlayerProfiles();
			if (m_profiles.Count > 0)
			{
				SetSelectedProfile(m_profiles[0].GetFilename());
			}
			else
			{
				UpdateCharacterList();
			}
		}
		CensorShittyWords.UGCPopupShown = (Action)Delegate.Remove(CensorShittyWords.UGCPopupShown, new Action(OnUGCPopupShown));
		CensorShittyWords.UGCPopupShown = (Action)Delegate.Combine(CensorShittyWords.UGCPopupShown, new Action(OnUGCPopupShown));
		SaveSystem.ClearWorldListCache(reload: true);
		if ((int)Application.platform == 1 || (int)Application.platform == 0)
		{
			CustomLogger.SetupSymbolicLink();
		}
		Player.m_debugMode = false;
	}

	private void SetupGui()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		HideAll();
		m_mainMenu.SetActive(true);
		if (SteamManager.APP_ID == 1223920)
		{
			m_betaText.SetActive(true);
			if (!Debug.isDebugBuild && !AcceptedNDA())
			{
				m_ndaPanel.SetActive(true);
				m_mainMenu.SetActive(false);
			}
		}
		m_moddedText.SetActive(Game.isModded);
		Rect rect = m_worldListRoot.rect;
		m_worldListBaseSize = ((Rect)(ref rect)).height;
		m_versionLabel.text = $"Version {Version.GetVersionString()} (n-{36u})";
		Localization.instance.Localize(((Component)this).transform);
		Localization.OnLanguageChange = (Action)Delegate.Combine(Localization.OnLanguageChange, new Action(OnLanguageChange));
	}

	private void HideAll()
	{
		m_worldVersionPanel.SetActive(false);
		m_playerVersionPanel.SetActive(false);
		m_newGameVersionPanel.SetActive(false);
		m_loading.SetActive(false);
		m_pleaseWait.SetActive(false);
		m_characterSelectScreen.SetActive(false);
		m_creditsPanel.SetActive(false);
		m_startGamePanel.SetActive(false);
		m_createWorldPanel.SetActive(false);
		((Component)m_serverOptions).gameObject.SetActive(false);
		m_mainMenu.SetActive(false);
		m_ndaPanel.SetActive(false);
		m_betaText.SetActive(false);
	}

	public static bool InitializeSteam()
	{
		if (SteamManager.Initialize())
		{
			string personaName = SteamFriends.GetPersonaName();
			ZLog.Log((object)("Steam initialized, persona:" + personaName));
			return true;
		}
		ZLog.LogError((object)"Steam is not initialized");
		Application.Quit();
		return false;
	}

	private void HandleStartupJoin()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		CSteamID lobbyID = default(CSteamID);
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string text = commandLineArgs[i];
			if (text == "+connect" && i < commandLineArgs.Length - 1)
			{
				string text2 = commandLineArgs[i + 1];
				ZLog.Log((object)("JOIN " + text2));
				ZSteamMatchmaking.instance.QueueServerJoin(text2);
			}
			else if (text == "+connect_lobby" && i < commandLineArgs.Length - 1)
			{
				string s = commandLineArgs[i + 1];
				((CSteamID)(ref lobbyID))._002Ector(ulong.Parse(s));
				ZSteamMatchmaking.instance.QueueLobbyJoin(lobbyID);
			}
		}
	}

	private void ParseArguments()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string text = commandLineArgs[i];
			if (text == "-console")
			{
				Console.SetConsoleEnabledForThisSession();
			}
			else if (text == "-joincode" && commandLineArgs.Length > i + 1)
			{
				string joinCode = commandLineArgs[i + 1];
				Action autoJoin = null;
				autoJoin = delegate
				{
					m_cliUpdateAction -= autoJoin;
					AutoJoinServer(joinCode);
				};
				m_cliUpdateAction += autoJoin;
			}
			else if (text == "-password" && commandLineArgs.Length > i + 1)
			{
				ServerPassword = commandLineArgs[i + 1];
			}
		}
	}

	private void AutoJoinServer(string joinCode)
	{
		if ((Object)(object)PlayFabManager.instance == (Object)null)
		{
			return;
		}
		PlayFabManager.instance.LoginFinished += delegate(LoginType loginType)
		{
			if (!m_autoConnectionInProgress)
			{
				m_autoConnectionInProgress = true;
				if (loginType != 0)
				{
					ZLog.LogError((object)"Failed to login to PlayFab");
					Application.Quit();
				}
				ZPlayFabMatchmaking.ResolveJoinCode(joinCode, delegate(PlayFabMatchmakingServerData serverData)
				{
					m_joinServer = new ServerJoinData(new ServerJoinDataPlayFabUser(serverData.remotePlayerId));
					JoinServer();
				}, delegate(ZPLayFabMatchmakingFailReason failReason)
				{
					ZLog.LogError((object)("Failed to resolve joincode: " + failReason));
					Application.Quit();
				});
			}
		};
	}

	private bool ParseServerArguments()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string text = "Dedicated";
		string password = "";
		string text2 = "";
		int num = 2456;
		bool flag = true;
		ZNet.m_backupCount = 4;
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string text3 = commandLineArgs[i].ToLower();
			switch (text3)
			{
			case "-world":
			{
				string text7 = commandLineArgs[i + 1];
				if (text7 != "")
				{
					text = text7;
				}
				i++;
				continue;
			}
			case "-name":
			{
				string text6 = commandLineArgs[i + 1];
				if (text6 != "")
				{
					text2 = text6;
				}
				i++;
				continue;
			}
			case "-port":
			{
				string text4 = commandLineArgs[i + 1];
				if (text4 != "")
				{
					num = int.Parse(text4);
				}
				i++;
				continue;
			}
			case "-password":
				password = commandLineArgs[i + 1];
				i++;
				continue;
			case "-savedir":
			{
				string text5 = commandLineArgs[i + 1];
				Utils.SetSaveDataPath(text5);
				ZLog.Log((object)("Setting -savedir to: " + text5));
				i++;
				continue;
			}
			case "-public":
			{
				string text8 = commandLineArgs[i + 1];
				if (text8 != "")
				{
					flag = text8 == "1";
				}
				i++;
				continue;
			}
			}
			int result;
			int result2;
			int result3;
			int result4;
			if (text3.ToLower() == "-logfile")
			{
				ZLog.Log((object)("Setting -logfile to: " + commandLineArgs[i + 1]));
			}
			else if (text3 == "-crossplay")
			{
				ZNet.m_onlineBackend = OnlineBackendType.PlayFab;
			}
			else if (text3 == "-instanceid" && commandLineArgs.Length > i + 1)
			{
				InstanceId = commandLineArgs[i + 1];
				i++;
			}
			else if (text3.ToLower() == "-backups" && int.TryParse(commandLineArgs[i + 1], out result))
			{
				ZNet.m_backupCount = result;
			}
			else if (text3 == "-backupshort" && int.TryParse(commandLineArgs[i + 1], out result2))
			{
				ZNet.m_backupShort = Mathf.Max(5, result2);
			}
			else if (text3 == "-backuplong" && int.TryParse(commandLineArgs[i + 1], out result3))
			{
				ZNet.m_backupLong = Mathf.Max(5, result3);
			}
			else if (text3 == "-saveinterval" && int.TryParse(commandLineArgs[i + 1], out result4))
			{
				Game.m_saveInterval = Mathf.Max(5, result4);
			}
		}
		if (text2 == "")
		{
			text2 = text;
		}
		World createWorld = World.GetCreateWorld(text, (FileSource)1);
		if (!Object.op_Implicit((Object)(object)ServerOptionsGUI.m_instance))
		{
			((Component)Object.Instantiate<ServerOptionsGUI>(m_serverOptions)).gameObject.SetActive(true);
		}
		for (int j = 0; j < commandLineArgs.Length; j++)
		{
			string text9 = commandLineArgs[j].ToLower();
			if (text9 == "-resetmodifiers")
			{
				createWorld.m_startingGlobalKeys.Clear();
				createWorld.m_startingKeysChanged = true;
				ZLog.Log((object)"Resetting world modifiers");
			}
			else if (text9 == "-preset" && commandLineArgs.Length > j + 1)
			{
				string text10 = commandLineArgs[j + 1];
				if (Enum.TryParse<WorldPresets>(text10, ignoreCase: true, out var result5))
				{
					createWorld.m_startingGlobalKeys.Clear();
					createWorld.m_startingKeysChanged = true;
					ServerOptionsGUI.m_instance.ReadKeys(createWorld);
					ServerOptionsGUI.m_instance.SetPreset(createWorld, result5);
					ServerOptionsGUI.m_instance.SetKeys(createWorld);
					ZLog.Log((object)("Setting world modifier preset: " + text10));
				}
				else
				{
					ZLog.LogError((object)("Could not parse '" + text10 + "' as a world modifier preset."));
				}
			}
			else if (text9 == "-modifier" && commandLineArgs.Length > j + 2)
			{
				string text11 = commandLineArgs[j + 1];
				string text12 = commandLineArgs[j + 2];
				if (Enum.TryParse<WorldModifiers>(text11, ignoreCase: true, out var result6) && Enum.TryParse<WorldModifierOption>(text12, ignoreCase: true, out var result7))
				{
					ServerOptionsGUI.m_instance.ReadKeys(createWorld);
					ServerOptionsGUI.m_instance.SetPreset(createWorld, result6, result7);
					ServerOptionsGUI.m_instance.SetKeys(createWorld);
					ZLog.Log((object)("Setting world modifier: " + text11 + "->" + text12));
				}
				else
				{
					ZLog.LogError((object)("Could not parse '" + text11 + "' with a value of '" + text12 + "' as a world modifier."));
				}
			}
			else if (text9 == "-setkey" && commandLineArgs.Length > j + 1)
			{
				string text13 = commandLineArgs[j + 1];
				if (!createWorld.m_startingGlobalKeys.Contains(text13))
				{
					createWorld.m_startingGlobalKeys.Add(text13.ToLower());
				}
			}
		}
		if (flag && !IsPublicPasswordValid(password, createWorld))
		{
			string publicPasswordError = GetPublicPasswordError(password, createWorld);
			ZLog.LogError((object)("Error bad password:" + publicPasswordError));
			Application.Quit();
			return false;
		}
		ZNet.SetServer(server: true, openServer: true, flag, text2, password, createWorld);
		ZNet.ResetServerHost();
		SteamManager.SetServerPort(num);
		ZSteamSocket.SetDataPort(num);
		ZPlayFabMatchmaking.SetDataPort(num);
		if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
		{
			ZPlayFabMatchmaking.LookupPublicIP();
		}
		return true;
	}

	private void SetupObjectDB()
	{
		ObjectDB objectDB = ((Component)this).gameObject.AddComponent<ObjectDB>();
		ObjectDB component = m_objectDBPrefab.GetComponent<ObjectDB>();
		objectDB.CopyOtherDB(component);
	}

	private void ShowConnectError(ZNet.ConnectionStatus statusOverride = ZNet.ConnectionStatus.None)
	{
		ZNet.ConnectionStatus connectionStatus = ((statusOverride == ZNet.ConnectionStatus.None) ? ZNet.GetConnectionStatus() : statusOverride);
		if (ZNet.m_loadError)
		{
			m_connectionFailedPanel.SetActive(true);
			m_connectionFailedError.text = Localization.instance.Localize("$error_worldfileload");
		}
		if (ZNet.m_loadError)
		{
			m_connectionFailedPanel.SetActive(true);
			m_connectionFailedError.text = Localization.instance.Localize("$error_worldfileload");
		}
		if (connectionStatus != ZNet.ConnectionStatus.Connected && connectionStatus != ZNet.ConnectionStatus.Connecting && connectionStatus != 0)
		{
			m_connectionFailedPanel.SetActive(true);
			switch (connectionStatus)
			{
			case ZNet.ConnectionStatus.ErrorVersion:
				m_connectionFailedError.text = Localization.instance.Localize("$error_incompatibleversion");
				break;
			case ZNet.ConnectionStatus.ErrorConnectFailed:
				m_connectionFailedError.text = Localization.instance.Localize("$error_failedconnect");
				break;
			case ZNet.ConnectionStatus.ErrorDisconnected:
				m_connectionFailedError.text = Localization.instance.Localize("$error_disconnected");
				break;
			case ZNet.ConnectionStatus.ErrorPassword:
				m_connectionFailedError.text = Localization.instance.Localize("$error_password");
				break;
			case ZNet.ConnectionStatus.ErrorAlreadyConnected:
				m_connectionFailedError.text = Localization.instance.Localize("$error_alreadyconnected");
				break;
			case ZNet.ConnectionStatus.ErrorBanned:
				m_connectionFailedError.text = Localization.instance.Localize("$error_banned");
				break;
			case ZNet.ConnectionStatus.ErrorFull:
				m_connectionFailedError.text = Localization.instance.Localize("$error_serverfull");
				break;
			case ZNet.ConnectionStatus.ErrorPlatformExcluded:
				m_connectionFailedError.text = Localization.instance.Localize("$error_platformexcluded");
				break;
			case ZNet.ConnectionStatus.ErrorCrossplayPrivilege:
				m_connectionFailedError.text = Localization.instance.Localize("$xbox_error_crossplayprivilege");
				break;
			case ZNet.ConnectionStatus.ErrorKicked:
				m_connectionFailedError.text = Localization.instance.Localize("$error_kicked");
				break;
			}
		}
	}

	public void OnNewVersionButtonDownload()
	{
		Application.OpenURL(m_downloadUrl);
		Application.Quit();
	}

	public void OnNewVersionButtonContinue()
	{
		m_newGameVersionPanel.SetActive(false);
	}

	public void OnStartGame()
	{
		Gogan.LogEvent("Screen", "Enter", "StartGame", 0L);
		m_mainMenu.SetActive(false);
		if (SaveSystem.GetAllPlayerProfiles().Count == 0)
		{
			ShowCharacterSelection();
			OnCharacterNew();
		}
		else
		{
			ShowCharacterSelection();
		}
	}

	private void ShowStartGame()
	{
		m_mainMenu.SetActive(false);
		m_createWorldPanel.SetActive(false);
		((Component)m_serverOptions).gameObject.SetActive(false);
		m_startGamePanel.SetActive(true);
		RefreshWorldSelection();
	}

	public void OnSelectWorldTab()
	{
		RefreshWorldSelection();
	}

	private void RefreshWorldSelection()
	{
		UpdateWorldList(centerSelection: true);
		if (m_world != null)
		{
			m_world = FindWorld(m_world.m_name);
			if (m_world != null)
			{
				UpdateWorldList(centerSelection: true);
			}
		}
		if (m_world == null)
		{
			string @string = PlatformPrefs.GetString("world", "");
			if (@string.Length > 0)
			{
				m_world = FindWorld(@string);
			}
			if (m_world == null)
			{
				m_world = ((m_worlds.Count > 0) ? m_worlds[0] : null);
			}
			if (m_world != null)
			{
				UpdateWorldList(centerSelection: true);
			}
			m_crossplayServerToggle.isOn = PlatformPrefs.GetInt("crossplay", 1) == 1;
		}
	}

	public void OnServerListTab()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayFabManager.IsLoggedIn && PlayFabManager.CurrentLoginState != LoginState.AttemptingLogin)
		{
			PlayFabManager.instance.SetShouldTryAutoLogin(value: true);
		}
		if ((int)PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)0) != 0)
		{
			((Component)m_startGamePanel.transform.GetChild(0)).GetComponent<TabHandler>().SetActiveTab(0);
			ShowOnlineMultiplayerPrivilegeWarning();
		}
	}

	private void OnOpenServerToggleClicked(bool wasToggledOn)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayFabManager.IsLoggedIn && PlayFabManager.CurrentLoginState != LoginState.AttemptingLogin)
		{
			PlayFabManager.instance.SetShouldTryAutoLogin(wasToggledOn);
		}
		if (wasToggledOn && (int)PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)0) != 0)
		{
			m_openServerToggle.isOn = false;
			ShowOnlineMultiplayerPrivilegeWarning();
		}
	}

	private void ShowNotLoggedInToPlatform()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		Platform platform = PlatformManager.DistributionPlatform.Platform;
		text = ((!(((object)(Platform)(ref platform)).ToString() == "GameCenter")) ? "$menu_logging_in_played_failed_not_signed_in_to_platform" : "$menu_logging_in_played_failed_not_signed_in_to_platform_gamecenter");
		PlatformManager.DistributionPlatform.UIProvider.SignInLocalUser.Open();
		UnifiedPopup.Push(new WarningPopup("$menu_logging_in_playfab_failed_header", text, delegate
		{
			UnifiedPopup.Pop();
		}));
	}

	private void ShowLogInWithPlayFabWindow()
	{
		if (!PlatformManager.DistributionPlatform.LocalUser.IsSignedIn)
		{
			if (PlatformManager.DistributionPlatform.UIProvider.SignInLocalUser != null)
			{
				PlatformManager.DistributionPlatform.UIProvider.SignInLocalUser.Open();
			}
		}
		else if (!PlayFabManager.IsLoggedIn)
		{
			UnifiedPopup.Push(new YesNoPopup("$menu_loginwithplayfab_header", "$menu_loginwithplayfab_text", delegate
			{
				PlayFabManager.instance.SetShouldTryAutoLogin(value: true);
				UnifiedPopup.Pop();
				UnifiedPopup.Push(new TaskPopup("$menu_logging_in_playfab_task_header", ""));
				PlayFabManager.instance.LoginFinished -= PlayFabManager.instance.OnPlayFabRespondRemoveUIBlock;
				PlayFabManager.instance.LoginFinished += PlayFabManager.instance.OnPlayFabRespondRemoveUIBlock;
			}, delegate
			{
				PlayFabManager.instance.SetShouldTryAutoLogin(value: false);
				UnifiedPopup.Pop();
				PlayFabManager.instance.ResetMainMenuButtons();
			}));
		}
	}

	private void ShowOnlineMultiplayerPrivilegeWarning()
	{
		if (PlayFabManager.CurrentLoginState != LoginState.LoggedIn)
		{
			string text = "";
			text = " Steam";
			UnifiedPopup.Push(new WarningPopup("$menu_logintext", "$menu_loginfailedtext" + text, delegate
			{
				RefreshWorldSelection();
				UnifiedPopup.Pop();
			}));
		}
		else if (PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege != null)
		{
			PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege.Open((Privilege)0, (PrivilegeResult)64);
		}
		else
		{
			UnifiedPopup.Push(new WarningPopup("$menu_privilegerequiredheader", "$menu_onlineprivilegetext", delegate
			{
				RefreshWorldSelection();
				UnifiedPopup.Pop();
			}));
		}
	}

	private void OnUGCPopupShown()
	{
		RefreshWorldSelection();
	}

	private World FindWorld(string name)
	{
		foreach (World world in m_worlds)
		{
			if (world.m_name == name)
			{
				return world;
			}
		}
		return null;
	}

	private void UpdateWorldList(bool centerSelection)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Invalid comparison between Unknown and I4
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Invalid comparison between Unknown and I4
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Invalid comparison between Unknown and I4
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Invalid comparison between Unknown and I4
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		m_worlds = SaveSystem.GetWorldList();
		float num = (float)m_worlds.Count * m_worldListElementStep;
		num = Mathf.Max(m_worldListBaseSize, num);
		m_worldListRoot.SetSizeWithCurrentAnchors((Axis)1, num);
		for (int i = 0; i < m_worlds.Count; i++)
		{
			World world = m_worlds[i];
			GameObject val;
			if (i < m_worldListElements.Count)
			{
				val = m_worldListElements[i];
			}
			else
			{
				val = Object.Instantiate<GameObject>(m_worldListElement, (Transform)(object)m_worldListRoot);
				m_worldListElements.Add(val);
				val.SetActive(true);
			}
			Transform transform = val.transform;
			((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(0f, (float)i * (0f - m_worldListElementStep));
			Button component = val.GetComponent<Button>();
			((UnityEventBase)component.onClick).RemoveAllListeners();
			int index = i;
			((UnityEvent)component.onClick).AddListener((UnityAction)delegate
			{
				OnSelectWorld(index);
			});
			TMP_Text component2 = ((Component)val.transform.Find("seed")).GetComponent<TMP_Text>();
			component2.text = world.m_seedName;
			((Component)val.transform.Find("modifiers")).GetComponent<TMP_Text>().text = Localization.instance.Localize(ServerOptionsGUI.GetWorldModifierSummary(world.m_startingGlobalKeys, alwaysShort: true));
			TMP_Text component3 = ((Component)val.transform.Find("name")).GetComponent<TMP_Text>();
			if (world.m_name == world.m_fileName)
			{
				component3.text = world.m_name;
			}
			else
			{
				component3.text = world.m_name + " (" + world.m_fileName + ")";
			}
			Transform obj = val.transform.Find("source_cloud");
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive((int)world.m_fileSource == 2);
			}
			Transform obj2 = val.transform.Find("source_local");
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive((int)world.m_fileSource == 1);
			}
			Transform obj3 = val.transform.Find("source_legacy");
			if (obj3 != null)
			{
				((Component)obj3).gameObject.SetActive((int)world.m_fileSource == 3);
			}
			switch (world.m_dataError)
			{
			case World.SaveDataError.BadVersion:
				component2.text = " [BAD VERSION]";
				break;
			case World.SaveDataError.LoadError:
				component2.text = " [LOAD ERROR]";
				break;
			case World.SaveDataError.Corrupt:
				component2.text = " [CORRUPT]";
				break;
			case World.SaveDataError.MissingMeta:
				component2.text = " [MISSING META]";
				break;
			case World.SaveDataError.MissingDB:
				component2.text = " [MISSING DB]";
				break;
			default:
				component2.text = $" [{world.m_dataError}]";
				break;
			case World.SaveDataError.None:
				break;
			}
			Transform obj4 = val.transform.Find("selected");
			RectTransform val2 = (RectTransform)(object)((obj4 is RectTransform) ? obj4 : null);
			bool flag = m_world != null && world.m_fileName == m_world.m_fileName;
			if (flag && m_world != world)
			{
				m_world = world;
			}
			((Component)val2).gameObject.SetActive(flag);
			if (flag)
			{
				((Selectable)component).Select();
			}
			if (flag && centerSelection)
			{
				m_worldListEnsureVisible.CenterOnItem(val2);
			}
		}
		for (int num2 = m_worldListElements.Count - 1; num2 >= m_worlds.Count; num2--)
		{
			Object.Destroy((Object)(object)m_worldListElements[num2]);
			m_worldListElements.RemoveAt(num2);
		}
		((TMP_Text)m_worldSourceInfo).text = "";
		m_worldSourceInfoPanel.SetActive(false);
		if (m_world != null)
		{
			((TMP_Text)m_worldSourceInfo).text = Localization.instance.Localize((((int)m_world.m_fileSource == 3) ? "$menu_legacynotice \n\n$menu_legacynotice_worlds \n\n" : "") + ((!FileHelpers.CloudStorageEnabled) ? "$menu_cloudsavesdisabled" : ""));
			m_worldSourceInfoPanel.SetActive(((TMP_Text)m_worldSourceInfo).text.Length > 0);
		}
		for (int j = 0; j < m_worlds.Count; j++)
		{
			World world2 = m_worlds[j];
			UITooltip componentInChildren = m_worldListElements[j].GetComponentInChildren<UITooltip>();
			if (componentInChildren != null)
			{
				string worldModifierSummary = ServerOptionsGUI.GetWorldModifierSummary(world2.m_startingGlobalKeys, alwaysShort: false, "\n");
				componentInChildren.Set(string.IsNullOrEmpty(worldModifierSummary) ? "" : "$menu_serveroptions", worldModifierSummary, m_worldSourceInfoPanel.activeSelf ? m_tooltipSecondaryAnchor : m_tooltipAnchor, default(Vector2));
			}
		}
	}

	public void OnWorldRemove()
	{
		if (m_world != null)
		{
			m_removeWorldName.text = m_world.m_fileName;
			m_removeWorldDialog.SetActive(true);
		}
	}

	public void OnButtonRemoveWorldYes()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		World.RemoveWorld(m_world.m_fileName, m_world.m_fileSource);
		m_world = null;
		m_worlds = SaveSystem.GetWorldList();
		SetSelectedWorld(0, centerSelection: true);
		m_removeWorldDialog.SetActive(false);
	}

	public void OnButtonRemoveWorldNo()
	{
		m_removeWorldDialog.SetActive(false);
	}

	private void OnSelectWorld(int index)
	{
		SetSelectedWorld(index, centerSelection: false);
	}

	private void SetSelectedWorld(int index, bool centerSelection)
	{
		if (m_worlds.Count != 0)
		{
			index = Mathf.Clamp(index, 0, m_worlds.Count - 1);
			m_world = m_worlds[index];
		}
		UpdateWorldList(centerSelection);
	}

	private int GetSelectedWorld()
	{
		if (m_world == null)
		{
			return -1;
		}
		for (int i = 0; i < m_worlds.Count; i++)
		{
			if (m_worlds[i].m_fileName == m_world.m_fileName)
			{
				return i;
			}
		}
		return -1;
	}

	private int FindSelectedWorld(GameObject button)
	{
		for (int i = 0; i < m_worldListElements.Count; i++)
		{
			if ((Object)(object)m_worldListElements[i] == (Object)(object)button)
			{
				return i;
			}
		}
		return -1;
	}

	private FileSource GetMoveTarget(FileSource source)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)source != 2)
		{
			return (FileSource)2;
		}
		return (FileSource)1;
	}

	public void OnWorldNew()
	{
		m_createWorldPanel.SetActive(true);
		((TMP_InputField)m_newWorldName).text = "";
		((TMP_InputField)m_newWorldSeed).text = World.GenerateSeed();
	}

	public void OnNewWorldDone(bool forceLocal)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Invalid comparison between Unknown and I4
		string text = ((TMP_InputField)m_newWorldName).text;
		string text2 = ((TMP_InputField)m_newWorldSeed).text;
		if (World.HaveWorld(text))
		{
			UnifiedPopup.Push(new WarningPopup(Localization.instance.Localize("$menu_newworldalreadyexists"), Localization.instance.Localize("$menu_newworldalreadyexistsmessage", new string[1] { text }), delegate
			{
				UnifiedPopup.Pop();
			}, localizeText: false));
			return;
		}
		m_world = new World(text, text2);
		m_world.m_fileSource = (FileSource)((!FileHelpers.CloudStorageEnabled || forceLocal) ? 1 : 2);
		m_world.m_needsDB = false;
		if ((int)m_world.m_fileSource == 2 && FileHelpers.OperationExceedsCloudCapacity(2097152uL))
		{
			ShowCloudQuotaWorldDialog();
			ZLog.LogWarning((object)"This operation may exceed the cloud save quota and has therefore been aborted! Prompt shown to user.");
			return;
		}
		m_world.SaveWorldMetaData(DateTime.Now);
		UpdateWorldList(centerSelection: true);
		ShowStartGame();
		Gogan.LogEvent("Menu", "NewWorld", text, 0L);
	}

	public void OnNewWorldBack()
	{
		ShowStartGame();
	}

	public void OnServerOptions()
	{
		RefreshWorldSelection();
		((Component)m_serverOptions).gameObject.SetActive(true);
		m_serverOptions.ReadKeys(m_world);
		EventSystem.current.SetSelectedGameObject(m_serverOptions.m_doneButton);
		if (PlatformPrefs.GetInt("ServerOptionsDisclaimer", 0) == 0)
		{
			UnifiedPopup.Push(new WarningPopup("$menu_modifier_popup_title", "$menu_modifier_popup_text", delegate
			{
				UnifiedPopup.Pop();
			}));
			PlatformPrefs.SetInt("ServerOptionsDisclaimer", 1);
		}
	}

	public void OnServerOptionsDone()
	{
		m_world.m_startingGlobalKeys.Clear();
		m_world.m_startingKeysChanged = true;
		m_serverOptions.SetKeys(m_world);
		DateTime now = DateTime.Now;
		if (!SaveSystem.TryGetSaveByName(m_world.m_fileName, SaveDataType.World, out var save) || save.IsDeleted)
		{
			ZLog.LogError((object)("Failed to retrieve world save " + m_world.m_fileName + " by name when modifying server options!"));
			ShowStartGame();
			return;
		}
		SaveSystem.CheckMove(m_world.m_fileName, SaveDataType.World, ref m_world.m_fileSource, now, save.PrimaryFile.Size, copyToNewLocation: true);
		m_world.SaveWorldMetaData(now);
		UpdateWorldList(centerSelection: true);
		ShowStartGame();
	}

	public void OnServerOptionsCancel()
	{
		ShowStartGame();
	}

	private void OpenBrowser(string url)
	{
		if (PlatformManager.DistributionPlatform.UIProvider.WebBrowser != null)
		{
			PlatformManager.DistributionPlatform.UIProvider.WebBrowser.Open(url);
		}
		else
		{
			Application.OpenURL(url);
		}
	}

	public void OnMerchStoreButton()
	{
		OpenBrowser("http://valheim.shop/?game_" + Version.GetPlatformPrefix("win"));
	}

	public void OnBoardGameButton()
	{
		OpenBrowser("http://bit.ly/valheimtheboardgame");
	}

	public void OnCloudStorageLowNextSaveWarningOk()
	{
		m_cloudStorageWarningNextSave.SetActive(false);
		RefreshWorldSelection();
	}

	public void OnWorldStart()
	{
		if (!SaveSystem.CanSaveToCloudStorage(m_world, m_profiles[m_profileIndex]) || Menu.ExceedCloudStorageTest)
		{
			m_cloudStorageWarningNextSave.SetActive(true);
		}
		else
		{
			if (m_world == null || m_startingWorld)
			{
				return;
			}
			Game.m_serverOptionsSummary = "";
			switch (m_world.m_dataError)
			{
			case World.SaveDataError.LoadError:
			case World.SaveDataError.Corrupt:
			{
				if (!SaveSystem.TryGetSaveByName(m_world.m_name, SaveDataType.World, out var save2))
				{
					UnifiedPopup.Push(new WarningPopup("$error_cantrestorebackup", "$menu_checklogfile", UnifiedPopup.Pop));
					ZLog.LogError((object)("Failed to restore backup! Couldn't get world " + m_world.m_name + " by name from save system."));
				}
				else if (save2.IsDeleted)
				{
					UnifiedPopup.Push(new WarningPopup("$error_cantrestorebackup", "$menu_checklogfile", UnifiedPopup.Pop));
					ZLog.LogError((object)("Failed to restore backup! World " + m_world.m_name + " retrieved from save system was deleted."));
				}
				else if (SaveSystem.HasRestorableBackup(save2))
				{
					RestoreBackupPrompt(save2);
				}
				else
				{
					UnifiedPopup.Push(new WarningPopup("$error_cantrestorebackup", "$error_nosuitablebackupfound", UnifiedPopup.Pop));
				}
				break;
			}
			case World.SaveDataError.MissingMeta:
			{
				if (!SaveSystem.TryGetSaveByName(m_world.m_name, SaveDataType.World, out var save))
				{
					UnifiedPopup.Push(new WarningPopup("$error_cantrestoremeta", "$menu_checklogfile", UnifiedPopup.Pop));
					ZLog.LogError((object)("Failed to restore meta file! Couldn't get world " + m_world.m_name + " by name from save system."));
				}
				else if (save.IsDeleted)
				{
					UnifiedPopup.Push(new WarningPopup("$error_cantrestoremeta", "$menu_checklogfile", UnifiedPopup.Pop));
					ZLog.LogError((object)("Failed to restore meta file! World " + m_world.m_name + " retrieved from save system was deleted."));
				}
				else if (SaveSystem.HasBackupWithMeta(save))
				{
					RestoreMetaFromBackupPrompt(save);
				}
				else
				{
					UnifiedPopup.Push(new WarningPopup("$error_cantrestoremeta", "$error_nosuitablebackupfound", UnifiedPopup.Pop));
				}
				break;
			}
			case World.SaveDataError.BadVersion:
				break;
			case World.SaveDataError.None:
			{
				PlatformPrefs.SetString("world", m_world.m_name);
				if (((Selectable)m_crossplayServerToggle).IsInteractable())
				{
					PlatformPrefs.SetInt("crossplay", m_crossplayServerToggle.isOn ? 1 : 0);
				}
				bool isOn = m_publicServerToggle.isOn;
				bool isOn2 = m_openServerToggle.isOn;
				bool isOn3 = m_crossplayServerToggle.isOn;
				string text = ((TMP_InputField)m_serverPassword).text;
				OnlineBackendType onlineBackend = GetOnlineBackend(isOn3);
				if (isOn2 && onlineBackend == OnlineBackendType.PlayFab && !PlayFabManager.IsLoggedIn)
				{
					ContinueWhenLoggedInPopup(OnWorldStart);
					break;
				}
				ZNet.m_onlineBackend = onlineBackend;
				ZSteamMatchmaking.instance.StopServerListing();
				m_startingWorld = true;
				ZNet.SetServer(server: true, isOn2, isOn, m_world.m_name, text, m_world);
				ZNet.ResetServerHost();
				string eventLabel = "open:" + isOn2 + ",public:" + isOn;
				Gogan.LogEvent("Menu", "WorldStart", eventLabel, 0L);
				TransitionToMainScene();
				break;
			}
			}
		}
		void RestoreBackupPrompt(SaveWithBackups saveToRestore)
		{
			UnifiedPopup.Push(new YesNoPopup("$menu_restorebackup", "$menu_corruptsaverestore", delegate
			{
				UnifiedPopup.Pop();
				SaveSystem.RestoreBackupResult restoreBackupResult = SaveSystem.RestoreMostRecentBackup(saveToRestore);
				switch (restoreBackupResult)
				{
				case SaveSystem.RestoreBackupResult.Success:
					SaveSystem.ClearWorldListCache(reload: true);
					RefreshWorldSelection();
					break;
				case SaveSystem.RestoreBackupResult.NoBackup:
					UnifiedPopup.Push(new WarningPopup("$error_cantrestorebackup", "$error_nosuitablebackupfound", UnifiedPopup.Pop));
					break;
				default:
					UnifiedPopup.Push(new WarningPopup("$error_cantrestorebackup", "$menu_checklogfile", UnifiedPopup.Pop));
					ZLog.LogError((object)$"Failed to restore backup! Result: {restoreBackupResult}");
					break;
				}
			}, UnifiedPopup.Pop));
		}
		void RestoreMetaFromBackupPrompt(SaveWithBackups saveToRestore)
		{
			UnifiedPopup.Push(new YesNoPopup("$menu_restorebackup", "$menu_missingmetarestore", delegate
			{
				UnifiedPopup.Pop();
				SaveSystem.RestoreBackupResult restoreBackupResult2 = SaveSystem.RestoreMetaFromMostRecentBackup(saveToRestore.PrimaryFile);
				switch (restoreBackupResult2)
				{
				case SaveSystem.RestoreBackupResult.Success:
					RefreshWorldSelection();
					break;
				case SaveSystem.RestoreBackupResult.NoBackup:
					UnifiedPopup.Push(new WarningPopup("$error_cantrestoremeta", "$error_nosuitablebackupfound", UnifiedPopup.Pop));
					break;
				default:
					UnifiedPopup.Push(new WarningPopup("$error_cantrestoremeta", "$menu_checklogfile", UnifiedPopup.Pop));
					ZLog.LogError((object)$"Failed to restore meta file! Result: {restoreBackupResult2}");
					break;
				}
			}, UnifiedPopup.Pop));
		}
	}

	private void ContinueWhenLoggedInPopup(ContinueAction continueAction)
	{
		string headerText = Localization.instance.Localize("$menu_loginheader");
		string loggingInText = Localization.instance.Localize("$menu_logintext");
		string retryText = "";
		int previousRetryCountdown = -1;
		PlayFabManager.instance.SetShouldTryAutoLogin(value: true);
		UnifiedPopup.Push(new CancelableTaskPopup(() => headerText, delegate
		{
			if (PlayFabManager.CurrentLoginState == LoginState.WaitingForRetry)
			{
				int num = Mathf.CeilToInt((float)(PlayFabManager.NextRetryUtc - DateTime.UtcNow).TotalSeconds);
				if (previousRetryCountdown != num)
				{
					previousRetryCountdown = num;
					retryText = Localization.instance.Localize("$menu_loginfailedtext") + "\n" + Localization.instance.Localize("$menu_loginretrycountdowntext", new string[1] { num.ToString() });
				}
				return retryText;
			}
			return loggingInText;
		}, delegate
		{
			if (PlayFabManager.IsLoggedIn)
			{
				continueAction?.Invoke();
			}
			return PlayFabManager.IsLoggedIn;
		}, delegate
		{
			UnifiedPopup.Pop();
		}));
	}

	private OnlineBackendType GetOnlineBackend(bool crossplayServer)
	{
		OnlineBackendType result = OnlineBackendType.PlayFab;
		if (!crossplayServer)
		{
			result = OnlineBackendType.Steamworks;
		}
		return result;
	}

	private void ShowCharacterSelection()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Gogan.LogEvent("Screen", "Enter", "CharacterSelection", 0L);
		ZLog.Log((object)"show character selection");
		m_characterSelectScreen.SetActive(true);
		m_selectCharacterPanel.SetActive(true);
		m_newCharacterPanel.SetActive(false);
		if (m_profiles == null)
		{
			m_profiles = SaveSystem.GetAllPlayerProfiles();
		}
		if (m_profileIndex >= m_profiles.Count)
		{
			m_profileIndex = m_profiles.Count - 1;
		}
		if (m_profileIndex >= 0 && m_profileIndex < m_profiles.Count)
		{
			PlayerProfile playerProfile = m_profiles[m_profileIndex];
			m_csFileSource.text = Localization.instance.Localize(FileHelpers.GetSourceString(playerProfile.m_fileSource));
		}
	}

	public void OnJoinStart()
	{
		JoinServer();
	}

	public void JoinServer()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayFabManager.IsLoggedIn && m_joinServer.m_type == ServerJoinDataType.PlayFabUser)
		{
			ContinueWhenLoggedInPopup(JoinServer);
			return;
		}
		if ((int)PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)0) != 0)
		{
			ZLog.LogWarning((object)"You should always prevent JoinServer() from being called when user does not have online multiplayer privilege!");
			HideAll();
			m_mainMenu.SetActive(true);
			ShowOnlineMultiplayerPrivilegeWarning();
			return;
		}
		ServerMatchmakingData serverMatchmakingData = MultiBackendMatchmaking.GetServerMatchmakingData(m_joinServer);
		if (serverMatchmakingData.m_onlineStatus.IsOnline() && serverMatchmakingData.m_networkVersion != 36)
		{
			UnifiedPopup.Push(new WarningPopup("$error_incompatibleversion", (36 < serverMatchmakingData.m_networkVersion) ? "$error_needslocalupdatetojoin" : "$error_needsserverupdatetojoin", delegate
			{
				UnifiedPopup.Pop();
			}));
			return;
		}
		if (serverMatchmakingData.IsUnjoinable)
		{
			if (serverMatchmakingData.IsCrossplay)
			{
				if (PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege != null)
				{
					PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege.Open((Privilege)1, (PrivilegeResult)64);
					return;
				}
				UnifiedPopup.Push(new WarningPopup(Localization.instance.Localize("$error_failedconnect"), Localization.instance.Localize("$xbox_error_crossplayprivilege"), delegate
				{
					UnifiedPopup.Pop();
				}, localizeText: false));
			}
			else
			{
				UnifiedPopup.Push(new WarningPopup(Localization.instance.Localize("$error_failedconnect"), Localization.instance.Localize("$xbox_error_crossplayprivilege"), delegate
				{
					UnifiedPopup.Pop();
				}, localizeText: false));
			}
			return;
		}
		ZNet.SetServer(server: false, openServer: false, publicServer: false, "", "", null);
		retries = 0;
		bool flag = false;
		if (m_joinServer.m_type == ServerJoinDataType.SteamUser)
		{
			ZNet.SetServerHost((ulong)m_joinServer.SteamUser.m_joinUserID);
			flag = true;
		}
		if (m_joinServer.m_type == ServerJoinDataType.PlayFabUser)
		{
			ZNet.SetServerHost(m_joinServer.PlayFabUser.m_remotePlayerId);
			flag = true;
		}
		if (m_joinServer.m_type == ServerJoinDataType.Dedicated)
		{
			ServerJoinDataDedicated serverJoin = m_joinServer.Dedicated;
			ZNet.ResetServerHost();
			MultiBackendMatchmaking.GetServerIPAsync(serverJoin, delegate(bool succeeded, IPv6Address? address)
			{
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0082: Unknown result type (might be due to invalid IL or missing references)
				//IL_0087: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				if (!succeeded || !address.HasValue)
				{
					retries = 50;
				}
				IPEndPoint endPoint = new IPEndPoint(address.Value, serverJoin.m_port);
				if (PlayFabManager.IsLoggedIn)
				{
					ZPlayFabMatchmaking.FindHostByIp(endPoint, delegate(PlayFabMatchmakingServerData result)
					{
						if (result != null)
						{
							ZNet.SetServerHost(result.remotePlayerId);
							ZLog.Log((object)"Determined backend of dedicated server to be PlayFab");
						}
						else
						{
							retries = 50;
						}
					}, delegate
					{
						ZNet.SetServerHost(((object)(IPEndPoint)(ref endPoint)).ToString(), serverJoin.m_port, OnlineBackendType.Steamworks);
						ZLog.Log((object)"Determined backend of dedicated server to be Steamworks");
					}, joinLobby: true);
				}
				else
				{
					IPv6Address address2 = endPoint.m_address;
					ZNet.SetServerHost(((object)(IPv6Address)(ref address2)).ToString(), endPoint.m_port, OnlineBackendType.Steamworks);
					ZLog.Log((object)"Determined backend of dedicated server to be Steamworks");
				}
			});
			flag = true;
		}
		if (!flag)
		{
			Debug.LogError((object)"Couldn't set the server host!");
			return;
		}
		Gogan.LogEvent("Menu", "JoinServer", "", 0L);
		ServerListGui.AddToRecentServersList(GetServerToJoin());
		TransitionToMainScene();
	}

	public void OnStartGameBack()
	{
		m_startGamePanel.SetActive(false);
		ShowCharacterSelection();
	}

	public void OnCredits()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		m_creditsPanel.SetActive(true);
		m_mainMenu.SetActive(false);
		Gogan.LogEvent("Screen", "Enter", "Credits", 0L);
		m_creditsList.anchoredPosition = new Vector2(0f, 0f);
	}

	public void OnCreditsBack()
	{
		m_mainMenu.SetActive(true);
		m_creditsPanel.SetActive(false);
		Gogan.LogEvent("Screen", "Enter", "StartMenu", 0L);
	}

	public void OnSelelectCharacterBack()
	{
		m_characterSelectScreen.SetActive(false);
		m_mainMenu.SetActive(true);
		m_queuedJoinServer = ServerJoinData.None;
		Gogan.LogEvent("Screen", "Enter", "StartMenu", 0L);
	}

	public void OnAbort()
	{
		Application.Quit();
	}

	public void OnWorldVersionYes()
	{
		m_worldVersionPanel.SetActive(false);
	}

	public void OnPlayerVersionOk()
	{
		m_playerVersionPanel.SetActive(false);
	}

	private void FixedUpdate()
	{
		ZInput.FixedUpdate(Time.fixedDeltaTime);
	}

	private void UpdateCursor()
	{
		Cursor.lockState = (CursorLockMode)(!ZInput.IsMouseActive());
		Cursor.visible = ZInput.IsMouseActive();
	}

	private void OnLanguageChange()
	{
		UpdateCharacterList();
	}

	private void Update()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		ZInput.Update(Time.deltaTime);
		Localization.instance.ReLocalizeVisible(((Component)this).transform);
		UpdateGamepad();
		UpdateKeyboard();
		CheckPendingJoinRequest();
		if (MasterClient.instance != null)
		{
			MasterClient.instance.Update(Time.deltaTime);
		}
		if (ZBroastcast.instance != null)
		{
			ZBroastcast.instance.Update(Time.deltaTime);
		}
		UpdateCharacterRotation(Time.deltaTime);
		UpdateCamera(Time.deltaTime);
		if (m_newCharacterPanel.activeInHierarchy)
		{
			((Selectable)m_csNewCharacterDone).interactable = ((TMP_InputField)m_csNewCharacterName).text.Length >= 3;
			Navigation navigation = ((Selectable)m_csNewCharacterName).navigation;
			((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)(((Selectable)m_csNewCharacterDone).interactable ? m_csNewCharacterDone : m_csNewCharacterCancel);
			((Selectable)m_csNewCharacterName).navigation = navigation;
		}
		if (m_newCharacterPanel.activeInHierarchy)
		{
			((Selectable)m_csNewCharacterDone).interactable = ((TMP_InputField)m_csNewCharacterName).text.Length >= 3;
		}
		if (((Component)m_serverOptionsButton).gameObject.activeInHierarchy)
		{
			((Selectable)m_serverOptionsButton).interactable = m_world != null;
		}
		if (m_createWorldPanel.activeInHierarchy)
		{
			((Selectable)m_newWorldDone).interactable = ((TMP_InputField)m_newWorldName).text.Length >= 5;
		}
		if (m_startGamePanel.activeInHierarchy)
		{
			((Selectable)m_worldStart).interactable = CanStartServer();
			((Selectable)m_worldRemove).interactable = m_world != null;
			UpdatePasswordError();
		}
		if (m_startGamePanel.activeInHierarchy)
		{
			bool flag = m_openServerToggle.isOn && ((Selectable)m_openServerToggle).interactable;
			SetToggleState(m_publicServerToggle, flag);
			SetToggleState(m_crossplayServerToggle, flag);
			((Selectable)m_serverPassword).interactable = flag;
		}
		if (m_creditsPanel.activeInHierarchy)
		{
			Transform parent = ((Transform)m_creditsList).parent;
			Transform obj = ((parent is RectTransform) ? parent : null);
			Vector3[] array = (Vector3[])(object)new Vector3[4];
			m_creditsList.GetWorldCorners(array);
			Vector3[] array2 = (Vector3[])(object)new Vector3[4];
			((RectTransform)obj).GetWorldCorners(array2);
			float num = array2[1].y - array2[0].y;
			if ((double)array[3].y < (double)num * 0.5)
			{
				Vector3 position = ((Transform)m_creditsList).position;
				position.y += Time.deltaTime * m_creditsSpeed * num;
				((Transform)m_creditsList).position = position;
			}
		}
		this.m_cliUpdateAction?.Invoke();
	}

	private void OnGUI()
	{
		ZInput.OnGUI();
	}

	private void SetToggleState(Toggle toggle, bool active)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		((Selectable)toggle).interactable = active;
		Color toggleColor = m_toggleColor;
		TMP_Text componentInChildren = ((Component)toggle).GetComponentInChildren<TMP_Text>();
		if (!active)
		{
			float num = 0.5f;
			float num2 = ((Color)(ref toggleColor)).linear.r * 0.2126f + ((Color)(ref toggleColor)).linear.g * 0.7152f + ((Color)(ref toggleColor)).linear.b * 0.0722f;
			num2 *= num;
			toggleColor.r = (toggleColor.g = (toggleColor.b = Mathf.LinearToGammaSpace(num2)));
		}
		((Graphic)componentInChildren).color = toggleColor;
	}

	private void LateUpdate()
	{
		if (ZInput.GetKeyDown((KeyCode)292, true))
		{
			GameCamera.ScreenShot();
		}
	}

	private void UpdateKeyboard()
	{
		if (ZInput.GetKeyDown((KeyCode)13, true) && m_menuList.activeInHierarchy && !((Component)m_passwordError).gameObject.activeInHierarchy)
		{
			if ((Object)(object)m_menuSelectedButton != (Object)null)
			{
				m_menuSelectedButton.OnSubmit((BaseEventData)null);
			}
			else
			{
				OnStartGame();
			}
		}
		if (m_worldListPanel.GetComponent<UIGamePad>().IsBlocked())
		{
			return;
		}
		if (ZInput.GetKeyDown((KeyCode)273, true))
		{
			if (m_worldListPanel.activeInHierarchy)
			{
				SetSelectedWorld(GetSelectedWorld() - 1, centerSelection: true);
			}
			if (m_menuList.activeInHierarchy)
			{
				if ((Object)(object)m_menuSelectedButton == (Object)null)
				{
					m_menuSelectedButton = m_menuButtons[0];
					((Selectable)m_menuSelectedButton).Select();
				}
				else
				{
					for (int i = 1; i < m_menuButtons.Length; i++)
					{
						if ((Object)(object)m_menuButtons[i] == (Object)(object)m_menuSelectedButton)
						{
							m_menuSelectedButton = m_menuButtons[i - 1];
							((Selectable)m_menuSelectedButton).Select();
							break;
						}
					}
				}
			}
		}
		if (!ZInput.GetKeyDown((KeyCode)274, true))
		{
			return;
		}
		if (m_worldListPanel.activeInHierarchy)
		{
			SetSelectedWorld(GetSelectedWorld() + 1, centerSelection: true);
		}
		if (!m_menuList.activeInHierarchy)
		{
			return;
		}
		if ((Object)(object)m_menuSelectedButton == (Object)null)
		{
			m_menuSelectedButton = m_menuButtons[0];
			((Selectable)m_menuSelectedButton).Select();
			return;
		}
		for (int j = 0; j < m_menuButtons.Length - 1; j++)
		{
			if ((Object)(object)m_menuButtons[j] == (Object)(object)m_menuSelectedButton)
			{
				m_menuSelectedButton = m_menuButtons[j + 1];
				((Selectable)m_menuSelectedButton).Select();
				break;
			}
		}
	}

	private void UpdateGamepad()
	{
		if (ZInput.IsGamepadActive() && m_menuList.activeInHierarchy && (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)null && m_menuButtons != null && m_menuButtons.Length != 0)
		{
			((MonoBehaviour)this).StartCoroutine(SelectFirstMenuEntry(m_menuButtons[0]));
		}
		if (!ZInput.IsGamepadActive() || m_worldListPanel.GetComponent<UIGamePad>().IsBlocked())
		{
			return;
		}
		if (m_worldListPanel.activeInHierarchy)
		{
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				SetSelectedWorld(GetSelectedWorld() + 1, centerSelection: true);
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				SetSelectedWorld(GetSelectedWorld() - 1, centerSelection: true);
			}
			if ((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)null)
			{
				RefreshWorldSelection();
			}
		}
		if (m_characterSelectScreen.activeInHierarchy && !m_newCharacterPanel.activeInHierarchy && ((Selectable)m_csLeftButton).interactable && ZInput.GetButtonDown("JoyDPadLeft"))
		{
			OnCharacterLeft();
		}
		if (m_characterSelectScreen.activeInHierarchy && !m_newCharacterPanel.activeInHierarchy && ((Selectable)m_csRightButton).interactable && ZInput.GetButtonDown("JoyDPadRight"))
		{
			OnCharacterRight();
		}
		if (((Component)m_patchLogScroll).gameObject.activeInHierarchy)
		{
			Scrollbar patchLogScroll = m_patchLogScroll;
			patchLogScroll.value -= ZInput.GetJoyRightStickY(true) * 0.02f;
		}
	}

	private IEnumerator SelectFirstMenuEntry(Button button)
	{
		if (m_menuList.activeInHierarchy)
		{
			if (Event.current != null)
			{
				Event.current.Use();
			}
			yield return null;
			yield return null;
			if (UnifiedPopup.IsVisible())
			{
				UnifiedPopup.SetFocus();
				yield break;
			}
			m_menuSelectedButton = button;
			((Selectable)m_menuSelectedButton).Select();
		}
	}

	private void CheckPendingJoinRequest()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (ZSteamMatchmaking.instance == null || !ZSteamMatchmaking.instance.GetJoinHost(out var joinData))
		{
			return;
		}
		if ((int)PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)0) != 0)
		{
			ShowOnlineMultiplayerPrivilegeWarning();
			return;
		}
		m_queuedJoinServer = joinData;
		if (m_serverListPanel.activeInHierarchy)
		{
			m_joinServer = m_queuedJoinServer;
			m_queuedJoinServer = ServerJoinData.None;
			JoinServer();
		}
		else
		{
			HideAll();
			ShowCharacterSelection();
		}
	}

	private void UpdateCharacterRotation(float dt)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_playerInstance == (Object)null) && m_characterSelectScreen.activeInHierarchy)
		{
			if (ZInput.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				float x = ZInput.GetMouseDelta().x;
				m_playerInstance.transform.Rotate(0f, (0f - x) * m_characterRotateSpeed, 0f);
			}
			float joyRightStickX = ZInput.GetJoyRightStickX(true);
			if (joyRightStickX != 0f)
			{
				m_playerInstance.transform.Rotate(0f, (0f - joyRightStickX) * m_characterRotateSpeedGamepad * dt, 0f);
			}
		}
	}

	private void UpdatePasswordError()
	{
		string text = "";
		if (NeedPassword())
		{
			text = GetPublicPasswordError(((TMP_InputField)m_serverPassword).text, m_world);
		}
		m_passwordError.text = text;
	}

	private bool NeedPassword()
	{
		return (m_publicServerToggle.isOn | m_crossplayServerToggle.isOn) & m_openServerToggle.isOn;
	}

	private string GetPublicPasswordError(string password, World world)
	{
		if (password.Length < m_minimumPasswordLength)
		{
			return Localization.instance.Localize("$menu_passwordshort");
		}
		if (world != null && (world.m_name.Contains(password) || world.m_seedName.Contains(password)))
		{
			return Localization.instance.Localize("$menu_passwordinvalid");
		}
		return "";
	}

	private bool IsPublicPasswordValid(string password, World world)
	{
		if (password.Length < m_minimumPasswordLength)
		{
			return false;
		}
		if (world.m_name.Contains(password))
		{
			return false;
		}
		if (world.m_seedName.Contains(password))
		{
			return false;
		}
		return true;
	}

	private bool CanStartServer()
	{
		if (m_world == null)
		{
			return false;
		}
		switch (m_world.m_dataError)
		{
		default:
			return false;
		case World.SaveDataError.None:
		case World.SaveDataError.LoadError:
		case World.SaveDataError.Corrupt:
		case World.SaveDataError.MissingMeta:
			if (NeedPassword() && !IsPublicPasswordValid(((TMP_InputField)m_serverPassword).text, m_world))
			{
				return false;
			}
			return true;
		}
	}

	private void UpdateCamera(float dt)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		Transform val = m_cameraMarkerMain;
		if (m_characterSelectScreen.activeSelf)
		{
			val = m_cameraMarkerCharacter;
		}
		else if (m_creditsPanel.activeSelf)
		{
			val = m_cameraMarkerCredits;
		}
		else if (m_startGamePanel.activeSelf)
		{
			val = m_cameraMarkerGame;
		}
		else if (m_manageSavesMenu.IsVisible())
		{
			val = m_cameraMarkerSaves;
		}
		m_mainCamera.transform.position = Vector3.SmoothDamp(m_mainCamera.transform.position, val.position, ref camSpeed, 1.5f, 1000f, dt);
		Vector3 val2 = Vector3.SmoothDamp(m_mainCamera.transform.forward, val.forward, ref camRotSpeed, 1.5f, 1000f, dt);
		((Vector3)(ref val2)).Normalize();
		m_mainCamera.transform.rotation = Quaternion.LookRotation(val2);
	}

	public void ShowCloudQuotaWarning()
	{
		UnifiedPopup.Push(new WarningPopup("$menu_cloudstoragefull", "$menu_cloudstoragefulloperationfailed", delegate
		{
			UnifiedPopup.Pop();
		}));
	}

	public void ShowCloudQuotaWorldDialog()
	{
		UnifiedPopup.Push(new YesNoPopup("$menu_cloudstoragefull", "$menu_cloudstoragefullworldprompt", delegate
		{
			UnifiedPopup.Pop();
			OnNewWorldDone(forceLocal: true);
		}, delegate
		{
			UnifiedPopup.Pop();
		}));
	}

	public void ShowCloudQuotaCharacterDialog()
	{
		UnifiedPopup.Push(new YesNoPopup("$menu_cloudstoragefull", "$menu_cloudstoragefullcharacterprompt", delegate
		{
			UnifiedPopup.Pop();
			OnNewCharacterDone(forceLocal: true);
		}, delegate
		{
			UnifiedPopup.Pop();
		}));
	}

	public void OnManageSaves(int index)
	{
		HideAll();
		switch (index)
		{
		case 0:
			m_manageSavesMenu.Open(SaveDataType.World, (m_world != null) ? m_world.m_fileName : null, ShowStartGame, OnSavesModified);
			break;
		case 1:
			m_manageSavesMenu.Open(SaveDataType.Character, (m_profileIndex >= 0 && m_profileIndex < m_profiles.Count && m_profiles[m_profileIndex] != null) ? m_profiles[m_profileIndex].m_filename : null, ShowCharacterSelection, OnSavesModified);
			break;
		}
	}

	private void OnSavesModified(SaveDataType dataType)
	{
		switch (dataType)
		{
		case SaveDataType.World:
			SaveSystem.ClearWorldListCache(reload: true);
			RefreshWorldSelection();
			break;
		case SaveDataType.Character:
		{
			string selectedProfile = null;
			if (m_profileIndex < m_profiles.Count && m_profileIndex >= 0)
			{
				selectedProfile = m_profiles[m_profileIndex].GetFilename();
			}
			m_profiles = SaveSystem.GetAllPlayerProfiles();
			SetSelectedProfile(selectedProfile);
			m_manageSavesMenu.Open(dataType, ShowCharacterSelection, OnSavesModified);
			break;
		}
		}
	}

	private void UpdateCharacterList()
	{
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Invalid comparison between Unknown and I4
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Invalid comparison between Unknown and I4
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Invalid comparison between Unknown and I4
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Invalid comparison between Unknown and I4
		if (m_profiles == null)
		{
			m_profiles = SaveSystem.GetAllPlayerProfiles();
		}
		if (m_profileIndex >= m_profiles.Count)
		{
			m_profileIndex = m_profiles.Count - 1;
		}
		((Component)m_csRemoveButton).gameObject.SetActive(m_profiles.Count > 0);
		((Component)m_csStartButton).gameObject.SetActive(m_profiles.Count > 0);
		((Component)m_csNewButton).gameObject.SetActive(m_profiles.Count > 0);
		((Component)m_csNewBigButton).gameObject.SetActive(m_profiles.Count == 0);
		((Selectable)m_csLeftButton).interactable = m_profileIndex > 0;
		((Selectable)m_csRightButton).interactable = m_profileIndex < m_profiles.Count - 1;
		if (m_profileIndex >= 0 && m_profileIndex < m_profiles.Count)
		{
			PlayerProfile playerProfile = m_profiles[m_profileIndex];
			if (playerProfile.GetName().ToLower() == playerProfile.m_filename.ToLower())
			{
				m_csName.text = playerProfile.GetName();
			}
			else
			{
				m_csName.text = playerProfile.GetName() + " (" + playerProfile.m_filename + ")";
			}
			((Component)m_csName).gameObject.SetActive(true);
			((Component)m_csFileSource).gameObject.SetActive(true);
			m_csFileSource.text = Localization.instance.Localize(FileHelpers.GetSourceString(playerProfile.m_fileSource));
			m_csSourceInfo.text = Localization.instance.Localize((((int)playerProfile.m_fileSource == 3) ? "$menu_legacynotice \n\n" : "") + ((!FileHelpers.CloudStorageEnabled) ? "$menu_cloudsavesdisabled" : ""));
			Transform obj = m_csFileSource.transform.Find("source_cloud");
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive((int)playerProfile.m_fileSource == 2);
			}
			Transform obj2 = m_csFileSource.transform.Find("source_local");
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive((int)playerProfile.m_fileSource == 1);
			}
			Transform obj3 = m_csFileSource.transform.Find("source_legacy");
			if (obj3 != null)
			{
				((Component)obj3).gameObject.SetActive((int)playerProfile.m_fileSource == 3);
			}
			SetupCharacterPreview(playerProfile);
		}
		else
		{
			((Component)m_csName).gameObject.SetActive(false);
			((Component)m_csFileSource).gameObject.SetActive(false);
			ClearCharacterPreview();
		}
	}

	private void SetSelectedProfile(string filename)
	{
		if (m_profiles == null)
		{
			m_profiles = SaveSystem.GetAllPlayerProfiles();
		}
		m_profileIndex = 0;
		if (filename != null)
		{
			for (int i = 0; i < m_profiles.Count; i++)
			{
				if (m_profiles[i].GetFilename() == filename)
				{
					m_profileIndex = i;
					break;
				}
			}
		}
		UpdateCharacterList();
	}

	public void OnNewCharacterDone(bool forceLocal)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		string text = ((TMP_InputField)m_csNewCharacterName).text;
		string text2 = text.ToLower();
		PlayerProfile playerProfile = new PlayerProfile(text2, (FileSource)0);
		if (forceLocal)
		{
			playerProfile.m_fileSource = (FileSource)1;
		}
		if ((int)playerProfile.m_fileSource == 2 && FileHelpers.OperationExceedsCloudCapacity(1048576uL * 3uL))
		{
			ShowCloudQuotaCharacterDialog();
			ZLog.LogWarning((object)"The character save operation may exceed the cloud save quota and has therefore been aborted! Prompt shown to user.");
			return;
		}
		if (PlayerProfile.HaveProfile(text2))
		{
			m_newCharacterError.SetActive(true);
			return;
		}
		Player component = m_playerInstance.GetComponent<Player>();
		component.GiveDefaultItems();
		playerProfile.SetName(text);
		playerProfile.SavePlayerData(component);
		playerProfile.Save();
		m_selectCharacterPanel.SetActive(true);
		m_newCharacterPanel.SetActive(false);
		m_profiles = null;
		SetSelectedProfile(text2);
		((TMP_InputField)m_csNewCharacterName).text = "";
		Gogan.LogEvent("Menu", "NewCharacter", text, 0L);
	}

	public void OnNewCharacterCancel()
	{
		m_selectCharacterPanel.SetActive(true);
		m_newCharacterPanel.SetActive(false);
		UpdateCharacterList();
	}

	public void OnCharacterNew()
	{
		m_newCharacterPanel.SetActive(true);
		m_selectCharacterPanel.SetActive(false);
		m_newCharacterError.SetActive(false);
		SetupCharacterPreview(null);
		Gogan.LogEvent("Screen", "Enter", "CreateCharacter", 0L);
	}

	public void OnCharacterRemove()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (m_profileIndex >= 0 && m_profileIndex < m_profiles.Count)
		{
			PlayerProfile playerProfile = m_profiles[m_profileIndex];
			m_removeCharacterName.text = playerProfile.GetName() + " (" + Localization.instance.Localize(FileHelpers.GetSourceString(playerProfile.m_fileSource)) + ")";
			m_tempRemoveCharacterName = playerProfile.GetFilename();
			m_tempRemoveCharacterSource = playerProfile.m_fileSource;
			m_tempRemoveCharacterIndex = m_profileIndex;
			m_removeCharacterDialog.SetActive(true);
		}
	}

	public void OnButtonRemoveCharacterYes()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)"Remove character");
		PlayerProfile.RemoveProfile(m_tempRemoveCharacterName, m_tempRemoveCharacterSource);
		m_profiles.RemoveAt(m_tempRemoveCharacterIndex);
		UpdateCharacterList();
		m_removeCharacterDialog.SetActive(false);
	}

	public void OnButtonRemoveCharacterNo()
	{
		m_removeCharacterDialog.SetActive(false);
	}

	public void OnCharacterLeft()
	{
		if (m_profileIndex > 0)
		{
			m_profileIndex--;
		}
		UpdateCharacterList();
	}

	public void OnCharacterRight()
	{
		if (m_profileIndex < m_profiles.Count - 1)
		{
			m_profileIndex++;
		}
		UpdateCharacterList();
	}

	public void OnCharacterStart()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)"OnCharacterStart");
		if (m_profileIndex < 0 || m_profileIndex >= m_profiles.Count)
		{
			return;
		}
		PlayerProfile playerProfile = m_profiles[m_profileIndex];
		PlatformPrefs.SetString("profile", playerProfile.GetFilename());
		Game.SetProfile(playerProfile.GetFilename(), playerProfile.m_fileSource);
		m_characterSelectScreen.SetActive(false);
		if (m_queuedJoinServer.IsValid)
		{
			m_joinServer = m_queuedJoinServer;
			m_queuedJoinServer = ServerJoinData.None;
			JoinServer();
			return;
		}
		ShowStartGame();
		if (m_worlds.Count == 0)
		{
			OnWorldNew();
		}
	}

	private void TransitionToMainScene()
	{
		m_menuAnimator.SetTrigger("FadeOut");
		((MonoBehaviour)this).Invoke("LoadMainSceneIfBackendSelected", 1.5f);
	}

	private void LoadMainSceneIfBackendSelected()
	{
		if (m_startingWorld || ZNet.HasServerHost())
		{
			ZLog.Log((object)"Loading main scene");
			LoadMainScene();
			return;
		}
		retries++;
		if (retries > 50)
		{
			ZLog.Log((object)"Max retries reached, reloading startup scene with connection error");
			ZNet.SetExternalError(ZNet.ConnectionStatus.ErrorConnectFailed);
			m_menuAnimator.SetTrigger("FadeIn");
			ShowConnectError(ZNet.ConnectionStatus.ErrorConnectFailed);
		}
		else
		{
			((MonoBehaviour)this).Invoke("LoadMainSceneIfBackendSelected", 0.25f);
			ZLog.Log((object)"Backend not retreived yet, checking again in 0.25 seconds...");
		}
	}

	private void LoadMainScene()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		m_loading.SetActive(true);
		SceneManager.LoadScene(m_mainScene, (LoadSceneMode)0);
		m_startingWorld = false;
	}

	public void OnButtonSettings()
	{
		m_mainMenu.SetActive(false);
		m_settingsPopup = Object.Instantiate<GameObject>(m_settingsPrefab, ((Component)this).transform);
		Settings component = m_settingsPopup.GetComponent<Settings>();
		component.SettingsClosed = (Action)Delegate.Combine(component.SettingsClosed, (Action)delegate
		{
			GameObject mainMenu = m_mainMenu;
			if (mainMenu != null)
			{
				mainMenu.SetActive(true);
			}
		});
	}

	public void OnButtonFeedback()
	{
		Object.Instantiate<GameObject>(m_feedbackPrefab, ((Component)this).transform);
	}

	public void OnButtonTwitter()
	{
		OpenBrowser("https://twitter.com/valheimgame");
	}

	public void OnButtonWebPage()
	{
		OpenBrowser("http://valheimgame.com/");
	}

	public void OnButtonDiscord()
	{
		OpenBrowser("https://discord.gg/44qXMJH");
	}

	public void OnButtonFacebook()
	{
		OpenBrowser("https://www.facebook.com/valheimgame/");
	}

	public void OnButtonShowLog()
	{
		Application.OpenURL(Application.persistentDataPath + "/");
	}

	private bool AcceptedNDA()
	{
		return PlatformPrefs.GetInt("accepted_nda", 0) == 1;
	}

	public void OnButtonNDAAccept()
	{
		PlatformPrefs.SetInt("accepted_nda", 1);
		m_ndaPanel.SetActive(false);
		m_mainMenu.SetActive(true);
	}

	public void OnButtonNDADecline()
	{
		Application.Quit();
	}

	public void OnConnectionFailedOk()
	{
		m_connectionFailedPanel.SetActive(false);
	}

	public Player GetPreviewPlayer()
	{
		if ((Object)(object)m_playerInstance != (Object)null)
		{
			return m_playerInstance.GetComponent<Player>();
		}
		return null;
	}

	private void ClearCharacterPreview()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_playerInstance))
		{
			Object.Instantiate<GameObject>(m_changeEffectPrefab, m_characterPreviewPoint.position, m_characterPreviewPoint.rotation);
			Object.Destroy((Object)(object)m_playerInstance);
			m_playerInstance = null;
		}
	}

	private void SetupCharacterPreview(PlayerProfile profile)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ClearCharacterPreview();
		ZNetView.m_forceDisableInit = true;
		GameObject val = Object.Instantiate<GameObject>(m_playerPrefab, m_characterPreviewPoint.position, m_characterPreviewPoint.rotation);
		ZNetView.m_forceDisableInit = false;
		Object.Destroy((Object)(object)val.GetComponent<Rigidbody>());
		Animator[] componentsInChildren = val.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].updateMode = (AnimatorUpdateMode)0;
		}
		Player component = val.GetComponent<Player>();
		if (profile != null)
		{
			try
			{
				profile.LoadPlayerData(component);
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)("Error loading player data: " + profile.GetPath() + ", error: " + ex.Message));
			}
		}
		m_playerInstance = val;
	}

	public void SetServerToJoin(ServerJoinData serverData)
	{
		m_joinServer = serverData;
	}

	public bool HasServerToJoin()
	{
		return m_joinServer.IsValid;
	}

	public ServerJoinData GetServerToJoin()
	{
		return m_joinServer;
	}
}
