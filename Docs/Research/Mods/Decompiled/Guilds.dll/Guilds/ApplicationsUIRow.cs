using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class ApplicationsUIRow : MonoBehaviour
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static PopupButtonCallback _003C0_003E__Pop;
	}

	private PlayerReference applicant;

	private Application application;

	private int guildId = -1;

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

	[Header("WhyMe Area")]
	public RectTransform whyMeArea;

	public VerticalLayoutGroup whyMeAreaVlg;

	public Button whyMeButton;

	public Image whyMeButtonImg;

	public TextMeshProUGUI whyMeAreaText;

	[Header("Applied Area")]
	public RectTransform appliedAreaTransform;

	public VerticalLayoutGroup appliedAreaLayoutGroup;

	public RectTransform statusTextTransform;

	public TextMeshProUGUI statusText;

	[Header("Action Area")]
	public RectTransform actionAreaTransform;

	public HorizontalLayoutGroup actionAreaLayoutGroup;

	public Button actionAreaAcceptMemberButton;

	public Image actionAreaAcceptMemberButtonImage;

	public Image actionAreaAcceptMemberButtonCheckmark;

	public Button actionAreaDenyMemberButton;

	public Image actionAreaDenyMemberButtonImage;

	public TextMeshProUGUI actionAreaDenyMemberButtonTextTMP;

	public ApplicationsUI applicationsUI;

	public void Setup(ApplicationsUI applicationsUI, PlayerReference applicant, Application application)
	{
		this.applicationsUI = applicationsUI;
		this.applicant = applicant;
		this.application = application;
		((TMP_Text)nameText).text = applicant.name;
		((TMP_Text)whyMeAreaText).text = application.description;
		((TMP_Text)statusText).text = Localization.instance.Localize("$guilds_apply_applied", new string[1] { Tools.GetHumanFriendlyTime((int)(DateTime.Now - application.applied).TotalSeconds) });
		guildId = API.GetOwnGuild().General.id;
	}

	public void OnDenyMember_ButtonClicked()
	{
		Guild guild = API.GetGuild(guildId);
		if (guild != null)
		{
			API.RemovePlayerApplication(applicant, guild);
			Guilds.SendMessageToPlayer(applicant, Localization.instance.Localize("$guilds_application_declined", new string[1] { guild.Name }));
		}
	}

	public void OnAcceptMember_ButtonClicked()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		Guild guild = API.GetGuild(guildId);
		if (guild == null)
		{
			return;
		}
		if (Guilds.maximumGuildMembers.Value != 0 && guild.Members.Count >= Guilds.maximumGuildMembers.Value)
		{
			object obj = _003C_003EO._003C0_003E__Pop;
			if (obj == null)
			{
				PopupButtonCallback val = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val;
				obj = (object)val;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_guild_full", "$guilds_guild_full_details", (PopupButtonCallback)obj, true));
		}
		else
		{
			API.RemovePlayerApplication(applicant, guild);
			guild = API.GetGuild(guildId);
			if (guild != null)
			{
				API.AddPlayerToGuild(applicant, guild);
				Guilds.SendMessageToPlayer(applicant, Localization.instance.Localize("$guilds_application_accepted", new string[1] { guild.Name }));
			}
		}
	}

	public void OnWhyMe_ButtonClicked()
	{
		((Component)applicationsUI.popupRootRect).gameObject.SetActive(true);
		((TMP_Text)applicationsUI.popupHeaderText).text = applicant.name;
		((TMP_Text)applicationsUI.popupViewportContentBodyText).text = application.description;
	}
}
