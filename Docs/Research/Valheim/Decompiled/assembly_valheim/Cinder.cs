using UnityEngine;

public class Cinder : MonoBehaviour
{
	public GameObject m_firePrefab;

	public GameObject m_houseFirePrefab;

	public float m_gravity = 10f;

	public float m_drag;

	public float m_windStrength;

	public int m_spread = 4;

	[Range(0f, 1f)]
	public float m_chanceToIgniteGrass = 0.1f;

	public EffectList m_hitEffects;

	private Vector3 m_vel;

	private static int m_raymask;

	private ZNetView m_nview;

	private bool m_haveHit;

	private void Awake()
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (m_raymask == 0)
		{
			m_raymask = LayerMask.GetMask(new string[12]
			{
				"Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "terrain", "character", "character_net", "character_ghost", "hitbox",
				"character_noenv", "vehicle"
			});
		}
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			Vector3 position = ((Component)this).transform.position;
			position -= EnvMan.instance.GetWindForce() * m_windStrength * 10f;
			((Component)this).transform.position = position;
		}
	}

	private void FixedUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		if (!m_haveHit && m_nview.IsValid() && m_nview.IsOwner())
		{
			float fixedDeltaTime = Time.fixedDeltaTime;
			m_vel += EnvMan.instance.GetWindForce() * (fixedDeltaTime * m_windStrength);
			m_vel += Vector3.down * (m_gravity * fixedDeltaTime);
			float num = Mathf.Pow(((Vector3)(ref m_vel)).magnitude, 2f) * m_drag * Time.fixedDeltaTime;
			m_vel += num * -((Vector3)(ref m_vel)).normalized;
			Vector3 position = ((Component)this).transform.position;
			Vector3 val = position + m_vel * fixedDeltaTime;
			((Component)this).transform.position = val;
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(position, ((Vector3)(ref m_vel)).normalized, ref val2, Vector3.Distance(position, val), m_raymask))
			{
				OnHit(((RaycastHit)(ref val2)).collider, ((RaycastHit)(ref val2)).point, ((RaycastHit)(ref val2)).normal);
			}
			ShieldGenerator.CheckObjectInsideShield(this);
		}
	}

	private void OnHit(Collider collider, Vector3 point, Vector3 normal)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		m_hitEffects.Create(point, Quaternion.identity);
		if (CanBurn(collider, point, out var isTerrain, m_chanceToIgniteGrass))
		{
			GameObject val = ((!isTerrain) ? Object.Instantiate<GameObject>(m_houseFirePrefab, point + normal * 0.1f, Quaternion.identity) : Object.Instantiate<GameObject>(m_firePrefab, point + normal * 0.1f, Quaternion.identity));
			val.GetComponent<CinderSpawner>()?.Setup(GetSpread(), ((Component)collider).gameObject);
		}
		m_haveHit = true;
		((Component)this).transform.position = point;
		((MonoBehaviour)this).InvokeRepeating("DestroyNow", 0.25f, 1f);
	}

	private void OnShieldHit()
	{
	}

	private void DestroyNow()
	{
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			m_nview.Destroy();
		}
	}

	public static bool CanBurn(Collider collider, Vector3 point, out bool isTerrain, float chanceToIgniteGrass = 0f)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		isTerrain = false;
		if (point.y < 30f)
		{
			return false;
		}
		if (Floating.GetLiquidLevel(point) > point.y)
		{
			return false;
		}
		Piece componentInParent = ((Component)collider).gameObject.GetComponentInParent<Piece>();
		if (componentInParent != null && Player.IsPlacementGhost(((Component)componentInParent).gameObject))
		{
			return false;
		}
		WearNTear componentInParent2 = ((Component)collider).gameObject.GetComponentInParent<WearNTear>();
		if (componentInParent2 != null)
		{
			if (componentInParent2.m_burnable && !componentInParent2.IsWet())
			{
				return true;
			}
		}
		else
		{
			if (((Component)collider).gameObject.GetComponentInParent<TreeBase>() != null)
			{
				return true;
			}
			if (((Component)collider).gameObject.GetComponentInParent<TreeLog>() != null)
			{
				return true;
			}
		}
		if (EnvMan.IsWet())
		{
			return false;
		}
		if (chanceToIgniteGrass > 0f)
		{
			Heightmap component = ((Component)collider).GetComponent<Heightmap>();
			if (Object.op_Implicit((Object)(object)component))
			{
				if (component.IsCleared(point))
				{
					return false;
				}
				Heightmap.Biome biome = component.GetBiome(point);
				if (biome == Heightmap.Biome.Mountain || biome == Heightmap.Biome.DeepNorth)
				{
					return false;
				}
				isTerrain = true;
				return Random.value <= chanceToIgniteGrass;
			}
		}
		return false;
	}

	public void Setup(Vector3 vel, int spread)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		m_vel = vel;
		m_nview.GetZDO().Set(ZDOVars.s_spread, spread);
	}

	private int GetSpread()
	{
		return m_nview.GetZDO().GetInt(ZDOVars.s_spread, m_spread);
	}
}
