using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Utils;

namespace ClanTerritory.Integration.Groups
{
    internal sealed class GroupsModule :
        IInitializable,
        IDisposableModule
    {
        private GroupsAdapter _adapter;

        public void Initialize()
        {
            _adapter = new GroupsAdapter();

            ServiceContainer.Register<IGroupsService>(_adapter);

            ModLog.Info("[Groups] Integration module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("[Groups] Integration module shutdown.");

            _adapter = null;
        }
    }
}