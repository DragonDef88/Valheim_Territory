using System;
using System.Collections.Generic;
using System.Linq;
using Dynamics;
using UnityEngine;
using UnityEngine.Serialization;

public class Catapult : MonoBehaviour
{
	[Header("Legs")]
	public List<Switch> m_legs = new List<Switch>();

	[FormerlySerializedAs("m_legAnimationDown")]
	public AnimationCurve m_legAnimationCurve;

	private AnimationCurve m_legAnimationCurveUp;

	public float m_legAnimationDegrees = 90f;

	public float m_legAnimationUpMultiplier = 1f;

	public float m_legAnimationTime = 5f;

	public float m_legDownMass = 500f;

	[Header("Shooting")]
	public GameObject m_forceVector;

	public GameObject m_arm;

	public Switch m_loadPoint;

	public Transform m_shootPoint;

	public AnimationCurve m_armAnimation;

	public float m_armAnimationDegrees = 180f;

	public float m_armAnimationTime = 2f;

	public float m_releaseAnimationTime;

	public float m_shootAfterLoadDelay = 1f;

	public Projectile m_projectile;

	public ItemDrop m_defaultAmmo;

	public int m_maxLoadStack = 1;

	public float m_hitNoise = 1f;

	public float m_randomRotationMin = 2f;

	public float m_randomRotationMax = 10f;

	public float m_shootVelocityVariation = 0.1f;

	[Header("Dynamics")]
	public DynamicsParameters m_armDynamicsSettings;

	private FloatDynamics m_armDynamics;

	public DynamicsParameters m_legDynamicsSettings;

	private FloatDynamics m_legDynamics;

	[Header("Ammo")]
	[Tooltip("If checked, will include all except listed types. If unchecked, will exclude all except listed types.")]
	public bool m_defaultIncludeAndListExclude = true;

	public bool m_onlyUseIncludedProjectiles = true;

	public bool m_onlyIncludedItemsDealDamage = true;

	public List<ItemDrop.ItemData.ItemType> m_includeExcludeTypesList = new List<ItemDrop.ItemData.ItemType>();

	public List<ItemDrop> m_includeItemsOverride = new List<ItemDrop>();

	public List<ItemDrop> m_excludeItemsOverride = new List<ItemDrop>();

	[Header("Character Launching")]
	public SphereCollider m_launchCollectArea;

	public float m_preLaunchForce = 5f;

	public float m_launchForce = 100f;

	[Header("Effects")]
	public EffectList m_legDownEffect = new EffectList();

	public EffectList m_legDownDoneEffect = new EffectList();

	public EffectList m_legUpEffect = new EffectList();

	public EffectList m_legUpDoneEffect = new EffectList();

	public EffectList m_shootStartEffect = new EffectList();

	public EffectList m_shootReleaseEffect = new EffectList();

	public EffectList m_armReturnEffect = new EffectList();

	public EffectList m_loadItemEffect = new EffectList();

	private static int m_characterMask;

	private ZNetView m_nview;

	private ZNetView m_wagonNview;

	private Vagon m_wagon;

	private Rigidbody m_rigidBody;

	private float m_baseMass;

	private ItemDrop.ItemData m_loadedItem;

	private int m_loadStack;

	private GameObject m_visualItem;

	private GameObject m_shotItem;

	private bool m_lockedLegs;

	private Vector3[] m_legRotations;

	private Quaternion[] m_legRotationQuat;

	private float m_legAnimTimer;

	private bool m_movingLegs;

	private Vector3 m_armRotation;

	private float m_armAnimTime;

	private Projectile m_lastProjectile;

	private ItemDrop.ItemData m_lastAmmo;

	private Collider[] m_colliders = (Collider[])(object)new Collider[10];

	private List<Character> m_launchCharacters = new List<Character>();

