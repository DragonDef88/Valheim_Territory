using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace ServerSync;

[PublicAPI]
public class ConfigSync
{
	[HarmonyPatch(typeof(ZRpc), "HandlePackage")]
	private static class SnatchCurrentlyHandlingRPC
	{
		public static ZRpc? currentRpc;

		[HarmonyPrefix]
		private static void Prefix(ZRpc __instance)
		{
			currentRpc = __instance;
		}
	}

	[HarmonyPatch(typeof(ZNet), "Awake")]
	internal static class RegisterRPCPatch
	{
		[HarmonyPostfix]
		private static void Postfix(ZNet __instance)
		{
			isServer = __instance.IsServer();
			foreach (ConfigSync configSync2 in configSyncs)
			{
				ZRoutedRpc.instance.Register<ZPackage>(configSync2.Name + " ConfigSync", (Action<long, ZPackage>)configSync2.RPC_FromOtherClientConfigSync);
				if (isServer)
				{
					configSync2.InitialSyncDone = true;
					Debug.Log((object)("Registered '" + configSync2.Name + " ConfigSync' RPC - waiting for incoming connections"));
				}
			}
			if (isServer)
			{
				((MonoBehaviour)__instance).StartCoroutine(WatchAdminListChanges());
			}
			static void SendAdmin(List<ZNetPeer> peers, bool isAdmin)
			{
				ZPackage package = ConfigsToPackage(null, null, new PackageEntry[1]
				{
					new PackageEntry
					{
						section = "Internal",
						key = "lockexempt",
						type = typeof(bool),
						value = isAdmin
					}
				});
				ConfigSync configSync = configSyncs.First();
				if (configSync != null)
				{
					((MonoBehaviour)ZNet.instance).StartCoroutine(configSync.sendZPackage(peers, package));
				}
			}
			static IEnumerator WatchAdminListChanges()
			{
				MethodInfo listContainsId = AccessTools.DeclaredMethod(typeof(ZNet), "ListContainsId", (Type[])null, (Type[])null);
				SyncedList adminList = (SyncedList)AccessTools.DeclaredField(typeof(ZNet), "m_adminList").GetValue(ZNet.instance);
				List<string> CurrentList = new List<string>(adminList.GetList());
				while (true)
				{
					yield return (object)new WaitForSeconds(30f);
					if (!adminList.GetList().SequenceEqual(CurrentList))
					{
						CurrentList = new List<string>(adminList.GetList());
						List<ZNetPeer> adminPeer = ZNet.instance.GetPeers().Where(delegate(ZNetPeer p)
						{
							string hostName = p.m_rpc.GetSocket().GetHostName();
							return ((object)listContainsId == null) ? adminList.Contains(hostName) : ((bool)listContainsId.Invoke(ZNet.instance, new object[2] { adminList, hostName }));
						}).ToList();
						List<ZNetPeer> nonAdminPeer = ZNet.instance.GetPeers().Except(adminPeer).ToList();
						SendAdmin(nonAdminPeer, isAdmin: false);
						SendAdmin(adminPeer, isAdmin: true);
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(ZNet), "OnNewConnection")]
	private static class RegisterClientRPCPatch
	{
		[HarmonyPostfix]
		private static void Postfix(ZNet __instance, ZNetPeer peer)
		{
			if (__instance.IsServer())
			{
				return;
			}
			foreach (ConfigSync configSync in configSyncs)
			{
				peer.m_rpc.Register<ZPackage>(configSync.Name + " ConfigSync", (Action<ZRpc, ZPackage>)configSync.RPC_FromServerConfigSync);
			}
		}
	}

	private class ParsedConfigs
	{
		public readonly Dictionary<OwnConfigEntryBase, object?> configValues = new Dictionary<OwnConfigEntryBase, object>();

		public readonly Dictionary<CustomSyncedValueBase, object?> customValues = new Dictionary<CustomSyncedValueBase, object>();
	}

	[HarmonyPatch(typeof(ZNet), "Shutdown")]
	private class ResetConfigsOnShutdown
	{
		[HarmonyPostfix]
		private static void Postfix()
		{
			ProcessingServerUpdate = true;
			foreach (ConfigSync configSync in configSyncs)
			{
				configSync.resetConfigsFromServer();
				configSync.IsSourceOfTruth = true;
				configSync.InitialSyncDone = false;
			}
			ProcessingServerUpdate = false;
		}
	}

	[HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
	private class SendConfigsAfterLogin
	{
		private class BufferingSocket : ZPlayFabSocket, ISocket
		{
			public volatile bool finished = false;

			public volatile int versionMatchQueued = -1;

			public readonly List<ZPackage> Package = new List<ZPackage>();

			public readonly ISocket Original;

			public BufferingSocket(ISocket original)
			{
				Original = original;
				((ZPlayFabSocket)this)._002Ector();
			}

			public bool IsConnected()
			{
				return Original.IsConnected();
			}

			public ZPackage Recv()
			{
				return Original.Recv();
			}

			public int GetSendQueueSize()
			{
				return Original.GetSendQueueSize();
			}

			public int GetCurrentSendRate()
			{
				return Original.GetCurrentSendRate();
			}

			public bool IsHost()
			{
				return Original.IsHost();
			}

			public void Dispose()
			{
				Original.Dispose();
			}

			public bool GotNewData()
			{
				return Original.GotNewData();
			}

			public void Close()
			{
				Original.Close();
			}

			public string GetEndPointString()
			{
				return Original.GetEndPointString();
			}

			public void GetAndResetStats(out int totalSent, out int totalRecv)
			{
				Original.GetAndResetStats(ref totalSent, ref totalRecv);
			}

			public void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec)
			{
				Original.GetConnectionQuality(ref localQuality, ref remoteQuality, ref ping, ref outByteSec, ref inByteSec);
			}

			public ISocket Accept()
			{
				return Original.Accept();
			}

			public int GetHostPort()
			{
				return Original.GetHostPort();
			}

			public bool Flush()
			{
				return Original.Flush();
			}

			public string GetHostName()
			{
				return Original.GetHostName();
			}

			public void VersionMatch()
			{
				if (finished)
				{
					Original.VersionMatch();
				}
				else
				{
					versionMatchQueued = Package.Count;
				}
			}

			public void Send(ZPackage pkg)
			{
				//IL_0057: Unknown result type (might be due to invalid IL or missing references)
				//IL_005d: Expected O, but got Unknown
				int pos = pkg.GetPos();
				pkg.SetPos(0);
				int num = pkg.ReadInt();
				if ((num == StringExtensionMethods.GetStableHashCode("PeerInfo") || num == StringExtensionMethods.GetStableHashCode("RoutedRPC") || num == StringExtensionMethods.GetStableHashCode("ZDOData")) && !finished)
				{
					ZPackage val = new ZPackage(pkg.GetArray());
					val.SetPos(pos);
					Package.Add(val);
				}
				else
				{
					pkg.SetPos(pos);
					Original.Send(pkg);
				}
			}
		}

		[HarmonyPriority(800)]
		[HarmonyPrefix]
		private static void Prefix(ref Dictionary<Assembly, BufferingSocket>? __state, ZNet __instance, ZRpc rpc)
		{
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Invalid comparison between Unknown and I4
			if (!__instance.IsServer())
			{
				return;
			}
			BufferingSocket bufferingSocket = new BufferingSocket(rpc.GetSocket());
			AccessTools.DeclaredField(typeof(ZRpc), "m_socket").SetValue(rpc, bufferingSocket);
			object? obj = AccessTools.DeclaredMethod(typeof(ZNet), "GetPeer", new Type[1] { typeof(ZRpc) }, (Type[])null).Invoke(__instance, new object[1] { rpc });
			ZNetPeer val = (ZNetPeer)((obj is ZNetPeer) ? obj : null);
			if (val != null && (int)ZNet.m_onlineBackend > 0)
			{
				FieldInfo fieldInfo = AccessTools.DeclaredField(typeof(ZNetPeer), "m_socket");
				object? value = fieldInfo.GetValue(val);
				ZPlayFabSocket val2 = (ZPlayFabSocket)((value is ZPlayFabSocket) ? value : null);
				if (val2 != null)
				{
					typeof(ZPlayFabSocket).GetField("m_remotePlayerId").SetValue(bufferingSocket, val2.m_remotePlayerId);
				}
				fieldInfo.SetValue(val, bufferingSocket);
			}
			if (__state == null)
			{
				__state = new Dictionary<Assembly, BufferingSocket>();
			}
			__state[Assembly.GetExecutingAssembly()] = bufferingSocket;
		}

		[HarmonyPostfix]
		private static void Postfix(Dictionary<Assembly, BufferingSocket> __state, ZNet __instance, ZRpc rpc)
		{
			ZRpc rpc2 = rpc;
			ZNet __instance2 = __instance;
			Dictionary<Assembly, BufferingSocket> __state2 = __state;
			ZNetPeer peer;
			if (__instance2.IsServer())
			{
				object obj = AccessTools.DeclaredMethod(typeof(ZNet), "GetPeer", new Type[1] { typeof(ZRpc) }, (Type[])null).Invoke(__instance2, new object[1] { rpc2 });
				peer = (ZNetPeer)((obj is ZNetPeer) ? obj : null);
				if (peer == null)
				{
					SendBufferedData();
				}
				else
				{
					((MonoBehaviour)__instance2).StartCoroutine(sendAsync());
				}
			}
			void SendBufferedData()
			{
				if (rpc2.GetSocket() is BufferingSocket bufferingSocket)
				{
					AccessTools.DeclaredField(typeof(ZRpc), "m_socket").SetValue(rpc2, bufferingSocket.Original);
					object? obj2 = AccessTools.DeclaredMethod(typeof(ZNet), "GetPeer", new Type[1] { typeof(ZRpc) }, (Type[])null).Invoke(__instance2, new object[1] { rpc2 });
					ZNetPeer val = (ZNetPeer)((obj2 is ZNetPeer) ? obj2 : null);
					if (val != null)
					{
						AccessTools.DeclaredField(typeof(ZNetPeer), "m_socket").SetValue(val, bufferingSocket.Original);
					}
				}
				BufferingSocket bufferingSocket2 = __state2[Assembly.GetExecutingAssembly()];
				bufferingSocket2.finished = true;
				for (int i = 0; i < bufferingSocket2.Package.Count; i++)
				{
					if (i == bufferingSocket2.versionMatchQueued)
					{
						bufferingSocket2.Original.VersionMatch();
					}
					bufferingSocket2.Original.Send(bufferingSocket2.Package[i]);
				}
				if (bufferingSocket2.Package.Count == bufferingSocket2.versionMatchQueued)
				{
					bufferingSocket2.Original.VersionMatch();
				}
			}
			IEnumerator sendAsync()
			{
				foreach (ConfigSync configSync in configSyncs)
				{
					List<PackageEntry> entries = new List<PackageEntry>();
					if (configSync.CurrentVersion != null)
					{
						entries.Add(new PackageEntry
						{
							section = "Internal",
							key = "serverversion",
							type = typeof(string),
							value = configSync.CurrentVersion
						});
					}
					MethodInfo listContainsId = AccessTools.DeclaredMethod(typeof(ZNet), "ListContainsId", (Type[])null, (Type[])null);
					SyncedList adminList = (SyncedList)AccessTools.DeclaredField(typeof(ZNet), "m_adminList").GetValue(ZNet.instance);
					entries.Add(new PackageEntry
					{
						section = "Internal",
						key = "lockexempt",
						type = typeof(bool),
						value = (((object)listContainsId == null) ? ((object)adminList.Contains(rpc2.GetSocket().GetHostName())) : listContainsId.Invoke(ZNet.instance, new object[2]
						{
							adminList,
							rpc2.GetSocket().GetHostName()
						}))
					});
					ZPackage package = ConfigsToPackage(configSync.allConfigs.Select((OwnConfigEntryBase c) => c.BaseConfig), configSync.allCustomValues, entries, partial: false);
					yield return ((MonoBehaviour)__instance2).StartCoroutine(configSync.sendZPackage(new List<ZNetPeer> { peer }, package));
				}
				SendBufferedData();
			}
		}
	}

	private class PackageEntry
	{
		public string section = null;

		public string key = null;

		public Type type = null;

		public object? value;
	}

	[HarmonyPatch(typeof(ConfigEntryBase), "GetSerializedValue")]
	private static class PreventSavingServerInfo
	{
		[HarmonyPrefix]
		private static bool Prefix(ConfigEntryBase __instance, ref string __result)
		{
			OwnConfigEntryBase ownConfigEntryBase = configData(__instance);
			if (ownConfigEntryBase == null || isWritableConfig(ownConfigEntryBase))
			{
				return true;
			}
			__result = TomlTypeConverter.ConvertToString(ownConfigEntryBase.LocalBaseValue, __instance.SettingType);
			return false;
		}
	}

	[HarmonyPatch(typeof(ConfigEntryBase), "SetSerializedValue")]
	private static class PreventConfigRereadChangingValues
	{
		[HarmonyPrefix]
		private static bool Prefix(ConfigEntryBase __instance, string value)
		{
			OwnConfigEntryBase ownConfigEntryBase = configData(__instance);
			if (ownConfigEntryBase == null || ownConfigEntryBase.LocalBaseValue == null)
			{
				return true;
			}
			try
			{
				ownConfigEntryBase.LocalBaseValue = TomlTypeConverter.ConvertToValue(value, __instance.SettingType);
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)$"Config value of setting \"{__instance.Definition}\" could not be parsed and will be ignored. Reason: {ex.Message}; Value: {value}");
			}
			return false;
		}
	}

