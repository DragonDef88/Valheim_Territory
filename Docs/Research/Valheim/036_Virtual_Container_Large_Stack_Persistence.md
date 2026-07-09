# Investigation 036: Virtual container large stack persistence

## Problem

Virtual ward containers saved item stacks above the vanilla item stack size, but reopening the container restored only the vanilla stack amount. For example, wood restored as `50 / 50` instead of a preparation slot such as `50 / 500`.

## Cause

Vanilla `Inventory.Save(...)` writes the real stack amount, but vanilla `Inventory.Load(...)` recreates items through item prefabs. During this flow the item stack is clamped by the prefab's `m_shared.m_maxStackSize`.

## Decision

Virtual Clan Territory containers no longer use vanilla `Inventory.Load(...)`.

They now use a custom loader for the vanilla inventory package format:

- Reads saved stack directly from the package.
- Creates an `ItemDrop.ItemData` clone from the prefab.
- Preserves the saved stack up to the Clan Territory slot limit.
- Applies a per-container stack limit clone to the item's shared data.

## Slot limits

- Terraforming tool slots: `1`.
- Terraforming fuel and stone slots: `500`.
- Treasury slots: `9999`.

This keeps normal items unchanged outside the virtual containers while showing and preserving larger virtual container stacks.
