using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.TerritoryInteraction.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.TerritoryInteraction
{
    internal sealed class TerritoryInteractionModule :
        IInitializable,
        IDisposableModule
    {
        private TerritoryInteractionService _territoryInteractionService;

        public void Initialize()
        {
            IRuntimeRegistry runtimeRegistry = ServiceContainer.Get<IRuntimeRegistry>();
            EventBus eventBus = ServiceContainer.Get<EventBus>();

            _territoryInteractionService = new TerritoryInteractionService(
                runtimeRegistry,
                eventBus);

            ServiceContainer.Register<ITerritoryInteractionService>(_territoryInteractionService);

            ModLog.Info("Territory Interaction module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("Territory Interaction module shutdown.");

            _territoryInteractionService = null;
        }
    }
}