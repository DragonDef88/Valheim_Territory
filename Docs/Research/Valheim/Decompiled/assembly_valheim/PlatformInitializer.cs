using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Splatform;
using UnityEngine;

public static class PlatformInitializer
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static InitializeSaveDataCompletedHandler _003C_003E9__26_0;

		public static InitializePreferencesCompletedHandler _003C_003E9__27_2;

		internal void _003CInitializeSaveDataStorage_003Eb__26_0(bool succeeded)
		{
			if (!succeeded)
			{
				return;
			}
			if (FileHelpers.LocalStorageSupported)
			{
				string[] files = FileHelpers.GetFiles((FileSource)1, Utils.GetSaveDataPath((FileSource)1), (string)null, (string)null);
				string text = "All files in local storage save data:";
				for (int i = 0; i < files.Length; i++)
				{
					text += $"\n{files[i]} ({FileHelpers.GetFileSize(files[i], (FileSource)1)})";
				}
				ZLog.Log((object)text);
			}
			else
			{
				ZLog.Log((object)"Local storage is not supported");
			}
			if (FileHelpers.CloudStorageSupported && FileHelpers.CloudStorageEnabled)
			{
				string[] files = FileHelpers.GetFiles((FileSource)2, Utils.GetSaveDataPath((FileSource)2), (string)null, (string)null);
				string text = "All files in platform save data:";
				for (int j = 0; j < files.Length; j++)
				{
					text += $"\n{files[j]} ({FileHelpers.GetFileSize(files[j], (FileSource)2)})";
				}
				ZLog.Log((object)text);
			}
			else
			{
				ZLog.Log((object)"Cloud storage is not supported or enabled");
			}
			InitializeAndLoadPrefs();
		}

		internal void _003CInitializeAndLoadPrefs_003Eb__27_2(bool succeeded)
		{
			if (!succeeded)
			{
				ZLog.LogError((object)"Failed to initialize preferences provider");
			}
			else
			{
				ZLog.Log((object)"Preferences initialized successfully!");
			}
		}
	}

	private static bool s_platformInitialized = false;

	private static bool s_startedStorageInitialization = false;

	private static bool s_allowStorageInitialization = true;

	private static bool s_inputDeviceRequired = false;

	public static bool PlatformInitialized => s_platformInitialized;

	public static bool StartedSaveDataInitialization => s_startedStorageInitialization;

	public static bool PreferencesInitialized
	{
		get
		{
			if (PlatformManager.DistributionPlatform.PreferencesProvider != null)
			{
				return PlatformManager.DistributionPlatform.PreferencesProvider.IsInitialized;
			}
			return true;
		}
	}

	public static bool SaveDataInitialized
	{
		get
		{
			if (PlatformManager.DistributionPlatform.SaveDataProvider == null || !PlatformManager.DistributionPlatform.SaveDataProvider.IsEnabled || PlatformManager.DistributionPlatform.SaveDataProvider.IsInitialized)
			{
				return PreferencesInitialized;
			}
			return false;
		}
	}

	public static bool AllowSaveDataInitialization
	{
		get
		{
			return s_allowStorageInitialization;
		}
		set
		{
			s_allowStorageInitialization = value;
			if (s_allowStorageInitialization && !s_startedStorageInitialization)
			{
				InitializeSaveDataStorage();
			}
		}
	}

	public static bool InputDeviceRequired
	{
		get
		{
			return s_inputDeviceRequired;
		}
		set
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			s_inputDeviceRequired = value;
			if (PlatformManager.DistributionPlatform != null && PlatformManager.DistributionPlatform.InputDeviceManager != null)
			{
				PlatformManager.DistributionPlatform.InputDeviceManager.SetInputDeviceRequiredForLocalUser(false, new CheckKeyboardMouseConnectedFunc(ZInput.CheckKeyboardMouseConnected), (UIDisplayedHandler)null, (GamepadAssignmentUpdatedHandler)null);
			}
		}
	}

	public static bool WaitingForInputDevice
	{
		get
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (PlatformManager.DistributionPlatform == null)
			{
				return false;
			}
			if (PlatformManager.DistributionPlatform.InputDeviceManager == null)
			{
				return false;
			}
			return !PlatformManager.DistributionPlatform.InputDeviceManager.HasInputDeviceAssociation(((IUser)PlatformManager.DistributionPlatform.LocalUser).PlatformUserID);
		}
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void EarlyInitialize()
	{
		if (!Application.isEditor)
		{
			GameObject.Find("");
		}
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void InitializePlatform()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		SetMainThreadName();
		ParseArguments();
		PlatformConfiguration val = default(PlatformConfiguration);
		SteamManager.Initialize();
		((PlatformConfiguration)(ref val)).SetBool("managesteamruntime", false);
		((PlatformConfiguration)(ref val)).SetUIntArray("acceptedappids", new uint[2] { 1223920u, 892970u });
		Logger.SetLogHandler(new LogHandler(OnSplatformLog));
		PlatformManager.InitializeAsync(val, new InitializeCompletedHandler(OnInitializeCompleted));
	}

	private static void SetMainThreadName()
	{
		if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
		{
			Thread.CurrentThread.Name = "MainValheimThread";
		}
	}

	private static void ParseArguments()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			_ = commandLineArgs[i];
		}
	}

	private static void OnInitializeCompleted(bool succeeded)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		if (!succeeded)
		{
			ZLog.LogError((object)"Failed to initialize platform!");
			Application.Quit();
			return;
		}
		SuspendManager.Initialize();
		s_platformInitialized = true;
		ZLog.Log((object)"Initialized platform!");
		PlatformManager.DistributionPlatform.LocalUser.SignedIn += new SignedInHandler(OnLoginCompleted);
	}

	private static void OnLoginCompleted()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		PlatformManager.DistributionPlatform.LocalUser.SignedIn -= new SignedInHandler(OnLoginCompleted);
		MatchmakingManager.Initialize();
		if (s_allowStorageInitialization)
		{
			InitializeSaveDataStorage();
		}
	}

	private static void InitializeSaveDataStorage()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		s_startedStorageInitialization = true;
		if (PlatformManager.DistributionPlatform.SaveDataProvider == null)
		{
			return;
		}
		ISaveDataProvider saveDataProvider = PlatformManager.DistributionPlatform.SaveDataProvider;
		object obj = _003C_003Ec._003C_003E9__26_0;
		if (obj == null)
		{
			InitializeSaveDataCompletedHandler val = delegate(bool succeeded)
			{
				if (succeeded)
				{
					if (FileHelpers.LocalStorageSupported)
					{
						string[] files = FileHelpers.GetFiles((FileSource)1, Utils.GetSaveDataPath((FileSource)1), (string)null, (string)null);
						string text = "All files in local storage save data:";
						for (int i = 0; i < files.Length; i++)
						{
							text += $"\n{files[i]} ({FileHelpers.GetFileSize(files[i], (FileSource)1)})";
						}
						ZLog.Log((object)text);
					}
					else
					{
						ZLog.Log((object)"Local storage is not supported");
					}
					if (FileHelpers.CloudStorageSupported && FileHelpers.CloudStorageEnabled)
					{
						string[] files = FileHelpers.GetFiles((FileSource)2, Utils.GetSaveDataPath((FileSource)2), (string)null, (string)null);
						string text = "All files in platform save data:";
						for (int j = 0; j < files.Length; j++)
						{
							text += $"\n{files[j]} ({FileHelpers.GetFileSize(files[j], (FileSource)2)})";
						}
						ZLog.Log((object)text);
					}
					else
					{
						ZLog.Log((object)"Cloud storage is not supported or enabled");
					}
					InitializeAndLoadPrefs();
				}
			};
			_003C_003Ec._003C_003E9__26_0 = val;
			obj = (object)val;
		}
		saveDataProvider.InitializeAsync((InitializeSaveDataCompletedHandler)obj);
	}

	private static void InitializeAndLoadPrefs()
	{
		ZLog.Log((object)"Initializing preferences provider...");
		IPreferencesProvider preferences = PlatformManager.DistributionPlatform.PreferencesProvider;
		if (preferences == null)
		{
			ZLog.Log((object)"No preference provider available for this platform!");
			return;
		}
		byte[] data = null;
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += delegate
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			if (!FileHelpers.FileExistsCloud("Preferences"))
			{
				ZLog.Log((object)"Preferences Provider save file with path Preferences does not exist.");
				return;
			}
			FileReader val2 = null;
			try
			{
				val2 = new FileReader("Preferences", (FileSource)2, (FileHelperType)0);
				int count = val2.m_binary.ReadInt32();
				data = val2.m_binary.ReadBytes(count);
			}
			catch (Exception ex)
			{
				ZLog.LogError((object)("Exception while loading preferences: " + ex.Message));
			}
			finally
			{
				if (val2 != null)
				{
					val2.Dispose();
				}
			}
		};
		backgroundWorker.RunWorkerCompleted += delegate
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			ZLog.Log((object)"Finished loading preference data!");
			IPreferencesProvider obj = preferences;
			byte[] array = data;
			object obj2 = _003C_003Ec._003C_003E9__27_2;
			if (obj2 == null)
			{
				InitializePreferencesCompletedHandler val = delegate(bool succeeded)
				{
					if (!succeeded)
					{
						ZLog.LogError((object)"Failed to initialize preferences provider");
					}
					else
					{
						ZLog.Log((object)"Preferences initialized successfully!");
					}
				};
				_003C_003Ec._003C_003E9__27_2 = val;
				obj2 = (object)val;
			}
			obj.InitializeAsync(array, (InitializePreferencesCompletedHandler)obj2);
		};
		backgroundWorker.RunWorkerAsync();
	}

	private static void OnSplatformLog(LogType logType, object message)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected I4, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		switch ((int)logType)
		{
		case 0:
			ZLog.LogError(message);
			break;
		case 2:
			ZLog.LogWarning(message);
			break;
		case 3:
			ZLog.Log(message);
			break;
		default:
			ZLog.LogError((object)$"Log type {logType} not implemented! Log message:\n{message}");
			break;
		}
	}
}
