using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class CreateGuildUI : MonoBehaviour
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static PopupButtonCallback _003C0_003E__Pop;
	}

	private int guildIconId;

	[Header("Root Objects")]
	public Canvas canvas;

	public RectTransform root;

	public Image Background;

	public Image BackgroundBack;

	[Header("Close Button")]
	public Image BackgroundBackButtonImage;

	public Button BackgroundBackButton;

	public TextMeshProUGUI BackgroundBackButtonTextMeshProUGui;

	[Header("GuildsColorPicker - Instance")]
	public GameObject guildsColorPicker;

	public RectTransform guildsColorPickerRect;

	public GuildColorPicker guildsColorPickerInstance;

	[Header("Header Area")]
	public RectTransform header;

	public HorizontalLayoutGroup headerHLayoutGroup;

	public Image headerLeftImage;

	public Image headerRightImage;

	public TextMeshProUGUI headerTMP;

	[Header("Content Area")]
	public RectTransform content;

	[Header("Column 1")]
	public RectTransform Col1;

	public VerticalLayoutGroup Col1VLayoutGroup;

	public RectTransform Col1IconContainerRect;

	public Image Col1IconContainerIcon;

	public RectTransform Col1IconContainerIconRect;

	public Image Col1IconContainerBorder;

	public RectTransform Col1IconContainerBorderRect;

	public Button Col1ButtonSelect;

	public Image Col1ButtonSelectImage;

	public TextMeshProUGUI Col1ButtonTMP;

	[Header("Column 2")]
	public RectTransform Col2;

	public VerticalLayoutGroup Col2VLG;

	public RectTransform Col2InputFieldGuildNameRect;

	public TMP_InputField Col2InputFieldGuildName;

	public TextMeshProUGUI Col2InputFieldGuildNamePlaceHolder;

	public TextMeshProUGUI Col2InputFieldGuildNameText;

	public TMP_InputField Col2InputFieldGuildDescription;

	public RectTransform Col2InputFieldGuildDescriptionRect;

	public TextMeshProUGUI Col2InputFieldGuildDescriptionPlaceholder;

	public TextMeshProUGUI Col2InputFieldGuildDescriptionText;

	public RectTransform Col2ButtonCreateRect;

	public Image Col2ButtonCreateImg;

	public Button Col2ButtonCreate;

	public TextMeshProUGUI Col2ButtonCreateTMP;

	public RectTransform Col3;

	public Image Col3Icon;

	public TextMeshProUGUI Col3RequirementsText;

	[Header("GuildIconUI - Instance")]
	public GameObject guildIconUI;

	public RectTransform guildIconUIRect;

	public GuildIconUI guildIconUIInstance;

	public Image guildsColorPlaceholderImg;

	public void Awake()
	{
		Col2InputFieldGuildName.characterValidation = (CharacterValidation)8;
		Col2InputFieldGuildName.inputValidator = (TMP_InputValidator)(object)ScriptableObject.CreateInstance<Tools.NameValidator>();
		((Component)Col3RequirementsText).gameObject.SetActive(false);
		guildsColorPickerInstance.chosenColorPreview = guildsColorPlaceholderImg;
	}

	public void OnButtonClosed_Clicked()
	{
		Interface.HideUI();
	}

	public void OnButtonSelectIcon_Clicked()
	{
		guildIconUIInstance.selectedGuildIcon = delegate(int idx)
		{
			guildIconId = idx;
			Col1IconContainerIcon.sprite = Interface.GuildIcons[idx];
		};
	}

	public void OnButtonCreate_Clicked()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		if (guildIconId == 0)
		{
			object obj = _003C_003EO._003C0_003E__Pop;
			if (obj == null)
			{
				PopupButtonCallback val = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val;
				obj = (object)val;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_no_icon_selected", "$guilds_no_icon_selected_details", (PopupButtonCallback)obj, true));
			return;
		}
		if (Col2InputFieldGuildName.text.Trim().Length > Guilds.maximumGuildNameLength.Value)
		{
			string text = Localization.instance.Localize("$guilds_name_too_long_details", new string[1] { Guilds.maximumGuildNameLength.Value.ToString() });
			object obj2 = _003C_003EO._003C0_003E__Pop;
			if (obj2 == null)
			{
				PopupButtonCallback val2 = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val2;
				obj2 = (object)val2;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_name_too_long", text, (PopupButtonCallback)obj2, true));
			return;
		}
		if (Col2InputFieldGuildName.text.Trim().Length < Guilds.minimumGuildNameLength.Value)
		{
			string text2 = Localization.instance.Localize("$guilds_name_too_short_details", new string[1] { Guilds.minimumGuildNameLength.Value.ToString() });
			object obj3 = _003C_003EO._003C0_003E__Pop;
			if (obj3 == null)
			{
				PopupButtonCallback val3 = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val3;
				obj3 = (object)val3;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_name_too_short", text2, (PopupButtonCallback)obj3, true));
			return;
		}
		Guild guild = API.CreateGuild(Col2InputFieldGuildName.text.Trim(), PlayerReference.forOwnPlayer());
		if (guild == null)
		{
			object obj4 = _003C_003EO._003C0_003E__Pop;
			if (obj4 == null)
			{
				PopupButtonCallback val4 = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val4;
				obj4 = (object)val4;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_name_taken", "$guilds_name_taken_details", (PopupButtonCallback)obj4, true));
		}
		else
		{
			API.RemovePlayerApplication(PlayerReference.forOwnPlayer());
			guild.General.description = Col2InputFieldGuildDescription.text;
			guild.General.icon = guildIconId;
			guild.General.color = guildsColorPickerInstance.chosenColor;
			API.SaveGuild(guild);
			Interface.HideUI();
			Interface.GuildManagementUI.SetActive(true);
		}
	}

	public void OnCol2InputField_GuildName_ValueChanged()
	{
	}

	public void OnCol2InputField_GuildName_EndEdit()
	{
	}

	public void OnCol2InputField_GuildName_Select()
	{
	}

	public void OnCol2InputField_GuildName_Deselect()
	{
	}

	public void OnCol2InputField_GuildDescription_ValueChanged()
	{
	}

	public void OnCol2InputField_GuildDescription_EndEdit()
	{
	}

	public void OnCol2InputField_GuildDescription_Select()
	{
	}

	public void OnCol2InputField_GuildDescription_Deselect()
	{
	}
}
