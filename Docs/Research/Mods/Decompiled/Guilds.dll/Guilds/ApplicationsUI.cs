using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guilds;

[PublicAPI]
public class ApplicationsUI : MonoBehaviour
{
	[Header("Placeholder Variables")]
	public ApplicationsUIRow rowPlaceHolderPrefab;

	public Transform rowPlaceHolderParentList;

	public List<ApplicationsUIRow> rowElements;

	[Header("Root UI")]
	public RectTransform rootTransform;

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

	public RectTransform contentList;

	public VerticalLayoutGroup contentListVlg;

	public ContentSizeFitter contentListSizeFitter;

	[Header("Row Placeholder UI")]
	public RectTransform rowTransform;

	public Image rowBackImage;

	public Image rowBorderImage;

	public HorizontalLayoutGroup contentHlg;

	[Header("Row Content UI - Name Area")]
	public RectTransform nameAreaRect;

	public VerticalLayoutGroup nameAreaVlg;

	public TextMeshProUGUI nameAreaNameTextTMP;

	[Header("Row Content UI - WhyMe Area")]
	public RectTransform whyMeArea;

	public VerticalLayoutGroup whyMeAreaVlg;

	public Button whyMeButton;

	public Image whyMeButtonImg;

	public TextMeshProUGUI whyMeAreaText;

	[Header("Row Content UI - Applied Area")]
	public RectTransform appliedAreaRect;

	public VerticalLayoutGroup appliedAreaVlg;

	public TextMeshProUGUI appliedAreaAppliedStatusTextTMP;

	[Header("Row Content UI - Action Area")]
	public RectTransform actionArea;

	public Button actionAreaAcceptMemberButton;

	public Image actionAreaAcceptMemberButtonImage;

	public Image actionAreaAcceptMemberButtonCheckmark;

	public Button actionAreaDenyMemberButton;

	public TextMeshProUGUI actionAreaDenyMemberButtonTextTMP;

	public Image actionAreaDenyMemberButtonImage;

	[Header("Root UI Global Scrollbar")]
	public Scrollbar scrollbar;

	public RectTransform scrollbarRect;

	public RectTransform scrollbarSlidingArea;

	public RectTransform scrollbarHandle;

	public Image scrollbarHandleImage;

	[Header("Back Button")]
	public Button backButton;

	public Image backButtonImage;

	public TextMeshProUGUI backButtonText;

	[Header("Accept Guild Member Button")]
	public Button acceptGuildMemberButton;

	public Image acceptGuildMemberButtonImage;

	public Image acceptGuildMemberButtonCheckmark;

	[Header("Deny Guild Member Button")]
	public Button denyGuildMemberButton;

	public Image denyGuildMemberImage;

	public TextMeshProUGUI denyGuildMemberText;

	[Header("Popup")]
	public RectTransform popupRootRect;

	public RectTransform popupBkgBlocking;

	public Image popupBkgBlockingImg;

	public CanvasGroup popupCanvasGroup;

	public RectTransform popupBkg;

	public Image popupBkgImg;

	public RectTransform popupScrollviewRect;

	public ScrollRect popupScrollview;

	public Image popupScrollviewImage;

	public RectTransform popupViewport;

	public Image popupViewportImage;

	public RectTransform popupViewportContentRect;

	public RectTransform popupViewportContentBodyTextRect;

	public TextMeshProUGUI popupViewportContentBodyText;

	public RectTransform popupScrollbarRect;

	public Scrollbar popupScrollbar;

	public Image popupScrollbarImg;

	public RectTransform popupSlidingAreaRect;

	public RectTransform popupHandleRect;

	public Image popupHandleImg;

	public RectTransform popupHeaderTextRect;

	public TextMeshProUGUI popupHeaderText;

	public RectTransform popupButtonGroupRect;

	public HorizontalLayoutGroup popupButtonGroup;

	public RectTransform popupButtonOkRect;

	public Button popupButtonOk;

	public Image popupButtonOkImg;

	public RectTransform popupButtonOkTextRect;

	public Text popupButtonOkText;

	private List<ApplicationsUIRow> _activeRows = new List<ApplicationsUIRow>();

	private Queue<ApplicationsUIRow> _pooledRows = new Queue<ApplicationsUIRow>();

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
				PopulateRows(API.GetOwnGuild().Applications);
			}
		}
	}

	public void PopulateRows(Dictionary<PlayerReference, Application> members)
	{
		foreach (ApplicationsUIRow activeRow in _activeRows)
		{
			((Component)activeRow).gameObject.SetActive(false);
			_pooledRows.Enqueue(activeRow);
		}
		_activeRows.Clear();
		foreach (KeyValuePair<PlayerReference, Application> member in members)
		{
			ApplicationsUIRow row = GetRow();
			row.Setup(this, member.Key, member.Value);
			_activeRows.Add(row);
		}
	}

	private ApplicationsUIRow GetRow()
	{
		ApplicationsUIRow obj = ((_pooledRows.Count > 0) ? _pooledRows.Dequeue() : Object.Instantiate<GameObject>(((Component)rowPlaceHolderPrefab).gameObject, rowPlaceHolderParentList).GetComponent<ApplicationsUIRow>());
		((Component)obj).gameObject.SetActive(true);
		return obj;
	}

	private void ReturnRowToPool(ApplicationsUIRow row)
	{
		((Component)row).gameObject.SetActive(false);
		_pooledRows.Enqueue(row);
	}

	public void whyMeAreaClicked()
	{
	}

	public void OnButtonClosed_Clicked()
	{
		Interface.HideUI();
	}

	public void ButtonBackClicked()
	{
		Interface.SwitchUI(Interface.GuildManagementUI);
	}

	public void ButtonEditGuildClicked()
	{
	}

	public void ButtonApplicationsClicked()
	{
	}
}
