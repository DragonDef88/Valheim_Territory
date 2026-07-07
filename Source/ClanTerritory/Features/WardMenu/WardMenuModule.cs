using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.TerritoryInteraction;
using ClanTerritory.Features.WardMenu.Services;
using ClanTerritory.Features.WardMenu.UI;
using ClanTerritory.Utils;
using UnityEngine;

namespace ClanTerritory.Features.WardMenu
{
    internal sealed class WardMenuModule :
        IInitializable,
        IDisposableModule
    {
        private GameObject _runnerObject;
        private WardMenuRunner _runner;
        private WardMenuService _wardMenuService;

        public void Initialize()
        {
            WardMenuView view = new WardMenuView();

            _wardMenuService = new WardMenuService(view);

            ServiceContainer.Register<IWardMenuService>(_wardMenuService);

            EventBus eventBus = ServiceContainer.Get<EventBus>();
            eventBus.Subscribe<TerritoryInteractionRequestedEvent>(_wardMenuService);

            _runnerObject = new GameObject("ClanTerritory_WardMenuRunner");
            Object.DontDestroyOnLoad(_runnerObject);

            _runner = _runnerObject.AddComponent<WardMenuRunner>();
            _runner.Initialize(_wardMenuService);

            ModLog.Info("Ward Menu module initialized.");
        }

        public void Shutdown()
        {
            if (_wardMenuService != null)
                _wardMenuService.Shutdown();

            if (_runnerObject != null)
                Object.Destroy(_runnerObject);

            _runner = null;
            _runnerObject = null;
            _wardMenuService = null;

            ModLog.Info("Ward Menu module shutdown.");
        }

        private sealed class WardMenuRunner : MonoBehaviour
        {
            private WardMenuService _service;

            public void Initialize(WardMenuService service)
            {
                _service = service;
            }

            private void Update()
            {
                if (_service != null)
                    _service.Update();
            }
        }
    }
}