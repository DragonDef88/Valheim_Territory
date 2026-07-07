using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class ShieldDomeImageEffect : MonoBehaviour
{
	[Serializable]
	public struct ShieldDome
	{
		public Vector3 position;

		public float radius;

		[FormerlySerializedAs("health")]
		[Range(0f, 1f)]
		public float fuelFactor;

		public float lastHitTime;
	}

	private static class Uniforms
	{
		internal static readonly int _TopLeft = Shader.PropertyToID("_TopLeft");

		internal static readonly int _TopRight = Shader.PropertyToID("_TopRight");

		internal static readonly int _BottomLeft = Shader.PropertyToID("_BottomLeft");

		internal static readonly int _BottomRight = Shader.PropertyToID("_BottomRight");

		internal static readonly int _Smoothing = Shader.PropertyToID("_Smoothing");

		internal static readonly int _DepthFade = Shader.PropertyToID("_DepthFade");

		internal static readonly int _EdgeGlow = Shader.PropertyToID("_EdgeGlow");

		internal static readonly int _DrawDistance = Shader.PropertyToID("_DrawDistance");

		internal static readonly int _DomeBuffer = Shader.PropertyToID("_DomeBuffer");

		internal static readonly int _DomeCount = Shader.PropertyToID("_DomeCount");

		internal static readonly int _MaxSteps = Shader.PropertyToID("_MaxSteps");

		internal static readonly int _SurfaceDistance = Shader.PropertyToID("_SurfaceDistance");

		internal static readonly int _NormalBias = Shader.PropertyToID("_NormalBias");

		internal static readonly int _RefractStrength = Shader.PropertyToID("_RefractStrength");

		internal static readonly int _ShieldColorGradient = Shader.PropertyToID("_ShieldColorGradient");

		internal static readonly int _ShieldTime = Shader.PropertyToID("_ShieldTime");

		internal static readonly int _NoiseTexture = Shader.PropertyToID("_NoiseTexture");

		internal static readonly int _CurlNoiseTexture = Shader.PropertyToID("_CurlNoiseTexture");

		internal static readonly int _NoiseSize = Shader.PropertyToID("_NoiseSize");

		internal static readonly int _CurlNoiseSize = Shader.PropertyToID("_CurlNoiseSize");

		internal static readonly int _CurlNoiseStrength = Shader.PropertyToID("_CurlNoiseStrength");
	}

	private int m_ShieldDomeStride = 24;

	private Material m_effectMaterial;

	private Camera m_cam;

	private ComputeBuffer m_shieldDomeBuffer;

	private Texture2D m_gradientTex;

	private static Gradient s_staticGradient;

	public static float Smoothing;

	[Min(0.1f)]
	public float m_smoothing = 0.25f;

	public float m_depthFadeDistance = 3f;

	[Min(0.25f)]
	public float m_edgeGlowDistance = 1f;

	public Gradient m_shieldColorGradient;

	[Range(-10f, 10f)]
	public float m_refractStrength = 1f;

	private ShieldDome[] m_shieldDomes;

	private Dictionary<ShieldGenerator, ShieldDome> m_shieldDomeData = new Dictionary<ShieldGenerator, ShieldDome>();

	[Header("Textures")]
	public Texture2D m_noiseTexture;

	public float m_noiseSize = 15f;

	public Texture3D m_curlNoiseTexture;

	public float m_curlSize;

	public float m_curlStrength;

	[Header("Quality")]
	[Range(0f, 0.2f)]
	public float m_surfaceDistance = 0.001f;

	[Range(0f, 0.5f)]
	public float m_normalBias = 0.1f;

	public float m_drawDistance = 100f;

	private void Awake()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		m_effectMaterial = new Material(Shader.Find("Hidden/ShieldDomePass"));
		m_gradientTex = new Texture2D(256, 1, (TextureFormat)3, false);
		((Texture)m_gradientTex).wrapMode = (TextureWrapMode)1;
		for (int i = 0; i < 256; i++)
		{
			Color val = m_shieldColorGradient.Evaluate((float)i / 256f);
			m_gradientTex.SetPixel(i, 0, val);
		}
		m_gradientTex.Apply();
		s_staticGradient = m_shieldColorGradient;
		Smoothing = m_smoothing;
		ToggleActiveImageEffect(m_shieldDomeData.Count);
	}

	[ImageEffectAllowedInSceneView]
	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		if (m_shieldDomes == null || m_shieldDomes.Length == 0)
		{
			Graphics.Blit((Texture)(object)src, dest);
			return;
		}
		if ((Object)(object)m_cam == (Object)null)
		{
			m_cam = Camera.main;
		}
		Ray val = m_cam.ViewportPointToRay(new Vector3(0f, 1f, 0f));
		Vector3 direction = ((Ray)(ref val)).direction;
		val = m_cam.ViewportPointToRay(new Vector3(1f, 1f, 0f));
		Vector3 direction2 = ((Ray)(ref val)).direction;
		val = m_cam.ViewportPointToRay(new Vector3(0f, 0f, 0f));
		Vector3 direction3 = ((Ray)(ref val)).direction;
		val = m_cam.ViewportPointToRay(new Vector3(1f, 0f, 0f));
		Vector3 direction4 = ((Ray)(ref val)).direction;
		m_effectMaterial.SetVector(Uniforms._TopLeft, Vector4.op_Implicit(direction));
		m_effectMaterial.SetVector(Uniforms._TopRight, Vector4.op_Implicit(direction2));
		m_effectMaterial.SetVector(Uniforms._BottomLeft, Vector4.op_Implicit(direction3));
		m_effectMaterial.SetVector(Uniforms._BottomRight, Vector4.op_Implicit(direction4));
		m_effectMaterial.SetFloat(Uniforms._Smoothing, m_smoothing);
		m_effectMaterial.SetFloat(Uniforms._DepthFade, m_depthFadeDistance);
		m_effectMaterial.SetFloat(Uniforms._EdgeGlow, m_edgeGlowDistance);
		m_effectMaterial.SetFloat(Uniforms._DrawDistance, m_drawDistance);
		m_effectMaterial.SetTexture(Uniforms._ShieldColorGradient, (Texture)(object)m_gradientTex);
		m_effectMaterial.SetFloat(Uniforms._ShieldTime, Time.time);
		m_effectMaterial.SetTexture(Uniforms._NoiseTexture, (Texture)(object)m_noiseTexture);
		m_effectMaterial.SetTexture(Uniforms._CurlNoiseTexture, (Texture)(object)m_curlNoiseTexture);
		m_effectMaterial.SetFloat(Uniforms._NoiseSize, m_noiseSize);
		m_effectMaterial.SetFloat(Uniforms._CurlNoiseSize, m_curlSize);
		m_effectMaterial.SetFloat(Uniforms._CurlNoiseStrength, m_curlStrength);
		int num = 32 + GraphicsSettingsManager.Instance.GetCurrentSettingsWithCurrentPresetApplied(includeBackground: true).m_lod * 32;
		m_effectMaterial.SetInt(Uniforms._MaxSteps, num);
		m_effectMaterial.SetFloat(Uniforms._SurfaceDistance, m_surfaceDistance);
		m_effectMaterial.SetFloat(Uniforms._NormalBias, m_normalBias);
		m_effectMaterial.SetFloat(Uniforms._RefractStrength, m_refractStrength);
		PrepareComputeBuffer();
		Graphics.Blit((Texture)(object)src, dest, m_effectMaterial, 0);
	}

	private void PrepareComputeBuffer()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		if (m_shieldDomes == null || m_shieldDomes.Length == 0)
		{
			ComputeBuffer shieldDomeBuffer = m_shieldDomeBuffer;
			if (shieldDomeBuffer != null)
			{
				shieldDomeBuffer.Release();
			}
			return;
		}
		if (m_shieldDomeBuffer == null || m_shieldDomeBuffer.count != m_shieldDomes.Length)
		{
			ComputeBuffer shieldDomeBuffer2 = m_shieldDomeBuffer;
			if (shieldDomeBuffer2 != null)
			{
				shieldDomeBuffer2.Release();
			}
			m_shieldDomeBuffer = new ComputeBuffer(m_shieldDomes.Length, m_ShieldDomeStride, (ComputeBufferType)16);
		}
		m_shieldDomeBuffer.SetData((Array)m_shieldDomes);
		m_effectMaterial.SetBuffer(Uniforms._DomeBuffer, m_shieldDomeBuffer);
		m_effectMaterial.SetInt(Uniforms._DomeCount, m_shieldDomes.Length);
	}

	public void SetShieldData(ShieldGenerator shield, Vector3 position, float radius, float fuelFactor, float lastHitTime)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_shieldDomeData[shield] = new ShieldDome
		{
			position = position,
			radius = radius,
			fuelFactor = fuelFactor,
			lastHitTime = lastHitTime
		};
		ToggleActiveImageEffect(m_shieldDomeData.Count);
		updateShieldData();
	}

	public void RemoveShield(ShieldGenerator shield)
	{
		if (m_shieldDomeData.Remove(shield))
		{
			ToggleActiveImageEffect(m_shieldDomeData.Count);
			updateShieldData();
		}
	}

	private void updateShieldData()
	{
		if (m_shieldDomes == null || m_shieldDomes.Length != m_shieldDomeData.Count)
		{
			m_shieldDomes = new ShieldDome[m_shieldDomeData.Count];
		}
		int num = 0;
		foreach (KeyValuePair<ShieldGenerator, ShieldDome> shieldDomeDatum in m_shieldDomeData)
		{
			m_shieldDomes[num++] = shieldDomeDatum.Value;
		}
	}

	private void ToggleActiveImageEffect(int domes)
	{
		((Behaviour)this).enabled = domes > 0;
	}

	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			Object.Destroy((Object)(object)m_effectMaterial);
		}
		else
		{
			Object.DestroyImmediate((Object)(object)m_effectMaterial);
		}
	}

	public static Color GetDomeColor(float fuelFactor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return s_staticGradient.Evaluate(fuelFactor);
	}
}
