using ClanTerritory.Features.Persistence.Storage;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Persistence.Services
{
    internal sealed class PersistenceService : IPersistenceService
    {
        private readonly JsonStorage _storage;

        public PersistenceService(JsonStorage storage)
        {
            _storage = storage;
        }

        public void SaveNow()
        {
            ModLog.Info("Persistence SaveNow prepared.");
        }

        public void LoadNow()
        {
            ModLog.Info("Persistence LoadNow prepared.");
        }
    }
}