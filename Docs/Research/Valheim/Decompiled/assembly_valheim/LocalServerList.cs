using System;
using System.Collections.Generic;
using System.IO;

public class LocalServerList : IServerList, IDisposable
{
	private const uint serverListVersion = 1u;

	private static List<ServerJoinData> m_tempServerList1 = new List<ServerJoinData>();

	private static List<ServerJoinData> m_tempServerList2 = new List<ServerJoinData>();

	private readonly string m_displayName;

	private readonly FileLocation[] m_locations;

	private readonly HashSet<string> m_activeDnsResolveRequests = new HashSet<string>();

	private DateTime m_lastRefreshedTimeUtc = DateTime.MinValue;

	private readonly List<ServerJoinData> m_list = new List<ServerJoinData>();

	private string m_filter = "";

	private bool m_isLoaded;

	private bool m_wasModified;

	public string DisplayName => m_displayName;

	public DateTime LastRefreshTimeUtc => m_lastRefreshedTimeUtc;

	public bool CanRefresh => true;

	public int Count => m_list.Count;

	public uint TotalServers => (uint)Count;

	public ServerJoinData this[int index] => m_list[index];

	public event ServerListUpdatedHandler ServerListUpdated;

	public LocalServerList(string displayName, FileLocation[] locations)
	{
		m_displayName = displayName;
		m_locations = locations;
		MultiBackendMatchmaking.Hold();
		LoadFromDisk();
		Refresh();
	}

	public void Dispose()
	{
		MultiBackendMatchmaking.Release();
		m_list.Clear();
		m_isLoaded = false;
	}

	public void Add(ServerJoinData entry)
	{
		m_wasModified = true;
		m_list.Add(entry);
	}

	public void AddToBeginning(ServerJoinData entry)
	{
		m_wasModified = true;
		m_list.Insert(0, entry);
	}

	public void Remove(ServerJoinData joinData)
	{
		m_wasModified = true;
		for (int num = m_list.Count - 1; num >= 0; num--)
		{
			if (m_list[num] == joinData)
			{
				m_list.RemoveAt(num);
				num--;
			}
		}
	}

	public void Swap(int index1, int index2)
	{
		m_wasModified = true;
		ServerJoinData value = m_list[index1];
		m_list[index1] = m_list[index2];
		m_list[index2] = value;
	}

	public void SetFilter(string filter, bool isTyping = false)
	{
		m_filter = filter;
	}

	public void GetFilteredList(List<ServerListEntryData> resultOutput)
	{
		resultOutput.Clear();
		GetFilteredListInternal(m_tempServerList2);
		for (int i = 0; i < m_tempServerList2.Count; i++)
		{
			ServerJoinData serverJoinData = m_tempServerList2[i];
			ServerMatchmakingData serverMatchmakingData = MultiBackendMatchmaking.GetServerMatchmakingData(serverJoinData, m_lastRefreshedTimeUtc);
			ServerData serverData = new ServerData(serverJoinData, serverMatchmakingData);
			string serverName;
			if (MultiBackendMatchmaking.TryGetServerName(serverJoinData, out var serverNameAtTimePoint, out var source))
			{
				if (source == ServerNameSource.Matchmaking)
				{
					MultiBackendMatchmaking.SetServerName(serverJoinData, serverNameAtTimePoint);
				}
				serverName = serverNameAtTimePoint.m_name;
			}
			else
			{
				serverName = serverJoinData.ToString();
			}
			resultOutput.Add(new ServerListEntryData(serverData, serverName));
		}
		m_tempServerList2.Clear();
	}

	public void GetFilteredListInternal(List<ServerJoinData> resultOutput)
	{
		resultOutput.Clear();
		ServerListUtils.GetFilteredList(m_list, m_filter, resultOutput);
	}

	public void Refresh()
	{
		m_lastRefreshedTimeUtc = DateTime.UtcNow;
		MultiBackendMatchmaking.Instance.m_dnsResolver.ClearCache();
		this.ServerListUpdated?.Invoke();
		if (!m_isLoaded)
		{
			ZLog.LogError((object)"Local server list was not loaded!");
		}
	}

	public void OnOpen()
	{
		if (!m_isLoaded)
		{
			LoadFromDisk();
		}
	}

	public void OnClose()
	{
	}

	public void Tick()
	{
		GetFilteredListInternal(m_tempServerList1);
		ResolveDomainNames(m_tempServerList1);
		ServerListUtils.UpdateServerOnlineStatus(m_tempServerList1, m_lastRefreshedTimeUtc, delegate
		{
			this.ServerListUpdated?.Invoke();
		});
		m_tempServerList1.Clear();
	}

