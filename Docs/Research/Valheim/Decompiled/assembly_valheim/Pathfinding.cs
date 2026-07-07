using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pathfinding : MonoBehaviour
{
	private class NavMeshTile
	{
		public Vector3Int m_tile;

		public Vector3 m_center;

		public float m_pokeTime = -1000f;

		public float m_buildTime = -1000f;

		public NavMeshData m_data;

		public NavMeshDataInstance m_instance;

		public List<KeyValuePair<Vector3, NavMeshLinkInstance>> m_links1 = new List<KeyValuePair<Vector3, NavMeshLinkInstance>>();

		public List<KeyValuePair<Vector3, NavMeshLinkInstance>> m_links2 = new List<KeyValuePair<Vector3, NavMeshLinkInstance>>();
	}

	public enum AgentType
	{
		Humanoid = 1,
		TrollSize,
		HugeSize,
		HorseSize,
		HumanoidNoSwim,
		HumanoidAvoidWater,
		Fish,
		HumanoidBig,
		BigFish,
		GoblinBruteSize,
		HumanoidBigNoSwim,
		Abomination,
		SeekerQueen
	}

	public enum AreaType
	{
		Default,
		NotWalkable,
		Jump,
		Water
	}

	private class AgentSettings
	{
		public AgentType m_agentType;

		public NavMeshBuildSettings m_build;

		public bool m_canWalk = true;

		public bool m_avoidWater;

		public bool m_canSwim = true;

		public float m_swimDepth;

		public int m_areaMask = -1;

		public AgentSettings(AgentType type)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			m_agentType = type;
			m_build = NavMesh.CreateSettings();
		}
	}

	private List<Vector3> tempPath = new List<Vector3>();

	private List<Vector3> optPath = new List<Vector3>();

	private List<Vector3> tempStitchPoints = new List<Vector3>();

	private RaycastHit[] tempHitArray = (RaycastHit[])(object)new RaycastHit[255];

	private static Pathfinding m_instance;

	public LayerMask m_layers;

	public LayerMask m_waterLayers;

	private Dictionary<Vector3Int, NavMeshTile> m_tiles = new Dictionary<Vector3Int, NavMeshTile>();

	public float m_tileSize = 32f;

	public float m_defaultCost = 1f;

	public float m_waterCost = 4f;

	public float m_linkCost = 10f;

	public float m_linkWidth = 1f;

	public float m_updateInterval = 5f;

	public float m_tileTimeout = 30f;

	private const float m_tileHeight = 6000f;

	private const float m_tileY = 2500f;

	private float m_updatePathfindingTimer;

	private Queue<Vector3Int> m_queuedAreas = new Queue<Vector3Int>();

	private Queue<NavMeshLinkInstance> m_linkRemoveQueue = new Queue<NavMeshLinkInstance>();

	private Queue<NavMeshDataInstance> m_tileRemoveQueue = new Queue<NavMeshDataInstance>();

	private Vector3Int m_cachedTileID = new Vector3Int(-9999999, -9999999, -9999999);

	private NavMeshTile m_cachedTile;

	private List<AgentSettings> m_agentSettings = new List<AgentSettings>();

	private AsyncOperation m_buildOperation;

	private NavMeshTile m_buildTile;

	private List<KeyValuePair<NavMeshTile, NavMeshTile>> m_edgeBuildQueue = new List<KeyValuePair<NavMeshTile, NavMeshTile>>();

	private NavMeshPath m_path;

	public static Pathfinding instance => m_instance;

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		m_instance = this;
		SetupAgents();
		m_path = new NavMeshPath();
	}

	private void ClearAgentSettings()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		List<NavMeshBuildSettings> list = new List<NavMeshBuildSettings>();
		for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
		{
			list.Add(NavMesh.GetSettingsByIndex(i));
		}
		foreach (NavMeshBuildSettings item in list)
		{
			NavMeshBuildSettings current = item;
			if (((NavMeshBuildSettings)(ref current)).agentTypeID != 0)
			{
				NavMesh.RemoveSettings(((NavMeshBuildSettings)(ref current)).agentTypeID);
			}
		}
	}

	private void OnDestroy()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		foreach (NavMeshTile value in m_tiles.Values)
		{
			ClearLinks(value);
			if (Object.op_Implicit((Object)(object)value.m_data))
			{
				NavMesh.RemoveNavMeshData(value.m_instance);
			}
		}
		m_tiles.Clear();
		DestroyAllLinks();
	}

	private AgentSettings AddAgent(AgentType type, AgentSettings copy = null)
	{
		while ((int)(type + 1) > m_agentSettings.Count)
		{
			m_agentSettings.Add(null);
		}
		AgentSettings agentSettings = new AgentSettings(type);
		if (copy != null)
		{
			((NavMeshBuildSettings)(ref agentSettings.m_build)).agentHeight = ((NavMeshBuildSettings)(ref copy.m_build)).agentHeight;
			((NavMeshBuildSettings)(ref agentSettings.m_build)).agentClimb = ((NavMeshBuildSettings)(ref copy.m_build)).agentClimb;
			((NavMeshBuildSettings)(ref agentSettings.m_build)).agentRadius = ((NavMeshBuildSettings)(ref copy.m_build)).agentRadius;
			((NavMeshBuildSettings)(ref agentSettings.m_build)).agentSlope = ((NavMeshBuildSettings)(ref copy.m_build)).agentSlope;
		}
		m_agentSettings[(int)type] = agentSettings;
		return agentSettings;
	}

	private void SetupAgents()
	{
		ClearAgentSettings();
		AgentSettings agentSettings = AddAgent(AgentType.Humanoid);
		((NavMeshBuildSettings)(ref agentSettings.m_build)).agentHeight = 1.8f;
		((NavMeshBuildSettings)(ref agentSettings.m_build)).agentClimb = 0.3f;
		((NavMeshBuildSettings)(ref agentSettings.m_build)).agentRadius = 0.4f;
		((NavMeshBuildSettings)(ref agentSettings.m_build)).agentSlope = 85f;
		AddAgent(AgentType.HumanoidNoSwim, agentSettings).m_canSwim = false;
		AgentSettings agentSettings2 = AddAgent(AgentType.HumanoidBig, agentSettings);
		((NavMeshBuildSettings)(ref agentSettings2.m_build)).agentHeight = 2.5f;
		((NavMeshBuildSettings)(ref agentSettings2.m_build)).agentClimb = 0.3f;
		((NavMeshBuildSettings)(ref agentSettings2.m_build)).agentRadius = 0.5f;
		((NavMeshBuildSettings)(ref agentSettings2.m_build)).agentSlope = 85f;
		AgentSettings agentSettings3 = AddAgent(AgentType.HumanoidBigNoSwim);
		((NavMeshBuildSettings)(ref agentSettings3.m_build)).agentHeight = 2.5f;
		((NavMeshBuildSettings)(ref agentSettings3.m_build)).agentClimb = 0.3f;
		((NavMeshBuildSettings)(ref agentSettings3.m_build)).agentRadius = 0.5f;
		((NavMeshBuildSettings)(ref agentSettings3.m_build)).agentSlope = 85f;
		agentSettings3.m_canSwim = false;
		AddAgent(AgentType.HumanoidAvoidWater, agentSettings).m_avoidWater = true;
		AgentSettings agentSettings4 = AddAgent(AgentType.TrollSize);
		((NavMeshBuildSettings)(ref agentSettings4.m_build)).agentHeight = 7f;
		((NavMeshBuildSettings)(ref agentSettings4.m_build)).agentClimb = 0.6f;
		((NavMeshBuildSettings)(ref agentSettings4.m_build)).agentRadius = 1f;
		((NavMeshBuildSettings)(ref agentSettings4.m_build)).agentSlope = 85f;
		AgentSettings agentSettings5 = AddAgent(AgentType.Abomination);
		((NavMeshBuildSettings)(ref agentSettings5.m_build)).agentHeight = 5f;
		((NavMeshBuildSettings)(ref agentSettings5.m_build)).agentClimb = 0.6f;
		((NavMeshBuildSettings)(ref agentSettings5.m_build)).agentRadius = 1.5f;
		((NavMeshBuildSettings)(ref agentSettings5.m_build)).agentSlope = 85f;
		AgentSettings agentSettings6 = AddAgent(AgentType.SeekerQueen);
		((NavMeshBuildSettings)(ref agentSettings6.m_build)).agentHeight = 7f;
		((NavMeshBuildSettings)(ref agentSettings6.m_build)).agentClimb = 0.6f;
		((NavMeshBuildSettings)(ref agentSettings6.m_build)).agentRadius = 1.5f;
		((NavMeshBuildSettings)(ref agentSettings6.m_build)).agentSlope = 85f;
		AgentSettings agentSettings7 = AddAgent(AgentType.GoblinBruteSize);
		((NavMeshBuildSettings)(ref agentSettings7.m_build)).agentHeight = 3.5f;
		((NavMeshBuildSettings)(ref agentSettings7.m_build)).agentClimb = 0.3f;
		((NavMeshBuildSettings)(ref agentSettings7.m_build)).agentRadius = 0.8f;
		((NavMeshBuildSettings)(ref agentSettings7.m_build)).agentSlope = 85f;
		AgentSettings agentSettings8 = AddAgent(AgentType.HugeSize);
		((NavMeshBuildSettings)(ref agentSettings8.m_build)).agentHeight = 10f;
		((NavMeshBuildSettings)(ref agentSettings8.m_build)).agentClimb = 0.6f;
		((NavMeshBuildSettings)(ref agentSettings8.m_build)).agentRadius = 2f;
		((NavMeshBuildSettings)(ref agentSettings8.m_build)).agentSlope = 85f;
		AgentSettings agentSettings9 = AddAgent(AgentType.HorseSize);
		((NavMeshBuildSettings)(ref agentSettings9.m_build)).agentHeight = 2.5f;
		((NavMeshBuildSettings)(ref agentSettings9.m_build)).agentClimb = 0.3f;
		((NavMeshBuildSettings)(ref agentSettings9.m_build)).agentRadius = 0.8f;
		((NavMeshBuildSettings)(ref agentSettings9.m_build)).agentSlope = 85f;
		AgentSettings agentSettings10 = AddAgent(AgentType.Fish);
		((NavMeshBuildSettings)(ref agentSettings10.m_build)).agentHeight = 0.5f;
		((NavMeshBuildSettings)(ref agentSettings10.m_build)).agentClimb = 1f;
		((NavMeshBuildSettings)(ref agentSettings10.m_build)).agentRadius = 0.5f;
		((NavMeshBuildSettings)(ref agentSettings10.m_build)).agentSlope = 90f;
		agentSettings10.m_canSwim = true;
		agentSettings10.m_canWalk = false;
		agentSettings10.m_swimDepth = 0.4f;
		agentSettings10.m_areaMask = 12;
		AgentSettings agentSettings11 = AddAgent(AgentType.BigFish);
		((NavMeshBuildSettings)(ref agentSettings11.m_build)).agentHeight = 1.5f;
		((NavMeshBuildSettings)(ref agentSettings11.m_build)).agentClimb = 1f;
		((NavMeshBuildSettings)(ref agentSettings11.m_build)).agentRadius = 1f;
		((NavMeshBuildSettings)(ref agentSettings11.m_build)).agentSlope = 90f;
		agentSettings11.m_canSwim = true;
		agentSettings11.m_canWalk = false;
		agentSettings11.m_swimDepth = 1.5f;
		agentSettings11.m_areaMask = 12;
		NavMesh.SetAreaCost(0, m_defaultCost);
		NavMesh.SetAreaCost(3, m_waterCost);
	}

	private AgentSettings GetSettings(AgentType agentType)
	{
		return m_agentSettings[(int)agentType];
	}

	private int GetAgentID(AgentType agentType)
	{
		return ((NavMeshBuildSettings)(ref GetSettings(agentType).m_build)).agentTypeID;
	}

	private void Update()
	{
		if (!IsBuilding())
		{
			m_updatePathfindingTimer += Time.deltaTime;
			if (m_updatePathfindingTimer > 0.1f)
			{
				m_updatePathfindingTimer = 0f;
				UpdatePathfinding();
			}
			if (!IsBuilding())
			{
				DestroyQueuedNavmeshData();
			}
		}
	}

	private void DestroyAllLinks()
	{
		while (m_linkRemoveQueue.Count > 0 || m_tileRemoveQueue.Count > 0)
		{
			DestroyQueuedNavmeshData();
		}
	}

	private void DestroyQueuedNavmeshData()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (m_linkRemoveQueue.Count > 0)
		{
			int num = Mathf.Min(m_linkRemoveQueue.Count, Mathf.Max(25, m_linkRemoveQueue.Count / 40));
			for (int i = 0; i < num; i++)
			{
				NavMesh.RemoveLink(m_linkRemoveQueue.Dequeue());
			}
		}
		else if (m_tileRemoveQueue.Count > 0)
		{
			NavMesh.RemoveNavMeshData(m_tileRemoveQueue.Dequeue());
		}
	}

	private void UpdatePathfinding()
	{
		Buildtiles();
		TimeoutTiles();
	}

	public bool HavePath(Vector3 from, Vector3 to, AgentType agentType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetPath(from, to, null, agentType, requireFullPath: true, cleanup: false, havePath: true);
	}

	public bool FindValidPoint(out Vector3 point, Vector3 center, float range, AgentType agentType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		PokePoint(center, agentType);
		AgentSettings settings = GetSettings(agentType);
		NavMeshQueryFilter val = default(NavMeshQueryFilter);
		((NavMeshQueryFilter)(ref val)).agentTypeID = (int)settings.m_agentType;
		((NavMeshQueryFilter)(ref val)).areaMask = settings.m_areaMask;
		NavMeshHit val2 = default(NavMeshHit);
		if (NavMesh.SamplePosition(center, ref val2, range, val))
		{
			point = ((NavMeshHit)(ref val2)).position;
			return true;
		}
		point = center;
		return false;
	}

	private bool IsUnderTerrain(Vector3 p)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (ZoneSystem.instance.GetGroundHeight(p, out var height) && p.y < height - 1f)
		{
			return true;
		}
		return false;
	}

	public bool GetPath(Vector3 from, Vector3 to, List<Vector3> path, AgentType agentType, bool requireFullPath = false, bool cleanup = true, bool havePath = false)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Invalid comparison between Unknown and I4
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		path?.Clear();
		PokeArea(from, agentType);
		PokeArea(to, agentType);
		AgentSettings settings = GetSettings(agentType);
		if (!SnapToNavMesh(ref from, extendedSearchArea: true, settings))
		{
			return false;
		}
		if (!SnapToNavMesh(ref to, !havePath, settings))
		{
			return false;
		}
		NavMeshQueryFilter val = default(NavMeshQueryFilter);
		((NavMeshQueryFilter)(ref val)).agentTypeID = ((NavMeshBuildSettings)(ref settings.m_build)).agentTypeID;
		((NavMeshQueryFilter)(ref val)).areaMask = settings.m_areaMask;
		if (NavMesh.CalculatePath(from, to, val, m_path))
		{
			if ((int)m_path.status == 1)
			{
				if (IsUnderTerrain(m_path.corners[0]) || IsUnderTerrain(m_path.corners[m_path.corners.Length - 1]))
				{
					return false;
				}
				if (requireFullPath)
				{
					return false;
				}
			}
			if (path != null)
			{
				path.AddRange(m_path.corners);
				if (cleanup)
				{
					CleanPath(path, settings);
				}
			}
			return true;
		}
		return false;
	}

	private void CleanPath(List<Vector3> basePath, AgentSettings settings)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		if (basePath.Count <= 2)
		{
			return;
		}
		NavMeshQueryFilter val = default(NavMeshQueryFilter);
		((NavMeshQueryFilter)(ref val)).agentTypeID = ((NavMeshBuildSettings)(ref settings.m_build)).agentTypeID;
		((NavMeshQueryFilter)(ref val)).areaMask = settings.m_areaMask;
		int num = 0;
		optPath.Clear();
		optPath.Add(basePath[num]);
		do
		{
			num = FindNextNode(basePath, val, num);
			optPath.Add(basePath[num]);
		}
		while (num < basePath.Count - 1);
		tempPath.Clear();
		tempPath.Add(optPath[0]);
		NavMeshHit val7 = default(NavMeshHit);
		for (int i = 1; i < optPath.Count - 1; i++)
		{
			Vector3 val2 = optPath[i - 1];
			Vector3 val3 = optPath[i];
			Vector3 val4 = optPath[i + 1];
			Vector3 val5 = val4 - val3;
			Vector3 normalized = ((Vector3)(ref val5)).normalized;
			val5 = val3 - val2;
			Vector3 normalized2 = ((Vector3)(ref val5)).normalized;
			val5 = normalized + normalized2;
			Vector3 val6 = val3 - ((Vector3)(ref val5)).normalized * Vector3.Distance(val3, val2) * 0.33f;
			val6.y = (val3.y + val2.y) * 0.5f;
			val5 = val6 - val3;
			Vector3 normalized3 = ((Vector3)(ref val5)).normalized;
			if (!NavMesh.Raycast(val3 + normalized3 * 0.1f, val6, ref val7, val) && !NavMesh.Raycast(val6, val2, ref val7, val))
			{
				tempPath.Add(val6);
			}
			tempPath.Add(val3);
			val5 = normalized + normalized2;
			Vector3 val8 = val3 + ((Vector3)(ref val5)).normalized * Vector3.Distance(val3, val4) * 0.33f;
			val8.y = (val3.y + val4.y) * 0.5f;
			val5 = val8 - val3;
			Vector3 normalized4 = ((Vector3)(ref val5)).normalized;
			if (!NavMesh.Raycast(val3 + normalized4 * 0.1f, val8, ref val7, val) && !NavMesh.Raycast(val8, val4, ref val7, val))
			{
				tempPath.Add(val8);
			}
		}
		tempPath.Add(optPath[optPath.Count - 1]);
		basePath.Clear();
		basePath.AddRange(tempPath);
	}

	private int FindNextNode(List<Vector3> path, NavMeshQueryFilter filter, int start)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		NavMeshHit val = default(NavMeshHit);
		for (int i = start + 2; i < path.Count; i++)
		{
			if (NavMesh.Raycast(path[start], path[i], ref val, filter))
			{
				return i - 1;
			}
		}
		return path.Count - 1;
	}

	private bool SnapToNavMesh(ref Vector3 point, bool extendedSearchArea, AgentSettings settings)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ZoneSystem.instance))
		{
			if (ZoneSystem.instance.GetGroundHeight(point, out var height) && point.y < height)
			{
				point.y = height;
			}
			if (settings.m_canSwim)
			{
				point.y = Mathf.Max(30f - settings.m_swimDepth, point.y);
			}
		}
		NavMeshQueryFilter val = default(NavMeshQueryFilter);
		((NavMeshQueryFilter)(ref val)).agentTypeID = ((NavMeshBuildSettings)(ref settings.m_build)).agentTypeID;
		((NavMeshQueryFilter)(ref val)).areaMask = settings.m_areaMask;
		NavMeshHit val2 = default(NavMeshHit);
		if (extendedSearchArea)
		{
			if (NavMesh.SamplePosition(point, ref val2, 1.5f, val))
			{
				point = ((NavMeshHit)(ref val2)).position;
				return true;
			}
			if (NavMesh.SamplePosition(point, ref val2, 3f, val))
			{
				point = ((NavMeshHit)(ref val2)).position;
				return true;
			}
			if (NavMesh.SamplePosition(point, ref val2, 6f, val))
			{
				point = ((NavMeshHit)(ref val2)).position;
				return true;
			}
			if (NavMesh.SamplePosition(point, ref val2, 12f, val))
			{
				point = ((NavMeshHit)(ref val2)).position;
				return true;
			}
		}
		else if (NavMesh.SamplePosition(point, ref val2, 1f, val))
		{
			point = ((NavMeshHit)(ref val2)).position;
			return true;
		}
		return false;
	}

	private void TimeoutTiles()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		foreach (KeyValuePair<Vector3Int, NavMeshTile> tile in m_tiles)
		{
			if (realtimeSinceStartup - tile.Value.m_pokeTime > m_tileTimeout)
			{
				ClearLinks(tile.Value);
				if (((NavMeshDataInstance)(ref tile.Value.m_instance)).valid)
				{
					m_tileRemoveQueue.Enqueue(tile.Value.m_instance);
				}
				m_tiles.Remove(tile.Key);
				break;
			}
		}
	}

	private void PokeArea(Vector3 point, AgentType agentType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Vector3Int tile = GetTile(point, agentType);
		PokeTile(tile);
		Vector3Int tileID = default(Vector3Int);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (j != 0 || i != 0)
				{
					((Vector3Int)(ref tileID))._002Ector(((Vector3Int)(ref tile)).x + j, ((Vector3Int)(ref tile)).y + i, ((Vector3Int)(ref tile)).z);
					PokeTile(tileID);
				}
			}
		}
	}

	private void PokePoint(Vector3 point, AgentType agentType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Vector3Int tile = GetTile(point, agentType);
		PokeTile(tile);
	}

	private void PokeTile(Vector3Int tileID)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetNavTile(tileID).m_pokeTime = Time.realtimeSinceStartup;
	}

	private void Buildtiles()
	{
		if (UpdateAsyncBuild())
		{
			return;
		}
		NavMeshTile navMeshTile = null;
		float num = 0f;
		foreach (KeyValuePair<Vector3Int, NavMeshTile> tile in m_tiles)
		{
			float num2 = tile.Value.m_pokeTime - tile.Value.m_buildTime;
			if (num2 > m_updateInterval && (navMeshTile == null || num2 > num))
			{
				navMeshTile = tile.Value;
				num = num2;
			}
		}
		if (navMeshTile != null)
		{
			BuildTile(navMeshTile);
			navMeshTile.m_buildTime = Time.realtimeSinceStartup;
		}
	}

	private void BuildTile(NavMeshTile tile)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		_ = DateTime.Now;
		List<NavMeshBuildSource> list = new List<NavMeshBuildSource>();
		List<NavMeshBuildMarkup> list2 = new List<NavMeshBuildMarkup>();
		AgentType z = (AgentType)((Vector3Int)(ref tile.m_tile)).z;
		AgentSettings settings = GetSettings(z);
		Bounds val = default(Bounds);
		((Bounds)(ref val))._002Ector(tile.m_center, new Vector3(m_tileSize, 6000f, m_tileSize));
		Bounds val2 = default(Bounds);
		((Bounds)(ref val2))._002Ector(Vector3.zero, new Vector3(m_tileSize, 6000f, m_tileSize));
		int num = ((!settings.m_canWalk) ? 1 : 0);
		NavMeshBuilder.CollectSources(val, ((LayerMask)(ref m_layers)).value, (NavMeshCollectGeometry)1, num, list2, list);
		if (settings.m_avoidWater)
		{
			List<NavMeshBuildSource> list3 = new List<NavMeshBuildSource>();
			NavMeshBuilder.CollectSources(val, ((LayerMask)(ref m_waterLayers)).value, (NavMeshCollectGeometry)1, 1, list2, list3);
			foreach (NavMeshBuildSource item in list3)
			{
				NavMeshBuildSource current = item;
				((NavMeshBuildSource)(ref current)).transform = ((NavMeshBuildSource)(ref current)).transform * Matrix4x4.Translate(Vector3.down * 0.2f);
				list.Add(current);
			}
		}
		else if (settings.m_canSwim)
		{
			List<NavMeshBuildSource> list4 = new List<NavMeshBuildSource>();
			NavMeshBuilder.CollectSources(val, ((LayerMask)(ref m_waterLayers)).value, (NavMeshCollectGeometry)1, 3, list2, list4);
			if (settings.m_swimDepth != 0f)
			{
				foreach (NavMeshBuildSource item2 in list4)
				{
					NavMeshBuildSource current2 = item2;
					((NavMeshBuildSource)(ref current2)).transform = ((NavMeshBuildSource)(ref current2)).transform * Matrix4x4.Translate(Vector3.down * settings.m_swimDepth);
					list.Add(current2);
				}
			}
			else
			{
				list.AddRange(list4);
			}
		}
		if ((Object)(object)tile.m_data == (Object)null)
		{
			tile.m_data = new NavMeshData();
			tile.m_data.position = tile.m_center;
		}
		m_buildOperation = NavMeshBuilder.UpdateNavMeshDataAsync(tile.m_data, settings.m_build, list, val2);
		m_buildTile = tile;
	}

	private bool IsBuilding()
	{
		if (m_buildOperation != null)
		{
			return !m_buildOperation.isDone;
		}
		return false;
	}

	private bool UpdateAsyncBuild()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (m_buildOperation == null)
		{
			return false;
		}
		if (!m_buildOperation.isDone)
		{
			return true;
		}
		if (!((NavMeshDataInstance)(ref m_buildTile.m_instance)).valid)
		{
			m_buildTile.m_instance = NavMesh.AddNavMeshData(m_buildTile.m_data);
		}
		RebuildLinks(m_buildTile);
		m_buildOperation = null;
		m_buildTile = null;
		return true;
	}

	private void ClearLinks(NavMeshTile tile)
	{
		ClearLinks(tile.m_links1);
		ClearLinks(tile.m_links2);
	}

	private void ClearLinks(List<KeyValuePair<Vector3, NavMeshLinkInstance>> links)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Vector3, NavMeshLinkInstance> link in links)
		{
			m_linkRemoveQueue.Enqueue(link.Value);
		}
		links.Clear();
	}

	private void RebuildLinks(NavMeshTile tile)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		AgentType z = (AgentType)((Vector3Int)(ref tile.m_tile)).z;
		AgentSettings settings = GetSettings(z);
		float num = m_tileSize / 2f;
		ConnectAlongEdge(tile.m_links1, tile.m_center + new Vector3(num, 0f, num), tile.m_center + new Vector3(num, 0f, 0f - num), m_linkWidth, settings);
		ConnectAlongEdge(tile.m_links2, tile.m_center + new Vector3(0f - num, 0f, num), tile.m_center + new Vector3(num, 0f, num), m_linkWidth, settings);
	}

	private void ConnectAlongEdge(List<KeyValuePair<Vector3, NavMeshLinkInstance>> links, Vector3 p0, Vector3 p1, float step, AgentSettings settings)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = p1 - p0;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = Vector3.Cross(Vector3.up, normalized);
		float num = Vector3.Distance(p0, p1);
		bool canSwim = settings.m_canSwim;
		tempStitchPoints.Clear();
		for (float num2 = step / 2f; num2 <= num; num2 += step)
		{
			Vector3 p2 = p0 + normalized * num2;
			FindGround(p2, canSwim, tempStitchPoints, settings);
		}
		if (CompareLinks(tempStitchPoints, links))
		{
			return;
		}
		ClearLinks(links);
		foreach (Vector3 tempStitchPoint in tempStitchPoints)
		{
			NavMeshLinkData val3 = default(NavMeshLinkData);
			((NavMeshLinkData)(ref val3)).startPosition = tempStitchPoint - val2 * 0.1f;
			((NavMeshLinkData)(ref val3)).endPosition = tempStitchPoint + val2 * 0.1f;
			((NavMeshLinkData)(ref val3)).width = step;
			((NavMeshLinkData)(ref val3)).costModifier = m_linkCost;
			((NavMeshLinkData)(ref val3)).bidirectional = true;
			((NavMeshLinkData)(ref val3)).agentTypeID = ((NavMeshBuildSettings)(ref settings.m_build)).agentTypeID;
			((NavMeshLinkData)(ref val3)).area = 2;
			NavMeshLinkInstance value = NavMesh.AddLink(val3);
			if (((NavMeshLinkInstance)(ref value)).valid)
			{
				links.Add(new KeyValuePair<Vector3, NavMeshLinkInstance>(tempStitchPoint, value));
			}
		}
	}

	private bool CompareLinks(List<Vector3> tempStitchPoints, List<KeyValuePair<Vector3, NavMeshLinkInstance>> links)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (tempStitchPoints.Count != links.Count)
		{
			return false;
		}
		for (int i = 0; i < tempStitchPoints.Count; i++)
		{
			if (tempStitchPoints[i] != links[i].Key)
			{
				return false;
			}
		}
		return true;
	}

	private bool SnapToNearestGround(Vector3 p, out Vector3 pos, float range)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(p + Vector3.up, Vector3.down, ref val, range + 1f, ((LayerMask)(ref m_layers)).value | ((LayerMask)(ref m_waterLayers)).value))
		{
			pos = ((RaycastHit)(ref val)).point;
			return true;
		}
		if (Physics.Raycast(p + Vector3.up * range, Vector3.down, ref val, range, ((LayerMask)(ref m_layers)).value | ((LayerMask)(ref m_waterLayers)).value))
		{
			pos = ((RaycastHit)(ref val)).point;
			return true;
		}
		pos = p;
		return false;
	}

	private void FindGround(Vector3 p, bool testWater, List<Vector3> hits, AgentSettings settings)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		p.y = 6000f;
		int num = (testWater ? (((LayerMask)(ref m_layers)).value | ((LayerMask)(ref m_waterLayers)).value) : ((LayerMask)(ref m_layers)).value);
		float agentHeight = ((NavMeshBuildSettings)(ref settings.m_build)).agentHeight;
		float y = p.y;
		int num2 = Physics.RaycastNonAlloc(p, Vector3.down, tempHitArray, 10000f, num);
		for (int i = 0; i < num2; i++)
		{
			Vector3 point = ((RaycastHit)(ref tempHitArray[i])).point;
			if (!(Mathf.Abs(point.y - y) < agentHeight))
			{
				y = point.y;
				if (((1 << ((Component)((RaycastHit)(ref tempHitArray[i])).collider).gameObject.layer) & LayerMask.op_Implicit(m_waterLayers)) != 0)
				{
					point.y -= settings.m_swimDepth;
				}
				hits.Add(point);
			}
		}
	}

	private NavMeshTile GetNavTile(Vector3 point, AgentType agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Vector3Int tile = GetTile(point, agent);
		return GetNavTile(tile);
	}

	private NavMeshTile GetNavTile(Vector3Int tile)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (tile == m_cachedTileID)
		{
			return m_cachedTile;
		}
		if (m_tiles.TryGetValue(tile, out var value))
		{
			m_cachedTileID = tile;
			m_cachedTile = value;
			return value;
		}
		value = new NavMeshTile();
		value.m_tile = tile;
		value.m_center = GetTilePos(tile);
		m_tiles.Add(tile, value);
		m_cachedTileID = tile;
		m_cachedTile = value;
		return value;
	}

	private Vector3Int GetTile(Vector3 point, AgentType agent)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt((point.x + m_tileSize / 2f) / m_tileSize);
		int num2 = Mathf.FloorToInt((point.z + m_tileSize / 2f) / m_tileSize);
		return new Vector3Int(num, num2, (int)agent);
	}

	public Vector3 GetTilePos(Vector3Int id)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3((float)((Vector3Int)(ref id)).x * m_tileSize, 2500f, (float)((Vector3Int)(ref id)).y * m_tileSize);
	}
}
