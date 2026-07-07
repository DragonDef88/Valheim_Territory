using System;
using UnityEngine;

public class Plant : SlowUpdate, Hoverable
{
	public enum Status
	{
		Healthy,
		NoSun,
		NoSpace,
		WrongBiome,
		NotCultivated,
		NoAttachPiece,
		TooHot,
		TooCold
	}

	private static Collider[] s_colliders = (Collider[])(object)new Collider[30];

	private static Collider[] s_hits = (Collider[])(object)new Collider[10];

	public string m_name = "Plant";

	public float m_growTime = 10f;

	public float m_growTimeMax = 2000f;

	public GameObject[] m_grownPrefabs = (GameObject[])(object)new GameObject[0];

	public float m_minScale = 1f;

	public float m_maxScale = 1f;

	public float m_growRadius = 1f;

	public float m_growRadiusVines;

	public bool m_needCultivatedGround;

	public bool m_destroyIfCantGrow;

	public bool m_tolerateHeat;

	public bool m_tolerateCold;

	[SerializeField]
	private GameObject m_healthy;

	[SerializeField]
	private GameObject m_unhealthy;

	[SerializeField]
	private GameObject m_healthyGrown;

	[SerializeField]
	private GameObject m_unhealthyGrown;

	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_biome;

	public EffectList m_growEffect = new EffectList();

	[Header("Attach to buildpiece (Vines)")]
	public float m_attachDistance;

	private Status m_status;

	private ZNetView m_nview;

	private float m_updateTime;

	private float m_spawnTime;

	private int m_seed;

	private Vector3 m_attachPos;

	private Vector3 m_attachNormal;

	private Quaternion m_attachRot;

	private Collider m_attachCollider;

	private static int m_spaceMask = 0;

	private static int m_roofMask = 0;

	private static int m_pieceMask = 0;

	public override void Awake()
	{
		base.Awake();
		m_nview = ((Component)this).gameObject.GetComponent<ZNetView>();
		if (m_nview.GetZDO() != null)
		{
			m_seed = m_nview.GetZDO().GetInt(ZDOVars.s_seed);
			if (m_seed == 0)
			{
				m_seed = (int)(m_nview.GetZDO().m_uid.ID + m_nview.GetZDO().m_uid.UserID);
				m_nview.GetZDO().Set(ZDOVars.s_seed, m_seed, okForNotOwner: true);
			}
			if (m_nview.IsOwner() && m_nview.GetZDO().GetLong(ZDOVars.s_plantTime, 0L) == 0L)
			{
				m_nview.GetZDO().Set(ZDOVars.s_plantTime, ZNet.instance.GetTime().Ticks);
			}
			m_spawnTime = Time.time;
		}
	}

	private void Start()
	{
		m_updateTime = Time.time + 10f;
	}

	public string GetHoverText()
	{
		return m_status switch
		{
			Status.Healthy => Localization.instance.Localize(m_name + " ( $piece_plant_healthy )"), 
			Status.NoSpace => Localization.instance.Localize(m_name + " ( $piece_plant_nospace )"), 
			Status.NoSun => Localization.instance.Localize(m_name + " ( $piece_plant_nosun )"), 
			Status.WrongBiome => Localization.instance.Localize(m_name + " ( $piece_plant_wrongbiome )"), 
			Status.NotCultivated => Localization.instance.Localize(m_name + " ( $piece_plant_notcultivated )"), 
			Status.TooHot => Localization.instance.Localize(m_name + " ( $piece_plant_toohot )"), 
			Status.TooCold => Localization.instance.Localize(m_name + " ( $piece_plant_toocold )"), 
			Status.NoAttachPiece => Localization.instance.Localize(m_name + " ( $piece_plant_nowall )"), 
			_ => "", 
		};
	}

	public string GetHoverName()
	{
		return Localization.instance.Localize(m_name);
	}

	private double TimeSincePlanted()
	{
		DateTime dateTime = new DateTime(m_nview.GetZDO().GetLong(ZDOVars.s_plantTime, ZNet.instance.GetTime().Ticks));
		return (ZNet.instance.GetTime() - dateTime).TotalSeconds;
	}

	public override void SUpdate(float time, Vector2i referenceZone)
	{
		if (m_nview.IsValid() && !(time > m_updateTime))
		{
			m_updateTime = time + 10f;
			double num = TimeSincePlanted();
			UpdateHealth(num);
			float growTime = GetGrowTime();
			if (Object.op_Implicit((Object)(object)m_healthyGrown))
			{
				bool flag = num > (double)(growTime * 0.5f);
				m_healthy.SetActive(!flag && m_status == Status.Healthy);
				m_unhealthy.SetActive(!flag && m_status != Status.Healthy);
				m_healthyGrown.SetActive(flag && m_status == Status.Healthy);
				m_unhealthyGrown.SetActive(flag && m_status != Status.Healthy);
			}
			else
			{
				m_healthy.SetActive(m_status == Status.Healthy);
				m_unhealthy.SetActive(m_status != Status.Healthy);
			}
			if (m_nview.IsOwner() && time - m_spawnTime > 10f && num > (double)growTime)
			{
				Grow();
			}
		}
	}

