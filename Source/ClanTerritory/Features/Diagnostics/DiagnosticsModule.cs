using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Features.Diagnostics.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Diagnostics
{
    internal sealed class DiagnosticsModule : IInitializable, IDisposableModule
    {
        private IDiagnosticsService _service;

        public void Initialize()
        {
            _service = new DiagnosticsService();

            ServiceContainer.Register<IDiagnosticsService>(_service);

            ModLog.Info("Diagnostics module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("Diagnostics module shutdown.");
        }
    }
}