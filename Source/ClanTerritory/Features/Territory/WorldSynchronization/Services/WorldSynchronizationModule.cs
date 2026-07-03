using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Features.WorldSynchronization.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WorldSynchronization
{
    internal sealed class WorldSynchronizationModule : IInitializable, IDisposableModule
    {
        private WorldSynchronizationService _service;

        public void Initialize()
        {
            _service = new WorldSynchronizationService();

            ServiceContainer.Register<IWorldSynchronizationService>(_service);

            ModLog.Info("World Synchronization module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("World Synchronization module shutdown.");
        }
    }
}