using TMPro;
using UnityEngine;

public class TextViewer : MonoBehaviour
{
	public enum Style
	{
		Rune,
		Intro,
		Raven
	}

	private static TextViewer m_instance;

	private Animator m_animator;

	private Animator m_animatorIntro;

	private Animator m_animatorRaven;

	[Header("Rune")]
	public GameObject m_root;

	public TMP_Text m_topic;

	public TMP_Text m_text;

	public TMP_Text m_runeText;

	public GameObject m_closeText;

	[Header("Intro")]
	public GameObject m_introRoot;

	public TMP_Text m_introTopic;

	public TMP_Text m_introText;

	[Header("Raven")]
	public GameObject m_ravenRoot;

	public TMP_Text m_ravenTopic;

	public TMP_Text m_ravenText;

	private static readonly int s_visibleID = ZSyncAnimation.GetHash("visible");

	private static readonly int s_animatorTagVisible = ZSyncAnimation.GetHash("visible");

	private float m_showTime;

	private bool m_autoHide;

	private Vector3 m_openPlayerPos = Vector3.zero;

	public static TextViewer instance => m_instance;

	private void Awake()
	{
		m_instance = this;
		m_root.SetActive(true);
		m_introRoot.SetActive(true);
		m_ravenRoot.SetActive(true);
		m_animator = m_root.GetComponent<Animator>();
		m_animatorIntro = m_introRoot.GetComponent<Animator>();
		m_animatorRaven = m_ravenRoot.GetComponent<Animator>();
	}

	private void OnDestroy()
	{
		m_instance = null;
	}

	private void LateUpdate()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!IsVisible())
		{
			return;
		}
		m_showTime += Time.deltaTime;
		if (m_showTime > 0.2f)
		{
			if (m_autoHide && Object.op_Implicit((Object)(object)Player.m_localPlayer) && Vector3.Distance(((Component)Player.m_localPlayer).transform.position, m_openPlayerPos) > 3f)
			{
				Hide();
			}
			if (ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("JoyUse") || ZInput.GetKeyDown((KeyCode)27, true))
			{
				Hide();
			}
		}
	}

	public void ShowText(Style style, string topic, string textId, bool autoHide)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Player.m_localPlayer == (Object)null && autoHide))
		{
			topic = Localization.instance.Localize(topic);
			string text = Localization.instance.Localize(textId);
			switch (style)
			{
			case Style.Rune:
				m_topic.text = topic;
				m_text.text = text;
				m_runeText.text = Localization.instance.TranslateSingleId(textId, "English");
				m_animator.SetBool(s_visibleID, true);
				break;
			case Style.Intro:
				m_introTopic.text = topic;
				m_introText.text = text;
				((Component)m_animatorIntro).gameObject.SetActive(true);
				m_animatorIntro.SetTrigger("play");
				ZLog.Log((object)("Show intro " + Time.frameCount));
				break;
			case Style.Raven:
				m_ravenTopic.text = topic;
				m_ravenText.text = text;
				m_animatorRaven.SetBool(s_visibleID, true);
				break;
			}
			m_autoHide = autoHide;
			if (m_autoHide)
			{
				m_openPlayerPos = ((Component)Player.m_localPlayer).transform.position;
			}
			m_showTime = 0f;
			ZLog.Log((object)("Show text " + topic + ":" + text));
		}
	}

	public void Hide()
	{
		m_autoHide = false;
		m_animator.SetBool(s_visibleID, false);
		m_animatorRaven.SetBool(s_visibleID, false);
	}

	public void HideIntro()
	{
		((Component)m_animatorIntro).gameObject.SetActive(false);
	}

	public bool IsVisible()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		AnimatorStateInfo currentAnimatorStateInfo = m_instance.m_animatorIntro.GetCurrentAnimatorStateInfo(0);
		if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).tagHash == s_animatorTagVisible)
		{
			return true;
		}
		if (!m_animator.GetBool(s_visibleID) && !m_animatorIntro.GetBool(s_visibleID))
		{
			return m_animatorRaven.GetBool(s_visibleID);
		}
		return true;
	}

	public static bool IsShowingIntro()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_instance != (Object)null)
		{
			AnimatorStateInfo currentAnimatorStateInfo = m_instance.m_animatorIntro.GetCurrentAnimatorStateInfo(0);
			return ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).tagHash == s_animatorTagVisible;
		}
		return false;
	}
}
