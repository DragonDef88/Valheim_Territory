using System.Globalization;
using System.Text;
using ClanTerritory.Features.Persistence.Models;

namespace ClanTerritory.Features.Persistence.Serialization
{
    internal sealed class JsonSerializerService
    {
        public string Serialize<T>(T data) where T : class
        {
            SaveFileModel saveFile = data as SaveFileModel;

            if (saveFile != null)
                return SerializeSaveFile(saveFile);

            return "{}";
        }

        public T Deserialize<T>(string json) where T : class, new()
        {
            return new T();
        }

        private string SerializeSaveFile(SaveFileModel saveFile)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("{");
            builder.AppendLine("  \"metadata\": {");
            builder.AppendLine("    \"schemaVersion\": " + saveFile.Metadata.SchemaVersion + ",");
            builder.AppendLine("    \"pluginVersion\": \"" + Escape(saveFile.Metadata.PluginVersion) + "\",");
            builder.AppendLine("    \"build\": \"" + Escape(saveFile.Metadata.Build) + "\",");
            builder.AppendLine("    \"createdBy\": \"" + Escape(saveFile.Metadata.CreatedBy) + "\",");
            builder.AppendLine("    \"saveId\": \"" + Escape(saveFile.Metadata.SaveId) + "\",");
            builder.AppendLine("    \"worldName\": \"" + Escape(saveFile.Metadata.WorldName) + "\",");
            builder.AppendLine("    \"savedAtUtc\": \"" + Escape(saveFile.Metadata.SavedAtUtc) + "\",");
            builder.AppendLine("    \"recordCount\": " + saveFile.Metadata.RecordCount);
            builder.AppendLine("  },");
            builder.AppendLine("  \"wards\": [");

            for (int i = 0; i < saveFile.Wards.Count; i++)
            {
                WardRecord ward = saveFile.Wards[i];

                builder.AppendLine("    {");
                builder.AppendLine("      \"wardId\": \"" + Escape(ward.WardId) + "\",");
                builder.AppendLine("      \"territory\": {");
                builder.AppendLine("        \"territoryId\": \"" + Escape(ward.Territory.TerritoryId) + "\",");
                builder.AppendLine("        \"ownerPlayerId\": " + ward.Territory.OwnerPlayerId + ",");
                builder.AppendLine("        \"ownerName\": \"" + Escape(ward.Territory.OwnerName) + "\",");
                builder.AppendLine("        \"x\": " + Float(ward.Territory.X) + ",");
                builder.AppendLine("        \"y\": " + Float(ward.Territory.Y) + ",");
                builder.AppendLine("        \"z\": " + Float(ward.Territory.Z) + ",");
                builder.AppendLine("        \"radius\": " + Float(ward.Territory.Radius));
                builder.AppendLine("      },");
                builder.AppendLine("      \"permissions\": null,");
                builder.AppendLine("      \"terrain\": null,");
                builder.AppendLine("      \"portals\": null,");
                builder.AppendLine("      \"extensions\": {}");
                builder.Append("    }");

                if (i < saveFile.Wards.Count - 1)
                    builder.AppendLine(",");
                else
                    builder.AppendLine();
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        private static string Float(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}