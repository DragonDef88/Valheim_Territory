# Investigation 069: Economy guild treasury transfers

## Goal

Add the first diplomacy/trade money flow between Guilds-backed economy accounts.

This is a safe foundation for:

- trade between guilds;
- contract payments;
- diplomatic gifts;
- war reparations;
- market settlement;
- service payments.

## Command

```text
/cteco transfer <guild name> <coins>
/cteco pay <guild name> <coins>

/cteconomy transfer <guild name> <coins>
/cteconomy pay <guild name> <coins>
```

The last argument is always parsed as the coin amount, so guild names with spaces are supported.

Example:

```text
/cteco transfer DMC 25
/cteco transfer Dragon Lords 25
```

## Rules

A transfer is allowed only when:

- the session is server/host;
- the player is in a Guilds guild;
- the player is the leader of the sender guild;
- the sender guild treasury has enough balance;
- the target guild already has an economy account;
- the sender and receiver guild are different.

## Persistence

Economy accounts now also store:

```text
TransferSentTotal
TransferReceivedTotal
```

The file remains world-scoped:

```text
BepInEx/config/ClanTerritory/worlds/<world>.economy.txt
```

## Notes

The target guild must already have an account in the economy file. This is intentional for the first pass: it avoids guessing Guilds internals and keeps the system based on known, already-seen guild accounts.

A guild account is created when a member of that guild uses economy commands such as:

```text
/cteco status
/cteco deposit 1
```
