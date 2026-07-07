using System;
using System.Collections.Generic;
using GUIFramework;
using NetworkingUtils;
using Splatform;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ServerListGui : MonoBehaviour
{
	private static ServerListGui s_instance;

	private const int c_MaxRecentServers = 11;

	private const string c_FavoriteListFileName = "favorite";

	private const string c_RecentListFileName = "recent";

	private const float c_TabAreaWidth = 515f;

	private const float c_TabSpacing = 5f;

	private List<IServerList> m_serverLists = new List<IServerList>();

	private LocalServerList m_favoriteServersList;

	private LocalServerList m_recentServersList;

	private List<ServerListEntryData> m_filteredList = new List<ServerListEntryData>();

	private int m_currentServerList = int.MaxValue;

	private bool m_isAwaitingServerAdd;

	private bool m_buttonsOutdated = true;

	private bool m_initialized;

	private bool m_filteredListOutdated;

	private bool m_updateServerListGui;

	private bool m_centerSelection;

	[SerializeField]
	private Button m_favoriteButton;

	[SerializeField]
	private Button m_removeButton;

	[SerializeField]
	private Button m_upButton;

	[SerializeField]
	private Button m_downButton;

	[SerializeField]
	private GameObject m_serverListTab;

	[SerializeField]
	private ConnectIcons m_connectIcons;

	[SerializeField]
	private FejdStartup m_startup;

	private UIGamePad m_uiGamePad;

	[Header("Join")]
	public float m_serverListElementStep = 32f;

	public RectTransform m_serverListRoot;

	public GameObject m_serverListElement;

	public ScrollRectEnsureVisible m_serverListEnsureVisible;

	public Button m_serverRefreshButton;

	public TextMeshProUGUI m_serverCount;

	public GuiInputField m_filterInputField;

	public RectTransform m_tooltipAnchor;

	public Button m_addServerButton;

	public GameObject m_addServerPanel;

	public Button m_addServerConfirmButton;

	public Button m_addServerCancelButton;

	public GuiInputField m_addServerTextInput;

	public TabHandler m_serverListTabHandler;

	public Button m_joinGameButton;

	private float m_serverListBaseSize;

	private List<GameObject> m_serverListTabs = new List<GameObject>();

	private List<ServerListElement> m_serverListElements = new List<ServerListElement>();

	private Dictionary<ServerJoinData, ServerListElement> m_tempJoinDataToElementMap = new Dictionary<ServerJoinData, ServerListElement>(200);

	private Stack<ServerListElement> m_serverListElementPool = new Stack<ServerListElement>();

	private List<ServerListEntryData> CurrentServerListFiltered
	{
		get
		{
			if (m_filteredListOutdated)
			{
				FilterList();
			}
			return m_filteredList;
		}
	}

	public static string GetServerListFolder(FileSource fileSource)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)fileSource != 1)
		{
			return "/serverlist/";
		}
		return "/serverlist_local/";
	}

	public static string GetServerListFolderPath(FileSource fileSource)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Utils.GetSaveDataPath(fileSource) + GetServerListFolder(fileSource);
	}

	public static FileLocation[] GetServerListLocations(string serverListFileName)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		List<FileLocation> list = new List<FileLocation>();
		if (FileHelpers.LocalStorageSupported)
		{
			list.Add(new FileLocation((FileSource)1, GetServerListFolderPath((FileSource)1) + serverListFileName));
		}
		if (FileHelpers.CloudStorageEnabled)
		{
			list.Add(new FileLocation((FileSource)2, GetServerListFolderPath((FileSource)2) + serverListFileName));
		}
		return list.ToArray();
	}

	private void Awake()
	{
		Initialize();
	}

	private void OnEnable()
	{
		if ((Object)(object)s_instance != (Object)null && (Object)(object)s_instance != (Object)(object)this)
		{
			ZLog.LogError((object)"More than one instance of ServerList!");
			return;
		}
		s_instance = this;
		for (int i = 0; i < m_serverLists.Count; i++)
		{
			m_serverLists[i].ServerListUpdated += OnCurrentServerListUpdated;
		}
		RecreateTabs();
		Update();
	}

	private void OnApplicationQuit()
	{
		SaveLocalServerLists();
	}

	private void OnDisable()
	{
		for (int i = 0; i < m_serverLists.Count; i++)
		{
			m_serverLists[i].ServerListUpdated -= OnCurrentServerListUpdated;
		}
		SaveLocalServerLists();
	}

	private void OnDestroy()
	{
		if ((Object)(object)s_instance != (Object)(object)this)
		{
			ZLog.LogError((object)"ServerList instance was not this!");
			return;
		}
		m_favoriteServersList.Dispose();
		m_recentServersList.Dispose();
		s_instance = null;
	}

	private void Update()
	{
		m_serverLists[m_currentServerList].Tick();
		UpdateInput();
		if (m_updateServerListGui)
		{
			UpdateServerListGuiInternal(m_centerSelection);
			m_updateServerListGui = false;
			m_centerSelection = false;
		}
		UpdateButtons();
	}

	private void SaveLocalServerLists(bool force = false)
	{
		SaveStatusCode saveStatusCode = m_favoriteServersList.Save(force);
		ZLog.Log((object)$"Saved favorite servers list with result {saveStatusCode}");
		saveStatusCode = m_recentServersList.Save(force);
		ZLog.Log((object)$"Saved recent servers list with result {saveStatusCode}");
	}

	private void UpdateInput()
	{
		if (!m_uiGamePad.IsBlocked())
		{
			UpdateGamepad();
			UpdateKeyboard();
		}
	}

	private void UpdateAddServerButtons()
	{
		if (m_addServerPanel.activeInHierarchy)
		{
			((Selectable)m_addServerConfirmButton).interactable = ((TMP_InputField)m_addServerTextInput).text.Length > 0 && !m_isAwaitingServerAdd;
			((Selectable)m_addServerCancelButton).interactable = !m_isAwaitingServerAdd;
		}
	}

	private void OnCurrentServerListUpdated()
	{
		m_filteredListOutdated = true;
		UpdateServerListGui(centerSelection: false);
		UpdateServerCount();
		bool flag = false;
		for (int i = 0; i < CurrentServerListFiltered.Count; i++)
		{
			if (CurrentServerListFiltered[i].m_joinData == m_startup.GetServerToJoin())
			{
				flag = true;
				break;
			}
		}
		if (m_startup.HasServerToJoin() && !flag)
		{
			ZLog.Log((object)"Serverlist does not contain selected server, clearing");
			if (CurrentServerListFiltered.Count > 0)
			{
				SetSelectedServer(0, centerSelection: true);
			}
			else
			{
				ClearSelectedServer();
			}
		}
	}

	private void Initialize()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (m_initialized)
		{
			ZLog.LogError((object)"Already initialized!");
			return;
		}
		m_initialized = true;
		((UnityEvent)m_favoriteButton.onClick).AddListener((UnityAction)delegate
		{
			OnFavoriteServerButton();
		});
		((UnityEvent)m_removeButton.onClick).AddListener((UnityAction)delegate
		{
			OnRemoveServerButton();
		});
		((UnityEvent)m_upButton.onClick).AddListener((UnityAction)delegate
		{
			OnMoveServerUpButton();
		});
		((UnityEvent)m_downButton.onClick).AddListener((UnityAction)delegate
		{
			OnMoveServerDownButton();
		});
		((UnityEvent<string>)(object)((TMP_InputField)m_filterInputField).onValueChanged).AddListener((UnityAction<string>)delegate
		{
			OnServerFilterChanged(isTyping: true);
		});
		((Component)m_addServerButton).gameObject.SetActive(true);
		if (PlatformPrefs.HasKey("LastIPJoined"))
		{
			PlatformPrefs.DeleteKey("LastIPJoined");
		}
		Rect rect = m_serverListRoot.rect;
		m_serverListBaseSize = ((Rect)(ref rect)).height;
		m_serverLists = new List<IServerList>();
		m_favoriteServersList = new LocalServerList("$menu_favorite", GetServerListLocations("favorite"));
		m_serverLists.Add(m_favoriteServersList);
		m_recentServersList = new LocalServerList("$menu_recent", GetServerListLocations("recent"));
		m_serverLists.Add(m_recentServersList);
		m_serverLists.Add(new FriendsServerList("$menu_friends"));
		m_serverLists.Add(new CommunityServerList("$menu_community"));
		m_uiGamePad = ((Component)this).GetComponent<UIGamePad>();
		if ((Object)(object)m_uiGamePad == (Object)null)
		{
			ZLog.LogError((object)"UI Gamepad component was null!");
		}
	}

	private void RecreateTabs()
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		for (int i = 0; i < m_serverListTabs.Count; i++)
		{
			Object.Destroy((Object)(object)m_serverListTabs[i]);
			m_serverListTabs[i] = null;
		}
		m_serverListTabs.Clear();
		m_serverListTabHandler.m_tabs.Clear();
		float x = (515f - 5f * (float)(m_serverLists.Count - 1)) / (float)m_serverLists.Count;
		for (int j = 0; j < m_serverLists.Count; j++)
		{
			GameObject val = Object.Instantiate<GameObject>(m_serverListTab, ((Component)this).transform);
			m_serverListTabs.Add(val);
			val.transform.SetSiblingIndex(j);
			val.SetActive(true);
			SetHint(val, j);
			TMP_Text componentInChildren = val.GetComponentInChildren<TMP_Text>();
			if (componentInChildren == null)
			{
				ZLog.LogError((object)"Couldn't find server list tab text component!");
			}
			else
			{
				componentInChildren.text = Localization.instance.Localize(m_serverLists[j].DisplayName);
			}
			RectTransform component = val.GetComponent<RectTransform>();
			if (component == null)
			{
				ZLog.LogError((object)"Couldn't find server list tab rect transform!");
			}
			else
			{
				Vector2 sizeDelta = component.sizeDelta;
				sizeDelta.x = x;
				component.sizeDelta = sizeDelta;
				Vector3 localPosition = ((Transform)component).localPosition;
				ref float x2 = ref localPosition.x;
				float num = x2;
				float num2 = j;
				Rect rect = component.rect;
				x2 = num + num2 * (((Rect)(ref rect)).width + 5f);
				((Transform)component).localPosition = localPosition;
			}
			UnityEvent val2 = new UnityEvent();
			val2.AddListener(new UnityAction(OnTab));
			TabHandler.Tab item = new TabHandler.Tab
			{
				m_button = val.GetComponent<Button>(),
				m_page = null,
				m_default = (j == 0),
				m_onClick = val2
			};
			m_serverListTabHandler.m_tabs.Add(item);
		}
		if (PlatformPrefs.HasKey("publicfilter"))
		{
			PlatformPrefs.DeleteKey("publicfilter");
		}
		int @int = PlatformPrefs.GetInt("serverListTab", 0);
		m_serverListTabHandler.SetActiveTab(@int, forceSelect: true);
		m_serverListTabHandler.Init(forceSelect: true);
	}

	private void SetHint(GameObject tabObject, int index)
	{
		UIGamePad component = tabObject.GetComponent<UIGamePad>();
		component.m_hint = null;
		if (m_serverLists.Count <= 1)
		{
			return;
		}
		bool flag = index == 0;
		bool flag2 = index == m_serverLists.Count - 1;
		if (flag || flag2)
		{
			Transform val = tabObject.transform.Find(flag ? "gamepad_hint_left" : "gamepad_hint_right");
			if (val == null)
			{
				ZLog.LogError((object)"Couldn't find server list tab hint object!");
			}
			else
			{
				component.m_hint = ((Component)val).gameObject;
			}
		}
	}

	public void FilterList()
	{
		m_serverLists[m_currentServerList].GetFilteredList(m_filteredList);
		m_filteredListOutdated = false;
	}

	private void UpdateButtons()
	{
		UpdateServerRefreshInteractability();
		UpdateAddServerButtons();
		if (m_buttonsOutdated)
		{
			m_buttonsOutdated = false;
			int selectedServer = GetSelectedServer();
			bool flag = selectedServer >= 0;
			bool flag2 = flag && m_favoriteServersList.Contains(CurrentServerListFiltered[selectedServer].m_joinData);
			if (m_serverLists[m_currentServerList] == m_favoriteServersList)
			{
				((Selectable)m_upButton).interactable = flag && selectedServer != 0;
				((Selectable)m_downButton).interactable = flag && selectedServer != CurrentServerListFiltered.Count - 1;
				((Selectable)m_removeButton).interactable = flag;
				((Selectable)m_favoriteButton).interactable = flag && ((Object)(object)m_removeButton == (Object)null || !((Component)m_removeButton).gameObject.activeSelf);
			}
			else if (m_serverLists[m_currentServerList] == m_recentServersList)
			{
				((Selectable)m_favoriteButton).interactable = flag && !flag2;
				((Selectable)m_removeButton).interactable = flag;
			}
			else
			{
				((Selectable)m_favoriteButton).interactable = flag && !flag2;
			}
			((Selectable)m_joinGameButton).interactable = flag;
		}
	}

	private void UpdateServerRefreshInteractability()
	{
		bool flag = true;
		flag &= (DateTime.UtcNow - m_serverLists[m_currentServerList].LastRefreshTimeUtc).TotalSeconds > 1.0;
		flag &= m_serverLists[m_currentServerList].CanRefresh;
		((Selectable)m_serverRefreshButton).interactable = flag;
	}

	private void SetServerFilter(string filter)
	{
		((TMP_InputField)m_filterInputField).text = filter;
		OnServerFilterChanged();
	}

	private void OnTab()
	{
		int activeTab = m_serverListTabHandler.GetActiveTab();
		if (m_currentServerList != activeTab)
		{
			m_filteredListOutdated = true;
			if (m_currentServerList >= 0 && m_currentServerList < m_serverLists.Count)
			{
				m_serverLists[m_currentServerList].OnClose();
			}
			m_currentServerList = activeTab;
			m_serverLists[m_currentServerList].OnOpen();
			SetServerFilter("");
			PlatformPrefs.SetInt("serverListTab", m_serverListTabHandler.GetActiveTab());
			UpdateServerListGui(centerSelection: true);
			UpdateServerCount();
			UpdateLocalServerListSelection();
			m_serverLists[m_currentServerList].Tick();
			ResetListManipulationButtons();
			if (m_serverLists[m_currentServerList] == m_favoriteServersList)
			{
				((Component)m_removeButton).gameObject.SetActive(true);
			}
			else
			{
				((Component)m_favoriteButton).gameObject.SetActive(true);
			}
		}
	}

	public void OnFavoriteServerButton()
	{
		if (((Object)(object)m_removeButton == (Object)null || !((Component)m_removeButton).gameObject.activeSelf) && m_serverLists[m_currentServerList] == m_favoriteServersList)
		{
			OnRemoveServerButton();
			return;
		}
		int selectedServer = GetSelectedServer();
		ServerListEntryData serverListEntryData = CurrentServerListFiltered[selectedServer];
		MultiBackendMatchmaking.SetServerName(serverListEntryData.m_joinData, new ServerNameAtTimePoint(serverListEntryData.m_serverName, serverListEntryData.m_timeStampUtc));
		m_favoriteServersList.Add(serverListEntryData.m_joinData);
		SetButtonsOutdated();
	}

	public void OnRemoveServerButton()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		int selectedServer = GetSelectedServer();
		UnifiedPopup.Push(new YesNoPopup("$menu_removeserver", CensorShittyWords.FilterUGC(CurrentServerListFiltered[selectedServer].m_serverName, UGCType.ServerName, default(PlatformUserID), 0L), delegate
		{
			OnRemoveServerConfirm();
		}, delegate
		{
			UnifiedPopup.Pop();
		}));
	}

	public void OnMoveServerUpButton()
	{
		int selectedServer = GetSelectedServer();
		m_favoriteServersList.Swap(selectedServer, selectedServer - 1);
		m_filteredListOutdated = true;
		UpdateServerListGui(centerSelection: true);
	}

	public void OnMoveServerDownButton()
	{
		int selectedServer = GetSelectedServer();
		m_favoriteServersList.Swap(selectedServer, selectedServer + 1);
		m_filteredListOutdated = true;
		UpdateServerListGui(centerSelection: true);
	}

	private void OnRemoveServerConfirm()
	{
		if (m_serverLists[m_currentServerList] != m_favoriteServersList)
		{
			ZLog.LogError((object)"Can't remove server from invalid list!");
			return;
		}
		int selectedServer = GetSelectedServer();
		ServerJoinData joinData = CurrentServerListFiltered[selectedServer].m_joinData;
		if (!m_favoriteServersList.TryGetIndexOf(joinData, out var index))
		{
			ZLog.LogError((object)"Selected server was not in the favorites list!");
			return;
		}
		m_favoriteServersList.Remove(m_favoriteServersList[index]);
		m_filteredListOutdated = true;
		if (CurrentServerListFiltered.Count <= 0 && ((TMP_InputField)m_filterInputField).text != "")
		{
			((TMP_InputField)m_filterInputField).text = "";
			OnServerFilterChanged();
			m_startup.SetServerToJoin(ServerJoinData.None);
		}
		else
		{
			UpdateLocalServerListSelection();
			SetSelectedServer(selectedServer, centerSelection: true);
		}
		UnifiedPopup.Pop();
	}

	private void ResetListManipulationButtons()
	{
		((Component)m_favoriteButton).gameObject.SetActive(false);
		((Component)m_removeButton).gameObject.SetActive(false);
		((Selectable)m_favoriteButton).interactable = false;
		((Selectable)m_upButton).interactable = false;
		((Selectable)m_downButton).interactable = false;
		((Selectable)m_removeButton).interactable = false;
	}

	private void SetButtonsOutdated()
	{
		m_buttonsOutdated = true;
	}

	private void UpdateServerListGui(bool centerSelection)
	{
		m_updateServerListGui = true;
		m_centerSelection |= centerSelection;
	}

	private void UpdateServerListGuiInternal(bool centerSelection)
	{
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Expected O, but got Unknown
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_serverListElements.Count; i++)
		{
			if (m_tempJoinDataToElementMap.TryGetValue(m_serverListElements[i].Server, out var _))
			{
				ZLog.LogWarning((object)("Join data " + m_serverListElements[i].Server.ToString() + " already has a server list element, even though duplicates are not allowed! Discarding this element.\nWhile this warning itself is fine, it might be an indication of a bug that may cause navigation issues in the server list."));
				Object.Destroy((Object)(object)m_serverListElements[i].m_element);
			}
			else
			{
				m_tempJoinDataToElementMap.Add(m_serverListElements[i].Server, m_serverListElements[i]);
			}
		}
		m_serverListElements.Clear();
		float num = 0f;
		for (int j = 0; j < CurrentServerListFiltered.Count; j++)
		{
			ServerListEntryData serverEntry = CurrentServerListFiltered[j];
			ServerListElement serverListElement;
			if (m_tempJoinDataToElementMap.ContainsKey(serverEntry.m_joinData))
			{
				serverListElement = m_tempJoinDataToElementMap[serverEntry.m_joinData];
				m_serverListElements.Add(serverListElement);
				m_tempJoinDataToElementMap.Remove(serverEntry.m_joinData);
			}
			else if (m_serverListElementPool.Count > 0)
			{
				serverListElement = m_serverListElementPool.Pop();
				serverListElement.m_element.SetActive(true);
				m_serverListElements.Add(serverListElement);
				((UnityEvent)serverListElement.m_button.onClick).AddListener((UnityAction)delegate
				{
					OnSelectedServer(serverEntry.m_joinData);
				});
			}
			else
			{
				GameObject obj = Object.Instantiate<GameObject>(m_serverListElement, (Transform)(object)m_serverListRoot);
				obj.SetActive(true);
				serverListElement = new ServerListElement(obj);
				((UnityEvent)serverListElement.m_button.onClick).AddListener((UnityAction)delegate
				{
					OnSelectedServer(serverEntry.m_joinData);
				});
				m_serverListElements.Add(serverListElement);
			}
			serverListElement.m_rectTransform.anchoredPosition = new Vector2(0f, 0f - num);
			num += serverListElement.m_rectTransform.sizeDelta.y;
			bool flag = m_startup.HasServerToJoin() && m_startup.GetServerToJoin().Equals(serverEntry.m_joinData);
			if (centerSelection && flag)
			{
				m_serverListEnsureVisible.CenterOnItem(serverListElement.m_selected);
			}
			serverListElement.UpdateDisplayData(ref serverEntry, flag, m_tooltipAnchor, ref m_connectIcons);
		}
		foreach (ServerListElement value2 in m_tempJoinDataToElementMap.Values)
		{
			value2.m_element.SetActive(false);
			m_serverListElementPool.Push(value2);
			((UnityEventBase)value2.m_button.onClick).RemoveAllListeners();
		}
		m_tempJoinDataToElementMap.Clear();
		m_serverListRoot.SetSizeWithCurrentAnchors((Axis)1, Mathf.Max(num, m_serverListBaseSize));
		SetButtonsOutdated();
	}

	private void UpdateServerCount()
	{
		uint totalServers = m_serverLists[m_currentServerList].TotalServers;
		uint num = 0u;
		for (int i = 0; i < CurrentServerListFiltered.Count; i++)
		{
			if (CurrentServerListFiltered[i].IsOnline)
			{
				num++;
			}
		}
		((TMP_Text)m_serverCount).text = $"{num} / {totalServers}";
	}

	private void OnSelectedServer(ServerJoinData selected)
	{
		m_startup.SetServerToJoin(selected);
		UpdateServerListGui(centerSelection: false);
	}

	private void SetSelectedServer(int index, bool centerSelection)
	{
		if (CurrentServerListFiltered.Count == 0)
		{
			if (m_startup.HasServerToJoin())
			{
				ZLog.Log((object)"Serverlist is empty, clearing selection");
			}
			ClearSelectedServer();
		}
		else
		{
			index = Mathf.Clamp(index, 0, CurrentServerListFiltered.Count - 1);
			m_startup.SetServerToJoin(CurrentServerListFiltered[index].m_joinData);
			UpdateServerListGui(centerSelection);
		}
	}

	private int GetSelectedServer()
	{
		if (!m_startup.HasServerToJoin())
		{
			return -1;
		}
		for (int i = 0; i < CurrentServerListFiltered.Count; i++)
		{
			if (m_startup.GetServerToJoin() == CurrentServerListFiltered[i].m_joinData)
			{
				return i;
			}
		}
		return -1;
	}

	private void ClearSelectedServer()
	{
		m_startup.SetServerToJoin(ServerJoinData.None);
		SetButtonsOutdated();
	}

	private int FindSelectedServer(GameObject button)
	{
		for (int i = 0; i < m_serverListElements.Count; i++)
		{
			if ((Object)(object)m_serverListElements[i].m_element == (Object)(object)button)
			{
				return i;
			}
		}
		return -1;
	}

	private void UpdateLocalServerListSelection()
	{
		if (GetSelectedServer() < 0)
		{
			ClearSelectedServer();
			UpdateServerListGui(centerSelection: true);
		}
	}

	public void OnRefreshButton()
	{
		RequestServerList();
	}

	public static void Refresh()
	{
		if (!((Object)(object)s_instance == (Object)null))
		{
			s_instance.RequestServerList();
		}
	}

	public static void UpdateServerListGuiStatic()
	{
		if (!((Object)(object)s_instance == (Object)null))
		{
			s_instance.UpdateServerListGui(centerSelection: false);
		}
	}

	public void RequestServerList()
	{
		ZLog.DevLog((object)"Request serverlist");
		if (!((Selectable)m_serverRefreshButton).interactable)
		{
			ZLog.DevLog((object)"Server queue already running");
			return;
		}
		m_serverLists[m_currentServerList].Refresh();
		UpdateServerRefreshInteractability();
	}

	public void OnServerFilterChanged(bool isTyping = false)
	{
		m_serverLists[m_currentServerList].SetFilter(((TMP_InputField)m_filterInputField).text, isTyping);
		m_filteredListOutdated = true;
		UpdateServerListGui(centerSelection: true);
		UpdateServerCount();
	}

	private void UpdateGamepad()
	{
		if (ZInput.IsGamepadActive())
		{
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				SetSelectedServer(GetSelectedServer() + 1, centerSelection: true);
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				SetSelectedServer(GetSelectedServer() - 1, centerSelection: true);
			}
		}
	}

	private void UpdateKeyboard()
	{
		if (ZInput.GetKeyDown((KeyCode)114, true) && !((TMP_InputField)m_filterInputField).isFocused)
		{
			RequestServerList();
		}
		UpdateKeyboardSelection();
		UpdateKeyboardMoveServer();
	}

	private void UpdateKeyboardSelection()
	{
		if (ZInput.GetKeyDown((KeyCode)273, true))
		{
			SetSelectedServer(GetSelectedServer() - 1, centerSelection: true);
		}
		if (ZInput.GetKeyDown((KeyCode)274, true))
		{
			SetSelectedServer(GetSelectedServer() + 1, centerSelection: true);
		}
	}

	private void UpdateKeyboardMoveServer()
	{
		if (((TMP_InputField)m_filterInputField).isFocused || m_serverLists[m_currentServerList] != m_favoriteServersList)
		{
			return;
		}
		int num = 0;
		num += (ZInput.GetKeyDown((KeyCode)119, true) ? (-1) : 0);
		num += (ZInput.GetKeyDown((KeyCode)115, true) ? 1 : 0);
		if (num != 0)
		{
			int selectedServer = GetSelectedServer();
			if (num > 0 && selectedServer + num < m_favoriteServersList.Count)
			{
				OnMoveServerDownButton();
			}
			else if (num < 0 && selectedServer + num >= 0)
			{
				OnMoveServerUpButton();
			}
		}
	}

	public static void AddToRecentServersList(ServerJoinData data)
	{
		if (!data.IsValid)
		{
			ZLog.LogError((object)$"Couldn't add server to server list, server data {data} was invalid!");
			return;
		}
		if ((Object)(object)s_instance != (Object)null)
		{
			s_instance.AddToRecentServersListCached(data);
			return;
		}
		LocalServerList localServerList = new LocalServerList(null, GetServerListLocations("recent"));
		localServerList.Remove(data);
		localServerList.AddToBeginning(data);
		while (localServerList.Count > 11)
		{
			localServerList.Remove(localServerList[localServerList.Count - 1]);
		}
		SaveStatusCode saveStatusCode = localServerList.Save();
		switch (saveStatusCode)
		{
		case SaveStatusCode.CloudQuotaExceeded:
			ZLog.LogWarning((object)("Couln't add server " + data.ToString() + " to server list, cloud quota exceeded."));
			break;
		default:
			ZLog.LogError((object)$"Couln't add server {data.ToString()} to server list: {saveStatusCode}");
			break;
		case SaveStatusCode.Succeess:
			ZLog.Log((object)("Added server " + data.ToString() + " to server list"));
			break;
		}
		localServerList.Dispose();
	}

	private void AddToRecentServersListCached(ServerJoinData data)
	{
		m_recentServersList.Remove(data);
		m_recentServersList.AddToBeginning(data);
		while (m_recentServersList.Count > 11)
		{
			m_recentServersList.Remove(m_recentServersList[m_recentServersList.Count - 1]);
		}
		ZLog.Log((object)("Added server with name " + MultiBackendMatchmaking.GetServerName(data) + " to server list"));
	}

	public void OnAddServerOpen()
	{
		if (!((TMP_InputField)m_filterInputField).isFocused)
		{
			m_addServerPanel.SetActive(true);
		}
	}

	public void OnAddServerClose()
	{
		m_addServerPanel.SetActive(false);
	}

	public void OnAddServer()
	{
		m_addServerPanel.SetActive(true);
		string text = ((TMP_InputField)m_addServerTextInput).text;
		string[] array = text.Split(':');
		if (array.Length == 0)
		{
			return;
		}
		if (array.Length == 1)
		{
			string text2 = array[0];
			if (ZPlayFabMatchmaking.IsJoinCode(text2))
			{
				if (PlayFabManager.IsLoggedIn)
				{
					OnManualAddToFavoritesStart();
					MultiBackendMatchmaking.PlayFabBackend.ResolveJoinCode(text2, OnPlayFabJoinCodeSuccess, OnJoinCodeFailed);
				}
				else
				{
					OnJoinCodeFailed(ZPLayFabMatchmakingFailReason.NotLoggedIn);
				}
				return;
			}
		}
		ServerJoinDataUtils.GetAddressAndPortFromString(text, out var ipAddress, out var _);
		if (!string.IsNullOrEmpty(ipAddress))
		{
			ServerJoinDataDedicated newServerListEntryDedicated = new ServerJoinDataDedicated(text);
			OnManualAddToFavoritesStart();
			MultiBackendMatchmaking.GetServerIPAsync(newServerListEntryDedicated, delegate(bool success, IPv6Address? address)
			{
				if (success && address.HasValue)
				{
					OnManualAddToFavoritesSuccess(new ServerJoinData(newServerListEntryDedicated));
				}
				else
				{
					if (newServerListEntryDedicated.IsURL)
					{
						UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$menu_addserverfaileddnslookup", delegate
						{
							UnifiedPopup.Pop();
						}));
					}
					else
					{
						UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$menu_addserverfailedincorrectformatting", delegate
						{
							UnifiedPopup.Pop();
						}));
					}
					m_isAwaitingServerAdd = false;
				}
			});
		}
		else
		{
			UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$menu_addserverfailedincorrectformatting", delegate
			{
				UnifiedPopup.Pop();
			}));
		}
	}

	private void OnManualAddToFavoritesStart()
	{
		m_isAwaitingServerAdd = true;
	}

	private void OnManualAddToFavoritesSuccess(ServerJoinData newFavoriteServer)
	{
		if (!m_favoriteServersList.Contains(newFavoriteServer))
		{
			m_favoriteServersList.Add(newFavoriteServer);
		}
		m_filteredListOutdated = true;
		m_serverListTabHandler.SetActiveTab(0);
		m_startup.SetServerToJoin(newFavoriteServer);
		UpdateServerListGui(centerSelection: true);
		OnAddServerClose();
		((TMP_InputField)m_addServerTextInput).text = "";
		m_isAwaitingServerAdd = false;
	}

	private void OnPlayFabJoinCodeSuccess(ServerData serverData)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (!serverData.m_joinData.IsValid || serverData.m_matchmakingData.m_networkVersion != 36)
		{
			UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$error_incompatibleversion", delegate
			{
				UnifiedPopup.Pop();
			}));
			m_isAwaitingServerAdd = false;
		}
		else if (!serverData.m_matchmakingData.IsCrossplay && !serverData.m_matchmakingData.IsRestrictedToOwnPlatform)
		{
			UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$error_platformexcluded", delegate
			{
				UnifiedPopup.Pop();
			}));
			m_isAwaitingServerAdd = false;
		}
		else if (serverData.m_matchmakingData.IsCrossplay && (int)PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)1) != 0)
		{
			if (PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege != null)
			{
				PlatformManager.DistributionPlatform.UIProvider.ResolvePrivilege.Open((Privilege)1, (PrivilegeResult)64);
			}
			else
			{
				UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$xbox_error_crossplayprivilege", delegate
				{
					UnifiedPopup.Pop();
				}));
			}
			m_isAwaitingServerAdd = false;
		}
		else
		{
			ZPlayFabMatchmaking.JoinCode = serverData.m_matchmakingData.m_joinCode;
			OnManualAddToFavoritesSuccess(serverData.m_joinData);
		}
	}

	private void OnJoinCodeFailed(ZPLayFabMatchmakingFailReason failReason)
	{
		ZLog.Log((object)("Failed to resolve join code for the following reason: " + failReason));
		m_isAwaitingServerAdd = false;
		UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$menu_addserverfailedresolvejoincode", delegate
		{
			UnifiedPopup.Pop();
		}));
	}
}
