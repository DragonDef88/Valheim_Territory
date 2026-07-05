using ClanTerritory.Features.Persistence.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Pipeline.Steps
{
    internal sealed class PersistenceLoadStep : IRuntimeStep
    {
        private readonly IPersistenceService _persistenceService;

        public PersistenceLoadStep(IPersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
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
            ModLog.Info("[Runtime Pipeline] Loading persistence.");

            _persistenceService.LoadNow();

            ModLog.Info("[Runtime Pipeline] Persistence load completed.");
        }
    }
}