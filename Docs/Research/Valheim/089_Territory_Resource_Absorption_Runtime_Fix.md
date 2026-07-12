# Investigation 089: Territory resource absorption runtime fix

## Problem

The resource absorption implementation existed but never executed.

`TerritoryRunner.Update()` deliberately skipped
`TerritoryTerraformingService.Update()`.

The service also returned immediately when built-in terraforming was
disabled. As a result, disabling terrain, rock and tree workers also
disabled the independent ground-item absorption worker.

## Intended behavior

Every two seconds, an owned ward may process ground item stacks inside
its radius.

A ground stack is moved only when a destination already contains a
matching stack:

1. matching preparation storage;
2. matching treasury storage;
3. matching stack in a real territory container.

The worker does not collect building pieces and does not process a
container while it is open.

## Fix

`TerritoryRunner` now calls `TerritoryTerraformingService.Update()`.

Inside the service:

1. resource absorption runs before the built-in terraforming gate;
2. `BuiltInTerraformingEnabled == false` still prevents leveling, rock
   mining and tree workers;
3. no terrain behavior is re-enabled.

## Files changed

- `Source/ClanTerritory/Features/Territory/TerritoryModule.cs`
- `Docs/Research/Valheim/089_Territory_Resource_Absorption_Runtime_Fix.md`

## Validation

1. Rebuild `ClanTerritory.sln`.
2. Start Valheim and enter an owned territory.
3. Put one resource stack into a real chest inside the ward radius.
4. Drop another stack of the same item on the ground inside the radius.
5. Close the chest and wait at least two seconds.
6. Confirm the ground stack disappears and the chest stack increases.
7. Confirm a resource without a matching destination stack remains on
   the ground.
8. Confirm the log contains a line similar to:

```text
[TerritoryResources] Absorbed ground item stacks:
```

9. Confirm terrain, rocks and trees are not processed automatically.