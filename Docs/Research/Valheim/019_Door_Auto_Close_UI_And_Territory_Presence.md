# Investigation 019: Door auto-close UI and territory presence messages

## Problems

Testing after `521ac39` showed:

1. Door auto-close works from config, but the owner needs in-game controls for the delay.
2. Radius sync is working in logs, but the player needs clearer visual/UX feedback when the territory radius changes or when crossing a territory boundary.

The log confirms that runtime territory radius sync is firing after ward radius changes through `Territory radius synced from ward`.

## Vanilla evidence

`PrivateArea.ShowAreaMarker()` activates the guard-stone area marker briefly and hides it after 0.5 seconds. The marker is normally triggered from hover text, not as a persistent territory overlay.

`CircleProjector` uses `m_radius` every frame while positioning its segments, so updating `m_areaMarker.m_radius` is enough for marker geometry while the marker is active.

## Decision

Door auto-close:

- Store auto-close seconds per ward in ZDO using `ct_territory_door_auto_close_seconds`.
- Keep `Territory.DoorAutoCloseSeconds` config as the default for old/new wards.
- Add owner-only `-1` / `+1` buttons in the Territory tab.
- Clamp values to 3-10 seconds.
- Reschedule currently open territory doors when the value changes while lock is enabled.

Territory presence:

- Add a lightweight runner that checks the local player's current `PrivateArea`.
- Show center-screen messages when entering and leaving a territory.
- Resolve territory name dynamically through `ITerritoryNamingService`.

Radius visual feedback:

- After owner radius changes, call `ShowAreaMarker()` and `PokeConnectionEffects()` so the player gets immediate ward/territory visual feedback.
- This does not create a persistent always-on territory circle; it uses vanilla guard-stone marker behavior.
