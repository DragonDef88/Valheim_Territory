using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightLod : MonoBehaviour
{
	private static HashSet<LightLod> m_lights = new HashSet<LightLod>();

	private static List<LightLod> m_sortedLights = new List<LightLod>();

	public static int m_lightLimit = -1;

	public static int m_shadowLimit = -1;

	public bool m_lightLod = true;

	public float m_lightDistance = 40f;

	public bool m_shadowLod = true;

	public float m_shadowDistance = 20f;

	private const float m_lightSizeWeight = 0.25f;

	private static float m_updateTimer = 0f;

	private static readonly WaitForSeconds s_waitFor1Sec = new WaitForSeconds(1f);

	private int m_lightPrio;

	private float m_cameraDistanceOuter;

	private Light m_light;

	private float m_baseRange;

	private float m_baseShadowStrength;

	private void Awake()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		m_light = ((Component)this).GetComponent<Light>();
		m_baseRange = m_light.range;
		m_baseShadowStrength = m_light.shadowStrength;
		if (m_shadowLod && (int)m_light.shadows == 0)
		{
			m_shadowLod = false;
		}
		if (m_lightLod)
		{
			m_light.range = 0f;
			((Behaviour)m_light).enabled = false;
		}
		if (m_shadowLod)
		{
			m_light.shadowStrength = 0f;
			m_light.shadows = (LightShadows)0;
		}
		m_lights.Add(this);
	}

	private void OnEnable()
	{
		((MonoBehaviour)this).StartCoroutine("UpdateLoop");
	}

	private void OnDisable()
	{
		((MonoBehaviour)this).StopCoroutine("UpdateLoop");
	}

	private void OnDestroy()
	{
		m_lights.Remove(this);
	}

	private IEnumerator UpdateLoop()
	{
		while (true)
		{
			if (Utils.GetMainCamera() != null && Object.op_Implicit((Object)(object)m_light))
			{
				Vector3 lightReferencePoint = GetLightReferencePoint();
				float distance = Vector3.Distance(lightReferencePoint, ((Component)this).transform.position);
				if (m_lightLod)
				{
					if (distance < m_lightDistance && (m_lightPrio < m_lightLimit || m_lightLimit < 0))
					{
						while (Object.op_Implicit((Object)(object)m_light) && (m_light.range < m_baseRange || !((Behaviour)m_light).enabled))
						{
							((Behaviour)m_light).enabled = true;
							m_light.range = Mathf.Min(m_baseRange, m_light.range + Time.deltaTime * m_baseRange);
							yield return null;
						}
					}
					else
					{
						while (Object.op_Implicit((Object)(object)m_light) && (m_light.range > 0f || ((Behaviour)m_light).enabled))
						{
							m_light.range = Mathf.Max(0f, m_light.range - Time.deltaTime * m_baseRange);
							if (m_light.range <= 0f)
							{
								((Behaviour)m_light).enabled = false;
							}
							yield return null;
						}
					}
				}
				if (m_shadowLod)
				{
					if (distance < m_shadowDistance && (m_lightPrio < m_shadowLimit || m_shadowLimit < 0))
					{
						while (Object.op_Implicit((Object)(object)m_light) && (m_light.shadowStrength < m_baseShadowStrength || (int)m_light.shadows == 0))
						{
							m_light.shadows = (LightShadows)2;
							m_light.shadowStrength = Mathf.Min(m_baseShadowStrength, m_light.shadowStrength + Time.deltaTime * m_baseShadowStrength);
							yield return null;
						}
					}
					else
					{
						while (Object.op_Implicit((Object)(object)m_light) && (m_light.shadowStrength > 0f || (int)m_light.shadows != 0))
						{
							m_light.shadowStrength = Mathf.Max(0f, m_light.shadowStrength - Time.deltaTime * m_baseShadowStrength);
							if (m_light.shadowStrength <= 0f)
							{
								m_light.shadows = (LightShadows)0;
							}
							yield return null;
						}
					}
				}
			}
			yield return s_waitFor1Sec;
		}
	}

	private static Vector3 GetLightReferencePoint()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if (GameCamera.InFreeFly() || Player.m_localPlayer == null)
		{
			return ((Component)mainCamera).transform.position;
		}
		return ((Component)Player.m_localPlayer).transform.position;
	}

	public static void UpdateLights(float dt)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Invalid comparison between Unknown and I4
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (m_lightLimit < 0 && m_shadowLimit < 0)
		{
			return;
		}
		m_updateTimer += dt;
		if (m_updateTimer < 1f)
		{
			return;
		}
		m_updateTimer = 0f;
		if ((Object)(object)Utils.GetMainCamera() == (Object)null)
		{
			return;
		}
		Vector3 lightReferencePoint = GetLightReferencePoint();
		m_sortedLights.Clear();
		foreach (LightLod light in m_lights)
		{
			if (((Behaviour)light).enabled && Object.op_Implicit((Object)(object)light.m_light) && (int)light.m_light.type == 2)
			{
				light.m_cameraDistanceOuter = Vector3.Distance(lightReferencePoint, ((Component)light).transform.position) - light.m_lightDistance * 0.25f;
				m_sortedLights.Add(light);
			}
		}
		m_sortedLights.Sort((LightLod a, LightLod b) => a.m_cameraDistanceOuter.CompareTo(b.m_cameraDistanceOuter));
		for (int i = 0; i < m_sortedLights.Count; i++)
		{
			m_sortedLights[i].m_lightPrio = i;
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (m_lightLod)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(((Component)this).transform.position, m_lightDistance);
		}
		if (m_shadowLod)
		{
			Gizmos.color = Color.grey;
			Gizmos.DrawWireSphere(((Component)this).transform.position, m_shadowDistance);
		}
	}
}
