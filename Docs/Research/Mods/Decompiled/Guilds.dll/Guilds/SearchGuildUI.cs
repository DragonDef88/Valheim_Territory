using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class SearchGuildUI : MonoBehaviour
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

	public RectTransform GuildRowPlaceholder;

	public RectTransform GuildRowPlaceholderBack;

	public RectTransform GuildRowPlaceholderBackBg;

	public RectTransform GuildRowPlaceholderBackBgImg;

	public RectTransform GuildRowPlaceholderBackBorder;

	public RectTransform GuildRowPlaceholderBackBorderImg;

	public RectTransform GuildRowPlaceholderContent;

	public RectTransform GuildRowPlaceholderContentLeftCol;

	public HorizontalLayoutGroup GuildRowPlaceholderContentLeftColHLG;

	public RectTransform GuildRowPlaceholderContentLeftColIcon;

	public Image GuildRowPlaceholderContentLeftColIconImage;

	public Image GuildRowPlaceholderContentLeftColIconImageSelected;

	public Image GuildRowPlaceholderContentLeftColIconImageBorder;

	public RectTransform GuildRowPlaceholderContentLeftColNaming;

	public VerticalLayoutGroup GuildRowPlaceholderContentLeftColNamingVLG;

	public RectTransform GuildRowPlaceholderContentLeftColNamingLvl;

	public TextMeshProUGUI GuildRowPlaceholderContentLeftColNamingLvlTMP;

	public RectTransform GuildRowPlaceholderContentLeftColNamingName;

	public TextMeshProUGUI GuildRowPlaceholderContentLeftColNamingNameTMP;

	public RectTransform GuildRowPlaceholderContentLeftColNamingLeader;

	public TextMeshProUGUI GuildRowPlaceholderContentLeftColNamingLeaderTMP;

	public RectTransform GuildRowPlaceholderContentRightCol;

	public Image GuildRowPlaceholderContentRightColBg;

	public TMP_InputField GuildRowPlaceholderContentRightColInputField;

	public Scrollbar contentScrollbar;

	public RectTransform contentScrollbarSlidingArea;

	public Image contentScrollbarImage;

	public RectTransform contentScrollbarHandle;

	public Image contentScrollbarHandleImage;

	private ApplyUI applyUI;

	private List<SearchGuildUIRow> _activeRows = new List<SearchGuildUIRow>();

	private Queue<SearchGuildUIRow> _pooledRows = new Queue<SearchGuildUIRow>();

	public void Awake()
	{
		((Component)GuildRowPlaceholder).gameObject.SetActive(false);
		applyUI = ((Component)((Component)this).transform.Find("ApplyUI")).GetComponent<ApplyUI>();
	}

	public void OnEnable()
	{
		UpdateRows();
	}

	public void UpdateRows()
	{
		if (((Component)this).gameObject.activeSelf)
		{
			if (API.GetOwnGuild() != null)
			{
				((Component)this).gameObject.SetActive(false);
				Interface.GuildManagementUI.SetActive(true);
			}
			else
			{
				PopulateRows(API.GetGuilds());
			}
		}
	}

	private void PopulateRows(List<Guild> guilds)
	{
		foreach (SearchGuildUIRow activeRow in _activeRows)
		{
			((Component)activeRow).gameObject.SetActive(false);
			_pooledRows.Enqueue(activeRow);
		}
		_activeRows.Clear();
		foreach (Guild guild in guilds)
		{
			SearchGuildUIRow row = GetRow();
			row.Setup(guild, applyUI);
			_activeRows.Add(row);
		}
	}

	private SearchGuildUIRow GetRow()
	{
		SearchGuildUIRow obj = ((_pooledRows.Count > 0) ? _pooledRows.Dequeue() : Object.Instantiate<GameObject>(((Component)GuildRowPlaceholder).gameObject, (Transform)(object)contentList).GetComponent<SearchGuildUIRow>());
		((Component)obj).gameObject.SetActive(true);
		return obj;
	}

	private void ReturnRowToPool(SearchGuildUIRow row)
	{
		((Component)row).gameObject.SetActive(false);
		_pooledRows.Enqueue(row);
	}

	public void OnButtonClosed_Clicked()
	{
		Interface.HideUI();
	}

	public void GuildRowPlaceholderContentRightColInputFieldOnValueChanged()
	{
	}

	public void GuildRowPlaceholderContentRightColInputFieldOnEndEdit()
	{
	}

	public void GuildRowPlaceholderContentRightColInputFieldOnSelect()
	{
	}

	public void GuildRowPlaceholderContentRightColInputFieldOnDeselect()
	{
	}
}
