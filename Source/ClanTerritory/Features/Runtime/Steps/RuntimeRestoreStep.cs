using ClanTerritory.Features.Runtime.Restore;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Pipeline.Steps
{
    internal sealed class RuntimeRestoreStep : IRuntimeStep
    {
        private readonly RuntimeRestoreContext _restoreContext;
        private readonly RuntimeRestoreMapper _restoreMapper;
        private readonly IRuntimeRegistryRestoreService _registryRestoreService;

        public RuntimeRestoreStep(
            RuntimeRestoreContext restoreContext,
            RuntimeRestoreMapper restoreMapper,
            IRuntimeRegistryRestoreService registryRestoreService)
        {
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