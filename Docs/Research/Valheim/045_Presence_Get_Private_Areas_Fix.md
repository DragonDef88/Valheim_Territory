# Investigation 045: Presence GetPrivateAreas compile fix

## Problem

`TerritoryPresenceService.FindCurrentTerritoryArea(...)` calls `GetPrivateAreas()`, but the helper method was missing from `TerritoryPresenceService` after the recent namespace/brace cleanup.

The compiler reported:

`The name 'GetPrivateAreas' does not exist in the current context`

The Plateautem-style worker also left a legacy field:

`TerritoryTerraformingService._nextLevelingWorkerTime`

That field was used by the first worker interval implementation, but the current worker uses scan speed stored in ward ZDO instead.

## Decision

Restore `GetPrivateAreas()` inside `TerritoryPresenceService`, using the existing `AllAreasField` reflection for `PrivateArea.m_allAreas`.

Remove the unused `_nextLevelingWorkerTime` field.

This is a compile-only fix.
