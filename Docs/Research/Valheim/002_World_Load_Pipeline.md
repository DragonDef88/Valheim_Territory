# Research 002 — World Load Pipeline

## Purpose

Understand how Valheim restores persistent world objects into runtime Unity objects.

This research supports the design of Clan Territory's future Runtime Restore Pipeline.

## Inspected Classes

- `ZDOMan`
- `ZNetScene`
- `ZNetView`
- `ZoneSystem`
- `PrivateArea`

## Confirmed Facts

### 1. Valheim stores persistent objects as ZDOs

`ZDOMan` manages ZDO objects.

During save preparation, persistent ZDOs are cloned into save data.

Only persistent ZDOs are included in the save clone.

### 2. ZNetScene creates Unity objects from ZDOs

`ZNetScene.CreateObject(ZDO zdo)` uses the ZDO prefab hash to find a prefab.

Then it sets:

```text
ZNetView.m_useInitZDO = true
ZNetView.m_initZDO = zdo

Existing ZDO path:
ZNetView.m_initZDO != null
    → use existing ZDO

New object path:
ZNetView.m_initZDO == null
    → create new ZDO
	
	This means loaded world objects and newly placed world objects share the same runtime component model, but differ in how their ZDO is obtained.

4. ZNetScene tracks active instances

After ZNetView.Awake() finishes loading fields, it calls:

ZNetScene.instance.AddInstance(zdo, znetView)

This marks the ZDO as created and maps it to the loaded runtime object.

5. Zone readiness affects object creation

ZNetScene.IsAreaReady() checks whether a zone is loaded and whether valid ZDOs in that sector already have created instances.

This means object creation is tied to zone loading and active area state.

Pipeline Summary
World Save
   │
   ▼
ZDOMan.Load()
   │
   ▼
ZDO collection
   │
   ▼
Zone / active area logic
   │
   ▼
ZNetScene.CreateObject(ZDO)
   │
   ▼
ZNetView.Awake()
   │
   ▼
Piece / PrivateArea / gameplay components
Architectural Conclusion

Valheim does not treat runtime objects as the source of truth.

Valheim restores runtime objects from persistent ZDO state.

Clan Territory should mirror this principle:

Clan Territory Save File
   │
   ▼
Persistence
   │
   ▼
Runtime Restore Pipeline
   │
   ▼
Runtime Registry
   │
   ▼
Gameplay

Persistence should load data.

Runtime Restore should construct runtime state.

Gameplay should operate on runtime state.