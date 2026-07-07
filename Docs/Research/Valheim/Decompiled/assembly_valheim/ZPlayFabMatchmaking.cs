using System;
using System.Collections.Generic;
using System.Threading;
using NetworkingUtils;
using PartyCSharpSDK;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using PlayFab.Party;
using Splatform;
using UnityEngine;

public class ZPlayFabMatchmaking
{
	private enum State
	{
		Uninitialized,
		Creating,
		RegenerateJoinCode,
		Active
	}

	private static ZPlayFabMatchmaking m_instance;

	private static string m_publicIP = "";

	private static readonly object m_mtx = new object();

	private static Thread m_publicIpLookupThread;

	private static int m_getPublicIpAttempts;

	public const uint JoinStringLength = 6u;

	public const uint MaxPlayers = 10u;

	internal const int NumSearchPages = 4;

	public const string RemotePlayerIdSearchKey = "string_key1";

	public const string IsActiveSearchKey = "string_key2";

	public const string IsCommunityServerSearchKey = "string_key3";

	public const string JoinCodeSearchKey = "string_key4";

	public const string ServerNameSearchKey = "string_key5";

	public const string GameVersionSearchKey = "string_key6";

	public const string IsDedicatedServerSearchKey = "string_key7";

	public const string PlatformUserIdSearchKey = "string_key8";

	public const string CreatedSearchKey = "string_key9";

	public const string ServerIpSearchKey = "string_key10";

	public const string PageSearchKey = "number_key11";

	public const string PlatformRestrictionKey = "string_key12";

	public const string NetworkVersionSearchKey = "number_key13";

	public const string ModifiersSearchKey = "string_key14";

	private const int NumStringSearchKeys = 14;

	public const string NoPlatformRestrictionString = "None";

	private State m_state;

	private PlayFabMatchmakingServerData m_serverData;

	private int m_retries;

	private float m_retryIn = -1f;

	private const float LostNetworkRetryDuration = 30f;

	private float m_lostNetworkRetryIn = -1f;

	private bool m_isConnectingToNetwork;

	private bool m_isResettingNetwork;

	private float m_submitBackgroundSearchIn = -1f;

	private int m_serverPort = -1;

	private float m_refreshLobbyTimer;

	private const float RefreshLobbyDurationMin = 540f;

	private const float RefreshLobbyDurationMax = 840f;

	private const float DurationBetwenBackgroundSearches = 2f;

	private readonly List<ZPlayFabLobbySearch> m_activeSearches = new List<ZPlayFabLobbySearch>();

	private readonly Queue<ZPlayFabLobbySearch> m_pendingSearches = new Queue<ZPlayFabLobbySearch>();

	private Action m_pendingRegisterServer;

