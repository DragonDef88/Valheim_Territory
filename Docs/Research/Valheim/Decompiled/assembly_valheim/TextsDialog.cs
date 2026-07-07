using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextsDialog : MonoBehaviour
{
	public class TextInfo
	{
		public string m_topic;

		public string m_text;

		public GameObject m_listElement;

		public GameObject m_selected;

		public TextInfo(string topic, string text)
		{
			m_topic = topic;
			m_text = text;
		}
	}

	public RectTransform m_listRoot;

	public ScrollRect m_leftScrollRect;

	public Scrollbar m_leftScrollbar;

	public Scrollbar m_rightScrollbar;

	public GameObject m_elementPrefab;

	public TMP_Text m_totalSkillText;

	public float m_spacing = 80f;

	public TMP_Text m_textAreaTopic;

	public TMP_Text m_textArea;

	public ScrollRectEnsureVisible m_recipeEnsureVisible;

	private List<TextInfo> m_texts = new List<TextInfo>();

	private float m_baseListSize;

	private int m_selectionIndex;

	private float m_inputDelayTimer;

	private const float InputDelay = 0.1f;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = m_listRoot.rect;
		m_baseListSize = ((Rect)(ref rect)).height;
	}

	public void Setup(Player player)
	{
		((Component)this).gameObject.SetActive(true);
		FillTextList();
		if (m_texts.Count > 0)
		{
			ShowText(0);
			return;
		}
		m_textAreaTopic.text = "";
		m_textArea.text = "";
	}

	private void Update()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		UpdateGamepadInput();
		if (m_texts.Count > 0)
		{
			Transform transform = ((Component)m_leftScrollRect).transform;
			RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			RectTransform listRoot = m_listRoot;
			Scrollbar leftScrollbar = m_leftScrollbar;
			Rect rect = val.rect;
			float height = ((Rect)(ref rect)).height;
			rect = listRoot.rect;
			leftScrollbar.size = height / ((Rect)(ref rect)).height;
		}
	}

	private IEnumerator FocusOnCurrentLevel(ScrollRect scrollRect, RectTransform listRoot, RectTransform element)
	{
		yield return null;
		yield return null;
		Canvas.ForceUpdateCanvases();
		SnapTo(scrollRect, m_listRoot, element);
	}

	private void SnapTo(ScrollRect scrollRect, RectTransform listRoot, RectTransform target)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Canvas.ForceUpdateCanvases();
		listRoot.anchoredPosition = Vector2.op_Implicit(((Component)scrollRect).transform.InverseTransformPoint(((Transform)listRoot).position)) - Vector2.op_Implicit(((Component)scrollRect).transform.InverseTransformPoint(((Transform)target).position)) - new Vector2(target.sizeDelta.x / 2f, 0f);
	}

	private void FillTextList()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected O, but got Unknown
		foreach (TextInfo text2 in m_texts)
		{
			Object.Destroy((Object)(object)text2.m_listElement);
		}
		m_texts.Clear();
		UpdateTextsList();
		for (int i = 0; i < m_texts.Count; i++)
		{
			TextInfo text = m_texts[i];
			GameObject val = Object.Instantiate<GameObject>(m_elementPrefab, Vector3.zero, Quaternion.identity, (Transform)(object)m_listRoot);
			val.SetActive(true);
			Transform transform = val.transform;
			((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(0f, (float)(-i) * m_spacing);
			((Component)Utils.FindChild(val.transform, "name", (IterativeSearchType)0)).GetComponent<TMP_Text>().text = Localization.instance.Localize(text.m_topic);
			text.m_listElement = val;
			text.m_selected = ((Component)Utils.FindChild(val.transform, "selected", (IterativeSearchType)0)).gameObject;
			text.m_selected.SetActive(false);
			((UnityEvent)val.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				OnSelectText(text);
			});
		}
		float num = Mathf.Max(m_baseListSize, (float)m_texts.Count * m_spacing);
		m_listRoot.SetSizeWithCurrentAnchors((Axis)1, num);
		if (m_texts.Count > 0)
		{
			ScrollRectEnsureVisible recipeEnsureVisible = m_recipeEnsureVisible;
			Transform transform2 = m_texts[0].m_listElement.transform;
			recipeEnsureVisible.CenterOnItem((RectTransform)(object)((transform2 is RectTransform) ? transform2 : null));
		}
	}

	private void UpdateGamepadInput()
	{
		if (m_inputDelayTimer > 0f)
		{
			m_inputDelayTimer -= Time.unscaledDeltaTime;
		}
		else if (ZInput.IsGamepadActive() && m_texts.Count > 0)
		{
			float joyRightStickY = ZInput.GetJoyRightStickY(true);
			float joyLeftStickY = ZInput.GetJoyLeftStickY(true);
			bool buttonDown = ZInput.GetButtonDown("JoyDPadUp");
			bool num = joyLeftStickY < -0.1f;
			bool buttonDown2 = ZInput.GetButtonDown("JoyDPadDown");
			bool flag = joyLeftStickY > 0.1f;
			if ((buttonDown2 || flag) && m_selectionIndex < m_texts.Count - 1)
			{
				ShowText(Mathf.Min(m_texts.Count - 1, GetSelectedText() + 1));
				m_inputDelayTimer = 0.1f;
			}
			if ((num || buttonDown) && m_selectionIndex > 0)
			{
				ShowText(Mathf.Max(0, GetSelectedText() - 1));
				m_inputDelayTimer = 0.1f;
			}
			if (((Component)m_rightScrollbar).gameObject.activeSelf && (joyRightStickY < -0.1f || joyRightStickY > 0.1f))
			{
				m_rightScrollbar.value = Mathf.Clamp01(m_rightScrollbar.value - joyRightStickY * 10f * Time.deltaTime * (1f - m_rightScrollbar.size));
				m_inputDelayTimer = 0.1f;
			}
		}
	}

	private void OnSelectText(TextInfo text)
	{
		ShowText(text);
	}

	private int GetSelectedText()
	{
		for (int i = 0; i < m_texts.Count; i++)
		{
			if (m_texts[i].m_selected.activeSelf)
			{
				return i;
			}
		}
		return 0;
	}

	private void ShowText(int i)
	{
		m_selectionIndex = i;
		ShowText(m_texts[i]);
	}

	private void ShowText(TextInfo text)
	{
		m_textAreaTopic.text = Localization.instance.Localize(text.m_topic);
		m_textArea.text = Localization.instance.Localize(text.m_text);
		foreach (TextInfo text2 in m_texts)
		{
			text2.m_selected.SetActive(false);
		}
		text.m_selected.SetActive(true);
		_003F val = this;
		ScrollRect leftScrollRect = m_leftScrollRect;
		RectTransform listRoot = m_listRoot;
		Transform transform = text.m_selected.transform;
		((MonoBehaviour)val).StartCoroutine(FocusOnCurrentLevel(leftScrollRect, listRoot, (RectTransform)(object)((transform is RectTransform) ? transform : null)));
	}

	public void OnClose()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void UpdateTextsList()
	{
		m_texts.Clear();
		foreach (KeyValuePair<string, string> knownText in Player.m_localPlayer.GetKnownTexts())
		{
			m_texts.Add(new TextInfo(Localization.instance.Localize(knownText.Key.Replace("\u0016", "")), Localization.instance.Localize(knownText.Value.Replace("\u0016", ""))));
		}
		m_texts.Sort((TextInfo a, TextInfo b) => a.m_topic.CompareTo(b.m_topic));
		AddLog();
		AddActiveEffects();
	}

	private void AddLog()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in MessageHud.instance.GetLog())
		{
			stringBuilder.Append(item + "\n\n");
		}
		m_texts.Insert(0, new TextInfo(Localization.instance.Localize("$inventory_logs"), stringBuilder.ToString()));
	}

	private void AddActiveEffects()
	{
		if (!Object.op_Implicit((Object)(object)Player.m_localPlayer))
		{
			return;
		}
		List<StatusEffect> list = new List<StatusEffect>();
		Player.m_localPlayer.GetSEMan().GetHUDStatusEffects(list);
		StringBuilder stringBuilder = new StringBuilder(256);
		foreach (StatusEffect item in list)
		{
			stringBuilder.Append("<color=orange>" + Localization.instance.Localize(item.m_name) + "</color>\n");
			stringBuilder.Append(Localization.instance.Localize(item.GetTooltipString()));
			stringBuilder.Append("\n\n");
		}
		Player.m_localPlayer.GetGuardianPowerHUD(out var se, out var _);
		if (Object.op_Implicit((Object)(object)se))
		{
			stringBuilder.Append("<color=yellow>" + Localization.instance.Localize("$inventory_selectedgp") + "</color>\n");
			stringBuilder.Append("<color=orange>" + Localization.instance.Localize(se.m_name) + "</color>\n");
			stringBuilder.Append(Localization.instance.Localize(se.GetTooltipString()));
		}
		m_texts.Insert(0, new TextInfo(Localization.instance.Localize("$inventory_activeeffects"), stringBuilder.ToString()));
	}
}
