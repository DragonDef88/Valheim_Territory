using System;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.WardMenu.Actions;
using ClanTerritory.Features.WardMenu.Models;
using ClanTerritory.Features.WardMenu.UI;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Controllers
{
    internal sealed class WardMenuController
    {
        private readonly WardMenuView _view;
        private readonly IWardMenuWardActions _wardActions;
        private readonly IWardMenuTerritoryActions _territoryActions;
        private readonly Action<string> _closeAction;

        private WardId _currentWardId;
        private PrivateArea _currentPrivateArea;
        private Player _currentPlayer;

        public WardMenuController(
            WardMenuView view,
            IWardMenuWardActions wardActions,
            IWardMenuTerritoryActions territoryActions,
            Action<string> closeAction)
        {
            _view = view;
            _wardActions = wardActions;
            _territoryActions = territoryActions;
            _closeAction = closeAction;
        }

        public void Show(
            WardMenuModel model,
            PrivateArea privateArea,
            Player player)
        {
            if (model == null)
                return;

            _currentWardId = model.Ward.WardId;
            _currentPrivateArea = privateArea;
            _currentPlayer = player;

            _view.Show(
                model,
                ShowOverview,
                ShowWard,
                ShowTerritory,
                RequestToggleProtection,
                CloseByInput,
                CloseByDistance);

            ShowOverview();

            ModLog.Debug("[WardMenuController] Shown: " + _currentWardId);
        }

        public void Tick(PrivateArea privateArea, Player player)
        {
            _view.Tick(privateArea, player);
        }

        public void Hide()
        {
            _view.Hide();

            _currentPrivateArea = null;
            _currentPlayer = null;
        }

        public void Destroy()
        {
            _view.Destroy();

            _currentPrivateArea = null;
            _currentPlayer = null;
        }

        public void RequestToggleProtection()
        {
            _wardActions.ToggleProtection(
                _currentWardId,
                _currentPrivateArea,
                _currentPlayer);
        }

        public void RequestSetRadius(float radius)
        {
            _wardActions.SetRadius(_currentWardId, radius);
        }

        public void RequestRemovePermittedPlayer(long playerId)
        {
            _wardActions.RemovePermittedPlayer(_currentWardId, playerId);
        }

        public void RequestRenameTerritory(string name)
        {
            _territoryActions.RenameTerritory(_currentWardId, name);
        }

        public void RequestToggleGuildAccess()
        {
            _territoryActions.ToggleGuildAccess(_currentWardId);
        }

        public void RequestToggleGroupAccess()
        {
            _territoryActions.ToggleGroupAccess(_currentWardId);
        }

        private void ShowOverview()
        {
            _view.ShowOverviewPanel();
        }

        private void ShowWard()
        {
            _view.ShowWardPanel();
        }

        private void ShowTerritory()
        {
            _view.ShowTerritoryPanel();
        }

        private void CloseByInput()
        {
            RequestClose("Input");
        }

        private void CloseByDistance()
        {
            RequestClose("Distance");
        }

        private void RequestClose(string reason)
        {
            if (_closeAction != null)
                _closeAction(reason);
        }
    }
}