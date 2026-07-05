# Architecture Audit 003 — Territory, WardDetection & WorldDiscovery

## Purpose

This document audits the Territory, WardDetection and WorldDiscovery areas of Clan Territory.

The goal is to verify whether the current gameplay-facing architecture still matches the project's direction after the Valheim lifecycle research.

This audit focuses on the real flow:

```text
Valheim object
   │
   ▼
Ward detection / discovery
   │
   ▼
Territory creation
   │
   ▼
Runtime registry
   │
   ▼
Persistence save
```

---

# Audit Status

| Area | Status |
|------|--------|
| Territory Registry | PASS |
| Territory Service | WATCH |
| WardDetection Hooks | WATCH |
| Ward Registry / Ward Service | WATCH |
| WorldDiscovery | WATCH |
| Harmony Hook Placement | ACTION |

---

# Territory Registry

## Responsibility

`TerritoryRegistry` stores territory domain entities and provides lookup operations.

It supports:

- register
- unregister
- find by ward
- find containing position
- find nearest territory
- overlap checks
- clear

---

## Assessment

Status:

PASS

`TerritoryRegistry` is focused on territory lookup and storage.

It does not perform persistence writes.

It does not patch Valheim.

It does not directly publish events.

This is a healthy separation.

---

## Notes

The registry currently provides overloads accepting Unity `Vector3`.

This is acceptable in the feature layer, but should not migrate into Domain.

Domain remains protected because conversion happens through `WorldPosition`.

---

## Recommendation

No immediate changes required.

Keep `TerritoryRegistry` focused on territory storage and queries.

---

# Territory Service

## Responsibility

`TerritoryService` receives ward lifecycle events and creates or removes territories.

It currently handles:

- `WardRegisteredEvent`
- `WardDestroyedEvent`
- territory creation
- overlap rejection
- registry updates
- persistence save trigger
- deletion tracking trigger

---

## Assessment

Status:

WATCH

The service works and remains understandable.

However, it currently has multiple reasons to change:

1. Territory creation rules change.
2. Ward destruction flow changes.
3. Persistence save strategy changes.
4. Delete tracking strategy changes.
5. Runtime restore pipeline changes.

This does not require immediate refactoring, but it is a growth risk.

The most important concern is that `TerritoryService` directly reaches into `IPersistenceService` through `ServiceContainer` to mark deletes and save.

This makes gameplay service responsible for persistence timing.

---

## Recommendation

No immediate code change required.

Future RFC candidate:

```text
Territory Change Events
```

Instead of calling Persistence directly, `TerritoryService` could publish domain/gameplay changes such as:

```text
TerritoryCreated
TerritoryRemoved
```

Persistence could subscribe and decide when to save.

This would reduce direct coupling.

---

# WardDetection Hooks

## Responsibility

WardDetection hooks currently detect Valheim ward placement and destruction.

Current hook examples:

- `Player.PlacePiece`
- `Player.RemovePiece`
- `PrivateArea.Awake`

---

## Assessment

Status:

WATCH

The current implementation is functional.

However, Valheim research showed that `Player.RemovePiece` only captures hammer removal.

It does not represent the full object destruction lifecycle.

Confirmed Valheim lifecycle research suggests that `WearNTear.m_onDestroyed` is a stronger lifecycle signal for WearNTear-backed objects.

This means current deletion detection may be incomplete for non-hammer destruction causes.

---

## Recommendation

Do not change code immediately.

Future RFC candidate:

```text
WearNTear-based Ward Destruction Detection
```

This RFC should only proceed after confirming ward prefab composition:

```text
PrivateArea
Piece
WearNTear
ZNetView
```

---

# Ward Registry / Ward Service

## Responsibility

`WardService` registers detected wards, updates `WardRegistry`, registers runtime wards, and publishes ward events.

---

## Assessment

Status:

WATCH

`WardService` currently bridges several concepts:

- detection model
- ward registry
- runtime registry
- event publication

This is acceptable now.

However, as the Integration Layer matures, WardService may need to become more narrowly focused.

It currently performs both ward registration and runtime registration.

---

## Recommendation

No immediate refactor required.

Future improvement may split:

```text
WardDetection
WardRuntimeRegistration
WardLifecycleEvents
```

Only do this when new lifecycle hooks are introduced.

---

# WorldDiscovery

## Responsibility

`WorldDiscoveryService` scans currently loaded Valheim runtime objects and converts `PrivateArea` instances into `WardModel`.

---

## Assessment

Status:

WATCH

The current implementation is correct for loaded runtime discovery.

It uses `Object.FindObjectsByType<PrivateArea>()`, which means it discovers only currently instantiated Unity objects.

This aligns with the Valheim research:

