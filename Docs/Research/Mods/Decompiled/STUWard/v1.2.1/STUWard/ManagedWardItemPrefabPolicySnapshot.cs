using System.Collections.Generic;

namespace STUWard;

internal readonly struct ManagedWardItemPrefabPolicySnapshot
{
	internal IReadOnlyList<string> BlockedItemPrefabs { get; }

	internal IReadOnlyList<string> PickupWhitelist { get; }

	internal IReadOnlyList<string> PickupBlacklist { get; }

	internal ManagedWardItemPrefabPolicySnapshot(IReadOnlyList<string> blockedItemPrefabs, IReadOnlyList<string> pickupWhitelist, IReadOnlyList<string> pickupBlacklist)
	{
		BlockedItemPrefabs = blockedItemPrefabs;
		PickupWhitelist = pickupWhitelist;
		PickupBlacklist = pickupBlacklist;
	}
}
