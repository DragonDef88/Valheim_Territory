# Investigation 025: Local vanilla 2m structure protection bubble

## Problem

The custom 5m LineRenderer bubble fixed the timing problem better than the previous approach, but it did not use the vanilla ward flash visual. The desired behavior is now:

- Keep the early `RPC_Damage` block so vanilla ward feedback does not run.
- Use the vanilla ward flash effect style.
- Show it only at the protected piece / hit position.
- Reduce the feedback radius to about 2m.

## Evidence

`WearNTear.RPC_Damage(...)` calls `PrivateArea.OnObjectDamaged(...)` before `ApplyDamage(...)`, so custom protection must keep blocking at the `RPC_Damage` prefix stage to prevent vanilla ward-level feedback.

`EffectList.Create(...)` accepts a `scale` argument and returns the created `GameObject[]`, allowing additional post-create scaling of the root and particle systems.

## Decision

- Keep the `WearNTear.RPC_Damage(...)` prefix from investigation 024.
- Stop drawing the custom LineRenderer bubble.
- Create `privateArea.m_flashEffect` directly at the protected piece / hit position.
- Do not call `PrivateArea.FlashShield(false)`.
- Do not invoke the ward-level `FlashShield` RPC.
- Use scale `2 / 50 = 0.04`.
- Also force child `ParticleSystem` scaling mode to `Hierarchy` and scale their start size to improve consistency.

## Limit

This is visual feedback only. Damage rules, ownership checks, radius sync, door locks, and vanilla ward behavior are unchanged.
