# Investigation 061: Biome Dominion ward menu helper compile fix

## Problem

The Biome Dominion ward menu package added calls to:

```csharp
TryGetBiomeDominionService(...)
```

inside `WardMenuTerritoryActions`, but the helper method itself was not inserted into the file.

This caused the compile error:

```text
The name 'TryGetBiomeDominionService' does not exist in the current context
```

## Fix

Added the missing helper method:

```csharp
private static bool TryGetBiomeDominionService(
    string actionName,
    WardId wardId,
    out BiomeDominionService biomeDominionService)
```

The helper resolves `BiomeDominionService` from `ServiceContainer`.

## Result

Ward menu Biome tab actions can now access the already registered Biome Dominion service.
