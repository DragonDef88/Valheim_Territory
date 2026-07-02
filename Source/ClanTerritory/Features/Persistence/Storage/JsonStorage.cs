using System.IO;
using ClanTerritory.Features.Persistence.Serialization;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Persistence.Storage
{
    internal sealed class JsonStorage
    {
        private readonly JsonSerializerService _serializer;

        public JsonStorage(JsonSerializerService serializer)
        {
            _serializer = serializer;
        }

        public void Save<T>(string path, T data) where T : class
        {
            string json = _serializer.Serialize(data);

            File.WriteAllText(path, json);

            ModLog.Info("JSON saved: " + path);
        }

        public T Load<T>(string path) where T : class, new()
        {
            if (!File.Exists(path))
            {
                ModLog.Warning("JSON file not found: " + path);
                return new T();
            }

            string json = File.ReadAllText(path);
            return _serializer.Deserialize<T>(json);
        }
    }
}