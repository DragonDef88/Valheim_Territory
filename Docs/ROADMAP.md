# Roadmap

## Purpose

This document describes the long-term evolution of Clan Territory.

It focuses on major project milestones rather than individual implementation tasks.

Detailed sprint planning belongs to the development workflow.

---

# Current Status

Project Phase:

```
Foundation          ✅ Completed
Living World        🚧 In Progress
Protection          ⏳ Planned
Permissions         ⏳ Planned
Public API          ⏳ Planned
User Interface      ⏳ Planned
Beta                ⏳ Planned
Release 1.0         ⏳ Planned
```

---

# Phase 1 — Foundation

**Status**

✅ Completed

## Objective

Build a stable architectural foundation.

## Completed

- Domain layer
- EventBus
- Territory lifecycle
- Territory Registry
- Persistence subsystem
- JSON schema v1
- Backup system
- World information service
- Documentation foundation
- Engineering principles
- Development workflow

The project now has a stable architecture suitable for long-term development.

---

# Phase 2 — Living World

**Status**

🚧 In Progress

## Objective

Allow the runtime state to be reconstructed directly from the Valheim world.

The game world becomes the authoritative source of runtime data.

## Milestones

### Phase 2.1 — World Discovery

Discover all existing Wards.

Output:

```
IReadOnlyList<WardModel>
```

---

### Phase 2.2 — Registry Synchronization

Synchronize Territory Registry with discovered Wards.

Responsibilities:

- create missing Territories;
- remove obsolete Territories;
- rebuild runtime state.

---

### Phase 2.3 — Runtime Initialization

Prepare runtime state after plugin startup.

Responsibilities:

- world discovery;
- registry synchronization;
- persistence merge;
- gameplay ready.

---

### Phase 2.4 — Persistence Merge

Restore additional Territory data.

Examples:

- permissions;
- upgrades;
- future extensions.

Persistence never creates Territories.

---

# Phase 3 — Protection

**Status**

⏳ Planned

## Objective

Protect player structures inside Territories.

Future capabilities:

- building protection;
- Ward protection;
- configurable rules;
- ownership validation;
- damage interception.

---

# Phase 4 — Permissions

**Status**

⏳ Planned

## Objective

Implement a flexible permission system.

Examples:

- build permission;
- destroy permission;
- container access;
- interaction rules;
- administrator overrides.

---

# Phase 5 — Public API

**Status**

⏳ Planned

## Objective

Provide a stable API for external mods.

Future goals:

- territory queries;
- events;
- permission checks;
- extension points.

---

# Phase 6 — User Interface

**Status**

⏳ Planned

## Objective

Provide user-facing gameplay interfaces.

Possible features:

- territory visualization;
- administration tools;
- debug interface;
- map integration;
- status information.

---

# Beta

## Objective

Stabilize the project.

Focus areas:

- optimization;
- compatibility;
- migration support;
- documentation review;
- API stability;
- gameplay balancing.

---

# Release 1.0

The project reaches production quality.

Expected characteristics:

- stable architecture;
- complete gameplay loop;
- documented API;
- reliable persistence;
- long-term maintainability.

---

# Beyond 1.0

Potential future directions:

- diplomacy;
- economy;
- kingdom mechanics;
- NPC integration;
- quests;
- territory upgrades;
- plugin ecosystem.

These ideas are intentionally outside the scope of the first stable release.

---

# Success Criteria

Clan Territory succeeds if:

- architecture remains maintainable;
- runtime state is always consistent with the world;
- subsystems remain independent;
- new functionality can be added without major refactoring;
- documentation stays synchronized with architecture.

---

# Summary

The roadmap is organized around product evolution rather than implementation details.

Each phase builds upon the previous one.

The project grows from a solid architectural foundation toward a complete territory management framework while preserving long-term maintainability.