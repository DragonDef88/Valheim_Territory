using ClanTerritory.Features.Persistence.Models;

namespace ClanTerritory.Features.Runtime.Restore
{
    internal sealed class RuntimeRestoreContext
    {
        public SaveFileModel SaveFile { get; private set; }

        public RuntimeRestoreSnapshot Snapshot { get; private set; }

        public void SetSaveFile(SaveFileModel saveFile)
        {
            SaveFile = saveFile;
        }

        public void SetSnapshot(RuntimeRestoreSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public void Clear()
        {
            SaveFile = null;
            Snapshot = null;
        }
    }
}