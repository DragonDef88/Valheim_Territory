# Investigation 035: Virtual ward containers without world chest placement

## Problem

The previous treasury and preparation implementations created real Valheim chest pieces in the world. That worked for the inventory UI, but it left visible/physical chests and could create extra linked chests when old ZDO links could not be found.

## Evidence

Runtime logs showed real world chest creation:

- `Real treasury chest created from piece_chest_blackmetal`
- `Real preparation chest created from piece_chest_wood`

## Decision

Keep using vanilla `Container` and `InventoryGui` behavior, but stop leaving chest pieces in the world.

The mod now creates a temporary hidden container object only while the inventory is open:

- renderers disabled,
- colliders disabled,
- open/close chest visuals disabled,
- opened directly with `InventoryGui.instance.Show(container)`,
- destroyed when `InventoryGui.Hide()` runs.

Inventory contents are saved to the ward ZDO instead of the chest ZDO:

- preparation chest items -> `ct_territory_terraforming_chest_items`,
- treasury chest items -> `ct_territory_treasury_chest_items`.

## Result

Opening Treasury or Leveling Preparation no longer leaves physical boxes in the world. The player still gets the vanilla chest inventory UI, but storage belongs to the ward.
