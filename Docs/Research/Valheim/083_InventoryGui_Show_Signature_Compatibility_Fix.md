# Investigation 083: InventoryGui.Show signature compatibility fix

## Problem

The previous virtual container switch compatibility hook targeted:

```csharp
InventoryGui.Show(Container)
```

On the tested Valheim runtime, Harmony reported:

```text
AccessTools.DeclaredMethod: Could not find method for type InventoryGui and name Show and parameters (Container)
ArgumentException: Undefined target method
```

This means the runtime method has a different signature, likely `Show(Container, ...)`.

## Fix

The hook no longer targets an exact signature.

It dynamically finds all `InventoryGui.Show` overloads where the first parameter is `Container`.

The prefix reads the next container from `object[] __args`, so it works across compatible overloads.

## Files changed

- `Source/ClanTerritory/Integration/Valheim/Harmony/PrivateAreaHooks.cs`
- `Docs/Research/Valheim/083_InventoryGui_Show_Signature_Compatibility_Fix.md`

## Notes

This is a runtime compatibility fix.

It does not change terrain logic.
