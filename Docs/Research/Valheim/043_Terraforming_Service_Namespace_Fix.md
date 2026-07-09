# Investigation 043: Terraforming service namespace compile fix

## Problem

After the leveling spirit brace fix, `TerritoryTerraformingService` compiled in the wrong namespace:

`ClanTerritory.Features.Territory`

But the rest of the project imports it from:

`ClanTerritory.Features.Territory.Services`

This caused:

`The type or namespace name 'TerritoryTerraformingService' could not be found`

## Decision

Close the `ClanTerritory.Features.Territory` namespace before the service declaration and restore the service namespace block:

```csharp
namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryTerraformingService
```

This is a compile-only namespace fix.
