using UnityEngine;

public class MovementTest : MonoBehaviour
{
	public float m_speed = 10f;

	private float m_timer;

	private Rigidbody m_body;

	private Vector3 m_center;

	private Vector3 m_vel;

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_center = ((Component)this).transform.position;
	}

	private void FixedUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		m_timer += Time.fixedDeltaTime;
		float num = 5f;
		Vector3 val = m_center + new Vector3(Mathf.Sin(m_timer * m_speed) * num, 0f, Mathf.Cos(m_timer * m_speed) * num);
		m_vel = (val - m_body.position) / Time.fixedDeltaTime;
		m_body.position = val;
		m_body.linearVelocity = m_vel;
	}
}
