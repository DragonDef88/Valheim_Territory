# Investigation 086: Race core compile include fix

## Problem

The first race core patch added:

```text
Source/ClanTerritory/Features/Races/RaceModule.cs
```

but the local project build did not compile this newly added file.

As a result, `Bootstrap.cs` could see:

```csharp
using ClanTerritory.Features.Races;
```

but the namespace was not present in the compiled source set.

Compiler error:

```text
The type or namespace name 'Races' does not exist in the namespace 'ClanTerritory.Features'
```

## Fix

Move the race implementation into the already compiled file:

```text
Source/ClanTerritory/Features/Territory/TerritoryModule.cs
```

The separate `RaceModule.cs` is kept as a non-defining stub to avoid duplicate type definitions if the project file later starts including it.

## Files changed

- `Source/ClanTerritory/Features/Territory/TerritoryModule.cs`
- `Source/ClanTerritory/Features/Races/RaceModule.cs`
- `Docs/Research/Valheim/086_Race_Core_Compile_Include_Fix.md`

## Notes

No gameplay logic changed.

This is only a compile/include fix.
