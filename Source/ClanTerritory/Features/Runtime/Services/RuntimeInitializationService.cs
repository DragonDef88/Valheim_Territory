using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Services
{
    internal sealed class RuntimeInitializationService : IRuntimeInitializationService
    {
        public void InitializeRuntime()
        {
            ModLog.Info("Runtime initialization prepared.");
        }
    }
}