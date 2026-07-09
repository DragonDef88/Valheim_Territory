# Investigation 056: Localize territory presence messages

## Problem

The ward menu localization worked, but the popup shown when entering or leaving a territory still used hardcoded English text:

- `Entered territory: ...`
- `Left territory: ...`

The fallback territory name was also hardcoded as `Unnamed Territory`.

## Fix

`TerritoryPresenceService` now uses `CtLocalization` for:

- `ct.message.entered_territory`
- `ct.message.left_territory`
- `ct.territory.unnamed`

English and Russian defaults were added to the built-in language dictionaries.

## Result

When `Localization.Language=auto` resolves to Russian, entering/leaving territory should show Russian popup text. Custom language packs can override the same keys.
