using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ShieldGenerator : MonoBehaviour, Hoverable, Interactable, IHasHoverMenu
{
	private static bool s_cleanShields = false;

	private static List<ShieldGenerator> m_instances = new List<ShieldGenerator>();

	private static int m_instanceChangeID = 0;

	private static ShieldDomeImageEffect m_shieldDomeEffect;

	public string m_name = "$piece_shieldgenerator";

	public string m_add = "$piece_shieldgenerator_add";

	public Switch m_addFuelSwitch;

	public GameObject m_enabledObject;

	public GameObject m_disabledObject;

	[Header("Fuel")]
	public List<ItemDrop> m_fuelItems = new List<ItemDrop>();

	public int m_maxFuel = 10;

	public int m_defaultFuel;

	public float m_fuelPerDamage = 0.01f;

	public EffectList m_fuelAddedEffects = new EffectList();

	[Header("Shield")]
	public GameObject m_shieldDome;

	public float m_minShieldRadius = 10f;

	public float m_maxShieldRadius = 30f;

	public float m_decreaseInertia = 0.98f;

	public float m_startStopSpeed = 0.5f;

	public bool m_offWhenNoFuel = true;

	[Header("Attack")]
	public bool m_enableAttack = true;

	public float m_attackChargeTime = 900f;

	public bool m_damagePlayers = true;

	public GameObject m_attackObject;

	public EffectList m_attackEffects = new EffectList();

	[Header("Effects")]
	public EffectList m_shieldHitEffects = new EffectList();

	public EffectList m_shieldStart = new EffectList();

	public EffectList m_shieldStop = new EffectList();

	public EffectList m_shieldLowLoop = new EffectList();

	public float m_shieldLowLoopFuelStart;

	public ParticleSystem[] m_energyParticles;

	public ParticleSystem m_energyParticlesFlare;

	public Light[] m_coloredLights;

	private static readonly int s_emissiveProperty = Shader.PropertyToID("_EmissionColor");

	private ZNetView m_nview;

	private StringBuilder m_sb = new StringBuilder();

	private bool m_firstCheck;

	private int m_projectileMask;

	private float m_radius;

	private float m_radiusTarget;

	private float m_radiusSent;

	private float m_lastFuel;

	private float m_lastFuelSent;

	private float m_lastHitTime;

	private float m_lastHitTimeSent;

	private float m_attackCharge;

	private bool m_isPlacementGhost;

	private GameObject[] m_lowLoopInstances;

	private Gradient m_particleFlareGradient;

	private MeshRenderer[] m_meshRenderers;

	private MaterialPropertyBlock m_propertyBlock;

	private void Start()
	{
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Expected O, but got Unknown
		if (Player.IsPlacementGhost(((Component)this).gameObject))
		{
			((Behaviour)this).enabled = false;
			m_isPlacementGhost = true;
			return;
		}
		m_instances.Add(this);
		m_instanceChangeID++;
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if ((Object)(object)m_nview == (Object)null)
		{
			m_nview = ((Component)this).GetComponentInParent<ZNetView>();
		}
		if (!((Object)(object)m_nview == (Object)null) && m_nview.GetZDO() != null)
		{
			if (Object.op_Implicit((Object)(object)m_addFuelSwitch))
			{
				Switch addFuelSwitch = m_addFuelSwitch;
				addFuelSwitch.m_onUse = (Switch.Callback)Delegate.Combine(addFuelSwitch.m_onUse, new Switch.Callback(OnAddFuel));
				m_addFuelSwitch.m_onHover = OnHoverAddFuel;
			}
			m_nview.Register("RPC_AddFuel", RPC_AddFuel);
			m_nview.Register<float>("RPC_SetFuel", RPC_SetFuel);
			m_nview.Register("RPC_Attack", RPC_Attack);
			m_nview.Register("RPC_HitNow", RPC_HitNow);
			m_projectileMask = LayerMask.GetMask(Array.Empty<string>());
			if (!Object.op_Implicit((Object)(object)m_shieldDomeEffect))
			{
				m_shieldDomeEffect = Object.FindFirstObjectByType<ShieldDomeImageEffect>();
			}
			if (!m_enableAttack && m_fuelItems.Count == 0)
			{
				((Component)m_addFuelSwitch).gameObject.SetActive(false);
			}
			m_particleFlareGradient = new Gradient();
			m_particleFlareGradient.colorKeys = (GradientColorKey[])(object)new GradientColorKey[1]
			{
				new GradientColorKey(Color.white, 0f)
			};
			m_particleFlareGradient.alphaKeys = (GradientAlphaKey[])(object)new GradientAlphaKey[1]
			{
				new GradientAlphaKey(0f, 0f)
			};
			m_propertyBlock = new MaterialPropertyBlock();
			m_meshRenderers = m_enabledObject.GetComponentsInChildren<MeshRenderer>();
			((MonoBehaviour)this).InvokeRepeating("UpdateShield", 0f, 0.22f);
		}
	}

	public string GetHoverText()
	{
		if (!m_enableAttack)
		{
			return "";
		}
		if (m_attackCharge <= 0f)
		{
			return Localization.instance.Localize(m_name + "\n$piece_shieldgenerator_waiting");
		}
		if (m_attackCharge >= 1f)
		{
			return Localization.instance.Localize(m_name + "\n$piece_shieldgenerator_ready \n[<color=yellow><b>$KEY_Use</b></color>] $piece_shieldgenerator_use");
		}
		return Localization.instance.Localize(m_name + "\n$piece_shieldgenerator_charging " + (Terminal.m_showTests ? m_attackCharge.ToString("0.00") : ""));
	}

	public string GetHoverName()
	{
		return "";
	}

	public bool Interact(Humanoid user, bool repeat, bool alt)
	{
		if (!m_enableAttack)
		{
			return false;
		}
		if (repeat)
		{
			return false;
		}
		if ((Object)(object)user == (Object)(object)Player.m_localPlayer && m_attackCharge >= 1f)
		{
			m_nview.InvokeRPC("RPC_Attack");
		}
		return false;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	private void RPC_Attack(long sender)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		m_attackCharge = 0f;
		if (!m_nview.IsOwner())
		{
			return;
		}
		SetFuel(0f);
		m_nview.GetZDO().Set(ZDOVars.s_startTime, 0L);
		UpdateAttackCharge();
		if (Object.op_Implicit((Object)(object)m_attackObject))
		{
			GameObject val = Object.Instantiate<GameObject>(m_attackObject, ((Component)this).transform.position, ((Component)this).transform.rotation);
			if (!m_damagePlayers)
			{
				val.GetComponentInChildren<Aoe>()?.Setup(Player.m_localPlayer, Vector3.zero, 1f, null, null, null);
			}
		}
		m_attackEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
		m_fuelAddedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
	}

	private void RPC_HitNow(long sender)
	{
		m_lastHitTime = Time.time;
	}

	private void UpdateAttackCharge()
	{
		m_attackCharge = GetAttackCharge();
	}

	private float GetAttackCharge()
	{
		long @long = m_nview.GetZDO().GetLong(ZDOVars.s_startTime, 0L);
		if (@long <= 0)
		{
			return 0f;
		}
		DateTime time = ZNet.instance.GetTime();
		DateTime dateTime = new DateTime(@long);
		return (float)((time - dateTime).TotalSeconds / (double)m_attackChargeTime);
	}

	private void OnDestroy()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!m_isPlacementGhost)
		{
			m_shieldDomeEffect.RemoveShield(this);
			m_instances.Remove(this);
			m_instanceChangeID++;
			Character.SetupContinuousEffect(((Component)this).transform, ((Component)this).transform.position, enabledEffect: false, m_shieldLowLoop, ref m_lowLoopInstances);
		}
	}

	private float GetFuel()
	{
		if (!m_nview.IsValid())
		{
			return 0f;
		}
		return m_nview.GetZDO().GetFloat(ZDOVars.s_fuel, m_defaultFuel);
	}

	private void SetFuel(float fuel)
	{
		m_nview.InvokeRPC("RPC_SetFuel", fuel);
	}

	private void RPC_SetFuel(long sender, float fuel)
	{
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_fuel, Mathf.Max(fuel, 0f));
		}
	}

	private bool OnAddFuel(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (GetFuel() > (float)(m_maxFuel - 1))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
			return false;
		}
		if (item != null)
		{
			bool flag = false;
			foreach (ItemDrop fuelItem in m_fuelItems)
			{
				if (item.m_shared.m_name == fuelItem.m_itemData.m_shared.m_name)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				user.Message(MessageHud.MessageType.Center, "$msg_wrongitem");
				return false;
			}
		}
		else
		{
			bool flag2 = false;
			foreach (ItemDrop fuelItem2 in m_fuelItems)
			{
				if (user.GetInventory().HaveItem(fuelItem2.m_itemData.m_shared.m_name))
				{
					item = fuelItem2.m_itemData;
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				user.Message(MessageHud.MessageType.Center, "$msg_donthaveany $piece_shieldgenerator_fuelname");
				return false;
			}
		}
		user.Message(MessageHud.MessageType.Center, "$msg_added " + item.m_shared.m_name);
		user.GetInventory().RemoveItem(item.m_shared.m_name, 1);
		m_nview.InvokeRPC("RPC_AddFuel");
		return true;
	}

	private void RPC_AddFuel(long sender)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			float fuel = GetFuel();
			SetFuel(fuel + 1f);
			m_fuelAddedEffects.Create(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
		}
	}

	private void Update()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_shieldDome))
		{
			float num = m_shieldDome.transform.localScale.x + (m_radius - m_shieldDome.transform.localScale.x) * m_decreaseInertia;
			m_shieldDome.transform.localScale = new Vector3(num, num, num);
		}
		if (m_radiusTarget != m_radius)
		{
			if (!m_firstCheck)
			{
				m_firstCheck = true;
				m_radius = m_radiusTarget;
			}
			float num2 = m_radiusTarget - m_radius;
			m_radius += Mathf.Min(m_startStopSpeed * Time.deltaTime, Mathf.Abs(num2)) * (float)((num2 > 0f) ? 1 : (-1));
		}
		if (m_lastFuel != m_lastFuelSent || m_radius != m_radiusSent || m_lastHitTime != m_lastHitTimeSent)
		{
			m_shieldDomeEffect.SetShieldData(this, m_shieldDome.transform.position, m_radius, m_lastFuel, m_lastHitTime);
			m_lastFuelSent = m_lastFuel;
			m_radiusSent = m_radius;
			m_lastHitTimeSent = m_lastHitTime;
		}
	}

	private void UpdateShield()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return;
		}
		float fuel = GetFuel();
		GameObject enabledObject = m_enabledObject;
		if (enabledObject != null)
		{
			enabledObject.SetActive(fuel > 0f);
		}
		GameObject disabledObject = m_disabledObject;
		if (disabledObject != null)
		{
			disabledObject.SetActive(fuel <= 0f);
		}
		_ = m_radius;
		m_lastFuel = fuel / (float)m_maxFuel;
		float radiusTarget = m_radiusTarget;
		m_radiusTarget = m_minShieldRadius + m_lastFuel * (m_maxShieldRadius - m_minShieldRadius);
		Color domeColor = ShieldDomeImageEffect.GetDomeColor(m_lastFuel);
		ParticleSystem[] energyParticles = m_energyParticles;
		foreach (ParticleSystem obj in energyParticles)
		{
			MainModule main = obj.main;
			((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(domeColor);
			EmissionModule emission = obj.emission;
			((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(m_lastFuel * 5f);
		}
		CustomDataModule customData = m_energyParticlesFlare.customData;
		((CustomDataModule)(ref customData)).SetColor((ParticleSystemCustomData)0, MinMaxGradient.op_Implicit(domeColor * Mathf.Pow(m_lastFuel, 0.5f)));
		Light[] coloredLights = m_coloredLights;
		for (int i = 0; i < coloredLights.Length; i++)
		{
			coloredLights[i].color = domeColor;
		}
		m_propertyBlock.SetColor(s_emissiveProperty, domeColor * 2f);
		MeshRenderer[] meshRenderers = m_meshRenderers;
		for (int i = 0; i < meshRenderers.Length; i++)
		{
			((Renderer)meshRenderers[i]).SetPropertyBlock(m_propertyBlock);
		}
		if (m_offWhenNoFuel)
		{
			if (fuel <= 0f)
			{
				m_radiusTarget = 0f;
				if (radiusTarget > 0f && m_nview.IsOwner())
				{
					m_shieldStop.Create(m_shieldDome.transform.position, m_shieldDome.transform.rotation);
				}
			}
			if (fuel > 0f && radiusTarget <= 0f && m_nview.IsOwner())
			{
				m_shieldStart.Create(m_shieldDome.transform.position, m_shieldDome.transform.rotation);
			}
		}
		if (m_shieldLowLoopFuelStart > 0f && m_nview.IsOwner())
		{
			Character.SetupContinuousEffect(((Component)this).transform, ((Component)this).transform.position, m_lastFuel > 0f && m_lastFuel < m_shieldLowLoopFuelStart, m_shieldLowLoop, ref m_lowLoopInstances);
		}
		if (m_nview.IsOwner() && fuel >= (float)m_maxFuel && m_nview.GetZDO().GetLong(ZDOVars.s_startTime, 0L) <= 0)
		{
			DateTime time = ZNet.instance.GetTime();
			m_nview.GetZDO().Set(ZDOVars.s_startTime, time.Ticks);
		}
		UpdateAttackCharge();
	}

	public static void CheckProjectile(Projectile projectile)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if (!projectile.HasBeenOutsideShields)
		{
			int num = m_instances.Count;
			foreach (ShieldGenerator instance in m_instances)
			{
				if (Vector3.Distance(instance.m_shieldDome.transform.position, ((Component)projectile).transform.position) < instance.m_radius || !CheckShield(instance))
				{
					num--;
				}
			}
			if (num == 0)
			{
				projectile.TriggerShieldsLeftFlag();
			}
		}
		else
		{
			foreach (ShieldGenerator instance2 in m_instances)
			{
				if (CheckShield(instance2) && Vector3.Distance(instance2.m_shieldDome.transform.position, projectile.m_startPoint) > instance2.m_radius && Vector3.Distance(instance2.m_shieldDome.transform.position, ((Component)projectile).transform.position) < instance2.m_radius)
				{
					instance2.OnProjectileHit(((Component)projectile).gameObject);
				}
			}
		}
		ShieldCleanup();
	}

	public float GetFuelRatio()
	{
		return Mathf.Clamp01(m_lastFuel);
	}

	public static void CheckObjectInsideShield(Cinder zinder)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		foreach (ShieldGenerator instance in m_instances)
		{
			if (CheckShield(instance) && Vector3.Distance(instance.m_shieldDome.transform.position, ((Component)zinder).transform.position) < instance.m_radius)
			{
				instance.OnProjectileHit(((Component)zinder).gameObject);
			}
		}
		ShieldCleanup();
	}

	public static bool IsInsideShield(Vector3 point)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		foreach (ShieldGenerator instance in m_instances)
		{
			if (CheckShield(instance) && Object.op_Implicit((Object)(object)instance) && Object.op_Implicit((Object)(object)instance.m_shieldDome) && Vector3.Distance(instance.m_shieldDome.transform.position, point) < instance.m_radius)
			{
				return true;
			}
		}
		ShieldCleanup();
		return false;
	}

	public static bool IsInsideMaxShield(Vector3 point)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		foreach (ShieldGenerator instance in m_instances)
		{
			if (CheckShield(instance) && Object.op_Implicit((Object)(object)instance) && Object.op_Implicit((Object)(object)instance.m_shieldDome) && Vector3.Distance(instance.m_shieldDome.transform.position, point) < instance.m_maxShieldRadius)
			{
				return true;
			}
		}
		ShieldCleanup();
		return false;
	}

	public static bool IsInsideShieldCached(Vector3 point, ref int changeID)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Abs(changeID) <= m_instanceChangeID)
		{
			if (!IsInsideMaxShield(point))
			{
				changeID = -m_instanceChangeID;
				return false;
			}
			changeID = m_instanceChangeID;
			if (IsInsideShield(point))
			{
				return true;
			}
		}
		if (changeID > 0 && IsInsideShield(point))
		{
			return true;
		}
		return false;
	}

	private static bool CheckShield(ShieldGenerator shield)
	{
		if (!Object.op_Implicit((Object)(object)shield) || !Object.op_Implicit((Object)(object)shield.m_shieldDome))
		{
			s_cleanShields = true;
			return false;
		}
		return true;
	}

	private static void ShieldCleanup()
	{
		if (s_cleanShields)
		{
			int num = m_instances.RemoveAll((ShieldGenerator x) => (Object)(object)x == (Object)null || (Object)(object)x.m_shieldDome == (Object)null);
			if (num > 0)
			{
				ZLog.LogWarning((object)$"Removed {num} invalid shield instances. Some shields may be broken?");
			}
			s_cleanShields = false;
		}
	}

	public void OnProjectileHit(GameObject obj)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = obj.transform.position;
		Projectile component = obj.GetComponent<Projectile>();
		component?.OnHit(null, position, water: false, -obj.transform.forward);
		ZNetScene.instance.Destroy(obj.gameObject);
		if (m_fuelPerDamage > 0f)
		{
			float num = m_fuelPerDamage * (Object.op_Implicit((Object)(object)component) ? component.m_damage.GetTotalDamage() : 10f);
			SetFuel(GetFuel() - num);
		}
		m_nview.InvokeRPC(ZNetView.Everybody, "RPC_HitNow");
		m_shieldHitEffects.Create(position, Quaternion.LookRotation(VectorExtensions.DirTo(((Component)this).transform.position, position)));
		UpdateShield();
	}

	private string OnHoverAddFuel()
	{
		float fuel = GetFuel();
		return Localization.instance.Localize($"{m_name} ({Mathf.Ceil(fuel)}/{m_maxFuel})\n[<color=yellow><b>$KEY_Use</b></color>] {m_add}");
	}

	public static bool HasShields()
	{
		if (m_instances == null)
		{
			return false;
		}
		foreach (ShieldGenerator instance in m_instances)
		{
			if (instance.m_lastFuel > 0f)
			{
				return true;
			}
		}
		return false;
	}

	private static float SDFSmoothMin(float a, float b, float k)
	{
		k *= 6f;
		float num = Mathf.Max(k - Mathf.Abs(a - b), 0f) / k;
		return Mathf.Min(a, b) - num * num * num * k * (1f / 6f);
	}

	public static Vector3 DirectionToShieldWall(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.001f;
		return -Vector3.Normalize(new Vector3(DistanceToShieldWall(pos + new Vector3(0f, num, 0f)), DistanceToShieldWall(pos + new Vector3(0f, 0f, num)), DistanceToShieldWall(pos + new Vector3(num, 0f, 0f))));
	}

	public static float DistanceToShieldWall(Vector3 pos)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		float num = float.PositiveInfinity;
		foreach (ShieldGenerator instance in m_instances)
		{
			if (instance.m_lastFuel != 0f)
			{
				float b = Vector3.Distance(((Component)instance).transform.position, pos) - instance.m_radius;
				num = SDFSmoothMin(num, b, ShieldDomeImageEffect.Smoothing);
			}
		}
		return num;
	}

	public static Vector3 GetClosestShieldPoint(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float num = DistanceToShieldWall(pos);
		Vector3 val = DirectionToShieldWall(pos);
		return pos + val * num;
	}

	public static ShieldGenerator GetClosestShieldGenerator(Vector3 pos, bool ignoreRadius = false)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		ShieldGenerator result = null;
		float num = float.PositiveInfinity;
		foreach (ShieldGenerator instance in m_instances)
		{
			float num2 = (ignoreRadius ? 0f : instance.m_radius);
			float num3 = Mathf.Abs(Vector3.Distance(((Component)instance).transform.position, pos) - num2);
			if (num3 < num)
			{
				num = num3;
				result = instance;
			}
		}
		return result;
	}

	public bool TryGetItems(Player player, out List<string> items)
	{
		items = new List<string>();
		if (!CanUseItems(player))
		{
			return true;
		}
		items = m_fuelItems.Select((ItemDrop item) => item.m_itemData.m_shared.m_name).ToList();
		return true;
	}

	public bool CanUseItems(Player player, bool sendErrorMessage = true)
	{
		if (GetFuel() > (float)(m_maxFuel - 1))
		{
			player.Message(MessageHud.MessageType.Center, "$msg_itsfull");
			return false;
		}
		if (m_fuelItems.Any((ItemDrop obj) => player.GetInventory().HaveItem(obj.m_itemData.m_shared.m_name)))
		{
			return true;
		}
		player.Message(MessageHud.MessageType.Center, "$msg_donthaveany $piece_shieldgenerator_fuelname");
		return false;
	}
}
