using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using SoftReferenceableAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZoneSystem : MonoBehaviour
{
	private class ZoneData
	{
		public GameObject m_root;

		public float m_ttl;
	}

	private class ClearArea
	{
		public Vector3 m_center;

		public float m_radius;

		public ClearArea(Vector3 p, float r)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			m_center = p;
			m_radius = r;
		}
	}

	[Serializable]
	public class ZoneVegetation
	{
		public string m_name = "veg";

		public GameObject m_prefab;

		public bool m_enable = true;

		public float m_min;

		public float m_max = 10f;

		public bool m_forcePlacement;

		public float m_scaleMin = 1f;

		public float m_scaleMax = 1f;

		public float m_randTilt;

		public float m_chanceToUseGroundTilt;

		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		[BitMask(typeof(Heightmap.BiomeArea))]
		public Heightmap.BiomeArea m_biomeArea = Heightmap.BiomeArea.Everything;

		public bool m_blockCheck = true;

		public bool m_snapToStaticSolid;

		public float m_minAltitude = -1000f;

		public float m_maxAltitude = 1000f;

		public float m_minVegetation;

		public float m_maxVegetation;

		[Header("Samples points around and choses the highest vegetation")]
		[Tooltip("Samples points around the placement point and choses the point with most total vegetation value")]
		public bool m_surroundCheckVegetation;

		[Tooltip("How far to check surroundings")]
		public float m_surroundCheckDistance = 20f;

		[Tooltip("How many layers of circles to sample. (If distance is large you should have more layers)")]
		public int m_surroundCheckLayers = 2;

		[Tooltip("How much better than the average an accepted point will be. (Procentually between average and best)")]
		public float m_surroundBetterThanAverage;

		[Space(10f)]
		public float m_minOceanDepth;

		public float m_maxOceanDepth;

		public float m_minTilt;

		public float m_maxTilt = 90f;

		public float m_terrainDeltaRadius;

		public float m_maxTerrainDelta = 2f;

		public float m_minTerrainDelta;

		public bool m_snapToWater;

		public float m_groundOffset;

		public int m_groupSizeMin = 1;

		public int m_groupSizeMax = 1;

		public float m_groupRadius;

		[Header("Distance from center")]
		public float m_minDistanceFromCenter;

		public float m_maxDistanceFromCenter;

		[Header("Forest fractal 0-1 inside forest")]
		public bool m_inForest;

		public float m_forestTresholdMin;

		public float m_forestTresholdMax = 1f;

		[HideInInspector]
		public bool m_foldout;

		public ZoneVegetation Clone()
		{
			return MemberwiseClone() as ZoneVegetation;
		}
	}

	[Serializable]
	public class ZoneLocation
	{
		public string m_name;

		public bool m_enable = true;

		[HideInInspector]
		public string m_prefabName;

		public SoftReference<GameObject> m_prefab;

		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		[BitMask(typeof(Heightmap.BiomeArea))]
		public Heightmap.BiomeArea m_biomeArea = Heightmap.BiomeArea.Everything;

		public int m_quantity;

		public bool m_prioritized;

		public bool m_centerFirst;

		public bool m_unique;

		public string m_group = "";

		public float m_minDistanceFromSimilar;

		public string m_groupMax = "";

		public float m_maxDistanceFromSimilar;

		public bool m_iconAlways;

		public bool m_iconPlaced;

		public bool m_randomRotation = true;

		public bool m_slopeRotation;

		public bool m_snapToWater;

		public float m_interiorRadius;

		public float m_exteriorRadius;

		public bool m_clearArea;

		public float m_minTerrainDelta;

		public float m_maxTerrainDelta = 2f;

		public float m_minimumVegetation;

		public float m_maximumVegetation = 1f;

		[Header("Samples points around and choses the highest vegetation")]
		[Tooltip("Samples points around the placement point and choses the point with most total vegetation value")]
		public bool m_surroundCheckVegetation;

		[Tooltip("How far to check surroundings")]
		public float m_surroundCheckDistance = 20f;

		[Tooltip("How many layers of circles to sample. (If distance is large you should have more layers)")]
		public int m_surroundCheckLayers = 2;

		[Tooltip("How much better than the average an accepted point will be. (Procentually between average and best)")]
		public float m_surroundBetterThanAverage;

		[Header("Forest fractal 0-1 inside forest")]
		public bool m_inForest;

		public float m_forestTresholdMin;

		public float m_forestTresholdMax = 1f;

		[Header("Distance from center")]
		public float m_minDistanceFromCenter;

		public float m_maxDistanceFromCenter;

		[Space(10f)]
		public float m_minDistance;

		public float m_maxDistance;

		public float m_minAltitude = -1000f;

		public float m_maxAltitude = 1000f;

		[HideInInspector]
		public bool m_foldout;

		public int Hash => StringExtensionMethods.GetStableHashCode(m_prefab.Name);

		public ZoneLocation Clone()
		{
			return MemberwiseClone() as ZoneLocation;
		}
	}

	public struct LocationInstance
	{
		public ZoneLocation m_location;

		public Vector3 m_position;

		public bool m_placed;
	}

	private class LocationPrefabLoadData
	{
		private SoftReference<GameObject> m_prefab;

		private SoftReference<GameObject>[] m_possibleRooms;

		private int m_roomsToLoad;

		private bool m_isFirstSpawn;

		public int m_iterationLifetime;

		public bool IsLoaded { get; private set; }

		public AssetID PrefabAssetID => m_prefab.m_assetID;

		public LocationPrefabLoadData(SoftReference<GameObject> prefab, bool isFirstSpawn)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			m_prefab = prefab;
			m_isFirstSpawn = isFirstSpawn;
			m_roomsToLoad = 0;
			m_prefab.LoadAsync(new LoadedHandler(OnPrefabLoaded));
		}

		public void Release()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (!m_prefab.IsValid)
			{
				return;
			}
			m_prefab.Release();
			m_prefab.m_assetID = default(AssetID);
			if (m_possibleRooms != null)
			{
				for (int i = 0; i < m_possibleRooms.Length; i++)
				{
					m_possibleRooms[i].Release();
				}
				m_possibleRooms = null;
			}
		}

		private void OnPrefabLoaded(AssetID assetID, LoadResult result)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Expected O, but got Unknown
			if ((int)result != 0 || !m_prefab.IsValid)
			{
				return;
			}
			if (!m_isFirstSpawn)
			{
				IsLoaded = true;
				return;
			}
			DungeonGenerator[] enabledComponentsInChildren = Utils.GetEnabledComponentsInChildren<DungeonGenerator>(m_prefab.Asset);
			if (enabledComponentsInChildren.Length == 0)
			{
				IsLoaded = true;
				return;
			}
			if (enabledComponentsInChildren.Length > 1)
			{
				ZLog.LogWarning((object)("Location " + ((Object)m_prefab.Asset).name + " has more than one dungeon generator! The preloading code only works for one dungeon generator per location."));
			}
			m_possibleRooms = enabledComponentsInChildren[0].GetAvailableRoomPrefabs();
			m_roomsToLoad = m_possibleRooms.Length;
			for (int i = 0; i < m_possibleRooms.Length; i++)
			{
				m_possibleRooms[i].LoadAsync(new LoadedHandler(OnRoomLoaded));
			}
		}

		private void OnRoomLoaded(AssetID assetID, LoadResult result)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if ((int)result == 0)
			{
				m_roomsToLoad--;
				if (m_possibleRooms != null && m_roomsToLoad <= 0)
				{
					IsLoaded = true;
				}
			}
		}
	}

	public enum SpawnMode
	{
		Full,
		Client,
		Ghost
	}

	private Dictionary<Vector3, string> tempIconList = new Dictionary<Vector3, string>();

	private List<float> s_tempVeg = new List<float>();

	private RaycastHit[] rayHits = (RaycastHit[])(object)new RaycastHit[200];

	private static readonly List<RaycastHit> s_rayHits = new List<RaycastHit>(64);

	private static readonly List<float> s_rayHitsHeight = new List<float>(64);

	private List<string> m_tempKeys = new List<string>();

	private static ZoneSystem m_instance;

	[HideInInspector]
	public List<Heightmap.Biome> m_biomeFolded = new List<Heightmap.Biome>();

	[HideInInspector]
	public List<Heightmap.Biome> m_vegetationFolded = new List<Heightmap.Biome>();

	[HideInInspector]
	public List<Heightmap.Biome> m_locationFolded = new List<Heightmap.Biome>();

	[NonSerialized]
	public bool m_drawLocations;

	[NonSerialized]
	public string m_drawLocationsFilter = "";

	[Tooltip("Zones to load around center sector")]
	public int m_activeArea = 1;

	public int m_activeDistantArea = 1;

	[Tooltip("Zone size, should match netscene sector size")]
	public float m_zoneSize = 64f;

	[Tooltip("Time before destroying inactive zone")]
	public float m_zoneTTL = 4f;

	[Tooltip("Time before spawning active zone")]
	public float m_zoneTTS = 4f;

	public GameObject m_zonePrefab;

	public GameObject m_zoneCtrlPrefab;

	public GameObject m_locationProxyPrefab;

	public float m_waterLevel = 30f;

	public const float c_WaterLevel = 30f;

	public const float c_ZoneSize = 64f;

	public const double c_ZoneSizeDouble = 64.0;

	public const float c_ZoneHalfSize = 32f;

	public const double c_ZoneHalfSizeDouble = 32.0;

	[Header("Versions")]
	public int m_pgwVersion = 53;

	public int m_locationVersion = 1;

	[Header("Generation data")]
	public List<string> m_locationScenes = new List<string>();

	public List<GameObject> m_locationLists = new List<GameObject>();

	public List<ZoneVegetation> m_vegetation = new List<ZoneVegetation>();

	public List<ZoneLocation> m_locations = new List<ZoneLocation>();

	private Dictionary<int, ZoneLocation> m_locationsByHash = new Dictionary<int, ZoneLocation>();

	private bool m_error;

	public bool m_didZoneTest;

	private int m_terrainRayMask;

	private int m_blockRayMask;

	private int m_solidRayMask;

	private int m_staticSolidRayMask;

	private float m_updateTimer;

	private float m_startTime;

	private float m_lastFixedTime;

	private Dictionary<Vector2i, ZoneData> m_zones = new Dictionary<Vector2i, ZoneData>();

	private HashSet<Vector2i> m_generatedZones = new HashSet<Vector2i>();

	private Dictionary<Vector2i, List<ZDO>> m_loadingObjectsInZones = new Dictionary<Vector2i, List<ZDO>>();

	private Coroutine m_generateLocationsCoroutine;

	private DateTime m_estimatedGenerateLocationsCompletionTime;

	private float m_timeSlicedGenerationTimeBudget = 0.01f;

	private bool m_locationsGenerated;

	private float m_generateLocationsProgress;

	[HideInInspector]
	public Dictionary<Vector2i, LocationInstance> m_locationInstances = new Dictionary<Vector2i, LocationInstance>();

	private Dictionary<Vector3, string> m_locationIcons = new Dictionary<Vector3, string>();

	private HashSet<string> m_globalKeys = new HashSet<string>();

	public HashSet<GlobalKeys> m_globalKeysEnums = new HashSet<GlobalKeys>();

	public Dictionary<string, string> m_globalKeysValues = new Dictionary<string, string>();

	private HashSet<Vector2i> m_tempGeneratedZonesSaveClone;

	private HashSet<string> m_tempGlobalKeysSaveClone;

	private List<LocationInstance> m_tempLocationsSaveClone;

	private bool m_tempLocationsGeneratedSaveClone;

	private List<ClearArea> m_tempClearAreas = new List<ClearArea>();

	private List<GameObject> m_tempSpawnedObjects = new List<GameObject>();

	private List<int> m_tempLocationPrefabsToRelease = new List<int>();

	private List<LocationPrefabLoadData> m_locationPrefabs = new List<LocationPrefabLoadData>();

	private Action m_generateLocationsCompleted;

	public static ZoneSystem instance => m_instance;

	public bool LocationsGenerated
	{
		get
		{
			return m_locationsGenerated;
		}
		private set
		{
			m_locationsGenerated = value;
			if (m_locationsGenerated)
			{
				m_generateLocationsCompleted?.Invoke();
				m_generateLocationsCompleted = null;
			}
		}
	}

	public float GenerateLocationsProgress => m_generateLocationsProgress;

	public event Action GenerateLocationsCompleted
	{
		add
		{
			if (m_locationsGenerated)
			{
				value?.Invoke();
			}
			else
			{
				m_generateLocationsCompleted = (Action)Delegate.Combine(m_generateLocationsCompleted, value);
			}
		}
		remove
		{
			m_generateLocationsCompleted = (Action)Delegate.Remove(m_generateLocationsCompleted, value);
		}
	}

	private ZoneSystem()
	{
	}

	private void Awake()
	{
		m_instance = this;
		m_terrainRayMask = LayerMask.GetMask(new string[1] { "terrain" });
		m_blockRayMask = LayerMask.GetMask(new string[4] { "Default", "static_solid", "Default_small", "piece" });
		m_solidRayMask = LayerMask.GetMask(new string[5] { "Default", "static_solid", "Default_small", "piece", "terrain" });
		m_staticSolidRayMask = LayerMask.GetMask(new string[2] { "static_solid", "terrain" });
		foreach (GameObject locationList in m_locationLists)
		{
			Object.Instantiate<GameObject>(locationList);
		}
		ZLog.Log((object)("Zonesystem Awake " + Time.frameCount));
	}

	private void Start()
	{
		ZLog.Log((object)("Zonesystem Start " + Time.frameCount));
		UpdateWorldRates();
		SetupLocations();
		ValidateVegetation();
		ZRoutedRpc zRoutedRpc = ZRoutedRpc.instance;
		zRoutedRpc.m_onNewPeer = (Action<long>)Delegate.Combine(zRoutedRpc.m_onNewPeer, new Action<long>(OnNewPeer));
		if (ZNet.instance.IsServer())
		{
			ZRoutedRpc.instance.Register<string>("SetGlobalKey", RPC_SetGlobalKey);
			ZRoutedRpc.instance.Register<string>("RemoveGlobalKey", RPC_RemoveGlobalKey);
		}
		else
		{
			ZRoutedRpc.instance.Register<List<string>>("GlobalKeys", RPC_GlobalKeys);
			ZRoutedRpc.instance.Register<ZPackage>("LocationIcons", RPC_LocationIcons);
		}
		m_startTime = (m_lastFixedTime = Time.fixedTime);
	}

	public void GenerateLocationsIfNeeded()
	{
		if (!LocationsGenerated)
		{
			GenerateLocations();
		}
	}

	private void SendGlobalKeys(long peer)
	{
		List<string> list = new List<string>(m_globalKeys);
		ZRoutedRpc.instance.InvokeRoutedRPC(peer, "GlobalKeys", list);
		Player.m_localPlayer?.UpdateEvents();
	}

	private void RPC_GlobalKeys(long sender, List<string> keys)
	{
		ZLog.Log((object)("client got keys " + keys.Count));
		ClearGlobalKeys();
		foreach (string key in keys)
		{
			GlobalKeyAdd(key);
		}
	}

	private void GlobalKeyAdd(string keyStr, bool canSaveToServerOptionKeys = true)
	{
		string value;
		GlobalKeys gk;
		string keyValue = GetKeyValue(keyStr.ToLower(), out value, out gk);
		bool flag = canSaveToServerOptionKeys && ZNet.World != null && gk < GlobalKeys.NonServerOption;
		if (m_globalKeysValues.TryGetValue(keyValue, out var value2))
		{
			string item = (keyValue + " " + value2).TrimEnd();
			m_globalKeys.Remove(item);
			if (flag)
			{
				ZNet.World.m_startingGlobalKeys.Remove(item);
			}
		}
		string text = (keyValue + " " + value).TrimEnd();
		m_globalKeys.Add(text);
		m_globalKeysValues[keyValue] = value;
		if (gk != GlobalKeys.NonServerOption)
		{
			m_globalKeysEnums.Add(gk);
		}
		Utils.IncrementOrSet<string>(Game.instance.GetPlayerProfile().m_knownWorldKeys, text, 1f);
		if (flag)
		{
			ZNet.World.m_startingGlobalKeys.Add(keyStr.ToLower());
		}
		UpdateWorldRates();
	}

	private bool GlobalKeyRemove(string keyStr, bool canSaveToServerOptionKeys = true)
	{
		string value;
		GlobalKeys gk;
		string keyValue = GetKeyValue(keyStr, out value, out gk);
		if (m_globalKeysValues.TryGetValue(keyValue, out var value2))
		{
			string item = (keyValue + " " + value2).TrimEnd();
			if (canSaveToServerOptionKeys && ZNet.World != null && gk < GlobalKeys.NonServerOption)
			{
				ZNet.World.m_startingGlobalKeys.Remove(item);
			}
			m_globalKeys.Remove(item);
			m_globalKeysValues.Remove(keyValue);
			if (gk != GlobalKeys.NonServerOption)
			{
				m_globalKeysEnums.Remove(gk);
			}
			UpdateWorldRates();
			return true;
		}
		return false;
	}

	public void UpdateWorldRates()
	{
		Game.UpdateWorldRates(m_globalKeys, m_globalKeysValues);
	}

	public void Reset()
	{
		ClearGlobalKeys();
		UpdateWorldRates();
	}

	private void ClearGlobalKeys()
	{
		m_globalKeys.Clear();
		m_globalKeysEnums.Clear();
		m_globalKeysValues.Clear();
	}

	public static string GetKeyValue(string key, out string value, out GlobalKeys gk)
	{
		int num = key.IndexOf(' ');
		value = "";
		string text;
		if (num > 0)
		{
			value = key.Substring(num + 1);
			text = key.Substring(0, num).ToLower();
		}
		else
		{
			text = key.ToLower();
		}
		if (!Enum.TryParse<GlobalKeys>(text, ignoreCase: true, out gk))
		{
			gk = GlobalKeys.NonServerOption;
		}
		return text;
	}

	private void SendLocationIcons(long peer)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		ZPackage zPackage = new ZPackage();
		tempIconList.Clear();
		GetLocationIcons(tempIconList);
		zPackage.Write(tempIconList.Count);
		foreach (KeyValuePair<Vector3, string> tempIcon in tempIconList)
		{
			zPackage.Write(tempIcon.Key);
			zPackage.Write(tempIcon.Value);
		}
		ZRoutedRpc.instance.InvokeRoutedRPC(peer, "LocationIcons", zPackage);
	}

	private void RPC_LocationIcons(long sender, ZPackage pkg)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)"client got location icons");
		m_locationIcons.Clear();
		int num = pkg.ReadInt();
		for (int i = 0; i < num; i++)
		{
			Vector3 key = pkg.ReadVector3();
			string value = pkg.ReadString();
			m_locationIcons[key] = value;
		}
		ZLog.Log((object)("Icons:" + num));
	}

	private void OnNewPeer(long peerID)
	{
		if (ZNet.instance.IsServer())
		{
			ZLog.Log((object)"Server: New peer connected,sending global keys");
			SendGlobalKeys(peerID);
			SendLocationIcons(peerID);
		}
	}

	private void SetupLocations()
	{
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		List<LocationList> allLocationLists = LocationList.GetAllLocationLists();
		allLocationLists.Sort((LocationList a, LocationList b) => a.m_sortOrder.CompareTo(b.m_sortOrder));
		foreach (LocationList item in allLocationLists)
		{
			m_locations.AddRange(item.m_locations);
			m_vegetation.AddRange(item.m_vegetation);
			foreach (EnvSetup environment in item.m_environments)
			{
				EnvMan.instance.AppendEnvironment(environment);
			}
			foreach (BiomeEnvSetup biomeEnvironment in item.m_biomeEnvironments)
			{
				EnvMan.instance.AppendBiomeSetup(biomeEnvironment);
			}
			ClutterSystem.instance.m_clutter.AddRange(item.m_clutter);
			string text = $"Added {item.m_locations.Count} locations, {item.m_vegetation.Count} vegetations, {item.m_environments.Count} environments, {item.m_biomeEnvironments.Count} biome env-setups, {item.m_clutter.Count} clutter  from ";
			Scene scene = ((Component)item).gameObject.scene;
			ZLog.Log((object)(text + ((Scene)(ref scene)).name));
			RandEventSystem.instance.m_events.AddRange(item.m_events);
		}
		foreach (ZoneLocation location in m_locations)
		{
			if ((location.m_enable || location.m_prefab.IsValid) && Application.isPlaying)
			{
				location.m_prefabName = location.m_prefab.Name;
				int hash = location.Hash;
				if (!m_locationsByHash.ContainsKey(hash))
				{
					m_locationsByHash.Add(hash, location);
				}
			}
		}
		if (!Settings.AssetMemoryUsagePolicy.HasFlag(AssetMemoryUsagePolicy.KeepAsynchronousLoadedBit))
		{
			return;
		}
		ReferenceHolder val = ((Component)this).gameObject.AddComponent<ReferenceHolder>();
		foreach (ZoneLocation location2 in m_locations)
		{
			if (location2.m_enable)
			{
				location2.m_prefab.Load();
				val.HoldReferenceTo((IReferenceCounted)(object)location2.m_prefab);
				location2.m_prefab.Release();
			}
		}
	}

	public static void PrepareNetViews(GameObject root, List<ZNetView> views)
	{
		views.Clear();
		ZNetView[] componentsInChildren = root.GetComponentsInChildren<ZNetView>(true);
		foreach (ZNetView zNetView in componentsInChildren)
		{
			if (Utils.IsEnabledInheirarcy(((Component)zNetView).gameObject, root))
			{
				views.Add(zNetView);
			}
		}
	}

	public static void PrepareRandomSpawns(GameObject root, List<RandomSpawn> randomSpawns)
	{
		randomSpawns.Clear();
		RandomSpawn[] componentsInChildren = root.GetComponentsInChildren<RandomSpawn>(true);
		foreach (RandomSpawn randomSpawn in componentsInChildren)
		{
			if (Utils.IsEnabledInheirarcy(((Component)randomSpawn).gameObject, root))
			{
				randomSpawns.Add(randomSpawn);
				randomSpawn.Prepare();
			}
		}
	}

	private void OnDestroy()
	{
		ForceReleaseLoadedPrefabs();
		m_instance = null;
	}

	private void ValidateVegetation()
	{
		foreach (ZoneVegetation item in m_vegetation)
		{
			if (item.m_enable && Object.op_Implicit((Object)(object)item.m_prefab) && (Object)(object)item.m_prefab.GetComponent<ZNetView>() == (Object)null)
			{
				ZLog.LogError((object)("Vegetation " + ((Object)item.m_prefab).name + " [ " + item.m_name + "] is missing ZNetView"));
			}
		}
	}

	public void PrepareSave()
	{
		m_tempGeneratedZonesSaveClone = new HashSet<Vector2i>(m_generatedZones);
		m_tempGlobalKeysSaveClone = new HashSet<string>(m_globalKeys);
		m_tempLocationsSaveClone = new List<LocationInstance>(m_locationInstances.Values);
		m_tempLocationsGeneratedSaveClone = LocationsGenerated;
	}

	public void SaveASync(BinaryWriter writer)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(m_tempGeneratedZonesSaveClone.Count);
		foreach (Vector2i item in m_tempGeneratedZonesSaveClone)
		{
			writer.Write(item.x);
			writer.Write(item.y);
		}
		writer.Write(0);
		writer.Write(m_locationVersion);
		m_tempGlobalKeysSaveClone.RemoveWhere(delegate(string x)
		{
			GetKeyValue(x, out var _, out var gk);
			return gk < GlobalKeys.NonServerOption;
		});
		writer.Write(m_tempGlobalKeysSaveClone.Count);
		foreach (string item2 in m_tempGlobalKeysSaveClone)
		{
			writer.Write(item2);
		}
		writer.Write(m_tempLocationsGeneratedSaveClone);
		writer.Write(m_tempLocationsSaveClone.Count);
		foreach (LocationInstance item3 in m_tempLocationsSaveClone)
		{
			writer.Write(item3.m_location.m_prefabName);
			writer.Write(item3.m_position.x);
			writer.Write(item3.m_position.y);
			writer.Write(item3.m_position.z);
			writer.Write(item3.m_placed);
		}
		m_tempGeneratedZonesSaveClone.Clear();
		m_tempGeneratedZonesSaveClone = null;
		m_tempGlobalKeysSaveClone.Clear();
		m_tempGlobalKeysSaveClone = null;
		m_tempLocationsSaveClone.Clear();
		m_tempLocationsSaveClone = null;
	}

	public void Load(BinaryReader reader, int version)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		m_generatedZones.Clear();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Vector2i item = default(Vector2i);
			item.x = reader.ReadInt32();
			item.y = reader.ReadInt32();
			m_generatedZones.Add(item);
		}
		if (version < 13)
		{
			return;
		}
		reader.ReadInt32();
		int num2 = ((version >= 21) ? reader.ReadInt32() : 0);
		if (version >= 14)
		{
			ClearGlobalKeys();
			int num3 = reader.ReadInt32();
			for (int j = 0; j < num3; j++)
			{
				string keyStr = reader.ReadString();
				GlobalKeyAdd(keyStr);
			}
		}
		if (version < 18)
		{
			return;
		}
		if (version >= 20)
		{
			LocationsGenerated = reader.ReadBoolean();
		}
		m_locationInstances.Clear();
		int num4 = reader.ReadInt32();
		for (int k = 0; k < num4; k++)
		{
			string text = reader.ReadString();
			Vector3 zero = Vector3.zero;
			zero.x = reader.ReadSingle();
			zero.y = reader.ReadSingle();
			zero.z = reader.ReadSingle();
			bool generated = false;
			if (version >= 19)
			{
				generated = reader.ReadBoolean();
			}
			ZoneLocation location = GetLocation(text);
			if (location != null)
			{
				RegisterLocation(location, zero, generated);
			}
			else
			{
				ZLog.DevLog((object)("Failed to find location " + text));
			}
		}
		ZLog.Log((object)("Loaded " + num4 + " locations"));
		if (num2 != m_locationVersion)
		{
			LocationsGenerated = false;
		}
	}

	private void Update()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		m_lastFixedTime = Time.fixedTime;
		if (ZNet.GetConnectionStatus() != ZNet.ConnectionStatus.Connected)
		{
			return;
		}
		if (ZNet.instance.IsServer() && !LocationsGenerated)
		{
			if (TextViewer.IsShowingIntro())
			{
				m_timeSlicedGenerationTimeBudget = GetGenerationTimeBudgetForTargetFrameRate(out var _);
			}
			else
			{
				m_timeSlicedGenerationTimeBudget = 0.1f;
			}
			return;
		}
		if (Terminal.m_showTests)
		{
			Terminal.m_testList["Time"] = Time.fixedTime.ToString("0.00") + " / " + TimeSinceStart().ToString("0.00");
		}
		m_updateTimer += Time.deltaTime;
		if (!(m_updateTimer > 0.1f))
		{
			return;
		}
		m_updateTimer = 0f;
		bool flag = CreateLocalZones(ZNet.instance.GetReferencePosition());
		UpdateTTL(0.1f);
		if (ZNet.instance.IsServer() && !flag)
		{
			CreateGhostZones(ZNet.instance.GetReferencePosition());
			foreach (ZNetPeer peer in ZNet.instance.GetPeers())
			{
				CreateGhostZones(peer.GetRefPos());
			}
		}
		UpdatePrefabLifetimes();
	}

	private void UpdatePrefabLifetimes()
	{
		for (int i = 0; i < m_locationPrefabs.Count; i++)
		{
			m_locationPrefabs[i].m_iterationLifetime--;
			if (m_locationPrefabs[i].m_iterationLifetime <= 0)
			{
				m_tempLocationPrefabsToRelease.Add(i);
			}
		}
		for (int num = m_tempLocationPrefabsToRelease.Count - 1; num >= 0; num--)
		{
			int index = m_tempLocationPrefabsToRelease[num];
			m_locationPrefabs[index].Release();
			m_locationPrefabs.RemoveAt(index);
		}
		m_tempLocationPrefabsToRelease.Clear();
	}

	private void ForceReleaseLoadedPrefabs()
	{
		foreach (LocationPrefabLoadData locationPrefab in m_locationPrefabs)
		{
			locationPrefab.Release();
		}
		m_locationPrefabs.Clear();
	}

	private bool CreateGhostZones(Vector3 refPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone = GetZone(refPoint);
		if (!IsZoneGenerated(zone) && SpawnZone(zone, SpawnMode.Ghost, out var _))
		{
			return true;
		}
		int num = m_activeArea + m_activeDistantArea;
		Vector2i zoneID = default(Vector2i);
		for (int i = zone.y - num; i <= zone.y + num; i++)
		{
			for (int j = zone.x - num; j <= zone.x + num; j++)
			{
				((Vector2i)(ref zoneID))._002Ector(j, i);
				if (!IsZoneGenerated(zoneID) && SpawnZone(zoneID, SpawnMode.Ghost, out var _))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CreateLocalZones(Vector3 refPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone = GetZone(refPoint);
		if (PokeLocalZone(zone))
		{
			return true;
		}
		Vector2i val = default(Vector2i);
		for (int i = zone.y - m_activeArea; i <= zone.y + m_activeArea; i++)
		{
			for (int j = zone.x - m_activeArea; j <= zone.x + m_activeArea; j++)
			{
				((Vector2i)(ref val))._002Ector(j, i);
				if (!(val == zone) && PokeLocalZone(val))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool PokeLocalZone(Vector2i zoneID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (m_zones.TryGetValue(zoneID, out var value))
		{
			value.m_ttl = 0f;
			return false;
		}
		SpawnMode mode = ((!ZNet.instance.IsServer() || IsZoneGenerated(zoneID)) ? SpawnMode.Client : SpawnMode.Full);
		if (SpawnZone(zoneID, mode, out var root))
		{
			ZoneData zoneData = new ZoneData();
			zoneData.m_root = root;
			m_zones.Add(zoneID, zoneData);
			return true;
		}
		return false;
	}

	public bool IsZoneLoaded(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone = GetZone(point);
		return IsZoneLoaded(zone);
	}

	public bool IsZoneLoaded(Vector2i zoneID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (m_zones.ContainsKey(zoneID))
		{
			return !m_loadingObjectsInZones.ContainsKey(zoneID);
		}
		return false;
	}

	public bool IsActiveAreaLoaded()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Vector2i zone = GetZone(ZNet.instance.GetReferencePosition());
		for (int i = zone.y - m_activeArea; i <= zone.y + m_activeArea; i++)
		{
			for (int j = zone.x - m_activeArea; j <= zone.x + m_activeArea; j++)
			{
				if (!m_zones.ContainsKey(new Vector2i(j, i)))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool SpawnZone(Vector2i zoneID, SpawnMode mode, out GameObject root)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zonePos = GetZonePos(zoneID);
		Heightmap componentInChildren = m_zonePrefab.GetComponentInChildren<Heightmap>();
		if (!HeightmapBuilder.instance.IsTerrainReady(zonePos, componentInChildren.m_width, componentInChildren.m_scale, componentInChildren.IsDistantLod, WorldGenerator.instance))
		{
			root = null;
			return false;
		}
		if (m_locationInstances.TryGetValue(zoneID, out var value) && !value.m_placed && !PokeCanSpawnLocation(value.m_location, isFirstSpawn: true))
		{
			root = null;
			return false;
		}
		root = Object.Instantiate<GameObject>(m_zonePrefab, zonePos, Quaternion.identity);
		if ((mode == SpawnMode.Ghost || mode == SpawnMode.Full) && !IsZoneGenerated(zoneID))
		{
			Heightmap componentInChildren2 = root.GetComponentInChildren<Heightmap>();
			m_tempClearAreas.Clear();
			m_tempSpawnedObjects.Clear();
			PlaceLocations(zoneID, zonePos, root.transform, componentInChildren2, m_tempClearAreas, mode, m_tempSpawnedObjects);
			PlaceVegetation(zoneID, zonePos, root.transform, componentInChildren2, m_tempClearAreas, mode, m_tempSpawnedObjects);
			PlaceZoneCtrl(zoneID, zonePos, mode, m_tempSpawnedObjects);
			if (mode == SpawnMode.Ghost)
			{
				foreach (GameObject tempSpawnedObject in m_tempSpawnedObjects)
				{
					Object.Destroy((Object)(object)tempSpawnedObject);
				}
				m_tempSpawnedObjects.Clear();
				Object.Destroy((Object)(object)root);
				root = null;
			}
			SetZoneGenerated(zoneID);
		}
		return true;
	}

	private void PlaceZoneCtrl(Vector2i zoneID, Vector3 zoneCenterPos, SpawnMode mode, List<GameObject> spawnedObjects)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (mode == SpawnMode.Full || mode == SpawnMode.Ghost)
		{
			if (mode == SpawnMode.Ghost)
			{
				ZNetView.StartGhostInit();
			}
			GameObject val = Object.Instantiate<GameObject>(m_zoneCtrlPrefab, zoneCenterPos, Quaternion.identity);
			val.GetComponent<ZNetView>();
			if (mode == SpawnMode.Ghost)
			{
				spawnedObjects.Add(val);
				ZNetView.FinishGhostInit();
			}
		}
	}

	private Vector3 GetRandomPointInRadius(Vector3 center, float radius)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.value * (float)Math.PI * 2f;
		float num2 = Random.Range(0f, radius);
		return center + new Vector3(Mathf.Sin(num) * num2, 0f, Mathf.Cos(num) * num2);
	}

	private void PlaceVegetation(Vector2i zoneID, Vector3 zoneCenterPos, Transform parent, Heightmap hmap, List<ClearArea> clearAreas, SpawnMode mode, List<GameObject> spawnedObjects)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_052a: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_055b: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0631: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_05aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05db: Unknown result type (might be due to invalid IL or missing references)
		State state = Random.state;
		int seed = WorldGenerator.instance.GetSeed();
		int num = 1;
		Vector3 val = default(Vector3);
		foreach (ZoneVegetation item in m_vegetation)
		{
			num++;
			if (!item.m_enable || !hmap.HaveBiome(item.m_biome))
			{
				continue;
			}
			Random.InitState(seed + zoneID.x * 4271 + zoneID.y * 9187 + StringExtensionMethods.GetStableHashCode(((Object)item.m_prefab).name));
			int num2 = 1;
			if (item.m_max < 1f)
			{
				if (Random.value > item.m_max)
				{
					continue;
				}
			}
			else
			{
				num2 = Random.Range((int)item.m_min, (int)item.m_max + 1);
			}
			bool flag = (Object)(object)item.m_prefab.GetComponent<ZNetView>() != (Object)null;
			float num3 = Mathf.Cos((float)Math.PI / 180f * item.m_maxTilt);
			float num4 = Mathf.Cos((float)Math.PI / 180f * item.m_minTilt);
			float num5 = 32f - item.m_groupRadius;
			s_tempVeg.Clear();
			int num6 = (item.m_forcePlacement ? (num2 * 50) : num2);
			int num7 = 0;
			for (int i = 0; i < num6; i++)
			{
				((Vector3)(ref val))._002Ector(Random.Range(zoneCenterPos.x - num5, zoneCenterPos.x + num5), 0f, Random.Range(zoneCenterPos.z - num5, zoneCenterPos.z + num5));
				int num8 = Random.Range(item.m_groupSizeMin, item.m_groupSizeMax + 1);
				bool flag2 = false;
				for (int j = 0; j < num8; j++)
				{
					Vector3 p = ((j == 0) ? val : GetRandomPointInRadius(val, item.m_groupRadius));
					float num9 = Random.Range(0, 360);
					float num10 = Random.Range(item.m_scaleMin, item.m_scaleMax);
					float num11 = Random.Range(0f - item.m_randTilt, item.m_randTilt);
					float num12 = Random.Range(0f - item.m_randTilt, item.m_randTilt);
					if (item.m_blockCheck && IsBlocked(p))
					{
						continue;
					}
					GetGroundData(ref p, out var normal, out var biome, out var biomeArea, out var hmap2);
					if ((item.m_biome & biome) == 0 || (item.m_biomeArea & biomeArea) == 0)
					{
						continue;
					}
					if (item.m_snapToStaticSolid && GetStaticSolidHeight(p, out var height, out var normal2))
					{
						p.y = height;
						normal = normal2;
					}
					float num13 = p.y - 30f;
					if (num13 < item.m_minAltitude || num13 > item.m_maxAltitude)
					{
						continue;
					}
					if (item.m_minVegetation != item.m_maxVegetation)
					{
						float vegetationMask = hmap2.GetVegetationMask(p);
						if (vegetationMask > item.m_maxVegetation || vegetationMask < item.m_minVegetation)
						{
							continue;
						}
					}
					if (item.m_minOceanDepth != item.m_maxOceanDepth)
					{
						float oceanDepth = hmap2.GetOceanDepth(p);
						if (oceanDepth < item.m_minOceanDepth || oceanDepth > item.m_maxOceanDepth)
						{
							continue;
						}
					}
					if (normal.y < num3 || normal.y > num4)
					{
						continue;
					}
					if (item.m_terrainDeltaRadius > 0f)
					{
						GetTerrainDelta(p, item.m_terrainDeltaRadius, out var delta, out var _);
						if (delta > item.m_maxTerrainDelta || delta < item.m_minTerrainDelta)
						{
							continue;
						}
					}
					if (item.m_minDistanceFromCenter > 0f || item.m_maxDistanceFromCenter > 0f)
					{
						float num14 = Utils.LengthXZ(p);
						if ((item.m_minDistanceFromCenter > 0f && num14 < item.m_minDistanceFromCenter) || (item.m_maxDistanceFromCenter > 0f && num14 > item.m_maxDistanceFromCenter))
						{
							continue;
						}
					}
					if (item.m_inForest)
					{
						float forestFactor = WorldGenerator.GetForestFactor(p);
						if (forestFactor < item.m_forestTresholdMin || forestFactor > item.m_forestTresholdMax)
						{
							continue;
						}
					}
					if (item.m_surroundCheckVegetation)
					{
						float num15 = 0f;
						for (int k = 0; k < item.m_surroundCheckLayers; k++)
						{
							float num16 = (float)(k + 1) / (float)item.m_surroundCheckLayers * item.m_surroundCheckDistance;
							for (int l = 0; l < 6; l++)
							{
								float num17 = (float)l / 6f * (float)Math.PI * 2f;
								float vegetationMask2 = hmap2.GetVegetationMask(p + new Vector3(Mathf.Sin(num17) * num16, 0f, Mathf.Cos(num17) * num16));
								float num18 = (1f - num16) / (item.m_surroundCheckDistance * 2f);
								num15 += vegetationMask2 * num18;
							}
						}
						s_tempVeg.Add(num15);
						if (s_tempVeg.Count < 10)
						{
							continue;
						}
						float num19 = s_tempVeg.Max();
						float num20 = s_tempVeg.Average();
						float num21 = num20 + (num19 - num20) * item.m_surroundBetterThanAverage;
						if (num15 < num21)
						{
							continue;
						}
					}
					if (InsideClearArea(clearAreas, p))
					{
						continue;
					}
					if (item.m_snapToWater)
					{
						p.y = 30f;
					}
					p.y += item.m_groundOffset;
					Quaternion identity = Quaternion.identity;
					if (item.m_chanceToUseGroundTilt > 0f && Random.value <= item.m_chanceToUseGroundTilt)
					{
						Quaternion val2 = Quaternion.Euler(0f, num9, 0f);
						identity = Quaternion.LookRotation(Vector3.Cross(normal, val2 * Vector3.forward), normal);
					}
					else
					{
						identity = Quaternion.Euler(num11, num9, num12);
					}
					if (flag)
					{
						if (mode == SpawnMode.Full || mode == SpawnMode.Ghost)
						{
							if (mode == SpawnMode.Ghost)
							{
								ZNetView.StartGhostInit();
							}
							GameObject val3 = Object.Instantiate<GameObject>(item.m_prefab, p, identity);
							ZNetView component = val3.GetComponent<ZNetView>();
							if (num10 != val3.transform.localScale.x)
							{
								component.SetLocalScale(new Vector3(num10, num10, num10));
								Collider[] componentsInChildren = val3.GetComponentsInChildren<Collider>();
								foreach (Collider obj in componentsInChildren)
								{
									obj.enabled = false;
									obj.enabled = true;
								}
							}
							if (mode == SpawnMode.Ghost)
							{
								spawnedObjects.Add(val3);
								ZNetView.FinishGhostInit();
							}
						}
					}
					else
					{
						GameObject obj2 = Object.Instantiate<GameObject>(item.m_prefab, p, identity);
						obj2.transform.localScale = new Vector3(num10, num10, num10);
						obj2.transform.SetParent(parent, true);
					}
					flag2 = true;
				}
				if (flag2)
				{
					num7++;
				}
				if (num7 >= num2)
				{
					break;
				}
			}
		}
		Random.state = state;
	}

	private bool InsideClearArea(List<ClearArea> areas, Vector3 point)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		foreach (ClearArea area in areas)
		{
			if (point.x > area.m_center.x - area.m_radius && point.x < area.m_center.x + area.m_radius && point.z > area.m_center.z - area.m_radius && point.z < area.m_center.z + area.m_radius)
			{
				return true;
			}
		}
		return false;
	}

	private ZoneLocation GetLocation(int hash)
	{
		if (m_locationsByHash.TryGetValue(hash, out var value))
		{
			return value;
		}
		return null;
	}

	private ZoneLocation GetLocation(string name)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		foreach (ZoneLocation location in m_locations)
		{
			if (!location.m_prefab.IsValid)
			{
				if (location.m_enable)
				{
					throw new NullReferenceException($"Location in list of locations was invalid! Asset: {location.m_prefab.m_assetID}");
				}
			}
			else if (location.m_prefab.Name == name)
			{
				return location;
			}
		}
		return null;
	}

	private void ClearNonPlacedLocations()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<Vector2i, LocationInstance> dictionary = new Dictionary<Vector2i, LocationInstance>();
		foreach (KeyValuePair<Vector2i, LocationInstance> locationInstance in m_locationInstances)
		{
			if (locationInstance.Value.m_placed)
			{
				dictionary.Add(locationInstance.Key, locationInstance.Value);
			}
		}
		m_locationInstances = dictionary;
	}

	private void CheckLocationDuplicates()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)"Checking for location duplicates");
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int i = 0; i < m_locations.Count; i++)
		{
			ZoneLocation zoneLocation = m_locations[i];
			if (!zoneLocation.m_enable)
			{
				continue;
			}
			for (int j = i + 1; j < m_locations.Count; j++)
			{
				ZoneLocation zoneLocation2 = m_locations[j];
				if (zoneLocation2.m_enable)
				{
					if (zoneLocation.m_prefab.Name == zoneLocation2.m_prefab.Name)
					{
						SoftReference<GameObject> prefab = zoneLocation.m_prefab;
						ZLog.LogWarning((object)("Two locations have the same location prefab name " + ((object)prefab).ToString()));
					}
					if (zoneLocation.m_prefab == zoneLocation2.m_prefab)
					{
						ZLog.LogWarning((object)$"Locations {zoneLocation.m_prefab} and {zoneLocation2.m_prefab} point to the same location prefab");
					}
				}
			}
		}
		stopwatch.Stop();
		ZLog.Log((object)$"Location duplicate check took {stopwatch.Elapsed.TotalMilliseconds} ms");
	}

	public void GenerateLocations()
	{
		if (m_generateLocationsCoroutine == null)
		{
			if (!Application.isPlaying)
			{
				ZLog.Log((object)"Setting up locations");
				SetupLocations();
			}
			m_generateLocationsCoroutine = ((MonoBehaviour)this).StartCoroutine(GenerateLocationsTimeSliced());
		}
	}

	private IEnumerator GenerateLocationsTimeSliced()
	{
		m_estimatedGenerateLocationsCompletionTime = DateTime.MaxValue;
		yield return null;
		Stopwatch timeSliceStopwatch = Stopwatch.StartNew();
		ZLog.Log((object)"Generating locations");
		DateTime startTime = DateTime.UtcNow;
		ClearNonPlacedLocations();
		List<ZoneLocation> ordered = m_locations.OrderByDescending((ZoneLocation a) => a.m_prioritized).ToList();
		int totalEstimatedIterationsLeft = 0;
		for (int num = ordered.Count - 1; num >= 0; num--)
		{
			ZoneLocation zoneLocation = ordered[num];
			if (!zoneLocation.m_enable || zoneLocation.m_quantity == 0)
			{
				ordered.RemoveAt(num);
			}
			else
			{
				totalEstimatedIterationsLeft += (zoneLocation.m_prioritized ? 200000 : 100000) * 20 / 2;
			}
		}
		int runIterations = 0;
		for (int i = 0; i < ordered.Count; i++)
		{
			ZoneLocation location = ordered[i];
			if (timeSliceStopwatch.Elapsed.TotalSeconds >= (double)m_timeSlicedGenerationTimeBudget)
			{
				yield return null;
				timeSliceStopwatch.Restart();
			}
			ZPackage iterationsPkg = new ZPackage();
			yield return GenerateLocationsTimeSliced(location, timeSliceStopwatch, iterationsPkg);
			runIterations += iterationsPkg.ReadInt();
			totalEstimatedIterationsLeft -= (location.m_prioritized ? 200000 : 100000) * 20 / 2;
			DateTime utcNow = DateTime.UtcNow;
			double totalSeconds = (utcNow - startTime).TotalSeconds;
			double num2 = ((runIterations != 0) ? (totalSeconds / (double)runIterations) : double.MaxValue);
			double num3 = num2 * (double)totalEstimatedIterationsLeft;
			if (double.IsInfinity(num3))
			{
				m_estimatedGenerateLocationsCompletionTime = DateTime.MaxValue;
			}
			else
			{
				m_estimatedGenerateLocationsCompletionTime = utcNow.AddSeconds(num3);
			}
			m_generateLocationsProgress = (float)(i + 1) / (float)ordered.Count;
		}
		LocationsGenerated = true;
		ZLog.Log((object)(" Done generating locations, duration:" + (DateTime.UtcNow - startTime).TotalMilliseconds + " ms"));
		m_generateLocationsCoroutine = null;
	}

	private int CountNrOfLocation(ZoneLocation location)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (LocationInstance value in m_locationInstances.Values)
		{
			if (value.m_location.m_prefab.Name == location.m_prefab.Name)
			{
				num++;
			}
		}
		if (num > 0)
		{
			SoftReference<GameObject> prefab = location.m_prefab;
			ZLog.Log((object)("Old location found " + ((object)prefab).ToString() + " x " + num));
		}
		return num;
	}

	private IEnumerator GenerateLocationsTimeSliced(ZoneLocation location, Stopwatch timeSliceStopwatch, ZPackage iterationsPkg)
	{
		DateTime t = DateTime.Now;
		int num = WorldGenerator.instance.GetSeed() + StringExtensionMethods.GetStableHashCode(location.m_prefab.Name);
		State state = Random.state;
		Random.InitState(num);
		int errorLocationInZone = 0;
		int errorCenterDistance = 0;
		int errorBiome = 0;
		int errorBiomeArea = 0;
		int errorAlt = 0;
		int errorForest = 0;
		int errorSimilar = 0;
		int errorNotSimilar = 0;
		int errorTerrainDelta = 0;
		int errorVegetation = 0;
		float maxRadius = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
		int attempts = (location.m_prioritized ? 200000 : 100000);
		int iterations = 0;
		int placed = CountNrOfLocation(location);
		float maxRange = 10000f;
		if (location.m_centerFirst)
		{
			maxRange = location.m_minDistance;
		}
		if (!location.m_unique || placed <= 0)
		{
			s_tempVeg.Clear();
			int i = 0;
			while (i < attempts && placed < location.m_quantity)
			{
				if (timeSliceStopwatch.Elapsed.TotalSeconds >= (double)m_timeSlicedGenerationTimeBudget)
				{
					State insideState2 = Random.state;
					Random.state = state;
					yield return null;
					timeSliceStopwatch.Restart();
					state = Random.state;
					Random.state = insideState2;
				}
				Vector2i zoneID = GetRandomZone(maxRange);
				if (location.m_centerFirst)
				{
					maxRange += 1f;
				}
				int num2;
				if (m_locationInstances.ContainsKey(zoneID))
				{
					num2 = errorLocationInZone + 1;
					errorLocationInZone = num2;
				}
				else if (!IsZoneGenerated(zoneID))
				{
					Vector3 zonePos = GetZonePos(zoneID);
					Heightmap.BiomeArea biomeArea = WorldGenerator.instance.GetBiomeArea(zonePos);
					if ((location.m_biomeArea & biomeArea) == 0)
					{
						num2 = errorBiomeArea + 1;
						errorBiomeArea = num2;
					}
					else
					{
						for (int j = 0; j < 20; num2 = j + 1, j = num2)
						{
							if (timeSliceStopwatch.Elapsed.TotalSeconds >= (double)m_timeSlicedGenerationTimeBudget)
							{
								State insideState2 = Random.state;
								Random.state = state;
								yield return null;
								timeSliceStopwatch.Restart();
								state = Random.state;
								Random.state = insideState2;
							}
							num2 = iterations + 1;
							iterations = num2;
							Vector3 randomPointInZone = GetRandomPointInZone(zoneID, maxRadius);
							float magnitude = ((Vector3)(ref randomPointInZone)).magnitude;
							if (location.m_minDistance != 0f && magnitude < location.m_minDistance)
							{
								num2 = errorCenterDistance + 1;
								errorCenterDistance = num2;
								continue;
							}
							if (location.m_maxDistance != 0f && magnitude > location.m_maxDistance)
							{
								num2 = errorCenterDistance + 1;
								errorCenterDistance = num2;
								continue;
							}
							Heightmap.Biome biome = WorldGenerator.instance.GetBiome(randomPointInZone);
							if ((location.m_biome & biome) == 0)
							{
								num2 = errorBiome + 1;
								errorBiome = num2;
								continue;
							}
							randomPointInZone.y = WorldGenerator.instance.GetHeight(randomPointInZone.x, randomPointInZone.z, out var mask);
							float num3 = (float)((double)randomPointInZone.y - 30.0);
							if (num3 < location.m_minAltitude || num3 > location.m_maxAltitude)
							{
								num2 = errorAlt + 1;
								errorAlt = num2;
								continue;
							}
							if (location.m_inForest)
							{
								float forestFactor = WorldGenerator.GetForestFactor(randomPointInZone);
								if (forestFactor < location.m_forestTresholdMin || forestFactor > location.m_forestTresholdMax)
								{
									num2 = errorForest + 1;
									errorForest = num2;
									continue;
								}
							}
							if (location.m_minDistanceFromCenter > 0f || location.m_maxDistanceFromCenter > 0f)
							{
								float num4 = Utils.LengthXZ(randomPointInZone);
								if ((location.m_minDistanceFromCenter > 0f && num4 < location.m_minDistanceFromCenter) || (location.m_maxDistanceFromCenter > 0f && num4 > location.m_maxDistanceFromCenter))
								{
									continue;
								}
							}
							WorldGenerator.instance.GetTerrainDelta(randomPointInZone, location.m_exteriorRadius, out var delta, out var _);
							if (delta > location.m_maxTerrainDelta || delta < location.m_minTerrainDelta)
							{
								num2 = errorTerrainDelta + 1;
								errorTerrainDelta = num2;
								continue;
							}
							if (location.m_minDistanceFromSimilar > 0f && HaveLocationInRange(location.m_prefab.Name, location.m_group, randomPointInZone, location.m_minDistanceFromSimilar))
							{
								num2 = errorSimilar + 1;
								errorSimilar = num2;
								continue;
							}
							if (location.m_maxDistanceFromSimilar > 0f && !HaveLocationInRange(location.m_prefabName, location.m_groupMax, randomPointInZone, location.m_maxDistanceFromSimilar, maxGroup: true))
							{
								num2 = errorNotSimilar + 1;
								errorNotSimilar = num2;
								continue;
							}
							float a = mask.a;
							if (location.m_minimumVegetation > 0f && a <= location.m_minimumVegetation)
							{
								num2 = errorVegetation + 1;
								errorVegetation = num2;
								continue;
							}
							if (location.m_maximumVegetation < 1f && a >= location.m_maximumVegetation)
							{
								num2 = errorVegetation + 1;
								errorVegetation = num2;
								continue;
							}
							if (location.m_surroundCheckVegetation)
							{
								float num5 = 0f;
								for (int k = 0; k < location.m_surroundCheckLayers; k++)
								{
									float num6 = (float)(k + 1) / (float)location.m_surroundCheckLayers * location.m_surroundCheckDistance;
									for (int l = 0; l < 6; l++)
									{
										float num7 = (float)l / 6f * (float)Math.PI * 2f;
										Vector3 val = randomPointInZone + new Vector3(Mathf.Sin(num7) * num6, 0f, Mathf.Cos(num7) * num6);
										WorldGenerator.instance.GetHeight(val.x, val.z, out var mask2);
										float num8 = (location.m_surroundCheckDistance - num6) / (location.m_surroundCheckDistance * 2f);
										num5 += mask2.a * num8;
									}
								}
								s_tempVeg.Add(num5);
								if (s_tempVeg.Count < 10)
								{
									continue;
								}
								float num9 = s_tempVeg.Max();
								float num10 = s_tempVeg.Average();
								float num11 = num10 + (num9 - num10) * location.m_surroundBetterThanAverage;
								if (num5 < num11)
								{
									continue;
								}
								ZLog.DevLog((object)$"Surround check passed with a value of {num5}, cutoff was {num11}, max: {num9}, average: {num10}.");
							}
							RegisterLocation(location, randomPointInZone, generated: false);
							num2 = placed + 1;
							placed = num2;
							break;
						}
					}
				}
				num2 = i + 1;
				i = num2;
			}
			if (placed < location.m_quantity)
			{
				ZLog.LogWarning((object)("Failed to place all " + location.m_prefab.Name + ", placed " + placed + " out of " + location.m_quantity));
				ZLog.DevLog((object)("errorLocationInZone " + errorLocationInZone));
				ZLog.DevLog((object)("errorCenterDistance " + errorCenterDistance));
				ZLog.DevLog((object)("errorBiome " + errorBiome));
				ZLog.DevLog((object)("errorBiomeArea " + errorBiomeArea));
				ZLog.DevLog((object)("errorAlt " + errorAlt));
				ZLog.DevLog((object)("errorForest " + errorForest));
				ZLog.DevLog((object)("errorSimilar " + errorSimilar));
				ZLog.DevLog((object)("errorNotSimilar " + errorNotSimilar));
				ZLog.DevLog((object)("errorTerrainDelta " + errorTerrainDelta));
				ZLog.DevLog((object)("errorVegetation " + errorVegetation));
			}
		}
		Random.state = state;
		_ = DateTime.Now - t;
		iterationsPkg.Write(iterations);
		iterationsPkg.SetPos(0);
	}

	public float GetEstimatedGenerationCompletionTimeFromNow()
	{
		if (m_generateLocationsCoroutine == null)
		{
			return 0f;
		}
		DateTime utcNow = DateTime.UtcNow;
		return (float)(m_estimatedGenerateLocationsCompletionTime - utcNow).TotalSeconds;
	}

	public static float GetGenerationTimeBudgetForTargetFrameRate(out float targetFrameTime)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		float num2;
		if (QualitySettings.vSyncCount > 0)
		{
			Resolution currentResolution = Screen.currentResolution;
			float num = ((Resolution)(ref currentResolution)).refreshRateRatio.denominator;
			currentResolution = Screen.currentResolution;
			num2 = num / (float)((Resolution)(ref currentResolution)).refreshRateRatio.numerator * (float)QualitySettings.vSyncCount;
		}
		else
		{
			num2 = 1f / (float)Application.targetFrameRate;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
		}
		targetFrameTime = Mathf.Clamp(num2, 1f / 60f, 1f / 30f);
		float num3 = 1f / 150f;
		return targetFrameTime - num3;
	}

	private static Vector2i GetRandomZone(float range)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)range / 64;
		Vector2i val = default(Vector2i);
		Vector3 zonePos;
		do
		{
			((Vector2i)(ref val))._002Ector(Random.Range(-num, num), Random.Range(-num, num));
			zonePos = GetZonePos(val);
		}
		while (!(((Vector3)(ref zonePos)).magnitude < 10000f));
		return val;
	}

	private static Vector3 GetRandomPointInZone(Vector2i zone, float locationRadius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zonePos = GetZonePos(zone);
		float num = Random.Range(-32f + locationRadius, 32f - locationRadius);
		float num2 = Random.Range(-32f + locationRadius, 32f - locationRadius);
		return zonePos + new Vector3(num, 0f, num2);
	}

	private static Vector3 GetRandomPointInZone(float locationRadius)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zonePos = GetZonePos(GetZone(new Vector3(Random.Range(-10000f, 10000f), 0f, Random.Range(-10000f, 10000f))));
		return new Vector3(Random.Range(zonePos.x - 32f + locationRadius, zonePos.x + 32f - locationRadius), 0f, Random.Range(zonePos.z - 32f + locationRadius, zonePos.z + 32f - locationRadius));
	}

	private void PlaceLocations(Vector2i zoneID, Vector3 zoneCenterPos, Transform parent, Heightmap hmap, List<ClearArea> clearAreas, SpawnMode mode, List<GameObject> spawnedObjects)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		DateTime now = DateTime.Now;
		if (m_locationInstances.TryGetValue(zoneID, out var value) && !value.m_placed)
		{
			Vector3 p = value.m_position;
			GetGroundData(ref p, out var _, out var _, out var _, out var _);
			if (value.m_location.m_snapToWater)
			{
				p.y = 30f;
			}
			if (value.m_location.m_clearArea)
			{
				ClearArea item = new ClearArea(p, value.m_location.m_exteriorRadius);
				clearAreas.Add(item);
			}
			Quaternion rot = Quaternion.identity;
			if (value.m_location.m_slopeRotation)
			{
				GetTerrainDelta(p, value.m_location.m_exteriorRadius, out var _, out var slopeDirection);
				Vector3 val = default(Vector3);
				((Vector3)(ref val))._002Ector(slopeDirection.x, 0f, slopeDirection.z);
				((Vector3)(ref val)).Normalize();
				rot = Quaternion.LookRotation(val);
				Vector3 eulerAngles = ((Quaternion)(ref rot)).eulerAngles;
				eulerAngles.y = Mathf.Round(eulerAngles.y / 22.5f) * 22.5f;
				((Quaternion)(ref rot)).eulerAngles = eulerAngles;
			}
			else if (value.m_location.m_randomRotation)
			{
				rot = Quaternion.Euler(0f, (float)Random.Range(0, 16) * 22.5f, 0f);
			}
			int seed = WorldGenerator.instance.GetSeed() + zoneID.x * 4271 + zoneID.y * 9187;
			SpawnLocation(value.m_location, seed, p, rot, mode, spawnedObjects);
			value.m_placed = true;
			m_locationInstances[zoneID] = value;
			TimeSpan timeSpan = DateTime.Now - now;
			string[] obj = new string[5] { "Placed locations in zone ", null, null, null, null };
			Vector2i val2 = zoneID;
			obj[1] = ((object)(Vector2i)(ref val2)).ToString();
			obj[2] = "  duration ";
			obj[3] = timeSpan.TotalMilliseconds.ToString();
			obj[4] = " ms";
			ZLog.Log((object)string.Concat(obj));
			if (value.m_location.m_unique)
			{
				RemoveUnplacedLocations(value.m_location);
			}
			if (value.m_location.m_iconPlaced)
			{
				SendLocationIcons(ZRoutedRpc.Everybody);
			}
		}
	}

	private void RemoveUnplacedLocations(ZoneLocation location)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		List<Vector2i> list = new List<Vector2i>();
		foreach (KeyValuePair<Vector2i, LocationInstance> locationInstance in m_locationInstances)
		{
			if (locationInstance.Value.m_location == location && !locationInstance.Value.m_placed)
			{
				list.Add(locationInstance.Key);
			}
		}
		foreach (Vector2i item in list)
		{
			m_locationInstances.Remove(item);
		}
		ZLog.DevLog((object)("Removed " + list.Count + " unplaced locations of type " + location.m_prefab.Name));
	}

	public bool TestSpawnLocation(string name, Vector3 pos, bool disableSave = true)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		if (!ZNet.instance.IsServer())
		{
			return false;
		}
		ZoneLocation location = GetLocation(name);
		if (location == null)
		{
			ZLog.Log((object)("Missing location:" + name));
			Console.instance.Print("Missing location:" + name);
			return false;
		}
		if (!location.m_prefab.IsValid)
		{
			ZLog.Log((object)("Missing prefab in location:" + name));
			Console.instance.Print("Missing location:" + name);
			return false;
		}
		float num = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
		Vector3 zonePos = GetZonePos(GetZone(pos));
		pos.x = Mathf.Clamp(pos.x, zonePos.x - 32f + num, zonePos.x + 32f - num);
		pos.z = Mathf.Clamp(pos.z, zonePos.z - 32f + num, zonePos.z + 32f - num);
		string[] obj = new string[6]
		{
			"radius ",
			num.ToString(),
			"  ",
			null,
			null,
			null
		};
		Vector3 val = zonePos;
		obj[3] = ((object)(Vector3)(ref val)).ToString();
		obj[4] = " ";
		val = pos;
		obj[5] = ((object)(Vector3)(ref val)).ToString();
		ZLog.Log((object)string.Concat(obj));
		MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Location spawned, " + (disableSave ? "world saving DISABLED until restart" : "CAUTION! world saving is ENABLED, use normal location command to disable it!"));
		m_didZoneTest = disableSave;
		float num2 = (float)Random.Range(0, 16) * 22.5f;
		List<GameObject> spawnedGhostObjects = new List<GameObject>();
		SpawnLocation(location, Random.Range(0, 99999), pos, Quaternion.Euler(0f, num2, 0f), SpawnMode.Full, spawnedGhostObjects);
		return true;
	}

	private bool PokeCanSpawnLocation(ZoneLocation location, bool isFirstSpawn)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		LocationPrefabLoadData locationPrefabLoadData = null;
		for (int i = 0; i < m_locationPrefabs.Count; i++)
		{
			if (m_locationPrefabs[i].PrefabAssetID == location.m_prefab.m_assetID)
			{
				locationPrefabLoadData = m_locationPrefabs[i];
				break;
			}
		}
		if (locationPrefabLoadData == null)
		{
			locationPrefabLoadData = new LocationPrefabLoadData(location.m_prefab, isFirstSpawn);
			m_locationPrefabs.Add(locationPrefabLoadData);
		}
		locationPrefabLoadData.m_iterationLifetime = GetLocationPrefabLifetime();
		return locationPrefabLoadData.IsLoaded;
	}

	public int GetLocationPrefabLifetime()
	{
		int num = 2 * (m_activeArea + m_activeDistantArea) + 1;
		int num2 = num * num;
		int num3 = ((!ZNet.instance.IsServer()) ? 1 : (ZNet.instance.GetPeers().Count + 1));
		return num2 * num3;
	}

	public bool ShouldDelayProxyLocationSpawning(int hash)
	{
		ZoneLocation location = GetLocation(hash);
		if (location == null)
		{
			ZLog.LogWarning((object)("Missing location:" + hash));
			return false;
		}
		return !PokeCanSpawnLocation(location, isFirstSpawn: false);
	}

	public GameObject SpawnProxyLocation(int hash, int seed, Vector3 pos, Quaternion rot)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		ZoneLocation location = GetLocation(hash);
		if (location == null)
		{
			ZLog.LogWarning((object)("Missing location:" + hash));
			return null;
		}
		List<GameObject> spawnedGhostObjects = new List<GameObject>();
		return SpawnLocation(location, seed, pos, rot, SpawnMode.Client, spawnedGhostObjects);
	}

	private GameObject SpawnLocation(ZoneLocation location, int seed, Vector3 pos, Quaternion rot, SpawnMode mode, List<GameObject> spawnedGhostObjects)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0577: Unknown result type (might be due to invalid IL or missing references)
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		location.m_prefab.Load();
		ZNetView[] enabledComponentsInChildren = Utils.GetEnabledComponentsInChildren<ZNetView>(location.m_prefab.Asset);
		RandomSpawn[] enabledComponentsInChildren2 = Utils.GetEnabledComponentsInChildren<RandomSpawn>(location.m_prefab.Asset);
		for (int i = 0; i < enabledComponentsInChildren2.Length; i++)
		{
			enabledComponentsInChildren2[i].Prepare();
		}
		Location component = location.m_prefab.Asset.GetComponent<Location>();
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.zero;
		if (Object.op_Implicit((Object)(object)component.m_interiorTransform) && Object.op_Implicit((Object)(object)component.m_generator))
		{
			val = component.m_interiorTransform.localPosition;
			val2 = ((Component)component.m_generator).transform.localPosition;
		}
		Vector3 position = location.m_prefab.Asset.transform.position;
		Quaternion rotation = location.m_prefab.Asset.transform.rotation;
		location.m_prefab.Asset.transform.position = Vector3.zero;
		location.m_prefab.Asset.transform.rotation = Quaternion.identity;
		Random.InitState(seed);
		bool flag = Object.op_Implicit((Object)(object)component) && component.m_useCustomInteriorTransform && Object.op_Implicit((Object)(object)component.m_interiorTransform) && Object.op_Implicit((Object)(object)component.m_generator);
		Vector3 localPosition = Vector3.zero;
		Vector3 localPosition2 = Vector3.zero;
		Quaternion localRotation = Quaternion.identity;
		if (flag)
		{
			localPosition = ((Component)component.m_generator).transform.localPosition;
			localPosition2 = component.m_interiorTransform.localPosition;
			localRotation = component.m_interiorTransform.localRotation;
			Vector3 zonePos = GetZonePos(GetZone(pos));
			((Component)component.m_generator).transform.localPosition = Vector3.zero;
			Vector3 val3 = zonePos + val + val2 - pos;
			Matrix4x4 val4 = Matrix4x4.Rotate(Quaternion.Inverse(rot)) * Matrix4x4.Translate(val3);
			Vector3 localPosition3 = Vector4.op_Implicit(((Matrix4x4)(ref val4)).GetColumn(3));
			localPosition3.y = component.m_interiorTransform.localPosition.y;
			component.m_interiorTransform.localPosition = localPosition3;
			component.m_interiorTransform.localRotation = Quaternion.Inverse(rot);
		}
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.m_generator) && component.m_useCustomInteriorTransform != component.m_generator.m_useCustomInteriorTransform)
		{
			ZLog.LogWarning((object)(((Object)component).name + " & " + ((Object)component.m_generator).name + " don't have matching m_useCustomInteriorTransform()! If one has it the other should as well!"));
		}
		GameObject val5 = null;
		if (mode == SpawnMode.Full || mode == SpawnMode.Ghost)
		{
			Random.InitState(seed);
			RandomSpawn[] array = enabledComponentsInChildren2;
			foreach (RandomSpawn obj in array)
			{
				Vector3 position2 = ((Component)obj).gameObject.transform.position;
				Vector3 pos2 = pos + rot * position2;
				obj.Randomize(pos2, component);
			}
			WearNTear.m_randomInitialDamage = component.m_applyRandomDamage;
			ZNetView[] array2 = enabledComponentsInChildren;
			foreach (ZNetView zNetView in array2)
			{
				if (!((Component)zNetView).gameObject.activeSelf)
				{
					continue;
				}
				Vector3 position3 = ((Component)zNetView).gameObject.transform.position;
				Vector3 val6 = pos + rot * position3;
				Quaternion rotation2 = ((Component)zNetView).gameObject.transform.rotation;
				Quaternion val7 = rot * rotation2;
				if (mode == SpawnMode.Ghost)
				{
					ZNetView.StartGhostInit();
				}
				GameObject val8 = Object.Instantiate<GameObject>(((Component)zNetView).gameObject, val6, val7);
				GameObjectExtentions.HoldReferenceTo(val8, location.m_prefab);
				DungeonGenerator component2 = val8.GetComponent<DungeonGenerator>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					if (flag)
					{
						component2.m_originalPosition = val2;
					}
					component2.Generate(mode);
				}
				if (mode == SpawnMode.Ghost)
				{
					spawnedGhostObjects.Add(val8);
					ZNetView.FinishGhostInit();
				}
			}
			WearNTear.m_randomInitialDamage = false;
			array = enabledComponentsInChildren2;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Reset();
			}
			array2 = enabledComponentsInChildren;
			for (int j = 0; j < array2.Length; j++)
			{
				((Component)array2[j]).gameObject.SetActive(true);
			}
			location.m_prefab.Asset.transform.position = position;
			location.m_prefab.Asset.transform.rotation = rotation;
			if (flag)
			{
				((Component)component.m_generator).transform.localPosition = localPosition;
				component.m_interiorTransform.localPosition = localPosition2;
				component.m_interiorTransform.localRotation = localRotation;
			}
			CreateLocationProxy(location, seed, pos, rot, mode, spawnedGhostObjects);
		}
		else
		{
			Random.InitState(seed);
			RandomSpawn[] array = enabledComponentsInChildren2;
			foreach (RandomSpawn obj2 in array)
			{
				Vector3 position4 = ((Component)obj2).gameObject.transform.position;
				Vector3 pos3 = pos + rot * position4;
				obj2.Randomize(pos3, component);
			}
			ZNetView[] array2 = enabledComponentsInChildren;
			for (int j = 0; j < array2.Length; j++)
			{
				((Component)array2[j]).gameObject.SetActive(false);
			}
			val5 = Utils.Instantiate(location.m_prefab, pos, rot);
			val5.SetActive(true);
			array = enabledComponentsInChildren2;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Reset();
			}
			array2 = enabledComponentsInChildren;
			for (int j = 0; j < array2.Length; j++)
			{
				((Component)array2[j]).gameObject.SetActive(true);
			}
			location.m_prefab.Asset.transform.position = position;
			location.m_prefab.Asset.transform.rotation = rotation;
			if (flag)
			{
				((Component)component.m_generator).transform.localPosition = localPosition;
				component.m_interiorTransform.localPosition = localPosition2;
				component.m_interiorTransform.localRotation = localRotation;
			}
		}
		location.m_prefab.Release();
		SnapToGround.SnappAll();
		return val5;
	}

	private void CreateLocationProxy(ZoneLocation location, int seed, Vector3 pos, Quaternion rotation, SpawnMode mode, List<GameObject> spawnedGhostObjects)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (mode == SpawnMode.Ghost)
		{
			ZNetView.StartGhostInit();
		}
		GameObject val = Object.Instantiate<GameObject>(m_locationProxyPrefab, pos, rotation);
		LocationProxy component = val.GetComponent<LocationProxy>();
		bool spawnNow = mode == SpawnMode.Full;
		component.SetLocation(location.m_prefab.Name, seed, spawnNow);
		if (mode == SpawnMode.Ghost)
		{
			spawnedGhostObjects.Add(val);
			ZNetView.FinishGhostInit();
		}
	}

	private void RegisterLocation(ZoneLocation location, Vector3 pos, bool generated)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		LocationInstance value = default(LocationInstance);
		value.m_location = location;
		value.m_position = pos;
		value.m_placed = generated;
		Vector2i zone = GetZone(pos);
		if (m_locationInstances.ContainsKey(zone))
		{
			Vector2i val = zone;
			ZLog.LogWarning((object)("Location already exist in zone " + ((object)(Vector2i)(ref val)).ToString()));
		}
		else
		{
			m_locationInstances.Add(zone, value);
		}
	}

	private bool HaveLocationInRange(string prefabName, string group, Vector3 p, float radius, bool maxGroup = false)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		foreach (LocationInstance value in m_locationInstances.Values)
		{
			if ((value.m_location.m_prefab.Name == prefabName || (!maxGroup && group.Length > 0 && group == value.m_location.m_group) || (maxGroup && group.Length > 0 && group == value.m_location.m_groupMax)) && Vector3.Distance(value.m_position, p) < radius)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetLocationIcon(string name, out Vector3 pos)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (ZNet.instance.IsServer())
		{
			foreach (KeyValuePair<Vector2i, LocationInstance> locationInstance in m_locationInstances)
			{
				if ((locationInstance.Value.m_location.m_iconAlways || (locationInstance.Value.m_location.m_iconPlaced && locationInstance.Value.m_placed)) && locationInstance.Value.m_location.m_prefab.Name == name)
				{
					pos = locationInstance.Value.m_position;
					return true;
				}
			}
		}
		else
		{
			foreach (KeyValuePair<Vector3, string> locationIcon in m_locationIcons)
			{
				if (locationIcon.Value == name)
				{
					pos = locationIcon.Key;
					return true;
				}
			}
		}
		pos = Vector3.zero;
		return false;
	}

	public void GetLocationIcons(Dictionary<Vector3, string> icons)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (ZNet.instance.IsServer())
		{
			foreach (LocationInstance value in m_locationInstances.Values)
			{
				if (value.m_location.m_iconAlways || (value.m_location.m_iconPlaced && value.m_placed))
				{
					icons[value.m_position] = value.m_location.m_prefab.Name;
				}
			}
			return;
		}
		foreach (KeyValuePair<Vector3, string> locationIcon in m_locationIcons)
		{
			icons.Add(locationIcon.Key, locationIcon.Value);
		}
	}

	private void GetTerrainDelta(Vector3 center, float radius, out float delta, out Vector3 slopeDirection)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		int num = 10;
		float num2 = -999999f;
		float num3 = 999999f;
		Vector3 val = center;
		Vector3 val2 = center;
		for (int i = 0; i < num; i++)
		{
			Vector2 val3 = Random.insideUnitCircle * radius;
			Vector3 val4 = center + new Vector3(val3.x, 0f, val3.y);
			float groundHeight = GetGroundHeight(val4);
			if (groundHeight < num3)
			{
				num3 = groundHeight;
				val2 = val4;
			}
			if (groundHeight > num2)
			{
				num2 = groundHeight;
				val = val4;
			}
		}
		delta = num2 - num3;
		slopeDirection = Vector3.Normalize(val2 - val);
	}

	public bool IsBlocked(Vector3 p)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		p.y += 2000f;
		if (Physics.Raycast(p, Vector3.down, 10000f, m_blockRayMask))
		{
			return true;
		}
		return false;
	}

	public float GetGroundHeight(Vector3 p)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = p;
		val.y = 6000f;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(val, Vector3.down, ref val2, 10000f, m_terrainRayMask))
		{
			return ((RaycastHit)(ref val2)).point.y;
		}
		return p.y;
	}

	public bool GetGroundHeight(Vector3 p, out float height)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		p.y = 6000f;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(p, Vector3.down, ref val, 10000f, m_terrainRayMask))
		{
			height = ((RaycastHit)(ref val)).point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	public float GetSolidHeight(Vector3 p)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = p;
		val.y += 1000f;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(val, Vector3.down, ref val2, 2000f, m_solidRayMask))
		{
			return ((RaycastHit)(ref val2)).point.y;
		}
		return p.y;
	}

	public bool GetSolidHeight(Vector3 p, out float height, int heightMargin = 1000)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		p.y += heightMargin;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(p, Vector3.down, ref val, 2000f, m_solidRayMask) && !Object.op_Implicit((Object)(object)((RaycastHit)(ref val)).collider.attachedRigidbody))
		{
			height = ((RaycastHit)(ref val)).point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	public bool GetSolidHeight(Vector3 p, float radius, out float height, Transform ignore)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		height = p.y - 1000f;
		p.y += 1000f;
		int num = ((!(radius <= 0f)) ? Physics.SphereCastNonAlloc(p, radius, Vector3.down, rayHits, 2000f, m_solidRayMask) : Physics.RaycastNonAlloc(p, Vector3.down, rayHits, 2000f, m_solidRayMask));
		s_rayHits.Clear();
		s_rayHitsHeight.Clear();
		for (int i = 0; i < num; i++)
		{
			RaycastHit val = rayHits[i];
			float y = ((RaycastHit)(ref val)).point.y;
			Utils.InsertSortNoAlloc<RaycastHit>(s_rayHits, val, s_rayHitsHeight, y);
		}
		for (int num2 = s_rayHits.Count - 1; num2 >= 0; num2--)
		{
			RaycastHit val2 = rayHits[num2];
			Collider collider = ((RaycastHit)(ref val2)).collider;
			if (!((Object)(object)collider.attachedRigidbody != (Object)null) && (!((Object)(object)ignore != (Object)null) || !Utils.IsParent(((Component)collider).transform, ignore)))
			{
				height = Mathf.Max(height, s_rayHitsHeight[num2]);
				return true;
			}
		}
		return false;
	}

	public bool GetSolidHeight(Vector3 p, out float height, out Vector3 normal, out GameObject go)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		p.y += 1000f;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(p, Vector3.down, ref val, 2000f, m_solidRayMask) && !Object.op_Implicit((Object)(object)((RaycastHit)(ref val)).collider.attachedRigidbody))
		{
			height = ((RaycastHit)(ref val)).point.y;
			normal = ((RaycastHit)(ref val)).normal;
			go = ((Component)((RaycastHit)(ref val)).collider).gameObject;
			return true;
		}
		height = 0f;
		normal = Vector3.zero;
		go = null;
		return false;
	}

	public bool GetStaticSolidHeight(Vector3 p, out float height, out Vector3 normal)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		p.y += 1000f;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(p, Vector3.down, ref val, 2000f, m_staticSolidRayMask) && !Object.op_Implicit((Object)(object)((RaycastHit)(ref val)).collider.attachedRigidbody))
		{
			height = ((RaycastHit)(ref val)).point.y;
			normal = ((RaycastHit)(ref val)).normal;
			return true;
		}
		height = 0f;
		normal = Vector3.zero;
		return false;
	}

	public bool FindFloor(Vector3 p, out float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(p + Vector3.up * 1f, Vector3.down, ref val, 1000f, m_solidRayMask))
		{
			height = ((RaycastHit)(ref val)).point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	public float GetGroundOffset(Vector3 position)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		GetGroundData(ref position, out var _, out var _, out var _, out var hmap);
		if (Object.op_Implicit((Object)(object)hmap))
		{
			return hmap.GetHeightOffset(position);
		}
		return 0f;
	}

	public static bool IsLavaPreHeightmap(Vector3 position, float lavaValue = 0.6f)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (WorldGenerator.instance.GetBiome(position.x, position.z) != Heightmap.Biome.AshLands)
		{
			return false;
		}
		WorldGenerator.instance.GetBiomeHeight(Heightmap.Biome.AshLands, position.x, position.z, out var mask);
		return mask.a > lavaValue;
	}

	public bool IsLava(Vector3 position, bool defaultTrue = false)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		GetGroundData(ref position, out var _, out var _, out var _, out var hmap);
		if (!Object.op_Implicit((Object)(object)hmap))
		{
			return defaultTrue;
		}
		return hmap.IsLava(position);
	}

	public bool IsLava(ref Vector3 position, bool defaultTrue = false)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		GetGroundData(ref position, out var _, out var _, out var _, out var hmap);
		if (!Object.op_Implicit((Object)(object)hmap))
		{
			return defaultTrue;
		}
		return hmap.IsLava(position);
	}

	public void GetGroundData(ref Vector3 p, out Vector3 normal, out Heightmap.Biome biome, out Heightmap.BiomeArea biomeArea, out Heightmap hmap)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		biome = Heightmap.Biome.None;
		biomeArea = Heightmap.BiomeArea.Everything;
		hmap = null;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(p + Vector3.up * 5000f, Vector3.down, ref val, 10000f, m_terrainRayMask))
		{
			p.y = ((RaycastHit)(ref val)).point.y;
			normal = ((RaycastHit)(ref val)).normal;
			Heightmap component = ((Component)((RaycastHit)(ref val)).collider).GetComponent<Heightmap>();
			if (Object.op_Implicit((Object)(object)component))
			{
				biome = component.GetBiome(((RaycastHit)(ref val)).point);
				biomeArea = component.GetBiomeArea();
				hmap = component;
			}
		}
		else
		{
			normal = Vector3.up;
		}
	}

	private void UpdateTTL(float dt)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Vector2i, ZoneData> zone in m_zones)
		{
			zone.Value.m_ttl += dt;
		}
		foreach (KeyValuePair<Vector2i, ZoneData> zone2 in m_zones)
		{
			if (zone2.Value.m_ttl > m_zoneTTL && !ZNetScene.instance.HaveInstanceInSector(zone2.Key))
			{
				Object.Destroy((Object)(object)zone2.Value.m_root);
				m_zones.Remove(zone2.Key);
				break;
			}
		}
	}

	public bool FindClosestLocation(string name, Vector3 point, out LocationInstance closest)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		float num = 999999f;
		closest = default(LocationInstance);
		bool result = false;
		foreach (LocationInstance value in m_locationInstances.Values)
		{
			float num2 = Vector3.Distance(value.m_position, point);
			if (value.m_location.m_prefab.Name == name && num2 < num)
			{
				num = num2;
				closest = value;
				result = true;
			}
		}
		return result;
	}

	public bool FindLocations(string name, ref List<LocationInstance> locations)
	{
		locations.Clear();
		foreach (LocationInstance value in m_locationInstances.Values)
		{
			if (value.m_location.m_prefab.Name == name)
			{
				locations.Add(value);
			}
		}
		return locations.Count > 0;
	}

	public static Vector2i GetZone(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		int num = Utils.FloorToInt((float)(((double)point.x + 32.0) / 64.0));
		int num2 = Utils.FloorToInt((float)(((double)point.z + 32.0) / 64.0));
		return new Vector2i(num, num2);
	}

	public static Vector3 GetZonePos(Vector2i id)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3((float)((double)id.x * 64.0), 0f, (float)((double)id.y * 64.0));
	}

	private void SetZoneGenerated(Vector2i zoneID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_generatedZones.Add(zoneID);
	}

	private bool IsZoneGenerated(Vector2i zoneID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return m_generatedZones.Contains(zoneID);
	}

	public bool IsZoneReadyForType(Vector2i zoneID, ZDO.ObjectType objectType)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (m_loadingObjectsInZones.Count <= 0)
		{
			return true;
		}
		if (!m_loadingObjectsInZones.ContainsKey(zoneID))
		{
			return true;
		}
		foreach (ZDO item in m_loadingObjectsInZones[zoneID])
		{
			if ((int)objectType < (int)item.Type)
			{
				return false;
			}
		}
		return true;
	}

	public void SetLoadingInZone(ZDO zdo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector2i sector = zdo.GetSector();
		if (m_loadingObjectsInZones.ContainsKey(sector))
		{
			m_loadingObjectsInZones[sector].Add(zdo);
			return;
		}
		List<ZDO> list = new List<ZDO>();
		list.Add(zdo);
		m_loadingObjectsInZones.Add(sector, list);
	}

	public void UnsetLoadingInZone(ZDO zdo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector2i sector = zdo.GetSector();
		m_loadingObjectsInZones[sector].Remove(zdo);
		if (m_loadingObjectsInZones[sector].Count <= 0)
		{
			m_loadingObjectsInZones.Remove(sector);
		}
	}

	public bool SkipSaving()
	{
		if (!m_error)
		{
			return m_didZoneTest;
		}
		return true;
	}

	public float TimeSinceStart()
	{
		return m_lastFixedTime - m_startTime;
	}

	public void ResetGlobalKeys()
	{
		ClearGlobalKeys();
		SetStartingGlobalKeys(send: false);
		SendGlobalKeys(ZRoutedRpc.Everybody);
	}

	public void ResetWorldKeys()
	{
		for (int i = 0; i < 32; i++)
		{
			GlobalKeys globalKeys = (GlobalKeys)i;
			RemoveGlobalKey(globalKeys.ToString());
		}
	}

	public void SetStartingGlobalKeys(bool send = true)
	{
		for (int i = 0; i < 32; i++)
		{
			GlobalKeys globalKeys = (GlobalKeys)i;
			GlobalKeyRemove(globalKeys.ToString(), canSaveToServerOptionKeys: false);
		}
		string text = null;
		m_tempKeys.Clear();
		m_tempKeys.AddRange(ZNet.World.m_startingGlobalKeys);
		foreach (string tempKey in m_tempKeys)
		{
			GetKeyValue(tempKey.ToLower(), out var value, out var gk);
			if (gk == GlobalKeys.Preset)
			{
				text = value;
			}
			GlobalKeyAdd(tempKey, canSaveToServerOptionKeys: false);
		}
		if (text != null)
		{
			ServerOptionsGUI.m_instance.SetPreset(ZNet.World, text);
		}
		if (send)
		{
			SendGlobalKeys(ZRoutedRpc.Everybody);
		}
	}

	public void SetGlobalKey(GlobalKeys key, float value)
	{
		SetGlobalKey($"{key} {value.ToString(CultureInfo.InvariantCulture)}");
	}

	public void SetGlobalKey(GlobalKeys key)
	{
		SetGlobalKey(key.ToString());
	}

	public void SetGlobalKey(string name)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC("SetGlobalKey", name);
	}

	public bool GetGlobalKey(GlobalKeys key)
	{
		return m_globalKeysEnums.Contains(key);
	}

	public bool GetGlobalKey(GlobalKeys key, out string value)
	{
		return m_globalKeysValues.TryGetValue(key.ToString().ToLower(), out value);
	}

	public bool GetGlobalKey(GlobalKeys key, out float value)
	{
		if (m_globalKeysValues.TryGetValue(key.ToString().ToLower(), out var value2) && float.TryParse(value2, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
		{
			return true;
		}
		value = 0f;
		return false;
	}

	public bool GetGlobalKey(string name)
	{
		string value;
		return GetGlobalKey(name, out value);
	}

	public bool GetGlobalKey(string name, out string value)
	{
		return m_globalKeysValues.TryGetValue(name.ToLower(), out value);
	}

	public bool GetGlobalKeyExact(string fullLine)
	{
		return m_globalKeys.Contains(fullLine);
	}

	public bool CheckKey(string key, GameKeyType type = GameKeyType.Global, bool trueWhenKeySet = true)
	{
		switch (type)
		{
		case GameKeyType.Global:
			return instance.GetGlobalKey(key) == trueWhenKeySet;
		case GameKeyType.Player:
			if (Object.op_Implicit((Object)(object)Player.m_localPlayer))
			{
				return Player.m_localPlayer.HaveUniqueKey(key) == trueWhenKeySet;
			}
			return false;
		default:
			ZLog.LogError((object)"Unknown GameKeyType type");
			return false;
		}
	}

	private void RPC_SetGlobalKey(long sender, string name)
	{
		if (!m_globalKeys.Contains(name))
		{
			GlobalKeyAdd(name);
			SendGlobalKeys(ZRoutedRpc.Everybody);
		}
	}

	public void RemoveGlobalKey(GlobalKeys key)
	{
		RemoveGlobalKey(key.ToString());
	}

	public void RemoveGlobalKey(string name)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC("RemoveGlobalKey", name);
	}

	private void RPC_RemoveGlobalKey(long sender, string name)
	{
		if (GlobalKeyRemove(name))
		{
			SendGlobalKeys(ZRoutedRpc.Everybody);
		}
	}

	public List<string> GetGlobalKeys()
	{
		return new List<string>(m_globalKeys);
	}

	public Dictionary<Vector2i, LocationInstance>.ValueCollection GetLocationList()
	{
		return m_locationInstances.Values;
	}
}
