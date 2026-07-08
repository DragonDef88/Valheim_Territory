# Investigation 022: Structure protection hit-position feedback

## Problem

The first feedback fix called vanilla `PrivateArea.FlashShield(false)` when custom structure protection blocked damage. Testing still showed no obvious ward bubble when a protected structure was hit outside the old/default visual range.

## Evidence

The latest runtime log shows Clan Territory loading normally, the ward menu opening with `structureDamageProtection: True`, and radius values being applied/synced. This means the rule state is active, but the feedback is still not visible enough in the tested position.

Vanilla `PrivateArea.FlashShield(false)` creates the ward's `m_flashEffect` at the ward position through the `FlashShield` RPC. That is faithful to vanilla, but for expanded territories the protected structure can be far from the ward, so the player may not see the ward-position effect.

## Decision

- Keep calling vanilla `PrivateArea.FlashShield(false)` so the ward itself still receives the vanilla shield feedback.
- Also create the same vanilla `m_flashEffect` directly at the protected piece position when custom structure protection blocks damage.
- Log the block at info level so the next test confirms that the protection branch executed even if a visual effect is missed.

## Limit

This is visual feedback only. Damage blocking, territory radius, door locking, and ward ownership rules are unchanged.
