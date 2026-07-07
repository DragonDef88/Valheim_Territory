using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoftReferenceableAssets;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
	[Serializable]
	public class DoorDef
	{
		public GameObject m_prefab;

		public string m_connectionType = "";

		[Tooltip("Will use default door chance set in DungeonGenerator if set to zero to default to old behaviour")]
		[Range(0f, 1f)]
		public float m_chance;
	}

	private struct RoomPlacementData
	{
		public DungeonDB.RoomData m_roomData;

		public Vector3 m_position;

		public Quaternion m_rotation;

		public RoomPlacementData(DungeonDB.RoomData roomData, Vector3 position, Quaternion rotation)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			m_roomData = roomData;
			m_position = position;
			m_rotation = rotation;
		}
	}

	public enum Algorithm
	{
		Dungeon,
		CampGrid,
		CampRadial
	}

	private static MemoryStream saveStream = new MemoryStream();

	private static BinaryWriter saveWriter = new BinaryWriter(saveStream);

	public static int m_forceSeed = int.MinValue;

	public Algorithm m_algorithm;

	public int m_maxRooms = 3;

	public int m_minRooms = 20;

	public int m_minRequiredRooms;

	public List<string> m_requiredRooms = new List<string>();

	[Tooltip("Rooms and endcaps will be placed using weights.")]
	public bool m_alternativeFunctionality;

	[BitMask(typeof(Room.Theme))]
	public Room.Theme m_themes = Room.Theme.Crypt;

	[Header("Dungeon")]
	public List<DoorDef> m_doorTypes = new List<DoorDef>();

	[Range(0f, 1f)]
	public float m_doorChance = 0.5f;

	[Header("Camp")]
	public float m_maxTilt = 10f;

	public float m_tileWidth = 8f;

	public int m_gridSize = 4;

	public float m_spawnChance = 1f;

	[Header("Camp radial")]
	public float m_campRadiusMin = 15f;

	public float m_campRadiusMax = 30f;

	public float m_minAltitude = 1f;

	public int m_perimeterSections;

	public float m_perimeterBuffer = 2f;

	[Header("Misc")]
	public Vector3 m_zoneCenter = new Vector3(0f, 0f, 0f);

	public Vector3 m_zoneSize = new Vector3(64f, 64f, 64f);

	[Tooltip("Makes the dungeon entrance start at the given interior transform (including rotation) rather than straight above the entrance, which gives the dungeon much more room to fill out the entire zone. Must use together with Location.m_useCustomInteriorTransform to make sure seeds are deterministic.")]
	public bool m_useCustomInteriorTransform;

	[HideInInspector]
	public int m_generatedSeed;

	private bool m_hasGeneratedSeed;

	private ZDO m_zdoSetToBeLoadingInZone;

	private int m_roomsToLoad;

	private RoomPlacementData[] m_loadedRooms;

	private List<IReferenceCounted> m_heldReferences = new List<IReferenceCounted>();

	private static List<Room> m_placedRooms = new List<Room>();

	private static List<RoomConnection> m_openConnections = new List<RoomConnection>();

	private static List<RoomConnection> m_doorConnections = new List<RoomConnection>();

	private static List<DungeonDB.RoomData> m_availableRooms = new List<DungeonDB.RoomData>();

	private static List<DungeonDB.RoomData> m_tempRooms = new List<DungeonDB.RoomData>();

	private BoxCollider m_colliderA;

	private BoxCollider m_colliderB;

	private ZNetView m_nview;

	[HideInInspector]
	public Vector3 m_originalPosition;

	private void Awake()
	{
		m_nview = ((Component)this).GetComponent<ZNetView>();
		Load();
		if (m_loadedRooms.Length != 0)
		{
			LoadRoomPrefabsAsync();
		}
	}

	private void OnDestroy()
	{
		ReleaseHeldReferences();
	}

	private void ReleaseHeldReferences()
	{
		for (int i = 0; i < m_heldReferences.Count; i++)
		{
			m_heldReferences[i].Release();
		}
		m_heldReferences.Clear();
		if (m_zdoSetToBeLoadingInZone != null)
		{
			ZoneSystem.instance.UnsetLoadingInZone(m_zdoSetToBeLoadingInZone);
			m_zdoSetToBeLoadingInZone = null;
		}
	}

	public void Clear()
	{
		while (((Component)this).transform.childCount > 0)
		{
			Object.DestroyImmediate((Object)(object)((Component)((Component)this).transform.GetChild(0)).gameObject);
		}
	}

	public void Generate(ZoneSystem.SpawnMode mode)
	{
		int seed = GetSeed();
		Generate(seed, mode);
	}

	public int GetSeed()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (m_hasGeneratedSeed)
		{
			return m_generatedSeed;
		}
		if (m_forceSeed != int.MinValue)
		{
			m_generatedSeed = m_forceSeed;
			m_forceSeed = int.MinValue;
		}
		else
		{
			int seed = WorldGenerator.instance.GetSeed();
			Vector3 position = ((Component)this).transform.position;
			Vector2i zone = ZoneSystem.GetZone(((Component)this).transform.position);
			m_generatedSeed = seed + zone.x * 4271 + zone.y * -7187 + (int)position.x * -4271 + (int)position.y * 9187 + (int)position.z * -2134;
		}
		m_hasGeneratedSeed = true;
		return m_generatedSeed;
	}

	public void Generate(int seed, ZoneSystem.SpawnMode mode)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		DateTime now = DateTime.Now;
		m_generatedSeed = seed;
		Clear();
		SetupColliders();
		SetupAvailableRooms();
		for (int i = 0; i < m_availableRooms.Count; i++)
		{
			m_availableRooms[i].m_prefab.Load();
		}
		if (Object.op_Implicit((Object)(object)ZoneSystem.instance))
		{
			Vector2i zone = ZoneSystem.GetZone(((Component)this).transform.position);
			m_zoneCenter = ZoneSystem.GetZonePos(zone);
			m_zoneCenter.y = ((Component)this).transform.position.y - m_originalPosition.y;
		}
		Bounds val = default(Bounds);
		((Bounds)(ref val))._002Ector(m_zoneCenter, m_zoneSize);
		ZLog.Log((object)$"Generating {((Object)this).name}, Seed: {seed}, Bounds diff: {((Bounds)(ref val)).min - ((Component)this).transform.position} / {((Bounds)(ref val)).max - ((Component)this).transform.position}");
		ZLog.Log((object)("Available rooms:" + m_availableRooms.Count));
		ZLog.Log((object)("To place:" + m_maxRooms));
		m_placedRooms.Clear();
		m_openConnections.Clear();
		m_doorConnections.Clear();
		State state = Random.state;
		Random.InitState(seed);
		GenerateRooms(mode);
		for (int j = 0; j < m_availableRooms.Count; j++)
		{
			m_availableRooms[j].m_prefab.Release();
		}
		Save();
		ZLog.Log((object)("Placed " + m_placedRooms.Count + " rooms"));
		Random.state = state;
		SnapToGround.SnappAll();
		if (mode == ZoneSystem.SpawnMode.Ghost)
		{
			foreach (Room placedRoom in m_placedRooms)
			{
				Object.DestroyImmediate((Object)(object)((Component)placedRoom).gameObject);
			}
		}
		Object.DestroyImmediate((Object)(object)m_colliderA);
		Object.DestroyImmediate((Object)(object)m_colliderB);
		m_placedRooms.Clear();
		m_openConnections.Clear();
		m_doorConnections.Clear();
		_ = DateTime.Now - now;
	}

	private void LoadRoomPrefabsAsync()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		ZLog.Log((object)"Loading room prefabs asynchronously");
		if (m_zdoSetToBeLoadingInZone == null)
		{
			m_zdoSetToBeLoadingInZone = m_nview.GetZDO();
			ZoneSystem.instance.SetLoadingInZone(m_zdoSetToBeLoadingInZone);
		}
		m_roomsToLoad = m_loadedRooms.Length;
		int num = m_loadedRooms.Length;
		for (int i = 0; i < num; i++)
		{
			m_heldReferences.Add((IReferenceCounted)(object)m_loadedRooms[i].m_roomData.m_prefab);
			m_loadedRooms[i].m_roomData.m_prefab.LoadAsync(new LoadedHandler(OnRoomLoaded));
		}
	}

	private void OnRoomLoaded(AssetID assetID, LoadResult result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)result == 0 && !((Object)(object)this == (Object)null) && !((Object)(object)((Component)this).gameObject == (Object)null))
		{
			m_roomsToLoad--;
			if (m_roomsToLoad <= 0)
			{
				Spawn();
				ReleaseHeldReferences();
			}
		}
	}

	private void Spawn()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)"Spawning dungeon");
		for (int i = 0; i < m_loadedRooms.Length; i++)
		{
			PlaceRoom(m_loadedRooms[i].m_roomData, m_loadedRooms[i].m_position, m_loadedRooms[i].m_rotation, null, ZoneSystem.SpawnMode.Client);
		}
		SnapToGround.SnappAll();
		m_loadedRooms = null;
	}

	private void GenerateRooms(ZoneSystem.SpawnMode mode)
	{
		switch (m_algorithm)
		{
		case Algorithm.Dungeon:
			GenerateDungeon(mode);
			break;
		case Algorithm.CampGrid:
			GenerateCampGrid(mode);
			break;
		case Algorithm.CampRadial:
			GenerateCampRadial(mode);
			break;
		}
	}

	private void GenerateDungeon(ZoneSystem.SpawnMode mode)
	{
		PlaceStartRoom(mode);
		PlaceRooms(mode);
		PlaceEndCaps(mode);
		PlaceDoors(mode);
	}

	private void GenerateCampGrid(ZoneSystem.SpawnMode mode)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Cos((float)Math.PI / 180f * m_maxTilt);
		Vector3 val = ((Component)this).transform.position + new Vector3((float)(-m_gridSize) * m_tileWidth * 0.5f, 0f, (float)(-m_gridSize) * m_tileWidth * 0.5f);
		for (int i = 0; i < m_gridSize; i++)
		{
			for (int j = 0; j < m_gridSize; j++)
			{
				if (Random.value > m_spawnChance)
				{
					continue;
				}
				Vector3 p = val + new Vector3((float)j * m_tileWidth, 0f, (float)i * m_tileWidth);
				DungeonDB.RoomData randomWeightedRoom = GetRandomWeightedRoom(perimeterRoom: false);
				if (randomWeightedRoom == null)
				{
					continue;
				}
				if (Object.op_Implicit((Object)(object)ZoneSystem.instance))
				{
					ZoneSystem.instance.GetGroundData(ref p, out var normal, out var _, out var _, out var _);
					if (normal.y < num)
					{
						continue;
					}
				}
				Quaternion rot = Quaternion.Euler(0f, (float)Random.Range(0, 16) * 22.5f, 0f);
				PlaceRoom(randomWeightedRoom, p, rot, null, mode);
			}
		}
	}

	private void GenerateCampRadial(ZoneSystem.SpawnMode mode)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(m_campRadiusMin, m_campRadiusMax);
		float num2 = Mathf.Cos((float)Math.PI / 180f * m_maxTilt);
		int num3 = Random.Range(m_minRooms, m_maxRooms);
		int num4 = num3 * 20;
		int num5 = 0;
		for (int i = 0; i < num4; i++)
		{
			Vector3 p = ((Component)this).transform.position + Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f) * Vector3.forward * Random.Range(0f, num - m_perimeterBuffer);
			DungeonDB.RoomData randomWeightedRoom = GetRandomWeightedRoom(perimeterRoom: false);
			if (randomWeightedRoom == null)
			{
				continue;
			}
			if (Object.op_Implicit((Object)(object)ZoneSystem.instance))
			{
				ZoneSystem.instance.GetGroundData(ref p, out var normal, out var _, out var _, out var _);
				if (normal.y < num2 || p.y - 30f < m_minAltitude)
				{
					continue;
				}
			}
			Quaternion campRoomRotation = GetCampRoomRotation(randomWeightedRoom, p);
			if (!TestCollision(randomWeightedRoom.RoomInPrefab, p, campRoomRotation))
			{
				PlaceRoom(randomWeightedRoom, p, campRoomRotation, null, mode);
				num5++;
				if (num5 >= num3)
				{
					break;
				}
			}
		}
		if (m_perimeterSections > 0)
		{
			PlaceWall(num, m_perimeterSections, mode);
		}
	}

	private Quaternion GetCampRoomRotation(DungeonDB.RoomData room, Vector3 pos)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (room.RoomInPrefab.m_faceCenter)
		{
			Vector3 val = ((Component)this).transform.position - pos;
			val.y = 0f;
			if (val == Vector3.zero)
			{
				val = Vector3.forward;
			}
			((Vector3)(ref val)).Normalize();
			float num = Mathf.Round(Utils.YawFromDirection(val) / 22.5f) * 22.5f;
			return Quaternion.Euler(0f, num, 0f);
		}
		return Quaternion.Euler(0f, (float)Random.Range(0, 16) * 22.5f, 0f);
	}

	private void PlaceWall(float radius, int sections, ZoneSystem.SpawnMode mode)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Cos((float)Math.PI / 180f * m_maxTilt);
		int num2 = 0;
		int num3 = sections * 20;
		for (int i = 0; i < num3; i++)
		{
			DungeonDB.RoomData randomWeightedRoom = GetRandomWeightedRoom(perimeterRoom: true);
			if (randomWeightedRoom == null)
			{
				continue;
			}
			Vector3 p = ((Component)this).transform.position + Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f) * Vector3.forward * radius;
			Quaternion campRoomRotation = GetCampRoomRotation(randomWeightedRoom, p);
			if (Object.op_Implicit((Object)(object)ZoneSystem.instance))
			{
				ZoneSystem.instance.GetGroundData(ref p, out var normal, out var _, out var _, out var _);
				if (normal.y < num || p.y - 30f < m_minAltitude)
				{
					continue;
				}
			}
			if (!TestCollision(randomWeightedRoom.RoomInPrefab, p, campRoomRotation))
			{
				PlaceRoom(randomWeightedRoom, p, campRoomRotation, null, mode);
				num2++;
				if (num2 >= sections)
				{
					break;
				}
			}
		}
	}

	private void Save()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_nview == (Object)null)
		{
			return;
		}
		ZDO zDO = m_nview.GetZDO();
		saveStream.SetLength(0L);
		saveWriter.Write(m_placedRooms.Count);
		for (int i = 0; i < m_placedRooms.Count; i++)
		{
			Room room = m_placedRooms[i];
			saveWriter.Write(room.GetHash());
			Utils.Write(saveWriter, ((Component)room).transform.position);
			Utils.Write(saveWriter, ((Component)room).transform.rotation);
		}
		zDO.Set(ZDOVars.s_roomData, saveStream.ToArray());
		if (zDO.GetInt(ZDOVars.s_rooms, out var value))
		{
			zDO.RemoveInt(ZDOVars.s_rooms);
			for (int j = 0; j < value; j++)
			{
				string text = "room" + j;
				zDO.RemoveInt(text);
				zDO.RemoveVec3(text + "_pos");
				zDO.RemoveQuaternion(text + "_rot");
				zDO.RemoveInt(text + "_seed");
			}
			ZLog.Log((object)$"Cleaned up old dungeon data format for {value} rooms.");
		}
	}

	private void Load()
	{
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_nview == (Object)null)
		{
			return;
		}
		DateTime now = DateTime.Now;
		ZLog.Log((object)"Loading dungeon");
		ZDO zDO = m_nview.GetZDO();
		int num = 0;
		if (zDO.GetByteArray(ZDOVars.s_roomData, out var value))
		{
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(value));
			int num2 = binaryReader.ReadInt32();
			m_loadedRooms = new RoomPlacementData[num2];
			for (int i = 0; i < num2; i++)
			{
				int hash = binaryReader.ReadInt32();
				Vector3 position = Utils.ReadVector3(binaryReader);
				Quaternion rotation = Utils.ReadQuaternion(binaryReader);
				DungeonDB.RoomData room = DungeonDB.instance.GetRoom(hash);
				if (room == null)
				{
					ZLog.LogWarning((object)("Missing room:" + hash));
				}
				else
				{
					m_loadedRooms[num++] = new RoomPlacementData(room, position, rotation);
				}
			}
			ZLog.Log((object)$"Dungeon loaded with {num2} rooms in {(DateTime.Now - now).TotalMilliseconds} ms.");
		}
		else
		{
			int @int = zDO.GetInt("rooms");
			m_loadedRooms = new RoomPlacementData[@int];
			for (int j = 0; j < @int; j++)
			{
				string text = "room" + j;
				int int2 = zDO.GetInt(text);
				Vector3 vec = zDO.GetVec3(text + "_pos", Vector3.zero);
				Quaternion quaternion = zDO.GetQuaternion(text + "_rot", Quaternion.identity);
				DungeonDB.RoomData room2 = DungeonDB.instance.GetRoom(int2);
				if (room2 == null)
				{
					ZLog.LogWarning((object)("Missing room:" + int2));
				}
				else
				{
					m_loadedRooms[num++] = new RoomPlacementData(room2, vec, quaternion);
				}
			}
			ZLog.Log((object)$"Dungeon loaded with {@int} rooms from old format in {(DateTime.Now - now).TotalMilliseconds} ms.");
		}
		if (num < m_loadedRooms.Length)
		{
			RoomPlacementData[] array = new RoomPlacementData[num];
			Array.Copy(m_loadedRooms, array, num);
			m_loadedRooms = array;
		}
	}

	private void SetupAvailableRooms()
	{
		m_availableRooms.Clear();
		foreach (DungeonDB.RoomData room in DungeonDB.GetRooms())
		{
			if ((room.m_theme & m_themes) != 0 && room.m_enabled)
			{
				m_availableRooms.Add(room);
			}
		}
	}

	public SoftReference<GameObject>[] GetAvailableRoomPrefabs()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		SetupAvailableRooms();
		SoftReference<GameObject>[] array = new SoftReference<GameObject>[m_availableRooms.Count];
		for (int i = 0; i < m_availableRooms.Count; i++)
		{
			array[i] = m_availableRooms[i].m_prefab;
		}
		return array;
	}

	private DoorDef FindDoorType(string type)
	{
		List<DoorDef> list = new List<DoorDef>();
		foreach (DoorDef doorType in m_doorTypes)
		{
			if (doorType.m_connectionType == type)
			{
				list.Add(doorType);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[Random.Range(0, list.Count)];
	}

	private void PlaceDoors(ZoneSystem.SpawnMode mode)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (RoomConnection doorConnection in m_doorConnections)
		{
			DoorDef doorDef = FindDoorType(doorConnection.m_type);
			if (doorDef == null)
			{
				ZLog.Log((object)("No door type for connection:" + doorConnection.m_type));
			}
			else if ((!(doorDef.m_chance > 0f) || !(Random.value > doorDef.m_chance)) && (!(doorDef.m_chance <= 0f) || !(Random.value > m_doorChance)))
			{
				GameObject val = Object.Instantiate<GameObject>(doorDef.m_prefab, ((Component)doorConnection).transform.position, ((Component)doorConnection).transform.rotation);
				if (mode == ZoneSystem.SpawnMode.Ghost)
				{
					Object.Destroy((Object)(object)val);
				}
				num++;
			}
		}
		ZLog.Log((object)("placed " + num + " doors"));
	}

	private void PlaceEndCaps(ZoneSystem.SpawnMode mode)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_openConnections.Count; i++)
		{
			RoomConnection roomConnection = m_openConnections[i];
			RoomConnection roomConnection2 = null;
			for (int j = 0; j < m_openConnections.Count; j++)
			{
				if (j != i && roomConnection.TestContact(m_openConnections[j]))
				{
					roomConnection2 = m_openConnections[j];
					break;
				}
			}
			if ((Object)(object)roomConnection2 != (Object)null)
			{
				if (roomConnection.m_type != roomConnection2.m_type)
				{
					FindDividers(m_tempRooms);
					if (m_tempRooms.Count > 0)
					{
						DungeonDB.RoomData weightedRoom = GetWeightedRoom(m_tempRooms);
						RoomConnection[] connections = weightedRoom.RoomInPrefab.GetConnections();
						CalculateRoomPosRot(connections[0], ((Component)roomConnection).transform.position, ((Component)roomConnection).transform.rotation, out var pos, out var rot);
						bool flag = false;
						foreach (Room placedRoom in m_placedRooms)
						{
							if (placedRoom.m_divider && Vector3.Distance(((Component)placedRoom).transform.position, pos) < 0.5f)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							PlaceRoom(weightedRoom, pos, rot, roomConnection, mode);
							ZLog.Log((object)("Cyclic detected. Door missmatch for cyclic room '" + roomConnection.m_type + "'-'" + roomConnection2.m_type + "', placing divider: " + weightedRoom.m_prefab.Name));
						}
					}
					else
					{
						ZLog.LogWarning((object)("Cyclic detected. Door missmatch for cyclic room '" + roomConnection.m_type + "'-'" + roomConnection2.m_type + "', but no dividers defined!"));
					}
				}
				else
				{
					ZLog.Log((object)"cyclic detected and door types match, cool");
				}
				continue;
			}
			FindEndCaps(roomConnection, m_tempRooms);
			bool flag2 = false;
			if (m_alternativeFunctionality)
			{
				for (int k = 0; k < 5; k++)
				{
					DungeonDB.RoomData weightedRoom2 = GetWeightedRoom(m_tempRooms);
					if (PlaceRoom(roomConnection, weightedRoom2, mode))
					{
						flag2 = true;
						break;
					}
				}
			}
			IOrderedEnumerable<DungeonDB.RoomData> orderedEnumerable = m_tempRooms.OrderByDescending((DungeonDB.RoomData item) => item.RoomInPrefab.m_endCapPrio);
			if (!flag2)
			{
				foreach (DungeonDB.RoomData item in orderedEnumerable)
				{
					if (PlaceRoom(roomConnection, item, mode))
					{
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				ZLog.LogWarning((object)("Failed to place end cap " + ((Object)roomConnection).name + " " + ((Object)((Component)((Component)roomConnection).transform.parent).gameObject).name));
			}
		}
	}

	private void FindDividers(List<DungeonDB.RoomData> rooms)
	{
		rooms.Clear();
		foreach (DungeonDB.RoomData availableRoom in m_availableRooms)
		{
			if (availableRoom.RoomInPrefab.m_divider)
			{
				rooms.Add(availableRoom);
			}
		}
		rooms.Shuffle(useUnityRandom: true);
	}

	private void FindEndCaps(RoomConnection connection, List<DungeonDB.RoomData> rooms)
	{
		rooms.Clear();
		foreach (DungeonDB.RoomData availableRoom in m_availableRooms)
		{
			if (availableRoom.RoomInPrefab.m_endCap && availableRoom.RoomInPrefab.HaveConnection(connection))
			{
				rooms.Add(availableRoom);
			}
		}
		rooms.Shuffle(useUnityRandom: true);
	}

	private DungeonDB.RoomData FindEndCap(RoomConnection connection)
	{
		m_tempRooms.Clear();
		foreach (DungeonDB.RoomData availableRoom in m_availableRooms)
		{
			if (availableRoom.RoomInPrefab.m_endCap && availableRoom.RoomInPrefab.HaveConnection(connection))
			{
				m_tempRooms.Add(availableRoom);
			}
		}
		if (m_tempRooms.Count == 0)
		{
			return null;
		}
		return m_tempRooms[Random.Range(0, m_tempRooms.Count)];
	}

	private void PlaceRooms(ZoneSystem.SpawnMode mode)
	{
		for (int i = 0; i < m_maxRooms; i++)
		{
			PlaceOneRoom(mode);
			if (CheckRequiredRooms() && m_placedRooms.Count > m_minRooms)
			{
				ZLog.Log((object)"All required rooms have been placed, stopping generation");
				break;
			}
		}
	}

	private void PlaceStartRoom(ZoneSystem.SpawnMode mode)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		DungeonDB.RoomData roomData = FindStartRoom();
		RoomConnection entrance = roomData.RoomInPrefab.GetEntrance();
		Quaternion rotation = ((Component)this).transform.rotation;
		CalculateRoomPosRot(entrance, ((Component)this).transform.position, rotation, out var pos, out var rot);
		PlaceRoom(roomData, pos, rot, entrance, mode);
	}

	private bool PlaceOneRoom(ZoneSystem.SpawnMode mode)
	{
		RoomConnection openConnection = GetOpenConnection();
		if ((Object)(object)openConnection == (Object)null)
		{
			return false;
		}
		for (int i = 0; i < 10; i++)
		{
			DungeonDB.RoomData roomData = (m_alternativeFunctionality ? GetRandomWeightedRoom(openConnection) : GetRandomRoom(openConnection));
			if (roomData == null)
			{
				break;
			}
			if (PlaceRoom(openConnection, roomData, mode))
			{
				return true;
			}
		}
		return false;
	}

	private void CalculateRoomPosRot(RoomConnection roomCon, Vector3 exitPos, Quaternion exitRot, out Vector3 pos, out Quaternion rot)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.Inverse(((Component)roomCon).transform.localRotation);
		rot = exitRot * val;
		Vector3 localPosition = ((Component)roomCon).transform.localPosition;
		pos = exitPos - rot * localPosition;
	}

	private bool PlaceRoom(RoomConnection connection, DungeonDB.RoomData roomData, ZoneSystem.SpawnMode mode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		SoftReference<GameObject> prefab = roomData.m_prefab;
		prefab.Load();
		Room component = prefab.Asset.GetComponent<Room>();
		Quaternion rotation = ((Component)connection).transform.rotation;
		rotation *= Quaternion.Euler(0f, 180f, 0f);
		RoomConnection connection2 = component.GetConnection(connection);
		if ((Object)(object)((Component)((Component)connection2).transform.parent).gameObject != (Object)(object)((Component)component).gameObject)
		{
			ZLog.LogWarning((object)("Connection '" + ((Object)component).name + "->" + ((Object)connection2).name + "' is not placed as a direct child of room!"));
		}
		CalculateRoomPosRot(connection2, ((Component)connection).transform.position, rotation, out var pos, out var rot);
		if (((Vector3Int)(ref component.m_size)).x != 0 && ((Vector3Int)(ref component.m_size)).z != 0 && TestCollision(component, pos, rot))
		{
			prefab.Release();
			return false;
		}
		PlaceRoom(roomData, pos, rot, connection, mode);
		if (!component.m_endCap)
		{
			if (connection.m_allowDoor && (!connection.m_doorOnlyIfOtherAlsoAllowsDoor || connection2.m_allowDoor))
			{
				m_doorConnections.Add(connection);
			}
			m_openConnections.Remove(connection);
		}
		prefab.Release();
		return true;
	}

	private Room PlaceRoom(DungeonDB.RoomData roomData, Vector3 pos, Quaternion rot, RoomConnection fromConnection, ZoneSystem.SpawnMode mode)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		roomData.m_prefab.Load();
		Room component = roomData.m_prefab.Asset.GetComponent<Room>();
		ZNetView[] enabledComponentsInChildren = Utils.GetEnabledComponentsInChildren<ZNetView>(roomData.m_prefab.Asset);
		RandomSpawn[] enabledComponentsInChildren2 = Utils.GetEnabledComponentsInChildren<RandomSpawn>(roomData.m_prefab.Asset);
		for (int i = 0; i < enabledComponentsInChildren2.Length; i++)
		{
			enabledComponentsInChildren2[i].Prepare();
		}
		Vector3 val = pos;
		if (m_useCustomInteriorTransform)
		{
			val = pos - ((Component)this).transform.position;
		}
		int num = (int)val.x * 4271 + (int)val.y * 9187 + (int)val.z * 2134;
		State state = Random.state;
		Random.InitState(num);
		Vector3 position = ((Component)component).transform.position;
		Quaternion val2 = Quaternion.Inverse(((Component)component).transform.rotation);
		RandomSpawn[] array;
		ZNetView[] array2;
		if (mode == ZoneSystem.SpawnMode.Full || mode == ZoneSystem.SpawnMode.Ghost)
		{
			Random.InitState(num);
			array = enabledComponentsInChildren2;
			foreach (RandomSpawn randomSpawn in array)
			{
				Vector3 val3 = val2 * (((Component)randomSpawn).gameObject.transform.position - position);
				Vector3 pos2 = pos + rot * val3;
				randomSpawn.Randomize(pos2, null, this);
			}
			array2 = enabledComponentsInChildren;
			foreach (ZNetView zNetView in array2)
			{
				if (((Component)zNetView).gameObject.activeSelf)
				{
					Vector3 val4 = val2 * (((Component)zNetView).gameObject.transform.position - position);
					Vector3 val5 = pos + rot * val4;
					Quaternion val6 = val2 * ((Component)zNetView).gameObject.transform.rotation;
					Quaternion val7 = rot * val6;
					GameObject val8 = Object.Instantiate<GameObject>(((Component)zNetView).gameObject, val5, val7);
					GameObjectExtentions.HoldReferenceTo(val8, roomData.m_prefab);
					if (mode == ZoneSystem.SpawnMode.Ghost)
					{
						Object.Destroy((Object)(object)val8);
					}
				}
			}
		}
		else
		{
			Random.InitState(num);
			array = enabledComponentsInChildren2;
			foreach (RandomSpawn randomSpawn2 in array)
			{
				Vector3 val9 = val2 * (((Component)randomSpawn2).gameObject.transform.position - position);
				Vector3 pos3 = pos + rot * val9;
				randomSpawn2.Randomize(pos3, null, this);
			}
		}
		array2 = enabledComponentsInChildren;
		for (int j = 0; j < array2.Length; j++)
		{
			((Component)array2[j]).gameObject.SetActive(false);
		}
		Room component2 = Utils.Instantiate(roomData.m_prefab, pos, rot, ((Component)this).transform).GetComponent<Room>();
		((Object)((Component)component2).gameObject).name = roomData.m_prefab.Name;
		if (mode != ZoneSystem.SpawnMode.Client)
		{
			component2.m_placeOrder = (Object.op_Implicit((Object)(object)fromConnection) ? (fromConnection.m_placeOrder + 1) : 0);
			component2.m_seed = num;
			m_placedRooms.Add(component2);
			AddOpenConnections(component2, fromConnection);
		}
		Random.state = state;
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
		roomData.m_prefab.Release();
		return component2;
	}

	private void AddOpenConnections(Room newRoom, RoomConnection skipConnection)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		RoomConnection[] connections = newRoom.GetConnections();
		if ((Object)(object)skipConnection != (Object)null)
		{
			RoomConnection[] array = connections;
			foreach (RoomConnection roomConnection in array)
			{
				if (!roomConnection.m_entrance && !(Vector3.Distance(((Component)roomConnection).transform.position, ((Component)skipConnection).transform.position) < 0.1f))
				{
					roomConnection.m_placeOrder = newRoom.m_placeOrder;
					m_openConnections.Add(roomConnection);
				}
			}
		}
		else
		{
			RoomConnection[] array = connections;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].m_placeOrder = newRoom.m_placeOrder;
			}
			m_openConnections.AddRange(connections);
		}
	}

	private void SetupColliders()
	{
		if (!((Object)(object)m_colliderA != (Object)null))
		{
			BoxCollider[] componentsInChildren = ((Component)this).gameObject.GetComponentsInChildren<BoxCollider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.DestroyImmediate((Object)(object)componentsInChildren[i]);
			}
			m_colliderA = ((Component)this).gameObject.AddComponent<BoxCollider>();
			m_colliderB = ((Component)this).gameObject.AddComponent<BoxCollider>();
		}
	}

	public void Derp()
	{
	}

	private bool IsInsideDungeon(Room room, Vector3 pos, Quaternion rot)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		Bounds val = default(Bounds);
		((Bounds)(ref val))._002Ector(m_zoneCenter, m_zoneSize);
		Vector3 val2 = Vector3Int.op_Implicit(room.m_size);
		val2 *= 0.5f;
		if (!((Bounds)(ref val)).Contains(pos + rot * new Vector3(val2.x, val2.y, 0f - val2.z)))
		{
			return false;
		}
		if (!((Bounds)(ref val)).Contains(pos + rot * new Vector3(0f - val2.x, val2.y, 0f - val2.z)))
		{
			return false;
		}
		if (!((Bounds)(ref val)).Contains(pos + rot * new Vector3(val2.x, val2.y, val2.z)))
		{
			return false;
		}
		if (!((Bounds)(ref val)).Contains(pos + rot * new Vector3(0f - val2.x, val2.y, val2.z)))
		{
			return false;
		}
		if (!((Bounds)(ref val)).Contains(pos + rot * new Vector3(val2.x, 0f - val2.y, 0f - val2.z)))
		{
			return false;
		}
		if (!((Bounds)(ref val)).Contains(pos + rot * new Vector3(0f - val2.x, 0f - val2.y, 0f - val2.z)))
		{
			return false;
		}
		if (!((Bounds)(ref val)).Contains(pos + rot * new Vector3(val2.x, 0f - val2.y, val2.z)))
		{
			return false;
		}
		if (!((Bounds)(ref val)).Contains(pos + rot * new Vector3(0f - val2.x, 0f - val2.y, val2.z)))
		{
			return false;
		}
		return true;
	}

	private bool TestCollision(Room room, Vector3 pos, Quaternion rot)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!IsInsideDungeon(room, pos, rot))
		{
			return true;
		}
		m_colliderA.size = new Vector3((float)((Vector3Int)(ref room.m_size)).x - 0.1f, (float)((Vector3Int)(ref room.m_size)).y - 0.1f, (float)((Vector3Int)(ref room.m_size)).z - 0.1f);
		Vector3 val = default(Vector3);
		float num = default(float);
		foreach (Room placedRoom in m_placedRooms)
		{
			m_colliderB.size = Vector3Int.op_Implicit(placedRoom.m_size);
			if (Physics.ComputePenetration((Collider)(object)m_colliderA, pos, rot, (Collider)(object)m_colliderB, ((Component)placedRoom).transform.position, ((Component)placedRoom).transform.rotation, ref val, ref num))
			{
				return true;
			}
		}
		return false;
	}

	private DungeonDB.RoomData GetRandomWeightedRoom(bool perimeterRoom)
	{
		m_tempRooms.Clear();
		float num = 0f;
		foreach (DungeonDB.RoomData availableRoom in m_availableRooms)
		{
			if (!availableRoom.RoomInPrefab.m_entrance && !availableRoom.RoomInPrefab.m_endCap && !availableRoom.RoomInPrefab.m_divider && availableRoom.RoomInPrefab.m_perimeter == perimeterRoom)
			{
				num += availableRoom.RoomInPrefab.m_weight;
				m_tempRooms.Add(availableRoom);
			}
		}
		if (m_tempRooms.Count == 0)
		{
			return null;
		}
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		foreach (DungeonDB.RoomData tempRoom in m_tempRooms)
		{
			num3 += tempRoom.RoomInPrefab.m_weight;
			if (num2 <= num3)
			{
				return tempRoom;
			}
		}
		return m_tempRooms[0];
	}

	private DungeonDB.RoomData GetRandomWeightedRoom(RoomConnection connection)
	{
		m_tempRooms.Clear();
		foreach (DungeonDB.RoomData availableRoom in m_availableRooms)
		{
			if (!availableRoom.RoomInPrefab.m_entrance && !availableRoom.RoomInPrefab.m_endCap && !availableRoom.RoomInPrefab.m_divider && (!Object.op_Implicit((Object)(object)connection) || (availableRoom.RoomInPrefab.HaveConnection(connection) && connection.m_placeOrder >= availableRoom.RoomInPrefab.m_minPlaceOrder)))
			{
				m_tempRooms.Add(availableRoom);
			}
		}
		if (m_tempRooms.Count == 0)
		{
			return null;
		}
		return GetWeightedRoom(m_tempRooms);
	}

	private DungeonDB.RoomData GetWeightedRoom(List<DungeonDB.RoomData> rooms)
	{
		float num = 0f;
		foreach (DungeonDB.RoomData room in rooms)
		{
			num += room.RoomInPrefab.m_weight;
		}
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		foreach (DungeonDB.RoomData room2 in rooms)
		{
			num3 += room2.RoomInPrefab.m_weight;
			if (num2 <= num3)
			{
				return room2;
			}
		}
		return m_tempRooms[0];
	}

	private DungeonDB.RoomData GetRandomRoom(RoomConnection connection)
	{
		m_tempRooms.Clear();
		foreach (DungeonDB.RoomData availableRoom in m_availableRooms)
		{
			if (!availableRoom.RoomInPrefab.m_entrance && !availableRoom.RoomInPrefab.m_endCap && !availableRoom.RoomInPrefab.m_divider && (!Object.op_Implicit((Object)(object)connection) || (availableRoom.RoomInPrefab.HaveConnection(connection) && connection.m_placeOrder >= availableRoom.RoomInPrefab.m_minPlaceOrder)))
			{
				m_tempRooms.Add(availableRoom);
			}
		}
		if (m_tempRooms.Count == 0)
		{
			return null;
		}
		return m_tempRooms[Random.Range(0, m_tempRooms.Count)];
	}

	private RoomConnection GetOpenConnection()
	{
		if (m_openConnections.Count == 0)
		{
			return null;
		}
		return m_openConnections[Random.Range(0, m_openConnections.Count)];
	}

	private DungeonDB.RoomData FindStartRoom()
	{
		m_tempRooms.Clear();
		foreach (DungeonDB.RoomData availableRoom in m_availableRooms)
		{
			if (availableRoom.RoomInPrefab.m_entrance)
			{
				m_tempRooms.Add(availableRoom);
			}
		}
		return m_tempRooms[Random.Range(0, m_tempRooms.Count)];
	}

	private bool CheckRequiredRooms()
	{
		if (m_minRequiredRooms == 0 || m_requiredRooms.Count == 0)
		{
			return false;
		}
		int num = 0;
		foreach (Room placedRoom in m_placedRooms)
		{
			if (m_requiredRooms.Contains(((Object)((Component)placedRoom).gameObject).name))
			{
				num++;
			}
		}
		return num >= m_minRequiredRooms;
	}

	private void OnDrawGizmos()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = new Color(0f, 1.5f, 0f, 0.5f);
		Gizmos.DrawWireCube(m_zoneCenter, new Vector3(m_zoneSize.x, m_zoneSize.y, m_zoneSize.z));
		Gizmos.matrix = Matrix4x4.identity;
	}
}
