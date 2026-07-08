# Investigation 018: Radius synchronization and locked door auto-close

## Problems

Testing after `146ddcd` showed two issues:

1. Ward radius and runtime territory radius can diverge.
2. Door lock blocks guests correctly, but locked doors do not close automatically after they are opened by allowed players.

## Evidence

The runtime territory cache was created through `TerritoryFactory.CreateFromWard(...)`, but the factory used `ConfigValues.TerritoryRadius` instead of the ward's actual stored/runtime radius. That means runtime territory rules that depend on `TerritoryRegistry` can lag behind the ward radius.

Valheim `Door` changes door state through the private `RPC_UseDoor(...)`, which writes `ZDOVars.s_state` to `1`, `-1`, or `0`. A postfix on `RPC_UseDoor(...)` is therefore the correct point to schedule auto-close after a door has actually opened.

## Decision

Radius synchronization:

- Add `Radius` to `WardModel`.
- Read ward radius from `ct_territory_radius` in ZDO discovery and PrivateArea discovery.
- Build `Territory` from `WardModel.Radius`, not global config radius.
- Subscribe `TerritoryService` to `TerritoryRadiusChangedEvent`.
- Update cached territory radius when ward radius changes.

Door auto-close:

- Add config `Territory.DoorAutoCloseSeconds`, clamped by BepInEx config to 3-10 seconds.
- Schedule auto-close after `Door.RPC_UseDoor(...)` if the door is open and door lock is enabled for its territory.
- When door lock is enabled, scan currently open doors inside that territory and schedule them too.
- Close doors by setting `ZDOVars.s_state` to `0` on the door owner and invoking private `Door.UpdateState()`.

## Limits

Auto-close applies only when the door lock rule is enabled. Owner/permitted players can still open locked territory doors, but those doors close automatically after the configured delay.