	public static ZPlayFabMatchmaking instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new ZPlayFabMatchmaking();
			}
			return m_instance;
		}
	}

	public static string JoinCode { get; internal set; }

	public static string PublicIP
	{
		get
		{
			lock (m_mtx)
			{
				return m_publicIP;
			}
		}
		private set
		{
			lock (m_mtx)
			{
				m_publicIP = value;
			}
		}
	}

	public static event ZPlayFabMatchmakeLobbyLeftCallback LobbyLeft;

	public static void Initialize(bool isServer)
	{
		JoinCode = (isServer ? "" : "000000");
	}

	public void Update(float deltaTime)
	{
		if (!ReconnectNetwork(deltaTime))
		{
			RefreshLobby(deltaTime);
			RetryJoinCodeUniquenessCheck(deltaTime);
			UpdateActiveLobbySearches(deltaTime);
			UpdateBackgroundLobbySearches(deltaTime);
		}
	}

	private bool IsJoinedToNetwork()
	{
		if (m_serverData != null)
		{
			return !string.IsNullOrEmpty(m_serverData.networkId);
		}
		return false;
	}

	private bool IsReconnectNetworkTimerActive()
	{
		return m_lostNetworkRetryIn > 0f;
	}

	private void StartReconnectNetworkTimer(int code = -1)
	{
		m_lostNetworkRetryIn = 30f;
		if (DoFastRecovery(code))
		{
			ZLog.Log((object)"PlayFab host fast recovery");
			m_lostNetworkRetryIn = 12f;
		}
	}

	private static bool DoFastRecovery(int code)
	{
		if (code != 63)
		{
			return code == 11;
		}
		return true;
	}

	private void StopReconnectNetworkTimer()
	{
		m_isResettingNetwork = false;
		m_lostNetworkRetryIn = -1f;
		if (m_serverData != null && !IsJoinedToNetwork())
		{
			CreateAndJoinNetwork();
		}
	}

	private bool ReconnectNetwork(float deltaTime)
	{
		if (!IsReconnectNetworkTimerActive())
		{
			if (IsJoinedToNetwork() && !PlayFabMultiplayerManager.Get().IsConnectedToNetworkState())
			{
				PlayFabMultiplayerManager.Get().ResetParty();
				StartReconnectNetworkTimer();
				m_serverData.networkId = null;
			}
			return false;
		}
		m_lostNetworkRetryIn -= deltaTime;
		if (m_lostNetworkRetryIn <= 0f)
		{
			ZLog.Log((object)$"PlayFab reconnect server '{m_serverData.serverName}'");
			m_isConnectingToNetwork = false;
			m_serverData.networkId = null;
			StopReconnectNetworkTimer();
		}
		else if (!m_isConnectingToNetwork && !m_isResettingNetwork && m_lostNetworkRetryIn <= 12f)
		{
			PlayFabMultiplayerManager.Get().ResetParty();
			m_isResettingNetwork = true;
			m_isConnectingToNetwork = false;
		}
		return true;
	}

	private void StartRefreshLobbyTimer()
	{
		m_refreshLobbyTimer = Random.Range(540f, 840f);
	}

	private void RefreshLobby(float deltaTime)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		if (m_serverData == null || m_serverData.networkId == null)
		{
			return;
		}
		bool flag = m_serverData.isDedicatedServer && string.IsNullOrEmpty(m_serverData.serverIp) && !string.IsNullOrEmpty(PublicIP);
		m_refreshLobbyTimer -= deltaTime;
		if (m_refreshLobbyTimer < 0f || flag)
		{
			StartRefreshLobbyTimer();
			UpdateLobbyRequest val = new UpdateLobbyRequest
			{
				LobbyId = m_serverData.lobbyId
			};
			if (flag)
			{
				m_serverData.serverIp = GetServerIPAndPort();
				ZLog.Log((object)("Updating lobby with public IP " + m_serverData.serverIp));
				Dictionary<string, string> searchData = new Dictionary<string, string> { ["string_key10"] = m_serverData.serverIp };
				val.SearchData = searchData;
			}
			PlayFabMultiplayerAPI.UpdateLobby(val, (Action<LobbyEmptyResult>)delegate
			{
				ZLog.Log((object)$"Lobby {m_serverData.lobbyId} for world '{m_serverData.serverName}' and network {m_serverData.networkId} refreshed");
			}, (Action<PlayFabError>)OnRefreshFailed, (object)null, (Dictionary<string, string>)null);
		}
	}

	private void OnRefreshFailed(PlayFabError err)
	{
		CreateLobby(refresh: true, delegate
		{
			ZLog.Log((object)$"Lobby {m_serverData.lobbyId} for world '{m_serverData.serverName}' recreated");
		}, delegate(PlayFabError err)
		{
			ZLog.LogWarning((object)$"Failed to refresh lobby {m_serverData.lobbyId} for world '{m_serverData.serverName}': {err.GenerateErrorReport()}");
		});
	}

	private void RetryJoinCodeUniquenessCheck(float deltaTime)
	{
		if (m_retryIn > 0f)
		{
			m_retryIn -= deltaTime;
			if (m_retryIn <= 0f)
			{
				CheckJoinCodeIsUnique();
			}
		}
	}

	private void UpdateActiveLobbySearches(float deltaTime)
	{
		for (int i = 0; i < m_activeSearches.Count; i++)
		{
			ZPlayFabLobbySearch zPlayFabLobbySearch = m_activeSearches[i];
			if (zPlayFabLobbySearch.IsDone)
			{
				m_activeSearches.RemoveAt(i);
				i--;
			}
			else
			{
				zPlayFabLobbySearch.Update(deltaTime);
			}
		}
	}

	private void UpdateBackgroundLobbySearches(float deltaTime)
	{
		if (m_submitBackgroundSearchIn >= 0f)
		{
			m_submitBackgroundSearchIn -= deltaTime;
		}
		else if (m_pendingSearches.Count > 0)
		{
			m_submitBackgroundSearchIn = 2f;
			ZPlayFabLobbySearch zPlayFabLobbySearch = m_pendingSearches.Dequeue();
			zPlayFabLobbySearch.FindLobby();
			m_activeSearches.Add(zPlayFabLobbySearch);
		}
	}

	private void OnFailed(string what, PlayFabError error)
	{
		ZLog.LogError((object)("PlayFab " + what + " failed: " + ((object)error).ToString()));
		UnregisterServer();
	}

	private void OnSessionUpdated(State newState)
	{
		m_state = newState;
		switch (m_state)
		{
		case State.Creating:
			ZLog.Log((object)$"Session \"{m_serverData.serverName}\" registered with join code {JoinCode}");
			m_retries = 100;
			CheckJoinCodeIsUnique();
			break;
		case State.RegenerateJoinCode:
			RegenerateLobbyJoinCode();
			ZLog.Log((object)$"Created new join code {JoinCode} for session \"{m_serverData.serverName}\"");
			break;
		case State.Active:
			ZLog.Log((object)$"Session \"{m_serverData.serverName}\" with join code {JoinCode} is active with {m_serverData.numPlayers} player(s)");
			break;
		}
	}

	private void SetPlatformMatchmakingData()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		IMatchmakingProvider matchmakingProvider = PlatformManager.DistributionPlatform.MatchmakingProvider;
		if (matchmakingProvider != null)
		{
			MultiplayerSessionData val = default(MultiplayerSessionData);
			val.m_connectionString = m_serverData.remotePlayerId;
			val.m_maxPlayers = 10u;
			val.m_currentPlayers = m_serverData.numPlayers;
			val.m_joinRestriction = (MultiplayerJoinRestriction)1;
			MultiplayerSessionData multiplayerSession = val;
			matchmakingProvider.SetMultiplayerSession(multiplayerSession);
		}
	}

	private void ClearPlatformMatchmakingData()
	{
		IMatchmakingProvider matchmakingProvider = PlatformManager.DistributionPlatform.MatchmakingProvider;
		if (matchmakingProvider != null)
		{
			matchmakingProvider.ClearMultiplayerSession();
		}
	}

	private void UpdateNumPlayers(string info)
	{
		m_serverData.numPlayers = ZPlayFabSocket.NumSockets();
		if (!m_serverData.isDedicatedServer)
		{
			m_serverData.numPlayers++;
		}
		ZLog.Log((object)$"{info} server \"{m_serverData.serverName}\" that has join code {JoinCode}, now {m_serverData.numPlayers} player(s)");
	}

	private void OnRemotePlayerLeft(object sender, PlayFabPlayer player)
	{
		if (player == null)
		{
			ZLog.LogWarning((object)"Player that left was null! Ignoring.");
			return;
		}
		ZPlayFabSocket.LostConnection(player);
		UpdateNumPlayers("Player connection lost");
	}

	private void OnRemotePlayerJoined(object sender, PlayFabPlayer player)
	{
		StopReconnectNetworkTimer();
		ZPlayFabSocket.QueueConnection(player);
		UpdateNumPlayers("Player joined");
	}

	private void OnNetworkJoined(object sender, string networkId)
	{
		ZLog.Log((object)$"Joined PlayFab Party network with ID \"{networkId}\"");
		if (m_serverData.networkId == null || m_serverData.networkId != networkId)
		{
			m_serverData.networkId = networkId;
			CreateLobby(refresh: false, OnCreateLobbySuccess, delegate(PlayFabError error)
			{
				OnFailed("create lobby", error);
			});
		}
		m_isConnectingToNetwork = false;
		m_isResettingNetwork = false;
		StopReconnectNetworkTimer();
		StartRefreshLobbyTimer();
	}

	private void CreateLobby(bool refresh, Action<CreateLobbyResult> resultCallback, Action<PlayFabError> errorCallback)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Expected O, but got Unknown
		EntityKey entityKeyForLocalUser = GetEntityKeyForLocalUser();
		List<Member> members = new List<Member>
		{
			new Member
			{
				MemberEntity = entityKeyForLocalUser
			}
		};
		Dictionary<string, string> lobbyData = new Dictionary<string, string>
		{
			[PlayFabAttrKey.HavePassword.ToKeyString()] = m_serverData.havePassword.ToString(),
			[PlayFabAttrKey.WorldName.ToKeyString()] = m_serverData.worldName,
			[PlayFabAttrKey.NetworkId.ToKeyString()] = m_serverData.networkId
		};
		string value = "";
		if (ServerOptionsGUI.TryConvertModifierKeysToCompactKVP<Dictionary<string, string>>(m_serverData.modifiers, out var result))
		{
			value = StringUtils.EncodeDictionaryAsString((IDictionary<string, string>)result, false);
		}
		Dictionary<string, string> obj = new Dictionary<string, string>
		{
			["string_key9"] = DateTime.UtcNow.Ticks.ToString(),
			["string_key5"] = m_serverData.serverName,
			["string_key3"] = m_serverData.isCommunityServer.ToString(),
			["string_key4"] = m_serverData.joinCode,
			["string_key2"] = refresh.ToString(),
			["string_key1"] = m_serverData.remotePlayerId,
			["string_key6"] = m_serverData.gameVersion.ToString(),
			["string_key14"] = value,
			["number_key13"] = m_serverData.networkVersion.ToString(),
			["string_key7"] = m_serverData.isDedicatedServer.ToString(),
			["string_key8"] = ((object)(PlatformUserID)(ref m_serverData.platformUserID)).ToString(),
			["string_key10"] = m_serverData.serverIp,
			["number_key11"] = GetSearchPage().ToString()
		};
		object value2;
		if ((int)PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)1) != 0)
		{
			Platform platform = PlatformManager.DistributionPlatform.Platform;
			value2 = ((object)(Platform)(ref platform)).ToString();
		}
		else
		{
			value2 = "None";
		}
		obj["string_key12"] = (string)value2;
		Dictionary<string, string> searchData = obj;
		Debug.Log((object)("This is the serverIP used to register the server: " + m_serverData.serverIp));
		CreateLobbyRequest val = new CreateLobbyRequest
		{
			AccessPolicy = (AccessPolicy)0,
			MaxPlayers = 10u,
			Members = members,
			Owner = entityKeyForLocalUser,
			LobbyData = lobbyData,
			SearchData = searchData
		};
		if (m_serverData.isCommunityServer)
		{
			AddNameSearchFilter(searchData, m_serverData.serverName);
		}
		PlayFabMultiplayerAPI.CreateLobby(val, resultCallback, errorCallback, (object)null, (Dictionary<string, string>)null);
	}

	private static int GetSearchPage()
	{
		return Random.Range(0, 4);
	}

	internal static EntityKey GetEntityKeyForLocalUser()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		EntityKey entity = PlayFabManager.instance.Entity;
		return new EntityKey
		{
			Id = entity.Id,
			Type = entity.Type
		};
	}

	private void OnCreateLobbySuccess(CreateLobbyResult result)
	{
		ZLog.Log((object)$"Created PlayFab lobby with ID \"{result.LobbyId}\", ConnectionString \"{result.ConnectionString}\" and owned by \"{m_serverData.remotePlayerId}\"");
		m_serverData.lobbyId = result.LobbyId;
		OnSessionUpdated(State.Creating);
	}

	private void GenerateJoinCode()
	{
		JoinCode = Random.Range(0, (int)Math.Pow(10.0, 6.0)).ToString("D" + 6u);
		m_serverData.joinCode = JoinCode;
	}

	private void RegenerateLobbyJoinCode()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		GenerateJoinCode();
		PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest
		{
			LobbyId = m_serverData.lobbyId,
			SearchData = new Dictionary<string, string> { ["string_key4"] = JoinCode }
		}, (Action<LobbyEmptyResult>)OnSetLobbyJoinCodeSuccess, (Action<PlayFabError>)delegate(PlayFabError error)
		{
			OnFailed("set lobby join-code", error);
		}, (object)null, (Dictionary<string, string>)null);
	}

	private void OnSetLobbyJoinCodeSuccess(LobbyEmptyResult _)
	{
		CheckJoinCodeIsUnique();
	}

	private void CheckJoinCodeIsUnique()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		PlayFabMultiplayerAPI.FindLobbies(new FindLobbiesRequest
		{
			Filter = string.Format("{0} eq '{1}'", "string_key4", JoinCode)
		}, (Action<FindLobbiesResult>)OnCheckJoinCodeSuccess, (Action<PlayFabError>)delegate(PlayFabError error)
		{
			OnFailed("find lobbies", error);
		}, (object)null, (Dictionary<string, string>)null);
	}

	private void ScheduleJoinCodeCheck()
	{
		m_retryIn = 1f;
	}

	private void OnCheckJoinCodeSuccess(FindLobbiesResult result)
	{
		if (result.Lobbies.Count == 0)
		{
			if (m_retries > 0)
			{
				m_retries--;
				ZLog.Log((object)("Retry join-code check " + m_retries));
				ScheduleJoinCodeCheck();
			}
			else
			{
				ZLog.LogWarning((object)"Zero lobbies returned, should be at least one");
				UnregisterServer();
			}
		}
		else if (result.Lobbies.Count == 1 && result.Lobbies[0].Owner.Id == GetEntityKeyForLocalUser().Id)
		{
			ActivateSession();
		}
		else
		{
			OnSessionUpdated(State.RegenerateJoinCode);
		}
	}

	private void ActivateSession()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest
		{
			LobbyId = m_serverData.lobbyId,
			SearchData = new Dictionary<string, string> { ["string_key2"] = true.ToString() }
		}, (Action<LobbyEmptyResult>)OnActivateLobbySuccess, (Action<PlayFabError>)delegate(PlayFabError error)
		{
			OnFailed("activate lobby", error);
		}, (object)null, (Dictionary<string, string>)null);
		SetPlatformMatchmakingData();
	}

	private void OnActivateLobbySuccess(LobbyEmptyResult _)
	{
		OnSessionUpdated(State.Active);
	}

	public void RegisterServer(string name, bool havePassword, bool isCommunityServer, GameVersion gameVersion, string[] modifiers, uint networkVersion, string worldName, bool needServerAccount = true)
	{
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Expected O, but got Unknown
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Expected O, but got Unknown
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Expected O, but got Unknown
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Expected O, but got Unknown
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Expected O, but got Unknown
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Expected O, but got Unknown
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Expected O, but got Unknown
		bool flag = false;
		if (!PlayFabMultiplayerAPI.IsEntityLoggedIn())
		{
			ZLog.LogWarning((object)"Calling ZPlayFabMatchmaking.RegisterServer() without logged in user");
			m_pendingRegisterServer = delegate
			{
				RegisterServer(name, havePassword, isCommunityServer, gameVersion, modifiers, networkVersion, worldName, needServerAccount);
			};
			return;
		}
		m_serverData = new PlayFabMatchmakingServerData
		{
			havePassword = havePassword,
			isCommunityServer = isCommunityServer,
			isDedicatedServer = flag,
			remotePlayerId = PlayFabManager.instance.Entity.Id,
			serverName = name,
			gameVersion = gameVersion,
			modifiers = modifiers,
			networkVersion = networkVersion,
			worldName = worldName
		};
		m_serverData.serverIp = GetServerIPAndPort();
		UpdateNumPlayers("New session");
		ZLog.Log((object)string.Format("Register PlayFab server \"{0}\"{1}", name, flag ? (" with IP " + m_serverData.serverIp) : ""));
		m_serverData.platformUserID = ((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID;
		GenerateJoinCode();
		CreateAndJoinNetwork();
		PlayFabMultiplayerManager obj = PlayFabMultiplayerManager.Get();
		obj.OnNetworkJoined -= new OnNetworkJoinedHandler(OnNetworkJoined);
		obj.OnNetworkJoined += new OnNetworkJoinedHandler(OnNetworkJoined);
		obj.OnNetworkChanged -= new OnNetworkChangedHandler(OnNetworkChanged);
		obj.OnNetworkChanged += new OnNetworkChangedHandler(OnNetworkChanged);
		obj.OnError -= new OnErrorEventHandler(OnNetworkError);
		obj.OnError += new OnErrorEventHandler(OnNetworkError);
		obj.OnRemotePlayerJoined -= new OnRemotePlayerJoinedHandler(OnRemotePlayerJoined);
		obj.OnRemotePlayerJoined += new OnRemotePlayerJoinedHandler(OnRemotePlayerJoined);
		obj.OnRemotePlayerLeft -= new OnRemotePlayerLeftHandler(OnRemotePlayerLeft);
		obj.OnRemotePlayerLeft += new OnRemotePlayerLeftHandler(OnRemotePlayerLeft);
	}

	private string GetServerIPAndPort()
	{
		if (!m_serverData.isDedicatedServer || string.IsNullOrEmpty(PublicIP))
		{
			return "";
		}
		if (PublicIP.Contains(":"))
		{
			Debug.Log((object)$"Likely an IPV6 address, returning [{PublicIP}]:{m_serverPort}");
			return $"[{PublicIP}]:{m_serverPort}";
		}
		Debug.Log((object)$"IPv4, returning {PublicIP}:{m_serverPort}");
		return $"{PublicIP}:{m_serverPort}";
	}

	private bool IsIPv6(string address)
	{
		return true;
	}

	public static void LookupPublicIP()
	{
		if (string.IsNullOrEmpty(PublicIP) && m_publicIpLookupThread == null)
		{
			m_publicIpLookupThread = new Thread(BackgroundLookupPublicIP);
			m_publicIpLookupThread.Name = "PlayfabLooupThread";
			m_publicIpLookupThread.Start();
		}
	}

	private static void BackgroundLookupPublicIP(object obj)
	{
		while (string.IsNullOrEmpty(PublicIP))
		{
			PublicIP = ZNet.GetPublicIP(m_getPublicIpAttempts++);
			Thread.Sleep(10);
		}
	}

	private void CreateAndJoinNetwork()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		PlayFabNetworkConfiguration val = new PlayFabNetworkConfiguration
		{
			MaxPlayerCount = 10u,
			DirectPeerConnectivityOptions = (PARTY_DIRECT_PEER_CONNECTIVITY_OPTIONS)15
		};
		ZLog.Log((object)$"Server '{m_serverData.serverName}' begin PlayFab create and join network for server ");
		PlayFabMultiplayerManager.Get().CreateAndJoinNetwork(val);
		m_isConnectingToNetwork = true;
		StartReconnectNetworkTimer();
	}

	public void UnregisterServer()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		Debug.Log((object)("ZPlayFabMatchmaking::UnregisterServer - unregistering server now. State: " + m_state));
		if (m_state != 0)
		{
			ZLog.Log((object)$"Unregister PlayFab server \"{m_serverData.serverName}\" and leaving network \"{m_serverData.networkId}\"");
			DeleteLobby(m_serverData.lobbyId);
			ZPlayFabSocket.DestroyListenSocket();
			PlayFabMultiplayerManager.Get().LeaveNetwork();
			PlayFabMultiplayerManager.Get().OnNetworkJoined -= new OnNetworkJoinedHandler(OnNetworkJoined);
			PlayFabMultiplayerManager.Get().OnNetworkChanged -= new OnNetworkChangedHandler(OnNetworkChanged);
			PlayFabMultiplayerManager.Get().OnError -= new OnErrorEventHandler(OnNetworkError);
			PlayFabMultiplayerManager.Get().OnRemotePlayerJoined -= new OnRemotePlayerJoinedHandler(OnRemotePlayerJoined);
			PlayFabMultiplayerManager.Get().OnRemotePlayerLeft -= new OnRemotePlayerLeftHandler(OnRemotePlayerLeft);
			m_serverData = null;
			m_retries = 0;
			m_state = State.Uninitialized;
			StopReconnectNetworkTimer();
		}
		else
		{
			ZPlayFabMatchmaking.LobbyLeft?.Invoke(success: true);
		}
	}

	internal static void ResetParty()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (instance != null && instance.IsJoinedToNetwork())
		{
			instance.OnNetworkError(null, new PlayFabMultiplayerManagerErrorArgs(9999, "Forced ResetParty", (PlayFabMultiplayerManagerErrorType)1));
		}
		else
		{
			ZLog.Log((object)"No active PlayFab Party to reset");
		}
	}

	private void OnNetworkError(object sender, PlayFabMultiplayerManagerErrorArgs args)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!IsReconnectNetworkTimerActive())
		{
			ZLog.LogWarning((object)$"PlayFab network error in session '{m_serverData.serverName}' and network {m_serverData.networkId} with type '{args.Type}' and code '{args.Code}': {args.Message}");
			StartReconnectNetworkTimer(args.Code);
		}
	}

	private void OnNetworkChanged(object sender, string newNetworkId)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		ZLog.LogWarning((object)$"PlayFab network session '{m_serverData.serverName}' and network {m_serverData.networkId} changed to network {newNetworkId}");
		m_serverData.networkId = newNetworkId;
		Dictionary<string, string> lobbyData = new Dictionary<string, string> { [PlayFabAttrKey.NetworkId.ToKeyString()] = m_serverData.networkId };
		PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest
		{
			LobbyId = m_serverData.lobbyId,
			LobbyData = lobbyData
		}, (Action<LobbyEmptyResult>)delegate
		{
			ZLog.Log((object)$"Lobby {m_serverData.lobbyId} for world '{m_serverData.serverName}' change to network {m_serverData.networkId}");
		}, (Action<PlayFabError>)OnRefreshFailed, (object)null, (Dictionary<string, string>)null);
	}

	private void DeleteLobby(string lobbyId)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest
		{
			LobbyId = lobbyId,
			SearchData = new Dictionary<string, string> { ["string_key2"] = false.ToString() }
		}, (Action<LobbyEmptyResult>)delegate
		{
			ZLog.Log((object)("Deactivated PlayFab lobby " + lobbyId));
		}, (Action<PlayFabError>)delegate(PlayFabError error)
		{
			ZLog.LogWarning((object)$"Failed to deactive lobby '{lobbyId}': {error.GenerateErrorReport()}");
		}, (object)null, (Dictionary<string, string>)null);
		LeaveLobby(lobbyId);
		ClearPlatformMatchmakingData();
	}

	public static void LeaveLobby(string lobbyId)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		PlayFabMultiplayerAPI.LeaveLobby(new LeaveLobbyRequest
		{
			LobbyId = lobbyId,
			MemberEntity = GetEntityKeyForLocalUser()
		}, (Action<LobbyEmptyResult>)delegate
		{
			ZLog.Log((object)("Left PlayFab lobby " + lobbyId));
			ZPlayFabMatchmaking.LobbyLeft?.Invoke(success: true);
		}, (Action<PlayFabError>)delegate(PlayFabError error)
		{
			ZLog.LogError((object)$"Failed to leave lobby '{lobbyId}': {error.GenerateErrorReport()}");
			ZPlayFabMatchmaking.LobbyLeft?.Invoke(success: false);
		}, (object)null, (Dictionary<string, string>)null);
	}

	public static void LeaveEmptyLobby()
	{
		ZPlayFabMatchmaking.LobbyLeft?.Invoke(success: true);
	}

	public static void ResolveJoinCode(string joinCode, ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction)
	{
		string searchFilter = string.Format("{0} eq '{1}' and {2} eq '{3}'", "string_key4", joinCode, "string_key2", true.ToString());
		instance.m_activeSearches.Add(new ZPlayFabLobbySearch(successAction, failedAction, searchFilter, ZPlayFabLobbySearchFlags.None));
	}

	public static void CheckHostOnlineStatus(string hostName, ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction, bool joinLobby = false)
	{
		FindHostSession(string.Format("{0} eq '{1}' and {2} eq '{3}'", "string_key1", hostName, "string_key2", true.ToString()), successAction, failedAction, joinLobby);
	}

	public static void FindHostByIp(IPEndPoint hostIp, ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction, bool joinLobby = false)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((IPEndPoint)(ref hostIp)).IsPublic)
		{
			failedAction?.Invoke(ZPLayFabMatchmakingFailReason.EndPointNotOnInternet);
			return;
		}
		FindHostSession(string.Format("{0} eq '{1}' and {2} eq '{3}'", "string_key10", hostIp, "string_key2", true.ToString()), successAction, failedAction, joinLobby);
	}

	public static void FindServerByHostUser(PlatformUserID user, ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction, bool joinLobby = false)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((PlatformUserID)(ref user)).IsValid)
		{
			failedAction?.Invoke(ZPLayFabMatchmakingFailReason.InvalidParameter);
			return;
		}
		FindHostSession(string.Format("{0} eq '{1}' and {2} eq '{3}'", "string_key8", user, "string_key2", true.ToString()), successAction, failedAction, joinLobby);
	}

	private static Dictionary<char, int> CreateCharHistogram(string str)
	{
		Dictionary<char, int> dictionary = new Dictionary<char, int>();
		string text = str.ToLowerInvariant();
		foreach (char key in text)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key]++;
			}
			else
			{
				dictionary.Add(key, 1);
			}
		}
		return dictionary;
	}

	private static void AddNameSearchFilter(Dictionary<string, string> searchData, string serverName)
	{
		Dictionary<char, int> dictionary = CreateCharHistogram(serverName);
		for (char c = 'a'; c <= 'z'; c = (char)(c + 1))
		{
			if (CharToKeyName(c, out var key))
			{
				dictionary.TryGetValue(c, out var value);
				searchData.Add(key, value.ToString());
			}
		}
	}

	private static string CreateNameSearchFilter(string name)
	{
		Dictionary<char, int> dictionary = CreateCharHistogram(name);
		string text = "";
		string text2 = name.ToLowerInvariant();
		foreach (char c in text2)
		{
			if (CharToKeyName(c, out var key) && dictionary.TryGetValue(c, out var value))
			{
				text += $" and {key} ge {value}";
			}
		}
		return text;
	}

	private static bool CharToKeyName(char ch, out string key)
	{
		int num = "eariotnslcudpmhgbfywkvxzjq".IndexOf(ch);
		if (num < 0 || num >= 16)
		{
			key = null;
			return false;
		}
		key = $"number_key{num + 14 + 1}";
		return true;
	}

	private void CancelPendingSearches()
	{
		foreach (ZPlayFabLobbySearch activeSearch in instance.m_activeSearches)
		{
			activeSearch.Cancel();
		}
		m_pendingSearches.Clear();
	}

	private static void FindHostSession(string searchFilter, ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction, bool joinLobby)
	{
		if (joinLobby)
		{
			instance.CancelPendingSearches();
			instance.m_activeSearches.Add(new ZPlayFabLobbySearch(successAction, failedAction, searchFilter, ZPlayFabLobbySearchFlags.Join | ZPlayFabLobbySearchFlags.AllowRetry));
		}
		else
		{
			instance.m_pendingSearches.Enqueue(new ZPlayFabLobbySearch(successAction, failedAction, searchFilter, ZPlayFabLobbySearchFlags.Queued));
		}
	}

	public static ZPlayFabLobbySearch ListServers(string nameFilter, ZPlayFabMatchmakingNewServersCallback serversFoundAction, ZPlayFabMatchmakingServerSearchDoneCallback listDone)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		string text = string.Format("{0} eq '{1}' and {2} eq '{3}'", "string_key3", true.ToString(), "string_key2", true.ToString());
		if (nameFilter == null)
		{
			nameFilter = string.Empty;
		}
		text = ((nameFilter.Length != 0) ? (text + CreateNameSearchFilter(nameFilter)) : (text + string.Format(" and {0} eq {1}", "number_key13", 36u)));
		bool flag = PrivilegeResultExtentions.IsGranted(PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)1));
		List<string> list = new List<string>(2);
		list.Add(CreateSearchFilter(text, flag));
		if (PlatformManager.DistributionPlatform.Platform != "Steam" && flag)
		{
			list.Add(CreateSearchFilter(text, isCrossplay: false));
		}
		ZPlayFabLobbySearch zPlayFabLobbySearch = new ZPlayFabLobbySearch(serversFoundAction, listDone, list.ToArray());
		instance.m_pendingSearches.Enqueue(zPlayFabLobbySearch);
		return zPlayFabLobbySearch;
	}

	private static string CreateSearchFilter(string baseFilter, bool isCrossplay)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		object obj;
		if (!isCrossplay)
		{
			Platform platform = PlatformManager.DistributionPlatform.Platform;
			obj = ((object)(Platform)(ref platform)).ToString();
		}
		else
		{
			obj = "None";
		}
		string text = (string)obj;
		return baseFilter + " and string_key12 eq '" + text + "'";
	}

	public static bool IsJoinCode(string joinString)
	{
		int result;
		if ((long)joinString.Length == 6)
		{
			return int.TryParse(joinString, out result);
		}
		return false;
	}

	public static void SetDataPort(int serverPort)
	{
		if (instance != null)
		{
			instance.m_serverPort = serverPort;
		}
	}

	public static void OnLogin()
	{
		if (instance != null && instance.m_pendingRegisterServer != null)
		{
			instance.m_pendingRegisterServer();
			instance.m_pendingRegisterServer = null;
		}
	}

	internal static void ForwardProgress()
	{
		if (instance != null)
		{
			instance.StopReconnectNetworkTimer();
		}
	}
}
