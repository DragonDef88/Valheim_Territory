using System;
using ClanTerritory.Core;
using ClanTerritory.Features.Persistence.FileSystem;
using ClanTerritory.Features.Persistence.Mappers;
using ClanTerritory.Features.Persistence.Models;
using ClanTerritory.Features.Persistence.Storage;
using ClanTerritory.Features.Territory.Registry;
using ClanTerritory.Features.World.Services;
using ClanTerritory.Utils;

using TerritoryEntity = ClanTerritory.Domain.Entities.Territory;

namespace ClanTerritory.Features.Persistence.Services
{
    internal sealed class PersistenceService : IPersistenceService
    {
        private readonly JsonStorage _storage;
        private readonly TerritoryMapper _territoryMapper;
        private readonly PersistenceFileSystem _fileSystem;
        private readonly BackupStorage _backupStorage;
        private readonly IWorldInfoService _worldInfoService;

        public PersistenceService(
            JsonStorage storage,
            TerritoryMapper territoryMapper,
            PersistenceFileSystem fileSystem,
            BackupStorage backupStorage,
            IWorldInfoService worldInfoService)
        {
            _storage = storage;
            _territoryMapper = territoryMapper;
            _fileSystem = fileSystem;
            _backupStorage = backupStorage;
            _worldInfoService = worldInfoService;
        }

        public void SaveNow()
        {
            SaveFileModel snapshot = CreateSnapshot();

            string path = _fileSystem.GetWorldSavePath(snapshot.Metadata.WorldName);

            _backupStorage.BackupIfExists(snapshot.Metadata.WorldName);

            _storage.Save(path, snapshot);

            ModLog.Info(
                "Persistence snapshot saved. Records: " +
                snapshot.Metadata.RecordCount);
        }

        public void LoadNow()
        {
            ModLog.Info("Persistence LoadNow prepared.");
        }

        private SaveFileModel CreateSnapshot()
        {
            SaveFileModel saveFile = new SaveFileModel();

            saveFile.Metadata.SchemaVersion = 1;
            saveFile.Metadata.PluginVersion = ModInfo.Version;
            saveFile.Metadata.Build = "Alpha";
            saveFile.Metadata.CreatedBy = "Clan Territory";
            saveFile.Metadata.WorldName = _worldInfoService.GetWorldName();
            saveFile.Metadata.SavedAtUtc = DateTime.UtcNow.ToString("o");

            TerritoryRegistry territoryRegistry;

            if (ServiceContainer.TryGet(out territoryRegistry))
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