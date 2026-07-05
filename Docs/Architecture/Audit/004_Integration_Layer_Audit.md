# Architecture Audit 004 — Integration Layer

## Purpose

This document audits all known places where Clan Territory directly depends on Valheim, Unity or Harmony.

The goal is to prepare RFC-001:

```text
Valheim Integration Layer
```

This audit does not propose implementation changes.

It identifies integration boundaries.

---

# Audit Status

| Area | Status |
|------|--------|
| Harmony Hook Placement | ACTION |
| Valheim Lifecycle Hooks | ACTION |
| Runtime Readiness Hook | ACTION |
| Ward Detection Hooks | ACTION |
| World Discovery Scanner | WATCH |
| Placement Policy | WATCH |
| Feature / Integration Boundary | ACTION |

---

# Core Finding

Clan Territory currently contains several direct Valheim integration points spread across feature folders.

Examples:

```text
Features/Diagnostics/Hooks
Features/Runtime/Hook
Features/WardDetection/Hooks
Features/WorldDiscovery/Scanners
Features/WorldDiscovery/Services
Features/Territory/Placement
```

These systems work, but their current locations blur the boundary between:

```text
Valheim Integration
```

and:

```text
Clan Territory Feature Logic
```

This is now an architectural issue because Valheim research confirmed that game behavior must be treated as an external engine boundary.

---

# Current Integration Inventory

## 1. Diagnostics Hooks

Path:

```text
Source/ClanTerritory/Features/Diagnostics/Hooks/ValheimLifecycleHooks.cs
```

Uses:

```text
HarmonyPatch
Game
ZNet
ZNetScene
ZoneSystem
Player
AccessTools
MethodBase
```

Purpose:

Logs Valheim lifecycle checkpoints for diagnostics.

Assessment:

ACTION

Reason:

This is pure Valheim lifecycle integration.

It should eventually live under a dedicated Integration layer, even if it still calls Diagnostics services.

---

## 2. Runtime World Ready Hook

Path:

```text
Source/ClanTerritory/Features/Runtime/Hook/RuntimeWorldReadyHooks.cs
```

Uses:

```text
HarmonyPatch
Game.UpdateRespawn
Player.m_localPlayer
PrivateArea
UnityEngine.Object.FindObjectsByType
```

Purpose:

Detects when the Valheim world appears ready enough to enter Clan Territory runtime world-loaded state.

Assessment:

ACTION

Reason:

This class is currently inside Runtime, but it directly patches Valheim and scans Unity objects.

It is not Runtime core.

It is an Integration adapter that signals Runtime.

---

## 3. Piece Creator Hook

Path:

```text
Source/ClanTerritory/Features/WardDetection/Hooks/PieceCreatorHooks.cs
```

Uses:

```text
HarmonyPatch
Piece.SetCreator
PrivateArea
GetComponent
PrivateAreaScanner
IWardService
```

Purpose:

Detects a ward after its creator has been set and registers it as a Clan Territory ward.

Assessment:

ACTION

Reason:

This class is an adapter from Valheim `Piece` lifecycle into Clan Territory `WardModel`.

It should eventually live behind Integration, not inside feature logic.

---

## 4. PrivateArea Hook

Path:

```text
Source/ClanTerritory/Features/WardDetection/Hooks/PrivateAreaHooks.cs
```

Uses:

```text
HarmonyPatch
PrivateArea.Awake
PrivateAreaScanner
IWardService
```

Purpose:

Detects loaded `PrivateArea` instances and registers them as wards.

Assessment:

ACTION

Reason:

`PrivateArea.Awake` is a Valheim lifecycle event.

This is Integration logic.

---

## 5. Piece Destroy Hook

Path:

```text
Source/ClanTerritory/Features/WardDetection/Hooks/PieceDestroyHooks.cs
```

Uses:

```text
HarmonyPatch
Player.RemovePiece
Piece
PrivateArea
ZNetView
ZDO
EventBus
WardDestroyedEvent
```

Purpose:

Detects hammer removal of ward pieces and publishes a destruction event.

Assessment:

ACTION

Reason:

This is direct Valheim player-action integration.

Valheim research also showed that `Player.RemovePiece` is not the complete destruction lifecycle.

Future work should consider `WearNTear.m_onDestroyed`.

---

## 6. Piece Placement Hook

Path:

```text
Source/ClanTerritory/Features/WardDetection/Hooks/PiecePlacementHooks.cs
```

Uses:

```text
HarmonyPatch
Player.PlacePiece
Piece
Player
Vector3
Quaternion
IWardPlacementPolicy
```

Purpose:

Blocks invalid ward placement before Valheim places the piece.

Assessment:

ACTION

Reason:

This is direct Valheim placement integration.

The policy itself may remain feature logic, but the hook belongs to Integration.

---

## 7. PrivateArea Scanner

Path:

```text
Source/ClanTerritory/Features/WorldDiscovery/Scanners/PrivateAreaScanner.cs
```

Uses:

```text
PrivateArea
ZNetView
Piece
ZDOVars
UnityEngine
```

Purpose:

Adapts a Valheim `PrivateArea` object into a Clan Territory `WardModel`.

Assessment:

WATCH

Reason:

This class is not a Harmony patch, but it is still Valheim-specific translation logic.

It may belong in Integration as an adapter.

