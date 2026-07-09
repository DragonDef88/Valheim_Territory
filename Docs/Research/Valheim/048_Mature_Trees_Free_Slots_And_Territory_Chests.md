# Investigation 048: Mature trees, free slots, and territory chest absorption

## Problem

The first expanded worker package added axe support and ground item absorption, but the behavior needed three refinements:

1. Tree chopping should ignore saplings / not-fully-grown trees.
2. Ground item absorption should continue into an empty slot when the chest already contains that same resource and the existing stack is full.
3. Ground item absorption should also target real world containers inside the territory, not only the ward virtual preparation/treasury storage.

## Valheim references

`TreeBase` is the grown-tree destructible class. It has a trunk object and a log prefab; when destroyed it spawns a log and configured drops. `Growup` is the growth component used for entities that later transform into a grown prefab.

`Container` exposes `GetInventory()`, `IsOwner()`, and `IsInUse()`. Its inventory change callback saves the container to ZDO when the container is owner-side.

## Decision

- `TreeBase` targets are accepted only if:
  - active in hierarchy;
  - no `Growup` component;
  - `m_logPrefab` exists;
  - `m_trunk` exists and is active.
- `TreeLog` remains a valid chopping target because it is already a harvested/fallen wood object.
- Virtual preparation/treasury absorption now:
  - requires that the exact resource already exists in that virtual chest;
  - fills matching stacks first;
  - then creates new stacks in free valid slots.
- Real world containers inside the ward radius are scanned every absorption pass:
  - virtual temporary containers are skipped;
  - containers must be owner-side and not in use;
  - item must already exist in the chest;
  - matching stacks are filled first, then empty slots are used.
- Real containers keep vanilla max stack size; virtual ward containers keep their custom stack limits.

## Safety

This preserves the "only if the chest already has that resource" rule to avoid every empty chest vacuuming the territory.
