# Investigation 065: Remove custom map icon bundle

## Problem

The map marker service attempted to load a custom asset bundle:

```text
BepInEx/plugins/ClanTerritory/clanterritory_mapicons
```

If the bundle was not present, the log contained:

```text
[Map] Ward icon asset bundle not found ... Using default Valheim pin icon.
```

The custom bundle is not needed right now and creates noise in the runtime log.

## Fix

Removed custom Clan Territory map icon bundle loading from `WardMapIconService`.

The map marker selection now uses:

1. Guilds icon from `Guilds.API.GetGuildIcon(Guild)` if the ward is linked to a Guilds guild and Guilds provides an icon.
2. The default Valheim minimap pin icon otherwise.

## Result

The missing `clanterritory_mapicons` warning is gone.

Guilds icons still work through the optional runtime Guilds integration.
