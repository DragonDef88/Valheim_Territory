using System;
using UnityEngine;

public class SpawnOnDamaged : MonoBehaviour
{
	public GameObject m_spawnOnDamage;

	private void Start()
	{
		WearNTear component = ((Component)this).GetComponent<WearNTear>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.m_onDamaged = (Action)Delegate.Combine(component.m_onDamaged, new Action(OnDamaged));
		}
		Destructible component2 = ((Component)this).GetComponent<Destructible>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.m_onDamaged = (Action)Delegate.Combine(component2.m_onDamaged, new Action(OnDamaged));
		}
	}

	private void OnDamaged()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_spawnOnDamage))
		{
			Object.Instantiate<GameObject>(m_spawnOnDamage, ((Component)this).transform.position, Quaternion.identity);
		}
	}
}
