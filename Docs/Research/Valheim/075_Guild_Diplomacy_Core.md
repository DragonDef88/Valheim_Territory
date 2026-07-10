# Investigation 075: Guild Diplomacy core

## Goal

Add a small, safe first layer of diplomacy between Guilds guilds.

The goal is to create persistent world-state for relations without immediately changing combat, doors, taxes, or territory protection.

## Commands

```text
/ctdip list
/ctdip status <guild>
/ctdip ally <guild>
/ctdip enemy <guild>
/ctdip vassal <guild>
/ctdip neutral <guild>
/ctdip set ally|enemy|vassal|neutral <guild>
```

Alias:

```text
/ctdiplomacy ...
```

## Rules

- Any guild member can view status/list.
- Only a guild leader can change diplomacy.
- Diplomacy changes require server/host authority.
- A guild cannot set diplomacy with itself.
- Setting `neutral` removes the stored relation.

## Relations

The first supported relation types are:

```text
Ally
Enemy
Vassal
Neutral
```

`Neutral` is the default when no saved record exists.

## Persistence

Relations are saved per world:

```text
BepInEx/config/ClanTerritory/worlds/<world>.diplomacy.txt
```

File sections:

```text
[Relation]
SourceGuildId=...
SourceGuildName=...
TargetGuildId=...
TargetGuildName=...
Relation=Ally
UpdatedAtUtc=...
```

## Integration

A new module/service is registered:

```csharp
DiplomacyModule
DiplomacyService
```

The service is intentionally independent from territory enforcement for now.

Public helper methods are available for later integration:

```csharp
TryGetRelation(...)
GetRelationOrNeutral(...)
AreAllies(...)
AreEnemies(...)
IsVassalRelation(...)
```

## Next step

After this core is proven in game, connect relations to specific systems one by one:

1. ward menu display;
2. door access for allies;
3. structure protection behavior for enemies;
4. tax/tribute modifiers;
5. map/overview diplomacy indicators.
