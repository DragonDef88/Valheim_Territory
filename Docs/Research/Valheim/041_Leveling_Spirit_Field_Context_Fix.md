# Investigation 041: Leveling spirit field context compile fix

## Problem

The Plateautem-style spirit package accidentally inserted one duplicate set of spirit helper methods into `TerritoryModule`.

Those helpers referenced `LevelingSpiritByWardId`, but the dictionary is declared inside `TerritoryTerraformingService`, so the compiler reported:

`The name 'LevelingSpiritByWardId' does not exist in the current context`

## Decision

Remove the accidental duplicate helper block from `TerritoryModule` and keep the spirit helpers only inside `TerritoryTerraformingService`, where `LevelingSpiritByWardId` is declared.

This is a compile-only fix.
