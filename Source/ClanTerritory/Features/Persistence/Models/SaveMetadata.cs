using System;

namespace ClanTerritory.Features.Persistence.Models
{
    internal sealed class SaveMetadata
    {
        public int Version { get; set; }
        public string WorldName { get; set; }
        public string SavedAtUtc { get; set; }
        public string PluginVersion { get; set; }
        public string Build { get; set; }
        public int RecordCount { get; set; }

        public SaveMetadata()
        {
            WorldName = string.Empty;
            SavedAtUtc = DateTime.UtcNow.ToString("o");
            PluginVersion = string.Empty;
            Build = "Alpha";
        }
    }
}