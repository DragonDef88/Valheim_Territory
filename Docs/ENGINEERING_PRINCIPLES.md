# Clan Territory Engineering Principles

> These principles are normative.
>
> They define how Clan Territory is designed, implemented, and evolved.
>
> Any deviation should be intentional, documented, and justified.

---

# Vision

Clan Territory is not simply a territory mod.

Clan Territory is a Persistent Living World Framework for Valheim.

Territories are only the first gameplay system built on top of that framework.

Every architectural decision should support future systems such as:

- NPCs
- Settlements
- Economy
- Roads
- Dynamic Events
- Future gameplay modules

The framework is more important than any individual feature.

---

# Engineering Principles

## 1. GitHub Main is the Source of Truth

GitHub `main` is the only authoritative source.

Never rely on local copies.

Never design from outdated code.

Always analyze the current repository before making architectural decisions.

---

## 2. Never Guess Valheim

Valheim behavior must never be assumed.

If behavior affects gameplay or architecture:

Research first.

dnSpy first.

Implementation second.

---

## 3. Facts Before Design

Architecture must be based on verified facts.

Research precedes implementation.

When knowledge is uncertain:

Research

↓

Document

↓

Implement

Never reverse this order.

---

## 4. Architecture Before Code

Code follows architecture.

Architecture never follows code.

Large implementation without architectural understanding is prohibited.

---

## 5. Small Safe Steps

Development proceeds through small engineering steps.

One commit equals one complete engineering idea.

Large unrelated commits are prohibited.

---

# Architecture Principles

## Runtime

Runtime represents only the currently loaded world.

Runtime is temporary.

Runtime is never the complete world.

---

## Persistence

Persistence owns the complete world.

Persistence is authoritative.

Runtime must never overwrite persistent data blindly.

---

## Gameplay

Gameplay is built on top of Runtime.

Gameplay never owns Runtime.

Gameplay never owns Persistence.

---

## Integration

Harmony patches are Integration.

Harmony is not gameplay.

Harmony is not business logic.

Harmony adapts Valheim to the framework.

---

## Domain

Domain represents business concepts only.

Domain must not depend on:

- Unity
- Harmony
- Valheim internals
- Serialization

---

## Event Driven Design

Whenever practical:

Systems communicate through events instead of direct dependencies.

Dependencies should point downward.

Knowledge should not.

---

# Development Workflow

Every architectural change follows the same lifecycle.

Research

↓

Architecture Audit

↓

RFC

↓

Implementation

↓

Verification

Skipping steps requires explicit justification.

---

# Documentation Rules

Important knowledge must never exist only in conversations.

If knowledge affects future development:

Document it.

If architecture changes:

Document it.

If Valheim behavior is discovered:

Document it.

Documentation is part of the implementation.

---

# Code Quality Principles

Each class should have one primary responsibility.

Each class should have one primary reason to change.

Avoid premature abstraction.

Avoid speculative architecture.

Prefer simple explicit code over clever code.

Working systems are not rewritten without architectural justification.

---

# Framework Principles

Every new system should be designed as a reusable framework component.

Avoid one-off gameplay implementations.

Prefer extension over duplication.

---

# Decision Making

Engineering decisions should be based on:

1. Verified facts
2. Research
3. Architecture
4. Long-term maintainability

Never optimize before understanding.

Never redesign because something "looks cleaner".

Redesign only when architecture requires it.

---

# Project Motto

Understand first.

Design second.

Implement third.

Optimize last.

---

# Long-Term Goal

We are building systems that should not require rewriting six months from now.

Every decision should reduce future complexity instead of increasing it.