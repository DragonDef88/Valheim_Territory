# ADR-0007 — Explicit Dependencies

## Status

Accepted (Future)

## Context

The project currently uses ServiceContainer as a lightweight service registry.

## Decision

Services should gradually move towards explicit constructor dependencies.

The ServiceContainer remains the composition root until a dedicated dependency injection container becomes necessary.

## Consequences

- Dependencies become visible.
- Testing becomes easier.
- Architecture remains simple.
- Migration can happen incrementally.