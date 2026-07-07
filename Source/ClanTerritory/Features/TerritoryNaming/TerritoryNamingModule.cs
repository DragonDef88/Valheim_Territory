using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.TerritoryNaming.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.TerritoryNaming
{
    internal sealed class TerritoryNamingModule :
        IInitializable,
        IDisposableModule
    {
        private TerritoryNamingService _territoryNamingService;

        public void Initialize()
        {
            EventBus eventBus =
                ServiceContainer.Get<EventBus>();

            _territoryNamingService =
                new TerritoryNamingService(eventBus);

            ServiceContainer.Register<ITerritoryNamingService>(_territoryNamingService);

            ModLog.Info("Territory Naming module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("Territory Naming module shutdown.");

            _territoryNamingService = null;
        }
    }
}