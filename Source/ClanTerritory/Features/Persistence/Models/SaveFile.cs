using System.Collections.Generic;

namespace ClanTerritory.Features.Persistence.Models
{
    internal sealed class SaveFileModel
    {
        public SaveMetadata Metadata { get; set; }
        public List<WardRecord> Wards { get; set; }

        public SaveFileModel()
        {
            Metadata = new SaveMetadata();
            Wards = new List<WardRecord>();
        }
    }
}