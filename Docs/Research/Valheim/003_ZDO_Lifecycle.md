# Research 003 — ZDO Lifecycle

## Purpose

Understand how ZDOs behave as the persistent/networked backing state for Valheim objects.

This research informs Clan Territory persistence and deletion tracking.

## Inspected Classes

- `ZDOMan`
- `ZNetView`
- `ZNetScene`

## Confirmed Facts

### 1. ZDOMan owns the ZDO collection

`ZDOMan` stores and manages ZDOs.

It resets object collections during shutdown and load.

### 2. Persistent ZDOs are saved

During save preparation, `ZDOMan` clones persistent ZDOs.

Non-persistent runtime objects are not part of the persistent save clone.

### 3. ZNetView binds GameObjects to ZDOs

`ZNetView.Awake()` either:

- uses an existing ZDO from `ZNetView.m_initZDO`;
- or creates a new ZDO via `ZDOMan.CreateNewZDO()`.

After that, it loads fields and registers the instance in `ZNetScene`.

### 4. ZNetScene destroys objects through ZDO ownership

`ZNetScene.Destroy(GameObject go)`:

1. retrieves the object's `ZNetView`;
2. gets the attached ZDO;
3. resets the ZDO reference on the `ZNetView`;
4. removes the ZDO from the runtime instance map;
5. calls `ZDOMan.DestroyZDO(zdo)` if the ZDO is owned locally;
6. destroys the Unity GameObject.

### 5. DestroyZDO requires ownership

`ZDOMan.DestroyZDO(ZDO zdo)` returns early if the local peer is not the ZDO owner.

This means deletion is authority-sensitive.

## Lifecycle Summary

```text
Create or load object
   │
   ▼
ZDO exists
   │
   ▼
ZNetView binds to ZDO
   │
   ▼
ZNetScene tracks instance
   │
   ▼
Object active in Unity
   │
   ▼
Destroy requested
   │
   ▼
If owner: DestroyZDO
   │
   ▼
Object removed from runtime and network state

Architectural Conclusion

Clan Territory deletion tracking is necessary because runtime absence alone does not mean deletion.

There are two different states:

Object unloaded

and

Object deleted

These must remain separate in Clan Territory architecture.

