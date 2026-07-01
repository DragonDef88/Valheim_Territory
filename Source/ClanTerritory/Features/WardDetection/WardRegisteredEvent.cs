using ClanTerritory.Events;
using ClanTerritory.Features.WardDetection.Models;

namespace ClanTerritory.Features.WardDetection
{
    internal sealed class WardRegisteredEvent : IEvent
    {
        public WardModel Ward { get; private set; }

        public WardRegisteredEvent(WardModel ward)
        {
            Ward = ward;
        }
    }
}