# Investigation 080: Plateautem-style terraforming default

## Goal

Make Clan Territory terraforming feel closer to Plateautem.

The previous worker was safe but too cautious:

- slow scan sweep;
- repeated verification delay;
- small operation radius;
- low terrain delta per operation;
- high work threshold.

This made terraforming look like it was not working, especially when many UI/container mods are active.

## Change

The default worker profile is now Plateautem-style:

```text
LevelingSampleSpacing: 2.6 -> 1.6
LevelingOperationRadius: 1.75 -> 3.0
LevelingSampleRadius: 1.5 -> 2.25
LevelingWorkThreshold: 0.45 -> 0.08
LevelingVerifyPasses: 3 -> 1
LevelingScanTime: 0.55 -> 0.08
LevelingFlatteningTime: 1.8 -> 0.16
LevelingTolerance: 0.25 -> 0.12
LevelingMaxDeltaPerOperation: 0.18 -> 0.65
LevelingTerrainFuelWorkMultiplier: 0.45 -> 0.22
```

The local evaluation amount multiplier changed from `0.35` to `0.85`.

## Expected behavior

The worker should now visibly flatten terrain shortly after starting, rather than spending a long time scanning and verifying.

The worker still respects:

- ward ownership/access validation;
- enabled/running state;
- configured terraforming radius;
- territory radius cap;
- fuel;
- stone;
- hoe/pickaxe requirements;
- virtual preparation chest persistence.

## Files changed

- `Source/ClanTerritory/Features/Territory/TerritoryModule.cs`
- `README.md`
- `Docs/Research/Valheim/080_Plateautem_Style_Terraforming_Default.md`

## Notes

This is not a rewrite and does not remove old persistence/state logic.

It is a safer first step: same architecture, Plateautem-like tuning.
