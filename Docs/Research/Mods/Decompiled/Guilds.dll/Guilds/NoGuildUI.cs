using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class NoGuildUI : MonoBehaviour
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static PopupButtonCallback _003C0_003E__Pop;
	}

	[Header("Root")]
	public RectTransform root;

	public Image Background;

	public Image BackgroundBackImg;

	[Header("Close Button")]
	public Button ButtonClose;

	public Image ButtonCloseImage;

	public RectTransform ButtonCloseRect;

	public TextMeshProUGUI ButtonCloseText;

	[Header("Content")]
	public RectTransform content;

	public VerticalLayoutGroup contentVLG;

	public TextMeshProUGUI contentText;

	[Header("Content Panel")]
	public RectTransform panelRect;

	public HorizontalLayoutGroup panelHLG;

	public Button panelButtonCreate;

	public Image panelButtonCreateImg;

	public TextMeshProUGUI panelButtonCreateText;

	public Button panelButtonConnect;

	public Image panelButtonConnectImg;

	public TextMeshProUGUI panelButtonConnectText;

	public Image Border;

	public void OnEnable()
	{
		Guild ownAppliedGuild = API.GetOwnAppliedGuild();
		if (ownAppliedGuild != null)
		{
			((TMP_Text)contentText).text = Localization.instance.Localize("$guilds_pending_application", new string[1] { ownAppliedGuild.Name });
		}
		else
		{
			((TMP_Text)contentText).text = Localization.instance.Localize("$guilds_noguild_message");
		}
	}

	public void OnButtonClosed_Clicked()
	{
		Interface.HideUI();
	}

	public void OnButtonCreate_Clicked()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		if (Guilds.allowGuildCreation.Value == Toggle.Off && !Guilds.configSync.IsAdmin)
		{
			object obj = _003C_003EO._003C0_003E__Pop;
			if (obj == null)
			{
				PopupButtonCallback val = UnifiedPopup.Pop;
				_003C_003EO._003C0_003E__Pop = val;
				obj = (object)val;
			}
			UnifiedPopup.Push((PopupBase)new WarningPopup("$guilds_creation_admin_only", "$guilds_creation_admin_only_details", (PopupButtonCallback)obj, true));
		}
		else
		{
			Interface.SwitchUI(Interface.CreateGuildUI);
		}
	}

	public void OnButtonConnect_Clicked()
	{
		Interface.SwitchUI(Interface.SearchGuildUI);
	}
}