However, it is currently small, focused and reusable.

Move only when the Integration Layer structure is defined.

---

## 8. World Discovery Service

Path:

```text
Source/ClanTerritory/Features/WorldDiscovery/Services/WorldDiscoveryService.cs
```

Uses:

```text
UnityEngine.Object.FindObjectsByType
PrivateArea
FindObjectsSortMode
```

Purpose:

Scans currently loaded Unity objects for `PrivateArea` instances.

Assessment:

WATCH

Reason:

This discovers loaded runtime objects, not the full world.

It is Valheim/Unity-facing discovery.

The service may eventually become `LoadedWardDiscovery` or move behind Integration.

Do not rename before RFC.

---

## 9. Ward Placement Policy

Path:

```text
Source/ClanTerritory/Features/Territory/Placement/WardPlacementPolicy.cs
```

Uses:

```text
Player
Vector3
```

Purpose:

Runs territory placement validation rules.

Assessment:

WATCH

Reason:

The policy is gameplay-facing, but currently accepts Valheim and Unity types.

This is acceptable short-term.

Future improvement may introduce an adapter input model so gameplay policies do not depend directly on `Player`.

---

# Integration Categories

## Harmony Integration

Includes:

```text
ValheimLifecycleHooks
RuntimeWorldReadyHooks
PieceCreatorHooks
PrivateAreaHooks
PieceDestroyHooks
PiecePlacementHooks
```

These classes patch Valheim methods and should move to:

```text
Integration/Valheim/Harmony
```

or an equivalent structure after RFC approval.

---

## Valheim Object Adapters

Includes:

```text
PrivateAreaScanner
```

Purpose:

Convert Valheim runtime objects into Clan Territory models.

Potential future location:

```text
Integration/Valheim/Adapters
```

---

## Loaded World Discovery

Includes:

```text
WorldDiscoveryService
```

Purpose:

Scan currently loaded Unity objects.

Potential future location:

```text
Integration/Valheim/Discovery
```

or remain under `WorldDiscovery` with a clearer boundary.

---

## Gameplay Policies with Valheim Inputs

Includes:

```text
WardPlacementPolicy
IPlacementRule
```

Purpose:

Apply gameplay rules using Valheim-provided context.

Potential future improvement:

```text
Valheim Player / Vector3
   │
   ▼
Placement Context Adapter
   │
   ▼
Gameplay Policy
```

---

# Proposed Future Boundary

```text
Valheim Engine
   │
   ▼
Integration/Valheim
   │
   ├── Harmony
   ├── Lifecycle
   ├── Discovery
   ├── Adapters
   └── Events
   │
   ▼
Clan Territory Features
   │
   ├── Runtime
   ├── WardDetection
   ├── Territory
   └── Persistence
```

---

# What Should Stay in Features

The following should remain feature logic:

```text
TerritoryService
TerritoryRegistry
WardRegistry
WardService
RuntimeRegistry
PersistenceService
Placement rules
```

These classes may receive events or models from Integration, but should not patch Valheim directly.

---

# What Should Move Later

After RFC-001 is accepted, the following are candidates for movement:

```text
Features/Diagnostics/Hooks/ValheimLifecycleHooks.cs
Features/Runtime/Hook/RuntimeWorldReadyHooks.cs
Features/WardDetection/Hooks/PieceCreatorHooks.cs
Features/WardDetection/Hooks/PrivateAreaHooks.cs
Features/WardDetection/Hooks/PieceDestroyHooks.cs
Features/WardDetection/Hooks/PiecePlacementHooks.cs
```

Possible later candidates:

```text
Features/WorldDiscovery/Scanners/PrivateAreaScanner.cs
Features/WorldDiscovery/Services/WorldDiscoveryService.cs
```

Do not move gameplay services in the same refactor.

---

# Risks

## Risk 1

Moving hooks may break Harmony registration if namespaces or compile includes are missed.

Mitigation:

Small commits.

One hook group per commit.

Compile after each step.

---

## Risk 2

Integration may become a dumping ground.

Mitigation:

Separate subfolders by responsibility:

```text
Harmony
Lifecycle
Discovery
Adapters
Events
```

---

## Risk 3

Moving scanner too early may blur WorldDiscovery responsibilities.

Mitigation:

Start with Harmony hook relocation only.

Move scanners only after RFC defines the boundary.

---

# Recommended RFC

Create:

```text
Docs/Architecture/RFC/RFC-001_Valheim_Integration_Layer.md
```

Status:

```text
Proposed
```

Purpose:

Define where Valheim-specific integration code belongs.

---

# Recommended Implementation Order

After RFC approval:

## Phase 1

Create Integration folder structure.

No behavior changes.

## Phase 2

Move diagnostics and runtime world-ready hooks.

## Phase 3

Move ward lifecycle hooks.

## Phase 4

Decide whether scanners belong to Integration or remain in WorldDiscovery.

## Phase 5

Consider placement policy context adapters.

---

# Final Assessment

Current integration works.

The architectural issue is placement, not behavior.

The project now needs a dedicated Valheim Integration Layer so that future lifecycle work does not spread Harmony and Valheim dependencies across feature folders.

Result:

```text
Integration Layer: ACTION
```

Next recommended architecture task:

```text
RFC-001 — Valheim Integration Layer
```