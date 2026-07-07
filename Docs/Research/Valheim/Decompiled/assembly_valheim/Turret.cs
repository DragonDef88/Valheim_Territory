using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Turret : MonoBehaviour, Hoverable, Interactable, IPieceMarker, IHasHoverMenu
{
	[Serializable]
	public struct AmmoType
	{
		public ItemDrop m_ammo;

		public GameObject m_visual;
	}

	[Serializable]
	public struct TrophyTarget
	{
		public string m_nameOverride;

		public ItemDrop m_item;

		public List<Character> m_targets;
	}

	public string m_name = "Turret";

	[Header("Turret")]
	public GameObject m_turretBody;

	public GameObject m_turretBodyArmed;

	public GameObject m_turretBodyUnarmed;

	public GameObject m_turretNeck;

	public GameObject m_eye;

	[Header("Look & Scan")]
	public float m_turnRate = 10f;

	public float m_horizontalAngle = 25f;

	public float m_verticalAngle = 20f;

	public float m_viewDistance = 10f;

	public float m_noTargetScanRate = 10f;

	public float m_lookAcceleration = 1.2f;

	public float m_lookDeacceleration = 0.05f;

	public float m_lookMinDegreesDelta = 0.005f;

	[Header("Attack Settings (rest in projectile)")]
	public ItemDrop m_defaultAmmo;

	public float m_attackCooldown = 1f;

	public float m_attackWarmup = 1f;

	public float m_hitNoise = 10f;

	public float m_shootWhenAimDiff = 0.9f;

	public float m_predictionModifier = 1f;

	public float m_updateTargetIntervalNear = 1f;

	public float m_updateTargetIntervalFar = 10f;

	[Header("Ammo")]
	public int m_maxAmmo;

	public string m_ammoType = "$ammo_turretbolt";

	public List<AmmoType> m_allowedAmmo = new List<AmmoType>();

	public bool m_returnAmmoOnDestroy = true;

	public float m_holdRepeatInterval = 0.2f;

	[Header("Target mode: Everything")]
	public bool m_targetPlayers = true;

	public bool m_targetTamed = true;

	public bool m_targetEnemies = true;

	[Header("Target mode: Configured")]
	public bool m_targetTamedConfig;

	public List<TrophyTarget> m_configTargets = new List<TrophyTarget>();

	public int m_maxConfigTargets = 1;

	[Header("Effects")]
	public CircleProjector m_marker;

	public float m_markerHideTime = 0.5f;

	public EffectList m_shootEffect;

	public EffectList m_addAmmoEffect;

	public EffectList m_reloadEffect;

	public EffectList m_warmUpStartEffect;

	public EffectList m_newTargetEffect;

	public EffectList m_lostTargetEffect;

	public EffectList m_setTargetEffect;

	private ZNetView m_nview;

	private GameObject m_lastProjectile;

	private ItemDrop.ItemData m_lastAmmo;

	private Character m_target;

	private bool m_haveTarget;

	private Quaternion m_baseBodyRotation;

	private Quaternion m_baseNeckRotation;

	private Quaternion m_lastRotation;

	private float m_aimDiffToTarget;

	private float m_updateTargetTimer;

	private float m_lastUseTime;

	private float m_scan;

	private readonly List<ItemDrop> m_targetItems = new List<ItemDrop>();

	private readonly List<Character> m_targetCharacters = new List<Character>();

	private string m_targetsText;

	private readonly StringBuilder sb = new StringBuilder();

	private uint m_lastUpdateTargetRevision = uint.MaxValue;

	protected void Awake()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (Object.op_Implicit((Object)(object)m_nview))
		{
			m_nview.Register<string>("RPC_AddAmmo", RPC_AddAmmo);
			m_nview.Register<ZDOID>("RPC_SetTarget", RPC_SetTarget);
		}
		m_updateTargetTimer = Random.Range(0f, m_updateTargetIntervalNear);
		m_baseBodyRotation = m_turretBody.transform.localRotation;
		m_baseNeckRotation = m_turretNeck.transform.localRotation;
		WearNTear component = ((Component)this).GetComponent<WearNTear>();
		if (component != null)
		{
			component.m_onDestroyed = (Action)Delegate.Combine(component.m_onDestroyed, new Action(OnDestroyed));
		}
		if (Object.op_Implicit((Object)(object)m_marker))
		{
			m_marker.m_radius = m_viewDistance;
			((Component)m_marker).gameObject.SetActive(false);
		}
		foreach (AmmoType item in m_allowedAmmo)
		{
			item.m_visual.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid())
		{
			UpdateVisualBolt();
		}
		ReadTargets();
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		UpdateReloadState();
		UpdateMarker(fixedDeltaTime);
		if (!m_nview.IsValid())
		{
			return;
		}
		UpdateTurretRotation();
		UpdateVisualBolt();
		if (!m_nview.IsOwner())
		{
			if (m_nview.IsValid() && m_lastUpdateTargetRevision != m_nview.GetZDO().DataRevision)
			{
				m_lastUpdateTargetRevision = m_nview.GetZDO().DataRevision;
				ReadTargets();
			}
		}
		else
		{
			UpdateTarget(fixedDeltaTime);
			UpdateAttack(fixedDeltaTime);
		}
	}

	private void UpdateTurretRotation()
	{
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		if (IsCoolingDown())
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		bool flag = Object.op_Implicit((Object)(object)m_target) && HasAmmo();
		Vector3 val2;
		Quaternion rotation;
		if (flag)
		{
			if (m_lastAmmo == null)
			{
				m_lastAmmo = GetAmmoItem();
			}
			if (m_lastAmmo == null)
			{
				ZLog.LogWarning((object)"Turret had invalid ammo, resetting ammo");
				m_nview.GetZDO().Set(ZDOVars.s_ammo, 0);
				return;
			}
			float num = Vector2.Distance(Vector2.op_Implicit(((Component)m_target).transform.position), Vector2.op_Implicit(m_eye.transform.position)) / m_lastAmmo.m_shared.m_attack.m_projectileVel;
			Vector3 val = m_target.GetVelocity() * num * m_predictionModifier;
			val2 = ((Component)m_target).transform.position + val - m_turretBody.transform.position;
			ref float y = ref val2.y;
			float num2 = y;
			CapsuleCollider componentInChildren = ((Component)m_target).GetComponentInChildren<CapsuleCollider>();
			y = num2 + ((componentInChildren != null) ? (componentInChildren.height / 2f) : 1f);
		}
		else if (!HasAmmo())
		{
			val2 = ((Component)this).transform.forward + new Vector3(0f, -0.3f, 0f);
		}
		else
		{
			m_scan += fixedDeltaTime;
			if (m_scan > m_noTargetScanRate * 2f)
			{
				m_scan = 0f;
			}
			rotation = ((Component)this).transform.rotation;
			val2 = Quaternion.Euler(0f, ((Quaternion)(ref rotation)).eulerAngles.y + (float)((m_scan - m_noTargetScanRate > 0f) ? 1 : (-1)) * m_horizontalAngle, 0f) * Vector3.forward;
		}
		((Vector3)(ref val2)).Normalize();
		Quaternion val3 = Quaternion.LookRotation(val2, Vector3.up);
		Vector3 eulerAngles = ((Quaternion)(ref val3)).eulerAngles;
		rotation = ((Component)this).transform.rotation;
		float y2 = ((Quaternion)(ref rotation)).eulerAngles.y;
		eulerAngles.y -= y2;
		if (m_horizontalAngle >= 0f)
		{
			float num3 = eulerAngles.y;
			if (num3 > 180f)
			{
				num3 -= 360f;
			}
			else if (num3 < -180f)
			{
				num3 += 360f;
			}
			if (num3 > m_horizontalAngle)
			{
				((Vector3)(ref eulerAngles))._002Ector(eulerAngles.x, m_horizontalAngle + y2, eulerAngles.z);
				((Quaternion)(ref val3)).eulerAngles = eulerAngles;
			}
			else if (num3 < 0f - m_horizontalAngle)
			{
				((Vector3)(ref eulerAngles))._002Ector(eulerAngles.x, 0f - m_horizontalAngle + y2, eulerAngles.z);
				((Quaternion)(ref val3)).eulerAngles = eulerAngles;
			}
		}
		Quaternion val4 = Utils.RotateTorwardsSmooth(m_turretBody.transform.rotation, val3, m_lastRotation, m_turnRate * fixedDeltaTime, m_lookAcceleration, m_lookDeacceleration, m_lookMinDegreesDelta);
		m_lastRotation = m_turretBody.transform.rotation;
		m_turretBody.transform.rotation = m_baseBodyRotation * val4;
		Transform transform = m_turretNeck.transform;
		Quaternion baseNeckRotation = m_baseNeckRotation;
		rotation = m_turretBody.transform.rotation;
		float y3 = ((Quaternion)(ref rotation)).eulerAngles.y;
		rotation = m_turretBody.transform.rotation;
		transform.rotation = baseNeckRotation * Quaternion.Euler(0f, y3, ((Quaternion)(ref rotation)).eulerAngles.z);
		m_aimDiffToTarget = (flag ? Quaternion.Dot(val4, val3) : (-1f));
	}

	private void UpdateTarget(float dt)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return;
		}
		if (!HasAmmo())
		{
			if (m_haveTarget)
			{
				m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetTarget", ZDOID.None);
			}
			return;
		}
		m_updateTargetTimer -= dt;
		if (m_updateTargetTimer <= 0f)
		{
			m_updateTargetTimer = (Character.IsCharacterInRange(((Component)this).transform.position, 40f) ? m_updateTargetIntervalNear : m_updateTargetIntervalFar);
			Character character = BaseAI.FindClosestCreature(((Component)this).transform, m_eye.transform.position, 0f, m_viewDistance, m_horizontalAngle, alerted: false, mistVision: false, passiveAggresive: true, m_targetPlayers, (m_targetItems.Count > 0) ? m_targetTamedConfig : m_targetTamed, m_targetEnemies, m_targetCharacters);
			if ((Object)(object)character != (Object)(object)m_target)
			{
				if (Object.op_Implicit((Object)(object)character))
				{
					m_newTargetEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
				}
				else
				{
					m_lostTargetEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
				}
				m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetTarget", Object.op_Implicit((Object)(object)character) ? character.GetZDOID() : ZDOID.None);
			}
		}
		if (m_haveTarget && (!Object.op_Implicit((Object)(object)m_target) || m_target.IsDead()))
		{
			ZLog.Log((object)"Target is gone");
			m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetTarget", ZDOID.None);
			m_lostTargetEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
		}
	}

	private void UpdateAttack(float dt)
	{
		if (Object.op_Implicit((Object)(object)m_target) && !(m_aimDiffToTarget < m_shootWhenAimDiff) && HasAmmo() && !IsCoolingDown())
		{
			ShootProjectile();
		}
	}

	public void ShootProjectile()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = m_eye.transform;
		m_shootEffect.Create(transform.position, transform.rotation);
		m_nview.GetZDO().Set(ZDOVars.s_lastAttack, (float)ZNet.instance.GetTimeSeconds());
		m_lastAmmo = GetAmmoItem();
		int @int = m_nview.GetZDO().GetInt(ZDOVars.s_ammo);
		int num = Mathf.Min(1, (m_maxAmmo == 0) ? m_lastAmmo.m_shared.m_attack.m_projectiles : Mathf.Min(@int, m_lastAmmo.m_shared.m_attack.m_projectiles));
		if (m_maxAmmo > 0)
		{
			m_nview.GetZDO().Set(ZDOVars.s_ammo, @int - num);
		}
		ZLog.Log((object)$"Turret '{((Object)this).name}' is shooting {num} projectiles, ammo: {@int}/{m_maxAmmo}");
		for (int i = 0; i < num; i++)
		{
			Vector3 forward = transform.forward;
			Vector3 val = Vector3.Cross(forward, Vector3.up);
			float projectileAccuracy = m_lastAmmo.m_shared.m_attack.m_projectileAccuracy;
			Quaternion val2 = Quaternion.AngleAxis(Random.Range(0f - projectileAccuracy, projectileAccuracy), Vector3.up);
			forward = Quaternion.AngleAxis(Random.Range(0f - projectileAccuracy, projectileAccuracy), val) * forward;
			forward = val2 * forward;
			m_lastProjectile = Object.Instantiate<GameObject>(m_lastAmmo.m_shared.m_attack.m_attackProjectile, transform.position, transform.rotation);
			HitData hitData = new HitData();
			hitData.m_toolTier = (short)m_lastAmmo.m_shared.m_toolTier;
			hitData.m_pushForce = m_lastAmmo.m_shared.m_attackForce;
			hitData.m_backstabBonus = m_lastAmmo.m_shared.m_backstabBonus;
			hitData.m_staggerMultiplier = m_lastAmmo.m_shared.m_attack.m_staggerMultiplier;
			hitData.m_damage.Add(m_lastAmmo.GetDamage());
			hitData.m_statusEffectHash = (Object.op_Implicit((Object)(object)m_lastAmmo.m_shared.m_attackStatusEffect) ? m_lastAmmo.m_shared.m_attackStatusEffect.NameHash() : 0);
			hitData.m_blockable = m_lastAmmo.m_shared.m_blockable;
			hitData.m_dodgeable = m_lastAmmo.m_shared.m_dodgeable;
			hitData.m_skill = m_lastAmmo.m_shared.m_skillType;
			hitData.m_itemWorldLevel = (byte)Game.m_worldLevel;
			hitData.m_hitType = HitData.HitType.Turret;
			if ((Object)(object)m_lastAmmo.m_shared.m_attackStatusEffect != (Object)null)
			{
				hitData.m_statusEffectHash = m_lastAmmo.m_shared.m_attackStatusEffect.NameHash();
			}
			m_lastProjectile.GetComponent<IProjectile>()?.Setup(null, forward * m_lastAmmo.m_shared.m_attack.m_projectileVel, m_hitNoise, hitData, null, m_lastAmmo);
		}
	}

	public bool IsCoolingDown()
	{
		if (!m_nview.IsValid())
		{
			return false;
		}
		return (double)(m_nview.GetZDO().GetFloat(ZDOVars.s_lastAttack) + m_attackCooldown) > ZNet.instance.GetTimeSeconds();
	}

	public bool HasAmmo()
	{
		if (m_maxAmmo != 0)
		{
			return GetAmmo() > 0;
		}
		return true;
	}

	public int GetAmmo()
	{
		return m_nview.GetZDO().GetInt(ZDOVars.s_ammo);
	}

	public string GetAmmoType()
	{
		if (!Object.op_Implicit((Object)(object)m_defaultAmmo))
		{
			return m_nview.GetZDO().GetString(ZDOVars.s_ammoType);
		}
		return ((Object)m_defaultAmmo).name;
	}

	public void UpdateReloadState()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = IsCoolingDown();
		if (!m_turretBodyArmed.activeInHierarchy && !flag)
		{
			m_reloadEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
		}
		m_turretBodyArmed.SetActive(!flag);
		m_turretBodyUnarmed.SetActive(flag);
	}

	private ItemDrop.ItemData GetAmmoItem()
	{
		string ammoType = GetAmmoType();
		GameObject prefab = ZNetScene.instance.GetPrefab(ammoType);
		if (!Object.op_Implicit((Object)(object)prefab))
		{
			ZLog.LogWarning((object)("Turret '" + ((Object)this).name + "' is trying to fire but has no ammo or default ammo!"));
			return null;
		}
		return prefab.GetComponent<ItemDrop>().m_itemData;
	}

	public string GetHoverText()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return "";
		}
		if (!m_targetEnemies)
		{
			return Localization.instance.Localize(m_name);
		}
		if (!PrivateArea.CheckAccess(((Component)this).transform.position, 0f, flash: false))
		{
			return Localization.instance.Localize(m_name + "\n$piece_noaccess");
		}
		sb.Clear();
		sb.Append((!HasAmmo()) ? (m_name + " ($piece_turret_noammo)") : $"{m_name} ({GetAmmo()} / {m_maxAmmo})");
		if (m_targetCharacters.Count == 0)
		{
			sb.Append(" $piece_turret_target $piece_turret_target_everything");
		}
		else
		{
			sb.Append(" $piece_turret_target ");
			sb.Append(m_targetsText);
		}
		sb.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_turret_addammo\n[<color=yellow><b>1-8</b></color>] $piece_turret_target_set");
		return Localization.instance.Localize(sb.ToString());
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			if (m_holdRepeatInterval <= 0f)
			{
				return false;
			}
			if (Time.time - m_lastUseTime < m_holdRepeatInterval)
			{
				return false;
			}
		}
		m_lastUseTime = Time.time;
		return UseItem(character, null);
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		if (item == null)
		{
			item = FindAmmoItem(user.GetInventory(), onlyCurrentlyLoadableType: true);
			if (item == null)
			{
				if (GetAmmo() > 0 && FindAmmoItem(user.GetInventory(), onlyCurrentlyLoadableType: false) != null)
				{
					ItemDrop component = ZNetScene.instance.GetPrefab(GetAmmoType()).GetComponent<ItemDrop>();
					user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_turretotherammo") + Localization.instance.Localize(component.m_itemData.m_shared.m_name));
					return false;
				}
				user.Message(MessageHud.MessageType.Center, "$msg_noturretammo");
				return false;
			}
		}
		foreach (TrophyTarget configTarget in m_configTargets)
		{
			if (!(item.m_shared.m_name == configTarget.m_item.m_itemData.m_shared.m_name))
			{
				continue;
			}
			if (m_targetItems.Contains(configTarget.m_item))
			{
				m_targetItems.Remove(configTarget.m_item);
			}
			else
			{
				if (m_targetItems.Count >= m_maxConfigTargets)
				{
					m_targetItems.RemoveAt(0);
				}
				m_targetItems.Add(configTarget.m_item);
			}
			SetTargets();
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$piece_turret_target_set_msg " + ((m_targetCharacters.Count == 0) ? "$piece_turret_target_everything" : m_targetsText)));
			m_setTargetEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			Game.instance.IncrementPlayerStat(PlayerStatType.TurretTrophySet);
			return true;
		}
		if (!IsItemAllowed(((Object)item.m_dropPrefab).name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_wontwork");
			return false;
		}
		if (GetAmmo() > 0 && GetAmmoType() != ((Object)item.m_dropPrefab).name)
		{
			ItemDrop component2 = ZNetScene.instance.GetPrefab(GetAmmoType()).GetComponent<ItemDrop>();
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_turretotherammo") + Localization.instance.Localize(component2.m_itemData.m_shared.m_name));
			return false;
		}
		ZLog.Log((object)("trying to add ammo " + item.m_shared.m_name));
		if (GetAmmo() >= m_maxAmmo)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
			return false;
		}
		user.Message(MessageHud.MessageType.Center, "$msg_added " + item.m_shared.m_name);
		user.GetInventory().RemoveItem(item, 1);
		Game.instance.IncrementPlayerStat(PlayerStatType.TurretAmmoAdded);
		m_nview.InvokeRPC("RPC_AddAmmo", ((Object)item.m_dropPrefab).name);
		return true;
	}

	private void RPC_AddAmmo(long sender, string name)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			if (!IsItemAllowed(name))
			{
				ZLog.Log((object)("Item not allowed " + name));
				return;
			}
			int @int = m_nview.GetZDO().GetInt(ZDOVars.s_ammo);
			m_nview.GetZDO().Set(ZDOVars.s_ammo, @int + 1);
			m_nview.GetZDO().Set(ZDOVars.s_ammoType, name);
			m_addAmmoEffect.Create(m_turretBody.transform.position, m_turretBody.transform.rotation);
			UpdateVisualBolt();
			ZLog.Log((object)("Added ammo " + name));
		}
	}

	private void RPC_SetTarget(long sender, ZDOID character)
	{
		GameObject val = ZNetScene.instance.FindInstance(character);
		if (Object.op_Implicit((Object)(object)val))
		{
			Character component = val.GetComponent<Character>();
			if (component != null)
			{
				m_target = component;
				m_haveTarget = true;
				return;
			}
		}
		m_target = null;
		m_haveTarget = false;
		m_scan = 0f;
	}

	private void UpdateVisualBolt()
	{
		if (HasAmmo())
		{
			_ = !IsCoolingDown();
		}
		else
			_ = 0;
		string ammoType = GetAmmoType();
		bool flag = HasAmmo() && !IsCoolingDown();
		foreach (AmmoType item in m_allowedAmmo)
		{
			bool flag2 = ((Object)item.m_ammo).name == ammoType;
			item.m_visual.SetActive(flag2 && flag);
		}
	}

	private bool IsItemAllowed(string itemName)
	{
		foreach (AmmoType item in m_allowedAmmo)
		{
			if (((Object)item.m_ammo).name == itemName)
			{
				return true;
			}
		}
		return false;
	}

	private ItemDrop.ItemData FindAmmoItem(Inventory inventory, bool onlyCurrentlyLoadableType)
	{
		if (onlyCurrentlyLoadableType && HasAmmo())
		{
			return inventory.GetAmmoItem(m_ammoType, GetAmmoType());
		}
		return inventory.GetAmmoItem(m_ammoType);
	}

	private void OnDestroyed()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner() && m_returnAmmoOnDestroy)
		{
			int ammo = GetAmmo();
			string ammoType = GetAmmoType();
			GameObject prefab = ZNetScene.instance.GetPrefab(ammoType);
			for (int i = 0; i < ammo; i++)
			{
				Vector3 val = ((Component)this).transform.position + Vector3.up + Random.insideUnitSphere * 0.3f;
				Quaternion val2 = Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
				Object.Instantiate<GameObject>(prefab, val, val2);
			}
		}
	}

	public void ShowHoverMarker()
	{
		ShowBuildMarker();
	}

	public void ShowBuildMarker()
	{
		if (Object.op_Implicit((Object)(object)m_marker))
		{
			((Component)m_marker).gameObject.SetActive(true);
			((MonoBehaviour)this).CancelInvoke("HideMarker");
			((MonoBehaviour)this).Invoke("HideMarker", m_markerHideTime);
		}
	}

	private void UpdateMarker(float dt)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_marker) && ((Behaviour)m_marker).isActiveAndEnabled)
		{
			CircleProjector marker = m_marker;
			Quaternion rotation = ((Component)this).transform.rotation;
			marker.m_start = ((Quaternion)(ref rotation)).eulerAngles.y - m_horizontalAngle;
			m_marker.m_turns = m_horizontalAngle * 2f / 360f;
		}
	}

	private void HideMarker()
	{
		if (Object.op_Implicit((Object)(object)m_marker))
		{
			((Component)m_marker).gameObject.SetActive(false);
		}
	}

	private void SetTargets()
	{
		if (!m_nview.IsOwner())
		{
			m_nview.ClaimOwnership();
		}
		m_nview.GetZDO().Set(ZDOVars.s_targets, m_targetItems.Count);
		for (int i = 0; i < m_targetItems.Count; i++)
		{
			m_nview.GetZDO().Set("target" + i, m_targetItems[i].m_itemData.m_shared.m_name);
		}
		ReadTargets();
	}

	private void ReadTargets()
	{
		if (!Object.op_Implicit((Object)(object)m_nview) || !m_nview.IsValid())
		{
			return;
		}
		m_targetItems.Clear();
		m_targetCharacters.Clear();
		m_targetsText = "";
		int @int = m_nview.GetZDO().GetInt(ZDOVars.s_targets);
		for (int i = 0; i < @int; i++)
		{
			string @string = m_nview.GetZDO().GetString("target" + i);
			foreach (TrophyTarget configTarget in m_configTargets)
			{
				if (!(configTarget.m_item.m_itemData.m_shared.m_name == @string))
				{
					continue;
				}
				m_targetItems.Add(configTarget.m_item);
				m_targetCharacters.AddRange(configTarget.m_targets);
				if (m_targetsText.Length > 0)
				{
					m_targetsText += ", ";
				}
				if (!string.IsNullOrEmpty(configTarget.m_nameOverride))
				{
					m_targetsText += configTarget.m_nameOverride;
					break;
				}
				for (int j = 0; j < configTarget.m_targets.Count; j++)
				{
					m_targetsText += configTarget.m_targets[j].m_name;
					if (j + 1 < configTarget.m_targets.Count)
					{
						m_targetsText += ", ";
					}
				}
				break;
			}
		}
	}

	public bool TryGetItems(Player player, out List<string> items)
	{
		items = new List<string>();
		if (!CanUseItems(player))
		{
			return true;
		}
		ItemDrop.ItemData itemData = FindAmmoItem(player.GetInventory(), onlyCurrentlyLoadableType: true);
		if (GetAmmo() > 0)
		{
			items.Add(itemData.m_shared.m_name);
		}
		else
		{
			items.AddRange(m_allowedAmmo.Select((AmmoType ammoType) => ammoType.m_ammo.m_itemData.m_shared.m_name));
		}
		return true;
	}

	public bool CanUseItems(Player player, bool sendErrorMessage = true)
	{
		if (GetAmmo() >= m_maxAmmo)
		{
			if (sendErrorMessage)
			{
				player.Message(MessageHud.MessageType.Center, "$msg_itsfull");
			}
			return false;
		}
		Inventory inventory = player.GetInventory();
		ItemDrop.ItemData itemData = FindAmmoItem(inventory, onlyCurrentlyLoadableType: true);
		if (GetAmmo() > 0 && itemData == null)
		{
			ItemDrop component = ZNetScene.instance.GetPrefab(GetAmmoType()).GetComponent<ItemDrop>();
			if (sendErrorMessage)
			{
				player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_turretotherammo") + Localization.instance.Localize(component.m_itemData.m_shared.m_name));
			}
			return false;
		}
		itemData = FindAmmoItem(inventory, onlyCurrentlyLoadableType: false);
		if (itemData != null)
		{
			return true;
		}
		if (sendErrorMessage)
		{
			player.Message(MessageHud.MessageType.Center, "$msg_noturretammo");
		}
		return false;
	}
}
