# Investigation 087: Player races audit and runtime synchronization

## Goal

Audit the first player-race implementation after the compile-include workaround and leave it in a maintainable, world-safe and multiplayer-aware state.

The gameplay rules remain unchanged:

```text
Werewolf:
- frost damage = 0;
- spirit/silver-like damage multiplier = 1.40.

Vampire:
- poison damage = 0;
- fire damage multiplier = 1.35;
- spirit/silver-like damage multiplier = 1.50.

Odin Blessed:
- spirit damage multiplier = 0.50;
- frost damage multiplier = 0.75;
- lightning damage multiplier = 0.75.
```

## Sources checked

1. Current `main` branch of `DragonDef88/Valheim_Territory`.
2. `Docs/Research/Valheim/085_Player_Races_Core.md`.
3. `Docs/Research/Valheim/086_Race_Core_Compile_Include_Fix.md`.
4. `Docs/ARCHITECTURE.md` and `Docs/DEVELOPMENT_WORKFLOW.md`.
5. Existing Valheim integration patterns already used by Clan Territory.
6. Decompiled Valheim `Character` flow as behavioral corroboration for networked damage delivery.

## Confirmed problems

### Race implementation was placed in the territory file

`RaceModule`, `RaceService` and the race Harmony hooks were appended to:

```text
Source/ClanTerritory/Features/Territory/TerritoryModule.cs
```

The separate race file was only a stub. This solved one compile error but violated the project's modular architecture and made the race feature difficult to review and maintain.

### The project did not compile the race file

`ClanTerritory.csproj` uses explicit `<Compile Include="..." />` entries. New source files are not picked up automatically.

The durable fix is to include:

```xml
<Compile Include="Features\Races\RaceModule.cs" />
```

and keep the implementation in that file.

### A fake `default` world could be used

The old service initialized before `ZNet` and the world were ready. If the world name was unavailable, it used `default` and could read or write:

```text
BepInEx/config/ClanTerritory/worlds/default.races.txt
```

Race persistence must not bind itself to a fabricated world. Loading and saving are now lazy and occur only after a real world name is available.

### World reflection was on the damage path

The old `GetRace` path resolved the world repeatedly. Incoming damage can be frequent, so the current `ZNet` instance and resolved world are now cached. A reload happens only when the network/world instance changes.

### Runtime race state was not shared

The text file remains the per-world persistence source, but other peers need the selected race while calculating damage against a player.

The selected race is now mirrored to the owning player's ZDO under:

```text
ct_player_race
```

The local persisted choice is published when the player spawns and whenever the choice changes. Damage lookup reads the player ZDO first and falls back to local persisted data.

This is runtime synchronization, not an anti-cheat or server-authoritative race system.

### Saves were nondeterministic and non-atomic

Dictionary enumeration produced unstable file ordering, and direct writes could leave a partial file after interruption.

Player IDs are now sorted and the file is written through a temporary file followed by replace/move fallback.

### Undocumented command alias

The old implementation accepted `set` as an alias for `choose`, although it was not documented and did not enable administrative changes. The hidden alias was removed. Supported commands remain:

```text
/ctrace status
/ctrace choose werewolf
/ctrace choose vampire
/ctrace choose odin
/ctrace reset
/ctraces
```

## Files changed

```text
Source/ClanTerritory/ClanTerritory.csproj
Source/ClanTerritory/Config/ConfigManager.cs
Source/ClanTerritory/Features/Territory/TerritoryModule.cs
Source/ClanTerritory/Features/Races/RaceModule.cs
Docs/Research/Valheim/087_Player_Races_Audit_And_Runtime_Sync.md
```

## Static validation

The patch installer checks:

```text
- race namespace is absent from TerritoryModule.cs;
- race namespace exists once in RaceModule.cs;
- RaceModule.cs is included once by ClanTerritory.csproj;
- EN and RU no-world localization keys exist once;
- no FindObjectsOfType call exists in RaceModule.cs;
- no default.races.txt fallback exists;
- braces are balanced in RaceModule.cs;
- git diff --check passes.
```

## Visual Studio and game test plan

1. Build `Source/ClanTerritory/ClanTerritory.sln` in Visual Studio 2026.
2. Confirm the DLL is copied to the configured Test2 profile.
3. Start Valheim and verify that no `default.races.txt` is created in the main menu.
4. Enter a world and run `/ctrace status`.
5. Choose each race after `/ctrace reset` and inspect the correct world file.
6. Restart the world and verify persistence.
7. Test poison, fire, frost, spirit and lightning damage channels.
8. In multiplayer, verify that another peer's attacks use the target player's published race.
9. Inspect the BepInEx log for Harmony target or persistence warnings.

## Follow-up architectural debt

The audit also confirmed that `TerritoryModule.cs` still contains the separate `BiomeDominion`, `Economy` and `Diplomacy` namespaces. They should be extracted into their own explicitly compiled files in a dedicated refactoring commit.

That broader extraction is intentionally not mixed into this race correctness patch because it touches several mature systems and requires its own compile and gameplay regression pass.
