using UnityEngine;

public class ReflectionUpdate : MonoBehaviour
{
	private static ReflectionUpdate m_instance;

	public ReflectionProbe m_probe1;

	public ReflectionProbe m_probe2;

	public float m_interval = 3f;

	public float m_reflectionHeight = 5f;

	public float m_transitionDuration = 3f;

	public float m_power = 1f;

	private ReflectionProbe m_current;

	private int m_renderID;

	private float m_updateTimer;

	public static ReflectionUpdate instance => m_instance;

	private void Start()
	{
		m_instance = this;
		m_current = m_probe1;
	}

	private void OnDestroy()
	{
		m_instance = null;
	}

	public void UpdateReflection()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Vector3 referencePosition = ZNet.instance.GetReferencePosition();
		referencePosition += Vector3.up * m_reflectionHeight;
		m_current = (((Object)(object)m_current == (Object)(object)m_probe1) ? m_probe2 : m_probe1);
		((Component)m_current).transform.position = referencePosition;
		m_renderID = m_current.RenderProbe();
	}

	private void Update()
	{
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		float deltaTime = Time.deltaTime;
		m_updateTimer += deltaTime;
		if (m_updateTimer > m_interval)
		{
			m_updateTimer = 0f;
			UpdateReflection();
		}
		if (m_current.IsFinishedRendering(m_renderID))
		{
			float num = Mathf.Clamp01(m_updateTimer / m_transitionDuration);
			num = Mathf.Pow(num, m_power);
			if ((Object)(object)m_probe1 == (Object)(object)m_current)
			{
				m_probe1.importance = 1;
				m_probe2.importance = 0;
				Vector3 size = m_probe1.size;
				size.x = 2000f * num;
				size.y = 1000f * num;
				size.z = 2000f * num;
				m_probe1.size = size;
				m_probe2.size = new Vector3(2001f, 1001f, 2001f);
			}
			else
			{
				m_probe1.importance = 0;
				m_probe2.importance = 1;
				Vector3 size2 = m_probe2.size;
				size2.x = 2000f * num;
				size2.y = 1000f * num;
				size2.z = 2000f * num;
				m_probe2.size = size2;
				m_probe1.size = new Vector3(2001f, 1001f, 2001f);
			}
		}
	}
}
