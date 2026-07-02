using ClanTerritory.Core;
using ClanTerritory.Features.Persistence.Mappers;
using ClanTerritory.Features.Persistence.Models;
using ClanTerritory.Features.Persistence.Storage;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Utils;
using SaveFileModelModel = ClanTerritory.Features.Persistence.Models.SaveFileModel;
using TerritoryEntity = ClanTerritory.Domain.Entities.Territory;

namespace ClanTerritory.Features.Persistence.Services
{
    internal sealed class PersistenceService : IPersistenceService
    {
        private readonly JsonStorage _storage;
        private readonly TerritoryMapper _territoryMapper;

        public PersistenceService(JsonStorage storage, TerritoryMapper territoryMapper)
        {
            _storage = storage;
            _territoryMapper = territoryMapper;
        }

        public void SaveNow()
        {
            SaveFileModelModel snapshot = CreateSnapshot();

            ModLog.Info(
                "Persistence snapshot created. Records: " +
                snapshot.Metadata.RecordCount
            );
        }

        public void LoadNow()
        {
            ModLog.Info("Persistence LoadNow prepared.");
        }

        private SaveFileModelModel CreateSnapshot()
        {
            SaveFileModelModel saveFile = new SaveFileModelModel();

            saveFile.Metadata.Version = 1;
            saveFile.Metadata.WorldName = "Unknown";
            saveFile.Metadata.PluginVersion = ModInfo.Version;
            saveFile.Metadata.Build = "Alpha";

            TerritoryRegistry territoryRegistry;

            if (ServiceContainer.TryGet<TerritoryRegistry>(out territoryRegistry))
            {
                foreach (TerritoryEntity territory in territoryRegistry.GetAll())
                {
                    WardRecord record = _territoryMapper.ToWardRecord(territory);

                    if (record != null)
                        saveFile.Wards.Add(record);
                }
            }

            saveFile.Metadata.RecordCount = saveFile.Wards.Count;

            return saveFile;
        }
    }
}