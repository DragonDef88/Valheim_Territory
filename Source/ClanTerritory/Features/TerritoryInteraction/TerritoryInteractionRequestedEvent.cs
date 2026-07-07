using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;

namespace ClanTerritory.Features.TerritoryInteraction
{
    internal sealed class TerritoryInteractionRequestedEvent : IEvent
    {
        public WardId WardId { get; private set; }

        public RuntimeWard RuntimeWard { get; private set; }

        public PrivateArea PrivateArea { get; private set; }

        public Player Player { get; private set; }

        public TerritoryInteractionRequestedEvent(
            WardId wardId,
            RuntimeWard runtimeWard,
            PrivateArea privateArea,
            Player player)
        {
            WardId = wardId;
            RuntimeWard = runtimeWard;
            PrivateArea = privateArea;
            Player = player;
        }
    }
}