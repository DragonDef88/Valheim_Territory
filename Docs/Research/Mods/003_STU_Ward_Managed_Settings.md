# STU_Ward Managed Ward Settings Research

## Source

Reference mod:

- STU_Ward v1.2.1 by sighsorry
- Decompiled source location:
  `Docs/Research/Mods/Decompiled/STUWard/v1.2.1/STUWard`

This document is research only. ClanTerritory must not copy STU_Ward implementation directly.

## Purpose

Identify which STU_Ward behavior should influence ClanTerritory ward and territory management UI.

ClanTerritory rules still apply:

1. Valheim first.
2. ZDO is the source of truth.
3. Harmony is only integration.
4. RuntimeRegistry is runtime state.
5. UI delegates actions to services/controllers.

## Managed settings discovered

STU_Ward stores extended ward settings in ZDO using custom keys:

- `stuw_show_area_marker`
- `stuw_area_marker_speed_multiplier`
- `stuw_area_marker_alpha`
- `stuw_radius`
- `stuw_auto_close_doors`
- `stuw_auto_close_delay`
- `stuw_warning_sound_enabled`
- `stuw_warning_flash_enabled`
- `stuw_restriction_options`

Source:

- `WardSettings.cs`
- keys are declared near the RPC/settings constants.
- configuration is read through `GetConfiguration(PrivateArea area)`.

## Restrictions discovered

STU_Ward defines restriction toggles for:

- Doors
- Portals
- Pickup
- Placed consumables
- Item stands
- Armor stands
- Containers
- Crafting stations
- Tameables and saddles

These are represented as restriction definitions and combined into a bitmask.

## Runtime application

STU_Ward does not only store settings.

It applies settings back to the live `PrivateArea` object:

- updates `PrivateArea.m_radius`;
- updates `CircleProjector.m_radius`;
- updates area marker segment count;
- updates marker speed;
- updates marker visibility;
- invalidates spatial/runtime caches when radius changes.

This is important for ClanTerritory because changing territory size must update both:

- ZDO-backed configuration;
- live runtime systems.

## UI behavior

STU_Ward UI has two main pages:

- General
- Restrictions

Observed general settings include:

- owner display;
- guild display;
- radius slider;
- area marker speed;
- area marker brightness;
- auto-close door delay;
- warning sound toggle;
- warning flash toggle;
- permitted players list.

Observed restrictions page contains toggles for the restriction definitions above.

## ClanTerritory decision

ClanTerritory should not use STU_Ward classes, RPC names, or ZDO keys directly.

ClanTerritory should implement its own keys and own model.

Suggested ClanTerritory key prefix:

- `ct_`

Suggested future keys:

- `ct_territory_name`
- `ct_territory_radius`
- `ct_show_area_marker`
- `ct_area_marker_alpha`
- `ct_area_marker_speed`
- `ct_auto_close_delay`
- `ct_warning_sound_enabled`
- `ct_warning_flash_enabled`
- `ct_restriction_options`
- `ct_guild_access_enabled`
- `ct_group_access_enabled`

## First implementation target

The first real action should be small and Valheim-aligned:

### Toggle ward protection

Use Valheim's existing `PrivateArea` behavior where possible.

Vanilla `PrivateArea.Interact` toggles protection through the `ToggleEnabled` RPC for the creator. ClanTerritory should prefer invoking the same Valheim mechanism instead of directly editing `ZDOVars.s_enabled`, unless research proves a direct ZDO update is required.

## Later implementation targets

After Toggle ward protection:

1. Read custom territory name from ZDO.
2. Save custom territory name to ZDO.
3. Read and display custom radius.
4. Apply runtime radius to `PrivateArea.m_radius` and `CircleProjector.m_radius`.
5. Add restriction bitmask model.
6. Add Members page based on vanilla permitted players.
7. Integrate Guilds.
8. Integrate Groups.

## Do not implement yet

Do not implement until separately researched:

- door auto-close;
- portal restrictions;
- pickup restrictions;
- crafting station restrictions;
- tame/saddle restrictions;
- Guilds integration;
- Groups integration.