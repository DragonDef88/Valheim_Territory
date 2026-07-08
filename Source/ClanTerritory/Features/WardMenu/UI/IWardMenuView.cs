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
            Action showTerraformingAction,
            Action toggleProtectionAction,
            Action decreaseRadiusAction,
            Action increaseRadiusAction,
            Action renameTerritoryAction,
            Action<long> removePermittedPlayerAction,
            Action toggleSelfPermissionAction,
            Action toggleDoorLockAction,
            Action decreaseDoorAutoCloseAction,
            Action increaseDoorAutoCloseAction,
            Action toggleStructureDamageProtectionAction,
            Action toggleTerraformingAction,
            Action toggleTerraformingRunningAction,
            Action cycleTerraformingModeAction,
            Action decreaseTerraformingRadiusAction,
            Action increaseTerraformingRadiusAction,
            Action setTerraformingTargetHeightFromWardAction,
            Action setTerraformingTargetHeightFromPlayerAction,
            Action storeTerraformingHoeAction,
            Action storeTerraformingPickaxeAction,
            Action addTerraformingFuelAction,
            Action addTerraformingStoneAction,
            Action closeByInputAction,
            Action closeByDistanceAction);

        void Refresh(WardMenuModel model);

        void Hide();

        void Tick(PrivateArea privateArea, Player player);

        void Destroy();

        void ShowOverviewPanel();

        void ShowWardPanel();

        void ShowTerritoryPanel();

        void ShowTerraformingPanel();
    }
}
