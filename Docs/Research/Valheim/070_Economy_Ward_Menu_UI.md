# Investigation 070: Economy ward menu UI

## Goal

Expose the working economy layer in the ward menu so it can be tested without typing commands.

## UI

A new ward menu tab was added:

```text
Economy
```

The tab shows:

- player guild treasury name;
- current balance;
- territory guild;
- total deposited;
- total withdrawn;
- total upkeep paid;
- total tribute received;
- transfer totals.

## Actions

The UI exposes the existing economy mechanics:

```text
Deposit
Withdraw
Upkeep
Transfer
```

Deposit / Withdraw / Upkeep open a Valheim text input and ask for a coin amount.

Transfer opens a text input in this format:

```text
<guild name> <coins>
```

Examples:

```text
DMC 25
Dragon Lords 25
```

The last token is parsed as amount, so guild names with spaces are supported.

## Compile fix note

Economy UI API must be inserted into `namespace ClanTerritory.Features.Economy`, inside `EconomyService`.

It must not be inserted into `BiomeDominionService`, because BiomeDominion already has its own `BuildMenuState(...)`.

## Permissions

The UI uses the same EconomyService logic as the console commands.

That means:

- deposit requires a Guilds guild and enough Coins in inventory;
- withdraw requires guild leader;
- upkeep requires territory guild leader;
- transfer requires guild leader and an existing target economy account.

## Files changed

- `Source/ClanTerritory/Features/Territory/TerritoryModule.cs`
- `Source/ClanTerritory/Features/WardMenu/Models/WardMenuModel.cs`
- `Source/ClanTerritory/Features/WardMenu/Builders/WardMenuModelBuilder.cs`
- `Source/ClanTerritory/Features/WardMenu/Actions/IWardMenuTerritoryActions.cs`
- `Source/ClanTerritory/Features/WardMenu/Actions/WardMenuTerritoryActions.cs`
- `Source/ClanTerritory/Features/WardMenu/Controllers/WardMenuController.cs`
- `Source/ClanTerritory/Features/WardMenu/UI/IWardMenuView.cs`
- `Source/ClanTerritory/Features/WardMenu/UI/JotunnWardMenuView.cs`
- `Source/ClanTerritory/Config/ConfigManager.cs`
- `README.md`
