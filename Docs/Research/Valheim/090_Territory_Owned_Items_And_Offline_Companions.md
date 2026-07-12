# Investigation 090: Territory-owned items and Offline Companions

## Goal

A ground item that enters a Clan Territory ward becomes owned by that
territory.

After assignment:

1. matching territory chests may absorb the item;
2. the ward creator may pick it up;
3. vanilla permitted players may pick it up;
4. members of the ward guild may pick it up;
5. an Offline Companions 1.3.0 companion may pick it up when its owner has
   the same territory access.

## Logical ownership

Valheim ZDO network ownership and Clan Territory ownership are different.

Network ownership identifies the peer allowed to mutate a network object.
Territory ownership is stored persistently on the ItemDrop ZDO:

```text
ct_item_territory_ward_id
```

The value is the owning ward ZDO ID.

The authoritative peer that owns the ward assigns this key and claims the
ItemDrop network object when required.

## Pickup control

Clan Territory patches both paths used by normal pickup flows:

- `ItemDrop.Pickup(Humanoid)`;
- `Humanoid.Pickup(GameObject, ...)` overloads discovered by reflection.

Untagged items keep vanilla behavior.

Tagged items require access to the tagged ward. Authorized pickup claims
the ItemDrop network object before vanilla pickup continues. Unauthorized
pickup is rejected.

When the owning ward is actually destroyed, the WardDestroyedEvent handler
removes the tag from loaded ground items belonging to that ward.

## Offline Companions 1.3.0

The integration has no compile-time dependency on the ProfMags DLL.

At runtime Clan Territory resolves:

```text
Companions.CompanionSetup
CompanionSetup.OwnerHash
HC_Owner
```

Offline Companions stores the owner as:

```text
Player.GetPlayerID().ToString()
```

The companion therefore receives exactly the same territory permissions as
its owner. The native Offline Companions Auto Pickup option remains the
switch that decides whether the companion attempts to collect items.

OfflineCompanionsPlus is not required by this integration.

## Chest absorption

The existing matching-stack rule remains:

- preparation storage with a matching stack;
- treasury storage with a matching stack;
- a closed real container inside the same territory with a matching stack.

Before changing or destroying the ground stack, the worker verifies that
the ItemDrop tag matches the current ward and claims network ownership.

## Validation

1. Rebuild `ClanTerritory.sln`.
2. Confirm zero build errors and no CS0162 warning.
3. Drop Wood inside an owned ward.
4. Confirm a matching closed chest absorbs it.
5. Drop an item without a matching stack and confirm it remains.
6. Confirm the creator, a permitted player and a guild member can pick it.
7. Confirm an outsider cannot pick it.
8. Enable Auto Pickup on an Offline Companions 1.3.0 companion whose owner
   has access and confirm the companion can collect it.
9. Confirm a companion owned by an outsider cannot collect it.
10. Confirm untagged items outside all territories retain vanilla behavior.
11. Confirm the log no longer repeats ItemDrop RequestOwn ownership errors
    from the territory worker.
