using System.Collections.Generic;
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
            if (string.IsNullOrEmpty(json))
                return new T();

            if (typeof(T) == typeof(SaveFileModel))
            {
                SaveFileModel saveFile = DeserializeSaveFile(json);
                return saveFile as T;
            }

            return new T();
        }

        private SaveFileModel DeserializeSaveFile(string json)
        {
            SaveFileModel saveFile = new SaveFileModel();

            string metadataJson = ExtractObject(json, "metadata");

            if (!string.IsNullOrEmpty(metadataJson))
            {
                saveFile.Metadata.SchemaVersion =
                    ReadInt(metadataJson, "schemaVersion", saveFile.Metadata.SchemaVersion);

                saveFile.Metadata.PluginVersion =
                    ReadString(metadataJson, "pluginVersion", saveFile.Metadata.PluginVersion);

                saveFile.Metadata.Build =
                    ReadString(metadataJson, "build", saveFile.Metadata.Build);

                saveFile.Metadata.CreatedBy =
                    ReadString(metadataJson, "createdBy", saveFile.Metadata.CreatedBy);

                saveFile.Metadata.SaveId =
                    ReadString(metadataJson, "saveId", saveFile.Metadata.SaveId);

                saveFile.Metadata.WorldName =
                    ReadString(metadataJson, "worldName", saveFile.Metadata.WorldName);

                saveFile.Metadata.SavedAtUtc =
                    ReadString(metadataJson, "savedAtUtc", saveFile.Metadata.SavedAtUtc);
            }

            string wardsJson = ExtractArray(json, "wards");

            if (!string.IsNullOrEmpty(wardsJson))
            {
                List<string> wardObjects = SplitObjects(wardsJson);

                foreach (string wardJson in wardObjects)
                {
                    WardRecord ward = DeserializeWard(wardJson);

                    if (ward != null)
                        saveFile.Wards.Add(ward);
                }
            }

            saveFile.Metadata.RecordCount = saveFile.Wards.Count;

            return saveFile;
        }

        private WardRecord DeserializeWard(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            WardRecord ward = new WardRecord();

            ward.WardId = ReadString(json, "wardId", string.Empty);

            string territoryJson = ExtractObject(json, "territory");

            if (!string.IsNullOrEmpty(territoryJson))
            {
                TerritoryRecord territory = new TerritoryRecord();

                territory.TerritoryId =
                    ReadString(territoryJson, "territoryId", territory.TerritoryId);

                territory.OwnerPlayerId =
                    ReadLong(territoryJson, "ownerPlayerId", territory.OwnerPlayerId);

                territory.OwnerName =
                    ReadString(territoryJson, "ownerName", territory.OwnerName);

                territory.X =
                    ReadFloat(territoryJson, "x", territory.X);

                territory.Y =
                    ReadFloat(territoryJson, "y", territory.Y);

                territory.Z =
                    ReadFloat(territoryJson, "z", territory.Z);

                territory.Radius =
                    ReadFloat(territoryJson, "radius", territory.Radius);

                ward.Territory = territory;
            }

            return ward;
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

        private static string ExtractObject(string json, string propertyName)
        {
            int propertyIndex = FindProperty(json, propertyName);

            if (propertyIndex < 0)
                return string.Empty;

            int braceStart = json.IndexOf('{', propertyIndex);

            if (braceStart < 0)
                return string.Empty;

            return ExtractBalanced(json, braceStart, '{', '}');
        }

        private static string ExtractArray(string json, string propertyName)
        {
            int propertyIndex = FindProperty(json, propertyName);

            if (propertyIndex < 0)
                return string.Empty;

            int arrayStart = json.IndexOf('[', propertyIndex);

            if (arrayStart < 0)
                return string.Empty;

            string array = ExtractBalanced(json, arrayStart, '[', ']');

            if (array.Length < 2)
                return string.Empty;

            return array.Substring(1, array.Length - 2);
        }

        private static int FindProperty(string json, string propertyName)
        {
            return json.IndexOf("\"" + propertyName + "\"");
        }

        private static string ExtractBalanced(
            string json,
            int startIndex,
            char open,
            char close)
        {
            int depth = 0;
            bool inString = false;
            bool escaped = false;

            for (int i = startIndex; i < json.Length; i++)
            {
                char current = json[i];

                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (current == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (inString)
                    continue;

                if (current == open)
                    depth++;

                if (current == close)
                    depth--;

                if (depth == 0)
                    return json.Substring(startIndex, i - startIndex + 1);
            }

            return string.Empty;
        }

        private static List<string> SplitObjects(string json)
        {
            List<string> objects = new List<string>();

            int index = 0;

            while (index < json.Length)
            {
                int objectStart = json.IndexOf('{', index);

                if (objectStart < 0)
                    break;

                string objectJson = ExtractBalanced(json, objectStart, '{', '}');

                if (string.IsNullOrEmpty(objectJson))
                    break;

                objects.Add(objectJson);

                index = objectStart + objectJson.Length;
            }

            return objects;
        }

        private static string ReadString(
            string json,
            string propertyName,
            string defaultValue)
        {
            int propertyIndex = FindProperty(json, propertyName);

            if (propertyIndex < 0)
                return defaultValue;

            int colonIndex = json.IndexOf(':', propertyIndex);

            if (colonIndex < 0)
                return defaultValue;

            int quoteStart = json.IndexOf('"', colonIndex + 1);

            if (quoteStart < 0)
                return defaultValue;

            StringBuilder builder = new StringBuilder();
            bool escaped = false;

            for (int i = quoteStart + 1; i < json.Length; i++)
            {
                char current = json[i];

                if (escaped)
                {
                    if (current == 'n')
                        builder.Append('\n');
                    else if (current == 'r')
                        builder.Append('\r');
                    else if (current == 't')
                        builder.Append('\t');
                    else
                        builder.Append(current);

                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (current == '"')
                    return builder.ToString();

                builder.Append(current);
            }

            return defaultValue;
        }

        private static int ReadInt(
            string json,
            string propertyName,
            int defaultValue)
        {
            string value = ReadNumber(json, propertyName);

            int result;

            if (int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out result))
                return result;

            return defaultValue;
        }

        private static long ReadLong(
            string json,
            string propertyName,
            long defaultValue)
        {
            string value = ReadNumber(json, propertyName);

            long result;

            if (long.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out result))
                return result;

            return defaultValue;
        }

        private static float ReadFloat(
            string json,
            string propertyName,
            float defaultValue)
        {
            string value = ReadNumber(json, propertyName);

            float result;

            if (float.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out result))
                return result;

            return defaultValue;
        }

        private static string ReadNumber(string json, string propertyName)
        {
            int propertyIndex = FindProperty(json, propertyName);

            if (propertyIndex < 0)
                return string.Empty;

            int colonIndex = json.IndexOf(':', propertyIndex);

            if (colonIndex < 0)
                return string.Empty;

            int start = colonIndex + 1;

            while (start < json.Length && char.IsWhiteSpace(json[start]))
                start++;

            int end = start;

            while (end < json.Length)
            {
                char current = json[end];

                if ((current >= '0' && current <= '9') ||
                    current == '-' ||
                    current == '+' ||
                    current == '.' ||
                    current == 'e' ||
                    current == 'E')
                {
                    end++;
                    continue;
                }

                break;
            }

            return json.Substring(start, end - start);
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