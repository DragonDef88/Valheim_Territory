using System;
using ClanTerritory.Features.WardMenu.Models;
using ClanTerritory.Features.WardMenu.UI;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Controllers
{
    internal sealed class WardMenuController
    {
        private readonly WardMenuView _view;
        private readonly Action<string> _closeAction;

        public WardMenuController(
            WardMenuView view,
            Action<string> closeAction)
        {
            _view = view;
            _closeAction = closeAction;
        }

        public void Show(WardMenuModel model)
        {
            if (model == null)
                return;

            _view.Show(
                model,
                ShowOverview,
                ShowWard,
                ShowTerritory,
                CloseByInput,
                CloseByDistance);

            ShowOverview();

            ModLog.Debug("[WardMenuController] Shown.");
        }

        public void Tick(PrivateArea privateArea, Player player)
        {
            _view.Tick(privateArea, player);
        }

        public void Hide()
        {
            _view.Hide();
        }

        public void Destroy()
        {
            _view.Destroy();
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