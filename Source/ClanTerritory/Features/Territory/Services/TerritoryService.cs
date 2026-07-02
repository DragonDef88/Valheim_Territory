using ClanTerritory.Events;
using ClanTerritory.Features.Territory.Factories;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.WardDetection;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Utils;
using TerritoryEntity = ClanTerritory.Domain.Entities.Territory;
using ClanTerritory.Core;
using ClanTerritory.Features.Persistence.Services;

namespace ClanTerritory.Features.Territory.Services
{
    internal sealed class TerritoryService : ITerritoryService, IEventHandler<WardRegisteredEvent>
    {
        private readonly TerritoryRegistry _registry;
        private readonly TerritoryFactory _factory;

        public TerritoryService(TerritoryRegistry registry, TerritoryFactory factory)
        {
            _registry = registry;
            _factory = factory;
        }

        public void Handle(WardRegisteredEvent eventData)
        {
            if (eventData == null)
                return;

            CreateTerritoryFromWard(eventData.Ward);
        }

        public void CreateTerritoryFromWard(WardModel ward)
        {
            if (ward == null)
                return;

            TerritoryEntity territory = _factory.CreateFromWard(ward);

            TerritoryEntity intersecting = _registry.FindIntersecting(territory);

            if (intersecting != null)
            {
                ModLog.Warning(
                    "Territory creation blocked: overlap with " +
                    intersecting.Id
                );

                return;
            }

            if (_registry.Register(territory))
            {
                ModLog.Info(
                    "Territory created: " +
                    territory.Id +
                    ", owner: " +
                    territory.Owner.DisplayName +
                    ", radius: " +
                    territory.Radius.Value +
                    ", total: " +
                    _registry.Count
                );
                IPersistenceService persistenceService;

                if (ServiceContainer.TryGet<IPersistenceService>(out persistenceService))
                    persistenceService.SaveNow();
            }
        }
    }
}