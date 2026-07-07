using System;
using PlayFab.Party;
using Splatform;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SuspendManager
{
	private static SuspendManager s_instance;

	public static void Initialize()
	{
		if (s_instance != null)
		{
			ZLog.LogError((object)"SuspendManager already initialized!");
		}
		else
		{
			s_instance = new SuspendManager();
		}
	}

	private SuspendManager()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		IPLMProvider pLMProvider = PlatformManager.DistributionPlatform.PLMProvider;
		if (pLMProvider == null)
		{
			ZLog.Log((object)"Platform doesn't implement Process Lifetime Management! Don't initialize suspend manager.");
			s_instance = null;
			return;
		}
		pLMProvider.EnteringSuspend += new EnteringSuspendHandler(OnEnteringSuspend);
		pLMProvider.LeavingSuspend += new LeavingSuspendHandler(OnLeavingSuspend);
		pLMProvider.ResumedFromSuspend += new ResumedFromSuspendHandler(OnResumedFromSuspend);
		pLMProvider.IsRunningInBackgroundChanged += new IsRunningInBackgroundChangedHandler(OnIsRunningInBackgroundChanged);
	}

	private void OnEnteringSuspend(DateTime deadlineUtc)
	{
		if ((Object)(object)Game.instance != (Object)null && !ZNet.IsSinglePlayer)
		{
			ZNetScene.instance.Shutdown();
			ZNet.instance.ShutdownWithoutSave(suspending: true);
		}
		PlayFabMultiplayerManager.Get().Suspend();
	}

	private void OnLeavingSuspend()
	{
		if (!((Object)(object)Game.instance == (Object)null))
		{
			bool num = (Object)(object)ZNet.instance != (Object)null && ZNet.instance.IsServer();
			bool flag = ZNet.IsOpenServer();
			if (num == flag)
			{
				SceneManager.sceneLoaded += OnMainMenuAfterResume;
				Game.instance.Logout();
			}
		}
	}

	private void OnResumedFromSuspend()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (((Enum)PlatformManager.DistributionPlatform.PLMProvider.SupportedSuspendEvents).HasFlag((Enum)(object)(SuspendEvents)2))
		{
			PlayFabMultiplayerManager.Get().Resume();
		}
	}

	private void OnMainMenuAfterResume(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnMainMenuAfterResume;
		if (UnifiedPopup.IsAvailable() && !UnifiedPopup.IsVisible())
		{
			string text = "$online_kickedfromsession_suspendresume_xbox_text";
			UnifiedPopup.Push(new WarningPopup("$online_kickedfromsession_header", text, delegate
			{
				UnifiedPopup.Pop();
			}));
		}
	}

	private void OnIsRunningInBackgroundChanged(bool isRunningInBackground)
	{
		if ((Object)(object)Minimap.instance != (Object)null)
		{
			Minimap.instance.PauseUpdateTemporarily();
		}
	}
}
