# Investigation 030: Preparation chest ground height compile fix

## Problem

The real preparation chest package used:

`ZoneSystem.instance.GetGroundHeight(position, ref groundHeight)`

In the current Valheim assembly this method expects the second argument to be passed with `out`, not `ref`.

## Decision

Change the call to:

`ZoneSystem.instance.GetGroundHeight(position, out groundHeight)`

This is a compile-only fix. No gameplay behavior is changed.
