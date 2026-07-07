using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.MultiplayerModels;
using Splatform;

public class ZPlayFabLobbySearch
{
	private delegate void QueueableAPICall();

	private readonly ZPlayFabMatchmakingSuccessCallback m_successAction;

	private readonly ZPlayFabMatchmakingFailedCallback m_failedAction;

	private readonly ZPlayFabMatchmakingNewServersCallback m_newServersAction;

	private readonly ZPlayFabMatchmakingServerSearchDoneCallback m_finishedAction;

	private readonly string[] m_searchFilters;

	private readonly bool m_joinLobby;

	private readonly bool m_verboseLog;

	private int m_retries;

	private float m_retryIn = -1f;

	private int m_currentPage;

	private int m_currentFilter;

	private const float rateLimit = 2f;

	private DateTime m_previousAPICallTime = DateTime.MinValue;

	private Queue<QueueableAPICall> m_APICallQueue = new Queue<QueueableAPICall>();

	internal bool IsDone { get; private set; }

	internal ZPlayFabLobbySearch(ZPlayFabMatchmakingNewServersCallback newServersAction, ZPlayFabMatchmakingServerSearchDoneCallback finishedAction, string[] searchFilters)
	{
		m_newServersAction = newServersAction;
		m_finishedAction = finishedAction;
		m_searchFilters = searchFilters;
		m_joinLobby = false;
	}

	internal ZPlayFabLobbySearch(ZPlayFabMatchmakingSuccessCallback successAction, ZPlayFabMatchmakingFailedCallback failedAction, string searchFilter, ZPlayFabLobbySearchFlags flags)
	{
		m_successAction = successAction;
		m_failedAction = failedAction;
		m_searchFilters = new string[1] { searchFilter };
		m_joinLobby = flags.HasFlag(ZPlayFabLobbySearchFlags.Join);
		if (!flags.HasFlag(ZPlayFabLobbySearchFlags.Queued))
		{
			FindLobby();
		}
		if (flags.HasFlag(ZPlayFabLobbySearchFlags.AllowRetry))
		{
			m_retries = 3;
		}
	}

	internal void Update(float deltaTime)
	{
		if (m_retryIn > 0f)
		{
			m_retryIn -= deltaTime;
			if (m_retryIn <= 0f)
			{
				FindLobby();
			}
		}
		TickAPICallRateLimiter();
	}

