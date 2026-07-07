using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour, IWaterInteractable, IMonoUpdater
{
	public float m_waterLevelOffset;

	public float m_forceDistance = 1f;

	public float m_force = 0.5f;

	public float m_balanceForceFraction = 0.02f;

	public float m_damping = 0.05f;

	public EffectList m_impactEffects = new EffectList();

	public GameObject m_surfaceEffects;

	private static int s_waterVolumeMask = 0;

	private static readonly Collider[] s_tempColliderArray = (Collider[])(object)new Collider[256];

	private static readonly Dictionary<int, WaterVolume> s_waterVolumeCache = new Dictionary<int, WaterVolume>();

	private static readonly Dictionary<int, LiquidSurface> s_liquidSurfaceCache = new Dictionary<int, LiquidSurface>();

	private float m_waterLevel = -10000f;

	private float m_tarLevel = -10000f;

	private bool m_beenFloating;

	private bool m_wasInWater = true;

	private const float c_MinImpactEffectVelocity = 0.5f;

	private Rigidbody m_body;

	private Collider m_collider;

	private ZNetView m_nview;

	private readonly int[] m_liquids = new int[2];

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_body = ((Component)this).GetComponent<Rigidbody>();
		m_collider = ((Component)this).GetComponentInChildren<Collider>();
		SetSurfaceEffect(enabled: false);
		s_waterVolumeMask = LayerMask.GetMask(new string[1] { "WaterVolume" });
		((MonoBehaviour)this).InvokeRepeating("TerrainCheck", Random.Range(10f, 30f), 30f);
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public Transform GetTransform()
	{
		if ((Object)(object)this == (Object)null)
		{
			return null;
		}
		return ((Component)this).transform;
	}

	private void TerrainCheck()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner())
		{
			return;
		}
		float groundHeight = ZoneSystem.instance.GetGroundHeight(((Component)this).transform.position);
		if (((Component)this).transform.position.y - groundHeight < -1f)
		{
			Vector3 position = ((Component)this).transform.position;
			position.y = groundHeight + 1f;
			((Component)this).transform.position = position;
			Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.linearVelocity = Vector3.zero;
			}
			ZLog.Log((object)("Moved up item " + ((Object)((Component)this).gameObject).name));
		}
	}

	public void CustomFixedUpdate(float fixedDeltaTime)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		CheckBody();
		if (!Object.op_Implicit((Object)(object)m_body) || !m_nview.IsValid() || !m_nview.IsOwner())
		{
			return;
		}
		if (!HaveLiquidLevel())
		{
			SetSurfaceEffect(enabled: false);
			return;
		}
		UpdateImpactEffect();
		float floatDepth = GetFloatDepth();
		if (floatDepth > 0f)
		{
			SetSurfaceEffect(enabled: false);
			return;
		}
		SetSurfaceEffect(enabled: true);
		Vector3 val = m_collider.ClosestPoint(((Component)this).transform.position + Vector3.down * 1000f);
		Vector3 worldCenterOfMass = m_body.worldCenterOfMass;
		float num = Mathf.Clamp01(Mathf.Abs(floatDepth) / m_forceDistance);
		Vector3 val2 = m_force * num * (fixedDeltaTime * 50f) * Vector3.up;
		m_body.WakeUp();
		m_body.AddForceAtPosition(val2 * m_balanceForceFraction * m_body.mass, val, (ForceMode)1);
		m_body.AddForceAtPosition(val2 * m_body.mass, worldCenterOfMass, (ForceMode)1);
		m_body.linearVelocity -= m_damping * num * m_body.linearVelocity;
		m_body.angularVelocity -= m_damping * num * m_body.angularVelocity;
	}

	public bool HaveLiquidLevel()
	{
		if (!(m_waterLevel > -10000f))
		{
			return m_tarLevel > -10000f;
		}
		return true;
	}

	private void SetSurfaceEffect(bool enabled)
	{
		if ((Object)(object)m_surfaceEffects != (Object)null)
		{
			m_surfaceEffects.SetActive(enabled);
		}
	}

	private void UpdateImpactEffect()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		CheckBody();
		if (!Object.op_Implicit((Object)(object)m_body) || m_body.IsSleeping() || !m_impactEffects.HasEffects())
		{
			return;
		}
		Vector3 val = m_collider.ClosestPoint(((Component)this).transform.position + Vector3.down * 1000f);
		float num = Mathf.Max(m_waterLevel, m_tarLevel);
		if (val.y < num)
		{
			if (!m_wasInWater)
			{
				m_wasInWater = true;
				Vector3 basePos = val;
				basePos.y = num;
				Vector3 pointVelocity = m_body.GetPointVelocity(val);
				if (((Vector3)(ref pointVelocity)).magnitude > 0.5f)
				{
					m_impactEffects.Create(basePos, Quaternion.identity);
				}
			}
		}
		else
		{
			m_wasInWater = false;
		}
	}

	private float GetFloatDepth()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		CheckBody();
		if (!Object.op_Implicit((Object)(object)m_body))
		{
			return 0f;
		}
		Vector3 worldCenterOfMass = m_body.worldCenterOfMass;
		float num = Mathf.Max(m_waterLevel, m_tarLevel);
		return worldCenterOfMass.y - num - m_waterLevelOffset;
	}

	public bool IsInTar()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		CheckBody();
		if (m_tarLevel <= -10000f)
		{
			return false;
		}
		return m_body.worldCenterOfMass.y - m_tarLevel - m_waterLevelOffset < -0.2f;
	}

	public void SetLiquidLevel(float level, LiquidType type, Component liquidObj)
	{
		if (type == LiquidType.Water || type == LiquidType.Tar)
		{
			if (type == LiquidType.Water)
			{
				m_waterLevel = level;
			}
			else
			{
				m_tarLevel = level;
			}
			if (!m_beenFloating && level > -10000f && GetFloatDepth() < 0f)
			{
				m_beenFloating = true;
			}
		}
	}

	private void CheckBody()
	{
		if (!Object.op_Implicit((Object)(object)m_body))
		{
			m_body = FloatingTerrain.GetBody(((Component)this).gameObject);
		}
	}

	public bool BeenFloating()
	{
		return m_beenFloating;
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
		Gizmos.DrawWireCube(((Component)this).transform.position + Vector3.down * m_waterLevelOffset, new Vector3(1f, 0.05f, 1f));
	}

	public static float GetLiquidLevel(Vector3 p, float waveFactor = 1f, LiquidType type = LiquidType.All)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (s_waterVolumeMask == 0)
		{
			s_waterVolumeMask = LayerMask.GetMask(new string[1] { "WaterVolume" });
		}
		float num = -10000f;
		int num2 = Physics.OverlapSphereNonAlloc(p, 0f, s_tempColliderArray, s_waterVolumeMask);
		for (int i = 0; i < num2; i++)
		{
			Collider val = s_tempColliderArray[i];
			int instanceID = ((Object)val).GetInstanceID();
			if (!s_waterVolumeCache.TryGetValue(instanceID, out var value))
			{
				value = ((Component)val).GetComponent<WaterVolume>();
				s_waterVolumeCache[instanceID] = value;
			}
			if (Object.op_Implicit((Object)(object)value))
			{
				if (type == LiquidType.All || value.GetLiquidType() == type)
				{
					num = Mathf.Max(num, value.GetWaterSurface(p, waveFactor));
				}
				continue;
			}
			if (!s_liquidSurfaceCache.TryGetValue(instanceID, out var value2))
			{
				value2 = ((Component)val).GetComponent<LiquidSurface>();
				s_liquidSurfaceCache[instanceID] = value2;
			}
			if (Object.op_Implicit((Object)(object)value2) && (type == LiquidType.All || value2.GetLiquidType() == type))
			{
				num = Mathf.Max(num, value2.GetSurface(p));
			}
		}
		return num;
	}

	public static float GetWaterLevel(Vector3 p, ref WaterVolume previousAndOut)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)previousAndOut != (Object)null)
		{
			Bounds bounds = ((Component)previousAndOut).gameObject.GetComponent<Collider>().bounds;
			if (((Bounds)(ref bounds)).Contains(p))
			{
				return previousAndOut.GetWaterSurface(p);
			}
		}
		float num = -10000f;
		int num2 = Physics.OverlapSphereNonAlloc(p, 0f, s_tempColliderArray, s_waterVolumeMask);
		for (int i = 0; i < num2; i++)
		{
			Collider val = s_tempColliderArray[i];
			int instanceID = ((Object)val).GetInstanceID();
			if (!s_waterVolumeCache.TryGetValue(instanceID, out var value))
			{
				value = ((Component)val).GetComponent<WaterVolume>();
				s_waterVolumeCache[instanceID] = value;
			}
			if (Object.op_Implicit((Object)(object)value))
			{
				if (value.GetLiquidType() == LiquidType.Water)
				{
					float waterSurface = value.GetWaterSurface(p);
					if (waterSurface > num)
					{
						num = waterSurface;
						previousAndOut = value;
					}
				}
			}
			else
			{
				if (!s_liquidSurfaceCache.TryGetValue(instanceID, out var value2))
				{
					value2 = ((Component)val).GetComponent<LiquidSurface>();
					s_liquidSurfaceCache[instanceID] = value2;
				}
				if (Object.op_Implicit((Object)(object)value2) && value2.GetLiquidType() == LiquidType.Water)
				{
					num = Mathf.Max(num, value2.GetSurface(p));
				}
			}
		}
		return num;
	}

	public static bool IsUnderWater(Vector3 p, ref WaterVolume previousAndOut)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)previousAndOut != (Object)null)
		{
			Bounds bounds = ((Component)previousAndOut).gameObject.GetComponent<Collider>().bounds;
			if (((Bounds)(ref bounds)).Contains(p))
			{
				return previousAndOut.GetWaterSurface(p) > p.y;
			}
		}
		float num = -10000f;
		previousAndOut = null;
		int num2 = Physics.OverlapSphereNonAlloc(p, 0f, s_tempColliderArray, s_waterVolumeMask);
		for (int i = 0; i < num2; i++)
		{
			Collider val = s_tempColliderArray[i];
			int instanceID = ((Object)val).GetInstanceID();
			if (!s_waterVolumeCache.TryGetValue(instanceID, out var value))
			{
				value = ((Component)val).GetComponent<WaterVolume>();
				s_waterVolumeCache[instanceID] = value;
			}
			if (Object.op_Implicit((Object)(object)value))
			{
				if (value.GetLiquidType() == LiquidType.Water)
				{
					float waterSurface = value.GetWaterSurface(p);
					if (waterSurface > num)
					{
						num = waterSurface;
						previousAndOut = value;
					}
				}
			}
			else
			{
				if (!s_liquidSurfaceCache.TryGetValue(instanceID, out var value2))
				{
					value2 = ((Component)val).GetComponent<LiquidSurface>();
					s_liquidSurfaceCache[instanceID] = value2;
				}
				if (Object.op_Implicit((Object)(object)value2) && value2.GetLiquidType() == LiquidType.Water)
				{
					num = Mathf.Max(num, value2.GetSurface(p));
				}
			}
		}
		return num > p.y;
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
