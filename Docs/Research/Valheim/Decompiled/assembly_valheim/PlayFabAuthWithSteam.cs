using System;
using System.Collections.Generic;
using System.Text;
using PlayFab;
using PlayFab.ClientModels;
using Steamworks;

public static class PlayFabAuthWithSteam
{
	private static string m_steamTicket;

	public static void Login()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SteamNetworkingIdentity serverIdentity = default(SteamNetworkingIdentity);
		byte[] array = ZSteamMatchmaking.instance.RequestSessionTicket(ref serverIdentity);
		if (array == null)
		{
			PlayFabManager.instance.OnLoginFailure(null);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.AppendFormat("{0:x2}", array[i]);
		}
		m_steamTicket = stringBuilder.ToString();
		ZSteamMatchmaking.instance.AuthSessionTicketResponse += OnAuthSessionTicketResponse;
	}

	private static void OnAuthSessionTicketResponse()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		ZSteamMatchmaking.instance.AuthSessionTicketResponse -= OnAuthSessionTicketResponse;
		LoginWithSteamRequest val = new LoginWithSteamRequest
		{
			CreateAccount = true,
			SteamTicket = m_steamTicket
		};
		m_steamTicket = null;
		PlayFabClientAPI.LoginWithSteam(val, (Action<LoginResult>)OnSteamLoginSuccess, (Action<PlayFabError>)OnSteamLoginFailed, (object)null, (Dictionary<string, string>)null);
	}

	private static void OnSteamLoginSuccess(LoginResult result)
	{
		ZLog.Log((object)"Logged in PlayFab user via Steam auth session ticket");
		PlayFabManager.instance.OnLoginSuccess(result);
		ZSteamMatchmaking.instance.ReleaseSessionTicket();
	}

	private static void OnSteamLoginFailed(PlayFabError error)
	{
		ZLog.LogError((object)("Failed to logged in PlayFab user via Steam auth session ticket: " + error.GenerateErrorReport()));
		PlayFabManager.instance.OnLoginFailure(error);
		ZSteamMatchmaking.instance.ReleaseSessionTicket();
	}
}
