# Investigation 085: Player races core

## Goal

Add the first safe version of player races:

- Werewolf
- Vampire
- Odin Blessed

This is implemented as personal player state, not as a Unity character model replacement.

## Commands

```text
/ctrace status
/ctrace choose werewolf
/ctrace choose vampire
/ctrace choose odin
/ctrace reset
```

Alias:

```text
/ctraces
```

## Passive damage rules

```text
Werewolf:
- frost damage is set to 0;
- spirit/silver-like damage is increased.

Vampire:
- poison damage is set to 0;
- fire damage is increased;
- spirit/silver-like damage is increased.

Odin Blessed:
- spirit damage is reduced;
- frost damage is reduced;
- lightning damage is reduced.
```

Valheim silver weapons usually apply spirit damage, so the first implementation treats spirit damage as the silver/holy weakness channel.

## Persistence

Race choices are saved per world:

```text
BepInEx/config/ClanTerritory/worlds/<world>.races.txt
```

Format:

```text
playerId=RaceKind
```

## Files changed

- `Source/ClanTerritory/Core/Bootstrap.cs`
- `Source/ClanTerritory/Config/ConfigManager.cs`
- `Source/ClanTerritory/Features/Races/RaceModule.cs`
- `README.md`
- `Docs/Research/Valheim/085_Player_Races_Core.md`

## Notes

This first version intentionally avoids transformations, visuals, and active abilities.

Those should be added only after the passive race state is stable.