	private class InvalidDeserializationTypeException : Exception
	{
		public string expected = null;

		public string received = null;

		public string field = "";
	}

	public static bool ProcessingServerUpdate;

	public readonly string Name;

	public string? DisplayName;

	public string? CurrentVersion;

	public string? MinimumRequiredVersion;

	public bool ModRequired = false;

	private bool? forceConfigLocking;

	private bool isSourceOfTruth = true;

	private static readonly HashSet<ConfigSync> configSyncs;

	private readonly HashSet<OwnConfigEntryBase> allConfigs = new HashSet<OwnConfigEntryBase>();

	private HashSet<CustomSyncedValueBase> allCustomValues = new HashSet<CustomSyncedValueBase>();

	private static bool isServer;

	private static bool lockExempt;

	private OwnConfigEntryBase? lockedConfig = null;

	private const byte PARTIAL_CONFIGS = 1;

	private const byte FRAGMENTED_CONFIG = 2;

	private const byte COMPRESSED_CONFIG = 4;

	private readonly Dictionary<string, SortedDictionary<int, byte[]>> configValueCache = new Dictionary<string, SortedDictionary<int, byte[]>>();

	private readonly List<KeyValuePair<long, string>> cacheExpirations = new List<KeyValuePair<long, string>>();

	private static long packageCounter;

