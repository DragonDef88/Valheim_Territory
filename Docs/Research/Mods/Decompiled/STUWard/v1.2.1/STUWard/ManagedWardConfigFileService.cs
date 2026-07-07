using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using YamlDotNet.Serialization;

namespace STUWard;

internal static class ManagedWardConfigFileService
{
	private sealed class ManagedWardConfigYaml
	{
		[YamlMember(Alias = "ward_limit_overrides")]
		public Dictionary<string, int>? WardLimitOverrides { get; set; }

		[YamlMember(Alias = "item_prefab_policy")]
		public ManagedWardItemPrefabPolicyYaml? ItemPrefabPolicy { get; set; }
	}

	private sealed class ManagedWardItemPrefabPolicyYaml
	{
		[YamlMember(Alias = "blocked_item_prefabs")]
		public List<string>? BlockedItemPrefabs { get; set; }

		[YamlMember(Alias = "pickup_whitelist")]
		public List<string>? PickupWhitelist { get; set; }

		[YamlMember(Alias = "pickup_blacklist")]
		public List<string>? PickupBlacklist { get; set; }
	}

	internal const string ConfigFileName = "STUWard.yml";

	private const double ReloadIntervalSeconds = 1.0;

	private static readonly IDeserializer Deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

	private static readonly ISerializer Serializer = new SerializerBuilder().Build();

	private static readonly ManagedWardConfigSnapshot DefaultSnapshot = CreateDefaultSnapshot();

	private static ManagedWardConfigSnapshot _currentSnapshot = DefaultSnapshot;

	private static DateTime _lastProcessedWriteUtc = DateTime.MinValue;

	private static DateTime _nextReloadCheckUtc = DateTime.MinValue;

	private static bool _initialized;

	internal static ManagedWardConfigSnapshot CurrentSnapshot => _currentSnapshot;

	internal static event Action? ConfigChanged;

