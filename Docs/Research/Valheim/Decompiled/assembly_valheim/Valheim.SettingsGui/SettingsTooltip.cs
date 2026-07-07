using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Valheim.SettingsGui;

public class SettingsTooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public enum TooltipAlignment
	{
		Left,
		Bottom,
		Right,
		Top
	}

	[SerializeField]
	private GameObject m_tooltip;

	[SerializeField]
	private float m_showDelay = 0.2f;

	[SerializeField]
	private int m_space = 15;

	[SerializeField]
	private string m_textId;

	[SerializeField]
	private string m_topicId;

	[SerializeField]
	private TooltipAlignment m_tooltipAlignment = TooltipAlignment.Right;

	private static Selectable s_current;

	private Selectable m_selectable;

	private Image m_background;

	private TMP_Text m_text;

	private TMP_Text m_topic;

	private bool m_shown;

	private void Start()
	{
		if ((Object)(object)m_tooltip == (Object)null)
		{
			Debug.LogWarning((object)("No tooltip object set, removing tooltip component from " + ((Object)((Component)this).gameObject).name));
			Object.Destroy((Object)(object)this);
			return;
		}
		m_selectable = ((Component)this).GetComponent<Selectable>();
		if ((Object)(object)m_selectable == (Object)null)
		{
			Debug.LogWarning((object)("No selectable found, removing tooltip component from " + ((Object)((Component)this).gameObject).name));
			Object.Destroy((Object)(object)this);
			return;
		}
		Transform obj = Utils.FindChild(m_tooltip.transform, "Topic", (IterativeSearchType)0);
		m_topic = ((obj != null) ? ((Component)obj).GetComponent<TMP_Text>() : null);
		Transform obj2 = Utils.FindChild(m_tooltip.transform, "Text", (IterativeSearchType)0);
		m_text = ((obj2 != null) ? ((Component)obj2).GetComponent<TMP_Text>() : null);
		Transform obj3 = Utils.FindChild(m_tooltip.transform, "Background", (IterativeSearchType)0);
		m_background = ((obj3 != null) ? ((Component)obj3).GetComponent<Image>() : null);
		if ((Object)(object)s_current == (Object)null)
		{
			m_tooltip.gameObject.SetActive(false);
		}
		ZInput.OnInputLayoutChanged += OnInputLayoutChanged;
	}

	public void OnInputLayoutChanged()
	{
		if (m_shown)
		{
			Hide();
		}
	}

	private void Update()
	{
		if (ZInput.IsGamepadActive())
		{
			if ((Object)(object)s_current == (Object)(object)m_selectable && (Object)(object)EventSystem.current.currentSelectedGameObject != (Object)(object)((Component)m_selectable).gameObject)
			{
				Hide();
			}
			else if (!((Object)(object)EventSystem.current.currentSelectedGameObject != (Object)(object)((Component)m_selectable).gameObject) && (!((Object)(object)s_current == (Object)(object)m_selectable) || !m_shown))
			{
				Show();
			}
		}
	}

	private void OnDestroy()
	{
		ZInput.OnInputLayoutChanged -= OnInputLayoutChanged;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!((Object)(object)s_current == (Object)(object)m_selectable) || !m_shown)
		{
			Show();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!ZInput.GamepadActive)
		{
			Hide();
		}
	}

	private void Show()
	{
		s_current = m_selectable;
		m_shown = true;
		((MonoBehaviour)this).StartCoroutine(DelayedShow());
	}

	private void Hide()
	{
		m_tooltip.gameObject.SetActive(false);
		m_shown = false;
		if ((Object)(object)s_current == (Object)(object)m_selectable)
		{
			s_current = null;
		}
	}

	private IEnumerator DelayedShow()
	{
		yield return (object)new WaitForSecondsRealtime(m_showDelay);
		if ((Object)(object)s_current != (Object)(object)m_selectable || !m_shown)
		{
			yield break;
		}
		if ((Object)(object)m_topic != (Object)null)
		{
			m_topic.text = Localization.instance.Localize(m_topicId);
		}
		RectTransform component;
		if ((Object)(object)m_text != (Object)null)
		{
			m_text.text = Localization.instance.Localize(m_textId);
			if ((Object)(object)m_background != (Object)null)
			{
				m_tooltip.gameObject.SetActive(true);
				m_topic.ForceMeshUpdate(false, false);
				m_text.ForceMeshUpdate(false, false);
				m_tooltip.gameObject.SetActive(false);
				yield return 0;
				component = ((Component)m_background).gameObject.GetComponent<RectTransform>();
				RectTransform obj = component;
				float x = component.sizeDelta.x;
				Bounds textBounds = m_topic.textBounds;
				float y = ((Bounds)(ref textBounds)).size.y;
				textBounds = m_text.textBounds;
				obj.sizeDelta = new Vector2(x, y + ((Bounds)(ref textBounds)).size.y + 15f);
				component = ((Component)m_text).gameObject.GetComponent<RectTransform>();
				Vector2 anchoredPosition = component.anchoredPosition;
				textBounds = m_topic.textBounds;
				anchoredPosition.y = 0f - ((Bounds)(ref textBounds)).size.y - 10f;
				component.anchoredPosition = anchoredPosition;
			}
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		Transform val = ((Component)m_selectable).transform.Find("Background");
		component = ((!((Object)(object)val != (Object)null)) ? ((Component)m_selectable).gameObject.GetComponent<RectTransform>() : ((Component)val).GetComponent<RectTransform>());
		component.GetWorldCorners(array);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(array[3].x - array[0].x, array[1].y - array[0].y);
		Vector3[] array2 = (Vector3[])(object)new Vector3[4];
		component = ((Component)m_background).gameObject.GetComponent<RectTransform>();
		component.GetWorldCorners(array2);
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(array2[3].x - array2[0].x, array2[1].y - array2[0].y);
		float x2 = val3.x;
		Rect rect = component.rect;
		float num = x2 / ((Rect)(ref rect)).width;
		switch (m_tooltipAlignment)
		{
		case TooltipAlignment.Right:
			m_tooltip.transform.position = Vector2.op_Implicit(new Vector2(array[2].x + val3.x / 2f + (float)m_space * num, array[2].y - val2.y / 2f));
			break;
		case TooltipAlignment.Bottom:
			m_tooltip.transform.position = Vector2.op_Implicit(new Vector2(array[0].x + val2.x / 2f, array[0].y - val3.y / 2f - (float)m_space * num));
			break;
		case TooltipAlignment.Left:
			m_tooltip.transform.position = Vector2.op_Implicit(new Vector2(array[0].x - val3.x / 2f - (float)m_space * num, array[2].y - val2.y / 2f));
			break;
		case TooltipAlignment.Top:
			m_tooltip.transform.position = Vector2.op_Implicit(new Vector2(array[1].x + val2.x / 2f, array[1].y + val3.y / 2f + (float)m_space * num));
			break;
		}
		m_tooltip.gameObject.SetActive(true);
	}
}
