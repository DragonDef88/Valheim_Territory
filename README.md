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

<<<<<<< HEAD
=======
# License

# DragonDef88 Valheim Mod License (DVML) v2.0

**Version:** 2.0
**Effective Date:** July 4, 2026
**Author:** DragonDef88
**Location:** Klaipėda, Lithuania

Copyright © 2026 DragonDef88. All Rights Reserved.

---

## 1. Definitions

For the purposes of this License:

* **Author** means DragonDef88.
* **Project** means all source code, compiled binaries, assets, configuration files, documentation, releases, scripts and other materials contained within this repository.
* **Derivative Work** means any modification, adaptation, translation, extension or work based on the Project.
* **Commercial Use** means any activity intended to generate direct or indirect financial benefit.
* **Gross Revenue** means total revenue received before taxes, fees, commissions or expenses.

---

## 2. Ownership

The Project and all intellectual property rights remain the exclusive property of DragonDef88.

No ownership rights are transferred under this License.

All rights not expressly granted are reserved.

---

## 3. Free License

Everyone may:

* Download the Project.
* Use the Project.
* Study the source code.
* Modify the Project.
* Fork the GitHub repository.
* Submit Pull Requests.
* Compile personal builds.
* Use the Project on private or public Valheim servers.

These permissions apply only to **Non-Commercial Use**.

---

## 4. Attribution

Redistributions must:

* retain this License;
* retain copyright notices;
* credit DragonDef88 as the original author;
* indicate modifications;
* include a link to the original GitHub repository.

Claiming authorship of the original Project is prohibited.

---

## 5. Non-Commercial Use

Allowed free of charge:

* Personal use
* Educational use
* Research
* Community servers
* Free modpacks
* Open-source collaboration
* Testing
* Development

---

## 6. Commercial Use

Commercial Use is prohibited unless the Author has granted a written Commercial License.

Examples include:

* Selling the Project.
* Selling modified versions.
* Paid modpacks.
* Premium server packages.
* Paid launcher distributions.
* Paid subscriptions.
* Commercial hosting.
* Commercial redistribution.
* Paid plugins based on this Project.
* Any revenue-generating use.

---

## 7. Commercial License

Unless otherwise agreed in writing:

Commercial users shall pay **10% of Gross Revenue** generated through products or services using this Project.

Alternative commercial agreements may be negotiated individually.

---

## 8. Valheim Servers

This Project may be used on public or private servers provided that:

* access to the mod itself is free;
* the mod is not sold;
* the server is not represented as officially affiliated with DragonDef88.

VIP packages, premium memberships or other monetized access specifically tied to this Project require a Commercial License.

---

## 9. Modpacks

Free community modpacks are permitted if:

* proper credit is given;
* this License is included;
* the original GitHub repository is linked.

Paid modpacks require written permission.

---

## 10. Warranty

THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND.

---

## 11. Limitation of Liability

THE AUTHOR SHALL NOT BE LIABLE FOR ANY DAMAGES ARISING FROM USE OF THIS PROJECT.

---

## 12. Termination

Violation of this License immediately terminates all granted rights.

The Author reserves the right to pursue all available legal remedies.

---

## 13. Contact

Commercial licensing requests:

DragonDef88

GitHub Repository (official)

Effective Date: July 4, 2026


---

# Summary

Clan Territory is not only a Valheim mod.

It is a long-term engineering project focused on creating a reliable, extensible and maintainable territory management framework.

The project values architecture, documentation and incremental engineering equally with implementation quality.
