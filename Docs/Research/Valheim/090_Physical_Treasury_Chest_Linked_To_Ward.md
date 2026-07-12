# Investigation 090: Physical treasury chest linked to ward

## Decision

The virtual Treasury container is removed from the active runtime path.

Each ward owns one real `piece_chest_blackmetal` placed on the ward local
center line, exactly behind the ward:

```text
position = ward.position - ward.forward * 1.75
```

The chest uses the ward yaw and ground height at the target position.

## Ownership link

The ward stores the treasury ZDO ID:

```text
ct_territory_treasury_chest_zdo_user
ct_territory_treasury_chest_zdo_id
```

The chest stores the ward ID:

```text
ct_territory_treasury_ward_id
```

Both objects therefore have a persistent two-way link.

## Lifetime

The physical chest is protected from:

- normal `WearNTear` damage;
- environmental `ApplyDamage`;
- `RPC_Remove`;
- hammer removal;
- normal `WearNTear.Destroy`.

When `WardDestroyedEvent` is published, the linked chest contents are
dropped and the chest is destroyed with the normal `ZNetView.Destroy()`
path.

## Inventory settings

The real blackmetal chest replaces the old virtual Treasury settings:

- name: `Territory Treasury`;
- size: 8 x 4;
- privacy: ward creator only;
- no guard-stone secondary check;
- no auto-destroy when empty;
- no rain/support/ash/burn wear;
- custom Treasury stack capacity: 9999 per slot.

The existing large-stack inventory hooks continue to recognize the real
chest through `TreasuryContainerByInventory`.

## Migration

If the ward still has legacy serialized virtual Treasury data in:

```text
ct_territory_treasury_chest_items
```

the data is loaded once into an empty physical chest. After a successful
migration, the old ward value is cleared.

The old key remains in code only for this migration and is not used as
active Treasury storage.

## Resource absorption

Ground resource absorption obtains the inventory of the linked physical
Treasury. It no longer creates a temporary hidden blackmetal chest and no
longer persists Treasury items into the ward ZDO.

Before the worker mutates or destroys a ground `ItemDrop`, it claims the
network object with `ZNetView.ClaimOwnership()` and verifies ownership.

## Expected effect on ZNetScene errors

The previous virtual Treasury instantiated a network prefab, hid it, and
destroyed it whenever the inventory UI closed. Removing that lifecycle
eliminates the most likely source of a `ZNetView` remaining in
`ZNetScene.m_instances` after its ZDO was reset.

The fix is confirmed only when a fresh runtime log no longer contains:

```text
ZNetScene.DMD<ZNetScene::RemoveObjects>
NullReferenceException
```

## Validation

1. Rebuild `ClanTerritory.sln`.
2. Confirm zero build errors.
3. Enter a world containing an existing ward.
4. Confirm one blackmetal chest appears 1.75 m behind the ward.
5. Confirm the chest center is aligned with the ward local X axis.
6. Open Treasury from the ward menu and directly from the chest.
7. Confirm the old virtual Treasury items migrated.
8. Confirm Treasury stacks may exceed vanilla stack limits up to 9999.
9. Confirm weapons, enemies, weather and the hammer cannot destroy it.
10. Remove the ward and confirm the Treasury contents drop and the chest
    disappears.
11. Drop a matching resource in the territory and confirm it is absorbed
    into the physical Treasury.
12. Inspect a fresh single-run BepInEx log for `RemoveObjects` errors.
