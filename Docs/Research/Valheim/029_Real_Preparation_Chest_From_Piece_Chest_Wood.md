# Investigation 029: Real leveling preparation chest

## Problem

The previous package implemented a chest-like preparation panel using custom ward-menu buttons. That did not match the requested behavior. The preparation action should open a real Valheim chest, for example based on `piece_chest_wood`.

## Evidence

Vanilla `Container` builds an `Inventory` in `Awake()` from `m_width` and `m_height`, registers open/take-all/stack RPCs, and opens with `InventoryGui.instance.Show(this)` after access is granted. This is the correct path for real chest behavior.

`ZNetScene` can fetch vanilla prefabs by name with `GetPrefab(string)`, instantiate them, and find existing spawned instances later by `ZDOID`. This lets the ward keep a persistent link to a real preparation chest.

## Decision

- Keep the Terraforming tab.
- Replace the fake chest-style storage panel behavior with a real `piece_chest_wood` container.
- `Open Preparation Chest` creates the linked chest once near the ward and then opens it through `Container.Interact(...)`.
- The ward stores the linked chest `ZDOID` in ZDO keys.
- The chest is configured as a 5x3 inventory:
  - top row intended for Hoe and Pickaxe,
  - second row intended for fuel,
  - third row intended for Stone.
- The chest uses vanilla ownership/opening/persistence behavior.

## Limit

Vanilla container grids are rectangular. A real `Container` cannot visually render a 2-cell top row and 5-cell lower rows without a custom inventory UI. The worker will treat the first two top-row slots as tool slots and the lower rows as fuel/stone storage.
