using System;
using System.Collections.Generic;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace STUWard;

internal static class WardItemPrefabPolicy
{
	private sealed class ItemPrefabYaml
	{
		[YamlMember(Alias = "blocked_item_prefabs")]
		public List<string>? BlockedItemPrefabs { get; set; }

		[YamlMember(Alias = "pickup_whitelist")]
		public List<string>? PickupWhitelist { get; set; }

		[YamlMember(Alias = "pickup_blacklist")]
		public List<string>? PickupBlacklist { get; set; }
	}

	private static readonly CustomSyncedValue<string> ItemPrefabData = new CustomSyncedValue<string>(Plugin.ConfigSync, "itemPrefabData", string.Empty);

	private static readonly IDeserializer Deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

	private static HashSet<string> _blockedItemPrefabNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static HashSet<string> _pickupWhitelistPrefabNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static HashSet<string> _pickupBlacklistPrefabNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static bool _initialized;

	internal static void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			ItemPrefabData.ValueChanged += ApplySyncedYaml;
			ManagedWardConfigFileService.ConfigChanged += HandleManagedWardConfigChanged;
			ApplyAuthoritativeConfigFromService();
		}
	}

	internal static void Shutdown()
	{
		if (_initialized)
		{
			ItemPrefabData.ValueChanged -= ApplySyncedYaml;
			ManagedWardConfigFileService.ConfigChanged -= HandleManagedWardConfigChanged;
			_initialized = false;
		}
	}

	internal static bool IsBlockedItem(string prefabName)
	{
		prefabName = NormalizePrefabName(prefabName);
		if (!string.IsNullOrWhiteSpace(prefabName))
		{
			return _blockedItemPrefabNames.Contains(prefabName);
		}
		return false;
	}

	internal static bool IsBlockedItem(ItemData? item)
	{
		return IsBlockedItem(GetItemPrefabName(item));
	}

	internal static bool HasBlockedItems()
	{
		return _blockedItemPrefabNames.Count > 0;
	}

	internal static bool CanAnyPickupBeBlocked()
	{
		if (Plugin.PickupBlockMode.Value == Plugin.PickupBlockRule.AllowAllExceptBlacklist)
		{
			return _pickupBlacklistPrefabNames.Count > 0;
		}
		return true;
	}

	internal static bool ShouldBlockPickup(ItemDrop? itemDrop)
	{
		return ShouldBlockPickup(GetItemPrefabName(itemDrop));
	}

	internal static bool ShouldBlockPickup(GameObject? go)
	{
		return ShouldBlockPickup(GetItemPrefabName(go));
	}

	internal static string GetItemPrefabName(ItemData? item)
	{
		if (item == null)
		{
			return string.Empty;
		}
		return NormalizePrefabName(((Object)(object)item.m_dropPrefab != (Object)null) ? ((Object)item.m_dropPrefab).name : string.Empty);
	}

	private static bool ShouldBlockPickup(string prefabName)
	{
		prefabName = NormalizePrefabName(prefabName);
		if (Plugin.PickupBlockMode.Value == Plugin.PickupBlockRule.AllowAllExceptBlacklist)
		{
			return !string.IsNullOrWhiteSpace(prefabName) && _pickupBlacklistPrefabNames.Contains(prefabName);
		}
		return string.IsNullOrWhiteSpace(prefabName) || !_pickupWhitelistPrefabNames.Contains(prefabName);
	}

	private static string GetItemPrefabName(ItemDrop? itemDrop)
	{
		if ((Object)(object)itemDrop == (Object)null)
		{
			return string.Empty;
		}
		string itemPrefabName = GetItemPrefabName(itemDrop.m_itemData);
		if (string.IsNullOrWhiteSpace(itemPrefabName))
		{
			return NormalizePrefabName(((Object)itemDrop).name);
		}
		return itemPrefabName;
	}

	private static string GetItemPrefabName(GameObject? go)
	{
		if ((Object)(object)go == (Object)null)
		{
			return string.Empty;
		}
		string itemPrefabName = GetItemPrefabName(go.GetComponent<ItemDrop>() ?? go.GetComponentInParent<ItemDrop>());
		if (string.IsNullOrWhiteSpace(itemPrefabName))
		{
			return NormalizePrefabName(((Object)go).name);
		}
		return itemPrefabName;
	}

	internal static string NormalizePrefabName(string prefabName)
	{
		prefabName = prefabName.Trim();
		if (prefabName.EndsWith("(Clone)", StringComparison.Ordinal))
		{
			prefabName = prefabName.Substring(0, prefabName.Length - "(Clone)".Length).Trim();
		}
		return prefabName;
	}

	private static void HandleManagedWardConfigChanged()
	{
		ApplyAuthoritativeConfigFromService();
	}

	private static void ApplyAuthoritativeConfigFromService()
	{
		if (_initialized && Plugin.ConfigSync.IsSourceOfTruth)
		{
			ItemPrefabData.AssignLocalValue(ManagedWardConfigFileService.CurrentSnapshot.ItemPrefabPolicyYaml);
		}
	}

	private static void ApplySyncedYaml()
	{
		if (TryParseYaml(ItemPrefabData.Value, out HashSet<string> blockedItemPrefabNames, out HashSet<string> pickupWhitelistPrefabNames, out HashSet<string> pickupBlacklistPrefabNames))
		{
			_blockedItemPrefabNames = blockedItemPrefabNames;
			_pickupWhitelistPrefabNames = pickupWhitelistPrefabNames;
			_pickupBlacklistPrefabNames = pickupBlacklistPrefabNames;
			Plugin.Log.LogInfo((object)$"Applied item prefab policy: blocked_item_prefabs={_blockedItemPrefabNames.Count}, pickup_whitelist={_pickupWhitelistPrefabNames.Count}, pickup_blacklist={_pickupBlacklistPrefabNames.Count}");
		}
	}

	private static bool TryParseYaml(string yaml, out HashSet<string> blockedItemPrefabNames, out HashSet<string> pickupWhitelistPrefabNames, out HashSet<string> pickupBlacklistPrefabNames)
	{
		blockedItemPrefabNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		pickupWhitelistPrefabNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		pickupBlacklistPrefabNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		try
		{
			ItemPrefabYaml itemPrefabYaml = (string.IsNullOrWhiteSpace(yaml) ? new ItemPrefabYaml() : (Deserializer.Deserialize<ItemPrefabYaml>(yaml) ?? new ItemPrefabYaml()));
			AddEntries(blockedItemPrefabNames, itemPrefabYaml.BlockedItemPrefabs);
			AddEntries(pickupWhitelistPrefabNames, itemPrefabYaml.PickupWhitelist);
			AddEntries(pickupBlacklistPrefabNames, itemPrefabYaml.PickupBlacklist);
			return true;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("Failed to parse item prefab policy YAML 'STUWard.yml:item_prefab_policy': " + ex.Message));
			return false;
		}
	}

	private static void AddEntries(HashSet<string> result, IEnumerable<string>? prefabNames)
	{
		if (prefabNames == null)
		{
			return;
		}
		foreach (string prefabName in prefabNames)
		{
			string text = NormalizePrefabName(prefabName);
			if (!string.IsNullOrWhiteSpace(text))
			{
				result.Add(text);
			}
		}
	}
}
