using ClanTerritory.Utils;

namespace ClanTerritory.Features.WorldSynchronization.Services
{
    internal sealed class WorldSynchronizationService : IWorldSynchronizationService
    {
        public void Synchronize()
        {
            ModLog.Info("World synchronization prepared.");
        }
    }
}