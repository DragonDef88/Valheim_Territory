# Territory RPC Architecture Research

Date:
2026-07-08

## Purpose

Decide where ClanTerritory should register custom ward/territory RPCs.

This supports the future feature:

- Rename Territory

## Existing project architecture

Current ward interaction entry point:

`Source/ClanTerritory/Integration/Valheim/Harmony/PrivateAreaHooks.cs`

Current responsibilities:

- `AwakePostfix`
  - scans `PrivateArea`;
  - creates `WardModel`;
  - registers ward through `IWardService`.

- `InteractPrefix`
  - replaces vanilla ward interaction;
  - forwards interaction to `ITerritoryInteractionService`;
  - does not contain gameplay logic.

This matches the project rule:

Harmony receives event → forwards to service → exits.

## Existing Valheim pattern

Research from `010_Object_Naming_Research.md` shows the preferred lifecycle:

UI / interaction

↓

`ZNetView.InvokeRPC(...)`

↓

object owner handles RPC

↓

owner writes ZDO

Observed in:

- `TeleportWorld`
- `Bed`
- `MapTable`

## Design decision

Do not put territory rename logic directly in `PrivateAreaHooks`.

`PrivateAreaHooks` must remain integration-only.

Instead, create a dedicated feature:

`Features/TerritoryNaming`

Suggested structure:

```text
Source/ClanTerritory/Features/TerritoryNaming/
├── Services/
│   ├── ITerritoryNamingService.cs
│   └── TerritoryNamingService.cs
├── TerritoryNamingModule.cs
└── TerritoryNamingZdoKeys.cs

RPC registration location

Register custom RPC from PrivateAreaHooks.AwakePostfix, but delegate to service.

Flow:

PrivateArea.Awake
  ↓
PrivateAreaHooks.AwakePostfix
  ↓
ITerritoryNamingService.RegisterRpc(privateArea)
  ↓
ZNetView.Register<string>("CT_SetTerritoryName", handler)

This keeps Harmony thin.

Rename flow
WardMenuView
  ↓
WardMenuController
  ↓
IWardMenuTerritoryActions.RenameTerritory
  ↓
ITerritoryNamingService.RequestRename(privateArea, player, name)
  ↓
ZNetView.InvokeRPC("CT_SetTerritoryName", playerId, name)
  ↓
owner RPC handler
  ↓
ZDO.Set("ct_territory_name", name)
ZDO key

Use ClanTerritory prefix:

ct_territory_name

Do not use STU_Ward keys.

Do not use JSON.

Why not direct ZDO.Set from UI action?

Because Valheim object state commonly uses owner-authoritative updates:

caller invokes RPC;
owner validates;
owner writes ZDO.

This avoids non-owner writes and follows Valheim lifecycle.

Conclusion

Territory naming should be implemented as its own feature.

Harmony should only register/delegate.

The first code commit should introduce:

TerritoryNamingModule
ITerritoryNamingService
TerritoryNamingService
TerritoryNamingZdoKeys
RPC registration from PrivateAreaHooks.AwakePostfix
model builder read from ct_territory_name