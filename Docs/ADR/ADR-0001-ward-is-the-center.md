# ADR-0001 — Ward is the Center of the Territory System

## Status

Accepted

## Context

Valheim already has a Ward object that players understand as a base protection object.

Clan Territory extends this concept instead of replacing it with a separate object.

## Decision

The original Ward is the center of the Clan Territory system.

Territory, ownership, permissions, UI, terrain control, portal control and future extensions are attached conceptually to the Ward.

## Consequences

- Players interact with one familiar object.
- Persistence is Ward-centered.
- Features can extend territory behavior without creating multiple competing objects.
- Ward UI becomes the main control panel.