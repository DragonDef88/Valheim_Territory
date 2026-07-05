using System;
using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Persistence.Services;
using ClanTerritory.Features.Runtime.Events;
using ClanTerritory.Features.Runtime.Pipeline;
using ClanTerritory.Features.Runtime.Pipeline.Steps;
using ClanTerritory.Features.Runtime.Restore;
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
        private RuntimeRestoreContext _runtimeRestoreContext;

        public void Initialize()
        {
            if (!ServiceContainer.TryGet<EventBus>(out EventBus eventBus))
            {
                throw new InvalidOperationException(
                    "EventBus is not registered.");
            }

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

            if (!ServiceContainer.TryGet<IPersistenceService>(
                    out IPersistenceService persistenceService))
            {
                throw new InvalidOperationException(
                    "PersistenceService is not registered.");
            }

            _stateMachine = new RuntimeStateMachine(eventBus);
            ServiceContainer.Register(_stateMachine);

            _runtimeInitializationService = new RuntimeInitializationService();
            ServiceContainer.Register<IRuntimeInitializationService>(
                _runtimeInitializationService);

            _runtimeRestoreContext = new RuntimeRestoreContext();
            ServiceContainer.Register(_runtimeRestoreContext);

            _runtimePipeline = new RuntimePipeline();

            _runtimePipeline.AddStep(
                new WorldDiscoveryStep(
                    worldDiscoveryService,
                    territoryService));

            _runtimePipeline.AddStep(
                new PersistenceLoadStep(
                    persistenceService,
                    _runtimeRestoreContext));

            ServiceContainer.Register(_runtimePipeline);

            _runtimePipelineCoordinator =
                new RuntimePipelineCoordinator(
                    _stateMachine,
                    _runtimePipeline);

            ServiceContainer.Register(_runtimePipelineCoordinator);

            eventBus.Subscribe<RuntimeStateChangedEvent>(
                _runtimePipelineCoordinator);

            ModLog.Info("Runtime module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("Runtime module shutdown.");
        }
    }
}