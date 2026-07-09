# Investigation 046: Stabilize spirit, terrain steps, and rock mining

## Problem

Testing showed three behavior problems:

1. The visible spirit moved too sharply.
2. Large terrain edits caused visual artifacts and cliff-like texture stretching.
3. Mineable world rocks inside the territory were not handled by the worker/pickaxe flow.

## Findings

The current worker moved the spirit to `FloorToInt(scanProgress)`, so the visible marker jumped from point to point instead of following the continuous spiral.

The custom `TerrainComp.LevelTerrain(...)` prefix also applied the full local target-height delta, only clamped by the broad terrain compiler level delta. This could create sharp vertical terrain walls when the ward height differed a lot from the local ground.

Valheim rocks use `MineRock` and `MineRock5`; both implement `Damage(HitData)`. `MineRock` stores per-collider area health and drops its configured drop list when an area is destroyed. `MineRock5` also processes `Damage(HitData)`, supports radius hits, stores area health, and spawns drops when an area is destroyed.

## Decision

This package stabilizes behavior without adding new UI:

- spirit follows continuous `scanProgress`, not floored scan index;
- spirit uses `Vector3.SmoothDamp` and max speed instead of direct high-speed lerp;
- terrain operation radius is reduced from `4m` to `1.75m`;
- each terrain operation applies only a small capped height delta;
- terrain falloff is smoothed with `Mathf.SmoothStep`;
- verification no longer rewinds scan progress;
- worker can apply one pickaxe hit to nearby `MineRock` / `MineRock5` objects inside the territory scan area.

## Safety

Rock mining uses Valheim `Damage(HitData)` so vanilla tool tier checks, damage modifiers, ZDO health, effects, and drops remain controlled by Valheim object logic.
