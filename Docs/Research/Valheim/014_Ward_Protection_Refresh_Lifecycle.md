# 014 - Ward Protection Refresh Lifecycle

## Problem

The ward menu displays the protection state from `WardMenuModel.Ward.Enabled`. Radius and rename changes already refresh the open menu through domain events, but the vanilla ward protection toggle does not publish a Clan Territory event yet.

Observed behaviour:

- Clicking the protection toggle invokes the vanilla `ToggleEnabled` RPC.
- The ward protection state is changed by Valheim.
- The open Clan Territory menu keeps displaying the previous state until the menu is closed and opened again.

## Current lifecycle

Radius lifecycle:

1. UI button calls `WardMenuController.RequestSetRadius`.
2. Action calls `TerritoryWardRadiusService.RequestSetRadius`.
3. Service normalizes and applies the radius.
4. Service publishes `TerritoryRadiusChangedEvent`.
5. `WardMenuService` handles the event and refreshes the view.

Protection lifecycle before this change:

1. UI button calls `WardMenuController.RequestToggleProtection`.
2. Action invokes Valheim `ToggleEnabled` RPC.
3. No Clan Territory event is published.
4. `WardMenuService` has no signal to refresh the open view.

## Decision

Do not optimistically flip the UI state in the view.

Instead, keep the model as the source of truth and schedule a short action refresh after the vanilla RPC is successfully invoked. The refresh rebuilds the model from `PrivateArea`/ZDO and updates the view without reopening the Jotunn UI.

This matches the existing Jotunn menu lifecycle:

- `Show` opens and blocks input once.
- `Refresh` updates text and button state without reopening the UI.
- Protection toggle now uses the same refresh path after the action starts.

## Implementation

- `IWardMenuWardActions.ToggleProtection` now returns `bool`.
- `WardMenuWardActions.ToggleProtection` returns `true` only when the Valheim RPC was invoked.
- `WardMenuController` receives a refresh callback.
- `WardMenuController.RequestToggleProtection` schedules refresh only after a successful action start.
- `WardMenuService` runs several short follow-up refresh attempts so the UI can catch the actual vanilla state after the RPC has mutated the ward ZDO.

## Test Matrix

1. Open ward menu.
2. Toggle protection off.
3. Verify the open menu updates to `Protection: Disabled` without closing.
4. Toggle protection on.
5. Verify the open menu updates to `Protection: Enabled` without closing.
6. Press radius `+5` / `-5` and verify radius still updates immediately.
7. Close after toggling/radius changes and verify cursor/gameplay input remains restored.
