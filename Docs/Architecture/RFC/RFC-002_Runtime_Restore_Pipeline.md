# RFC-002 — Runtime Restore Pipeline

Status: Proposed

Author: Clan Territory Engineering

Source: Audit-005 Runtime Implementation Review

---

# Purpose

Activate and evolve the existing Runtime Pipeline so Clan Territory can restore runtime state from persistent world data.

This RFC defines the architecture for runtime restore.

It does not introduce Unity object spawning or Valheim object creation.

---

# Key Finding

Audit-005 discovered that Runtime Pipeline infrastructure already exists.

Existing components:

- `RuntimePipeline`
- `IRuntimeStep`
- `RuntimePipelineCoordinator`
- `WorldReadyStep`

Therefore this RFC should not create a new pipeline from scratch.

This RFC should activate and evolve the existing Runtime Pipeline.

---

# Problem Statement

Current architecture supports:

- Runtime discovery
- Runtime registry
- Territory creation
- Merge Save
- Delete tracking

Persistence can save a complete world.

However, runtime restore is not implemented.

Current flow:

```text
Valheim Runtime
        │
        ▼
World Discovery
        │
        ▼
Runtime Registry
        │
        ▼
Territory
        │
        ▼
Merge Save

Missing reverse direction:

Save File
   │
   ▼
Persistence Load
   │
   ▼
Runtime Restore
   │
   ▼
Gameplay Ready

This creates an asymmetric architecture.

Current Runtime State

Runtime currently contains:

PluginLoaded
InfrastructureReady
WorldLoading
WorldLoaded
DiscoveryCompleted
RegistrySynchronized
GameplayReady

Some states already exist but are not fully used.

RFC-002 should use the existing state model instead of replacing it.

Design Goals

The Runtime Restore Pipeline must:

restore runtime state from persistent storage;
rebuild runtime representations only;
never instantiate Unity objects;
never create ZDOs;
never replace Valheim streaming;
avoid duplicating World Discovery;
keep Territory as one gameplay step, not the whole pipeline.
Non Goals

This RFC does not introduce:

prefab spawning;
Unity object creation;
ZDO creation;
Harmony patches;
Valheim lifecycle changes;
new save file format.

Those remain outside Runtime Restore.

Architectural Principles
Principle 1

Persistence owns data.

Runtime owns loaded runtime state.

Gameplay owns behaviour.

Principle 2

Persistence never creates gameplay.

Gameplay never reads JSON directly.

Runtime does not own persistent storage.

Principle 3

Runtime is reconstructed from persistence.

Persistence is never reconstructed blindly from runtime.

Merge Save remains the persistence safety boundary.

Proposed Pipeline
WorldLoaded
   │
   ▼
Runtime Pipeline
   │
   ├── Runtime Discovery Step
   ├── Persistence Load Step
   ├── Runtime Restore Step
   ├── Registry Synchronization Step
   ├── Gameplay Build Step
   └── Gameplay Ready Step
Proposed Runtime State Flow
InfrastructureReady
   │
   ▼
WorldLoaded
   │
   ▼
DiscoveryCompleted
   │
   ▼
RegistrySynchronized
   │
   ▼
GameplayReady

Future RFCs may add additional states only if required.

Responsibilities
Integration

Responsible for:

detecting when Valheim world runtime is ready;
translating Valheim lifecycle signals into Runtime state transitions.

Integration is not responsible for restore logic.

Runtime Pipeline

Responsible for:

ordering runtime lifecycle steps;
executing steps based on current Runtime state;
advancing Runtime state after successful steps;
publishing runtime lifecycle events.
Persistence

Responsible for:

reading save files;
validating data;
mapping persistence models;
producing data usable by Runtime restore.

Persistence is not responsible for gameplay initialization.

Runtime Registry

Responsible for:

storing loaded runtime representations;
providing runtime lookup;
exposing runtime state to gameplay systems.
Gameplay

Responsible for:

reacting after Runtime is restored;
building gameplay systems from Runtime data;
never reading persistence files directly.
Required Changes
Phase 1 — Activate Pipeline Coordinator

Wire RuntimePipelineCoordinator into RuntimeModule.

Pipeline should listen to RuntimeStateChangedEvent.

Phase 2 — Use OutputState

Update pipeline execution so successful steps can advance Runtime state.

IRuntimeStep.OutputState already exists and should be used.

Phase 3 — Discovery Step

Move current world discovery orchestration into a pipeline step.

Current logic exists in RuntimeOrchestrator.

Phase 4 — Persistence Load Step

Add a pipeline step that loads persistence data.

This step must not create gameplay.

Phase 5 — Runtime Restore Step

Add a pipeline step that rebuilds Runtime Registry data from persistence data.

This step must not spawn Valheim objects.

Phase 6 — Gameplay Ready Step

Advance Runtime to GameplayReady after restore and gameplay initialization complete.

Risks
Risk 1 — Pipeline loops

If state transitions trigger pipeline execution repeatedly, steps must avoid loops.

Mitigation:

state machine ignores transitions to the same state;
each step should have a clear input and output state.
Risk 2 — Runtime and Gameplay coupling

Current Runtime orchestration directly creates territories from discovered wards.

Mitigation:

Move this into a gameplay-oriented pipeline step or event handler.

Risk 3 — Persistence timing

Persistence must load after the world context is available but before gameplay finalization.

Mitigation:

Keep persistence load as an explicit pipeline step.

Acceptance Criteria

RFC-002 is implemented when:

RuntimePipelineCoordinator is wired into Runtime;
RuntimePipeline uses IRuntimeStep.OutputState;
world discovery runs as a pipeline step;
persistence load participates in the pipeline;
runtime restore rebuilds runtime state without spawning Unity objects;
gameplay becomes ready through pipeline completion;
project rebuild succeeds.
Final Decision

Proposed.

Implementation should proceed in small phases.

Do not rewrite Runtime.

Evolve the existing Runtime Pipeline.