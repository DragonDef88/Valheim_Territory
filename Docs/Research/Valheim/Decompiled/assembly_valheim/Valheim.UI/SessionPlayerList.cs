using System.Collections.Generic;
using Splatform;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using UserManagement;

namespace Valheim.UI;

public class SessionPlayerList : MonoBehaviour
{
	[SerializeField]
	protected SessionPlayerListEntry _ownPlayer;

	[SerializeField]
	protected ScrollRect _scrollRect;

	[SerializeField]
	protected Button _backButton;

	private List<ZNet.PlayerInfo> _players;

	private readonly List<SessionPlayerListEntry> _remotePlayers = new List<SessionPlayerListEntry>();

	private readonly List<SessionPlayerListEntry> _allPlayers = new List<SessionPlayerListEntry>();

	protected void Awake()
	{
		MuteList.Load(Init);
	}

	private void Init()
	{
		SetEntries();
		foreach (SessionPlayerListEntry allPlayer in _allPlayers)
		{
			allPlayer.OnKicked += OnPlayerWasKicked;
		}
		_ownPlayer.FocusObject.Select();
		UpdateBlockButtons();
	}

	private void UpdateBlockButtons()
	{
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		foreach (SessionPlayerListEntry allPlayer in _allPlayers)
		{
			allPlayer.UpdateBlockButton();
		}
	}

	private void OnPlayerWasKicked(SessionPlayerListEntry player)
	{
		player.OnKicked -= OnPlayerWasKicked;
		_allPlayers.Remove(player);
		_remotePlayers.Remove(player);
		Object.Destroy((Object)(object)((Component)player).gameObject);
		UpdateNavigation();
	}

