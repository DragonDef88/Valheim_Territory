# Architecture Audit 005 — Runtime Implementation Review

## Purpose

This document reviews the current Runtime implementation before starting RFC-002:

```text
Runtime Restore Pipeline

The goal is to determine whether the existing Runtime infrastructure should be replaced or evolved.

Review Status
Area	Status
RuntimeState	PASS
RuntimeStateMachine	WATCH
RuntimeRegistry	PASS
RuntimeWard	PASS / WATCH
RuntimeModule	WATCH
RuntimeOrchestrator	WATCH
RuntimePipeline	ACTION
RuntimePipelineCoordinator	ACTION
WorldReadyStep	ACTION
WorldReadyDetector	ACTION
Key Finding

The project already contains Runtime Pipeline infrastructure.

Therefore, RFC-002 should not create a new pipeline from scratch.

RFC-002 should activate and evolve the existing Runtime Pipeline.

RuntimeState

Status:

PASS

RuntimeState already describes a world runtime lifecycle:

PluginLoaded
InfrastructureReady
WorldLoading
WorldLoaded
DiscoveryCompleted
RegistrySynchronized
GameplayReady

This is a good foundation for RFC-002.

Current concern:

Some later states are defined but not yet actively used.

RuntimeStateMachine

Status:

WATCH

The state machine is simple and healthy.

It owns the current Runtime state and publishes RuntimeStateChangedEvent.

Current limitation:

It does not validate transition order.

This is acceptable now, but RFC-002 may require stricter transition control.

RuntimeRegistry

Status:

PASS

RuntimeRegistry stores loaded runtime wards.

It does not own Persistence.

It does not own gameplay rules.

This matches the Runtime principle:

Runtime represents the loaded world only.
RuntimeWard

Status:

PASS / WATCH

RuntimeWard stores runtime ward identity, position and loaded/active state.

It currently uses Unity Vector3.

This is acceptable inside Runtime, but should not migrate into Domain.

RuntimeModule

Status:

WATCH

RuntimeModule currently composes Runtime services manually.

It creates:

RuntimeStateMachine
RuntimeInitializationService
RuntimeOrchestrator

It also wires RuntimeOrchestrator directly to:

IWorldDiscoveryService
ITerritoryService

This works today.

However, RFC-002 should reduce direct orchestration coupling and move toward pipeline-based steps.

RuntimeOrchestrator

Status:

WATCH

RuntimeOrchestrator currently handles RuntimeStateChangedEvent.

When state becomes:

WorldLoaded

it:

runs world discovery;
creates territories from discovered wards;
sets state to DiscoveryCompleted.

This makes the orchestrator currently Territory-oriented.

For RFC-002, orchestration should become more generic and pipeline-driven.

RuntimePipeline

Status:

ACTION

RuntimePipeline already exists.

It stores IRuntimeStep instances and executes steps matching the current input state.

Current limitation:

IRuntimeStep.OutputState exists but is not used by the pipeline.

This means pipeline steps currently cannot advance the Runtime state by themselves.

RFC-002 should address this.

RuntimePipelineCoordinator

Status:

ACTION

RuntimePipelineCoordinator already listens to RuntimeStateChangedEvent and executes the pipeline for the current state.

Current limitation:

It is not currently wired into RuntimeModule.

RFC-002 should decide whether this coordinator replaces or complements RuntimeOrchestrator.

IRuntimeStep

Status:

PASS / WATCH

IRuntimeStep already defines:

InputState
OutputState
Execute()

This is a strong foundation.

Current limitation:

There is no explicit result model.

Future pipeline steps may need success/failure reporting.

Do not add this before RFC-002 requires it.

WorldReadyStep

Status:

ACTION

WorldReadyStep exists but only logs.

It does not advance Runtime state.

This is acceptable as a placeholder, but it must evolve during RFC-002.

WorldReadyDetector

Status:

ACTION

WorldReadyDetector.IsWorldReady() currently returns false.

This is a placeholder.

Current actual world-ready detection happens in Valheim Integration through runtime world-ready hooks.

RFC-002 should decide whether this detector is removed, implemented, or replaced by Integration events.

Architectural Recommendation

Do not replace Runtime.

Evolve it.

Recommended RFC-002 direction:

Activate existing Runtime Pipeline

Instead of:

Create new Runtime Pipeline
RFC-002 Starting Point

RFC-002 should focus on these tasks:

Wire RuntimePipelineCoordinator.
Make pipeline steps able to advance Runtime state.
Move world discovery into a pipeline step.
Add persistence load/merge/restore steps.
Keep Territory as one gameplay step, not the whole Runtime pipeline.
Non-Goals

Do not rewrite:

RuntimeRegistry
RuntimeStateMachine
RuntimeWard
PersistenceService
TerritoryService

unless RFC-002 proves a direct need.

Final Assessment

Runtime is healthy.

The main issue is that pipeline infrastructure exists but is not yet active.

Result:

Runtime: PASS with ACTION items for RFC-002

Next recommended task:

RFC-002 — Runtime Restore Pipeline