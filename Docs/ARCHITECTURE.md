# Architecture

## Purpose

This document describes the overall architecture of Clan Territory.

Its purpose is to explain how the system is organized, how data flows through it, and how the major subsystems interact.

Detailed implementation belongs to the dedicated subsystem documents.

---

# Goals

Clan Territory is designed as a long-term, modular framework for territory management in Valheim.

The architecture prioritizes:

- maintainability;
- modularity;
- explicit responsibilities;
- event-driven communication;
- long-term evolution.

The project is built around one fundamental idea:

> The game world is always the source of truth.

---

# High-Level Architecture

```
                 Valheim World
                        │
                        ▼
                World Discovery
                        │
                        ▼
          Registry Synchronization
                        │
                        ▼
             Territory Registry
              ╱       │       ╲
             ▼        ▼        ▼
      Gameplay   Query Engine  API
               ╲      │      ╱
                    ▼
               Persistence
                    ▼
                 JSON Schema
```

The runtime state is always derived from the Valheim world.

Persistence stores additional information only.

---

# Core Principles

The architecture follows a small set of fundamental rules.

## World is the Source of Truth

The existence of a Territory is determined only by the existence of its Ward.

The world is authoritative.

Persistence is not.

---

## Registry is a Cache

TerritoryRegistry represents the current runtime state.

It can always be rebuilt from the game world.

Registry never becomes the ultimate source of truth.

---

## Event Driven Communication

Subsystems communicate through events whenever practical.

This minimizes coupling and allows new systems to be added without modifying existing ones.

---

## Domain Independence

The Domain layer has no dependency on:

- Unity
- Harmony
- BepInEx
- Jotunn
- Persistence

Domain contains business rules only.

---

## One Module — One Question

Every subsystem should answer one architectural question.

Examples:

World Discovery

> Which Wards currently exist?

Registry Synchronization

> How do we make Registry match the world?

Persistence

> What additional data should be stored?

---

# Static Architecture

The project is divided into several major layers.

```
Core
│
├── Bootstrap
├── ModuleManager
├── ServiceContainer
│
Domain
│
├── Entities
├── Value Objects
├── Identifiers
│
Features
│
├── Ward Detection
├── Territory
├── World Discovery
├── Persistence
│
Documentation
```

Each layer has clearly defined responsibilities.

---

# Runtime Architecture

System startup follows the same lifecycle every time.

```
Plugin Loaded
        │
        ▼
Bootstrap.Initialize()
        │
        ▼
Infrastructure Ready
        │
        ▼
Module Initialization
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
Gameplay
```

Bootstrap prepares infrastructure.

Gameplay begins only after runtime initialization.

---

# Main Subsystems

## Core

Responsible for infrastructure.

Contains:

- Bootstrap
- ModuleManager
- ServiceContainer

Core never contains gameplay logic.

---

## World Discovery

Reads the current game world.

Produces:

```
IReadOnlyList<WardModel>
```

World Discovery never modifies Registry.

---

## Registry Synchronization

Synchronizes runtime state with the discovered world.

Responsible for:

- creating missing Territories;
- removing obsolete Territories;
- rebuilding Registry state.

---

## Territory Registry

Stores the current runtime Territory state.

Registry exists only in memory.

---

## Persistence

Stores additional information.

Examples:

- metadata;
- permissions;
- upgrades;
- extensions.

Persistence never creates Territories by itself.

---

# Event Flow

Example:

```
Ward placed
        │
        ▼
WardRegisteredEvent
        │
        ▼
TerritoryService
        │
        ▼
Registry
        │
        ▼
Persistence
```

Removing a Ward follows the same pattern.

---

# Data Flow

The project follows one directional flow.

```
Valheim World
        │
        ▼
World Discovery
        │
        ▼
Registry Synchronization
        │
        ▼
Territory Registry
        │
        ▼
Persistence
```

Data never flows backwards.

---

# Dependency Rules

The following dependencies are intentionally prohibited.

Domain must never depend on:

- Unity
- Harmony
- Persistence
- Configuration

Persistence must never become the source of truth.

Bootstrap must never contain gameplay logic.

---

# Future Evolution

Current development phases.

```
Foundation
    │
    ▼
Living World
    │
    ▼
Protection
    │
    ▼
Permissions
    │
    ▼
Public API
    │
    ▼
User Interface
```

Each phase builds upon the previous one.

---

# Summary

Clan Territory is built around a simple architectural idea.

The Valheim world defines reality.

World Discovery observes that reality.

Registry represents it in memory.

Gameplay operates on it.

Persistence stores additional information.

By keeping responsibilities explicit and communication event-driven, the project remains maintainable, testable and extensible over the long term.

#Related Documents

• SYSTEM_LIFECYCLE.md
• DOMAIN_MODEL.md
• EVENT_ARCHITECTURE.md