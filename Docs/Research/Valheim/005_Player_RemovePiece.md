# Research 005 — Player.RemovePiece

## Purpose

Understand how Valheim removes player-built pieces through the hammer remove action.

This research is important for Clan Territory because ward destruction is one of the primary signals used to remove territories.

## Inspected Classes

- `Player`
- `Piece`
- `PrivateArea`
- `ZNetView`
- `WearNTear`
- `ZNetScene`
- `ZDOMan`

## Confirmed Facts

### 1. RemovePiece is driven by Player input

`Player.RemovePiece()` is the hammer removal path.

It starts from the player camera raycast and searches for a `Piece` component on the hovered object.

If no direct piece is found and the raycast hit is a `Heightmap`, Valheim tries to find a nearby terrain modifier piece.

### 2. Removal has several guard checks

Before removing a piece, Valheim checks:

- the piece exists;
- `piece.m_canBeRemoved`;
- the piece is not inside a no-build location;
- `PrivateArea.CheckAccess(...)`;
- `CheckCanRemovePiece(piece)`;
- the piece has a `ZNetView`;
- `piece.CanBeRemoved()`.

If any check fails, removal stops.

### 3. PrivateArea access is checked before removal

`Player.RemovePiece()` calls:

```text
PrivateArea.CheckAccess(piece.transform.position, 0f, true, false)

This means ward/private-area access rules are part of normal piece removal.

Clan Territory should not bypass this behavior unless there is a deliberate architecture decision.

4. IRemoved is called before physical removal

If the target piece has an IRemoved component, Valheim calls:

IRemoved.OnRemoved()

before the actual removal path continues.

This may be useful for future investigation, but Clan Territory should not assume all pieces implement this interface.

5. WearNTear is the main removal path

If the piece has a WearNTear component, Valheim calls:

WearNTear.Remove(false)

This means normal building-piece removal usually flows through WearNTear.

The full WearNTear.Remove() behavior should be researched separately.

6. Non-WearNTear pieces use a fallback removal path

If there is no WearNTear component:

if the piece has a Character, Valheim applies massive damage;
otherwise Valheim:
logs removal;
claims ZNetView ownership;
drops resources;
creates placement/remove effects;
destroys the object through the networked object path.
7. Networked destruction goes through ZNetScene

ZNetView.Destroy() delegates to:

ZNetScene.instance.Destroy(gameObject)

ZNetScene.Destroy(GameObject go):

gets the object's ZNetView;
gets the attached ZDO;
resets the ZNetView ZDO reference;
removes the ZDO from the runtime instance map;
calls ZDOMan.DestroyZDO(zdo) if the ZDO is locally owned;
destroys the Unity GameObject.
8. ZDO destruction is ownership-sensitive

ZDOMan.DestroyZDO(ZDO zdo) returns if the ZDO is not owned locally.

This means deletion is authority-sensitive.

Runtime destruction and persistent network deletion are not the same thing unless ownership allows ZDO destruction.

Removal Pipeline
Player input
   │
   ▼
Player.RemovePiece()
   │
   ▼
Raycast target
   │
   ▼
Piece
   │
   ▼
Validation checks
   │
   ├─ m_canBeRemoved
   │
   ├─ no-build location
   │
   ├─ PrivateArea.CheckAccess
   │
   ├─ CheckCanRemovePiece
   │
   ├─ ZNetView exists
   │
   └─ Piece.CanBeRemoved
   │
   ▼
IRemoved.OnRemoved()
   │
   ▼
WearNTear.Remove(false)
   │
   ▼
ZNetScene / ZDO destruction path
Architectural Conclusions
1. Ward deletion should be detected after Valheim confirms removal

Clan Territory should not treat player intent as deletion.

The correct signal is after Valheim confirms that removal succeeded.

This supports our current strategy of using a postfix around removal result rather than only observing input intent.

2. Loaded runtime absence is still not deletion

A piece can disappear from runtime because a zone unloads.

A piece is deleted only when the Valheim removal/destruction path removes its ZDO.

This confirms the architecture rule:

Unloaded != Deleted
3. Deletion tracking in Clan Territory is justified

Because merge-save preserves unloaded records, deletion must be explicit.

Player.RemovePiece() and the destruction path confirm that deletion is a distinct event, not just absence from runtime.

4. WearNTear requires separate research

The most important unknown after this pass is:

WearNTear.Remove(false)

We need to inspect it before finalizing any deeper destruction hooks.

Open Questions
What exactly does WearNTear.Remove(false) do?
Does WearNTear.Remove(false) always call ZNetView.Destroy()?
Does WearNTear claim ownership before destruction?
Is WearNTear.Remove(false) safe to patch directly?
Which hook is safest for Clan Territory: Player.RemovePiece, WearNTear.Remove, or ZNetScene.Destroy?