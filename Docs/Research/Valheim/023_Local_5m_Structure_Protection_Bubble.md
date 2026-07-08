# Investigation 023: Local 5m structure protection bubble

## Problem

The previous structure-protection feedback still used vanilla ward-level `FlashShield(false)` in addition to local hit-position feedback. For expanded territories this can be visually confusing because the ward bubble may be far away from the protected building.

## Evidence

`EffectList.Create(...)` returns the created `GameObject[]`. This allows us to create the existing vanilla ward flash effect at the protected structure position and then scale the created effect objects.

## Decision

- Do not call vanilla `PrivateArea.FlashShield(false)` for custom structure-protection blocks.
- Do not invoke the ward-level `FlashShield` RPC fallback.
- Create `privateArea.m_flashEffect` only at the protected piece / hit position.
- Scale created effect objects by `5 / 50 = 0.1`, treating vanilla ward bubble scale as the old 50m reference.
- Log the custom feedback as `Local 5m shield bubble shown at protected piece`.

## Limit

This only changes custom structure-protection feedback. Vanilla ward protection feedback remains untouched for vanilla ward mechanics.
