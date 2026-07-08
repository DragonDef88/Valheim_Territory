# Investigation 027: Territory Terraforming and Plateautem menu foundation

## Goal

Add a new ward-menu section for territory terraforming. The long-term feature is gradual terrain leveling inside the current territory, with tool slots, fuel storage, stone storage for raising terrain, and safe terrain saving through Valheim's terrain pipeline.

This package is the first safe layer: persistent controls, menu section, tool/fuel/stone cells, and ZDO/RPC state. It intentionally does not perform terrain operations yet.

## Plateautem findings

Plateautem v0.3.8 keeps dedicated ZDO values for radius, fuel, stone, scan progress, scan index, scan speed, lower-tool index, and raise-tool index. It also has separate lower tools and raise tools: pickaxes for lowering and hoe for raising. Fuel items are Wood, Coal, and Resin with different values, and Stone is a separate material used for raising terrain.

Plateautem's core leveling behavior is not a single instant operation. It scans a spiral over the work radius, samples ground around each point, computes required raise/lower work, consumes fuel, consumes or refunds stone depending on raise/lower direction, and instantiates a terrain operation at the current scan point.

The scan formula uses `PolarPointOnSpiral`, `CountPointsInSpiral`, and ground sampling with `ZoneSystem.instance.GetGroundHeight(...)`. This is the behavior we should port into Clan Territory's worker instead of directly changing the whole territory in one frame.

## Vanilla terrain pipeline findings

Valheim's `TerrainOp.Settings` supports `level`, `raise`, `smooth`, and paint settings. `TerrainOp.Awake()` finds affected heightmaps and calls `Heightmap.GetAndCreateTerrainCompiler().ApplyOperation(...)`.

`TerrainComp.ApplyOperation(...)` sends an `ApplyOperation` RPC to the owner. The owner runs `DoOperation(...)`, mutates stored terrain deltas, saves compressed data to ZDO, pokes the heightmap, and resets grass. This is the correct persistence path for terrain changes.

`Heightmap.LevelTerrain(...)` and `TerrainComp.LevelTerrain(...)` clamp player modification deltas to about +/-8m from base terrain. Clan Territory must respect that limit.

## Decisions in this package

- Add `Territory Terraforming` as a separate ward-menu tab.
- Add persistent ZDO keys for:
  - enabled/running
  - mode
  - work radius
  - target height
  - fuel stored
  - stone stored
  - hoe slot
  - pickaxe slot
  - scan progress/index
- Add `TerritoryTerraformingService` with owner-guarded RPCs.
- Add menu cells for:
  - Hoe slot
  - Pickaxe slot
  - Fuel
  - Stone
- Add controls for:
  - enable/disable
  - start/stop
  - cycle mode
  - radius +/- 2m
  - set target from ward/player height
  - place logical hoe/pickaxe
  - add logical fuel/stone

## Next step

The next package should replace the placeholder resource buttons with real item transfer and implement the gradual worker:

1. Validate mode/tool requirements.
2. Scan territory points with the Plateautem spiral.
3. Sample nearby ground points.
4. Compute raise/lower effort.
5. Consume fuel and stone.
6. Apply terrain via vanilla `TerrainOp`/`TerrainComp`.
7. Save terrain through the owner-side vanilla terrain compiler.
8. Clamp work to the ward territory radius.
