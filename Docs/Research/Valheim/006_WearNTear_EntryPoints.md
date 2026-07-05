# Research 006 — WearNTear Entry Points

## Purpose

Identify all confirmed entry points into `WearNTear` from the currently inspected Valheim dnSpy exports.

This is a preparatory research note.

The full `WearNTear` class has not yet been inspected in this pass.

## Inspected Classes

- `Player`
- `PrivateArea`
- `Piece`
- `ZNetView`
- `ZNetScene`
- `ZDOMan`

## Confirmed Facts

### 1. Player.RemovePiece calls WearNTear.Remove(false)

During hammer removal, `Player.RemovePiece()` checks for a `WearNTear` component on the selected `Piece`.

If the component exists, Valheim calls:

```text
WearNTear.Remove(false)

This confirms that normal hammer removal of WearNTear-backed pieces enters the WearNTear removal path.

2. PrivateArea subscribes to WearNTear damage

PrivateArea.Awake() retrieves WearNTear from the same object and subscribes to:

WearNTear.m_onDamaged

This confirms that guard stones react to damage through WearNTear.

3. Player.PlacePiece calls WearNTear.OnPlaced()

When a player places a piece, Player.PlacePiece() checks for a WearNTear component and calls:

WearNTear.OnPlaced()

This confirms that WearNTear participates in the placement lifecycle.

4. ZNetView.Destroy delegates to ZNetScene.Destroy

ZNetView.Destroy() calls:

ZNetScene.instance.Destroy(gameObject)

This is the confirmed networked destruction path once execution reaches ZNetView.Destroy().

5. ZNetScene.Destroy may destroy the ZDO

ZNetScene.Destroy(GameObject go):

retrieves the object's ZNetView;
gets the attached ZDO;
resets the ZDO reference on the ZNetView;
removes the instance from the runtime instance map;
calls ZDOMan.DestroyZDO(zdo) if the ZDO is locally owned;
destroys the Unity GameObject.

This means destruction is ownership-sensitive.

Confirmed Entry Points
Placement:
Player.PlacePiece()
    ↓
WearNTear.OnPlaced()

Hammer removal:
Player.RemovePiece()
    ↓
WearNTear.Remove(false)

PrivateArea damage reaction:
PrivateArea.Awake()
    ↓
WearNTear.m_onDamaged += PrivateArea.OnDamaged
Not Yet Confirmed

The following must not be treated as fact until WearNTear itself is inspected in dnSpy:

whether WearNTear.Remove(false) always calls ZNetView.Destroy();
whether WearNTear.Remove(false) claims ownership;
whether WearNTear.Remove(false) drops resources directly or delegates to Piece.DropResources();
whether destruction by damage and hammer removal share the same final path;
whether m_onDestroyed, m_onDamaged, or similar callbacks exist and when they fire;
whether WearNTear.Remove(false) is safe as a Harmony patch point.
Architectural Conclusion

At this point, WearNTear is confirmed as a central lifecycle component, but not yet understood deeply enough to base architecture on it.

Clan Territory should not move deletion hooks from Player.RemovePiece() to WearNTear.Remove() until the full WearNTear class is inspected.

Next Required dnSpy Export

Export the full decompiled class:

WearNTear

Required methods / members:

Awake
OnPlaced
Damage
RPC_Damage
Remove
Destroy
m_onDamaged
m_onDestroyed
m_nview
m_piece
Current Status
Research status: Partial
Architecture impact: Do not change hooks yet
Next step: Inspect full WearNTear class