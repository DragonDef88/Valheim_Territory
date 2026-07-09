# Investigation 053: Guild menu title and Guilds map icons

## Request

The ward menu title should no longer always say `Clan Territory`.

The `Clan` part should be replaced by the creator ward guild name from Guilds. If the ward creator has no guild, the title should not show an empty clan placeholder.

Map icons also needed to follow the Guilds/TameGuildWars-style icon lookup instead of using the previous guild player map icon/color-tint fallback.

## Runtime findings

The uploaded log shows:

- `Guilds 1.1.13` loads together with `Clan Territory`.
- `TameGuildWars 1.0.0` is also loaded.
- Clan Territory connects to Guilds at runtime.
- A ward guild was synced as `DMC`.

## TameGuildWars reference

`TameGuildWars.GuildUtils.GetGuildIcon(string guildName)` uses:

- `Guilds.API.GetGuild(guildName)`
- `Guilds.API.GetGuildIcon(guild)`

The decompiled Guilds API exposes `GetGuildIcon(Guild guild)` and resolves the icon from Guilds' own `Interface.GuildIcons` table using `guild.General.icon`, falling back to icon `1`.

## Decision

Clan Territory now follows the same style:

- The ward menu title is `<GuildName> Territory`.
- If no guild is bound, the title is just `Territory`.
- The overview owner remains the real ward creator/player name.
- The map pin label uses the Guilds guild name when the ward has one.
- The map pin icon uses `Guilds.API.GetGuild(guildName)` + `Guilds.API.GetGuildIcon(guild)` through runtime reflection.
- If no guild or icon is available, Clan Territory falls back to the standard marker.
