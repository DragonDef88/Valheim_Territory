# Clan Territory — Event Architecture

## Purpose

Events allow features to communicate without direct dependencies.

## Core Rule

Features publish facts. Other features react to facts.

## Current Event Flow

```text
WardService
↓
WardRegisteredEvent
↓
EventBus
↓
TerritoryService
Rules
Events describe something that already happened.
Events do not return values.
Events must be immutable.
Event handlers must not know about each other.
Services contain logic.
Events contain data.
EventBus must remain simple until complexity is required.
Naming

Events should use past tense.

Good:

WardRegisteredEvent
TerritoryCreatedEvent
PermissionChangedEvent
PlayerEnteredTerritoryEvent

Bad:

CreateTerritoryEvent
CheckPermissionEvent
DoSaveEvent
Current Events
WardRegisteredEvent
Planned Events
TerritoryCreatedEvent
TerritoryRemovedEvent
TerritoryOwnerChangedEvent
TerritorySavedEvent
PlayerEnteredTerritoryEvent
PlayerLeftTerritoryEvent
PermissionChangedEvent