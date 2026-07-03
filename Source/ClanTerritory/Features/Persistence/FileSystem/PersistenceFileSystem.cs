using System;
using System.IO;
using BepInEx;

namespace ClanTerritory.Features.Persistence.FileSystem
{
    internal sealed class PersistenceFileSystem
    {
        private const string RootFolder = "ClanTerritory";
        private const string WorldsFolder = "worlds";
        private const string BackupsFolder = "backups";

        public string RootDirectory
        {
            get
            {
                return Path.Combine(Paths.ConfigPath, RootFolder);
            }
        }

        public string WorldsDirectory
        {
            get
            {
                return Path.Combine(RootDirectory, WorldsFolder);
            }
        }

        public string BackupsDirectory
        {
            get
            {
                return Path.Combine(RootDirectory, BackupsFolder);
            }
        }

        public void EnsureDirectories()
        {
            Directory.CreateDirectory(RootDirectory);
            Directory.CreateDirectory(WorldsDirectory);
            Directory.CreateDirectory(BackupsDirectory);
        }

        public string GetWorldSavePath(string worldName)
        {
            if (string.IsNullOrWhiteSpace(worldName))
                worldName = "Unknown";

            return Path.Combine(
                WorldsDirectory,
                worldName + ".json");
        }

        public string GetBackupPath(string worldName)
        {
            if (string.IsNullOrWhiteSpace(worldName))
                worldName = "Unknown";

            string timestamp =
                DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

            return Path.Combine(
                BackupsDirectory,
                worldName + "_" + timestamp + ".json");
        }
    }
}