	public bool IsLocked
	{
		get
		{
			bool? flag = forceConfigLocking;
			bool num;
			if (!flag.HasValue)
			{
				if (lockedConfig == null)
				{
					goto IL_0052;
				}
				num = ((IConvertible)lockedConfig.BaseConfig.BoxedValue).ToInt32(CultureInfo.InvariantCulture) != 0;
			}
			else
			{
				num = flag.GetValueOrDefault();
			}
			if (!num)
			{
				goto IL_0052;
			}
			int result = ((!lockExempt) ? 1 : 0);
			goto IL_0053;
			IL_0052:
			result = 0;
			goto IL_0053;
			IL_0053:
			return (byte)result != 0;
		}
		set
		{
			forceConfigLocking = value;
		}
	}

	public bool IsAdmin => lockExempt || isSourceOfTruth;

	public bool IsSourceOfTruth
	{
		get
		{
			return isSourceOfTruth;
		}
		private set
		{
			if (value != isSourceOfTruth)
			{
				isSourceOfTruth = value;
				this.SourceOfTruthChanged?.Invoke(value);
			}
		}
	}

	public bool InitialSyncDone { get; private set; } = false;


	public event Action<bool>? SourceOfTruthChanged;

	private event Action? lockedConfigChanged;

	static ConfigSync()
	{
		ProcessingServerUpdate = false;
		configSyncs = new HashSet<ConfigSync>();
		lockExempt = false;
		packageCounter = 0L;
		RuntimeHelpers.RunClassConstructor(typeof(VersionCheck).TypeHandle);
	}

