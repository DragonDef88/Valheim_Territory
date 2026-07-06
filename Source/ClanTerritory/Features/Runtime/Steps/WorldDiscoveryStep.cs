using ClanTerritory.Features.WorldDiscovery.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Pipeline.Steps
{
    internal sealed class WorldDiscoveryStep : IRuntimeStep
    {
        private readonly IWorldDiscoveryService _worldDiscoveryService;

        public WorldDiscoveryStep(
            IWorldDiscoveryService worldDiscoveryService)
        {
            _worldDiscoveryService = worldDiscoveryService;
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
            ModLog.Info("[Runtime Pipeline] Starting world discovery scan.");

            var wards = _worldDiscoveryService.Discover();

            ModLog.Info(
                "[Runtime Pipeline] World discovery scan completed. Wards observed: " +
                wards.Count);
        }
    }
}