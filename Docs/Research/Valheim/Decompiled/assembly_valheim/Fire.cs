using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
	public static List<Fire> s_fires = new List<Fire>();

	private static Collider[] m_colliders = (Collider[])(object)new Collider[128];

	private static List<KeyValuePair<IDestructible, Collider>> m_destructibles = new List<KeyValuePair<IDestructible, Collider>>();

	public float m_dotInterval = 1f;

	public float m_dotRadius = 1f;

	public float m_fireDamage = 10f;

	public float m_chopDamage = 10f;

	public short m_toolTier = 2;

	public int m_spread = 4;

	public float m_updateRate = 2f;

	[Header("Terrain hit")]
	public float m_terrainHitDelay;

	public float m_terrainMaxDist;

	public bool m_terrainCheckCultivated;

	public bool m_terrainCheckCleared;

	public GameObject m_terrainHitSpawn;

	public Heightmap.Biome m_terrainHitBiomes = Heightmap.Biome.All;

	[Header("Burn fuel from fireplaces")]
	public float m_fuelBurnChance = 0.5f;

	public float m_fuelBurnAmount = 0.1f;

	[Header("Smoke")]
	public SmokeSpawner m_smokeSpawner;

	public float m_smokeCheckHeight = 0.25f;

	public float m_smokeCheckRadius = 0.5f;

	public float m_smokeOxygenCheckHeight = 1.25f;

	public float m_smokeOxygenCheckRadius = 1.5f;

	public float m_smokeSuffocationPerHit = 0.2f;

	public int m_oxygenSmokeTolerance = 2;

	public int m_oxygenInteriorChecks = 5;

	public float m_smokeDieChance = 0.5f;

	public float m_maxSmoke = 3f;

	[Header("Effects")]
	public EffectList m_hitEffect;

	private static int s_dotMask = 0;

	private static int s_solidMask = 0;

	private static int s_terrainMask = 0;

	private static int s_smokeRayMask = 0;

	private static readonly RaycastHit[] s_raycastHits = (RaycastHit[])(object)new RaycastHit[32];

	private static readonly Collider[] s_hits = (Collider[])(object)new Collider[32];

	private int m_smokeHits;

	private bool m_inSmoke;

	private GameObject m_roof;

	private ZNetView m_nview;

	private float m_suffocating;

	private void Awake()
	{
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		m_nview = ((Component)this).GetComponent<ZNetView>();
		if (s_dotMask == 0)
		{
			s_dotMask = LayerMask.GetMask(new string[11]
			{
				"Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "character", "character_net", "character_ghost", "hitbox", "character_noenv",
				"vehicle"
			});
			s_solidMask = LayerMask.GetMask(new string[4] { "Default", "static_solid", "Default_small", "piece" });
			s_terrainMask = LayerMask.GetMask(new string[1] { "terrain" });
			s_smokeRayMask = LayerMask.GetMask(new string[1] { "smoke" });
		}
		((MonoBehaviour)this).InvokeRepeating("Dot", m_dotInterval, m_dotInterval);
		if (Object.op_Implicit((Object)(object)m_terrainHitSpawn) && (m_terrainHitBiomes == Heightmap.Biome.All || m_terrainHitBiomes.HasFlag(WorldGenerator.instance.GetBiome(((Component)this).transform.position))))
		{
			((MonoBehaviour)this).Invoke("HitTerrain", m_terrainHitDelay);
		}
		((MonoBehaviour)this).InvokeRepeating("UpdateFire", Random.Range(m_updateRate / 2f, m_updateRate), m_updateRate);
		s_fires.Add(this);
	}

	private void OnDestroy()
	{
		s_fires.Remove(this);
	}

	private void Dot()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner())
		{
			return;
		}
		m_destructibles.Clear();
		int num = Physics.OverlapSphereNonAlloc(((Component)this).transform.position, m_dotRadius, m_colliders, s_dotMask);
		for (int i = 0; i < num; i++)
		{
			GameObject val = Projectile.FindHitObject(m_colliders[i]);
			IDestructible component = val.GetComponent<IDestructible>();
			if (Random.Range(0f, 1f) < m_fuelBurnChance)
			{
				val.GetComponent<Fireplace>()?.AddFuel(0f - m_fuelBurnAmount);
			}
			if (Object.op_Implicit((Object)(object)val.GetComponent<Character>()))
			{
				DoDamage(component, m_colliders[i]);
				continue;
			}
			WearNTear component2 = val.GetComponent<WearNTear>();
			if ((component2 == null || component2.m_burnable) && component != null)
			{
				m_destructibles.Add(new KeyValuePair<IDestructible, Collider>(component, m_colliders[i]));
			}
		}
		if (m_destructibles.Count > 0)
		{
			KeyValuePair<IDestructible, Collider> keyValuePair = m_destructibles[Random.Range(0, m_destructibles.Count)];
			DoDamage(keyValuePair.Key, keyValuePair.Value);
		}
	}

	private void DoDamage(IDestructible toHit, Collider collider)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		HitData hitData = new HitData();
		hitData.m_hitCollider = collider;
		hitData.m_damage.m_fire = m_fireDamage;
		hitData.m_damage.m_chop = m_chopDamage;
		hitData.m_toolTier = m_toolTier;
		Vector3 position = ((Component)this).transform.position;
		Bounds bounds = collider.bounds;
		hitData.m_point = (position + ((Bounds)(ref bounds)).center) * 0.5f;
		hitData.m_dodgeable = false;
		hitData.m_blockable = false;
		hitData.m_hitType = HitData.HitType.CinderFire;
		m_hitEffect.Create(hitData.m_point, Quaternion.identity);
		toHit.Damage(hitData);
	}

	private void HitTerrain()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)this).transform.position, Vector3.down, ref val, m_terrainMaxDist, s_terrainMask))
		{
			Heightmap component = ((Component)((RaycastHit)(ref val)).collider).GetComponent<Heightmap>();
			if (component != null && !component.IsLava(((RaycastHit)(ref val)).point) && ((m_terrainCheckCultivated && !component.IsCultivated(((RaycastHit)(ref val)).point)) || (m_terrainCheckCleared && !component.IsCleared(((RaycastHit)(ref val)).point)) || (!m_terrainCheckCleared && !m_terrainCheckCultivated)))
			{
				Object.Instantiate<GameObject>(m_terrainHitSpawn, ((RaycastHit)(ref val)).point, Quaternion.identity);
			}
		}
	}

	private void UpdateFire()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid() || !m_nview.IsOwner())
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)m_roof))
		{
			WearNTear.RoofCheck(((Component)this).transform.position, out m_roof);
		}
		if (!Object.op_Implicit((Object)(object)m_roof) && EnvMan.IsWet())
		{
			ZNetScene.instance.Destroy(((Component)this).gameObject);
		}
		if (Object.op_Implicit((Object)(object)m_roof))
		{
			m_smokeHits = Physics.OverlapSphereNonAlloc(((Component)this).transform.position + Vector3.up * m_smokeOxygenCheckHeight, m_smokeOxygenCheckRadius, s_hits, s_smokeRayMask);
			m_smokeHits -= m_oxygenSmokeTolerance;
			if (m_smokeHits > 0)
			{
				m_suffocating += (float)m_smokeHits * m_smokeSuffocationPerHit;
				Terminal.Log($"Fire suffocation in interior with {m_smokeHits} smoke hits");
			}
			else
			{
				m_suffocating = Mathf.Max(0f, m_suffocating - 1f);
			}
		}
		else
		{
			m_inSmoke = Physics.CheckSphere(((Component)this).transform.position + Vector3.up * m_smokeCheckHeight, m_smokeCheckRadius, s_smokeRayMask);
			if (m_inSmoke)
			{
				m_suffocating += 1f;
				Terminal.Log("Fire in direct smoke");
			}
			else
			{
				m_suffocating = Mathf.Max(0f, m_suffocating - 1f);
			}
		}
		if (m_suffocating >= m_maxSmoke && (m_smokeDieChance >= 1f || Random.Range(0f, 1f) < m_smokeDieChance))
		{
			Terminal.Log("Fire suffocated");
			ZNetScene.instance.Destroy(((Component)this).gameObject);
		}
	}
}
