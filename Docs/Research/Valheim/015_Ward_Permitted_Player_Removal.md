# Investigation 015: Ward permitted player removal

## Goal

Add the first STU_Ward-style ward-management action to the Jotunn ward menu: remove a permitted player directly from the Ward tab.

## Evidence

Valheim stores permitted players in the ward ZDO using `ZDOVars.s_permitted`, `pu_id{index}`, and `pu_name{index}`. The decompiled `PrivateArea` implementation reads those fields in `GetPermittedPlayers()`, writes them in `SetPermittedPlayers(...)`, and removes a player by rebuilding the permitted list without that player.

Valheim's vanilla `TogglePermitted` RPC only toggles the interacting player's own access and only when the ward is disabled. It is not a direct "owner removes arbitrary permitted player" action. For the owner-management UI, this mod therefore writes the same ZDO fields directly when the local instance owns the ward ZDO.

## Decision

Implement the feature as a local-owner action:

- Show permitted players as rows on the Ward tab.
- Add a `Remove` button for each visible permitted player row.
- Require the local player to be the ward creator.
- Require the local instance to own the ward ZDO before modifying the permitted list.
- Rebuild the ZDO permitted-player fields using the same storage layout as vanilla `PrivateArea`.
- Reuse the existing delayed action refresh lifecycle to update the open menu after removal.

## Limits

This first implementation targets host/single-player/local-owner behavior. If a dedicated-server client needs owner-management actions while not owning the ZDO, add a custom owner-routed RPC in a later commit.
