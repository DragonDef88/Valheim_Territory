# Research 007 — WearNTear Lifecycle

## Purpose

Understand the full lifecycle of Valheim's `WearNTear` component.

This research is important because most player-built structures, including ward-backed objects, use `WearNTear` as their destruction and durability component.

## Inspected Class

- `WearNTear`

Source: full dnSpy export of `WearNTear`.

## Confirmed Facts

### 1. WearNTear is a runtime component

`WearNTear` is a `MonoBehaviour` and implements `IDestructible`.

It is not persistent storage by itself.

It operates on top of `ZNetView` and `ZDO`.

### 2. Awake binds WearNTear to ZNetView and Piece

`Awake()` retrieves:

```text
ZNetView
Piece

If m_nview.GetZDO() is null, initialization stops.

This confirms that WearNTear only becomes active when a valid ZDO-backed object exists.

3. Awake registers network RPCs

WearNTear.Awake() registers:

RPC_Damage
RPC_Remove
RPC_Repair
RPC_HealthChanged
RPC_ClearCachedSupport
RPC_CreateFragments

This confirms that damage, removal, repair, health updates, and support cache clearing are network-routed operations.

4. WearNTear is tracked in a runtime-only global list

Awake() adds the instance to:

WearNTear.s_allInstances

OnDestroy() removes it.

This list is runtime-only and does not represent the full persistent world.

5. OnPlaced marks newly placed objects

OnPlaced() sets:

m_createTime = -1f
m_clearCachedSupport = true

This confirms that placement affects support recalculation and runtime state.

6. Damage is routed through RPC_Damage

Damage(HitData hit) does not directly apply damage.

It invokes:

RPC_Damage

RPC_Damage() only proceeds if the object is valid and locally owned.

7. RPC_Damage applies resistances and then calls ApplyDamage

RPC_Damage():

checks current health;
applies damage resistance;
checks tool tier;
optionally notifies PrivateArea.OnObjectDamaged;
calls ApplyDamage(totalDamage, hit);
invokes m_onDamaged if assigned.

This means m_onDamaged fires after damage is applied, not before.

8. ApplyDamage is the shared damage destruction path

ApplyDamage() subtracts health from the ZDO.

If health reaches zero or below, it calls:

Destroy(hitData, false)

Otherwise, it sends:

RPC_HealthChanged
9. Remove is RPC-based

Remove(bool blockDrop = false) does not directly destroy the object.

It invokes:

RPC_Remove
10. RPC_Remove is owner-gated

RPC_Remove() only proceeds if:

m_nview.IsValid()
m_nview.IsOwner()

Then it calls:

Destroy(null, blockDrop)

This confirms that hammer removal is authority-sensitive.

11. Destroy is the central destruction method

Destroy(HitData hitData = null, bool blockDrop = false) performs the actual object destruction lifecycle.

It:

removes bed spawn point if the object is a bed;
sets ZDO health to 0;
sets ZDO support to 0;
clears support cache;
drops resources through Piece.DropResources() unless blocked;
invokes m_onDestroyed if assigned;
creates destruction noise;
creates destruction effects;
optionally creates fragments;
calls:
ZNetScene.instance.Destroy(gameObject)
12. m_onDestroyed fires before ZNetScene.Destroy

m_onDestroyed is invoked before the Unity object is passed to ZNetScene.Destroy.

This makes m_onDestroyed a strong lifecycle signal for "object is being destroyed", but it still occurs before the final network-scene destroy call.

13. Multiple destruction causes converge into Destroy

Confirmed paths:

Hammer remove
  ↓
Remove()
  ↓
RPC_Remove()
  ↓
Destroy(null, blockDrop)

Damage
  ↓
Damage()
  ↓
RPC_Damage()
  ↓
ApplyDamage()
  ↓
Destroy(hitData, false)

Wear / support / environment damage
  ↓
UpdateWear()
  ↓
ApplyDamage()
  ↓
Destroy(null, false)
Lifecycle Diagram
Awake()
  │
  ├─ bind ZNetView
  ├─ bind Piece
  ├─ register RPCs
  └─ add to s_allInstances

Placed
  │
  ▼
OnPlaced()

Damage path
  │
  ▼
Damage()
  │
  ▼
RPC_Damage()
  │
  ▼
ApplyDamage()
  │
  ├─ health > 0 → RPC_HealthChanged()
  │
  └─ health <= 0 → Destroy()

Hammer remove path
  │
  ▼
Remove()
  │
  ▼
RPC_Remove()
  │
  ▼
Destroy()

Destroy()
  │
  ├─ set health/support to 0
  ├─ DropResources()
  ├─ m_onDestroyed()
  ├─ effects/fragments
  └─ ZNetScene.Destroy()
Architectural Conclusions
1. WearNTear.Destroy is the real lifecycle convergence point

For WearNTear-backed objects, destruction converges in:

WearNTear.Destroy()

This is deeper and more complete than Player.RemovePiece().

2. Player.RemovePiece only covers hammer removal

Player.RemovePiece() is useful for player intent and access checks, but it does not cover all destruction causes.

It does not fully represent object lifecycle.

3. m_onDestroyed is a promising integration point

Because m_onDestroyed fires inside Destroy(), it observes multiple destruction causes:

hammer removal;
combat damage;
environmental wear;
support collapse.

However, before using it in Clan Territory, we must research who else subscribes to m_onDestroyed.

4. Runtime absence still does not mean deletion

WearNTear.s_allInstances is runtime-only.

An object missing from this list may simply be unloaded.

Deletion should be tied to explicit destruction flow, not runtime absence.

5. Current Clan Territory deletion tracking is valid

The current explicit deletion tracking approach is consistent with Valheim behavior.

But future improvement may move detection from player removal hooks toward a lower-level WearNTear destruction signal.

Open Questions
Which Valheim classes subscribe to m_onDestroyed?
Is m_onDestroyed stable enough as a mod integration point?
Should Clan Territory track ward deletion through WearNTear.m_onDestroyed instead of Player.RemovePiece?
How does this behave for non-WearNTear objects?
Does every ward prefab always contain WearNTear?
Next Research
Research 008 — WearNTear Event Subscribers

Goal:

Find all assignments to:

m_onDestroyed
m_onDamaged

and determine whether these callbacks are official lifecycle extension points used by Valheim itself.