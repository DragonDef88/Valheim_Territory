# Object Naming Research

Date:
2026-07-08

## Purpose

Research how Valheim stores and updates player-provided object text/name data in ZDO.

This research supports the future ClanTerritory feature:

- Rename Territory

The goal is to follow Valheim lifecycle instead of inventing a custom persistence path.

---

# Sources

Decompiled Valheim sources:

- `Docs/Research/Valheim/Decompiled/assembly_valheim/Sign.cs`
- `Docs/Research/Valheim/Decompiled/assembly_valheim/TeleportWorld.cs`
- `Docs/Research/Valheim/Decompiled/assembly_valheim/Bed.cs`
- `Docs/Research/Valheim/Decompiled/assembly_valheim/ItemStand.cs`
- `Docs/Research/Valheim/Decompiled/assembly_valheim/MapTable.cs`

---

# Sign

## Storage

Sign text is stored in ZDO:

- `ZDOVars.s_text`
- `ZDOVars.s_author`
- `ZDOVars.s_authorDisplayName`

`UpdateText()` reads the current ZDO data revision and refreshes local UI text when the ZDO changes.

## Change lifecycle

`Interact()` opens `TextInput`.

`SetText(string text)` then:

1. checks `PrivateArea.CheckAccess`;
2. calls `m_nview.ClaimOwnership()`;
3. writes `ZDOVars.s_text`;
4. writes author metadata;
5. calls `UpdateText()` locally.

## Notes

Sign is an example where Valheim directly claims ownership and writes ZDO from the interaction object.

---

# TeleportWorld

## Storage

Portal tag is stored in ZDO:

- `ZDOVars.s_tag`
- `ZDOVars.s_tagauthor`

## Change lifecycle

`Interact()` opens `TextInput`.

`SetText(string text)` does not write ZDO directly.

Instead it calls:

`m_nview.InvokeRPC("RPC_SetTag", text, authorId)`

`RPC_SetTag(...)` then:

1. runs only when `m_nview.IsOwner()`;
2. clears portal connection;
3. writes `ZDOVars.s_tag`;
4. writes `ZDOVars.s_tagauthor`.

## Notes

TeleportWorld is the cleanest reference for user-provided object naming through RPC-owner-ZDO lifecycle.

---

# Bed

## Storage

Bed ownership display uses ZDO:

- `ZDOVars.s_owner`
- `ZDOVars.s_ownerName`

## Change lifecycle

`SetOwner(long uid, string name)` calls:

`m_nview.InvokeRPC("SetOwner", uid, name)`

`RPC_SetOwner(...)` then:

1. runs only when `m_nview.IsOwner()`;
2. writes `ZDOVars.s_owner`;
3. writes `ZDOVars.s_ownerName`.

## Notes

Bed confirms the same pattern:

caller → RPC → owner → ZDO.Set

---

# ItemStand

## Storage

ItemStand stores item attachment state in ZDO:

- `ZDOVars.s_item`
- `ZDOVars.s_type`
- item data saved into object ZDO

This is not naming, but it is a useful ownership/state lifecycle reference.

## Change lifecycle

For non-owner interaction, ItemStand requests ownership:

`m_nview.InvokeRPC("RPC_RequestOwn")`

The owner handles:

`RPC_RequestOwn(long sender)`

and transfers ownership through:

`m_nview.GetZDO().SetOwner(sender)`

After ownership is obtained, the local owner writes ZDO and sends visual refresh RPCs.

## Notes

ItemStand shows that Valheim avoids blind remote ZDO mutation. It ensures the correct owner writes object state.

---

# MapTable

## Storage

Shared map data is stored in ZDO:

- `ZDOVars.s_data`

## Change lifecycle

`OnWrite(...)` builds a `ZPackage` and calls:

`m_nview.InvokeRPC("MapData", mapData)`

`RPC_MapData(...)` then:

1. runs only when `m_nview.IsOwner()`;
2. writes `ZDOVars.s_data`.

## Notes

MapTable is another example of:

caller → RPC → owner → ZDO.Set

---

# Pattern summary

Valheim uses two related patterns.

## Pattern A: Claim ownership then write ZDO

Observed in:

- Sign

Lifecycle:

1. access check;
2. `ClaimOwnership()`;
3. write ZDO;
4. refresh local view.

This is simple and works for direct text editing on an object.

## Pattern B: RPC to owner then owner writes ZDO

Observed in:

- TeleportWorld
- Bed
- MapTable

Lifecycle:

1. access check;
2. caller invokes RPC on `ZNetView`;
3. RPC handler validates `m_nview.IsOwner()`;
4. owner writes ZDO;
5. world sync follows through ZDO replication.

This is the preferred lifecycle for shared object state that should be authoritative on the object owner.

---

# Decision for ClanTerritory

Territory name should be stored in the ward ZDO because the ward is the world object anchoring the territory.

Suggested key:

- `ct_territory_name`

ClanTerritory should prefer the Valheim RPC-owner-ZDO lifecycle:

1. UI requests rename.
2. WardMenuController dispatches action.
3. Ward action invokes RPC on the ward `ZNetView`.
4. RPC handler runs only on owner.
5. owner writes `ct_territory_name` to ward ZDO.
6. WardMenuModelBuilder reads name from ZDO.
7. UI refreshes from rebuilt model.

Do not store territory name in JSON.

Do not store territory name only in RuntimeRegistry.

JSON may contain it only as snapshot/export data if needed later.

---

# Implementation direction

Future code commit:

`Implement territory naming through ward ZDO`

Required parts:

1. Add territory ZDO key constant.
2. Register RPC for territory rename on `PrivateArea` or integration component.
3. Implement territory action:
   - validate name;
   - invoke RPC;
   - do not write ZDO directly unless owner.
4. Update `WardMenuModelBuilder` to read `ct_territory_name`.
5. Update UI after rename.

---

# Open questions

Need separate research before implementation:

- Where should ClanTerritory register custom ward RPCs?
- Should rename RPC live in `PrivateAreaHooks`, `TerritoryInteraction`, or a dedicated `TerritoryNaming` feature?
- Should territory naming be part of `WardMenuActions` or separate `TerritoryActions` service?