using System;
using System.ComponentModel;
using System.Threading;
using Splatform;
using Steamworks;
using UnityEngine;

public static class PlatformPrefs
{
	public const string c_PreferencesPath = "Preferences";

	private static readonly SemaphoreSlim s_saveSemaphore = new SemaphoreSlim(1);

	public static float GetFloat(string name, float defaultValue = 0f)
	{
		float value = default(float);
		if (TryGetPreferencesProvider(out var preferencesProvider))
		{
			if (!preferencesProvider.TryGetFloat(name, ref value))
			{
				return defaultValue;
			}
			return value;
		}
		if (!TryGetFloatFromPlayerPrefsMigrate(name, out value))
		{
			return defaultValue;
		}
		return value;
	}

	public static void SetFloat(string name, float value)
	{
		if (TryGetPreferencesProvider(out var preferencesProvider))
		{
			preferencesProvider.SetFloat(name, value);
		}
		else
		{
			PlayerPrefs.SetFloat(name, value);
		}
	}

	public static int GetInt(string name, int defaultValue = 0)
	{
		int value = default(int);
		if (TryGetPreferencesProvider(out var preferencesProvider))
		{
			if (!preferencesProvider.TryGetInt(name, ref value))
			{
				return defaultValue;
			}
			return value;
		}
		if (!TryGetIntFromPlayerPrefsMigrate(name, out value))
		{
			return defaultValue;
		}
		return value;
	}

	public static void SetInt(string name, int value)
	{
		if (TryGetPreferencesProvider(out var preferencesProvider))
		{
			preferencesProvider.SetInt(name, value);
		}
		else
		{
			PlayerPrefs.SetInt(name, value);
		}
	}

	public static string GetString(string name, string defaultValue = "")
	{
		string value = default(string);
		if (TryGetPreferencesProvider(out var preferencesProvider))
		{
			if (!preferencesProvider.TryGetString(name, ref value))
			{
				return defaultValue;
			}
			return value;
		}
		if (!TryGetStringFromPlayerPrefsMigrate(name, out value))
		{
			return defaultValue;
		}
		return value;
	}

	public static void SetString(string name, string value)
	{
		if (TryGetPreferencesProvider(out var preferencesProvider))
		{
			preferencesProvider.SetString(name, value);
		}
		else
		{
			PlayerPrefs.SetString(name, value);
		}
	}

	public static bool GetBool(string name, bool defaultValue = false)
	{
		return GetInt(name, defaultValue ? 1 : 0) == 1;
	}

	public static void SetBool(string name, bool value)
	{
		SetInt(name, value ? 1 : 0);
	}

	public static bool HasKey(string name)
	{
		if (TryGetPreferencesProvider(out var preferencesProvider))
		{
			return preferencesProvider.HasKey(name);
		}
		return HasKeyInPlayerPrefsMigrate(name);
	}

	public static void DeleteKey(string name)
	{
		if (TryGetPreferencesProvider(out var preferencesProvider))
		{
			preferencesProvider.DeleteKey(name);
		}
		else
		{
			DeleteKeyInPlayerPrefsMigrate(name);
		}
	}

	public static void DeleteAll()
	{
		if (TryGetPreferencesProvider(out var preferencesProvider))
		{
			preferencesProvider.DeleteAll();
		}
		else
		{
			PlayerPrefs.DeleteAll();
		}
	}

