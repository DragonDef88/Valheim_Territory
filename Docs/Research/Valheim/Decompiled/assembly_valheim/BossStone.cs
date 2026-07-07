using System.Collections;
using UnityEngine;

public class BossStone : MonoBehaviour
{
	public ItemStand m_itemStand;

	public GameObject m_activeEffect;

	public EffectList m_activateStep1 = new EffectList();

	public EffectList m_activateStep2 = new EffectList();

	public EffectList m_activateStep3 = new EffectList();

	public string m_completedMessage = "";

	public MeshRenderer m_mesh;

	public int m_emissiveMaterialIndex;

	public Color m_activeEmissiveColor = Color.white;

	private bool m_active;

	private ZNetView m_nview;

	private void Start()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (((Renderer)m_mesh).materials[m_emissiveMaterialIndex].HasProperty("_EmissionColor"))
		{
			((Renderer)m_mesh).materials[m_emissiveMaterialIndex].SetColor("_EmissionColor", Color.black);
		}
		if (Object.op_Implicit((Object)(object)m_activeEffect))
		{
			m_activeEffect.SetActive(false);
		}
		SetActivated(m_itemStand.HaveAttachment(), triggerEffect: false);
		((MonoBehaviour)this).InvokeRepeating("UpdateVisual", 1f, 1f);
	}

	private void UpdateVisual()
	{
		SetActivated(m_itemStand.HaveAttachment(), triggerEffect: true);
	}

	private void SetActivated(bool active, bool triggerEffect)
	{
		if (active == m_active)
		{
			return;
		}
		m_active = active;
		if (triggerEffect && active)
		{
			((MonoBehaviour)this).Invoke("DelayedAttachEffects_Step1", 1f);
			((MonoBehaviour)this).Invoke("DelayedAttachEffects_Step2", 5f);
			((MonoBehaviour)this).Invoke("DelayedAttachEffects_Step3", 11f);
			return;
		}
		if (Object.op_Implicit((Object)(object)m_activeEffect))
		{
			m_activeEffect.SetActive(active);
		}
		((MonoBehaviour)this).StopCoroutine("FadeEmission");
		((MonoBehaviour)this).StartCoroutine("FadeEmission");
	}

	private void DelayedAttachEffects_Step1()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		m_activateStep1.Create(((Component)m_itemStand).transform.position, ((Component)this).transform.rotation);
	}

	private void DelayedAttachEffects_Step2()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		m_activateStep2.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
	}

	private void DelayedAttachEffects_Step3()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_activeEffect))
		{
			m_activeEffect.SetActive(true);
		}
		m_activateStep3.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
		((MonoBehaviour)this).StopCoroutine("FadeEmission");
		((MonoBehaviour)this).StartCoroutine("FadeEmission");
		Player.MessageAllInRange(((Component)this).transform.position, 20f, MessageHud.MessageType.Center, m_completedMessage);
	}

	private IEnumerator FadeEmission()
	{
		if (Object.op_Implicit((Object)(object)m_mesh) && ((Renderer)m_mesh).materials[m_emissiveMaterialIndex].HasProperty("_EmissionColor"))
		{
			Color startColor = ((Renderer)m_mesh).materials[m_emissiveMaterialIndex].GetColor("_EmissionColor");
			Color targetColor = (m_active ? m_activeEmissiveColor : Color.black);
			for (float t = 0f; t < 1f; t += Time.deltaTime)
			{
				Color val = Color.Lerp(startColor, targetColor, t / 1f);
				((Renderer)m_mesh).materials[m_emissiveMaterialIndex].SetColor("_EmissionColor", val);
				yield return null;
			}
		}
		ZLog.Log((object)"Done fading color");
	}

	public bool IsActivated()
	{
		return m_active;
	}
}
