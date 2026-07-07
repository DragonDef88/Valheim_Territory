using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(Camera))]
public class UpscaledFrameBuffer : MonoBehaviour
{
	public static bool m_autoTargetResolution = false;

	public static uint m_targetResolutionVertical = uint.MaxValue;

	public static UpscalingAlgorithm m_upscalingAlgorithm = UpscalingAlgorithm.Bilinear;

	private Camera m_camera;

	private Camera m_clearCamera;

	private RenderTexture m_renderTexture;

	private bool m_isUsingScaledRendering;

	private List<FrameBufferScaler> m_subscribers = new List<FrameBufferScaler>();

	public static bool AutomaticRenderScaleSupported()
	{
		return true;
	}

	public void Subscribe(FrameBufferScaler subscriber)
	{
		m_subscribers.Add(subscriber);
		if ((Object)(object)m_renderTexture != (Object)null)
		{
			subscriber.OnBufferCreated(this, m_renderTexture);
		}
		UpdateCameraTarget();
	}

	public void Unsubscribe(FrameBufferScaler subscriber)
	{
		m_subscribers.Remove(subscriber);
		if ((Object)(object)m_renderTexture != (Object)null)
		{
			subscriber.OnBufferDestroyed(this);
		}
		UpdateCameraTarget();
	}

	private void Update()
	{
		UpdateCameraTarget();
	}

	private void CreateClearCamera()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject();
		val.transform.parent = ((Component)this).transform;
		m_clearCamera = val.AddComponent<Camera>();
		m_clearCamera.cullingMask = 0;
		m_clearCamera.allowHDR = false;
		m_clearCamera.allowMSAA = false;
		m_clearCamera.renderingPath = (RenderingPath)1;
		m_clearCamera.clearFlags = (CameraClearFlags)2;
		m_clearCamera.backgroundColor = Color.black;
	}

	private void DestroyClearCameraIfExists()
	{
		if (!((Object)(object)m_clearCamera == (Object)null))
		{
			Object.Destroy((Object)(object)((Component)m_clearCamera).gameObject);
		}
	}

	private void UpdateCurrentRenderScale()
	{
		if (m_autoTargetResolution)
		{
			uint num = 96u;
			if (Screen.dpi <= (float)num)
			{
				m_targetResolutionVertical = uint.MaxValue;
			}
			else
			{
				m_targetResolutionVertical = (uint)(Screen.height * (int)num) / (uint)Screen.dpi;
			}
		}
	}

	private static Resolution GetHighestSupportedResolution()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Resolution[] resolutions = Screen.resolutions;
		int num = 0;
		int num2 = ((Resolution)(ref resolutions[num])).width * ((Resolution)(ref resolutions[num])).height;
		for (int i = 1; i < resolutions.Length; i++)
		{
			int num3 = ((Resolution)(ref resolutions[i])).width * ((Resolution)(ref resolutions[i])).height;
			if (num3 > num2)
			{
				num = i;
				num2 = num3;
			}
		}
		return resolutions[num];
	}

	private void UpdateCameraTarget()
	{
		if ((Object)(object)m_camera == (Object)null)
		{
			m_camera = ((Component)this).GetComponent<Camera>();
		}
		UpdateCurrentRenderScale();
		bool flag = m_targetResolutionVertical < Screen.height && m_subscribers.Count > 0;
		if (flag)
		{
			if (!m_isUsingScaledRendering)
			{
				CreateClearCamera();
			}
			ReassignTextureIfNeeded();
		}
		else if (m_isUsingScaledRendering)
		{
			ReleaseTextureIfExists();
			DestroyClearCameraIfExists();
		}
		m_isUsingScaledRendering = flag;
	}

	private FilterMode GetFilterModeFromUpscalingMode()
	{
		UpscalingAlgorithm upscalingAlgorithm = m_upscalingAlgorithm;
		if (upscalingAlgorithm != 0 && upscalingAlgorithm == UpscalingAlgorithm.NearestNeighbor)
		{
			return (FilterMode)0;
		}
		return (FilterMode)1;
	}

	private void ReassignTextureIfNeeded()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = default(Vector2Int);
		((Vector2Int)(ref val))._002Ector((int)((uint)((int)m_targetResolutionVertical * Screen.width) / (uint)Screen.height), (int)m_targetResolutionVertical);
		if (((Vector2Int)(ref val)).x < 8 || ((Vector2Int)(ref val)).y < 8)
		{
			if (((Vector2Int)(ref val)).y < ((Vector2Int)(ref val)).x)
			{
				((Vector2Int)(ref val))._002Ector((int)((uint)(8 * Screen.width) / (uint)Screen.height), 8);
			}
			else
			{
				((Vector2Int)(ref val))._002Ector(8, (int)((uint)(8 * Screen.height) / (uint)Screen.width));
			}
		}
		if ((Object)(object)m_renderTexture == (Object)null || val != new Vector2Int(((Texture)m_renderTexture).width, ((Texture)m_renderTexture).height) || ((Texture)m_renderTexture).filterMode != GetFilterModeFromUpscalingMode())
		{
			RecreateAndAssignRenderTexture(val);
		}
	}

	private void RecreateAndAssignRenderTexture(Vector2Int viewportResolution)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		ReleaseTextureIfExists();
		m_renderTexture = new RenderTexture(((Vector2Int)(ref viewportResolution)).x, ((Vector2Int)(ref viewportResolution)).y, 24, (DefaultFormat)1);
		m_renderTexture.Create();
		((Texture)m_renderTexture).filterMode = GetFilterModeFromUpscalingMode();
		m_camera.targetTexture = m_renderTexture;
		for (int i = 0; i < m_subscribers.Count; i++)
		{
			m_subscribers[i].OnBufferCreated(this, m_renderTexture);
		}
	}

	private void ReleaseTextureIfExists()
	{
		if (!((Object)(object)m_renderTexture == (Object)null))
		{
			for (int i = 0; i < m_subscribers.Count; i++)
			{
				m_subscribers[i].OnBufferDestroyed(this);
			}
			m_camera.targetTexture = null;
			m_renderTexture.Release();
			m_renderTexture = null;
		}
	}

	private void OnDestroy()
	{
		ReleaseTextureIfExists();
		DestroyClearCameraIfExists();
	}
}
