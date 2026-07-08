# Investigation 021: Structure protection shield feedback

## Problem

Testing confirmed that structure protection blocks damage inside expanded territory radius, but the vanilla guard-stone shield bubble does not flash when a protected structure is hit outside the old/default ward range.

## Evidence

The runtime log shows the tested ward loaded with radius 200, opened with `structureDamageProtection: True`, and synchronized radius changes correctly. This means the territory rule area is already expanded and functional.

Vanilla `PrivateArea.OnObjectDamaged(...)` calls `FlashShield(false)`, and `CheckAccess(...)` also calls `FlashShield(false)` when access is denied. `FlashShield(...)` invokes the `FlashShield` RPC on all clients, and `RPC_FlashShield(...)` creates `m_flashEffect` at the ward position. This is the vanilla bubble visual we want to reuse.

Our structure-protection Harmony prefix blocks `WearNTear.ApplyDamage(...)` before vanilla damage flow can reach the vanilla ward notification path. Therefore, the damage is prevented correctly, but the feedback effect must be triggered explicitly.

## Decision

- Replace the read-only `IsStructureDamageProtected(...)` call in `WearNTearHooks` with `TryBlockStructureDamage(...)`.
- When custom structure protection blocks damage, find the protecting ward using the current expanded `PrivateArea.m_radius`.
- Invoke vanilla private `PrivateArea.FlashShield(false)` through Harmony `AccessTools`.
- Keep a fallback direct `FlashShield` RPC if reflection is unavailable.
- Keep the final damage result blocked exactly as before.

## Limit

This only affects visual feedback for blocked structure damage. It does not change damage rules, territory radius, door locks, or ownership checks.
