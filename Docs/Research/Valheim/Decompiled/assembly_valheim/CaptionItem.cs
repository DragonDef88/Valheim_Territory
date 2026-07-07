using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaptionItem : MonoBehaviour
{
	public string m_captionText;

	public ClosedCaptions.CaptionType m_type;

	private TextMeshProUGUI m_text;

	private float m_timer;

	private bool m_dying;

	public bool Killed => m_dying;

	public float TimeSinceSpawn { get; private set; }

	public event Action<CaptionItem> OnDestroyingCaption = delegate
	{
	};

	public void Setup()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		m_text = ((Component)this).GetComponent<TextMeshProUGUI>();
		((Graphic)m_text).color = ClosedCaptions.Instance.GetCaptionColor(m_type);
		((TMP_Text)m_text).text = m_captionText ?? "";
		MonoBehaviour.print((object)Localization.instance);
		Refresh();
	}

	private void OnDestroy()
	{
		this.OnDestroyingCaption?.Invoke(this);
	}

	public void CustomUpdate(float dt)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		m_timer -= dt;
		TimeSinceSpawn += dt;
		if (m_timer <= 0f)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		float num = Mathf.Clamp01(TimeSinceSpawn * 2f);
		float num2 = Mathf.Clamp01(m_timer * 4f);
		float num3 = Mathf.Min(num, num2);
		Vector3 localScale = ((Component)this).transform.localScale;
		localScale.y = num3;
		localScale.x = 1f;
		localScale.z = 1f;
		((Component)this).transform.localScale = localScale;
		((TMP_Text)m_text).alpha = num3;
	}

	public void Refresh()
	{
		if (!m_dying)
		{
			m_timer = ClosedCaptions.Instance.m_captionDuration;
		}
	}

	public void Kill()
	{
		m_dying = true;
		m_timer = Mathf.Min(m_timer, 0.5f);
	}

	public int GetImportance()
	{
		return (int)m_type;
	}
}
