# Development Workflow

## Philosophy

Clan Territory is developed as a long-term software project, not as a collection of scripts.

Every change should improve the architecture while preserving stability.

Core principles:

- Small, testable increments
- Stable main branch
- Feature branches for all new work
- Architecture first
- Documentation is part of the code
- Ward is the heart of every Territory

---

# Git Workflow

## Main branch

The `main` branch must always be stable.

Every commit in `main` must:

- compile successfully;
- pass in-game testing;
- include documentation if architecture changed.

---

## Feature branches

Every new feature starts from a dedicated branch.

Example:

feature/domain

feature/territory-manager

feature/persistence

feature/world-synchronization

feature/ui

No direct development on `main`.

---

## Merge Strategy

Feature branches are merged using:

git merge --no-ff

This preserves project history and completed sprint boundaries.

---

# Sprint Workflow

Every sprint follows the same lifecycle.

Planning

↓

Implementation

↓

Build

↓

In-game testing

↓

Documentation

↓

Commit

↓

Push

↓

Merge

↓

Review

---

# Definition of Done

A sprint is complete only if:

- project builds successfully;
- feature works in-game;
- logs are clean;
- documentation is updated;
- architecture remains consistent;
- Git history is clean.

---

# Architecture Rules

## Single Responsibility

One module = one responsibility.

Modules communicate through events.

---

## Source of Truth

Valheim World

↓

Ward

↓

Territory

↓

Registry

↓

Persistence

The game world is always the source of truth.

---

## Registry

Registry is a cache.

It can always be rebuilt from the game world.

---

## Persistence

Persistence never creates Territories.

Persistence stores only additional information.

Examples:

- permissions
- diplomacy
- economy
- upgrades
- future extensions

---

## Domain Layer

Domain never depends on:

- Unity
- Jotunn
- Harmony
- BepInEx

Domain contains only business logic.

---

# Event Driven Design

Features never communicate directly.

Communication happens only through EventBus whenever possible.

Example:

WardRegisteredEvent

↓

TerritoryService

↓

PersistenceService

---

# Documentation Rules

Every important architectural decision requires:

- ADR
- Roadmap update (if necessary)
- Architecture update (if necessary)

---

# Code Style

Prefer readability over cleverness.

Avoid premature optimization.

Keep methods small.

Avoid duplicated logic.

Prefer explicit names.

---

# Testing

Every feature is tested in Valheim before merge.

Required checks:

- compile
- game startup
- logs
- feature behaviour
- persistence (if affected)

---

# Commit Messages

Examples:

feat(domain): add territory entity

feat(persistence): implement schema v1

feat(world): add synchronization foundation

fix(registry): prevent duplicate territories

docs: update architecture

merge: persistence schema v1

---

# Long-Term Vision

The goal is to build a modular, maintainable and extensible territory framework for Valheim.

Every sprint should move the project toward that vision while avoiding technical debt.