using TerritoryEntity = ClanTerritory.Domain.Entities.Territory;
using ClanTerritory.Features.Persistence.Models;

namespace ClanTerritory.Features.Persistence.Mappers
{
    internal sealed class TerritoryMapper
    {
        public WardRecord ToWardRecord(TerritoryEntity territory)
        {
            if (territory == null)
                return null;

            return new WardRecord
            {
                WardId = territory.WardId.ToString(),
                Territory = new TerritoryRecord
                {
                    TerritoryId = territory.Id.ToString(),
                    OwnerPlayerId = long.Parse(territory.Owner.PlayerId.ToString()),
                    OwnerName = territory.Owner.DisplayName,
                    X = territory.Position.X,
                    Y = territory.Position.Y,
                    Z = territory.Position.Z,
                    Radius = territory.Radius.Value
                }
            };
        }
    }
}