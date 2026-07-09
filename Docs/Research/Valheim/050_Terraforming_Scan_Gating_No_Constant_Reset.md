# Investigation 050: Terraforming scan gating without constant reset

## Problem

After separating the tree worker from the terrain worker, the terrain spirit could appear to keep returning to the beginning of the scan.

## Cause

Two scan-state issues were found in the current implementation:

1. `RPC_SetRunning(true)` reset the terrain scan every time the owner RPC received `true`, even when the worker was already running.
2. `AdvanceLevelingScan(...)` forced `TerraformingScanProgress` to the next integer scan index. That bypassed the intended time gate, so the terrain worker could process a new scan point every frame and wrap back to the beginning too quickly.

## Fix

- `Running = true` resets the terrain scan only on the transition from stopped to running.
- `AdvanceLevelingScan(...)` now advances `scanIndex` but preserves continuous `scanProgress`, unless the scan really wraps at the end.
- Verification now uses a cooldown ZDO key instead of pushing or rewinding `scanProgress`.
- Radius changes still intentionally reset the scan.
- Scan wrap explicitly persists `scanIndex = 0` and clears verification state.

## Expected behavior

Starting the worker begins at the ward. While it is already running, UI refreshes or duplicate true RPCs no longer pull the scan back to the ward. The spirit should move outward continuously and should not rapidly jump through indices.
