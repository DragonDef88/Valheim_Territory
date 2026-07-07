using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Splatform;
using UnityEngine;

public static class FileHelpers
{
	public enum FileHelperType
	{
		Binary,
		Stream
	}

	public enum FileSource
	{
		Auto,
		Local,
		Cloud,
		Legacy
	}

	public struct FileLocation
	{
		public readonly FileSource m_fileSource;

		public readonly string m_path;

		public FileLocation(FileSource fileSource, string path)
		{
			m_fileSource = fileSource;
			m_path = path;
		}
	}

	private static ISaveDataDepot m_mountedDepot;

	private static uint m_depotReferenceCounter;

	private const string m_defaultMountPointName = "Valheim";

	public static LocalStorageSupport LocalStorageSupport => LocalStorageSupport.Supported;

	public static bool LocalStorageSupported => LocalStorageSupport != LocalStorageSupport.Unsupported;

	public static bool CloudStorageSupported => PlatformManager.DistributionPlatform.SaveDataProvider != null;

	public static bool CloudStorageEnabled
	{
		get
		{
			if (CloudStorageSupported)
			{
				return PlatformManager.DistributionPlatform.SaveDataProvider.IsEnabled;
			}
			return false;
		}
	}

	internal static IFileAccess CloudStorage
	{
		get
		{
			if (m_mountedDepot == null)
			{
				ZLog.LogError($"Save data depot isn't mounted (reference counter to mounted depot is {m_depotReferenceCounter})");
			}
			return (IFileAccess)(object)m_mountedDepot;
		}
	}

