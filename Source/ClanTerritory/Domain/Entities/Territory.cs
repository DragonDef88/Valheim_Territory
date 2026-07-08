using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Domain.ValueObjects;

namespace ClanTerritory.Domain.Entities
{
    internal sealed class Territory
    {
        public TerritoryId Id { get; private set; }
        public WardId WardId { get; private set; }
        public OwnerInfo Owner { get; private set; }
        public WorldPosition Position { get; private set; }
        public TerritoryRadius Radius { get; private set; }

        public Territory(
            TerritoryId id,
            WardId wardId,
            OwnerInfo owner,
            WorldPosition position,
            TerritoryRadius radius)
        {
            Id = id;
            WardId = wardId;
            Owner = owner;
            Position = position;
            Radius = radius;
        }

        public void SetRadius(TerritoryRadius radius)
        {
            Radius = radius;
        }

        public bool Contains(WorldPosition position)
        {
            return Position.DistanceTo(position) <= Radius.Value;
        }

        public bool Overlaps(Territory other)
        {
            if (other == null)
                return false;

            return Position.DistanceTo(other.Position) < Radius.Value + other.Radius.Value;
        }
    }
}
