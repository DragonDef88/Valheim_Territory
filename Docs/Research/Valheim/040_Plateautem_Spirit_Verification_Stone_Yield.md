# Investigation 040: Plateautem-style spirit verification and stone yield

## Problem

The ward-height leveling worker was functional, but still felt too fast and rough compared with Plateautem.

Observed differences:

- Plateautem has a visible moving spirit/drone that scans around the totem.
- Plateautem does not immediately apply every operation; it scans, checks local ground samples, then applies work.
- Lowering terrain in Plateautem produces stone instead of only consuming resources.

## Plateautem references

Plateautem scans from the object with `currentScanProgress` and `currentScanIndex`. The visual drone position is derived from the same spiral progress that drives the worker.

Plateautem also handles stone as a signed resource delta:

- raising terrain creates positive stone cost;
- lowering terrain creates negative stone cost;
- `currentStoneStored -= num6` therefore increases stored stone when terrain is lowered.

## Clan Territory decision

The worker now moves closer to the Plateautem model while keeping Clan Territory ward-based storage:

- adds a visible local spirit sphere/light that flies along the scan spiral;
- stores scan speed, pending scan index, and verification count in ward ZDO;
- requires repeated verification passes before applying a terrain operation;
- lowers terrain into stone yield and stores yielded stone back into the virtual preparation chest;
- skips lowering when the preparation chest stone row has no free capacity;
- clamps terraforming max radius to `40`, matching Plateautem's default maximum radius.

## Safety

The spirit object is local runtime feedback only. It is destroyed when leveling is disabled/stopped and is not saved as a world object.