	private void Start()
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected O, but got Unknown
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected O, but got Unknown
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Expected O, but got Unknown
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_wagon = ((Component)this).GetComponent<Vagon>();
		m_rigidBody = ((Component)this).GetComponent<Rigidbody>();
		m_baseMass = m_rigidBody.mass;
		m_legRotations = (Vector3[])(object)new Vector3[m_legs.Count];
		m_legRotationQuat = (Quaternion[])(object)new Quaternion[m_legs.Count];
		for (int i = 0; i < m_legs.Count; i++)
		{
			Switch @switch = m_legs[i];
			@switch.m_onUse = (Switch.Callback)Delegate.Combine(@switch.m_onUse, new Switch.Callback(OnLegUse));
			Switch switch2 = m_legs[i];
			switch2.m_onHover = (Switch.TooltipCallback)Delegate.Combine(switch2.m_onHover, new Switch.TooltipCallback(OnLegHover));
			m_legRotations[i] = ((Component)m_legs[i]).transform.localEulerAngles;
			m_legRotationQuat[i] = ((Component)m_legs[i]).transform.localRotation;
		}
		Switch loadPoint = m_loadPoint;
		loadPoint.m_onUse = (Switch.Callback)Delegate.Combine(loadPoint.m_onUse, new Switch.Callback(OnLoadPointUse));
		Switch loadPoint2 = m_loadPoint;
		loadPoint2.m_onHover = (Switch.TooltipCallback)Delegate.Combine(loadPoint2.m_onHover, new Switch.TooltipCallback(OnHoverLoadPoint));
		Quaternion localRotation = m_arm.transform.localRotation;
		m_armRotation = ((Quaternion)(ref localRotation)).eulerAngles;
		m_armDynamics = new FloatDynamics(m_armDynamicsSettings, 0f);
		m_legDynamics = new FloatDynamics(m_legDynamicsSettings, 1f);
		m_legAnimationCurveUp = new AnimationCurve();
		for (int j = 0; j < m_legAnimationCurve.keys.Length; j++)
		{
			Keyframe val = m_legAnimationCurve.keys[j];
			((Keyframe)(ref val)).value = 1f - ((Keyframe)(ref val)).value;
			m_legAnimationCurveUp.AddKey(val);
		}
		if (Object.op_Implicit((Object)(object)m_nview))
		{
			ZDO zDO = m_nview.GetZDO();
			if (zDO != null && zDO.GetBool(ZDOVars.s_locked))
			{
				m_lockedLegs = zDO.GetBool(ZDOVars.s_locked);
			}
		}
		m_nview.Register("RPC_Shoot", RPC_Shoot);
		m_nview.Register<bool>("RPC_OnLegUse", RPC_OnLegUse);
		m_nview.Register<string>("RPC_SetLoadedVisual", RPC_SetLoadedVisual);
		m_legAnimTimer = 1f;
		m_movingLegs = true;
		UpdateLegAnimation(Time.fixedDeltaTime);
		if (m_characterMask == 0)
		{
			m_characterMask = LayerMask.GetMask(new string[1] { "character" });
		}
	}

	private void FixedUpdate()
	{
		UpdateArmAnimation(Time.fixedDeltaTime);
		UpdateLegAnimation(Time.fixedDeltaTime);
	}

	private void UpdateArmAnimation(float dt)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		float num = m_armAnimTime / m_armAnimationTime;
		float num2 = m_armDynamics.Update(dt, m_armAnimation.Evaluate(num), float.NegativeInfinity, false);
		m_arm.transform.localEulerAngles = new Vector3(m_armRotation.x + num2 * m_armAnimationDegrees, m_armRotation.y, m_armRotation.z);
		if (m_armAnimTime <= 0f)
		{
			return;
		}
		m_armAnimTime += dt;
		if (m_armAnimTime > m_armAnimationTime)
		{
			m_armAnimTime = 0f;
			m_arm.transform.localEulerAngles = m_armRotation;
			m_armReturnEffect.Create(((Component)m_loadPoint).transform.position, ((Component)m_loadPoint).transform.rotation);
		}
		else if (num > m_releaseAnimationTime && (m_loadedItem != null || m_launchCharacters.Count > 0))
		{
			Release();
		}
		else
		{
			if (!(m_preLaunchForce > 0f))
			{
				return;
			}
			Vector3 val = m_forceVector.transform.position - ((Component)this).transform.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			foreach (Character launchCharacter in m_launchCharacters)
			{
				launchCharacter.ForceJump(normalized * m_preLaunchForce);
			}
		}
	}

	private void UpdateLegAnimation(float dt)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_nview) || !m_nview.IsValid())
		{
			return;
		}
		if (m_movingLegs)
		{
			m_legAnimTimer += Time.deltaTime;
			if (m_legAnimTimer >= m_legAnimationTime)
			{
				m_movingLegs = false;
				for (int i = 0; i < m_legs.Count; i++)
				{
					Vector3 position = ((Component)((Component)m_legs[i]).transform.GetChild(0)).transform.position;
					if (!m_lockedLegs)
					{
						m_legUpDoneEffect.Create(((Component)m_legs[i]).transform.position, ((Component)m_legs[i]).transform.rotation);
						continue;
					}
					m_legDownDoneEffect.Create(position, Quaternion.identity);
					m_rigidBody.mass = m_legDownMass;
				}
				return;
			}
		}
		float num = m_legAnimTimer / m_legAnimationTime;
		AnimationCurve val = (m_lockedLegs ? m_legAnimationCurve : m_legAnimationCurveUp);
		float num2 = m_legDynamics.Update(dt, val.Evaluate(num), float.NegativeInfinity, false);
		for (int j = 0; j < m_legs.Count; j++)
		{
			Vector3 localEulerAngles = m_legRotations[j];
			localEulerAngles.z += num2 * m_legAnimationDegrees;
			((Component)m_legs[j]).transform.localEulerAngles = localEulerAngles;
		}
	}

	private bool OnLegUse(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (m_movingLegs)
		{
			return false;
		}
		m_nview.InvokeRPC(ZNetView.Everybody, "RPC_OnLegUse", !m_lockedLegs);
		if (!m_nview.IsOwner())
		{
			m_nview.InvokeRPC("RPC_RequestOwn");
		}
		return true;
	}

	private void RPC_OnLegUse(long sender, bool value)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		m_lockedLegs = value;
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_locked, m_lockedLegs);
		}
		m_legAnimTimer = 0f;
		m_movingLegs = true;
		if (m_lockedLegs)
		{
			m_legDownEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			return;
		}
		m_legUpEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
		m_rigidBody.mass = m_baseMass;
	}

	private string OnLegHover()
	{
		if (m_movingLegs)
		{
			return "";
		}
		return Localization.instance.Localize(m_lockedLegs ? "[<color=yellow><b>$KEY_Use</b></color>] $piece_catapult_legsup" : "[<color=yellow><b>$KEY_Use</b></color>] $piece_catapult_legsdown");
	}

	private bool OnLoadPointUse(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		if (m_loadedItem != null || item == null || m_armAnimTime != 0f)
		{
			user.UseIemBlockkMessage();
			return false;
		}
		if (!CanItemBeLoaded(item))
		{
			user.Message(MessageHud.MessageType.Center, "$piece_catapult_wontfit");
			user.UseIemBlockkMessage();
			return false;
		}
		m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetLoadedVisual", ((Object)item.m_dropPrefab).name);
		m_loadedItem = item;
		m_loadStack = Mathf.Min(item.m_stack, m_maxLoadStack);
		((MonoBehaviour)this).Invoke("Shoot", m_shootAfterLoadDelay);
		if (item.m_equipped)
		{
			user.UnequipItem(item);
		}
		user.GetInventory().RemoveItem(item, m_loadStack);
		if (!m_nview.IsOwner())
		{
			m_nview.InvokeRPC("RPC_RequestOwn");
		}
		m_loadItemEffect.Create(((Component)m_loadPoint).transform.position, ((Component)m_loadPoint).transform.rotation);
		return true;
	}

	private bool CanItemBeLoaded(ItemDrop.ItemData item)
	{
		if (m_includeItemsOverride.Any((ItemDrop x) => x.m_itemData.m_shared.m_name == item.m_shared.m_name))
		{
			return true;
		}
		if (m_onlyUseIncludedProjectiles && (Object)(object)ItemStand.GetAttachPrefab(item.m_dropPrefab) == (Object)null)
		{
			return false;
		}
		if (m_defaultIncludeAndListExclude && m_includeExcludeTypesList.Contains(item.m_shared.m_itemType))
		{
			return false;
		}
		if (!m_defaultIncludeAndListExclude && !m_includeExcludeTypesList.Contains(item.m_shared.m_itemType))
		{
			return false;
		}
		if (m_excludeItemsOverride.Any((ItemDrop x) => x.m_itemData.m_shared.m_name == item.m_shared.m_name))
		{
			return false;
		}
		return true;
	}

	private void Shoot()
	{
		m_nview.InvokeRPC(ZNetView.Everybody, "RPC_Shoot");
	}

	private void RPC_Shoot(long sender)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		m_shootStartEffect.Create(((Component)m_loadPoint).transform.position, ((Component)m_loadPoint).transform.rotation);
		m_armAnimTime = 1E-06f;
		CollectLaunchCharacters();
	}

	private void RPC_SetLoadedVisual(long sender, string name)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		if (component != null)
		{
			m_loadedItem = component.m_itemData;
		}
		GameObject attachPrefab = ItemStand.GetAttachPrefab(itemPrefab);
		if ((Object)(object)attachPrefab == (Object)null)
		{
			ZLog.LogError((object)("Valid catapult ammo '" + name + "' is missing attach prefab, aborting."));
			return;
		}
		attachPrefab = ItemStand.GetAttachGameObject(attachPrefab);
		m_visualItem = Object.Instantiate<GameObject>(attachPrefab, ((Component)m_loadPoint).transform);
		m_visualItem.transform.localPosition = Vector3.zero;
	}

	private void Release()
	{
		ShootProjectile();
		LaunchCharacters();
		m_loadedItem = null;
	}

	private void ShootProjectile()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = m_forceVector.transform.position - ((Component)this).transform.position;
		Vector3 val2 = ((Vector3)(ref val)).normalized;
		m_shootReleaseEffect.Create(((Component)m_loadPoint).transform.position, Quaternion.LookRotation(val2));
		Projectile projectile = m_projectile;
		bool flag = m_includeItemsOverride.Any((ItemDrop x) => x.m_itemData.m_shared.m_name == m_loadedItem.m_shared.m_name);
		if ((!m_onlyUseIncludedProjectiles || (m_onlyUseIncludedProjectiles && flag)) && (Object)(object)m_loadedItem.m_shared.m_attack.m_attackProjectile != (Object)null)
		{
			Projectile component = m_loadedItem.m_shared.m_attack.m_attackProjectile.GetComponent<Projectile>();
			if (component != null)
			{
				projectile = component;
			}
		}
		m_lastAmmo = m_defaultAmmo.m_itemData;
		if (m_nview.IsOwner())
		{
			for (int i = 0; i < m_loadStack; i++)
			{
				m_lastProjectile = Object.Instantiate<Projectile>(projectile, ((Component)m_shootPoint).transform.position, ((Component)m_shootPoint).transform.rotation);
				HitData hitData = new HitData();
				if ((Object)(object)projectile == (Object)(object)m_projectile)
				{
					if (Object.op_Implicit((Object)(object)m_lastProjectile.m_visual))
					{
						m_lastProjectile.m_visual.gameObject.SetActive(false);
					}
					((Component)m_lastProjectile).GetComponent<ZNetView>().GetZDO().Set(ZDOVars.s_visual, ((Object)m_loadedItem.m_dropPrefab).name);
					Collider componentInChildren = m_lastProjectile.m_visual.GetComponentInChildren<Collider>();
					if (componentInChildren != null)
					{
						componentInChildren.enabled = false;
					}
					if (!m_onlyIncludedItemsDealDamage || (m_onlyIncludedItemsDealDamage && flag))
					{
						hitData.m_toolTier = (short)m_lastAmmo.m_shared.m_toolTier;
						hitData.m_pushForce = m_lastAmmo.m_shared.m_attackForce;
						hitData.m_backstabBonus = m_lastAmmo.m_shared.m_backstabBonus;
						hitData.m_staggerMultiplier = m_lastAmmo.m_shared.m_attack.m_staggerMultiplier;
						hitData.m_damage.Add(m_lastAmmo.GetDamage());
						hitData.m_statusEffectHash = (Object.op_Implicit((Object)(object)m_lastAmmo.m_shared.m_attackStatusEffect) ? m_lastAmmo.m_shared.m_attackStatusEffect.NameHash() : 0);
						hitData.m_blockable = m_lastAmmo.m_shared.m_blockable;
						hitData.m_dodgeable = m_lastAmmo.m_shared.m_dodgeable;
						hitData.m_skill = m_lastAmmo.m_shared.m_skillType;
						if ((Object)(object)m_lastAmmo.m_shared.m_attackStatusEffect != (Object)null)
						{
							hitData.m_statusEffectHash = m_lastAmmo.m_shared.m_attackStatusEffect.NameHash();
						}
					}
				}
				else if (!m_onlyIncludedItemsDealDamage || (m_onlyIncludedItemsDealDamage && flag))
				{
					hitData.m_toolTier = (short)m_loadedItem.m_shared.m_toolTier;
					hitData.m_pushForce = m_loadedItem.m_shared.m_attackForce;
					hitData.m_backstabBonus = m_loadedItem.m_shared.m_backstabBonus;
					hitData.m_damage.Add(m_loadedItem.GetDamage());
					hitData.m_statusEffectHash = (Object.op_Implicit((Object)(object)m_loadedItem.m_shared.m_attackStatusEffect) ? m_loadedItem.m_shared.m_attackStatusEffect.NameHash() : 0);
					hitData.m_skillLevel = 1f;
					hitData.m_itemLevel = (short)m_loadedItem.m_quality;
					hitData.m_itemWorldLevel = (byte)m_loadedItem.m_worldLevel;
					hitData.m_blockable = m_loadedItem.m_shared.m_blockable;
					hitData.m_dodgeable = m_loadedItem.m_shared.m_dodgeable;
					hitData.m_skill = m_loadedItem.m_shared.m_skillType;
					hitData.m_hitType = HitData.HitType.Catapult;
				}
				if (m_lastAmmo.m_shared.m_attack.m_projectileAccuracyMin > 0f || m_lastAmmo.m_shared.m_attack.m_projectileAccuracy > 0f)
				{
					float num = Random.Range(m_lastAmmo.m_shared.m_attack.m_projectileAccuracyMin, m_lastAmmo.m_shared.m_attack.m_projectileAccuracy);
					Vector3 val3 = Vector3.Cross(val2, Vector3.up);
					Quaternion val4 = Quaternion.AngleAxis(Random.Range(0f - num, num), Vector3.up);
					val2 = Quaternion.AngleAxis(Random.Range(0f - num, num), val3) * val2;
					val2 = val4 * val2;
				}
				Vector3 velocity = val * m_lastAmmo.m_shared.m_attack.m_projectileVel * Random.Range(1f, 1f + m_shootVelocityVariation);
				projectile.m_respawnItemOnHit = !flag;
				m_lastProjectile.Setup(null, velocity, m_hitNoise, hitData, m_loadedItem, m_lastAmmo);
				m_lastProjectile.m_rotateVisual = Random.Range(m_randomRotationMin, m_randomRotationMax);
				m_lastProjectile.m_rotateVisualY = Random.Range(m_randomRotationMin, m_randomRotationMax);
				m_lastProjectile.m_rotateVisualZ = Random.Range(m_randomRotationMin, m_randomRotationMax);
			}
		}
		Object.Destroy((Object)(object)m_visualItem);
		m_visualItem = null;
	}

	private void CollectLaunchCharacters()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		m_launchCharacters.Clear();
		int num = Physics.OverlapSphereNonAlloc(((Component)m_launchCollectArea).transform.position, m_launchCollectArea.radius, m_colliders, m_characterMask);
		for (int i = 0; i < num; i++)
		{
			Character componentInParent = ((Component)m_colliders[i]).GetComponentInParent<Character>();
			if (componentInParent != null)
			{
				ZNetView component = ((Component)componentInParent).GetComponent<ZNetView>();
				if (component != null && component.IsOwner())
				{
					m_launchCharacters.Add(componentInParent);
					componentInParent.SetTempParent(m_arm.transform);
				}
			}
		}
	}

	private void LaunchCharacters()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		foreach (Character launchCharacter in m_launchCharacters)
		{
			launchCharacter.ReleaseTempParent();
			Vector3 val = m_forceVector.transform.position - ((Component)this).transform.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			launchCharacter.ForceJump(normalized * m_launchForce);
			launchCharacter.StandUpOnNextGround();
		}
		m_launchCharacters.Clear();
	}

	private string OnHoverLoadPoint()
	{
		return Localization.instance.Localize((m_loadedItem == null) ? "[<color=yellow><b>1-8</b></color>] $piece_catapult_placeitem" : "");
	}
}