	private float GetGrowTime()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		State state = Random.state;
		Random.InitState(m_seed);
		float value = Random.value;
		Random.state = state;
		return Mathf.Lerp(m_growTime, m_growTimeMax, value);
	}

	public GameObject Grow()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		if (m_status != 0)
		{
			if (m_destroyIfCantGrow)
			{
				Destroy();
			}
			return null;
		}
		float num = 11.25f;
		GameObject obj = m_grownPrefabs[Random.Range(0, m_grownPrefabs.Length)];
		GameObject val = null;
		Vector3 val2 = ((m_attachDistance > 0f) ? m_attachPos : ((Component)this).transform.position);
		Quaternion val3;
		Quaternion val4;
		if (!(m_attachDistance > 0f))
		{
			val3 = ((Component)this).transform.rotation;
			float x = ((Quaternion)(ref val3)).eulerAngles.x;
			val3 = ((Component)this).transform.rotation;
			float num2 = ((Quaternion)(ref val3)).eulerAngles.y + Random.Range(0f - num, num);
			val3 = ((Component)this).transform.rotation;
			val4 = Quaternion.Euler(x, num2, ((Quaternion)(ref val3)).eulerAngles.z);
		}
		else
		{
			val4 = m_attachRot;
		}
		Quaternion val5 = val4;
		val = Object.Instantiate<GameObject>(obj, val2, val5);
		if (m_attachDistance > 0f)
		{
			PlaceAgainst(val, m_attachRot, m_attachPos, m_attachNormal);
		}
		val3 = val5;
		ZLog.Log((object)("Starting to grow plant with rotation: " + ((object)(Quaternion)(ref val3)).ToString()));
		ZNetView component = val.GetComponent<ZNetView>();
		float num3 = Random.Range(m_minScale, m_maxScale);
		component.SetLocalScale(new Vector3(num3, num3, num3));
		val.GetComponent<TreeBase>()?.Grow();
		if (Object.op_Implicit((Object)(object)m_nview))
		{
			m_nview.Destroy();
			m_growEffect.Create(((Component)this).transform.position, val5, null, num3);
		}
		return val;
	}

	public void UpdateHealth(double timeSincePlanted)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (timeSincePlanted < 10.0)
		{
			m_status = Status.Healthy;
			return;
		}
		Heightmap heightmap = Heightmap.FindHeightmap(((Component)this).transform.position);
		if (Object.op_Implicit((Object)(object)heightmap))
		{
			Heightmap.Biome biome = heightmap.GetBiome(((Component)this).transform.position);
			if ((biome & m_biome) == 0)
			{
				m_status = Status.WrongBiome;
				return;
			}
			if (m_needCultivatedGround && !heightmap.IsCultivated(((Component)this).transform.position))
			{
				m_status = Status.NotCultivated;
				return;
			}
			if (!m_tolerateHeat && biome == Heightmap.Biome.AshLands && !ShieldGenerator.IsInsideShield(((Component)this).transform.position))
			{
				m_status = Status.TooHot;
				return;
			}
			if (!m_tolerateCold && (biome == Heightmap.Biome.DeepNorth || biome == Heightmap.Biome.Mountain) && !ShieldGenerator.IsInsideShield(((Component)this).transform.position))
			{
				m_status = Status.TooCold;
				return;
			}
		}
		if (HaveRoof())
		{
			m_status = Status.NoSun;
		}
		else if (!HaveGrowSpace())
		{
			m_status = Status.NoSpace;
		}
		else if (m_attachDistance > 0f && !GetClosestAttachPosRot(out m_attachPos, out m_attachRot, out m_attachNormal))
		{
			m_status = Status.NoAttachPiece;
		}
		else
		{
			m_status = Status.Healthy;
		}
	}

	public Collider GetClosestAttachObject()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return GetClosestAttachObject(((Component)this).transform.position);
	}

	public Collider GetClosestAttachObject(Vector3 from)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (m_pieceMask == 0)
		{
			m_pieceMask = LayerMask.GetMask(new string[1] { "piece" });
		}
		int num = Physics.OverlapSphereNonAlloc(from, m_attachDistance, s_hits, m_pieceMask);
		Collider result = null;
		float num2 = float.MaxValue;
		for (int i = 0; i < num; i++)
		{
			Collider val = s_hits[i];
			Bounds bounds = val.bounds;
			float num3 = Vector3.Distance(from, ((Bounds)(ref bounds)).center);
			if (num3 < num2)
			{
				Piece componentInParent = ((Component)val).GetComponentInParent<Piece>();
				if (componentInParent != null && !componentInParent.m_noVines)
				{
					result = val;
					num2 = num3;
				}
			}
		}
		return result;
	}

	public bool GetClosestAttachPosRot(out Vector3 pos, out Quaternion rot, out Vector3 normal)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return GetClosestAttachPosRot(((Component)this).transform.position, out pos, out rot, out normal);
	}

	public bool GetClosestAttachPosRot(Vector3 from, out Vector3 pos, out Quaternion rot, out Vector3 normal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		Collider closestAttachObject = GetClosestAttachObject(from);
		Vector3 val3;
		if (closestAttachObject != null)
		{
			if (m_pieceMask == 0)
			{
				m_pieceMask = LayerMask.GetMask(new string[1] { "piece" });
			}
			float y = from.y;
			Bounds bounds = closestAttachObject.bounds;
			if (y < ((Bounds)(ref bounds)).min.y)
			{
				ref float y2 = ref from.y;
				float num = y2;
				bounds = closestAttachObject.bounds;
				y2 = num + (((Bounds)(ref bounds)).min.y - from.y + 0.01f);
			}
			float y3 = from.y;
			bounds = closestAttachObject.bounds;
			if (y3 > ((Bounds)(ref bounds)).max.y)
			{
				ref float y4 = ref from.y;
				float num2 = y4;
				bounds = closestAttachObject.bounds;
				y4 = num2 + (((Bounds)(ref bounds)).max.y - from.y - 0.01f);
			}
			Vector3 val = closestAttachObject.ClosestPoint(from);
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(from, val - from, ref val2, 50f, m_pieceMask) && Object.op_Implicit((Object)(object)((RaycastHit)(ref val2)).collider) && !Object.op_Implicit((Object)(object)((RaycastHit)(ref val2)).collider.attachedRigidbody))
			{
				pos = ((RaycastHit)(ref val2)).point;
				rot = Quaternion.Euler(0f, 90f, 0f) * Quaternion.LookRotation(((RaycastHit)(ref val2)).normal);
				normal = ((RaycastHit)(ref val2)).normal;
				val3 = ((RaycastHit)(ref val2)).normal;
				Terminal.Log("Plant found grow normal: " + ((object)(Vector3)(ref val3)).ToString());
				return true;
			}
			Terminal.Log("Plant ray didn't hit any valid colliders");
		}
		val3 = (pos = (normal = Vector3.zero));
		rot = Quaternion.identity;
		Terminal.Log("Plant found no attach obj.");
		return false;
	}

	public void PlaceAgainst(GameObject obj, Quaternion rot, Vector3 hitPos, Vector3 hitNormal)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		obj.transform.position = hitPos + hitNormal * 50f;
		obj.transform.rotation = rot;
		Vector3 val = Vector3.zero;
		float num = 999999f;
		Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>();
		foreach (Collider val2 in componentsInChildren)
		{
			if (val2.isTrigger || !val2.enabled)
			{
				continue;
			}
			MeshCollider val3 = (MeshCollider)(object)((val2 is MeshCollider) ? val2 : null);
			if (!((Object)(object)val3 != (Object)null) || val3.convex)
			{
				Vector3 val4 = val2.ClosestPoint(hitPos);
				float num2 = Vector3.Distance(val4, hitPos);
				if (num2 < num)
				{
					val = val4;
					num = num2;
				}
			}
		}
		Vector3 val5 = obj.transform.position - val;
		obj.transform.position = hitPos + val5;
		obj.transform.rotation = rot;
	}

	private void Destroy()
	{
		IDestructible component = ((Component)this).GetComponent<IDestructible>();
		if (component != null)
		{
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = 9999f;
			component.Damage(hitData);
		}
	}

	private bool HaveRoof()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (m_roofMask == 0)
		{
			m_roofMask = LayerMask.GetMask(new string[3] { "Default", "static_solid", "piece" });
		}
		if (Physics.Raycast(((Component)this).transform.position, Vector3.up, 100f, m_roofMask))
		{
			return true;
		}
		return false;
	}

	private bool HaveGrowSpace()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (m_spaceMask == 0)
		{
			m_spaceMask = LayerMask.GetMask(new string[5] { "Default", "static_solid", "Default_small", "piece", "piece_nonsolid" });
		}
		int num = Physics.OverlapSphereNonAlloc(((Component)this).transform.position, m_growRadius, s_colliders, m_spaceMask);
		for (int i = 0; i < num; i++)
		{
			Plant component = ((Component)s_colliders[i]).GetComponent<Plant>();
			if (!Object.op_Implicit((Object)(object)component) || (!((Object)(object)component == (Object)(object)this) && component.GetStatus() == Status.Healthy))
			{
				return false;
			}
		}
		if (m_growRadiusVines > 0f)
		{
			num = Physics.OverlapSphereNonAlloc(((Component)this).transform.position, m_growRadiusVines, s_colliders, m_spaceMask);
			for (int j = 0; j < num; j++)
			{
				if ((Object)(object)((Component)s_colliders[j]).GetComponentInParent<Vine>() != (Object)null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public Status GetStatus()
	{
		return m_status;
	}
}
