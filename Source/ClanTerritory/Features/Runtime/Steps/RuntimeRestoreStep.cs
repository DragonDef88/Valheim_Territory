using ClanTerritory.Features.Runtime.Restore;
using ClanTerritory.Utils;
using ClanTerritory.Features.Persistence.Services;

namespace ClanTerritory.Features.Runtime.Pipeline.Steps
{
    internal sealed class RuntimeRestoreStep : IRuntimeStep
    {
        private readonly RuntimeRestoreContext _restoreContext;
        private readonly RuntimeRestoreMapper _restoreMapper;
        private readonly PersistenceWriteGate _writeGate;
        private readonly IRuntimeRegistryRestoreService _registryRestoreService;

        public RuntimeRestoreStep(
            RuntimeRestoreContext restoreContext,
            RuntimeRestoreMapper restoreMapper,
            IRuntimeRegistryRestoreService registryRestoreService)
        {
            _writeGate.Open();
            ModLog.Info("[Persistence] Write gate opened after runtime restore.");
            _restoreContext = restoreContext;
            _restoreMapper = restoreMapper;
            _registryRestoreService = registryRestoreService;
        }

        public RuntimeState InputState
        {
            get { return RuntimeState.RegistrySynchronized; }
        }

        public RuntimeState OutputState
        {
            get { return RuntimeState.GameplayReady; }
        }

        public void Execute()
        {
            ModLog.Info("[Runtime Pipeline] Building runtime restore snapshot.");

            RuntimeRestoreSnapshot snapshot =
                _restoreMapper.ToRuntimeSnapshot(_restoreContext.SaveFile);

            _restoreContext.SetSnapshot(snapshot);

            _registryRestoreService.Restore(snapshot);

            int wardCount = 0;

            if (snapshot != null && snapshot.Wards != null)
                wardCount = snapshot.Wards.Count;

            ModLog.Info(
                "[Runtime Pipeline] Runtime restore completed. Wards: " +
                wardCount);
        }
    }
}