using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Registry
{
    internal sealed class RuntimeRegistryModule :
        IInitializable,
        IDisposableModule
    {
        private RuntimeRegistry _registry;

        public void Initialize()
        {
            _registry = new RuntimeRegistry();

            ServiceContainer.Register<IRuntimeRegistry>(_registry);

            ModLog.Info("Runtime registry module initialized.");
        }

        public void Shutdown()
        {
            if (_registry != null)
                _registry.Clear();

            ModLog.Info("Runtime registry module shutdown.");
        }
    }
}