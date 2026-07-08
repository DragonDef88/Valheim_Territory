# Investigation 026: Visible local vanilla 2m bubble scaling

## Problem

Testing the local vanilla 2m structure-protection bubble produced sound but no visible shield. The log confirmed that the custom structure-protection branch executed repeatedly, so the issue was visual scaling, not rule execution.

## Evidence

The log contains repeated lines:

`Structure damage blocked. Local vanilla 2m shield bubble shown at protected piece.`

This means `TryBlockStructureDamage(...)` found the protected territory and called the local effect code.

## Cause

The first 2m implementation over-scaled the vanilla effect:

- passed `scale = 0.04` to `EffectList.Create(...)`;
- then set created root `localScale = 0.04`;
- then multiplied particle start sizes by `0.04`.

Depending on the prefab internals this can shrink the particle visuals multiple times while the audio still plays normally.

## Decision

- Create the vanilla flash effect at normal scale with `EffectList.Create(position, rotation)`.
- Do not pass the scale argument to `Create(...)`.
- Do not multiply particle start sizes.
- Set child particle systems to `ParticleSystemScalingMode.Hierarchy`.
- Scale only the created root objects by `2 / 50 = 0.04`.

## Limit

If this is still too small visually, the next safe adjustment is to keep the same logic and change only `customBubbleRadius` from `2f` to `3f` or `4f`.
