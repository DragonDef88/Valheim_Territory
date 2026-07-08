using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;

namespace ClanTerritory.Features.Territory.Events
{
    internal sealed class TerritoryRadiusChangedEvent : IEvent
    {
        public WardId WardId { get; private set; }

        public float Radius { get; private set; }

        public TerritoryRadiusChangedEvent(
            WardId wardId,
            float radius)
        {
            WardId = wardId;
            Radius = radius;
        }
    }
}