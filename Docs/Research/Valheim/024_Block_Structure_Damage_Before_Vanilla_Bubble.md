# Investigation 024: Block structure damage before vanilla ward bubble

## Problem

After switching custom structure-protection feedback to the hit position, testing still showed a large bubble at the hit location and a ward bubble. The log confirmed our custom block branch was running, but vanilla feedback was still visible.

## Evidence

`WearNTear.RPC_Damage(...)` calls `PrivateArea.OnObjectDamaged(...)` before it calls `ApplyDamage(...)`. The old custom block was in an `ApplyDamage` prefix, which means vanilla ward feedback had already fired by the time our protection stopped the health loss.

`EffectList.Create(...)` returns created effect objects, but the vanilla ward flash prefab is still a vanilla ward-scale effect and is not suitable for a small local-only custom territory bubble.

## Decision

- Add a prefix on `WearNTear.RPC_Damage(...)`.
- If custom structure protection applies, stop `RPC_Damage(...)` before vanilla `PrivateArea.OnObjectDamaged(...)`.
- Keep the `ApplyDamage(...)` prefix as a fallback for environmental damage paths that do not go through `RPC_Damage(...)`.
- Stop using `PrivateArea.m_flashEffect` for custom structure protection feedback.
- Draw a custom 5m bubble at the hit/protected-piece position using three temporary `LineRenderer` circles.
- Do not call vanilla ward `FlashShield` or its RPC for custom structure protection.

## Limit

This changes only custom structure-protection feedback and the timing of custom damage blocking. Vanilla ward mechanics remain untouched.
