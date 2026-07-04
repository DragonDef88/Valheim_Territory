# Clan Territory

> A modular, event-driven territory management framework for Valheim.

Clan Territory is an open-source project focused on building a robust, maintainable and extensible territory system for Valheim.

The project is designed around long-term architecture rather than short-term implementation.

---

# Vision

Clan Territory aims to become a complete territory management platform.

Core goals:

- World-driven runtime state
- Modular architecture
- Event-driven communication
- Stable public API
- Long-term maintainability

The Valheim world is always the source of truth.

---

# Current Status

```
Foundation          ✅ Completed
Living World        🚧 In Progress
Protection          ⏳ Planned
Permissions         ⏳ Planned
Public API          ⏳ Planned
User Interface      ⏳ Planned
Release 1.0         ⏳ Planned
```

---

# Architecture Overview

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

The runtime state is rebuilt from the game world.

Persistence stores additional information only.

---

# Project Principles

Clan Territory is developed according to a stable set of engineering principles.

Highlights include:

- Architecture Before Implementation
- The World Is the Source of Truth
- Registry Is a Cache
- One Module — One Question
- Documentation Is Part of the Project
- Every Commit Leaves the Project Better

See:

```
Docs/ENGINEERING_PRINCIPLES.md
```

---

# Documentation

Project documentation is organized by responsibility.

| Document | Purpose |
|----------|---------|
| ARCHITECTURE.md | Overall system architecture |
| ROADMAP.md | Long-term project evolution |
| SYSTEM_LIFECYCLE.md | Runtime lifecycle |
| DOMAIN_MODEL.md | Business model |
| EVENT_ARCHITECTURE.md | Event system |
| PERSISTENCE_SPECIFICATION.md | Persistence design |
| FILE_FORMAT.md | Save file format |
| DEVELOPMENT_WORKFLOW.md | Development process |
| ENGINEERING_PRINCIPLES.md | Engineering philosophy |

---

# Repository Structure

```
Docs/
Source/
Assets/
```

Source code is organized into independent feature modules.

Each subsystem has a single responsibility.

---

# Development Workflow

Every feature follows the same process.

```
Idea

↓

Architecture

↓

Documentation

↓

Implementation

↓

Testing

↓

Review

↓

Merge
```

---

# Current Development

Current phase:

```
Living World
```

Current focus:

- World Discovery
- Registry Synchronization
- Runtime Initialization

---

# Long-Term Goals

Planned capabilities:

- Territory protection
- Permission system
- Public API
- User interface
- Extension ecosystem

---

# Contributing

Contributions should follow the project architecture.

Before implementing a major feature:

- understand the architecture;
- read the engineering principles;
- discuss significant architectural changes;
- keep documentation synchronized with implementation.

---

# Summary

Clan Territory is not only a Valheim mod.

It is a long-term engineering project focused on creating a reliable, extensible and maintainable territory management framework.

The project values architecture, documentation and incremental engineering equally with implementation quality.