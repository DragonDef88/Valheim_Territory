using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerProfile
{
	private class WorldPlayerData
	{
		public Vector3 m_spawnPoint = Vector3.zero;

		public bool m_haveCustomSpawnPoint;

		public Vector3 m_logoutPoint = Vector3.zero;

		public bool m_haveLogoutPoint;

		public Vector3 m_deathPoint = Vector3.zero;

		public bool m_haveDeathPoint;

		public Vector3 m_homePoint = Vector3.zero;

		public byte[] m_mapData;
	}

	public class PlayerStats
	{
		public Dictionary<PlayerStatType, float> m_stats = new Dictionary<PlayerStatType, float>();

		public float this[PlayerStatType type]
		{
			get
			{
				return m_stats[type];
			}
			set
			{
				m_stats[type] = value;
			}
		}

		public PlayerStats()
		{
			for (int i = 0; i < 105; i++)
			{
				m_stats[(PlayerStatType)i] = 0f;
			}
		}
	}

	public static Action SavingStarted;

	public static Action SavingFinished;

	public static Vector3 m_originalSpawnPoint = new Vector3(-676f, 50f, 299f);

	public readonly PlayerStats m_playerStats = new PlayerStats();

	public bool m_firstSpawn = true;

	public FileSource m_fileSource = (FileSource)1;

	public readonly string m_filename = "";

	private string m_playerName = "";

	private long m_playerID;

	private string m_startSeed = "";

	private readonly Dictionary<long, WorldPlayerData> m_worldData = new Dictionary<long, WorldPlayerData>();

	private bool m_createBackupBeforeSaving;

	private DateTime m_lastSaveLoad = DateTime.Now;

	public Dictionary<string, float> m_knownWorlds = new Dictionary<string, float>();

	public Dictionary<string, float> m_knownWorldKeys = new Dictionary<string, float>();

	public Dictionary<string, float> m_knownCommands = new Dictionary<string, float>();

	public Dictionary<string, float> m_enemyStats = new Dictionary<string, float>();

	public Dictionary<string, float> m_itemPickupStats = new Dictionary<string, float>();

	public Dictionary<string, float> m_itemCraftStats = new Dictionary<string, float>();

	public bool m_usedCheats;

	public DateTime m_dateCreated = DateTime.Now;

	private byte[] m_playerData;

	public static Dictionary<PlayerStatType, string> m_statTypeDates = new Dictionary<PlayerStatType, string>
	{
		{
			PlayerStatType.Deaths,
			"Since beginning"
		},
		{
			PlayerStatType.Jumps,
			"Hildirs Request (2023-06-16)"
		}
	};

	public PlayerProfile(string filename = null, FileSource fileSource = 0)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		m_filename = filename;
		if ((int)fileSource == 0)
		{
			m_fileSource = (FileSource)((!FileHelpers.CloudStorageEnabled) ? 1 : 2);
		}
		else
		{
			m_fileSource = fileSource;
		}
		m_playerName = "Stranger";
		m_playerID = Utils.GenerateUID();
	}

	public bool Load()
	{
		if (m_filename == null)
		{
			return false;
		}
		return LoadPlayerFromDisk();
	}

	public bool Save()
	{
		if (m_filename == null)
		{
			return false;
		}
		return SavePlayerToDisk();
	}

	public bool HaveIncompatiblPlayerData()
	{
		if (m_filename == null)
		{
			return false;
		}
		ZPackage zPackage = LoadPlayerDataFromDisk();
		if (zPackage == null)
		{
			return false;
		}
		if (!Version.IsPlayerVersionCompatible(zPackage.ReadInt()))
		{
			ZLog.Log((object)"Player data is not compatible, ignoring");
			return true;
		}
		return false;
	}

	public void SavePlayerData(Player player)
	{
		ZPackage zPackage = new ZPackage();
		player.Save(zPackage);
		m_playerData = zPackage.GetArray();
	}

	public void LoadPlayerData(Player player)
	{
		player.SetPlayerID(m_playerID, GetName());
		if (m_playerData != null)
		{
			ZPackage pkg = new ZPackage(m_playerData);
			player.Load(pkg);
		}
		else
		{
			player.GiveDefaultItems();
		}
	}

	public void SaveLogoutPoint()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Player.m_localPlayer) && !Player.m_localPlayer.IsDead() && !Player.m_localPlayer.InIntro())
		{
			SetLogoutPoint(((Component)Player.m_localPlayer).transform.position);
		}
	}

	private bool SavePlayerToDisk()
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Expected O, but got Unknown
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		//IL_067c: Invalid comparison between Unknown and I4
		//IL_070e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Unknown result type (might be due to invalid IL or missing references)
		//IL_0688: Invalid comparison between Unknown and I4
		SavingStarted?.Invoke();
		DateTime now = DateTime.Now;
		bool flag = SaveSystem.CheckMove(m_filename, SaveDataType.Character, ref m_fileSource, now, 0uL);
		if (m_createBackupBeforeSaving && !flag)
		{
			if (SaveSystem.TryGetSaveByName(m_filename, SaveDataType.Character, out var save) && !save.IsDeleted)
			{
				if (SaveSystem.CreateBackup(save.PrimaryFile, DateTime.Now, m_fileSource))
				{
					ZLog.Log((object)"Migrating character save from an old save format, created backup!");
				}
				else
				{
					ZLog.LogError((object)("Failed to create backup of character save " + m_filename + "!"));
				}
			}
			else
			{
				ZLog.LogError((object)("Failed to get character save " + m_filename + " from save system, so a backup couldn't be created!"));
			}
		}
		m_createBackupBeforeSaving = false;
		string text = GetCharacterFolderPath(m_fileSource) + m_filename + ".fch";
		string text2 = text + ".old";
		string text3 = text + ".new";
		string characterFolderPath = GetCharacterFolderPath(m_fileSource);
		if (!Directory.Exists(characterFolderPath) && (int)m_fileSource != 2)
		{
			Directory.CreateDirectory(characterFolderPath);
		}
		ZPackage zPackage = new ZPackage();
		zPackage.Write(43);
		zPackage.Write(105);
		for (int i = 0; i < 105; i++)
		{
			zPackage.Write(m_playerStats.m_stats[(PlayerStatType)i]);
		}
		zPackage.Write(m_firstSpawn);
		zPackage.Write(m_worldData.Count);
		foreach (KeyValuePair<long, WorldPlayerData> worldDatum in m_worldData)
		{
			zPackage.Write(worldDatum.Key);
			zPackage.Write(worldDatum.Value.m_haveCustomSpawnPoint);
			zPackage.Write(worldDatum.Value.m_spawnPoint);
			zPackage.Write(worldDatum.Value.m_haveLogoutPoint);
			zPackage.Write(worldDatum.Value.m_logoutPoint);
			zPackage.Write(worldDatum.Value.m_haveDeathPoint);
			zPackage.Write(worldDatum.Value.m_deathPoint);
			zPackage.Write(worldDatum.Value.m_homePoint);
			zPackage.Write(worldDatum.Value.m_mapData != null);
			if (worldDatum.Value.m_mapData != null)
			{
				zPackage.Write(worldDatum.Value.m_mapData);
			}
		}
		zPackage.Write(m_playerName);
		zPackage.Write(m_playerID);
		zPackage.Write(m_startSeed);
		int num = (int)(DateTime.Now - m_lastSaveLoad).TotalSeconds;
		m_lastSaveLoad = DateTime.Now;
		zPackage.Write(m_usedCheats);
		zPackage.Write(new DateTimeOffset(m_dateCreated).ToUnixTimeSeconds());
		if (Object.op_Implicit((Object)(object)ZNet.instance) && Object.op_Implicit((Object)(object)ZoneSystem.instance) && ZNet.World != null)
		{
			Utils.IncrementOrSet<string>(m_knownWorlds, ZNet.instance.GetWorldName(), (float)num);
			for (int j = 0; j < 43; j++)
			{
				if (ZoneSystem.instance.GetGlobalKey((GlobalKeys)j, out string value))
				{
					Dictionary<string, float> knownWorldKeys = m_knownWorldKeys;
					GlobalKeys globalKeys = (GlobalKeys)j;
					Utils.IncrementOrSet<string>(knownWorldKeys, globalKeys.ToString().ToLower() + " " + value, (float)num);
				}
				else
				{
					Dictionary<string, float> knownWorldKeys2 = m_knownWorldKeys;
					GlobalKeys globalKeys = (GlobalKeys)j;
					Utils.IncrementOrSet<string>(knownWorldKeys2, globalKeys.ToString().ToLower() + " default", (float)num);
				}
			}
		}
		zPackage.Write(m_knownWorlds.Count);
		foreach (KeyValuePair<string, float> knownWorld in m_knownWorlds)
		{
			zPackage.Write(knownWorld.Key);
			zPackage.Write(knownWorld.Value);
		}
		zPackage.Write(m_knownWorldKeys.Count);
		foreach (KeyValuePair<string, float> knownWorldKey in m_knownWorldKeys)
		{
			zPackage.Write(knownWorldKey.Key);
			zPackage.Write(knownWorldKey.Value);
		}
		zPackage.Write(m_knownCommands.Count);
		foreach (KeyValuePair<string, float> knownCommand in m_knownCommands)
		{
			zPackage.Write(knownCommand.Key);
			zPackage.Write(knownCommand.Value);
		}
		zPackage.Write(m_enemyStats.Count);
		foreach (KeyValuePair<string, float> enemyStat in m_enemyStats)
		{
			zPackage.Write(enemyStat.Key);
			zPackage.Write(enemyStat.Value);
		}
		zPackage.Write(m_itemPickupStats.Count);
		foreach (KeyValuePair<string, float> itemPickupStat in m_itemPickupStats)
		{
			zPackage.Write(itemPickupStat.Key);
			zPackage.Write(itemPickupStat.Value);
		}
		zPackage.Write(m_itemCraftStats.Count);
		foreach (KeyValuePair<string, float> itemCraftStat in m_itemCraftStats)
		{
			zPackage.Write(itemCraftStat.Key);
			zPackage.Write(itemCraftStat.Value);
		}
		if (m_playerData != null)
		{
			zPackage.Write(data: true);
			zPackage.Write(m_playerData);
		}
		else
		{
			zPackage.Write(data: false);
		}
		byte[] array = zPackage.GenerateHash();
		byte[] array2 = zPackage.GetArray();
		FileWriter val = new FileWriter(text3, (FileHelperType)0, m_fileSource);
		val.m_binary.Write(array2.Length);
		val.m_binary.Write(array2);
		val.m_binary.Write(array.Length);
		val.m_binary.Write(array);
		val.Finish();
		SaveSystem.InvalidateCache();
		if ((int)val.Status != 2 && (int)m_fileSource == 2)
		{
			string text4 = GetCharacterFolderPath((FileSource)1) + m_filename + "_backup_cloud-" + now.ToString("yyyyMMdd-HHmmss") + ".fch";
			val.DumpCloudWriteToLocalFile(text4);
			SaveSystem.InvalidateCache();
			ZLog.LogError((object)("Cloud save to location \"" + text + "\" failed! Saved as local backup \"" + text4 + "\". Use the \"Manage saves\" menu to restore this backup."));
		}
		else
		{
			FileHelpers.ReplaceOldFile(text, text3, text2, m_fileSource);
			SaveSystem.InvalidateCache();
			ZNet.ConsiderAutoBackup(m_filename, SaveDataType.Character, now);
		}
		SavingFinished?.Invoke();
		return true;
	}

	private bool LoadPlayerFromDisk()
	{
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			ZPackage zPackage = LoadPlayerDataFromDisk();
			if (zPackage == null)
			{
				ZLog.LogWarning((object)"No player data");
				return false;
			}
			int num = zPackage.ReadInt();
			if (!Version.IsPlayerVersionCompatible(num))
			{
				ZLog.Log((object)"Player data is not compatible, ignoring");
				return false;
			}
			if (num != 43)
			{
				m_createBackupBeforeSaving = true;
			}
			if (num >= 38)
			{
				int num2 = zPackage.ReadInt();
				for (int i = 0; i < num2; i++)
				{
					m_playerStats[(PlayerStatType)i] = zPackage.ReadSingle();
				}
			}
			else if (num >= 28)
			{
				m_playerStats[PlayerStatType.EnemyKills] = zPackage.ReadInt();
				m_playerStats[PlayerStatType.Deaths] = zPackage.ReadInt();
				m_playerStats[PlayerStatType.CraftsOrUpgrades] = zPackage.ReadInt();
				m_playerStats[PlayerStatType.Builds] = zPackage.ReadInt();
			}
			if (num >= 40)
			{
				m_firstSpawn = zPackage.ReadBool();
			}
			m_worldData.Clear();
			int num3 = zPackage.ReadInt();
			for (int j = 0; j < num3; j++)
			{
				long key = zPackage.ReadLong();
				WorldPlayerData worldPlayerData = new WorldPlayerData();
				worldPlayerData.m_haveCustomSpawnPoint = zPackage.ReadBool();
				worldPlayerData.m_spawnPoint = zPackage.ReadVector3();
				worldPlayerData.m_haveLogoutPoint = zPackage.ReadBool();
				worldPlayerData.m_logoutPoint = zPackage.ReadVector3();
				if (num >= 30)
				{
					worldPlayerData.m_haveDeathPoint = zPackage.ReadBool();
					worldPlayerData.m_deathPoint = zPackage.ReadVector3();
				}
				worldPlayerData.m_homePoint = zPackage.ReadVector3();
				if (num >= 29 && zPackage.ReadBool())
				{
					worldPlayerData.m_mapData = zPackage.ReadByteArray();
				}
				m_worldData.Add(key, worldPlayerData);
			}
			SetName(zPackage.ReadString());
			m_playerID = zPackage.ReadLong();
			m_startSeed = zPackage.ReadString();
			if (num >= 38)
			{
				m_usedCheats = zPackage.ReadBool();
				m_dateCreated = DateTimeOffset.FromUnixTimeSeconds(zPackage.ReadLong()).Date;
				int num4 = zPackage.ReadInt();
				for (int k = 0; k < num4; k++)
				{
					m_knownWorlds[zPackage.ReadString()] = zPackage.ReadSingle();
				}
				num4 = zPackage.ReadInt();
				for (int l = 0; l < num4; l++)
				{
					m_knownWorldKeys[zPackage.ReadString()] = zPackage.ReadSingle();
				}
				num4 = zPackage.ReadInt();
				for (int m = 0; m < num4; m++)
				{
					m_knownCommands[zPackage.ReadString()] = zPackage.ReadSingle();
				}
				if (num >= 42)
				{
					num4 = zPackage.ReadInt();
					for (int n = 0; n < num4; n++)
					{
						m_enemyStats[zPackage.ReadString()] = zPackage.ReadSingle();
					}
					num4 = zPackage.ReadInt();
					for (int num5 = 0; num5 < num4; num5++)
					{
						m_itemPickupStats[zPackage.ReadString()] = zPackage.ReadSingle();
					}
					num4 = zPackage.ReadInt();
					for (int num6 = 0; num6 < num4; num6++)
					{
						m_itemCraftStats[zPackage.ReadString()] = zPackage.ReadSingle();
					}
				}
			}
			else
			{
				m_dateCreated = new DateTime(2021, 2, 2);
			}
			if (zPackage.ReadBool())
			{
				m_playerData = zPackage.ReadByteArray();
			}
			else
			{
				m_playerData = null;
			}
			if (num < 40)
			{
				GetFirstSpawnFromPlayerData();
			}
			m_lastSaveLoad = DateTime.Now;
		}
		catch (Exception ex)
		{
			ZLog.LogWarning((object)("Exception while loading player profile:" + m_filename + " , " + ex.ToString()));
		}
		return true;
		void GetFirstSpawnFromPlayerData()
		{
			if (m_playerData != null)
			{
				ZPackage zPackage2 = new ZPackage(m_playerData);
				int num7 = zPackage2.ReadInt();
				if (num7 >= 8 && num7 < 28)
				{
					if (num7 >= 7)
					{
						zPackage2.ReadSingle();
					}
					zPackage2.ReadSingle();
					if (num7 >= 10)
					{
						zPackage2.ReadSingle();
					}
					m_firstSpawn = zPackage2.ReadBool();
				}
			}
		}
	}

	private ZPackage LoadPlayerDataFromDisk()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		string path = GetPath(m_fileSource, m_filename);
		FileReader val;
		try
		{
			val = new FileReader(path, m_fileSource, (FileHelperType)0);
		}
		catch (Exception ex)
		{
			ZLog.Log((object)("  failed to load: " + path + " (" + ex.Message + ")"));
			return null;
		}
		byte[] data;
		try
		{
			BinaryReader binary = val.m_binary;
			int count = binary.ReadInt32();
			data = binary.ReadBytes(count);
			int count2 = binary.ReadInt32();
			binary.ReadBytes(count2);
		}
		catch (Exception ex2)
		{
			ZLog.LogError((object)$"  error loading player.dat. Source: {m_fileSource}, Path: {path}, Error: {ex2.Message}");
			val.Dispose();
			return null;
		}
		val.Dispose();
		return new ZPackage(data);
	}

	public void SetLogoutPoint(Vector3 point)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		GetWorldData(ZNet.instance.GetWorldUID()).m_haveLogoutPoint = true;
		GetWorldData(ZNet.instance.GetWorldUID()).m_logoutPoint = point;
	}

	public void SetDeathPoint(Vector3 point)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		GetWorldData(ZNet.instance.GetWorldUID()).m_haveDeathPoint = true;
		GetWorldData(ZNet.instance.GetWorldUID()).m_deathPoint = point;
	}

	public void SetMapData(byte[] data)
	{
		long worldUID = ZNet.instance.GetWorldUID();
		if (worldUID != 0L)
		{
			GetWorldData(worldUID).m_mapData = data;
		}
	}

	public byte[] GetMapData()
	{
		return GetWorldData(ZNet.instance.GetWorldUID()).m_mapData;
	}

	public void ClearLoguoutPoint()
	{
		GetWorldData(ZNet.instance.GetWorldUID()).m_haveLogoutPoint = false;
	}

	public bool HaveLogoutPoint()
	{
		return GetWorldData(ZNet.instance.GetWorldUID()).m_haveLogoutPoint;
	}

	public Vector3 GetLogoutPoint()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return GetWorldData(ZNet.instance.GetWorldUID()).m_logoutPoint;
	}

	public bool HaveDeathPoint()
	{
		return GetWorldData(ZNet.instance.GetWorldUID()).m_haveDeathPoint;
	}

	public Vector3 GetDeathPoint()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return GetWorldData(ZNet.instance.GetWorldUID()).m_deathPoint;
	}

	public void SetCustomSpawnPoint(Vector3 point)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		GetWorldData(ZNet.instance.GetWorldUID()).m_haveCustomSpawnPoint = true;
		GetWorldData(ZNet.instance.GetWorldUID()).m_spawnPoint = point;
	}

	public Vector3 GetCustomSpawnPoint()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return GetWorldData(ZNet.instance.GetWorldUID()).m_spawnPoint;
	}

	public bool HaveCustomSpawnPoint()
	{
		return GetWorldData(ZNet.instance.GetWorldUID()).m_haveCustomSpawnPoint;
	}

	public void ClearCustomSpawnPoint()
	{
		GetWorldData(ZNet.instance.GetWorldUID()).m_haveCustomSpawnPoint = false;
	}

	public void SetHomePoint(Vector3 point)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		GetWorldData(ZNet.instance.GetWorldUID()).m_homePoint = point;
	}

	public Vector3 GetHomePoint()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return GetWorldData(ZNet.instance.GetWorldUID()).m_homePoint;
	}

	public void SetName(string name)
	{
		m_playerName = name;
	}

	public string GetName()
	{
		return m_playerName;
	}

	public long GetPlayerID()
	{
		return m_playerID;
	}

	public static void RemoveProfile(string name, FileSource fileSource = 0)
	{
		if (SaveSystem.TryGetSaveByName(name, SaveDataType.Character, out var save) && !save.IsDeleted)
		{
			SaveSystem.Delete(save.PrimaryFile);
		}
	}

	public static bool HaveProfile(string name)
	{
		if (SaveSystem.TryGetSaveByName(name, SaveDataType.Character, out var save))
		{
			return !save.IsDeleted;
		}
		return false;
	}

	public void IncrementStat(PlayerStatType stat, float amount = 1f)
	{
		m_playerStats[stat] += amount;
	}

	private static string GetCharacterFolder(FileSource fileSource)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)fileSource != 1)
		{
			return "/characters/";
		}
		return "/characters_local/";
	}

	public static string GetCharacterFolderPath(FileSource fileSource)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Utils.GetSaveDataPath(fileSource) + GetCharacterFolder(fileSource);
	}

	public string GetFilename()
	{
		return m_filename;
	}

	public string GetPath()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetPath(m_fileSource, m_filename);
	}

	public static string GetPath(FileSource fileSource, string name)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetCharacterFolderPath(fileSource) + name + ".fch";
	}

	private WorldPlayerData GetWorldData(long worldUID)
	{
		if (m_worldData.TryGetValue(worldUID, out var value))
		{
			return value;
		}
		value = new WorldPlayerData();
		m_worldData.Add(worldUID, value);
		return value;
	}
}
