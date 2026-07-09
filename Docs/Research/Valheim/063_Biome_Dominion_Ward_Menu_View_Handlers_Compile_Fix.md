# Investigation 063: Biome Dominion ward menu view handlers compile fix

## Problem

The Biome Dominion ward menu package added button listeners in `JotunnWardMenuView.cs`, including:

```csharp
_claimBiomeDominionButton.onClick.AddListener(RequestClaimBiomeDominion);
```

but the matching request handler methods were not inserted into the view.

This caused compile errors like:

```text
The name 'RequestClaimBiomeDominion' does not exist in the current context
```

## Fix

Added the missing Jotunn UI handler methods:

- `RequestShowBiomeDominion`
- `RequestClaimBiomeDominion`
- `RequestReleaseBiomeDominion`
- `RequestToggleBiomeDoorLock`
- `RequestDecreaseBiomeDoorAutoClose`
- `RequestIncreaseBiomeDoorAutoClose`
- `RequestToggleBiomeStructureDamageProtection`

Each method forwards to the matching stored `Action`.

## Result

The Biome tab buttons now have local handlers and can route clicks to the ward menu controller.
