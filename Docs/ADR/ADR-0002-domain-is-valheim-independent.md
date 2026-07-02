# ADR-0002 — Domain is Valheim Independent

## Status

Accepted

## Context

Game objects such as Player, PrivateArea, ZDO and UnityEngine.Vector3 are runtime objects.

They are not safe for persistence, testing or long-term domain logic.

## Decision

Domain code must not depend on Unity, Valheim, BepInEx or Harmony.

Valheim-facing code belongs in Features.

## Consequences

- Domain can be tested without launching Valheim.
- Save format is easier to maintain.
- Runtime objects do not leak into long-term state.
- Feature modules must adapt Valheim data into Domain objects.