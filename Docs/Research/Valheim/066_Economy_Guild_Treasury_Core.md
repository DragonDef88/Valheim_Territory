# Investigation 066: Economy guild treasury core

## Goal

Add the first safe economy layer without changing existing territory, biome dominion, treasury, terraforming, Guilds, or Groups behavior.

The first layer is a Guilds-backed guild treasury / clan bank.

## Scope

The economy core adds:

- one economy account per Guilds guild;
- world-scoped persistence;
- Valheim `Coins` as the first currency;
- deposit from player inventory into guild treasury;
- leader-only withdrawal from guild treasury;
- console/chat commands.

## Commands

```text
/cteco status
/cteco deposit <coins>
/cteco withdraw <coins>

/cteconomy status
/cteconomy deposit <coins>
/cteconomy withdraw <coins>
```

## Persistence

The economy file is stored per world:

```text
BepInEx/config/ClanTerritory/worlds/<world>.economy.txt
```

The file stores:

- guild id;
- guild name;
- current balance;
- total deposited;
- total withdrawn;
- last update time.

## Permissions

Deposit:

- player must be in a Guilds guild;
- player must have enough `Coins` in inventory.

Withdraw:

- player must be in a Guilds guild;
- player must be guild leader;
- guild treasury must have enough balance.

## Multiplayer note

This first layer is intentionally host/server-only for mutations. It matches the existing safe pattern used by the first Biome Dominion layer.

Future work can add routed RPCs for client-to-server economy requests.

## Future layers

This core can support:

- territory upkeep;
- biome taxes;
- vassal tribute;
- market orders;
- territory service fees;
- war/reparation payments.
