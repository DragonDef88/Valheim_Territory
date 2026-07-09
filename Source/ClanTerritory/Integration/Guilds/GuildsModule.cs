using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Utils;

namespace ClanTerritory.Integration.Guilds
{
    internal sealed class GuildsModule :
        IInitializable,
        IDisposableModule
    {
        private GuildsAdapter _adapter;

        public void Initialize()
        {
            _adapter = new GuildsAdapter();

            ServiceContainer.Register<IGuildService>(_adapter);

            ModLog.Info("[Guilds] Integration module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("[Guilds] Integration module shutdown.");

            _adapter = null;
        }
    }
}
