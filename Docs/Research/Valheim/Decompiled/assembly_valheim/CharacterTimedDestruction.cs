using UnityEngine;

public class CharacterTimedDestruction : MonoBehaviour
{
	public float m_timeoutMin = 1f;

	public float m_timeoutMax = 1f;

	public bool m_triggerOnAwake;

	private ZNetView m_nview;

	private Character m_character;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_triggerOnAwake)
		{
			Trigger();
		}
	}

	public void Trigger()
	{
		((MonoBehaviour)this).InvokeRepeating("DestroyNow", Random.Range(m_timeoutMin, m_timeoutMax), 1f);
	}

	public void Trigger(float timeout)
	{
		((MonoBehaviour)this).InvokeRepeating("DestroyNow", timeout, 1f);
	}

	private void DestroyNow()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			((Component)this).GetComponent<Character>().ApplyDamage(new HitData
			{
				m_damage = 
				{
					m_damage = 99999f
				},
				m_point = ((Component)this).transform.position
			}, showDamageText: false, triggerEffects: true);
		}
	}
}
