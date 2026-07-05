# Valheim Integration Layer

This directory contains code that adapts Valheim runtime behavior into Clan Territory systems.

Integration code may depend on:

- Valheim classes
- Unity types
- Harmony
- Clan Territory events
- Clan Territory models
- Clan Territory services

Integration code must not own:

- gameplay rules
- persistence logic
- domain rules
- territory lifecycle decisions

## Subdirectories

| Directory | Purpose |
|----------|---------|
| `Harmony/` | Harmony patches against Valheim methods |
| `Lifecycle/` | Valheim lifecycle and world readiness adapters |
| `Discovery/` | Loaded Valheim world scanning |
| `Adapters/` | Valheim object to Clan Territory model conversion |
| `Events/` | Integration-level events if required |

## Rule

Valheim is an external engine boundary.

Clan Territory features should receive models and events from this layer instead of depending directly on Valheim internals.