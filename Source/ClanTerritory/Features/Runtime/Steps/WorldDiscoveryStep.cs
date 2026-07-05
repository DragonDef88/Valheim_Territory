using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Features.WorldDiscovery.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Pipeline.Steps
{
    internal sealed class WorldDiscoveryStep : IRuntimeStep
    {
        private readonly IWorldDiscoveryService _worldDiscoveryService;
        private readonly ITerritoryService _territoryService;

        public WorldDiscoveryStep(
            IWorldDiscoveryService worldDiscoveryService,
            ITerritoryService territoryService)
        {
            _worldDiscoveryService = worldDiscoveryService;
            _territoryService = territoryService;
        }

        public RuntimeState InputState
        {
            get { return RuntimeState.WorldLoaded; }
        }

        public RuntimeState OutputState
        {
            get { return RuntimeState.DiscoveryCompleted; }
        }

        public void Execute()
        {
            ModLog.Info("[Runtime Pipeline] Starting world discovery.");

            var wards = _worldDiscoveryService.Discover();

            foreach (WardModel ward in wards)
            {
                _territoryService.CreateTerritoryFromWard(ward);
            }

            ModLog.Info(
                "[Runtime Pipeline] World discovery completed. Wards: " +
                wards.Count);
        }
    }
}