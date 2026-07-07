using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class World
{
	public enum SaveDataError
	{
		None,
		BadVersion,
		LoadError,
		Corrupt,
		MissingMeta,
		MissingDB
	}

	public string m_fileName = "";

	public string m_name = "";

	public string m_seedName = "";

	public int m_seed;

	public long m_uid;

	public List<string> m_startingGlobalKeys = new List<string>();

	public bool m_startingKeysChanged;

	public int m_worldGenVersion;

	public int m_worldVersion;

	public bool m_menu;

	public bool m_needsDB;

	public bool m_createBackupBeforeSaving;

	public SaveWithBackups saves;

	public SaveDataError m_dataError;

	public FileSource m_fileSource = (FileSource)1;

	public World()
	{
	}//IL_002e: Unknown result type (might be due to invalid IL or missing references)


	public World(SaveWithBackups save, SaveDataError dataError)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		m_fileName = (m_name = save.m_name);
		m_dataError = dataError;
		m_fileSource = save.PrimaryFile.m_source;
	}

	public World(string name, string seed)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		m_fileName = (m_name = name);
		m_seedName = seed;
		m_seed = ((!(m_seedName == "")) ? StringExtensionMethods.GetStableHashCode(m_seedName) : 0);
		m_uid = StringExtensionMethods.GetStableHashCode(name) + Utils.GenerateUID();
		m_worldGenVersion = 2;
	}

	public static string GetWorldSavePath(FileSource fileSource = 0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		return Utils.GetSaveDataPath(fileSource) + (((int)fileSource == 1) ? "/worlds_local" : "/worlds");
	}

	public static void RemoveWorld(string name, FileSource fileSource)
	{
		if (SaveSystem.TryGetSaveByName(name, SaveDataType.World, out var save) && !save.IsDeleted)
		{
			SaveSystem.Delete(save.PrimaryFile);
		}
	}

	public string GetRootPath(FileSource fileSource)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetWorldSavePath(fileSource) + "/" + m_fileName;
	}

	public string GetDBPath()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetDBPath(m_fileSource);
	}

	public string GetDBPath(FileSource fileSource)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetWorldSavePath(fileSource) + "/" + m_fileName + ".db";
	}

	public static string GetDBPath(string name, FileSource fileSource = 0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetWorldSavePath(fileSource) + "/" + name + ".db";
	}

	public string GetMetaPath()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetMetaPath(m_fileSource);
	}

	public string GetMetaPath(FileSource fileSource)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetWorldSavePath(fileSource) + "/" + m_fileName + ".fwl";
	}

	public static string GetMetaPath(string name, FileSource fileSource = 0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetWorldSavePath(fileSource) + "/" + name + ".fwl";
	}

	public static bool HaveWorld(string name)
	{
		if (SaveSystem.TryGetSaveByName(name, SaveDataType.World, out var save))
		{
			return !save.IsDeleted;
		}
		return false;
	}

	public static World GetMenuWorld()
	{
		return new World("menu", "")
		{
			m_menu = true
		};
	}

	public static World GetEditorWorld()
	{
		return new World("editor", "");
	}

	public static string GenerateSeed()
	{
		string text = "";
		for (int i = 0; i < 10; i++)
		{
			text += "abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ023456789"[Random.Range(0, "abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ023456789".Length)];
		}
		return text;
	}

	public static World GetCreateWorld(string name, FileSource source)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		ZLog.Log((object)("Get create world " + name));
		World world;
		if (SaveSystem.TryGetSaveByName(name, SaveDataType.World, out var save) && !save.IsDeleted)
		{
			world = LoadWorld(save);
			if (world.m_dataError == SaveDataError.None)
			{
				return world;
			}
			ZLog.LogError((object)$"Failed to load world with name \"{name}\", data error {world.m_dataError}.");
		}
		ZLog.Log((object)" creating");
		world = new World(name, GenerateSeed());
		world.m_fileSource = source;
		world.SaveWorldMetaData(DateTime.Now);
		return world;
	}

	public static World GetDevWorld()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		World world;
		if (SaveSystem.TryGetSaveByName(Game.instance.m_devWorldName, SaveDataType.World, out var save) && !save.IsDeleted)
		{
			world = LoadWorld(save);
			if (world.m_dataError == SaveDataError.None)
			{
				return world;
			}
			ZLog.Log((object)$"Failed to load dev world, data error {world.m_dataError}. Creating...");
		}
		world = new World(Game.instance.m_devWorldName, Game.instance.m_devWorldSeed);
		world.m_fileSource = (FileSource)1;
		world.SaveWorldMetaData(DateTime.Now);
		return world;
	}

	public void SaveWorldMetaData(DateTime backupTimestamp)
	{
		SaveWorldMetaData(backupTimestamp, considerBackup: true, out var _, out var _);
	}

	public void SaveWorldMetaData(DateTime now, bool considerBackup, out bool cloudSaveFailed, out FileWriter metaWriter)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Invalid comparison between Unknown and I4
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Invalid comparison between Unknown and I4
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Invalid comparison between Unknown and I4
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Invalid comparison between Unknown and I4
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		GetDBPath();
		SaveSystem.CheckMove(m_fileName, SaveDataType.World, ref m_fileSource, now, 0uL);
		ZPackage zPackage = new ZPackage();
		zPackage.Write(37);
		zPackage.Write(m_name);
		zPackage.Write(m_seedName);
		zPackage.Write(m_seed);
		zPackage.Write(m_uid);
		zPackage.Write(m_worldGenVersion);
		zPackage.Write(m_needsDB);
		zPackage.Write(m_startingGlobalKeys.Count);
		for (int i = 0; i < m_startingGlobalKeys.Count; i++)
		{
			zPackage.Write(m_startingGlobalKeys[i]);
		}
		if ((int)m_fileSource != 2)
		{
			Directory.CreateDirectory(GetWorldSavePath(m_fileSource));
		}
		string metaPath = GetMetaPath();
		string text = metaPath + ".new";
		string text2 = metaPath + ".old";
		byte[] array = zPackage.GetArray();
		bool flag = (int)m_fileSource == 2;
		FileWriter val = new FileWriter(flag ? metaPath : text, (FileHelperType)0, m_fileSource);
		val.m_binary.Write(array.Length);
		val.m_binary.Write(array);
		val.Finish();
		SaveSystem.InvalidateCache();
		cloudSaveFailed = (int)val.Status != 2 && (int)m_fileSource == 2;
		if (!cloudSaveFailed)
		{
			if (!flag)
			{
				FileHelpers.ReplaceOldFile(metaPath, text, text2, m_fileSource);
				SaveSystem.InvalidateCache();
			}
			if (considerBackup)
			{
				ZNet.ConsiderAutoBackup(m_fileName, SaveDataType.World, now);
			}
		}
		metaWriter = val;
	}

	public static World LoadWorld(SaveWithBackups saveFile)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		FileReader val = null;
		if (saveFile.IsDeleted)
		{
			ZLog.Log((object)("save deleted " + saveFile.m_name));
			return new World(saveFile, SaveDataError.LoadError);
		}
		FileSource source = saveFile.PrimaryFile.m_source;
		string pathPrimary = saveFile.PrimaryFile.PathPrimary;
		string text = ((saveFile.PrimaryFile.PathsAssociated.Length != 0) ? saveFile.PrimaryFile.PathsAssociated[0] : null);
		if (FileHelpers.IsFileCorrupt(pathPrimary, source) || (text != null && FileHelpers.IsFileCorrupt(text, source)))
		{
			ZLog.Log((object)("  corrupt save " + saveFile.m_name));
			return new World(saveFile, SaveDataError.Corrupt);
		}
		try
		{
			val = new FileReader(pathPrimary, source, (FileHelperType)0);
		}
		catch (Exception ex)
		{
			if (val != null)
			{
				val.Dispose();
			}
			ZLog.Log((object)("  failed to load " + saveFile.m_name + " Exception: " + ex));
			return new World(saveFile, SaveDataError.LoadError);
		}
		try
		{
			BinaryReader binary = val.m_binary;
			int count = binary.ReadInt32();
			ZPackage zPackage = new ZPackage(binary.ReadBytes(count));
			int num = zPackage.ReadInt();
			if (!Version.IsWorldVersionCompatible(num))
			{
				ZLog.Log((object)("incompatible world version " + num));
				return new World(saveFile, SaveDataError.BadVersion);
			}
			World world = new World();
			world.m_fileSource = source;
			world.m_fileName = saveFile.m_name;
			world.m_name = zPackage.ReadString();
			world.m_seedName = zPackage.ReadString();
			world.m_seed = zPackage.ReadInt();
			world.m_uid = zPackage.ReadLong();
			world.m_worldVersion = num;
			if (num >= 26)
			{
				world.m_worldGenVersion = zPackage.ReadInt();
			}
			world.m_needsDB = num >= 30 && zPackage.ReadBool();
			if (num != 37)
			{
				world.m_createBackupBeforeSaving = true;
			}
			if (world.CheckDbFile())
			{
				world.m_dataError = SaveDataError.MissingDB;
			}
			if (num >= 32)
			{
				int num2 = zPackage.ReadInt();
				for (int i = 0; i < num2; i++)
				{
					world.m_startingGlobalKeys.Add(zPackage.ReadString());
				}
			}
			return world;
		}
		catch
		{
			ZLog.LogWarning((object)("  error loading world " + saveFile.m_name));
			return new World(saveFile, SaveDataError.LoadError);
		}
		finally
		{
			if (val != null)
			{
				val.Dispose();
			}
		}
	}

	private bool CheckDbFile()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (m_needsDB)
		{
			return !FileHelpers.Exists(GetDBPath(), m_fileSource);
		}
		return false;
	}
}
