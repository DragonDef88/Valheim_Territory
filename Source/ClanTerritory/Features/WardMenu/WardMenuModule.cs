using ClanTerritory.Abstractions;
using ClanTerritory.Core;
using ClanTerritory.Events;
using ClanTerritory.Features.Territory.Events;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Features.TerritoryInteraction;
using ClanTerritory.Features.TerritoryNaming.Events;
using ClanTerritory.Features.TerritoryNaming.Services;
using ClanTerritory.Features.WardMenu.Actions;
using ClanTerritory.Features.WardMenu.Builders;
using ClanTerritory.Features.WardMenu.Controllers;
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
        private WardMenuController _wardMenuController;
        private WardMenuModelBuilder _wardMenuModelBuilder;
        private IWardMenuWardActions _wardActions;
        private IWardMenuTerritoryActions _territoryActions;

        public void Initialize()
        {
            IWardMenuView view = new JotunnWardMenuView();

            _wardActions = new WardMenuWardActions();
            ITerritoryNamingService territoryNamingService =
                ServiceContainer.Get<ITerritoryNamingService>();

            TerritoryRuleService territoryRuleService =
                ServiceContainer.Get<TerritoryRuleService>();

            _territoryActions = new WardMenuTerritoryActions(
                territoryNamingService,
                territoryRuleService);

            _wardMenuModelBuilder = new WardMenuModelBuilder(
                territoryNamingService,
                territoryRuleService);

            _wardMenuController = new WardMenuController(
                view,
                _wardActions,
                _territoryActions,
                CloseWithReason,
                RequestRefreshAfterAction);

            _wardMenuService = new WardMenuService(
                _wardMenuController,
                _wardMenuModelBuilder);

            ServiceContainer.Register<IWardMenuWardActions>(_wardActions);
            ServiceContainer.Register<IWardMenuTerritoryActions>(_territoryActions);
            ServiceContainer.Register<IWardMenuService>(_wardMenuService);

            EventBus eventBus = ServiceContainer.Get<EventBus>();
            eventBus.Subscribe<TerritoryInteractionRequestedEvent>(_wardMenuService);
            eventBus.Subscribe<TerritoryRenamedEvent>(_wardMenuService);
            eventBus.Subscribe<TerritoryRadiusChangedEvent>(_wardMenuService);

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
            _territoryActions = null;
            _wardActions = null;
            _wardMenuModelBuilder = null;
            _wardMenuController = null;
            _wardMenuService = null;

            ModLog.Info("Ward Menu module shutdown.");
        }

        private void CloseWithReason(string reason)
        {
            if (_wardMenuService != null)
                _wardMenuService.CloseWithReason(reason);
        }

        private void RequestRefreshAfterAction(string reason)
        {
            if (_wardMenuService != null)
                _wardMenuService.RequestRefreshAfterAction(reason);
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
