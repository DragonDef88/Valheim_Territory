using System;
using System.Collections;
using LlamAcademy.Spring;
using UnityEngine;

public class ConditionalObject : MonoBehaviour, Hoverable
{
	private float m_delayTimer;

	[NonSerialized]
	public float m_dropTimer;

	private SpringVector3 m_scaleSpring;

	private bool m_springActive;

	private Vector3 m_startScale;

	private float m_startHeight;

	public GameObject m_enableObject;

	public string m_hoverName = "Oddity";

	public string m_globalKeyCondition = "";

	public float m_appearDelay;

	public string m_animatorBool;

	public EffectList m_showEffects = new EffectList();

	[Header("Drop Settings")]
	public bool m_dropEnabled;

	public float m_dropHeight = 1f;

	public float m_dropTime = 0.5f;

	public float m_dropTimeVariance;

	private float m_dropTimeActual;

	public AnimationCurve m_dropCurve = AnimationCurve.Linear(0f, 1f, 0f, 1f);

	[Header("Spring Settings")]
	public bool m_springEnabled;

	public float m_springDisableTime = 3f;

	[Min(0f)]
	public float m_springDamping = 8f;

	[Min(0f)]
	public float m_springStiffness = 180f;

	public Vector3 m_startSpringVelocity = new Vector3(1.5f, -1f, 1.5f);

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0078: Expected O, but got Unknown
		m_startScale = m_enableObject.transform.localScale;
		m_startHeight = m_enableObject.transform.position.y;
		SpringVector3 val = new SpringVector3();
		((BaseSpring<Vector3>)val).Damping = m_springDamping;
		((BaseSpring<Vector3>)val).Stiffness = m_springStiffness;
		((BaseSpring<Vector3>)val).StartValue = m_startScale;
		((BaseSpring<Vector3>)val).EndValue = m_startScale;
		((BaseSpring<Vector3>)val).InitialVelocity = m_startSpringVelocity;
		m_scaleSpring = val;
		if (ShouldBeVisible() && !string.IsNullOrEmpty(m_globalKeyCondition))
		{
			m_enableObject.SetActive(true);
			if (!string.IsNullOrEmpty(m_animatorBool))
			{
				Animator componentInChildren = m_enableObject.GetComponentInChildren<Animator>();
				if (componentInChildren != null)
				{
					componentInChildren.SetBool(m_animatorBool, true);
				}
			}
			m_springActive = false;
			m_dropTimer = float.PositiveInfinity;
		}
		else
		{
			m_enableObject.SetActive(false);
		}
		m_dropTimeActual = m_dropTime + Random.Range(0f, m_dropTimeVariance);
	}

	private void Update()
	{
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (!m_enableObject.activeInHierarchy && ShouldBeVisible())
		{
			m_delayTimer += Time.deltaTime;
			if (m_delayTimer > m_appearDelay)
			{
				if (m_dropEnabled)
				{
					m_enableObject.transform.position = m_enableObject.transform.position + Vector3.up * m_dropHeight;
				}
				else if (m_springEnabled)
				{
					ActivateSpring();
				}
				m_enableObject.SetActive(true);
				m_showEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
				if (!string.IsNullOrEmpty(m_animatorBool))
				{
					Animator componentInChildren = m_enableObject.GetComponentInChildren<Animator>();
					if (componentInChildren != null)
					{
						componentInChildren.SetBool(m_animatorBool, true);
					}
					else
					{
						ZLog.LogError((object)("Object '" + ((Object)this).name + "' trying to set animation trigger '" + m_animatorBool + "' but no animator was found!"));
					}
				}
			}
		}
		if (!m_enableObject.activeInHierarchy)
		{
			return;
		}
		if (m_springEnabled && m_springActive)
		{
			m_enableObject.transform.localScale = ((BaseSpring<Vector3>)(object)m_scaleSpring).Evaluate(Time.deltaTime);
		}
		if (m_dropEnabled)
		{
			if (m_dropTimer <= m_dropTimeActual)
			{
				m_dropTimer += Time.deltaTime;
				Vector3 position = m_enableObject.transform.position;
				float num = (1f - m_dropCurve.Evaluate(m_dropTimer / m_dropTimeActual)) * m_dropHeight;
				position.y = m_startHeight + num;
				m_enableObject.transform.position = position;
			}
			if (m_dropTimer > m_dropTimeActual && !m_springActive)
			{
				ActivateSpring();
			}
		}
	}

	private bool ShouldBeVisible()
	{
		if (!string.IsNullOrEmpty(m_globalKeyCondition))
		{
			if (Object.op_Implicit((Object)(object)ZoneSystem.instance))
			{
				return ZoneSystem.instance.GetGlobalKey(m_globalKeyCondition);
			}
			return false;
		}
		return true;
	}

	private void ActivateSpring()
	{
		((MonoBehaviour)this).StartCoroutine(DisableSpring());
		m_springActive = true;
	}

	private IEnumerator DisableSpring()
	{
		yield return (object)new WaitForSeconds(m_springDisableTime);
		m_springActive = false;
		m_enableObject.transform.localScale = m_startScale;
	}

	public string GetHoverText()
	{
		return Localization.instance.Localize(m_hoverName);
	}

	public string GetHoverName()
	{
		return Localization.instance.Localize(m_hoverName);
	}
}
