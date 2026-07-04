using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Features.WorldDiscovery.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WorldDiscovery
{
    internal sealed class WorldDiscoveryModule : IInitializable, IDisposableModule
    {
        private IWorldDiscoveryService _service;

        public void Initialize()
        {
            _service = new WorldDiscoveryService();

            ServiceContainer.Register<IWorldDiscoveryService>(_service);

            ModLog.Info("World Discovery module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("World Discovery module shutdown.");
        }
    }
}