```text
Unity objects = loaded runtime
ZDOs = persistent game state
```

However, the name "WorldDiscovery" can be misleading.

It does not discover the whole world.

It discovers the currently loaded Unity scene.

---

## Recommendation

No immediate code change required.

Future naming / architecture RFC candidate:

```text
Loaded World Discovery
```

Possible future names:

```text
LoadedWardDiscovery
RuntimeWardDiscovery
ValheimLoadedWardScanner
```

Do not change names until Integration Layer architecture is defined.

---

# Harmony Hook Placement

## Responsibility

Harmony hooks are the boundary between Clan Territory and Valheim.

They are not gameplay logic.

They are not domain logic.

They are integration infrastructure.

---

## Current Structure

Harmony hooks currently live under feature folders, for example:

```text
Features/WardDetection/Hooks
Features/Diagnostics/Hooks
Features/Runtime/Hook
```

---

## Assessment

Status:

ACTION

This is the clearest architectural issue found in this audit.

The hooks are working, but their location blurs the boundary between:

```text
Valheim Integration
```

and:

```text
Clan Territory Feature Logic
```

Valheim research has made this more important.

Harmony hooks should be treated as an Integration Layer because they depend directly on Valheim internals.

---

## Recommendation

Create an RFC for:

```text
Valheim Integration Layer
```

The goal should be to define where Harmony hooks, lifecycle adapters, scanners, and Valheim-specific translation logic belong.

Possible future structure:

```text
Source/ClanTerritory/Integration/Valheim/
    Harmony/
    Lifecycle/
    Discovery/
    Models/
```

Implementation should not happen before RFC approval.

---

# Flow Assessment

## Placement Flow

Current flow:

```text
Player.PlacePiece
   │
   ▼
PiecePlacementHooks
   │
   ▼
IWardPlacementPolicy
   │
   ▼
Valheim placement allowed / blocked
```

Assessment:

WATCH

Placement validation is currently close to Valheim input flow.

This works, but it belongs architecturally to Integration, not pure WardDetection.

---

## Registration Flow

Current flow:

```text
PrivateArea.Awake
   │
   ▼
PrivateAreaHooks
   │
   ▼
PrivateAreaScanner
   │
   ▼
WardService.RegisterWard
   │
   ▼
WardRegisteredEvent
   │
   ▼
TerritoryService
```

Assessment:

WATCH

This is conceptually correct because Valheim creates loaded runtime objects through Awake.

However, `PrivateAreaHooks` belongs to Integration.

The scanner may also eventually move to Integration or be wrapped by it.

---

## Destruction Flow

Current flow:

```text
Player.RemovePiece
   │
   ▼
PieceDestroyHooks
   │
   ▼
WardDestroyedEvent
   │
   ▼
TerritoryService
   │
   ▼
Persistence delete tracking + save
```

Assessment:

WATCH

This works for hammer removal.

It is not yet aligned with Valheim's full destruction lifecycle.

Research suggests future flow may become:

```text
PrivateArea / WearNTear attached
   │
   ▼
WearNTear.m_onDestroyed
   │
   ▼
WardDestroyedEvent
   │
   ▼
TerritoryService
```

---

# What Should Not Be Changed Yet

Do not refactor immediately:

- Territory Registry
- Territory Factory
- Placement rules
- Ward models
- Existing EventBus flow
- Current deletion tracking

The system is working.

The correct next step is RFC design, not code movement.

---

# RFC Candidates

## RFC-011 — Valheim Integration Layer

Status:

Required

Reason:

Harmony hooks are currently spread across feature folders.

Research now shows integration should be a first-class boundary.

---

## RFC-012 — WearNTear-based Ward Destruction Detection

Status:

Future / dependent

Reason:

Current destruction detection is based on `Player.RemovePiece`.

WearNTear lifecycle may be more complete.

Depends on confirming ward prefab composition.

---

## RFC-013 — Territory Change Events

Status:

Optional / future

Reason:

`TerritoryService` currently calls Persistence directly.

---

## RFC-014 — Loaded World Discovery Naming

Status:

Optional / future

Reason:

WorldDiscovery currently discovers loaded Unity objects, not the complete world.

---

# Final Assessment

The Territory and WardDetection architecture is functional and mostly healthy.

The strongest issue is not behavior.

The strongest issue is boundary clarity.

Harmony hooks are integration infrastructure and should eventually move behind a dedicated Valheim Integration Layer.

Result:

```text
Territory Registry: PASS
Territory Service: WATCH
WardDetection Hooks: WATCH
Ward Service: WATCH
WorldDiscovery: WATCH
Harmony Hook Placement: ACTION
```

Next recommended architecture task:

```text
RFC-011 — Valheim Integration Layer
```