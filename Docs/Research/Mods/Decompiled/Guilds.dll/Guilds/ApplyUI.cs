using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class ApplyUI : MonoBehaviour
{
	public Guild guild;

	[Header("Root Objects")]
	public RectTransform root;

	public Image Background;

	public Image BackgroundBack;

	[Header("Close Button")]
	public Image BackgroundBackButtonImage;

	public Button BackgroundBackButton;

	public TextMeshProUGUI BackgroundBackButtonTextMeshProUGui;

	[Header("Header Area")]
	public RectTransform header;

	public HorizontalLayoutGroup headerHLayoutGroup;

	public Image headerLeftImage;

	public Image headerRightImage;

	public TextMeshProUGUI headerTMP;

	[Header("Content Area")]
	public RectTransform content;

	public HorizontalLayoutGroup contentHLayoutGroup;

	[Header("Content - Text Entry Area")]
	public RectTransform textArea;

	public VerticalLayoutGroup textAreaVLayoutGroup;

	public RectTransform textAreaContainerRect;

	public TMP_InputField textAreaInputField;

	public Image textAreaInputFieldBkg;

	public TextMeshProUGUI textareaInputPlaceholderText;

	public TextMeshProUGUI textAreaInputText;

	[Header("Content - Action Area")]
	public RectTransform actionAreaRect;

	public HorizontalLayoutGroup actionAreaHLayoutGroup;

	public Button actionAreaButtonCancel;

	public Image actionAreaButtonCancelImg;

	public TextMeshProUGUI actionAreaButtonCancelText;

	public Button actionAreaButtonApply;

	public Image actionAreaButtonApplyImg;

	public TextMeshProUGUI actionAreaButtonApplyText;

	public void Setup(Guild guild)
	{
		this.guild = guild;
		((TMP_Text)headerTMP).text = Localization.instance.Localize("$guilds_applyguildui_title", new string[1] { guild.Name });
	}

	public void OnButtonClose_Clicked()
	{
		Interface.HideUI();
	}

	public void OnButtonCancel_Clicked()
	{
		Interface.SwitchUI(Interface.SearchGuildUI);
	}

	public void OnButtonApply_Clicked()
	{
		API.ApplyToGuild(PlayerReference.forOwnPlayer(), textAreaInputField.text, guild);
		Interface.HideUI();
	}

	public void OnTextAreaInputField_ValueChanged()
	{
	}

	public void OnTextAreaInputField_EndEdit()
	{
	}

	public void OnTextAreaInputField_Select()
	{
	}

	public void OnTextAreaInputField_Deselect()
	{
	}
}
