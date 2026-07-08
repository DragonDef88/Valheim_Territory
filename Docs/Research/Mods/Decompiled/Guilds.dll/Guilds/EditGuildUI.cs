using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class EditGuildUI : MonoBehaviour
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

	[Header("GuildsColorPicker - Instance")]
	public GameObject guildsColorPicker;

	public RectTransform guildsColorPickerRect;

	public GuildColorPicker guildsColorPickerInstance;

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

	public RectTransform Col2ButtonEditRect;

	public Image Col2ButtonEditImg;

	public Button Col2ButtonEdit;

	public TextMeshProUGUI Col2ButtonEditTMP;

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

	public void OnEnable()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		Guild ownGuild = API.GetOwnGuild();
		Col2InputFieldGuildName.text = ownGuild.Name;
		Col2InputFieldGuildDescription.text = ownGuild.General.description;
		Col1IconContainerIcon.sprite = API.GetGuildIcon(ownGuild);
		guildIconId = ownGuild.General.icon;
		guildsColorPickerInstance.chosenColor = ownGuild.General.color;
		Color color = default(Color);
		if (ColorUtility.TryParseHtmlString(guildsColorPickerInstance.chosenColor, ref color))
		{
			((Graphic)guildsColorPlaceholderImg).color = color;
		}
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

	public void OnButtonEdit_Clicked()
	{
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected O, but got Unknown
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Expected O, but got Unknown
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Expected O, but got Unknown
		Guild ownGuild = API.GetOwnGuild();
		if (ownGuild.General.description != Col2InputFieldGuildDescription.text)
		{
			ownGuild.General.description = Col2InputFieldGuildDescription.text;
			API.SaveGuild(ownGuild);
		}
		if (ownGuild.General.icon != guildIconId)
		{
			ownGuild.General.icon = guildIconId;
			API.SaveGuild(ownGuild);
		}
		if (ownGuild.General.color != guildsColorPickerInstance.chosenColor)
		{
			ownGuild.General.color = guildsColorPickerInstance.chosenColor;
			API.SaveGuild(ownGuild);
		}
		if (Col2InputFieldGuildName.text.Trim().Length > Guilds.maximumGuildNameLength.Value)
		{
			string text = Localization.instance.Localize("$guilds_name_too_long_details", new string[1] { Guilds.maximumGuildNameLength.Value.ToString() });
			object obj = _003C_003EO._003C0_003E__Pop;
			if (obj == null)
			{
				PopupButtonCallback val = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val;
				obj = (object)val;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_name_too_long", text, (PopupButtonCallback)obj, true));
		}
		else if (Col2InputFieldGuildName.text.Trim().Length < Guilds.minimumGuildNameLength.Value)
		{
			string text2 = Localization.instance.Localize("$guilds_name_too_short_details", new string[1] { Guilds.minimumGuildNameLength.Value.ToString() });
			object obj2 = _003C_003EO._003C0_003E__Pop;
			if (obj2 == null)
			{
				PopupButtonCallback val2 = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val2;
				obj2 = (object)val2;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_name_too_short", text2, (PopupButtonCallback)obj2, true));
		}
		else if (ownGuild.Name != Col2InputFieldGuildName.text && !API.RenameGuild(ownGuild, Col2InputFieldGuildName.text.Trim()))
		{
			object obj3 = _003C_003EO._003C0_003E__Pop;
			if (obj3 == null)
			{
				PopupButtonCallback val3 = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val3;
				obj3 = (object)val3;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_name_taken", "$guilds_name_taken_details", (PopupButtonCallback)obj3, true));
		}
		else
		{
			Interface.SwitchUI(Interface.GuildManagementUI);
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
