# Development Workflow

## Purpose

This document defines the development workflow used by Clan Territory.

Its purpose is to ensure that every change follows the same engineering process, resulting in a consistent, maintainable and well-documented codebase.

---

# Philosophy

Clan Territory is developed through small, complete and reviewable iterations.

The workflow prioritizes:

- architecture;
- maintainability;
- documentation;
- testing;
- incremental progress.

The objective is long-term quality rather than short-term speed.

---

# Development Lifecycle

Every feature follows the same lifecycle.

```
Idea
    │
    ▼
Architecture
    │
    ▼
Documentation
    │
    ▼
Implementation
    │
    ▼
Game Testing
    │
    ▼
Review
    │
    ▼
Git Commit
    │
    ▼
Merge
```

Skipping stages is discouraged.

---

# Feature Development

Each feature should answer one architectural question.

A feature should:

- have a clear objective;
- remain focused;
- be independently testable;
- finish in a working state.

Features should not be left partially implemented.

---

# Architecture First

Before implementing a significant feature:

- discuss the architecture;
- define responsibilities;
- identify dependencies;
- document major decisions if necessary.

Implementation begins only after the design is understood.

---

# Documentation

Documentation is part of implementation.

Whenever architecture changes:

- update documentation;
- update related ADRs if required;
- verify Related Documents links.

Documentation should describe reality.

---

# Implementation

Implementation should follow the approved architecture.

General recommendations:

- keep methods small;
- keep responsibilities explicit;
- avoid unnecessary abstractions;
- write readable code;
- prefer composition over coupling.

---

# Game Testing

Every completed feature should be tested inside Valheim.

Typical validation:

- startup;
- gameplay behaviour;
- persistence;
- logging;
- edge cases.

A feature is not complete until it has been tested.

---

# Code Review

Before merging:

Review:

- architecture;
- engineering principles;
- documentation;
- readability;
- gameplay behaviour.

Review is an engineering activity rather than a formality.

---

# Git Workflow

Branch strategy:

```
main

↓

feature/<feature-name>

↓

review

↓

merge
```

Every feature is developed in its own branch.

---

# Commit Messages

Commits should describe intent rather than implementation.

Examples:

```
feat(persistence): implement JSON schema v1

feat(territory): remove territory when ward is destroyed

docs: update architecture documentation

refactor(runtime): simplify startup pipeline
```

---

# Pull Requests

Every Pull Request should answer:

- What problem does this solve?
- Why is this solution appropriate?
- Has gameplay been tested?
- Has documentation been updated?
- Does it comply with Engineering Principles?

---

# Refactoring

Refactoring is encouraged.

Rules:

- preserve behaviour;
- keep changes focused;
- improve readability;
- remove technical debt;
- avoid unnecessary rewrites.

Refactoring should leave the project in a better state.

---

# Documentation Freeze

Core documents:

- ENGINEERING_PRINCIPLES.md
- ARCHITECTURE.md
- SYSTEM_LIFECYCLE.md
- ROADMAP.md

These documents should only change when the architecture changes.

Minor wording improvements alone do not justify modifications.

---

# Definition of Done

A feature is considered complete when:

- architecture is consistent;
- implementation is complete;
- gameplay is tested;
- documentation is updated;
- review is finished;
- project builds successfully;
- changes are committed.

Only then may the feature be merged.

---

# Continuous Improvement

Clan Territory follows one simple rule:

> Every commit leaves the project better.

Improvements may include:

- functionality;
- architecture;
- documentation;
- readability;
- maintainability;
- testing.

---

# Summary

Clan Territory is developed through disciplined, incremental engineering.

Architecture guides implementation.

Documentation evolves with architecture.

Every feature is completed, reviewed and tested before being merged.

The workflow prioritizes long-term quality over short-term speed.

---

# Related Documents

- ENGINEERING_PRINCIPLES.md
- ARCHITECTURE.md
- SYSTEM_LIFECYCLE.md
- ROADMAP.md
- README.md