	public static bool Mount(SaveDataAccess access)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (m_mountedDepot == null)
		{
			ISaveDataProvider saveDataProvider = PlatformManager.DistributionPlatform.SaveDataProvider;
			string text;
			if (saveDataProvider.FixedDepot)
			{
				text = "";
			}
			else
			{
				bool flag = default(bool);
				if (!saveDataProvider.DepotExists("Valheim", ref flag))
				{
					ZLog.LogError("Failed to check if depot exists!");
					return false;
				}
				if (!flag && !saveDataProvider.CreateDepot("Valheim", saveDataProvider.UnallocatedStorage))
				{
					ZLog.LogError("Failed to create mount point with name Valheim!");
					return false;
				}
				text = "Valheim";
			}
			if (!saveDataProvider.MountDepot(text, access, ref m_mountedDepot))
			{
				ZLog.LogError("Mounting failed.");
				return false;
			}
		}
		m_depotReferenceCounter++;
		return true;
	}

	public static void Unmount()
	{
		m_depotReferenceCounter--;
		if (m_depotReferenceCounter == 0)
		{
			if (m_mountedDepot == null)
			{
				ZLog.LogError("Tried to unmount when mounted depot was null!");
			}
			else if (!m_mountedDepot.Unmount((UnmountMode)1))
			{
				ZLog.LogError("Unmounting depot failed!");
			}
			else
			{
				m_mountedDepot = null;
			}
		}
	}

	public static string BytesAsNumberString(ulong bytes, uint decimalCount)
	{
		string[] array = new string[5] { "B", "KB", "MB", "GB", "TB" };
		uint num = 0u;
		float num2 = bytes;
		while (num2 >= 1000f && num < array.Length - 1)
		{
			num++;
			num2 *= 0.001f;
		}
		return num2.ToString("N" + decimalCount) + " " + array[num];
	}

	public static void SplitFilePath(string path, out string directory, out string fileName, out string fileExtension)
	{
		int num = path.LastIndexOfAny(new char[2] { '/', '\\' });
		int num2 = path.LastIndexOf('.');
		directory = ((num < 0) ? "" : path.Substring(0, num + 1));
		if (num2 < 0 || num2 <= num)
		{
			fileName = path.Substring(directory.Length);
			fileExtension = "";
		}
		else
		{
			fileName = path.Substring(directory.Length).Substring(0, num2 - directory.Length);
			fileExtension = path.Substring(directory.Length + fileName.Length);
		}
	}

	public static void ReplaceOldFile(string saveFile, string newFile, string oldFile, FileSource fileSource = FileSource.Auto)
	{
		if (CloudStorageEnabled && (fileSource == FileSource.Auto || fileSource == FileSource.Cloud))
		{
			if (Mount((SaveDataAccess)3))
			{
				bool flag = default(bool);
				if (CloudStorage.FileExists(saveFile, ref flag) && flag)
				{
					bool flag2 = default(bool);
					if (CloudStorage.FileExists(oldFile, ref flag2) && flag2)
					{
						CloudStorage.DeleteFile(oldFile);
					}
					CloudStorage.MoveFile(saveFile, oldFile, (CloudStorageFileGrouping)1);
				}
				CloudStorage.MoveFile(newFile, saveFile, (CloudStorageFileGrouping)1);
			}
			Unmount();
			return;
		}
		if (File.Exists(saveFile))
		{
			if (File.Exists(oldFile))
			{
				File.Delete(oldFile);
			}
			File.Move(saveFile, oldFile);
		}
		File.Move(newFile, saveFile);
	}

	public static void Copy(string source, string dest, FileSource fileSource)
	{
		if (CloudStorageSupported && CloudStorageEnabled && fileSource == FileSource.Cloud && Mount((SaveDataAccess)3))
		{
			CloudStorage.CopyFile(source, dest, (CloudStorageFileGrouping)1);
			Unmount();
		}
		else
		{
			File.Copy(source, dest);
		}
	}

	public static bool Copy(string source, FileSource sourceLocation, string dest, FileSource destLocation = FileSource.Auto)
	{
		if (sourceLocation == FileSource.Auto)
		{
			ZLog.LogError($"Can't copy file from source location {sourceLocation}");
			return false;
		}
		if (destLocation == FileSource.Auto)
		{
			destLocation = sourceLocation;
		}
		if (sourceLocation == FileSource.Cloud == (destLocation == FileSource.Cloud))
		{
			Copy(source, dest, sourceLocation);
		}
		else if (destLocation == FileSource.Cloud)
		{
			if (!FileCopyIntoCloud(source, dest))
			{
				return false;
			}
		}
		else
		{
			FileCopyOutFromCloud(source, dest, deleteOnCloud: false);
		}
		return true;
	}

	public static bool CloudMove(string source, string dest)
	{
		if (CloudStorageSupported)
		{
			ZLog.Log("Cloud Move: " + source + " -> " + dest);
			if (!Mount((SaveDataAccess)3))
			{
				ZLog.LogError("Failed to mount!");
				return false;
			}
			bool flag = default(bool);
			bool flag2 = CloudStorage.FileExists(dest, ref flag);
			if (flag2 && flag)
			{
				flag2 = CloudStorage.DeleteFile(dest);
			}
			if (flag2)
			{
				flag2 = CloudStorage.MoveFile(source, dest, (CloudStorageFileGrouping)1);
			}
			if (!flag2)
			{
				ZLog.LogWarning("Failed to move file!");
			}
			Unmount();
			return flag2;
		}
		ZLog.LogError("Tried to move a file in cloud storage in a build that doesn't support cloud storage.");
		return false;
	}

	public static bool FileCopyIntoCloud(string source, string target)
	{
		byte[] data = File.ReadAllBytes(source);
		return CloudFileWriteInChunks(target, data);
	}

	public static void FileCopyOutFromCloud(string cloudFilePath, string target, bool deleteOnCloud)
	{
		if (CloudStorageSupported)
		{
			if (!Mount((SaveDataAccess)3))
			{
				throw new IOException("Mount failed.");
			}
			bool flag = default(bool);
			if (!(CloudStorage.FileExists(cloudFilePath, ref flag) && flag))
			{
				Unmount();
				throw new FileNotFoundException();
			}
			EnsureDirectoryExists(target);
			byte[] bytes = default(byte[]);
			if (!CloudStorage.ReadFile(cloudFilePath, ref bytes))
			{
				Unmount();
				throw new FileLoadException();
			}
			File.WriteAllBytes(target, bytes);
			if (deleteOnCloud)
			{
				CloudStorage.DeleteFile(cloudFilePath);
			}
			Unmount();
			return;
		}
		ZLog.LogError("Tried to copy a file out from cloud storage in a build that doesn't support cloud storage.");
		throw new FileNotFoundException();
	}

	public static bool FileExistsCloud(string cloudFilePath)
	{
		if (CloudStorageSupported)
		{
			if (!Mount((SaveDataAccess)1))
			{
				return false;
			}
			bool flag = default(bool);
			bool result = CloudStorage.FileExists(cloudFilePath, ref flag) && flag;
			Unmount();
			return result;
		}
		return false;
	}

	public static bool CloudFileWriteInChunks(string pchFile, byte[] data)
	{
		if (CloudStorageSupported)
		{
			if (!Mount((SaveDataAccess)3))
			{
				return false;
			}
			if (!CloudStorage.WriteFile(pchFile, data, (CloudStorageFileGrouping)1, true))
			{
				ZLog.LogError("Cloud save failed!");
				Unmount();
				return false;
			}
			Unmount();
			return true;
		}
		ZLog.LogError("Tried to copy a file out from cloud storage in a build that doesn't support cloud storage.");
		return false;
	}

	public static string GetSourceString(FileSource source)
	{
		switch (source)
		{
		case FileSource.Local:
			return "$settings_localsave";
		case FileSource.Cloud:
			return "$settings_cloudsave";
		case FileSource.Legacy:
			return "$settings_legacysave";
		case FileSource.Auto:
			if (!CloudStorageEnabled)
			{
				return GetSourceString(FileSource.Local);
			}
			return GetSourceString(FileSource.Cloud);
		default:
			throw new Exception();
		}
	}

	public static string[] GetFiles(FileSource fileSource, string path = null, string fileSuffix = null, string searchPattern = null)
	{
		if (CloudStorageSupported && CloudStorageEnabled && (fileSource == FileSource.Auto || fileSource == FileSource.Cloud))
		{
			path = normalizePath(path);
			if (!string.IsNullOrEmpty(searchPattern))
			{
				searchPattern = searchPattern.Replace("*", "").ToLower();
			}
			if (!string.IsNullOrEmpty(fileSuffix))
			{
				fileSuffix = fileSuffix.Replace("*", "").ToLower();
			}
			List<string> list = new List<string>();
			if (!Mount((SaveDataAccess)1))
			{
				ZLog.LogError("Mount failed.");
				return null;
			}
			string[] array = default(string[]);
			if (!CloudStorage.GetAllFilePaths(ref array))
			{
				ZLog.LogError("Failed to get file paths from connected storage!");
				return null;
			}
			Unmount();
			foreach (string text in array)
			{
				string text2 = normalizePath(text);
				if ((string.IsNullOrEmpty(path) || (text2.Length >= path.Length && text2.Substring(0, path.Length) == path)) && (string.IsNullOrEmpty(searchPattern) || Path.GetFileName(text2).Contains(searchPattern)) && (string.IsNullOrEmpty(fileSuffix) || Path.GetExtension(text2) == fileSuffix))
				{
					list.Add(text);
				}
			}
			return list.ToArray();
		}
		if (fileSource == FileSource.Cloud)
		{
			throw new Exception("Cloud not enabled");
		}
		string[] array2 = ((fileSuffix != null) ? Directory.GetFiles(path, fileSuffix) : Directory.GetFiles(path));
		if (searchPattern != null)
		{
			List<string> list2 = array2.ToList();
			for (int num = list2.Count - 1; num >= 0; num--)
			{
				if (!Path.GetFileName(list2[num]).Contains(searchPattern))
				{
					list2.RemoveAt(num);
				}
			}
			array2 = list2.ToArray();
		}
		return array2;
		static string normalizePath(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return str;
			}
			str = str.Replace('\\', '/');
			if (str.Length > 0 && str[0] == '.')
			{
				str = str.Substring(1);
			}
			if (str.Length > 0 && str[0] == '/')
			{
				str = str.Substring(1);
			}
			return str.ToLowerInvariant();
		}
	}

	public static bool IsFileCorrupt(string path, FileSource fileSource)
	{
		if (CloudStorageSupported)
		{
			if (!Mount((SaveDataAccess)1))
			{
				return false;
			}
			bool flag = default(bool);
			bool flag2 = default(bool);
			bool result = CloudStorage.FileExists(path, ref flag, ref flag2) && flag2;
			Unmount();
			return result;
		}
		return false;
	}

	public static string[] GetCorruptFiles(FileSource fileSource)
	{
		if (CloudStorageSupported)
		{
			if (!Mount((SaveDataAccess)1))
			{
				return new string[0];
			}
			string[] result = default(string[]);
			bool corruptFiles = CloudStorage.GetCorruptFiles(ref result);
			Unmount();
			if (corruptFiles)
			{
				return result;
			}
			ZLog.LogError("Failed to get corrupt files!");
		}
		return new string[0];
	}

	public static bool Delete(string path, FileSource fileSource)
	{
		if (fileSource == FileSource.Cloud)
		{
			if (!CloudStorageEnabled)
			{
				return false;
			}
			if (CloudStorageSupported)
			{
				if (!Mount((SaveDataAccess)3))
				{
					ZLog.LogError("Failed to mount.");
					return false;
				}
				bool flag = default(bool);
				bool result = CloudStorage.FileExists(path, ref flag) && flag && CloudStorage.DeleteFile(path);
				Unmount();
				return result;
			}
			return false;
		}
		if (!File.Exists(path))
		{
			return false;
		}
		File.Delete(path);
		return true;
	}

	public static DateTime GetLastWriteTime(string path, FileSource fileSource)
	{
		if (CloudStorageSupported)
		{
			if (CloudStorageEnabled && (fileSource == FileSource.Auto || fileSource == FileSource.Cloud) && Mount((SaveDataAccess)1))
			{
				DateTime dateTime = default(DateTime);
				CloudStorage.GetLastModifiedTimeStamp(path, ref dateTime);
				Unmount();
				return dateTime.ToLocalTime();
			}
		}
		else if (CloudStorageEnabled && (fileSource == FileSource.Auto || fileSource == FileSource.Cloud))
		{
			ZLog.LogError("Tried to get the last write time of a file in cloud storage in a build that doesn't support cloud storage.");
		}
		return File.GetLastWriteTime(path);
	}

	public static ulong GetTotalCloudUsage()
	{
		if (CloudStorageEnabled)
		{
			if (CloudStorageSupported)
			{
				if (Mount((SaveDataAccess)1))
				{
					ulong result = default(ulong);
					bool storageUsage = CloudStorage.GetStorageUsage(ref result);
					Unmount();
					if (storageUsage)
					{
						return result;
					}
				}
			}
			else
			{
				ZLog.LogError("Tried to get used cloud storage space in a build that doesn't support cloud storage.");
			}
		}
		return 0uL;
	}

	public static ulong GetTotalCloudCapacity()
	{
		if (CloudStorageEnabled)
		{
			if (CloudStorageSupported)
			{
				if (Mount((SaveDataAccess)1))
				{
					ulong result = default(ulong);
					bool storageCapacity = CloudStorage.GetStorageCapacity(ref result);
					Unmount();
					if (storageCapacity)
					{
						return result;
					}
				}
			}
			else
			{
				ZLog.LogError("Tried to get total quota of cloud storage in a build that doesn't support cloud storage.");
			}
		}
		return 0uL;
	}

	public static long GetRemainingCloudCapacity()
	{
		if (CloudStorageEnabled)
		{
			if (CloudStorageSupported)
			{
				if (Mount((SaveDataAccess)1))
				{
					long result = default(long);
					bool remainingStorageCapacity = CloudStorage.GetRemainingStorageCapacity(ref result);
					Unmount();
					if (remainingStorageCapacity)
					{
						return result;
					}
				}
			}
			else
			{
				ZLog.LogError("Tried to get remaining quota of cloud storage in a build that doesn't support cloud storage.");
			}
		}
		return 0L;
	}

	public static ulong GetFileSize(string path, FileSource fileSource)
	{
		if (CloudStorageEnabled && fileSource == FileSource.Cloud)
		{
			if (CloudStorageSupported)
			{
				if (Mount((SaveDataAccess)1))
				{
					ulong result = default(ulong);
					bool fileSize = CloudStorage.GetFileSize(path, ref result);
					Unmount();
					if (fileSize)
					{
						return result;
					}
				}
				return 0uL;
			}
			ZLog.LogError("Tried to get the size of a file in cloud storage in a build that doesn't support cloud storage.");
			return 0uL;
		}
		return (ulong)new FileInfo(path).Length;
	}

	public static bool OperationExceedsCloudCapacity(ulong requiredBytes)
	{
		return GetRemainingCloudCapacity() < (long)requiredBytes;
	}

	public static void CheckDiskSpace(string worldSavePath, string playerProfileSavePath, FileSource worldFileSource, FileSource playerFileSource, out ulong availableFreeSpace, out ulong byteLimitWarning, out ulong byteLimitBlock)
	{
		ulong num = ((!Exists(worldSavePath, worldFileSource)) ? 104857600 : GetFileSize(worldSavePath, worldFileSource));
		ulong num2 = ((!Exists(playerProfileSavePath, playerFileSource)) ? 2097152 : GetFileSize(playerProfileSavePath, playerFileSource));
		availableFreeSpace = ulong.MaxValue;
		byteLimitWarning = (num + num2) * 4;
		byteLimitBlock = (num + num2) * 2;
		if (LocalStorageSupport == LocalStorageSupport.Unsupported)
		{
			ZLog.Log("Local storage is not supported at all - do not attempt to query local storage.");
			return;
		}
		if (string.IsNullOrEmpty(worldSavePath) || worldFileSource == FileSource.Cloud)
		{
			worldSavePath = Application.persistentDataPath;
		}
		string text = "";
		try
		{
			text = Path.GetDirectoryName(worldSavePath);
		}
		catch (Exception)
		{
			ZLog.LogError("Could not get directory name for " + worldSavePath + "!");
			return;
		}
		if (CloudStorageEnabled && (worldFileSource == FileSource.Cloud || worldFileSource == FileSource.Auto || worldFileSource == FileSource.Legacy))
		{
			text = GetSteamPathWin();
		}
		availableFreeSpace = GetFreeSpaceWindows(text);
		ZLog.Log($"Available space to current user: {availableFreeSpace}. Saving is blocked if below: {byteLimitBlock} bytes. Warnings are given if below: {byteLimitWarning}");
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

	public static ulong GetFreeSpaceWindows(string folderName)
	{
		if (!GetDiskFreeSpaceEx(folderName, out var lpFreeBytesAvailable, out var _, out var _))
		{
			Debug.LogError((object)"Error encountered while getting free disk space - returning max amount of disk space in order to not block saving.");
			return ulong.MaxValue;
		}
		return lpFreeBytesAvailable;
	}

	public static string GetSteamPathWin()
	{
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			Type type = Type.GetType("Microsoft.Win32.Registry, mscorlib");
			if (type != null)
			{
				MethodInfo method = type.GetMethod("GetValue", new Type[3]
				{
					typeof(string),
					typeof(string),
					typeof(object)
				});
				if (method != null)
				{
					string text = (string)method.Invoke(null, new object[3] { "HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null });
					if (string.IsNullOrEmpty(text))
					{
						text = (string)method.Invoke(null, new object[3] { "HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "InstallPath", null });
					}
					return text;
				}
			}
		}
		return Application.persistentDataPath;
	}

	public static bool Exists(string path, FileSource fileSource)
	{
		if (CloudStorageSupported)
		{
			if (CloudStorageEnabled && (fileSource == FileSource.Auto || fileSource == FileSource.Cloud) && Mount((SaveDataAccess)1))
			{
				bool flag = default(bool);
				bool result = CloudStorage.FileExists(path, ref flag) && flag;
				Unmount();
				return result;
			}
		}
		else if (CloudStorageEnabled && (fileSource == FileSource.Auto || fileSource == FileSource.Cloud))
		{
			return false;
		}
		return File.Exists(path);
	}

	public static void EnsureDirectoryExists(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
	}

	public static void MigrateLocalSyncedListsToCloud()
	{
		if (LocalStorageSupport == LocalStorageSupport.NotPersistent)
		{
			string text = Utils.GetSaveDataPath(FileSource.Local) + "/bannedlist.txt";
			string text2 = Utils.GetSaveDataPath(FileSource.Cloud) + "/bannedlist.txt";
			if (Exists(text, FileSource.Local) && !Exists(text2, FileSource.Cloud) && FileCopyIntoCloud(text, text2) && Delete(text, FileSource.Local))
			{
				ZLog.Log("Successfully migrated list of banned players from " + text + " to " + text2 + ".");
			}
		}
	}
}
