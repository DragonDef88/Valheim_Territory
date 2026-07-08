# Investigation 017: Territory door lock and structure damage protection

## Goal

Add two owner-controlled territory rules:

- Door lock: non-owner/non-permitted players cannot open or close doors inside the territory when this rule is enabled.
- Structure damage protection: building pieces inside the territory ignore `WearNTear.ApplyDamage(...)` while this rule is enabled.

## Vanilla evidence

Decompiled `Door` checks vanilla guard-stone access in `GetHoverText`, `Interact`, and `UseItem`, then calls the `UseDoor` RPC to toggle ZDO state. This means a Harmony prefix on `Door.Interact` and `Door.UseItem` can block door usage before the vanilla door state changes.

Decompiled `WearNTear` routes damage through `Damage(...)`, `RPC_Damage(...)`, and finally `ApplyDamage(...)`. Environmental wear also calls `ApplyDamage(...)` directly. Therefore, patching `ApplyDamage(...)` is the narrowest point for blocking structure damage consistently.

## Decision

Store territory rule flags on the ward ZDO:

- `ct_territory_door_lock_enabled`
- `ct_territory_structure_damage_protection_enabled`

Register owner-routed RPCs on every ward:

- `CT_SetTerritoryDoorLock`
- `CT_SetTerritoryStructureDamageProtection`

Expose owner-only buttons in the Territory tab:

- `Lock Doors` / `Unlock Doors`
- `Enable Structure Protection` / `Disable Structure Protection`

Use the existing delayed refresh lifecycle after toggles.

## Limits

Door lock checks permitted access against the ward creator and vanilla permitted-player ZDO list. Structure damage protection blocks damage to objects with `WearNTear` and `Piece` inside the territory radius. Hammer remove/dismantle is not blocked by this first implementation because it does not go through `ApplyDamage(...)`.
