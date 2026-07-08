using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class GuildManagementUI : MonoBehaviour
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static PopupButtonCallback _003C0_003E__Pop;
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static PopupButtonCallback _003C_003E9__80_0;

		public static PopupButtonCallback _003C_003E9__80_1;

		internal void _003CButtonLeaveGuildClicked_003Eb__80_0()
		{
			API.DeleteGuild(API.GetOwnGuild());
			UnifiedPopup.Pop();
		}

		internal void _003CButtonLeaveGuildClicked_003Eb__80_1()
		{
			API.RemovePlayerFromGuild(PlayerReference.forOwnPlayer());
			UnifiedPopup.Pop();
		}
	}

	[Header("Placeholder Variables")]
	public GuildManagementUIRow rowPlaceHolderPrefab;

	public Transform rowPlaceHolderParentList;

	public List<GuildManagementUIRow> rowElements;

	[Header("Root UI")]
	public RectTransform rootTransform;

	public Image backgroundBack;

	public Image background;

	[Header("Header UI")]
	public RectTransform headerTransform;

	public HorizontalLayoutGroup headerHlg;

	public Image headerImageLeft;

	public TextMeshProUGUI headerTextTMP;

	public Image headerImageRight;

	[Header("Close Button UI")]
	public Button buttonClose;

	public Image buttonCloseImage;

	public TextMeshProUGUI buttonCloseText;

	[Header("Content UI")]
	public RectTransform contentTransform;

	public ScrollRect contentScrollRect;

	public Image contentScrollRectImage;

	public RectTransform contentList;

	public VerticalLayoutGroup contentListVlg;

	public ContentSizeFitter contentListSizeFitter;

	[Header("Row Placeholder UI")]
	public RectTransform rowTransform;

	public Image rowBackImage;

	public Image rowBorderImage;

	public HorizontalLayoutGroup contentHlg;

	[Header("Row Content UI - Name Area")]
	public RectTransform nameAreaRect;

	public VerticalLayoutGroup nameAreaVlg;

	public TextMeshProUGUI nameAreaNameTextTMP;

	[Header("Row Content UI - Rank Area")]
	public RectTransform rankArea;

	public VerticalLayoutGroup rankAreaVlg;

	public TMP_Dropdown rankAreaDropdown;

	public Image rankAreaDropdownImage;

	public TextMeshProUGUI rankAreaDropdownLabel;

	public Image rankAreaDropdownArrow;

	public RectTransform rankAreaDropdownTemplate;

	public ScrollRect rankAreaDropdownTemplateScrollRect;

	public Image rankAreaDropdownTemplateScrollRectImage;

	public RectTransform rankAreaDropdownTemplateViewport;

	public Image rankAreaDropdownTemplateViewportImage;

	public RectTransform rankAreaDropdownTemplateContent;

	public RectTransform rankAreaDropdownTemplateItem;

	public Image rankAreaDropdownTemplateItemBackgroundImage;

	public Image rankAreaDropdownTemplateItemCheckmarkImage;

	public TextMeshProUGUI rankAreaDropdownTemplateItemLabel;

	public Scrollbar rankAreaDropdownTemplateScrollbar;

	public Image rankAreaDropdownTemplateScrollbarImage;

	public RectTransform rankAreaDropdownTemplateSlidingAreaRect;

	public RectTransform rankAreaDropdownTemplateHandle;

	public Image rankAreaDropdownTemplateHandleImage;

	[Header("Row Content UI - Online Area")]
	public RectTransform onlineAreaRect;

	public VerticalLayoutGroup onlineAreaVlg;

	public TextMeshProUGUI onlineAreaOnlineStatusTextTMP;

	[Header("Row Content UI - Action Area")]
	public RectTransform actionArea;

	public Button actionAreaRemoveMemberButton;

	public TextMeshProUGUI actionAreaRemoveMemberButtonTextTMP;

	public Image actionAreaRemoveMemberButtonImage;

	[Header("Root UI Global Scrollbar")]
	public Scrollbar scrollbar;

	public RectTransform scrollbarRect;

	public RectTransform scrollbarSlidingArea;

	public RectTransform scrollbarHandle;

	public Image scrollbarHandleImage;

	[Header("Leave Guild Button")]
	public Button leaveGuildButton;

	public Image leaveGuildButtonImage;

	public TextMeshProUGUI leaveGuildButtonText;

	[Header("Achievements Button")]
	public Button achievementsButton;

	public Image achievementsButtonImage;

	public TextMeshProUGUI achievementsButtonText;

	[Header("Edit Guild Button")]
	public Button editGuildButton;

	public Image editGuildButtonImage;

	public TextMeshProUGUI editGuildButtonText;

	[Header("Applications Button")]
	public Button applicationsButton;

	public Image applicationsButtonImage;

	public TextMeshProUGUI applicationsButtonText;

	private List<GuildManagementUIRow> _activeRows = new List<GuildManagementUIRow>();

	private Queue<GuildManagementUIRow> _pooledRows = new Queue<GuildManagementUIRow>();

	public void OnEnable()
	{
		UpdateRows();
		((TMP_Text)headerTextTMP).text = API.GetOwnGuild().Name;
	}

	public void Awake()
	{
		((TMP_Text)headerTextTMP).text = "";
		((Component)rowPlaceHolderPrefab).gameObject.SetActive(false);
	}

	public void UpdateRows()
	{
		if (((Component)this).gameObject.activeSelf)
		{
			if (API.GetOwnGuild() == null)
			{
				Interface.SwitchUI(Interface.NoGuildUI);
				return;
			}
			PopulateRows(API.GetOwnGuild().Members);
			GameObject gameObject = ((Component)applicationsButton).gameObject;
			Ranks playerRank = API.GetPlayerRank(PlayerReference.forOwnPlayer());
			bool active = (uint)playerRank <= 2u;
			gameObject.SetActive(active);
			gameObject = ((Component)editGuildButton).gameObject;
			playerRank = API.GetPlayerRank(PlayerReference.forOwnPlayer());
			active = (uint)playerRank <= 1u;
			gameObject.SetActive(active);
		}
	}

	private void PopulateRows(Dictionary<PlayerReference, GuildMember> members)
	{
		foreach (GuildManagementUIRow activeRow in _activeRows)
		{
			((Component)activeRow).gameObject.SetActive(false);
			_pooledRows.Enqueue(activeRow);
		}
		_activeRows.Clear();
		foreach (KeyValuePair<PlayerReference, GuildMember> member in members)
		{
			GuildManagementUIRow row = GetRow();
			row.Setup(member);
			_activeRows.Add(row);
		}
	}

	private GuildManagementUIRow GetRow()
	{
		GuildManagementUIRow obj = ((_pooledRows.Count > 0) ? _pooledRows.Dequeue() : Object.Instantiate<GameObject>(((Component)rowTransform).gameObject, rowPlaceHolderParentList).GetComponent<GuildManagementUIRow>());
		((Component)obj).gameObject.SetActive(true);
		return obj;
	}

	private void ReturnRowToPool(GuildManagementUIRow row)
	{
		((Component)row).gameObject.SetActive(false);
		_pooledRows.Enqueue(row);
	}

	public void ButtonLeaveGuildClicked()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		if (API.GetPlayerRank(PlayerReference.forOwnPlayer()) == Ranks.Leader)
		{
			if (API.GetOwnGuild().Members.Count == 1)
			{
				object obj = _003C_003Ec._003C_003E9__80_0;
				if (obj == null)
				{
					PopupButtonCallback val = delegate
					{
						API.DeleteGuild(API.GetOwnGuild());
						UnifiedPopup.Pop();
					};
					_003C_003Ec._003C_003E9__80_0 = val;
					obj = (object)val;
				}
				object obj2 = _003C_003EO._003C0_003E__Pop;
				if (obj2 == null)
				{
					PopupButtonCallback val2 = UnifiedPopup.Pop;
					_003C_003EO._003C0_003E__Pop = val2;
					obj2 = (object)val2;
				}
				UnifiedPopup.Push((PopupBase)new YesNoPopup("$guilds_confirm_deletion", "$guilds_confirm_deletion_details", (PopupButtonCallback)obj, (PopupButtonCallback)obj2, true));
			}
			else
			{
				object obj3 = _003C_003EO._003C0_003E__Pop;
				if (obj3 == null)
				{
					PopupButtonCallback val3 = UnifiedPopup.Pop;
					_003C_003EO._003C0_003E__Pop = val3;
					obj3 = (object)val3;
				}
				UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_leader_left", "$guilds_leader_left_details", (PopupButtonCallback)obj3, true));
			}
			return;
		}
		object obj4 = _003C_003Ec._003C_003E9__80_1;
		if (obj4 == null)
		{
			PopupButtonCallback val4 = delegate
			{
				API.RemovePlayerFromGuild(PlayerReference.forOwnPlayer());
				UnifiedPopup.Pop();
			};
			_003C_003Ec._003C_003E9__80_1 = val4;
			obj4 = (object)val4;
		}
		object obj5 = _003C_003EO._003C0_003E__Pop;
		if (obj5 == null)
		{
			PopupButtonCallback val5 = UnifiedPopup.Pop;
			_003C_003EO._003C0_003E__Pop = val5;
			obj5 = (object)val5;
		}
		UnifiedPopup.Push((PopupBase)new YesNoPopup("$guilds_confirm_leave", "$guilds_confirm_leave_details", (PopupButtonCallback)obj4, (PopupButtonCallback)obj5, true));
	}

	public void ButtonEditGuildClicked()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		if (Guilds.allowGuildEdit.Value == Toggle.Off && !Guilds.configSync.IsAdmin)
		{
			object obj = _003C_003EO._003C0_003E__Pop;
			if (obj == null)
			{
				PopupButtonCallback val = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val;
				obj = (object)val;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_edit_disabled", "$guilds_edit_disabled_details", (PopupButtonCallback)obj, true));
		}
		else
		{
			Interface.SwitchUI(Interface.EditGuildUI);
		}
	}

	public void ButtonApplicationsClicked()
	{
		Interface.SwitchUI(Interface.ApplicationsUI);
	}

	public void ButtonCloseClicked()
	{
		Interface.HideUI();
	}

	public void ButtonAchievementsClicked()
	{
		Interface.SwitchUI(Interface.AchievementUI);
	}
}
