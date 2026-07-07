using GUIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Feedback : MonoBehaviour
{
	private static Feedback m_instance;

	public GuiInputField m_subject;

	public GuiInputField m_text;

	public Button m_sendButton;

	public Toggle m_catBug;

	public Toggle m_catFeedback;

	public Toggle m_catIdea;

	private void Awake()
	{
		m_instance = this;
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_instance == (Object)(object)this)
		{
			m_instance = null;
		}
	}

	public static bool IsVisible()
	{
		return (Object)(object)m_instance != (Object)null;
	}

	private void LateUpdate()
	{
		((Selectable)m_sendButton).interactable = IsValid();
		if (IsVisible() && (ZInput.GetKeyDown((KeyCode)27, true) || (ZInput.GetButtonDown("JoyMenu") && (!ZInput.GetButton("JoyLTrigger") || !ZInput.GetButton("JoyLBumper")))))
		{
			OnBack();
		}
	}

	private bool IsValid()
	{
		if (((TMP_InputField)m_subject).text.Length == 0)
		{
			return false;
		}
		if (((TMP_InputField)m_text).text.Length == 0)
		{
			return false;
		}
		return true;
	}

	public void OnBack()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void OnSend()
	{
		if (IsValid())
		{
			string category = GetCategory();
			Gogan.LogEvent("Feedback_" + category, ((TMP_InputField)m_subject).text, ((TMP_InputField)m_text).text, 0L);
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private string GetCategory()
	{
		if (m_catBug.isOn)
		{
			return "Bug";
		}
		if (m_catFeedback.isOn)
		{
			return "Feedback";
		}
		if (m_catIdea.isOn)
		{
			return "Idea";
		}
		return "";
	}
}
