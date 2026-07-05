# Research 008 — WearNTear Destroyed Callbacks

## Purpose

Understand whether `WearNTear.m_onDestroyed` is used by Valheim as a real lifecycle callback.

This research supports future decisions about Clan Territory ward deletion detection.

## Inspected Source

- dnSpy Analyzer result for `WearNTear.m_onDestroyed`
- Full dnSpy export of `WearNTear`

## Confirmed Facts

### 1. WearNTear exposes destruction and damage callbacks

`WearNTear` defines:

```text
public Action m_onDestroyed;
public Action m_onDamaged;

m_onDestroyed is invoked inside WearNTear.Destroy() before ZNetScene.instance.Destroy(gameObject).

2. m_onDestroyed is assigned by multiple Valheim systems

dnSpy Analyzer shows WearNTear.m_onDestroyed is assigned by:

ArcheryTarget.Start()
ArmorStand.Awake()
Container.Awake()
CookingStation.Awake()
DropOnDestroyed.Awake()
Fermenter.Awake()
ItemStand.Awake()
Ship.Awake()
Smelter.Awake()
Turret.Awake()

This confirms that m_onDestroyed is not an unused field.

It is a real internal lifecycle callback used by several Valheim gameplay systems.

3. WearNTear.Destroy is the callback source

WearNTear.Destroy() invokes m_onDestroyed during destruction.

The callback fires before the object is passed to ZNetScene.Destroy().

This means subscribers can react while the object still exists, but after destruction has already been accepted by WearNTear.

4. Valheim uses composition for destruction behavior

Valheim systems do not need to own destruction logic directly.

Instead, systems attach to WearNTear and subscribe to m_onDestroyed.

Examples:

Container + WearNTear
Smelter + WearNTear
Fermenter + WearNTear
ItemStand + WearNTear
Ship + WearNTear
Turret + WearNTear

This is a composition-based lifecycle pattern.

Lifecycle Pattern
Game object
   │
   ├─ WearNTear
   │     │
   │     └─ m_onDestroyed
   │
   ├─ Container
   ├─ Smelter
   ├─ Fermenter
   ├─ ItemStand
   ├─ Ship
   └─ Other systems
Destruction Flow
Damage / Remove / Support failure
   │
   ▼
WearNTear.Destroy()
   │
   ▼
m_onDestroyed()
   │
   ▼
Effects / Fragments
   │
   ▼
ZNetScene.Destroy()
Architectural Conclusions
1. m_onDestroyed is a valid Valheim lifecycle signal

m_onDestroyed is used by multiple internal Valheim systems.

This makes it a strong candidate for mod integration when reacting to final object destruction.

2. m_onDestroyed is deeper than Player.RemovePiece

Player.RemovePiece() only observes hammer removal.

WearNTear.m_onDestroyed observes destruction after multiple causes converge:

Hammer removal
Damage destruction
Support collapse
Environmental wear
3. Clan Territory should consider this as a future deletion hook

Ward deletion detection may be more robust if tied to WearNTear.m_onDestroyed instead of only Player.RemovePiece.

However, code should not be changed until ward prefab composition is confirmed.

Remaining Questions
Does every ward / guard stone prefab have WearNTear?
Does every ward / guard stone prefab have PrivateArea and WearNTear on the same GameObject?
Should Clan Territory subscribe to m_onDestroyed at runtime when registering a ward?
Should deletion detection stay in Harmony hooks or move into a Valheim integration service?
Next Research
Research 009 — PrivateArea and WearNTear Composition

Goal:

Confirm whether ward objects consistently combine:

PrivateArea
Piece
WearNTear
ZNetView

on the same object.

Only after that should we write an RFC for moving territory deletion detection to the WearNTear lifecycle.