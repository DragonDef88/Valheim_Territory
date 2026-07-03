using System.Collections.Generic;

namespace ClanTerritory.Features.Persistence.Models
{
    internal sealed class WardRecord
    {
        public string WardId { get; set; }
        public TerritoryRecord Territory { get; set; }
        public Dictionary<string, string> Extensions { get; set; }

        public WardRecord()
        {
            WardId = string.Empty;
            Extensions = new Dictionary<string, string>();
        }
    }
}