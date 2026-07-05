# Research 004 — Zone Loading

## Purpose

Understand how Valheim decides which objects are loaded into runtime.

This research supports the separation between loaded runtime state and full persistent world state.

## Inspected Classes

- `ZoneSystem`
- `ZNetScene`
- `ZDOMan`

## Confirmed Facts

### 1. ZoneSystem tracks active world areas

`ZoneSystem` exposes active area values used by ZDO and scene logic.

These active areas determine which world sectors are relevant for object ownership and runtime creation.

### 2. ZNetScene checks whether an area is ready

`ZNetScene.IsAreaReady(Vector3 point)`:

1. converts a point into a zone;
2. checks whether the zone is loaded;
3. finds ZDOs in nearby sectors;
4. checks whether valid ZDOs have runtime instances.

This confirms that runtime readiness is tied to zone loading.

### 3. ZNetScene removes objects outside current active sets

`ZNetScene.RemoveObjects(...)` removes Unity runtime instances that are no longer part of the current near/distant object sets.

If the removed ZDO is not persistent and is owned locally, it can also be destroyed.

Persistent ZDOs can lose runtime instances without being deleted from the world.

### 4. Loaded runtime state is partial by design

Valheim does not keep every world object instantiated as a Unity object.

It creates runtime objects from ZDOs when zones are active and removes runtime instances when zones are no longer active.

## Runtime vs Persistence

```text
Persistent world:
All saved ZDOs

Runtime world:
Only objects in active / loaded zones

Architectural Conclusion

Clan Territory must follow the same separation:

Persistence:
Complete world

Runtime:
Loaded / discovered subset

Gameplay:
Operates on runtime only

This validates the existing Clan Territory architecture and supports future Runtime Restore work.