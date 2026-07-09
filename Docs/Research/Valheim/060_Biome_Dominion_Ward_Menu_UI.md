# Investigation 060: Biome Dominion Ward Menu UI

## Problem

Biome dominion was functional through `/ctbiome` console/chat commands, but normal territory management is centered around the ward menu.

The next step was to expose biome ownership and biome-level rules in the existing ward menu without removing command support.

## Implementation

A new Biome tab was added to the Jotunn ward menu.

The menu now shows:

- current biome name;
- claim status;
- owner guild;
- vassal status for the opened ward territory;
- biome door lock state;
- biome structure protection state;
- biome door auto-close seconds.

The menu now supports:

- claim current ward biome;
- release current ward biome;
- toggle biome door lock;
- change biome door auto-close seconds;
- toggle biome structure damage protection.

Only a Guilds guild leader can claim, release, or change biome dominion rules.

## Architecture

`BiomeDominionService` remains the source of truth.

The UI uses a read-only `BiomeDominionMenuState` snapshot built from the service.

Ward menu actions call service methods directly instead of routing through `/ctbiome` command text.

The `/ctbiome` commands remain available for testing/admin use.

## Safety

This is a UI layer over the already tested Biome Dominion system.

No existing territory rules, terraforming worker, treasury storage, or Guilds runtime reflection flow was removed.
