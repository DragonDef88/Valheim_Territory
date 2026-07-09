# Investigation 038: Territory leveling worker from ward height

## Goal

Make territory leveling work from the ward height instead of using a separate Plateautem object.

## Plateautem references

Plateautem scans outward from its object with a spiral:

- `CountPointsInSpiral(radius + spacing, spacing)`
- `PolarPointOnSpiral(scanProgress, spacing)`
- current operation point = object position + spiral offset

It samples terrain around that point, computes whether terrain should be raised or lowered, consumes resources, then applies a terrain level operation at the sampled point.

## Clan Territory decision

Clan Territory now uses the same high-level model, but the source point is the ward:

- center = ward / `PrivateArea.transform.position`
- target height = ward Y coordinate
- scan radius = configured territory leveling radius, clamped to ward radius
- scan state is saved in ward ZDO through existing scan index/progress keys
- resources and tools are read from the virtual preparation chest stored in ward ZDO

## First implementation

The first worker implementation is deliberately conservative:

- one active ward is processed per worker tick
- up to 12 spiral scan points are inspected per tick
- one terrain operation is applied per tick
- operation radius is small (`2m`) to avoid sudden world changes
- fuel cost is one fuel item per applied operation
- raising also consumes one stone
- missing pickaxe, hoe, fuel, or stone pauses the worker

## Terrain persistence

The worker applies terrain through Valheim `TerrainComp`:

- find/create the terrain compiler for the affected heightmap
- invoke `TerrainComp.DoOperation(...)` through reflection
- use a `TerrainOp.Settings` level operation with the ward Y as the target height

This keeps terrain changes in Valheim terrain compiler/ZDO persistence instead of custom JSON.