	private void ResolveDomainNames(IReadOnlyList<ServerJoinData> servers)
	{
		for (int i = 0; i < servers.Count; i++)
		{
			ServerJoinData serverJoinData = servers[i];
			if (serverJoinData.m_type == ServerJoinDataType.Dedicated && !MultiBackendMatchmaking.ServerIPAddressIsKnown(serverJoinData.Dedicated) && !m_activeDnsResolveRequests.Contains(serverJoinData.Dedicated.m_host))
			{
				MultiBackendMatchmaking.Instance.m_dnsResolver.ResolveDomainNameAsync(serverJoinData.Dedicated.m_host, delegate
				{
					this.ServerListUpdated?.Invoke();
				});
			}
		}
	}

	public bool Contains(ServerJoinData joinData)
	{
		int index;
		return TryGetIndexOf(joinData, out index);
	}

	public bool TryGetIndexOf(ServerJoinData joinData, out int index)
	{
		for (int i = 0; i < m_list.Count; i++)
		{
			if (m_list[i] == joinData)
			{
				index = i;
				return true;
			}
		}
		index = -1;
		return false;
	}

	private void LoadFromDisk()
	{
		List<ServerJoinData> destination = new List<ServerJoinData>();
		LoadServerListFromDisk(ref destination);
		m_list.Clear();
		m_list.AddRange(destination);
		m_list.TrimExcess();
		m_isLoaded = true;
	}

