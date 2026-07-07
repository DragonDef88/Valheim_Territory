using System;
using Splatform;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UserManagement;

namespace Valheim.UI;

public class SessionPlayerListEntry : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private Selectable _focusPoint;

	[SerializeField]
	private Image _selection;

	[SerializeField]
	private GameObject _viewPlayerCard;

	[SerializeField]
	private Image _outline;

	[Header("Player")]
	[SerializeField]
	private Image _hostIcon;

	[SerializeField]
	private Image _gamerpic;

	[SerializeField]
	private Sprite otherPlatformPlayerPic;

	[SerializeField]
	private TextMeshProUGUI _gamertagText;

	[SerializeField]
	private TextMeshProUGUI _characterNameText;

	[Header("Block")]
	[SerializeField]
	private Button _blockButton;

	[SerializeField]
	private Image _blockButtonImage;

	[SerializeField]
	private Sprite _blockSprite;

	[SerializeField]
	private Sprite _unblockSprite;

	[Header("Kick")]
	[SerializeField]
	private Button _kickButton;

	[SerializeField]
	private Image _kickButtonImage;

	private PlatformUserID _user;

	private IUserProfile _userProfile;

	private string _gamertag;

	private string _characterName;

	public bool IsSelected => ((Behaviour)_selection).enabled;

	public Selectable FocusObject => _focusPoint;

	public Selectable BlockButton => (Selectable)(object)_blockButton;

	public Selectable KickButton => (Selectable)(object)_kickButton;

	public PlatformUserID User => _user;

	public bool HasFocusObject => ((Component)_focusPoint).gameObject.activeSelf;

	public bool HasBlock => ((Component)_blockButtonImage).gameObject.activeSelf;

	public bool HasKick => ((Component)_kickButtonImage).gameObject.activeSelf;

	public bool HasActivatedButtons
	{
		get
		{
			if (!((Component)_blockButtonImage).gameObject.activeSelf)
			{
				return ((Component)_kickButtonImage).gameObject.activeSelf;
			}
			return true;
		}
	}

	public bool IsSamePlatform => _user.m_platform == PlatformManager.DistributionPlatform.Platform;

	public bool IsOwnPlayer
	{
		get
		{
			return ((Component)_outline).gameObject.activeSelf;
		}
		set
		{
			((Component)_outline).gameObject.SetActive(value);
		}
	}

	public bool IsHost
	{
		get
		{
			return ((Component)_hostIcon).gameObject.activeSelf;
		}
		set
		{
			((Component)_hostIcon).gameObject.SetActive(value);
		}
	}

	private bool CanBeKicked
	{
		get
		{
			return ((Component)_kickButtonImage).gameObject.activeSelf;
		}
		set
		{
			((Component)_kickButtonImage).gameObject.SetActive(value && !IsHost);
		}
	}

	private bool CanBeBlocked
	{
		get
		{
			return ((Component)_blockButtonImage).gameObject.activeSelf;
		}
		set
		{
			((Component)_blockButtonImage).gameObject.SetActive(value);
		}
	}

	private bool CanBeMuted
	{
		get
		{
			return false;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string Gamertag
	{
		get
		{
			return _gamertag;
		}
		set
		{
			string text = "";
			if (value != null)
			{
				text += value;
			}
			_gamertag = text;
			if (IsHost)
			{
				text += " (Host)";
			}
			((TMP_Text)_gamertagText).text = text;
		}
	}

	public string CharacterName
	{
		get
		{
			return _characterName;
		}
		set
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			string text = value;
			if (!IsOwnPlayer)
			{
				text = CensorShittyWords.FilterUGC(text, UGCType.CharacterName, _user, 0L);
			}
			_characterName = text;
			((TMP_Text)_characterNameText).text = text;
		}
	}

	public event Action<SessionPlayerListEntry> OnKicked;

	private void Awake()
	{
		((Behaviour)_selection).enabled = false;
		_viewPlayerCard.SetActive(false);
		if ((Object)(object)_button != (Object)null)
		{
			((Behaviour)_button).enabled = true;
		}
	}

	private void Update()
	{
		if ((Object)(object)EventSystem.current != (Object)null && ((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)_focusPoint).gameObject || (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)_blockButton).gameObject || (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)_kickButton).gameObject || (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)_button).gameObject))
		{
			SelectEntry();
		}
		else
		{
			Deselect();
		}
		UpdateFocusPoint();
	}

	public void SelectEntry()
	{
		((Behaviour)_selection).enabled = true;
		_viewPlayerCard.SetActive(IsSamePlatform);
	}

	public void Deselect()
	{
		((Behaviour)_selection).enabled = false;
		_viewPlayerCard.SetActive(false);
	}

	public void OnBlock()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (RelationsManager.IsBlocked(_user))
		{
			OnViewCard();
			return;
		}
		if (MuteList.Contains(_user))
		{
			MuteList.Unblock(_user);
		}
		else
		{
			MuteList.Block(_user);
		}
		UpdateBlockButton();
	}

	private void UpdateButtons()
	{
		UpdateBlockButton();
		UpdateFocusPoint();
	}

	private void UpdateFocusPoint()
	{
		((Component)_focusPoint).gameObject.SetActive(!HasActivatedButtons);
	}

	public void UpdateBlockButton()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_blockButtonImage.sprite = ((MuteList.Contains(_user) || RelationsManager.IsBlocked(_user)) ? _unblockSprite : _blockSprite);
	}

	public void OnKick()
	{
		if ((Object)(object)ZNet.instance != (Object)null)
		{
			UnifiedPopup.Push(new YesNoPopup("$menu_kick_player_title", Localization.instance.Localize("$menu_kick_player", new string[1] { CharacterName }), delegate
			{
				ZNet.instance.Kick(CharacterName);
				this.OnKicked?.Invoke(this);
				UnifiedPopup.Pop();
			}, delegate
			{
				UnifiedPopup.Pop();
			}));
		}
	}

	public void SetValues(string characterName, PlatformUserID user, bool isHost, bool canBeBlocked, bool canBeKicked)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_006d: Expected O, but got Unknown
		_user = user;
		IsHost = isHost;
		CharacterName = characterName;
		Gamertag = "";
		_gamerpic.sprite = otherPlatformPlayerPic;
		if (IsSamePlatform && PlatformManager.DistributionPlatform.RelationsProvider != null)
		{
			PlatformManager.DistributionPlatform.RelationsProvider.GetUserProfileAsync(user, new GetUserProfileCompletedHandler(GetUserProfileCompleted), new GetUserProfileFailedHandler(GetUserProfileFailed));
		}
		else
		{
			UpdateProfile();
		}
		CanBeKicked = !isHost && canBeKicked;
		CanBeBlocked = canBeBlocked;
		UpdateButtons();
	}

	private void GetUserProfileCompleted(IUserProfile profile)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		if (!((Object)(object)this == (Object)null))
		{
			_ = DateTime.UtcNow;
			_userProfile = profile;
			_userProfile.RequestProfilePictureAsync(GetProfilePictureResolution());
			UpdateProfile();
			_userProfile.ProfileDataUpdated += new ProfileDataUpdatedHandler(UpdateProfile);
			UpdateProfilePicture();
			_userProfile.ProfilePictureUpdated += new ProfilePictureUpdatedHandler(UpdateProfilePicture);
		}
	}

	private static uint GetProfilePictureResolution()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (PlatformManager.DistributionPlatform.HardwareInfoProvider == null)
		{
			return 128u;
		}
		HardwareInfo hardwareInfo = PlatformManager.DistributionPlatform.HardwareInfoProvider.HardwareInfo;
		if ((int)hardwareInfo.m_category == 0)
		{
			return 128u;
		}
		if ((int)hardwareInfo.m_category < 2)
		{
			return 50u;
		}
		if ((int)hardwareInfo.m_category == 2 && hardwareInfo.m_generation <= 8)
		{
			return 50u;
		}
		return 128u;
	}

	private void GetUserProfileFailed(PlatformUserID userId, GetUserProfileFailReason failReason)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		switch ((int)failReason)
		{
		case 1:
		case 2:
			return;
		}
		Debug.LogError((object)$"Failed to get user profile for user {userId}: {failReason}");
	}

	private void UpdateProfile()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		string displayName;
		if (IsSamePlatform)
		{
			Gamertag = ((IUser)_userProfile).DisplayName;
		}
		else if (ZNet.TryGetServerAssignedDisplayName(_user, out displayName))
		{
			Gamertag = displayName;
		}
		UpdateButtons();
	}

	private void UpdateProfilePicture()
	{
		if (IsSamePlatform && (Object)(object)_userProfile.ProfilePicture != (Object)null)
		{
			_gamerpic.SetSpriteFromTexture(_userProfile.ProfilePicture);
		}
		else
		{
			_gamerpic.sprite = otherPlatformPlayerPic;
		}
	}

	public void OnViewCard()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (PlatformManager.DistributionPlatform.UIProvider.ShowUserProfile != null && IsSamePlatform)
		{
			PlatformManager.DistributionPlatform.UIProvider.ShowUserProfile.Open(_user);
		}
	}

	public void RemoveCallbacks()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		if (_userProfile != null)
		{
			_userProfile.ProfileDataUpdated -= new ProfileDataUpdatedHandler(UpdateProfile);
			_userProfile.ProfilePictureUpdated -= new ProfilePictureUpdatedHandler(UpdateProfilePicture);
		}
	}
}
