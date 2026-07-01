using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Features.WardDetection.Registry;
using ClanTerritory.Features.WardDetection.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardDetection
{
    internal sealed class WardDetectionModule : IInitializable, IDisposableModule
    {
        private WardRegistry _wardRegistry;
        private WardService _wardService;

        public void Initialize()
        {
            _wardRegistry = new WardRegistry();
            _wardService = new WardService(_wardRegistry);

            ServiceContainer.Register<WardRegistry>(_wardRegistry);
            ServiceContainer.Register<IWardService>(_wardService);

            ModLog.Info("Ward Detection module initialized.");
        }

        public void Shutdown()
        {
            if (_wardRegistry != null)
                _wardRegistry.Clear();

            ModLog.Info("Ward Detection module shutdown.");
        }
    }
}