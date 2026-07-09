# Investigation 042: Leveling spirit missing braces compile fix

## Problem

After removing the accidental duplicate spirit helper block, `TerritoryModule.cs` missed two closing braces before the namespace-level `TerritoryTerraformingService` declaration.

The compiler reported:

` } expected`

## Decision

Restore the two missing braces:

- close `TerritoryPresenceService`
- close `TerritoryModule`

`TerritoryTerraformingService` remains namespace-level as before.
