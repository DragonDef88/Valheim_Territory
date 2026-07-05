using System;
using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Events;
using ClanTerritory.Features.Runtime.Orchestration;
using ClanTerritory.Features.Runtime.Pipeline;
using ClanTerritory.Features.Runtime.Pipeline.Steps;
using ClanTerritory.Features.Runtime.Services;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.WorldDiscovery.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime
{
    internal sealed class RuntimeModule : IInitializable, IDisposableModule
    {
        private RuntimeStateMachine _stateMachine;
        private IRuntimeInitializationService _runtimeInitializationService;
        private RuntimePipeline _runtimePipeline;
        private RuntimePipelineCoordinator _runtimePipelineCoordinator;

        public void Initialize()
        {
            if (!ServiceContainer.TryGet<EventBus>(out EventBus eventBus))
            {
                throw new InvalidOperationException(
                    "EventBus is not registered.");
            }

            _stateMachine = new RuntimeStateMachine(eventBus);
            ServiceContainer.Register(_stateMachine);

            _runtimeInitializationService = new RuntimeInitializationService();
            ServiceContainer.Register<IRuntimeInitializationService>(
                _runtimeInitializationService);

            if (!ServiceContainer.TryGet<IWorldDiscoveryService>(
                    out IWorldDiscoveryService worldDiscoveryService))
            {
                throw new InvalidOperationException(
                    "WorldDiscoveryService is not registered.");
            }

            if (!ServiceContainer.TryGet<ITerritoryService>(
                    out ITerritoryService territoryService))
            {
                throw new InvalidOperationException(
                    "TerritoryService is not registered.");
            }

            _runtimePipeline = new RuntimePipeline();
            _runtimePipeline.AddStep(
                new WorldDiscoveryStep(
                    worldDiscoveryService,
                    territoryService));

            ServiceContainer.Register(_runtimePipeline);

            _runtimePipelineCoordinator =
                new RuntimePipelineCoordinator(
                    _stateMachine,
                    _runtimePipeline);

            ServiceContainer.Register(_runtimePipelineCoordinator);

            eventBus.Subscribe<RuntimeStateChangedEvent>(
                _runtimePipelineCoordinator);

            RuntimeOrchestrator orchestrator = new RuntimeOrchestrator(
                _stateMachine,
                worldDiscoveryService,
                territoryService);

            ServiceContainer.Register<IRuntimeOrchestrator>(orchestrator);

            ModLog.Info("Runtime module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("Runtime module shutdown.");
        }
    }
}