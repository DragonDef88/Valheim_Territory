# Investigation 049: Separate tree worker and terraforming scan reset

## Problem

Tree chopping was tied to the current terraforming scan point. This meant that trees were only hit when the terrain worker happened to scan near a tree.

Starting `Running` also reused the previous `TerraformingScanIndex` and `TerraformingScanProgress`, so a new run could continue from the last scan point instead of starting from the ward.

## Decision

- Split tree chopping into a separate worker pass.
- The tree worker scans the whole real ward radius, not the selected terraforming radius.
- The tree worker requires an axe in the preparation chest and spends fuel through the same work-meter as terrain/rock work.
- Terrain leveling no longer attempts tree chopping while scanning terrain points.
- Missing terrain tools no longer pause the whole running state; terrain is skipped so the independent tree worker can continue if an axe and fuel are present.
- `Running = true` now resets terrain scan state to the ward origin.

## Reset state

When running is started, the following ZDO values are reset:

- `TerraformingScanProgress = 0`
- `TerraformingScanIndex = 0`
- `TerraformingScanSpeed = 1 / LevelingScanTime`
- `TerraformingPendingScanIndex = -1`
- `TerraformingVerifyCount = 0`

This makes the visible spirit and terrain scan start from the ward instead of the previous point.