	internal void FindLobby()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (m_newServersAction == null)
		{
			FindLobbiesRequest request = new FindLobbiesRequest
			{
				Filter = m_searchFilters[m_currentFilter]
			};
			QueueAPICall(delegate
			{
				PlayFabMultiplayerAPI.FindLobbies(request, (Action<FindLobbiesResult>)OnFindLobbySuccess, (Action<PlayFabError>)OnFindLobbyFailed, (object)null, (Dictionary<string, string>)null);
			});
		}
		else
		{
			FindLobbyWithPagination();
		}
	}

	private void FindLobbyWithPagination()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_0062: Expected O, but got Unknown
		FindLobbiesRequest request = new FindLobbiesRequest
		{
			Filter = m_searchFilters[m_currentFilter] + string.Format(" and {0} eq {1}", "number_key11", m_currentPage),
			Pagination = new PaginationRequest
			{
				PageSizeRequested = 50u
			}
		};
		if (m_verboseLog)
		{
			ZLog.Log((object)$"Page {m_currentPage}, {4 - m_currentPage - 1} remains: {request.Filter}");
		}
		QueueAPICall(delegate
		{
			PlayFabMultiplayerAPI.FindLobbies(request, (Action<FindLobbiesResult>)OnFindServersSuccess, (Action<PlayFabError>)OnFindLobbyFailed, (object)null, (Dictionary<string, string>)null);
		});
	}

	private void RetryOrFail(string error)
	{
		if (m_retries > 0)
		{
			m_retries--;
			m_retryIn = 1f;
		}
		else
		{
			ZLog.Log((object)$"PlayFab lobby matching search filter '{m_searchFilters[m_currentFilter]}': {error}");
			OnFailed(ZPLayFabMatchmakingFailReason.Unknown);
		}
	}

	private void OnFindLobbyFailed(PlayFabError error)
	{
		if (!IsDone)
		{
			RetryOrFail(((object)error).ToString());
		}
	}

	private void OnFindLobbySuccess(FindLobbiesResult result)
	{
		if (IsDone)
		{
			return;
		}
		if (result.Lobbies.Count == 0)
		{
			RetryOrFail("Got back zero lobbies");
			return;
		}
		LobbySummary val = result.Lobbies[0];
		if (result.Lobbies.Count > 1)
		{
			ZLog.LogWarning((object)$"Expected zero or one lobby got {result.Lobbies.Count} matching lobbies, returning newest lobby");
			long num = long.Parse(val.SearchData["string_key9"]);
			foreach (LobbySummary lobby in result.Lobbies)
			{
				long num2 = long.Parse(lobby.SearchData["string_key9"]);
				if (num < num2)
				{
					val = lobby;
					num = num2;
				}
			}
		}
		if (m_joinLobby)
		{
			JoinLobby(val.LobbyId, val.ConnectionString);
			ZPlayFabMatchmaking.JoinCode = val.SearchData["string_key4"];
		}
		else
		{
			DeliverLobby(val);
			IsDone = true;
		}
	}

	private void JoinLobby(string lobbyId, string connectionString)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		JoinLobbyRequest request = new JoinLobbyRequest
		{
			ConnectionString = connectionString,
			MemberEntity = ZPlayFabMatchmaking.GetEntityKeyForLocalUser()
		};
		QueueAPICall(delegate
		{
			PlayFabMultiplayerAPI.JoinLobby(request, (Action<JoinLobbyResult>)delegate(JoinLobbyResult result)
			{
				OnJoinLobbySuccess(result.LobbyId);
			}, (Action<PlayFabError>)delegate(PlayFabError error)
			{
				OnJoinLobbyFailed(error, lobbyId);
			}, (object)null, (Dictionary<string, string>)null);
		});
	}

	private void OnJoinLobbySuccess(string lobbyId)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (!IsDone)
		{
			GetLobbyRequest request = new GetLobbyRequest
			{
				LobbyId = lobbyId
			};
			QueueAPICall(delegate
			{
				PlayFabMultiplayerAPI.GetLobby(request, (Action<GetLobbyResult>)OnGetLobbySuccess, (Action<PlayFabError>)OnGetLobbyFailed, (object)null, (Dictionary<string, string>)null);
			});
		}
	}

	private void OnJoinLobbyFailed(PlayFabError error, string lobbyId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		PlayFabErrorCode error2 = error.Error;
		if ((int)error2 <= 1199)
		{
			if ((int)error2 == 1130 || (int)error2 == 1199)
			{
				goto IL_0055;
			}
		}
		else
		{
			if ((int)error2 == 13002)
			{
				OnJoinLobbySuccess(lobbyId);
				return;
			}
			if ((int)error2 == 13003)
			{
				ZLog.Log((object)"Can't join lobby because it's not joinable, likely because it's full.");
				OnFailed(ZPLayFabMatchmakingFailReason.ServerFull);
				return;
			}
			if ((int)error2 == 13008)
			{
				goto IL_0055;
			}
		}
		ZLog.LogError((object)("Failed to get lobby: " + ((object)error).ToString()));
		OnFailed(ZPLayFabMatchmakingFailReason.Unknown);
		return;
		IL_0055:
		OnFailed(ZPLayFabMatchmakingFailReason.APIRequestLimitExceeded);
	}

	private void DeliverLobby(LobbySummary lobbySummary)
	{
		PlayFabMatchmakingServerData playFabMatchmakingServerData = ToServerData(lobbySummary.LobbyId, lobbySummary.CurrentPlayers, lobbySummary.MaxPlayers, lobbySummary.SearchData, null, subtractOneFromPlayerCountIfDedicated: true);
		if (m_verboseLog && playFabMatchmakingServerData != null)
		{
			ZLog.Log((object)("Deliver server data\n" + playFabMatchmakingServerData.ToString()));
		}
		m_successAction(playFabMatchmakingServerData);
	}

	private void DeliverLobbies(IReadOnlyList<LobbySummary> lobbySummaies)
	{
		PlayFabMatchmakingServerData[] array = new PlayFabMatchmakingServerData[lobbySummaies.Count];
		for (int i = 0; i < lobbySummaies.Count; i++)
		{
			LobbySummary val = lobbySummaies[i];
			PlayFabMatchmakingServerData playFabMatchmakingServerData = ToServerData(val.LobbyId, val.CurrentPlayers, val.MaxPlayers, val.SearchData, null, subtractOneFromPlayerCountIfDedicated: true);
			if (m_verboseLog && playFabMatchmakingServerData != null)
			{
				ZLog.Log((object)("Deliver server data\n" + playFabMatchmakingServerData.ToString()));
			}
			array[i] = playFabMatchmakingServerData;
		}
		m_newServersAction(array);
	}

	private void OnFindServersSuccess(FindLobbiesResult result)
	{
		if (IsDone)
		{
			return;
		}
		DeliverLobbies(result.Lobbies);
		m_currentPage++;
		if (m_currentPage >= 4)
		{
			m_currentFilter++;
			if (m_currentFilter >= m_searchFilters.Length)
			{
				OnFinished(ZPLayFabMatchmakingFailReason.None);
				return;
			}
			m_currentPage = 0;
		}
		FindLobbyWithPagination();
	}

	private void OnGetLobbySuccess(GetLobbyResult result)
	{
		PlayFabMatchmakingServerData playFabMatchmakingServerData = ToServerData(result);
		if (IsDone)
		{
			OnFailed(ZPLayFabMatchmakingFailReason.Cancelled);
			return;
		}
		if (playFabMatchmakingServerData == null)
		{
			OnFailed(ZPLayFabMatchmakingFailReason.InvalidServerData);
			return;
		}
		IsDone = true;
		ZLog.Log((object)("Get Lobby\n" + playFabMatchmakingServerData.ToString()));
		m_successAction(playFabMatchmakingServerData);
	}

	private void OnGetLobbyFailed(PlayFabError error)
	{
		ZLog.LogError((object)("Failed to get lobby: " + ((object)error).ToString()));
		OnFailed(ZPLayFabMatchmakingFailReason.Unknown);
	}

	private static PlayFabMatchmakingServerData ToServerData(string lobbyID, uint playerCount, uint maxPlayerCount, Dictionary<string, string> searchData, Dictionary<string, string> lobbyData = null, bool subtractOneFromPlayerCountIfDedicated = false)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!bool.TryParse(searchData["string_key3"], out var result) || !bool.TryParse(searchData["string_key7"], out var result2) || !long.TryParse(searchData["string_key9"], out var result3))
			{
				ZLog.LogWarning((object)"Got PlayFab lobby entry with invalid data");
				return null;
			}
			string versionString = searchData["string_key6"];
			uint num = uint.Parse(searchData["number_key13"]);
			if (!GameVersion.TryParseGameVersion(versionString, out var version) || version < Version.FirstVersionWithNetworkVersion)
			{
				num = 0u;
			}
			Dictionary<string, string> kvps = default(Dictionary<string, string>);
			if (num != 36 || !searchData.TryGetValue("string_key14", out var value) || !StringUtils.TryDecodeStringAsIDictionary<Dictionary<string, string>>(value, ref kvps) || !ServerOptionsGUI.TryConvertCompactKVPToModifierKeys(kvps, out var result4))
			{
				result4 = new string[0];
			}
			PlatformUserID none = default(PlatformUserID);
			if (!PlatformUserID.TryParse(searchData["string_key8"], ref none))
			{
				none = PlatformUserID.None;
			}
			PlayFabMatchmakingServerData playFabMatchmakingServerData = new PlayFabMatchmakingServerData
			{
				isCommunityServer = result,
				isDedicatedServer = result2,
				joinCode = searchData["string_key4"],
				lobbyId = lobbyID,
				numPlayers = ((result2 && subtractOneFromPlayerCountIfDedicated) ? (playerCount - 1) : playerCount),
				maxNumPlayers = ((result2 && subtractOneFromPlayerCountIfDedicated) ? (maxPlayerCount - 1) : maxPlayerCount),
				remotePlayerId = searchData["string_key1"],
				serverIp = searchData["string_key10"],
				serverName = searchData["string_key5"],
				tickCreated = result3,
				gameVersion = version,
				modifiers = result4,
				networkVersion = num,
				platformUserID = none,
				platformRestriction = new Platform((searchData["string_key12"] == "None") ? null : searchData["string_key12"])
			};
			if (lobbyData != null)
			{
				playFabMatchmakingServerData.havePassword = bool.Parse(lobbyData[PlayFabAttrKey.HavePassword.ToKeyString()]);
				playFabMatchmakingServerData.networkId = lobbyData[PlayFabAttrKey.NetworkId.ToKeyString()];
				playFabMatchmakingServerData.worldName = lobbyData[PlayFabAttrKey.WorldName.ToKeyString()];
			}
			return playFabMatchmakingServerData;
		}
		catch (KeyNotFoundException)
		{
			ZLog.LogWarning((object)"Got PlayFab lobby entry with missing key(s)");
			return null;
		}
		catch
		{
			return null;
		}
	}

	private static PlayFabMatchmakingServerData ToServerData(GetLobbyResult result)
	{
		return ToServerData(result.Lobby.LobbyId, (uint)result.Lobby.Members.Count, result.Lobby.MaxPlayers, result.Lobby.SearchData, result.Lobby.LobbyData);
	}

	private void OnFailed(ZPLayFabMatchmakingFailReason failReason)
	{
		if (!IsDone)
		{
			IsDone = true;
			if (m_failedAction != null)
			{
				m_failedAction(failReason);
			}
		}
	}

	private void OnFinished(ZPLayFabMatchmakingFailReason failReason)
	{
		if (!IsDone)
		{
			IsDone = true;
			if (m_finishedAction != null)
			{
				m_finishedAction(failReason);
			}
		}
	}

	public void Cancel()
	{
		IsDone = true;
	}

	private void QueueAPICall(QueueableAPICall apiCallDelegate)
	{
		m_APICallQueue.Enqueue(apiCallDelegate);
		TickAPICallRateLimiter();
	}

	private void TickAPICallRateLimiter()
	{
		if (m_APICallQueue.Count > 0 && (DateTime.UtcNow - m_previousAPICallTime).TotalSeconds >= 2.0)
		{
			m_APICallQueue.Dequeue()();
			m_previousAPICallTime = DateTime.UtcNow;
		}
	}
}
