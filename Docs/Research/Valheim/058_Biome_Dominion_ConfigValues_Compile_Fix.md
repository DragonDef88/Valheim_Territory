# Investigation 058: Biome Dominion ConfigValues compile fix

## Problem

The first biome dominion package appended the initial BiomeDominion implementation into `TerritoryModule.cs`.

Inside that appended implementation, the default biome door auto-close value used:

```csharp
ConfigValues.DoorAutoCloseSeconds
```

`TerritoryModule.cs` did not import `ClanTerritory.Config`, so compile could fail with:

```text
The name 'ConfigValues' does not exist in the current context
```

## Fix

The references are now fully qualified:

```csharp
ClanTerritory.Config.ConfigValues.DoorAutoCloseSeconds
```

`using ClanTerritory.Config;` was also added for clarity.

## Result

Biome dominion default auto-close seconds now compiles and uses the same configured default as normal territory rules.
