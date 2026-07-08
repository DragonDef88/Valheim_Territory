# Investigation 016: Ward self-permission and owner action guards

## Problem

Testing showed that a non-owner can open the custom ward menu on a disabled ward and rename the territory, but cannot add themselves to the ward permitted list.

## Root cause

The custom ward interaction menu intercepts the player interaction that vanilla `PrivateArea.Interact(...)` normally uses to call `TogglePermitted`. Vanilla permits non-owner self add/remove only when the ward is disabled. The custom menu therefore needs an explicit self-permission action.

The same test also exposed missing owner-only guards for territory rename and radius changes. Protection toggle already had a creator guard, but rename and radius actions must follow the same rule.

## Vanilla evidence

Decompiled `PrivateArea` registers two RPCs:

- `ToggleEnabled`
- `TogglePermitted`

`TogglePermitted` toggles the interacting player's own permitted status and only runs when the ward is disabled. Vanilla stores permitted players as:

- `ZDOVars.s_permitted`
- `pu_id{index}`
- `pu_name{index}`

## Decision

- Add current-player creator/permitted flags to the ward menu model.
- Show owner controls only to the ward creator.
- Show `Add Me` / `Remove Me` to non-owners only when the ward is disabled.
- Route self-permission through vanilla `TogglePermitted` RPC.
- Guard rename and radius changes with creator checks.
- Keep owner remove-permitted rows for future host/local-owner management.
