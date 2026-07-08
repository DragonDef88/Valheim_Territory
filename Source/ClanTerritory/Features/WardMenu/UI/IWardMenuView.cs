using System;
using ClanTerritory.Features.WardMenu.Models;

namespace ClanTerritory.Features.WardMenu.UI
{
    internal interface IWardMenuView
    {
        bool IsVisible { get; }

        void Show(
            WardMenuModel model,
            Action showOverviewAction,
            Action showWardAction,
            Action showTerritoryAction,
            Action toggleProtectionAction,
            Action decreaseRadiusAction,
            Action increaseRadiusAction,
            Action renameTerritoryAction,
            Action<long> removePermittedPlayerAction,
            Action toggleSelfPermissionAction,
            Action closeByInputAction,
            Action closeByDistanceAction);

        void Refresh(WardMenuModel model);

        void Hide();

        void Tick(PrivateArea privateArea, Player player);

        void Destroy();

        void ShowOverviewPanel();

        void ShowWardPanel();

        void ShowTerritoryPanel();
    }
}
