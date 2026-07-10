using System;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Diplomacy;
using ClanTerritory.Features.WardMenu.Actions;
using ClanTerritory.Features.WardMenu.Models;
using ClanTerritory.Features.WardMenu.UI;
using ClanTerritory.Localization;
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
        private int _currentDoorAutoCloseSeconds;
        private int _currentBiomeDoorAutoCloseSeconds;
        private WardId _currentWardId;
        private PrivateArea _currentPrivateArea;
        private Player _currentPlayer;
        private string _currentTerritoryName = "";
        private DiplomacyRelationKind _pendingDiplomacyRelation = DiplomacyRelationKind.Neutral;
        private WardMenuTab _currentTab;
        private TextInputMode _textInputMode;

        private enum TextInputMode
        {
            None,
            RenameTerritory,
            EconomyDeposit,
            EconomyWithdraw,
            EconomyUpkeep,
            EconomyTax,
            EconomyTransfer,
            DiplomacyTargetGuild
        }

        private enum WardMenuTab
        {
            Overview,
            Ward,
            Territory,
            Economy,
            BiomeDominion,
            Terraforming
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

        public void Show(WardMenuModel model, PrivateArea privateArea, Player player)
        {
            if (model == null)
                return;

            _currentWardId = model.Ward.WardId;
            _currentPrivateArea = privateArea;
            _currentPlayer = player;
            _currentTerritoryName = model.Territory.Name;
            _currentWardRadius = model.Ward.Radius;
            _currentDoorAutoCloseSeconds = model.Territory.DoorAutoCloseSeconds;
            _currentBiomeDoorAutoCloseSeconds = model.BiomeDominion != null
                ? model.BiomeDominion.DoorAutoCloseSeconds
                : 5;
            _currentTab = WardMenuTab.Overview;

            _view.Show(
                model,
                ShowOverview,
                ShowWard,
                ShowTerritory,
                ShowBiomeDominion,
                ShowEconomy,
                ShowTerraforming,
                RequestOpenTreasuryChest,
                RequestToggleProtection,
                RequestDecreaseRadius,
                RequestIncreaseRadius,
                RequestRenameTerritoryDialog,
                RequestRemovePermittedPlayer,
                RequestToggleSelfPermission,
                RequestToggleDoorLock,
                RequestDecreaseDoorAutoCloseSeconds,
                RequestIncreaseDoorAutoCloseSeconds,
                RequestToggleStructureDamageProtection,
                RequestToggleTerraforming,
                RequestToggleTerraformingRunning,
                RequestOpenTerraformingPreparationChest,
                RequestDecreaseTerraformingRadius,
                RequestIncreaseTerraformingRadius,
                RequestStoreTerraformingHoe,
                RequestStoreTerraformingPickaxe,
                RequestAddTerraformingFuelSlot,
                RequestAddTerraformingStoneSlot,
                RequestClaimBiomeDominion,
                RequestReleaseBiomeDominion,
                RequestToggleBiomeDoorLock,
                RequestDecreaseBiomeDoorAutoCloseSeconds,
                RequestIncreaseBiomeDoorAutoCloseSeconds,
                RequestToggleBiomeStructureDamageProtection,
                RequestEconomyDepositDialog,
                RequestEconomyWithdrawDialog,
                RequestEconomyUpkeepDialog,
                RequestEconomyTaxDialog,
                RequestEconomyTransferDialog,
                RequestDiplomacyAllyDialog,
                RequestDiplomacyEnemyDialog,
                RequestDiplomacyVassalDialog,
                RequestDiplomacyNeutralDialog,
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
            _currentDoorAutoCloseSeconds = model.Territory.DoorAutoCloseSeconds;
            _currentBiomeDoorAutoCloseSeconds = model.BiomeDominion != null
                ? model.BiomeDominion.DoorAutoCloseSeconds
                : _currentBiomeDoorAutoCloseSeconds;

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

        public void RequestDecreaseDoorAutoCloseSeconds()
        {
            RequestSetDoorAutoCloseSeconds(_currentDoorAutoCloseSeconds - 1);
        }

        public void RequestIncreaseDoorAutoCloseSeconds()
        {
            RequestSetDoorAutoCloseSeconds(_currentDoorAutoCloseSeconds + 1);
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
            if (_textInputMode == TextInputMode.EconomyDeposit ||
                _textInputMode == TextInputMode.EconomyWithdraw ||
                _textInputMode == TextInputMode.EconomyUpkeep ||
                _textInputMode == TextInputMode.EconomyTax)
            {
                return "10";
            }

            if (_textInputMode == TextInputMode.EconomyTransfer ||
                _textInputMode == TextInputMode.DiplomacyTargetGuild)
                return "";

            return _currentTerritoryName;
        }

        public void SetText(string text)
        {
            TextInputMode mode = _textInputMode;
            _textInputMode = TextInputMode.None;

            if (mode == TextInputMode.EconomyDeposit)
            {
                RequestEconomyDeposit(text);
                return;
            }

            if (mode == TextInputMode.EconomyWithdraw)
            {
                RequestEconomyWithdraw(text);
                return;
            }

            if (mode == TextInputMode.EconomyUpkeep)
            {
                RequestEconomyUpkeep(text);
                return;
            }

            if (mode == TextInputMode.EconomyTax)
            {
                RequestEconomyTax(text);
                return;
            }

            if (mode == TextInputMode.EconomyTransfer)
            {
                RequestEconomyTransfer(text);
                return;
            }

            if (mode == TextInputMode.DiplomacyTargetGuild)
            {
                RequestSetDiplomacyRelation(text);
                return;
            }

            RequestRenameTerritory(text);
        }

        public void RequestToggleProtection()
        {
            bool actionStarted = _wardActions.ToggleProtection(_currentWardId, _currentPrivateArea, _currentPlayer);

            if (actionStarted && _refreshAction != null)
                _refreshAction("ProtectionToggle");
        }

        public void RequestSetRadius(float radius)
        {
            _wardActions.SetRadius(_currentWardId, _currentPrivateArea, _currentPlayer, radius);
            _currentWardRadius = radius;
        }

        public void RequestRemovePermittedPlayer(long playerId)
        {
            bool actionStarted = _wardActions.RemovePermittedPlayer(_currentWardId, _currentPrivateArea, _currentPlayer, playerId);

            if (actionStarted && _refreshAction != null)
                _refreshAction("RemovePermittedPlayer");
        }

        public void RequestToggleSelfPermission()
        {
            bool actionStarted = _wardActions.ToggleSelfPermission(_currentWardId, _currentPrivateArea, _currentPlayer);

            if (actionStarted && _refreshAction != null)
                _refreshAction("ToggleSelfPermission");
        }

        public void RequestToggleDoorLock()
        {
            bool actionStarted = _territoryActions.ToggleDoorLock(_currentWardId, _currentPrivateArea, _currentPlayer);

            if (actionStarted && _refreshAction != null)
                _refreshAction("ToggleDoorLock");
        }

        public void RequestSetDoorAutoCloseSeconds(int seconds)
        {
            bool actionStarted = _territoryActions.SetDoorAutoCloseSeconds(_currentWardId, _currentPrivateArea, _currentPlayer, seconds);

            if (actionStarted)
                _currentDoorAutoCloseSeconds = seconds;

            if (actionStarted && _refreshAction != null)
                _refreshAction("SetDoorAutoCloseSeconds");
        }

        public void RequestToggleStructureDamageProtection()
        {
            bool actionStarted = _territoryActions.ToggleStructureDamageProtection(_currentWardId, _currentPrivateArea, _currentPlayer);

            if (actionStarted && _refreshAction != null)
                _refreshAction("ToggleStructureDamageProtection");
        }

        public void RequestClaimBiomeDominion()
        {
            RefreshIfActionStarted(
                _territoryActions.ClaimBiomeDominion(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer),
                "ClaimBiomeDominion");
        }

        public void RequestReleaseBiomeDominion()
        {
            RefreshIfActionStarted(
                _territoryActions.ReleaseBiomeDominion(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer),
                "ReleaseBiomeDominion");
        }

        public void RequestToggleBiomeDoorLock()
        {
            RefreshIfActionStarted(
                _territoryActions.ToggleBiomeDoorLock(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer),
                "ToggleBiomeDoorLock");
        }

        public void RequestDecreaseBiomeDoorAutoCloseSeconds()
        {
            RequestSetBiomeDoorAutoCloseSeconds(_currentBiomeDoorAutoCloseSeconds - 1);
        }

        public void RequestIncreaseBiomeDoorAutoCloseSeconds()
        {
            RequestSetBiomeDoorAutoCloseSeconds(_currentBiomeDoorAutoCloseSeconds + 1);
        }

        public void RequestSetBiomeDoorAutoCloseSeconds(int seconds)
        {
            bool actionStarted =
                _territoryActions.SetBiomeDoorAutoCloseSeconds(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer,
                    seconds);

            if (actionStarted)
                _currentBiomeDoorAutoCloseSeconds = seconds;

            if (actionStarted && _refreshAction != null)
                _refreshAction("SetBiomeDoorAutoCloseSeconds");
        }

        public void RequestToggleBiomeStructureDamageProtection()
        {
            RefreshIfActionStarted(
                _territoryActions.ToggleBiomeStructureDamageProtection(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer),
                "ToggleBiomeStructureDamageProtection");
        }

        public void RequestToggleTerraforming()
        {
            RefreshIfActionStarted(
                _territoryActions.ToggleTerraforming(_currentWardId, _currentPrivateArea, _currentPlayer),
                "ToggleTerraforming");
        }

        public void RequestToggleTerraformingRunning()
        {
            RefreshIfActionStarted(
                _territoryActions.ToggleTerraformingRunning(_currentWardId, _currentPrivateArea, _currentPlayer),
                "ToggleTerraformingRunning");
        }

        public void RequestOpenTerraformingPreparationChest()
        {
            _view.Hide();

            bool actionStarted =
                _territoryActions.OpenTerraformingPreparationChest(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer);

            if (actionStarted && _closeAction != null)
                _closeAction("OpenTerraformingPreparationChest");
        }

        public void RequestOpenTreasuryChest()
        {
            _view.Hide();

            bool actionStarted =
                _territoryActions.OpenTreasuryChest(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer);

            if (actionStarted && _closeAction != null)
                _closeAction("OpenTreasuryChest");
        }

        public void RequestDecreaseTerraformingRadius()
        {
            RefreshIfActionStarted(
                _territoryActions.DecreaseTerraformingRadius(_currentWardId, _currentPrivateArea, _currentPlayer),
                "DecreaseTerraformingRadius");
        }

        public void RequestIncreaseTerraformingRadius()
        {
            RefreshIfActionStarted(
                _territoryActions.IncreaseTerraformingRadius(_currentWardId, _currentPrivateArea, _currentPlayer),
                "IncreaseTerraformingRadius");
        }

        public void RequestStoreTerraformingHoe()
        {
            RefreshIfActionStarted(
                _territoryActions.StoreTerraformingHoe(_currentWardId, _currentPrivateArea, _currentPlayer),
                "StoreTerraformingHoe");
        }

        public void RequestStoreTerraformingPickaxe()
        {
            RefreshIfActionStarted(
                _territoryActions.StoreTerraformingPickaxe(_currentWardId, _currentPrivateArea, _currentPlayer),
                "StoreTerraformingPickaxe");
        }

        public void RequestAddTerraformingFuelSlot(int slotIndex)
        {
            RefreshIfActionStarted(
                _territoryActions.AddTerraformingFuelSlot(_currentWardId, _currentPrivateArea, _currentPlayer, slotIndex),
                "AddTerraformingFuelSlot");
        }

        public void RequestAddTerraformingStoneSlot(int slotIndex)
        {
            RefreshIfActionStarted(
                _territoryActions.AddTerraformingStoneSlot(_currentWardId, _currentPrivateArea, _currentPlayer, slotIndex),
                "AddTerraformingStoneSlot");
        }


        public void RequestDiplomacyAllyDialog()
        {
            RequestDiplomacyRelationDialog(DiplomacyRelationKind.Ally);
        }

        public void RequestDiplomacyEnemyDialog()
        {
            RequestDiplomacyRelationDialog(DiplomacyRelationKind.Enemy);
        }

        public void RequestDiplomacyVassalDialog()
        {
            RequestDiplomacyRelationDialog(DiplomacyRelationKind.Vassal);
        }

        public void RequestDiplomacyNeutralDialog()
        {
            RequestDiplomacyRelationDialog(DiplomacyRelationKind.Neutral);
        }

        private void RequestDiplomacyRelationDialog(DiplomacyRelationKind relation)
        {
            if (TextInput.instance == null)
            {
                ModLog.Debug("[WardMenuController] Diplomacy relation dialog ignored. TextInput is null: " + _currentWardId);
                return;
            }

            _view.Hide();
            _pendingDiplomacyRelation = relation;
            _textInputMode = TextInputMode.DiplomacyTargetGuild;
            TextInput.instance.RequestText(this, CtLocalization.Get("ct.menu.diplomacy.target_prompt"), 80);
            ModLog.Debug("[WardMenuController] Diplomacy relation dialog opened: " + relation + ", ward: " + _currentWardId);
        }

        private void RequestSetDiplomacyRelation(string targetGuildName)
        {
            if (string.IsNullOrWhiteSpace(targetGuildName))
            {
                ShowPlayerMessage(CtLocalization.Get("ct.diplomacy.command.target_required"));
                return;
            }

            RefreshIfActionStarted(
                _territoryActions.SetDiplomacyRelation(
                    _currentWardId,
                    _currentPlayer,
                    targetGuildName,
                    _pendingDiplomacyRelation),
                "DiplomacyRelation");
        }

        public void RequestEconomyDepositDialog()
        {
            RequestEconomyAmountDialog(
                TextInputMode.EconomyDeposit,
                CtLocalization.Get("ct.menu.economy.deposit_prompt"));
        }

        public void RequestEconomyWithdrawDialog()
        {
            RequestEconomyAmountDialog(
                TextInputMode.EconomyWithdraw,
                CtLocalization.Get("ct.menu.economy.withdraw_prompt"));
        }

        public void RequestEconomyUpkeepDialog()
        {
            RequestEconomyAmountDialog(
                TextInputMode.EconomyUpkeep,
                CtLocalization.Get("ct.menu.economy.upkeep_prompt"));
        }

        public void RequestEconomyTaxDialog()
        {
            RequestEconomyAmountDialog(
                TextInputMode.EconomyTax,
                CtLocalization.Get("ct.menu.economy.tax_prompt"));
        }

        public void RequestEconomyTransferDialog()
        {
            if (TextInput.instance == null)
            {
                ModLog.Debug("[WardMenuController] Economy transfer dialog ignored. TextInput is null: " + _currentWardId);
                return;
            }

            _view.Hide();
            _textInputMode = TextInputMode.EconomyTransfer;
            TextInput.instance.RequestText(this, CtLocalization.Get("ct.menu.economy.transfer_prompt"), 80);
            ModLog.Debug("[WardMenuController] Economy transfer dialog opened: " + _currentWardId);
        }

        private void RequestEconomyAmountDialog(TextInputMode mode, string title)
        {
            if (TextInput.instance == null)
            {
                ModLog.Debug("[WardMenuController] Economy amount dialog ignored. TextInput is null: " + _currentWardId);
                return;
            }

            _view.Hide();
            _textInputMode = mode;
            TextInput.instance.RequestText(this, title, 12);
            ModLog.Debug("[WardMenuController] Economy amount dialog opened: " + mode + ", ward: " + _currentWardId);
        }

        private void RequestEconomyDeposit(string amountText)
        {
            int amount;

            if (!TryParseEconomyAmount(amountText, out amount))
            {
                ShowPlayerMessage(CtLocalization.Get("ct.economy.command.invalid_amount"));
                return;
            }

            RefreshIfActionStarted(
                _territoryActions.DepositEconomyCoins(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer,
                    amount),
                "EconomyDeposit");
        }

        private void RequestEconomyWithdraw(string amountText)
        {
            int amount;

            if (!TryParseEconomyAmount(amountText, out amount))
            {
                ShowPlayerMessage(CtLocalization.Get("ct.economy.command.invalid_amount"));
                return;
            }

            RefreshIfActionStarted(
                _territoryActions.WithdrawEconomyCoins(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer,
                    amount),
                "EconomyWithdraw");
        }

        private void RequestEconomyUpkeep(string amountText)
        {
            int amount;

            if (!TryParseEconomyAmount(amountText, out amount))
            {
                ShowPlayerMessage(CtLocalization.Get("ct.economy.command.invalid_amount"));
                return;
            }

            RefreshIfActionStarted(
                _territoryActions.PayEconomyUpkeep(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer,
                    amount),
                "EconomyUpkeep");
        }

        private void RequestEconomyTax(string amountText)
        {
            int amount;

            if (!TryParseEconomyAmount(amountText, out amount))
            {
                ShowPlayerMessage(CtLocalization.Get("ct.economy.command.invalid_amount"));
                return;
            }

            RefreshIfActionStarted(
                _territoryActions.PayEconomyTax(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer,
                    amount),
                "EconomyTax");
        }

        private void RequestEconomyTransfer(string value)
        {
            string targetGuildName;
            int amount;

            if (!TryParseEconomyTransfer(value, out targetGuildName, out amount))
            {
                ShowPlayerMessage(CtLocalization.Get("ct.economy.command.transfer_usage"));
                return;
            }

            RefreshIfActionStarted(
                _territoryActions.TransferEconomyCoins(
                    _currentWardId,
                    _currentPrivateArea,
                    _currentPlayer,
                    targetGuildName,
                    amount),
                "EconomyTransfer");
        }

        private static bool TryParseEconomyAmount(string value, out int amount)
        {
            amount = 0;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (!int.TryParse(value.Trim(), out amount))
                return false;

            return amount > 0 && amount <= 1000000;
        }

        private static bool TryParseEconomyTransfer(string value, out string targetGuildName, out int amount)
        {
            targetGuildName = "";
            amount = 0;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            string[] parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts == null || parts.Length < 2)
                return false;

            if (!int.TryParse(parts[parts.Length - 1], out amount) ||
                amount <= 0 ||
                amount > 1000000)
            {
                return false;
            }

            targetGuildName = string.Join(" ", parts, 0, parts.Length - 1);
            return !string.IsNullOrWhiteSpace(targetGuildName);
        }

        private static void ShowPlayerMessage(string message)
        {
            if (Player.m_localPlayer == null || string.IsNullOrEmpty(message))
                return;

            Player.m_localPlayer.Message(
                MessageHud.MessageType.Center,
                message);
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
            _textInputMode = TextInputMode.RenameTerritory;
            TextInput.instance.RequestText(this, "Territory name", TerritoryNameCharacterLimit);
            ModLog.Debug("[WardMenuController] Rename dialog opened: " + _currentWardId);
        }

        public void RequestRenameTerritory(string name)
        {
            _territoryActions.RenameTerritory(_currentWardId, _currentPrivateArea, _currentPlayer, name);
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

        private void ShowBiomeDominion()
        {
            _currentTab = WardMenuTab.BiomeDominion;
            _view.ShowBiomeDominionPanel();
        }

        private void ShowEconomy()
        {
            _currentTab = WardMenuTab.Economy;
            _view.ShowEconomyPanel();
        }

        private void ShowTerraforming()
        {
            _currentTab = WardMenuTab.Terraforming;
            _view.ShowTerraformingPanel();
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

            if (_currentTab == WardMenuTab.BiomeDominion)
            {
                _view.ShowBiomeDominionPanel();
                return;
            }

            if (_currentTab == WardMenuTab.Economy)
            {
                _view.ShowEconomyPanel();
                return;
            }

            if (_currentTab == WardMenuTab.Terraforming)
            {
                _view.ShowTerraformingPanel();
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

        private void RefreshIfActionStarted(bool actionStarted, string reason)
        {
            if (actionStarted && _refreshAction != null)
                _refreshAction(reason);
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
