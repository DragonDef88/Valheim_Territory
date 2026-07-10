# Investigation 068: Economy upkeep helper duplicate compile fix

## Problem

The territory upkeep / vassal tribute package inserted economy helper methods into the wrong service block.

The helpers:

```csharp
GetPrivateAreas()
GetZdo(...)
```

were inserted inside `BiomeDominionService`, which already had methods with the same signatures.

This caused compile errors:

```text
Type 'BiomeDominionService' already defines a member called 'GetPrivateAreas' with the same parameter types
Type 'BiomeDominionService' already defines a member called 'GetZdo' with the same parameter types
```

## Fix

Removed the mistakenly inserted duplicate helpers from `BiomeDominionService`.

Added economy-specific helpers inside the Economy service with unique names:

```csharp
FindEconomyPrivateAreaAt(...)
GetEconomyPrivateAreas()
GetEconomyZdo(...)
```

## Result

BiomeDominion keeps its original helpers.

Economy upkeep uses its own helper names and no longer collides with BiomeDominion.
