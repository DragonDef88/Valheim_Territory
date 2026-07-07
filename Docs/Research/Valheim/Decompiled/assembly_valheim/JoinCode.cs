using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinCode : MonoBehaviour
{
	public static JoinCode m_instance;

	public GameObject m_root;

	public Button m_btn;

	public TMP_Text m_text;

	public CanvasRenderer m_darken;

	public float m_firstShowDuration = 7f;

	public float m_fadeOutDuration = 3f;

	private bool m_initialized;

	private string m_joinCode = "";

	private float m_textAlpha;

	private float m_darkenAlpha;

	private float m_isVisible;

	private bool m_inMenu;

	private bool m_inputBlocked;

	public static void Show(bool firstSpawn = false)
	{
		if ((Object)(object)m_instance != (Object)null)
		{
			m_instance.Activate(firstSpawn);
		}
	}

	public static void Hide()
	{
		if ((Object)(object)m_instance != (Object)null)
		{
			m_instance.Deactivate();
		}
	}

	private void Start()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		m_instance = this;
		m_textAlpha = ((Graphic)m_text).color.a;
		m_darkenAlpha = m_darken.GetAlpha();
		Deactivate();
	}

	private void Init()
	{
		if (!m_initialized)
		{
			if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
			{
				m_joinCode = ZPlayFabMatchmaking.JoinCode;
				m_root.SetActive(m_joinCode.Length > 0);
			}
			else
			{
				m_root.SetActive(false);
			}
			m_initialized = true;
		}
	}

	private void Activate(bool firstSpawn)
	{
		Init();
		if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
		{
			m_joinCode = ZPlayFabMatchmaking.JoinCode;
		}
		ResetAlpha();
		m_root.SetActive(m_joinCode.Length > 0);
		m_inMenu = !firstSpawn;
		m_isVisible = (firstSpawn ? m_firstShowDuration : 0f);
	}

	public void Deactivate()
	{
		m_root.SetActive(false);
		m_inMenu = false;
		m_isVisible = 0f;
	}

	private void ResetAlpha()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((Graphic)m_text).color;
		color.a = m_textAlpha;
		((Graphic)m_text).color = color;
		m_darken.SetAlpha(m_darkenAlpha);
	}

	private void Update()
	{
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		if (!m_inMenu && !(m_isVisible > 0f))
		{
			return;
		}
		((Component)m_btn).gameObject.GetComponentInChildren<TMP_Text>().text = Localization.instance.Localize("$menu_joincode", new string[1] { m_joinCode });
		if (m_inMenu)
		{
			if ((Object)(object)Settings.instance == (Object)null && ((Object)(object)Menu.instance == (Object)null || (!((Component)Menu.instance.m_logoutDialog).gameObject.activeSelf && !Menu.instance.PlayerListActive)) && m_inputBlocked)
			{
				m_inputBlocked = false;
				return;
			}
			m_inputBlocked = (Object)(object)Settings.instance != (Object)null || ((Object)(object)Menu.instance != (Object)null && (((Component)Menu.instance.m_logoutDialog).gameObject.activeSelf || Menu.instance.PlayerListActive));
			if (!m_inputBlocked && (Object)(object)Settings.instance == (Object)null && !string.IsNullOrEmpty(ZPlayFabMatchmaking.JoinCode) && (ZInput.GetButtonDown("JoyButtonX") || ZInput.GetKeyDown((KeyCode)106, true)))
			{
				CopyJoinCodeToClipboard();
			}
			return;
		}
		m_isVisible -= Time.deltaTime;
		if (m_isVisible < 0f)
		{
			Hide();
		}
		else if (m_isVisible < m_fadeOutDuration)
		{
			float num = m_isVisible / m_fadeOutDuration;
			float a = Mathf.Lerp(0f, m_textAlpha, num);
			float alpha = Mathf.Lerp(0f, m_darkenAlpha, num);
			Color color = ((Graphic)m_text).color;
			color.a = a;
			((Graphic)m_text).color = color;
			m_darken.SetAlpha(alpha);
		}
	}

	public void OnClick()
	{
		CopyJoinCodeToClipboard();
	}

	private void CopyJoinCodeToClipboard()
	{
		Gogan.LogEvent("Screen", "CopyToClipboard", "JoinCode", 0L);
		GUIUtility.systemCopyBuffer = m_joinCode;
		if ((Object)(object)MessageHud.instance != (Object)null)
		{
			MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, "$menu_joincode_copied");
		}
	}
}
