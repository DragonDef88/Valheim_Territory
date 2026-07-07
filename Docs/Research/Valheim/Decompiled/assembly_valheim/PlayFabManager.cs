using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Party;
using PlayFab.SharedModels;
using Splatform;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
	public const string TitleId = "6E223";

	private LoginState m_loginState;

	private string m_entityToken;

	private DateTime? m_tokenExpiration;

	private PlayFabAuthenticationContext m_authenticationContext;

	private float m_refreshThresh;

	private int m_loginAttempts;

	private bool m_shouldTryAutoLogin;

	private bool m_deletionRequestDoneOrTimedOut;

	private PlayFabMultiplayerManager m_playFabMultiplayerManager;

	private const float EntityTokenUpdateDurationMin = 420f;

	private const float EntityTokenUpdateDurationMax = 840f;

	private const float LoginRetryDelay = 1f;

	private const float LoginRetryDelayMax = 30f;

	private const float LoginRetryJitterFactor = 0.125f;

	public string PlayFabUniqueId = "";

	private static PlatformUserID m_customId;

	private GameObject m_multiplayerManager;

	private Coroutine m_updateEntityTokenCoroutine;

	public static bool IsLoggedIn
	{
		get
		{
			if ((Object)(object)instance == (Object)null)
			{
				return false;
			}
			return instance.m_loginState == LoginState.LoggedIn;
		}
	}

	public static LoginState CurrentLoginState
	{
		get
		{
			if ((Object)(object)instance == (Object)null)
			{
				return LoginState.NotLoggedIn;
			}
			return instance.m_loginState;
		}
	}

	public bool ShouldTryAutoLogin => instance.m_shouldTryAutoLogin;

	public static DateTime NextRetryUtc { get; private set; } = DateTime.MinValue;


	public EntityKey Entity { get; private set; }

	public static PlayFabManager instance { get; private set; }

	public event LoginFinishedCallback LoginFinished;

	public void SetShouldTryAutoLogin(bool value)
	{
		PlatformPrefs.SetInt("ShouldTryAutoLogin", value ? 1 : 0);
		if (PlatformPrefs.GetInt("ShouldTryAutoLogin", 0) == 1)
		{
			instance.m_shouldTryAutoLogin = true;
			instance.Login();
		}
		else
		{
			instance.m_shouldTryAutoLogin = false;
		}
	}

	public static void SetCustomId(PlatformUserID id)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		m_customId = id;
		ZLog.Log((object)$"PlayFab custom ID set to \"{m_customId}\"");
		if ((Object)(object)instance != (Object)null && CurrentLoginState == LoginState.NotLoggedIn)
		{
			instance.Login();
		}
	}

	public static void Initialize()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)instance == (Object)null)
		{
			Application.logMessageReceived += new LogCallback(HandleLog);
			new GameObject("PlayFabManager").AddComponent<PlayFabManager>();
		}
	}

	public void Awake()
	{
	}

	private void EnsureMultiplayerManagerCreated()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		if (!((Object)(object)m_multiplayerManager != (Object)null))
		{
			m_multiplayerManager = new GameObject("PlayFabMultiplayerManager");
			m_multiplayerManager.AddComponent<PlayFabMultiplayerManager>();
		}
	}

	private void EnsureMultiplayerManagerDestroyed()
	{
		if (!((Object)(object)m_multiplayerManager == (Object)null))
		{
			Object.Destroy((Object)(object)m_multiplayerManager);
			m_multiplayerManager = null;
		}
	}

	public void Start()
	{
		if ((Object)(object)instance != (Object)null)
		{
			ZLog.LogError((object)"Tried to create another PlayFabManager when one already exists! Ignoring and destroying the new one.");
			Object.Destroy((Object)(object)this);
			return;
		}
		m_shouldTryAutoLogin = PlatformPrefs.GetInt("ShouldTryAutoLogin", 1) == 1;
		instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		Login();
		((MonoBehaviour)this).Invoke("StopListeningToLogMsgs", 5f);
	}

	private void Login()
	{
		if (!m_shouldTryAutoLogin)
		{
			return;
		}
		if (m_loginState == LoginState.AttemptingLogin)
		{
			ZLog.LogError((object)("Can't log in while in the " + m_loginState.ToString() + " state!"));
			return;
		}
		m_loginAttempts++;
		ZLog.Log((object)$"Sending PlayFab login request (attempt {m_loginAttempts})");
		EnsureMultiplayerManagerCreated();
		if (m_loginState != LoginState.LoggedIn)
		{
			m_loginState = LoginState.AttemptingLogin;
		}
		PlayFabAuthWithSteam.Login();
	}

	public void OnLoginSuccess(LoginResult result)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		Entity = result.EntityToken.Entity;
		PlayFabUniqueId = result.PlayFabId;
		m_entityToken = result.EntityToken.EntityToken;
		m_tokenExpiration = result.EntityToken.TokenExpiration;
		m_authenticationContext = ((PlayFabLoginResultCommon)result).AuthenticationContext;
		if (!m_tokenExpiration.HasValue)
		{
			ZLog.LogError((object)"Token expiration time was null!");
			m_loginState = LoginState.LoggedIn;
			return;
		}
		m_refreshThresh = (float)(m_tokenExpiration.Value - DateTime.UtcNow).TotalSeconds / 2f;
		if (IsLoggedIn)
		{
			ZLog.Log((object)$"PlayFab local entity ID {Entity.Id} lifetime extended ");
			this.LoginFinished?.Invoke(LoginType.Refresh);
		}
		else
		{
			if (((PlatformUserID)(ref m_customId)).IsValid)
			{
				ZLog.Log((object)$"PlayFab logged in as \"{m_customId}\"");
			}
			ZLog.Log((object)("PlayFab local entity ID is " + Entity.Id));
			m_loginState = LoginState.LoggedIn;
			this.LoginFinished?.Invoke(LoginType.Success);
		}
		if (m_updateEntityTokenCoroutine == null)
		{
			m_updateEntityTokenCoroutine = ((MonoBehaviour)this).StartCoroutine(UpdateEntityTokenCoroutine());
		}
		ZPlayFabMatchmaking.OnLogin();
	}

	public void OnLoginFailure(PlayFabError error)
	{
		if (error == null)
		{
			ZLog.LogError((object)"Unknown login error");
		}
		else
		{
			ZLog.LogError((object)error.GenerateErrorReport());
		}
		this.LoginFinished?.Invoke(LoginType.Failed);
		RetryLoginAfterDelay(GetRetryDelay(m_loginAttempts));
	}

	private float GetRetryDelay(int attemptCount)
	{
		return Mathf.Min(1f * Mathf.Pow(2f, (float)(attemptCount - 1)), 30f) * Random.Range(0.875f, 1.125f);
	}

	private void RetryLoginAfterDelay(float delay)
	{
		m_loginState = LoginState.WaitingForRetry;
		ZLog.Log((object)$"Retrying login in {delay}s");
		((MonoBehaviour)this).StartCoroutine(DelayThenLoginCoroutine(delay));
		IEnumerator DelayThenLoginCoroutine(float delay)
		{
			ZLog.Log((object)$"PlayFab login failed! Retrying in {delay}s, total attempts: {m_loginAttempts}");
			NextRetryUtc = DateTime.UtcNow + TimeSpan.FromSeconds(delay);
			while (DateTime.UtcNow < NextRetryUtc)
			{
				yield return null;
			}
			Login();
		}
	}

	public static void CheckIfUserAuthenticated(string playfabID, PlatformUserID platformUserId, Action<bool> resultCallback)
	{
		resultCallback(obj: true);
	}

	private IEnumerator UpdateEntityTokenCoroutine()
	{
		while (true)
		{
			yield return (object)new WaitForSecondsRealtime(420f);
			ZLog.Log((object)"Update PlayFab entity token");
			PlayFabMultiplayerManager.Get().UpdateEntityToken(m_entityToken);
			if (!m_tokenExpiration.HasValue)
			{
				break;
			}
			if ((float)(m_tokenExpiration.Value - DateTime.UtcNow).TotalSeconds <= m_refreshThresh)
			{
				ZLog.Log((object)"Renew PlayFab entity token");
				m_refreshThresh /= 1.5f;
				Login();
			}
			yield return (object)new WaitForSecondsRealtime(Random.Range(420f, 840f));
		}
		ZLog.LogError((object)"Token expiration time was null!");
		m_updateEntityTokenCoroutine = null;
	}

	private static void HandleLog(string logString, string stackTrace, LogType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)type == 4 && logString.ToLower().Contains("DllNotFoundException: Party", StringComparison.InvariantCultureIgnoreCase))
		{
			ZLog.LogError((object)"DLL Not Found: This error usually occurs when you do not have the correct dependencies installed, and will prevent crossplay from working. The dependencies are different depending on which platform you play on.\n For windows: You need VC++ Redistributables. https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170Linux: You need Pulse Audio. https://learn.microsoft.com/it-it/gaming/playfab/features/multiplayer/networking/linux-specific-requirementsSteam deck: Try using Proton Compatability Layer.Other platforms: If the issue persists, please report it as a bug.");
			Object.FindObjectOfType<PlayFabManager>().WaitForPopupEnabled();
		}
	}

	private void StopListeningToLogMsgs()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		Application.logMessageReceived -= new LogCallback(HandleLog);
	}

	private void WaitForPopupEnabled()
	{
		if (UnifiedPopup.IsAvailable())
		{
			DelayedVCRedistWarningPopup();
		}
		else
		{
			UnifiedPopup.OnPopupEnabled += DelayedVCRedistWarningPopup;
		}
	}

	private void DelayedVCRedistWarningPopup()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		string playFabErrorBodyText = GetPlayFabErrorBodyText();
		UnifiedPopup.Push(new WarningPopup("$playfab_couldnotloadplayfabparty_header", playFabErrorBodyText, delegate
		{
			UnifiedPopup.Pop();
		}));
		UnifiedPopup.OnPopupEnabled -= DelayedVCRedistWarningPopup;
		Application.logMessageReceived -= new LogCallback(HandleLog);
	}

	private string GetPlayFabErrorBodyText()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		if (Settings.IsSteamRunningOnSteamDeck())
		{
			return "$playfab_couldnotloadplayfabparty_text_linux_steamdeck";
		}
		if (!Settings.IsSteamRunningOnSteamDeck() && ((int)Application.platform == 13 || (int)Application.platform == 44 || (int)Application.platform == 7))
		{
			return "$playfab_couldnotloadplayfabparty_text_linux";
		}
		if ((int)Application.platform == 2 || (int)Application.platform == 44 || (int)Application.platform == 7)
		{
			return "$playfab_couldnotloadplayfabparty_text_windows";
		}
		return "$playfab_couldnotloadplayfabparty_text_otherplatforms";
	}

	public void LoginFailed()
	{
		OnPlayFabRespondRemoveUIBlock(LoginType.Refresh);
		RetryLoginAfterDelay(GetRetryDelay(m_loginAttempts));
	}

	private void Update()
	{
		ZPlayFabMatchmaking.instance?.Update(Time.unscaledDeltaTime);
	}

	public void DeletePlayerTitleAccount()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		if (string.IsNullOrEmpty(PlayFabUniqueId))
		{
			ZLog.LogError((object)"No associated PlayFab ID found. Cannot delete account.");
			return;
		}
		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
		{
			FunctionName = "deletePlayerAccount",
			FunctionParameter = new { PlayFabUniqueId },
			GeneratePlayStreamEvent = true
		}, (Action<ExecuteCloudScriptResult>)OnCloudDeletePlayerResult, (Action<PlayFabError>)OnDeletePlayerFailed, (object)null, (Dictionary<string, string>)null);
		UnifiedPopup.Push(new TaskPopup("$settings_deleteplayfab_task_header", ""));
	}

	public void OnPlayFabRespondRemoveUIBlock(LoginType loginType = LoginType.Success)
	{
		if (UnifiedPopup.IsAvailable())
		{
			if (!UnifiedPopup.IsVisible())
			{
				return;
			}
			UnifiedPopup.Pop();
		}
		if (loginType == LoginType.Failed || loginType == LoginType.Refresh)
		{
			ZLog.LogWarning((object)"Could not log in to PlayFab. ");
			ResetMainMenuButtons();
			LoginFinished -= OnPlayFabRespondRemoveUIBlock;
			UnifiedPopup.Push(new WarningPopup("$menu_logging_in_playfab_failed_header", "", UnifiedPopup.Pop));
		}
	}

	public void ResetMainMenuButtons()
	{
		if ((Object)(object)FejdStartup.instance == (Object)null)
		{
			return;
		}
		TabHandler[] componentsInChildren = ((Component)((Component)FejdStartup.instance).transform).GetComponentsInChildren<TabHandler>(true);
		int num = 0;
		if (num < componentsInChildren.Length)
		{
			TabHandler tabHandler = componentsInChildren[num];
			if (tabHandler.m_tabs.Count == 2)
			{
				tabHandler.SetActiveTab(0);
			}
		}
		FejdStartup.instance.m_openServerToggle.isOn = false;
	}

	private void OnCloudDeletePlayerResult(ExecuteCloudScriptResult obj)
	{
		bool flag = false;
		string text = "";
		if (obj.FunctionResult != null)
		{
			string? text2 = obj.FunctionResult.ToString();
			flag = text2.Contains("\"success\":true,\"");
			text = text2;
		}
		else
		{
			Debug.LogError((object)"Result of PlayFab API is null or invalid.");
		}
		m_loginState = LoginState.NotLoggedIn;
		SetShouldTryAutoLogin(value: false);
		EnsureMultiplayerManagerDestroyed();
		OnPlayFabRespondRemoveUIBlock();
		ZLog.Log((object)("Delete Player Result: " + ((PlayFabBaseModel)obj).ToJson()));
		ResetMainMenuButtons();
		string text3 = (flag ? "$settings_deleteplayfabaccount_success" : ("$settings_deleteplayfabaccount_failure" + text));
		UnifiedPopup.Push(new WarningPopup("", text3, UnifiedPopup.Pop));
	}

	private void OnDeletePlayerFailed(PlayFabError error)
	{
		OnPlayFabRespondRemoveUIBlock();
		string text = error.GenerateErrorReport();
		ZLog.LogError((object)("Could not remove player account: " + text));
		UnifiedPopup.Push(new WarningPopup("", "$settings_deleteplayfabaccount_failure" + text, UnifiedPopup.Pop));
	}
}
