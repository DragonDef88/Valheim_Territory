using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NetworkingUtils;
using Splatform;
using Steamworks;
using UnityEngine;

public class ZSteamMatchmaking
{
	public delegate void AuthSessionTicketResponseHandler();

	public delegate void ServerRegistered(bool success);

	public struct DedicatedPing
	{
		public IPEndPoint m_endPoint;

		public ServerPingCompletedHandler m_completedHandler;

		public DedicatedPing(IPEndPoint endPoint, ServerPingCompletedHandler completedHandler)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_endPoint = endPoint;
			m_completedHandler = completedHandler ?? throw new ArgumentNullException("completedHandler");
		}
	}

	public delegate void ServerPingCompletedHandler(ServerData serverData);

	private static ZSteamMatchmaking m_instance;

	private const int maxServers = 200;

	private List<ServerData> m_matchmakingServers = new List<ServerData>();

	private List<ServerData> m_dedicatedServers = new List<ServerData>();

	private List<ServerData> m_friendServers = new List<ServerData>();

	private int m_serverListRevision;

	private int m_updateTriggerAccumulator;

	private CallResult<LobbyCreated_t> m_lobbyCreated;

	private CallResult<LobbyMatchList_t> m_lobbyMatchList;

	private CallResult<LobbyEnter_t> m_lobbyEntered;

	private Callback<GameServerChangeRequested_t> m_changeServer;

	private Callback<GameLobbyJoinRequested_t> m_joinRequest;

	private Callback<LobbyDataUpdate_t> m_lobbyDataUpdate;

	private Callback<GetAuthSessionTicketResponse_t> m_authSessionTicketResponse;

	private Callback<SteamServerConnectFailure_t> m_steamServerConnectFailure;

	private Callback<SteamServersConnected_t> m_steamServersConnected;

	private Callback<SteamServersDisconnected_t> m_steamServersDisconnected;

	private ServerRegistered serverRegisteredCallback;

	private CSteamID m_myLobby = CSteamID.Nil;

	private CSteamID m_queuedJoinLobby = CSteamID.Nil;

	private ServerJoinData m_joinData = ServerJoinData.None;

	private List<KeyValuePair<CSteamID, string>> m_requestedFriendGames = new List<KeyValuePair<CSteamID, string>>();

	private Dictionary<CSteamID, (CSteamID, ServerPingCompletedHandler)> m_requestedLobbiesUserHosted = new Dictionary<CSteamID, (CSteamID, ServerPingCompletedHandler)>();

	private Queue<DedicatedPing> m_queuedPings = new Queue<DedicatedPing>();

	private DedicatedPing? m_currentPing;

	private ISteamMatchmakingServerListResponse m_steamServerCallbackHandler;

	private ISteamMatchmakingPingResponse m_pingResponseCallbackHandler;

	private HServerQuery m_pingQuery;

	private HServerListRequest m_serverListRequest;

	private bool m_haveListRequest;

	private bool m_refreshingDedicatedServers;

	private bool m_refreshingPublicGames;

	private string m_registerServerName = "";

	private bool m_registerPassword;

	private GameVersion m_registerGameVerson;

	private uint m_registerNetworkVerson;

	private string[] m_registerModifiers = new string[0];

	private string m_nameFilter = "";

	private bool m_friendsFilter = true;

	private HAuthTicket m_authTicket = HAuthTicket.Invalid;

	public static ZSteamMatchmaking instance => m_instance;

	public bool IsRefreshing { get; private set; }

	public event AuthSessionTicketResponseHandler AuthSessionTicketResponse;

	public static void Initialize()
	{
		if (m_instance == null)
		{
			m_instance = new ZSteamMatchmaking();
		}
	}

	private ZSteamMatchmaking()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00c7: Expected O, but got Unknown
		//IL_00c7: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		m_steamServerCallbackHandler = new ISteamMatchmakingServerListResponse(new ServerResponded(OnServerResponded), new ServerFailedToRespond(OnServerFailedToRespond), new RefreshComplete(OnRefreshComplete));
		m_pingResponseCallbackHandler = new ISteamMatchmakingPingResponse(new ServerResponded(OnPingRespond), new ServerFailedToRespond(OnPingFailed));
		m_lobbyCreated = CallResult<LobbyCreated_t>.Create((APIDispatchDelegate<LobbyCreated_t>)OnLobbyCreated);
		m_lobbyMatchList = CallResult<LobbyMatchList_t>.Create((APIDispatchDelegate<LobbyMatchList_t>)OnLobbyMatchList);
		m_changeServer = Callback<GameServerChangeRequested_t>.Create((DispatchDelegate<GameServerChangeRequested_t>)OnChangeServerRequest);
		m_joinRequest = Callback<GameLobbyJoinRequested_t>.Create((DispatchDelegate<GameLobbyJoinRequested_t>)OnJoinRequest);
		m_lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create((DispatchDelegate<LobbyDataUpdate_t>)OnLobbyDataUpdate);
		m_authSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create((DispatchDelegate<GetAuthSessionTicketResponse_t>)OnAuthSessionTicketResponse);
	}

	public byte[] RequestSessionTicket(ref SteamNetworkingIdentity serverIdentity)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		ReleaseSessionTicket();
		byte[] array = new byte[1024];
		uint num = 0u;
		SteamNetworkingIdentity val = default(SteamNetworkingIdentity);
		m_authTicket = SteamUser.GetAuthSessionTicket(array, 1024, ref num, ref val);
		if (m_authTicket == HAuthTicket.Invalid)
		{
			return null;
		}
		byte[] array2 = new byte[num];
		Buffer.BlockCopy(array, 0, array2, 0, (int)num);
		return array2;
	}

	public void ReleaseSessionTicket()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_authTicket == HAuthTicket.Invalid))
		{
			SteamUser.CancelAuthTicket(m_authTicket);
			m_authTicket = HAuthTicket.Invalid;
			ZLog.Log((object)"Released session ticket");
		}
	}

	public bool VerifySessionTicket(byte[] ticket, CSteamID steamID)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		return (int)SteamUser.BeginAuthSession(ticket, ticket.Length, steamID) == 0;
	}

	private void OnAuthSessionTicketResponse(GetAuthSessionTicketResponse_t data)
	{
		ZLog.Log((object)"Session auth respons callback");
		this.AuthSessionTicketResponse?.Invoke();
	}

	private void OnSteamServersConnected(SteamServersConnected_t data)
	{
		ZLog.Log((object)"Game server connected");
	}

	private void OnSteamServersDisconnected(SteamServersDisconnected_t data)
	{
		ZLog.LogWarning((object)"Game server disconnected");
	}

	private void OnSteamServersConnectFail(SteamServerConnectFailure_t data)
	{
		ZLog.LogWarning((object)"Game server connected failed");
	}

	private void OnChangeServerRequest(GameServerChangeRequested_t data)
	{
		ZLog.Log((object)("ZSteamMatchmaking got change server request to:" + ((GameServerChangeRequested_t)(ref data)).m_rgchServer));
		QueueServerJoin(((GameServerChangeRequested_t)(ref data)).m_rgchServer);
	}

	private void OnJoinRequest(GameLobbyJoinRequested_t data)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		CSteamID val = data.m_steamIDFriend;
		string? text = ((object)(CSteamID)(ref val)).ToString();
		val = data.m_steamIDLobby;
		ZLog.Log((object)("ZSteamMatchmaking got join request friend:" + text + "  lobby:" + ((object)(CSteamID)(ref val)).ToString()));
		QueueLobbyJoin(data.m_steamIDLobby);
	}

	private IPAddress FindIP(string host)
	{
		try
		{
			if (IPAddress.TryParse(host, out IPAddress address))
			{
				return address;
			}
			ZLog.Log((object)("Not an ip address " + host + " doing dns lookup"));
			IPHostEntry hostEntry = Dns.GetHostEntry(host);
			if (hostEntry.AddressList.Length == 0)
			{
				ZLog.Log((object)"Dns lookup failed");
				return null;
			}
			ZLog.Log((object)("Got dns entries: " + hostEntry.AddressList.Length));
			IPAddress[] addressList = hostEntry.AddressList;
			foreach (IPAddress iPAddress in addressList)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					return iPAddress;
				}
			}
			return null;
		}
		catch (Exception ex)
		{
			ZLog.Log((object)("Exception while finding ip:" + ex.ToString()));
			return null;
		}
	}

	public bool ResolveIPFromAddrString(string addr, ref SteamNetworkingIPAddr destination)
	{
		try
		{
			string[] array = addr.Split(':');
			if (array.Length < 2)
			{
				return false;
			}
			IPAddress iPAddress = FindIP(array[0]);
			if (iPAddress == null)
			{
				ZLog.Log((object)("Invalid address " + array[0]));
				return false;
			}
			uint num = (uint)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(iPAddress.GetAddressBytes(), 0));
			int num2 = int.Parse(array[1]);
			ZLog.Log((object)("connect to ip:" + iPAddress.ToString() + " port:" + num2));
			((SteamNetworkingIPAddr)(ref destination)).SetIPv4(num, (ushort)num2);
			return true;
		}
		catch (Exception ex)
		{
			ZLog.Log((object)("Exception when resolving IP address: " + ex));
			return false;
		}
	}

	public void QueueServerJoin(string addr)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		SteamNetworkingIPAddr destination = default(SteamNetworkingIPAddr);
		if (ResolveIPFromAddrString(addr, ref destination))
		{
			m_joinData = new ServerJoinData(new ServerJoinDataDedicated(((SteamNetworkingIPAddr)(ref destination)).GetIPv4(), destination.m_port));
		}
		else
		{
			ZLog.Log((object)"Couldn't resolve IP address.");
		}
	}

	private void EnqueuePing(IPEndPoint server, ServerPingCompletedHandler completedCallback)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_queuedPings.Enqueue(new DedicatedPing(server, completedCallback));
		if (!m_currentPing.HasValue)
		{
			PingNextInQueue();
		}
	}

	private void PingNextInQueue()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (m_queuedPings.Count <= 0)
		{
			ZLog.LogError((object)"Queued pings was 0!");
			return;
		}
		if (m_currentPing.HasValue)
		{
			ZLog.LogError((object)"Ping was already active!");
			return;
		}
		m_currentPing = m_queuedPings.Dequeue();
		IPv6Address address = m_currentPing.Value.m_endPoint.m_address;
		if ((int)((IPv6Address)(ref address)).AddressRange != 2)
		{
			ZLog.LogError((object)$"Address {address} was not an IPv4 address!");
		}
		else
		{
			m_pingQuery = SteamMatchmakingServers.PingServer(((IPv6Address)(ref address)).IPv4.m_value, (ushort)((m_currentPing.Value.m_endPoint.m_port + 1) % 65535), m_pingResponseCallbackHandler);
		}
	}

	private void OnPingRespond(gameserveritem_t serverData)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		string serverName = serverData.GetServerName();
		CSteamID steamID = serverData.m_steamID;
		ZLog.Log((object)("Got join server data " + serverName + "  " + ((object)(CSteamID)(ref steamID)).ToString()));
		IPEndPoint val = default(IPEndPoint);
		((IPEndPoint)(ref val))._002Ector(IPv6Address.op_Implicit(new IPv4Address(((servernetadr_t)(ref serverData.m_NetAdr)).GetIP())), ((servernetadr_t)(ref serverData.m_NetAdr)).GetConnectionPort());
		DecodeTags(serverData.GetGameTags(), out var gameVersion, out var networkVersion, out var modifiers);
		ServerMatchmakingData matchmakingData = new ServerMatchmakingData(DateTime.UtcNow, serverData.GetServerName(), (uint)serverData.m_nPlayers, (uint)serverData.m_nMaxPlayers, PlatformUserID.None, gameVersion, networkVersion, null, serverData.m_bPassword, new Platform("Steam"), modifiers);
		if (!m_currentPing.HasValue)
		{
			ZLog.LogError((object)("Server " + matchmakingData.m_serverName + " got callback but wasn't requested!"));
			return;
		}
		if (m_currentPing.Value.m_endPoint != val)
		{
			ZLog.LogError((object)$"Retrieved address {val} is not equal to the stored address {m_currentPing.Value.m_endPoint}!");
			return;
		}
		ServerJoinData joinData = new ServerJoinData(new ServerJoinDataDedicated(val));
		FinishPing(new ServerData(joinData, matchmakingData));
	}

	private void OnPingFailed()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)"Failed to get join server data");
		if (!m_currentPing.HasValue)
		{
			ZLog.LogError((object)$"Server {m_currentPing.Value.m_endPoint} got callback but wasn't requested!");
		}
		else
		{
			FinishPing(new ServerData(new ServerJoinData(new ServerJoinDataDedicated(m_currentPing.Value.m_endPoint)), new ServerMatchmakingData(DateTime.UtcNow)));
		}
	}

	private void FinishPing(ServerData serverData)
	{
		InvokeCurrentPingCallback(serverData);
		if (m_queuedPings.Count > 0)
		{
			PingNextInQueue();
		}
	}

	private void InvokeCurrentPingCallback(ServerData serverData)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		ServerPingCompletedHandler completedHandler = m_currentPing.Value.m_completedHandler;
		m_currentPing = null;
		m_pingQuery = default(HServerQuery);
		completedHandler?.Invoke(serverData);
	}

	private bool TryGetLobbyData(CSteamID lobbyID)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		uint num = default(uint);
		ushort num2 = default(ushort);
		CSteamID val = default(CSteamID);
		if (!SteamMatchmaking.GetLobbyGameServer(lobbyID, ref num, ref num2, ref val))
		{
			return false;
		}
		CSteamID val2 = val;
		ZLog.Log((object)("  hostid: " + ((object)(CSteamID)(ref val2)).ToString()));
		m_queuedJoinLobby = CSteamID.Nil;
		m_joinData = GetLobbyServerData(lobbyID).m_joinData;
		return true;
	}

	public void QueueLobbyJoin(CSteamID lobbyID)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetLobbyData(lobbyID))
		{
			CSteamID val = lobbyID;
			ZLog.Log((object)("Failed to get lobby data for lobby " + ((object)(CSteamID)(ref val)).ToString() + ", requesting lobby data"));
			m_queuedJoinLobby = lobbyID;
			SteamMatchmaking.RequestLobbyData(lobbyID);
		}
		if (!((Object)(object)FejdStartup.instance == (Object)null))
		{
			return;
		}
		if (UnifiedPopup.IsAvailable() && (Object)(object)Menu.instance != (Object)null)
		{
			UnifiedPopup.Push(new YesNoPopup("$menu_joindifferentserver", "$menu_logoutprompt", delegate
			{
				UnifiedPopup.Pop();
				if ((Object)(object)Menu.instance != (Object)null)
				{
					Menu.instance.OnLogoutYes();
				}
			}, delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				UnifiedPopup.Pop();
				m_queuedJoinLobby = CSteamID.Nil;
				m_joinData = ServerJoinData.None;
			}));
		}
		else
		{
			Debug.LogWarning((object)"Couldn't handle invite appropriately! Ignoring.");
			m_queuedJoinLobby = CSteamID.Nil;
			m_joinData = ServerJoinData.None;
		}
	}

	private void OnLobbyDataUpdate(LobbyDataUpdate_t data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		CSteamID val = default(CSteamID);
		((CSteamID)(ref val))._002Ector(data.m_ulSteamIDLobby);
		if (val == m_queuedJoinLobby)
		{
			if (TryGetLobbyData(val))
			{
				ZLog.Log((object)"Got lobby data, for queued lobby");
			}
			return;
		}
		ZLog.Log((object)"Got requested lobby data");
		ServerData lobbyServerData = GetLobbyServerData(val);
		foreach (KeyValuePair<CSteamID, string> requestedFriendGame in m_requestedFriendGames)
		{
			if (requestedFriendGame.Key == val && lobbyServerData.m_joinData.IsValid)
			{
				m_friendServers.Add(lobbyServerData);
				m_serverListRevision++;
			}
		}
		if (m_requestedLobbiesUserHosted.TryGetValue(val, out var value))
		{
			m_requestedLobbiesUserHosted.Remove(val);
			value.Item2?.Invoke(lobbyServerData);
		}
	}

	public void RegisterServer(string name, bool password, GameVersion gameVersion, string[] modifiers, uint networkVersion, bool publicServer, string worldName, ServerRegistered serverRegisteredCallback)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		UnregisterServer();
		this.serverRegisteredCallback = serverRegisteredCallback;
		SteamAPICall_t val = SteamMatchmaking.CreateLobby((ELobbyType)((!publicServer) ? 1 : 2), 10);
		m_lobbyCreated.Set(val, (APIDispatchDelegate<LobbyCreated_t>)null);
		m_registerServerName = name;
		m_registerPassword = password;
		m_registerGameVerson = gameVersion;
		m_registerNetworkVerson = networkVersion;
		m_registerModifiers = modifiers;
		ZLog.Log((object)"Registering lobby");
	}

	private void OnLobbyCreated(LobbyCreated_t data, bool ioError)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)("Lobby was created " + ((object)(EResult)(ref data.m_eResult)).ToString() + "  " + data.m_ulSteamIDLobby + "  error:" + ioError));
		if (ioError)
		{
			serverRegisteredCallback?.Invoke(success: false);
			return;
		}
		if ((int)data.m_eResult == 3)
		{
			ZLog.LogWarning((object)"Failed to connect to Steam to register the server!");
			serverRegisteredCallback?.Invoke(success: false);
			return;
		}
		m_myLobby = new CSteamID(data.m_ulSteamIDLobby);
		if (!SteamMatchmaking.SetLobbyData(m_myLobby, "name", m_registerServerName))
		{
			Debug.LogError((object)"Couldn't set name in lobby");
		}
		if (!SteamMatchmaking.SetLobbyData(m_myLobby, "password", m_registerPassword ? "1" : "0"))
		{
			Debug.LogError((object)"Couldn't set password in lobby");
		}
		if (!SteamMatchmaking.SetLobbyData(m_myLobby, "version", m_registerGameVerson.ToString()))
		{
			Debug.LogError((object)"Couldn't set game version in lobby");
		}
		if (!SteamMatchmaking.SetLobbyData(m_myLobby, "networkversion", m_registerNetworkVerson.ToString()))
		{
			Debug.LogError((object)"Couldn't set network version in lobby");
		}
		if (!SteamMatchmaking.SetLobbyData(m_myLobby, "modifiers", StringUtils.EncodeStringListAsString((IReadOnlyList<string>)m_registerModifiers, true)))
		{
			Debug.LogError((object)"Couldn't set modifiers in lobby");
		}
		string text;
		string text2;
		string text3;
		switch (ZNet.m_onlineBackend)
		{
		case OnlineBackendType.CustomSocket:
			text = "Dedicated";
			text2 = ZNet.GetServerString(includeBackend: false);
			text3 = "1";
			break;
		case OnlineBackendType.Steamworks:
			text = "Steam user";
			text2 = "";
			text3 = "0";
			break;
		case OnlineBackendType.PlayFab:
			text = "PlayFab user";
			text2 = PlayFabManager.instance.Entity.Id;
			text3 = "1";
			break;
		default:
			Debug.LogError((object)"Can't create lobby for server with unknown or unsupported backend");
			text = "";
			text2 = "";
			text3 = "";
			break;
		}
		if ((int)PlatformManager.DistributionPlatform.PrivilegeProvider.CheckPrivilege((Privilege)1) != 0)
		{
			text3 = "0";
		}
		if (!SteamMatchmaking.SetLobbyData(m_myLobby, "serverType", text))
		{
			Debug.LogError((object)"Couldn't set backend in lobby");
		}
		if (!SteamMatchmaking.SetLobbyData(m_myLobby, "hostID", text2))
		{
			Debug.LogError((object)"Couldn't set host in lobby");
		}
		if (!SteamMatchmaking.SetLobbyData(m_myLobby, "isCrossplay", text3))
		{
			Debug.LogError((object)"Couldn't set crossplay in lobby");
		}
		SteamMatchmaking.SetLobbyGameServer(m_myLobby, 0u, (ushort)0, SteamUser.GetSteamID());
		serverRegisteredCallback?.Invoke(success: true);
	}

	private void OnLobbyEnter(LobbyEnter_t data, bool ioError)
	{
		ZLog.LogWarning((object)("Entering lobby " + data.m_ulSteamIDLobby));
	}

	public void UnregisterServer()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (m_myLobby != CSteamID.Nil)
		{
			SteamMatchmaking.SetLobbyJoinable(m_myLobby, false);
			SteamMatchmaking.LeaveLobby(m_myLobby);
			m_myLobby = CSteamID.Nil;
		}
	}

	public void RequestServerlist()
	{
		IsRefreshing = true;
		RequestFriendGames();
		RequestPublicLobbies();
		RequestDedicatedServers();
	}

	public void StopServerListing()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (m_haveListRequest)
		{
			SteamMatchmakingServers.ReleaseRequest(m_serverListRequest);
			m_haveListRequest = false;
			IsRefreshing = false;
		}
	}

	private void RequestFriendGames()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		m_friendServers.Clear();
		m_requestedFriendGames.Clear();
		int num = SteamFriends.GetFriendCount((EFriendFlags)4);
		if (num == -1)
		{
			ZLog.Log((object)"GetFriendCount returned -1, the current user is not logged in.");
			num = 0;
		}
		FriendGameInfo_t val = default(FriendGameInfo_t);
		for (int i = 0; i < num; i++)
		{
			CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, (EFriendFlags)4);
			string friendPersonaName = SteamFriends.GetFriendPersonaName(friendByIndex);
			if (SteamFriends.GetFriendGamePlayed(friendByIndex, ref val) && val.m_gameID == (CGameID)(ulong)SteamManager.APP_ID && val.m_steamIDLobby != CSteamID.Nil)
			{
				ZLog.Log((object)"Friend is in our game");
				m_requestedFriendGames.Add(new KeyValuePair<CSteamID, string>(val.m_steamIDLobby, friendPersonaName));
				SteamMatchmaking.RequestLobbyData(val.m_steamIDLobby);
			}
		}
		m_serverListRevision++;
	}

	private void RequestPublicLobbies()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		SteamAPICall_t val = SteamMatchmaking.RequestLobbyList();
		m_lobbyMatchList.Set(val, (APIDispatchDelegate<LobbyMatchList_t>)null);
		m_refreshingPublicGames = true;
	}

	private void RequestDedicatedServers()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (m_haveListRequest)
		{
			SteamMatchmakingServers.ReleaseRequest(m_serverListRequest);
			m_haveListRequest = false;
		}
		m_dedicatedServers.Clear();
		m_serverListRequest = SteamMatchmakingServers.RequestInternetServerList(SteamUtils.GetAppID(), (MatchMakingKeyValuePair_t[])(object)new MatchMakingKeyValuePair_t[0], 0u, m_steamServerCallbackHandler);
		m_haveListRequest = true;
	}

	private void OnLobbyMatchList(LobbyMatchList_t data, bool ioError)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		m_refreshingPublicGames = false;
		m_matchmakingServers.Clear();
		for (int i = 0; i < data.m_nLobbiesMatching; i++)
		{
			CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(i);
			ServerData lobbyServerData = GetLobbyServerData(lobbyByIndex);
			if (lobbyServerData.m_joinData.IsValid)
			{
				m_matchmakingServers.Add(lobbyServerData);
			}
		}
		m_serverListRevision++;
	}

	private ServerData GetLobbyServerData(CSteamID lobbyID)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		string lobbyData = SteamMatchmaking.GetLobbyData(lobbyID, "name");
		bool isPasswordProtected = SteamMatchmaking.GetLobbyData(lobbyID, "password") == "1";
		GameVersion gameVersion = GameVersion.ParseGameVersion(SteamMatchmaking.GetLobbyData(lobbyID, "version"));
		string[] modifiers = default(string[]);
		StringUtils.TryDecodeStringAsArray(SteamMatchmaking.GetLobbyData(lobbyID, "modifiers"), ref modifiers);
		uint result = (uint.TryParse(SteamMatchmaking.GetLobbyData(lobbyID, "networkversion"), out result) ? result : 0u);
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
		int lobbyMemberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);
		uint num = default(uint);
		ushort num2 = default(ushort);
		CSteamID joinUserID = default(CSteamID);
		if (!SteamMatchmaking.GetLobbyGameServer(lobbyID, ref num, ref num2, ref joinUserID))
		{
			ZLog.Log((object)"Failed to get lobby gameserver");
			return ServerData.None;
		}
		string lobbyData2 = SteamMatchmaking.GetLobbyData(lobbyID, "hostID");
		string lobbyData3 = SteamMatchmaking.GetLobbyData(lobbyID, "serverType");
		string lobbyData4 = SteamMatchmaking.GetLobbyData(lobbyID, "isCrossplay");
		if (lobbyData3 == null || lobbyData3.Length != 0)
		{
			switch (lobbyData3)
			{
			case "Steam user":
				break;
			case "PlayFab user":
				goto IL_00ff;
			case "Dedicated":
				goto IL_010f;
			default:
				return ServerData.None;
			}
		}
		ServerJoinData joinData = new ServerJoinData(new ServerJoinDataSteamUser(joinUserID));
		goto IL_0125;
		IL_0125:
		if (!joinData.IsValid)
		{
			return ServerData.None;
		}
		ServerMatchmakingData matchmakingData = new ServerMatchmakingData(DateTime.UtcNow, lobbyData, (uint)numLobbyMembers, (uint)lobbyMemberLimit, new PlatformUserID(new Platform("Steam"), ((object)(CSteamID)(ref joinUserID)).ToString()), gameVersion, result, null, isPasswordProtected, (lobbyData4 == "1") ? Platform.Unknown : PlatformManager.DistributionPlatform.Platform, modifiers);
		return new ServerData(joinData, matchmakingData);
		IL_010f:
		joinData = new ServerJoinData(new ServerJoinDataDedicated(lobbyData2));
		goto IL_0125;
		IL_00ff:
		joinData = new ServerJoinData(new ServerJoinDataPlayFabUser(lobbyData2));
		goto IL_0125;
	}

	public string KnownBackendsString()
	{
		List<string> list = new List<string>();
		list.Add("Steam user");
		list.Add("PlayFab user");
		list.Add("Dedicated");
		return "Known backends: " + string.Join(", ", list.Select((string s) => "\"" + s + "\""));
	}

	public void GetServers(List<ServerData> allServers)
	{
		if (m_friendsFilter)
		{
			FilterServers(m_friendServers, allServers);
			return;
		}
		FilterServers(m_matchmakingServers, allServers);
		FilterServers(m_dedicatedServers, allServers);
	}

	private void FilterServers(List<ServerData> input, List<ServerData> allServers)
	{
		string text = m_nameFilter.ToLowerInvariant();
		foreach (ServerData item in input)
		{
			if (text.Length == 0 || item.m_matchmakingData.m_serverName.ToLowerInvariant().Contains(text))
			{
				allServers.Add(item);
			}
			if (allServers.Count >= 200)
			{
				break;
			}
		}
	}

	public void CheckIfOnlineAsync(IPEndPoint server, ServerPingCompletedHandler completedCallback)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		IPv6Address address = server.m_address;
		if ((int)((IPv6Address)(ref address)).AddressRange == 2)
		{
			EnqueuePing(server, completedCallback);
		}
		else
		{
			completedCallback?.Invoke(new ServerData(new ServerJoinData(new ServerJoinDataDedicated(server)), ServerMatchmakingData.None));
		}
	}

	public void CheckIfOnlineAsync(ServerJoinDataSteamUser server, ServerPingCompletedHandler completedCallback)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		int friendCount = SteamFriends.GetFriendCount((EFriendFlags)4);
		bool flag = false;
		for (int i = 0; i < friendCount; i++)
		{
			if (SteamFriends.GetFriendByIndex(i, (EFriendFlags)4) == server.m_joinUserID)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			completedCallback?.Invoke(new ServerData(new ServerJoinData(server), new ServerMatchmakingData(DateTime.UtcNow, couldCheck: false)));
			return;
		}
		FriendGameInfo_t val = default(FriendGameInfo_t);
		if (!SteamFriends.GetFriendGamePlayed(server.m_joinUserID, ref val) || val.m_gameID != (CGameID)(ulong)SteamManager.APP_ID || val.m_steamIDLobby == CSteamID.Nil)
		{
			completedCallback?.Invoke(new ServerData(new ServerJoinData(server), new ServerMatchmakingData(DateTime.UtcNow)));
			return;
		}
		if (m_requestedLobbiesUserHosted.TryGetValue(val.m_steamIDLobby, out var value))
		{
			ref ServerPingCompletedHandler item = ref value.Item2;
			item = (ServerPingCompletedHandler)Delegate.Combine(item, completedCallback);
		}
		else
		{
			m_requestedLobbiesUserHosted.Add(val.m_steamIDLobby, (server.m_joinUserID, completedCallback));
		}
		SteamMatchmaking.RequestLobbyData(val.m_steamIDLobby);
	}

	public void AbortCheckIfOnlineAsync(ServerJoinDataSteamUser server)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		CSteamID val = CSteamID.Nil;
		ServerPingCompletedHandler serverPingCompletedHandler = null;
		foreach (KeyValuePair<CSteamID, (CSteamID, ServerPingCompletedHandler)> item in m_requestedLobbiesUserHosted)
		{
			if (item.Value.Item1 == server.m_joinUserID)
			{
				val = item.Key;
				serverPingCompletedHandler = item.Value.Item2;
				break;
			}
		}
		if (!(val == CSteamID.Nil))
		{
			m_requestedLobbiesUserHosted.Remove(val);
			serverPingCompletedHandler(new ServerData(new ServerJoinData(server), new ServerMatchmakingData(DateTime.UtcNow)));
		}
	}

	public bool CheckIfOnline(ServerJoinData dataToMatchAgainst, ref ServerData serverData)
	{
		for (int i = 0; i < m_friendServers.Count; i++)
		{
			if (m_friendServers[i].m_joinData.Equals(dataToMatchAgainst))
			{
				serverData = m_friendServers[i];
				return true;
			}
		}
		for (int j = 0; j < m_matchmakingServers.Count; j++)
		{
			if (m_matchmakingServers[j].m_joinData.Equals(dataToMatchAgainst))
			{
				serverData = m_matchmakingServers[j];
				return true;
			}
		}
		for (int k = 0; k < m_dedicatedServers.Count; k++)
		{
			if (m_dedicatedServers[k].m_joinData.Equals(dataToMatchAgainst))
			{
				serverData = m_dedicatedServers[k];
				return true;
			}
		}
		if (!IsRefreshing)
		{
			serverData = new ServerData(dataToMatchAgainst, ServerMatchmakingData.None);
			return true;
		}
		return false;
	}

	public bool GetJoinHost(out ServerJoinData joinData)
	{
		if (!m_joinData.IsValid)
		{
			joinData = default(ServerJoinData);
			return false;
		}
		joinData = m_joinData;
		m_joinData = ServerJoinData.None;
		return true;
	}

	private void OnServerResponded(HServerListRequest request, int iServer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(request, iServer);
		string serverName = serverDetails.GetServerName();
		SteamNetworkingIPAddr val = default(SteamNetworkingIPAddr);
		((SteamNetworkingIPAddr)(ref val)).SetIPv4(((servernetadr_t)(ref serverDetails.m_NetAdr)).GetIP(), ((servernetadr_t)(ref serverDetails.m_NetAdr)).GetConnectionPort());
		ServerJoinData joinData = new ServerJoinData(new ServerJoinDataDedicated(((SteamNetworkingIPAddr)(ref val)).GetIPv4(), val.m_port));
		DecodeTags(serverDetails.GetGameTags(), out var gameVersion, out var networkVersion, out var modifiers);
		ServerMatchmakingData matchmakingData = new ServerMatchmakingData(DateTime.UtcNow, serverName, (uint)serverDetails.m_nPlayers, (uint)serverDetails.m_nMaxPlayers, PlatformUserID.None, gameVersion, networkVersion, null, serverDetails.m_bPassword, PlatformManager.DistributionPlatform.Platform, modifiers);
		m_dedicatedServers.Add(new ServerData(joinData, matchmakingData));
		m_updateTriggerAccumulator++;
		if (m_updateTriggerAccumulator > 100)
		{
			m_updateTriggerAccumulator = 0;
			m_serverListRevision++;
		}
	}

	private static void DecodeTags(string tagsString, out GameVersion gameVersion, out uint networkVersion, out string[] modifiers)
	{
		Dictionary<string, string> dictionary = default(Dictionary<string, string>);
		string value;
		if (!StringUtils.TryDecodeStringAsIDictionary<Dictionary<string, string>>(tagsString, ref dictionary))
		{
			value = tagsString;
			networkVersion = 0u;
			modifiers = new string[0];
		}
		else
		{
			if ((!dictionary.TryGetValue("g", out value) && !dictionary.TryGetValue("gameversion", out value)) || (!dictionary.TryGetValue("n", out var value2) && !dictionary.TryGetValue("networkversion", out value2)) || !uint.TryParse(value2, out networkVersion))
			{
				value = tagsString;
				networkVersion = 0u;
			}
			Dictionary<string, string> kvps = default(Dictionary<string, string>);
			if (networkVersion != 36 || !dictionary.TryGetValue("m", out var value3) || !StringUtils.TryDecodeStringAsIDictionary<Dictionary<string, string>>(value3, ref kvps) || !ServerOptionsGUI.TryConvertCompactKVPToModifierKeys(kvps, out modifiers))
			{
				modifiers = new string[0];
			}
		}
		gameVersion = GameVersion.ParseGameVersion(value);
	}

	private void OnServerFailedToRespond(HServerListRequest request, int iServer)
	{
	}

	private void OnRefreshComplete(HServerListRequest request, EMatchMakingServerResponse response)
	{
		ZLog.Log((object)("Refresh complete " + m_dedicatedServers.Count + "  " + ((object)(EMatchMakingServerResponse)(ref response)).ToString()));
		IsRefreshing = false;
		m_serverListRevision++;
	}

	public void SetNameFilter(string filter)
	{
		if (!(m_nameFilter == filter))
		{
			m_nameFilter = filter;
			m_serverListRevision++;
		}
	}

	public void SetFriendFilter(bool enabled)
	{
		if (m_friendsFilter != enabled)
		{
			m_friendsFilter = enabled;
			m_serverListRevision++;
		}
	}

	public int GetServerListRevision()
	{
		return m_serverListRevision;
	}

	public bool GetServerListRevision(ref int revision)
	{
		bool result = m_serverListRevision != revision;
		revision = m_serverListRevision;
		return result;
	}

	public int GetTotalNrOfServers()
	{
		return m_matchmakingServers.Count + m_dedicatedServers.Count + m_friendServers.Count;
	}
}