	private void SetEntries()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		_allPlayers.Add(_ownPlayer);
		PlatformUserID platformUserID = ((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID;
		_players = ZNet.instance.GetPlayerList();
		ZNetPeer serverPeer = ZNet.instance.GetServerPeer();
		if (!ZNet.instance.IsServer() && _players.TryFindPlayerByPlayername(serverPeer.m_playerName, out var playerInfo))
		{
			if (ZNet.m_onlineBackend == OnlineBackendType.Steamworks)
			{
				PlatformUserID user = default(PlatformUserID);
				((PlatformUserID)(ref user))._002Ector(PlatformManager.DistributionPlatform.Platform, serverPeer.m_socket.GetEndPointString());
				CreatePlayerEntry(user, serverPeer.m_playerName, isHost: true);
			}
			else
			{
				CreatePlayerEntry(playerInfo.Value.m_userInfo.m_id, playerInfo.Value.m_name, isHost: true);
			}
		}
		PlatformUserID user2 = default(PlatformUserID);
		for (int i = 0; i < _players.Count; i++)
		{
			ZNet.PlayerInfo playerInfo2 = _players[i];
			if (playerInfo2.m_userInfo.m_id == platformUserID)
			{
				((PlatformUserID)(ref user2))._002Ector(PlatformManager.DistributionPlatform.Platform, (ulong)SteamUser.GetSteamID(), true);
				SetOwnPlayer(user2, ZNet.instance.IsServer());
			}
			else if (serverPeer == null || playerInfo2.m_name != serverPeer.m_playerName)
			{
				CreatePlayerEntry(playerInfo2.m_userInfo.m_id, playerInfo2.m_name);
			}
		}
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		Navigation val = default(Navigation);
		((Navigation)(ref val)).mode = (Mode)4;
		Navigation navigation = val;
		int count = _allPlayers.Count;
		for (int i = 0; i < count; i++)
		{
			SessionPlayerListEntry sessionPlayerListEntry = _allPlayers[i];
			SessionPlayerListEntry sessionPlayerListEntry2 = ((i < count - 1) ? _allPlayers[i + 1] : null);
			Navigation navigation2 = sessionPlayerListEntry.BlockButton.navigation;
			((Navigation)(ref navigation2)).mode = (Mode)(sessionPlayerListEntry.HasBlock ? 4 : 0);
			Navigation navigation3 = sessionPlayerListEntry.KickButton.navigation;
			((Navigation)(ref navigation3)).mode = (Mode)(sessionPlayerListEntry.HasKick ? 4 : 0);
			Navigation navigation4 = sessionPlayerListEntry.FocusObject.navigation;
			((Navigation)(ref navigation4)).mode = (Mode)(sessionPlayerListEntry.HasFocusObject ? 4 : 0);
			if ((Object)(object)sessionPlayerListEntry2 != (Object)null)
			{
				if (!sessionPlayerListEntry.HasActivatedButtons && !sessionPlayerListEntry2.HasActivatedButtons)
				{
					((Navigation)(ref navigation4)).selectOnDown = sessionPlayerListEntry2.FocusObject;
				}
				else if (!sessionPlayerListEntry.HasActivatedButtons && sessionPlayerListEntry2.HasActivatedButtons)
				{
					if (sessionPlayerListEntry2.HasBlock)
					{
						((Navigation)(ref navigation4)).selectOnDown = sessionPlayerListEntry2.BlockButton;
					}
					else if (sessionPlayerListEntry2.HasKick)
					{
						((Navigation)(ref navigation4)).selectOnDown = sessionPlayerListEntry2.KickButton;
					}
				}
				else if (sessionPlayerListEntry.HasActivatedButtons && !sessionPlayerListEntry2.HasActivatedButtons)
				{
					if (sessionPlayerListEntry.HasBlock)
					{
						((Navigation)(ref navigation2)).selectOnDown = sessionPlayerListEntry2.FocusObject;
					}
					if (sessionPlayerListEntry.HasKick)
					{
						((Navigation)(ref navigation3)).selectOnDown = sessionPlayerListEntry2.FocusObject;
					}
				}
				else
				{
					if (sessionPlayerListEntry.HasBlock)
					{
						if (sessionPlayerListEntry2.HasBlock)
						{
							((Navigation)(ref navigation2)).selectOnDown = sessionPlayerListEntry2.BlockButton;
						}
						else if (sessionPlayerListEntry2.HasKick)
						{
							((Navigation)(ref navigation2)).selectOnDown = sessionPlayerListEntry2.KickButton;
						}
					}
					if (sessionPlayerListEntry.HasKick)
					{
						if (sessionPlayerListEntry2.HasKick)
						{
							((Navigation)(ref navigation3)).selectOnDown = sessionPlayerListEntry2.KickButton;
						}
						else if (sessionPlayerListEntry2.HasBlock)
						{
							((Navigation)(ref navigation3)).selectOnDown = sessionPlayerListEntry2.BlockButton;
						}
					}
				}
			}
			else if (sessionPlayerListEntry.HasActivatedButtons)
			{
				if (sessionPlayerListEntry.HasBlock)
				{
					((Navigation)(ref navigation)).selectOnUp = sessionPlayerListEntry.BlockButton;
				}
				else if (sessionPlayerListEntry.HasKick)
				{
					((Navigation)(ref navigation)).selectOnUp = sessionPlayerListEntry.KickButton;
				}
				if (sessionPlayerListEntry.HasBlock)
				{
					((Navigation)(ref navigation2)).selectOnDown = (Selectable)(object)_backButton;
				}
				if (sessionPlayerListEntry.HasKick)
				{
					((Navigation)(ref navigation3)).selectOnDown = (Selectable)(object)_backButton;
				}
			}
			else
			{
				((Navigation)(ref navigation4)).selectOnDown = (Selectable)(object)_backButton;
				((Navigation)(ref navigation)).selectOnUp = sessionPlayerListEntry.FocusObject;
			}
			sessionPlayerListEntry.BlockButton.navigation = navigation2;
			sessionPlayerListEntry.KickButton.navigation = navigation3;
			sessionPlayerListEntry.FocusObject.navigation = navigation4;
		}
		for (int num = count - 1; num >= 0; num--)
		{
			SessionPlayerListEntry sessionPlayerListEntry3 = _allPlayers[num];
			SessionPlayerListEntry sessionPlayerListEntry4 = ((num > 0) ? _allPlayers[num - 1] : null);
			Navigation navigation5 = sessionPlayerListEntry3.BlockButton.navigation;
			Navigation navigation6 = sessionPlayerListEntry3.KickButton.navigation;
			Navigation navigation7 = sessionPlayerListEntry3.FocusObject.navigation;
			if ((Object)(object)sessionPlayerListEntry4 != (Object)null)
			{
				if (!sessionPlayerListEntry3.HasActivatedButtons && !sessionPlayerListEntry4.HasActivatedButtons)
				{
					((Navigation)(ref navigation7)).selectOnUp = sessionPlayerListEntry4.FocusObject;
				}
				else if (!sessionPlayerListEntry3.HasActivatedButtons && sessionPlayerListEntry4.HasActivatedButtons)
				{
					if (sessionPlayerListEntry4.HasBlock)
					{
						((Navigation)(ref navigation7)).selectOnUp = sessionPlayerListEntry4.BlockButton;
					}
					else if (sessionPlayerListEntry4.HasKick)
					{
						((Navigation)(ref navigation7)).selectOnUp = sessionPlayerListEntry4.KickButton;
					}
				}
				else if (sessionPlayerListEntry3.HasActivatedButtons && !sessionPlayerListEntry4.HasActivatedButtons)
				{
					if (sessionPlayerListEntry3.HasBlock)
					{
						((Navigation)(ref navigation5)).selectOnUp = sessionPlayerListEntry4.FocusObject;
					}
					if (sessionPlayerListEntry3.HasKick)
					{
						((Navigation)(ref navigation6)).selectOnUp = sessionPlayerListEntry4.FocusObject;
					}
				}
				else
				{
					if (sessionPlayerListEntry3.HasBlock)
					{
						if (sessionPlayerListEntry4.HasBlock)
						{
							((Navigation)(ref navigation5)).selectOnUp = sessionPlayerListEntry4.BlockButton;
						}
						else if (sessionPlayerListEntry4.HasKick)
						{
							((Navigation)(ref navigation5)).selectOnUp = sessionPlayerListEntry4.KickButton;
						}
					}
					if (sessionPlayerListEntry3.HasKick)
					{
						if (sessionPlayerListEntry4.HasKick)
						{
							((Navigation)(ref navigation6)).selectOnUp = sessionPlayerListEntry4.KickButton;
						}
						else if (sessionPlayerListEntry4.HasBlock)
						{
							((Navigation)(ref navigation6)).selectOnUp = sessionPlayerListEntry4.BlockButton;
						}
					}
				}
			}
			sessionPlayerListEntry3.BlockButton.navigation = navigation5;
			sessionPlayerListEntry3.KickButton.navigation = navigation6;
			sessionPlayerListEntry3.FocusObject.navigation = navigation7;
		}
		((Selectable)_backButton).navigation = navigation;
	}

	private void SetOwnPlayer(PlatformUserID user, bool isHost)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		UserInfo localUser = UserInfo.GetLocalUser();
		_ownPlayer.IsOwnPlayer = true;
		_ownPlayer.SetValues(localUser.Name, user, isHost, canBeBlocked: false, canBeKicked: false);
	}

