# ADR-0003 — Features Communicate Through Events

## Status

Accepted

## Context

Ward Detection originally called Territory Service directly.

That created an unnecessary dependency between features.

## Decision

Feature modules communicate through EventBus when possible.

Ward Detection publishes WardRegisteredEvent.
Territory reacts to WardRegisteredEvent.

## Consequences

- Ward Detection does not know Territory exists.
- New systems can react to the same events.
- UI, Save, Permissions and Integrations can subscribe without changing existing features.
- EventBus remains synchronous until there is a real need for async dispatching.