	public ConfigSync(string name)
	{
		Name = name;
		configSyncs.Add(this);
		new VersionCheck(this);
	}

	public SyncedConfigEntry<T> AddConfigEntry<T>(ConfigEntry<T> configEntry)
	{
		ConfigEntry<T> configEntry2 = configEntry;
		OwnConfigEntryBase ownConfigEntryBase = configData((ConfigEntryBase)(object)configEntry2);
		SyncedConfigEntry<T> syncedEntry = ownConfigEntryBase as SyncedConfigEntry<T>;
		if (syncedEntry == null)
		{
			syncedEntry = new SyncedConfigEntry<T>(configEntry2);
			AccessTools.DeclaredField(typeof(ConfigDescription), "<Tags>k__BackingField").SetValue(((ConfigEntryBase)configEntry2).Description, new object[1]
			{
				new ConfigurationManagerAttributes()
			}.Concat(((ConfigEntryBase)configEntry2).Description.Tags ?? Array.Empty<object>()).Concat(new SyncedConfigEntry<T>[1] { syncedEntry }).ToArray());
			configEntry2.SettingChanged += delegate
			{
				if (!ProcessingServerUpdate && syncedEntry.SynchronizedConfig)
				{
					Broadcast(ZRoutedRpc.Everybody, (ConfigEntryBase)configEntry2);
				}
			};
			allConfigs.Add(syncedEntry);
		}
		return syncedEntry;
	}

	public SyncedConfigEntry<T> AddLockingConfigEntry<T>(ConfigEntry<T> lockingConfig) where T : IConvertible
	{
		if (lockedConfig != null)
		{
			throw new Exception("Cannot initialize locking ConfigEntry twice");
		}
		lockedConfig = AddConfigEntry<T>(lockingConfig);
		lockingConfig.SettingChanged += delegate
		{
			this.lockedConfigChanged?.Invoke();
		};
		return (SyncedConfigEntry<T>)lockedConfig;
	}

	internal void AddCustomValue(CustomSyncedValueBase customValue)
	{
		CustomSyncedValueBase customValue2 = customValue;
		if (allCustomValues.Select((CustomSyncedValueBase v) => v.Identifier).Concat(new string[1] { "serverversion" }).Contains(customValue2.Identifier))
		{
			throw new Exception("Cannot have multiple settings with the same name or with a reserved name (serverversion)");
		}
		allCustomValues.Add(customValue2);
		allCustomValues = new HashSet<CustomSyncedValueBase>(allCustomValues.OrderByDescending((CustomSyncedValueBase v) => v.Priority));
		customValue2.ValueChanged += delegate
		{
			if (!ProcessingServerUpdate)
			{
				Broadcast(ZRoutedRpc.Everybody, customValue2);
			}
		};
	}

	private void RPC_FromServerConfigSync(ZRpc rpc, ZPackage package)
	{
		lockedConfigChanged += serverLockedSettingChanged;
		IsSourceOfTruth = false;
		if (HandleConfigSyncRPC(0L, package, clientUpdate: false))
		{
			InitialSyncDone = true;
		}
	}

	private void RPC_FromOtherClientConfigSync(long sender, ZPackage package)
	{
		HandleConfigSyncRPC(sender, package, clientUpdate: true);
	}

