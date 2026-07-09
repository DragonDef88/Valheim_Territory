# Investigation 052: Guilds runtime access and guild ward markers

## Goal

Connect Clan Territory to the installed Guilds mod and use Guild information for territory access and map markers.

## Guilds API reference

The decompiled Guilds API exposes:

- `API.GetPlayerGuild(PlayerReference)`
- `API.GetPlayerGuild(Player)`
- `API.GetOwnGuild()`
- `API.GetGuild(string name)`
- `API.GetGuild(int id)`
- `API.GetGuilds()`
- `API.GetGuildLeader(Guild guild)`

`Guild` contains `Name`, `General`, and `Members`. `GuildGeneral` contains `id`, `icon`, `level`, and `color`.

Guilds also has map sprites for guild players and pings through `Guilds.Map`.

## Decision

Clan Territory still does not take a hard compile-time dependency on Guilds.dll.

`GuildsAdapter` connects to the loaded Guilds assembly at runtime and calls the public API by reflection. This avoids breaking Clan Territory when Guilds is not installed.

## Ward guild binding

A ward stores these optional ZDO values:

- `ct_territory_guild_id`
- `ct_territory_guild_name`
- `ct_territory_guild_color`

When the ward creator opens the territory menu while in a guild, the ward is bound to that guild. If the creator has no guild, the stored guild values are cleared and the ward uses the standard marker.

Guild members can refresh the stored name/color after the ward is already bound to their guild.

## Access

Guild members can use the same owner-level territory controls:

- rename territory;
- territory rules;
- doors;
- terraforming;
- preparation/treasury access paths that use owner validation.

The creator still always has direct access.

## Map marker

If the ward has a guild binding:

- the pin label uses the Guilds guild name instead of the generic Clan Territory ward label;
- if the local player is in that guild and the Guilds map sprite is available, Clan Territory reuses that Guilds sprite;
- otherwise Clan Territory tries to tint the ward icon using the stored guild color;
- if no guild exists, the marker remains the standard/default marker.
