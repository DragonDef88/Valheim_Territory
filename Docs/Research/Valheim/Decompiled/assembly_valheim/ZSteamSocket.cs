using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Steamworks;
using UnityEngine;

public class ZSteamSocket : IDisposable, ISocket
{
	private static List<ZSteamSocket> m_sockets = new List<ZSteamSocket>();

	private static Callback<SteamNetConnectionStatusChangedCallback_t> m_statusChanged;

	private static int m_steamDataPort = 2459;

	private Queue<ZSteamSocket> m_pendingConnections = new Queue<ZSteamSocket>();

	private HSteamNetConnection m_con = HSteamNetConnection.Invalid;

	private SteamNetworkingIdentity m_peerID;

	private Queue<ZPackage> m_pkgQueue = new Queue<ZPackage>();

	private Queue<byte[]> m_sendQueue = new Queue<byte[]>();

	private int m_totalSent;

	private int m_totalRecv;

	private bool m_gotData;

	private HSteamListenSocket m_listenSocket = HSteamListenSocket.Invalid;

	private static ZSteamSocket m_hostSocket;

	private static ESteamNetworkingConfigValue[] m_configValues = (ESteamNetworkingConfigValue[])(object)new ESteamNetworkingConfigValue[1];

	public ZSteamSocket()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		RegisterGlobalCallbacks();
		m_sockets.Add(this);
	}

	public ZSteamSocket(SteamNetworkingIPAddr host)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		RegisterGlobalCallbacks();
		string text = default(string);
		((SteamNetworkingIPAddr)(ref host)).ToString(ref text, true);
		ZLog.Log((object)("Starting to connect to " + text));
		m_con = SteamNetworkingSockets.ConnectByIPAddress(ref host, 0, (SteamNetworkingConfigValue_t[])null);
		m_sockets.Add(this);
	}

	public ZSteamSocket(CSteamID peerID)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		RegisterGlobalCallbacks();
		((SteamNetworkingIdentity)(ref m_peerID)).SetSteamID(peerID);
		m_con = SteamNetworkingSockets.ConnectP2P(ref m_peerID, 0, 0, (SteamNetworkingConfigValue_t[])null);
		CSteamID steamID = ((SteamNetworkingIdentity)(ref m_peerID)).GetSteamID();
		ZLog.Log((object)("Connecting to " + ((object)(CSteamID)(ref steamID)).ToString()));
		m_sockets.Add(this);
	}

	public ZSteamSocket(HSteamNetConnection con)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		RegisterGlobalCallbacks();
		m_con = con;
		SteamNetConnectionInfo_t val = default(SteamNetConnectionInfo_t);
		SteamNetworkingSockets.GetConnectionInfo(m_con, ref val);
		m_peerID = val.m_identityRemote;
		ZLog.Log((object)("Connecting to " + ((object)(SteamNetworkingIdentity)(ref m_peerID)).ToString()));
		m_sockets.Add(this);
	}

	private static void RegisterGlobalCallbacks()
	{
		if (m_statusChanged == null)
		{
			m_statusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create((DispatchDelegate<SteamNetConnectionStatusChangedCallback_t>)OnStatusChanged);
			GCHandle gCHandle = GCHandle.Alloc(30000f, GCHandleType.Pinned);
			GCHandle gCHandle2 = GCHandle.Alloc(1, GCHandleType.Pinned);
			GCHandle gCHandle3 = GCHandle.Alloc(153600, GCHandleType.Pinned);
			SteamNetworkingUtils.SetConfigValue((ESteamNetworkingConfigValue)25, (ESteamNetworkingConfigScope)1, IntPtr.Zero, (ESteamNetworkingConfigDataType)3, gCHandle.AddrOfPinnedObject());
			SteamNetworkingUtils.SetConfigValue((ESteamNetworkingConfigValue)23, (ESteamNetworkingConfigScope)1, IntPtr.Zero, (ESteamNetworkingConfigDataType)1, gCHandle2.AddrOfPinnedObject());
			SteamNetworkingUtils.SetConfigValue((ESteamNetworkingConfigValue)10, (ESteamNetworkingConfigScope)1, IntPtr.Zero, (ESteamNetworkingConfigDataType)1, gCHandle3.AddrOfPinnedObject());
			SteamNetworkingUtils.SetConfigValue((ESteamNetworkingConfigValue)11, (ESteamNetworkingConfigScope)1, IntPtr.Zero, (ESteamNetworkingConfigDataType)1, gCHandle3.AddrOfPinnedObject());
			gCHandle.Free();
			gCHandle2.Free();
			gCHandle3.Free();
		}
	}

	private static void UnregisterGlobalCallbacks()
	{
		ZLog.Log((object)("ZSteamSocket  UnregisterGlobalCallbacks, existing sockets:" + m_sockets.Count));
		if (m_statusChanged != null)
		{
			m_statusChanged.Dispose();
			m_statusChanged = null;
		}
	}

	private static void OnStatusChanged(SteamNetConnectionStatusChangedCallback_t data)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Invalid comparison between Unknown and I4
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Invalid comparison between Unknown and I4
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)("Got status changed msg " + ((object)(ESteamNetworkingConnectionState)(ref data.m_info.m_eState)).ToString()));
		if ((int)data.m_info.m_eState == 3 && (int)data.m_eOldState == 1)
		{
			ZLog.Log((object)"Connected");
			ZSteamSocket zSteamSocket = FindSocket(data.m_hConn);
			if (zSteamSocket != null)
			{
				SteamNetConnectionInfo_t val = default(SteamNetConnectionInfo_t);
				if (SteamNetworkingSockets.GetConnectionInfo(data.m_hConn, ref val))
				{
					zSteamSocket.m_peerID = val.m_identityRemote;
				}
				CSteamID steamID = ((SteamNetworkingIdentity)(ref zSteamSocket.m_peerID)).GetSteamID();
				ZLog.Log((object)("Got connection SteamID " + ((object)(CSteamID)(ref steamID)).ToString()));
			}
		}
		if ((int)data.m_info.m_eState == 1 && (int)data.m_eOldState == 0)
		{
			ZLog.Log((object)"New connection");
			GetListner()?.OnNewConnection(data.m_hConn);
		}
		if ((int)data.m_info.m_eState == 5)
		{
			ZLog.Log((object)("Got problem " + data.m_info.m_eEndReason + ":" + ((SteamNetConnectionInfo_t)(ref data.m_info)).m_szEndDebug));
			ZSteamSocket zSteamSocket2 = FindSocket(data.m_hConn);
			if (zSteamSocket2 != null)
			{
				ZLog.Log((object)("  Closing socket " + zSteamSocket2.GetHostName()));
				zSteamSocket2.Close();
			}
		}
		if ((int)data.m_info.m_eState == 4)
		{
			ZLog.Log((object)("Socket closed by peer " + ((object)(SteamNetConnectionStatusChangedCallback_t)(ref data)).ToString()));
			ZSteamSocket zSteamSocket3 = FindSocket(data.m_hConn);
			if (zSteamSocket3 != null)
			{
				ZLog.Log((object)("  Closing socket " + zSteamSocket3.GetHostName()));
				zSteamSocket3.Close();
			}
		}
	}

	private static ZSteamSocket FindSocket(HSteamNetConnection con)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		foreach (ZSteamSocket socket in m_sockets)
		{
			if (socket.m_con == con)
			{
				return socket;
			}
		}
		return null;
	}

	public void Dispose()
	{
		ZLog.Log((object)"Disposing socket");
		Close();
		m_pkgQueue.Clear();
		m_sockets.Remove(this);
		if (m_sockets.Count == 0)
		{
			ZLog.Log((object)"Last socket, unregistering callback");
			UnregisterGlobalCallbacks();
		}
	}

	public void Close()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (m_con != HSteamNetConnection.Invalid)
		{
			ZLog.Log((object)("Closing socket " + GetEndPointString()));
			Flush();
			ZLog.Log((object)("  send queue size:" + m_sendQueue.Count));
			Thread.Sleep(100);
			CSteamID steamID = ((SteamNetworkingIdentity)(ref m_peerID)).GetSteamID();
			SteamNetworkingSockets.CloseConnection(m_con, 0, "", false);
			SteamUser.EndAuthSession(steamID);
			m_con = HSteamNetConnection.Invalid;
		}
		if (m_listenSocket != HSteamListenSocket.Invalid)
		{
			ZLog.Log((object)"Stopping listening socket");
			SteamNetworkingSockets.CloseListenSocket(m_listenSocket);
			m_listenSocket = HSteamListenSocket.Invalid;
		}
		if (m_hostSocket == this)
		{
			m_hostSocket = null;
		}
		((SteamNetworkingIdentity)(ref m_peerID)).Clear();
	}

	public bool StartHost()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (m_hostSocket != null)
		{
			ZLog.Log((object)"Listen socket already started");
			return false;
		}
		m_listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, (SteamNetworkingConfigValue_t[])null);
		m_hostSocket = this;
		m_pendingConnections.Clear();
		return true;
	}

	private void OnNewConnection(HSteamNetConnection con)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		EResult val = SteamNetworkingSockets.AcceptConnection(con);
		ZLog.Log((object)("Accepting connection " + ((object)(EResult)(ref val)).ToString()));
		if ((int)val == 1)
		{
			QueuePendingConnection(con);
		}
	}

	private void QueuePendingConnection(HSteamNetConnection con)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		ZSteamSocket item = new ZSteamSocket(con);
		m_pendingConnections.Enqueue(item);
	}

	public ISocket Accept()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (m_listenSocket == HSteamListenSocket.Invalid)
		{
			return null;
		}
		if (m_pendingConnections.Count > 0)
		{
			return m_pendingConnections.Dequeue();
		}
		return null;
	}

	public bool IsConnected()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return m_con != HSteamNetConnection.Invalid;
	}

	public void Send(ZPackage pkg)
	{
		if (pkg.Size() != 0 && IsConnected())
		{
			byte[] array = pkg.GetArray();
			m_sendQueue.Enqueue(array);
			SendQueuedPackages();
		}
	}

	public bool Flush()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		SendQueuedPackages();
		_ = m_con;
		SteamNetworkingSockets.FlushMessagesOnConnection(m_con);
		return m_sendQueue.Count == 0;
	}

	private void SendQueuedPackages()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		if (!IsConnected())
		{
			return;
		}
		long num = default(long);
		while (m_sendQueue.Count > 0)
		{
			byte[] array = m_sendQueue.Peek();
			IntPtr intPtr = Marshal.AllocHGlobal(array.Length);
			Marshal.Copy(array, 0, intPtr, array.Length);
			EResult val = SteamNetworkingSockets.SendMessageToConnection(m_con, intPtr, (uint)array.Length, 8, ref num);
			Marshal.FreeHGlobal(intPtr);
			if ((int)val == 1)
			{
				m_totalSent += array.Length;
				m_sendQueue.Dequeue();
				continue;
			}
			ZLog.Log((object)("Failed to send data " + ((object)(EResult)(ref val)).ToString()));
			break;
		}
	}

	public static void UpdateAllSockets(float dt)
	{
		foreach (ZSteamSocket socket in m_sockets)
		{
			socket.Update(dt);
		}
	}

	private void Update(float dt)
	{
		SendQueuedPackages();
	}

	private static ZSteamSocket GetListner()
	{
		return m_hostSocket;
	}

	public ZPackage Recv()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!IsConnected())
		{
			return null;
		}
		IntPtr[] array = new IntPtr[1];
		if (SteamNetworkingSockets.ReceiveMessagesOnConnection(m_con, array, 1) == 1)
		{
			SteamNetworkingMessage_t val = Marshal.PtrToStructure<SteamNetworkingMessage_t>(array[0]);
			byte[] array2 = new byte[val.m_cbSize];
			Marshal.Copy(val.m_pData, array2, 0, val.m_cbSize);
			ZPackage zPackage = new ZPackage(array2);
			SteamNetworkingMessage_t.Release(array[0]);
			m_totalRecv += zPackage.Size();
			m_gotData = true;
			return zPackage;
		}
		return null;
	}

	public string GetEndPointString()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		CSteamID steamID = ((SteamNetworkingIdentity)(ref m_peerID)).GetSteamID();
		return ((object)(CSteamID)(ref steamID)).ToString();
	}

	public string GetHostName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		CSteamID steamID = ((SteamNetworkingIdentity)(ref m_peerID)).GetSteamID();
		return ((object)(CSteamID)(ref steamID)).ToString();
	}

	public CSteamID GetPeerID()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((SteamNetworkingIdentity)(ref m_peerID)).GetSteamID();
	}

	public bool IsHost()
	{
		return m_hostSocket != null;
	}

	public int GetSendQueueSize()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (!IsConnected())
		{
			return 0;
		}
		int num = 0;
		foreach (byte[] item in m_sendQueue)
		{
			num += item.Length;
		}
		SteamNetConnectionRealTimeStatus_t val = default(SteamNetConnectionRealTimeStatus_t);
		SteamNetConnectionRealTimeLaneStatus_t val2 = default(SteamNetConnectionRealTimeLaneStatus_t);
		if ((int)SteamNetworkingSockets.GetConnectionRealTimeStatus(m_con, ref val, 0, ref val2) == 1)
		{
			num += val.m_cbPendingReliable + val.m_cbPendingUnreliable + val.m_cbSentUnackedReliable;
		}
		return num;
	}

	public int GetCurrentSendRate()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		SteamNetConnectionRealTimeStatus_t val = default(SteamNetConnectionRealTimeStatus_t);
		SteamNetConnectionRealTimeLaneStatus_t val2 = default(SteamNetConnectionRealTimeLaneStatus_t);
		if ((int)SteamNetworkingSockets.GetConnectionRealTimeStatus(m_con, ref val, 0, ref val2) != 1)
		{
			return 0;
		}
		int num = val.m_cbPendingReliable + val.m_cbPendingUnreliable + val.m_cbSentUnackedReliable;
		foreach (byte[] item in m_sendQueue)
		{
			num += item.Length;
		}
		return num / Mathf.Clamp(val.m_nPing, 5, 250) * 1000;
	}

	public void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		SteamNetConnectionRealTimeStatus_t val = default(SteamNetConnectionRealTimeStatus_t);
		SteamNetConnectionRealTimeLaneStatus_t val2 = default(SteamNetConnectionRealTimeLaneStatus_t);
		if ((int)SteamNetworkingSockets.GetConnectionRealTimeStatus(m_con, ref val, 0, ref val2) == 1)
		{
			localQuality = val.m_flConnectionQualityLocal;
			remoteQuality = val.m_flConnectionQualityRemote;
			ping = val.m_nPing;
			outByteSec = val.m_flOutBytesPerSec;
			inByteSec = val.m_flInBytesPerSec;
		}
		else
		{
			localQuality = 0f;
			remoteQuality = 0f;
			ping = 0;
			outByteSec = 0f;
			inByteSec = 0f;
		}
	}

	public void GetAndResetStats(out int totalSent, out int totalRecv)
	{
		totalSent = m_totalSent;
		totalRecv = m_totalRecv;
		m_totalSent = 0;
		m_totalRecv = 0;
	}

	public bool GotNewData()
	{
		bool gotData = m_gotData;
		m_gotData = false;
		return gotData;
	}

	public int GetHostPort()
	{
		if (IsHost())
		{
			return 1;
		}
		return -1;
	}

	public static void SetDataPort(int port)
	{
		m_steamDataPort = port;
	}

	public void VersionMatch()
	{
	}
}
