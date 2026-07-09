# Investigation 033: Territory treasury and preparation chest top-row cleanup

## Goal

Add a territory treasury button directly under the Clan Territory title and open a real black-metal chest.

## Treasury behavior

The new `Treasury` button opens a real `piece_chest_blackmetal` container linked to the ward by ZDOID.

The linked treasury chest is marked on its own ZDO and registered by inventory instance at runtime. Clan Territory intercepts movement into this marked inventory so each occupied treasury cell can store up to 9999 items of the same stack type.

Supported movement paths:

- dragging an item into a treasury slot;
- shift-clicking an item into the treasury.

The treasury allows any item type. Different item types still cannot occupy the same slot.

## Preparation chest top row

The real terraforming preparation chest still uses a 5x3 vanilla container because Valheim's container grid is rectangular. The logic already blocks top-row slots 3, 4, and 5.

This package additionally hides those reserved top-row UI cells for the linked preparation chest by patching `InventoryGrid.UpdateInventory(...)` and disabling the corresponding grid elements:

- `(2, 0)`;
- `(3, 0)`;
- `(4, 0)`.

The effective visible layout is now:

- top row: Pickaxe, Hoe;
- second row: five fuel slots;
- third row: five stone slots.

## Notes

The treasury uses custom stack movement instead of changing global item stack sizes. This avoids globally modifying Valheim items while still allowing large stacks inside the linked treasury chest.
