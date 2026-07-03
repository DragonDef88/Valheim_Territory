# System Lifecycle

## Purpose

This document describes how Clan Territory behaves throughout its lifetime.

Unlike the architecture document, which explains system structure, this document explains **when** each subsystem operates and **why**.

Every subsystem has a defined place within the lifecycle.

---

# Lifecycle Overview

```
Valheim Starts
        │
        ▼
BepInEx Loads Plugin
        │
        ▼
Bootstrap.Initialize()
        │
        ▼
Infrastructure Ready
        │
        ▼
Runtime Initialization
        │
        ▼
World Discovery
        │
        ▼
Registry Synchronization
        │
        ▼
Persistence Merge
        │
        ▼
Gameplay Ready
        │
        ▼
Gameplay Events
        │
        ▼
Shutdown
```

---

# Plugin Lifecycle

The plugin lifecycle is controlled by BepInEx.

```
Valheim
        │
        ▼
BepInEx
        │
        ▼
Plugin
        │
        ▼
Bootstrap.Initialize()
```

Responsibilities:

- create infrastructure;
- initialize configuration;
- register modules;
- apply Harmony patches.

Bootstrap **must not** execute gameplay logic.

---

# Infrastructure Lifecycle

Infrastructure exists before gameplay.

```
Bootstrap
        │
        ▼
Config
        │
        ▼
EventBus
        │
        ▼
ServiceContainer
        │
        ▼
Modules
        │
        ▼
Harmony
```

At this stage:

- no Territories exist;
- Registry is empty;
- Discovery has not started.

The infrastructure is only prepared.

---

# Runtime Initialization

After infrastructure is ready, runtime initialization prepares the current world state.

```
Infrastructure Ready
        │
        ▼
Runtime Initialization
        │
        ▼
Gameplay Ready
```

Runtime initialization is responsible for coordinating startup.

Future responsibilities include:

- world discovery;
- registry synchronization;
- persistence merge;
- runtime validation.

---

# World Discovery

Purpose:

> Determine which Wards currently exist in the world.

Input:

```
Valheim World
```

Output:

```
IReadOnlyList<WardModel>
```

Responsibilities:

- scan the world;
- identify valid Wards;
- create Ward models.

World Discovery **must not**:

- modify Registry;
- create Territories;
- save files;
- publish gameplay events.

Discovery is read-only.

---

# Registry Synchronization

Purpose:

> Make Registry match the current world.

Input:

```
WardModel[]
```

Output:

```
Territory Registry
```

Responsibilities:

- create missing Territories;
- remove obsolete Territories;
- rebuild runtime state.

Registry Synchronization is the only subsystem responsible for rebuilding runtime state.

---

# Persistence Merge

Purpose:

Restore additional Territory information.

Examples:

- permissions;
- upgrades;
- future extensions.

Persistence Merge never creates Territories.

If a Ward does not exist, no Territory may be restored.

---

# Gameplay Lifecycle

Once runtime initialization completes, normal gameplay begins.

Typical event flow:

```
Ward Placed
        │
        ▼
WardRegisteredEvent
        │
        ▼
Territory Service
        │
        ▼
Registry
        │
        ▼
Persistence
```

Removing a Ward follows the same pattern.

```
Ward Destroyed
        │
        ▼
WardDestroyedEvent
        │
        ▼
Territory Service
        │
        ▼
Registry
        │
        ▼
Persistence
```

Gameplay never bypasses Registry.

---

# Runtime State

Runtime state consists of:

- Territory Registry;
- active Ward models;
- runtime services;
- event subscriptions.

Runtime state can always be rebuilt from the game world.

---

# Shutdown Lifecycle

Shutdown occurs in reverse order.

```
Gameplay Stops
        │
        ▼
Harmony Unpatch
        │
        ▼
Module Shutdown
        │
        ▼
EventBus Clear
        │
        ▼
ServiceContainer Clear
        │
        ▼
Process Exit
```

Runtime state is discarded.

Persistent data should already be safely stored.

---

# Lifecycle Rules

1. Bootstrap prepares infrastructure only.
2. Runtime Initialization prepares the world.
3. World Discovery only reads the world.
4. Registry Synchronization updates runtime state.
5. Persistence restores additional information only.
6. Gameplay operates on Registry.
7. Shutdown releases runtime resources.
8. Runtime state is always rebuildable.
9. The Valheim world is the source of truth.
10. Every subsystem has exactly one place in the lifecycle.

---

# Summary

Clan Territory separates infrastructure, runtime initialization and gameplay into distinct lifecycle stages.

This separation keeps startup deterministic, gameplay predictable and runtime state fully reconstructible from the Valheim world.

Every subsystem has one responsibility and one well-defined position within the lifecycle.

#Related Documents

• ARCHITECTURE.md
• PERSISTENCE_SPECIFICATION.md
• DEVELOPMENT_WORKFLOW.md