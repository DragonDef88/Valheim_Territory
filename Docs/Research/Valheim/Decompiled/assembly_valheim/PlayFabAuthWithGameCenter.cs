using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Splatform;
using UnityEngine;

public static class PlayFabAuthWithGameCenter
{
	public static void Login()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		if (!((Object)(object)PlayFabManager.instance == (Object)null))
		{
			PlayFabClientAPI.LoginWithGameCenter(new LoginWithGameCenterRequest
			{
				TitleId = "6E223",
				CreateAccount = true,
				PlayerId = ((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID.m_userID
			}, (Action<LoginResult>)OnLoginSuccess, (Action<PlayFabError>)OnLoginFailure, (object)null, (Dictionary<string, string>)null);
		}
	}

	private static void OnLoginSuccess(LoginResult result)
	{
		ZLog.Log((object)("PlayFab logged in via Game Center with ID " + result.PlayFabId));
		PlayFabManager.instance.OnLoginSuccess(result);
	}

	private static void OnLoginFailure(PlayFabError error)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ZLog.LogWarning((object)$"PlayFab failed to login via Game Center with error code {error.Error}");
		PlayFabManager.instance.OnLoginFailure(error);
	}
}
