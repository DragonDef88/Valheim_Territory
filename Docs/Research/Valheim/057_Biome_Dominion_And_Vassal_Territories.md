# Investigation 057: Biome Dominion and Vassal Territories

## Goal

Add the first playable layer for biome-level clan ownership.

A Guilds guild can claim the current biome and apply biome-wide rules. Existing ward territories inside a claimed biome become vassal territories of the biome owner guild.

## Valheim biome API

Valheim already resolves biomes through `WorldGenerator.instance.GetBiome(Vector3)` and through `Heightmap.Biome`. Minimap uses the same path to display the current biome name with Valheim localization keys such as `$biome_meadows`.

## First implementation

The first implementation is intentionally command-driven and low-risk.

Command:

```text
/ctbiome status
/ctbiome claim
/ctbiome release
/ctbiome list
/ctbiome set doorlock on|off
/ctbiome set protection on|off
/ctbiome set autoclose 3-10
```

Rules:

- only Guilds guild leaders can claim/release/change biome rules;
- Guilds remains optional and runtime-reflection based;
- if Guilds is unavailable, biome dominion commands do not work but the mod does not fail;
- claims are stored per world under `BepInEx/config/ClanTerritory/worlds/<world>.biome_dominions.txt`;
- one owner guild per biome;
- if a ward territory inside a claimed biome belongs to another guild or has no guild binding, it is treated as a vassal territory.

## Biome rules

First pass rules:

- biome door lock;
- biome structure damage protection;
- biome door auto-close seconds.

Door access:

- biome owner guild members can pass;
- local territory owner/permitted/guild access can still pass inside their ward territory;
- outsiders are blocked when biome door lock is enabled.

Structure protection:

- when enabled, structure damage inside the claimed biome is blocked through the existing WearNTear Harmony path.

## Vassal territories

Territory presence checks the ward territory on entry. If the ward is inside a claimed biome and does not belong to the same guild as the biome owner, the entry message becomes a localized vassal-territory message.

This is the first gameplay layer. Later UI can move these commands into ward menu / biome map controls.
