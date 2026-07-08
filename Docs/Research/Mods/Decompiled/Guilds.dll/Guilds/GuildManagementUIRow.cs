using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class GuildManagementUIRow : MonoBehaviour
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static PopupButtonCallback _003C0_003E__Pop;
	}

	public PlayerReference player;

	[Header("Row Root")]
	public RectTransform rowRootTransform;

	public GameObject rowRootGameObject;

	public RectTransform back;

	public Image backImage;

	public Image borderImage;

	[Header("Content Area")]
	public RectTransform contentAreaRectTransform;

	public HorizontalLayoutGroup contentAreaHorizontalLayoutGroup;

	[Header("Name Area")]
	public RectTransform nameAreaTransform;

	public VerticalLayoutGroup nameAreaVerticalLayoutGroup;

	public TextMeshProUGUI nameText;

	[Header("Rank Area")]
	public RectTransform rankAreaTransform;

	public VerticalLayoutGroup rankAreaVerticalLayoutGroup;

	public TMP_Dropdown rankDropdown;

	public Image rankDropdownImage;

	public TextMeshProUGUI rankDropdownLabel;

	public Image rankDropdownArrow;

	public RectTransform rankDropdownTemplate;

	public ScrollRect rankDropdownTemplateScrollRect;

	public Image rankDropdownTemplateScrollRectImage;

	public RectTransform rankDropdownTemplateViewport;

	public Image rankDropdownTemplateViewportImage;

	public RectTransform rankDropdownTemplateContent;

	public RectTransform rankDropdownTemplateItem;

	public Image rankDropdownTemplateItemBackgroundImage;

	public Image rankDropdownTemplateItemCheckmarkImage;

	public TextMeshProUGUI rankDropdownTemplateItemLabel;

	[Header("Rank Dropdown Scrollbar")]
	public RectTransform rankScrollbarTransform;

	public Scrollbar rankScrollbar;

	public Image rankScrollbarImage;

	public RectTransform slidingArea;

	public RectTransform handle;

	public Image handleImage;

	[Header("Online Area")]
	public RectTransform onlineAreaTransform;

	public VerticalLayoutGroup onlineAreaLayoutGroup;

	public RectTransform statusTextTransform;

	public TextMeshProUGUI statusText;

	[Header("Action Area")]
	public RectTransform actionAreaTransform;

	public VerticalLayoutGroup actionAreaLayoutGroup;

	public Button removeMemberButton;

	public Image actionAreaButtonImage;

	public TextMeshProUGUI actionAreaButtonText;

	public void Awake()
	{
		rankDropdown.options = ((IEnumerable<Ranks>)(Ranks[])typeof(Ranks).GetEnumValues()).Select((Func<Ranks, OptionData>)((Ranks rank) => new OptionData(Localization.instance.Localize("$guilds_rank_" + rank.ToString().ToLower())))).ToList();
	}

	public void Setup(KeyValuePair<PlayerReference, GuildMember> memberData)
	{
		((TMP_Text)nameText).text = memberData.Key.name;
		rankDropdown.SetValueWithoutNotify((int)memberData.Value.rank);
		((TMP_Text)statusText).text = (ZNet.instance.m_players.Any((PlayerInfo p) => PlayerReference.fromPlayerInfo(p) == memberData.Key) ? Localization.instance.Localize("$guilds_online") : Localization.instance.Localize("$guilds_last_online", new string[1] { Tools.GetHumanFriendlyTime((int)(DateTime.Now - memberData.Value.lastOnline).TotalSeconds) }));
		player = memberData.Key;
		TMP_Dropdown val = rankDropdown;
		Ranks playerRank = API.GetPlayerRank(PlayerReference.forOwnPlayer());
		bool interactable = (uint)playerRank <= 1u;
		((Selectable)val).interactable = interactable;
		GameObject gameObject = ((Component)removeMemberButton).gameObject;
		playerRank = API.GetPlayerRank(PlayerReference.forOwnPlayer());
		interactable = (uint)playerRank <= 2u;
		gameObject.SetActive(interactable);
	}

	public void OnRemoveMember_ButtonClicked()
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_00fe: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		if (player == PlayerReference.forOwnPlayer())
		{
			object obj = _003C_003EO._003C0_003E__Pop;
			if (obj == null)
			{
				PopupButtonCallback val = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val;
				obj = (object)val;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_kicked_self", "$guilds_kicked_self_details", (PopupButtonCallback)obj, true));
			return;
		}
		if (API.GetPlayerRank(player) <= API.GetPlayerRank(PlayerReference.forOwnPlayer()))
		{
			string text = Localization.instance.Localize("$guilds_higher_rank_kicked_details", new string[1] { player.name });
			object obj2 = _003C_003EO._003C0_003E__Pop;
			if (obj2 == null)
			{
				PopupButtonCallback val2 = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val2;
				obj2 = (object)val2;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_higher_rank_kicked", text, (PopupButtonCallback)obj2, true));
			return;
		}
		string text2 = Localization.instance.Localize("$guilds_confirm_kick_details", new string[1] { player.name });
		PopupButtonCallback val3 = delegate
		{
			API.RemovePlayerFromGuild(player);
			Guilds.SendMessageToPlayer(player, Localization.instance.Localize("$guilds_kicked_out", new string[1] { API.GetOwnGuild().Name }));
			UnifiedPopup.Pop();
		};
		object obj3 = _003C_003EO._003C0_003E__Pop;
		if (obj3 == null)
		{
			PopupButtonCallback val4 = UnifiedPopup.Pop;
			_003C_003EO._003C0_003E__Pop = val4;
			obj3 = (object)val4;
		}
		UnifiedPopup.Push((PopupBase)new YesNoPopup("$guilds_confirm_kick", text2, val3, (PopupButtonCallback)obj3, true));
	}

	public void OnRankDropdown_ValueChanged(int value)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Expected O, but got Unknown
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Expected O, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Expected O, but got Unknown
		value = rankDropdown.value;
		if (value == (int)API.GetPlayerRank(player))
		{
			return;
		}
		if (player == PlayerReference.forOwnPlayer())
		{
			object obj = _003C_003EO._003C0_003E__Pop;
			if (obj == null)
			{
				PopupButtonCallback val = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val;
				obj = (object)val;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_own_rank_changed", "$guilds_own_rank_changed_details", (PopupButtonCallback)obj, true));
			rankDropdown.value = (int)API.GetPlayerRank(player);
			return;
		}
		if (API.GetPlayerRank(PlayerReference.forOwnPlayer()) == Ranks.Leader)
		{
			if (value == 0)
			{
				rankDropdown.value = (int)API.GetPlayerRank(player);
				string text = Localization.instance.Localize("$guilds_transfer_guild_details", new string[1] { player.name });
				PopupButtonCallback val2 = delegate
				{
					API.UpdatePlayerRank(PlayerReference.forOwnPlayer(), Ranks.Coleader);
					API.UpdatePlayerRank(player, (Ranks)value);
					UnifiedPopup.Pop();
				};
				object obj2 = _003C_003EO._003C0_003E__Pop;
				if (obj2 == null)
				{
					PopupButtonCallback val3 = UnifiedPopup.Pop;
					_003C_003EO._003C0_003E__Pop = val3;
					obj2 = (object)val3;
				}
				UnifiedPopup.Push((PopupBase)new YesNoPopup("$guilds_transfer_guild", text, val2, (PopupButtonCallback)obj2, true));
			}
			else
			{
				API.UpdatePlayerRank(player, (Ranks)value);
			}
			return;
		}
		Ranks playerRank = API.GetPlayerRank(player);
		if ((uint)playerRank <= 1u)
		{
			object obj3 = _003C_003EO._003C0_003E__Pop;
			if (obj3 == null)
			{
				PopupButtonCallback val4 = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val4;
				obj3 = (object)val4;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_higher_rank_changed", "$guilds_higher_rank_changed_details", (PopupButtonCallback)obj3, true));
			rankDropdown.value = (int)API.GetPlayerRank(player);
			return;
		}
		playerRank = (Ranks)value;
		if ((uint)playerRank <= 1u)
		{
			object obj4 = _003C_003EO._003C0_003E__Pop;
			if (obj4 == null)
			{
				PopupButtonCallback val5 = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val5;
				obj4 = (object)val5;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_new_rank_too_high", "$guilds_new_rank_too_high_details", (PopupButtonCallback)obj4, true));
			rankDropdown.value = (int)API.GetPlayerRank(player);
		}
		else
		{
			API.UpdatePlayerRank(player, (Ranks)value);
		}
	}
}
