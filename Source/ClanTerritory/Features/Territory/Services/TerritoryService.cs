using ClanTerritory.Events;
using ClanTerritory.Features.Territory.Factories;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.WardDetection;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Utils;
using TerritoryEntity = ClanTerritory.Domain.Entities.Territory;

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

            if (HasOverlap(territory))
            {
                ModLog.Warning("Territory creation blocked: overlap detected.");
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
                    territory.Radius.Value
                );
            }
        }

        private bool HasOverlap(TerritoryEntity territory)
        {
            foreach (TerritoryEntity existing in _registry.All)
            {
                if (existing.Overlaps(territory))
                    return true;
            }

            return false;
        }
    }
}