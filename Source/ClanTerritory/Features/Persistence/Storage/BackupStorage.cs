using System;
using System.IO;
using ClanTerritory.Features.Persistence.FileSystem;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Persistence.Storage
{
    internal sealed class BackupStorage
    {
        private readonly PersistenceFileSystem _fileSystem;

        public BackupStorage(PersistenceFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void BackupIfExists(string worldName)
        {
            string sourcePath = _fileSystem.GetWorldSavePath(worldName);

            if (!File.Exists(sourcePath))
                return;

            try
            {
                string backupPath = _fileSystem.GetBackupPath(worldName);

                File.Copy(sourcePath, backupPath, true);

                ModLog.Info("Save backup created: " + backupPath);
            }
            catch (Exception ex)
            {
                ModLog.Warning("Save backup failed: " + ex.Message);
            }
        }
    }
}