	internal static void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			Plugin.ConfigSync.SourceOfTruthChanged += HandleSourceOfTruthChanged;
			ReloadAuthoritativeLocalFile(force: true);
		}
	}

	internal static void Shutdown()
	{
		if (_initialized)
		{
			Plugin.ConfigSync.SourceOfTruthChanged -= HandleSourceOfTruthChanged;
			_initialized = false;
			_lastProcessedWriteUtc = DateTime.MinValue;
			_nextReloadCheckUtc = DateTime.MinValue;
			_currentSnapshot = DefaultSnapshot;
		}
	}

	internal static void Update()
	{
		if (_initialized && Plugin.ConfigSync.IsSourceOfTruth && !(DateTime.UtcNow < _nextReloadCheckUtc))
		{
			_nextReloadCheckUtc = DateTime.UtcNow.AddSeconds(1.0);
			ReloadLocalFile(force: false);
		}
	}

	private static void HandleSourceOfTruthChanged(bool isSourceOfTruth)
	{
		if (isSourceOfTruth)
		{
			ReloadAuthoritativeLocalFile(force: true);
		}
	}

	private static void ReloadAuthoritativeLocalFile(bool force)
	{
		if (Plugin.ConfigSync.IsSourceOfTruth)
		{
			EnsureConfigFileExists();
			_nextReloadCheckUtc = DateTime.MinValue;
			ReloadLocalFile(force);
		}
	}

	private static void ReloadLocalFile(bool force)
	{
		string configFilePath = GetConfigFilePath();
		if (!File.Exists(configFilePath))
		{
			return;
		}
		DateTime lastWriteTimeUtc;
		string yaml;
		try
		{
			lastWriteTimeUtc = File.GetLastWriteTimeUtc(configFilePath);
			if (!force && lastWriteTimeUtc == _lastProcessedWriteUtc)
			{
				return;
			}
			yaml = File.ReadAllText(configFilePath);
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Failed to read managed ward config file '" + configFilePath + "': " + ex.Message));
			return;
		}
		if (!TryParseYaml(yaml, out var snapshot))
		{
			_lastProcessedWriteUtc = lastWriteTimeUtc;
			return;
		}
		_lastProcessedWriteUtc = lastWriteTimeUtc;
		_currentSnapshot = snapshot;
		ManagedWardConfigFileService.ConfigChanged?.Invoke();
	}

	private static string GetConfigFilePath()
	{
		return Path.Combine(Paths.ConfigPath, "STUWard.yml");
	}

	private static void EnsureConfigFileExists()
	{
		string configFilePath = GetConfigFilePath();
		if (File.Exists(configFilePath))
		{
			return;
		}
		try
		{
			File.WriteAllText(configFilePath, GetDefaultConfigFileContents());
			Plugin.Log.LogInfo((object)("Created managed ward config file '" + configFilePath + "'."));
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Failed to create managed ward config file '" + configFilePath + "': " + ex.Message));
		}
	}

	private static bool TryParseYaml(string yaml, out ManagedWardConfigSnapshot snapshot)
	{
		snapshot = DefaultSnapshot;
		try
		{
			ManagedWardConfigYaml managedWardConfigYaml = (string.IsNullOrWhiteSpace(yaml) ? new ManagedWardConfigYaml() : (Deserializer.Deserialize<ManagedWardConfigYaml>(yaml) ?? new ManagedWardConfigYaml()));
			Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
			if (managedWardConfigYaml.WardLimitOverrides != null)
			{
				foreach (KeyValuePair<string, int> wardLimitOverride in managedWardConfigYaml.WardLimitOverrides)
				{
					string text = WardOwnership.NormalizeOverrideAccountIdValue(wardLimitOverride.Key);
					if (string.IsNullOrWhiteSpace(text) || !ulong.TryParse(text, out var _))
					{
						Plugin.Log.LogWarning((object)("Ignoring invalid ward_limit_overrides entry for account '" + wardLimitOverride.Key + "'."));
					}
					else
					{
						dictionary[text] = wardLimitOverride.Value;
					}
				}
			}
			ManagedWardItemPrefabPolicyYaml? data = managedWardConfigYaml.ItemPrefabPolicy ?? new ManagedWardItemPrefabPolicyYaml();
			ManagedWardItemPrefabPolicySnapshot itemPrefabPolicy = CreateItemPrefabPolicySnapshot(data);
			string itemPrefabPolicyYaml = SerializeItemPrefabPolicy(data);
			snapshot = new ManagedWardConfigSnapshot(dictionary, itemPrefabPolicy, itemPrefabPolicyYaml);
			return true;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Failed to parse managed ward config YAML 'STUWard.yml': " + ex.Message));
			return false;
		}
	}

	private static ManagedWardItemPrefabPolicySnapshot CreateItemPrefabPolicySnapshot(ManagedWardItemPrefabPolicyYaml data)
	{
		return new ManagedWardItemPrefabPolicySnapshot(NormalizePrefabNames(data.BlockedItemPrefabs), NormalizePrefabNames(data.PickupWhitelist), NormalizePrefabNames(data.PickupBlacklist));
	}

	private static List<string> NormalizePrefabNames(IEnumerable<string>? prefabNames)
	{
		List<string> list = new List<string>();
		if (prefabNames == null)
		{
			return list;
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string prefabName in prefabNames)
		{
			string text = WardItemPrefabPolicy.NormalizePrefabName(prefabName);
			if (!string.IsNullOrWhiteSpace(text) && hashSet.Add(text))
			{
				list.Add(text);
			}
		}
		return list;
	}

	private static string SerializeItemPrefabPolicy(ManagedWardItemPrefabPolicyYaml data)
	{
		return Serializer.Serialize(data);
	}

	private static string GetDefaultConfigFileContents()
	{
		return "# Generated by STUWard\n# Unified server config for ward limit overrides and item prefab policy.\n#\n# ward_limit_overrides:\n#   Map Steam64 account ids to max ward counts.\n#   Use -1 for unlimited wards.\n#\n# item_prefab_policy.blocked_item_prefabs:\n#   Cannot be used, equipped, or attacked with inside a foreign enabled ward.\n#\n# item_prefab_policy.pickup_whitelist:\n#   Allowed when Pickup Block Mode = BlockAllExceptWhitelist.\n#\n# item_prefab_policy.pickup_blacklist:\n#   Blocked when Pickup Block Mode = AllowAllExceptBlacklist.\n\nward_limit_overrides:\n  \"76561198000000000\": 6\n  \"76561198000000001\": -1\nitem_prefab_policy:\n  blocked_item_prefabs:\n    - kg_TameableCollector\n    - PalStone\n    - PalStoneSpeed\n    - PalStoneArmour\n    - PalStoneHeal\n  pickup_whitelist:\n    - Wood\n  pickup_blacklist: []\n";
	}

	private static ManagedWardConfigSnapshot CreateDefaultSnapshot()
	{
		ManagedWardItemPrefabPolicyYaml data = new ManagedWardItemPrefabPolicyYaml
		{
			BlockedItemPrefabs = new List<string> { "kg_TameableCollector", "PalStone", "PalStoneSpeed", "PalStoneArmour", "PalStoneHeal" },
			PickupWhitelist = new List<string> { "Wood" },
			PickupBlacklist = new List<string>()
		};
		ManagedWardItemPrefabPolicySnapshot itemPrefabPolicy = CreateItemPrefabPolicySnapshot(data);
		return new ManagedWardConfigSnapshot(new Dictionary<string, int>(StringComparer.Ordinal), itemPrefabPolicy, SerializeItemPrefabPolicy(data));
	}
}
