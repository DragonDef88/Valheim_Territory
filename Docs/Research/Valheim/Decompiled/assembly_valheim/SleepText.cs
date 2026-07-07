using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SleepText : MonoBehaviour
{
	public TMP_Text m_textField;

	public TMP_Text m_dreamField;

	public DreamTexts m_dreamTexts;

	private void OnEnable()
	{
		((Graphic)m_textField).CrossFadeAlpha(0f, 0f, true);
		((Graphic)m_textField).CrossFadeAlpha(1f, 1f, true);
		((Behaviour)m_dreamField).enabled = false;
		((MonoBehaviour)this).Invoke("CollectResources", 5f);
		((MonoBehaviour)this).Invoke("HideZZZ", 2f);
		((MonoBehaviour)this).Invoke("ShowDreamText", 4f);
	}

	private void HideZZZ()
	{
		((Graphic)m_textField).CrossFadeAlpha(0f, 2f, true);
	}

	private void CollectResources()
	{
		Game.instance.CollectResourcesCheck();
	}

	private void ShowDreamText()
	{
		DreamTexts.DreamText randomDreamText = m_dreamTexts.GetRandomDreamText();
		if (randomDreamText != null)
		{
			m_dreamField.text = Localization.instance.Localize(randomDreamText.m_text);
			((Behaviour)m_dreamField).enabled = true;
			((MonoBehaviour)this).Invoke("DelayedCrossFadeStart", 0.1f);
			((MonoBehaviour)this).Invoke("HideDreamText", 6.5f);
		}
	}

	private void DelayedCrossFadeStart()
	{
		((Graphic)m_dreamField).CrossFadeAlpha(0f, 0f, true);
		((Graphic)m_dreamField).CrossFadeAlpha(1f, 1.5f, true);
	}

	private void HideDreamText()
	{
		((Graphic)m_dreamField).CrossFadeAlpha(0f, 1.5f, true);
	}
}
