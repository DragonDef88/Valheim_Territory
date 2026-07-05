# Architecture Audit 002 — Runtime & Persistence

## Purpose

This document audits the Runtime and Persistence layers of Clan Territory.

The goal is to evaluate whether the current architecture still matches the project's core rule:

> Runtime represents the loaded world.
> Persistence owns the complete world.

This audit is based on:

- Current GitHub main
- Valheim runtime research
- ZDO / ZNetView / WearNTear lifecycle research
- Current Merge Save and Delete Tracking implementation

---

# Audit Status

| Layer | Status |
|--------|--------|
| Runtime Registry | PASS |
| Runtime Ward Model | WATCH |
| Runtime Orchestration | WATCH |
| Persistence Save Flow | WATCH |
| Persistence Merge Save | PASS |
| Persistence Delete Tracking | WATCH |
| Persistence Load Flow | ACTION |

---

# Runtime Registry

## Responsibility

The Runtime Registry should store only loaded runtime objects.

It must not represent the full persistent world.

It must not become a replacement for Persistence.

---

## Current Structure

`RuntimeRegistry` stores runtime wards in a dictionary keyed by `WardId`.

It supports:

- add
- remove
- lookup
- get all
- clear

---

## Assessment

Status:

PASS

The registry is simple and correctly scoped.

It does not attempt to load the whole world.

It does not perform persistence operations.

It does not contain gameplay rules.

This aligns with Valheim's own architecture, where runtime objects are only the currently loaded representation of persistent ZDO state.

---

## Recommendation

No immediate changes required.

Keep `RuntimeRegistry` small.

Do not add persistence behavior here.

---

# Runtime Ward Model

## Responsibility

`RuntimeWard` represents a loaded ward in the runtime layer.

It should describe runtime state only.

---

## Current Structure

`RuntimeWard` stores:

- `WardId`
- position
- loaded state
- active state

It also supports:

- activate
- deactivate

---

## Assessment

Status:

WATCH

The model is currently small and acceptable.

However, it already uses Unity's `Vector3`.

That is acceptable for runtime because runtime is game-engine-facing.

It would not be acceptable in Domain or Persistence.

---

## Recommendation

No immediate refactor required.

If runtime entities grow, consider introducing runtime-specific position wrappers later.

Do not move `RuntimeWard` into Domain.

---

# Runtime Orchestration

## Responsibility

Runtime orchestration should coordinate runtime startup and discovery.

It should not create gameplay state directly unless that is explicitly part of the runtime startup phase.

---

## Current Structure

`RuntimeOrchestrator` listens for `RuntimeStateChangedEvent`.

When the state becomes `WorldLoaded`, it runs world discovery and creates territories from discovered wards.

---

## Assessment

Status:

WATCH

The current flow works.

However, `RuntimeOrchestrator` currently depends on both:

- `IWorldDiscoveryService`
- `ITerritoryService`

This means runtime orchestration directly triggers gameplay territory creation.

This is acceptable at the current project size, but it may become too tightly coupled as the Living World Framework grows.

Valheim research suggests a cleaner pattern:

```text
World state
   │
   ▼
Runtime objects
   │
   ▼
Lifecycle events
   │
   ▼
Gameplay systems
```

Clan Territory is currently closer to:

```text
Runtime state change
   │
   ▼
WorldDiscovery
   │
   ▼
TerritoryService.CreateTerritoryFromWard()
```

This is not wrong, but it should be watched.

---

## Recommendation

No immediate code change required.

Future RFC candidate:

```text
Runtime Discovery Events
```

Instead of directly calling `TerritoryService`, runtime discovery could publish a discovered-ward event.

Gameplay systems could then react through the event bus.

---

# Persistence Save Flow

## Responsibility

Persistence should save the complete world state.

It should not treat loaded runtime state as the full world.

---

## Current Structure

`PersistenceService.SaveNow()`:

1. creates a snapshot;
2. gets world save path;
3. merges the snapshot with existing save data;
4. creates backup;
5. saves merged data;
6. clears deleted ward tracking.

---

## Assessment

Status:

WATCH

The current implementation works and fixes the original data-loss problem.

However, `PersistenceService` currently performs several responsibilities:

- snapshot creation;
- merge-save;
- delete tracking;
- backup coordination;
- final save write.