	private bool HandleConfigSyncRPC(long sender, ZPackage package, bool clientUpdate)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Expected O, but got Unknown
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Expected O, but got Unknown
		try
		{
			if (isServer && IsLocked)
			{
				ZRpc? currentRpc = SnatchCurrentlyHandlingRPC.currentRpc;
				object obj;
				if (currentRpc == null)
				{
					obj = null;
				}
				else
				{
					ISocket socket = currentRpc.GetSocket();
					obj = ((socket != null) ? socket.GetHostName() : null);
				}
				string text = (string)obj;
				if (text != null)
				{
					MethodInfo methodInfo = AccessTools.DeclaredMethod(typeof(ZNet), "ListContainsId", (Type[])null, (Type[])null);
					SyncedList val = (SyncedList)AccessTools.DeclaredField(typeof(ZNet), "m_adminList").GetValue(ZNet.instance);
					if (!(((object)methodInfo == null) ? val.Contains(text) : ((bool)methodInfo.Invoke(ZNet.instance, new object[2] { val, text }))))
					{
						return false;
					}
				}
			}
			cacheExpirations.RemoveAll(delegate(KeyValuePair<long, string> kv)
			{
				if (kv.Key < DateTimeOffset.Now.Ticks)
				{
					configValueCache.Remove(kv.Value);
					return true;
				}
				return false;
			});
			byte b = package.ReadByte();
			if ((b & 2u) != 0)
			{
				long num = package.ReadLong();
				string text2 = sender.ToString() + num;
				if (!configValueCache.TryGetValue(text2, out SortedDictionary<int, byte[]> value))
				{
					value = new SortedDictionary<int, byte[]>();
					configValueCache[text2] = value;
					cacheExpirations.Add(new KeyValuePair<long, string>(DateTimeOffset.Now.AddSeconds(60.0).Ticks, text2));
				}
				int key = package.ReadInt();
				int num2 = package.ReadInt();
				value.Add(key, package.ReadByteArray());
				if (value.Count < num2)
				{
					return false;
				}
				configValueCache.Remove(text2);
				package = new ZPackage(value.Values.SelectMany((byte[] a) => a).ToArray());
				b = package.ReadByte();
			}
			ProcessingServerUpdate = true;
			if ((b & 4u) != 0)
			{
				byte[] buffer = package.ReadByteArray();
				MemoryStream stream = new MemoryStream(buffer);
				MemoryStream memoryStream = new MemoryStream();
				using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
				{
					deflateStream.CopyTo(memoryStream);
				}
				package = new ZPackage(memoryStream.ToArray());
				b = package.ReadByte();
			}
			if ((b & 1) == 0)
			{
				resetConfigsFromServer();
			}
			ParsedConfigs parsedConfigs = ReadConfigsFromPackage(package);
			ConfigFile val2 = null;
			bool saveOnConfigSet = false;
			foreach (KeyValuePair<OwnConfigEntryBase, object> configValue in parsedConfigs.configValues)
			{
				if (!isServer && configValue.Key.LocalBaseValue == null)
				{
					configValue.Key.LocalBaseValue = configValue.Key.BaseConfig.BoxedValue;
				}
				if (val2 == null)
				{
					val2 = configValue.Key.BaseConfig.ConfigFile;
					saveOnConfigSet = val2.SaveOnConfigSet;
					val2.SaveOnConfigSet = false;
				}
				configValue.Key.BaseConfig.BoxedValue = configValue.Value;
			}
			if (val2 != null)
			{
				val2.SaveOnConfigSet = saveOnConfigSet;
				val2.Save();
			}
			foreach (KeyValuePair<CustomSyncedValueBase, object> customValue in parsedConfigs.customValues)
			{
				if (!isServer)
				{
					CustomSyncedValueBase key2 = customValue.Key;
					if (key2.LocalBaseValue == null)
					{
						key2.LocalBaseValue = customValue.Key.BoxedValue;
					}
				}
				customValue.Key.BoxedValue = customValue.Value;
			}
			Debug.Log((object)string.Format("Received {0} configs and {1} custom values from {2} for mod {3}", parsedConfigs.configValues.Count, parsedConfigs.customValues.Count, (isServer || clientUpdate) ? $"client {sender}" : "the server", DisplayName ?? Name));
			if (!isServer)
			{
				serverLockedSettingChanged();
			}
			return true;
		}
		finally
		{
			ProcessingServerUpdate = false;
		}
	}

