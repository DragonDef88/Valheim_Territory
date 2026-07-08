using System;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.WardMenu.Actions;
using ClanTerritory.Features.WardMenu.Models;
using ClanTerritory.Features.WardMenu.UI;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Controllers
{
    internal sealed class WardMenuController : TextReceiver
    {
        private const int TerritoryNameCharacterLimit = 50;

        private readonly WardMenuView _view;
        private readonly IWardMenuWardActions _wardActions;
        private readonly IWardMenuTerritoryActions _territoryActions;
        private readonly Action<string> _closeAction;
        private float _currentWardRadius;
        private WardId _currentWardId;
        private PrivateArea _currentPrivateArea;
        private Player _currentPlayer;
        private string _currentTerritoryName = "";

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
            _currentTerritoryName = model.Territory.Name;
            _currentWardRadius = model.Ward.Radius;

            _view.Show(
                        model,
                        ShowOverview,
                        ShowWard,
                        ShowTerritory,
                        RequestToggleProtection,
                        RequestDecreaseRadius,
                        RequestIncreaseRadius,
                        RequestRenameTerritoryDialog,
                        CloseByInput,
                        CloseByDistance);

            ShowOverview();

            ModLog.Debug("[WardMenuController] Shown: " + _currentWardId);
        }

        public void Refresh(WardMenuModel model)
        {
            if (model == null)
                return;

            _currentTerritoryName = model.Territory.Name;
            _currentWardRadius = model.Ward.Radius;

            _view.Show(
                       model,
                       ShowOverview,
                       ShowWard,
                       ShowTerritory,
                       RequestToggleProtection,
                       RequestDecreaseRadius,
                       RequestIncreaseRadius,
                       RequestRenameTerritoryDialog,
                       CloseByInput,
                       CloseByDistance);

            ShowTerritory();

            ModLog.Debug("[WardMenuController] Refreshed: " + model.Ward.WardId);
        }

        public void Tick(PrivateArea privateArea, Player player)
        {
            _view.Tick(privateArea, player);
        }

        public void RequestDecreaseRadius()
        {
            RequestSetRadius(_currentWardRadius - 5f);
        }

        public void RequestIncreaseRadius()
        {
            RequestSetRadius(_currentWardRadius + 5f);
        }
        public void Hide()
        {
            _view.Hide();

            _currentPrivateArea = null;
            _currentPlayer = null;
            _currentTerritoryName = "";
        }

        public void Destroy()
        {
            _view.Destroy();

            _currentPrivateArea = null;
            _currentPlayer = null;
            _currentTerritoryName = "";
        }

        public string GetText()
        {
            return _currentTerritoryName;
        }

        public void SetText(string text)
        {
            RequestRenameTerritory(text);
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

        public void RequestRenameTerritoryDialog()
        {
            if (TextInput.instance == null)
            {
                ModLog.Debug("[WardMenuController] Rename ignored. TextInput is null: " + _currentWardId);
                return;
            }

            TextInput.instance.RequestText(
                this,
                "Territory name",
                TerritoryNameCharacterLimit);

            ModLog.Debug("[WardMenuController] Rename dialog opened: " + _currentWardId);
        }

        public void RequestRenameTerritory(string name)
        {
            _territoryActions.RenameTerritory(
                _currentWardId,
                _currentPrivateArea,
                _currentPlayer,
                name);
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