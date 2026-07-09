# Investigation 051: Tree worker stump chopping

## Problem

The separate tree worker handled grown `TreeBase` and fallen `TreeLog`, but stumps were not included.

## Valheim reference

Stumps are handled through Valheim destructible logic rather than the same grown tree target path. `Destructible.Damage(HitData)` forwards damage through its ZNetView RPC, applies resistances, checks tool tier, stores health on ZDO, and destroys the object when health reaches zero.

## Decision

The tree worker now also scans `Destructible` objects in the territory and accepts only objects whose prefab-style name contains `stump` or `stub`.

The worker still uses the axe slot and creates the same axe `HitData`, so vanilla chop damage, tool tier, resistances, effects, and destruction behavior remain controlled by Valheim.

## Safety

The stump filter explicitly skips objects that are already `TreeBase`, `TreeLog`, `MineRock`, `MineRock5`, or `Growup` so the generic destructible path does not interfere with tree, log, rock, or young-growth handling.
