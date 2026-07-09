# Investigation 059: Biome Dominion world load timing fix

## Problem

The Biome Dominion service is initialized during plugin bootstrap, before Valheim has a real world name.

The runtime log showed this path on startup:

```text
[BiomeDominion] No biome dominion save found: ...\worlds\Unknown.biome_dominions.txt
```

That means biome dominions could be loaded from `Unknown.biome_dominions.txt` instead of the actual world file, such as `Test2.biome_dominions.txt`.

## Fix

Biome dominion loading is now delayed until the world name is known.

The service now tracks `_loadedWorldName` and calls `EnsureLoadedForCurrentWorld()` from:

- `Initialize()`
- `Update()`
- `TryGetDominionAt(...)`

If the current world is still `Unknown`, loading is deferred.

Saving is also protected: if the world name is unknown, biome dominion saving is skipped instead of writing an `Unknown.biome_dominions.txt` file.

## Result

Claims made in `Test2` are saved and loaded from:

```text
BepInEx/config/ClanTerritory/worlds/Test2.biome_dominions.txt
```

The initial bootstrap can still happen before the world is loaded, but it no longer binds biome dominions to the wrong file.
