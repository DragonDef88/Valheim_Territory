using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomFlyingBird : MonoBehaviour, IMonoUpdater
{
	public float m_flyRange = 20f;

	public float m_minAlt = 5f;

	public float m_maxAlt = 20f;

	public float m_speed = 10f;

	public float m_turnRate = 10f;

	public float m_wpDuration = 4f;

	public float m_flapDuration = 2f;

	public float m_sailDuration = 4f;

	public float m_landChance = 0.5f;

	public float m_landDuration = 2f;

	public float m_avoidDangerDistance = 4f;

	public bool m_noRandomFlightAtNight = true;

	public float m_randomNoiseIntervalMin = 3f;

	public float m_randomNoiseIntervalMax = 6f;

	public bool m_noNoiseAtNight = true;

	public EffectList m_randomNoise = new EffectList();

	public int m_randomIdles;

	public float m_randomIdleTimeMin = 1f;

	public float m_randomIdleTimeMax = 4f;

	public bool m_singleModel;

	public GameObject m_flyingModel;

	public GameObject m_landedModel;

	private Vector3 m_spawnPoint;

	private Vector3 m_waypoint;

	private bool m_groundwp;

	private float m_flyTimer;

	private float m_modeTimer;

	private float m_idleTimer;

	private float m_idleTargetTime = 1f;

	private float m_randomNoiseTimer;

	private ZSyncAnimation m_anim;

	private bool m_flapping = true;

	private float m_landedTimer;

	private static readonly int s_flapping = ZSyncAnimation.GetHash("flapping");

	private static readonly int s_flying = ZSyncAnimation.GetHash("flying");

	private static readonly int s_idle = ZSyncAnimation.GetHash("idle");

	private ZNetView m_nview;

	protected LODGroup m_lodGroup;

	private Vector3 m_originalLocalRef;

	private bool m_lodVisible = true;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_anim = ((Component)this).GetComponentInChildren<ZSyncAnimation>();
		m_lodGroup = ((Component)this).GetComponent<LODGroup>();
		if (!m_singleModel)
		{
			m_landedModel.SetActive(true);
			m_flyingModel.SetActive(true);
		}
		else
		{
			m_flyingModel.SetActive(true);
		}
		m_idleTargetTime = Random.Range(m_randomIdleTimeMin, m_randomIdleTimeMax);
		m_spawnPoint = m_nview.GetZDO().GetVec3(ZDOVars.s_spawnPoint, ((Component)this).transform.position);
		if (m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_spawnPoint, m_spawnPoint);
		}
		m_randomNoiseTimer = Random.Range(m_randomNoiseIntervalMin, m_randomNoiseIntervalMax);
		if (m_nview.IsOwner())
		{
			RandomizeWaypoint(ground: false);
		}
		if (Object.op_Implicit((Object)(object)m_lodGroup))
		{
			m_originalLocalRef = m_lodGroup.localReferencePoint;
		}
	}

	private void OnEnable()
	{
		if ((Object)(object)m_nview != (Object)null)
		{
			Instances.Add(this);
		}
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public void CustomFixedUpdate(float dt)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return;
		}
		bool flag = EnvMan.IsDaylight();
		m_randomNoiseTimer -= dt;
		if (m_randomNoiseTimer <= 0f)
		{
			if (flag || !m_noNoiseAtNight)
			{
				m_randomNoise.Create(((Component)this).transform.position, Quaternion.identity, ((Component)this).transform);
			}
			m_randomNoiseTimer = Random.Range(m_randomNoiseIntervalMin, m_randomNoiseIntervalMax);
		}
		bool @bool = m_nview.GetZDO().GetBool(ZDOVars.s_landed);
		if (!m_singleModel)
		{
			m_landedModel.SetActive(@bool);
			m_flyingModel.SetActive(!@bool);
		}
		SetVisible(m_nview.HasOwner());
		if (!m_nview.IsOwner())
		{
			return;
		}
		m_flyTimer += dt;
		m_modeTimer += dt;
		if (m_singleModel)
		{
			m_anim.SetBool(s_flying, !@bool);
		}
		if (@bool)
		{
			Vector3 forward = ((Component)this).transform.forward;
			forward.y = 0f;
			((Vector3)(ref forward)).Normalize();
			((Component)this).transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
			if (m_randomIdles > 0)
			{
				m_idleTimer += Time.fixedDeltaTime;
				if (m_idleTimer > m_idleTargetTime)
				{
					m_idleTargetTime = Random.Range(m_randomIdleTimeMin, m_randomIdleTimeMax);
					m_idleTimer = 0f;
					m_anim.SetFloat(s_idle, Random.Range(0, m_randomIdles));
				}
			}
			m_landedTimer += dt;
			if (((flag || !m_noRandomFlightAtNight) && m_landedTimer > m_landDuration) || DangerNearby(((Component)this).transform.position))
			{
				m_nview.GetZDO().Set(ZDOVars.s_landed, value: false);
				RandomizeWaypoint(ground: false);
			}
			return;
		}
		if (m_flapping)
		{
			if (m_modeTimer > m_flapDuration)
			{
				m_modeTimer = 0f;
				m_flapping = false;
			}
		}
		else if (m_modeTimer > m_sailDuration)
		{
			m_flapping = true;
			m_modeTimer = 0f;
		}
		m_anim.SetBool(s_flapping, m_flapping);
		Vector3 val = Vector3.Normalize(m_waypoint - ((Component)this).transform.position);
		float num = (m_groundwp ? (m_turnRate * 4f) : m_turnRate);
		Vector3 val2 = Vector3.RotateTowards(((Component)this).transform.forward, val, num * ((float)Math.PI / 180f) * dt, 1f);
		float num2 = Vector3.SignedAngle(((Component)this).transform.forward, val, Vector3.up);
		Vector3 val3 = Vector3.Cross(val2, Vector3.up);
		Vector3 val4 = Vector3.up;
		val4 = ((!(num2 > 0f)) ? (val4 + val3 * 1.5f * Utils.LerpStep(0f, 45f, 0f - num2)) : (val4 + -val3 * 1.5f * Utils.LerpStep(0f, 45f, num2)));
		float num3 = m_speed;
		bool flag2 = false;
		if (m_groundwp)
		{
			float num4 = Vector3.Distance(((Component)this).transform.position, m_waypoint);
			if (num4 < 5f)
			{
				num3 *= Mathf.Clamp(num4 / 5f, 0.2f, 1f);
				val2.y = 0f;
				((Vector3)(ref val2)).Normalize();
				val4 = Vector3.up;
				flag2 = true;
			}
			if (num4 < 0.2f)
			{
				((Component)this).transform.position = m_waypoint;
				m_nview.GetZDO().Set(ZDOVars.s_landed, value: true);
				m_landedTimer = 0f;
				m_flapping = true;
				m_modeTimer = 0f;
			}
		}
		else if (m_flyTimer >= m_wpDuration)
		{
			bool ground = Random.value < m_landChance;
			RandomizeWaypoint(ground);
		}
		Quaternion val5 = Quaternion.LookRotation(val2, ((Vector3)(ref val4)).normalized);
		((Component)this).transform.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, val5, 200f * dt);
		if (flag2)
		{
			Transform transform = ((Component)this).transform;
			transform.position += val * num3 * dt;
		}
		else
		{
			Transform transform2 = ((Component)this).transform;
			transform2.position += ((Component)this).transform.forward * num3 * dt;
		}
	}

	private void RandomizeWaypoint(bool ground)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		m_flyTimer = 0f;
		if (ground && FindLandingPoint(out var waypoint))
		{
			m_waypoint = waypoint;
			m_groundwp = true;
			return;
		}
		Vector2 val = Random.insideUnitCircle * m_flyRange;
		m_waypoint = m_spawnPoint + new Vector3(val.x, 0f, val.y);
		if (ZoneSystem.instance.GetSolidHeight(m_waypoint, out var height))
		{
			float num = 32f;
			if (height < num)
			{
				height = num;
			}
			m_waypoint.y = height + Random.Range(m_minAlt, m_maxAlt);
		}
		m_groundwp = false;
	}

	private bool FindLandingPoint(out Vector3 waypoint)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		waypoint = new Vector3(0f, -999f, 0f);
		bool result = false;
		for (int i = 0; i < 10; i++)
		{
			Vector2 val = Random.insideUnitCircle * m_flyRange;
			Vector3 val2 = m_spawnPoint + new Vector3(val.x, 0f, val.y);
			if (ZoneSystem.instance.GetSolidHeight(val2, out var height) && height > 30f && height > waypoint.y)
			{
				val2.y = height;
				if (!DangerNearby(val2))
				{
					waypoint = val2;
					result = true;
				}
			}
		}
		return result;
	}

	private bool DangerNearby(Vector3 p)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Player.IsPlayerInRange(p, m_avoidDangerDistance);
	}

	private void SetVisible(bool visible)
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
}
