using UnityEngine;

public class EmitterRotation : MonoBehaviour
{
	public float m_maxSpeed = 10f;

	public float m_rotSpeed = 90f;

	private Vector3 m_lastPos;

	private ParticleSystem m_ps;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_lastPos = ((Component)this).transform.position;
		m_ps = ((Component)this).GetComponentInChildren<ParticleSystem>();
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		EmissionModule emission = m_ps.emission;
		if (((EmissionModule)(ref emission)).enabled)
		{
			Vector3 position = ((Component)this).transform.position;
			Vector3 val = position - m_lastPos;
			m_lastPos = position;
			float num = Mathf.Clamp01(((Vector3)(ref val)).magnitude / Time.deltaTime / m_maxSpeed);
			if (val == Vector3.zero)
			{
				val = Vector3.up;
			}
			Quaternion val2 = Quaternion.LookRotation(Vector3.up);
			Quaternion val3 = Quaternion.LookRotation(val);
			Quaternion val4 = Quaternion.Lerp(val2, val3, num);
			((Component)this).transform.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, val4, Time.deltaTime * m_rotSpeed);
		}
	}
}
