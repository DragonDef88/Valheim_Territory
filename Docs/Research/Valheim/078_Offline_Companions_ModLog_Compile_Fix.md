# Investigation 078: Offline Companions compatibility ModLog compile fix

## Problem

The Offline Companions inventory UI compatibility patch used:

```csharp
ModLog.Debug(...)
```

inside a file that did not import the utility namespace.

The compiler reported:

```text
The name 'ModLog' does not exist in the current context
```

## Fix

Added:

```csharp
using ClanTerritory.Utils;
```

to the affected source file.

## Files changed

- `Source/ClanTerritory/Integration/Valheim/Harmony/PrivateAreaHooks.cs`
- `Docs/Research/Valheim/078_Offline_Companions_ModLog_Compile_Fix.md`

## Notes

No gameplay logic was changed.
