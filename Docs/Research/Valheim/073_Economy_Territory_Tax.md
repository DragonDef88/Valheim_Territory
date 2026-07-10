# Investigation 073: Economy territory tax

## Goal

Add the first territory-facing tax/payment mechanic.

This creates a foundation for:

- taxes from residents;
- tolls and access fees;
- trade fees;
- service payments to a territory owner;
- future market and contract settlement.

## Commands

```text
/cteco tax <coins>
/cteco fee <coins>

/cteconomy tax <coins>
/cteconomy fee <coins>
```

The player must stand inside a territory.

## Rules

The tax payment:

- removes `Coins` from the player's inventory;
- sends the tax to the guild treasury bound to the current territory;
- does not withdraw from the player's guild treasury;
- requires a territory with a Guilds-backed guild owner;
- uses the existing world-scoped economy file.

## Vassal tribute

If the territory is a vassal territory inside a claimed biome, the current tribute split is reused:

```text
40% to the biome owner guild
60% to the territory owner guild
```

The payer still pays the full tax amount from inventory.

## Persistence

Economy accounts now also store:

```text
TaxPaidTotal
TaxReceivedTotal
```

These fields are added to:

```text
BepInEx/config/ClanTerritory/worlds/<world>.economy.txt
```

Existing economy files remain compatible because missing fields default to `0`.

## Ward menu

The Economy tab now has a new button:

```text
Tax
```

It opens a text input for the tax amount.

## Notes

This is intentionally a manual tax/payment step. Automatic recurring taxes should be built later after the rules are proven in gameplay.
