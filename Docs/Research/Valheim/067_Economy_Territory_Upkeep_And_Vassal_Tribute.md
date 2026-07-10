# Investigation 067: Economy territory upkeep and vassal tribute

## Goal

Add the next safe economy layer on top of the guild treasury core:

- territory upkeep;
- vassal tribute when the territory is inside a claimed biome owned by another guild.

## Command

```text
/cteco upkeep
/cteco upkeep <coins>

/cteconomy upkeep
/cteconomy upkeep <coins>
```

If no amount is provided, the default upkeep is 10 coins.

## Rules

The command checks the territory where the local player currently stands.

The payment is allowed only when:

- the session is server/host;
- the player is inside a Clan Territory ward territory;
- the territory is linked to a Guilds guild;
- the player is a member of that territory guild;
- the player is the guild leader;
- the guild treasury has enough balance.

## Payment flow

For a normal territory:

```text
territory guild treasury -= upkeep
```

For a vassal territory:

```text
territory guild treasury -= upkeep
biome owner guild treasury += 40% of upkeep
```

The remaining part is treated as the territory upkeep sink.

## Persistence

The economy save now stores additional counters:

```text
UpkeepPaidTotal
TributeReceivedTotal
```

The file remains world-scoped:

```text
BepInEx/config/ClanTerritory/worlds/<world>.economy.txt
```

## Notes

This is intentionally a manual command for the first pass. It validates the money flow without adding automatic timers, penalties, or territory shutdown behavior.

Future layers can add:

- automatic daily upkeep;
- unpaid territory warnings;
- protection downgrade when upkeep is unpaid;
- biome tax rates;
- market fees;
- war reparations.
