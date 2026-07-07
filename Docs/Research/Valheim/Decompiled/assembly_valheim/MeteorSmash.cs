using System;
using UnityEngine;

public class MeteorSmash : MonoBehaviour
{
	[Tooltip("Should be a child of this object.")]
	public GameObject m_meteorObject;

	[Tooltip("Should be a child of this object.")]
	public GameObject m_landingEffect;

	[Header("Timing")]
	public AnimationCurve m_speedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve m_scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float m_timeToLand = 10f;

	[Header("Spawn Position")]
	public float m_spawnDistance = 500f;

	public float m_spawnAngle = 45f;

	private float m_timer;

	private bool m_crashed;

	private Vector3 m_startPos;

	private Vector3 m_originalScale;

	private void Start()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.RotateTowards(Vector3.forward, Vector3.up, (float)Math.PI / 180f * m_spawnAngle, 0f);
		val = Quaternion.Euler(0f, Random.value * 360f, 0f) * val;
		val = ((Vector3)(ref val)).normalized * m_spawnDistance;
		m_startPos = ((Component)this).transform.position + val;
		m_originalScale = m_meteorObject.transform.localScale;
		m_meteorObject.SetActive(true);
		m_landingEffect.SetActive(false);
		m_meteorObject.transform.position = Vector3.Lerp(m_startPos, ((Component)this).transform.position, m_speedCurve.Evaluate(0f));
		m_meteorObject.transform.localScale = Vector3.Lerp(Vector3.zero, m_originalScale, m_scaleCurve.Evaluate(0f));
		m_meteorObject.transform.LookAt(((Component)this).transform.position);
	}

	private void Update()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (!m_crashed)
		{
			m_timer += Time.deltaTime;
			float num = m_timer / m_timeToLand;
			m_meteorObject.transform.position = Vector3.Lerp(m_startPos, ((Component)this).transform.position, m_speedCurve.Evaluate(num));
			m_meteorObject.transform.localScale = Vector3.Lerp(Vector3.zero, m_originalScale, m_scaleCurve.Evaluate(num));
			if (!(m_timer < m_timeToLand) || m_crashed)
			{
				m_crashed = true;
				m_landingEffect.SetActive(true);
			}
		}
	}
}
