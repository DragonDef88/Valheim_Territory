# Investigation 034: Treasury chest helper compile fix

## Problem

The treasury button called `GetOrCreateTreasuryChest(...)`, but the helper implementation was missing from `TerritoryModule.cs`.

## Decision

Add the missing helper set:

- `GetOrCreateTreasuryChest(...)`
- `FindLinkedTreasuryChest(...)`
- `CalculateTreasuryChestPosition(...)`
- `ConfigureTreasuryChest(...)`

The helper mirrors the real preparation chest workflow but uses `piece_chest_blackmetal` and marks the linked chest as a treasury chest.
