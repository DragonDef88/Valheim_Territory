using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Registry;

namespace ClanTerritory.Features.WardInteraction
{
    internal sealed class WardInteractionRequestedEvent : IEvent
    {
        public WardId WardId { get; private set; }

        public RuntimeWard RuntimeWard { get; private set; }

        public PrivateArea PrivateArea { get; private set; }

        public Player Player { get; private set; }

        public WardInteractionRequestedEvent(
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