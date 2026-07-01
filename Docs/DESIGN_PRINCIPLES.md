# Clan Territory — Design Principles

## Core Idea

The original Valheim Ward is the heart of a territory.

Clan Territory extends the Ward into a full territory control center.

## Principles

1. Quality over speed.
2. Every feature must compile and be tested in Valheim.
3. Main branch must always be stable.
4. No temporary architecture.
5. Domain logic must not depend on Unity or Valheim.
6. Features communicate through events, not direct dependencies.
7. Ward is the central object of the territory.
8. UI comes after gameplay systems.
9. No mandatory dependency on Plateautem or STU_Ward.
10. Integrate with Groups, Guilds, EpicMMO later as optional providers.

## Development Rule

If a system can be tested without Valheim, it belongs outside Valheim-specific code.