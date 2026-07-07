# Clan Territory

> **Persistent Living World Framework for Valheim**

Clan Territory is an open-source framework for building persistent gameplay systems for Valheim.

The project introduces a Living World Runtime that separates persistent world state from Unity runtime objects, providing a solid foundation for territory management and future gameplay systems.

The Territory System is the first gameplay subsystem built on top of the Living World Runtime.

---

# Mission

> **We do not manage Unity objects.**
>
> **We manage persistent world state.**

Clan Territory is designed around long-term architecture rather than short-term implementation.

The goal of the project is to provide a reliable runtime capable of supporting complex gameplay systems while remaining modular, maintainable and extensible.

---

# Current Features

## Living World Runtime

- ✅ Runtime State Machine
- ✅ Runtime Orchestrator
- ✅ Runtime Discovery
- ✅ Runtime Infrastructure
- ✅ Runtime Registry

## Territory System

- ✅ Territory Registration
- ✅ Territory Lifecycle
- ✅ Ward Detection
- ✅ Territory Persistence
- ✅ Territory Overlap Protection
- ✅ Ward Placement Validation

## Persistence

- ✅ World-based Save Files
- ✅ JSON Serialization
- ✅ Versioned Save Schema

## Architecture

- ✅ Event-Driven Communication
- ✅ Modular Subsystems
- ✅ Dependency Injection
- ✅ Service-Oriented Design

---

# Architecture Overview

```
                     Gameplay Subsystems
                              ▲
                              │
      Territory │ Map │ Clans │ Future Systems
                              ▲
                              │
                 Living World Runtime
                              ▲
                              │
          Runtime Registry ◄──► Persistence
                              ▲
                              │
                        Valheim World
```

The Runtime synchronizes loaded Unity objects with the persistent world state.

Gameplay systems communicate with the Runtime rather than directly with Unity objects.

---

# Current Status

```
Foundation                 ✅ Completed

Persistence                ✅ Completed

Living World Runtime v1    ✅ Completed

Living World Runtime v2    ✅ Completed

Territory Gameplay         🚧 In Progress

Clan Foundation            ⏳ Planned

Public API                 ⏳ Planned

Version 1.0                ⏳ Planned
```

---

# Current Development Focus

**Sprint 4 — Living World Runtime 2.0**

Current objectives:

- Runtime Registry
- Territory Synchronizer
- Runtime Events
- Territory Map Integration

---

# Engineering Principles

The project follows a small set of architectural principles.

- World First
- Persistence is the Source of Truth
- Runtime before Gameplay
- Architecture before Implementation
- Event-Driven Systems
- Small Independent Subsystems
- Documentation evolves together with implementation

---

# Repository Structure

```
Source/
Docs/
Assets/
LICENSE
README.md
```

The source code is organized into independent feature subsystems with clearly defined responsibilities.

---

# Documentation

Project documentation is located in the `/Docs` directory.

Planned documentation includes:

- Vision
- Architecture
- Runtime
- Development Guide
- Roadmap
- RFC
- ADR

---

# Roadmap

Current milestone:

**Living World Runtime 2.0**

Next milestones:

- Territory Synchronizer
- Runtime Events
- Territory Map Runtime
- Clan Foundation
- Public API

---

# Contributing

Contributions are welcome.

Before implementing major functionality:

- understand the project architecture;
- discuss significant architectural changes;
- keep documentation synchronized with implementation;
- prefer small, focused commits.

---

# License

This project is licensed under the **DragonDef88 Valheim Mod License (DVML) v2.0**.

See the **LICENSE** file for complete license information.

---

> **We do not program objects.**
>
> **We build worlds.**