using ClanTerritory.Features.Persistence.Models;
using ClanTerritory.Features.Persistence.Services;
using ClanTerritory.Features.Runtime.Restore;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Pipeline.Steps
{
    internal sealed class PersistenceLoadStep : IRuntimeStep
    {
        private readonly IPersistenceService _persistenceService;
        private readonly RuntimeRestoreContext _restoreContext;

        public PersistenceLoadStep(
            IPersistenceService persistenceService,
            RuntimeRestoreContext restoreContext)
        {
            _persistenceService = persistenceService;
            _restoreContext = restoreContext;
        }

        public RuntimeState InputState
        {
            get { return RuntimeState.DiscoveryCompleted; }
        }

        public RuntimeState OutputState
        {
            get { return RuntimeState.RegistrySynchronized; }
        }

        public void Execute()
        {
            ModLog.Info("[Runtime Pipeline] Loading persistence snapshot.");

            SaveFileModel saveFile = _persistenceService.LoadSnapshot();

            _restoreContext.SetSaveFile(saveFile);

            ModLog.Info("[Runtime Pipeline] Persistence snapshot stored.");
        }
    }
}