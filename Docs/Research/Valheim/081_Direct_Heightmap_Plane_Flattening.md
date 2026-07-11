# Investigation 081: Direct heightmap plane flattening

## Problem

Plateautem produces cleaner platforms than our previous worker.

The previous Clan Territory implementation still relied on a vanilla-like `TerrainOp.Level` flow:

- run a small operation around a spiral point;
- let `TerrainComp.LevelTerrain` apply radial falloff;
- repeat many times.

This can create artifacts:

- small hills between operation centers;
- overlapping falloff ridges;
- missed low/high vertices;
- smoothing leftovers;
- terrain trapped around rocks and ores.

## Change

The leveling hook now treats a terrain operation as a direct plane target.

Instead of adding another local falloff delta, it:

1. reads the terrain vertex arrays from `TerrainComp`;
2. calculates the desired delta for a target plane height;
3. writes each affected vertex toward that plane;
4. uses full weight inside the inner radius;
5. uses falloff only near the outer edge;
6. clears `smoothDelta` for modified vertices.

This should reduce humps and micro-artifacts.

## Rock and ore pre-pass

Before terrain leveling, the worker now performs a broader Plateautem-like blocking-node pass:

```text
MineRock
MineRock5
```

The worker picks a blocking rock/ore node inside the flattening area and applies a pickaxe hit before leveling terrain.

## Files changed

- `Source/ClanTerritory/Features/Territory/TerritoryModule.cs`
- `README.md`
- `Docs/Research/Valheim/081_Direct_Heightmap_Plane_Flattening.md`

## Notes

This is still incremental and does not remove the old worker state model.

The important change is how the actual TerrainComp vertices are modified.