	public static void Save()
	{
		if (!TryGetPreferencesProvider(out var preferencesProvider))
		{
			PlayerPrefs.Save();
			return;
		}
		byte[] data = null;
		preferencesProvider.Serialize(ref data);
		s_saveSemaphore.Wait();
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		bool savedSuccessfully = false;
		backgroundWorker.DoWork += delegate
		{
			if (data != null)
			{
				ZLog.Log("Writing prefs to disk!");
				savedSuccessfully = TryWritePreferencesToSaveData(data);
			}
			s_saveSemaphore.Release();
		};
		backgroundWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				ZLog.LogError($"Preferences save failed due to exception: {e.Error}");
			}
			else if (savedSuccessfully)
			{
				ZLog.Log("Preferences saved successfully");
			}
			else
			{
				ZLog.LogError("Preferences save failed!");
			}
		};
		backgroundWorker.RunWorkerAsync();
	}

	private static bool TryGetPreferencesProvider(out IPreferencesProvider preferencesProvider)
	{
		IDistributionPlatform distributionPlatform = PlatformManager.DistributionPlatform;
		return (preferencesProvider = ((distributionPlatform != null) ? distributionPlatform.PreferencesProvider : null)) != null;
	}

	private static bool TryWritePreferencesToSaveData(byte[] data)
	{
		FileWriter fileWriter = null;
		try
		{
			fileWriter = new FileWriter("Preferences", FileHelpers.FileHelperType.Binary, FileHelpers.FileSource.Cloud);
			fileWriter.m_binary.Write(data.Length);
			fileWriter.m_binary.Write(data);
		}
		catch (Exception ex)
		{
			ZLog.LogError("Exception while saving preferences - " + ex.Message);
			return false;
		}
		finally
		{
			fileWriter?.Finish();
		}
		return true;
	}

	private static bool TryGetFloatFromPlayerPrefsMigrate(string name, out float value)
	{
		if (TryGetFloatFromPlayerPrefs(name, out value))
		{
			return true;
		}
		if (!MigratePlatformKeyIfNeeded(name, out var prefixedKey))
		{
			return false;
		}
		return TryGetFloatFromPlayerPrefs(prefixedKey, out value);
	}

	private static bool TryGetIntFromPlayerPrefsMigrate(string name, out int value)
	{
		if (TryGetIntFromPlayerPrefs(name, out value))
		{
			return true;
		}
		if (!MigratePlatformKeyIfNeeded(name, out var prefixedKey))
		{
			return false;
		}
		return TryGetIntFromPlayerPrefs(prefixedKey, out value);
	}

	private static bool TryGetStringFromPlayerPrefsMigrate(string name, out string value)
	{
		if (TryGetStringFromPlayerPrefs(name, out value))
		{
			return true;
		}
		if (!MigratePlatformKeyIfNeeded(name, out var prefixedKey))
		{
			return false;
		}
		return TryGetStringFromPlayerPrefs(prefixedKey, out value);
	}

	private static bool HasKeyInPlayerPrefsMigrate(string name)
	{
		if (PlayerPrefs.HasKey(name))
		{
			return true;
		}
		if (!MigratePlatformKeyIfNeeded(name, out var prefixedKey))
		{
			return false;
		}
		return PlayerPrefs.HasKey(prefixedKey);
	}

	private static void DeleteKeyInPlayerPrefsMigrate(string name)
	{
		PlayerPrefs.DeleteKey(name);
		if (MigratePlatformKeyIfNeeded(name, out var prefixedKey))
		{
			PlayerPrefs.DeleteKey(prefixedKey);
		}
	}

	private static bool TryGetFloatFromPlayerPrefs(string name, out float value)
	{
		value = PlayerPrefs.GetFloat(name, 0f);
		if (value != 0f)
		{
			return true;
		}
		return value == PlayerPrefs.GetFloat(name, -1f);
	}

	private static bool TryGetIntFromPlayerPrefs(string name, out int value)
	{
		value = PlayerPrefs.GetInt(name, 0);
		if (value != 0)
		{
			return true;
		}
		return value == PlayerPrefs.GetInt(name, -1);
	}

	private static bool TryGetStringFromPlayerPrefs(string name, out string value)
	{
		value = PlayerPrefs.GetString(name, string.Empty);
		if (value != string.Empty)
		{
			return true;
		}
		return value == PlayerPrefs.GetString(name, "0");
	}

	private static bool MigratePlatformKeyIfNeeded(string key, out string prefixedKey)
	{
		if (SteamUtils.IsSteamRunningOnSteamDeck())
		{
			prefixedKey = "deck_" + key;
			return true;
		}
		prefixedKey = null;
		return false;
	}
}
