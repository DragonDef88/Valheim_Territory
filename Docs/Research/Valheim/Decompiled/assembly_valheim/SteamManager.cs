using System;
using System.IO;
using System.Linq;
using System.Text;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	public static uint[] ACCEPTED_APPIDs = new uint[2] { 1223920u, 892970u };

	public static uint APP_ID = 0u;

	private static int m_serverPort = 2456;

	private static SteamManager s_instance;

	private static bool s_EverInialized;

	private bool m_bInitialized;

	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	public static SteamManager instance => s_instance;

	public static bool Initialized
	{
		get
		{
			if ((Object)(object)s_instance != (Object)null)
			{
				return s_instance.m_bInitialized;
			}
			return false;
		}
	}

	public static bool Initialize()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)s_instance == (Object)null)
		{
			new GameObject("SteamManager").AddComponent<SteamManager>();
		}
		return Initialized;
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning((object)pchDebugText);
	}

	public static void SetServerPort(int port)
	{
		m_serverPort = port;
	}

	private uint LoadAPPID()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SteamAppId");
		if (environmentVariable != null)
		{
			ZLog.Log((object)("Using environment steamid " + environmentVariable));
			return uint.Parse(environmentVariable);
		}
		try
		{
			string s = File.ReadAllText("steam_appid.txt");
			ZLog.Log((object)"Using steam_appid.txt");
			return uint.Parse(s);
		}
		catch
		{
		}
		ZLog.LogWarning((object)"Failed to find APPID");
		return 0u;
	}

	private void Awake()
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)s_instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		s_instance = this;
		APP_ID = LoadAPPID();
		ZLog.Log((object)("Using steam APPID:" + APP_ID));
		if (!ACCEPTED_APPIDs.Contains(APP_ID))
		{
			ZLog.Log((object)"Invalid APPID");
			Application.Quit();
			return;
		}
		if (s_EverInialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError((object)"[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", (Object)(object)this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError((object)"[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", (Object)(object)this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary((AppId_t)APP_ID))
			{
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			Debug.LogError((object)("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex), (Object)(object)this);
			Application.Quit();
			return;
		}
		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			Debug.LogError((object)"[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", (Object)(object)this);
			return;
		}
		ESteamNetworkingAvailability val = SteamNetworkingSockets.InitAuthentication();
		ZLog.Log((object)("Authentication:" + ((object)(ESteamNetworkingAvailability)(ref val)).ToString()));
		s_EverInialized = true;
	}

	private void OnEnable()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if ((Object)(object)s_instance == (Object)null)
		{
			s_instance = this;
		}
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	private void OnDestroy()
	{
		ZLog.Log((object)"Steam manager on destroy");
		if (!((Object)(object)s_instance != (Object)(object)this))
		{
			s_instance = null;
			if (m_bInitialized)
			{
				SteamAPI.Shutdown();
			}
		}
	}

	private void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}
}
