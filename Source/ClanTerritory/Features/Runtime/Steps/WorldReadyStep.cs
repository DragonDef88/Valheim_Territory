using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Pipeline.Steps
{
    internal sealed class WorldReadyStep : IRuntimeStep
    {
        public RuntimeState InputState
        {
            get { return RuntimeState.InfrastructureReady; }
        }

        public RuntimeState OutputState
        {
            get { return RuntimeState.WorldLoaded; }
        }

        public void Execute()
        {
            ModLog.Info("[Runtime Pipeline] WorldReadyStep prepared.");
        }
    }
}