using System.Collections.Generic;

namespace STUWard;

internal readonly struct ManagedWardConfigSnapshot
{
	internal IReadOnlyDictionary<string, int> WardLimitOverrides { get; }

	internal ManagedWardItemPrefabPolicySnapshot ItemPrefabPolicy { get; }

	internal string ItemPrefabPolicyYaml { get; }

	internal ManagedWardConfigSnapshot(IReadOnlyDictionary<string, int> wardLimitOverrides, ManagedWardItemPrefabPolicySnapshot itemPrefabPolicy, string itemPrefabPolicyYaml)
	{
		WardLimitOverrides = wardLimitOverrides;
		ItemPrefabPolicy = itemPrefabPolicy;
		ItemPrefabPolicyYaml = itemPrefabPolicyYaml ?? string.Empty;
	}
}
