using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Events;

namespace ClanTerritory.Features.TerritoryNaming.Events
{
    internal sealed class TerritoryRenamedEvent : IEvent
    {
        public WardId WardId { get; private set; }

        public string TerritoryName { get; private set; }

        public TerritoryRenamedEvent(
            WardId wardId,
            string territoryName)
        {
            WardId = wardId;
            TerritoryName = territoryName;
        }
    }
}