# Investigation 055: Auto localization uses Valheim client language

## Problem

`Localization.Language=auto` used `Application.systemLanguage`. That can return the operating system / Unity language rather than the selected Valheim client language in r2modman or Steam launch configurations.

## Fix

`auto` now resolves language in this order:

1. Valheim runtime `Localization` object by reflection:
   - `GetSelectedLanguage()`
   - `GetCurrentLanguage()`
   - `GetLanguage()`
   - common language fields such as `m_selectedLanguage`, `m_currentLanguage`, `m_language`, `m_languageName`
2. Valheim/Unity `PlayerPrefs` language strings.
3. `Application.systemLanguage` as fallback.

The detected value is normalized to a language pack code:

- Russian / ru / русский -> `ru`
- English / en -> `en`
- common other client names map to their standard code when possible.

Custom language packs still work by setting `Localization.Language=<code>` manually.

## Result

With `Localization.Language=auto`, the menu should follow the actual Valheim client language instead of the OS language.