	private ParsedConfigs ReadConfigsFromPackage(ZPackage package)
	{
		ParsedConfigs parsedConfigs = new ParsedConfigs();
		Dictionary<string, OwnConfigEntryBase> dictionary = allConfigs.Where((OwnConfigEntryBase c) => c.SynchronizedConfig).ToDictionary((OwnConfigEntryBase c) => c.BaseConfig.Definition.Section + "_" + c.BaseConfig.Definition.Key, (OwnConfigEntryBase c) => c);
		Dictionary<string, CustomSyncedValueBase> dictionary2 = allCustomValues.ToDictionary((CustomSyncedValueBase c) => c.Identifier, (CustomSyncedValueBase c) => c);
		int num = package.ReadInt();
		for (int i = 0; i < num; i++)
		{
			string text = package.ReadString();
			string text2 = package.ReadString();
			string text3 = package.ReadString();
			Type type = Type.GetType(text3);
			if (text3 == "" || type != null)
			{
				object obj;
				try
				{
					obj = ((text3 == "") ? null : ReadValueWithTypeFromZPackage(package, type));
				}
				catch (InvalidDeserializationTypeException ex)
				{
					Debug.LogWarning((object)("Got unexpected struct internal type " + ex.received + " for field " + ex.field + " struct " + text3 + " for " + text2 + " in section " + text + " for mod " + (DisplayName ?? Name) + ", expecting " + ex.expected));
					continue;
				}
				OwnConfigEntryBase value2;
				if (text == "Internal")
				{
					CustomSyncedValueBase value;
					if (text2 == "serverversion")
					{
						if (obj?.ToString() != CurrentVersion)
						{
							Debug.LogWarning((object)("Received server version is not equal: server version = " + (obj?.ToString() ?? "null") + "; local version = " + (CurrentVersion ?? "unknown")));
						}
					}
					else if (text2 == "lockexempt")
					{
						if (obj is bool flag)
						{
							lockExempt = flag;
						}
					}
					else if (dictionary2.TryGetValue(text2, out value))
					{
						if ((text3 == "" && (!value.Type.IsValueType || Nullable.GetUnderlyingType(value.Type) != null)) || GetZPackageTypeString(value.Type) == text3)
						{
							parsedConfigs.customValues[value] = obj;
							continue;
						}
						Debug.LogWarning((object)("Got unexpected type " + text3 + " for internal value " + text2 + " for mod " + (DisplayName ?? Name) + ", expecting " + value.Type.AssemblyQualifiedName));
					}
				}
				else if (dictionary.TryGetValue(text + "_" + text2, out value2))
				{
					Type type2 = configType(value2.BaseConfig);
					if ((text3 == "" && (!type2.IsValueType || Nullable.GetUnderlyingType(type2) != null)) || GetZPackageTypeString(type2) == text3)
					{
						parsedConfigs.configValues[value2] = obj;
						continue;
					}
					Debug.LogWarning((object)("Got unexpected type " + text3 + " for " + text2 + " in section " + text + " for mod " + (DisplayName ?? Name) + ", expecting " + type2.AssemblyQualifiedName));
				}
				else
				{
					Debug.LogWarning((object)("Received unknown config entry " + text2 + " in section " + text + " for mod " + (DisplayName ?? Name) + ". This may happen if client and server versions of the mod do not match."));
				}
				continue;
			}
			Debug.LogWarning((object)("Got invalid type " + text3 + ", abort reading of received configs"));
			return new ParsedConfigs();
		}
		return parsedConfigs;
	}

	private static bool isWritableConfig(OwnConfigEntryBase config)
	{
		OwnConfigEntryBase config2 = config;
		ConfigSync configSync = configSyncs.FirstOrDefault((ConfigSync cs) => cs.allConfigs.Contains(config2));
		if (configSync == null)
		{
			return true;
		}
		return configSync.IsSourceOfTruth || !config2.SynchronizedConfig || config2.LocalBaseValue == null || (!configSync.IsLocked && (config2 != configSync.lockedConfig || lockExempt));
	}

	private void serverLockedSettingChanged()
	{
		foreach (OwnConfigEntryBase allConfig in allConfigs)
		{
			configAttribute<ConfigurationManagerAttributes>(allConfig.BaseConfig).ReadOnly = !isWritableConfig(allConfig);
		}
	}

	private void resetConfigsFromServer()
	{
		ConfigFile val = null;
		bool saveOnConfigSet = false;
		foreach (OwnConfigEntryBase item in allConfigs.Where((OwnConfigEntryBase config) => config.LocalBaseValue != null))
		{
			if (val == null)
			{
				val = item.BaseConfig.ConfigFile;
				saveOnConfigSet = val.SaveOnConfigSet;
				val.SaveOnConfigSet = false;
			}
			item.BaseConfig.BoxedValue = item.LocalBaseValue;
			item.LocalBaseValue = null;
		}
		if (val != null)
		{
			val.SaveOnConfigSet = saveOnConfigSet;
		}
		foreach (CustomSyncedValueBase item2 in allCustomValues.Where((CustomSyncedValueBase config) => config.LocalBaseValue != null))
		{
			item2.BoxedValue = item2.LocalBaseValue;
			item2.LocalBaseValue = null;
		}
		lockedConfigChanged -= serverLockedSettingChanged;
		serverLockedSettingChanged();
	}

