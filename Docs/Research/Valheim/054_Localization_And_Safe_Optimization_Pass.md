# Investigation 054: Localization and safe optimization pass

## Goal

Review the current GitHub main package, avoid risky gameplay rewrites, and add a localization system that starts with English and Russian while allowing new language packs.

## Review result

The current codebase already has the latest Guilds access and Guilds map marker integration. The runtime log confirms:

- Clan Territory loads successfully.
- Guilds and Groups load.
- Clan Territory connects to Guilds API.
- The ward guild is synced as `DMC`.

No broad rewrite was made because the current runtime behavior is stable.

## Optimization

A safe optimization was applied to `WardMapIconService`:

- reflection lookup of `Minimap.m_pins` is now cached in a static field;
- map pin registration checks no longer call `AccessTools.Field(...)` every time.

This is intentionally small and low-risk.

## Localization

A lightweight localization layer was added without introducing new dependencies or new project references.

Language selection:

- config key: `Localization.Language`
- default: `auto`
- `auto` selects Russian when Unity system language is Russian, otherwise English.
- users can set `en`, `ru`, or any custom language code.

Language pack location:

```text
BepInEx/plugins/ClanTerritory/Localization/<code>.txt
```

Format:

```text
key=value
```

Line breaks inside values use `\n`.

At runtime the mod creates default `en.txt`, `ru.txt`, and `README.txt` if they do not exist.

## First localized areas

- Jotunn ward menu title, subtitle, tabs, panels, buttons, status values, and preparation chest labels.
- Locked door message.
- Terraforming service unavailable status.
- Basic legacy fallback text remains safe if a value is missing.

## Adding a language pack

1. Copy `en.txt` or `ru.txt`.
2. Rename it to a language code, for example `de.txt`.
3. Translate values.
4. Set `Localization.Language=de` in the BepInEx config.
5. Restart the game.
