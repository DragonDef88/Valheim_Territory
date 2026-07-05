# Research 001 — PrivateArea Lifecycle

## Purpose

Understand how Valheim guard stones are represented at runtime.

Clan Territory treats wards as territory anchors, so we must understand the real Valheim lifecycle before designing persistence or runtime restore systems.

## Inspected Classes

- `PrivateArea`
- `Piece`
- `ZNetView`
- `ZNetScene`
- `Player`

## Confirmed Facts

### 1. PrivateArea is a Unity runtime component

`PrivateArea` is a `MonoBehaviour`.

It is not the persistent source of truth by itself.

It is the runtime representation of a ward-like object.

### 2. PrivateArea depends on ZNetView

In `PrivateArea.Awake()` Valheim retrieves the `ZNetView` component.

If the `ZNetView` is not valid, `PrivateArea.Awake()` returns early.

This means `PrivateArea` only becomes active when a valid networked object exists.

### 3. PrivateArea registers itself globally

`PrivateArea.Awake()` adds the instance to `PrivateArea.m_allAreas`.

`PrivateArea.OnDestroy()` removes the instance from `PrivateArea.m_allAreas`.

This confirms that `m_allAreas` is a runtime-only collection of currently loaded `PrivateArea` objects.

It is not a complete persistent world index.

### 4. PrivateArea state is stored in ZDO

`PrivateArea` reads and writes state through `ZNetView.GetZDO()`.

Examples:

- enabled state
- creator name
- permitted players

This confirms that the durable game-side state is attached to the ZDO, not to the MonoBehaviour instance.

### 5. Piece also depends on ZNetView

`Piece.Awake()` retrieves `ZNetView`.

If the `ZNetView` is valid, `Piece` reads creator data from the ZDO.

This confirms that building pieces use ZDO data as their persistent backing state.

## Lifecycle Summary

```text
ZDO
 │
 ▼
ZNetView
 │
 ▼
Piece
 │
 ▼
PrivateArea

Architectural Conclusion

For Clan Territory:

PrivateArea.m_allAreas = loaded runtime state only
ZDO = Valheim persistent/networked object state
Clan Territory Persistence = mod persistent world state

Therefore, Clan Territory must not treat loaded PrivateArea objects as the complete world.

This supports the project rule:

Runtime is not the full world.
Persistence owns the full world.