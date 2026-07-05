# RFC-010 — Runtime Restore Pipeline

Status: Proposed

Author: Clan Territory Engineering

---

# Purpose

Introduce a Runtime Restore Pipeline that reconstructs the loaded runtime world from persistent storage.

This RFC defines architectural responsibilities only.

It does not introduce implementation details.

---

# Problem Statement

Current architecture supports:

- Runtime discovery
- Runtime registry
- Territory creation
- Merge Save
- Delete tracking

Persistence can now save a complete world.

However, the reverse direction does not yet exist.

Current flow:

```
Valheim Runtime
        │
        ▼
World Discovery
        │
        ▼
Runtime Registry
        │
        ▼
Territory
        │
        ▼
Merge Save
```

Missing:

```
Save File

↓

Runtime Restore
```

This creates an asymmetric architecture.

---

# Design Goals

The Runtime Restore Pipeline must:

- restore runtime state from persistent storage;
- rebuild runtime representation only;
- never instantiate Unity objects;
- never replace Valheim streaming;
- never duplicate World Discovery.

---

# Non Goals

This RFC does not introduce:

- prefab spawning;
- Unity object creation;
- ZDO creation;
- Harmony patches;
- gameplay initialization.

Those remain outside Persistence.

---

# Architectural Principles

## Principle 1

Persistence owns data.

Runtime owns loaded objects.

Gameplay owns behaviour.

---

## Principle 2

Persistence never creates gameplay.

Gameplay never reads JSON directly.

---

## Principle 3

Runtime is reconstructed from persistence.

Persistence is never reconstructed from runtime.

---

# Proposed Pipeline

```
World Save

↓

PersistenceService.Load()

↓

WorldSnapshot

↓

Runtime Restore Pipeline

↓

Runtime Registry

↓

Gameplay Initialization

↓

Normal Runtime
```

---

# Responsibilities

## Persistence

Responsible for:

- reading save files;
- validating data;
- mapping persistence models;
- producing an immutable world snapshot.

Persistence is **not** responsible for gameplay.

---

## Runtime Restore Pipeline

Responsible for:

- receiving a world snapshot;
- rebuilding runtime representations;
- populating runtime registries;
- publishing runtime restore events.

Runtime Restore Pipeline does **not** create Unity objects.

---

## Runtime Registry

Responsible for:

- storing loaded runtime objects;
- providing runtime lookup;
- exposing runtime state to gameplay systems.

---

## Gameplay

Gameplay reacts after runtime exists.

Gameplay must never read persistence files directly.

---

# Event Flow

```
Persistence Loaded

↓

Runtime Restore Started

↓

Runtime Registry Rebuilt

↓

Runtime Restore Completed

↓

Gameplay Ready
```

This follows the event-driven architecture already used by Runtime.

---

# Future Extension

The Runtime Restore Pipeline should become the common entry point for:

- territory restoration;
- NPC restoration;
- settlement restoration;
- economy restoration;
- road network restoration;
- future living-world systems.

No gameplay system should implement its own persistence loading.

---

# Alternatives Considered

## Option A

Persistence directly creates gameplay.

Rejected.

Reason:

Violates separation of responsibilities.

---

## Option B

Gameplay loads persistence.

Rejected.

Reason:

Gameplay becomes coupled to serialization.

---

## Option C

Runtime Restore Pipeline

Accepted.

Reason:

Preserves architecture.

Matches Living World principles.

---

# Risks

Runtime Restore may become too large.

Mitigation:

Split into restore stages when necessary.

Examples:

```
Runtime Restore

↓

Territory Restore

↓

NPC Restore

↓

Economy Restore

↓

Road Restore
```

---

# Dependencies

This RFC depends on:

- Merge Save
- Runtime Registry
- Territory Registry
- Runtime State Machine

---

# Implementation Plan

Phase 1

- Introduce Runtime Restore Pipeline.

Phase 2

- Restore Territory runtime state.

Phase 3

- Publish Runtime Restore events.

Phase 4

- Allow gameplay systems to subscribe.

---

# Expected Benefits

- Symmetric save/load architecture.
- Clear separation between Persistence and Gameplay.
- Runtime becomes reconstructible.
- Future systems reuse one restore mechanism.
- No duplication of persistence logic.

---

# Decision

Accepted for future implementation.

Implementation will begin only after the architecture audit series is completed.