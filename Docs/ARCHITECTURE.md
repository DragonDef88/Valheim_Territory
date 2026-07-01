# Clan Territory — Architecture

## Current Structure

- Core
- API
- Abstractions
- Config
- Utils
- Domain
- Events
- Features

## Core

Responsible for plugin startup, module loading, service registration and shutdown.

Includes:

- Plugin
- Bootstrap
- ModuleManager
- ServiceContainer
- Globals
- ModInfo

## Domain

Pure game rules and data.

Domain must not depend on:

- UnityEngine
- Valheim classes
- BepInEx
- Harmony

Current domain objects:

- Territory
- WardId
- TerritoryId
- PlayerId
- WorldPosition
- TerritoryRadius
- OwnerInfo

## Features

Feature modules contain Valheim-facing gameplay logic.

Current features:

- WardDetection
- Territory

## Event Flow

Ward placement flow:

```text
Player places Ward
↓
PiecePlacementHooks
↓
WardService
↓
WardRegisteredEvent
↓
EventBus
↓
TerritoryService
↓
TerritoryFactory
↓
Domain.Territory
↓
TerritoryRegistry