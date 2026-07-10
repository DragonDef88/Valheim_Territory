# Investigation 072: Unity FindObjectsByType warning cleanup

## Problem

Unity now marks this API as obsolete:

```csharp
UnityEngine.Object.FindObjectsOfType<T>()
```

The compiler warns:

```text
Object.FindObjectsOfType<T>() is obsolete
```

## Fix

Replaced the remaining calls with the newer unsorted lookup:

```csharp
UnityEngine.Object.FindObjectsByType<T>(UnityEngine.FindObjectsSortMode.None)
```

## Files changed

- `Source/ClanTerritory/Features/Territory/TerritoryModule.cs`
- `Source/ClanTerritory/Integration/Guilds/GuildsAdapter.cs`

## Notes

This is a small warning-cleanup pass only.

No gameplay rules, economy logic, biome dominion logic, ward menu logic, persistence, or Guilds behavior were intentionally changed.
