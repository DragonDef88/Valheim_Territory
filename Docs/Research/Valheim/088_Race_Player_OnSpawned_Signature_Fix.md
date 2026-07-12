# Investigation 088: Race Player.OnSpawned signature compatibility

## Problem

Clan Territory completed module initialization, but Harmony patching failed during `Bootstrap.Initialize`.

The runtime error was:

```text
AccessTools.Method: Could not find method for type Player and name OnSpawned and parameters ()
Undefined target method for patch method ClanTerritory.Features.Races.RacePlayerSpawnHook.Postfix
```

The hook explicitly requested a parameterless `Player.OnSpawned`. Valheim 0.221.12 still has the method, but its current signature is not parameterless.

## Consequence

`Harmony.PatchAll()` threw before bootstrap completion, leaving partial Harmony state and preventing the final successful initialization marker.

## Fix

Patch `Player.OnSpawned` by name:

```csharp
[HarmonyPatch(typeof(Player), "OnSpawned")]
```

The postfix only consumes `Player __instance`, so it remains independent of the original method parameters.

## Validation

Rebuild, start Valheim, and confirm:

```text
Clan Territory v0.1.0 initialized.
```

There must be no `Undefined target method` or `RacePlayerSpawnHook` Harmony exception.
