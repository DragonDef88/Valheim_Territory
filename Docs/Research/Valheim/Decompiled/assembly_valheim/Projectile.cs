using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IProjectile
{
	public ProjectileType m_type;

	public HitData.DamageTypes m_damage;

	public float m_aoe;

	public bool m_dodgeable;

	public bool m_blockable;

	public float m_adrenaline = 2f;

	public float m_attackForce;

	public float m_backstabBonus = 4f;

	public string m_statusEffect = "";

	private int m_statusEffectHash;

	public float m_healthReturn;

	public bool m_canHitWater;

	public float m_ttl = 4f;

	public float m_gravity;

	public float m_drag;

	public float m_rayRadius;

	public float m_hitNoise = 50f;

	public bool m_doOwnerRaytest;

	public bool m_stayAfterHitStatic;

	public bool m_stayAfterHitDynamic;

	public float m_stayTTL = 1f;

	public bool m_attachToRigidBody;

	public bool m_attachToClosestBone;

	public float m_attachPenetration;

	public float m_attachBoneNearify = 0.25f;

	public GameObject m_hideOnHit;

	public bool m_stopEmittersOnHit = true;

	public EffectList m_hitEffects = new EffectList();

	public EffectList m_hitWaterEffects = new EffectList();

	[Header("Bounce")]
	public bool m_bounce;

	public bool m_bounceOnWater;

	[Range(0f, 1f)]
	public float m_bouncePower = 0.85f;

	[Range(0f, 1f)]
	public float m_bounceRoughness = 0.3f;

	[Min(1f)]
	public int m_maxBounces = 99;

	[Min(0.01f)]
	public float m_minBounceVel = 0.25f;

	[Header("Spawn on hit")]
	public bool m_respawnItemOnHit;

	public bool m_spawnOnTtl;

	public GameObject m_spawnOnHit;

	[Range(0f, 1f)]
	public float m_spawnOnHitChance = 1f;

	public int m_spawnCount = 1;

	public List<GameObject> m_randomSpawnOnHit = new List<GameObject>();

	public int m_randomSpawnOnHitCount = 1;

	public bool m_randomSpawnSkipLava;

	public bool m_showBreakMessage;

	public bool m_staticHitOnly;

	public bool m_groundHitOnly;

	public Vector3 m_spawnOffset = Vector3.zero;

	public bool m_copyProjectileRotation = true;

	public bool m_spawnRandomRotation;

	public bool m_spawnFacingRotation;

	public EffectList m_spawnOnHitEffects = new EffectList();

	public OnProjectileHit m_onHit;

	[Header("Projectile Spawning")]
	public bool m_spawnProjectileNewVelocity;

	public float m_spawnProjectileMinVel = 1f;

	public float m_spawnProjectileMaxVel = 5f;

	[Range(0f, 1f)]
	public float m_spawnProjectileRandomDir;

	public bool m_spawnProjectileHemisphereDir;

	public bool m_projectilesInheritHitData;

	public bool m_onlySpawnedProjectilesDealDamage;

	public bool m_divideDamageBetweenProjectiles;

	[Header("Rotate projectile")]
	public float m_rotateVisual;

	public float m_rotateVisualY;

	public float m_rotateVisualZ;

	public GameObject m_visual;

	public bool m_canChangeVisuals;

	private ZNetView m_nview;

	private GameObject m_attachParent;

	private Vector3 m_attachParentOffset;

	private Quaternion m_attachParentOffsetRot;

	private bool m_hasLeftShields = true;

	private Vector3 m_vel = Vector3.zero;

	private Character m_owner;

	[NonSerialized]
	public Skills.SkillType m_skill;

	[NonSerialized]
	public float m_raiseSkillAmount = 1f;

	private ItemDrop.ItemData m_weapon;

	private ItemDrop.ItemData m_ammo;

	[NonSerialized]
	public ItemDrop.ItemData m_spawnItem;

	private HitData m_originalHitData;

	private bool m_didHit;

	private int m_bounceCount;

	private bool m_didBounce;

	private bool m_changedVisual;

	[HideInInspector]
	public Vector3 m_startPoint;

	private bool m_haveStartPoint;

	private static int s_rayMaskSolids;

	public bool HasBeenOutsideShields => m_hasLeftShields;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (s_rayMaskSolids == 0)
		{
			s_rayMaskSolids = LayerMask.GetMask(new string[12]
			{
				"Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "terrain", "character", "character_net", "character_ghost", "hitbox",
				"character_noenv", "vehicle"
			});
		}
		if (!string.IsNullOrEmpty(m_statusEffect))
		{
			m_statusEffectHash = StringExtensionMethods.GetStableHashCode(m_statusEffect);
		}
		m_nview.Register<float>("RPC_SetStayTTL", RPC_SetStayTTL);
		m_nview.Register("RPC_OnHit", RPC_OnHit);
		m_nview.Register<ZDOID>("RPC_Attach", RPC_Attach);
		UpdateVisual();
	}

	public string GetTooltipString(int itemQuality)
	{
		return "";
	}

	private void FixedUpdate()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return;
		}
		UpdateRotation(Time.fixedDeltaTime);
		if (!m_nview.IsOwner())
		{
			return;
		}
		if (!m_didHit)
		{
			Vector3 val = ((Component)this).transform.position;
			if (m_haveStartPoint)
			{
				val = m_startPoint;
			}
			m_vel += Vector3.down * (m_gravity * Time.fixedDeltaTime);
			float num = Mathf.Pow(((Vector3)(ref m_vel)).magnitude, 2f) * m_drag * Time.fixedDeltaTime;
			m_vel += num * -((Vector3)(ref m_vel)).normalized;
			Transform transform = ((Component)this).transform;
			transform.position += m_vel * Time.fixedDeltaTime;
			if (m_rotateVisual == 0f)
			{
				((Component)this).transform.rotation = Quaternion.LookRotation(m_vel);
			}
			if (m_canHitWater)
			{
				float liquidLevel = Floating.GetLiquidLevel(((Component)this).transform.position);
				if (((Component)this).transform.position.y < liquidLevel)
				{
					OnHit(null, ((Component)this).transform.position, water: true, Vector3.up);
				}
			}
			m_didBounce = false;
			if (!m_didHit)
			{
				Vector3 val2 = ((Component)this).transform.position - val;
				if (!m_haveStartPoint)
				{
					val -= ((Vector3)(ref val2)).normalized * (((Vector3)(ref val2)).magnitude * 0.5f);
				}
				RaycastHit[] array = ((m_rayRadius != 0f) ? Physics.SphereCastAll(val, m_rayRadius, ((Vector3)(ref val2)).normalized, ((Vector3)(ref val2)).magnitude, s_rayMaskSolids) : Physics.RaycastAll(val, ((Vector3)(ref val2)).normalized, ((Vector3)(ref val2)).magnitude * 1.5f, s_rayMaskSolids));
				Debug.DrawLine(val, ((Component)this).transform.position, (array.Length != 0) ? Color.red : Color.yellow, 5f);
				if (array.Length != 0)
				{
					Array.Sort(array, (RaycastHit x, RaycastHit y) => ((RaycastHit)(ref x)).distance.CompareTo(((RaycastHit)(ref y)).distance));
					RaycastHit[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						RaycastHit val3 = array2[i];
						Vector3 hitPoint = ((((RaycastHit)(ref val3)).distance == 0f) ? val : ((RaycastHit)(ref val3)).point);
						OnHit(((RaycastHit)(ref val3)).collider, hitPoint, water: false, ((RaycastHit)(ref val3)).normal);
						if (m_didHit || m_didBounce)
						{
							break;
						}
					}
				}
			}
			if (m_haveStartPoint)
			{
				m_haveStartPoint = false;
			}
		}
		if (m_ttl > 0f)
		{
			m_ttl -= Time.fixedDeltaTime;
			if (m_ttl <= 0f)
			{
				if (m_spawnOnTtl)
				{
					SpawnOnHit(null, null, -((Vector3)(ref m_vel)).normalized);
				}
				ZNetScene.instance.Destroy(((Component)this).gameObject);
			}
		}
		if (m_nview.IsValid())
		{
			ShieldGenerator.CheckProjectile(this);
		}
	}

	private void Update()
	{
		UpdateVisual();
	}

	private void LateUpdate()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_attachParent))
		{
			Vector3 val = m_attachParent.transform.position - m_attachParentOffset;
			Quaternion val2 = m_attachParent.transform.rotation * m_attachParentOffsetRot;
			((Component)this).transform.position = Utils.RotatePointAroundPivot(val, m_attachParent.transform.position, val2);
			((Component)this).transform.localRotation = val2;
		}
	}

	private void UpdateVisual()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (!m_canChangeVisuals || m_nview == null || !m_nview.IsValid() || m_changedVisual || !m_nview.GetZDO().GetString(ZDOVars.s_visual, out var value))
		{
			return;
		}
		ZLog.Log((object)("Visual prefab is " + value));
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(value);
		if (itemPrefab.GetComponent<ItemDrop>() != null)
		{
			GameObject attachPrefab = ItemStand.GetAttachPrefab(itemPrefab);
			if (!((Object)(object)attachPrefab == (Object)null))
			{
				attachPrefab = ItemStand.GetAttachGameObject(attachPrefab);
				m_visual.gameObject.SetActive(false);
				m_visual = Object.Instantiate<GameObject>(attachPrefab, ((Component)this).transform);
				m_visual.transform.localPosition = Vector3.zero;
				m_changedVisual = true;
			}
		}
	}

	public Vector3 GetVelocity()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner())
		{
			return Vector3.zero;
		}
		if (m_didHit)
		{
			return Vector3.zero;
		}
		return m_vel;
	}

	private void UpdateRotation(float dt)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_visual == (Object)null) && ((double)m_rotateVisual != 0.0 || (double)m_rotateVisualY != 0.0 || (double)m_rotateVisualZ != 0.0))
		{
			m_visual.transform.Rotate(new Vector3(m_rotateVisual * dt, m_rotateVisualY * dt, m_rotateVisualZ * dt));
		}
	}

	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		m_owner = owner;
		m_vel = velocity;
		m_ammo = ammo;
		m_weapon = item;
		if (hitNoise >= 0f)
		{
			m_hitNoise = hitNoise;
		}
		if (hitData != null)
		{
			m_originalHitData = hitData;
			m_damage = hitData.m_damage;
			m_blockable = hitData.m_blockable;
			m_dodgeable = hitData.m_dodgeable;
			m_attackForce = hitData.m_pushForce;
			m_backstabBonus = hitData.m_backstabBonus;
			m_healthReturn = hitData.m_healthReturn;
			if (m_statusEffectHash != hitData.m_statusEffectHash)
			{
				m_statusEffectHash = hitData.m_statusEffectHash;
				m_statusEffect = "";
			}
			m_skill = hitData.m_skill;
			m_raiseSkillAmount = hitData.m_skillRaiseAmount;
		}
		if ((Object)(object)m_spawnOnHit != (Object)null && m_onlySpawnedProjectilesDealDamage)
		{
			m_damage.Modify(0f);
		}
		if (m_respawnItemOnHit)
		{
			m_spawnItem = item;
		}
		if (m_doOwnerRaytest && Object.op_Implicit((Object)(object)owner))
		{
			m_startPoint = owner.GetCenterPoint();
			m_startPoint.y = ((Component)this).transform.position.y;
			m_haveStartPoint = true;
		}
		else
		{
			m_startPoint = ((Component)this).transform.position;
		}
		LineConnect component = ((Component)this).GetComponent<LineConnect>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)owner))
		{
			component.SetPeer(owner.GetZDOID());
		}
		m_hasLeftShields = !ShieldGenerator.IsInsideShield(((Component)this).transform.position);
	}

	private void DoAOE(Vector3 hitPoint, ref bool hitCharacter, ref bool didDamage)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		Collider[] array = Physics.OverlapSphere(hitPoint, m_aoe, s_rayMaskSolids, (QueryTriggerInteraction)0);
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		Collider[] array2 = array;
		foreach (Collider val in array2)
		{
			GameObject val2 = FindHitObject(val);
			IDestructible component = val2.GetComponent<IDestructible>();
			if (component == null || hashSet.Contains(val2))
			{
				continue;
			}
			hashSet.Add(val2);
			if (IsValidTarget(component))
			{
				if (component is Character)
				{
					hitCharacter = true;
				}
				Vector3 val3 = val.ClosestPointOnBounds(hitPoint);
				Vector3 val4 = ((Vector3.Distance(val3, hitPoint) > 0.1f) ? (val3 - hitPoint) : m_vel);
				val4.y = 0f;
				((Vector3)(ref val4)).Normalize();
				HitData hitData = new HitData();
				hitData.m_hitCollider = val;
				hitData.m_damage = m_damage;
				hitData.m_pushForce = m_attackForce;
				hitData.m_backstabBonus = m_backstabBonus;
				hitData.m_ranged = true;
				hitData.m_point = val3;
				hitData.m_dir = ((Vector3)(ref val4)).normalized;
				hitData.m_statusEffectHash = m_statusEffectHash;
				hitData.m_skillLevel = (Object.op_Implicit((Object)(object)m_owner) ? m_owner.GetSkillLevel(m_skill) : 1f);
				hitData.m_dodgeable = m_dodgeable;
				hitData.m_blockable = m_blockable;
				hitData.m_skill = m_skill;
				hitData.m_skillRaiseAmount = m_raiseSkillAmount;
				hitData.SetAttacker(m_owner);
				hitData.m_hitType = ((!(hitData.GetAttacker() is Player)) ? HitData.HitType.EnemyHit : HitData.HitType.PlayerHit);
				hitData.m_healthReturn = m_healthReturn;
				component.Damage(hitData);
				didDamage = true;
			}
		}
	}

	private bool IsValidTarget(IDestructible destr)
	{
		Character character = destr as Character;
		if (Object.op_Implicit((Object)(object)character))
		{
			if ((Object)(object)character == (Object)(object)m_owner)
			{
				return false;
			}
			if ((Object)(object)m_owner != (Object)null)
			{
				bool flag = BaseAI.IsEnemy(m_owner, character) || (Object.op_Implicit((Object)(object)character.GetBaseAI()) && character.GetBaseAI().IsAggravatable() && m_owner.IsPlayer());
				if (!m_owner.IsPlayer() && !flag)
				{
					return false;
				}
				if (m_owner.IsPlayer() && !m_owner.IsPVPEnabled() && !flag)
				{
					return false;
				}
			}
			if (m_dodgeable && character.IsDodgeInvincible())
			{
				if (character.IsPlayer())
				{
					(character as Player).HitWhileDodging();
				}
				return false;
			}
		}
		return true;
	}

	public void OnHit(Collider collider, Vector3 hitPoint, bool water, Vector3 normal)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = (Object.op_Implicit((Object)(object)collider) ? FindHitObject(collider) : null);
		bool didDamage = false;
		bool hitCharacter = false;
		bool flag = m_bounce && normal != Vector3.zero;
		if (water)
		{
			flag = flag && m_bounceOnWater;
		}
		IDestructible destructible = (Object.op_Implicit((Object)(object)val) ? val.GetComponent<IDestructible>() : null);
		if (destructible != null)
		{
			hitCharacter = destructible is Character;
			flag = flag && !hitCharacter;
			if (!IsValidTarget(destructible))
			{
				return;
			}
		}
		if (Object.op_Implicit((Object)(object)collider))
		{
			IHitProjectile component = ((Component)collider).GetComponent<IHitProjectile>();
			if (component != null && !component.OnProjectileHit(m_owner, m_weapon, this, collider, hitPoint, water, normal))
			{
				return;
			}
		}
		if (flag && m_bounceCount < m_maxBounces && ((Vector3)(ref m_vel)).magnitude > m_minBounceVel)
		{
			Vector3 normalized = ((Vector3)(ref m_vel)).normalized;
			if (m_bounceRoughness > 0f)
			{
				Vector3 onUnitSphere = Random.onUnitSphere;
				float num = Vector3.Dot(normal, onUnitSphere);
				onUnitSphere *= Mathf.Sign(num);
				Vector3 val2 = Vector3.Lerp(normal, onUnitSphere, m_bounceRoughness);
				normal = ((Vector3)(ref val2)).normalized;
			}
			m_vel = Vector3.Reflect(normalized, normal) * (((Vector3)(ref m_vel)).magnitude * m_bouncePower);
			m_bounceCount++;
			m_didBounce = true;
			return;
		}
		if (m_aoe > 0f)
		{
			DoAOE(hitPoint, ref hitCharacter, ref didDamage);
		}
		else if (destructible != null)
		{
			HitData hitData = new HitData();
			hitData.m_hitCollider = collider;
			hitData.m_damage = m_damage;
			hitData.m_pushForce = m_attackForce;
			hitData.m_backstabBonus = m_backstabBonus;
			hitData.m_point = hitPoint;
			hitData.m_dir = ((Component)this).transform.forward;
			hitData.m_statusEffectHash = m_statusEffectHash;
			hitData.m_dodgeable = m_dodgeable;
			hitData.m_blockable = m_blockable;
			hitData.m_ranged = true;
			hitData.m_skill = m_skill;
			hitData.m_skillRaiseAmount = m_raiseSkillAmount;
			hitData.SetAttacker(m_owner);
			hitData.m_hitType = ((!(hitData.GetAttacker() is Player)) ? HitData.HitType.EnemyHit : HitData.HitType.PlayerHit);
			hitData.m_healthReturn = m_healthReturn;
			destructible.Damage(hitData);
			if (m_healthReturn > 0f && Object.op_Implicit((Object)(object)m_owner))
			{
				m_owner.Heal(m_healthReturn);
			}
			didDamage = true;
		}
		if (water)
		{
			m_hitWaterEffects.Create(hitPoint, Quaternion.identity);
		}
		else
		{
			m_hitEffects.Create(hitPoint, Quaternion.identity);
		}
		if ((Object)(object)m_spawnOnHit != (Object)null || m_spawnItem != null || m_randomSpawnOnHit.Count > 0)
		{
			SpawnOnHit(val, collider, normal);
		}
		m_onHit?.Invoke(collider, hitPoint, water);
		if (m_hitNoise > 0f)
		{
			BaseAI.DoProjectileHitNoise(((Component)this).transform.position, m_hitNoise, m_owner);
		}
		if (didDamage && (Object)(object)m_owner != (Object)null && hitCharacter)
		{
			m_owner.RaiseSkill(m_skill, m_raiseSkillAmount);
			m_owner.AddAdrenaline(m_adrenaline);
		}
		m_didHit = true;
		((Component)this).transform.position = hitPoint;
		m_nview.InvokeRPC("RPC_OnHit");
		m_ttl = m_stayTTL;
		if (Object.op_Implicit((Object)(object)collider) && (Object)(object)collider.attachedRigidbody != (Object)null)
		{
			ZNetView componentInParent = ((Component)collider).gameObject.GetComponentInParent<ZNetView>();
			if (Object.op_Implicit((Object)(object)componentInParent) && (m_attachToClosestBone || m_attachToRigidBody))
			{
				m_nview.InvokeRPC("RPC_Attach", componentInParent.GetZDO().m_uid);
			}
			else if (!m_stayAfterHitDynamic)
			{
				ZNetScene.instance.Destroy(((Component)this).gameObject);
			}
		}
		else if (!m_stayAfterHitStatic)
		{
			ZNetScene.instance.Destroy(((Component)this).gameObject);
		}
	}

	private void RPC_OnHit(long sender)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_hideOnHit))
		{
			m_hideOnHit.SetActive(false);
		}
		if (m_stopEmittersOnHit)
		{
			ParticleSystem[] componentsInChildren = ((Component)this).GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				EmissionModule emission = componentsInChildren[i].emission;
				((EmissionModule)(ref emission)).enabled = false;
			}
		}
	}

	private void RPC_Attach(long sender, ZDOID parent)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		m_attachParent = ZNetScene.instance.FindInstance(parent);
		if (!Object.op_Implicit((Object)(object)m_attachParent))
		{
			return;
		}
		if (m_attachToClosestBone)
		{
			float dist = float.MaxValue;
			Animator componentInChildren = m_attachParent.gameObject.GetComponentInChildren<Animator>();
			if (componentInChildren != null)
			{
				Utils.IterateHierarchy(((Component)componentInChildren).gameObject, (ChildHandler)delegate(GameObject obj)
				{
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0016: Unknown result type (might be due to invalid IL or missing references)
					float num = Vector3.Distance(((Component)this).transform.position, obj.transform.position);
					if (num < dist)
					{
						dist = num;
						m_attachParent = obj;
					}
				}, false);
			}
		}
		Transform transform = ((Component)this).transform;
		transform.position += ((Component)this).transform.forward * m_attachPenetration;
		Transform transform2 = ((Component)this).transform;
		transform2.position += (m_attachParent.transform.position - ((Component)this).transform.position) * m_attachBoneNearify;
		m_attachParentOffset = m_attachParent.transform.position - ((Component)this).transform.position;
		m_attachParentOffsetRot = Quaternion.Inverse(m_attachParent.transform.localRotation * ((Component)this).transform.localRotation);
	}

	private void SpawnOnHit(GameObject go, Collider collider, Vector3 normal)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		if ((m_groundHitOnly && (go == null || (Object)(object)go.GetComponent<Heightmap>() == (Object)null)) || (m_staticHitOnly && ((Object.op_Implicit((Object)(object)collider) && (Object)(object)collider.attachedRigidbody != (Object)null) || (Object.op_Implicit((Object)(object)go) && go.GetComponent<IDestructible>() != null))))
		{
			return;
		}
		Vector3 val = ((Component)this).transform.position + ((Component)this).transform.TransformDirection(m_spawnOffset);
		Quaternion val2 = Quaternion.identity;
		if (m_copyProjectileRotation)
		{
			val2 = ((Component)this).transform.rotation;
		}
		if (m_spawnRandomRotation)
		{
			val2 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
		}
		if (m_spawnFacingRotation)
		{
			Quaternion rotation = ((Component)this).transform.rotation;
			val2 = Quaternion.Euler(0f, ((Quaternion)(ref rotation)).eulerAngles.y, 0f);
		}
		if ((Object)(object)m_spawnOnHit != (Object)null && (m_spawnOnHitChance >= 1f || Random.value < m_spawnOnHitChance))
		{
			for (int i = 0; i < m_spawnCount; i++)
			{
				GameObject val3 = Object.Instantiate<GameObject>(m_spawnOnHit, val, val2);
				Vector3 normalized = ((Vector3)(ref m_vel)).normalized;
				Vector3 val4 = Random.onUnitSphere;
				if (m_spawnProjectileHemisphereDir)
				{
					val4 *= Mathf.Sign(Vector3.Dot(normal, val4));
				}
				Vector3 val5 = Vector3.Lerp(normalized, val4, m_spawnProjectileRandomDir);
				normalized = ((Vector3)(ref val5)).normalized;
				float num = ((Vector3)(ref m_vel)).magnitude;
				if (m_spawnProjectileNewVelocity)
				{
					num = Random.Range(m_spawnProjectileMinVel, m_spawnProjectileMaxVel);
				}
				IProjectile componentInChildren = val3.GetComponentInChildren<IProjectile>();
				if (componentInChildren == null)
				{
					continue;
				}
				Transform transform = val3.transform;
				transform.position += normal * 0.25f;
				HitData hitData = null;
				if (m_projectilesInheritHitData)
				{
					hitData = m_originalHitData;
					if (m_divideDamageBetweenProjectiles)
					{
						hitData.m_damage.Modify(1f / (float)m_spawnCount);
					}
				}
				componentInChildren.Setup(m_owner, normalized * num, m_hitNoise, hitData, m_weapon, m_ammo);
			}
		}
		if (m_spawnItem != null)
		{
			ItemDrop.DropItem(m_spawnItem, 1, val, ((Component)this).transform.rotation);
		}
		if (m_randomSpawnOnHit.Count > 0 && (!m_randomSpawnSkipLava || !ZoneSystem.instance.IsLava(((Component)this).transform.position)))
		{
			for (int j = 0; j < m_randomSpawnOnHitCount; j++)
			{
				GameObject val6 = m_randomSpawnOnHit[Random.Range(0, m_randomSpawnOnHit.Count)];
				if (Object.op_Implicit((Object)(object)val6))
				{
					Object.Instantiate<GameObject>(val6, val, val2).GetComponent<IProjectile>()?.Setup(m_owner, m_vel, m_hitNoise, null, null, m_ammo);
				}
			}
		}
		m_spawnOnHitEffects.Create(val, Quaternion.identity);
	}

	public void SetStayTTL(float seconds)
	{
		if (seconds <= 0f)
		{
			seconds = 0.001f;
		}
		if (Object.op_Implicit((Object)(object)m_nview))
		{
			if (m_nview.IsOwner())
			{
				m_stayTTL = (m_ttl = seconds);
				return;
			}
			m_nview.InvokeRPC(m_nview.GetZDO().GetOwner(), "RPC_SetStayTTL", seconds);
		}
	}

	public void RPC_SetStayTTL(long sender, float sec)
	{
		m_stayTTL = (m_ttl = sec);
	}

	public static GameObject FindHitObject(Collider collider)
	{
		IDestructible componentInParent = ((Component)collider).gameObject.GetComponentInParent<IDestructible>();
		if (componentInParent != null)
		{
			return ((Component)((componentInParent is MonoBehaviour) ? componentInParent : null)).gameObject;
		}
		if (Object.op_Implicit((Object)(object)collider.attachedRigidbody))
		{
			return ((Component)collider.attachedRigidbody).gameObject;
		}
		return ((Component)collider).gameObject;
	}

	public void TriggerShieldsLeftFlag()
	{
		m_hasLeftShields = true;
	}
}
