using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Valheim.UI;

public class GroupElement : RadialMenuElement
{
	protected Coroutine m_colorChangeCoroutine;

	public void Init(IRadialConfig config, IRadialConfig backConfig, RadialBase radial)
	{
		if (config == null)
		{
			base.Name = "";
			base.Interact = null;
		}
		else
		{
			base.Name = config.LocalizedName;
			base.Interact = delegate
			{
				radial.QueuedOpen(config, backConfig);
				return true;
			};
		}
		((Component)m_icon).gameObject.SetActive((Object)(object)config.Sprite != (Object)null);
		m_icon.sprite = config.Sprite;
	}

	public virtual void ChangeToSelectColor()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (m_colorChangeCoroutine != null)
		{
			((MonoBehaviour)Hud.instance).StopCoroutine(m_colorChangeCoroutine);
		}
		m_colorChangeCoroutine = ((MonoBehaviour)Hud.instance).StartCoroutine(ChangeColor(base.BackgroundMaterial.GetColor("_SelectedColor"), 0.1f));
	}

	public virtual void ChangeToDeselectColor()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (m_colorChangeCoroutine != null)
		{
			((MonoBehaviour)Hud.instance).StopCoroutine(m_colorChangeCoroutine);
		}
		m_colorChangeCoroutine = ((MonoBehaviour)Hud.instance).StartCoroutine(ChangeColor(Color.white, 0.1f));
	}

	protected IEnumerator ChangeColor(Color targetColor, float speed)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_icon == (Object)null))
		{
			float alpha = 0f;
			float duration = 0f;
			Color startColor = ((Graphic)m_icon).color;
			while ((Object)(object)m_icon != (Object)null && duration <= speed + 0.1f)
			{
				((Graphic)m_icon).color = Color.Lerp(startColor, targetColor, alpha);
				duration += Time.deltaTime;
				alpha = Mathf.Clamp01(duration / speed);
				yield return null;
			}
		}
	}

	protected void OnDisable()
	{
		if (m_colorChangeCoroutine != null)
		{
			((MonoBehaviour)Hud.instance).StopCoroutine(m_colorChangeCoroutine);
		}
	}
}
