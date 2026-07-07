using UnityEngine;

public class ThorFly : MonoBehaviour
{
	public float m_speed = 100f;

	public float m_ttl = 10f;

	private float m_timer;

	private void Start()
	{
	}

	private void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = ((Component)this).transform.position + ((Component)this).transform.forward * m_speed * Time.deltaTime;
		m_timer += Time.deltaTime;
		if (m_timer > m_ttl)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
