using System.Collections.Generic;

namespace ClanTerritory.Features.Persistence.Models
{
    internal sealed class SaveFile
    {
        public SaveMetadata Metadata { get; set; }
        public List<WardRecord> Wards { get; set; }

        public SaveFile()
        {
            Metadata = new SaveMetadata();
            Wards = new List<WardRecord>();
        }
    }
}