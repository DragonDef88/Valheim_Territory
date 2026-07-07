using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Features.WardInteraction.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardInteraction
{
    internal sealed class WardInteractionModule : IInitializable, IDisposableModule
    {
        private WardInteractionService _wardInteractionService;

        public void Initialize()
        {
            IRuntimeRegistry runtimeRegistry = ServiceContainer.Get<IRuntimeRegistry>();
            EventBus eventBus = ServiceContainer.Get<EventBus>();

            _wardInteractionService = new WardInteractionService(
                runtimeRegistry,
                eventBus);

            ServiceContainer.Register<IWardInteractionService>(_wardInteractionService);

            ModLog.Info("Ward Interaction module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("Ward Interaction module shutdown.");
        }
    }
}