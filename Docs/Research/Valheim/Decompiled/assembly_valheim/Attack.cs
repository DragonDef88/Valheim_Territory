using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Attack
{
	public class HitPoint
	{
		public GameObject go;

		public Vector3 avgPoint = Vector3.zero;

		public int count;

		public Vector3 firstPoint;

		public Collider collider;

		public Dictionary<Collider, Vector3> allHits = new Dictionary<Collider, Vector3>();

		public Vector3 closestPoint;

		public float closestDistance = 999999f;
	}

	public enum AttackType
	{
		Horizontal,
		Vertical,
		Projectile,
		None,
		Area,
		TriggerProjectile
	}

	public enum HitPointType
	{
		Closest,
		Average,
		First
	}

	private static Collider[] s_hits = (Collider[])(object)new Collider[128];

	private static HashSet<GameObject> s_hitSet = new HashSet<GameObject>();

	private static List<RaycastHit> s_hitList = new List<RaycastHit>();

	private static RaycastHit[] s_hits2 = (RaycastHit[])(object)new RaycastHit[50];

	[Header("Common")]
	public AttackType m_attackType;

	public string m_attackAnimation = "";

	public string m_chargeAnimationBool = "";

	public int m_attackRandomAnimations;

	public int m_attackChainLevels;

	public bool m_loopingAttack;

	public bool m_consumeItem;

	public bool m_hitTerrain = true;

	public bool m_hitFriendly;

	public bool m_isHomeItem;

	public float m_attackStamina = 20f;

	public float m_attackAdrenaline = 1f;

	public float m_attackUseAdrenaline;

	public float m_attackEitr;

	public float m_attackHealth;

	[Range(0f, 100f)]
	public float m_attackHealthPercentage;

	public bool m_attackHealthLowBlockUse = true;

	public float m_attackHealthReturnHit;

	public bool m_attackKillsSelf;

	public float m_speedFactor = 0.2f;

	public float m_speedFactorRotation = 0.2f;

	public float m_attackStartNoise = 10f;

	public float m_attackHitNoise = 30f;

	public float m_damageMultiplier = 1f;

	[Tooltip("For each missing health point, increase damage this much.")]
	public float m_damageMultiplierPerMissingHP;

	[Tooltip("At 100% missing HP the damage will increase by this much, and gradually inbetween.")]
	public float m_damageMultiplierByTotalHealthMissing;

	[Tooltip("For each missing health point, return one stamina point.")]
	public float m_staminaReturnPerMissingHP;

	public float m_forceMultiplier = 1f;

	public float m_staggerMultiplier = 1f;

	public float m_recoilPushback;

	public int m_selfDamage;

	[Header("Misc")]
	public string m_attackOriginJoint = "";

	public float m_attackRange = 1.5f;

	public float m_attackHeight = 0.6f;

	public float m_attackHeightChar1;

	public float m_attackHeightChar2;

	public float m_attackOffset;

	public GameObject m_spawnOnTrigger;

	public bool m_toggleFlying;

	public bool m_attach;

	public bool m_cantUseInDungeon;

	[Header("Loading")]
	public bool m_requiresReload;

	public string m_reloadAnimation = "";

	public float m_reloadTime = 2f;

	public float m_reloadStaminaDrain;

	public float m_reloadEitrDrain;

	[Header("Draw")]
	public bool m_bowDraw;

	public float m_drawDurationMin;

	public float m_drawStaminaDrain;

	public float m_drawEitrDrain;

	public string m_drawAnimationState = "";

	public AnimationCurve m_drawVelocityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Header("Melee/AOE")]
	public float m_attackAngle = 90f;

	public float m_attackRayWidth;

	public float m_attackRayWidthCharExtra;

	public float m_maxYAngle;

	public bool m_lowerDamagePerHit = true;

	public HitPointType m_hitPointtype;

	public bool m_hitThroughWalls;

	public bool m_multiHit = true;

	public bool m_pickaxeSpecial;

	public float m_lastChainDamageMultiplier = 2f;

	[BitMask(typeof(DestructibleType))]
	public DestructibleType m_resetChainIfHit;

	[Header("Spawn on hit")]
	public GameObject m_spawnOnHit;

	public float m_spawnOnHitChance;

	[Header("Skill settings")]
	public float m_raiseSkillAmount = 1f;

	[BitMask(typeof(DestructibleType))]
	public DestructibleType m_skillHitType = DestructibleType.Character;

	public Skills.SkillType m_specialHitSkill;

	[BitMask(typeof(DestructibleType))]
	public DestructibleType m_specialHitType;

	[Header("Projectile")]
	public GameObject m_attackProjectile;

	public float m_projectileVel = 10f;

	public float m_projectileVelMin = 2f;

	[Tooltip("When not using Draw, randomize velocity between Velocity and Velocity Min")]
	public bool m_randomVelocity;

	public float m_projectileAccuracy = 10f;

	public float m_projectileAccuracyMin = 20f;

	public bool m_circularProjectileLaunch;

	public bool m_distributeProjectilesAroundCircle;

	public bool m_skillAccuracy;

	public bool m_useCharacterFacing;

	public bool m_useCharacterFacingYAim;

	[FormerlySerializedAs("m_useCharacterFacingAngle")]
	public float m_launchAngle;

	public int m_projectiles = 1;

	public int m_projectileBursts = 1;

	public float m_burstInterval;

	public bool m_destroyPreviousProjectile;

	public bool m_perBurstResourceUsage;

	[Header("Harvest")]
	public bool m_harvest;

	public float m_harvestRadius;

	public float m_harvestRadiusMaxLevel;

	private static readonly Collider[] s_pieceColliders = (Collider[])(object)new Collider[200];

	[Header("Attack-Effects")]
	public EffectList m_hitEffect = new EffectList();

	public EffectList m_hitTerrainEffect = new EffectList();

	public EffectList m_startEffect = new EffectList();

	public EffectList m_triggerEffect = new EffectList();

	public EffectList m_trailStartEffect = new EffectList();

	public EffectList m_burstEffect = new EffectList();

	protected static int m_attackMask = 0;

	protected static int m_attackMaskTerrain = 0;

	protected static int m_attackMaskCharacters = 0;

	protected static int m_harvestRayMask = 0;

	private Humanoid m_character;

	private BaseAI m_baseAI;

	private Rigidbody m_body;

	private ZSyncAnimation m_zanim;

	private CharacterAnimEvent m_animEvent;

	[NonSerialized]
	private ItemDrop.ItemData m_weapon;

	private VisEquipment m_visEquipment;

	[NonSerialized]
	private ItemDrop.ItemData m_lastUsedAmmo;

	private float m_attackDrawPercentage;

	private const float m_freezeFrameDuration = 0.15f;

	private const float m_chainAttackMaxTime = 0.2f;

	private int m_nextAttackChainLevel;

	private int m_currentAttackCainLevel;

	private bool m_wasInAttack;

	private float m_time;

	private bool m_abortAttack;

	private bool m_attackTowardsCameraDir = true;

	private bool m_projectileAttackStarted;

	private float m_projectileFireTimer = -1f;

	private int m_projectileBurstsFired;

	[NonSerialized]
	private ItemDrop.ItemData m_ammoItem;

	private bool m_attackDone;

	private bool m_isAttached;

	private Transform m_attachTarget;

	private Vector3 m_attachOffset;

	private float m_attachDistance;

	private Vector3 m_attachHitPoint;

	private float m_detachTimer;

	public bool StartDraw(Humanoid character, ItemDrop.ItemData weapon)
	{
		if (!HaveAmmo(character, weapon))
		{
			return false;
		}
		EquipAmmoItem(character, weapon);
		return true;
	}

	public bool Start(Humanoid character, Rigidbody body, ZSyncAnimation zanim, CharacterAnimEvent animEvent, VisEquipment visEquipment, ItemDrop.ItemData weapon, Attack previousAttack, float timeSinceLastAttack, float attackDrawPercentage)
	{
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		if (m_attackAnimation == "")
		{
			return false;
		}
		m_character = character;
		m_baseAI = ((Component)m_character).GetComponent<BaseAI>();
		m_body = body;
		m_zanim = zanim;
		m_animEvent = animEvent;
		m_visEquipment = visEquipment;
		m_weapon = weapon;
		m_attackDrawPercentage = attackDrawPercentage;
		if (m_attackMask == 0)
		{
			m_attackMask = LayerMask.GetMask(new string[11]
			{
				"Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "character", "character_net", "character_ghost", "hitbox", "character_noenv",
				"vehicle"
			});
			m_attackMaskTerrain = LayerMask.GetMask(new string[12]
			{
				"Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "terrain", "character", "character_net", "character_ghost", "hitbox",
				"character_noenv", "vehicle"
			});
			m_attackMaskCharacters = LayerMask.GetMask(new string[6] { "character", "character_net", "character_ghost", "hitbox", "character_noenv", "vehicle" });
			m_harvestRayMask = LayerMask.GetMask(new string[3] { "piece", "piece_nonsolid", "item" });
		}
		if (m_requiresReload && (!m_character.IsWeaponLoaded() || m_character.InMinorAction()))
		{
			return false;
		}
		if (m_cantUseInDungeon && m_character.InInterior() && m_character is Player player)
		{
			player.Message(MessageHud.MessageType.Center, "$msg_blocked");
			return false;
		}
		float attackStamina = GetAttackStamina();
		if (attackStamina > 0f && !character.HaveStamina(attackStamina + 0.1f))
		{
			if (character.IsPlayer())
			{
				Hud.instance.StaminaBarEmptyFlash();
			}
			return false;
		}
		if (!character.TryUseEitr(GetAttackEitr()))
		{
			return false;
		}
		float attackHealth = GetAttackHealth();
		if (attackHealth > 0f && !character.HaveHealth(attackHealth + 0.1f) && m_attackHealthLowBlockUse && character.IsPlayer())
		{
			Hud.instance.FlashHealthBar();
		}
		if (!HaveAmmo(character, m_weapon))
		{
			return false;
		}
		EquipAmmoItem(character, m_weapon);
		string text = null;
		if (m_attackChainLevels > 1)
		{
			if (previousAttack != null && previousAttack.m_attackAnimation == m_attackAnimation)
			{
				m_currentAttackCainLevel = previousAttack.m_nextAttackChainLevel;
			}
			if (m_currentAttackCainLevel >= m_attackChainLevels || timeSinceLastAttack > 0.2f)
			{
				m_currentAttackCainLevel = 0;
			}
			m_zanim.SetTrigger(text = m_attackAnimation + m_currentAttackCainLevel);
		}
		else if (m_attackRandomAnimations >= 2)
		{
			int num = Random.Range(0, m_attackRandomAnimations);
			m_zanim.SetTrigger(text = m_attackAnimation + num);
		}
		else
		{
			m_zanim.SetTrigger(text = m_attackAnimation);
		}
		if (character.IsPlayer() && m_attackType != AttackType.None && m_currentAttackCainLevel == 0 && ((Object)(object)Player.m_localPlayer == (Object)null || !Player.m_localPlayer.AttackTowardsPlayerLookDir || m_attackType == AttackType.Projectile))
		{
			((Component)character).transform.rotation = character.GetLookYaw();
			m_body.rotation = ((Component)character).transform.rotation;
		}
		weapon.m_lastAttackTime = Time.time;
		m_animEvent.ResetChain();
		return true;
	}

	public void StartWithoutAnimation(Humanoid character, Rigidbody body, VisEquipment visEquipment, ItemDrop.ItemData weapon, float attackDrawPercentage = 0f)
	{
		m_character = character;
		m_baseAI = ((Component)m_character).GetComponent<BaseAI>();
		m_body = body;
		m_visEquipment = visEquipment;
		m_weapon = weapon;
		m_attackDrawPercentage = attackDrawPercentage;
		weapon.m_lastAttackTime = Time.time;
		OnAttackTrigger();
	}

	private float GetAttackStamina()
	{
		if (m_attackStamina <= 0f)
		{
			return 0f;
		}
		float staminaUse = m_attackStamina;
		float skillFactor = m_character.GetSkillFactor(m_weapon.m_shared.m_skillType);
		if (m_character is Player player)
		{
			staminaUse = ((!m_isHomeItem) ? (staminaUse * (1f + player.GetEquipmentAttackStaminaModifier())) : (staminaUse * (1f + player.GetEquipmentHomeItemModifier())));
		}
		m_character.GetSEMan().ModifyAttackStaminaUsage(staminaUse, ref staminaUse);
		staminaUse -= staminaUse * 0.33f * skillFactor;
		if (m_staminaReturnPerMissingHP > 0f)
		{
			staminaUse -= (m_character.GetMaxHealth() - m_character.GetHealth()) * m_staminaReturnPerMissingHP;
		}
		return staminaUse;
	}

	private float GetAttackEitr()
	{
		if (m_attackEitr <= 0f)
		{
			return 0f;
		}
		float attackEitr = m_attackEitr;
		float skillFactor = m_character.GetSkillFactor(m_weapon.m_shared.m_skillType);
		return attackEitr - attackEitr * 0.33f * skillFactor;
	}

	private float GetAttackHealth()
	{
		if (m_attackHealth <= 0f && m_attackHealthPercentage <= 0f)
		{
			return 0f;
		}
		float num = m_attackHealth + m_character.GetHealth() * m_attackHealthPercentage / 100f;
		float skillFactor = m_character.GetSkillFactor(m_weapon.m_shared.m_skillType);
		return num - num * 0.33f * skillFactor;
	}

	public void Update(float dt)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (m_attackDone)
		{
			return;
		}
		m_time += dt;
		bool num = m_character.InAttack();
		if (num)
		{
			if (m_character.IsStaggering())
			{
				Abort();
			}
			if (!m_wasInAttack)
			{
				m_character.GetBaseAI()?.ChargeStop();
				if (m_attackType != AttackType.Projectile || !m_perBurstResourceUsage)
				{
					m_character.UseStamina(GetAttackStamina());
					m_character.UseEitr(GetAttackEitr());
					m_character.UseHealth(Mathf.Min(m_character.GetHealth() - 1f, GetAttackHealth()));
				}
				Transform attackOrigin = GetAttackOrigin();
				m_weapon.m_shared.m_startEffect.Create(attackOrigin.position, ((Component)m_character).transform.rotation, attackOrigin);
				m_startEffect.Create(attackOrigin.position, ((Component)m_character).transform.rotation, attackOrigin);
				m_character.AddNoise(m_attackStartNoise);
				m_nextAttackChainLevel = m_currentAttackCainLevel + 1;
				if (m_nextAttackChainLevel >= m_attackChainLevels)
				{
					m_nextAttackChainLevel = 0;
				}
				m_wasInAttack = true;
			}
			if (m_isAttached)
			{
				UpdateAttach(dt);
			}
		}
		UpdateProjectile(dt);
		if ((!num && m_wasInAttack) || m_abortAttack)
		{
			Stop();
		}
	}

	public bool IsDone()
	{
		return m_attackDone;
	}

	public void Stop()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (m_attackDone)
		{
			return;
		}
		if (m_loopingAttack)
		{
			m_zanim.SetTrigger("attack_abort");
		}
		if (m_isAttached)
		{
			m_zanim.SetTrigger("detach");
			m_isAttached = false;
			m_attachTarget = null;
		}
		if (m_wasInAttack)
		{
			if (Object.op_Implicit((Object)(object)m_visEquipment))
			{
				m_visEquipment.SetWeaponTrails(enabled: false);
			}
			m_wasInAttack = false;
		}
		m_attackDone = true;
		if (m_attackKillsSelf)
		{
			HitData hitData = new HitData();
			hitData.m_point = m_character.GetCenterPoint();
			hitData.m_damage.m_damage = 9999999f;
			hitData.m_hitType = HitData.HitType.Self;
			m_character.ApplyDamage(hitData, showDamageText: false, triggerEffects: true);
		}
	}

	public void Abort()
	{
		m_abortAttack = true;
	}

	public void OnAttackTrigger()
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (!UseAmmo(out m_lastUsedAmmo) || m_character.IsStaggering())
		{
			return;
		}
		if (m_attackUseAdrenaline > 0f)
		{
			m_character.AddAdrenaline(m_attackUseAdrenaline);
		}
		switch (m_attackType)
		{
		case AttackType.Horizontal:
		case AttackType.Vertical:
			DoMeleeAttack();
			break;
		case AttackType.Area:
			DoAreaAttack();
			break;
		case AttackType.Projectile:
			ProjectileAttackTriggered();
			break;
		case AttackType.None:
			DoNonAttack();
			break;
		}
		if (m_toggleFlying)
		{
			if (m_character.IsFlying())
			{
				m_character.Land();
			}
			else
			{
				m_character.TakeOff();
			}
		}
		if (m_recoilPushback != 0f)
		{
			m_character.ApplyPushback(-((Component)m_character).transform.forward, m_recoilPushback);
		}
		if (m_selfDamage > 0)
		{
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = m_selfDamage;
			m_character.Damage(hitData);
		}
		if (m_consumeItem)
		{
			ConsumeItem();
		}
		if (m_requiresReload)
		{
			m_character.ResetLoadedWeapon();
		}
	}

	private void ConsumeItem()
	{
		if (m_weapon.m_shared.m_maxStackSize > 1 && m_weapon.m_stack > 1)
		{
			m_weapon.m_stack--;
			return;
		}
		m_character.UnequipItem(m_weapon, triggerEquipEffects: false);
		m_character.GetInventory().RemoveItem(m_weapon);
	}

	private static ItemDrop.ItemData FindAmmo(Humanoid character, ItemDrop.ItemData weapon)
	{
		if (string.IsNullOrEmpty(weapon.m_shared.m_ammoType))
		{
			return null;
		}
		ItemDrop.ItemData itemData = character.GetAmmoItem();
		if (itemData != null && (!character.GetInventory().ContainsItem(itemData) || itemData.m_shared.m_ammoType != weapon.m_shared.m_ammoType))
		{
			itemData = null;
		}
		if (itemData == null)
		{
			itemData = character.GetInventory().GetAmmoItem(weapon.m_shared.m_ammoType);
		}
		return itemData;
	}

	private static bool EquipAmmoItem(Humanoid character, ItemDrop.ItemData weapon)
	{
		FindAmmo(character, weapon);
		if (!string.IsNullOrEmpty(weapon.m_shared.m_ammoType))
		{
			ItemDrop.ItemData ammoItem = character.GetAmmoItem();
			if (ammoItem != null && character.GetInventory().ContainsItem(ammoItem) && ammoItem.m_shared.m_ammoType == weapon.m_shared.m_ammoType)
			{
				return true;
			}
			ItemDrop.ItemData ammoItem2 = character.GetInventory().GetAmmoItem(weapon.m_shared.m_ammoType);
			if (ammoItem2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo || ammoItem2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.AmmoNonEquipable)
			{
				return character.EquipItem(ammoItem2);
			}
		}
		return true;
	}

	private static bool HaveAmmo(Humanoid character, ItemDrop.ItemData weapon)
	{
		if (!string.IsNullOrEmpty(weapon.m_shared.m_ammoType))
		{
			ItemDrop.ItemData itemData = character.GetAmmoItem();
			if (itemData != null && (!character.GetInventory().ContainsItem(itemData) || itemData.m_shared.m_ammoType != weapon.m_shared.m_ammoType))
			{
				itemData = null;
			}
			if (itemData == null)
			{
				itemData = character.GetInventory().GetAmmoItem(weapon.m_shared.m_ammoType);
			}
			if (itemData == null)
			{
				character.Message(MessageHud.MessageType.Center, "$msg_outof " + weapon.m_shared.m_ammoType);
				return false;
			}
			if (itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable)
			{
				return character.CanConsumeItem(itemData);
			}
			return true;
		}
		return true;
	}

	private bool UseAmmo(out ItemDrop.ItemData ammoItem)
	{
		m_ammoItem = null;
		ammoItem = null;
		if (!string.IsNullOrWhiteSpace(m_weapon.m_shared.m_ammoType))
		{
			ammoItem = m_character.GetAmmoItem();
			if (ammoItem != null && (!m_character.GetInventory().ContainsItem(ammoItem) || ammoItem.m_shared.m_ammoType != m_weapon.m_shared.m_ammoType))
			{
				ammoItem = null;
			}
			if (ammoItem == null)
			{
				ammoItem = m_character.GetInventory().GetAmmoItem(m_weapon.m_shared.m_ammoType);
			}
			if (ammoItem == null)
			{
				m_character.Message(MessageHud.MessageType.Center, "$msg_outof " + m_weapon.m_shared.m_ammoType);
				return false;
			}
			if (ammoItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable)
			{
				bool num = m_character.ConsumeItem(m_character.GetInventory(), ammoItem);
				if (num)
				{
					m_ammoItem = ammoItem;
				}
				return num;
			}
			m_character.GetInventory().RemoveItem(ammoItem, 1);
			m_ammoItem = ammoItem;
			return true;
		}
		return true;
	}

	private void ProjectileAttackTriggered()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		GetProjectileSpawnPoint(out var spawnPoint, out var aimDir);
		m_weapon.m_shared.m_triggerEffect.Create(spawnPoint, Quaternion.LookRotation(aimDir));
		m_triggerEffect.Create(spawnPoint, Quaternion.LookRotation(aimDir));
		if (m_weapon.m_shared.m_useDurability && m_character.IsPlayer())
		{
			m_weapon.m_durability -= m_weapon.m_shared.m_useDurabilityDrain;
		}
		if (m_projectileBursts == 1)
		{
			FireProjectileBurst();
		}
		else
		{
			m_projectileAttackStarted = true;
		}
	}

	private void UpdateProjectile(float dt)
	{
		if (m_projectileAttackStarted && m_projectileBurstsFired < m_projectileBursts)
		{
			m_projectileFireTimer -= dt;
			if (m_projectileFireTimer <= 0f)
			{
				m_projectileFireTimer = m_burstInterval;
				FireProjectileBurst();
				m_projectileBurstsFired++;
			}
		}
	}

	private Transform GetAttackOrigin()
	{
		if (m_attackOriginJoint.Length > 0)
		{
			return Utils.FindChild(m_character.GetVisual().transform, m_attackOriginJoint, (IterativeSearchType)0);
		}
		return ((Component)m_character).transform;
	}

	private void GetProjectileSpawnPoint(out Vector3 spawnPoint, out Vector3 aimDir)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		Transform attackOrigin = GetAttackOrigin();
		Transform transform = ((Component)m_character).transform;
		spawnPoint = attackOrigin.position + transform.up * m_attackHeight + transform.forward * m_attackRange + transform.right * m_attackOffset;
		aimDir = m_character.GetAimDir(spawnPoint);
		if (Object.op_Implicit((Object)(object)m_baseAI))
		{
			Character targetCreature = m_baseAI.GetTargetCreature();
			if (Object.op_Implicit((Object)(object)targetCreature))
			{
				Vector3 val = targetCreature.GetCenterPoint() - spawnPoint;
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				aimDir = Vector3.RotateTowards(((Component)m_character).transform.forward, normalized, (float)Math.PI / 2f, 1f);
			}
		}
		if (m_useCharacterFacing)
		{
			Vector3 forward = Vector3.forward;
			if (m_useCharacterFacingYAim)
			{
				forward.y = aimDir.y;
			}
			aimDir = transform.TransformDirection(forward);
		}
	}

	private void FireProjectileBurst()
	{
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a5: Unknown result type (might be due to invalid IL or missing references)
		if (m_perBurstResourceUsage)
		{
			float attackStamina = GetAttackStamina();
			if (attackStamina > 0f)
			{
				if (!m_character.HaveStamina(attackStamina))
				{
					Stop();
					return;
				}
				m_character.UseStamina(attackStamina);
			}
			float attackEitr = GetAttackEitr();
			if (attackEitr > 0f)
			{
				if (!m_character.HaveEitr(attackEitr))
				{
					Stop();
					return;
				}
				m_character.UseEitr(attackEitr);
			}
			float attackHealth = GetAttackHealth();
			if (attackHealth > 0f)
			{
				if (!m_character.HaveHealth(attackHealth) && m_attackHealthLowBlockUse)
				{
					Stop();
					return;
				}
				m_character.UseHealth(Mathf.Min(m_character.GetHealth() - 1f, attackHealth));
			}
			if (m_attackUseAdrenaline > 0f)
			{
				m_character.AddAdrenaline(m_attackUseAdrenaline);
			}
		}
		ItemDrop.ItemData ammoItem = m_ammoItem;
		GameObject attackProjectile = m_attackProjectile;
		float num = m_projectileVel;
		float num2 = m_projectileVelMin;
		float num3 = m_projectileAccuracy;
		float num4 = m_projectileAccuracyMin;
		float num5 = m_attackHitNoise;
		AnimationCurve drawVelocityCurve = m_drawVelocityCurve;
		if (ammoItem != null && Object.op_Implicit((Object)(object)ammoItem.m_shared.m_attack.m_attackProjectile))
		{
			attackProjectile = ammoItem.m_shared.m_attack.m_attackProjectile;
			num += ammoItem.m_shared.m_attack.m_projectileVel;
			num2 += ammoItem.m_shared.m_attack.m_projectileVelMin;
			num3 += ammoItem.m_shared.m_attack.m_projectileAccuracy;
			num4 += ammoItem.m_shared.m_attack.m_projectileAccuracyMin;
			num5 += ammoItem.m_shared.m_attack.m_attackHitNoise;
			drawVelocityCurve = ammoItem.m_shared.m_attack.m_drawVelocityCurve;
		}
		float num6 = m_character.GetRandomSkillFactor(m_weapon.m_shared.m_skillType);
		if (m_bowDraw)
		{
			num3 = Mathf.Lerp(num4, num3, Mathf.Pow(m_attackDrawPercentage, 0.5f));
			num6 *= m_attackDrawPercentage;
			num = Mathf.Lerp(num2, num, drawVelocityCurve.Evaluate(m_attackDrawPercentage));
			Game.instance.IncrementPlayerStat(PlayerStatType.ArrowsShot);
		}
		else if (m_skillAccuracy)
		{
			float skillFactor = m_character.GetSkillFactor(m_weapon.m_shared.m_skillType);
			num3 = Mathf.Lerp(num4, num3, skillFactor);
		}
		GetProjectileSpawnPoint(out var spawnPoint, out var aimDir);
		if (m_launchAngle != 0f)
		{
			Vector3 val = Vector3.Cross(Vector3.up, aimDir);
			aimDir = Quaternion.AngleAxis(m_launchAngle, val) * aimDir;
		}
		if (m_burstEffect.HasEffects())
		{
			m_burstEffect.Create(spawnPoint, Quaternion.LookRotation(aimDir));
		}
		for (int i = 0; i < m_projectiles; i++)
		{
			if (m_destroyPreviousProjectile && Object.op_Implicit((Object)(object)m_weapon.m_lastProjectile))
			{
				ZNetScene.instance.Destroy(m_weapon.m_lastProjectile);
				m_weapon.m_lastProjectile = null;
			}
			Vector3 val2 = aimDir;
			if (!m_bowDraw && m_randomVelocity)
			{
				num = Random.Range(num2, num);
			}
			Vector3 val3 = Vector3.Cross(val2, Vector3.up);
			Quaternion val4 = Quaternion.AngleAxis(Random.Range(0f - num3, num3), Vector3.up);
			if (m_circularProjectileLaunch && !m_distributeProjectilesAroundCircle)
			{
				val4 = Quaternion.AngleAxis(Random.value * 360f, Vector3.up);
			}
			else if (m_circularProjectileLaunch && !m_distributeProjectilesAroundCircle)
			{
				val4 = Quaternion.AngleAxis(Random.Range(0f - num3, num3) + (float)(i * (360 / m_projectiles)), Vector3.up);
			}
			val2 = Quaternion.AngleAxis(Random.Range(0f - num3, num3), val3) * val2;
			val2 = val4 * val2;
			GameObject val5 = Object.Instantiate<GameObject>(attackProjectile, spawnPoint, Quaternion.LookRotation(val2));
			HitData hitData = new HitData();
			hitData.m_toolTier = (short)m_weapon.m_shared.m_toolTier;
			hitData.m_pushForce = m_weapon.m_shared.m_attackForce * m_forceMultiplier;
			hitData.m_backstabBonus = m_weapon.m_shared.m_backstabBonus;
			hitData.m_staggerMultiplier = m_staggerMultiplier;
			hitData.m_damage.Add(m_weapon.GetDamage());
			hitData.m_statusEffectHash = ((Object.op_Implicit((Object)(object)m_weapon.m_shared.m_attackStatusEffect) && (m_weapon.m_shared.m_attackStatusEffectChance == 1f || Random.Range(0f, 1f) < m_weapon.m_shared.m_attackStatusEffectChance)) ? m_weapon.m_shared.m_attackStatusEffect.NameHash() : 0);
			hitData.m_skillLevel = m_character.GetSkillLevel(m_weapon.m_shared.m_skillType);
			hitData.m_itemLevel = (short)m_weapon.m_quality;
			hitData.m_itemWorldLevel = (byte)m_weapon.m_worldLevel;
			hitData.m_blockable = m_weapon.m_shared.m_blockable;
			hitData.m_dodgeable = m_weapon.m_shared.m_dodgeable;
			hitData.m_skill = m_weapon.m_shared.m_skillType;
			hitData.m_skillRaiseAmount = m_raiseSkillAmount;
			hitData.SetAttacker(m_character);
			hitData.m_hitType = ((!(hitData.GetAttacker() is Player)) ? HitData.HitType.EnemyHit : HitData.HitType.PlayerHit);
			hitData.m_healthReturn = m_attackHealthReturnHit;
			if (ammoItem != null)
			{
				hitData.m_damage.Add(ammoItem.GetDamage());
				hitData.m_pushForce += ammoItem.m_shared.m_attackForce;
				if ((Object)(object)ammoItem.m_shared.m_attackStatusEffect != (Object)null && (ammoItem.m_shared.m_attackStatusEffectChance == 1f || Random.Range(0f, 1f) < ammoItem.m_shared.m_attackStatusEffectChance))
				{
					hitData.m_statusEffectHash = ammoItem.m_shared.m_attackStatusEffect.NameHash();
				}
				if (!ammoItem.m_shared.m_blockable)
				{
					hitData.m_blockable = false;
				}
				if (!ammoItem.m_shared.m_dodgeable)
				{
					hitData.m_dodgeable = false;
				}
			}
			hitData.m_pushForce *= num6;
			ModifyDamage(hitData, num6);
			m_character.GetSEMan().ModifyAttack(m_weapon.m_shared.m_skillType, ref hitData);
			IProjectile component = val5.GetComponent<IProjectile>();
			component?.Setup(m_character, val2 * num, num5, hitData, m_weapon, m_lastUsedAmmo);
			m_weapon.m_lastProjectile = val5;
			if (m_spawnOnHitChance > 0f && Object.op_Implicit((Object)(object)m_spawnOnHit) && component is Projectile projectile)
			{
				projectile.m_spawnOnHit = m_spawnOnHit;
				projectile.m_spawnOnHitChance = m_spawnOnHitChance;
			}
		}
	}

	private void ModifyDamage(HitData hitData, float damageFactor = 1f)
	{
		if (m_damageMultiplier != 1f)
		{
			hitData.m_damage.Modify(m_damageMultiplier);
		}
		if (damageFactor != 1f)
		{
			hitData.m_damage.Modify(damageFactor);
		}
		hitData.m_damage.Modify(GetLevelDamageFactor());
		if (m_damageMultiplierPerMissingHP > 0f)
		{
			hitData.m_damage.Modify(1f + (m_character.GetMaxHealth() - m_character.GetHealth()) * m_damageMultiplierPerMissingHP);
		}
		if (m_damageMultiplierByTotalHealthMissing > 0f)
		{
			hitData.m_damage.Modify(1f + (1f - m_character.GetHealthPercentage()) * m_damageMultiplierByTotalHealthMissing);
		}
	}

	private void DoNonAttack()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (m_weapon.m_shared.m_useDurability && m_character.IsPlayer())
		{
			m_weapon.m_durability -= m_weapon.m_shared.m_useDurabilityDrain;
		}
		Transform attackOrigin = GetAttackOrigin();
		m_weapon.m_shared.m_triggerEffect.Create(attackOrigin.position, ((Component)m_character).transform.rotation, attackOrigin);
		m_triggerEffect.Create(attackOrigin.position, ((Component)m_character).transform.rotation, attackOrigin);
		if (Object.op_Implicit((Object)(object)m_weapon.m_shared.m_consumeStatusEffect))
		{
			m_character.GetSEMan().AddStatusEffect(m_weapon.m_shared.m_consumeStatusEffect, resetTime: true);
		}
		m_character.AddNoise(m_attackHitNoise);
	}

	private float GetLevelDamageFactor()
	{
		return 1f + (float)Mathf.Max(0, m_character.GetLevel() - 1) * 0.5f;
	}

	private void DoAreaAttack()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)m_character).transform;
		Transform attackOrigin = GetAttackOrigin();
		Vector3 origin = attackOrigin.position + Vector3.up * m_attackHeight + transform.forward * m_attackRange + transform.right * m_attackOffset;
		m_weapon.m_shared.m_triggerEffect.Create(origin, transform.rotation, attackOrigin);
		m_triggerEffect.Create(origin, transform.rotation, attackOrigin);
		int nrOfHits = 0;
		Vector3 avgHitPoint = Vector3.zero;
		bool raiseSkill = false;
		float skillDamageFactor = m_character.GetRandomSkillFactor(m_weapon.m_shared.m_skillType);
		int num = (m_hitTerrain ? m_attackMaskTerrain : m_attackMask);
		float maxAdrenalineMultiplier = 0f;
		s_hitSet.Clear();
		checkHits(Physics.OverlapSphereNonAlloc(origin, m_attackRayWidth, s_hits, num, (QueryTriggerInteraction)0));
		if (m_attackRayWidthCharExtra > 0f || m_attackHeightChar1 != 0f)
		{
			checkHits(Physics.OverlapSphereNonAlloc(origin + Vector3.up * m_attackHeightChar1, m_attackRayWidth + m_attackRayWidthCharExtra, s_hits, m_attackMaskCharacters, (QueryTriggerInteraction)0));
			if (m_attackHeightChar2 != m_attackHeightChar1)
			{
				checkHits(Physics.OverlapSphereNonAlloc(origin + Vector3.up * m_attackHeightChar2, m_attackRayWidth + m_attackRayWidthCharExtra, s_hits, m_attackMaskCharacters, (QueryTriggerInteraction)0));
			}
		}
		if (nrOfHits > 0)
		{
			avgHitPoint /= (float)nrOfHits;
			m_weapon.m_shared.m_hitEffect.Create(avgHitPoint, Quaternion.identity);
			m_hitEffect.Create(avgHitPoint, Quaternion.identity);
			if (m_weapon.m_shared.m_useDurability && m_character.IsPlayer())
			{
				m_weapon.m_durability -= 1f;
			}
			m_character.AddNoise(m_attackHitNoise);
			if (maxAdrenalineMultiplier > 0f)
			{
				m_character.AddAdrenaline(m_attackAdrenaline * maxAdrenalineMultiplier);
			}
			if (raiseSkill)
			{
				m_character.RaiseSkill(m_weapon.m_shared.m_skillType, m_raiseSkillAmount);
			}
		}
		if (Object.op_Implicit((Object)(object)m_spawnOnTrigger))
		{
			Object.Instantiate<GameObject>(m_spawnOnTrigger, origin, Quaternion.identity).GetComponent<IProjectile>()?.Setup(m_character, ((Component)m_character).transform.forward, -1f, null, null, m_lastUsedAmmo);
		}
		void checkHits(int count)
		{
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_0269: Unknown result type (might be due to invalid IL or missing references)
			//IL_0270: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < count; i++)
			{
				Collider val = s_hits[i];
				if (!((Object)(object)((Component)val).gameObject == (Object)(object)((Component)m_character).gameObject))
				{
					GameObject val2 = Projectile.FindHitObject(val);
					if (!((Object)(object)val2 == (Object)(object)((Component)m_character).gameObject) && !s_hitSet.Contains(val2))
					{
						s_hitSet.Add(val2);
						Vector3 val3 = ((!(val is MeshCollider)) ? val.ClosestPoint(origin) : val.ClosestPointOnBounds(origin));
						IDestructible component = val2.GetComponent<IDestructible>();
						if (component != null)
						{
							Vector3 val4 = val3 - origin;
							val4.y = 0f;
							Vector3 val5 = val3 - transform.position;
							if (Vector3.Dot(val5, val4) < 0f)
							{
								val4 = val5;
							}
							((Vector3)(ref val4)).Normalize();
							HitData hitData = new HitData();
							hitData.m_toolTier = (short)m_weapon.m_shared.m_toolTier;
							hitData.m_statusEffectHash = ((Object.op_Implicit((Object)(object)m_weapon.m_shared.m_attackStatusEffect) && (m_weapon.m_shared.m_attackStatusEffectChance == 1f || Random.Range(0f, 1f) < m_weapon.m_shared.m_attackStatusEffectChance)) ? m_weapon.m_shared.m_attackStatusEffect.NameHash() : 0);
							hitData.m_skillLevel = m_character.GetSkillLevel(m_weapon.m_shared.m_skillType);
							hitData.m_itemLevel = (short)m_weapon.m_quality;
							hitData.m_itemWorldLevel = (byte)m_weapon.m_worldLevel;
							hitData.m_pushForce = m_weapon.m_shared.m_attackForce * skillDamageFactor * m_forceMultiplier;
							hitData.m_backstabBonus = m_weapon.m_shared.m_backstabBonus;
							hitData.m_staggerMultiplier = m_staggerMultiplier;
							hitData.m_dodgeable = m_weapon.m_shared.m_dodgeable;
							hitData.m_blockable = m_weapon.m_shared.m_blockable;
							hitData.m_skill = m_weapon.m_shared.m_skillType;
							hitData.m_skillRaiseAmount = m_raiseSkillAmount;
							hitData.m_damage.Add(m_weapon.GetDamage());
							hitData.m_point = val3;
							hitData.m_dir = val4;
							hitData.m_hitCollider = val;
							hitData.SetAttacker(m_character);
							hitData.m_hitType = ((!(hitData.GetAttacker() is Player)) ? HitData.HitType.EnemyHit : HitData.HitType.PlayerHit);
							hitData.m_healthReturn = m_attackHealthReturnHit;
							ModifyDamage(hitData, skillDamageFactor);
							SpawnOnHit(val2);
							if (m_attackChainLevels > 1 && m_currentAttackCainLevel == m_attackChainLevels - 1 && m_lastChainDamageMultiplier > 1f)
							{
								hitData.m_damage.Modify(m_lastChainDamageMultiplier);
								hitData.m_pushForce *= 1.2f;
							}
							m_character.GetSEMan().ModifyAttack(m_weapon.m_shared.m_skillType, ref hitData);
							Character character = component as Character;
							bool flag = false;
							if (Object.op_Implicit((Object)(object)character))
							{
								flag = BaseAI.IsEnemy(m_character, character) || (Object.op_Implicit((Object)(object)character.GetBaseAI()) && character.GetBaseAI().IsAggravatable() && m_character.IsPlayer());
								if (((!m_hitFriendly || m_character.IsTamed()) && !m_character.IsPlayer() && !flag) || (!m_weapon.m_shared.m_tamedOnly && m_character.IsPlayer() && !m_character.IsPVPEnabled() && !flag) || (m_weapon.m_shared.m_tamedOnly && !character.IsTamed()))
								{
									continue;
								}
								if (flag && character.m_enemyAdrenalineMultiplier > maxAdrenalineMultiplier)
								{
									maxAdrenalineMultiplier = character.m_enemyAdrenalineMultiplier;
								}
								if (hitData.m_dodgeable && character.IsDodgeInvincible())
								{
									if (character.IsPlayer())
									{
										(character as Player).HitWhileDodging();
									}
									continue;
								}
							}
							else if (m_weapon.m_shared.m_tamedOnly)
							{
								continue;
							}
							if (m_attackHealthReturnHit > 0f && Object.op_Implicit((Object)(object)m_character) && flag)
							{
								m_character.Heal(m_attackHealthReturnHit);
							}
							component.Damage(hitData);
							if ((component.GetDestructibleType() & m_skillHitType) != 0)
							{
								raiseSkill = true;
							}
						}
						int num2 = nrOfHits + 1;
						nrOfHits = num2;
						avgHitPoint += val3;
					}
				}
			}
		}
	}

	private void GetMeleeAttackDir(out Transform originJoint, out Vector3 attackDir)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		originJoint = GetAttackOrigin();
		Vector3 forward = ((Component)m_character).transform.forward;
		Vector3 aimDir = m_character.GetAimDir(originJoint.position);
		aimDir.x = forward.x;
		aimDir.z = forward.z;
		((Vector3)(ref aimDir)).Normalize();
		attackDir = Vector3.RotateTowards(((Component)m_character).transform.forward, aimDir, (float)Math.PI / 180f * m_maxYAngle, 10f);
	}

	private void AddHitPoint(List<HitPoint> list, GameObject go, Collider collider, Vector3 point, float distance, bool multiCollider)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		HitPoint hitPoint = null;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if ((!multiCollider && (Object)(object)list[num].go == (Object)(object)go) || (multiCollider && (Object)(object)list[num].collider == (Object)(object)collider))
			{
				hitPoint = list[num];
				break;
			}
		}
		if (hitPoint == null)
		{
			hitPoint = new HitPoint();
			hitPoint.go = go;
			hitPoint.collider = collider;
			hitPoint.firstPoint = point;
			list.Add(hitPoint);
		}
		HitPoint hitPoint2 = hitPoint;
		hitPoint2.avgPoint += point;
		hitPoint.count++;
		if (distance < hitPoint.closestDistance)
		{
			hitPoint.closestPoint = point;
			hitPoint.closestDistance = distance;
		}
	}

	private void DoMeleeAttack()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0591: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Unknown result type (might be due to invalid IL or missing references)
		//IL_0603: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a93: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a98: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c19: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b23: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b25: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c44: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd3: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a22: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c80: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c82: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c89: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0848: Unknown result type (might be due to invalid IL or missing references)
		//IL_084a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0851: Unknown result type (might be due to invalid IL or missing references)
		//IL_0853: Unknown result type (might be due to invalid IL or missing references)
		//IL_0855: Unknown result type (might be due to invalid IL or missing references)
		//IL_085a: Unknown result type (might be due to invalid IL or missing references)
		//IL_085e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0863: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		GetMeleeAttackDir(out var originJoint, out var attackDir);
		Vector3 val = ((Component)m_character).transform.InverseTransformDirection(attackDir);
		Quaternion val2 = Quaternion.LookRotation(attackDir, Vector3.up);
		m_weapon.m_shared.m_triggerEffect.Create(originJoint.position, val2, originJoint);
		m_triggerEffect.Create(originJoint.position, val2, originJoint);
		Vector3 val3 = originJoint.position + Vector3.up * m_attackHeight + ((Component)m_character).transform.right * m_attackOffset;
		float num = m_attackAngle / 2f;
		float num2 = 4f;
		float attackRange = m_attackRange;
		List<HitPoint> list = new List<HitPoint>();
		HashSet<Skills.SkillType> hashSet = new HashSet<Skills.SkillType>();
		int num3 = (m_hitTerrain ? m_attackMaskTerrain : m_attackMask);
		for (float num4 = 0f - num; num4 <= num; num4 += num2)
		{
			Quaternion val4 = Quaternion.identity;
			if (m_attackType == AttackType.Horizontal)
			{
				val4 = Quaternion.Euler(0f, 0f - num4, 0f);
			}
			else if (m_attackType == AttackType.Vertical)
			{
				val4 = Quaternion.Euler(num4, 0f, 0f);
			}
			Vector3 val5 = ((Component)m_character).transform.TransformDirection(val4 * val);
			Debug.DrawLine(val3, val3 + val5 * attackRange);
			s_hitList.Clear();
			if (m_attackRayWidth > 0f)
			{
				addHits(Physics.SphereCastNonAlloc(val3, m_attackRayWidth, val5, s_hits2, Mathf.Max(0f, attackRange - m_attackRayWidth), num3, (QueryTriggerInteraction)1));
				if (m_attackRayWidthCharExtra > 0f || m_attackHeightChar1 != 0f)
				{
					addHits(Physics.SphereCastNonAlloc(val3 + Vector3.up * m_attackHeightChar1, m_attackRayWidth + m_attackRayWidthCharExtra, val5, s_hits2, Mathf.Max(0f, attackRange - (m_attackRayWidth + m_attackRayWidthCharExtra)), m_attackMaskCharacters, (QueryTriggerInteraction)1));
					if (m_attackHeightChar2 != m_attackHeightChar1)
					{
						addHits(Physics.SphereCastNonAlloc(val3 + Vector3.up * m_attackHeightChar2, m_attackRayWidth + m_attackRayWidthCharExtra, val5, s_hits2, Mathf.Max(0f, attackRange - (m_attackRayWidth + m_attackRayWidthCharExtra)), m_attackMaskCharacters, (QueryTriggerInteraction)1));
					}
				}
			}
			else
			{
				addHits(Physics.RaycastNonAlloc(val3, val5, s_hits2, attackRange, num3, (QueryTriggerInteraction)1));
			}
			s_hitList.Sort((RaycastHit x, RaycastHit y) => ((RaycastHit)(ref x)).distance.CompareTo(((RaycastHit)(ref y)).distance));
			foreach (RaycastHit s_hit in s_hitList)
			{
				RaycastHit current = s_hit;
				if ((Object)(object)((Component)((RaycastHit)(ref current)).collider).gameObject == (Object)(object)((Component)m_character).gameObject)
				{
					continue;
				}
				Vector3 val6 = ((RaycastHit)(ref current)).point;
				if (((RaycastHit)(ref current)).normal == -val5 && ((RaycastHit)(ref current)).point == Vector3.zero)
				{
					val6 = ((!(((RaycastHit)(ref current)).collider is MeshCollider)) ? ((RaycastHit)(ref current)).collider.ClosestPoint(val3) : (val3 + val5 * attackRange));
				}
				if (m_attackAngle < 180f && Vector3.Dot(val6 - val3, attackDir) <= 0f)
				{
					continue;
				}
				GameObject val7 = Projectile.FindHitObject(((RaycastHit)(ref current)).collider);
				if ((Object)(object)val7 == (Object)(object)((Component)m_character).gameObject)
				{
					continue;
				}
				Vagon component = val7.GetComponent<Vagon>();
				if (Object.op_Implicit((Object)(object)component) && component.IsAttached(m_character))
				{
					continue;
				}
				Character component2 = val7.GetComponent<Character>();
				if ((Object)(object)component2 != (Object)null)
				{
					bool flag = BaseAI.IsEnemy(m_character, component2) || (Object.op_Implicit((Object)(object)component2.GetBaseAI()) && component2.GetBaseAI().IsAggravatable() && m_character.IsPlayer());
					if (((!m_hitFriendly || m_character.IsTamed()) && !m_character.IsPlayer() && !flag) || (!m_weapon.m_shared.m_tamedOnly && m_character.IsPlayer() && !m_character.IsPVPEnabled() && !flag) || (m_weapon.m_shared.m_tamedOnly && !component2.IsTamed()))
					{
						continue;
					}
					if (m_weapon.m_shared.m_dodgeable && component2.IsDodgeInvincible())
					{
						if (component2.IsPlayer())
						{
							(component2 as Player).HitWhileDodging();
						}
						continue;
					}
				}
				else if (m_weapon.m_shared.m_tamedOnly)
				{
					continue;
				}
				bool multiCollider = m_pickaxeSpecial && (Object.op_Implicit((Object)(object)val7.GetComponent<MineRock5>()) || Object.op_Implicit((Object)(object)val7.GetComponent<MineRock>()));
				AddHitPoint(list, val7, ((RaycastHit)(ref current)).collider, val6, ((RaycastHit)(ref current)).distance, multiCollider);
				if (!m_hitThroughWalls)
				{
					break;
				}
			}
		}
		int num5 = 0;
		Vector3 val8 = Vector3.zero;
		bool flag2 = false;
		Character character = null;
		bool flag3 = false;
		foreach (HitPoint item in list)
		{
			GameObject go = item.go;
			Vector3 val9 = item.avgPoint / (float)item.count;
			Vector3 val10 = val9;
			switch (m_hitPointtype)
			{
			case HitPointType.Average:
				val10 = val9;
				break;
			case HitPointType.First:
				val10 = item.firstPoint;
				break;
			case HitPointType.Closest:
				val10 = item.closestPoint;
				break;
			}
			num5++;
			val8 += val9;
			m_weapon.m_shared.m_hitEffect.Create(val10, Quaternion.identity);
			m_hitEffect.Create(val10, Quaternion.identity);
			IDestructible component3 = go.GetComponent<IDestructible>();
			if (component3 != null)
			{
				DestructibleType destructibleType = component3.GetDestructibleType();
				Skills.SkillType skillType = m_weapon.m_shared.m_skillType;
				if (m_specialHitSkill != 0 && (destructibleType & m_specialHitType) != 0)
				{
					skillType = m_specialHitSkill;
					hashSet.Add(m_specialHitSkill);
				}
				else if ((destructibleType & m_skillHitType) != 0)
				{
					hashSet.Add(skillType);
				}
				float num6 = m_character.GetRandomSkillFactor(skillType);
				if (m_multiHit && m_lowerDamagePerHit && list.Count > 1)
				{
					num6 /= (float)list.Count * 0.75f;
				}
				HitData hitData = new HitData();
				hitData.m_toolTier = (short)m_weapon.m_shared.m_toolTier;
				hitData.m_statusEffectHash = ((Object.op_Implicit((Object)(object)m_weapon.m_shared.m_attackStatusEffect) && (m_weapon.m_shared.m_attackStatusEffectChance == 1f || Random.Range(0f, 1f) < m_weapon.m_shared.m_attackStatusEffectChance)) ? m_weapon.m_shared.m_attackStatusEffect.NameHash() : 0);
				hitData.m_skillLevel = m_character.GetSkillLevel(m_weapon.m_shared.m_skillType);
				hitData.m_itemLevel = (short)m_weapon.m_quality;
				hitData.m_itemWorldLevel = (byte)m_weapon.m_worldLevel;
				hitData.m_pushForce = m_weapon.m_shared.m_attackForce * num6 * m_forceMultiplier;
				hitData.m_backstabBonus = m_weapon.m_shared.m_backstabBonus;
				hitData.m_staggerMultiplier = m_staggerMultiplier;
				hitData.m_dodgeable = m_weapon.m_shared.m_dodgeable;
				hitData.m_blockable = m_weapon.m_shared.m_blockable;
				hitData.m_skill = skillType;
				hitData.m_skillRaiseAmount = m_raiseSkillAmount;
				hitData.m_damage = m_weapon.GetDamage();
				hitData.m_point = val10;
				HitData hitData2 = hitData;
				Vector3 val11 = val10 - val3;
				hitData2.m_dir = ((Vector3)(ref val11)).normalized;
				hitData.m_hitCollider = item.collider;
				hitData.SetAttacker(m_character);
				hitData.m_hitType = ((!(hitData.GetAttacker() is Player)) ? HitData.HitType.EnemyHit : HitData.HitType.PlayerHit);
				hitData.m_healthReturn = m_attackHealthReturnHit;
				ModifyDamage(hitData, num6);
				SpawnOnHit(go);
				if (m_attackChainLevels > 1 && m_currentAttackCainLevel == m_attackChainLevels - 1)
				{
					hitData.m_damage.Modify(2f);
					hitData.m_pushForce *= 1.2f;
				}
				m_character.GetSEMan().ModifyAttack(skillType, ref hitData);
				if (component3 is Character)
				{
					character = component3 as Character;
				}
				component3.Damage(hitData);
				if (m_attackHealthReturnHit > 0f && Object.op_Implicit((Object)(object)m_character) && Object.op_Implicit((Object)(object)character))
				{
					m_character.Heal(m_attackHealthReturnHit);
				}
				if ((destructibleType & m_resetChainIfHit) != 0)
				{
					m_nextAttackChainLevel = 0;
				}
				if (Object.op_Implicit((Object)(object)character) && Object.op_Implicit((Object)(object)m_character))
				{
					m_character.AddAdrenaline(m_attackAdrenaline * character.m_enemyAdrenalineMultiplier);
				}
				if (!m_multiHit)
				{
					break;
				}
			}
			if ((Object)(object)go.GetComponent<Heightmap>() != (Object)null && !flag2 && (!m_pickaxeSpecial || !flag3))
			{
				flag2 = true;
				m_weapon.m_shared.m_hitTerrainEffect.Create(val10, val2);
				m_hitTerrainEffect.Create(val10, val2);
				if (Object.op_Implicit((Object)(object)m_weapon.m_shared.m_spawnOnHitTerrain))
				{
					SpawnOnHitTerrain(val10, m_weapon.m_shared.m_spawnOnHitTerrain, m_character, m_attackHitNoise, m_weapon, m_lastUsedAmmo);
				}
				if (!m_multiHit || m_pickaxeSpecial)
				{
					break;
				}
			}
			else
			{
				flag3 = true;
			}
		}
		if (num5 > 0)
		{
			val8 /= (float)num5;
			if (m_weapon.m_shared.m_useDurability && m_character.IsPlayer())
			{
				m_weapon.m_durability -= m_weapon.m_shared.m_useDurabilityDrain;
			}
			m_character.AddNoise(m_attackHitNoise);
			m_character.FreezeFrame(0.15f);
			if (Object.op_Implicit((Object)(object)m_weapon.m_shared.m_spawnOnHit))
			{
				Object.Instantiate<GameObject>(m_weapon.m_shared.m_spawnOnHit, val8, val2).GetComponent<IProjectile>()?.Setup(m_character, Vector3.zero, m_attackHitNoise, null, m_weapon, m_lastUsedAmmo);
			}
			foreach (Skills.SkillType item2 in hashSet)
			{
				m_character.RaiseSkill(item2, m_raiseSkillAmount * (((Object)(object)character != (Object)null) ? 1.5f : 1f));
			}
			if (m_attach && !m_isAttached && Object.op_Implicit((Object)(object)character))
			{
				TryAttach(character, val8);
			}
		}
		if ((Object)(object)character == (Object)null && m_character is Player player)
		{
			m_character.AddAdrenaline(player.m_attackMissAdrenaline);
		}
		if (Object.op_Implicit((Object)(object)m_spawnOnTrigger))
		{
			GameObject obj = Object.Instantiate<GameObject>(m_spawnOnTrigger, val3, Quaternion.identity);
			obj.GetComponent<IProjectile>()?.Setup(m_character, ((Component)m_character).transform.forward, -1f, null, m_weapon, m_lastUsedAmmo);
			Piece component4 = obj.GetComponent<Piece>();
			if (component4 != null && m_character is Player player2)
			{
				player2.PlacePiece(component4, val3 + attackDir * m_attackRange, ((Component)player2).transform.rotation, doAttack: false);
			}
		}
		if (!m_harvest || !((Object)(object)m_character == (Object)(object)Player.m_localPlayer))
		{
			return;
		}
		Vector3 val12 = val3 + attackDir * m_attackRange;
		float skillFactor = Player.m_localPlayer.GetSkillFactor(Skills.SkillType.Farming);
		float num7 = Mathf.Lerp(m_harvestRadius, m_harvestRadiusMaxLevel, skillFactor);
		int num8 = Physics.OverlapSphereNonAlloc(val12, num7, s_pieceColliders, m_harvestRayMask);
		for (int i = 0; i < num8; i++)
		{
			GameObject gameObject = ((Component)s_pieceColliders[i]).gameObject;
			Pickable component5 = gameObject.GetComponent<Pickable>();
			if (component5 != null && component5.m_harvestable && component5.CanBePicked())
			{
				component5.Interact(Player.m_localPlayer, repeat: false, alt: false);
				continue;
			}
			Plant component6 = gameObject.GetComponent<Plant>();
			if (component6 != null && component6.GetStatus() != 0)
			{
				gameObject.GetComponent<Destructible>()?.Destroy();
			}
		}
		static void addHits(int count)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			for (int j = 0; j < count; j++)
			{
				s_hitList.Add(s_hits2[j]);
			}
		}
	}

	private void SpawnOnHit(GameObject target)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (m_spawnOnHitChance > 0f && Object.op_Implicit((Object)(object)m_spawnOnHit) && Random.Range(0f, 1f) < m_spawnOnHitChance)
		{
			Object.Instantiate<GameObject>(m_spawnOnHit, target.transform.position, target.transform.rotation).GetComponentInChildren<IProjectile>()?.Setup(m_character, ((Component)m_character).transform.forward, -1f, null, m_weapon, m_lastUsedAmmo);
		}
	}

	private bool TryAttach(Character hitCharacter, Vector3 hitPoint)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (hitCharacter.IsDodgeInvincible())
		{
			return false;
		}
		if (hitCharacter.IsBlocking())
		{
			Vector3 val = ((Component)hitCharacter).transform.position - ((Component)m_character).transform.position;
			val.y = 0f;
			((Vector3)(ref val)).Normalize();
			if (Vector3.Dot(val, ((Component)hitCharacter).transform.forward) < 0f)
			{
				return false;
			}
		}
		m_isAttached = true;
		m_attachTarget = ((Component)hitCharacter).transform;
		float num = hitCharacter.GetRadius() + m_character.GetRadius() + 0.1f;
		Vector3 val2 = ((Component)hitCharacter).transform.position - ((Component)m_character).transform.position;
		val2.y = 0f;
		((Vector3)(ref val2)).Normalize();
		m_attachDistance = num;
		Vector3 val3 = hitCharacter.GetCenterPoint() - val2 * num;
		m_attachOffset = m_attachTarget.InverseTransformPoint(val3);
		hitPoint.y = Mathf.Clamp(hitPoint.y, ((Component)hitCharacter).transform.position.y + hitCharacter.GetRadius(), ((Component)hitCharacter).transform.position.y + hitCharacter.GetHeight() - hitCharacter.GetRadius() * 1.5f);
		m_attachHitPoint = m_attachTarget.InverseTransformPoint(hitPoint);
		m_zanim.SetTrigger("attach");
		return true;
	}

	private void UpdateAttach(float dt)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_attachTarget))
		{
			Character component = ((Component)m_attachTarget).GetComponent<Character>();
			if ((Object)(object)component != (Object)null)
			{
				if (component.IsDead())
				{
					Stop();
					return;
				}
				m_detachTimer += dt;
				if (m_detachTimer > 0.3f)
				{
					m_detachTimer = 0f;
					if (component.IsDodgeInvincible())
					{
						Stop();
						return;
					}
				}
			}
			Vector3 val = m_attachTarget.TransformPoint(m_attachOffset);
			Vector3 val2 = m_attachTarget.TransformPoint(m_attachHitPoint);
			Vector3 val3 = Vector3.Lerp(((Component)m_character).transform.position, val, 0.1f);
			Vector3 val4 = val2 - val3;
			((Vector3)(ref val4)).Normalize();
			Quaternion rotation = Quaternion.LookRotation(val4);
			Vector3 position = val2 - val4 * m_character.GetRadius();
			((Component)m_character).transform.position = position;
			((Component)m_character).transform.rotation = rotation;
			((Component)m_character).GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
		}
		else
		{
			Stop();
		}
	}

	public bool IsAttached()
	{
		return m_isAttached;
	}

	public bool GetAttachData(out ZDOID parent, out string attachJoint, out Vector3 relativePos, out Quaternion relativeRot, out Vector3 relativeVel)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		attachJoint = "";
		parent = ZDOID.None;
		relativePos = Vector3.zero;
		relativeRot = Quaternion.identity;
		relativeVel = Vector3.zero;
		if (!m_isAttached || !Object.op_Implicit((Object)(object)m_attachTarget))
		{
			return false;
		}
		ZNetView component = ((Component)m_attachTarget).GetComponent<ZNetView>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return false;
		}
		parent = component.GetZDO().m_uid;
		relativePos = ((Component)component).transform.InverseTransformPoint(((Component)m_character).transform.position);
		relativeRot = Quaternion.Inverse(((Component)component).transform.rotation) * ((Component)m_character).transform.rotation;
		relativeVel = Vector3.zero;
		return true;
	}

	public static GameObject SpawnOnHitTerrain(Vector3 hitPoint, GameObject prefab, Character character, float attackHitNoise, ItemDrop.ItemData weapon, ItemDrop.ItemData ammo, bool randomRotation = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		TerrainModifier componentInChildren = prefab.GetComponentInChildren<TerrainModifier>();
		if (Object.op_Implicit((Object)(object)componentInChildren))
		{
			if (!PrivateArea.CheckAccess(hitPoint, componentInChildren.GetRadius()))
			{
				return null;
			}
			if (Location.IsInsideNoBuildLocation(hitPoint))
			{
				return null;
			}
		}
		TerrainOp componentInChildren2 = prefab.GetComponentInChildren<TerrainOp>();
		if (Object.op_Implicit((Object)(object)componentInChildren2))
		{
			if (!PrivateArea.CheckAccess(hitPoint, componentInChildren2.GetRadius()))
			{
				return null;
			}
			if (Location.IsInsideNoBuildLocation(hitPoint))
			{
				return null;
			}
		}
		TerrainModifier.SetTriggerOnPlaced(trigger: true);
		GameObject obj = Object.Instantiate<GameObject>(prefab, hitPoint, randomRotation ? Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f) : (((Object)(object)character != (Object)null) ? Quaternion.LookRotation(((Component)character).transform.forward) : Quaternion.identity));
		TerrainModifier.SetTriggerOnPlaced(trigger: false);
		obj.GetComponent<IProjectile>()?.Setup(character, Vector3.zero, attackHitNoise, null, weapon, ammo);
		return obj;
	}

	public Attack Clone()
	{
		return MemberwiseClone() as Attack;
	}

	public ItemDrop.ItemData GetWeapon()
	{
		return m_weapon;
	}

	public bool CanStartChainAttack()
	{
		if (m_nextAttackChainLevel > 0)
		{
			return m_animEvent.CanChain();
		}
		return false;
	}

	public void OnTrailStart()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (m_attackType == AttackType.Projectile)
		{
			Transform attackOrigin = GetAttackOrigin();
			m_weapon.m_shared.m_trailStartEffect.Create(attackOrigin.position, ((Component)m_character).transform.rotation, ((Component)m_character).transform);
			m_trailStartEffect.Create(attackOrigin.position, ((Component)m_character).transform.rotation, ((Component)m_character).transform);
		}
		else
		{
			GetMeleeAttackDir(out var originJoint, out var attackDir);
			Quaternion baseRot = Quaternion.LookRotation(attackDir, Vector3.up);
			m_weapon.m_shared.m_trailStartEffect.Create(originJoint.position, baseRot, ((Component)m_character).transform);
			m_trailStartEffect.Create(originJoint.position, baseRot, ((Component)m_character).transform);
		}
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, {2}", "Attack", m_attackAnimation, m_attackType);
	}
}
