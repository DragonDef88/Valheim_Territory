using System;

namespace STUWard;

[Flags]
internal enum WardRestrictionOptions
{
	None = 0,
	Doors = 1,
	Portals = 2,
	Pickup = 4,
	PlacedConsumables = 8,
	ItemStands = 0x10,
	ArmorStands = 0x20,
	Containers = 0x40,
	CraftingStations = 0x80,
	TameablesAndSaddles = 0x100,
	All = 0x1FF
}