	private IEnumerator<bool> distributeConfigToPeers(ZNetPeer peer, ZPackage package)
	{
		ZNetPeer peer2 = peer;
		ZRoutedRpc rpc = ZRoutedRpc.instance;
		if (rpc == null)
		{
			yield break;
		}
		byte[] data = package.GetArray();
		if (data != null && data.LongLength > 250000)
		{
			int fragments = (int)(1 + (data.LongLength - 1) / 250000);
			long packageIdentifier = ++packageCounter;
			int fragment = 0;
			while (fragment < fragments)
			{
				foreach (bool item in waitForQueue())
				{
					yield return item;
				}
				if (peer2.m_socket.IsConnected())
				{
					ZPackage fragmentedPackage = new ZPackage();
					fragmentedPackage.Write((byte)2);
					fragmentedPackage.Write(packageIdentifier);
					fragmentedPackage.Write(fragment);
					fragmentedPackage.Write(fragments);
					fragmentedPackage.Write(data.Skip(250000 * fragment).Take(250000).ToArray());
					SendPackage(fragmentedPackage);
					if (fragment != fragments - 1)
					{
						yield return true;
					}
					int num = fragment + 1;
					fragment = num;
					continue;
				}
				break;
			}
			yield break;
		}
		foreach (bool item2 in waitForQueue())
		{
			yield return item2;
		}
		SendPackage(package);
		void SendPackage(ZPackage pkg)
		{
			string text = Name + " ConfigSync";
			if (isServer)
			{
				peer2.m_rpc.Invoke(text, new object[1] { pkg });
			}
			else
			{
				rpc.InvokeRoutedRPC(peer2.m_server ? 0 : peer2.m_uid, text, new object[1] { pkg });
			}
		}
		IEnumerable<bool> waitForQueue()
		{
			float timeout = Time.time + 30f;
			while (peer2.m_socket.GetSendQueueSize() > 20000)
			{
				if (Time.time > timeout)
				{
					Debug.Log((object)$"Disconnecting {peer2.m_uid} after 30 seconds config sending timeout");
					peer2.m_rpc.Invoke("Error", new object[1] { (object)(ConnectionStatus)5 });
					ZNet.instance.Disconnect(peer2);
					break;
				}
				yield return false;
			}
		}
	}

	private IEnumerator sendZPackage(long target, ZPackage package)
	{
		if (!Object.op_Implicit((Object)(object)ZNet.instance))
		{
			return Enumerable.Empty<object>().GetEnumerator();
		}
		List<ZNetPeer> list = (List<ZNetPeer>)AccessTools.DeclaredField(typeof(ZRoutedRpc), "m_peers").GetValue(ZRoutedRpc.instance);
		if (target != ZRoutedRpc.Everybody)
		{
			list = list.Where((ZNetPeer p) => p.m_uid == target).ToList();
		}
		return sendZPackage(list, package);
	}

	private IEnumerator sendZPackage(List<ZNetPeer> peers, ZPackage package)
	{
		ZPackage package2 = package;
		if (!Object.op_Implicit((Object)(object)ZNet.instance))
		{
			yield break;
		}
		byte[] rawData = package2.GetArray();
		if (rawData != null && rawData.LongLength > 10000)
		{
			ZPackage compressedPackage = new ZPackage();
			compressedPackage.Write((byte)4);
			MemoryStream output = new MemoryStream();
			using (DeflateStream deflateStream = new DeflateStream(output, CompressionLevel.Optimal))
			{
				deflateStream.Write(rawData, 0, rawData.Length);
			}
			compressedPackage.Write(output.ToArray());
			package2 = compressedPackage;
		}
		List<IEnumerator<bool>> writers = (from peer in peers
			where peer.IsReady()
			select peer into p
			select distributeConfigToPeers(p, package2)).ToList();
		writers.RemoveAll((IEnumerator<bool> writer) => !writer.MoveNext());
		while (writers.Count > 0)
		{
			yield return null;
			writers.RemoveAll((IEnumerator<bool> writer) => !writer.MoveNext());
		}
	}

	private void Broadcast(long target, params ConfigEntryBase[] configs)
	{
		if (!IsLocked || isServer)
		{
			ZPackage package = ConfigsToPackage(configs);
			ZNet instance = ZNet.instance;
			if (instance != null)
			{
				((MonoBehaviour)instance).StartCoroutine(sendZPackage(target, package));
			}
		}
	}

	private void Broadcast(long target, params CustomSyncedValueBase[] customValues)
	{
		if (!IsLocked || isServer)
		{
			ZPackage package = ConfigsToPackage(null, customValues);
			ZNet instance = ZNet.instance;
			if (instance != null)
			{
				((MonoBehaviour)instance).StartCoroutine(sendZPackage(target, package));
			}
		}
	}

	private static OwnConfigEntryBase? configData(ConfigEntryBase config)
	{
		return config.Description.Tags?.OfType<OwnConfigEntryBase>().SingleOrDefault();
	}

	public static SyncedConfigEntry<T>? ConfigData<T>(ConfigEntry<T> config)
	{
		return ((ConfigEntryBase)config).Description.Tags?.OfType<SyncedConfigEntry<T>>().SingleOrDefault();
	}

	private static T configAttribute<T>(ConfigEntryBase config)
	{
		return config.Description.Tags.OfType<T>().First();
	}

	private static Type configType(ConfigEntryBase config)
	{
		return configType(config.SettingType);
	}

	private static Type configType(Type type)
	{
		return type.IsEnum ? Enum.GetUnderlyingType(type) : type;
	}

