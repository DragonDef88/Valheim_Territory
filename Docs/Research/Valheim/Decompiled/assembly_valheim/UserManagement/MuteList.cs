using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Splatform;
using UnityEngine;

namespace UserManagement;

public static class MuteList
{
	private static readonly HashSet<PlatformUserID> _mutedUsers = new HashSet<PlatformUserID>();

	private static bool _hasBeenLoaded;

	private static bool _isLoading;

	private static readonly string _block_list_file_name = "blocked_players";

	private static readonly string _block_list_file_name_noncloud = Path.Combine(Application.persistentDataPath, _block_list_file_name) + ".txt";

	public static bool Contains(PlatformUserID user)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return _mutedUsers.Contains(user);
	}

	public static void Block(PlatformUserID user)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (PlatformManager.DistributionPlatform != null)
		{
			if (!((PlatformUserID)(ref user)).IsValid)
			{
				Debug.LogError((object)"User was invalid!");
			}
			else if (user == ((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID)
			{
				Debug.LogError((object)"Local user was added to the block list! This should never happen! Ignoring.");
			}
			else if (!_mutedUsers.Contains(user))
			{
				_mutedUsers.Add(user);
			}
		}
	}

	public static void Unblock(PlatformUserID user)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (_mutedUsers.Contains(user))
		{
			_mutedUsers.Remove(user);
		}
	}

	public static string GetBlockListFileName()
	{
		if (!FileHelpers.CloudStorageEnabled)
		{
			return _block_list_file_name_noncloud;
		}
		return _block_list_file_name;
	}

	public static void Persist()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (_mutedUsers.Count > 0)
		{
			byte[] buffer = Encode();
			if (FileHelpers.LocalStorageSupported)
			{
				FileWriter val = new FileWriter(_block_list_file_name_noncloud, (FileHelperType)0, (FileSource)1);
				Debug.Log((object)"writing banned users to local file storage");
				val.m_binary.Write(buffer);
				val.Finish();
			}
			if (FileHelpers.CloudStorageEnabled)
			{
				FileWriter val2 = new FileWriter(_block_list_file_name, (FileHelperType)0, (FileSource)2);
				Debug.Log((object)"Writing banned users to cloud file storage.");
				val2.m_binary.Write(buffer);
				val2.Finish();
			}
		}
	}

	public static void Load(Action onLoaded)
	{
		if (_isLoading)
		{
			return;
		}
		if (!_hasBeenLoaded)
		{
			string users = "";
			DateTime lastWriteTime = DateTime.UnixEpoch;
			string users2 = "";
			DateTime lastWriteTime2 = DateTime.UnixEpoch;
			if (FileHelpers.Exists(_block_list_file_name, (FileSource)2) && TryCreateFileReader(onLoaded, _block_list_file_name, (FileSource)2, out var file, out lastWriteTime2))
			{
				TryReadBlockListFromFile(file, _block_list_file_name, out users2);
			}
			if (FileHelpers.Exists(_block_list_file_name_noncloud, (FileSource)1) && TryCreateFileReader(onLoaded, _block_list_file_name_noncloud, (FileSource)1, out var file2, out lastWriteTime))
			{
				TryReadBlockListFromFile(file2, _block_list_file_name_noncloud, out users);
			}
			if (!string.IsNullOrEmpty(users) && !string.IsNullOrEmpty(users2))
			{
				Debug.Log((object)$"DateTime for cloudWrite: {lastWriteTime2.ToString()}, for local: {lastWriteTime.ToString()}. DateTime.Compare: {DateTime.Compare(lastWriteTime2, lastWriteTime)}");
				if (DateTime.Compare(lastWriteTime2, lastWriteTime) >= 0)
				{
					BlockUsers(users2);
				}
				else
				{
					BlockUsers(users);
				}
			}
			else if (!string.IsNullOrEmpty(users))
			{
				Debug.Log((object)("there was no cloud users, instead using localusers! " + users));
				BlockUsers(users);
			}
			else if (!string.IsNullOrEmpty(users2))
			{
				Debug.Log((object)("there was no local user, instead using cloud users! " + users2));
				BlockUsers(users2);
			}
			_hasBeenLoaded = true;
			onLoaded?.Invoke();
		}
		else
		{
			onLoaded?.Invoke();
		}
	}

	private static bool TryCreateFileReader(Action onLoaded, string path, FileSource fileSource, out FileReader file, out DateTime lastWriteTime)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_isLoading = true;
		try
		{
			file = new FileReader(path, fileSource, (FileHelperType)1);
			lastWriteTime = FileHelpers.GetLastWriteTime(path, fileSource);
		}
		catch (Exception ex)
		{
			ZLog.Log((object)("Failed to load: " + path + " (" + ex.Message + ")"));
			_isLoading = false;
			_hasBeenLoaded = true;
			onLoaded?.Invoke();
			file = null;
			lastWriteTime = DateTime.UnixEpoch;
			return false;
		}
		return true;
	}

	private static void TryReadBlockListFromFile(FileReader file, string fileName, out string users)
	{
		try
		{
			StreamReader stream = file.m_stream;
			users = stream.ReadToEnd();
			Debug.Log((object)("now getting block list from file " + fileName + ". got these users: " + users));
		}
		catch (Exception ex)
		{
			ZLog.LogError((object)("error loading blocked_players. FileName: " + fileName + ", Error: " + ex.Message));
			file.Dispose();
			users = null;
		}
		file.Dispose();
		_isLoading = false;
	}

	private static byte[] Encode()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		foreach (PlatformUserID mutedUser in _mutedUsers)
		{
			stringBuilder.Append(mutedUser).Append('\n');
		}
		return Encoding.UTF8.GetBytes(stringBuilder.ToString());
	}

	private static void BlockUsers(string textUsers)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		_mutedUsers.Clear();
		string[] array = textUsers.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			Block(new PlatformUserID(array[i]));
		}
	}
}
