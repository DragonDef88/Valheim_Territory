using System;
using UnityEngine;

public class Leviathan : MonoBehaviour
{
	public float m_waveScale = 0.5f;

	public float m_floatOffset;

	public float m_movementSpeed = 0.1f;

	public float m_maxSpeed = 1f;

	public MineRock m_mineRock;

	public float m_hitReactionChance = 0.25f;

	public int m_leaveDelay = 5;

	public EffectList m_reactionEffects = new EffectList();

	public EffectList m_leaveEffects = new EffectList();

	public bool m_alignToWaterLevel = true;

	private Rigidbody m_body;

	private ZNetView m_nview;

	private ZSyncAnimation m_zanimator;

	private Animator m_animator;

	private bool m_left;

	private void Awake()
	{
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_zanimator = ((Component)this).GetComponent<ZSyncAnimation>();
		m_animator = ((Component)this).GetComponentInChildren<Animator>();
		if (Object.op_Implicit((Object)(object)((Component)this).GetComponent<MineRock>()))
		{
			MineRock mineRock = m_mineRock;
			mineRock.m_onHit = (Action)Delegate.Combine(mineRock.m_onHit, new Action(OnHit));
		}
		if (m_nview.IsValid() && m_nview.IsOwner() && m_nview.GetZDO().GetBool(ZDOVars.s_dead))
		{
			m_nview.Destroy();
		}
	}

	private void FixedUpdate()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner())
		{
			return;
		}
		float liquidLevel = Floating.GetLiquidLevel(((Component)this).transform.position, m_waveScale);
		if (m_alignToWaterLevel)
		{
			if (liquidLevel > -100f)
			{
				Vector3 position = m_body.position;
				float num = Mathf.Clamp((liquidLevel - (position.y + m_floatOffset)) * m_movementSpeed * Time.fixedDeltaTime, 0f - m_maxSpeed, m_maxSpeed);
				position.y += num;
				m_body.MovePosition(position);
			}
			else
			{
				Vector3 position2 = m_body.position;
				position2.y = 0f;
				m_body.MovePosition(Vector3.MoveTowards(m_body.position, position2, Time.deltaTime));
			}
		}
		AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
		if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsTag("submerged"))
		{
			m_nview.Destroy();
		}
	}

	private void OnHit()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (Random.value <= m_hitReactionChance && !m_left)
		{
			m_reactionEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			m_zanimator.SetTrigger("shake");
			((MonoBehaviour)this).Invoke("Leave", (float)m_leaveDelay);
		}
	}

	private void Leave()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner() && !m_left)
		{
			m_left = true;
			m_leaveEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			m_zanimator.SetTrigger("dive");
			m_nview.GetZDO().Set(ZDOVars.s_dead, value: true);
		}
	}

	private void OnDestroy()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (m_left && m_nview.IsValid() && !m_nview.IsOwner() && Player.GetPlayersInRangeXZ(((Component)this).transform.position, 40f) == 0)
		{
			m_nview.Destroy();
		}
	}
}
