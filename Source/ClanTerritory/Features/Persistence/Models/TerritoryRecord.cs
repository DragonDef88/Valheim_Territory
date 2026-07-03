namespace ClanTerritory.Features.Persistence.Models
{
    internal sealed class TerritoryRecord
    {
        public string TerritoryId { get; set; }
        public long OwnerPlayerId { get; set; }
        public string OwnerName { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float Radius { get; set; }

        public TerritoryRecord()
        {
            TerritoryId = string.Empty;
            OwnerName = "Unknown";
        }
    }
}