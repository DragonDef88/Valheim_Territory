using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainModifier : MonoBehaviour
{
	public enum PaintType
	{
		Dirt,
		Cultivate,
		Paved,
		Reset,
		ClearVegetation
	}

	private static bool m_triggerOnPlaced = false;

	public int m_sortOrder;

	public bool m_useTerrainCompiler;

	public bool m_playerModifiction;

	public float m_levelOffset;

	[Header("Level")]
	public bool m_level;

	public float m_levelRadius = 2f;

	public bool m_square = true;

	[Header("Smooth")]
	public bool m_smooth;

	public float m_smoothRadius = 2f;

	public float m_smoothPower = 3f;

	[Header("Paint")]
	public bool m_paintCleared = true;

	public bool m_paintHeightCheck;

	public PaintType m_paintType;

	public float m_paintRadius = 2f;

	[Header("Effects")]
	public EffectList m_onPlacedEffect = new EffectList();

	[Header("Spawn items")]
	public GameObject m_spawnOnPlaced;

	public float m_chanceToSpawn = 1f;

	public int m_maxSpawned = 1;

	public bool m_spawnAtMaxLevelDepth = true;

	private bool m_wasEnabled;

	private long m_creationTime;

	private ZNetView m_nview;

	private static readonly List<TerrainModifier> s_instances = new List<TerrainModifier>();

	private static bool s_needsSorting = false;

	private static bool s_delayedPokeHeightmaps = false;

	private static int s_lastFramePoked = 0;

	private void Awake()
	{
		s_instances.Add(this);
		s_needsSorting = true;
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_wasEnabled = ((Behaviour)this).enabled;
		if (((Behaviour)this).enabled)
		{
			if (m_triggerOnPlaced)
			{
				OnPlaced();
			}
			PokeHeightmaps(forcedDelay: true);
		}
		m_creationTime = GetCreationTime();
	}

	private void OnDestroy()
	{
		s_instances.Remove(this);
		s_needsSorting = true;
		if (m_wasEnabled)
		{
			PokeHeightmaps();
		}
	}

	public static void RemoveAll()
	{
		s_instances.Clear();
	}

	private void PokeHeightmaps(bool forcedDelay = false)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		bool delayed = !m_triggerOnPlaced || forcedDelay;
		foreach (Heightmap allHeightmap in Heightmap.GetAllHeightmaps())
		{
			if (allHeightmap.TerrainVSModifier(this))
			{
				allHeightmap.Poke(delayed);
			}
		}
		if (Object.op_Implicit((Object)(object)ClutterSystem.instance))
		{
			ClutterSystem.instance.ResetGrass(((Component)this).transform.position, GetRadius());
		}
	}

	public float GetRadius()
	{
		float num = 0f;
		if (m_level && m_levelRadius > num)
		{
			num = m_levelRadius;
		}
		if (m_smooth && m_smoothRadius > num)
		{
			num = m_smoothRadius;
		}
		if (m_paintCleared && m_paintRadius > num)
		{
			num = m_paintRadius;
		}
		return num;
	}

	public static void SetTriggerOnPlaced(bool trigger)
	{
		m_triggerOnPlaced = trigger;
	}

	private void OnPlaced()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		RemoveOthers(((Component)this).transform.position, GetRadius() / 4f);
		m_onPlacedEffect.Create(((Component)this).transform.position, Quaternion.identity);
		if (Object.op_Implicit((Object)(object)m_spawnOnPlaced) && (m_spawnAtMaxLevelDepth || !Heightmap.AtMaxLevelDepth(((Component)this).transform.position + Vector3.up * m_levelOffset)) && Random.value <= m_chanceToSpawn)
		{
			Vector3 val = Vector2.op_Implicit(Random.insideUnitCircle * 0.2f);
			GameObject obj = Object.Instantiate<GameObject>(m_spawnOnPlaced, ((Component)this).transform.position + Vector3.up * 0.5f + val, Quaternion.identity);
			obj.GetComponent<ItemDrop>().m_itemData.m_stack = Random.Range(1, m_maxSpawned + 1);
			obj.GetComponent<Rigidbody>().linearVelocity = Vector3.up * 4f;
		}
	}

	private static void GetModifiers(Vector3 point, float range, List<TerrainModifier> modifiers, TerrainModifier ignore = null)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		foreach (TerrainModifier s_instance in s_instances)
		{
			if (!((Object)(object)s_instance == (Object)(object)ignore) && Utils.DistanceXZ(point, ((Component)s_instance).transform.position) < range)
			{
				modifiers.Add(s_instance);
			}
		}
	}

	public static Piece FindClosestModifierPieceInRange(Vector3 point, float range)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float num = 999999f;
		TerrainModifier terrainModifier = null;
		foreach (TerrainModifier s_instance in s_instances)
		{
			if (!((Object)(object)s_instance.m_nview == (Object)null))
			{
				float num2 = Utils.DistanceXZ(point, ((Component)s_instance).transform.position);
				if (!(num2 > range) && !(num2 > num))
				{
					num = num2;
					terrainModifier = s_instance;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)terrainModifier))
		{
			return ((Component)terrainModifier).GetComponent<Piece>();
		}
		return null;
	}

	private void RemoveOthers(Vector3 point, float range)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<TerrainModifier> list = new List<TerrainModifier>();
		GetModifiers(point, range, list, this);
		int num = 0;
		foreach (TerrainModifier item in list)
		{
			if ((m_level || !item.m_level) && (!m_paintCleared || m_paintType != PaintType.Reset || (item.m_paintCleared && item.m_paintType == PaintType.Reset)) && Object.op_Implicit((Object)(object)item.m_nview) && item.m_nview.IsValid())
			{
				num++;
				item.m_nview.ClaimOwnership();
				item.m_nview.Destroy();
			}
		}
	}

	private static int SortByModifiers(TerrainModifier a, TerrainModifier b)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (a.m_playerModifiction != b.m_playerModifiction)
		{
			return a.m_playerModifiction.CompareTo(b.m_playerModifiction);
		}
		if (a.m_sortOrder != b.m_sortOrder)
		{
			return a.m_sortOrder.CompareTo(b.m_sortOrder);
		}
		if (a.m_creationTime != b.m_creationTime)
		{
			return a.m_creationTime.CompareTo(b.m_creationTime);
		}
		Vector3 position = ((Component)a).transform.position;
		float sqrMagnitude = ((Vector3)(ref position)).sqrMagnitude;
		position = ((Component)b).transform.position;
		return sqrMagnitude.CompareTo(((Vector3)(ref position)).sqrMagnitude);
	}

	public static List<TerrainModifier> GetAllInstances()
	{
		if (s_needsSorting)
		{
			s_instances.Sort(SortByModifiers);
			s_needsSorting = false;
		}
		return s_instances;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.matrix = Matrix4x4.TRS(((Component)this).transform.position + Vector3.up * m_levelOffset, Quaternion.identity, new Vector3(1f, 0f, 1f));
		if (m_level)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(Vector3.zero, m_levelRadius);
		}
		if (m_smooth)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(Vector3.zero, m_smoothRadius);
		}
		if (m_paintCleared)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Vector3.zero, m_paintRadius);
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

	public ZDOID GetZDOID()
	{
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.GetZDO() != null)
		{
			return m_nview.GetZDO().m_uid;
		}
		return ZDOID.None;
	}

	private long GetCreationTime()
	{
		long num = 0L;
		if (Object.op_Implicit((Object)(object)m_nview) && m_nview.GetZDO() != null)
		{
			m_nview.GetZDO().GetPrefab();
			ZDO zDO = m_nview.GetZDO();
			ZDOID uid = zDO.m_uid;
			num = zDO.GetLong(ZDOVars.s_terrainModifierTimeCreated, 0L);
			if (num == 0L)
			{
				num = ZDOExtraData.GetTimeCreated(uid);
				if (num != 0L)
				{
					zDO.Set(ZDOVars.s_terrainModifierTimeCreated, num);
					Debug.LogError((object)("CreationTime should already be set for " + ((Object)m_nview).name + "  Prefab: " + m_nview.GetZDO().GetPrefab()));
				}
			}
		}
		return num;
	}
}
