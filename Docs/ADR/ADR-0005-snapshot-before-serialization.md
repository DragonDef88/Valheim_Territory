# ADR-0005 — Snapshot Before Serialization

## Status

Accepted

## Context

Persistence should not serialize live domain objects directly.

The system needs a stable in-memory representation of the world before writing data to disk.

## Decision

Clan Territory creates a complete SaveFile snapshot before serialization.

The snapshot is then passed to storage.

## Consequences

- Domain objects are not serialized directly.
- Save format is isolated from runtime logic.
- Snapshot can later be tested, exported, backed up, or sent over network.
- Storage remains unaware of Territory, Registry, and Domain.