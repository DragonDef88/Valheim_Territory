using System;
using UnityEngine;

public class CinderSpawner : MonoBehaviour
{
	public GameObject m_cinderPrefab;

	public float m_cinderInterval = 2f;

	public float m_cinderChance = 0.1f;

	public float m_cinderVel = 5f;

	public float m_spawnOffset = 1f;

	public Vector3 m_spawnOffsetPoint;

	public int m_spread = 4;

	public int m_instancesPerSpawn = 1;

	public bool m_spawnOnAwake;

	public bool m_spawnOnProjectileHit;

	private ZNetView m_nview;

	private Heightmap.Biome m_biome;

	private GameObject m_attachObj;

	private bool m_hasAttachObj;

	private Fireplace m_fireplace;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponentInParent<ZNetView>();
		if (m_cinderInterval > 0f)
		{
			((MonoBehaviour)this).InvokeRepeating("UpdateSpawnCinder", m_cinderInterval, m_cinderInterval);
		}
		if (m_spawnOnAwake)
		{
			SpawnCinder();
		}
		if (m_spawnOnProjectileHit)
		{
			Projectile component = ((Component)this).GetComponent<Projectile>();
			if (component != null)
			{
				component.m_onHit = (OnProjectileHit)Delegate.Combine(component.m_onHit, (OnProjectileHit)delegate
				{
					SpawnCinder();
				});
			}
		}
		m_fireplace = ((Component)this).GetComponent<Fireplace>();
	}

	private void FixedUpdate()
	{
		if (m_hasAttachObj && !Object.op_Implicit((Object)(object)m_attachObj))
		{
			DestroyNow();
		}
	}

	public void Setup(int spread, GameObject attachObj)
	{
		m_nview.GetZDO().Set(ZDOVars.s_spread, spread);
		m_hasAttachObj = (Object)(object)attachObj != (Object)null;
		m_attachObj = attachObj;
	}

	private int GetSpread()
	{
		return m_nview.GetZDO().GetInt(ZDOVars.s_spread, m_spread);
	}

	private void UpdateSpawnCinder()
	{
		if (m_nview.IsValid() && m_nview.IsOwner() && (!Object.op_Implicit((Object)(object)m_fireplace) || m_fireplace.IsBurning()) && CanSpawnCinder() && GetSpread() > 0 && !(Random.value > m_cinderChance))
		{
			SpawnCinder();
		}
	}

	public void SpawnCinder()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsValid() && m_nview.IsOwner() && CanSpawnCinder() && !ShieldGenerator.IsInsideShield(((Component)this).transform.position))
		{
			for (int i = 0; i < m_instancesPerSpawn; i++)
			{
				Vector3 insideUnitSphere = Random.insideUnitSphere;
				insideUnitSphere.y = Mathf.Abs(insideUnitSphere.y * 2f);
				((Vector3)(ref insideUnitSphere)).Normalize();
				Object.Instantiate<GameObject>(m_cinderPrefab, ((Component)this).transform.position + insideUnitSphere * m_spawnOffset, Quaternion.identity).GetComponent<Cinder>().Setup(insideUnitSphere * m_cinderVel, GetSpread() - 1);
			}
		}
	}

	public bool CanSpawnCinder()
	{
		return CanSpawnCinder(((Component)this).transform, ref m_biome);
	}

	public static bool CanSpawnCinder(Transform transform, ref Heightmap.Biome biome)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (biome == Heightmap.Biome.None)
		{
			Vector3 p = transform.position;
			ZoneSystem.instance.GetGroundData(ref p, out var _, out var _, out var _, out var hmap);
			if ((Object)(object)hmap != (Object)null)
			{
				biome = hmap.GetBiome(transform.position);
			}
		}
		if (biome != Heightmap.Biome.AshLands)
		{
			return ZoneSystem.instance.GetGlobalKey(GlobalKeys.Fire);
		}
		return true;
	}

	private void DestroyNow()
	{
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			m_nview.Destroy();
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(((Component)this).transform.position + m_spawnOffsetPoint, 0.05f);
	}
}
