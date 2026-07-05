# Architecture Audit 001 — Core & Domain

## Purpose

This document audits the Core and Domain layers of Clan Territory.

The goal is not to redesign working systems.

The goal is to verify that the current architecture still aligns with the engineering principles established during the Valheim research phase.

This audit is based on:

- Current GitHub main
- Valheim runtime research
- Living World architecture
- Runtime / Persistence separation

---

# Audit Status

| Layer | Status |
|--------|--------|
| Core | PASS |
| Domain | PASS |

---

# Core

## Responsibility

Core is responsible only for application composition.

It should never contain gameplay logic.

It should never depend on Runtime.

It should never depend on Persistence.

It should never know anything about Valheim gameplay.

Its responsibilities include:

- Bootstrap
- Module initialization
- Dependency registration
- Service composition
- Global configuration

---

## Current Structure

Core currently contains:

- Bootstrap
- Plugin
- ModuleManager
- ServiceContainer
- Globals
- VersionInfo
- ModInfo

The responsibilities are clearly separated.

Core acts as the composition root of the application.

---

## Strengths

### PASS

Core does not implement gameplay.

Core does not implement persistence.

Core does not own runtime state.

Core coordinates modules without containing business logic.

This is consistent with Clean Architecture.

---

## Risks

None currently identified.

---

## Recommendations

No architectural changes required.

Future features should continue to register through modules rather than directly modifying Bootstrap.

---

# Domain

## Responsibility

Domain represents the business model of Clan Territory.

The Domain must remain independent from:

- Unity
- Harmony
- Valheim runtime
- Persistence
- Serialization

---

## Current Structure

Domain currently contains:

- Territory
- TerritoryId
- WardId
- PlayerId
- OwnerInfo
- TerritoryRadius
- WorldPosition

This follows the concept of Entities, Identifiers and Value Objects.

---

## Strengths

### PASS

Identifiers are strongly typed.

Business concepts are explicit.

Domain terminology is clear.

Domain is reusable independently of the game engine.

The Domain layer is not coupled to Harmony patches.

The Domain layer is not coupled to Runtime.

---

## Risks

None currently identified.

---

## Recommendations

Avoid introducing Unity types into Domain.

Avoid introducing serialization attributes into Domain.

Persistence models should remain separate.

---

# Architectural Assessment

Current status:

PASS

The Core and Domain layers are consistent with the architectural goals of Clan Territory.

No refactoring is recommended.

Future development should preserve this separation.

---

# Result

PASS

No RFC required.