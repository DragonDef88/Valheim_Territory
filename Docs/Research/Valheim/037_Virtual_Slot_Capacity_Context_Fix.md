# Investigation 037: Virtual slot capacity context compile fix

## Problem

The virtual container large-stack package called `GetVirtualSlotCapacity(...)`, but the helper was not in the same class/context as the custom virtual inventory loader.

## Decision

Move/add the helper set next to the custom virtual inventory loader:

- `GetVirtualSlotCapacity(...)`
- `ApplyVirtualStackLimit(...)`
- `CloneSharedData(...)`

This is a compile-only fix for the large-stack virtual container loader.
