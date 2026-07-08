# Investigation 028: Territory leveling preparation chest

## Change in design

The terraforming feature is narrowed to one safe behavior:

- The territory surface levels to the ward height.
- There are no separate raise/lower target buttons.
- There is no manual target-height selector.
- The ward itself is the leveling reference point.

This matches the desired territory-leveling workflow better than the first generic terraform menu foundation.

## Preparation chest

The ward menu now has a `Prepare Leveling` button. It opens a chest-style preparation panel:

- Top row: two tool cells.
  - Hoe slot.
  - Pickaxe slot.
- Middle row: five fuel cells.
  - Each fuel cell has capacity 500.
- Bottom row: five stone cells.
  - Each stone cell has capacity 500.

Fuel is intentionally stored as generic leveling fuel count. Valid fuel item prefabs are aligned with the Plateautem study: `Wood`, `Coal`, and `Resin`.

Stone is stored separately and uses the `Stone` prefab.

## Item transfer behavior in this package

This package changes the preparation storage from fake counters to inventory-backed transfer:

- Clicking `Hoe Slot` consumes one `Hoe` from the current player's inventory.
- Clicking `Pickaxe Slot` consumes one pickaxe from the current player's inventory.
- Clicking a fuel cell moves fuel items from the current player's inventory into that specific cell, up to 500.
- Clicking a stone cell moves `Stone` from the current player's inventory into that specific cell, up to 500.

The stored values are saved on the ward ZDO.

## Why not a vanilla Container yet

Vanilla `Container` creates its own `Inventory` from width/height and opens through `InventoryGui.instance.Show(this)`. It expects a normal container object with its own ZNetView/ZDO ownership flow. A pure hidden runtime container attached to a ward would be risky before the leveling worker exists because the container lifecycle, item persistence, access control, and ownership transfer all need careful handling.

This package implements a chest-style preparation panel inside the ward menu first. It gives us deterministic ZDO storage and slot validation. If later we want a real `InventoryGui` container, it can be added after the worker is stable.

## Next step

The next implementation step is the actual worker:

1. Use ward height as the fixed target.
2. Scan points inside the territory radius using the Plateautem spiral.
3. Sample the ground around each scan point.
4. Consume fuel for scanning/work.
5. Consume stone when raising toward the ward height.
6. Require hoe/pickaxe slots before work starts.
7. Apply terrain through Valheim `TerrainOp`/`TerrainComp`, so terrain data is saved by vanilla owner-side terrain compiler.
