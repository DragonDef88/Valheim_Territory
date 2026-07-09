# Investigation 039: Smoother Plateautem-style leveling

## Problem

The first ward-height worker worked, but it was too fast and visually rough.

The worker used a direct Valheim `TerrainComp.DoOperation(...)` level operation. Vanilla `TerrainComp.LevelTerrain(...)` pulls all vertices in the operation radius directly to the target height, which creates abrupt changes.

## Plateautem comparison

Plateautem improves this in two important ways:

1. It scans outward from the object with a spiral.
2. It patches `TerrainComp.LevelTerrain(...)` so the level delta is weighted by distance:
   - full strength in the center
   - softer falloff near the edge

Plateautem also samples multiple nearby ground points before deciding whether an operation is worth applying.

## Decision

Clan Territory now keeps the ward-height target, but changes the leveling worker to be closer to Plateautem:

- slower worker interval
- fewer scan attempts per tick
- multi-point local sampling around each scan point
- threshold before applying terrain work
- wider `4m` terrain operation radius
- Plateautem-style falloff patch only while Clan Territory is applying its own ward-height operation

## Important safety detail

The `TerrainComp.LevelTerrain(...)` hook is gated by a static Clan Territory flag.

Normal Valheim hoe leveling and other mods should continue through vanilla behavior. The falloff prefix only runs during `TerritoryTerraformingService.ApplyWardHeightLevelingOperation(...)`.
