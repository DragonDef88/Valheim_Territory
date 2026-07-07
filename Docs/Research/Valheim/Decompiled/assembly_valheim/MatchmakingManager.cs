using Splatform;
using UnityEngine;

public class MatchmakingManager
{
	private static MatchmakingManager s_instance;

	private Invite? m_pendingInvite;

	public static void Initialize()
	{
		if (s_instance != null)
		{
			ZLog.LogError((object)"MatchmakingManager already initialized!");
		}
		else
		{
			s_instance = new MatchmakingManager();
		}
	}

	private MatchmakingManager()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		IMatchmakingProvider matchmakingProvider = PlatformManager.DistributionPlatform.MatchmakingProvider;
		if (matchmakingProvider == null)
		{
			ZLog.Log((object)"Platform doesn't implement matchmaking! Don't initialize matchmaking manager.");
			s_instance = null;
		}
		else
		{
			matchmakingProvider.AcceptMultiplayerSessionInvite += new AcceptMultiplayerSessionInviteHandler(OnAcceptMultiplayerSessionInvite);
		}
	}

	private void OnAcceptMultiplayerSessionInvite(Invite invite)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (m_pendingInvite.HasValue)
		{
			ZLog.Log((object)"Existing pending invite was reset");
		}
		m_pendingInvite = null;
		_ = (Object)(object)FejdStartup.instance != (Object)null;
		if ((Object)(object)Game.instance != (Object)null && !Game.instance.IsShuttingDown() && UnifiedPopup.IsAvailable() && (Object)(object)Menu.instance != (Object)null)
		{
			InviteType inviteType = invite.m_inviteType;
			string header;
			string text;
			if ((int)inviteType != 0)
			{
				if ((int)inviteType != 1)
				{
					ZLog.LogError((object)"This part of the code should be unreachable - can't join a game via the invite/join system without having been invited or joined!");
					return;
				}
				header = "$menu_joindifferentserver";
				text = "$menu_logoutprompt";
			}
			else
			{
				header = "$menu_acceptedinvite";
				text = "$menu_logoutprompt";
			}
			m_pendingInvite = invite;
			UnifiedPopup.Push(new YesNoPopup(header, text, delegate
			{
				UnifiedPopup.Pop();
				if ((Object)(object)Menu.instance != (Object)null)
				{
					Menu.instance.OnLogoutYes();
				}
			}, delegate
			{
				UnifiedPopup.Pop();
				m_pendingInvite = null;
			}));
		}
		else
		{
			m_pendingInvite = invite;
		}
	}

	public static bool TryConsumePendingInvite(out Invite invite)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (s_instance == null)
		{
			invite = default(Invite);
			return false;
		}
		if (!s_instance.m_pendingInvite.HasValue)
		{
			invite = default(Invite);
			return false;
		}
		invite = s_instance.m_pendingInvite.Value;
		s_instance.m_pendingInvite = null;
		return true;
	}
}
