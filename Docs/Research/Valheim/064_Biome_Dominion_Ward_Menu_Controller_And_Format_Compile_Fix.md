# Investigation 064: Biome Dominion ward menu controller and format compile fix

## Problem

Some Biome Dominion ward menu method names already existed as call sites, so the package-generation script falsely assumed that the methods themselves were present.

Compile errors included missing controller methods:

- `RequestClaimBiomeDominion`
- `RequestReleaseBiomeDominion`
- `RequestToggleBiomeDoorLock`
- `RequestDecreaseBiomeDoorAutoCloseSeconds`
- `RequestIncreaseBiomeDoorAutoCloseSeconds`
- `RequestToggleBiomeStructureDamageProtection`

and missing Jotunn view formatting helpers:

- `FormatBiomeDominionOverview`
- `FormatBiomeName`
- `FormatBiomeDominionStatus`
- `FormatBiomeOwner`
- `FormatBiomeVassal`
- `FormatBiomeDoorLock`
- `FormatBiomeStructures`

## Fix

Added the missing public controller request methods in `WardMenuController.cs`.

Added the missing private formatting helpers in `JotunnWardMenuView.cs`.

## Note

`Object.FindObjectsOfType<T>() is obsolete` messages are Unity warnings, not compile-stopping errors. They can be cleaned up in a later optimization pass.
