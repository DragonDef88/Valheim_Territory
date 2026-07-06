using ClanTerritory.Features.Persistence.Services;
using ClanTerritory.Features.Runtime.Restore;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Pipeline.Steps
{
    internal sealed class RuntimeRestoreStep : IRuntimeStep
    {
        private readonly RuntimeRestoreContext _restoreContext;
        private readonly RuntimeRestoreMapper _restoreMapper;
        private readonly IRuntimeRegistryRestoreService _registryRestoreService;
        private readonly PersistenceWriteGate _writeGate;

        public RuntimeRestoreStep(
            RuntimeRestoreContext restoreContext,
            RuntimeRestoreMapper restoreMapper,
            IRuntimeRegistryRestoreService registryRestoreService,
            PersistenceWriteGate writeGate)
        {
            _restoreContext = restoreContext;
            _restoreMapper = restoreMapper;
            _registryRestoreService = registryRestoreService;
            _writeGate = writeGate;
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

            _writeGate.Open();
            ModLog.Info("[Persistence] Write gate opened after runtime restore.");

            int wardCount = 0;

            if (snapshot != null && snapshot.Wards != null)
                wardCount = snapshot.Wards.Count;

            ModLog.Info(
                "[Runtime Pipeline] Runtime restore completed. Wards: " +
                wardCount);
        }
    }
}