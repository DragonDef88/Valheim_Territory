# Investigation 074: Ward menu clan description overview

## Goal

Show clan/guild information directly in the ward menu Overview.

## User-facing behavior

When a ward is bound to a Guilds guild:

- the Overview tab shows the clan name;
- the Overview tab shows a `Clan` button;
- pressing `Clan` switches the central Overview text to the clan description;
- pressing the button again switches back to the normal Overview text.

If Guilds does not expose a description, the UI shows a safe fallback text instead of failing.

## Integration

The Guilds integration remains optional.

The new method is added to the optional interface:

```csharp
bool TryGetGuildDescription(string guildName, out string description);
```

`GuildsAdapter` resolves the guild through the already-used runtime reflection path:

```csharp
Guilds.API.GetGuild(string)
```

Then it tries to read description-like members from the guild object:

```text
Description
description
Desc
desc
About
about
Info
info
Motto
motto
Bio
bio
```

It also checks the nested `General` object when present.

## Files changed

- `Source/ClanTerritory/Integration/Guilds/IGuildService.cs`
- `Source/ClanTerritory/Integration/Guilds/GuildsAdapter.cs`
- `Source/ClanTerritory/Features/WardMenu/Models/WardMenuModel.cs`
- `Source/ClanTerritory/Features/WardMenu/Builders/WardMenuModelBuilder.cs`
- `Source/ClanTerritory/Features/WardMenu/UI/JotunnWardMenuView.cs`
- `Source/ClanTerritory/Config/ConfigManager.cs`
- `README.md`

## Notes

No compile-time dependency on `Guilds.dll` is introduced.
