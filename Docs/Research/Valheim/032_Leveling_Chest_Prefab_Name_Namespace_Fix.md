# Investigation 032: Leveling chest prefab name namespace fix

## Problem

The leveling preparation chest slot-rule code called:

`Utils.GetPrefabName(...)`

Inside the Clan Territory namespace this resolved against `ClanTerritory.Utils` instead of Valheim's global `Utils` class, causing a compile error.

## Decision

Use an explicit global namespace reference:

`global::Utils.GetPrefabName(...)`

This is a compile-only fix. Slot-rule behavior is unchanged.
