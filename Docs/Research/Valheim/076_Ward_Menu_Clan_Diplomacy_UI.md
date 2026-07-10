# Investigation 076: Ward menu Clan diplomacy UI

## Goal

Expose the new Guild Diplomacy core inside the ward menu Clan view.

## User-facing behavior

Open:

```text
Ward Menu -> Overview -> Clan
```

The Clan view now shows:

- clan name;
- clan description when available;
- diplomacy section for the current player's guild;
- saved relations list;
- relation buttons for guild leaders.

Leader buttons:

```text
Ally
Enemy
Vassal
Neutral
```

After clicking a relation button, the player enters the target guild name.

## Rules

The UI uses the same server-side rules as console commands:

- player must be in a Guilds guild;
- only the guild leader can change diplomacy;
- diplomacy changes require server/host authority;
- neutral removes the saved relation.

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

## Notes

This still does not change door or damage behavior.

Diplomacy remains a world-state layer until the next integration step.
