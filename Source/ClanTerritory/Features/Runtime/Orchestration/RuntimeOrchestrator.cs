using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Events;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Features.WorldDiscovery.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Orchestration
{
    internal sealed class RuntimeOrchestrator :
        IRuntimeOrchestrator,
        IEventHandler<RuntimeStateChangedEvent>
    {
        private readonly RuntimeStateMachine _stateMachine;
        private readonly IWorldDiscoveryService _worldDiscoveryService;
        private readonly ITerritoryService _territoryService;

        public RuntimeOrchestrator(
    RuntimeStateMachine stateMachine,
    IWorldDiscoveryService worldDiscoveryService,
    ITerritoryService territoryService)
        {
            _stateMachine = stateMachine;
            _worldDiscoveryService = worldDiscoveryService;
            _territoryService = territoryService;
        }

        public void Handle(RuntimeStateChangedEvent eventData)
        {
            if (eventData == null)
                return;

            if (eventData.CurrentState == RuntimeState.WorldLoaded)
            {
                RunWorldDiscovery();
            }
        }

        private void RunWorldDiscovery()
        {
            ModLog.Info("[Runtime] Starting world discovery.");

            var wards = _worldDiscoveryService.Discover();

            foreach (WardModel ward in wards)
            {
                _territoryService.CreateTerritoryFromWard(ward);
            }

            ModLog.Info(
                "[Runtime] World discovery completed. Wards: " +
                wards.Count);

            _stateMachine.SetState(
                RuntimeState.DiscoveryCompleted);
        }
    }
}