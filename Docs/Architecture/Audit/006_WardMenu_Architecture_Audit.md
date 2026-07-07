# WardMenu Architecture Audit

Date:
2026-07-08

---

# Purpose

Review the current WardMenu implementation before adding real gameplay
features.

The goal is to ensure the architecture follows ClanTerritory principles
before complexity increases.

---

# Current architecture

PrivateArea

↓

TerritoryInteraction

↓

WardMenuService

↓

WardMenuModelBuilder

↓

WardMenuController

↓

WardMenuView

↓

Player

---

# Implemented

## Integration

✔ Harmony only redirects interaction.

✔ No gameplay logic inside Harmony.

Compliant.

---

## Runtime

WardMenu does not own runtime state.

Uses RuntimeWard from RuntimeRegistry.

Compliant.

---

## ZDO

WardMenuModelBuilder reads:

- ZNetView
- ZDO
- ZDOVars

No duplicated persistence.

Compliant.

---

## UI

View is responsible only for:

- rendering
- input
- visibility

Controller owns navigation.

Compliant.

---

## Controller

Controller delegates gameplay actions.

Needs improvement:

Currently actions are called directly through interfaces.

Preferred future design:

Controller

↓

EventBus

↓

Action handlers

Status:

Planned.

---

## Actions

Current:

IWardMenuWardActions

IWardMenuTerritoryActions

Good separation.

Needs event-based dispatch.

---

## Services

WardMenuService owns:

- lifecycle
- open
- close
- update

No UI logic.

Compliant.

---

## Builder

WardMenuModelBuilder owns:

- ZDO reading
- RuntimeWard reading
- model construction

No UI code.

Compliant.

---

# Remaining coupling

Current:

Controller

↓

Actions

Preferred:

Controller

↓

EventBus

↓

Handlers

Priority:

Medium

---

# Future menu structure

Overview

Ward

Territory

Members

Guild

Groups

Permissions

Settings

---

# Planned implementation order

1.

EventBus dispatch

2.

Rename Territory

3.

Ward Protection

4.

Radius

5.

Members

6.

Guild integration

7.

Groups integration

8.

Restrictions

---

# Valheim compliance

Current implementation uses:

✔ PrivateArea

✔ ZDO

✔ ZNetView

✔ RuntimeRegistry

✔ Harmony integration only

No duplicated world state detected.

---

# Conclusion

WardMenu foundation is complete.

Future work should focus on gameplay functionality rather than further
structural refactoring.