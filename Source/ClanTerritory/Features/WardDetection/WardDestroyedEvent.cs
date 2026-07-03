using ClanTerritory.Events;
using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Features.WardDetection
{
    internal sealed class WardDestroyedEvent : IEvent
    {
        public WardId WardId { get; private set; }

        public WardDestroyedEvent(WardId wardId)
        {
            WardId = wardId;
        }
    }
}