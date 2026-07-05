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
        private readonly IWorldInfoService _worldInfoService;

        private readonly HashSet<string> _deletedWardIds =
            new HashSet<string>();

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

            SaveFileModel saveFile = MergeWithExisting(path, snapshot);

            _backupStorage.BackupIfExists(snapshot.Metadata.WorldName);

            _storage.Save(path, saveFile);

            _deletedWardIds.Clear();

            ModLog.Info(
                "Persistence merge-save completed. Records: " +
                saveFile.Metadata.RecordCount);
        }

        public void LoadNow()
        {
            LoadSnapshot();
        }

        public SaveFileModel LoadSnapshot()
        {
            string worldName = _worldInfoService.GetWorldName();
            string path = _fileSystem.GetWorldSavePath(worldName);

            SaveFileModel saveFile = _storage.Load<SaveFileModel>(path);

            if (saveFile == null)
                saveFile = new SaveFileModel();

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

            _deletedWardIds.Add(wardId);

            ModLog.Info("Persistence delete tracked for ward: " + wardId);
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

        private SaveFileModel MergeWithExisting(string path, SaveFileModel snapshot)
        {
            SaveFileModel existing = _storage.Load<SaveFileModel>(path);
            SaveFileModel merged = new SaveFileModel();

            merged.Metadata = snapshot.Metadata;

            List<WardRecord> records = new List<WardRecord>();
            Dictionary<string, int> indexByKey = new Dictionary<string, int>();

            AddOrReplaceRecords(existing.Wards, records, indexByKey, true);
            AddOrReplaceRecords(snapshot.Wards, records, indexByKey, false);

            merged.Wards = records;
            merged.Metadata.RecordCount = merged.Wards.Count;

            return merged;
        }

        private void AddOrReplaceRecords(
            IEnumerable<WardRecord> source,
            List<WardRecord> records,
            Dictionary<string, int> indexByKey,
            bool skipDeleted)
        {
            if (source == null)
                return;

            foreach (WardRecord record in source)
            {
                if (record == null)
                    continue;

                string key = GetRecordKey(record);

                if (skipDeleted && IsDeleted(record))
                    continue;

                if (string.IsNullOrEmpty(key))
                {
                    records.Add(record);
                    continue;
                }

                int index;

                if (indexByKey.TryGetValue(key, out index))
                {
                    records[index] = record;
                    continue;
                }

                indexByKey.Add(key, records.Count);
                records.Add(record);
            }
        }

        private bool IsDeleted(WardRecord record)
        {
            if (record == null)
                return false;

            if (!string.IsNullOrEmpty(record.WardId) &&
                _deletedWardIds.Contains(record.WardId))
                return true;

            return false;
        }

        private static string GetRecordKey(WardRecord record)
        {
            if (record == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(record.WardId))
                return record.WardId;

            if (record.Territory != null &&
                !string.IsNullOrEmpty(record.Territory.TerritoryId))
                return record.Territory.TerritoryId;

            return string.Empty;
        }
    }
}