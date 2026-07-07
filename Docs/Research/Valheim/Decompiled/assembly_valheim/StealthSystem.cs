using UnityEngine;

public class StealthSystem : MonoBehaviour
{
	private static StealthSystem m_instance;

	public LayerMask m_shadowTestMask;

	public float m_minLightLevel = 0.2f;

	public float m_maxLightLevel = 1.6f;

	private Light[] m_allLights;

	private float m_lastLightListUpdate;

	private const float m_lightUpdateInterval = 1f;

	public static StealthSystem instance => m_instance;

	private void Awake()
	{
		m_instance = this;
	}

	private void OnDestroy()
	{
		m_instance = null;
	}

	public float GetLightFactor(Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		float lightLevel = GetLightLevel(point);
		return Utils.LerpStep(m_minLightLevel, m_maxLightLevel, lightLevel);
	}

	public float GetLightLevel(Vector3 point)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time - m_lastLightListUpdate > 1f)
		{
			m_lastLightListUpdate = Time.time;
			m_allLights = Object.FindObjectsOfType<Light>();
		}
		float ambientIntensity = RenderSettings.ambientIntensity;
		Color val = RenderSettings.ambientLight;
		float num = ambientIntensity * ((Color)(ref val)).grayscale;
		Light[] allLights = m_allLights;
		foreach (Light val2 in allLights)
		{
			if ((Object)(object)val2 == (Object)null)
			{
				continue;
			}
			if ((int)val2.type == 1)
			{
				float num2 = 1f;
				if ((int)val2.shadows != 0 && (Physics.Raycast(point - ((Component)val2).transform.forward * 1000f, ((Component)val2).transform.forward, 1000f, LayerMask.op_Implicit(m_shadowTestMask)) || Physics.Raycast(point, -((Component)val2).transform.forward, 1000f, LayerMask.op_Implicit(m_shadowTestMask))))
				{
					num2 = 1f - val2.shadowStrength;
				}
				float intensity = val2.intensity;
				val = val2.color;
				float num3 = intensity * ((Color)(ref val)).grayscale * num2;
				num += num3;
				continue;
			}
			float num4 = Vector3.Distance(((Component)val2).transform.position, point);
			if (num4 > val2.range)
			{
				continue;
			}
			float num5 = 1f;
			if ((int)val2.shadows != 0)
			{
				Vector3 val3 = point - ((Component)val2).transform.position;
				if (Physics.Raycast(((Component)val2).transform.position, ((Vector3)(ref val3)).normalized, ((Vector3)(ref val3)).magnitude, LayerMask.op_Implicit(m_shadowTestMask)) || Physics.Raycast(point, -((Vector3)(ref val3)).normalized, ((Vector3)(ref val3)).magnitude, LayerMask.op_Implicit(m_shadowTestMask)))
				{
					num5 = 1f - val2.shadowStrength;
				}
			}
			float num6 = 1f - num4 / val2.range;
			float intensity2 = val2.intensity;
			val = val2.color;
			float num7 = intensity2 * ((Color)(ref val)).grayscale * num6 * num5;
			num += num7;
		}
		return num;
	}
}