This is acceptable now, but it is a growth point.

As Persistence expands, these responsibilities may need to split into smaller services.

---

## Recommendation

No immediate refactor required.

Future RFC candidates:

```text
Persistence Snapshot Provider
Persistence Merge Engine
Persistence Deletion Journal
```

---

# Persistence Merge Save

## Responsibility

Merge Save must preserve unloaded world records while updating loaded or changed records.

---

## Current Structure

The merge flow loads existing save data, indexes records by key, adds existing records, then overlays snapshot records.

Deleted records are skipped when loading existing records.

---

## Assessment

Status:

PASS

This is architecturally correct.

It matches the core rule:

```text
Runtime is partial.
Persistence is complete.
```

Merge Save protects unloaded territories from being erased by runtime-only snapshots.

---

## Recommendation

Keep this behavior.

Do not return to overwrite-only saves.

---

# Persistence Delete Tracking

## Responsibility

Delete Tracking must distinguish:

```text
unloaded
```

from:

```text
deleted
```

---

## Current Structure

`PersistenceService` stores deleted ward ids in an in-memory `HashSet<string>`.

`MarkWardDeleted()` adds ids.

`SaveNow()` clears the set after saving.

---

## Assessment

Status:

WATCH

The current mechanism is valid for immediate deletion followed by save.

However, the delete journal is currently in-memory only.

This is safe for the current flow because deletion immediately triggers save.

But as the project grows, this can become fragile if:

- save fails;
- multiple deletion sources appear;
- deletion is detected asynchronously;
- world shutdown interrupts the flow;
- future systems need persistent tombstones.

---

## Recommendation

No immediate code change required.

Future RFC candidate:

```text
Persistent Deletion Journal
```

This should only be introduced when deletion events become more complex.

---

# Persistence Load Flow

## Responsibility

Persistence must be able to load saved world state.

Runtime must then restore or discover loaded representations from that state.

---

## Current Structure

`LoadNow()` currently only logs that loading is prepared.

---

## Assessment

Status:

ACTION

Persistence can now save a complete merged world, but it does not yet load it back into the system.

This means the save pipeline is ahead of the load pipeline.

For a Persistent Living World Framework, this is a real missing capability.

This should become one of the next major architecture tasks.

---

## Recommendation

Create an RFC for:

```text
Runtime Restore Pipeline
```

The goal should not be to make Persistence directly create gameplay objects.

Recommended direction:

```text
Save File
   │
   ▼
Persistence Load
   │
   ▼
World Snapshot
   │
   ▼
Runtime Restore Pipeline
   │
   ▼
Runtime Registry
   │
   ▼
Gameplay
```

Persistence should load data.

Runtime Restore should rebuild runtime state.

Gameplay should react after runtime state exists.

---

# What Should Not Be Changed

The following should not be refactored yet:

- `RuntimeRegistry`
- Merge Save behavior
- `WardId`-based merge keys
- Backup creation before write
- Runtime / Persistence conceptual separation

These systems are currently aligned with the project architecture.

---

# RFC Candidates

## RFC-010 — Runtime Restore Pipeline

Status:

Required

Reason:

`LoadNow()` is currently not implemented.

---

## RFC-011 — Persistence Snapshot Provider

Status:

Optional / future

Reason:

`PersistenceService` currently builds snapshots itself.

---

## RFC-012 — Persistence Merge Engine

Status:

Optional / future

Reason:

Merge logic is still small, but may grow.

---

## RFC-013 — Persistent Deletion Journal

Status:

Optional / future

Reason:

Current in-memory delete tracking is acceptable for immediate-save flow, but may not be enough later.

---

## RFC-014 — Runtime Discovery Events

Status:

Optional / future

Reason:

`RuntimeOrchestrator` currently calls `TerritoryService` directly.

---

# Final Assessment

The Runtime and Persistence architecture is healthy.

There is no evidence that the existing systems need a major rewrite.

The main architectural gap is the missing load / restore side of Persistence.

Result:

```text
Runtime Registry: PASS
Runtime Orchestration: WATCH
Persistence Save: WATCH
Merge Save: PASS
Delete Tracking: WATCH
Persistence Load: ACTION
```

Next recommended architecture task:

```text
RFC-010 — Runtime Restore Pipeline
```