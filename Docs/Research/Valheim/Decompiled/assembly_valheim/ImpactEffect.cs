using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
	public EffectList m_hitEffect = new EffectList();

	public EffectList m_destroyEffect = new EffectList();

	public float m_hitDestroyChance;

	public float m_minVelocity;

	public float m_maxVelocity;

	public bool m_damageToSelf;

	public bool m_damagePlayers = true;

	public bool m_damageFish;

	public HitData.HitType m_hitType = HitData.HitType.Impact;

	public int m_toolTier;

	public HitData.DamageTypes m_damages;

	public LayerMask m_triggerMask;

	public float m_interval = 0.5f;

	private bool m_firstHit = true;

	private bool m_hitEffectEnabled = true;

	private ZNetView m_nview;

	private Rigidbody m_body;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_body = ((Component)this).GetComponent<Rigidbody>();
		if (m_maxVelocity < m_minVelocity)
		{
			m_maxVelocity = m_minVelocity;
		}
	}

	public void OnCollisionEnter(Collision info)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || (Object.op_Implicit((Object)(object)m_nview) && !m_nview.IsOwner()) || info.contacts.Length == 0 || !m_hitEffectEnabled || (((LayerMask)(ref m_triggerMask)).value & (1 << ((Component)info.collider).gameObject.layer)) == 0)
		{
			return;
		}
		Vector3 relativeVelocity = info.relativeVelocity;
		float magnitude = ((Vector3)(ref relativeVelocity)).magnitude;
		if (magnitude < m_minVelocity)
		{
			return;
		}
		ContactPoint val = info.contacts[0];
		Vector3 point = ((ContactPoint)(ref val)).point;
		Vector3 pointVelocity = m_body.GetPointVelocity(point);
		m_hitEffectEnabled = false;
		((MonoBehaviour)this).Invoke("ResetHitTimer", m_interval);
		if (m_damages.HaveDamage())
		{
			GameObject val2 = Projectile.FindHitObject(((ContactPoint)(ref val)).otherCollider);
			float num = (num = Utils.LerpStep(m_minVelocity, m_maxVelocity, magnitude));
			IDestructible component = val2.GetComponent<IDestructible>();
			if (component != null)
			{
				Character character = component as Character;
				if (Object.op_Implicit((Object)(object)character))
				{
					if (!m_damagePlayers && character.IsPlayer())
					{
						return;
					}
					relativeVelocity = info.relativeVelocity;
					float num2 = Vector3.Dot(-((Vector3)(ref relativeVelocity)).normalized, pointVelocity);
					if (num2 < m_minVelocity)
					{
						return;
					}
					ZLog.Log((object)("Rel vel " + num2));
					num = Utils.LerpStep(m_minVelocity, m_maxVelocity, num2);
					if (character.GetSEMan().HaveStatusAttribute(StatusEffect.StatusAttribute.DoubleImpactDamage))
					{
						num *= 2f;
					}
				}
				if (!m_damageFish && Object.op_Implicit((Object)(object)val2.GetComponent<Fish>()))
				{
					return;
				}
				HitData hitData = new HitData();
				hitData.m_point = point;
				hitData.m_dir = ((Vector3)(ref pointVelocity)).normalized;
				hitData.m_hitCollider = info.collider;
				hitData.m_toolTier = (short)m_toolTier;
				hitData.m_hitType = m_hitType;
				hitData.m_damage = m_damages.Clone();
				hitData.m_damage.Modify(num);
				component.Damage(hitData);
			}
			if (m_damageToSelf)
			{
				IDestructible component2 = ((Component)this).GetComponent<IDestructible>();
				if (component2 != null)
				{
					HitData hitData2 = new HitData();
					hitData2.m_point = point;
					hitData2.m_dir = -((Vector3)(ref pointVelocity)).normalized;
					hitData2.m_toolTier = (short)m_toolTier;
					hitData2.m_hitType = m_hitType;
					hitData2.m_damage = m_damages.Clone();
					hitData2.m_damage.Modify(num);
					component2.Damage(hitData2);
				}
			}
		}
		Vector3 val3 = Vector3.Cross(-Vector3.Normalize(info.relativeVelocity), ((ContactPoint)(ref val)).normal);
		Vector3 val4 = Vector3.Cross(((ContactPoint)(ref val)).normal, val3);
		Quaternion baseRot = Quaternion.identity;
		if (val4 != Vector3.zero && ((ContactPoint)(ref val)).normal != Vector3.zero)
		{
			baseRot = Quaternion.LookRotation(val4, ((ContactPoint)(ref val)).normal);
		}
		m_hitEffect.Create(point, baseRot);
		if (m_firstHit && m_hitDestroyChance > 0f && Random.value <= m_hitDestroyChance)
		{
			m_destroyEffect.Create(point, baseRot);
			GameObject gameObject = ((Component)this).gameObject;
			if (Object.op_Implicit((Object)(object)((Component)this).transform.parent))
			{
				Animator componentInParent = ((Component)((Component)this).transform).GetComponentInParent<Animator>();
				if (Object.op_Implicit((Object)(object)componentInParent))
				{
					gameObject = ((Component)componentInParent).gameObject;
				}
			}
			Object.Destroy((Object)(object)gameObject);
		}
		m_firstHit = false;
	}

	private Vector3 GetAVGPos(ContactPoint[] points)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)("Pooints " + points.Length));
		Vector3 val = Vector3.zero;
		for (int i = 0; i < points.Length; i++)
		{
			ContactPoint val2 = points[i];
			ZLog.Log((object)("P " + ((Object)((Component)((ContactPoint)(ref val2)).otherCollider).gameObject).name));
			val += ((ContactPoint)(ref val2)).point;
		}
		return val;
	}

	private void ResetHitTimer()
	{
		m_hitEffectEnabled = true;
	}
}
