# Investigation 044: Missing ClearCurrentTerritory compile fix

## Problem

`TerritoryPresenceService.Update()` calls `ClearCurrentTerritory()` when:

- local player is not available;
- player leaves the current territory.

The method was accidentally removed during the Plateautem spirit / namespace brace fixes, causing:

`The name 'ClearCurrentTerritory' does not exist in the current context`

## Decision

Restore `ClearCurrentTerritory()` inside `TerritoryPresenceService`.

The method clears the local presence cache:

```csharp
_currentWardId = "";
_currentTerritoryName = "";
```

This is a compile-only fix.
