using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Diagnostics.Services;
using ClanTerritory.Features.Runtime.Events;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Diagnostics
{
    internal sealed class DiagnosticsModule : IInitializable, IDisposableModule
    {
        private IDiagnosticsService _service;
        private RuntimeStateLogger _runtimeLogger;

        public void Initialize()
        {
            _service = new DiagnosticsService();

            ServiceContainer.Register<IDiagnosticsService>(_service);

            _runtimeLogger = new RuntimeStateLogger();

            EventBus runtimeEventBus;

            if (ServiceContainer.TryGet<EventBus>(out runtimeEventBus))
            {
                runtimeEventBus.Subscribe<RuntimeStateChangedEvent>(_runtimeLogger);
            }

            ModLog.Info("Diagnostics module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("Diagnostics module shutdown.");
        }
    }
}