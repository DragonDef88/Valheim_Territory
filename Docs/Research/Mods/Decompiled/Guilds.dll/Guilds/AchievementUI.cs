using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class AchievementUI : MonoBehaviour
{
	public Canvas canvas;

	public RectTransform root;

	public Image BackgroundBack;

	public Button ButtonClose;

	public Image ButtonCloseImage;

	public TextMeshProUGUI ButtonCloseTMP;

	public Image Background;

	public RectTransform Header;

	public HorizontalLayoutGroup HeaderHLG;

	public Image HeaderImageLeft;

	public Image HeaderImageRight;

	public TextMeshProUGUI HeaderTMP;

	public RectTransform content;

	public ScrollRect contentScrollRect;

	public Image contentScrollRectImage;

	public RectTransform contentList;

	public VerticalLayoutGroup contentListVLG;

	public ContentSizeFitter contentListContentSizeFitter;

	public TextMeshProUGUI guildLevel;

	public TextMeshProUGUI achievementsCompleted;

	[Header("Placeholder")]
	public RectTransform RowPlaceholder;

	public AchievementUIRow PlaceholderInstance;

	public RectTransform PlaceholderBack;

	public RectTransform PlaceholderBackBg;

	public RectTransform PlaceholderBackBgImg;

	public RectTransform PlaceholderBackBorder;

	public RectTransform PlaceholderBackBorderImg;

	public RectTransform PlaceholderContent;

	public RectTransform PlaceholderContentLeftCol;

	public HorizontalLayoutGroup PlaceholderContentLeftColHLG;

	public RectTransform PlaceholderContentLeftColIcon;

	public Image PlaceholderContentLeftColIconImage;

	public Image PlaceholderContentLeftColIconImageSelected;

	public Image PlaceholderContentLeftColIconImageBorder;

	public RectTransform PlaceholderContentRightColIcon;

	public Image PlaceholderContentRightColIconImage;

	public Image PlaceholderContentRightColIconImageSelected;

	public Image PlaceholderContentRightColIconImageBorder;

	public TextMeshProUGUI PlaceholderContentRightColIconGuildLevelTMP;

	public RectTransform PlaceholderContentAchievementInformationRect;

	public RectTransform PlaceholderContentAchievementInformationHeader;

	public HorizontalLayoutGroup PlaceholderContentAchievementInformationHeaderHLG;

	public TextMeshProUGUI PlaceholderContentAchievementInformationHeaderTitle;

	public TextMeshProUGUI PlaceholderContentAchievementInformationHeaderDate;

	public TextMeshProUGUI PlaceholderContentAchievementInformationHeaderDesc;

	public RectTransform PlaceholderContentAchievementInformationProgressBar;

	public Image PlaceholderContentAchievementInformationProgressBarBg;

	public Image PlaceholderContentAchievementInformationProgressBarGreen;

	public TextMeshProUGUI PlaceholderContentAchievementInformationProgressBarText;

	public RectTransform PlaceholderContentRightCol;

	public Image PlaceholderContentRightColBg;

	[Header("Content - Scroll")]
	public Scrollbar contentScrollbar;

	public RectTransform contentScrollbarSlidingArea;

	public Image contentScrollbarImage;

	public RectTransform contentScrollbarHandle;

	public Image contentScrollbarHandleImage;

	private List<AchievementUIRow> _activeRows = new List<AchievementUIRow>();

	private Queue<AchievementUIRow> _pooledRows = new Queue<AchievementUIRow>();

	public void OnEnable()
	{
		UpdateRows();
	}

	public void UpdateRows()
	{
		if (((Component)this).gameObject.activeSelf)
		{
			if (API.GetOwnGuild() == null)
			{
				Interface.SwitchUI(Interface.NoGuildUI);
			}
			else
			{
				PopulateRows(API.GetOwnGuild().Achievements);
			}
		}
	}

	public void PopulateRows(Dictionary<string, AchievementData> achievements)
	{
		int num = 0;
		foreach (AchievementUIRow activeRow in _activeRows)
		{
			((Component)activeRow).gameObject.SetActive(false);
			_pooledRows.Enqueue(activeRow);
		}
		_activeRows.Clear();
		foreach (KeyValuePair<string, AchievementConfig> kv in Achievements.AllAchievementConfigs())
		{
			if (achievements.TryGetValue(kv.Key, out AchievementData value) && value.completed.Count >= kv.Value.progress.Count)
			{
				num++;
			}
			if (!kv.Value.first || (value?.completed.Count ?? 0) >= 1 || !GuildList.guildList.Values.Any((Guild g) => g.Achievements.TryGetValue(kv.Key, out AchievementData value2) && value2.completed.Count > 0))
			{
				AchievementUIRow row = GetRow();
				row.Setup(kv.Value, value);
				_activeRows.Add(row);
			}
		}
		((TMP_Text)guildLevel).text = Localization.instance.Localize("$guilds_guildlevel", new string[1] { API.GetOwnGuild().General.level.ToString() });
		((TMP_Text)achievementsCompleted).text = Localization.instance.Localize("$guilds_achievements_completed", new string[1] { num.ToString() });
	}

	private AchievementUIRow GetRow()
	{
		AchievementUIRow obj = ((_pooledRows.Count > 0) ? _pooledRows.Dequeue() : Object.Instantiate<GameObject>(((Component)RowPlaceholder).gameObject, (Transform)(object)contentList).GetComponent<AchievementUIRow>());
		((Component)obj).gameObject.SetActive(true);
		return obj;
	}

	private void ReturnRowToPool(AchievementUIRow row)
	{
		((Component)row).gameObject.SetActive(false);
		_pooledRows.Enqueue(row);
	}

	public void OnButtonClosed_Clicked()
	{
		Interface.HideUI();
	}

	public void OnButtonGoBack_Clicked()
	{
		Interface.SwitchUI(Interface.GuildManagementUI);
	}
}
