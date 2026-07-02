using ClanTerritory.Utils;

namespace ClanTerritory.Features.Persistence.Storage
{
    internal sealed class JsonStorage
    {
        public void Save<T>(string path, T data) where T : class
        {
            ModLog.Debug("JsonStorage.Save prepared for: " + path);
        }

        public T Load<T>(string path) where T : class, new()
        {
            ModLog.Debug("JsonStorage.Load prepared for: " + path);
            return new T();
        }
    }
}