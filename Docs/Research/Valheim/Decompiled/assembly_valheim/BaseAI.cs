using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseAI : MonoBehaviour, IUpdateAI
{
	public enum AggravatedReason
	{
		Damage,
		Building,
		Theif
	}

	private float m_lastMoveToWaterUpdate;

	private bool m_haveWaterPosition;

	private Vector3 m_moveToWaterPosition = Vector3.zero;

	private float m_fleeTargetUpdateTime;

	private Vector3 m_fleeTarget = Vector3.zero;

	private float m_nearFireTime;

	private EffectArea m_nearFireArea;

	private float aroundPointUpdateTime;

	private Vector3 arroundPointTarget = Vector3.zero;

	private Vector3 m_lastMovementCheck;

	private float m_lastMoveTime;

	private const bool m_debugDraw = false;

	public Action<AggravatedReason> m_onBecameAggravated;

	public float m_viewRange = 50f;

	public float m_viewAngle = 90f;

	public float m_hearRange = 9999f;

	public bool m_mistVision;

	private const float m_interiorMaxHearRange = 12f;

	private const float m_despawnDistance = 80f;

	public EffectList m_alertedEffects = new EffectList();

	public EffectList m_idleSound = new EffectList();

	public float m_idleSoundInterval = 5f;

	public float m_idleSoundChance = 0.5f;

	public Pathfinding.AgentType m_pathAgentType = Pathfinding.AgentType.Humanoid;

	public float m_moveMinAngle = 10f;

	public bool m_smoothMovement = true;

	public bool m_serpentMovement;

	public float m_serpentTurnRadius = 20f;

	public float m_jumpInterval;

	[Header("Random circle")]
	public float m_randomCircleInterval = 2f;

	[Header("Random movement")]
	public float m_randomMoveInterval = 5f;

	public float m_randomMoveRange = 4f;

	[Header("Fly behaviour")]
	public bool m_randomFly;

	public float m_chanceToTakeoff = 1f;

	public float m_chanceToLand = 1f;

	public float m_groundDuration = 10f;

	public float m_airDuration = 10f;

	public float m_maxLandAltitude = 5f;

	public float m_takeoffTime = 5f;

	public float m_flyAltitudeMin = 3f;

	public float m_flyAltitudeMax = 10f;

	public float m_flyAbsMinAltitude = 32f;

	[Header("Other")]
	public bool m_avoidFire;

	public bool m_afraidOfFire;

	public bool m_avoidWater = true;

	public bool m_avoidLava = true;

	public bool m_skipLavaTargets;

	public bool m_avoidLavaFlee = true;

	public bool m_aggravatable;

	public bool m_passiveAggresive;

	public string m_spawnMessage = "";

	public string m_deathMessage = "";

	public string m_alertedMessage = "";

	[Header("Flee")]
	public float m_fleeRange = 25f;

	public float m_fleeAngle = 45f;

	public float m_fleeInterval = 2f;

	private bool m_patrol;

	private Vector3 m_patrolPoint = Vector3.zero;

	private float m_patrolPointUpdateTime;

	protected ZNetView m_nview;

	protected Character m_character;

	protected ZSyncAnimation m_animator;

	protected Tameable m_tamable;

	protected Rigidbody m_body;

	private static int m_solidRayMask = 0;

	private static int m_viewBlockMask = 0;

	private static int m_monsterTargetRayMask = 0;

	private Vector3 m_randomMoveTarget = Vector3.zero;

	private float m_randomMoveUpdateTimer;

	private bool m_reachedRandomMoveTarget = true;

	private float m_jumpTimer;

	private float m_randomFlyTimer;

	private float m_regenTimer;

	private bool m_alerted;

	private bool m_huntPlayer;

	private bool m_aggravated;

	private float m_lastAggravatedCheck;

	protected Vector3 m_spawnPoint = Vector3.zero;

	private const float m_getOfOfCornerMaxAngle = 20f;

	private float m_getOutOfCornerTimer;

	private float m_getOutOfCornerAngle;

	private Vector3 m_lastPosition = Vector3.zero;

	private float m_stuckTimer;

	protected float m_timeSinceHurt = 99999f;

	protected float m_lastFlee;

	private string m_charging;

	private Vector3 m_lastFindPathTarget = new Vector3(-999999f, -999999f, -999999f);

	private float m_lastFindPathTime;

	private bool m_lastFindPathResult;

	private readonly List<Vector3> m_path = new List<Vector3>();

	private static readonly RaycastHit[] s_tempRaycastHits = (RaycastHit[])(object)new RaycastHit[128];

	private static readonly Collider[] s_tempSphereOverlap = (Collider[])(object)new Collider[128];

	private static List<BaseAI> m_instances = new List<BaseAI>();

	public static List<IUpdateAI> Instances { get; } = new List<IUpdateAI>();


	public static List<BaseAI> BaseAIInstances { get; } = new List<BaseAI>();


	protected virtual void Awake()
	{
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		m_instances.Add(this);
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_character = ((Component)this).GetComponent<Character>();
		m_animator = ((Component)this).GetComponent<ZSyncAnimation>();
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_tamable = ((Component)this).GetComponent<Tameable>();
		if (m_solidRayMask == 0)
		{
			m_solidRayMask = LayerMask.GetMask(new string[6] { "Default", "static_solid", "Default_small", "piece", "terrain", "vehicle" });
			m_viewBlockMask = LayerMask.GetMask(new string[7] { "Default", "static_solid", "Default_small", "piece", "terrain", "viewblock", "vehicle" });
			m_monsterTargetRayMask = LayerMask.GetMask(new string[6] { "piece", "piece_nonsolid", "Default", "static_solid", "Default_small", "vehicle" });
		}
		Character character = m_character;
		character.m_onDamaged = (Action<float, Character>)Delegate.Combine(character.m_onDamaged, new Action<float, Character>(OnDamaged));
		Character character2 = m_character;
		character2.m_onDeath = (Action)Delegate.Combine(character2.m_onDeath, new Action(OnDeath));
		if (m_nview.IsOwner() && m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L) == 0L)
		{
			m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
			if (!string.IsNullOrEmpty(m_spawnMessage))
			{
				MessageHud.instance.MessageAll(MessageHud.MessageType.Center, m_spawnMessage);
			}
		}
		m_randomMoveUpdateTimer = Random.Range(0f, m_randomMoveInterval);
		m_nview.Register("Alert", RPC_Alert);
		m_nview.Register<Vector3, float, ZDOID>("OnNearProjectileHit", RPC_OnNearProjectileHit);
		m_nview.Register<bool, int>("SetAggravated", RPC_SetAggravated);
		ZDO zDO = m_nview.GetZDO();
		if (zDO != null)
		{
			m_huntPlayer = zDO.GetBool(ZDOVars.s_huntPlayer, m_huntPlayer);
			m_spawnPoint = zDO.GetVec3(ZDOVars.s_spawnPoint, ((Component)this).transform.position);
			if (m_nview.IsOwner())
			{
				zDO.Set(ZDOVars.s_spawnPoint, m_spawnPoint);
			}
		}
		((MonoBehaviour)this).InvokeRepeating("DoIdleSound", m_idleSoundInterval, m_idleSoundInterval);
	}

	private void OnDestroy()
	{
		m_instances.Remove(this);
	}

	protected virtual void OnEnable()
	{
		Instances.Add(this);
		BaseAIInstances.Add(this);
	}

	protected virtual void OnDisable()
	{
		Instances.Remove(this);
		BaseAIInstances.Remove(this);
	}

	public void SetPatrolPoint()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		SetPatrolPoint(((Component)this).transform.position);
	}

	private void SetPatrolPoint(Vector3 point)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		m_patrol = true;
		m_patrolPoint = point;
		m_nview.GetZDO().Set(ZDOVars.s_patrolPoint, point);
		m_nview.GetZDO().Set(ZDOVars.s_patrol, value: true);
	}

	public void ResetPatrolPoint()
	{
		m_patrol = false;
		m_nview.GetZDO().Set(ZDOVars.s_patrol, value: false);
	}

	protected bool GetPatrolPoint(out Vector3 point)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time - m_patrolPointUpdateTime > 1f)
		{
			m_patrolPointUpdateTime = Time.time;
			m_patrol = m_nview.GetZDO().GetBool(ZDOVars.s_patrol);
			if (m_patrol)
			{
				m_patrolPoint = m_nview.GetZDO().GetVec3(ZDOVars.s_patrolPoint, m_patrolPoint);
			}
		}
		point = m_patrolPoint;
		return m_patrol;
	}

	public virtual bool UpdateAI(float dt)
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		if (!m_nview.IsOwner())
		{
			m_alerted = m_nview.GetZDO().GetBool(ZDOVars.s_alert);
			return false;
		}
		UpdateTakeoffLanding(dt);
		if (m_jumpInterval > 0f)
		{
			m_jumpTimer += dt;
		}
		if (m_randomMoveUpdateTimer > 0f)
		{
			m_randomMoveUpdateTimer -= dt;
		}
		UpdateRegeneration(dt);
		m_timeSinceHurt += dt;
		return true;
	}

	private void UpdateRegeneration(float dt)
	{
		m_regenTimer += dt;
		if (!(m_regenTimer <= 2f))
		{
			m_regenTimer = 0f;
			if (!Object.op_Implicit((Object)(object)m_tamable) || !m_character.IsTamed() || !m_tamable.IsHungry())
			{
				float worldTimeDelta = GetWorldTimeDelta();
				float num = m_character.GetMaxHealth() / m_character.m_regenAllHPTime;
				m_character.Heal(num * worldTimeDelta, Object.op_Implicit((Object)(object)m_tamable) && m_character.IsTamed());
			}
		}
	}

	protected bool IsTakingOff()
	{
		if (m_randomFly && m_character.IsFlying())
		{
			return m_randomFlyTimer < m_takeoffTime;
		}
		return false;
	}

	private void UpdateTakeoffLanding(float dt)
	{
		if (!m_randomFly)
		{
			return;
		}
		m_randomFlyTimer += dt;
		if (m_character.InAttack() || m_character.IsStaggering())
		{
			return;
		}
		if (m_character.IsFlying())
		{
			if (m_randomFlyTimer > m_airDuration && GetAltitude() < m_maxLandAltitude)
			{
				m_randomFlyTimer = 0f;
				if (Random.value <= m_chanceToLand)
				{
					m_character.Land();
				}
			}
		}
		else if (m_randomFlyTimer > m_groundDuration)
		{
			m_randomFlyTimer = 0f;
			if (Random.value <= m_chanceToTakeoff)
			{
				m_character.TakeOff();
			}
		}
	}

	private float GetWorldTimeDelta()
	{
		DateTime time = ZNet.instance.GetTime();
		long @long = m_nview.GetZDO().GetLong(ZDOVars.s_worldTimeHash, 0L);
		if (@long == 0L)
		{
			m_nview.GetZDO().Set(ZDOVars.s_worldTimeHash, time.Ticks);
			return 0f;
		}
		DateTime dateTime = new DateTime(@long);
		TimeSpan timeSpan = time - dateTime;
		m_nview.GetZDO().Set(ZDOVars.s_worldTimeHash, time.Ticks);
		return (float)timeSpan.TotalSeconds;
	}

	public TimeSpan GetTimeSinceSpawned()
	{
		if (!Object.op_Implicit((Object)(object)m_nview) || !m_nview.IsValid())
		{
			return TimeSpan.Zero;
		}
		long num = m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime, 0L);
		if (num == 0L)
		{
			num = ZNet.instance.GetTime().Ticks;
			m_nview.GetZDO().Set(ZDOVars.s_spawnTime, num);
		}
		DateTime dateTime = new DateTime(num);
		return ZNet.instance.GetTime() - dateTime;
	}

	private void DoIdleSound()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!IsSleeping() && !(Random.value > m_idleSoundChance))
		{
			m_idleSound.Create(((Component)this).transform.position, Quaternion.identity);
		}
	}

	protected void Follow(GameObject go, float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(go.transform.position, ((Component)this).transform.position);
		bool run = num > 10f;
		if (num < 3f)
		{
			StopMoving();
		}
		else
		{
			MoveTo(dt, go.transform.position, 0f, run);
		}
	}

	protected void MoveToWater(float dt, float maxRange)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		float num = (m_haveWaterPosition ? 2f : 0.5f);
		if (Time.time - m_lastMoveToWaterUpdate > num)
		{
			m_lastMoveToWaterUpdate = Time.time;
			Vector3 val = ((Component)this).transform.position;
			for (int i = 0; i < 10; i++)
			{
				Vector3 val2 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f) * Vector3.forward * Random.Range(4f, maxRange);
				Vector3 val3 = ((Component)this).transform.position + val2;
				val3.y = ZoneSystem.instance.GetSolidHeight(val3);
				if (val3.y < val.y)
				{
					val = val3;
				}
			}
			if (val.y < 30f)
			{
				m_moveToWaterPosition = val;
				m_haveWaterPosition = true;
			}
			else
			{
				m_haveWaterPosition = false;
			}
		}
		if (m_haveWaterPosition)
		{
			MoveTowards(m_moveToWaterPosition - ((Component)this).transform.position, run: true);
		}
	}

	protected void MoveAwayAndDespawn(float dt, bool run)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, 40f);
		if ((Object)(object)closestPlayer != (Object)null)
		{
			Vector3 val = ((Component)closestPlayer).transform.position - ((Component)this).transform.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			MoveTo(dt, ((Component)this).transform.position - normalized * 5f, 0f, run);
		}
		else
		{
			m_nview.Destroy();
		}
	}

	protected void IdleMovement(float dt)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector3 centerPoint = ((m_character.IsTamed() || HuntPlayer()) ? ((Component)this).transform.position : m_spawnPoint);
		if (GetPatrolPoint(out var point))
		{
			centerPoint = point;
		}
		RandomMovement(dt, centerPoint, snapToGround: true);
	}

	protected void RandomMovement(float dt, Vector3 centerPoint, bool snapToGround = false)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		if (m_randomMoveUpdateTimer <= 0f)
		{
			if (snapToGround && ZoneSystem.instance.GetSolidHeight(m_randomMoveTarget, out var height))
			{
				centerPoint.y = height;
			}
			if (Utils.DistanceXZ(centerPoint, ((Component)this).transform.position) > m_randomMoveRange * 2f)
			{
				Vector3 val = centerPoint - ((Component)this).transform.position;
				val.y = 0f;
				((Vector3)(ref val)).Normalize();
				val = Quaternion.Euler(0f, (float)Random.Range(-30, 30), 0f) * val;
				m_randomMoveTarget = ((Component)this).transform.position + val * m_randomMoveRange * 2f;
			}
			else
			{
				Vector3 val2 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f) * ((Component)this).transform.forward * Random.Range(m_randomMoveRange * 0.7f, m_randomMoveRange);
				m_randomMoveTarget = centerPoint + val2;
			}
			if (m_character.IsFlying())
			{
				m_randomMoveTarget.y = Mathf.Max(m_flyAbsMinAltitude, m_randomMoveTarget.y + Random.Range(m_flyAltitudeMin, m_flyAltitudeMax));
			}
			if (!IsValidRandomMovePoint(m_randomMoveTarget))
			{
				return;
			}
			m_reachedRandomMoveTarget = false;
			m_randomMoveUpdateTimer = Random.Range(m_randomMoveInterval, m_randomMoveInterval + m_randomMoveInterval / 2f);
			if ((m_avoidWater && m_character.IsSwimming()) || (m_avoidLava && m_character.InLava()))
			{
				m_randomMoveUpdateTimer /= 4f;
			}
		}
		if (!m_reachedRandomMoveTarget)
		{
			bool flag = IsAlerted() || Utils.DistanceXZ(((Component)this).transform.position, centerPoint) > m_randomMoveRange * 2f;
			if (MoveTo(dt, m_randomMoveTarget, 0f, flag))
			{
				m_reachedRandomMoveTarget = true;
				if (flag)
				{
					m_randomMoveUpdateTimer = 0f;
				}
			}
		}
		else
		{
			StopMoving();
		}
	}

	public void ResetRandomMovement()
	{
		m_reachedRandomMoveTarget = true;
		m_randomMoveUpdateTimer = Random.Range(m_randomMoveInterval, m_randomMoveInterval + m_randomMoveInterval / 2f);
	}

	protected bool Flee(float dt, Vector3 from)
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		if (time - m_fleeTargetUpdateTime > m_fleeInterval)
		{
			m_lastFlee = time;
			m_fleeTargetUpdateTime = time;
			Vector3 val = -(from - ((Component)this).transform.position);
			val.y = 0f;
			((Vector3)(ref val)).Normalize();
			bool flag = false;
			for (int i = 0; i < 10; i++)
			{
				m_fleeTarget = ((Component)this).transform.position + Quaternion.Euler(0f, Random.Range(0f - m_fleeAngle, m_fleeAngle), 0f) * val * m_fleeRange;
				if (HavePath(m_fleeTarget) && (!m_avoidWater || m_character.IsSwimming() || !(ZoneSystem.instance.GetSolidHeight(m_fleeTarget) < 30f)) && (!m_avoidLavaFlee || !ZoneSystem.instance.IsLava(m_fleeTarget)))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				m_fleeTarget = ((Component)this).transform.position + Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f) * Vector3.forward * m_fleeRange;
			}
		}
		return MoveTo(dt, m_fleeTarget, 1f, IsAlerted());
	}

	protected bool AvoidFire(float dt, Character moveToTarget, bool superAfraid)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (m_character.IsTamed())
		{
			return false;
		}
		if (superAfraid)
		{
			EffectArea effectArea = EffectArea.IsPointInsideArea(((Component)this).transform.position, EffectArea.Type.Fire, 3f);
			if (Object.op_Implicit((Object)(object)effectArea))
			{
				m_nearFireTime = Time.time;
				m_nearFireArea = effectArea;
			}
			if (Time.time - m_nearFireTime < 6f && Object.op_Implicit((Object)(object)m_nearFireArea))
			{
				SetAlerted(alert: true);
				Flee(dt, ((Component)m_nearFireArea).transform.position);
				return true;
			}
		}
		else
		{
			EffectArea effectArea2 = EffectArea.IsPointInsideArea(((Component)this).transform.position, EffectArea.Type.Fire, 3f);
			if (Object.op_Implicit((Object)(object)effectArea2))
			{
				if ((Object)(object)moveToTarget != (Object)null && Object.op_Implicit((Object)(object)EffectArea.IsPointInsideArea(((Component)moveToTarget).transform.position, EffectArea.Type.Fire)))
				{
					RandomMovementArroundPoint(dt, ((Component)effectArea2).transform.position, effectArea2.GetRadius() + 3f + 1f, IsAlerted());
					return true;
				}
				RandomMovementArroundPoint(dt, ((Component)effectArea2).transform.position, (effectArea2.GetRadius() + 3f) * 1.5f, IsAlerted());
				return true;
			}
		}
		return false;
	}

	protected void RandomMovementArroundPoint(float dt, Vector3 point, float distance, bool run)
	{
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		ChargeStop();
		float time = Time.time;
		if (time - aroundPointUpdateTime > m_randomCircleInterval)
		{
			aroundPointUpdateTime = time;
			Vector3 val = ((Component)this).transform.position - point;
			val.y = 0f;
			((Vector3)(ref val)).Normalize();
			float num = ((!(Vector3.Distance(((Component)this).transform.position, point) < distance / 2f)) ? ((float)(((double)Random.value > 0.5) ? 40 : (-40))) : ((float)(((double)Random.value > 0.5) ? 90 : (-90))));
			Vector3 val2 = Quaternion.Euler(0f, num, 0f) * val;
			arroundPointTarget = point + val2 * distance;
			if (Vector3.Dot(((Component)this).transform.forward, arroundPointTarget - ((Component)this).transform.position) < 0f)
			{
				val2 = Quaternion.Euler(0f, 0f - num, 0f) * val;
				arroundPointTarget = point + val2 * distance;
				if (m_serpentMovement && Vector3.Distance(point, ((Component)this).transform.position) > distance / 2f && Vector3.Dot(((Component)this).transform.forward, arroundPointTarget - ((Component)this).transform.position) < 0f)
				{
					arroundPointTarget = point - val2 * distance;
				}
			}
			if (m_character.IsFlying())
			{
				arroundPointTarget.y += Random.Range(m_flyAltitudeMin, m_flyAltitudeMax);
			}
		}
		if (MoveTo(dt, arroundPointTarget, 0f, run))
		{
			if (run)
			{
				aroundPointUpdateTime = 0f;
			}
			if (!m_serpentMovement && !run)
			{
				LookAt(point);
			}
		}
	}

	private bool GetSolidHeight(Vector3 p, float maxUp, float maxDown, out float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(p + Vector3.up * maxUp, Vector3.down, ref val, maxDown, m_solidRayMask))
		{
			height = ((RaycastHit)(ref val)).point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	protected bool IsValidRandomMovePoint(Vector3 point)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (m_character.IsFlying())
		{
			return true;
		}
		if (m_avoidWater && GetSolidHeight(point, 20f, 100f, out var height))
		{
			if (m_character.IsSwimming())
			{
				if (GetSolidHeight(((Component)this).transform.position, 20f, 100f, out var height2) && height < height2)
				{
					return false;
				}
			}
			else if (height < 30f)
			{
				return false;
			}
		}
		if (m_avoidLava && ZoneSystem.instance.IsLava(point))
		{
			return false;
		}
		if ((m_afraidOfFire || m_avoidFire) && Object.op_Implicit((Object)(object)EffectArea.IsPointInsideArea(point, EffectArea.Type.Fire)))
		{
			return false;
		}
		return true;
	}

	protected virtual void OnDamaged(float damage, Character attacker)
	{
		m_timeSinceHurt = 0f;
	}

	protected virtual void OnDeath()
	{
		if (!string.IsNullOrEmpty(m_deathMessage))
		{
			MessageHud.instance.MessageAll(MessageHud.MessageType.Center, m_deathMessage);
		}
	}

	public bool CanSenseTarget(Character target)
	{
		return CanSenseTarget(target, m_passiveAggresive);
	}

	public bool CanSenseTarget(Character target, bool passiveAggresive)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return CanSenseTarget(((Component)this).transform, m_character.m_eye.position, m_hearRange, m_viewRange, m_viewAngle, IsAlerted(), m_mistVision, target, passiveAggresive, m_character.IsTamed());
	}

	public static bool CanSenseTarget(Transform me, Vector3 eyePoint, float hearRange, float viewRange, float viewAngle, bool alerted, bool mistVision, Character target, bool passiveAggresive, bool isTamed)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!passiveAggresive && ZoneSystem.instance.GetGlobalKey(GlobalKeys.PassiveMobs) && (!isTamed || !target.GetBaseAI().IsAlerted()))
		{
			return false;
		}
		if (CanHearTarget(me, hearRange, target))
		{
			return true;
		}
		if (CanSeeTarget(me, eyePoint, viewRange, viewAngle, alerted, mistVision, target))
		{
			return true;
		}
		return false;
	}

	public bool CanHearTarget(Character target)
	{
		return CanHearTarget(((Component)this).transform, m_hearRange, target);
	}

	public static bool CanHearTarget(Transform me, float hearRange, Character target)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (target.IsPlayer())
		{
			Player player = target as Player;
			if (player.InDebugFlyMode() || player.InGhostMode())
			{
				return false;
			}
		}
		float num = Vector3.Distance(((Component)target).transform.position, me.position);
		if (Character.InInterior(me))
		{
			hearRange = Mathf.Min(12f, hearRange);
		}
		if (num > hearRange)
		{
			return false;
		}
		if (num < target.GetNoiseRange())
		{
			return true;
		}
		return false;
	}

	public bool CanSeeTarget(Character target)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return CanSeeTarget(((Component)this).transform, m_character.m_eye.position, m_viewRange, m_viewAngle, IsAlerted(), m_mistVision, target);
	}

	public static bool CanSeeTarget(Transform me, Vector3 eyePoint, float viewRange, float viewAngle, bool alerted, bool mistVision, Character target)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null || (Object)(object)me == (Object)null)
		{
			return false;
		}
		if (target.IsPlayer())
		{
			Player player = target as Player;
			if (player.InDebugFlyMode() || player.InGhostMode())
			{
				return false;
			}
		}
		float num = Vector3.Distance(((Component)target).transform.position, me.position);
		if (num > viewRange)
		{
			return false;
		}
		_ = num / viewRange;
		float stealthFactor = target.GetStealthFactor();
		float num2 = viewRange * stealthFactor;
		if (num > num2)
		{
			return false;
		}
		if (!alerted && Vector3.Angle(((Component)target).transform.position - me.position, me.forward) > viewAngle)
		{
			return false;
		}
		Vector3 val = (target.IsCrouching() ? target.GetCenterPoint() : target.m_eye.position);
		Vector3 val2 = val - eyePoint;
		if (Physics.Raycast(eyePoint, ((Vector3)(ref val2)).normalized, ((Vector3)(ref val2)).magnitude, m_viewBlockMask))
		{
			return false;
		}
		if (!mistVision && ParticleMist.IsMistBlocked(eyePoint, val))
		{
			return false;
		}
		return true;
	}

	protected bool CanSeeTarget(StaticTarget target)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null)
		{
			return false;
		}
		Vector3 center = target.GetCenter();
		if (Vector3.Distance(center, ((Component)this).transform.position) > m_viewRange)
		{
			return false;
		}
		Vector3 val = center - m_character.m_eye.position;
		if (m_viewRange > 0f && !IsAlerted() && Vector3.Dot(((Component)this).transform.forward, val) < 0f)
		{
			return false;
		}
		List<Collider> allColliders = target.GetAllColliders();
		int num = Physics.RaycastNonAlloc(m_character.m_eye.position, ((Vector3)(ref val)).normalized, s_tempRaycastHits, ((Vector3)(ref val)).magnitude, m_viewBlockMask);
		for (int i = 0; i < num; i++)
		{
			RaycastHit val2 = s_tempRaycastHits[i];
			if (!allColliders.Contains(((RaycastHit)(ref val2)).collider))
			{
				return false;
			}
		}
		if (!m_mistVision && ParticleMist.IsMistBlocked(m_character.m_eye.position, center))
		{
			return false;
		}
		return true;
	}

	private void MoveTowardsSwoop(Vector3 dir, bool run, float distance)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		dir = ((Vector3)(ref dir)).normalized;
		float num = Mathf.Clamp01(Vector3.Dot(dir, ((Component)m_character).transform.forward));
		num *= num;
		float num2 = Mathf.Clamp01(distance / m_serpentTurnRadius);
		float num3 = 1f - (1f - num2) * (1f - num);
		num3 = num3 * 0.9f + 0.1f;
		Vector3 moveDir = ((Component)this).transform.forward * num3;
		LookTowards(dir);
		m_character.SetMoveDir(moveDir);
		m_character.SetRun(run);
	}

	public void MoveTowards(Vector3 dir, bool run)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		dir = ((Vector3)(ref dir)).normalized;
		LookTowards(dir);
		if (m_smoothMovement)
		{
			float num = Vector3.Angle(new Vector3(dir.x, 0f, dir.z), ((Component)this).transform.forward);
			float num2 = 1f - Mathf.Clamp01(num / m_moveMinAngle);
			Vector3 moveDir = ((Component)this).transform.forward * num2;
			moveDir.y = dir.y;
			m_character.SetMoveDir(moveDir);
			m_character.SetRun(run);
			if (m_jumpInterval > 0f && m_jumpTimer >= m_jumpInterval)
			{
				m_jumpTimer = 0f;
				m_character.Jump();
			}
		}
		else if (IsLookingTowards(dir, m_moveMinAngle))
		{
			m_character.SetMoveDir(dir);
			m_character.SetRun(run);
			if (m_jumpInterval > 0f && m_jumpTimer >= m_jumpInterval)
			{
				m_jumpTimer = 0f;
				m_character.Jump();
			}
		}
		else
		{
			StopMoving();
		}
	}

	protected void LookAt(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - m_character.m_eye.position;
		if (!(Utils.LengthXZ(val) < 0.01f))
		{
			((Vector3)(ref val)).Normalize();
			LookTowards(val);
		}
	}

	public void LookTowards(Vector3 dir)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_character.SetLookDir(dir);
	}

	protected bool IsLookingAt(Vector3 point, float minAngle, bool inverted = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - ((Component)this).transform.position;
		return IsLookingTowards(((Vector3)(ref val)).normalized, minAngle) ^ inverted;
	}

	public bool IsLookingTowards(Vector3 dir, float minAngle)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		dir.y = 0f;
		Vector3 forward = ((Component)this).transform.forward;
		forward.y = 0f;
		return Vector3.Angle(dir, forward) < minAngle;
	}

	public void StopMoving()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_character.SetMoveDir(Vector3.zero);
	}

	protected bool HavePath(Vector3 target)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (m_character.IsFlying())
		{
			return true;
		}
		return Pathfinding.instance.HavePath(((Component)this).transform.position, target, m_pathAgentType);
	}

	protected bool FindPath(Vector3 target)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		float num = time - m_lastFindPathTime;
		if (num < 1f)
		{
			return m_lastFindPathResult;
		}
		if (Vector3.Distance(target, m_lastFindPathTarget) < 1f && num < 5f)
		{
			return m_lastFindPathResult;
		}
		m_lastFindPathTarget = target;
		m_lastFindPathTime = time;
		m_lastFindPathResult = Pathfinding.instance.GetPath(((Component)this).transform.position, target, m_path, m_pathAgentType);
		return m_lastFindPathResult;
	}

	protected bool FoundPath()
	{
		return m_lastFindPathResult;
	}

	protected bool MoveTo(float dt, Vector3 point, float dist, bool run)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		if (m_character.m_flying)
		{
			dist = Mathf.Max(dist, 1f);
			if (GetSolidHeight(point, 0f, m_flyAltitudeMin * 2f, out var height))
			{
				point.y = Mathf.Max(point.y, height + m_flyAltitudeMin);
			}
			return MoveAndAvoid(dt, point, dist, run);
		}
		float num = (run ? 1f : 0.5f);
		if (m_serpentMovement)
		{
			num = 3f;
		}
		if (Utils.DistanceXZ(point, ((Component)this).transform.position) < Mathf.Max(dist, num))
		{
			StopMoving();
			return true;
		}
		if (!FindPath(point))
		{
			StopMoving();
			return true;
		}
		if (m_path.Count == 0)
		{
			StopMoving();
			return true;
		}
		Vector3 val = m_path[0];
		Vector3 val2;
		if (Utils.DistanceXZ(val, ((Component)this).transform.position) < num)
		{
			m_path.RemoveAt(0);
			if (m_path.Count == 0)
			{
				StopMoving();
				return true;
			}
		}
		else if (m_serpentMovement)
		{
			float distance = Vector3.Distance(val, ((Component)this).transform.position);
			val2 = val - ((Component)this).transform.position;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			MoveTowardsSwoop(normalized, run, distance);
		}
		else
		{
			val2 = val - ((Component)this).transform.position;
			Vector3 normalized2 = ((Vector3)(ref val2)).normalized;
			MoveTowards(normalized2, run);
		}
		return false;
	}

	protected bool MoveAndAvoid(float dt, Vector3 point, float dist, bool run)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - ((Component)this).transform.position;
		if (m_character.IsFlying())
		{
			if (((Vector3)(ref val)).magnitude < dist)
			{
				StopMoving();
				return true;
			}
		}
		else
		{
			val.y = 0f;
			if (((Vector3)(ref val)).magnitude < dist)
			{
				StopMoving();
				return true;
			}
		}
		((Vector3)(ref val)).Normalize();
		float radius = m_character.GetRadius();
		float num = radius + 1f;
		if (!m_character.InAttack())
		{
			m_getOutOfCornerTimer -= dt;
			if (m_getOutOfCornerTimer > 0f)
			{
				Vector3 dir = Quaternion.Euler(0f, m_getOutOfCornerAngle, 0f) * -val;
				MoveTowards(dir, run);
				return false;
			}
			m_stuckTimer += Time.fixedDeltaTime;
			if (m_stuckTimer > 1.5f)
			{
				if (Vector3.Distance(((Component)this).transform.position, m_lastPosition) < 0.2f)
				{
					m_getOutOfCornerTimer = 4f;
					m_getOutOfCornerAngle = Random.Range(-20f, 20f);
					m_stuckTimer = 0f;
					return false;
				}
				m_stuckTimer = 0f;
				m_lastPosition = ((Component)this).transform.position;
			}
		}
		if (CanMove(val, radius, num))
		{
			MoveTowards(val, run);
		}
		else
		{
			Vector3 forward = ((Component)this).transform.forward;
			if (m_character.IsFlying())
			{
				forward.y = 0.2f;
				((Vector3)(ref forward)).Normalize();
			}
			Vector3 val2 = ((Component)this).transform.right * radius * 0.75f;
			float num2 = num * 1.5f;
			Vector3 centerPoint = m_character.GetCenterPoint();
			float num3 = Raycast(centerPoint - val2, forward, num2, 0.1f);
			float num4 = Raycast(centerPoint + val2, forward, num2, 0.1f);
			if (num3 >= num2 && num4 >= num2)
			{
				MoveTowards(forward, run);
			}
			else
			{
				Vector3 dir2 = Quaternion.Euler(0f, -20f, 0f) * forward;
				Vector3 dir3 = Quaternion.Euler(0f, 20f, 0f) * forward;
				if (num3 > num4)
				{
					MoveTowards(dir2, run);
				}
				else
				{
					MoveTowards(dir3, run);
				}
			}
		}
		return false;
	}

	private bool CanMove(Vector3 dir, float checkRadius, float distance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Vector3 centerPoint = m_character.GetCenterPoint();
		Vector3 right = ((Component)this).transform.right;
		if (Raycast(centerPoint, dir, distance, 0.1f) < distance)
		{
			return false;
		}
		if (Raycast(centerPoint - right * (checkRadius - 0.1f), dir, distance, 0.1f) < distance)
		{
			return false;
		}
		if (Raycast(centerPoint + right * (checkRadius - 0.1f), dir, distance, 0.1f) < distance)
		{
			return false;
		}
		return true;
	}

	public float Raycast(Vector3 p, Vector3 dir, float distance, float radius)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (radius == 0f)
		{
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(p, dir, ref val, distance, m_solidRayMask))
			{
				return ((RaycastHit)(ref val)).distance;
			}
			return distance;
		}
		RaycastHit val2 = default(RaycastHit);
		if (Physics.SphereCast(p, radius, dir, ref val2, distance, m_solidRayMask))
		{
			return ((RaycastHit)(ref val2)).distance;
		}
		return distance;
	}

	public void SetAggravated(bool aggro, AggravatedReason reason)
	{
		if (m_aggravatable && m_nview.IsValid() && m_aggravated != aggro)
		{
			m_nview.InvokeRPC("SetAggravated", aggro, (int)reason);
		}
	}

	private void RPC_SetAggravated(long sender, bool aggro, int reason)
	{
		if (m_nview.IsOwner() && m_aggravated != aggro)
		{
			m_aggravated = aggro;
			m_nview.GetZDO().Set(ZDOVars.s_aggravated, m_aggravated);
			if (m_onBecameAggravated != null)
			{
				m_onBecameAggravated((AggravatedReason)reason);
			}
		}
	}

	public bool IsAggravatable()
	{
		return m_aggravatable;
	}

	public bool IsAggravated()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		if (!m_aggravatable)
		{
			return false;
		}
		if (Time.time - m_lastAggravatedCheck > 1f)
		{
			m_lastAggravatedCheck = Time.time;
			m_aggravated = m_nview.GetZDO().GetBool(ZDOVars.s_aggravated, m_aggravated);
		}
		return m_aggravated;
	}

	public bool IsEnemy(Character other)
	{
		return IsEnemy(m_character, other);
	}

	public static bool IsEnemy(Character a, Character b)
	{
		if ((Object)(object)a == (Object)(object)b)
		{
			return false;
		}
		if (!Object.op_Implicit((Object)(object)a) || !Object.op_Implicit((Object)(object)b))
		{
			return false;
		}
		string group = a.GetGroup();
		if (group.Length > 0 && group == b.GetGroup())
		{
			return false;
		}
		Character.Faction faction = a.GetFaction();
		Character.Faction faction2 = b.GetFaction();
		bool flag = a.IsTamed();
		bool flag2 = b.IsTamed();
		bool flag3 = Object.op_Implicit((Object)(object)a.GetBaseAI()) && a.GetBaseAI().IsAggravated();
		bool flag4 = Object.op_Implicit((Object)(object)b.GetBaseAI()) && b.GetBaseAI().IsAggravated();
		if (flag || flag2)
		{
			if ((flag && flag2) || (flag && faction2 == Character.Faction.Players) || (flag2 && faction == Character.Faction.Players) || (flag && faction2 == Character.Faction.Dverger && !flag4) || (flag2 && faction == Character.Faction.Dverger && !flag3))
			{
				return false;
			}
			return true;
		}
		if ((flag3 || flag4) && ((flag3 && faction2 == Character.Faction.Players) || (flag4 && faction == Character.Faction.Players)))
		{
			return true;
		}
		if (faction == faction2)
		{
			return false;
		}
		switch (faction)
		{
		case Character.Faction.AnimalsVeg:
		case Character.Faction.PlayerSpawned:
			return true;
		case Character.Faction.Players:
			return faction2 != Character.Faction.Dverger;
		case Character.Faction.ForestMonsters:
			if (faction2 != Character.Faction.AnimalsVeg)
			{
				return faction2 != Character.Faction.Boss;
			}
			return false;
		case Character.Faction.Undead:
			if (faction2 != Character.Faction.Demon)
			{
				return faction2 != Character.Faction.Boss;
			}
			return false;
		case Character.Faction.Demon:
			if (faction2 != Character.Faction.Undead)
			{
				return faction2 != Character.Faction.Boss;
			}
			return false;
		case Character.Faction.MountainMonsters:
			return faction2 != Character.Faction.Boss;
		case Character.Faction.SeaMonsters:
			return faction2 != Character.Faction.Boss;
		case Character.Faction.PlainsMonsters:
			return faction2 != Character.Faction.Boss;
		case Character.Faction.MistlandsMonsters:
			if (faction2 != Character.Faction.AnimalsVeg)
			{
				return faction2 != Character.Faction.Boss;
			}
			return false;
		case Character.Faction.Dverger:
			if (faction2 != Character.Faction.AnimalsVeg && faction2 != Character.Faction.Boss)
			{
				return faction2 != Character.Faction.Players;
			}
			return false;
		case Character.Faction.Boss:
			if (faction2 != 0)
			{
				return faction2 == Character.Faction.PlayerSpawned;
			}
			return true;
		case Character.Faction.TrainingDummy:
			return faction2 == Character.Faction.Players;
		default:
			return false;
		}
	}

	protected StaticTarget FindRandomStaticTarget(float maxDistance)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		float radius = m_character.GetRadius();
		int num = Physics.OverlapSphereNonAlloc(((Component)this).transform.position, radius + maxDistance, s_tempSphereOverlap);
		if (num == 0)
		{
			return null;
		}
		List<StaticTarget> list = new List<StaticTarget>();
		for (int i = 0; i < num; i++)
		{
			StaticTarget componentInParent = ((Component)s_tempSphereOverlap[i]).GetComponentInParent<StaticTarget>();
			if (!((Object)(object)componentInParent == (Object)null) && componentInParent.IsRandomTarget() && CanSeeTarget(componentInParent))
			{
				list.Add(componentInParent);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[Random.Range(0, list.Count)];
	}

	protected StaticTarget FindClosestStaticPriorityTarget()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		float num = ((m_viewRange > 0f) ? m_viewRange : m_hearRange);
		int num2 = Physics.OverlapSphereNonAlloc(((Component)this).transform.position, num, s_tempSphereOverlap, m_monsterTargetRayMask);
		if (num2 == 0)
		{
			return null;
		}
		StaticTarget result = null;
		float num3 = num;
		for (int i = 0; i < num2; i++)
		{
			StaticTarget componentInParent = ((Component)s_tempSphereOverlap[i]).GetComponentInParent<StaticTarget>();
			if (!((Object)(object)componentInParent == (Object)null) && componentInParent.IsPriorityTarget())
			{
				float num4 = Vector3.Distance(((Component)this).transform.position, componentInParent.GetCenter());
				if (num4 < num3 && CanSeeTarget(componentInParent))
				{
					result = componentInParent;
					num3 = num4;
				}
			}
		}
		return result;
	}

	protected void HaveFriendsInRange(float range, out Character hurtFriend, out Character friend)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		friend = HaveFriendInRange(allCharacters, range);
		hurtFriend = HaveHurtFriendInRange(allCharacters, range);
	}

	private Character HaveFriendInRange(List<Character> characters, float range)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Character character in characters)
		{
			if (!((Object)(object)character == (Object)(object)m_character) && !IsEnemy(m_character, character) && !(Vector3.Distance(((Component)character).transform.position, ((Component)this).transform.position) > range))
			{
				return character;
			}
		}
		return null;
	}

	protected Character HaveFriendInRange(float range)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		return HaveFriendInRange(allCharacters, range);
	}

	private Character HaveHurtFriendInRange(List<Character> characters, float range)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		foreach (Character character in characters)
		{
			if (!IsEnemy(m_character, character) && !(Vector3.Distance(((Component)character).transform.position, ((Component)this).transform.position) > range) && character.GetHealth() < character.GetMaxHealth())
			{
				return character;
			}
		}
		return null;
	}

	protected float StandStillDuration(float distanceTreshold)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(((Component)this).transform.position, m_lastMovementCheck) > distanceTreshold)
		{
			m_lastMovementCheck = ((Component)this).transform.position;
			m_lastMoveTime = Time.time;
		}
		return Time.time - m_lastMoveTime;
	}

	protected Character HaveHurtFriendInRange(float range)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		return HaveHurtFriendInRange(allCharacters, range);
	}

	protected Character FindEnemy()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		List<Character> allCharacters = Character.GetAllCharacters();
		Character character = null;
		float num = 99999f;
		foreach (Character item in allCharacters)
		{
			if (!IsEnemy(m_character, item) || item.IsDead() || item.m_aiSkipTarget)
			{
				continue;
			}
			BaseAI baseAI = item.GetBaseAI();
			if ((!((Object)(object)baseAI != (Object)null) || !baseAI.IsSleeping()) && CanSenseTarget(item))
			{
				float num2 = Vector3.Distance(((Component)item).transform.position, ((Component)this).transform.position);
				if (num2 < num || (Object)(object)character == (Object)null)
				{
					character = item;
					num = num2;
				}
			}
		}
		if ((Object)(object)character == (Object)null && HuntPlayer())
		{
			Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, 200f);
			if (Object.op_Implicit((Object)(object)closestPlayer) && (closestPlayer.InDebugFlyMode() || closestPlayer.InGhostMode()))
			{
				return null;
			}
			return closestPlayer;
		}
		return character;
	}

	public static Character FindClosestCreature(Transform me, Vector3 eyePoint, float hearRange, float viewRange, float viewAngle, bool alerted, bool mistVision, bool passiveAggresive, bool includePlayers = true, bool includeTamed = true, bool includeEnemies = true, List<Character> onlyTargets = null)
	{
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		List<Character> allCharacters = Character.GetAllCharacters();
		Character character = null;
		float num = 99999f;
		if (!includeEnemies && ZoneSystem.instance.GetGlobalKey(GlobalKeys.PassiveMobs))
		{
			WearNTear component = ((Component)me).GetComponent<WearNTear>();
			if (component != null && component.GetHealthPercentage() == 1f)
			{
				return null;
			}
		}
		foreach (Character item in allCharacters)
		{
			bool flag = item is Player;
			if ((!includePlayers && flag) || (!includeEnemies && !flag) || (!includeTamed && item.IsTamed()))
			{
				continue;
			}
			if (onlyTargets != null && onlyTargets.Count > 0)
			{
				bool flag2 = false;
				foreach (Character onlyTarget in onlyTargets)
				{
					if (item.m_name == onlyTarget.m_name)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					continue;
				}
			}
			if (item.IsDead())
			{
				continue;
			}
			BaseAI baseAI = item.GetBaseAI();
			if ((!((Object)(object)baseAI != (Object)null) || !baseAI.IsSleeping()) && CanSenseTarget(me, eyePoint, hearRange, viewRange, viewAngle, alerted, mistVision, item, passiveAggresive, isTamed: false))
			{
				float num2 = Vector3.Distance(((Component)item).transform.position, me.position);
				if (num2 < num || (Object)(object)character == (Object)null)
				{
					character = item;
					num = num2;
				}
			}
		}
		return character;
	}

	public void SetHuntPlayer(bool hunt)
	{
		if (m_huntPlayer != hunt)
		{
			m_huntPlayer = hunt;
			if (m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_huntPlayer, m_huntPlayer);
			}
		}
	}

	public virtual bool HuntPlayer()
	{
		return m_huntPlayer;
	}

	protected bool HaveAlertedCreatureInRange(float range)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		foreach (BaseAI instance in m_instances)
		{
			if (Vector3.Distance(((Component)this).transform.position, ((Component)instance).transform.position) < range && instance.IsAlerted())
			{
				return true;
			}
		}
		return false;
	}

	public static void DoProjectileHitNoise(Vector3 center, float range, Character attacker)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		foreach (BaseAI instance in m_instances)
		{
			if ((!Object.op_Implicit((Object)(object)attacker) || instance.IsEnemy(attacker)) && Vector3.Distance(((Component)instance).transform.position, center) < range && Object.op_Implicit((Object)(object)instance.m_nview) && instance.m_nview.IsValid())
			{
				instance.m_nview.InvokeRPC("OnNearProjectileHit", center, range, Object.op_Implicit((Object)(object)attacker) ? attacker.GetZDOID() : ZDOID.None);
			}
		}
	}

	protected virtual void RPC_OnNearProjectileHit(long sender, Vector3 center, float range, ZDOID attacker)
	{
		if (!ZoneSystem.instance.GetGlobalKey(GlobalKeys.PassiveMobs))
		{
			Alert();
		}
	}

	public void Alert()
	{
		if (m_nview.IsValid() && !IsAlerted())
		{
			if (m_nview.IsOwner())
			{
				SetAlerted(alert: true);
			}
			else
			{
				m_nview.InvokeRPC("Alert");
			}
		}
	}

	private void RPC_Alert(long sender)
	{
		if (m_nview.IsOwner())
		{
			SetAlerted(alert: true);
		}
	}

	protected virtual void SetAlerted(bool alert)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (m_alerted != alert)
		{
			m_alerted = alert;
			m_animator.SetBool("alert", m_alerted);
			if (m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_alert, m_alerted);
			}
			if (m_alerted)
			{
				m_alertedEffects.Create(((Component)this).transform.position, Quaternion.identity);
			}
			if (m_character.IsBoss() && !m_nview.GetZDO().GetBool("bosscount"))
			{
				ZoneSystem.instance.GetGlobalKey(GlobalKeys.activeBosses, out float value);
				ZoneSystem.instance.SetGlobalKey(GlobalKeys.activeBosses, value + (float)(alert ? 1 : (-1)));
				m_nview.GetZDO().Set("bosscount", value: true);
			}
			if (alert && m_alertedMessage.Length > 0 && !m_nview.GetZDO().GetBool(ZDOVars.s_shownAlertMessage))
			{
				m_nview.GetZDO().Set(ZDOVars.s_shownAlertMessage, value: true);
				MessageHud.instance.MessageAll(MessageHud.MessageType.Center, m_alertedMessage);
			}
		}
	}

	public static bool InStealthRange(Character me)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		foreach (BaseAI baseAIInstance in BaseAIInstances)
		{
			if (!IsEnemy(me, baseAIInstance.m_character))
			{
				continue;
			}
			float num = Vector3.Distance(((Component)me).transform.position, ((Component)baseAIInstance).transform.position);
			if (num < baseAIInstance.m_viewRange || num < 10f)
			{
				if (baseAIInstance.IsAlerted())
				{
					return false;
				}
				result = true;
			}
		}
		return result;
	}

	public static bool HaveEnemyInRange(Character me, Vector3 point, float range)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		foreach (Character allCharacter in Character.GetAllCharacters())
		{
			if (IsEnemy(me, allCharacter) && Vector3.Distance(((Component)allCharacter).transform.position, point) < range)
			{
				return true;
			}
		}
		return false;
	}

	public static Character FindClosestEnemy(Character me, Vector3 point, float maxDistance)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Character character = null;
		float num = maxDistance;
		foreach (Character allCharacter in Character.GetAllCharacters())
		{
			if (IsEnemy(me, allCharacter))
			{
				float num2 = Vector3.Distance(((Component)allCharacter).transform.position, point);
				if ((Object)(object)character == (Object)null || num2 < num)
				{
					character = allCharacter;
					num = num2;
				}
			}
		}
		return character;
	}

	public static Character FindRandomEnemy(Character me, Vector3 point, float maxDistance)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		List<Character> list = new List<Character>();
		foreach (Character allCharacter in Character.GetAllCharacters())
		{
			if (IsEnemy(me, allCharacter) && Vector3.Distance(((Component)allCharacter).transform.position, point) < maxDistance)
			{
				list.Add(allCharacter);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[Random.Range(0, list.Count)];
	}

	public bool IsAlerted()
	{
		return m_alerted;
	}

	protected void SetTargetInfo(ZDOID targetID)
	{
		m_nview.GetZDO().Set(ZDOVars.s_haveTargetHash, !targetID.IsNone());
	}

	public bool HaveTarget()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		return m_nview.GetZDO().GetBool(ZDOVars.s_haveTargetHash);
	}

	private float GetAltitude()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)this).transform.position, Vector3.down, ref val, (float)m_solidRayMask))
		{
			return ((Component)m_character).transform.position.y - ((RaycastHit)(ref val)).point.y;
		}
		return 1000f;
	}

	public static List<BaseAI> GetAllInstances()
	{
		return m_instances;
	}

	protected virtual void OnDrawGizmosSelected()
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		if (m_lastFindPathResult)
		{
			Gizmos.color = Color.yellow;
			for (int i = 0; i < m_path.Count - 1; i++)
			{
				Vector3 val = m_path[i];
				Vector3 val2 = m_path[i + 1];
				Gizmos.DrawLine(val + Vector3.up * 0.1f, val2 + Vector3.up * 0.1f);
			}
			Gizmos.color = Color.cyan;
			foreach (Vector3 item in m_path)
			{
				Gizmos.DrawSphere(item + Vector3.up * 0.1f, 0.1f);
			}
			Gizmos.color = Color.green;
			Gizmos.DrawLine(((Component)this).transform.position, m_lastFindPathTarget);
			Gizmos.DrawSphere(m_lastFindPathTarget, 0.2f);
		}
		else
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(((Component)this).transform.position, m_lastFindPathTarget);
			Gizmos.DrawSphere(m_lastFindPathTarget, 0.2f);
		}
	}

	public virtual bool IsSleeping()
	{
		return false;
	}

	public bool HasZDOOwner()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		return m_nview.GetZDO().HasOwner();
	}

	public bool CanUseAttack(ItemDrop.ItemData item)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (item.m_shared.m_aiInDungeonOnly && !m_character.InInterior())
		{
			return false;
		}
		if (item.m_shared.m_aiMaxHealthPercentage < 1f && m_character.GetHealthPercentage() > item.m_shared.m_aiMaxHealthPercentage)
		{
			return false;
		}
		if (item.m_shared.m_aiMinHealthPercentage > 0f && m_character.GetHealthPercentage() < item.m_shared.m_aiMinHealthPercentage)
		{
			return false;
		}
		bool flag = m_character.IsFlying();
		bool flag2 = m_character.IsSwimming();
		if (item.m_shared.m_aiWhenFlying && flag)
		{
			float altitude = GetAltitude();
			if (altitude > item.m_shared.m_aiWhenFlyingAltitudeMin)
			{
				return altitude < item.m_shared.m_aiWhenFlyingAltitudeMax;
			}
			return false;
		}
		if (item.m_shared.m_aiInMistOnly && !ParticleMist.IsInMist(m_character.GetCenterPoint()))
		{
			return false;
		}
		if (item.m_shared.m_aiWhenWalking && !flag && !flag2)
		{
			return true;
		}
		if (item.m_shared.m_aiWhenSwiming && flag2)
		{
			return true;
		}
		return false;
	}

	public virtual Character GetTargetCreature()
	{
		return null;
	}

	public bool HaveRider()
	{
		if (Object.op_Implicit((Object)(object)m_tamable))
		{
			return m_tamable.HaveRider();
		}
		return false;
	}

	public float GetRiderSkill()
	{
		if (Object.op_Implicit((Object)(object)m_tamable))
		{
			return m_tamable.GetRiderSkill();
		}
		return 0f;
	}

	public static void AggravateAllInArea(Vector3 point, float radius, AggravatedReason reason)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		foreach (BaseAI baseAIInstance in BaseAIInstances)
		{
			if (baseAIInstance.IsAggravatable() && !(Vector3.Distance(point, ((Component)baseAIInstance).transform.position) > radius))
			{
				baseAIInstance.SetAggravated(aggro: true, reason);
				baseAIInstance.Alert();
			}
		}
	}

	public void ChargeStart(string animBool)
	{
		if (!IsCharging())
		{
			m_character.GetZAnim().SetBool(animBool, value: true);
			m_charging = animBool;
		}
	}

	public void ChargeStop()
	{
		if (IsCharging())
		{
			m_character.GetZAnim().SetBool(m_charging, value: false);
			m_charging = null;
		}
	}

	public bool IsCharging()
	{
		return m_charging != null;
	}
}