	private static ZPackage ConfigsToPackage(IEnumerable<ConfigEntryBase>? configs = null, IEnumerable<CustomSyncedValueBase>? customValues = null, IEnumerable<PackageEntry>? packageEntries = null, bool partial = true)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		List<ConfigEntryBase> list = configs?.Where((ConfigEntryBase config) => configData(config).SynchronizedConfig).ToList() ?? new List<ConfigEntryBase>();
		List<CustomSyncedValueBase> list2 = customValues?.ToList() ?? new List<CustomSyncedValueBase>();
		ZPackage val = new ZPackage();
		val.Write((byte)(partial ? 1 : 0));
		val.Write(list.Count + list2.Count + (packageEntries?.Count() ?? 0));
		foreach (PackageEntry item in packageEntries ?? Array.Empty<PackageEntry>())
		{
			AddEntryToPackage(val, item);
		}
		foreach (CustomSyncedValueBase item2 in list2)
		{
			AddEntryToPackage(val, new PackageEntry
			{
				section = "Internal",
				key = item2.Identifier,
				type = item2.Type,
				value = item2.BoxedValue
			});
		}
		foreach (ConfigEntryBase item3 in list)
		{
			AddEntryToPackage(val, new PackageEntry
			{
				section = item3.Definition.Section,
				key = item3.Definition.Key,
				type = configType(item3),
				value = item3.BoxedValue
			});
		}
		return val;
	}

	private static void AddEntryToPackage(ZPackage package, PackageEntry entry)
	{
		package.Write(entry.section);
		package.Write(entry.key);
		package.Write((entry.value == null) ? "" : GetZPackageTypeString(entry.type));
		AddValueToZPackage(package, entry.value);
	}

	private static string GetZPackageTypeString(Type type)
	{
		return type.AssemblyQualifiedName;
	}

	private static void AddValueToZPackage(ZPackage package, object? value)
	{
		Type type = value?.GetType();
		if (value is Enum)
		{
			value = ((IConvertible)value).ToType(Enum.GetUnderlyingType(value.GetType()), CultureInfo.InvariantCulture);
		}
		else
		{
			if (value is ICollection collection)
			{
				package.Write(collection.Count);
				{
					foreach (object item in collection)
					{
						AddValueToZPackage(package, item);
					}
					return;
				}
			}
			if ((object)type != null && type.IsValueType && !type.IsPrimitive)
			{
				FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				package.Write(fields.Length);
				FieldInfo[] array = fields;
				foreach (FieldInfo fieldInfo in array)
				{
					package.Write(GetZPackageTypeString(fieldInfo.FieldType));
					AddValueToZPackage(package, fieldInfo.GetValue(value));
				}
				return;
			}
		}
		ZRpc.Serialize(new object[1] { value }, ref package);
	}

	private static object ReadValueWithTypeFromZPackage(ZPackage package, Type type)
	{
		if ((object)type != null && type.IsValueType && !type.IsPrimitive && !type.IsEnum)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			int num = package.ReadInt();
			if (num != fields.Length)
			{
				throw new InvalidDeserializationTypeException
				{
					received = $"(field count: {num})",
					expected = $"(field count: {fields.Length})"
				};
			}
			object uninitializedObject = FormatterServices.GetUninitializedObject(type);
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				string text = package.ReadString();
				if (text != GetZPackageTypeString(fieldInfo.FieldType))
				{
					throw new InvalidDeserializationTypeException
					{
						received = text,
						expected = GetZPackageTypeString(fieldInfo.FieldType),
						field = fieldInfo.Name
					};
				}
				fieldInfo.SetValue(uninitializedObject, ReadValueWithTypeFromZPackage(package, fieldInfo.FieldType));
			}
			return uninitializedObject;
		}
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
		{
			int num2 = package.ReadInt();
			IDictionary dictionary = (IDictionary)Activator.CreateInstance(type);
			Type type2 = typeof(KeyValuePair<, >).MakeGenericType(type.GenericTypeArguments);
			FieldInfo field = type2.GetField("key", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo field2 = type2.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
			for (int j = 0; j < num2; j++)
			{
				object obj = ReadValueWithTypeFromZPackage(package, type2);
				dictionary.Add(field.GetValue(obj), field2.GetValue(obj));
			}
			return dictionary;
		}
		if (type != typeof(List<string>) && type.IsGenericType)
		{
			Type type3 = typeof(ICollection<>).MakeGenericType(type.GenericTypeArguments[0]);
			if ((object)type3 != null && type3.IsAssignableFrom(type))
			{
				int num3 = package.ReadInt();
				object obj2 = Activator.CreateInstance(type);
				MethodInfo method = type3.GetMethod("Add");
				for (int k = 0; k < num3; k++)
				{
					method.Invoke(obj2, new object[1] { ReadValueWithTypeFromZPackage(package, type.GenericTypeArguments[0]) });
				}
				return obj2;
			}
		}
		ParameterInfo parameterInfo = (ParameterInfo)FormatterServices.GetUninitializedObject(typeof(ParameterInfo));
		AccessTools.DeclaredField(typeof(ParameterInfo), "ClassImpl").SetValue(parameterInfo, type);
		List<object> source = new List<object>();
		ZRpc.Deserialize(new ParameterInfo[2] { null, parameterInfo }, package, ref source);
		return source.First();
	}
}
