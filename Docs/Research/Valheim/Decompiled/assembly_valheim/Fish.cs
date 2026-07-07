using System;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour, IWaterInteractable, Hoverable, Interactable, IMonoUpdater
{
	[Serializable]
	public class BaitSetting
	{
		public ItemDrop m_bait;

		[Range(0f, 1f)]
		public float m_chance;
	}

	public string m_name = "Fish";

	public float m_swimRange = 20f;

	public float m_minDepth = 1f;

	public float m_maxDepth = 4f;

	public float m_speed = 10f;

	public float m_acceleration = 5f;

	public float m_turnRate = 10f;

	public float m_wpDurationMin = 4f;

	public float m_wpDurationMax = 4f;

	public float m_avoidSpeedScale = 2f;

	public float m_avoidRange = 5f;

	public float m_height = 0.2f;

	public float m_hookForce = 4f;

	public float m_staminaUse = 1f;

	public float m_escapeStaminaUse = 2f;

	public float m_escapeMin = 0.5f;

	public float m_escapeMax = 3f;

	public float m_escapeWaitMin = 0.75f;

	public float m_escapeWaitMax = 4f;

	public float m_escapeMaxPerLevel = 1.5f;

	public float m_baseHookChance = 0.5f;

	public GameObject m_pickupItem;

	public int m_pickupItemStackSize = 1;

	[Tooltip("Fish aren't smart enough to change their mind too often (and makes reactions/collisions feel less artificial)")]
	public float m_blockChangeDurationMin = 0.1f;

	public float m_blockChangeDurationMax = 0.6f;

	public float m_collisionFleeTimeout = 1.5f;

	private Vector3 m_waypoint;

	private FishingFloat m_waypointFF;

	private FishingFloat m_failedBait;

	private bool m_haveWaypoint;

	[Header("Baits")]
	public List<BaitSetting> m_baits = new List<BaitSetting>();

	public DropTable m_extraDrops = new DropTable();

	[Header("Jumping")]
	public float m_jumpSpeed = 3f;

	public float m_jumpHeight = 14f;

	public float m_jumpForwardStrength = 16f;

	public float m_jumpHeightLand = 3f;

	public float m_jumpChance = 0.25f;

	public float m_jumpOnLandChance = 0.5f;

	public float m_jumpOnLandDecay = 0.5f;

	public float m_maxJumpDepthOffset = 0.5f;

	public float m_jumpFrequencySeconds = 0.1f;

	public float m_jumpOnLandRotation = 2f;

	public float m_waveJumpMultiplier = 0.05f;

	public float m_jumpMaxLevel = 2f;

	public EffectList m_jumpEffects = new EffectList();

	private float m_JumpHeightStrength;

	private bool m_jumpedFromLand;

	private int m_isColliding;

	private bool m_isJumping;

	private float m_lastJumpCheck;

	private float m_swimTimer;

	private float m_lastNibbleTime;

	private float m_escapeTime;

	private float m_nextEscape;

	private Vector3 m_spawnPoint;

	private bool m_fast;

	private float m_lastCollision;

	private float m_blockChange;

	[Header("Waves")]
	public float m_waveFollowDirection = 7f;

	private float m_lastWave;

	private float m_inWater = -10000f;

	private WaterVolume m_waterVolume;

	private LiquidSurface m_liquidSurface;

	private FishingFloat m_fishingFloat;

	private float m_pickupTime;

	private long m_lastOwner = -1L;

	private Vector3 m_originalLocalRef;

	private bool m_lodVisible = true;

	private ZNetView m_nview;

	private Rigidbody m_body;

	private ItemDrop m_itemDrop;

	private LODGroup m_lodGroup;

	private static Vector4 s_wind;

	private static float s_wrappedTimeSeconds;

	private static float s_deltaTime;

	private static float s_time;

	private static float s_dawnDusk;

	private static int s_updatedFrame;

	private float m_waterDepth;

	private float m_waterWave;

	private int m_waterWaveCount;

	private readonly int[] m_liquids = new int[2];

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_itemDrop = ((Component)this).GetComponent<ItemDrop>();
		m_lodGroup = ((Component)this).GetComponent<LODGroup>();
		if (Object.op_Implicit((Object)(object)m_itemDrop))
		{
			if (m_itemDrop.m_itemData.m_quality > 1)
			{
				m_itemDrop.SetQuality(m_itemDrop.m_itemData.m_quality);
			}
			ItemDrop itemDrop = m_itemDrop;
			itemDrop.m_onDrop = (Action<ItemDrop>)Delegate.Combine(itemDrop.m_onDrop, new Action<ItemDrop>(onDrop));
			if ((Object)(object)m_pickupItem == (Object)null)
			{
				m_pickupItem = ((Component)this).gameObject;
			}
		}
		m_waterWaveCount = Random.Range(0, 1);
		if (Object.op_Implicit((Object)(object)m_lodGroup))
		{
			m_originalLocalRef = m_lodGroup.localReferencePoint;
		}
	}

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		m_spawnPoint = m_nview.GetZDO().GetVec3(ZDOVars.s_spawnPoint, ((Component)this).transform.position);
		if (m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_spawnPoint, m_spawnPoint);
			RandomizeWaypoint(canHook: true, Time.time);
		}
		if (m_nview.IsValid())
		{
			m_nview.Register("RequestPickup", RPC_RequestPickup);
			m_nview.Register("Pickup", RPC_Pickup);
		}
		if ((Object)(object)m_waterVolume != (Object)null)
		{
			m_waterDepth = m_waterVolume.Depth(((Component)this).transform.position);
			m_waterWave = m_waterVolume.CalcWave(((Component)this).transform.position, m_waterDepth, s_wrappedTimeSeconds, 1f);
		}
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public string GetHoverText()
	{
		string text = m_name;
		if (IsOutOfWater())
		{
			if (Object.op_Implicit((Object)(object)m_itemDrop))
			{
				return m_itemDrop.GetHoverText();
			}
			text += "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup";
		}
		return Localization.instance.Localize(text);
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		if (repeat)
		{
			return false;
		}
		if (!IsOutOfWater())
		{
			return false;
		}
		if (Pickup(character))
		{
			return true;
		}
		return false;
	}

	public bool Pickup(Humanoid character)
	{
		if (Object.op_Implicit((Object)(object)m_itemDrop))
		{
			m_itemDrop.Pickup(character);
			return true;
		}
		if ((Object)(object)m_pickupItem == (Object)null)
		{
			return false;
		}
		if (!character.GetInventory().CanAddItem(m_pickupItem, m_pickupItemStackSize))
		{
			character.Message(MessageHud.MessageType.Center, "$msg_noroom");
			return false;
		}
		m_nview.InvokeRPC("RequestPickup");
		return true;
	}

	private void RPC_RequestPickup(long uid)
	{
		if (Time.time - m_pickupTime > 2f)
		{
			m_pickupTime = Time.time;
			m_nview.InvokeRPC(uid, "Pickup");
		}
	}

	private void RPC_Pickup(long uid)
	{
		if (Object.op_Implicit((Object)(object)Player.m_localPlayer) && Player.m_localPlayer.PickupPrefab(m_pickupItem, m_pickupItemStackSize) != null)
		{
			m_nview.ClaimOwnership();
			m_nview.Destroy();
		}
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	public void SetLiquidLevel(float level, LiquidType type, Component liquidObj)
	{
		if (type == LiquidType.Water)
		{
			m_inWater = level;
		}
		m_liquidSurface = null;
		m_waterVolume = null;
		if (liquidObj is WaterVolume waterVolume)
		{
			m_waterVolume = waterVolume;
		}
		else if (liquidObj is LiquidSurface liquidSurface)
		{
			m_liquidSurface = liquidSurface;
		}
	}

	public Transform GetTransform()
	{
		if ((Object)(object)this == (Object)null)
		{
			return null;
		}
		return ((Component)this).transform;
	}

	public bool IsOutOfWater()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return m_inWater < ((Component)this).transform.position.y - m_height;
	}

	public void CustomFixedUpdate(float fixedDeltaTime)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0593: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_057e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0583: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0639: Unknown result type (might be due to invalid IL or missing references)
		//IL_063e: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_0649: Unknown result type (might be due to invalid IL or missing references)
		//IL_064c: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ec: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return;
		}
		if (Time.frameCount != s_updatedFrame)
		{
			EnvMan.instance.GetWindData(out var wind, out var wind2, out var _);
			s_wind = wind + wind2;
			s_wrappedTimeSeconds = ZNet.instance.GetWrappedDayTimeSeconds();
			s_deltaTime = fixedDeltaTime;
			s_time = Time.time;
			s_dawnDusk = 1f - Utils.Abs(Utils.Abs(EnvMan.instance.GetDayFraction() * 2f - 1f) - 0.5f) * 2f;
			s_updatedFrame = Time.frameCount;
		}
		if (m_isColliding > 0)
		{
			if (s_time > m_lastCollision + 0.5f)
			{
				onCollision();
			}
			if (m_isJumping)
			{
				m_isJumping = false;
			}
		}
		Vector3 position = ((Component)this).transform.position;
		bool flag = IsOutOfWater();
		if ((Object)(object)m_waterVolume != (Object)null)
		{
			if ((++m_waterWaveCount & 1) == 1)
			{
				m_waterDepth = m_waterVolume.Depth(position);
			}
			else
			{
				m_waterWave = m_waterVolume.CalcWave(position, m_waterDepth, s_wrappedTimeSeconds, 1f);
			}
		}
		SetVisible(m_nview.HasOwner());
		if (m_lastOwner != m_nview.GetZDO().GetOwner())
		{
			m_lastOwner = m_nview.GetZDO().GetOwner();
			m_body.WakeUp();
		}
		if (!flag && Random.value > 0.975f && m_nview.GetZDO().GetInt(ZDOVars.s_hooked) == 1 && m_nview.GetZDO().GetFloat(ZDOVars.s_escape) > 0f)
		{
			m_jumpEffects.Create(position, Quaternion.identity, ((Component)this).transform);
		}
		if (!m_nview.IsOwner())
		{
			return;
		}
		FishingFloat fishingFloat = FishingFloat.FindFloat(this);
		if (Object.op_Implicit((Object)(object)fishingFloat))
		{
			Utils.Pull(m_body, ((Component)fishingFloat).transform.position, 1f, m_hookForce, 1f, 0.5f, false, false, 1f);
		}
		if (m_isColliding > 0 && flag)
		{
			ConsiderJump(s_time);
		}
		if (m_escapeTime > 0f)
		{
			Rigidbody body = m_body;
			body.rotation *= Quaternion.AngleAxis(MathF.Sin(m_escapeTime * 40f) * 12f, Vector3.up);
			m_escapeTime -= s_deltaTime;
			if (m_escapeTime <= 0f)
			{
				m_nview.GetZDO().Set(ZDOVars.s_escape, 0);
				m_nextEscape = s_time + Random.Range(m_escapeWaitMin, m_escapeWaitMax);
			}
		}
		else if (s_time > m_nextEscape && IsHooked())
		{
			Escape();
		}
		if (m_inWater <= -10000f || m_inWater < position.y + m_height)
		{
			m_body.useGravity = true;
			if (flag)
			{
				if (m_isJumping)
				{
					Vector3 linearVelocity = m_body.linearVelocity;
					if (!m_jumpedFromLand && linearVelocity != Vector3.zero)
					{
						linearVelocity.y *= 1.6f;
						m_body.rotation = Quaternion.RotateTowards(m_body.rotation, Quaternion.LookRotation(linearVelocity), 5f);
					}
				}
				return;
			}
		}
		if (m_isJumping)
		{
			if (m_body.linearVelocity.y < 0f)
			{
				m_jumpEffects.Create(position, Quaternion.identity);
				m_isJumping = false;
				Rigidbody body2 = m_body;
				Quaternion rotation = m_body.rotation;
				body2.rotation = Quaternion.Euler(0f, ((Quaternion)(ref rotation)).eulerAngles.y, 0f);
				RandomizeWaypoint(canHook: true, s_time);
			}
		}
		else if (m_waterWave >= m_minDepth && m_waterWave < m_minDepth + m_maxJumpDepthOffset)
		{
			ConsiderJump(s_time);
		}
		m_JumpHeightStrength = 1f;
		m_body.useGravity = false;
		m_fast = false;
		bool flag2 = s_time > m_blockChange;
		Player playerNoiseRange = Player.GetPlayerNoiseRange(position);
		if (Object.op_Implicit((Object)(object)playerNoiseRange))
		{
			if (Vector3.Distance(position, ((Component)playerNoiseRange).transform.position) > m_avoidRange / 2f && !IsHooked())
			{
				if (flag2 || s_time > m_lastCollision + m_collisionFleeTimeout)
				{
					Vector3 val = position - ((Component)playerNoiseRange).transform.position;
					Vector3 normalized = ((Vector3)(ref val)).normalized;
					SwimDirection(normalized, fast: true, avoidLand: true, s_deltaTime);
				}
				return;
			}
			m_fast = true;
			if (m_swimTimer > 0.5f)
			{
				m_swimTimer = 0.5f;
			}
		}
		m_swimTimer -= s_deltaTime;
		if (m_swimTimer <= 0f && flag2)
		{
			RandomizeWaypoint(!m_fast, s_time);
		}
		if (m_haveWaypoint)
		{
			if (Object.op_Implicit((Object)(object)m_waypointFF))
			{
				m_waypoint = ((Component)m_waypointFF).transform.position + Vector3.down;
			}
			if (Vector2.Distance(Vector2.op_Implicit(m_waypoint), Vector2.op_Implicit(position)) < 0.2f || (m_escapeTime < 0f && IsHooked()))
			{
				if (!Object.op_Implicit((Object)(object)m_waypointFF))
				{
					m_haveWaypoint = false;
					return;
				}
				if (s_time - m_lastNibbleTime > 1f && (Object)(object)m_failedBait != (Object)(object)m_waypointFF)
				{
					m_lastNibbleTime = s_time;
					bool flag3 = TestBate(m_waypointFF);
					m_waypointFF.Nibble(this, flag3);
					if (!flag3)
					{
						m_failedBait = m_waypointFF;
					}
				}
			}
			Vector3 dir = Vector3.Normalize(m_waypoint - position);
			SwimDirection(dir, m_fast, avoidLand: false, s_deltaTime);
		}
		else
		{
			Stop(s_deltaTime);
		}
		if (!flag && (Object)(object)m_waterVolume != (Object)null)
		{
			m_body.AddForce(new Vector3(0f, m_waterWave - m_lastWave, 0f) * 10f, (ForceMode)2);
			m_lastWave = m_waterWave;
			if (m_waterWave > 0f)
			{
				m_body.AddForce(Vector4.op_Implicit(s_wind) * m_waveFollowDirection * m_waterWave);
			}
		}
	}

	private void Stop(float dt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_inWater < ((Component)this).transform.position.y + m_height))
		{
			Vector3 forward = ((Component)this).transform.forward;
			forward.y = 0f;
			((Vector3)(ref forward)).Normalize();
			Quaternion val = Quaternion.LookRotation(forward, Vector3.up);
			Quaternion val2 = Quaternion.RotateTowards(m_body.rotation, val, m_turnRate * dt);
			m_body.MoveRotation(val2);
			Vector3 val3 = -m_body.linearVelocity * m_acceleration;
			m_body.AddForce(val3, (ForceMode)2);
		}
	}

	private void SwimDirection(Vector3 dir, bool fast, bool avoidLand, float dt)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = dir;
		val.y = 0f;
		if (val == Vector3.zero)
		{
			ZLog.LogWarning((object)"Invalid swim direction");
			return;
		}
		((Vector3)(ref val)).Normalize();
		float num = m_turnRate;
		if (fast)
		{
			num *= m_avoidSpeedScale;
		}
		Quaternion val2 = Quaternion.LookRotation(val, Vector3.up);
		Quaternion rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, val2, num * dt);
		if (!m_isJumping || !(m_body.linearVelocity.y > 0f))
		{
			if (!m_isJumping)
			{
				m_body.rotation = rotation;
			}
			float num2 = m_speed;
			if (fast)
			{
				num2 *= m_avoidSpeedScale;
			}
			if (avoidLand && GetPointDepth(((Component)this).transform.position + ((Component)this).transform.forward) < m_minDepth)
			{
				num2 = 0f;
			}
			if (fast && Vector3.Dot(dir, ((Component)this).transform.forward) < 0f)
			{
				num2 = 0f;
			}
			Vector3 forward = ((Component)this).transform.forward;
			forward.y = dir.y;
			Vector3 val3 = forward * num2 - m_body.linearVelocity;
			if (m_inWater < ((Component)this).transform.position.y + m_height && val3.y > 0f)
			{
				val3.y = 0f;
			}
			m_body.AddForce(val3 * m_acceleration, (ForceMode)2);
		}
	}

	private FishingFloat FindFloat()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		foreach (FishingFloat allInstance in FishingFloat.GetAllInstances())
		{
			if (allInstance.IsInWater() && !(Vector3.Distance(((Component)this).transform.position, ((Component)allInstance).transform.position) > allInstance.m_range) && !((Object)(object)allInstance.GetCatch() != (Object)null))
			{
				float baseHookChance = m_baseHookChance;
				if (Random.value < baseHookChance)
				{
					return allInstance;
				}
			}
		}
		return null;
	}

	private bool TestBate(FishingFloat ff)
	{
		string bait = ff.GetBait();
		foreach (BaitSetting bait2 in m_baits)
		{
			if (((Object)bait2.m_bait).name == bait && Random.value < bait2.m_chance)
			{
				return true;
			}
		}
		return false;
	}

	private bool RandomizeWaypoint(bool canHook, float timeNow)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (m_isJumping)
		{
			return false;
		}
		Vector2 val = Random.insideUnitCircle * m_swimRange;
		m_waypoint = m_spawnPoint + new Vector3(val.x, 0f, val.y);
		m_waypointFF = null;
		if (canHook)
		{
			FishingFloat fishingFloat = FindFloat();
			if (Object.op_Implicit((Object)(object)fishingFloat) && (Object)(object)fishingFloat != (Object)(object)m_failedBait)
			{
				m_waypointFF = fishingFloat;
				m_waypoint = ((Component)fishingFloat).transform.position + Vector3.down;
			}
		}
		float pointDepth = GetPointDepth(m_waypoint);
		if (pointDepth < m_minDepth)
		{
			return false;
		}
		Vector3 p = (m_waypoint + ((Component)this).transform.position) * 0.5f;
		if (GetPointDepth(p) < m_minDepth)
		{
			return false;
		}
		float num = Mathf.Min(m_maxDepth, pointDepth - m_height);
		float waterLevel = GetWaterLevel(m_waypoint);
		m_waypoint.y = waterLevel - Random.Range(m_minDepth, num);
		m_haveWaypoint = true;
		m_swimTimer = Random.Range(m_wpDurationMin, m_wpDurationMax);
		m_blockChange = timeNow + Random.Range(m_blockChangeDurationMin, m_blockChangeDurationMax);
		return true;
	}

	private void Escape()
	{
		m_escapeTime = Random.Range(m_escapeMin, m_escapeMax + (float)((!Object.op_Implicit((Object)(object)m_itemDrop)) ? 1 : m_itemDrop.m_itemData.m_quality) * m_escapeMaxPerLevel);
		m_nview.GetZDO().Set(ZDOVars.s_escape, m_escapeTime);
	}

	private float GetPointDepth(Vector3 p)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ZoneSystem.instance) && ZoneSystem.instance.GetSolidHeight(p, out var height, (!((Object)(object)m_waterVolume != (Object)null)) ? 1000 : 0))
		{
			return GetWaterLevel(p) - height;
		}
		return 0f;
	}

	private float GetWaterLevel(Vector3 point)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)m_waterVolume != (Object)null))
		{
			return 30f;
		}
		return m_waterVolume.GetWaterSurface(point);
	}

	private bool DangerNearby()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return (Object)(object)Player.GetPlayerNoiseRange(((Component)this).transform.position) != (Object)null;
	}

	public ZDOID GetZDOID()
	{
		return m_nview.GetZDO().m_uid;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(((Component)this).transform.position + Vector3.up * m_height, new Vector3(1f, 0.02f, 1f));
	}

	private void OnCollisionEnter(Collision collision)
	{
		m_isColliding++;
	}

	private void OnCollisionExit(Collision collision)
	{
		m_isColliding--;
	}

	private void onCollision()
	{
		m_lastCollision = s_time;
		if (!Object.op_Implicit((Object)(object)m_nview) || !m_nview.IsOwner())
		{
			return;
		}
		for (int i = 0; i < 10; i++)
		{
			if (RandomizeWaypoint(!m_fast, s_time))
			{
				break;
			}
		}
	}

	private void onDrop(ItemDrop item)
	{
		m_JumpHeightStrength = 0f;
	}

	private void ConsiderJump(float timeNow)
	{
		if ((Object.op_Implicit((Object)(object)m_itemDrop) && (float)m_itemDrop.m_itemData.m_quality > m_jumpMaxLevel) || !(m_JumpHeightStrength > 0f) || !(timeNow > m_lastJumpCheck + m_jumpFrequencySeconds))
		{
			return;
		}
		m_lastJumpCheck = timeNow;
		if (IsOutOfWater())
		{
			if (Random.Range(0f, 1f) < m_jumpOnLandChance * m_JumpHeightStrength)
			{
				Jump();
			}
		}
		else if (Random.Range(0f, 1f) < (m_jumpChance + Mathf.Min(0f, m_lastWave) * m_waveJumpMultiplier) * s_dawnDusk)
		{
			Jump();
		}
	}

	private void Jump()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!m_isJumping)
		{
			m_isJumping = true;
			if (IsOutOfWater())
			{
				m_jumpedFromLand = true;
				m_JumpHeightStrength *= m_jumpOnLandDecay;
				float jumpOnLandRotation = m_jumpOnLandRotation;
				m_body.AddForce(new Vector3(0f, m_JumpHeightStrength * m_jumpHeightLand * ((Component)this).transform.localScale.y, 0f), (ForceMode)1);
				m_body.AddTorque(Random.Range(0f - jumpOnLandRotation, jumpOnLandRotation), Random.Range(0f - jumpOnLandRotation, jumpOnLandRotation), Random.Range(0f - jumpOnLandRotation, jumpOnLandRotation), (ForceMode)1);
			}
			else
			{
				m_jumpedFromLand = false;
				m_jumpEffects.Create(((Component)this).transform.position, Quaternion.identity);
				m_body.AddForce(new Vector3(0f, m_jumpHeight * ((Component)this).transform.localScale.y, 0f), (ForceMode)1);
				m_body.AddForce(((Component)this).transform.forward * (m_jumpForwardStrength * ((Component)this).transform.localScale.y), (ForceMode)1);
			}
		}
	}

	public void OnHooked(FishingFloat ff)
	{
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.IsValid())
		{
			m_nview.ClaimOwnership();
		}
		m_fishingFloat = ff;
		if (m_nview.IsValid())
		{
			m_nview.GetZDO().Set(ZDOVars.s_hooked, ((Object)(object)ff != (Object)null) ? 1 : 0);
			Escape();
		}
	}

	public bool IsHooked()
	{
		return (Object)(object)m_fishingFloat != (Object)null;
	}

	public bool IsEscaping()
	{
		if (m_escapeTime > 0f)
		{
			return IsHooked();
		}
		return false;
	}

	public float GetStaminaUse()
	{
		if (!IsEscaping())
		{
			return m_staminaUse;
		}
		return m_escapeStaminaUse;
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

	public int Increment(LiquidType type)
	{
		return ++m_liquids[(int)type];
	}

	public int Decrement(LiquidType type)
	{
		return --m_liquids[(int)type];
	}
}
