# Engineering Principles

## Purpose

This document defines the engineering principles that guide the development of Clan Territory.

Unlike implementation details, these principles are expected to remain stable throughout the lifetime of the project.

Every architectural decision should be consistent with these principles.

---

# P1 — Architecture Before Implementation

## Principle

Design the solution before writing production code.

## Why

Architecture determines long-term maintainability.

Changing architecture early is inexpensive.

Changing architecture after implementation is expensive.

## Consequences

- Design first.
- Implement second.
- Major features begin with architecture.

---

# P2 — Ward Is the Heart of Every Territory

## Principle

A Territory exists because a Ward exists.

## Why

The Ward is the anchor point of the territory.

Without a Ward, the Territory no longer exists.

## Consequences

- Territory lifecycle follows Ward lifecycle.
- Destroying a Ward removes the Territory.
- Persistence never restores Territories without existing Wards.

---

# P3 — The World Is the Source of Truth

## Principle

The Valheim world defines reality.

## Why

The game world always represents the current state.

Runtime data must reflect the world rather than replace it.

## Consequences

- Discovery reads the world.
- Runtime can be rebuilt.
- JSON stores additional information only.

---

# P4 — Registry Is a Cache

## Principle

Registry represents runtime state.

## Why

Registry exists only to make runtime operations efficient.

It is never authoritative.

## Consequences

- Registry may be rebuilt.
- Registry mirrors the world.
- Registry never replaces the world.

---

# P5 — One Sprint = One Finished Feature

## Principle

Every sprint delivers a complete piece of functionality.

## Why

Incomplete systems increase technical debt.

Small completed features are easier to test and maintain.

## Consequences

- Avoid half-implemented systems.
- Finish before expanding.
- Every sprint ends in a working state.

---

# P6 — Small Safe Iterations

## Principle

Prefer many small improvements over large rewrites.

## Why

Small iterations reduce risk.

Problems become easier to identify and correct.

## Consequences

- Keep commits focused.
- Refactor gradually.
- Preserve working behaviour.

---

# P7 — Documentation Is Part of the Project

## Principle

Documentation evolves together with the architecture.

## Why

Architecture that exists only in code eventually becomes unclear.

Documentation preserves engineering intent.

## Consequences

- Update documentation when architecture changes.
- Maintain consistency.
- Avoid outdated documents.

---

# P8 — Build for Years, Not for Today

## Principle

Always choose the solution that remains maintainable over time.

## Why

The project is expected to evolve over many releases.

Short-term shortcuts create long-term costs.

## Consequences

- Prefer maintainability.
- Avoid unnecessary technical debt.
- Think beyond the current sprint.

---

# P9 — One Module — One Question

## Principle

Every subsystem answers one architectural question.

## Why

Clear responsibilities produce maintainable software.

## Consequences

Examples:

World Discovery

> Which Wards exist?

Registry Synchronization

> How do we synchronize runtime state?

Persistence

> What additional information should be stored?

---

# P10 — The Simplest Correct Solution Wins

## Principle

Choose the simplest solution that fully satisfies the architectural requirements.

## Why

Complexity is expensive.

Unnecessary abstraction makes maintenance harder.

## Consequences

- Avoid premature abstraction.
- Prefer clarity.
- Keep solutions explicit.

---

# P11 — Separate Unity from Business Logic

## Principle

Unity-specific code belongs to infrastructure.

Business rules belong to the Domain.

## Why

Separating gameplay framework from business logic improves maintainability and testing.

## Consequences

- Domain remains independent.
- Unity APIs stay outside business rules.
- Infrastructure adapts Unity to the Domain.

---

# P12 — Every Boundary Has a Translator

## Principle

Every architectural boundary should have an explicit translation layer.

## Why

Different layers represent different concepts.

Translation keeps responsibilities isolated.

## Consequences

Examples:

Unity

↓

WardBuilder

↓

WardModel

Domain

↓

Mapper

↓

Persistence

---

# P13 — Every Commit Leaves the Project Better

## Principle

Every commit should improve the project.

## Why

Continuous improvement produces long-term quality.

Large improvements are achieved through many small ones.

## Consequences

A commit may improve:

- functionality;
- architecture;
- documentation;
- maintainability;
- readability;
- testing.

Every commit should leave the project in a better state than before.

---

# Summary

These principles define how engineering decisions are made within Clan Territory.

Architecture, implementation and documentation should always remain aligned with them.

Stable engineering principles allow the project to evolve while preserving consistency, maintainability and long-term quality.

---

# Related Documents

- ARCHITECTURE.md
- DEVELOPMENT_WORKFLOW.md
- SYSTEM_LIFECYCLE.md
- ROADMAP.md
- ADR/