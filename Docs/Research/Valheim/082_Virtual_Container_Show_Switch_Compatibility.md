# Investigation 082: Virtual container Show switch compatibility

## Problem

Offline Companions still reported `HideInternal reason=ContainerSwitch` after Clan Territory virtual containers were opened.

Runtime sequence:

```text
[Clan Territory] [TerritoryContainers] Virtual container opened without world chest: piece_chest_wood
[Offline Companions] [UI] Show ...
[Offline Companions] [UI] HideInternal reason=ContainerSwitch ...
```

Clan Territory already saved virtual containers on `InventoryGui.Hide`, but another mod can switch containers by calling `InventoryGui.Show(newContainer)` directly.

In that path, the previous virtual Clan Territory container may still be the current container when another container is shown.

## Fix

Add a Harmony prefix for:

```csharp
InventoryGui.Show(Container)
```

Before another container is shown, if the current container is a Clan Territory virtual container, close/persist it first:

```csharp
TerritoryTerraformingService.CloseVirtualTerritoryContainer(currentContainer);
```

## Files changed

- `Source/ClanTerritory/Integration/Valheim/Harmony/PrivateAreaHooks.cs`
- `Docs/Research/Valheim/082_Virtual_Container_Show_Switch_Compatibility.md`

## Notes

This does not depend on Offline Companions directly.

It makes Clan Territory virtual containers cleanly close when any other mod switches `InventoryGui` to another container.
