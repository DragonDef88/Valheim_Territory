using UnityEngine;
using UnityEngine.UI;

public class CaptionArrow : MonoBehaviour
{
	public float m_fadeTime = 1.5f;

	private Vector3 m_sfxPosition;

	private float m_timer;

	public RawImage m_imageComponent;

	private Color m_color;

	private ClosedCaptions.CaptionType m_type;

	private float m_alpha;

	public AnimationCurve m_distanceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private static readonly int s_CaptionDistance = Shader.PropertyToID("_CaptionDistance");

	public void Setup(ClosedCaptions.CaptionType type, Vector3 position, float distanceFactor = 0f)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		m_alpha = ((Graphic)m_imageComponent).color.a;
		m_color = ClosedCaptions.Instance.GetCaptionColor(type);
		m_color.a = m_alpha;
		((Graphic)m_imageComponent).color = m_color;
		m_timer = m_fadeTime;
		m_sfxPosition = position;
		RotateArrow();
		((Graphic)m_imageComponent).material = new Material(((Graphic)m_imageComponent).material);
		((Graphic)m_imageComponent).material.SetFloat(s_CaptionDistance, m_distanceCurve.Evaluate(distanceFactor));
	}

	private void Update()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		m_timer -= Time.deltaTime;
		if (m_timer <= 0f)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		m_color.a = m_alpha * Mathf.Clamp01(m_timer / m_fadeTime);
		m_color.a = Mathf.SmoothStep(0f, 1f, m_color.a);
		((Graphic)m_imageComponent).color = m_color;
		RotateArrow();
	}

	public void RotateArrow()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)AudioMan.instance.GetActiveAudioListener()).transform.position;
		position.y = m_sfxPosition.y;
		Vector3 val = Vector3.ProjectOnPlane(((Component)Utils.GetMainCamera()).transform.forward, Vector3.up);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = VectorExtensions.DirTo(position, m_sfxPosition);
		float num = Vector3.SignedAngle(normalized, val2, Vector3.up);
		((Component)this).transform.localEulerAngles = new Vector3(0f, 0f, 0f - num);
	}
}
