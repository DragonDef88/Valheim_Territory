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
            Action showBiomeDominionAction,
            Action showEconomyAction,
            Action showTerraformingAction,
            Action openTreasuryChestAction,
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
            Action openTerraformingPreparationChestAction,
            Action decreaseTerraformingRadiusAction,
            Action increaseTerraformingRadiusAction,
            Action storeTerraformingHoeAction,
            Action storeTerraformingPickaxeAction,
            Action<int> addTerraformingFuelSlotAction,
            Action<int> addTerraformingStoneSlotAction,
            Action claimBiomeDominionAction,
            Action releaseBiomeDominionAction,
            Action toggleBiomeDoorLockAction,
            Action decreaseBiomeDoorAutoCloseAction,
            Action increaseBiomeDoorAutoCloseAction,
            Action toggleBiomeStructureDamageProtectionAction,
            Action economyDepositAction,
            Action economyWithdrawAction,
            Action economyUpkeepAction,
            Action economyTaxAction,
            Action economyTransferAction,
            Action closeByInputAction,
            Action closeByDistanceAction);

        void Refresh(WardMenuModel model);

        void Hide();

        void Tick(PrivateArea privateArea, Player player);

        void Destroy();

        void ShowOverviewPanel();

        void ShowWardPanel();

        void ShowTerritoryPanel();

        void ShowBiomeDominionPanel();

        void ShowEconomyPanel();

        void ShowTerraformingPanel();
    }
}
