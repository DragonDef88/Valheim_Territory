using System;

namespace ClanTerritory.Features.Persistence.Models
{
    internal sealed class SaveMetadata
    {
        public int SchemaVersion { get; set; }

        public string PluginVersion { get; set; }

        public string Build { get; set; }

        public string CreatedBy { get; set; }

        public string SaveId { get; set; }

        public string WorldName { get; set; }

        public string SavedAtUtc { get; set; }

        public int RecordCount { get; set; }

        public SaveMetadata()
        {
            SchemaVersion = 1;
            PluginVersion = string.Empty;
            Build = "Alpha";
            CreatedBy = "Clan Territory";
            SaveId = Guid.NewGuid().ToString();
            WorldName = "Unknown";
            SavedAtUtc = DateTime.UtcNow.ToString("o");
            RecordCount = 0;
        }
    }
}