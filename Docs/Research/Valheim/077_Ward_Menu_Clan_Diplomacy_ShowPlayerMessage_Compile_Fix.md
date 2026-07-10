# Investigation 077: Ward menu Clan diplomacy ShowPlayerMessage compile fix

## Problem

The first Clan diplomacy UI patch accidentally duplicated this helper in `BiomeDominionService`:

```csharp
private static void ShowPlayerMessage(Player player, string message)
```

The compiler reported:

```text
Type 'BiomeDominionService' already defines a member called 'ShowPlayerMessage' with the same parameter types
```

## Fix

- Removed the duplicate helper from `BiomeDominionService`.
- Kept one helper in `BiomeDominionService`.
- Kept one helper in `EconomyService`.
- Added/kept one helper in `DiplomacyService`, where the new UI diplomacy action uses it.

## Files changed

- `Source/ClanTerritory/Features/Territory/TerritoryModule.cs`
- `Docs/Research/Valheim/077_Ward_Menu_Clan_Diplomacy_ShowPlayerMessage_Compile_Fix.md`

## Notes

No gameplay logic was changed.
