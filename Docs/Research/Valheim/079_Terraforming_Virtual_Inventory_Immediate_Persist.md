# Investigation 079: Terraforming virtual inventory immediate persist

## Problem

After adding inventory UI compatibility for Offline Companions, the player could open the territory preparation chest, but the terraforming worker did not see the prepared items reliably.

The runtime log showed:

```text
[TerritoryContainers] Virtual container opened without world chest: piece_chest_wood
[WardMenu] Closed ward territory menu: ..., reason: OpenTerraformingPreparationChest
```

but no following terraforming state logs such as:

```text
[TerritoryTerraforming] Enabled saved
[TerritoryTerraforming] Running saved
[TerritoryTerraforming] Hoe slot saved
[TerritoryTerraforming] Pickaxe slot saved
[TerritoryTerraforming] Fuel slot ...
[TerritoryTerraforming] Stone slot ...
```

## Cause

The virtual preparation chest was persisted mostly when the virtual container closed.

With other mods also interacting with `InventoryGui`, especially Offline Companions, container switching can make close/order behavior less reliable.

The worker reads its inventory from ward ZDO state, so delayed or missed persistence means the worker can run without seeing hoe/pickaxe/fuel/stone.

## Fix

Persist virtual territory inventories immediately after item movement:

- preparation chest direct slot move;
- preparation chest auto move;
- treasury chest direct slot move;
- large-stack virtual movement.

The existing close-time persistence is kept as a safety net.

## Files changed

- `Source/ClanTerritory/Features/Territory/TerritoryModule.cs`
- `Docs/Research/Valheim/079_Terraforming_Virtual_Inventory_Immediate_Persist.md`

## Notes

No rule logic was changed.

This is a persistence timing fix for virtual containers.
