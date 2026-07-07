using UnityEngine;

public class MovementDamage : MonoBehaviour
{
	public GameObject m_runDamageObject;

	public float m_speedTreshold = 6f;

	private Character m_character;

	private ZNetView m_nview;

	private Rigidbody m_body;

	private void Awake()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		m_character = ((Component)this).GetComponent<Character>();
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_body = ((Component)this).GetComponent<Rigidbody>();
		Aoe component = m_runDamageObject.GetComponent<Aoe>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.Setup(m_character, Vector3.zero, 0f, null, null, null);
		}
	}

	private void Update()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner())
		{
			m_runDamageObject.SetActive(false);
			return;
		}
		Vector3 linearVelocity = m_body.linearVelocity;
		bool active = ((Vector3)(ref linearVelocity)).magnitude > m_speedTreshold;
		m_runDamageObject.SetActive(active);
	}
}
