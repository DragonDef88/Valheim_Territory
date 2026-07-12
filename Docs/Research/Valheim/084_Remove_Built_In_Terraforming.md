# Investigation 084: Remove built-in terraforming

## Decision

Built-in Clan Territory terraforming is removed from active gameplay.

Plateautem produces better terrain results and avoids the artifacts caused by trying to duplicate terrain editing inside Clan Territory.

## What changed

Clan Territory no longer:

- runs the built-in terrain worker;
- patches `TerrainComp.LevelTerrain`;
- registers terraforming RPCs on wards;
- exposes the ward menu terraforming tab;
- opens the virtual terraforming preparation chest;
- consumes hoe/pickaxe/fuel/stone for built-in leveling;
- moves items into the preparation chest through inventory hooks.

## What remains

Clan Territory still handles:

- ward territory detection;
- ownership and guild access;
- door/protection rules;
- economy;
- treasury virtual container;
- diplomacy;
- biome dominion;
- map markers;
- localization.

The `TerritoryTerraformingService` type remains in source as a disabled legacy container for compatibility with existing internal references and treasury storage helpers.

## Why not delete every legacy class immediately

A hard delete would require a larger split because the old service also contains shared virtual-container and treasury helper code.

For this patch, the dangerous behavior is removed:

- no worker update;
- no heightmap hook;
- no preparation chest UI;
- no preparation-chest inventory hooks.

A later cleanup can move treasury storage out of the legacy service and then delete the remaining terraforming code completely.

## Files changed

- `Source/ClanTerritory/Features/Territory/TerritoryModule.cs`
- `Source/ClanTerritory/Integration/Valheim/Harmony/PrivateAreaHooks.cs`
- `Source/ClanTerritory/Features/WardMenu/Builders/WardMenuModelBuilder.cs`
- `Source/ClanTerritory/Features/WardMenu/UI/JotunnWardMenuView.cs`
- `README.md`
- `Docs/Research/Valheim/084_Remove_Built_In_Terraforming.md`
