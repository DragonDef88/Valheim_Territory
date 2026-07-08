using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class SearchGuildUIRow : MonoBehaviour
{
	private Guild guild;

	private ApplyUI applyUI;

	[Header("Row Root")]
	public RectTransform rowRootTransform;

	public GameObject rowRootGameObject;

	public RectTransform back;

	public Image backImage;

	public Image borderImage;

	[Header("Content Area")]
	public RectTransform contentAreaRectTransform;

	[Header("Left Column")]
	public RectTransform leftColumnRect;

	public HorizontalLayoutGroup leftColumnHLayoutGroup;

	[Header("Left Column - Icon Container")]
	public RectTransform iconContainerRect;

	public Image iconContainerImg;

	public RectTransform guildIconImgRect;

	public RectTransform guildIconBorderRect;

	public Image guildIconBorderImg;

	public Image guildIconImg;

	public Button guildIconButton;

	[Header("Left Column - Naming Area")]
	public RectTransform namingAreaRect;

	public VerticalLayoutGroup namingAreaVLayoutGroup;

	public RectTransform levelTextRect;

	public TextMeshProUGUI levelText;

	public RectTransform nameTextRect;

	public TextMeshProUGUI nameText;

	public RectTransform leaderTextRect;

	public TextMeshProUGUI leaderText;

	[Header("Right Column")]
	public RectTransform rightColumnRect;

	public Image rightColumnBkg;

	public RectTransform rightColumnInputFieldRect;

	public TMP_InputField rightColumnInputField;

	public TextMeshProUGUI rightColumnInputFieldPlaceholderText;

	public TextMeshProUGUI rightColumnInputFieldText;

	public void Setup(Guild guild, ApplyUI applyUI)
	{
		this.guild = guild;
		this.applyUI = applyUI;
		((TMP_Text)leaderText).text = Localization.instance.Localize("$guilds_rank_leader: ") + API.GetGuildLeader(guild).name;
		((TMP_Text)nameText).text = guild.Name;
		guildIconImg.sprite = API.GetGuildIcon(guild);
		((TMP_Text)levelText).text = Localization.instance.Localize("$guilds_level ") + guild.General.level;
		rightColumnInputField.text = guild.General.description;
	}

	public void OnGuildIconButton_Clicked()
	{
		applyUI.Setup(guild);
	}
}
