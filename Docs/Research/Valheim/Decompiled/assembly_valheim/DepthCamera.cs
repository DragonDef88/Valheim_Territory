using UnityEngine;

public class DepthCamera : MonoBehaviour
{
	public Shader m_depthShader;

	public float m_offset = 50f;

	public RenderTexture m_texture;

	public float m_updateInterval = 1f;

	private Camera m_camera;

	private void Start()
	{
		m_camera = ((Component)this).GetComponent<Camera>();
		((MonoBehaviour)this).InvokeRepeating("RenderDepth", m_updateInterval, m_updateInterval);
	}

	private void RenderDepth()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = Utils.GetMainCamera();
		if (!((Object)(object)mainCamera == (Object)null))
		{
			Vector3 val = (Object.op_Implicit((Object)(object)Player.m_localPlayer) ? ((Component)Player.m_localPlayer).transform.position : ((Component)mainCamera).transform.position) + Vector3.up * m_offset;
			val.x = Mathf.Round(val.x);
			val.y = Mathf.Round(val.y);
			val.z = Mathf.Round(val.z);
			((Component)this).transform.position = val;
			float lodBias = QualitySettings.lodBias;
			QualitySettings.lodBias = 10f;
			m_camera.RenderWithShader(m_depthShader, "RenderType");
			QualitySettings.lodBias = lodBias;
			Shader.SetGlobalTexture("_SkyAlphaTexture", (Texture)(object)m_texture);
			Shader.SetGlobalVector("_SkyAlphaPosition", Vector4.op_Implicit(((Component)this).transform.position));
		}
	}
}
