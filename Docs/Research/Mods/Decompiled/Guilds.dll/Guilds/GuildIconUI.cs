using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class GuildIconUI : MonoBehaviour
{
	[Header("Root UI")]
	public RectTransform rootTransform;

	public GameObject rootGameObject;

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

	public Scrollbar guildIconListScroll;

	public Image guildIconListScrollImage;

	public RectTransform guildIconListScrollSlidingArea;

	public RectTransform guildIconListScrollHandle;

	public Image guildIconListScrollHandleImage;

	[Header("Content UI - Guild Icons")]
	public RectTransform guildIconListRoot;

	public GridLayoutGroup guildIconListRootGlg;

	public ContentSizeFitter guildIconListRootSizeFitter;

	[Header("Guild Icon Placeholder")]
	public GameObject guidIconElementPrefab;

	public GuildIconElement guildIconElementPrefabComponent;

	public Button guildIconElementButton;

	public Image guildIconElementButtonImage;

	public Image guildIconElementButtonIconBackground;

	public Image guildIconElementButtonIcon;

	public List<GameObject> guildIconList = new List<GameObject>();

	public Action<int> selectedGuildIcon;

	public void Awake()
	{
		FillTrophyList();
	}

	public void FillTrophyList()
	{
		foreach (KeyValuePair<int, Sprite> guildIcon in Interface.GuildIcons)
		{
			GuildIconElement component = Object.Instantiate<GameObject>(guidIconElementPrefab, (Transform)(object)guildIconListRoot).GetComponent<GuildIconElement>();
			component.guildIcon.sprite = guildIcon.Value;
			((Component)component).gameObject.SetActive(true);
			component.guildIconUI = this;
			component.guildIconId = guildIcon.Key;
		}
	}

	public void OnButtonClosed_Clicked()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
