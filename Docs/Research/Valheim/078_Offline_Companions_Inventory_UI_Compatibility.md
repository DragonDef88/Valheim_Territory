# Investigation 078: Offline Companions inventory UI compatibility

## Problem

With Offline Companions installed, companion inventory windows can be open while the player is near a Clan Territory ward.

The log showed this sequence:

```text
[Offline Companions] [Interact] ... opening inventory
[Offline Companions] [UI] Show ... companionContainer=...
[Offline Companions] [UI] HideInternal reason=ContainerSwitch ...
[Clan Territory] [TerritoryInteraction] Territory menu requested ...
[Clan Territory] [TerritoryContainers] Virtual container opened without world chest ...
```

Clan Territory was allowed to open ward/virtual-container UI while another inventory/container UI was already active. This can force Offline Companions into a container switch/hide path.

## Fix

Added a small compatibility guard:

- `PrivateArea.Interact` now ignores ward interaction while another major UI/container is open.
- Territory virtual containers do not open if `InventoryGui` is already visible.
- The player receives a short localized message: close the current container first.

## Files changed

- `Source/ClanTerritory/Integration/Valheim/Harmony/PrivateAreaHooks.cs`
- `Source/ClanTerritory/Features/Territory/TerritoryModule.cs`
- `Source/ClanTerritory/Config/ConfigManager.cs`
- `README.md`

## Notes

This does not depend on Offline Companions at compile time.

It is a generic compatibility rule for any mod that keeps `InventoryGui` open.
