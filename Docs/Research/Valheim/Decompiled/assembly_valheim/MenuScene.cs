using UnityEngine;

[ExecuteInEditMode]
public class MenuScene : MonoBehaviour
{
	public Light m_dirLight;

	public Color m_sunFogColor = Color.white;

	public Color m_fogColor = Color.white;

	public Color m_ambientLightColor = Color.white;

	public float m_fogDensity = 1f;

	public Vector3 m_windDir = Vector3.left;

	public float m_windIntensity = 0.5f;

	private void Awake()
	{
		Shader.SetGlobalFloat("_Wet", 0f);
	}

	private void Update()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalVector("_SkyboxSunDir", Vector4.op_Implicit(-((Component)m_dirLight).transform.forward));
		Shader.SetGlobalVector("_SunDir", Vector4.op_Implicit(-((Component)m_dirLight).transform.forward));
		Shader.SetGlobalColor("_SunFogColor", m_sunFogColor);
		Shader.SetGlobalColor("_SunColor", m_dirLight.color * m_dirLight.intensity);
		Shader.SetGlobalColor("_AmbientColor", RenderSettings.ambientLight);
		RenderSettings.fogColor = m_fogColor;
		RenderSettings.fogDensity = m_fogDensity;
		RenderSettings.ambientLight = m_ambientLightColor;
		Vector3 normalized = ((Vector3)(ref m_windDir)).normalized;
		Shader.SetGlobalVector("_GlobalWindForce", Vector4.op_Implicit(normalized * m_windIntensity));
		Shader.SetGlobalVector("_GlobalWind1", new Vector4(normalized.x, normalized.y, normalized.z, m_windIntensity));
		Shader.SetGlobalVector("_GlobalWind2", Vector4.one);
		Shader.SetGlobalFloat("_GlobalWindAlpha", 0f);
	}
}
