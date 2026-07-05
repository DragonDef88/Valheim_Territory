# RFC-003 — Runtime Restore Model

Status: Proposed

Author: Clan Territory Engineering

Related:

- RFC-002 — Runtime Restore Pipeline
- Audit-005 — Runtime Implementation Review

---

# Purpose

Define the data model used between Persistence and Runtime during restore.

RFC-002 activates the Runtime Pipeline.

This RFC defines what data is passed through that pipeline when restoring runtime state.

---

# Problem

`PersistenceService.LoadNow()` currently exists as a placeholder.

The next runtime pipeline step requires real loaded data.

Before implementing restore, the project must define:

- what Persistence returns;
- what Runtime receives;
- who maps persistence records into runtime records;
- where temporary restore data lives;
- which layer owns each responsibility.

---

# Core Rule

Persistence returns persistence data.

Runtime builds runtime state.

Gameplay builds gameplay.

```text
Persistence
    │
    ▼
Persistence Snapshot
    │
    ▼
Runtime Restore
    │
    ▼
Runtime Registry
    │
    ▼
Gameplay
Non Goals

This RFC does not introduce:

Unity object spawning;
ZDO creation;
prefab placement;
Valheim object reconstruction;
terrain modification;
new Harmony hooks.

Runtime Restore must not replace Valheim streaming.

Proposed Model
SaveFileModel

Existing persistence model.

Owned by:

Features/Persistence

Used for:

JSON serialization;
merge save;
backup;
persistence schema.

Persistence may load this model from disk.

Runtime should not mutate it.

RuntimeRestoreSnapshot

New proposed runtime-facing model.

Owned by:

Features/Runtime/Restore

Purpose:

provide immutable restore input to Runtime;
hide persistence implementation details from Runtime steps;
allow future systems beyond Territory.

Initial structure:

internal sealed class RuntimeRestoreSnapshot
{
    public IReadOnlyList<RuntimeWardRestoreRecord> Wards { get; }
}
RuntimeWardRestoreRecord

New proposed runtime-facing record.

Owned by:

Features/Runtime/Restore

Purpose:

represent ward data needed to rebuild Runtime Registry;
avoid leaking WardRecord into Runtime;
avoid leaking JSON schema into Runtime.

Initial fields:

internal sealed class RuntimeWardRestoreRecord
{
    public WardId WardId { get; }
    public Vector3 Position { get; }
}

This is intentionally smaller than WardRecord.

Runtime should only receive what it needs.

Mapping Responsibility

Mapping from Persistence to Runtime should be explicit.

Proposed component:

RuntimeRestoreMapper

Owned by:

Features/Runtime/Restore

Input:

SaveFileModel

Output:

RuntimeRestoreSnapshot

Reason:

Persistence should not build Runtime objects.

Runtime should not understand full persistence schema.

A mapper at the Runtime boundary keeps both layers clean.

Temporary Restore State

Pipeline steps need to pass data between phases.

Example:

PersistenceLoadStep
    │
    ▼
RuntimeRestoreStep

A pipeline step should not store loaded data in static globals.

Proposed component:

RuntimeRestoreContext

Owned by:

Features/Runtime/Restore

Responsibilities:

store the latest loaded SaveFileModel;
store the latest mapped RuntimeRestoreSnapshot;
clear state when restore completes or fails.

This is pipeline-scoped state, not long-term Runtime Registry state.

Proposed Flow
DiscoveryCompleted
    │
    ▼
PersistenceLoadStep
    │
    ├── calls IPersistenceService.LoadSnapshot()
    │
    └── stores SaveFileModel in RuntimeRestoreContext
    │
    ▼
RuntimeRestoreStep
    │
    ├── maps SaveFileModel to RuntimeRestoreSnapshot
    │
    ├── rebuilds RuntimeRegistry
    │
    └── stores restore result
    │
    ▼
RegistrySynchronized
Required Persistence API Change

Current API:

void LoadNow();

Required API:

SaveFileModel LoadSnapshot();

Reason:

Restore pipeline needs loaded data, not only a side effect.

LoadNow() may remain temporarily as compatibility wrapper, but RFC-002 implementation should use a data-returning API.

Runtime Registry Restore Rule

Runtime restore should rebuild Runtime Registry from restore snapshot.

It must not:

create Unity objects;
create Valheim objects;
spawn prefabs;
write JSON;
overwrite Persistence.

Registry restore is in-memory only.

First Implementation Phases
Phase 1

Add restore model classes:

RuntimeRestoreSnapshot
RuntimeWardRestoreRecord
RuntimeRestoreContext

No behavior change.

Phase 2

Add LoadSnapshot() to IPersistenceService.

Initially it may return an empty SaveFileModel or real loaded data depending on existing storage readiness.

Phase 3

Update PersistenceLoadStep to store loaded data in RuntimeRestoreContext.

Phase 4

Add RuntimeRestoreStep.

It maps loaded persistence data into Runtime restore data.

Phase 5

Populate RuntimeRegistry.

Acceptance Criteria

This RFC is implemented when:

Persistence exposes a data-returning load API;
Runtime restore does not depend directly on JSON files;
Runtime restore does not mutate persistence models;
Runtime Registry can be rebuilt from restore snapshot;
pipeline state is passed through RuntimeRestoreContext;
project rebuild succeeds.
Final Decision

Proposed.

Do not implement RuntimeRestoreStep until this restore model exists.