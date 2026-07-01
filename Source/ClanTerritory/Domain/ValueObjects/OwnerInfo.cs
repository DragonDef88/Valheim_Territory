using ClanTerritory.Domain.Identifiers;

namespace ClanTerritory.Domain.ValueObjects
{
    internal sealed class OwnerInfo
    {
        public PlayerId PlayerId { get; private set; }
        public string DisplayName { get; private set; }

        public OwnerInfo(PlayerId playerId, string displayName)
        {
            PlayerId = playerId;
            DisplayName = string.IsNullOrEmpty(displayName) ? "Unknown" : displayName;
        }
    }
}