	private bool LoadServerListFromDisk(ref List<ServerJoinData> destination)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		if (m_locations == null)
		{
			ZLog.LogError((object)"No locations to load!");
			return false;
		}
		SortedList<DateTime, List<FileLocation>> sortedList = new SortedList<DateTime, List<FileLocation>>(m_locations.Length);
		for (int i = 0; i < m_locations.Length; i++)
		{
			if (FileHelpers.Exists(m_locations[i].m_path, m_locations[i].m_fileSource))
			{
				DateTime lastWriteTime = FileHelpers.GetLastWriteTime(m_locations[i].m_path, m_locations[i].m_fileSource);
				if (sortedList.ContainsKey(lastWriteTime))
				{
					sortedList[lastWriteTime].Add(m_locations[i]);
					continue;
				}
				List<FileLocation> list = new List<FileLocation>();
				list.Add(m_locations[i]);
				sortedList.Add(lastWriteTime, list);
			}
		}
		if (sortedList.Count <= 0)
		{
			ZLog.Log((object)"No list saved! Aborting load operation");
			return false;
		}
		List<ServerJoinData> joinData = new List<ServerJoinData>();
		for (int num = sortedList.Count - 1; num >= 0; num--)
		{
			for (int j = 0; j < sortedList.Values[num].Count; j++)
			{
				if (!LoadUniqueServerListEntriesIntoList(sortedList.Values[num][j], ref joinData))
				{
					ZLog.Log((object)"Failed to load list entries! Aborting load operation.");
					return false;
				}
			}
		}
		destination = joinData;
		return true;
	}

	private static bool LoadUniqueServerListEntriesIntoList(FileLocation location, ref List<ServerJoinData> joinData)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		HashSet<ServerJoinData> hashSet = new HashSet<ServerJoinData>();
		for (int i = 0; i < joinData.Count; i++)
		{
			hashSet.Add(joinData[i]);
		}
		DateTime timestampUtc = FileHelpers.GetLastWriteTime(location.m_path, location.m_fileSource).ToUniversalTime();
		FileReader val;
		try
		{
			val = new FileReader(location.m_path, location.m_fileSource, (FileHelperType)0);
		}
		catch (Exception ex)
		{
			ZLog.Log((object)("Failed to load: " + location.m_path + " (" + ex.Message + ")"));
			return false;
		}
		byte[] data;
		try
		{
			BinaryReader binary = val.m_binary;
			int count = binary.ReadInt32();
			data = binary.ReadBytes(count);
		}
		catch (Exception ex2)
		{
			ZLog.LogError((object)$"error loading player.dat. Source: {location.m_fileSource}, Path: {location.m_path}, Error: {ex2.Message}");
			val.Dispose();
			return false;
		}
		val.Dispose();
		ZPackage zPackage = new ZPackage(data);
		try
		{
			uint num = zPackage.ReadUInt();
			if (num != 0 && num != 1)
			{
				ZLog.LogError((object)("Couldn't read list of version " + num));
				return false;
			}
			int num2 = zPackage.ReadInt();
			for (int j = 0; j < num2; j++)
			{
				ServerJoinData serverJoinData = ServerJoinData.None;
				string text = zPackage.ReadString();
				string serverName = zPackage.ReadString();
				switch (text)
				{
				case "Steam user":
				{
					ulong joinUserID = zPackage.ReadULong();
					serverJoinData = new ServerJoinData(new ServerJoinDataSteamUser(joinUserID));
					break;
				}
				case "PlayFab user":
				{
					string remotePlayerId = zPackage.ReadString();
					serverJoinData = new ServerJoinData(new ServerJoinDataPlayFabUser(remotePlayerId));
					break;
				}
				case "Dedicated":
					serverJoinData = new ServerJoinData((num == 0) ? new ServerJoinDataDedicated(zPackage.ReadUInt(), (ushort)zPackage.ReadUInt()) : new ServerJoinDataDedicated(zPackage.ReadString(), (ushort)zPackage.ReadUInt()));
					break;
				default:
					ZLog.LogError((object)"Unsupported backend! This should be an impossible code path if the server list was saved and loaded properly.");
					return false;
				}
				if (serverJoinData.IsValid && !hashSet.Contains(serverJoinData))
				{
					joinData.Add(serverJoinData);
				}
				MultiBackendMatchmaking.SetServerName(serverJoinData, new ServerNameAtTimePoint(serverName, timestampUtc));
			}
		}
		catch (EndOfStreamException ex3)
		{
			ZLog.LogWarning((object)($"Something is wrong with the server list at path {location.m_path} and source {location.m_fileSource}, reached the end of the stream unexpectedly! Entries that have successfully been read so far have been added to the server list. \n" + ex3.StackTrace));
		}
		return true;
	}

	public SaveStatusCode Save(bool forceSave = false)
	{
		if (m_wasModified || forceSave)
		{
			SaveStatusCode num = SaveServerListToDisk(m_list);
			if (num == SaveStatusCode.Succeess)
			{
				m_wasModified = false;
			}
			return num;
		}
		return SaveStatusCode.SuccessNoWriteNeeded;
	}

	private SaveStatusCode SaveServerListToDisk(List<ServerJoinData> list)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (m_locations == null)
		{
			ZLog.LogError((object)"No locations to save to!");
			return SaveStatusCode.UnsupportedServerListType;
		}
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < m_locations.Length; i++)
		{
			switch (SaveServerListEntries(m_locations[i], list))
			{
			case SaveStatusCode.Succeess:
				flag = true;
				break;
			case SaveStatusCode.CloudQuotaExceeded:
				flag2 = true;
				break;
			default:
				ZLog.LogError((object)"Unknown error when saving server list");
				break;
			case SaveStatusCode.UnknownServerBackend:
				break;
			}
		}
		if (flag)
		{
			return SaveStatusCode.Succeess;
		}
		if (flag2)
		{
			return SaveStatusCode.CloudQuotaExceeded;
		}
		return SaveStatusCode.FailedUnknownReason;
	}

	private static SaveStatusCode SaveServerListEntries(FileLocation location, List<ServerJoinData> list)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Invalid comparison between Unknown and I4
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		string text = location.m_path + ".old";
		string text2 = location.m_path + ".new";
		ZPackage zPackage = new ZPackage();
		zPackage.Write(1u);
		zPackage.Write(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			ServerJoinData server = list[i];
			zPackage.Write(server.GetDataName());
			string serverName = MultiBackendMatchmaking.GetServerName(server);
			zPackage.Write(serverName);
			switch (server.GetDataName())
			{
			case "Steam user":
				zPackage.Write((ulong)server.SteamUser.m_joinUserID);
				break;
			case "PlayFab user":
				zPackage.Write(server.PlayFabUser.m_remotePlayerId.ToString());
				break;
			case "Dedicated":
				zPackage.Write((server.Dedicated.m_host == null) ? "" : server.Dedicated.m_host);
				zPackage.Write((uint)server.Dedicated.m_port);
				break;
			default:
				ZLog.LogError((object)"Unsupported backend! Aborting save operation.");
				return SaveStatusCode.UnknownServerBackend;
			}
		}
		if (FileHelpers.CloudStorageEnabled && (int)location.m_fileSource == 2)
		{
			ulong num = 0uL;
			if (FileHelpers.FileExistsCloud(location.m_path))
			{
				num += FileHelpers.GetFileSize(location.m_path, location.m_fileSource);
			}
			num = Math.Max(4uL + (ulong)zPackage.Size(), num);
			num *= 2;
			if (FileHelpers.OperationExceedsCloudCapacity(num))
			{
				ZLog.LogWarning((object)"Saving server list to cloud would exceed the cloud storage quota. Therefore the operation has been aborted!");
				return SaveStatusCode.CloudQuotaExceeded;
			}
		}
		byte[] array = zPackage.GetArray();
		FileWriter val = new FileWriter(text2, (FileHelperType)0, location.m_fileSource);
		val.m_binary.Write(array.Length);
		val.m_binary.Write(array);
		val.Finish();
		FileHelpers.ReplaceOldFile(location.m_path, text2, text, location.m_fileSource);
		return SaveStatusCode.Succeess;
	}
}
