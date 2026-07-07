using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Location : MonoBehaviour
{
	[FormerlySerializedAs("m_radius")]
	public float m_exteriorRadius = 20f;

	public bool m_noBuild = true;

	public float m_noBuildRadiusOverride;

	public bool m_clearArea = true;

	public string m_discoverLabel = "";

	[Header("Other")]
	public bool m_applyRandomDamage;

	[Header("Interior")]
	public bool m_hasInterior;

	public float m_interiorRadius = 20f;

	public string m_interiorEnvironment = "";

	public Transform m_interiorTransform;

	[Tooltip("Makes the dungeon entrance start at the given interior transform (including rotation) rather than straight above the entrance, which gives the dungeon much more room to fill out the entire zone. Must use together with DungeonGenerator.m_useCustomInteriorTransform to make sure seeds are deterministic.")]
	public bool m_useCustomInteriorTransform;

	public DungeonGenerator m_generator;

	public GameObject m_interiorPrefab;

	[Header("Spawners")]
	public int m_enemyMinLevelOverride = -1;

	public int m_enemyMaxLevelOverride = -1;

	public float m_enemyLevelUpOverride = -1f;

	[Tooltip("Exludes CreatureSpawners of specified groups for level up override values above.")]
	public List<int> m_excludeEnemyLevelOverrideGroups = new List<int>();

	[Tooltip("Blocks any CreatureSpawner that is set to given SpawnGroups of these IDs.")]
	public List<int> m_blockSpawnGroups = new List<int>();

	private static List<Location> s_allLocations = new List<Location>();

	public Heightmap.Biome m_biome;

	private void Awake()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		s_allLocations.Add(this);
		if (m_hasInterior)
		{
			Vector3 zoneCenter = GetZoneCenter();
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(zoneCenter.x, ((Component)this).transform.position.y + 5000f, zoneCenter.z);
			GameObject obj = Object.Instantiate<GameObject>(m_interiorPrefab, val, Quaternion.identity, ((Component)this).transform);
			obj.transform.localScale = new Vector3(64f, 500f, 64f);
			obj.GetComponent<EnvZone>().m_environment = m_interiorEnvironment;
		}
	}

	private Vector3 GetZoneCenter()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return ZoneSystem.GetZonePos(ZoneSystem.GetZone(((Component)this).transform.position));
	}

	private void OnDestroy()
	{
		s_allLocations.Remove(this);
	}

	private void OnDrawGizmosSelected()
	{
	}

	private void OnDrawGizmos()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
		Gizmos.matrix = Matrix4x4.TRS(((Component)this).transform.position + new Vector3(0f, -0.01f, 0f), Quaternion.identity, new Vector3(1f, 0.001f, 1f));
		Gizmos.DrawSphere(Vector3.zero, m_exteriorRadius);
		Utils.DrawGizmoCircle(((Component)this).transform.position, m_noBuildRadiusOverride, 32);
		Gizmos.matrix = Matrix4x4.identity;
		if (m_hasInterior)
		{
			Utils.DrawGizmoCircle(((Component)this).transform.position + new Vector3(0f, 5000f, 0f), m_interiorRadius, 32);
			Utils.DrawGizmoCircle(((Component)this).transform.position, m_interiorRadius, 32);
			Gizmos.matrix = Matrix4x4.TRS(((Component)this).transform.position + new Vector3(0f, 5000f, 0f), Quaternion.identity, new Vector3(1f, 0.001f, 1f));
			Gizmos.DrawSphere(Vector3.zero, m_interiorRadius);
			Gizmos.matrix = Matrix4x4.identity;
		}
		Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.1f);
		Utils.DrawGizmoCircle(((Component)this).transform.position, m_exteriorRadius, 32);
	}

	public float GetMaxRadius()
	{
		if (!m_hasInterior)
		{
			return m_exteriorRadius;
		}
		return Mathf.Max(m_exteriorRadius, m_interiorRadius);
	}

	public bool IsInside(Vector3 point, float radius, bool buildCheck = false)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		float num = ((buildCheck && m_noBuildRadiusOverride > 0f) ? m_noBuildRadiusOverride : GetMaxRadius());
		return Utils.DistanceXZ(((Component)this).transform.position, point) < num + radius;
	}

	public static bool IsInsideLocation(Vector3 point, float distance)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Location s_allLocation in s_allLocations)
		{
			if (s_allLocation.IsInside(point, distance))
			{
				return true;
			}
		}
		return false;
	}

	public static Location GetLocation(Vector3 point, bool checkDungeons = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (Character.InInterior(point))
		{
			return GetZoneLocation(point);
		}
		foreach (Location s_allLocation in s_allLocations)
		{
			if (s_allLocation.IsInside(point, 0f))
			{
				return s_allLocation;
			}
		}
		return null;
	}

	public static Location GetZoneLocation(Vector2i zone)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		foreach (Location s_allLocation in s_allLocations)
		{
			if (zone == ZoneSystem.GetZone(((Component)s_allLocation).transform.position))
			{
				return s_allLocation;
			}
		}
		return null;
	}

	public static Location GetZoneLocation(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone = ZoneSystem.GetZone(point);
		foreach (Location s_allLocation in s_allLocations)
		{
			if (zone == ZoneSystem.GetZone(((Component)s_allLocation).transform.position))
			{
				return s_allLocation;
			}
		}
		return null;
	}

	public static bool IsInsideNoBuildLocation(Vector3 point)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Location s_allLocation in s_allLocations)
		{
			if (s_allLocation.m_noBuild && s_allLocation.IsInside(point, 0f, buildCheck: true))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsInsideActiveBossDungeon(Vector3 point)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)EnemyHud.instance != (Object)null)
		{
			Character activeBoss = EnemyHud.instance.GetActiveBoss();
			if (activeBoss != null && activeBoss.m_bossEvent.Length > 0)
			{
				Vector2i zone = ZoneSystem.GetZone(point);
				Vector2i zone2 = ZoneSystem.GetZone(((Component)activeBoss).transform.position);
				if (zone == zone2)
				{
					return true;
				}
			}
		}
		return false;
	}
}
