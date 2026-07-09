# Investigation 031: Real preparation chest slot rules and menu handoff

## Problem

The real preparation chest opened while the Clan Territory ward menu remained visible. The chest was opened by vanilla `Container.Interact(...)`, but the custom ward menu stayed on top, so clicking the chest grid was unreliable.

The log confirms the chest was created and opened, then the ward menu immediately scheduled and ran refreshes for `OpenTerraformingPreparationChest`.

## Decision

- Hide and close the ward menu when `Open Preparation Chest` is pressed.
- Do not schedule the normal action refresh for this action.
- Keep the opened vanilla container as the active UI.

## Slot rules

A real `piece_chest_wood` container is still used, but Clan Territory now controls item placement in the linked preparation chest.

Slot layout:

- `(0, 0)`: Pickaxe only.
- `(1, 0)`: Hoe only.
- `(2..4, 0)`: reserved/blocked.
- row `y = 1`: fuel only (`Wood`, `Coal`, `Resin`).
- row `y = 2`: stone only (`Stone`).

Fuel and stone rows use a custom preparation capacity of 500 per slot.

## Implementation notes

Vanilla `InventoryGui.OnSelectedItem(...)` moves dragged items through `InventoryGrid.DropItem(...)`, so the patch intercepts `InventoryGrid.DropItem(...)` for the preparation chest.

Shift-click movement into the container uses `Inventory.MoveItemToThis(Inventory, ItemDrop.ItemData)`, so the patch intercepts that overload and routes the item to the first valid preparation slot.

This keeps the visible UI as a real Valheim chest while enforcing Clan Territory preparation rules.
