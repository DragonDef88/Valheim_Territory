# RFC-001 — Valheim Integration Layer

Status: Proposed

Author: Clan Territory Engineering

Source: Audit-004 Integration Layer

---

# Purpose

Introduce a dedicated Valheim Integration Layer.

This layer will contain all code that directly depends on Valheim, Unity lifecycle, Harmony patches, Valheim runtime objects, and Valheim-specific adapters.

---

# Problem

Clan Territory currently has direct Valheim integration code spread across feature folders.

Examples:

```text
Features/Diagnostics/Hooks
Features/Runtime/Hook
Features/WardDetection/Hooks
Features/WorldDiscovery/Scanners
Features/WorldDiscovery/Services
Features/Territory/Placement
```

This works today, but it weakens architectural boundaries.

Harmony hooks and Valheim runtime adapters are not gameplay logic.

They are Integration.

---

# Motivation

Valheim research confirmed that game behavior must be treated as an external engine boundary.

Clan Territory should not mix:

```text
Valheim lifecycle
```

with:

```text
Clan Territory gameplay logic
```

A dedicated Integration Layer makes the boundary explicit.

---

# Current Architecture

Current simplified flow:

```text
Valheim
   │
   ▼
Feature Hooks
   │
   ▼
Feature Services
   │
   ▼
Runtime / Gameplay / Persistence
```

Problem:

Feature folders contain both gameplay logic and engine integration logic.

---

# Proposed Architecture

New conceptual flow:

```text
Valheim Engine
   │
   ▼
Integration/Valheim
   │
   ▼
Clan Territory Runtime
   │
   ▼
Gameplay Features
   │
   ▼
Persistence
```

Integration adapts Valheim into Clan Territory concepts.

Features operate on Clan Territory models, services and events.

---

# Proposed Folder Structure

Initial structure:

```text
Source/ClanTerritory/Integration/
└── Valheim/
    ├── Harmony/
    ├── Lifecycle/
    ├── Discovery/
    ├── Adapters/
    └── Events/
```

## Harmony

Contains Harmony patch classes.

Examples:

```text
ValheimLifecycleHooks
RuntimeWorldReadyHooks
PieceCreatorHooks
PrivateAreaHooks
PieceDestroyHooks
PiecePlacementHooks
```

## Lifecycle

Contains Valheim lifecycle detection and world-readiness integration.

## Discovery

Contains Valheim-loaded-world scanning.

## Adapters

Contains conversion from Valheim runtime objects to Clan Territory models.

Example:

```text
PrivateArea → WardModel
```

## Events

Contains Integration-level events if required.

---

# Responsibility Rules

## Integration May Depend On

Integration may depend on:

- Valheim classes;
- Unity types;
- Harmony;
- Clan Territory abstractions;
- Clan Territory events;
- Clan Territory models.

## Integration Must Not Own

Integration must not own:

- gameplay rules;
- persistence logic;
- domain rules;
- territory lifecycle decisions.

## Features Should Prefer

Features should prefer:

- Clan Territory models;
- Clan Territory events;
- runtime registries;
- services;
- domain objects.

---

# Boundary Examples

## Good

```text
Valheim PrivateArea
   │
   ▼
Integration Adapter
   │
   ▼
WardModel
   │
   ▼
WardService
```

## Bad

```text
TerritoryService
   │
   ▼
PrivateArea / ZNetView / ZDO
```

---

# Migration Strategy

The migration must be incremental.

No behavior change should be introduced during structural moves.

## Phase 1

Create Integration folder structure.

No code movement.

## Phase 2

Move diagnostics Valheim lifecycle hooks.

## Phase 3

Move runtime world-ready hook.

## Phase 4

Move ward detection Harmony hooks.

## Phase 5

Evaluate whether `PrivateAreaScanner` should move to Integration adapters.

## Phase 6

Evaluate whether loaded world discovery should move to Integration discovery.

## Phase 7

Evaluate placement policy input adapters.

---

# Initial Move Candidates

Move first:

```text
Features/Diagnostics/Hooks/ValheimLifecycleHooks.cs
Features/Runtime/Hook/RuntimeWorldReadyHooks.cs
Features/WardDetection/Hooks/PieceCreatorHooks.cs
Features/WardDetection/Hooks/PrivateAreaHooks.cs
Features/WardDetection/Hooks/PieceDestroyHooks.cs
Features/WardDetection/Hooks/PiecePlacementHooks.cs
```

Watch later:

```text
Features/WorldDiscovery/Scanners/PrivateAreaScanner.cs
Features/WorldDiscovery/Services/WorldDiscoveryService.cs
Features/Territory/Placement/WardPlacementPolicy.cs
```

---

# Risks

## Risk 1 — Broken compile includes

This project uses explicit `.csproj` compile includes.

Moving files requires updating:

```text
Source/ClanTerritory/ClanTerritory.csproj
```

Mitigation:

Move one group at a time.

Compile after each move.

---

## Risk 2 — Harmony patches stop applying

Namespace changes should not affect Harmony directly, but missing compile includes will.

Mitigation:

Verify logs after each move.

---

## Risk 3 — Integration becomes a dumping ground

Mitigation:

Subdivide Integration by responsibility:

```text
Harmony
Lifecycle
Discovery
Adapters
Events
```

---

# Acceptance Criteria

RFC-001 is implemented when:

- Integration folder exists;
- Harmony hooks are no longer spread across feature folders;
- Valheim lifecycle hooks live under Integration;
- runtime world-ready hook lives under Integration;
- ward lifecycle hooks live under Integration;
- `.csproj` includes updated paths;
- project compiles;
- behavior remains unchanged.

---

# Non Goals

This RFC does not change:

- ward deletion behavior;
- persistence behavior;
- runtime restore behavior;
- placement rules;
- registry behavior.

This RFC only defines the Integration Layer boundary.

---

# Decision

Proposed.

Implementation may begin after this RFC is committed.