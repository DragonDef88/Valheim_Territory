using System.Collections.Generic;
using UnityEngine;

public class ProximityState : MonoBehaviour
{
	public bool m_playerOnly = true;

	public Animator m_animator;

	public EffectList m_movingClose = new EffectList();

	public EffectList m_movingAway = new EffectList();

	private List<Collider> m_near = new List<Collider>();

	private void Start()
	{
		m_animator.SetBool("near", false);
	}

	private void OnTriggerEnter(Collider other)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (m_playerOnly)
		{
			Character component = ((Component)other).GetComponent<Character>();
			if (!Object.op_Implicit((Object)(object)component) || !component.IsPlayer())
			{
				return;
			}
		}
		if (!m_near.Contains(other))
		{
			m_near.Add(other);
			if (!m_animator.GetBool("near"))
			{
				m_animator.SetBool("near", true);
				m_movingClose.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		m_near.Remove(other);
		if (m_near.Count == 0 && m_animator.GetBool("near"))
		{
			m_animator.SetBool("near", false);
			m_movingAway.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
		}
	}
}
