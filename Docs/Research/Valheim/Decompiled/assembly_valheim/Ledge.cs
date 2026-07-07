using System;
using System.Collections.Generic;
using UnityEngine;

public class Ledge : MonoBehaviour
{
	public Collider m_collider;

	public TriggerTracker m_above;

	private void Awake()
	{
		if (((Component)this).GetComponent<ZNetView>().GetZDO() != null)
		{
			m_collider.enabled = true;
			TriggerTracker above = m_above;
			above.m_changed = (Action)Delegate.Combine(above.m_changed, new Action(Changed));
		}
	}

	private void Changed()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> colliders = m_above.GetColliders();
		if (colliders.Count == 0)
		{
			m_collider.enabled = true;
			return;
		}
		bool enabled = false;
		foreach (Collider item in colliders)
		{
			if (((Component)item).transform.position.y > ((Component)this).transform.position.y)
			{
				enabled = true;
				break;
			}
		}
		m_collider.enabled = enabled;
	}
}
