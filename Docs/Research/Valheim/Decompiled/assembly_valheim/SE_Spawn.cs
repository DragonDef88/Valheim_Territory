using UnityEngine;

public class SE_Spawn : StatusEffect
{
	[Header("__SE_Spawn__")]
	public float m_delay = 10f;

	public GameObject m_prefab;

	public Vector3 m_spawnOffset = new Vector3(0f, 0f, 0f);

	public EffectList m_spawnEffect = new EffectList();

	private bool m_spawned;

	public override void UpdateStatusEffect(float dt)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateStatusEffect(dt);
		if (!m_spawned && m_time > m_delay)
		{
			m_spawned = true;
			Vector3 val = ((Component)m_character).transform.TransformVector(m_spawnOffset);
			GameObject val2 = Object.Instantiate<GameObject>(m_prefab, val, Quaternion.identity);
			Projectile component = val2.GetComponent<Projectile>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.Setup(m_character, Vector3.zero, -1f, null, null, null);
			}
			m_spawnEffect.Create(val2.transform.position, val2.transform.rotation);
		}
	}
}
