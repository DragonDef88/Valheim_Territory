using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : BaseAI
{
	private float m_lastDespawnInDayCheck = -9999f;

	private float m_lastEventCreatureCheck = -9999f;

	public Action<ItemDrop> m_onConsumedItem;

	private const float m_giveUpTime = 30f;

	private const float m_updateTargetFarRange = 50f;

	private const float m_updateTargetIntervalNear = 2f;

	private const float m_updateTargetIntervalFar = 6f;

	private const float m_updateWeaponInterval = 1f;

	private const float m_unableToAttackTargetDuration = 15f;

	[Header("Monster AI")]
	public float m_alertRange = 9999f;

	public bool m_fleeIfHurtWhenTargetCantBeReached = true;

	public float m_fleeUnreachableSinceAttacking = 30f;

	public float m_fleeUnreachableSinceHurt = 20f;

	public bool m_fleeIfNotAlerted;

	public float m_fleeIfLowHealth;

	public float m_fleeTimeSinceHurt = 20f;

	public bool m_fleeInLava = true;

	public float m_fleePheromoneMin = 3f;

	public float m_fleePheromoneMax = 8f;

	public bool m_circulateWhileCharging;

	public bool m_circulateWhileChargingFlying;

	public bool m_enableHuntPlayer;

	public bool m_attackPlayerObjects = true;

	public int m_privateAreaTriggerTreshold = 4;

	public float m_interceptTimeMax;

	public float m_interceptTimeMin;

	public float m_maxChaseDistance;

	public float m_minAttackInterval;

	[Header("Circle target")]
	public float m_circleTargetInterval;

	public float m_circleTargetDuration = 5f;

	public float m_circleTargetDistance = 10f;

	[Header("Sleep")]
	public bool m_sleeping;

	public float m_wakeupRange = 5f;

	public bool m_noiseWakeup;

	public float m_maxNoiseWakeupRange = 50f;

	public EffectList m_wakeupEffects = new EffectList();

	public EffectList m_sleepEffects = new EffectList();

	public float m_wakeUpDelayMin;

	public float m_wakeUpDelayMax;

	public float m_fallAsleepDistance;

	[Header("Other")]
	public bool m_avoidLand;

	[Header("Consume items")]
	public List<ItemDrop> m_consumeItems;

	public float m_consumeRange = 2f;

	public float m_consumeSearchRange = 5f;

	public float m_consumeSearchInterval = 10f;

	private ItemDrop m_consumeTarget;

	private float m_consumeSearchTimer;

	private static int m_itemMask = 0;

	private bool m_despawnInDay;

	private bool m_eventCreature;

	private Character m_targetCreature;

	private Vector3 m_lastKnownTargetPos = Vector3.zero;

	private bool m_beenAtLastPos;

	private StaticTarget m_targetStatic;

	private float m_timeSinceAttacking;

	private float m_timeSinceSensedTargetCreature;

	private float m_updateTargetTimer;

	private float m_updateWeaponTimer;

	private float m_interceptTime;

	private float m_sleepDelay = 0.5f;

	private float m_pauseTimer;

	private float m_sleepTimer;

	private float m_unableToAttackTargetTimer;

	private GameObject m_follow;

	private int m_privateAreaAttacks;

	private static readonly int s_sleeping = ZSyncAnimation.GetHash("sleeping");

	protected override void Awake()
	{
		base.Awake();
		ZDO zDO = m_nview.GetZDO();
		if (zDO != null)
		{
			m_despawnInDay = zDO.GetBool(ZDOVars.s_despawnInDay, m_despawnInDay);
			m_eventCreature = zDO.GetBool(ZDOVars.s_eventCreature, m_eventCreature);
			m_sleeping = zDO.GetBool(ZDOVars.s_sleeping, m_sleeping);
			m_animator.SetBool(s_sleeping, IsSleeping());
		}
		m_interceptTime = Random.Range(m_interceptTimeMin, m_interceptTimeMax);
		m_pauseTimer = Random.Range(0f, m_circleTargetInterval);
		m_updateTargetTimer = Random.Range(0f, 2f);
		if (m_wakeUpDelayMin > 0f || m_wakeUpDelayMax > 0f)
		{
			m_sleepDelay = Random.Range(m_wakeUpDelayMin, m_wakeUpDelayMax);
		}
		if (m_enableHuntPlayer)
		{
			SetHuntPlayer(hunt: true);
		}
		m_nview.Register("RPC_Wakeup", RPC_Wakeup);
		m_nview.Register("RPC_Sleep", RPC_Sleep);
	}

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid() && m_nview.IsOwner())
		{
			Humanoid humanoid = m_character as Humanoid;
			if (Object.op_Implicit((Object)(object)humanoid))
			{
				humanoid.EquipBestWeapon(null, null, null, null);
			}
		}
	}

	protected override void OnDamaged(float damage, Character attacker)
	{
		base.OnDamaged(damage, attacker);
		Wakeup();
		SetAlerted(alert: true);
		SetTarget(attacker);
	}

	private void SetTarget(Character attacker)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)attacker != (Object)null && (Object)(object)m_targetCreature == (Object)null && (!attacker.IsPlayer() || !m_character.IsTamed()))
		{
			m_targetCreature = attacker;
			m_lastKnownTargetPos = ((Component)attacker).transform.position;
			m_beenAtLastPos = false;
			m_targetStatic = null;
		}
	}

	protected override void RPC_OnNearProjectileHit(long sender, Vector3 center, float range, ZDOID attackerID)
	{
		if (!m_nview.IsOwner() || ZoneSystem.instance.GetGlobalKey(GlobalKeys.PassiveMobs))
		{
			return;
		}
		SetAlerted(alert: true);
		if (m_fleeIfNotAlerted)
		{
			return;
		}
		GameObject val = ZNetScene.instance.FindInstance(attackerID);
		if ((Object)(object)val != (Object)null)
		{
			Character component = val.GetComponent<Character>();
			if (Object.op_Implicit((Object)(object)component))
			{
				SetTarget(component);
			}
		}
	}

	public void MakeTame()
	{
		m_character.SetTamed(tamed: true);
		SetAlerted(alert: false);
		m_targetCreature = null;
		m_targetStatic = null;
	}

	private void UpdateTarget(Humanoid humanoid, float dt, out bool canHearTarget, out bool canSeeTarget)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		m_unableToAttackTargetTimer -= dt;
		m_updateTargetTimer -= dt;
		if (m_updateTargetTimer <= 0f && !m_character.InAttack())
		{
			bool flag = Player.IsPlayerInRange(((Component)this).transform.position, 50f);
			m_updateTargetTimer = (flag ? 2f : 6f);
			Character character = FindEnemy();
			if (Object.op_Implicit((Object)(object)character))
			{
				m_targetCreature = character;
				m_targetStatic = null;
			}
			bool flag2 = (Object)(object)m_targetCreature != (Object)null && m_targetCreature.IsPlayer();
			bool flag3 = (Object)(object)m_targetCreature != (Object)null && m_unableToAttackTargetTimer > 0f && !HavePath(((Component)m_targetCreature).transform.position);
			if (m_attackPlayerObjects && (!m_aggravatable || IsAggravated()) && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.PassiveMobs) && ((Object)(object)m_targetCreature == (Object)null || flag3) && !m_character.IsTamed())
			{
				StaticTarget staticTarget = FindClosestStaticPriorityTarget();
				if (Object.op_Implicit((Object)(object)staticTarget))
				{
					m_targetStatic = staticTarget;
					m_targetCreature = null;
				}
				bool flag4 = false;
				if ((Object)(object)m_targetStatic != (Object)null)
				{
					Vector3 target = m_targetStatic.FindClosestPoint(((Component)m_character).transform.position);
					flag4 = HavePath(target);
				}
				if (((Object)(object)m_targetStatic == (Object)null || !flag4) && IsAlerted() && flag2)
				{
					StaticTarget staticTarget2 = FindRandomStaticTarget(10f);
					if (Object.op_Implicit((Object)(object)staticTarget2))
					{
						m_targetStatic = staticTarget2;
						m_targetCreature = null;
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)m_targetCreature) && m_character.IsTamed())
		{
			if (GetPatrolPoint(out var point))
			{
				if (Vector3.Distance(((Component)m_targetCreature).transform.position, point) > m_alertRange)
				{
					m_targetCreature = null;
				}
			}
			else if (Object.op_Implicit((Object)(object)m_follow) && Vector3.Distance(((Component)m_targetCreature).transform.position, m_follow.transform.position) > m_alertRange)
			{
				m_targetCreature = null;
			}
		}
		if (Object.op_Implicit((Object)(object)m_targetCreature))
		{
			if (m_targetCreature.IsDead())
			{
				m_targetCreature = null;
			}
			else if (!IsEnemy(m_targetCreature))
			{
				m_targetCreature = null;
			}
			else if (m_skipLavaTargets && m_targetCreature.AboveOrInLava())
			{
				m_targetCreature = null;
			}
		}
		canHearTarget = false;
		canSeeTarget = false;
		if (Object.op_Implicit((Object)(object)m_targetCreature))
		{
			canHearTarget = CanHearTarget(m_targetCreature);
			canSeeTarget = CanSeeTarget(m_targetCreature);
			if (canSeeTarget | canHearTarget)
			{
				m_timeSinceSensedTargetCreature = 0f;
			}
			if (m_targetCreature.IsPlayer())
			{
				m_targetCreature.OnTargeted(canSeeTarget | canHearTarget, IsAlerted());
			}
			SetTargetInfo(m_targetCreature.GetZDOID());
		}
		else
		{
			SetTargetInfo(ZDOID.None);
		}
		m_timeSinceSensedTargetCreature += dt;
		if (IsAlerted() || (Object)(object)m_targetCreature != (Object)null)
		{
			m_timeSinceAttacking += dt;
			float num = 60f;
			float num2 = Vector3.Distance(m_spawnPoint, ((Component)this).transform.position);
			bool flag5 = HuntPlayer() && Object.op_Implicit((Object)(object)m_targetCreature) && m_targetCreature.IsPlayer();
			if (m_timeSinceSensedTargetCreature > 30f || (!flag5 && (m_timeSinceAttacking > num || (m_maxChaseDistance > 0f && m_timeSinceSensedTargetCreature > 1f && num2 > m_maxChaseDistance))))
			{
				SetAlerted(alert: false);
				m_targetCreature = null;
				m_targetStatic = null;
				m_timeSinceAttacking = 0f;
				m_updateTargetTimer = 5f;
			}
		}
	}

	public override bool UpdateAI(float dt)
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Unknown result type (might be due to invalid IL or missing references)
		//IL_0603: Unknown result type (might be due to invalid IL or missing references)
		//IL_060b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_0722: Unknown result type (might be due to invalid IL or missing references)
		//IL_0727: Unknown result type (might be due to invalid IL or missing references)
		//IL_072d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0738: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0991: Unknown result type (might be due to invalid IL or missing references)
		//IL_099c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0681: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_090f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07da: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0885: Unknown result type (might be due to invalid IL or missing references)
		//IL_0850: Unknown result type (might be due to invalid IL or missing references)
		//IL_0898: Unknown result type (might be due to invalid IL or missing references)
		if (!base.UpdateAI(dt))
		{
			return false;
		}
		UpdateSleep(dt);
		if (IsSleeping())
		{
			return true;
		}
		Humanoid humanoid = m_character as Humanoid;
		if (HuntPlayer())
		{
			SetAlerted(alert: true);
		}
		UpdateTarget(humanoid, dt, out var canHearTarget, out var canSeeTarget);
		if (Object.op_Implicit((Object)(object)m_tamable) && Object.op_Implicit((Object)(object)m_tamable.m_saddle) && m_tamable.m_saddle.UpdateRiding(dt))
		{
			return true;
		}
		if (m_avoidLand && !m_character.IsSwimming())
		{
			MoveToWater(dt, 20f);
			return true;
		}
		if (DespawnInDay() && EnvMan.IsDay() && ((Object)(object)m_targetCreature == (Object)null || !canSeeTarget))
		{
			MoveAwayAndDespawn(dt, run: true);
			return true;
		}
		if (IsEventCreature() && !RandEventSystem.HaveActiveEvent())
		{
			SetHuntPlayer(hunt: false);
			if ((Object)(object)m_targetCreature == (Object)null && !IsAlerted())
			{
				MoveAwayAndDespawn(dt, run: false);
				return true;
			}
		}
		if (m_fleeIfNotAlerted && !HuntPlayer() && Object.op_Implicit((Object)(object)m_targetCreature) && !IsAlerted() && Vector3.Distance(((Component)m_targetCreature).transform.position, ((Component)this).transform.position) - m_targetCreature.GetRadius() > m_alertRange)
		{
			Flee(dt, ((Component)m_targetCreature).transform.position);
			return true;
		}
		if (m_fleeIfLowHealth > 0f && m_timeSinceHurt < m_fleeTimeSinceHurt && (Object)(object)m_targetCreature != (Object)null && m_character.GetHealthPercentage() < m_fleeIfLowHealth)
		{
			Flee(dt, ((Component)m_targetCreature).transform.position);
			return true;
		}
		if (m_fleeInLava && m_character.InLava() && ((Object)(object)m_targetCreature == (Object)null || m_targetCreature.AboveOrInLava()))
		{
			Flee(dt, ((Component)m_character).transform.position - ((Component)m_character).transform.forward);
			return true;
		}
		if ((m_afraidOfFire || m_avoidFire) && AvoidFire(dt, m_targetCreature, m_afraidOfFire))
		{
			if (m_afraidOfFire)
			{
				m_targetStatic = null;
				m_targetCreature = null;
			}
			return true;
		}
		if (!m_character.IsTamed())
		{
			if ((Object)(object)m_targetCreature != (Object)null)
			{
				if (Object.op_Implicit((Object)(object)EffectArea.IsPointInsideNoMonsterArea(((Component)m_targetCreature).transform.position)))
				{
					Flee(dt, ((Component)m_targetCreature).transform.position);
					return true;
				}
			}
			else
			{
				EffectArea effectArea = EffectArea.IsPointCloseToNoMonsterArea(((Component)this).transform.position);
				if ((Object)(object)effectArea != (Object)null)
				{
					Flee(dt, ((Component)effectArea).transform.position);
					return true;
				}
			}
		}
		if (m_fleeIfHurtWhenTargetCantBeReached && (Object)(object)m_targetCreature != (Object)null && m_timeSinceAttacking > 30f && m_timeSinceHurt < 20f)
		{
			Flee(dt, ((Component)m_targetCreature).transform.position);
			m_lastKnownTargetPos = ((Component)this).transform.position;
			m_updateTargetTimer = 1f;
			return true;
		}
		if ((!IsAlerted() || ((Object)(object)m_targetStatic == (Object)null && (Object)(object)m_targetCreature == (Object)null)) && UpdateConsumeItem(humanoid, dt))
		{
			return true;
		}
		if (m_circleTargetInterval > 0f && Object.op_Implicit((Object)(object)m_targetCreature))
		{
			m_pauseTimer += dt;
			if (m_pauseTimer > m_circleTargetInterval)
			{
				if (m_pauseTimer > m_circleTargetInterval + m_circleTargetDuration)
				{
					m_pauseTimer = Random.Range(0f, m_circleTargetInterval / 10f);
				}
				RandomMovementArroundPoint(dt, ((Component)m_targetCreature).transform.position, m_circleTargetDistance, IsAlerted());
				return true;
			}
		}
		ItemDrop.ItemData itemData = SelectBestAttack(humanoid, dt);
		bool flag = itemData != null && Time.time - itemData.m_lastAttackTime > itemData.m_shared.m_aiAttackInterval;
		bool flag2 = m_character.GetTimeSinceLastAttack() >= m_minAttackInterval;
		bool flag3 = itemData != null && flag && flag2 && !IsTakingOff();
		if (!IsCharging() && ((Object)(object)m_targetStatic != (Object)null || (Object)(object)m_targetCreature != (Object)null) && itemData != null && flag3 && !m_character.InAttack() && itemData.m_shared.m_attack != null && !itemData.m_shared.m_attack.IsDone() && !string.IsNullOrEmpty(itemData.m_shared.m_attack.m_chargeAnimationBool))
		{
			ChargeStart(itemData.m_shared.m_attack.m_chargeAnimationBool);
		}
		if ((m_character.IsFlying() ? m_circulateWhileChargingFlying : m_circulateWhileCharging) && ((Object)(object)m_targetStatic != (Object)null || (Object)(object)m_targetCreature != (Object)null) && itemData != null && !flag3 && !m_character.InAttack())
		{
			Vector3 point = (Object.op_Implicit((Object)(object)m_targetCreature) ? ((Component)m_targetCreature).transform.position : ((Component)m_targetStatic).transform.position);
			RandomMovementArroundPoint(dt, point, m_randomMoveRange, IsAlerted());
			return true;
		}
		if (((Object)(object)m_targetStatic == (Object)null && (Object)(object)m_targetCreature == (Object)null) || itemData == null)
		{
			if (Object.op_Implicit((Object)(object)m_follow))
			{
				Follow(m_follow, dt);
			}
			else
			{
				IdleMovement(dt);
			}
			ChargeStop();
			return true;
		}
		if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.Enemy)
		{
			if (Object.op_Implicit((Object)(object)m_targetStatic))
			{
				Vector3 val = m_targetStatic.FindClosestPoint(((Component)this).transform.position);
				if (Vector3.Distance(val, ((Component)this).transform.position) < itemData.m_shared.m_aiAttackRange && CanSeeTarget(m_targetStatic))
				{
					LookAt(m_targetStatic.GetCenter());
					if (itemData.m_shared.m_aiAttackMaxAngle == 0f)
					{
						ZLog.LogError((object)("AI Attack Max Angle for " + itemData.m_shared.m_name + " is 0!"));
					}
					if (IsLookingAt(m_targetStatic.GetCenter(), itemData.m_shared.m_aiAttackMaxAngle, itemData.m_shared.m_aiInvertAngleCheck) && flag3)
					{
						DoAttack(null, isFriend: false);
					}
					else
					{
						StopMoving();
					}
				}
				else
				{
					MoveTo(dt, val, 0f, IsAlerted());
					ChargeStop();
				}
			}
			else if (Object.op_Implicit((Object)(object)m_targetCreature))
			{
				if (canHearTarget || canSeeTarget || (HuntPlayer() && m_targetCreature.IsPlayer()))
				{
					m_beenAtLastPos = false;
					m_lastKnownTargetPos = ((Component)m_targetCreature).transform.position;
					float num = Vector3.Distance(m_lastKnownTargetPos, ((Component)this).transform.position) - m_targetCreature.GetRadius();
					float num2 = m_alertRange * m_targetCreature.GetStealthFactor();
					if (canSeeTarget && num < num2)
					{
						SetAlerted(alert: true);
					}
					bool num3 = num < itemData.m_shared.m_aiAttackRange;
					if (!num3 || !canSeeTarget || itemData.m_shared.m_aiAttackRangeMin < 0f || !IsAlerted())
					{
						Vector3 velocity = m_targetCreature.GetVelocity();
						Vector3 val2 = velocity * m_interceptTime;
						Vector3 val3 = m_lastKnownTargetPos;
						if (num > ((Vector3)(ref val2)).magnitude / 4f)
						{
							val3 += velocity * m_interceptTime;
						}
						MoveTo(dt, val3, 0f, IsAlerted());
						if (m_timeSinceAttacking > 15f)
						{
							m_unableToAttackTargetTimer = 15f;
						}
					}
					else
					{
						StopMoving();
					}
					if (num3 && canSeeTarget && IsAlerted())
					{
						if (PheromoneFleeCheck(m_targetCreature))
						{
							Flee(dt, ((Component)m_targetCreature).transform.position);
							m_updateTargetTimer = Random.Range(m_fleePheromoneMin, m_fleePheromoneMax);
							m_targetCreature = null;
						}
						else
						{
							LookAt(m_targetCreature.GetTopPoint());
							if (flag3 && IsLookingAt(m_lastKnownTargetPos, itemData.m_shared.m_aiAttackMaxAngle, itemData.m_shared.m_aiInvertAngleCheck))
							{
								DoAttack(m_targetCreature, isFriend: false);
							}
						}
					}
				}
				else
				{
					ChargeStop();
					if (m_beenAtLastPos)
					{
						RandomMovement(dt, m_lastKnownTargetPos);
						if (m_timeSinceAttacking > 15f)
						{
							m_unableToAttackTargetTimer = 15f;
						}
					}
					else if (MoveTo(dt, m_lastKnownTargetPos, 0f, IsAlerted()))
					{
						m_beenAtLastPos = true;
					}
				}
			}
		}
		else if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.FriendHurt || itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.Friend)
		{
			Character character = ((itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.FriendHurt) ? HaveHurtFriendInRange(m_viewRange) : HaveFriendInRange(m_viewRange));
			if (Object.op_Implicit((Object)(object)character))
			{
				if (Vector3.Distance(((Component)character).transform.position, ((Component)this).transform.position) < itemData.m_shared.m_aiAttackRange)
				{
					if (flag3)
					{
						StopMoving();
						LookAt(((Component)character).transform.position);
						DoAttack(character, isFriend: true);
					}
					else
					{
						RandomMovement(dt, ((Component)character).transform.position);
					}
				}
				else
				{
					MoveTo(dt, ((Component)character).transform.position, 0f, IsAlerted());
				}
			}
			else
			{
				RandomMovement(dt, ((Component)this).transform.position, snapToGround: true);
			}
		}
		return true;
	}

	private bool PheromoneFleeCheck(Character target)
	{
		if (target is Player player)
		{
			foreach (StatusEffect statusEffect in player.GetSEMan().GetStatusEffects())
			{
				if (statusEffect is SE_Stats { m_pheromoneFlee: not false } sE_Stats)
				{
					Character component = sE_Stats.m_pheromoneTarget.GetComponent<Character>();
					if (component != null && component.m_name == m_character.m_name)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool UpdateConsumeItem(Humanoid humanoid, float dt)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if (m_consumeItems == null || m_consumeItems.Count == 0)
		{
			return false;
		}
		m_consumeSearchTimer += dt;
		if (m_consumeSearchTimer > m_consumeSearchInterval)
		{
			m_consumeSearchTimer = 0f;
			if (Object.op_Implicit((Object)(object)m_tamable) && !m_tamable.IsHungry())
			{
				return false;
			}
			m_consumeTarget = FindClosestConsumableItem(m_consumeSearchRange);
		}
		if (Object.op_Implicit((Object)(object)m_consumeTarget))
		{
			if (MoveTo(dt, ((Component)m_consumeTarget).transform.position, m_consumeRange, run: false))
			{
				LookAt(((Component)m_consumeTarget).transform.position);
				if (IsLookingAt(((Component)m_consumeTarget).transform.position, 20f) && m_consumeTarget.RemoveOne())
				{
					if (m_onConsumedItem != null)
					{
						m_onConsumedItem(m_consumeTarget);
					}
					humanoid.m_consumeItemEffects.Create(((Component)this).transform.position, Quaternion.identity);
					m_animator.SetTrigger("consume");
					m_consumeTarget = null;
				}
			}
			return true;
		}
		return false;
	}

	private ItemDrop FindClosestConsumableItem(float maxRange)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (m_itemMask == 0)
		{
			m_itemMask = LayerMask.GetMask(new string[1] { "item" });
		}
		Collider[] array = Physics.OverlapSphere(((Component)this).transform.position, maxRange, m_itemMask);
		ItemDrop itemDrop = null;
		float num = 999999f;
		Collider[] array2 = array;
		foreach (Collider val in array2)
		{
			if (!Object.op_Implicit((Object)(object)val.attachedRigidbody))
			{
				continue;
			}
			ItemDrop component = ((Component)val.attachedRigidbody).GetComponent<ItemDrop>();
			if (!((Object)(object)component == (Object)null) && ((Component)component).GetComponent<ZNetView>().IsValid() && CanConsume(component.m_itemData))
			{
				float num2 = Vector3.Distance(((Component)component).transform.position, ((Component)this).transform.position);
				if ((Object)(object)itemDrop == (Object)null || num2 < num)
				{
					itemDrop = component;
					num = num2;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)itemDrop) && HavePath(((Component)itemDrop).transform.position))
		{
			return itemDrop;
		}
		return null;
	}

	private bool CanConsume(ItemDrop.ItemData item)
	{
		foreach (ItemDrop consumeItem in m_consumeItems)
		{
			if (consumeItem.m_itemData.m_shared.m_name == item.m_shared.m_name)
			{
				return true;
			}
		}
		return false;
	}

	private ItemDrop.ItemData SelectBestAttack(Humanoid humanoid, float dt)
	{
		if (Object.op_Implicit((Object)(object)m_targetCreature) || Object.op_Implicit((Object)(object)m_targetStatic))
		{
			m_updateWeaponTimer -= dt;
			if (m_updateWeaponTimer <= 0f && !m_character.InAttack())
			{
				m_updateWeaponTimer = 1f;
				HaveFriendsInRange(m_viewRange, out var hurtFriend, out var friend);
				humanoid.EquipBestWeapon(m_targetCreature, m_targetStatic, hurtFriend, friend);
			}
		}
		return humanoid.GetCurrentWeapon();
	}

	private bool DoAttack(Character target, bool isFriend)
	{
		ItemDrop.ItemData currentWeapon = (m_character as Humanoid).GetCurrentWeapon();
		if (currentWeapon == null)
		{
			return false;
		}
		if (!CanUseAttack(currentWeapon))
		{
			return false;
		}
		bool num = m_character.StartAttack(target, charge: false);
		if (num)
		{
			m_timeSinceAttacking = 0f;
		}
		return num;
	}

	public void SetDespawnInDay(bool despawn)
	{
		m_despawnInDay = despawn;
		m_nview.GetZDO().Set(ZDOVars.s_despawnInDay, despawn);
	}

	public bool DespawnInDay()
	{
		if (Time.time - m_lastDespawnInDayCheck > 4f)
		{
			m_lastDespawnInDayCheck = Time.time;
			m_despawnInDay = m_nview.GetZDO().GetBool(ZDOVars.s_despawnInDay, m_despawnInDay);
		}
		return m_despawnInDay;
	}

	public void SetEventCreature(bool despawn)
	{
		m_eventCreature = despawn;
		m_nview.GetZDO().Set(ZDOVars.s_eventCreature, despawn);
	}

	public bool IsEventCreature()
	{
		if (Time.time - m_lastEventCreatureCheck > 4f)
		{
			m_lastEventCreatureCheck = Time.time;
			m_eventCreature = m_nview.GetZDO().GetBool(ZDOVars.s_eventCreature, m_eventCreature);
		}
		return m_eventCreature;
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		DrawAILabel();
	}

	private void OnDrawGizmos()
	{
		if (Terminal.m_showTests)
		{
			DrawAILabel();
		}
	}

	private void DrawAILabel()
	{
	}

	public override Character GetTargetCreature()
	{
		return m_targetCreature;
	}

	public StaticTarget GetStaticTarget()
	{
		return m_targetStatic;
	}

	private void UpdateSleep(float dt)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		Player player = null;
		if (m_wakeupRange > 0f || m_fallAsleepDistance > 0f)
		{
			player = Player.GetClosestPlayer(((Component)this).transform.position, m_wakeupRange);
			if ((Object)(object)player != (Object)null && (player.InGhostMode() || player.IsDebugFlying()))
			{
				player = null;
			}
		}
		if (!IsSleeping())
		{
			if (m_fallAsleepDistance > 0f && ((Object)(object)player == (Object)null || Vector3.Distance(((Component)player).transform.position, ((Component)this).transform.position) > m_fallAsleepDistance))
			{
				Sleep();
			}
			return;
		}
		m_sleepTimer += dt;
		if (m_sleepTimer < m_sleepDelay)
		{
			return;
		}
		if (HuntPlayer())
		{
			Wakeup();
		}
		else if (m_wakeupRange > 0f && Object.op_Implicit((Object)(object)player))
		{
			Wakeup();
		}
		else if (m_noiseWakeup)
		{
			Player playerNoiseRange = Player.GetPlayerNoiseRange(((Component)this).transform.position, m_maxNoiseWakeupRange);
			if (Object.op_Implicit((Object)(object)playerNoiseRange) && !playerNoiseRange.InGhostMode() && !playerNoiseRange.IsDebugFlying())
			{
				Wakeup();
			}
		}
	}

	public void OnPrivateAreaAttacked(Character attacker, bool destroyed)
	{
		if (attacker.IsPlayer() && IsAggravatable() && !IsAggravated())
		{
			m_privateAreaAttacks++;
			if (m_privateAreaAttacks > m_privateAreaTriggerTreshold || destroyed)
			{
				SetAggravated(aggro: true, AggravatedReason.Damage);
			}
		}
	}

	private void RPC_Wakeup(long sender)
	{
		if (!m_nview.GetZDO().IsOwner())
		{
			m_sleeping = false;
		}
	}

	private void Wakeup()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (IsSleeping())
		{
			m_animator.SetBool(s_sleeping, value: false);
			m_nview.GetZDO().Set(ZDOVars.s_sleeping, value: false);
			m_wakeupEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			m_sleeping = false;
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_Wakeup");
		}
	}

	private void RPC_Sleep(long sender)
	{
		if (!m_nview.GetZDO().IsOwner())
		{
			m_sleeping = true;
		}
	}

	private void Sleep()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!IsSleeping())
		{
			SetAlerted(alert: false);
			m_sleepTimer = 0f;
			m_targetCreature = null;
			m_animator.SetBool(s_sleeping, value: true);
			m_nview.GetZDO().Set(ZDOVars.s_sleeping, value: true);
			m_sleepEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			m_sleeping = true;
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_Sleep");
		}
	}

	public override bool IsSleeping()
	{
		return m_sleeping;
	}

	protected override void SetAlerted(bool alert)
	{
		if (alert)
		{
			m_timeSinceSensedTargetCreature = 0f;
		}
		base.SetAlerted(alert);
	}

	public override bool HuntPlayer()
	{
		if (base.HuntPlayer())
		{
			if (IsEventCreature() && !RandEventSystem.InEvent())
			{
				return false;
			}
			if (DespawnInDay() && EnvMan.IsDay())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public GameObject GetFollowTarget()
	{
		return m_follow;
	}

	public void SetFollowTarget(GameObject go)
	{
		m_follow = go;
	}
}