	private void CreatePlayerEntry(PlatformUserID user, string name, bool isHost = false)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		SessionPlayerListEntry sessionPlayerListEntry = Object.Instantiate<SessionPlayerListEntry>(_ownPlayer, (Transform)(object)_scrollRect.content);
		sessionPlayerListEntry.IsOwnPlayer = false;
		sessionPlayerListEntry.SetValues(name, user, isHost, canBeBlocked: true, !isHost && ZNet.instance.LocalPlayerIsAdminOrHost());
		if (!isHost)
		{
			_remotePlayers.Add(sessionPlayerListEntry);
		}
		_allPlayers.Add(sessionPlayerListEntry);
	}

	public void OnBack()
	{
		foreach (SessionPlayerListEntry allPlayer in _allPlayers)
		{
			allPlayer.RemoveCallbacks();
		}
		MuteList.Persist();
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private void Update()
	{
		UpdateScrollPosition();
		if (ZInput.GetKeyDown((KeyCode)27, true))
		{
			OnBack();
		}
	}

	private void UpdateScrollPosition()
	{
		if (!((Component)_scrollRect.verticalScrollbar).gameObject.activeSelf)
		{
			return;
		}
		foreach (SessionPlayerListEntry allPlayer in _allPlayers)
		{
			if (allPlayer.IsSelected && !_scrollRect.IsVisible((RectTransform)/*isinst with value type is only supported in some contexts*/))
			{
				ScrollRect scrollRect = _scrollRect;
				Transform transform = ((Component)allPlayer).transform;
				scrollRect.SnapToChild((RectTransform)(object)((transform is RectTransform) ? transform : null));
				break;
			}
		}
	}
}
