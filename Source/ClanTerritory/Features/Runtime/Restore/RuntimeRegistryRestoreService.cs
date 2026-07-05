using ClanTerritory.Features.Runtime.Registry;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Restore
{
    internal sealed class RuntimeRegistryRestoreService :
        IRuntimeRegistryRestoreService
    {
        private readonly IRuntimeRegistry _runtimeRegistry;

        public RuntimeRegistryRestoreService(IRuntimeRegistry runtimeRegistry)
        {
            _runtimeRegistry = runtimeRegistry;
        }

        public void Restore(RuntimeRestoreSnapshot snapshot)
        {
            _runtimeRegistry.Clear();

            if (snapshot == null || snapshot.Wards == null)
            {
                ModLog.Info("[Runtime Restore] Runtime registry restored. Wards: 0");
                return;
            }

            int restoredCount = 0;

            foreach (RuntimeWardRestoreRecord record in snapshot.Wards)
            {
                if (record == null)
                    continue;

                RuntimeWard ward = new RuntimeWard(
                    record.WardId,
                    record.Position);

                if (_runtimeRegistry.TryAdd(ward))
                    restoredCount++;
            }

            ModLog.Info(
                "[Runtime Restore] Runtime registry restored. Wards: " +
                restoredCount);
        }
    }
}