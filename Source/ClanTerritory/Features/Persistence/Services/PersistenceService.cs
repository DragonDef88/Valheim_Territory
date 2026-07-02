using ClanTerritory.Core;
using ClanTerritory.Features.Persistence.Mappers;
using ClanTerritory.Features.Persistence.Models;
using ClanTerritory.Features.Persistence.Storage;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Utils;
using SaveFileModelModel = ClanTerritory.Features.Persistence.Models.SaveFileModel;
using TerritoryEntity = ClanTerritory.Domain.Entities.Territory;
using ClanTerritory.Features.Persistence.FileSystem;

namespace ClanTerritory.Features.Persistence.Services
{
    internal sealed class PersistenceService : IPersistenceService
    {
        private readonly JsonStorage _storage;
        private readonly TerritoryMapper _territoryMapper;
        private readonly BackupStorage _backupStorage;
        private readonly PersistenceFileSystem _fileSystem;

        public PersistenceService(
               JsonStorage storage,
               TerritoryMapper territoryMapper,
               PersistenceFileSystem fileSystem,
               BackupStorage backupStorage)
        {
            _storage = storage;
            _territoryMapper = territoryMapper;
            _fileSystem = fileSystem;
            _backupStorage = backupStorage;
        }

        public void SaveNow()
        {
            SaveFileModel snapshot = CreateSnapshot();

            string path = _fileSystem.GetWorldSavePath(snapshot.Metadata.WorldName);
            _backupStorage.BackupIfExists(snapshot.Metadata.WorldName);
            _storage.Save(path, snapshot);

            ModLog.Info(
                "Persistence snapshot saved. Records: " +
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