# Investigation 071: Economy ward menu TryGetEconomyService compile fix

## Problem

`WardMenuTerritoryActions` used economy UI actions that called:

```csharp
TryGetEconomyService(...)
```

but the helper method itself was not inserted.

The previous generator check looked for the text `TryGetEconomyService` anywhere in the file. Because the calls already contained that text, it skipped adding the helper method.

## Fix

Added the missing helper:

```csharp
private static bool TryGetEconomyService(
    string actionName,
    WardId wardId,
    out EconomyService economyService)
```

The helper resolves `EconomyService` from `ServiceContainer` and logs a debug message if it is unavailable.

## Notes

The `Object.FindObjectsOfType<T>()` messages are compiler warnings from older compatibility code. They are not part of this compile failure.
