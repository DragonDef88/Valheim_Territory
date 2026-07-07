using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class FrameBufferScaler : MonoBehaviour
{
	private RawImage m_rawImage;

	private UpscaledFrameBuffer m_virtualFrameBuffer;

	private void Start()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		m_rawImage = ((Component)this).GetComponent<RawImage>();
		((Graphic)m_rawImage).raycastTarget = false;
		((MaskableGraphic)m_rawImage).maskable = false;
		((Graphic)m_rawImage).color = new Color(1f, 1f, 1f, 0f);
		m_virtualFrameBuffer = Object.FindAnyObjectByType<UpscaledFrameBuffer>();
		if ((Object)(object)m_virtualFrameBuffer == (Object)null)
		{
			ZLog.LogError((object)"Failed to find UpscaledFrameBuffer");
		}
		else
		{
			m_virtualFrameBuffer.Subscribe(this);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)m_virtualFrameBuffer != (Object)null)
		{
			m_virtualFrameBuffer.Unsubscribe(this);
		}
	}

	public void OnBufferCreated(UpscaledFrameBuffer virtualFrameBuffer, RenderTexture texture)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		m_rawImage.texture = (Texture)(object)texture;
		((Graphic)m_rawImage).color = new Color(1f, 1f, 1f, 1f);
	}

	public void OnBufferDestroyed(UpscaledFrameBuffer virtualFrameBuffer)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		m_rawImage.texture = null;
		((Graphic)m_rawImage).color = new Color(1f, 1f, 1f, 0f);
	}
}
