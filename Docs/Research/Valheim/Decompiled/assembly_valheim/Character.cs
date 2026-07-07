using System;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IDestructible, Hoverable, IWaterInteractable, IMonoUpdater
{
	public enum Faction
	{
		Players,
		AnimalsVeg,
		ForestMonsters,
		Undead,
		Demon,
		MountainMonsters,
		SeaMonsters,
		PlainsMonsters,
		Boss,
		MistlandsMonsters,
		Dverger,
		PlayerSpawned,
		TrainingDummy
	}

	public enum GroundTiltType
	{
		None,
		Pitch,
		Full,
		PitchRaycast,
		FullRaycast,
		Flying
	}

	private float m_underWorldCheckTimer;

	private static float takeInputDelay = 0f;

	private float currentRotSpeedFactor;

	private float m_standUp;

	private const float c_StandUpTime = 2f;

	private const float c_StandUpTriggerState = -100f;

	private Collider m_lowestContactCollider;

	private bool m_groundContact;

	private Vector3 m_groundContactPoint = Vector3.zero;

	private Vector3 m_groundContactNormal = Vector3.zero;

	private Transform oldParent;

	private int m_cachedCurrentAnimHash;

	private int m_cachedNextAnimHash;

	private int m_cachedNextOrCurrentAnimHash;

	private int m_cachedAnimHashFrame;

	public ZNetView m_nViewOverride;

	public Action<float, Character> m_onDamaged;

	public Action m_onDeath;

	public Action<int> m_onLevelSet;

	public Action<Vector3> m_onLand;

	[Header("Character")]
	public string m_name = "";

	public string m_group = "";

	public Faction m_faction = Faction.AnimalsVeg;

	public bool m_boss;

	public bool m_dontHideBossHud;

	public string m_bossEvent = "";

	[Tooltip("Also sets player unique key")]
	public string m_defeatSetGlobalKey = "";

	public bool m_aiSkipTarget;

	[Header("Movement & Physics")]
	public float m_crouchSpeed = 2f;

	public float m_walkSpeed = 5f;

	public float m_speed = 10f;

	public float m_turnSpeed = 300f;

	public float m_runSpeed = 20f;

	public float m_runTurnSpeed = 300f;

	public float m_flySlowSpeed = 5f;

	public float m_flyFastSpeed = 12f;

	public float m_flyTurnSpeed = 12f;

	public float m_acceleration = 1f;

	public float m_jumpForce = 10f;

	public float m_jumpForceForward;

	public float m_jumpForceTiredFactor = 0.7f;

	public float m_airControl = 0.1f;

	public bool m_canSwim = true;

	public float m_swimDepth = 2f;

	public float m_swimSpeed = 2f;

	public float m_swimTurnSpeed = 100f;

	public float m_swimAcceleration = 0.05f;

	public GroundTiltType m_groundTilt;

	public float m_groundTiltSpeed = 50f;

	public bool m_flying;

	public float m_jumpStaminaUsage = 10f;

	public bool m_disableWhileSleeping;

	[Header("Bodyparts")]
	public Transform m_eye;

	protected Transform m_head;

	[Header("Effects")]
	public EffectList m_hitEffects = new EffectList();

	public EffectList m_critHitEffects = new EffectList();

	public EffectList m_backstabHitEffects = new EffectList();

	public EffectList m_deathEffects = new EffectList();

	public EffectList m_waterEffects = new EffectList();

	public EffectList m_tarEffects = new EffectList();

	public EffectList m_slideEffects = new EffectList();

	public EffectList m_jumpEffects = new EffectList();

	public EffectList m_flyingContinuousEffect = new EffectList();

	public EffectList m_pheromoneLoveEffect = new EffectList();

	public bool m_useAltStatusEffectScaling;

	[Header("Health & Damage")]
	public bool m_tolerateWater = true;

	public bool m_tolerateFire;

	public bool m_tolerateSmoke = true;

	public bool m_tolerateTar;

	public float m_health = 10f;

	public float m_regenAllHPTime = 3600f;

	public HitData.DamageModifiers m_damageModifiers;

	public WeakSpot[] m_weakSpots;

	public bool m_staggerWhenBlocked = true;

	public float m_staggerDamageFactor;

	public float m_enemyAdrenalineMultiplier = 1f;

	private float m_staggerDamage;

	private float m_backstabTime = -99999f;

	private GameObject[] m_waterEffects_instances;

	private GameObject[] m_slideEffects_instances;

	private GameObject[] m_flyingEffects_instances;

	protected Vector3 m_moveDir = Vector3.zero;

	protected Vector3 m_lookDir = Vector3.forward;

	protected Quaternion m_lookYaw = Quaternion.identity;

	protected bool m_run;

	protected bool m_walk;

	private Vector3 m_lookTransitionStart;

	private Vector3 m_lookTransitionTarget;

	protected float m_lookTransitionTime;

	protected float m_lookTransitionTimeTotal;

	protected bool m_attack;

	protected bool m_attackHold;

	protected bool m_secondaryAttack;

	protected bool m_secondaryAttackHold;

	protected bool m_blocking;

	protected GameObject m_visual;

	protected LODGroup m_lodGroup;

	protected Rigidbody m_body;

	protected CapsuleCollider m_collider;

	protected ZNetView m_nview;

	protected ZSyncAnimation m_zanim;

	protected Animator m_animator;

	protected CharacterAnimEvent m_animEvent;

	protected BaseAI m_baseAI;

	private const float c_MaxFallHeight = 20f;

	private const float c_MinFallHeight = 4f;

	private const float c_MaxFallDamage = 100f;

	private const float c_StaggerDamageBonus = 2f;

	private const float c_AutoJumpInterval = 0.5f;

	private const float c_MinSlideDegreesPlayer = 38f;

	private const float c_MinSlideDegreesMount = 45f;

	private const float c_MinSlideDegreesMonster = 90f;

	private const float c_RootMotionMultiplier = 55f;

	private const float c_PushForceScale = 2.5f;

	private const float c_ContinuousPushForce = 20f;

	private const float c_PushForceDissipation = 100f;

	private const float c_MaxMoveForce = 20f;

	private const float c_StaggerResetTime = 5f;

	private const float c_BackstabResetTime = 300f;

	private const float m_slopeStaminaDrain = 10f;

	public const float m_minSlideDegreesPlayer = 38f;

	public const float m_minSlideDegreesMount = 45f;

	public const float m_minSlideDegreesMonster = 90f;

	private const float m_rootMotionMultiplier = 55f;

	private const float m_pushForceScale = 2.5f;

	private const float m_continousPushForce = 20f;

	private const float m_pushForcedissipation = 100f;

	private const float m_maxMoveForce = 20f;

	private const float m_staggerResetTime = 5f;

	private const float m_backstabResetTime = 300f;

	private float m_jumpTimer;

	private float m_lastAutoJumpTime;

	private float m_lastGroundTouch;

	private Vector3 m_lastGroundNormal = Vector3.up;

	private Vector3 m_lastGroundPoint = Vector3.up;

	private Collider m_lastGroundCollider;

	private Rigidbody m_lastGroundBody;

	private float m_groundForceTimer;

	private float m_originalMass;

	private Vector3 m_lastAttachPos = Vector3.zero;

	private Rigidbody m_lastAttachBody;

	protected float m_maxAirAltitude = -10000f;

	private float m_waterLevel = -10000f;

	private float m_tarLevel = -10000f;

	private float m_liquidLevel = -10000f;

	private float m_swimTimer = 999f;

	private float m_lavaTimer = 999f;

	private float m_aboveOrInLavaTimer = 999f;

	private float m_fallTimer;

	protected SEMan m_seman;

	private float m_noiseRange;

	private float m_syncNoiseTimer;

	private bool m_tamed;

	private float m_lastTamedCheck;

	private Tameable m_tameable;

	private MonsterAI m_tameableMonsterAI;

	private int m_level = 1;

	private RaycastHit[] m_lavaRoofCheck = (RaycastHit[])(object)new RaycastHit[1];

	private bool m_localPlayerHasHit;

	protected HitData m_lastHit;

	private Vector3 m_currentVel = Vector3.zero;

	private float m_currentTurnVel;

	private float m_currentTurnVelChange;

	private Vector3 m_groundTiltNormal = Vector3.up;

	protected Vector3 m_pushForce = Vector3.zero;

	private Vector3 m_rootMotion = Vector3.zero;

	private static readonly int s_forwardSpeed = ZSyncAnimation.GetHash("forward_speed");

	private static readonly int s_sidewaySpeed = ZSyncAnimation.GetHash("sideway_speed");

	private static readonly int s_turnSpeed = ZSyncAnimation.GetHash("turn_speed");

	private static readonly int s_inWater = ZSyncAnimation.GetHash("inWater");

	private static readonly int s_onGround = ZSyncAnimation.GetHash("onGround");

	private static readonly int s_encumbered = ZSyncAnimation.GetHash("encumbered");

	private static readonly int s_flying = ZSyncAnimation.GetHash("flying");

	private float m_slippage;

	protected bool m_wallRunning;

	private bool m_sliding;

	private bool m_running;

	private bool m_walking;

	private Vector3 m_originalLocalRef;

	private bool m_lodVisible = true;

	private static int s_smokeRayMask = 0;

	private float m_smokeCheckTimer;

	[Header("Heat & Lava")]
	private float m_minLavaMaskThreshold = 0.05f;

	public float m_heatBuildupBase = 1.5f;

	public float m_heatCooldownBase = 1f;

	public float m_heatBuildupWater = 2f;

	public float m_heatWaterTouchMultiplier = 0.2f;

	public float m_lavaDamageTickInterval = 0.2f;

	public float m_heatLevelFirstDamageThreshold = 0.7f;

	public float m_lavaFirstDamage = 10f;

	public float m_lavaFullDamage = 100f;

	public float m_lavaAirDamageHeight = 3f;

	public float m_dayHeatGainRunning = 0.2f;

	public float m_dayHeatGainStill = -0.05f;

	public float m_dayHeatEquipmentStop = 0.5f;

	public float m_lavaSlowMax = 0.5f;

	public float m_lavaSlowHeight = 0.8f;

	public EffectList m_lavaHeatEffects = new EffectList();

	private Dictionary<ParticleSystem, float> m_lavaHeatParticles = new Dictionary<ParticleSystem, float>();

	private List<ZSFX> m_lavaHeatAudio = new List<ZSFX>();

	private float m_lavaHeatLevel;

	private float m_lavaProximity;

	private float m_lavaHeightFactor;

	private float m_lavaDamageTimer;

	private static bool s_dpsDebugEnabled = false;

	private static readonly List<KeyValuePair<float, float>> s_enemyDamage = new List<KeyValuePair<float, float>>();

	private static readonly List<KeyValuePair<float, float>> s_playerDamage = new List<KeyValuePair<float, float>>();

	private static readonly List<Character> s_characters = new List<Character>();

	private static int s_characterLayer = 0;

	private static int s_characterNetLayer = 0;

	private static int s_characterGhostLayer = 0;

	protected static int s_groundRayMask = 0;

	protected static int s_characterLayerMask = 0;

	private static int s_blockedRayMask;

	private float m_pheromoneTimer = 5f;

	private float m_cashedInLiquidDepth;

	private Quaternion m_tiltRotCached = Quaternion.identity;

	private Vector3 m_bodyVelocityCached = Vector3.negativeInfinity;

	protected static readonly int s_animatorTagFreeze = ZSyncAnimation.GetHash("freeze");

	protected static readonly int s_animatorTagStagger = ZSyncAnimation.GetHash("stagger");

	protected static readonly int s_animatorTagSitting = ZSyncAnimation.GetHash("sitting");

	private static readonly int s_animatorFalling = ZSyncAnimation.GetHash("falling");

	private static readonly int s_tilt = ZSyncAnimation.GetHash("tilt");

	public static int m_debugFlySpeed = 20;

	private readonly int[] m_liquids = new int[2];

	public int InNumShipVolumes { get; set; }

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	protected virtual void Awake()
	{
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		s_characters.Add(this);
		m_collider = ((Component)this).GetComponent<CapsuleCollider>();
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_zanim = ((Component)this).GetComponent<ZSyncAnimation>();
		m_nview = (((Object)(object)m_nViewOverride != (Object)null) ? m_nViewOverride : ((Component)this).GetComponent<ZNetView>());
		m_animator = ((Component)this).GetComponentInChildren<Animator>();
		m_animEvent = ((Component)m_animator).GetComponent<CharacterAnimEvent>();
		m_baseAI = ((Component)this).GetComponent<BaseAI>();
		m_animator.logWarnings = false;
		Transform val = ((Component)this).transform.Find("Visual");
		m_visual = ((Component)val).gameObject;
		m_lodGroup = m_visual.GetComponent<LODGroup>();
		m_head = Utils.GetBoneTransform(m_animator, (HumanBodyBones)10);
		m_body.maxDepenetrationVelocity = 2f;
		m_originalMass = m_body.mass;
		if (s_smokeRayMask == 0)
		{
			s_smokeRayMask = LayerMask.GetMask(new string[1] { "smoke" });
			s_characterLayer = LayerMask.NameToLayer("character");
			s_characterNetLayer = LayerMask.NameToLayer("character_net");
			s_characterGhostLayer = LayerMask.NameToLayer("character_ghost");
			s_groundRayMask = LayerMask.GetMask(new string[7] { "Default", "static_solid", "Default_small", "piece", "terrain", "blocker", "vehicle" });
			s_characterLayerMask = LayerMask.GetMask(new string[2] { "character", "character_noenv" });
			s_blockedRayMask = LayerMask.GetMask(new string[5] { "piece", "Default", "static_solid", "Default_small", "terrain" });
		}
		if (Object.op_Implicit((Object)(object)m_lodGroup))
		{
			m_originalLocalRef = m_lodGroup.localReferencePoint;
		}
		m_seman = new SEMan(this, m_nview);
		if (m_nview.GetZDO() != null)
		{
			if (!IsPlayer())
			{
				m_tamed = m_nview.GetZDO().GetBool(ZDOVars.s_tamed, m_tamed);
				m_level = m_nview.GetZDO().GetInt(ZDOVars.s_level, 1);
				if (m_nview.IsOwner() && GetHealth() == GetMaxHealth())
				{
					SetupMaxHealth();
				}
			}
			m_nview.Register<HitData>("RPC_Damage", RPC_Damage);
			m_nview.Register<float>("RPC_AddAdrenaline", RPC_AddAdrenaline);
			m_nview.Register<float, bool>("RPC_Heal", RPC_Heal);
			m_nview.Register<float>("RPC_AddNoise", RPC_AddNoise);
			m_nview.Register<Vector3>("RPC_Stagger", RPC_Stagger);
			m_nview.Register("RPC_ResetCloth", RPC_ResetCloth);
			m_nview.Register<bool>("RPC_SetTamed", RPC_SetTamed);
			m_nview.Register<float>("RPC_FreezeFrame", RPC_FreezeFrame);
			m_nview.Register<Vector3, Quaternion, bool>("RPC_TeleportTo", RPC_TeleportTo);
		}
		if (!IsPlayer())
		{
			if (Game.m_enemySpeedSize != 1f && !InInterior())
			{
				Transform transform = ((Component)this).transform;
				transform.localScale *= Game.m_enemySpeedSize;
			}
			if (Game.m_worldLevel > 0)
			{
				Transform transform2 = ((Component)this).transform;
				transform2.localScale *= 1f + (float)Game.m_worldLevel * Game.instance.m_worldLevelEnemyMoveSpeedMultiplier;
			}
		}
	}

	protected virtual void OnEnable()
	{
		Instances.Add(this);
	}

	protected virtual void OnDisable()
	{
		Instances.Remove(this);
	}

	private void SetupMaxHealth()
	{
		int level = GetLevel();
		SetMaxHealth(GetMaxHealthBase() * (float)level);
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnDestroy()
	{
		m_seman.OnDestroy();
		s_characters.Remove(this);
		if ((Object)(object)EnemyHud.instance != (Object)null)
		{
			EnemyHud.instance.RemoveCharacterHud(this);
		}
	}

	public void SetLevel(int level)
	{
		if (level >= 1)
		{
			m_level = level;
			m_nview.GetZDO().Set(ZDOVars.s_level, level);
			SetupMaxHealth();
			if (m_onLevelSet != null)
			{
				m_onLevelSet(m_level);
			}
		}
	}

	public int GetLevel()
	{
		return m_level;
	}

	public virtual bool IsPlayer()
	{
		return false;
	}

	public Faction GetFaction()
	{
		return m_faction;
	}

	public string GetGroup()
	{
		return m_group;
	}

	public virtual void CustomFixedUpdate(float dt)
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return;
		}
		ZDO zDO = m_nview.GetZDO();
		bool num = zDO.IsOwner();
		bool visible = zDO.HasOwner();
		CalculateLiquidDepth();
		UpdateLayer();
		UpdateContinousEffects();
		UpdateWater(dt);
		UpdateGroundTilt(dt);
		SetVisible(visible);
		UpdateLookTransition(dt);
		if (num)
		{
			UpdateGroundContact(dt);
			UpdateNoise(dt);
			m_seman.Update(zDO, dt);
			UpdateStagger(dt);
			UpdatePushback(dt);
			UpdateMotion(dt);
			UpdateSmoke(dt);
			UpdateLava(dt);
			UpdateAshlandsWater(dt);
			UpdateHeatDamage(dt);
			UnderWorldCheck(dt);
			UpdatePheromones(dt);
			SyncVelocity();
			if (m_groundForceTimer > 0f)
			{
				m_groundForceTimer -= dt;
			}
			CheckDeath();
		}
		UpdateHeatEffects(dt);
		if (IsPlayer() && Terminal.m_showTests)
		{
			Dictionary<string, string> testList = Terminal.m_testList;
			Vector2i zone = ZoneSystem.GetZone(((Component)this).transform.position);
			testList["Player.Zone"] = ((object)(Vector2i)(ref zone)).ToString();
		}
	}

	private void UpdateLayer()
	{
		int layer = ((Component)m_collider).gameObject.layer;
		if (layer == s_characterLayer || layer == s_characterNetLayer)
		{
			int num = s_characterNetLayer;
			if (m_nview.IsOwner() && !IsAttached())
			{
				num = s_characterLayer;
			}
			if (layer != num)
			{
				((Component)m_collider).gameObject.layer = num;
			}
		}
		if (m_disableWhileSleeping)
		{
			if (Object.op_Implicit((Object)(object)m_baseAI) && m_baseAI.IsSleeping())
			{
				m_body.isKinematic = true;
			}
			else
			{
				m_body.isKinematic = false;
			}
		}
	}

	private void UnderWorldCheck(float dt)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (IsDead())
		{
			return;
		}
		m_underWorldCheckTimer += dt;
		if (m_underWorldCheckTimer > 5f || IsPlayer())
		{
			m_underWorldCheckTimer = 0f;
			float groundHeight = ZoneSystem.instance.GetGroundHeight(((Component)this).transform.position);
			if (((Component)this).transform.position.y < groundHeight - 1f)
			{
				Vector3 position = ((Component)this).transform.position;
				position.y = groundHeight + 0.5f;
				((Component)this).transform.position = position;
				m_body.position = position;
				m_body.linearVelocity = Vector3.zero;
			}
		}
	}

	private void UpdatePheromones(float dt)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_pheromoneTimer >= 0f))
		{
			return;
		}
		m_pheromoneTimer -= dt;
		if (!(m_pheromoneTimer <= 0f) || !m_pheromoneLoveEffect.HasEffects())
		{
			return;
		}
		m_pheromoneTimer = 5f;
		foreach (Player allPlayer in Player.GetAllPlayers())
		{
			foreach (StatusEffect statusEffect in allPlayer.GetSEMan().GetStatusEffects())
			{
				if (statusEffect is SE_Stats sE_Stats && (Object)(object)sE_Stats.m_pheromoneTarget != (Object)null && sE_Stats.m_pheromoneTarget.GetComponent<Character>().m_name == m_name)
				{
					m_pheromoneLoveEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
					if (GetBaseAI() is MonsterAI monsterAI)
					{
						monsterAI.Alert();
					}
				}
			}
		}
	}

	private void UpdateSmoke(float dt)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (m_tolerateSmoke)
		{
			return;
		}
		m_smokeCheckTimer += dt;
		if (m_smokeCheckTimer > 2f)
		{
			m_smokeCheckTimer = 0f;
			if (Physics.CheckSphere(GetTopPoint() + Vector3.up * 0.1f, 0.5f, s_smokeRayMask))
			{
				m_seman.AddStatusEffect(SEMan.s_statusEffectSmoked, resetTime: true);
			}
			else
			{
				m_seman.RemoveStatusEffect(SEMan.s_statusEffectSmoked, quiet: true);
			}
		}
	}

	private void UpdateLava(float dt)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		m_lavaTimer += dt;
		m_aboveOrInLavaTimer += dt;
		if (!WorldGenerator.IsAshlands(((Component)this).transform.position.x, ((Component)this).transform.position.z))
		{
			return;
		}
		Vector3 p = ((Component)this).transform.position;
		m_lavaProximity = 0f;
		ZoneSystem.instance.GetGroundData(ref p, out var _, out var biome, out var _, out var hmap);
		if ((Object)(object)hmap != (Object)null)
		{
			m_lavaProximity = Mathf.Min(1f, Utils.SmoothStep(0.1f, 1f, hmap.GetLava(p)));
		}
		if (m_lavaProximity > m_minLavaMaskThreshold)
		{
			m_aboveOrInLavaTimer = 0f;
		}
		m_lavaHeightFactor = ((Component)this).transform.position.y - p.y;
		m_lavaHeightFactor = (m_lavaAirDamageHeight - m_lavaHeightFactor) / m_lavaAirDamageHeight;
		bool flag = false;
		RaycastHit val = default(RaycastHit);
		if (m_lavaProximity > m_minLavaMaskThreshold && Physics.Raycast(((Component)this).transform.position + Vector3.up, Vector3.down, ref val, 50f, s_blockedRayMask) && (Object)(object)((Component)((RaycastHit)(ref val)).collider).GetComponent<Heightmap>() == (Object)null)
		{
			flag = true;
		}
		if (!flag && IsRiding())
		{
			flag = true;
		}
		float num = 1f - GetEquipmentHeatResistanceModifier();
		if (Terminal.m_showTests && IsPlayer())
		{
			Terminal.m_testList["Lava/Height/Resist"] = m_lavaProximity.ToString("0.00") + " / " + m_lavaHeightFactor.ToString("0.00") + " / " + num.ToString("0.00");
		}
		if (m_lavaProximity > m_minLavaMaskThreshold && m_lavaHeightFactor > 0f && !flag)
		{
			m_lavaHeatLevel += m_lavaProximity * dt * m_heatBuildupBase * m_lavaHeightFactor * num;
			m_lavaTimer = 0f;
		}
		else if (biome == Heightmap.Biome.AshLands && m_dayHeatGainRunning != 0f && IsPlayer() && EnvMan.IsDay() && !IsUnderRoof() && GetEquipmentHeatResistanceModifier() < m_dayHeatEquipmentStop)
		{
			if (((Vector3)(ref m_currentVel)).magnitude > 0.1f && IsOnGround())
			{
				m_lavaHeatLevel += dt * m_dayHeatGainRunning * num;
			}
			else if (!InWater())
			{
				m_lavaHeatLevel += dt * m_dayHeatGainStill;
			}
			if (m_lavaHeatLevel > m_heatLevelFirstDamageThreshold)
			{
				m_lavaHeatLevel = m_heatLevelFirstDamageThreshold;
			}
		}
		else if (!InWater())
		{
			m_lavaHeatLevel -= dt * m_heatCooldownBase;
		}
		if (m_tolerateFire)
		{
			m_lavaHeatLevel = 0f;
		}
		else
		{
			m_lavaHeatLevel = Mathf.Clamp(m_lavaHeatLevel, 0f, 1f);
		}
	}

	private void UpdateHeatEffects(float dt)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (!((Object)(object)Player.m_localPlayer == (Object)(object)this))
		{
			return;
		}
		((Behaviour)GameCamera.instance.m_heatDistortImageEffect).enabled = m_lavaHeatLevel > 0f;
		GameCamera.instance.m_heatDistortImageEffect.m_intensity = (flag ? 0f : m_lavaHeatLevel);
		if (!m_lavaHeatEffects.HasEffects())
		{
			return;
		}
		if (m_lavaHeatLevel > 0f && m_lavaHeatParticles.Count == 0 && !IsDead())
		{
			GameObject[] array = m_lavaHeatEffects.Create(((Component)this).transform.position, Quaternion.identity, ((Component)this).transform);
			foreach (KeyValuePair<ParticleSystem, float> lavaHeatParticle in m_lavaHeatParticles)
			{
				Object.Destroy((Object)(object)((Component)lavaHeatParticle.Key).gameObject);
			}
			foreach (ZSFX item2 in m_lavaHeatAudio)
			{
				Object.Destroy((Object)(object)((Component)item2).gameObject);
			}
			m_lavaHeatParticles.Clear();
			m_lavaHeatAudio.Clear();
			GameObject[] array2 = array;
			foreach (GameObject val in array2)
			{
				ParticleSystem[] componentsInChildren = val.GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem val2 in componentsInChildren)
				{
					m_lavaHeatParticles.Add(val2, val2.emissionRate);
				}
				ZSFX[] componentsInChildren2 = val.GetComponentsInChildren<ZSFX>();
				foreach (ZSFX item in componentsInChildren2)
				{
					m_lavaHeatAudio.Add(item);
				}
			}
		}
		foreach (KeyValuePair<ParticleSystem, float> lavaHeatParticle2 in m_lavaHeatParticles)
		{
			if ((Object)(object)lavaHeatParticle2.Key != (Object)null)
			{
				lavaHeatParticle2.Key.emissionRate = m_lavaHeatLevel * lavaHeatParticle2.Value;
			}
		}
		if ((Object)(object)Player.m_localPlayer == (Object)(object)this)
		{
			foreach (ZSFX item3 in m_lavaHeatAudio)
			{
				if ((Object)(object)item3 != (Object)null)
				{
					item3.SetVolumeModifier(IsDead() ? 0f : m_lavaHeatLevel);
				}
			}
		}
		if (!IsDead())
		{
			return;
		}
		foreach (KeyValuePair<ParticleSystem, float> lavaHeatParticle3 in m_lavaHeatParticles)
		{
			ZNetView component = ((Component)lavaHeatParticle3.Key).gameObject.GetComponent<ZNetView>();
			if (component != null && component.IsValid() && component.IsOwner())
			{
				component.Destroy();
			}
		}
		foreach (ZSFX item4 in m_lavaHeatAudio)
		{
			ZNetView component2 = ((Component)item4).gameObject.GetComponent<ZNetView>();
			if (component2 != null && component2.IsValid() && component2.IsOwner())
			{
				component2.Destroy();
			}
		}
		m_lavaHeatParticles.Clear();
		m_lavaHeatAudio.Clear();
	}

	private void UpdateHeatDamage(float dt)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		m_lavaDamageTimer += dt;
		if (!(m_lavaDamageTimer > m_lavaDamageTickInterval) || flag)
		{
			return;
		}
		m_lavaDamageTimer = 0f;
		float num = (InWater() ? 1f : Mathf.Max(m_lavaProximity, 0.05f));
		float num2 = 1f - GetEquipmentHeatResistanceModifier();
		if (m_lavaHeatLevel >= 1f)
		{
			if (!InWater() && m_lavaProximity > m_minLavaMaskThreshold)
			{
				m_seman.AddStatusEffect(SEMan.s_statusEffectBurning, resetTime: true);
			}
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = m_lavaFullDamage * num2 * num;
			hitData.m_point = m_lastGroundPoint;
			hitData.m_dir = m_lastGroundNormal;
			hitData.m_hitType = HitData.HitType.Burning;
			Damage(hitData);
		}
		else if (m_lavaHeatLevel >= m_heatLevelFirstDamageThreshold)
		{
			if (!InWater() && m_lavaProximity > m_minLavaMaskThreshold)
			{
				m_seman.AddStatusEffect(SEMan.s_statusEffectBurning, resetTime: true);
			}
			HitData hitData2 = new HitData();
			hitData2.m_damage.m_damage = m_lavaFirstDamage * num2 * num;
			hitData2.m_point = m_lastGroundPoint;
			hitData2.m_dir = m_lastGroundNormal;
			hitData2.m_hitType = HitData.HitType.Burning;
			Damage(hitData2);
		}
	}

	private bool IsUnderRoof()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return Physics.RaycastNonAlloc(((Component)this).transform.position + Vector3.up * 0.2f, Vector3.up, m_lavaRoofCheck, 20f, LayerMask.GetMask(new string[3] { "Default", "static_solid", "piece" })) > 0;
	}

	private void UpdateAshlandsWater(float dt)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (m_tolerateFire || !InWater())
		{
			return;
		}
		float num = WorldGenerator.GetAshlandsOceanGradient(((Component)this).transform.position);
		if (!IsSwimming())
		{
			num *= m_heatWaterTouchMultiplier;
		}
		if (!(num < 0f))
		{
			num = Mathf.Clamp01(num);
			float num2 = 1f - GetEquipmentHeatResistanceModifier();
			m_lavaHeatLevel += num * dt * m_heatBuildupWater * num2;
			if (m_lavaHeatLevel > m_heatLevelFirstDamageThreshold)
			{
				m_lavaHeatLevel = m_heatLevelFirstDamageThreshold;
			}
		}
	}

	private void UpdateContinousEffects()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		SetupContinuousEffect(((Component)this).transform, ((Component)this).transform.position, m_sliding, m_slideEffects, ref m_slideEffects_instances);
		Vector3 position = ((Component)this).transform.position;
		position.y = GetLiquidLevel() + 0.05f;
		EffectList effects = ((InTar() && m_tarEffects.HasEffects()) ? m_tarEffects : m_waterEffects);
		SetupContinuousEffect(((Component)this).transform, position, InLiquid(), effects, ref m_waterEffects_instances);
		SetupContinuousEffect(((Component)this).transform, ((Component)this).transform.position, IsFlying(), m_flyingContinuousEffect, ref m_flyingEffects_instances);
	}

	public static void SetupContinuousEffect(Transform transform, Vector3 point, bool enabledEffect, EffectList effects, ref GameObject[] instances)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (!effects.HasEffects())
		{
			return;
		}
		if (enabledEffect)
		{
			if (instances == null)
			{
				instances = effects.Create(point, Quaternion.identity, transform);
				return;
			}
			GameObject[] array = instances;
			foreach (GameObject val in array)
			{
				if (Object.op_Implicit((Object)(object)val))
				{
					val.transform.position = point;
				}
			}
		}
		else
		{
			if (instances == null)
			{
				return;
			}
			GameObject[] array = instances;
			foreach (GameObject val2 in array)
			{
				if (Object.op_Implicit((Object)(object)val2))
				{
					ParticleSystem[] componentsInChildren = val2.GetComponentsInChildren<ParticleSystem>();
					foreach (ParticleSystem obj in componentsInChildren)
					{
						EmissionModule emission = obj.emission;
						((EmissionModule)(ref emission)).enabled = false;
						obj.Stop();
					}
					CamShaker componentInChildren = val2.GetComponentInChildren<CamShaker>();
					if (Object.op_Implicit((Object)(object)componentInChildren))
					{
						Object.Destroy((Object)(object)componentInChildren);
					}
					ZSFX componentInChildren2 = val2.GetComponentInChildren<ZSFX>();
					if (Object.op_Implicit((Object)(object)componentInChildren2))
					{
						componentInChildren2.FadeOut();
					}
					TimedDestruction component = val2.GetComponent<TimedDestruction>();
					if (Object.op_Implicit((Object)(object)component))
					{
						component.Trigger();
					}
					else
					{
						Object.Destroy((Object)(object)val2);
					}
				}
			}
			instances = null;
		}
	}

	protected virtual void OnSwimming(Vector3 targetVel, float dt)
	{
	}

	protected virtual void OnSneaking(float dt)
	{
	}

	protected virtual void OnJump()
	{
	}

	protected virtual bool TakeInput()
	{
		return true;
	}

	private float GetSlideAngle()
	{
		if (IsPlayer())
		{
			return 38f;
		}
		if (HaveRider())
		{
			return 45f;
		}
		return 90f;
	}

	public bool HaveRider()
	{
		if (Object.op_Implicit((Object)(object)m_baseAI))
		{
			return m_baseAI.HaveRider();
		}
		return false;
	}

	private void ApplySlide(float dt, ref Vector3 currentVel, Vector3 bodyVel, bool running)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		bool flag = CanWallRun();
		float num = Mathf.Clamp(Mathf.Acos(Mathf.Clamp01(((m_groundTilt != 0) ? m_groundTiltNormal : m_lastGroundNormal).y)) * 57.29578f, 0f, 90f);
		Vector3 lastGroundNormal = m_lastGroundNormal;
		lastGroundNormal.y = 0f;
		((Vector3)(ref lastGroundNormal)).Normalize();
		_ = m_body.linearVelocity;
		Vector3 val = Vector3.Cross(m_lastGroundNormal, Vector3.up);
		Vector3 val2 = Vector3.Cross(m_lastGroundNormal, val);
		bool flag2 = ((Vector3)(ref currentVel)).magnitude > 0.1f;
		if (num > GetSlideAngle())
		{
			if (running && flag && flag2)
			{
				m_slippage = 0f;
				m_wallRunning = true;
			}
			else
			{
				m_slippage = Mathf.MoveTowards(m_slippage, 1f, 1f * dt);
			}
			Vector3 val3 = val2 * 5f;
			currentVel = Vector3.Lerp(currentVel, val3, m_slippage);
			m_sliding = m_slippage > 0.5f;
		}
		else
		{
			m_slippage = 0f;
		}
	}

	private void UpdateMotion(float dt)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		UpdateBodyFriction();
		m_sliding = false;
		m_wallRunning = false;
		m_running = false;
		m_walking = false;
		if (IsDead())
		{
			return;
		}
		if (IsDebugFlying())
		{
			UpdateDebugFly(dt);
			return;
		}
		if (InIntro())
		{
			m_maxAirAltitude = ((Component)this).transform.position.y;
			m_body.linearVelocity = Vector3.zero;
			m_body.angularVelocity = Vector3.zero;
		}
		if (!InLiquidSwimDepth() && !IsOnGround() && !IsAttached())
		{
			float y = ((Component)this).transform.position.y;
			m_maxAirAltitude = Mathf.Max(m_maxAirAltitude, y);
			m_fallTimer += dt;
			if (IsPlayer() && m_fallTimer > 0.1f)
			{
				m_zanim.SetBool(s_animatorFalling, value: true);
			}
		}
		else
		{
			m_fallTimer = 0f;
			if (IsPlayer())
			{
				m_zanim.SetBool(s_animatorFalling, value: false);
			}
		}
		if (IsSwimming())
		{
			UpdateSwimming(dt);
		}
		else if (m_flying)
		{
			UpdateFlying(dt);
		}
		else
		{
			if (Object.op_Implicit((Object)(object)m_baseAI) && !m_baseAI.IsSleeping())
			{
				UpdateWalking(dt);
			}
			if (!Object.op_Implicit((Object)(object)m_baseAI))
			{
				UpdateWalking(dt);
			}
		}
		UpdateSinkingPlatform(dt);
		m_lastGroundTouch += Time.fixedDeltaTime;
		m_jumpTimer += Time.fixedDeltaTime;
		if (m_standUp > 0f)
		{
			m_standUp -= Time.fixedDeltaTime;
		}
	}

	private void UpdateSinkingPlatform(float dt)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this != (Object)(object)Player.m_localPlayer || InLiquidSwimDepth() || IsOnGround() || IsAttached() || !((Object)(object)m_lastGroundBody != (Object)null) || !((Object)(object)((Component)m_lastGroundBody).GetComponent<Leviathan>() != (Object)null))
		{
			return;
		}
		StaticPhysics component = ((Component)m_lastGroundBody).GetComponent<StaticPhysics>();
		if (component != null && component.IsFalling)
		{
			((Component)this).transform.position = ((Component)this).transform.position + m_lastGroundBody.linearVelocity * dt;
			if (m_fallTimer > 0.1f)
			{
				m_zanim.SetBool(s_animatorFalling, value: false);
			}
		}
	}

	public static void SetTakeInputDelay(float delayInSeconds)
	{
		takeInputDelay = delayInSeconds;
	}

	private void UpdateDebugFly(float dt)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		takeInputDelay = Mathf.Max(0f, takeInputDelay - dt);
		float num = (m_run ? ((float)m_debugFlySpeed * 2.5f) : ((float)m_debugFlySpeed));
		Vector3 val = m_moveDir * num;
		if (TakeInput())
		{
			if ((ZInput.GetButton("Jump") || ZInput.GetButton("JoyJump")) && !(takeInputDelay > 0f) && !Hud.IsPieceSelectionVisible())
			{
				val.y = num;
			}
			else if (ZInput.GetKey((KeyCode)306, true) || ZInput.GetButtonPressedTimer("JoyCrouch") > 0.33f)
			{
				val.y = 0f - num;
			}
		}
		m_currentVel = Vector3.Lerp(m_currentVel, val, 0.5f);
		m_body.linearVelocity = m_currentVel;
		m_body.useGravity = false;
		m_lastGroundTouch = 0f;
		m_maxAirAltitude = ((Component)this).transform.position.y;
		m_body.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, m_lookYaw, m_turnSpeed * dt);
		m_body.angularVelocity = Vector3.zero;
		UpdateEyeRotation();
	}

	private void UpdateSwimming(float dt)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		bool flag = IsOnGround();
		if (Mathf.Max(0f, m_maxAirAltitude - ((Component)this).transform.position.y) > 0.5f && m_onLand != null)
		{
			m_onLand(new Vector3(((Component)this).transform.position.x, GetLiquidLevel(), ((Component)this).transform.position.z));
		}
		m_maxAirAltitude = ((Component)this).transform.position.y;
		float speed = m_swimSpeed * GetAttackSpeedFactorMovement();
		if (InMinorActionSlowdown())
		{
			speed = 0f;
		}
		m_seman.ApplyStatusEffectSpeedMods(ref speed, m_moveDir);
		Vector3 val = m_moveDir * speed;
		if (IsPlayer())
		{
			m_currentVel = Vector3.Lerp(m_currentVel, val, m_swimAcceleration);
		}
		else
		{
			float magnitude = ((Vector3)(ref val)).magnitude;
			float magnitude2 = ((Vector3)(ref m_currentVel)).magnitude;
			if (magnitude > magnitude2)
			{
				magnitude = Mathf.MoveTowards(magnitude2, magnitude, m_swimAcceleration);
				val = ((Vector3)(ref val)).normalized * magnitude;
			}
			m_currentVel = Vector3.Lerp(m_currentVel, val, 0.5f);
		}
		if (((Vector3)(ref m_currentVel)).magnitude > 0.1f)
		{
			AddNoise(15f);
		}
		AddPushbackForce(ref m_currentVel);
		Vector3 val2 = m_currentVel - m_body.linearVelocity;
		val2.y = 0f;
		if (((Vector3)(ref val2)).magnitude > 20f)
		{
			val2 = ((Vector3)(ref val2)).normalized * 20f;
		}
		m_body.AddForce(val2, (ForceMode)2);
		float num = GetLiquidLevel() - m_swimDepth;
		if (((Component)this).transform.position.y < num)
		{
			float num2 = Mathf.Clamp01((num - ((Component)this).transform.position.y) / 2f);
			float num3 = Mathf.Lerp(0f, 10f, num2);
			Vector3 linearVelocity = m_body.linearVelocity;
			linearVelocity.y = Mathf.MoveTowards(linearVelocity.y, num3, 50f * dt);
			m_body.linearVelocity = linearVelocity;
		}
		else
		{
			float num4 = Mathf.Clamp01((0f - (num - ((Component)this).transform.position.y)) / 1f);
			float num5 = Mathf.Lerp(0f, 10f, num4);
			Vector3 linearVelocity2 = m_body.linearVelocity;
			linearVelocity2.y = Mathf.MoveTowards(linearVelocity2.y, 0f - num5, 30f * dt);
			m_body.linearVelocity = linearVelocity2;
		}
		float num6 = 0f;
		if (((Vector3)(ref m_moveDir)).magnitude > 0.1f || AlwaysRotateCamera())
		{
			float speed2 = m_swimTurnSpeed;
			m_seman.ApplyStatusEffectSpeedMods(ref speed2, m_currentVel);
			num6 = UpdateRotation(speed2, dt, smooth: false);
		}
		m_body.angularVelocity = Vector3.zero;
		UpdateEyeRotation();
		m_body.useGravity = true;
		float value = ((IsPlayer() || HaveRider()) ? Vector3.Dot(m_currentVel, ((Component)this).transform.forward) : Vector3.Dot(m_body.linearVelocity, ((Component)this).transform.forward));
		float value2 = Vector3.Dot(m_currentVel, ((Component)this).transform.right);
		m_currentTurnVel = Mathf.SmoothDamp(m_currentTurnVel, num6, ref m_currentTurnVelChange, 0.5f, 99f);
		m_zanim.SetFloat(s_forwardSpeed, value);
		m_zanim.SetFloat(s_sidewaySpeed, value2);
		m_zanim.SetFloat(s_turnSpeed, m_currentTurnVel);
		m_zanim.SetBool(s_inWater, !flag);
		m_zanim.SetBool(s_onGround, value: false);
		m_zanim.SetBool(s_encumbered, value: false);
		m_zanim.SetBool(s_flying, value: false);
		if (!flag)
		{
			OnSwimming(val, dt);
		}
	}

	private void UpdateFlying(float dt)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		float num = (m_run ? m_flyFastSpeed : m_flySlowSpeed) * GetAttackSpeedFactorMovement();
		Vector3 val = (CanMove() ? (m_moveDir * num) : Vector3.zero);
		m_currentVel = Vector3.Lerp(m_currentVel, val, m_acceleration);
		m_maxAirAltitude = ((Component)this).transform.position.y;
		ApplyRootMotion(ref m_currentVel);
		AddPushbackForce(ref m_currentVel);
		Vector3 val2 = m_currentVel - m_body.linearVelocity;
		if (((Vector3)(ref val2)).magnitude > 20f)
		{
			val2 = ((Vector3)(ref val2)).normalized * 20f;
		}
		m_body.AddForce(val2, (ForceMode)2);
		float num2 = 0f;
		if ((((Vector3)(ref m_moveDir)).magnitude > 0.1f || AlwaysRotateCamera()) && !InDodge() && CanMove())
		{
			float speed = m_flyTurnSpeed;
			m_seman.ApplyStatusEffectSpeedMods(ref speed, m_currentVel);
			num2 = UpdateRotation(speed, dt, smooth: true);
		}
		m_body.angularVelocity = Vector3.zero;
		UpdateEyeRotation();
		m_body.useGravity = false;
		float num3 = Vector3.Dot(m_currentVel, ((Component)this).transform.forward);
		float value = Vector3.Dot(m_currentVel, ((Component)this).transform.right);
		float num4 = Vector3.Dot(m_body.linearVelocity, ((Component)this).transform.forward);
		m_currentTurnVel = Mathf.SmoothDamp(m_currentTurnVel, num2, ref m_currentTurnVelChange, 0.5f, 99f);
		m_zanim.SetFloat(s_forwardSpeed, IsPlayer() ? num3 : num4);
		m_zanim.SetFloat(s_sidewaySpeed, value);
		m_zanim.SetFloat(s_turnSpeed, m_currentTurnVel);
		m_zanim.SetBool(s_inWater, value: false);
		m_zanim.SetBool(s_onGround, value: false);
		m_zanim.SetBool(s_encumbered, value: false);
		m_zanim.SetBool(s_flying, value: true);
	}

	private void UpdateWalking(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		Vector3 moveDir = m_moveDir;
		bool flag = IsCrouching();
		m_running = CheckRun(moveDir, dt);
		float speed = m_speed * GetJogSpeedFactor();
		if ((m_walk || InMinorActionSlowdown()) && !flag)
		{
			speed = m_walkSpeed;
			m_walking = ((Vector3)(ref moveDir)).magnitude > 0.1f;
		}
		else if (m_running)
		{
			speed = m_runSpeed * GetRunSpeedFactor();
			if (IsPlayer() && ((Vector3)(ref moveDir)).magnitude > 0f)
			{
				((Vector3)(ref moveDir)).Normalize();
			}
		}
		else if (flag || IsEncumbered())
		{
			speed = m_crouchSpeed;
		}
		if (!IsPlayer())
		{
			if (!InInterior())
			{
				speed *= Game.m_enemySpeedSize;
			}
			speed *= 1f + (float)Game.m_worldLevel * Game.instance.m_worldLevelEnemyMoveSpeedMultiplier;
		}
		ApplyLiquidResistance(ref speed);
		speed *= GetAttackSpeedFactorMovement();
		m_seman.ApplyStatusEffectSpeedMods(ref speed, moveDir);
		if (m_lavaProximity > 0.33f && m_lavaHeightFactor > m_lavaSlowHeight)
		{
			float num = (m_lavaProximity - 0.33f) * 1.4925374f;
			speed *= 1f - num * m_lavaSlowMax;
		}
		if (Terminal.m_showTests && (Object)(object)Player.m_localPlayer == (Object)(object)this)
		{
			Terminal.m_testList["Player Speed"] = speed.ToString("0.000");
		}
		Vector3 val = (CanMove() ? (moveDir * speed) : Vector3.zero);
		Vector3 val2;
		if (((Vector3)(ref val)).magnitude > 0f && IsOnGround())
		{
			val2 = Vector3.ProjectOnPlane(val, m_lastGroundNormal);
			val = ((Vector3)(ref val2)).normalized * ((Vector3)(ref val)).magnitude;
		}
		float magnitude = ((Vector3)(ref val)).magnitude;
		float magnitude2 = ((Vector3)(ref m_currentVel)).magnitude;
		if (magnitude > magnitude2)
		{
			magnitude = Mathf.MoveTowards(magnitude2, magnitude, m_acceleration);
			val = ((Vector3)(ref val)).normalized * magnitude;
		}
		else
		{
			magnitude = Mathf.MoveTowards(magnitude2, magnitude, m_acceleration * 2f);
			val = ((((Vector3)(ref val)).magnitude > 0f) ? (((Vector3)(ref val)).normalized * magnitude) : (((Vector3)(ref m_currentVel)).normalized * magnitude));
		}
		m_currentVel = Vector3.Lerp(m_currentVel, val, 0.5f);
		Vector3 linearVelocity = m_body.linearVelocity;
		Vector3 currentVel = m_currentVel;
		currentVel.y = linearVelocity.y;
		if (IsOnGround() && (Object)(object)m_lastAttachBody == (Object)null)
		{
			ApplySlide(dt, ref currentVel, linearVelocity, m_running);
			currentVel.y = Mathf.Min(currentVel.y, 3f);
		}
		ApplyRootMotion(ref currentVel);
		AddPushbackForce(ref currentVel);
		ApplyGroundForce(ref currentVel, val);
		Vector3 val3 = currentVel - linearVelocity;
		if (!IsOnGround())
		{
			val3 = ((!(((Vector3)(ref val)).magnitude > 0.1f)) ? Vector3.zero : (val3 * m_airControl));
		}
		if (IsAttached())
		{
			val3 = Vector3.zero;
		}
		if (((Vector3)(ref val3)).magnitude > 20f)
		{
			val3 = ((Vector3)(ref val3)).normalized * 20f;
		}
		if (((Vector3)(ref val3)).magnitude > 0.01f)
		{
			m_body.AddForce(val3, (ForceMode)2);
		}
		Vector3 vel = m_body.linearVelocity;
		m_seman.ModifyWalkVelocity(ref vel);
		m_body.linearVelocity = vel;
		if (Object.op_Implicit((Object)(object)m_lastGroundBody) && ((Component)m_lastGroundBody).gameObject.layer != ((Component)this).gameObject.layer && m_lastGroundBody.mass > m_body.mass)
		{
			float num2 = m_body.mass / m_lastGroundBody.mass;
			m_lastGroundBody.AddForceAtPosition(-val3 * num2 * m_lastGroundBody.mass, ((Component)this).transform.position, (ForceMode)1);
		}
		float num3 = 0f;
		if (((((Vector3)(ref moveDir)).magnitude > 0.1f || AlwaysRotateCamera() || m_standUp > 0f) && !InDodge() && CanMove()) || m_groundContact)
		{
			float speed2 = (m_run ? m_runTurnSpeed : m_turnSpeed);
			m_seman.ApplyStatusEffectSpeedMods(ref speed2, m_currentVel);
			num3 = UpdateRotation(speed2, dt, smooth: false);
		}
		if (IsSneaking())
		{
			OnSneaking(dt);
		}
		UpdateEyeRotation();
		m_body.useGravity = true;
		Vector3 currentVel2 = m_currentVel;
		val2 = Vector3.ProjectOnPlane(((Component)this).transform.forward, m_lastGroundNormal);
		float num4 = Vector3.Dot(currentVel2, ((Vector3)(ref val2)).normalized);
		float num5 = Vector3.Dot(m_body.linearVelocity, m_visual.transform.forward);
		if (IsRiding())
		{
			num4 = num5;
		}
		else if (!IsPlayer() && !HaveRider())
		{
			num4 = Mathf.Min(num4, num5);
		}
		Vector3 currentVel3 = m_currentVel;
		val2 = Vector3.ProjectOnPlane(((Component)this).transform.right, m_lastGroundNormal);
		float value = Vector3.Dot(currentVel3, ((Vector3)(ref val2)).normalized);
		m_currentTurnVel = Mathf.SmoothDamp(m_currentTurnVel, num3, ref m_currentTurnVelChange, 0.5f, 99f);
		m_zanim.SetFloat(s_forwardSpeed, num4);
		m_zanim.SetFloat(s_sidewaySpeed, value);
		m_zanim.SetFloat(s_turnSpeed, m_currentTurnVel);
		m_zanim.SetBool(s_inWater, value: false);
		m_zanim.SetBool(s_onGround, IsOnGround());
		m_zanim.SetBool(s_encumbered, IsEncumbered());
		m_zanim.SetBool(s_flying, value: false);
		if (((Vector3)(ref m_currentVel)).magnitude > 0.1f)
		{
			if (m_running)
			{
				AddNoise(30f);
			}
			else if (!flag)
			{
				AddNoise(15f);
			}
		}
	}

	public bool IsSneaking()
	{
		if (IsCrouching() && ((Vector3)(ref m_currentVel)).magnitude > 0.1f)
		{
			return IsOnGround();
		}
		return false;
	}

	private float GetSlopeAngle()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOnGround())
		{
			return 0f;
		}
		float num = Vector3.SignedAngle(((Component)this).transform.forward, m_lastGroundNormal, ((Component)this).transform.right);
		return 0f - (90f - (0f - num));
	}

	protected void AddPushbackForce(ref Vector3 velocity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (m_pushForce != Vector3.zero)
		{
			Vector3 normalized = ((Vector3)(ref m_pushForce)).normalized;
			float num = Vector3.Dot(normalized, velocity);
			if (num < 20f)
			{
				velocity += normalized * (20f - num);
			}
			if (IsSwimming() || m_flying)
			{
				velocity *= 0.5f;
			}
		}
	}

	private void ApplyPushback(HitData hit)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		ApplyPushback(hit.m_dir, hit.m_pushForce);
	}

	public void ApplyPushback(Vector3 dir, float pushForce)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (pushForce != 0f && dir != Vector3.zero)
		{
			float num = pushForce * Mathf.Clamp01(1f + GetEquipmentMovementModifier()) / m_body.mass * 2.5f;
			dir.y = 0f;
			((Vector3)(ref dir)).Normalize();
			Vector3 pushForce2 = dir * num;
			if (((Vector3)(ref m_pushForce)).magnitude < ((Vector3)(ref pushForce2)).magnitude)
			{
				m_pushForce = pushForce2;
			}
		}
	}

	private void UpdatePushback(float dt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		m_pushForce = Vector3.MoveTowards(m_pushForce, Vector3.zero, 100f * dt);
	}

	public void TimeoutGroundForce(float time)
	{
		m_groundForceTimer = time;
	}

	private void ApplyGroundForce(ref Vector3 vel, Vector3 targetVel)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		if (IsOnGround() && Object.op_Implicit((Object)(object)m_lastGroundBody) && m_groundForceTimer <= 0f)
		{
			val = m_lastGroundBody.GetPointVelocity(((Component)this).transform.position);
			val.y = 0f;
		}
		Ship standingOnShip = GetStandingOnShip();
		if ((Object)(object)standingOnShip != (Object)null)
		{
			if (((Vector3)(ref targetVel)).magnitude > 0.01f)
			{
				m_lastAttachBody = null;
			}
			else if ((Object)(object)m_lastAttachBody != (Object)(object)m_lastGroundBody)
			{
				m_lastAttachBody = m_lastGroundBody;
				m_lastAttachPos = ((Component)m_lastAttachBody).transform.InverseTransformPoint(m_body.position);
			}
			if (Object.op_Implicit((Object)(object)m_lastAttachBody))
			{
				Vector3 val2 = ((Component)m_lastAttachBody).transform.TransformPoint(m_lastAttachPos);
				Vector3 val3 = val2 - m_body.position;
				if (((Vector3)(ref val3)).magnitude < 4f)
				{
					Vector3 position = val2;
					position.y = m_body.position.y;
					if (standingOnShip.IsOwner())
					{
						val3.y = 0f;
						val += val3 * 10f;
					}
					else
					{
						m_body.position = position;
					}
				}
				else
				{
					m_lastAttachBody = null;
				}
			}
		}
		else
		{
			m_lastAttachBody = null;
		}
		vel += val;
	}

	private float UpdateRotation(float turnSpeed, float dt, bool smooth)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = ((AlwaysRotateCamera() || m_moveDir == Vector3.zero) ? m_lookYaw : Quaternion.LookRotation(m_moveDir));
		float yawDeltaAngle = Utils.GetYawDeltaAngle(((Component)this).transform.rotation, val);
		float num = 1f;
		if (!IsPlayer())
		{
			num = Mathf.Clamp01(Mathf.Abs(yawDeltaAngle) / 90f);
			num = Mathf.Pow(num, 0.5f);
			float num2 = Mathf.Clamp01(Mathf.Abs(yawDeltaAngle) / 90f);
			num2 = Mathf.Pow(num2, 0.5f);
			if (smooth)
			{
				currentRotSpeedFactor = Mathf.MoveTowards(currentRotSpeedFactor, num2, dt);
				num = currentRotSpeedFactor;
			}
			else
			{
				num = num2;
			}
		}
		float num3 = turnSpeed * GetAttackSpeedFactorRotation() * num;
		Quaternion rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, val, num3 * dt);
		if (Mathf.Abs(yawDeltaAngle) > 0.001f)
		{
			((Component)this).transform.rotation = rotation;
		}
		return num3 * Mathf.Sign(yawDeltaAngle) * ((float)Math.PI / 180f);
	}

	private void UpdateGroundTilt(float dt)
	{
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_visual == (Object)null || (Object.op_Implicit((Object)(object)m_baseAI) && m_baseAI.IsSleeping()))
		{
			return;
		}
		if (m_nview.IsOwner())
		{
			if (m_groundTilt != 0)
			{
				if (!IsFlying() && IsOnGround() && !IsAttached())
				{
					Vector3 val = m_lastGroundNormal;
					if (m_groundTilt == GroundTiltType.PitchRaycast || m_groundTilt == GroundTiltType.FullRaycast)
					{
						Vector3 p = ((Component)this).transform.position + ((Component)this).transform.forward * m_collider.radius;
						Vector3 p2 = ((Component)this).transform.position - ((Component)this).transform.forward * m_collider.radius;
						GetGroundHeight(p, out var _, out var normal);
						GetGroundHeight(p2, out var _, out var normal2);
						Vector3 val2 = val + normal + normal2;
						val = ((Vector3)(ref val2)).normalized;
					}
					Vector3 val3 = ((Component)this).transform.InverseTransformVector(val);
					val3 = Vector3.RotateTowards(Vector3.up, val3, 0.87266463f, 1f);
					m_groundTiltNormal = Vector3.Lerp(m_groundTiltNormal, val3, 0.05f);
					Vector3 val5;
					if (m_groundTilt == GroundTiltType.Pitch || m_groundTilt == GroundTiltType.PitchRaycast)
					{
						Vector3 val4 = Vector3.Project(m_groundTiltNormal, Vector3.right);
						val5 = m_groundTiltNormal - val4;
					}
					else
					{
						val5 = m_groundTiltNormal;
					}
					Quaternion val6 = Quaternion.LookRotation(Vector3.Cross(val5, Vector3.left), val5);
					m_visual.transform.localRotation = Quaternion.RotateTowards(m_visual.transform.localRotation, val6, dt * m_groundTiltSpeed);
				}
				else if (IsFlying() && !IsOnGround() && m_groundTilt == GroundTiltType.Flying && ((Vector3)(ref m_currentVel)).sqrMagnitude > 0f)
				{
					m_groundTiltNormal = Vector3.Cross(((Component)this).transform.InverseTransformVector(((Vector3)(ref m_currentVel)).normalized), Vector3.right);
					Quaternion val7 = Quaternion.LookRotation(Vector3.Cross(m_groundTiltNormal, Vector3.left), m_groundTiltNormal);
					val7 = Quaternion.Lerp(Quaternion.identity, val7, ((Vector3)(ref m_currentVel)).magnitude * 0.33f);
					m_visual.transform.localRotation = Quaternion.RotateTowards(m_visual.transform.localRotation, val7, dt * m_groundTiltSpeed);
				}
				else
				{
					m_groundTiltNormal = Vector3.up;
					if (IsSwimming())
					{
						m_visual.transform.localRotation = Quaternion.RotateTowards(m_visual.transform.localRotation, Quaternion.identity, dt * m_groundTiltSpeed);
					}
					else
					{
						m_visual.transform.localRotation = Quaternion.RotateTowards(m_visual.transform.localRotation, Quaternion.identity, dt * m_groundTiltSpeed * 2f);
					}
				}
				Quaternion localRotation = m_visual.transform.localRotation;
				if (!((Quaternion)(ref localRotation)).Equals(m_tiltRotCached))
				{
					m_nview.GetZDO().Set(ZDOVars.s_tiltrot, localRotation);
					m_tiltRotCached = localRotation;
				}
			}
			else if (CanWallRun())
			{
				if (m_wallRunning)
				{
					Vector3 val8 = Vector3.Lerp(Vector3.up, m_lastGroundNormal, 0.65f);
					Vector3 val9 = Vector3.ProjectOnPlane(((Component)this).transform.forward, val8);
					((Vector3)(ref val9)).Normalize();
					Quaternion val10 = Quaternion.LookRotation(val9, val8);
					m_visual.transform.rotation = Quaternion.RotateTowards(m_visual.transform.rotation, val10, 30f * dt);
				}
				else
				{
					m_visual.transform.localRotation = Quaternion.RotateTowards(m_visual.transform.localRotation, Quaternion.identity, dt * m_groundTiltSpeed * 2f);
				}
				Quaternion localRotation2 = m_visual.transform.localRotation;
				if (!((Quaternion)(ref localRotation2)).Equals(m_tiltRotCached))
				{
					m_nview.GetZDO().Set(ZDOVars.s_tiltrot, localRotation2);
					m_tiltRotCached = localRotation2;
				}
			}
		}
		else if (m_groundTilt != 0 || CanWallRun())
		{
			Quaternion quaternion = m_nview.GetZDO().GetQuaternion(ZDOVars.s_tiltrot, Quaternion.identity);
			m_visual.transform.localRotation = Quaternion.RotateTowards(m_visual.transform.localRotation, quaternion, dt * m_groundTiltSpeed);
		}
		m_animator.SetFloat(s_tilt, Vector3.Dot(m_visual.transform.forward, Vector3.up));
	}

	private bool GetGroundHeight(Vector3 p, out float height, out Vector3 normal)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		p.y += 10f;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(p, Vector3.down, ref val, 20f, s_groundRayMask))
		{
			height = ((RaycastHit)(ref val)).point.y;
			normal = ((RaycastHit)(ref val)).normal;
			return true;
		}
		height = p.y;
		normal = Vector3.zero;
		return false;
	}

	public bool IsWallRunning()
	{
		return m_wallRunning;
	}

	private bool IsOnSnow()
	{
		return false;
	}

	public void Heal(float hp, bool showText = true)
	{
		if (!(hp <= 0f))
		{
			if (m_nview.IsOwner())
			{
				RPC_Heal(0L, hp, showText);
				return;
			}
			m_nview.InvokeRPC("RPC_Heal", hp, showText);
		}
	}

	private void RPC_Heal(long sender, float hp, bool showText)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner())
		{
			return;
		}
		float health = GetHealth();
		if (health <= 0f || IsDead())
		{
			return;
		}
		float num = Mathf.Min(health + hp, GetMaxHealth());
		if (num > health)
		{
			SetHealth(num);
			if (showText)
			{
				Vector3 topPoint = GetTopPoint();
				DamageText.instance.ShowText(DamageText.TextType.Heal, topPoint, hp, IsPlayer());
			}
		}
	}

	public Vector3 GetTopPoint()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.TransformPoint(m_collider.center) + m_visual.transform.up * (m_collider.height * 0.5f);
	}

	public float GetRadius()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (IsPlayer() || m_useAltStatusEffectScaling)
		{
			return m_collider.radius;
		}
		float radius = m_collider.radius;
		Vector3 localScale = ((Component)this).transform.localScale;
		return radius * ((Vector3)(ref localScale)).magnitude;
	}

	public float GetHeight()
	{
		return Mathf.Max(m_collider.height, m_collider.radius * 2f);
	}

	public Vector3 GetHeadPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return m_head.position;
	}

	public Vector3 GetEyePoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return m_eye.position;
	}

	public Vector3 GetCenterPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = ((Collider)m_collider).bounds;
		return ((Bounds)(ref bounds)).center;
	}

	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Character;
	}

	private short FindWeakSpotIndex(Collider c)
	{
		if ((Object)(object)c == (Object)null || m_weakSpots == null || m_weakSpots.Length == 0)
		{
			return -1;
		}
		for (short num = 0; num < m_weakSpots.Length; num++)
		{
			if ((Object)(object)m_weakSpots[num].m_collider == (Object)(object)c)
			{
				return num;
			}
		}
		return -1;
	}

	private WeakSpot GetWeakSpot(short index)
	{
		if (index < 0 || index >= m_weakSpots.Length)
		{
			return null;
		}
		return m_weakSpots[index];
	}

	public void Damage(HitData hit)
	{
		if (m_nview.IsValid())
		{
			hit.m_weakSpot = FindWeakSpotIndex(hit.m_hitCollider);
			m_nview.InvokeRPC("RPC_Damage", hit);
		}
	}

	private void RPC_Damage(long sender, HitData hit)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		if (IsDebugFlying())
		{
			return;
		}
		if ((Object)(object)hit.GetAttacker() == (Object)(object)Player.m_localPlayer)
		{
			Game.instance.IncrementPlayerStat(IsPlayer() ? PlayerStatType.PlayerHits : PlayerStatType.EnemyHits);
			m_localPlayerHasHit = true;
		}
		if (!m_nview.IsOwner())
		{
			return;
		}
		Character attacker = hit.GetAttacker();
		if (!IsDead() && !InCutscene() && !IsTeleporting() && hit.m_staggerMultiplier >= 100f)
		{
			Stagger(hit.m_dir);
		}
		if (GetHealth() <= 0f || IsDead() || IsTeleporting() || InCutscene() || (hit.m_dodgeable && IsDodgeInvincible()) || (hit.HaveAttacker() && (Object)(object)attacker == (Object)null) || (IsPlayer() && !IsPVPEnabled() && (Object)(object)attacker != (Object)null && attacker.IsPlayer() && !hit.m_ignorePVP))
		{
			return;
		}
		if ((Object)(object)attacker != (Object)null && !attacker.IsPlayer())
		{
			float difficultyDamageScalePlayer = Game.instance.GetDifficultyDamageScalePlayer(((Component)this).transform.position);
			hit.ApplyModifier(difficultyDamageScalePlayer);
			hit.ApplyModifier(Game.m_enemyDamageRate);
		}
		m_seman.OnDamaged(hit, attacker);
		if ((Object)(object)m_baseAI != (Object)null && m_baseAI.IsAggravatable() && !m_baseAI.IsAggravated() && Object.op_Implicit((Object)(object)attacker) && attacker.IsPlayer() && hit.GetTotalDamage() > 0f)
		{
			BaseAI.AggravateAllInArea(((Component)this).transform.position, 20f, BaseAI.AggravatedReason.Damage);
		}
		if ((Object)(object)m_baseAI != (Object)null && !m_baseAI.IsAlerted() && hit.m_backstabBonus > 1f && Time.time - m_backstabTime > 300f && (!ZoneSystem.instance.GetGlobalKey(GlobalKeys.PassiveMobs) || !m_baseAI.CanSeeTarget(attacker)))
		{
			m_backstabTime = Time.time;
			hit.ApplyModifier(hit.m_backstabBonus);
			m_backstabHitEffects.Create(hit.m_point, Quaternion.identity, ((Component)this).transform);
		}
		if (IsStaggering() && !IsPlayer())
		{
			hit.ApplyModifier(2f);
			m_critHitEffects.Create(hit.m_point, Quaternion.identity, ((Component)this).transform);
		}
		if (hit.m_blockable && IsBlocking())
		{
			BlockAttack(hit, attacker);
		}
		else if (this is Player player)
		{
			AddAdrenaline(player.m_nonBlockDamageAdrenaline);
		}
		ApplyPushback(hit);
		if (hit.m_statusEffectHash != 0)
		{
			StatusEffect statusEffect = m_seman.GetStatusEffect(hit.m_statusEffectHash);
			if ((Object)(object)statusEffect == (Object)null)
			{
				statusEffect = m_seman.AddStatusEffect(hit.m_statusEffectHash, resetTime: false, hit.m_itemLevel, hit.m_skillLevel);
			}
			else
			{
				statusEffect.ResetTime();
				statusEffect.SetLevel(hit.m_itemLevel, hit.m_skillLevel);
			}
			if ((Object)(object)statusEffect != (Object)null && (Object)(object)attacker != (Object)null)
			{
				statusEffect.SetAttacker(attacker);
			}
		}
		WeakSpot weakSpot = GetWeakSpot(hit.m_weakSpot);
		if ((Object)(object)weakSpot != (Object)null)
		{
			ZLog.Log((object)("HIT Weakspot:" + ((Object)((Component)weakSpot).gameObject).name));
		}
		HitData.DamageModifiers damageModifiers = GetDamageModifiers(weakSpot);
		hit.ApplyResistance(damageModifiers, out var significantModifier);
		if (IsPlayer())
		{
			float bodyArmor = GetBodyArmor();
			hit.ApplyArmor(bodyArmor);
			DamageArmorDurability(hit);
		}
		else if (Game.m_worldLevel > 0)
		{
			hit.ApplyArmor(Game.m_worldLevel * Game.instance.m_worldLevelEnemyBaseAC);
		}
		float poison = hit.m_damage.m_poison;
		float fire = hit.m_damage.m_fire;
		float spirit = hit.m_damage.m_spirit;
		hit.m_damage.m_poison = 0f;
		hit.m_damage.m_fire = 0f;
		hit.m_damage.m_spirit = 0f;
		ApplyDamage(hit, showDamageText: true, triggerEffects: true, significantModifier);
		AddFireDamage(fire);
		AddSpiritDamage(spirit);
		AddPoisonDamage(poison);
		AddFrostDamage(hit.m_damage.m_frost);
		AddLightningDamage(hit.m_damage.m_lightning);
	}

	protected HitData.DamageModifier GetDamageModifier(HitData.DamageType damageType)
	{
		return GetDamageModifiers().GetModifier(damageType);
	}

	public HitData.DamageModifiers GetDamageModifiers(WeakSpot weakspot = null)
	{
		HitData.DamageModifiers mods = (Object.op_Implicit((Object)(object)weakspot) ? weakspot.m_damageModifiers.Clone() : m_damageModifiers.Clone());
		ApplyArmorDamageMods(ref mods);
		m_seman.ApplyDamageMods(ref mods);
		return mods;
	}

	public void ApplyDamage(HitData hit, bool showDamageText, bool triggerEffects, HitData.DamageModifier mod = HitData.DamageModifier.Normal)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		if (IsDebugFlying() || IsDead() || IsTeleporting() || InCutscene())
		{
			return;
		}
		float totalDamage = hit.GetTotalDamage();
		if (!IsPlayer())
		{
			float difficultyDamageScaleEnemy = Game.instance.GetDifficultyDamageScaleEnemy(((Component)this).transform.position);
			hit.ApplyModifier(difficultyDamageScaleEnemy);
			hit.ApplyModifier(Game.m_playerDamageRate);
		}
		else
		{
			hit.ApplyModifier(Game.m_localDamgeTakenRate);
			Game.instance.IncrementPlayerStat((hit.GetAttacker() is Player) ? PlayerStatType.HitsTakenPlayers : PlayerStatType.HitsTakenEnemies);
		}
		float totalDamage2 = hit.GetTotalDamage();
		if (totalDamage2 <= 0.1f)
		{
			return;
		}
		if (showDamageText && (totalDamage2 > 0f || !IsPlayer()))
		{
			DamageText.instance.ShowText(mod, hit.m_point, totalDamage, IsPlayer() || IsTamed());
		}
		m_lastHit = hit;
		float health = GetHealth();
		health -= totalDamage2;
		if (health <= 0f && (InGodMode() || InGhostMode()))
		{
			health = 1f;
		}
		if (Terminal.m_showTests && Terminal.m_testList.ContainsKey("damage"))
		{
			Terminal.Log($"Damage: Character {m_name} took {totalDamage2} damage from {hit}");
		}
		SetHealth(health);
		float totalStaggerDamage = hit.m_damage.GetTotalStaggerDamage();
		AddStaggerDamage(totalStaggerDamage * hit.m_staggerMultiplier, hit.m_dir, hit);
		if (triggerEffects && totalDamage2 > GetMaxHealth() / 10f)
		{
			DoDamageCameraShake(hit);
			if (hit.m_damage.GetTotalPhysicalDamage() > 0f)
			{
				m_hitEffects.Create(hit.m_point, Quaternion.identity, ((Component)this).transform);
			}
		}
		OnDamaged(hit);
		if (health < 0f)
		{
			((Component)this).GetComponent<Piece>()?.DropResources(hit);
		}
		if (m_onDamaged != null)
		{
			m_onDamaged(totalDamage2, hit.GetAttacker());
		}
		if (s_dpsDebugEnabled)
		{
			AddDPS(totalDamage2, this);
		}
	}

	protected virtual void DoDamageCameraShake(HitData hit)
	{
	}

	protected virtual void DamageArmorDurability(HitData hit)
	{
	}

	private void AddFireDamage(float damage)
	{
		if (!(damage <= 0f))
		{
			SE_Burning sE_Burning = m_seman.GetStatusEffect(SEMan.s_statusEffectBurning) as SE_Burning;
			if ((Object)(object)sE_Burning == (Object)null)
			{
				sE_Burning = m_seman.AddStatusEffect(SEMan.s_statusEffectBurning) as SE_Burning;
			}
			if (!sE_Burning.AddFireDamage(damage))
			{
				m_seman.RemoveStatusEffect(sE_Burning);
			}
		}
	}

	private void AddSpiritDamage(float damage)
	{
		if (!(damage <= 0f))
		{
			SE_Burning sE_Burning = m_seman.GetStatusEffect(SEMan.s_statusEffectSpirit) as SE_Burning;
			if ((Object)(object)sE_Burning == (Object)null)
			{
				sE_Burning = m_seman.AddStatusEffect(SEMan.s_statusEffectSpirit) as SE_Burning;
			}
			if (!sE_Burning.AddSpiritDamage(damage))
			{
				m_seman.RemoveStatusEffect(sE_Burning);
			}
		}
	}

	private void AddPoisonDamage(float damage)
	{
		if (!(damage <= 0f))
		{
			SE_Poison sE_Poison = m_seman.GetStatusEffect(SEMan.s_statusEffectPoison) as SE_Poison;
			if ((Object)(object)sE_Poison == (Object)null)
			{
				sE_Poison = m_seman.AddStatusEffect(SEMan.s_statusEffectPoison) as SE_Poison;
			}
			sE_Poison.AddDamage(damage);
		}
	}

	private void AddFrostDamage(float damage)
	{
		if (!(damage <= 0f))
		{
			SE_Frost sE_Frost = m_seman.GetStatusEffect(SEMan.s_statusEffectFrost) as SE_Frost;
			if ((Object)(object)sE_Frost == (Object)null)
			{
				sE_Frost = m_seman.AddStatusEffect(SEMan.s_statusEffectFrost) as SE_Frost;
			}
			sE_Frost.AddDamage(damage);
		}
	}

	private void AddLightningDamage(float damage)
	{
		if (!(damage <= 0f))
		{
			m_seman.AddStatusEffect(SEMan.s_statusEffectLightning, resetTime: true);
		}
	}

	private static void AddDPS(float damage, Character me)
	{
		if ((Object)(object)me == (Object)(object)Player.m_localPlayer)
		{
			CalculateDPS("To-you ", s_playerDamage, damage);
		}
		else
		{
			CalculateDPS("To-others ", s_enemyDamage, damage);
		}
	}

	private static void CalculateDPS(string name, List<KeyValuePair<float, float>> damages, float damage)
	{
		float time = Time.time;
		if (damages.Count > 0 && Time.time - damages[damages.Count - 1].Key > 5f)
		{
			damages.Clear();
		}
		damages.Add(new KeyValuePair<float, float>(time, damage));
		float num = Time.time - damages[0].Key;
		if (num < 0.01f)
		{
			return;
		}
		float num2 = 0f;
		foreach (KeyValuePair<float, float> damage2 in damages)
		{
			num2 += damage2.Value;
		}
		float num3 = num2 / num;
		string text = string.Format("DPS {0} ( {1} attacks, {2}s ): {3}", name, damages.Count, num.ToString("0.0"), num3.ToString("0.0"));
		ZLog.Log((object)text);
		MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, text);
	}

	public float GetStaggerPercentage()
	{
		return Mathf.Clamp01(m_staggerDamage / GetStaggerTreshold());
	}

	private float GetStaggerTreshold()
	{
		return GetMaxHealth() * m_staggerDamageFactor;
	}

	protected bool AddStaggerDamage(float damage, Vector3 forceDirection, HitData hit)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (m_staggerDamageFactor <= 0f)
		{
			return false;
		}
		m_seman.ModifyStagger(damage, ref damage);
		m_staggerDamage += damage;
		float staggerTreshold = GetStaggerTreshold();
		if (m_staggerDamage >= staggerTreshold)
		{
			m_staggerDamage = staggerTreshold;
			Stagger(forceDirection);
			if (IsPlayer())
			{
				Hud.instance.StaggerBarFlash();
			}
			if (hit != null)
			{
				Character attacker = hit.GetAttacker();
				if (attacker is Player player)
				{
					if (attacker.IsOwner())
					{
						attacker.AddAdrenaline(player.m_staggerEnemyAdrenaline * m_enemyAdrenalineMultiplier);
					}
					else
					{
						attacker.m_nview.InvokeRPC("RPC_AddAdrenaline", player.m_staggerEnemyAdrenaline * m_enemyAdrenalineMultiplier);
					}
				}
			}
			return true;
		}
		return false;
	}

	private void UpdateStagger(float dt)
	{
		if (!(m_staggerDamageFactor <= 0f) || IsPlayer())
		{
			float num = GetMaxHealth() * m_staggerDamageFactor;
			m_staggerDamage -= num / 5f * dt;
			if (m_staggerDamage < 0f)
			{
				m_staggerDamage = 0f;
			}
		}
	}

	public void Stagger(Vector3 forceDirection)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			RPC_Stagger(0L, forceDirection);
			return;
		}
		m_nview.InvokeRPC("RPC_Stagger", forceDirection);
	}

	private void RPC_Stagger(long sender, Vector3 forceDirection)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!IsStaggering())
		{
			if (((Vector3)(ref forceDirection)).magnitude > 0.01f)
			{
				forceDirection.y = 0f;
				((Component)this).transform.rotation = Quaternion.LookRotation(-forceDirection);
			}
			m_zanim.SetSpeed(1f);
			m_zanim.SetTrigger("stagger");
		}
	}

	private void RPC_AddAdrenaline(long sender, float amount)
	{
		if (m_nview.IsOwner())
		{
			AddAdrenaline(amount);
		}
	}

	protected virtual void ApplyArmorDamageMods(ref HitData.DamageModifiers mods)
	{
	}

	public virtual float GetBodyArmor()
	{
		return 0f;
	}

	protected virtual bool BlockAttack(HitData hit, Character attacker)
	{
		return false;
	}

	protected virtual void OnDamaged(HitData hit)
	{
	}

	private void OnCollisionStay(Collision collision)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsOwner() || m_jumpTimer < 0.1f)
		{
			return;
		}
		ContactPoint[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint val = contacts[i];
			float num = ((ContactPoint)(ref val)).point.y - ((Component)this).transform.position.y;
			Vector3 normal = ((ContactPoint)(ref val)).normal;
			if (normal.y < 0f)
			{
				normal.y *= -1f;
			}
			if (!(normal.y > 0.1f) || !(num < m_collider.radius) || !(((ContactPoint)(ref val)).point.y < ((Component)m_collider).transform.position.y + m_collider.center.y))
			{
				continue;
			}
			if (normal.y > m_groundContactNormal.y || !m_groundContact)
			{
				if (m_standUp == -100f)
				{
					m_standUp = 2f;
				}
				m_groundContact = true;
				m_groundContactNormal = normal;
				m_groundContactPoint = ((ContactPoint)(ref val)).point;
				m_lowestContactCollider = collision.collider;
			}
			else
			{
				Vector3 val2 = Vector3.Normalize(m_groundContactNormal + normal);
				if (val2.y > m_groundContactNormal.y)
				{
					m_groundContactNormal = val2;
					m_groundContactPoint = (m_groundContactPoint + ((ContactPoint)(ref val)).point) * 0.5f;
				}
			}
		}
	}

	public void StandUpOnNextGround()
	{
		m_standUp = -100f;
	}

	private void UpdateGroundContact(float dt)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		if (!m_groundContact)
		{
			return;
		}
		m_lastGroundCollider = m_lowestContactCollider;
		m_lastGroundNormal = m_groundContactNormal;
		m_lastGroundPoint = m_groundContactPoint;
		m_lastGroundBody = (Object.op_Implicit((Object)(object)m_lastGroundCollider) ? m_lastGroundCollider.attachedRigidbody : null);
		if (!IsPlayer() && (Object)(object)m_lastGroundBody != (Object)null && ((Component)m_lastGroundBody).gameObject.layer == ((Component)this).gameObject.layer)
		{
			m_lastGroundCollider = null;
			m_lastGroundBody = null;
		}
		float num = Mathf.Max(0f, m_maxAirAltitude - ((Component)this).transform.position.y);
		if (num > 0.8f && m_onLand != null)
		{
			Vector3 lastGroundPoint = m_lastGroundPoint;
			if (InLiquid())
			{
				lastGroundPoint.y = GetLiquidLevel();
			}
			m_onLand(m_lastGroundPoint);
		}
		if (IsPlayer() && num > 4f)
		{
			float damage = Mathf.Clamp01((num - 4f) / 16f) * 100f;
			m_seman.ModifyFallDamage(damage, ref damage);
			if (damage > 0f)
			{
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = damage;
				hitData.m_point = m_lastGroundPoint;
				hitData.m_dir = m_lastGroundNormal;
				hitData.m_hitType = HitData.HitType.Fall;
				Damage(hitData);
			}
		}
		ResetGroundContact();
		m_lastGroundTouch = 0f;
		m_maxAirAltitude = ((Component)this).transform.position.y;
		if (IsPlayer() && Terminal.m_showTests)
		{
			Dictionary<string, string> testList = Terminal.m_testList;
			Collider lastGroundCollider = Player.m_localPlayer.GetLastGroundCollider();
			testList["Player.CollisionLayer"] = ((lastGroundCollider != null && Object.op_Implicit((Object)(object)lastGroundCollider)) ? LayerMask.LayerToName(((Component)lastGroundCollider).gameObject.layer) : "none");
		}
	}

	private void ResetGroundContact()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		m_lowestContactCollider = null;
		m_groundContact = false;
		m_groundContactNormal = Vector3.zero;
		m_groundContactPoint = Vector3.zero;
	}

	public Ship GetStandingOnShip()
	{
		if (InNumShipVolumes == 0)
		{
			return null;
		}
		if (!IsOnGround())
		{
			return null;
		}
		if (Object.op_Implicit((Object)(object)m_lastGroundBody))
		{
			return ((Component)m_lastGroundBody).GetComponent<Ship>();
		}
		return null;
	}

	public bool IsOnGround()
	{
		if (!(m_lastGroundTouch < 0.2f))
		{
			return m_body.IsSleeping();
		}
		return true;
	}

	private void CheckDeath()
	{
		if (!IsDead() && GetHealth() <= 0f)
		{
			OnDeath();
		}
	}

	protected virtual void OnRagdollCreated(Ragdoll ragdoll)
	{
	}

	protected virtual void OnDeath()
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		bool flag = m_lastHit != null && (Object)(object)m_lastHit.GetAttacker() == (Object)(object)Player.m_localPlayer;
		if (flag && IsPlayer() && this is Player player)
		{
			playerProfile.IncrementStat(PlayerStatType.PlayerKills);
			Utils.IncrementOrSet<string>(playerProfile.m_enemyStats, player.GetPlayerName(), 1f);
		}
		if (!IsPlayer())
		{
			if (m_localPlayerHasHit)
			{
				playerProfile.IncrementStat(IsBoss() ? PlayerStatType.BossKills : PlayerStatType.EnemyKills);
			}
			if (flag)
			{
				playerProfile.IncrementStat(IsBoss() ? PlayerStatType.BossLastHits : PlayerStatType.EnemyKillsLastHits);
			}
			Utils.IncrementOrSet<string>(playerProfile.m_enemyStats, m_name, 1f);
		}
		if (!string.IsNullOrEmpty(m_defeatSetGlobalKey))
		{
			Player.m_addUniqueKeyQueue.Add(m_defeatSetGlobalKey);
		}
		if (Object.op_Implicit((Object)(object)m_nview) && !m_nview.IsOwner())
		{
			return;
		}
		GameObject[] array = m_deathEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		for (int i = 0; i < array.Length; i++)
		{
			Ragdoll component = array[i].GetComponent<Ragdoll>();
			if (Object.op_Implicit((Object)(object)component))
			{
				CharacterDrop component2 = ((Component)this).GetComponent<CharacterDrop>();
				LevelEffects componentInChildren = ((Component)this).GetComponentInChildren<LevelEffects>();
				Vector3 velocity = m_body.linearVelocity;
				if (((Vector3)(ref m_pushForce)).magnitude * 0.5f > ((Vector3)(ref velocity)).magnitude)
				{
					velocity = m_pushForce * 0.5f;
				}
				float hue = 0f;
				float saturation = 0f;
				float value = 0f;
				if (Object.op_Implicit((Object)(object)componentInChildren))
				{
					componentInChildren.GetColorChanges(out hue, out saturation, out value);
				}
				component.Setup(velocity, hue, saturation, value, component2);
				OnRagdollCreated(component);
				if (Object.op_Implicit((Object)(object)component2) && component.m_dropItems)
				{
					component2.SetDropsEnabled(enabled: false);
				}
			}
		}
		if (!string.IsNullOrEmpty(m_defeatSetGlobalKey))
		{
			ZoneSystem.instance.SetGlobalKey(m_defeatSetGlobalKey);
		}
		if (m_onDeath != null)
		{
			m_onDeath();
		}
		if (IsBoss() && m_nview.GetZDO().GetBool("bosscount"))
		{
			ZoneSystem.instance.GetGlobalKey(GlobalKeys.activeBosses, out float value2);
			ZoneSystem.instance.SetGlobalKey(GlobalKeys.activeBosses, Mathf.Max(0f, value2 - 1f));
		}
		ZNetScene.instance.Destroy(((Component)this).gameObject);
		Gogan.LogEvent("Game", "Killed", m_name, 0L);
	}

	public float GetHealth()
	{
		return m_nview.GetZDO()?.GetFloat(ZDOVars.s_health, GetMaxHealth()) ?? GetMaxHealth();
	}

	public void SetHealth(float health)
	{
		if (health >= GetMaxHealth())
		{
			m_localPlayerHasHit = false;
		}
		ZDO zDO = m_nview.GetZDO();
		if (zDO != null && m_nview.IsOwner())
		{
			if (health < 0f)
			{
				health = 0f;
			}
			zDO.Set(ZDOVars.s_health, health);
		}
	}

	public void UseHealth(float hp)
	{
		if (!(hp <= 0f))
		{
			float health = GetHealth();
			health -= hp;
			health = Mathf.Clamp(health, 0f, GetMaxHealth());
			SetHealth(health);
			if (IsPlayer())
			{
				Hud.instance.DamageFlash();
			}
		}
	}

	public float GetHealthPercentage()
	{
		return GetHealth() / GetMaxHealth();
	}

	public virtual bool IsDead()
	{
		return false;
	}

	public void SetMaxHealth(float health)
	{
		if (m_nview.GetZDO() != null)
		{
			m_nview.GetZDO().Set(ZDOVars.s_maxHealth, health);
		}
		if (GetHealth() > health)
		{
			SetHealth(health);
		}
	}

	public float GetMaxHealth()
	{
		if (m_nview.GetZDO() != null)
		{
			return m_nview.GetZDO().GetFloat(ZDOVars.s_maxHealth, m_health);
		}
		return GetMaxHealthBase();
	}

	public float GetMaxHealthBase()
	{
		float num = m_health;
		if (!IsPlayer() && Game.m_worldLevel > 0)
		{
			num *= (float)Game.m_worldLevel * Game.instance.m_worldLevelEnemyHPMultiplier;
		}
		return num;
	}

	public virtual float GetMaxStamina()
	{
		return 0f;
	}

	public virtual float GetMaxAdrenaline()
	{
		return GetEquipmentMaxAdrenaline();
	}

	public virtual float GetMaxEitr()
	{
		return 0f;
	}

	public virtual float GetEitrPercentage()
	{
		return 1f;
	}

	public virtual float GetStaminaPercentage()
	{
		return 1f;
	}

	public bool IsBoss()
	{
		return m_boss;
	}

	public bool TryUseEitr(float eitrUse = 0f)
	{
		if (eitrUse == 0f)
		{
			return true;
		}
		if (GetMaxEitr() == 0f)
		{
			Message(MessageHud.MessageType.Center, "$hud_eitrrequired");
			return false;
		}
		if (!HaveEitr(eitrUse + 0.1f))
		{
			if (IsPlayer())
			{
				Hud.instance.EitrBarEmptyFlash();
			}
			return false;
		}
		return true;
	}

	public void SetLookDir(Vector3 dir, float transitionTime = 0f)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (transitionTime > 0f)
		{
			m_lookTransitionTime = (m_lookTransitionTimeTotal = transitionTime);
			m_lookTransitionStart = GetLookDir();
			m_lookTransitionTarget = Vector3.Normalize(dir);
			return;
		}
		if (((Vector3)(ref dir)).magnitude <= Mathf.Epsilon)
		{
			dir = ((Component)this).transform.forward;
		}
		else
		{
			((Vector3)(ref dir)).Normalize();
		}
		m_lookDir = dir;
		dir.y = 0f;
		m_lookYaw = Quaternion.LookRotation(dir);
	}

	private void UpdateLookTransition(float dt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (m_lookTransitionTime > 0f)
		{
			SetLookDir(Vector3.Lerp(m_lookTransitionTarget, m_lookTransitionStart, Mathf.SmoothStep(0f, 1f, m_lookTransitionTime / m_lookTransitionTimeTotal)));
			m_lookTransitionTime -= dt;
		}
	}

	public Vector3 GetLookDir()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return m_eye.forward;
	}

	public virtual void OnAttackTrigger()
	{
	}

	public virtual void OnStopMoving()
	{
	}

	public virtual void OnWeaponTrailStart()
	{
	}

	public void SetMoveDir(Vector3 dir)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		m_moveDir = dir;
	}

	public void SetRun(bool run)
	{
		m_run = run;
	}

	public void SetWalk(bool walk)
	{
		m_walk = walk;
	}

	public bool GetWalk()
	{
		return m_walk;
	}

	protected virtual void UpdateEyeRotation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_eye.rotation = Quaternion.LookRotation(m_lookDir);
	}

	public void OnAutoJump(Vector3 dir, float upVel, float forwardVel)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner() && IsOnGround() && !IsDead() && !InAttack() && !InDodge() && !IsKnockedBack() && !(Time.time - m_lastAutoJumpTime < 0.5f))
		{
			m_lastAutoJumpTime = Time.time;
			if (!(Vector3.Dot(m_moveDir, dir) < 0.5f))
			{
				Vector3 zero = Vector3.zero;
				zero.y = upVel;
				zero += dir * forwardVel;
				m_body.linearVelocity = zero;
				m_lastGroundTouch = 1f;
				m_jumpTimer = 0f;
				m_jumpEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
				SetCrouch(crouch: false);
				UpdateBodyFriction();
			}
		}
	}

	public void Jump(bool force = false)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOnGround() || IsDead() || (!force && InAttack()) || IsEncumbered() || InDodge() || IsKnockedBack() || IsStaggering())
		{
			return;
		}
		bool flag = false;
		if (!HaveStamina(m_jumpStaminaUsage))
		{
			if (IsPlayer())
			{
				Hud.instance.StaminaBarEmptyFlash();
			}
			flag = true;
		}
		float speed = m_speed;
		m_seman.ApplyStatusEffectSpeedMods(ref speed, m_currentVel);
		if (speed <= 0f)
		{
			flag = true;
		}
		float num = 0f;
		Skills skills = GetSkills();
		if ((Object)(object)skills != (Object)null)
		{
			num = skills.GetSkillFactor(Skills.SkillType.Jump);
			if (!flag)
			{
				RaiseSkill(Skills.SkillType.Jump);
			}
		}
		Vector3 val = m_body.linearVelocity;
		Mathf.Acos(Mathf.Clamp01(m_lastGroundNormal.y));
		Vector3 val2 = m_lastGroundNormal + Vector3.up;
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		float num2 = 1f + num * 0.4f;
		float num3 = m_jumpForce * num2;
		float num4 = Vector3.Dot(normalized, val);
		if (num4 < num3)
		{
			val += normalized * (num3 - num4);
		}
		val = ((!IsPlayer()) ? (val + ((Component)this).transform.forward * m_jumpForceForward * num2) : (val + m_moveDir * m_jumpForceForward * num2));
		if (flag)
		{
			val *= m_jumpForceTiredFactor;
		}
		m_seman.ApplyStatusEffectJumpMods(ref val);
		if (!(val.x <= 0f) || !(val.y <= 0f) || !(val.z <= 0f))
		{
			ForceJump(val);
		}
	}

	public void ForceJump(Vector3 vel, bool effects = true)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		m_body.WakeUp();
		m_body.linearVelocity = vel;
		ResetGroundContact();
		m_lastGroundTouch = 1f;
		m_jumpTimer = 0f;
		AddNoise(30f);
		if (effects)
		{
			m_zanim.SetTrigger("jump");
			m_jumpEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
			ResetCloth();
			OnJump();
			SetCrouch(crouch: false);
			UpdateBodyFriction();
		}
	}

	public void SetTempParent(Transform t)
	{
		oldParent = ((Component)this).transform.parent;
		((Component)this).transform.parent = t;
	}

	public void ReleaseTempParent()
	{
		((Component)this).transform.parent = oldParent;
	}

	private void UpdateBodyFriction()
	{
		((Collider)m_collider).material.frictionCombine = (PhysicsMaterialCombine)1;
		if (IsDead())
		{
			((Collider)m_collider).material.staticFriction = 1f;
			((Collider)m_collider).material.dynamicFriction = 1f;
			((Collider)m_collider).material.frictionCombine = (PhysicsMaterialCombine)3;
		}
		else if (IsSwimming())
		{
			((Collider)m_collider).material.staticFriction = 0.2f;
			((Collider)m_collider).material.dynamicFriction = 0.2f;
		}
		else if (!IsOnGround())
		{
			((Collider)m_collider).material.staticFriction = 0f;
			((Collider)m_collider).material.dynamicFriction = 0f;
		}
		else if (IsFlying())
		{
			((Collider)m_collider).material.staticFriction = 0f;
			((Collider)m_collider).material.dynamicFriction = 0f;
		}
		else if (((Vector3)(ref m_moveDir)).magnitude < 0.1f)
		{
			((Collider)m_collider).material.staticFriction = 0.8f * (1f - m_slippage);
			((Collider)m_collider).material.dynamicFriction = 0.8f * (1f - m_slippage);
			((Collider)m_collider).material.frictionCombine = (PhysicsMaterialCombine)3;
		}
		else
		{
			((Collider)m_collider).material.staticFriction = 0.4f * (1f - m_slippage);
			((Collider)m_collider).material.dynamicFriction = 0.4f * (1f - m_slippage);
		}
	}

	public virtual bool StartAttack(Character target, bool charge)
	{
		return false;
	}

	public virtual float GetTimeSinceLastAttack()
	{
		return 99999f;
	}

	public virtual void OnNearFire(Vector3 point)
	{
	}

	public ZDOID GetZDOID()
	{
		if (!m_nview.IsValid())
		{
			return ZDOID.None;
		}
		return m_nview.GetZDO().m_uid;
	}

	public bool IsOwner()
	{
		if (m_nview.IsValid())
		{
			return m_nview.IsOwner();
		}
		return false;
	}

	public long GetOwner()
	{
		if (!m_nview.IsValid())
		{
			return 0L;
		}
		return m_nview.GetZDO().GetOwner();
	}

	public virtual bool UseMeleeCamera()
	{
		return false;
	}

	protected virtual bool AlwaysRotateCamera()
	{
		return true;
	}

	public void SetLiquidLevel(float level, LiquidType type, Component liquidObj)
	{
		switch (type)
		{
		case LiquidType.Water:
			m_waterLevel = level;
			break;
		case LiquidType.Tar:
			m_tarLevel = level;
			break;
		}
		m_liquidLevel = Mathf.Max(m_waterLevel, m_tarLevel);
	}

	public virtual bool IsPVPEnabled()
	{
		return false;
	}

	public virtual bool InIntro()
	{
		return false;
	}

	public virtual bool InCutscene()
	{
		return false;
	}

	public virtual bool IsCrouching()
	{
		return false;
	}

	public virtual bool InBed()
	{
		return false;
	}

	public virtual bool IsAttached()
	{
		return false;
	}

	public virtual bool IsAttachedToShip()
	{
		return false;
	}

	public virtual bool IsRiding()
	{
		return false;
	}

	protected virtual void SetCrouch(bool crouch)
	{
	}

	public virtual void AttachStart(Transform attachPoint, GameObject colliderRoot, bool hideWeapons, bool isBed, bool onShip, string attachAnimation, Vector3 detachOffset, Transform cameraPos = null)
	{
	}

	public virtual void AttachStop()
	{
	}

	private void UpdateWater(float dt)
	{
		m_swimTimer += dt;
		float depth = InLiquidDepth();
		if (m_canSwim && InLiquidSwimDepth(depth))
		{
			m_swimTimer = 0f;
		}
		if (m_nview.IsOwner() && InLiquidWetDepth(depth))
		{
			if (m_waterLevel > m_tarLevel)
			{
				m_seman.AddStatusEffect(SEMan.s_statusEffectWet, resetTime: true);
			}
			else if (m_tarLevel > m_waterLevel && !m_tolerateTar)
			{
				m_seman.AddStatusEffect(SEMan.s_statusEffectTared, resetTime: true);
			}
		}
	}

	private void ApplyLiquidResistance(ref float speed)
	{
		float num = InLiquidDepth();
		if (!(num <= 0f) && !m_seman.HaveStatusEffect(SEMan.s_statusEffectTared))
		{
			float num2 = ((m_tarLevel > m_waterLevel) ? 0.1f : 0.05f);
			float num3 = m_collider.height / 3f;
			float num4 = Mathf.Clamp01(num / num3);
			speed -= speed * speed * num4 * num2;
		}
	}

	public bool IsSwimming()
	{
		return m_swimTimer < 0.5f;
	}

	public bool InLava()
	{
		return m_lavaTimer < 0.5f;
	}

	public bool AboveOrInLava()
	{
		return m_aboveOrInLavaTimer < 0.5f;
	}

	private bool InLiquidSwimDepth()
	{
		return InLiquidDepth() > Mathf.Max(0f, m_swimDepth - 0.4f);
	}

	private bool InLiquidSwimDepth(float depth)
	{
		return depth > Mathf.Max(0f, m_swimDepth - 0.4f);
	}

	private bool InLiquidKneeDepth()
	{
		return InLiquidDepth() > 0.4f;
	}

	private bool InLiquidKneeDepth(float depth)
	{
		return depth > 0.4f;
	}

	private bool InLiquidWetDepth(float depth)
	{
		if (!InLiquidSwimDepth(depth))
		{
			if (IsSitting())
			{
				return InLiquidKneeDepth(depth);
			}
			return false;
		}
		return true;
	}

	private float InLiquidDepth()
	{
		return m_cashedInLiquidDepth;
	}

	private void CalculateLiquidDepth()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (IsTeleporting() || (Object)(object)GetStandingOnShip() != (Object)null || IsAttachedToShip())
		{
			m_cashedInLiquidDepth = 0f;
		}
		else
		{
			m_cashedInLiquidDepth = Mathf.Max(0f, GetLiquidLevel() - ((Component)this).transform.position.y);
		}
	}

	protected void InvalidateCachedLiquidDepth()
	{
		m_cashedInLiquidDepth = 0f;
	}

	public float GetLiquidLevel()
	{
		return m_liquidLevel;
	}

	public bool InLiquid()
	{
		return InLiquidDepth() > 0f;
	}

	private bool InTar()
	{
		if (m_tarLevel > m_waterLevel)
		{
			return InLiquid();
		}
		return false;
	}

	public bool InWater()
	{
		if (m_waterLevel > m_tarLevel)
		{
			return InLiquid();
		}
		return false;
	}

	protected virtual bool CheckRun(Vector3 moveDir, float dt)
	{
		if (!m_run)
		{
			return false;
		}
		if (((Vector3)(ref moveDir)).magnitude < 0.1f)
		{
			return false;
		}
		if (IsCrouching() || IsEncumbered())
		{
			return false;
		}
		if (InDodge())
		{
			return false;
		}
		return true;
	}

	public bool IsRunning()
	{
		return m_running;
	}

	public bool IsWalking()
	{
		return m_walking;
	}

	public virtual bool InPlaceMode()
	{
		return false;
	}

	public virtual void AddEitr(float v)
	{
	}

	public virtual void UseEitr(float eitr)
	{
	}

	public virtual bool HaveEitr(float amount = 0f)
	{
		return true;
	}

	public virtual bool HaveStamina(float amount = 0f)
	{
		return true;
	}

	public bool HaveHealth(float amount = 0f)
	{
		return GetHealth() >= amount;
	}

	public virtual void AddStamina(float v)
	{
	}

	public virtual void UseStamina(float stamina)
	{
	}

	public virtual void AddAdrenaline(float v)
	{
	}

	protected int GetNextOrCurrentAnimHash()
	{
		if (m_cachedAnimHashFrame == MonoUpdaters.UpdateCount)
		{
			return m_cachedNextOrCurrentAnimHash;
		}
		UpdateCachedAnimHashes();
		return m_cachedNextOrCurrentAnimHash;
	}

	protected int GetCurrentAnimHash()
	{
		if (m_cachedAnimHashFrame == MonoUpdaters.UpdateCount)
		{
			return m_cachedCurrentAnimHash;
		}
		UpdateCachedAnimHashes();
		return m_cachedCurrentAnimHash;
	}

	protected int GetNextAnimHash()
	{
		if (m_cachedAnimHashFrame == MonoUpdaters.UpdateCount)
		{
			return m_cachedNextAnimHash;
		}
		UpdateCachedAnimHashes();
		return m_cachedNextAnimHash;
	}

	private void UpdateCachedAnimHashes()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		m_cachedAnimHashFrame = MonoUpdaters.UpdateCount;
		AnimatorStateInfo val = m_animator.GetCurrentAnimatorStateInfo(0);
		m_cachedCurrentAnimHash = ((AnimatorStateInfo)(ref val)).tagHash;
		m_cachedNextAnimHash = 0;
		m_cachedNextOrCurrentAnimHash = m_cachedCurrentAnimHash;
		if (m_animator.IsInTransition(0))
		{
			val = m_animator.GetNextAnimatorStateInfo(0);
			m_cachedNextAnimHash = ((AnimatorStateInfo)(ref val)).tagHash;
			m_cachedNextOrCurrentAnimHash = m_cachedNextAnimHash;
		}
	}

	public bool IsStaggering()
	{
		if (GetNextAnimHash() != s_animatorTagStagger)
		{
			return GetCurrentAnimHash() == s_animatorTagStagger;
		}
		return true;
	}

	public virtual bool CanMove()
	{
		if (IsStaggering())
		{
			return false;
		}
		int nextOrCurrentAnimHash = GetNextOrCurrentAnimHash();
		if (nextOrCurrentAnimHash != s_animatorTagFreeze)
		{
			return nextOrCurrentAnimHash != s_animatorTagSitting;
		}
		return false;
	}

	public virtual bool IsEncumbered()
	{
		return false;
	}

	public virtual bool IsTeleporting()
	{
		return false;
	}

	private bool CanWallRun()
	{
		return IsPlayer();
	}

	public void ShowPickupMessage(ItemDrop.ItemData item, int amount)
	{
		Message(MessageHud.MessageType.TopLeft, "$msg_added " + item.m_shared.m_name, amount, item.GetIcon());
	}

	public void ShowRemovedMessage(ItemDrop.ItemData item, int amount)
	{
		Message(MessageHud.MessageType.TopLeft, "$msg_removed " + item.m_shared.m_name, amount, item.GetIcon());
	}

	public virtual void Message(MessageHud.MessageType type, string msg, int amount = 0, Sprite icon = null)
	{
	}

	public CapsuleCollider GetCollider()
	{
		return m_collider;
	}

	public virtual float GetStealthFactor()
	{
		return 1f;
	}

	private void UpdateNoise(float dt)
	{
		m_noiseRange = Mathf.Max(0f, m_noiseRange - dt * 4f);
		m_syncNoiseTimer += dt;
		if (m_syncNoiseTimer > 0.5f)
		{
			m_syncNoiseTimer = 0f;
			m_nview.GetZDO().Set(ZDOVars.s_noise, m_noiseRange);
		}
	}

	public void AddNoise(float range)
	{
		if (m_nview.IsValid())
		{
			if (m_nview.IsOwner())
			{
				RPC_AddNoise(0L, range);
				return;
			}
			m_nview.InvokeRPC("RPC_AddNoise", range);
		}
	}

	private void RPC_AddNoise(long sender, float range)
	{
		if (m_nview.IsOwner() && range > m_noiseRange)
		{
			m_noiseRange = range;
			m_seman.ModifyNoise(m_noiseRange, ref m_noiseRange);
		}
	}

	public float GetNoiseRange()
	{
		if (!m_nview.IsValid())
		{
			return 0f;
		}
		if (m_nview.IsOwner())
		{
			return m_noiseRange;
		}
		return m_nview.GetZDO().GetFloat(ZDOVars.s_noise);
	}

	public virtual bool InGodMode()
	{
		return false;
	}

	public virtual bool InGhostMode()
	{
		return false;
	}

	public virtual bool IsDebugFlying()
	{
		return false;
	}

	public virtual string GetHoverText()
	{
		Tameable component = ((Component)this).GetComponent<Tameable>();
		if (Object.op_Implicit((Object)(object)component))
		{
			return component.GetHoverText();
		}
		return "";
	}

	public virtual string GetHoverName()
	{
		Tameable component = ((Component)this).GetComponent<Tameable>();
		if (Object.op_Implicit((Object)(object)component))
		{
			return component.GetHoverName();
		}
		return Localization.instance.Localize(m_name);
	}

	public virtual bool IsDrawingBow()
	{
		return false;
	}

	public virtual bool InAttack()
	{
		return false;
	}

	protected virtual void StopEmote()
	{
	}

	public virtual bool InMinorAction()
	{
		return false;
	}

	public virtual bool InMinorActionSlowdown()
	{
		return false;
	}

	public virtual bool InDodge()
	{
		return false;
	}

	public virtual bool IsDodgeInvincible()
	{
		return false;
	}

	public virtual bool InEmote()
	{
		return false;
	}

	public virtual bool IsBlocking()
	{
		return false;
	}

	public bool IsFlying()
	{
		return m_flying;
	}

	public bool IsKnockedBack()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return m_pushForce != Vector3.zero;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_nview != (Object)null && m_nview.GetZDO() != null)
		{
			float @float = m_nview.GetZDO().GetFloat(ZDOVars.s_noise);
			Gizmos.DrawWireSphere(((Component)this).transform.position, @float);
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(((Component)this).transform.position + Vector3.up * m_swimDepth, new Vector3(1f, 0.05f, 1f));
		if (IsOnGround())
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(m_lastGroundPoint, m_lastGroundPoint + m_lastGroundNormal);
		}
	}

	public virtual bool TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		return false;
	}

	protected void RPC_TeleportTo(long sender, Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			TeleportTo(pos, rot, distantTeleport);
		}
	}

	private void SyncVelocity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 linearVelocity = m_body.linearVelocity;
		if (!((Vector3)(ref linearVelocity)).Equals(m_bodyVelocityCached))
		{
			m_nview.GetZDO().Set(ZDOVars.s_bodyVelocity, linearVelocity);
		}
		m_bodyVelocityCached = linearVelocity;
	}

	public Vector3 GetVelocity()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return Vector3.zero;
		}
		if (m_nview.IsOwner())
		{
			return m_body.linearVelocity;
		}
		return m_nview.GetZDO().GetVec3(ZDOVars.s_bodyVelocity, Vector3.zero);
	}

	public void AddRootMotion(Vector3 vel)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (InDodge() || InAttack() || InEmote())
		{
			m_rootMotion += vel;
		}
	}

	private void ApplyRootMotion(ref Vector3 vel)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = m_rootMotion * 55f;
		if (((Vector3)(ref val)).magnitude > ((Vector3)(ref vel)).magnitude)
		{
			vel = val;
		}
		m_rootMotion = Vector3.zero;
	}

	public static void GetCharactersInRange(Vector3 point, float radius, List<Character> characters)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		float num = radius * radius;
		foreach (Character s_character in s_characters)
		{
			if (Utils.DistanceSqr(((Component)s_character).transform.position, point) < num)
			{
				characters.Add(s_character);
			}
		}
	}

	public static List<Character> GetAllCharacters()
	{
		return s_characters;
	}

	public static bool IsCharacterInRange(Vector3 point, float range)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Character s_character in s_characters)
		{
			if (Vector3.Distance(((Component)s_character).transform.position, point) < range)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void OnTargeted(bool sensed, bool alerted)
	{
	}

	public GameObject GetVisual()
	{
		return m_visual;
	}

	protected void UpdateLodgroup()
	{
		if (!((Object)(object)m_lodGroup == (Object)null))
		{
			Renderer[] componentsInChildren = m_visual.GetComponentsInChildren<Renderer>();
			LOD[] lODs = m_lodGroup.GetLODs();
			lODs[0].renderers = componentsInChildren;
			m_lodGroup.SetLODs(lODs);
		}
	}

	public virtual bool IsSitting()
	{
		return false;
	}

	public virtual float GetEquipmentMovementModifier()
	{
		return 0f;
	}

	public virtual float GetEquipmentHomeItemModifier()
	{
		return 0f;
	}

	public virtual float GetEquipmentHeatResistanceModifier()
	{
		return 0f;
	}

	public virtual float GetEquipmentJumpStaminaModifier()
	{
		return 0f;
	}

	public virtual float GetEquipmentAttackStaminaModifier()
	{
		return 0f;
	}

	public virtual float GetEquipmentBlockStaminaModifier()
	{
		return 0f;
	}

	public virtual float GetEquipmentDodgeStaminaModifier()
	{
		return 0f;
	}

	public virtual float GetEquipmentSwimStaminaModifier()
	{
		return 0f;
	}

	public virtual float GetEquipmentSneakStaminaModifier()
	{
		return 0f;
	}

	public virtual float GetEquipmentRunStaminaModifier()
	{
		return 0f;
	}

	protected virtual float GetJogSpeedFactor()
	{
		return 1f;
	}

	public virtual float GetEquipmentMaxAdrenaline()
	{
		return 0f;
	}

	protected virtual float GetRunSpeedFactor()
	{
		if (HaveRider())
		{
			float riderSkill = m_baseAI.GetRiderSkill();
			return 1f + riderSkill * 0.25f;
		}
		return 1f;
	}

	protected virtual float GetAttackSpeedFactorMovement()
	{
		return 1f;
	}

	protected virtual float GetAttackSpeedFactorRotation()
	{
		return 1f;
	}

	public virtual void RaiseSkill(Skills.SkillType skill, float value = 1f)
	{
		if (!IsTamed())
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)m_tameable))
		{
			m_tameable = ((Component)this).GetComponent<Tameable>();
			m_tameableMonsterAI = ((Component)this).GetComponent<MonsterAI>();
		}
		if (!Object.op_Implicit((Object)(object)m_tameable) || !Object.op_Implicit((Object)(object)m_tameableMonsterAI))
		{
			ZLog.LogWarning((object)(m_name + " is tamed but missing tameable or monster AI script!"));
		}
		else
		{
			if (m_tameable.m_levelUpOwnerSkill == Skills.SkillType.None)
			{
				return;
			}
			GameObject followTarget = m_tameableMonsterAI.GetFollowTarget();
			if (followTarget == null || !Object.op_Implicit((Object)(object)followTarget))
			{
				return;
			}
			Character component = followTarget.GetComponent<Character>();
			if (component != null)
			{
				Skills skills = component.GetSkills();
				if (skills != null)
				{
					skills.RaiseSkill(m_tameable.m_levelUpOwnerSkill, value * m_tameable.m_levelUpFactor);
					Terminal.Log($"{((Object)this).name} leveling up from '{skill}' to master {((Object)component).name} skill '{m_tameable.m_levelUpOwnerSkill}' at factor {value * m_tameable.m_levelUpFactor}");
				}
			}
		}
	}

	public virtual Skills GetSkills()
	{
		return null;
	}

	public float GetSkillLevel(Skills.SkillType skillType)
	{
		return GetSkills()?.GetSkillLevel(skillType) ?? 0f;
	}

	public virtual float GetSkillFactor(Skills.SkillType skill)
	{
		return 0f;
	}

	public virtual float GetRandomSkillFactor(Skills.SkillType skill)
	{
		return Mathf.Pow(Random.Range(0.75f, 1f), 0.5f) * m_nview.GetZDO().GetFloat(ZDOVars.s_randomSkillFactor, 1f);
	}

	public bool IsMonsterFaction(float time)
	{
		if (IsTamed(time))
		{
			return false;
		}
		if (m_faction != Faction.ForestMonsters && m_faction != Faction.Undead && m_faction != Faction.Demon && m_faction != Faction.PlainsMonsters && m_faction != Faction.MountainMonsters && m_faction != Faction.SeaMonsters)
		{
			return m_faction == Faction.MistlandsMonsters;
		}
		return true;
	}

	public Transform GetTransform()
	{
		if ((Object)(object)this == (Object)null)
		{
			return null;
		}
		return ((Component)this).transform;
	}

	public Collider GetLastGroundCollider()
	{
		return m_lastGroundCollider;
	}

	public Vector3 GetLastGroundNormal()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return m_groundContactNormal;
	}

	public void ResetCloth()
	{
		m_nview.InvokeRPC(ZNetView.Everybody, "RPC_ResetCloth");
	}

	private void RPC_ResetCloth(long sender)
	{
		Cloth[] componentsInChildren = ((Component)this).GetComponentsInChildren<Cloth>();
		foreach (Cloth val in componentsInChildren)
		{
			if (val.enabled)
			{
				val.enabled = false;
				val.enabled = true;
			}
		}
	}

	public virtual bool GetRelativePosition(out ZDOID parent, out string attachJoint, out Vector3 relativePos, out Quaternion relativeRot, out Vector3 relativeVel)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		relativeVel = Vector3.zero;
		if (IsOnGround() && Object.op_Implicit((Object)(object)m_lastGroundBody))
		{
			ZNetView component = ((Component)m_lastGroundBody).GetComponent<ZNetView>();
			if (Object.op_Implicit((Object)(object)component) && component.IsValid())
			{
				parent = component.GetZDO().m_uid;
				attachJoint = "";
				relativePos = ((Component)component).transform.InverseTransformPoint(((Component)this).transform.position);
				relativeRot = Quaternion.Inverse(((Component)component).transform.rotation) * ((Component)this).transform.rotation;
				relativeVel = ((Component)component).transform.InverseTransformVector(m_body.linearVelocity - m_lastGroundBody.linearVelocity);
				return true;
			}
		}
		parent = ZDOID.None;
		attachJoint = "";
		relativePos = Vector3.zero;
		relativeRot = Quaternion.identity;
		return false;
	}

	public Quaternion GetLookYaw()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return m_lookYaw;
	}

	public Vector3 GetMoveDir()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return m_moveDir;
	}

	public BaseAI GetBaseAI()
	{
		return m_baseAI;
	}

	public float GetMass()
	{
		return m_body.mass;
	}

	protected void SetVisible(bool visible)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_lodGroup == (Object)null) && m_lodVisible != visible)
		{
			m_lodVisible = visible;
			if (m_lodVisible)
			{
				m_lodGroup.localReferencePoint = m_originalLocalRef;
			}
			else
			{
				m_lodGroup.localReferencePoint = new Vector3(999999f, 999999f, 999999f);
			}
		}
	}

	public void SetTamed(bool tamed)
	{
		if (m_nview.IsValid() && m_tamed != tamed)
		{
			m_nview.InvokeRPC("RPC_SetTamed", tamed);
		}
	}

	private void RPC_SetTamed(long sender, bool tamed)
	{
		if (m_nview.IsOwner() && m_tamed != tamed)
		{
			m_tamed = tamed;
			m_nview.GetZDO().Set(ZDOVars.s_tamed, m_tamed);
		}
	}

	private bool IsTamed(float time)
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		if (!m_nview.GetZDO().IsOwner() && time - m_lastTamedCheck > 1f)
		{
			m_lastTamedCheck = time;
			m_tamed = m_nview.GetZDO().GetBool(ZDOVars.s_tamed, m_tamed);
		}
		return m_tamed;
	}

	public bool IsTamed()
	{
		return IsTamed(Time.time);
	}

	public ZSyncAnimation GetZAnim()
	{
		return m_zanim;
	}

	public SEMan GetSEMan()
	{
		return m_seman;
	}

	public bool InInterior()
	{
		return InInterior(((Component)this).transform);
	}

	public static bool InInterior(Transform me)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return InInterior(me.position);
	}

	public static bool InInterior(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return position.y > 3000f;
	}

	public static void SetDPSDebug(bool enabled)
	{
		s_dpsDebugEnabled = enabled;
	}

	public static bool IsDPSDebugEnabled()
	{
		return s_dpsDebugEnabled;
	}

	public void TakeOff()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		m_flying = true;
		m_jumpEffects.Create(((Component)this).transform.position, Quaternion.identity);
		m_animator.SetTrigger("fly_takeoff");
	}

	public void Land()
	{
		m_flying = false;
		m_animator.SetTrigger("fly_land");
	}

	public void FreezeFrame(float duration)
	{
		m_nview.InvokeRPC(ZNetView.Everybody, "RPC_FreezeFrame", duration);
	}

	private void RPC_FreezeFrame(long sender, float duration)
	{
		m_animEvent.FreezeFrame(duration);
	}

	public void SetExtraMass(float amount)
	{
		m_body.mass = m_originalMass + amount;
	}

	public int Increment(LiquidType type)
	{
		return ++m_liquids[(int)type];
	}

	public int Decrement(LiquidType type)
	{
		return --m_liquids[(int)type];
	}
}
