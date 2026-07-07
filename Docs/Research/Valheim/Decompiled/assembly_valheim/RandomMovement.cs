using UnityEngine;

public class RandomMovement : MonoBehaviour
{
	public float m_frequency = 10f;

	public float m_movement = 0.1f;

	private Vector3 m_basePosition = Vector3.zero;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_basePosition = ((Component)this).transform.localPosition;
	}

	private void Update()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time * m_frequency;
		Vector3 val = new Vector3(Mathf.Sin(num) * Mathf.Sin(num * 0.56436f), Mathf.Sin(num * 0.56436f) * Mathf.Sin(num * 0.688742f), Mathf.Cos(num * 0.758348f) * Mathf.Cos(num * 0.4563696f)) * m_movement;
		((Component)this).transform.localPosition = m_basePosition + val;
	}
}
