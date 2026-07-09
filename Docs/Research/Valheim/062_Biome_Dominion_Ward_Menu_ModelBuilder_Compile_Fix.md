# Investigation 062: Biome Dominion ward menu model builder compile fix

## Problem

The Biome Dominion ward menu package updated `WardMenuModelBuilder.Build(...)` to call:

```csharp
BuildBiomeDominionSection(...)
```

but the helper method itself was not inserted into `WardMenuModelBuilder.cs`.

This caused:

```text
The name 'BuildBiomeDominionSection' does not exist in the current context
```

## Fix

Added:

```csharp
private static WardMenuBiomeDominionSection BuildBiomeDominionSection(
    PrivateArea privateArea,
    Player player)
```

The method resolves `BiomeDominionService` from `ServiceContainer`, asks it for `BiomeDominionMenuState`, and converts that state into `WardMenuBiomeDominionSection`.

## Result

The ward menu model now includes Biome Dominion state for the Biome tab.
