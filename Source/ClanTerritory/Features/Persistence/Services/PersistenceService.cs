using System;
using System.Collections.Generic;
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
        private readonly PersistenceWriteGate _writeGate;
        private readonly IWorldInfoService _worldInfoService;

        public PersistenceService(
            JsonStorage storage,
            TerritoryMapper territoryMapper,
            PersistenceFileSystem fileSystem,
            BackupStorage backupStorage,
            PersistenceWriteGate writeGate,
            IWorldInfoService worldInfoService)
        {
            _storage = storage;
            _territoryMapper = territoryMapper;
            _fileSystem = fileSystem;
            _backupStorage = backupStorage;
            _writeGate = writeGate;
            _worldInfoService = worldInfoService;
        }

        public void SaveNow()
        {
            if (!_writeGate.CanWrite)
            {
                ModLog.Info("[Persistence] Save skipped: write gate is closed.");
                return;
            }

            SaveFileModel snapshot = CreateSnapshot();

            string path =
                _fileSystem.GetWorldSavePath(snapshot.Metadata.WorldName);

            _backupStorage.BackupIfExists(snapshot.Metadata.WorldName);
            _storage.Save(path, snapshot);

            ModLog.Info(
                "[Persistence] Snapshot saved from runtime state. Records: " +
                snapshot.Metadata.RecordCount);
        }

        public void LoadNow()
        {
            LoadSnapshot();
        }

        public SaveFileModel LoadSnapshot()
        {
            string worldName = _worldInfoService.GetWorldName();
            string path = _fileSystem.GetWorldSavePath(worldName);

            ModLog.Info("[Persistence] Loading snapshot.");
            ModLog.Info("[Persistence] World name: " + worldName);
            ModLog.Info("[Persistence] Snapshot path: " + path);
            ModLog.Info("[Persistence] Snapshot file exists: " + System.IO.File.Exists(path));

            SaveFileModel saveFile = _storage.Load<SaveFileModel>(path);

            if (saveFile == null)
            {
                ModLog.Info("[Persistence] Storage returned null snapshot.");
                saveFile = new SaveFileModel();
            }

            if (saveFile.Metadata == null)
                saveFile.Metadata = new SaveMetadata();

            if (saveFile.Wards == null)
                saveFile.Wards = new List<WardRecord>();

            saveFile.Metadata.RecordCount = saveFile.Wards.Count;

            ModLog.Info(
                "Persistence snapshot loaded. Records: " +
                saveFile.Metadata.RecordCount);

            return saveFile;
        }

        public void MarkWardDeleted(string wardId)
        {
            if (string.IsNullOrEmpty(wardId))
                return;

            ModLog.Info(
                "[Persistence] Ward delete observed: " +
                wardId +
                ". Snapshot save will use current runtime state.");
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