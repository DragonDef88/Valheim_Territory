# System Lifecycle

## Purpose

This document describes the lifecycle of Clan Territory as a system.

It separates plugin startup, runtime initialization, world discovery, gameplay events, persistence and shutdown.

---

# Lifecycle Overview

```text
Plugin Loaded
↓
Bootstrap.Initialize()
↓
Infrastructure Ready
↓
Modules Initialized
↓
Runtime Initialization
↓
World Discovery
↓
World Synchronization
↓
Gameplay
↓
Persistence
↓
Shutdown

Plugin Startup

The plugin starts through BepInEx.

Plugin calls Bootstrap.Initialize().

At this stage the mod should only prepare infrastructure.

Bootstrap

Bootstrap is responsible for composition only.

Bootstrap may:

assign globals;
initialize config;
create EventBus;
create ModuleManager;
register modules;
initialize modules;
apply Harmony patches.

Bootstrap must not contain gameplay logic.

Bootstrap must not call world discovery directly.

Infrastructure Ready

After Bootstrap completes:

Config is initialized;
EventBus is available;
ServiceContainer is available;
ModuleManager is initialized;
Harmony patches are applied;
all modules are registered.

At this point the mod infrastructure is ready.

Module Initialization

Modules register their services.

Examples:

PersistenceModule registers persistence services;
TerritoryModule registers registry and territory services;
WardDetectionModule registers ward detection services;
WorldDiscoveryModule registers world discovery services.

Modules should not perform heavy gameplay synchronization during initialization.

Runtime Initialization

Runtime initialization begins after infrastructure is ready.

Runtime initialization is responsible for preparing the current world state.

Future runtime initialization steps:

RuntimeInitializationService
↓
WorldDiscoveryService.Discover()
↓
WorldSynchronizationService.Synchronize()
↓
Persistence extras merge
↓
Gameplay ready
World Discovery

World Discovery answers one question:

Which Ward objects exist in the world right now?

World Discovery must not:

modify Registry;
save files;
create Territory directly;
publish gameplay events unnecessarily.

World Discovery only reads the Valheim world and returns Ward models.

World Synchronization

World Synchronization answers one question:

How do we make Registry match the discovered world?

World Synchronization may:

add missing territories;
remove territories whose Ward no longer exists;
rebuild Registry;
request persistence save after synchronization.

World Synchronization uses World Discovery output.

Registry

Registry is an in-memory cache.

Registry reflects the current known state of the world.

Registry is not the ultimate source of truth.

The Valheim world is the source of truth for Ward existence.

Persistence

Persistence stores additional data and snapshots.

Persistence must not be the authority for whether a Ward exists.

Persistence may store:

metadata;
territory snapshot;
permissions;
terrain settings;
portal settings;
extensions.

Persistence must not create ghost territories for Wards that do not exist in the world.

Gameplay Events

Gameplay events modify the runtime state.

Current flows:

Ward placed
↓
WardRegisteredEvent
↓
TerritoryService
↓
Registry
↓
Persistence
Ward removed
↓
WardDestroyedEvent
↓
TerritoryService
↓
Registry
↓
Persistence
Shutdown

During shutdown:

Harmony patches are removed;
modules are shut down;
EventBus is cleared;
ServiceContainer is cleared;
runtime state is discarded.

Persistence should already contain the latest saved state.

Rules
Bootstrap composes the system.
Runtime initializes the world.
Discovery reads the world.
Synchronization changes Registry.
Registry is a cache.
Persistence stores additional data.
The Valheim world is the source of truth.
Ward existence defines Territory existence.
No gameplay logic belongs in Bootstrap.
No ghost Territories are created from JSON alone.