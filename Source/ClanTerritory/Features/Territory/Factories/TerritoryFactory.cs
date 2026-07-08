using ClanTerritory.Domain.Entities;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Domain.ValueObjects;
using ClanTerritory.Features.WardDetection.Models;

namespace ClanTerritory.Features.Territory.Factories
{
    internal sealed class TerritoryFactory
    {
        public Domain.Entities.Territory CreateFromWard(WardModel ward)
        {
            WardId wardId = new WardId(ward.Id);
            TerritoryId territoryId = new TerritoryId("territory_" + ward.Id);

            OwnerInfo owner = new OwnerInfo(
                new PlayerId(ward.OwnerId),
                ward.OwnerName
            );

            WorldPosition position = new WorldPosition(
                ward.Position.x,
                ward.Position.y,
                ward.Position.z
            );

            TerritoryRadius radius = new TerritoryRadius(ward.Radius);

            return new Domain.Entities.Territory(
                territoryId,
                wardId,
                owner,
                position,
                radius
            );
        }
    }
}
