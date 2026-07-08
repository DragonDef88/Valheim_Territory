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

        private readonly IWardMenuView _view;
        private readonly IWardMenuWardActions _wardActions;
        private readonly IWardMenuTerritoryActions _territoryActions;
        private readonly Action<string> _closeAction;
        private readonly Action<string> _refreshAction;

        private float _currentWardRadius;
        private WardId _currentWardId;
        private PrivateArea _currentPrivateArea;
        private Player _currentPlayer;
        private string _currentTerritoryName = "";
        private WardMenuTab _currentTab;

        private enum WardMenuTab
        {
            Overview,
            Ward,
            Territory
        }

        public WardMenuController(
            IWardMenuView view,
            IWardMenuWardActions wardActions,
            IWardMenuTerritoryActions territoryActions,
            Action<string> closeAction,
            Action<string> refreshAction)
        {
            _view = view;
            _wardActions = wardActions;
            _territoryActions = territoryActions;
            _closeAction = closeAction;
            _refreshAction = refreshAction;
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
            _currentTab = WardMenuTab.Overview;

            _view.Show(
                model,
                ShowOverview,
                ShowWard,
                ShowTerritory,
                RequestToggleProtection,
                RequestDecreaseRadius,
                RequestIncreaseRadius,
                RequestRenameTerritoryDialog,
                RequestRemovePermittedPlayer,
                RequestToggleSelfPermission,
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

            _view.Refresh(model);
            ShowCurrentTab();

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
            bool actionStarted = _wardActions.ToggleProtection(
                _currentWardId,
                _currentPrivateArea,
                _currentPlayer);

            if (actionStarted && _refreshAction != null)
                _refreshAction("ProtectionToggle");
        }

        public void RequestSetRadius(float radius)
        {
            _wardActions.SetRadius(
                _currentWardId,
                _currentPrivateArea,
                _currentPlayer,
                radius);

            _currentWardRadius = radius;
        }

        public void RequestRemovePermittedPlayer(long playerId)
        {
            bool actionStarted = _wardActions.RemovePermittedPlayer(
                _currentWardId,
                _currentPrivateArea,
                _currentPlayer,
                playerId);

            if (actionStarted && _refreshAction != null)
                _refreshAction("RemovePermittedPlayer");
        }

        public void RequestToggleSelfPermission()
        {
            bool actionStarted = _wardActions.ToggleSelfPermission(
                _currentWardId,
                _currentPrivateArea,
                _currentPlayer);

            if (actionStarted && _refreshAction != null)
                _refreshAction("ToggleSelfPermission");
        }

        public void RequestRenameTerritoryDialog()
        {
            if (!IsCurrentPlayerWardCreator())
            {
                ModLog.Debug("[WardMenuController] Rename ignored. Current player is not ward creator: " + _currentWardId);
                return;
            }

            if (TextInput.instance == null)
            {
                ModLog.Debug("[WardMenuController] Rename ignored. TextInput is null: " + _currentWardId);
                return;
            }

            _view.Hide();

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
            _currentTab = WardMenuTab.Overview;
            _view.ShowOverviewPanel();
        }

        private void ShowWard()
        {
            _currentTab = WardMenuTab.Ward;
            _view.ShowWardPanel();
        }

        private void ShowTerritory()
        {
            _currentTab = WardMenuTab.Territory;
            _view.ShowTerritoryPanel();
        }

        private void ShowCurrentTab()
        {
            if (_currentTab == WardMenuTab.Ward)
            {
                _view.ShowWardPanel();
                return;
            }

            if (_currentTab == WardMenuTab.Territory)
            {
                _view.ShowTerritoryPanel();
                return;
            }

            _view.ShowOverviewPanel();
        }

        private bool IsCurrentPlayerWardCreator()
        {
            if (_currentPrivateArea == null || _currentPlayer == null)
                return false;

            Piece piece = _currentPrivateArea.GetComponent<Piece>();

            if (piece == null)
                return false;

            return piece.GetCreator() == _currentPlayer.GetPlayerID();
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
