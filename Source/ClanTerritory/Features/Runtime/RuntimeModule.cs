using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Services;
using ClanTerritory.Utils;
using System;


namespace ClanTerritory.Features.Runtime
{
    internal sealed class RuntimeModule : IInitializable, IDisposableModule
    {
        private RuntimeStateMachine _stateMachine;
        private IRuntimeInitializationService _runtimeInitializationService;

        public void Initialize()
        {
            if (!ServiceContainer.TryGet<EventBus>(out EventBus eventBus))
            {
                throw new InvalidOperationException(
                    "EventBus is not registered.");
            }

            _stateMachine = new RuntimeStateMachine(eventBus);

            ServiceContainer.Register(_stateMachine);

            _runtimeInitializationService = new RuntimeInitializationService();

            ServiceContainer.Register<IRuntimeInitializationService>(
                _runtimeInitializationService);

            ModLog.Info("Runtime module initialized.");
        }

        public void Shutdown()
        {
            ModLog.Info("Runtime module shutdown.");
        }
    }
}