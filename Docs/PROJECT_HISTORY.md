# Clan Territory Project History

## v0 — Territory Mod

Clan Territory started as a Valheim territory mod built around the original Ward.

The first goal was simple:

> extend the Ward into a territory control center.

---

## v1 — Domain Foundation

The project introduced explicit domain concepts:

- Territory
- WardId
- TerritoryId
- PlayerId
- OwnerInfo
- TerritoryRadius
- WorldPosition

Domain was separated from Unity, Valheim and Persistence.

---

## v2 — Runtime Registry

Runtime Registry was introduced to represent the currently loaded world.

Key principle:

> Runtime represents loaded world state only.

Runtime is not the complete world.

---

## v3 — Persistence Foundation

Persistence was introduced to store long-term Clan Territory data.

The save format became Ward-centered.

Persistence stores records, not Unity or Valheim objects.

---

## v4 — Merge Save

Persistence was redesigned to avoid data loss from unloaded chunks.

Old problem:

> Saving from Runtime could erase territories that were not currently loaded.

New rule:

> Merge Save preserves unloaded persistent records.

---

## v5 — Delete Tracking

Deletion tracking was added so Persistence can distinguish:
v6 — Valheim Runtime Research

The project adopted a research-first approach for Valheim internals.

Research topics included:

PrivateArea lifecycle
World loading pipeline
ZDO lifecycle
Zone loading
Player.RemovePiece
WearNTear entry points
WearNTear lifecycle
WearNTear destroyed callbacks

Key discovery:

WearNTear.m_onDestroyed is a real Valheim lifecycle callback used by multiple internal systems.

v7 — Architecture Audit

The project introduced Architecture Audits.

Completed audits:

Audit 001 — Core & Domain
Audit 002 — Runtime & Persistence
Audit 003 — Territory, WardDetection & WorldDiscovery
Audit 004 — Integration Layer

Audits established the current architectural roadmap.

v8 — RFC Process

The project introduced RFC documents for architectural changes.

First RFCs:

RFC-001 — Valheim Integration Layer
RFC-002 — Runtime Restore Pipeline

RFCs became the bridge between audits and implementation.

v9 — Documentation Infrastructure

Documentation was reorganized into a structured engineering knowledge base.

Major documentation areas:

Vision
Engineering Principles
Architecture
System Lifecycle
Domain Model
Event Architecture
Persistence Specification
Development Workflow
Roadmap
Research
Architecture Audits
RFCs

Key rule:

Important knowledge must not exist only in conversation.

v10 — Valheim Integration Layer

The project introduced a dedicated Valheim Integration Layer.

Created structure:

Source/ClanTerritory/Integration/Valheim/
    Harmony/
    Lifecycle/
    Discovery/
    Adapters/
    Events/

Moved into Integration:

ValheimLifecycleHooks
RuntimeWorldReadyHooks
PiecePlacementHooks
PieceCreatorHooks
PrivateAreaHooks
PieceDestroyHooks

This changed the project from feature-scattered Harmony patches to a dedicated engine boundary.

Current Direction

Clan Territory is no longer only a territory mod.

It is becoming a Persistent Living World Framework for Valheim.

Current architectural direction:

Valheim Engine
   │
   ▼
Integration Layer
   │
   ▼
Runtime
   │
   ▼
Gameplay Systems
   │
   ▼
Persistence

Territories are the first gameplay system built on top of this framework.

Future systems may include:

NPCs
settlements
economy
roads
dynamic events
permission systems
public API
Current Milestone

The project is currently implementing:

RFC-001 — Valheim Integration Layer

Next planned major architecture task:

RFC-002 — Runtime Restore Pipeline