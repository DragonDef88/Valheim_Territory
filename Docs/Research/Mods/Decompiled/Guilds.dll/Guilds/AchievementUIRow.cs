using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class AchievementUIRow : MonoBehaviour
{
	[Header("Row Root")]
	public RectTransform rowRootTransform;

	public AchievementUIRow rowInstance;

	public GameObject rowRootGameObject;

	public RectTransform back;

	public Image backImage;

	public Image borderImage;

	public GameObject rowNotCompleted;

	[Header("Content Area")]
	public RectTransform contentAreaRectTransform;

	[Header("Left Column")]
	public RectTransform leftColumnRect;

	public HorizontalLayoutGroup leftColumnHLayoutGroup;

	[Header("Left Column - Icon Container")]
	public RectTransform achievementIconContainerRect;

	public Image achievementIconContainerImg;

	public RectTransform achievementIconImgRect;

	public RectTransform achievementIconBorderRect;

	public Image achievementIconBorderImg;

	public Image achievementIconImg;

	[Header("Right Column - Rank Area")]
	public RectTransform rightCol;

	public Image rightColBg;

	public RectTransform rightColIconContainer;

	public Image rightColIconContainerSelectedIcon;

	public Image rightColIconcontainerBorder;

	public RectTransform guildLevelRect;

	public TextMeshProUGUI guildLevelText;

	[Header("Right Column - AchievementInformation")]
	public RectTransform aiRect;

	public RectTransform aiHeader;

	public HorizontalLayoutGroup aiHeaderHLG;

	public TextMeshProUGUI aiHeaderTitle;

	public TextMeshProUGUI aiHeaderDate;

	public TextMeshProUGUI aiDescription;

	public RectTransform progressBar;

	public Image progressBarBg;

	public Image progressBarBgGreen;

	public TextMeshProUGUI progressText;

	public Sprite defaultIcon;

	public void Awake()
	{
		defaultIcon = achievementIconImg.sprite;
	}

	public void Setup(AchievementConfig config, AchievementData? data)
	{
		if (data == null)
		{
			data = new AchievementData();
		}
		int num = Math.Min(data.completed.Count, config.progress.Count - 1);
		float num2 = config.progress[num];
		((TMP_Text)aiDescription).text = config.config.Aggregate<KeyValuePair<string, string>, string>(Localization.instance.Localize(config.description), (string text, KeyValuePair<string, string> kv) => text.Replace("{" + kv.Key + "}", kv.Value));
		((TMP_Text)aiHeaderTitle).text = config.name;
		achievementIconImg.sprite = config.GetIcon() ?? defaultIcon;
		((TMP_Text)guildLevelText).text = config.GetLevel(num + 1).ToString();
		((Component)progressBar).gameObject.SetActive(num2 > 1f);
		if (config.progress.Count > data.completed.Count)
		{
			float? progress = data.progress;
			if (progress.HasValue)
			{
				progressBarBgGreen.fillAmount = data.progress.Value / num2;
				((TMP_Text)progressText).text = $"{data.progress} / {num2}";
				goto IL_0182;
			}
		}
		float num3 = config.progress.Last();
		progressBarBgGreen.fillAmount = 1f;
		((TMP_Text)progressText).text = $"{num3} / {num3}";
		goto IL_0182;
		IL_0182:
		((TMP_Text)aiHeaderDate).text = ((data.completed.Count > 0) ? data.completed.Last().ToString("yyyy-MM-dd HH:mm") : "");
		rowNotCompleted.SetActive(data.completed.Count == 0);
	}
}
