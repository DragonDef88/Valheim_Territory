using System;
using System.Collections.Generic;
using ClanTerritory.Features.WardMenu.Models;
using ClanTerritory.Localization;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ClanTerritory.Features.WardMenu.UI
{
    internal sealed class JotunnWardMenuView : IWardMenuView
    {
        private const float HideDistance = 5f;
        private const int MaxVisiblePermittedPlayers = 4;

        private readonly List<GameObject> _permittedPlayerRowObjects =
            new List<GameObject>();

        private WardMenuModel _lastModel;
        private bool _showClanOverview;

        private GameObject _root;
        private GameObject _panel;
        private GameObject _overviewPanel;
        private GameObject _wardPanel;
        private GameObject _territoryPanel;
        private GameObject _biomeDominionPanel;
        private GameObject _economyPanel;
        private GameObject _terraformingPanel;
        private GameObject _terraformingStoragePanel;

        private Text _titleText;
        private Text _subtitleText;
        private Text _overviewText;
        private Text _wardText;
        private Text _territoryText;
        private Text _biomeDominionText;
        private Text _economyText;
        private Text _terraformingText;
        private Text _radiusValueText;
        private Text _doorAutoCloseValueText;
        private Text _biomeDoorAutoCloseValueText;
        private Text _terraformingRadiusValueText;

        private Button _overviewButton;
        private Button _wardButton;
        private Button _territoryButton;
        private Button _biomeDominionButton;
        private Button _economyButton;
        private Button _terraformingButton;
        private Button _openTreasuryButton;
        private Button _clanOverviewButton;
        private Button _diplomacyAllyButton;
        private Button _diplomacyEnemyButton;
        private Button _diplomacyVassalButton;
        private Button _diplomacyNeutralButton;
        private Button _closeButton;
        private Button _toggleProtectionButton;
        private Button _toggleSelfPermissionButton;
        private Button _decreaseRadiusButton;
        private Button _increaseRadiusButton;
        private Button _renameTerritoryButton;
        private Button _toggleDoorLockButton;
        private Button _decreaseDoorAutoCloseButton;
        private Button _increaseDoorAutoCloseButton;
        private Button _toggleStructureDamageProtectionButton;
        private Button _claimBiomeDominionButton;
        private Button _releaseBiomeDominionButton;
        private Button _toggleBiomeDoorLockButton;
        private Button _decreaseBiomeDoorAutoCloseButton;
        private Button _increaseBiomeDoorAutoCloseButton;
        private Button _toggleBiomeStructureDamageProtectionButton;
        private Button _economyDepositButton;
        private Button _economyWithdrawButton;
        private Button _economyUpkeepButton;
        private Button _economyTaxButton;
        private Button _economyTransferButton;
        private Button _toggleTerraformingButton;
        private Button _toggleTerraformingRunningButton;
        private Button _openTerraformingPreparationButton;
        private Button _closeTerraformingPreparationButton;
        private Button _decreaseTerraformingRadiusButton;
        private Button _increaseTerraformingRadiusButton;
        private Button _storeTerraformingHoeButton;
        private Button _storeTerraformingPickaxeButton;
        private readonly Button[] _terraformingFuelSlotButtons = new Button[5];
        private readonly Button[] _terraformingStoneSlotButtons = new Button[5];

        private Action _showOverviewAction;
        private Action _showWardAction;
        private Action _showTerritoryAction;
        private Action _showBiomeDominionAction;
        private Action _showEconomyAction;
        private Action _showTerraformingAction;
        private Action _openTreasuryChestAction;
        private Action _toggleProtectionAction;
        private Action _decreaseRadiusAction;
        private Action _increaseRadiusAction;
        private Action _renameTerritoryAction;
        private Action<long> _removePermittedPlayerAction;
        private Action _toggleSelfPermissionAction;
        private Action _toggleDoorLockAction;
        private Action _decreaseDoorAutoCloseAction;
        private Action _increaseDoorAutoCloseAction;
        private Action _toggleStructureDamageProtectionAction;
        private Action _toggleTerraformingAction;
        private Action _toggleTerraformingRunningAction;
        private Action _openTerraformingPreparationChestAction;
        private Action _decreaseTerraformingRadiusAction;
        private Action _increaseTerraformingRadiusAction;
        private Action _storeTerraformingHoeAction;
        private Action _storeTerraformingPickaxeAction;
        private Action<int> _addTerraformingFuelSlotAction;
        private Action<int> _addTerraformingStoneSlotAction;
        private Action _claimBiomeDominionAction;
        private Action _releaseBiomeDominionAction;
        private Action _toggleBiomeDoorLockAction;
        private Action _decreaseBiomeDoorAutoCloseAction;
        private Action _increaseBiomeDoorAutoCloseAction;
        private Action _toggleBiomeStructureDamageProtectionAction;
        private Action _economyDepositAction;
        private Action _economyWithdrawAction;
        private Action _economyUpkeepAction;
        private Action _economyTaxAction;
        private Action _economyTransferAction;
        private Action _diplomacyAllyAction;
        private Action _diplomacyEnemyAction;
        private Action _diplomacyVassalAction;
        private Action _diplomacyNeutralAction;
        private Action _closeByInputAction;
        private Action _closeByDistanceAction;

        private bool _visible;
        private bool _useReleasedAfterOpen;

        public bool IsVisible
        {
            get { return _visible; }
        }

        public JotunnWardMenuView()
        {
            GUIManager.OnCustomGUIAvailable += BuildGui;
        }

        public void Show(
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
            Action diplomacyAllyAction,
            Action diplomacyEnemyAction,
            Action diplomacyVassalAction,
            Action diplomacyNeutralAction,
            Action closeByInputAction,
            Action closeByDistanceAction)
        {
            if (model == null)
                return;

            SetActions(
                showOverviewAction,
                showWardAction,
                showTerritoryAction,
                showBiomeDominionAction,
                showEconomyAction,
                showTerraformingAction,
                openTreasuryChestAction,
                toggleProtectionAction,
                decreaseRadiusAction,
                increaseRadiusAction,
                renameTerritoryAction,
                removePermittedPlayerAction,
                toggleSelfPermissionAction,
                toggleDoorLockAction,
                decreaseDoorAutoCloseAction,
                increaseDoorAutoCloseAction,
                toggleStructureDamageProtectionAction,
                toggleTerraformingAction,
                toggleTerraformingRunningAction,
                openTerraformingPreparationChestAction,
                decreaseTerraformingRadiusAction,
                increaseTerraformingRadiusAction,
                storeTerraformingHoeAction,
                storeTerraformingPickaxeAction,
                addTerraformingFuelSlotAction,
                addTerraformingStoneSlotAction,
                claimBiomeDominionAction,
                releaseBiomeDominionAction,
                toggleBiomeDoorLockAction,
                decreaseBiomeDoorAutoCloseAction,
                increaseBiomeDoorAutoCloseAction,
                toggleBiomeStructureDamageProtectionAction,
                economyDepositAction,
                economyWithdrawAction,
                economyUpkeepAction,
                economyTaxAction,
                economyTransferAction,
                diplomacyAllyAction,
                diplomacyEnemyAction,
                diplomacyVassalAction,
                diplomacyNeutralAction,
                closeByInputAction,
                closeByDistanceAction);

            _useReleasedAfterOpen = false;

            EnsureCreated();

            if (_root == null)
                return;

            ApplyModel(model);
            SetVisible(true);
        }

        public void Refresh(WardMenuModel model)
        {
            if (model == null)
                return;

            EnsureCreated();

            if (_root == null)
                return;

            ApplyModel(model);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void Tick(PrivateArea privateArea, Player player)
        {
            if (!_visible)
                return;

            if (privateArea == null || player == null)
            {
                RequestCloseByDistance();
                return;
            }

            if (Vector3.Distance(privateArea.transform.position, player.transform.position) > HideDistance)
            {
                RequestCloseByDistance();
                return;
            }

            bool usePressed =
                ZInput.GetButtonDown("Use") ||
                ZInput.GetButtonDown("JoyUse");

            bool useHeld =
                ZInput.GetButton("Use") ||
                ZInput.GetButton("JoyUse");

            if (!_useReleasedAfterOpen)
            {
                if (!useHeld)
                    _useReleasedAfterOpen = true;

                return;
            }

            if (TextInput.IsVisible())
                return;

            if ((Chat.instance == null || !Chat.instance.HasFocus()) &&
                !Console.IsVisible() &&
                !Menu.IsVisible() &&
                !Minimap.IsOpen() &&
                ZInput.GetKeyDown(KeyCode.R, true))
            {
                RequestRenameTerritory();
                return;
            }

            if ((Chat.instance == null || !Chat.instance.HasFocus()) &&
                !Console.IsVisible() &&
                !Menu.IsVisible() &&
                !Minimap.IsOpen() &&
                (ZInput.GetKeyDown(KeyCode.Escape, true) ||
                 usePressed ||
                 ZInput.GetButtonDown("JoyButtonB")))
            {
                ZInput.ResetButtonStatus("Use");
                ZInput.ResetButtonStatus("JoyUse");
                ZInput.ResetButtonStatus("JoyButtonB");
                RequestCloseByInput();
            }
        }

        public void Destroy()
        {
            GUIManager.OnCustomGUIAvailable -= BuildGui;

            SetVisible(false);
            ClearPermittedPlayerRows();

            if (_root != null)
                UnityEngine.Object.Destroy(_root);

            _root = null;
            _panel = null;
            _overviewPanel = null;
            _wardPanel = null;
            _territoryPanel = null;
            _biomeDominionPanel = null;
            _economyPanel = null;
            _terraformingPanel = null;
            _terraformingStoragePanel = null;

            _titleText = null;
            _subtitleText = null;
            _overviewText = null;
            _wardText = null;
            _territoryText = null;
            _biomeDominionText = null;
            _economyText = null;
            _terraformingText = null;
            _radiusValueText = null;
            _doorAutoCloseValueText = null;
            _biomeDoorAutoCloseValueText = null;
            _terraformingRadiusValueText = null;

            _overviewButton = null;
            _wardButton = null;
            _territoryButton = null;
            _biomeDominionButton = null;
            _economyButton = null;
            _terraformingButton = null;
            _openTreasuryButton = null;
            _clanOverviewButton = null;
            _diplomacyAllyButton = null;
            _diplomacyEnemyButton = null;
            _diplomacyVassalButton = null;
            _diplomacyNeutralButton = null;
            _closeButton = null;
            _toggleProtectionButton = null;
            _toggleSelfPermissionButton = null;
            _decreaseRadiusButton = null;
            _increaseRadiusButton = null;
            _renameTerritoryButton = null;
            _toggleDoorLockButton = null;
            _decreaseDoorAutoCloseButton = null;
            _increaseDoorAutoCloseButton = null;
            _toggleStructureDamageProtectionButton = null;
            _claimBiomeDominionButton = null;
            _releaseBiomeDominionButton = null;
            _toggleBiomeDoorLockButton = null;
            _decreaseBiomeDoorAutoCloseButton = null;
            _increaseBiomeDoorAutoCloseButton = null;
            _toggleBiomeStructureDamageProtectionButton = null;
            _economyDepositButton = null;
            _economyWithdrawButton = null;
            _economyUpkeepButton = null;
            _economyTaxButton = null;
            _economyTransferButton = null;
            _toggleTerraformingButton = null;
            _toggleTerraformingRunningButton = null;
            _openTerraformingPreparationButton = null;
            _closeTerraformingPreparationButton = null;
            _decreaseTerraformingRadiusButton = null;
            _increaseTerraformingRadiusButton = null;
            _storeTerraformingHoeButton = null;
            _storeTerraformingPickaxeButton = null;

            ClearActions();
        }

        public void ShowOverviewPanel()
        {
            SetActivePanel(_overviewPanel);
        }

        public void ShowWardPanel()
        {
            SetActivePanel(_wardPanel);
        }

        public void ShowTerritoryPanel()
        {
            SetActivePanel(_territoryPanel);
        }

        public void ShowBiomeDominionPanel()
        {
            SetActivePanel(_biomeDominionPanel);
        }

        public void ShowEconomyPanel()
        {
            SetActivePanel(_economyPanel);
        }

        public void ShowTerraformingPanel()
        {
            SetActivePanel(_terraformingPanel);
        }

        private void SetActions(
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
            Action diplomacyAllyAction,
            Action diplomacyEnemyAction,
            Action diplomacyVassalAction,
            Action diplomacyNeutralAction,
            Action closeByInputAction,
            Action closeByDistanceAction)
        {
            _showOverviewAction = showOverviewAction;
            _showWardAction = showWardAction;
            _showTerritoryAction = showTerritoryAction;
            _showBiomeDominionAction = showBiomeDominionAction;
            _showEconomyAction = showEconomyAction;
            _showTerraformingAction = showTerraformingAction;
            _openTreasuryChestAction = openTreasuryChestAction;
            _toggleProtectionAction = toggleProtectionAction;
            _decreaseRadiusAction = decreaseRadiusAction;
            _increaseRadiusAction = increaseRadiusAction;
            _renameTerritoryAction = renameTerritoryAction;
            _removePermittedPlayerAction = removePermittedPlayerAction;
            _toggleSelfPermissionAction = toggleSelfPermissionAction;
            _toggleDoorLockAction = toggleDoorLockAction;
            _decreaseDoorAutoCloseAction = decreaseDoorAutoCloseAction;
            _increaseDoorAutoCloseAction = increaseDoorAutoCloseAction;
            _toggleStructureDamageProtectionAction = toggleStructureDamageProtectionAction;
            _toggleTerraformingAction = toggleTerraformingAction;
            _toggleTerraformingRunningAction = toggleTerraformingRunningAction;
            _openTerraformingPreparationChestAction = openTerraformingPreparationChestAction;
            _decreaseTerraformingRadiusAction = decreaseTerraformingRadiusAction;
            _increaseTerraformingRadiusAction = increaseTerraformingRadiusAction;
            _storeTerraformingHoeAction = storeTerraformingHoeAction;
            _storeTerraformingPickaxeAction = storeTerraformingPickaxeAction;
            _addTerraformingFuelSlotAction = addTerraformingFuelSlotAction;
            _addTerraformingStoneSlotAction = addTerraformingStoneSlotAction;
            _claimBiomeDominionAction = claimBiomeDominionAction;
            _releaseBiomeDominionAction = releaseBiomeDominionAction;
            _toggleBiomeDoorLockAction = toggleBiomeDoorLockAction;
            _decreaseBiomeDoorAutoCloseAction = decreaseBiomeDoorAutoCloseAction;
            _increaseBiomeDoorAutoCloseAction = increaseBiomeDoorAutoCloseAction;
            _toggleBiomeStructureDamageProtectionAction = toggleBiomeStructureDamageProtectionAction;
            _economyDepositAction = economyDepositAction;
            _economyWithdrawAction = economyWithdrawAction;
            _economyUpkeepAction = economyUpkeepAction;
            _economyTaxAction = economyTaxAction;
            _economyTransferAction = economyTransferAction;
            _diplomacyAllyAction = diplomacyAllyAction;
            _diplomacyEnemyAction = diplomacyEnemyAction;
            _diplomacyVassalAction = diplomacyVassalAction;
            _diplomacyNeutralAction = diplomacyNeutralAction;
            _closeByInputAction = closeByInputAction;
            _closeByDistanceAction = closeByDistanceAction;
        }

        private void ClearActions()
        {
            _showOverviewAction = null;
            _showWardAction = null;
            _showTerritoryAction = null;
            _showBiomeDominionAction = null;
            _showEconomyAction = null;
            _showTerraformingAction = null;
            _openTreasuryChestAction = null;
            _toggleProtectionAction = null;
            _decreaseRadiusAction = null;
            _increaseRadiusAction = null;
            _renameTerritoryAction = null;
            _removePermittedPlayerAction = null;
            _toggleSelfPermissionAction = null;
            _toggleDoorLockAction = null;
            _decreaseDoorAutoCloseAction = null;
            _increaseDoorAutoCloseAction = null;
            _toggleStructureDamageProtectionAction = null;
            _toggleTerraformingAction = null;
            _toggleTerraformingRunningAction = null;
            _openTerraformingPreparationChestAction = null;
            _decreaseTerraformingRadiusAction = null;
            _increaseTerraformingRadiusAction = null;
            _storeTerraformingHoeAction = null;
            _storeTerraformingPickaxeAction = null;
            _addTerraformingFuelSlotAction = null;
            _addTerraformingStoneSlotAction = null;
            _claimBiomeDominionAction = null;
            _releaseBiomeDominionAction = null;
            _toggleBiomeDoorLockAction = null;
            _decreaseBiomeDoorAutoCloseAction = null;
            _increaseBiomeDoorAutoCloseAction = null;
            _toggleBiomeStructureDamageProtectionAction = null;
            _economyDepositAction = null;
            _economyWithdrawAction = null;
            _economyUpkeepAction = null;
            _economyTaxAction = null;
            _economyTransferAction = null;
            _diplomacyAllyAction = null;
            _diplomacyEnemyAction = null;
            _diplomacyVassalAction = null;
            _diplomacyNeutralAction = null;
            _closeByInputAction = null;
            _closeByDistanceAction = null;
        }

        private static string BuildMenuTitle(WardMenuModel model)
        {
            if (model == null ||
                model.Ward == null ||
                string.IsNullOrEmpty(model.Ward.CreatorGuildName))
            {
                return CtLocalization.Get("ct.menu.title.default");
            }

            return CtLocalization.Format(
                "ct.menu.title.guild",
                model.Ward.CreatorGuildName);
        }


        private void ApplyModel(WardMenuModel model)
        {
            _lastModel = model;

            string radiusText = FormatRadius(model.Ward.Radius);
            string protectionText = FormatProtection(model.Ward.Enabled);
            string doorText = FormatDoorLock(model);
            bool ownerMode = model.Ward.IsCurrentPlayerCreator;
            bool selfPermissionMode = !ownerMode && !model.Ward.Enabled;

            _titleText.text = BuildMenuTitle(model);
            _subtitleText.text = CtLocalization.Format(
                "ct.menu.subtitle",
                radiusText,
                protectionText);

            _overviewText.alignment = _showClanOverview
                ? TextAnchor.MiddleCenter
                : TextAnchor.UpperLeft;

            _overviewText.text =
                _showClanOverview
                    ? FormatClanOverview(model)
                    : FormatWardOverview(
                        model,
                        radiusText,
                        protectionText,
                        doorText);

            SetButtonText(
                _clanOverviewButton,
                _showClanOverview
                    ? CtLocalization.Get("ct.menu.button.overview")
                    : CtLocalization.Get("ct.menu.button.clan"));

            SetButtonActive(
                _clanOverviewButton,
                HasClanInfo(model));

            SetDiplomacyButtonsActive(model);

            _wardText.text =
                CtLocalization.Get("ct.menu.ward.title") + "\n\n" +
                CtLocalization.Get("ct.menu.field.protection") + ": " + protectionText + "\n" +
                CtLocalization.Get("ct.menu.field.radius") + ": " + radiusText + " m\n" +
                CtLocalization.Get("ct.menu.field.permitted_players") + ": " + model.Ward.PermittedPlayers.Count + "\n" +
                CtLocalization.Get("ct.menu.field.your_access") + ": " + FormatCurrentAccess(model);

            _territoryText.text =
                CtLocalization.Get("ct.menu.territory.title") + "\n\n" +
                CtLocalization.Get("ct.menu.field.name") + ":\n" + model.Territory.Name + "\n\n" +
                CtLocalization.Get("ct.menu.field.doors") + ": " + doorText + "\n" +
                CtLocalization.Get("ct.menu.field.structures") + ": " + FormatStructures(model.Territory.StructureDamageProtectionEnabled) + "\n\n" +
                CtLocalization.Get("ct.menu.field.guild_access") + ": " + FormatEnabled(model.Territory.GuildAccessEnabled) + "\n" +
                CtLocalization.Get("ct.menu.field.group_access") + ": " + FormatEnabled(model.Territory.GroupAccessEnabled);

            if (_biomeDominionText != null)
            {
                _biomeDominionText.text =
                    CtLocalization.Get("ct.menu.biome.title") + "\n\n" +
                    CtLocalization.Get("ct.menu.field.biome") + ": " + FormatBiomeName(model) + "\n" +
                    CtLocalization.Get("ct.menu.field.status") + ": " + FormatBiomeDominionStatus(model) + "\n" +
                    CtLocalization.Get("ct.menu.field.owner_guild") + ": " + FormatBiomeOwner(model) + "\n" +
                    CtLocalization.Get("ct.menu.field.vassal_status") + ": " + FormatBiomeVassal(model) + "\n\n" +
                    CtLocalization.Get("ct.menu.field.doors") + ": " + FormatBiomeDoorLock(model) + "\n" +
                    CtLocalization.Get("ct.menu.field.structures") + ": " + FormatBiomeStructures(model);
            }

            if (_economyText != null)
                _economyText.text = FormatEconomy(model);

            _terraformingText.text =
                CtLocalization.Get("ct.menu.leveling.title") + "\n\n" +
                CtLocalization.Get("ct.menu.field.status") + ": " + FormatTerraformingStatus(model.Terraforming.Status) + "\n" +
                CtLocalization.Get("ct.menu.field.target") + ": " + CtLocalization.Format("ct.menu.value.ward_height", FormatHeight(model.Terraforming.TargetHeight)) + "\n" +
                CtLocalization.Get("ct.menu.field.work_radius") + ": " + FormatRadius(model.Terraforming.Radius) + " m\n" +
                CtLocalization.Get("ct.menu.field.tools") + ": " +
                    FormatSlot(model.Terraforming.HoeStored, CtLocalization.Get("ct.menu.tool.hoe")) + " / " +
                    FormatSlot(model.Terraforming.PickaxeStored, CtLocalization.Get("ct.menu.tool.pickaxe")) + " / " +
                    FormatSlot(model.Terraforming.AxeStored, CtLocalization.Get("ct.menu.tool.axe")) + "\n" +
                CtLocalization.Get("ct.menu.field.fuel") + ": " + model.Terraforming.FuelStored + " / 2500\n" +
                CtLocalization.Get("ct.menu.field.stone") + ": " + model.Terraforming.StoneStored + " / 2500\n" +
                CtLocalization.Get("ct.menu.field.scan") + ": " + FormatAmount(model.Terraforming.ScanProgress) + " / index " + model.Terraforming.ScanIndex + "\n\n" +
                CtLocalization.Get("ct.menu.preparation.note");

            if (_radiusValueText != null)
                _radiusValueText.text = radiusText + " m";

            if (_doorAutoCloseValueText != null)
                _doorAutoCloseValueText.text = model.Territory.DoorAutoCloseSeconds + "s";

            if (_biomeDoorAutoCloseValueText != null)
                _biomeDoorAutoCloseValueText.text = model.BiomeDominion != null
                    ? model.BiomeDominion.DoorAutoCloseSeconds + "s"
                    : "";

            if (_terraformingRadiusValueText != null)
                _terraformingRadiusValueText.text = FormatRadius(model.Terraforming.Radius) + " m";

            SetButtonText(
                _toggleProtectionButton,
                model.Ward.Enabled
                    ? CtLocalization.Get("ct.menu.button.disable_protection")
                    : CtLocalization.Get("ct.menu.button.enable_protection"));

            SetButtonText(
                _toggleSelfPermissionButton,
                model.Ward.IsCurrentPlayerPermitted
                    ? CtLocalization.Get("ct.menu.button.remove_me")
                    : CtLocalization.Get("ct.menu.button.add_me"));

            SetButtonText(
                _toggleDoorLockButton,
                model.Territory.DoorLockEnabled
                    ? CtLocalization.Get("ct.menu.button.unlock_doors")
                    : CtLocalization.Get("ct.menu.button.lock_doors"));

            SetButtonText(
                _toggleStructureDamageProtectionButton,
                model.Territory.StructureDamageProtectionEnabled
                    ? CtLocalization.Get("ct.menu.button.disable_structure_protection")
                    : CtLocalization.Get("ct.menu.button.enable_structure_protection"));

            SetButtonText(_claimBiomeDominionButton, CtLocalization.Get("ct.menu.button.claim_biome"));
            SetButtonText(_releaseBiomeDominionButton, CtLocalization.Get("ct.menu.button.release_biome"));
            SetButtonText(
                _toggleBiomeDoorLockButton,
                model.BiomeDominion != null && model.BiomeDominion.DoorLockEnabled
                    ? CtLocalization.Get("ct.menu.button.unlock_biome_doors")
                    : CtLocalization.Get("ct.menu.button.lock_biome_doors"));
            SetButtonText(
                _toggleBiomeStructureDamageProtectionButton,
                model.BiomeDominion != null && model.BiomeDominion.StructureDamageProtectionEnabled
                    ? CtLocalization.Get("ct.menu.button.disable_biome_structure_protection")
                    : CtLocalization.Get("ct.menu.button.enable_biome_structure_protection"));

            SetButtonText(_decreaseRadiusButton, "-5");
            SetButtonText(_increaseRadiusButton, "+5");
            SetButtonText(_decreaseDoorAutoCloseButton, "-1");
            SetButtonText(_increaseDoorAutoCloseButton, "+1");
            SetButtonText(
                _toggleTerraformingButton,
                model.Terraforming.Enabled
                    ? CtLocalization.Get("ct.menu.button.disable_leveling")
                    : CtLocalization.Get("ct.menu.button.enable_leveling"));
            SetButtonText(
                _toggleTerraformingRunningButton,
                model.Terraforming.Running
                    ? CtLocalization.Get("ct.menu.button.stop_leveling")
                    : CtLocalization.Get("ct.menu.button.start_leveling"));
            SetButtonText(_openTerraformingPreparationButton, CtLocalization.Get("ct.menu.button.open_preparation"));
            SetButtonText(_decreaseTerraformingRadiusButton, "-2");
            SetButtonText(_increaseTerraformingRadiusButton, "+2");
            SetButtonText(
                _storeTerraformingHoeButton,
                model.Terraforming.HoeStored
                    ? CtLocalization.Get("ct.menu.button.hoe_set")
                    : CtLocalization.Get("ct.menu.button.hoe_slot"));
            SetButtonText(
                _storeTerraformingPickaxeButton,
                model.Terraforming.PickaxeStored
                    ? CtLocalization.Get("ct.menu.button.pickaxe_set")
                    : CtLocalization.Get("ct.menu.button.pickaxe_slot"));
            UpdateTerraformingStorageSlots(model);

            SetButtonActive(_economyDepositButton, model.Economy != null && model.Economy.CanDeposit);
            SetButtonActive(_economyWithdrawButton, model.Economy != null && model.Economy.CanWithdraw);
            SetButtonActive(_economyUpkeepButton, model.Economy != null && model.Economy.CanPayUpkeep);
            SetButtonActive(_economyTaxButton, model.Economy != null && model.Economy.CanPayTax);
            SetButtonActive(_economyTransferButton, model.Economy != null && model.Economy.CanTransfer);

            SetButtonActive(_toggleProtectionButton, ownerMode);
            SetButtonActive(_decreaseRadiusButton, ownerMode);
            SetButtonActive(_increaseRadiusButton, ownerMode);
            SetTextActive(_radiusValueText, ownerMode);
            SetButtonActive(_renameTerritoryButton, ownerMode);
            SetButtonActive(_toggleDoorLockButton, ownerMode);
            SetButtonActive(_decreaseDoorAutoCloseButton, ownerMode);
            SetButtonActive(_increaseDoorAutoCloseButton, ownerMode);
            SetTextActive(_doorAutoCloseValueText, ownerMode);
            SetButtonActive(_toggleStructureDamageProtectionButton, ownerMode);

            bool biomeCanClaim =
                model.BiomeDominion != null &&
                model.BiomeDominion.CanClaim;
            bool biomeCanManage =
                model.BiomeDominion != null &&
                model.BiomeDominion.CanManage;

            SetButtonActive(_claimBiomeDominionButton, biomeCanClaim);
            SetButtonActive(_releaseBiomeDominionButton, biomeCanManage);
            SetButtonActive(_toggleBiomeDoorLockButton, biomeCanManage);
            SetButtonActive(_decreaseBiomeDoorAutoCloseButton, biomeCanManage);
            SetButtonActive(_increaseBiomeDoorAutoCloseButton, biomeCanManage);
            SetTextActive(_biomeDoorAutoCloseValueText, biomeCanManage);
            SetButtonActive(_toggleBiomeStructureDamageProtectionButton, biomeCanManage);

            SetButtonActive(_toggleTerraformingButton, ownerMode);
            SetButtonActive(_toggleTerraformingRunningButton, ownerMode);
            SetButtonActive(_openTerraformingPreparationButton, ownerMode);
            SetButtonActive(_decreaseTerraformingRadiusButton, ownerMode);
            SetButtonActive(_increaseTerraformingRadiusButton, ownerMode);
            SetButtonActive(_storeTerraformingHoeButton, ownerMode);
            SetButtonActive(_storeTerraformingPickaxeButton, ownerMode);
            SetStorageSlotButtonsActive(ownerMode);
            SetTextActive(_terraformingRadiusValueText, ownerMode);
            SetButtonActive(_toggleSelfPermissionButton, selfPermissionMode);

            BuildPermittedPlayerRows(
                model,
                ownerMode);
        }


        private void EnsureCreated()
        {
            if (_root != null)
                return;

            BuildGui();
        }

        private void BuildGui()
        {
            if (GUIManager.CustomGUIFront == null)
                return;

            if (_root != null)
                UnityEngine.Object.Destroy(_root);

            GUIManager gui = GUIManager.Instance;

            _root = new GameObject(
                "ClanTerritory_JotunnWardMenu",
                typeof(RectTransform),
                typeof(Image));

            _root.transform.SetParent(GUIManager.CustomGUIFront.transform, false);

            RectTransform rootRect = _root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.anchoredPosition = Vector2.zero;

            Image rootImage = _root.GetComponent<Image>();
            rootImage.color = new Color(0f, 0f, 0f, 0.55f);
            rootImage.raycastTarget = true;

            _panel = gui.CreateWoodpanel(
                _root.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                860f,
                580f,
                false);

            _panel.name = "ClanTerritory_JotunnWardPanel";

            _titleText = CreateLabel(
                CtLocalization.Get("ct.menu.title.default"),
                new Vector2(0f, 240f),
                30,
                700f,
                44f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerifBold,
                gui.ValheimOrange);

            _subtitleText = CreateLabel(
                "",
                new Vector2(0f, 205f),
                18,
                700f,
                32f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _openTreasuryButton = CreateButton(CtLocalization.Get("ct.menu.button.treasury"), new Vector2(0f, 178f), 180f, 30f);

            _overviewButton = CreateButton(CtLocalization.Get("ct.menu.tab.overview"), new Vector2(-330f, 138f), 112f, 38f);
            _wardButton = CreateButton(CtLocalization.Get("ct.menu.tab.ward"), new Vector2(-198f, 138f), 112f, 38f);
            _territoryButton = CreateButton(CtLocalization.Get("ct.menu.tab.territory"), new Vector2(-66f, 138f), 112f, 38f);
            _economyButton = CreateButton(CtLocalization.Get("ct.menu.tab.economy"), new Vector2(66f, 138f), 112f, 38f);
            _biomeDominionButton = CreateButton(CtLocalization.Get("ct.menu.tab.biome"), new Vector2(198f, 138f), 112f, 38f);
            _terraformingButton = CreateButton(CtLocalization.Get("ct.menu.tab.terraforming"), new Vector2(330f, 138f), 112f, 38f);

            _overviewPanel = CreatePanelRoot("OverviewPanel");
            _wardPanel = CreatePanelRoot("WardPanel");
            _territoryPanel = CreatePanelRoot("TerritoryPanel");
            _biomeDominionPanel = CreatePanelRoot("BiomeDominionPanel");
            _economyPanel = CreatePanelRoot("EconomyPanel");
            _terraformingPanel = CreatePanelRoot("TerraformingPanel");
            _terraformingStoragePanel = CreatePanelRoot("TerraformingPreparationChest");

            _overviewText = CreateLabel(
                "",
                new Vector2(0f, -25f),
                20,
                650f,
                250f,
                TextAnchor.UpperLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _overviewText.transform.SetParent(_overviewPanel.transform, false);

            _clanOverviewButton = CreateButton(
                _overviewPanel.transform,
                CtLocalization.Get("ct.menu.button.clan"),
                new Vector2(0f, -218f),
                220f,
                30f);

            _diplomacyAllyButton = CreateButton(_overviewPanel.transform, CtLocalization.Get("ct.menu.diplomacy.ally"), new Vector2(-255f, -178f), 120f, 30f);
            _diplomacyEnemyButton = CreateButton(_overviewPanel.transform, CtLocalization.Get("ct.menu.diplomacy.enemy"), new Vector2(-85f, -178f), 120f, 30f);
            _diplomacyVassalButton = CreateButton(_overviewPanel.transform, CtLocalization.Get("ct.menu.diplomacy.vassal"), new Vector2(85f, -178f), 120f, 30f);
            _diplomacyNeutralButton = CreateButton(_overviewPanel.transform, CtLocalization.Get("ct.menu.diplomacy.neutral"), new Vector2(255f, -178f), 120f, 30f);

            _wardText = CreateLabel(
                "",
                new Vector2(0f, 76f),
                19,
                650f,
                112f,
                TextAnchor.UpperLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _wardText.transform.SetParent(_wardPanel.transform, false);

            _toggleProtectionButton = CreateButton(_wardPanel.transform, CtLocalization.Get("ct.menu.button.toggle_protection"), new Vector2(0f, -115f), 240f, 38f);
            _toggleSelfPermissionButton = CreateButton(_wardPanel.transform, CtLocalization.Get("ct.menu.button.add_me"), new Vector2(0f, -115f), 240f, 38f);
            _decreaseRadiusButton = CreateButton(_wardPanel.transform, "-5", new Vector2(-130f, -160f), 110f, 38f);

            _radiusValueText = CreateLabel(
                "",
                new Vector2(0f, -160f),
                22,
                120f,
                38f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerifBold,
                gui.ValheimOrange);

            _radiusValueText.transform.SetParent(_wardPanel.transform, false);

            _increaseRadiusButton = CreateButton(_wardPanel.transform, "+5", new Vector2(130f, -160f), 110f, 38f);

            _territoryText = CreateLabel(
                "",
                new Vector2(0f, 45f),
                20,
                650f,
                150f,
                TextAnchor.UpperLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _territoryText.transform.SetParent(_territoryPanel.transform, false);

            _toggleDoorLockButton = CreateButton(_territoryPanel.transform, CtLocalization.Get("ct.menu.button.lock_doors"), new Vector2(0f, -62f), 280f, 30f);
            _decreaseDoorAutoCloseButton = CreateButton(_territoryPanel.transform, "-1", new Vector2(-104f, -96f), 76f, 28f);

            _doorAutoCloseValueText = CreateLabel(
                "",
                new Vector2(0f, -96f),
                20,
                94f,
                28f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerifBold,
                gui.ValheimOrange);

            _doorAutoCloseValueText.transform.SetParent(_territoryPanel.transform, false);

            _increaseDoorAutoCloseButton = CreateButton(_territoryPanel.transform, "+1", new Vector2(104f, -96f), 76f, 28f);
            _toggleStructureDamageProtectionButton = CreateButton(_territoryPanel.transform, CtLocalization.Get("ct.menu.button.enable_structure_protection"), new Vector2(0f, -132f), 280f, 30f);
            _renameTerritoryButton = CreateButton(_territoryPanel.transform, CtLocalization.Get("ct.menu.button.rename_territory"), new Vector2(0f, -168f), 280f, 30f);

            _biomeDominionText = CreateLabel(
                "",
                new Vector2(0f, 55f),
                18,
                650f,
                155f,
                TextAnchor.UpperLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _biomeDominionText.transform.SetParent(_biomeDominionPanel.transform, false);

            _claimBiomeDominionButton = CreateButton(_biomeDominionPanel.transform, CtLocalization.Get("ct.menu.button.claim_biome"), new Vector2(-180f, -55f), 220f, 30f);
            _releaseBiomeDominionButton = CreateButton(_biomeDominionPanel.transform, CtLocalization.Get("ct.menu.button.release_biome"), new Vector2(180f, -55f), 220f, 30f);
            _toggleBiomeDoorLockButton = CreateButton(_biomeDominionPanel.transform, CtLocalization.Get("ct.menu.button.lock_biome_doors"), new Vector2(-180f, -94f), 220f, 30f);
            _decreaseBiomeDoorAutoCloseButton = CreateButton(_biomeDominionPanel.transform, "-1", new Vector2(74f, -94f), 64f, 28f);

            _biomeDoorAutoCloseValueText = CreateLabel(
                "",
                new Vector2(150f, -94f),
                18,
                82f,
                28f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerifBold,
                gui.ValheimOrange);

            _biomeDoorAutoCloseValueText.transform.SetParent(_biomeDominionPanel.transform, false);

            _increaseBiomeDoorAutoCloseButton = CreateButton(_biomeDominionPanel.transform, "+1", new Vector2(226f, -94f), 64f, 28f);
            _toggleBiomeStructureDamageProtectionButton = CreateButton(_biomeDominionPanel.transform, CtLocalization.Get("ct.menu.button.enable_biome_structure_protection"), new Vector2(0f, -134f), 320f, 30f);


            _economyText = CreateLabel(
                "",
                new Vector2(0f, 55f),
                18,
                650f,
                150f,
                TextAnchor.UpperLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _economyText.transform.SetParent(_economyPanel.transform, false);

            _economyDepositButton = CreateButton(_economyPanel.transform, CtLocalization.Get("ct.menu.button.economy_deposit"), new Vector2(-280f, -85f), 120f, 30f);
            _economyWithdrawButton = CreateButton(_economyPanel.transform, CtLocalization.Get("ct.menu.button.economy_withdraw"), new Vector2(-140f, -85f), 120f, 30f);
            _economyUpkeepButton = CreateButton(_economyPanel.transform, CtLocalization.Get("ct.menu.button.economy_upkeep"), new Vector2(0f, -85f), 120f, 30f);
            _economyTaxButton = CreateButton(_economyPanel.transform, CtLocalization.Get("ct.menu.button.economy_tax"), new Vector2(140f, -85f), 120f, 30f);
            _economyTransferButton = CreateButton(_economyPanel.transform, CtLocalization.Get("ct.menu.button.economy_transfer"), new Vector2(280f, -85f), 120f, 30f);

            _terraformingText = CreateLabel(
                "",
                new Vector2(0f, 52f),
                18,
                650f,
                170f,
                TextAnchor.UpperLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _terraformingText.transform.SetParent(_terraformingPanel.transform, false);

            _toggleTerraformingButton = CreateButton(_terraformingPanel.transform, CtLocalization.Get("ct.menu.button.enable_leveling"), new Vector2(-190f, -82f), 220f, 30f);
            _toggleTerraformingRunningButton = CreateButton(_terraformingPanel.transform, CtLocalization.Get("ct.menu.button.start_leveling"), new Vector2(190f, -82f), 220f, 30f);
            _openTerraformingPreparationButton = CreateButton(_terraformingPanel.transform, CtLocalization.Get("ct.menu.button.open_preparation"), new Vector2(0f, -120f), 280f, 30f);

            _decreaseTerraformingRadiusButton = CreateButton(_terraformingPanel.transform, "-2", new Vector2(-150f, -158f), 76f, 28f);

            _terraformingRadiusValueText = CreateLabel(
                "",
                new Vector2(0f, -158f),
                18,
                110f,
                28f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerifBold,
                gui.ValheimOrange);

            _terraformingRadiusValueText.transform.SetParent(_terraformingPanel.transform, false);

            _increaseTerraformingRadiusButton = CreateButton(_terraformingPanel.transform, "+2", new Vector2(150f, -158f), 76f, 28f);

            BuildTerraformingPreparationChest(gui);

            _closeButton = CreateButton(CtLocalization.Get("ct.menu.button.close"), new Vector2(0f, -265f), 180f, 34f);

            _overviewButton.onClick.AddListener(RequestShowOverview);
            _wardButton.onClick.AddListener(RequestShowWard);
            _territoryButton.onClick.AddListener(RequestShowTerritory);
            _economyButton.onClick.AddListener(RequestShowEconomy);
            _biomeDominionButton.onClick.AddListener(RequestShowBiomeDominion);
            _terraformingButton.onClick.AddListener(RequestShowTerraforming);
            _openTreasuryButton.onClick.AddListener(RequestOpenTreasuryChest);
            _clanOverviewButton.onClick.AddListener(RequestToggleClanOverview);
            _diplomacyAllyButton.onClick.AddListener(RequestDiplomacyAlly);
            _diplomacyEnemyButton.onClick.AddListener(RequestDiplomacyEnemy);
            _diplomacyVassalButton.onClick.AddListener(RequestDiplomacyVassal);
            _diplomacyNeutralButton.onClick.AddListener(RequestDiplomacyNeutral);
            _closeButton.onClick.AddListener(RequestCloseByInput);
            _toggleProtectionButton.onClick.AddListener(RequestToggleProtection);
            _toggleSelfPermissionButton.onClick.AddListener(RequestToggleSelfPermission);
            _decreaseRadiusButton.onClick.AddListener(RequestDecreaseRadius);
            _increaseRadiusButton.onClick.AddListener(RequestIncreaseRadius);
            _renameTerritoryButton.onClick.AddListener(RequestRenameTerritory);
            _toggleDoorLockButton.onClick.AddListener(RequestToggleDoorLock);
            _decreaseDoorAutoCloseButton.onClick.AddListener(RequestDecreaseDoorAutoClose);
            _increaseDoorAutoCloseButton.onClick.AddListener(RequestIncreaseDoorAutoClose);
            _toggleStructureDamageProtectionButton.onClick.AddListener(RequestToggleStructureDamageProtection);
            _claimBiomeDominionButton.onClick.AddListener(RequestClaimBiomeDominion);
            _releaseBiomeDominionButton.onClick.AddListener(RequestReleaseBiomeDominion);
            _toggleBiomeDoorLockButton.onClick.AddListener(RequestToggleBiomeDoorLock);
            _decreaseBiomeDoorAutoCloseButton.onClick.AddListener(RequestDecreaseBiomeDoorAutoClose);
            _increaseBiomeDoorAutoCloseButton.onClick.AddListener(RequestIncreaseBiomeDoorAutoClose);
            _toggleBiomeStructureDamageProtectionButton.onClick.AddListener(RequestToggleBiomeStructureDamageProtection);
            _economyDepositButton.onClick.AddListener(RequestEconomyDeposit);
            _economyWithdrawButton.onClick.AddListener(RequestEconomyWithdraw);
            _economyUpkeepButton.onClick.AddListener(RequestEconomyUpkeep);
            _economyTaxButton.onClick.AddListener(RequestEconomyTax);
            _economyTransferButton.onClick.AddListener(RequestEconomyTransfer);
            _toggleTerraformingButton.onClick.AddListener(RequestToggleTerraforming);
            _toggleTerraformingRunningButton.onClick.AddListener(RequestToggleTerraformingRunning);
            _openTerraformingPreparationButton.onClick.AddListener(RequestShowTerraformingPreparationChest);
            _decreaseTerraformingRadiusButton.onClick.AddListener(RequestDecreaseTerraformingRadius);
            _increaseTerraformingRadiusButton.onClick.AddListener(RequestIncreaseTerraformingRadius);
            _storeTerraformingHoeButton.onClick.AddListener(RequestStoreTerraformingHoe);
            _storeTerraformingPickaxeButton.onClick.AddListener(RequestStoreTerraformingPickaxe);

            SetActivePanel(_overviewPanel);
            SetVisible(_visible);
        }

        private void BuildTerraformingPreparationChest(GUIManager gui)
        {
            if (_terraformingStoragePanel == null)
                return;

            Text title = CreateLabel(
                CtLocalization.Get("ct.menu.preparation.title"),
                new Vector2(0f, 105f),
                20,
                620f,
                28f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerifBold,
                gui.ValheimOrange);

            title.transform.SetParent(_terraformingStoragePanel.transform, false);

            Text toolsLabel = CreateLabel(
                CtLocalization.Get("ct.menu.preparation.tools"),
                new Vector2(-300f, 67f),
                16,
                120f,
                24f,
                TextAnchor.MiddleLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            toolsLabel.transform.SetParent(_terraformingStoragePanel.transform, false);

            _storeTerraformingHoeButton = CreateButton(_terraformingStoragePanel.transform, CtLocalization.Get("ct.menu.button.hoe_slot"), new Vector2(-75f, 67f), 155f, 34f);
            _storeTerraformingPickaxeButton = CreateButton(_terraformingStoragePanel.transform, CtLocalization.Get("ct.menu.button.pickaxe_slot"), new Vector2(105f, 67f), 155f, 34f);

            Text fuelLabel = CreateLabel(
                CtLocalization.Get("ct.menu.preparation.fuel_slots"),
                new Vector2(-245f, 23f),
                15,
                220f,
                22f,
                TextAnchor.MiddleLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            fuelLabel.transform.SetParent(_terraformingStoragePanel.transform, false);

            for (int i = 0; i < _terraformingFuelSlotButtons.Length; i++)
            {
                int capturedIndex = i;
                _terraformingFuelSlotButtons[i] = CreateButton(
                    _terraformingStoragePanel.transform,
                    CtLocalization.Format("ct.menu.storage.fuel.short", i + 1),
                    new Vector2(-240f + i * 120f, -15f),
                    108f,
                    34f);

                _terraformingFuelSlotButtons[i].onClick.AddListener(
                    delegate
                    {
                        RequestAddTerraformingFuelSlot(capturedIndex);
                    });
            }

            Text stoneLabel = CreateLabel(
                CtLocalization.Get("ct.menu.preparation.stone_slots"),
                new Vector2(-245f, -61f),
                15,
                220f,
                22f,
                TextAnchor.MiddleLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            stoneLabel.transform.SetParent(_terraformingStoragePanel.transform, false);

            for (int i = 0; i < _terraformingStoneSlotButtons.Length; i++)
            {
                int capturedIndex = i;
                _terraformingStoneSlotButtons[i] = CreateButton(
                    _terraformingStoragePanel.transform,
                    CtLocalization.Format("ct.menu.storage.stone.short", i + 1),
                    new Vector2(-240f + i * 120f, -99f),
                    108f,
                    34f);

                _terraformingStoneSlotButtons[i].onClick.AddListener(
                    delegate
                    {
                        RequestAddTerraformingStoneSlot(capturedIndex);
                    });
            }

            _closeTerraformingPreparationButton = CreateButton(_terraformingStoragePanel.transform, CtLocalization.Get("ct.menu.button.back"), new Vector2(0f, -145f), 140f, 30f);
            _closeTerraformingPreparationButton.onClick.AddListener(RequestCloseTerraformingPreparationChest);
            _storeTerraformingHoeButton.onClick.AddListener(RequestStoreTerraformingHoe);
            _storeTerraformingPickaxeButton.onClick.AddListener(RequestStoreTerraformingPickaxe);
            _terraformingStoragePanel.SetActive(false);
        }

        private GameObject CreatePanelRoot(string name)
        {
            GameObject panelRoot = new GameObject(name, typeof(RectTransform));
            panelRoot.transform.SetParent(_panel.transform, false);

            RectTransform rect = panelRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -25f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 680f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 260f);

            return panelRoot;
        }

        private Text CreateLabel(
            string text,
            Vector2 position,
            int fontSize,
            float width,
            float height,
            TextAnchor alignment,
            Font font,
            Color color)
        {
            Text label = GUIManager.Instance.CreateText(
                text,
                _panel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                font,
                fontSize,
                color,
                true,
                Color.black,
                width,
                height,
                false).GetComponent<Text>();

            label.alignment = alignment;
            return label;
        }

        private Button CreateButton(string text, Vector2 position, float width, float height)
        {
            return CreateButton(_panel.transform, text, position, width, height);
        }

        private Button CreateButton(Transform parent, string text, Vector2 position, float width, float height)
        {
            return GUIManager.Instance.CreateButton(
                text,
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                width,
                height).GetComponent<Button>();
        }

        private void SetVisible(bool visible)
        {
            bool wasVisible = _visible;

            _visible = visible;

            if (_root != null)
                _root.SetActive(visible);

            if (wasVisible == visible)
                return;

            GUIManager.BlockInput(visible);
        }

        private void SetActivePanel(GameObject activePanel)
        {
            if (_overviewPanel != null)
                _overviewPanel.SetActive(activePanel == _overviewPanel);

            if (_wardPanel != null)
                _wardPanel.SetActive(activePanel == _wardPanel);

            if (_territoryPanel != null)
                _territoryPanel.SetActive(activePanel == _territoryPanel);

            if (_biomeDominionPanel != null)
                _biomeDominionPanel.SetActive(activePanel == _biomeDominionPanel);

            if (_economyPanel != null)
                _economyPanel.SetActive(activePanel == _economyPanel);

            if (_terraformingPanel != null)
                _terraformingPanel.SetActive(activePanel == _terraformingPanel);

            if (_terraformingStoragePanel != null)
                _terraformingStoragePanel.SetActive(false);
        }

        private void BuildPermittedPlayerRows(WardMenuModel model, bool allowRemove)
        {
            ClearPermittedPlayerRows();

            if (_wardPanel == null)
                return;

            if (model == null || model.Ward.PermittedPlayers.Count <= 0)
                return;

            GUIManager gui = GUIManager.Instance;

            int visibleCount = Mathf.Min(model.Ward.PermittedPlayers.Count, MaxVisiblePermittedPlayers);

            const float startY = 8f;
            const float rowSpacing = 27f;

            for (int i = 0; i < visibleCount; i++)
            {
                WardMenuPlayerModel player = model.Ward.PermittedPlayers[i];
                float y = startY - i * rowSpacing;

                Text playerText = CreateLabel(
                    player.PlayerName + " (" + player.PlayerId + ")",
                    new Vector2(allowRemove ? -115f : 0f, y),
                    16,
                    allowRemove ? 390f : 620f,
                    24f,
                    allowRemove ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter,
                    gui.AveriaSerif,
                    gui.ValheimBeige);

                playerText.transform.SetParent(_wardPanel.transform, false);
                _permittedPlayerRowObjects.Add(playerText.gameObject);

                if (!allowRemove)
                    continue;

                Button removeButton = CreateButton(_wardPanel.transform, CtLocalization.Get("ct.menu.button.remove"), new Vector2(235f, y), 115f, 28f);

                long capturedPlayerId = player.PlayerId;

                removeButton.onClick.AddListener(
                    delegate
                    {
                        RequestRemovePermittedPlayer(capturedPlayerId);
                    });

                _permittedPlayerRowObjects.Add(removeButton.gameObject);
            }

            if (model.Ward.PermittedPlayers.Count > MaxVisiblePermittedPlayers)
            {
                int remainingCount = model.Ward.PermittedPlayers.Count - MaxVisiblePermittedPlayers;
                float y = startY - visibleCount * rowSpacing;

                Text overflowText = CreateLabel(
                    CtLocalization.Format("ct.menu.more_players", remainingCount),
                    new Vector2(allowRemove ? -115f : 0f, y),
                    16,
                    allowRemove ? 390f : 620f,
                    24f,
                    allowRemove ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter,
                    gui.AveriaSerif,
                    gui.ValheimBeige);

                overflowText.transform.SetParent(_wardPanel.transform, false);
                _permittedPlayerRowObjects.Add(overflowText.gameObject);
            }
        }

        private void ClearPermittedPlayerRows()
        {
            for (int i = 0; i < _permittedPlayerRowObjects.Count; i++)
            {
                GameObject rowObject = _permittedPlayerRowObjects[i];

                if (rowObject != null)
                    UnityEngine.Object.Destroy(rowObject);
            }

            _permittedPlayerRowObjects.Clear();
        }

        private void UpdateTerraformingStorageSlots(WardMenuModel model)
        {
            if (model == null || model.Terraforming == null)
                return;

            for (int i = 0; i < _terraformingFuelSlotButtons.Length; i++)
            {
                int value = GetSlotValue(model.Terraforming.FuelSlots, i);

                SetButtonText(
                    _terraformingFuelSlotButtons[i],
                    CtLocalization.Format("ct.menu.storage.fuel.value", i + 1, value));
            }

            for (int i = 0; i < _terraformingStoneSlotButtons.Length; i++)
            {
                int value = GetSlotValue(model.Terraforming.StoneSlots, i);

                SetButtonText(
                    _terraformingStoneSlotButtons[i],
                    CtLocalization.Format("ct.menu.storage.stone.value", i + 1, value));
            }
        }

        private static int GetSlotValue(int[] slots, int index)
        {
            if (slots == null || index < 0 || index >= slots.Length)
                return 0;

            return slots[index];
        }

        private void SetStorageSlotButtonsActive(bool active)
        {
            for (int i = 0; i < _terraformingFuelSlotButtons.Length; i++)
                SetButtonActive(_terraformingFuelSlotButtons[i], active);

            for (int i = 0; i < _terraformingStoneSlotButtons.Length; i++)
                SetButtonActive(_terraformingStoneSlotButtons[i], active);
        }

        private static string FormatRadius(float radius)
        {
            return Mathf.RoundToInt(radius).ToString();
        }

        private static string FormatProtection(bool enabled)
        {
            return FormatEnabled(enabled);
        }

        private static string FormatEnabled(bool enabled)
        {
            return enabled
                ? CtLocalization.Get("ct.menu.value.enabled")
                : CtLocalization.Get("ct.menu.value.disabled");
        }

        private static string FormatDoorLock(WardMenuModel model)
        {
            if (!model.Territory.DoorLockEnabled)
                return CtLocalization.Get("ct.menu.value.unlocked");

            return CtLocalization.Format(
                "ct.menu.value.locked_auto_close",
                model.Territory.DoorAutoCloseSeconds);
        }

        private static string FormatStructures(bool protectedStructures)
        {
            return protectedStructures
                ? CtLocalization.Get("ct.menu.value.protected")
                : CtLocalization.Get("ct.menu.value.vulnerable");
        }

        private static string FormatTerraformingStatus(string status)
        {
            if (string.Equals(status, "Disabled", StringComparison.OrdinalIgnoreCase))
                return CtLocalization.Get("ct.menu.value.disabled");

            if (string.Equals(status, "Running", StringComparison.OrdinalIgnoreCase))
                return CtLocalization.Get("ct.menu.value.running");

            if (string.Equals(status, "Ready", StringComparison.OrdinalIgnoreCase))
                return CtLocalization.Get("ct.menu.value.ready");

            if (string.Equals(status, "Terraforming service unavailable", StringComparison.OrdinalIgnoreCase))
                return CtLocalization.Get("ct.status.terraforming_unavailable");

            return status ?? "";
        }

        private static string FormatHeight(float height)
        {
            return height.ToString("0.0") + " m";
        }

        private static string FormatAmount(float amount)
        {
            return amount.ToString("0.0");
        }

        private static string FormatSlot(bool stored, string name)
        {
            return stored
                ? name
                : CtLocalization.Get("ct.menu.value.empty");
        }

        private static string FormatCurrentAccess(WardMenuModel model)
        {
            if (model.Ward.IsCurrentPlayerCreator)
                return CtLocalization.Get("ct.menu.value.owner");

            if (model.Ward.IsCurrentPlayerPermitted)
                return CtLocalization.Get("ct.menu.value.permitted");

            return CtLocalization.Get("ct.menu.value.guest");
        }



        private static string FormatWardOverview(
            WardMenuModel model,
            string radiusText,
            string protectionText,
            string doorText)
        {
            return CtLocalization.Get("ct.menu.overview.title") + "\n\n" +
                   CtLocalization.Get("ct.menu.field.territory") + ": " + model.Territory.Name + "\n" +
                   CtLocalization.Get("ct.menu.field.ward_id") + ": " + model.Ward.WardId + "\n" +
                   CtLocalization.Get("ct.menu.field.owner") + ": " + model.Ward.OwnerName + "\n" +
                   FormatClanLine(model) +
                   CtLocalization.Get("ct.menu.field.radius") + ": " + radiusText + " m\n" +
                   CtLocalization.Get("ct.menu.field.protection") + ": " + protectionText + "\n" +
                   CtLocalization.Get("ct.menu.field.your_access") + ": " + FormatCurrentAccess(model) + "\n" +
                   CtLocalization.Get("ct.menu.field.doors") + ": " + doorText + "\n" +
                   CtLocalization.Get("ct.menu.field.structures") + ": " + FormatStructures(model.Territory.StructureDamageProtectionEnabled) + "\n" +
                   CtLocalization.Get("ct.menu.field.biome") + ": " + FormatBiomeDominionOverview(model);
        }

        private static string FormatClanLine(WardMenuModel model)
        {
            if (!HasClanInfo(model))
                return "";

            return CtLocalization.Get("ct.menu.field.clan") + ": " + model.Ward.CreatorGuildName + "\n";
        }

        private static string FormatClanOverview(WardMenuModel model)
        {
            if (!HasClanInfo(model))
                return CtLocalization.Get("ct.menu.clan.no_clan");

            string description =
                model.Ward.CreatorGuildDescription;

            if (string.IsNullOrEmpty(description))
                description = CtLocalization.Get("ct.menu.clan.description_unavailable");

            return CtLocalization.Get("ct.menu.clan.description_title") + "\n\n" +
                   model.Ward.CreatorGuildName + "\n\n" +
                   description + "\n\n" +
                   FormatDiplomacy(model);
        }

        private static bool HasClanInfo(WardMenuModel model)
        {
            return model != null &&
                   model.Ward != null &&
                   !string.IsNullOrEmpty(model.Ward.CreatorGuildName);
        }


        private static string FormatDiplomacy(WardMenuModel model)
        {
            if (model == null || model.Diplomacy == null)
                return CtLocalization.Get("ct.menu.diplomacy.title") + "\n" +
                       CtLocalization.Get("ct.menu.value.unavailable");

            WardMenuDiplomacySection diplomacy = model.Diplomacy;

            if (!diplomacy.Available)
            {
                return CtLocalization.Get("ct.menu.diplomacy.title") + "\n" +
                       (string.IsNullOrEmpty(diplomacy.StatusText)
                           ? CtLocalization.Get("ct.menu.value.unavailable")
                           : diplomacy.StatusText);
            }

            return CtLocalization.Get("ct.menu.diplomacy.title") + "\n" +
                   CtLocalization.Get("ct.menu.field.guild") + ": " + diplomacy.GuildName + "\n" +
                   CtLocalization.Get("ct.menu.diplomacy.relations") + ":\n" +
                   (string.IsNullOrEmpty(diplomacy.RelationsText)
                       ? CtLocalization.Get("ct.diplomacy.menu.no_relations")
                       : diplomacy.RelationsText);
        }

        private void SetDiplomacyButtonsActive(WardMenuModel model)
        {
            bool active =
                _showClanOverview &&
                model != null &&
                model.Diplomacy != null &&
                model.Diplomacy.Available &&
                model.Diplomacy.CanChange;

            SetButtonActive(_diplomacyAllyButton, active);
            SetButtonActive(_diplomacyEnemyButton, active);
            SetButtonActive(_diplomacyVassalButton, active);
            SetButtonActive(_diplomacyNeutralButton, active);
        }


        private static string FormatEconomy(WardMenuModel model)
        {
            if (model == null || model.Economy == null)
                return CtLocalization.Get("ct.menu.value.unavailable");

            WardMenuEconomySection economy = model.Economy;

            if (!economy.Available)
            {
                return CtLocalization.Get("ct.menu.economy.title") + "\n\n" +
                       (string.IsNullOrEmpty(economy.StatusText)
                           ? CtLocalization.Get("ct.menu.value.unavailable")
                           : economy.StatusText);
            }

            return CtLocalization.Get("ct.menu.economy.title") + "\n\n" +
                   CtLocalization.Get("ct.menu.field.guild") + ": " + economy.GuildName + "\n" +
                   CtLocalization.Get("ct.menu.field.balance") + ": " + economy.Balance + "\n" +
                   CtLocalization.Get("ct.menu.field.territory_guild") + ": " + FormatEconomyTerritoryGuild(economy) + "\n\n" +
                   CtLocalization.Get("ct.menu.field.deposited") + ": " + economy.DepositedTotal + "\n" +
                   CtLocalization.Get("ct.menu.field.withdrawn") + ": " + economy.WithdrawnTotal + "\n" +
                   CtLocalization.Get("ct.menu.field.upkeep_paid") + ": " + economy.UpkeepPaidTotal + "\n" +
                   CtLocalization.Get("ct.menu.field.tribute_received") + ": " + economy.TributeReceivedTotal + "\n" +
                   CtLocalization.Get("ct.menu.field.taxes") + ": " + economy.TaxPaidTotal + " / " + economy.TaxReceivedTotal + "\n" +
                   CtLocalization.Get("ct.menu.field.transfers") + ": " + economy.TransferSentTotal + " / " + economy.TransferReceivedTotal;
        }

        private static string FormatEconomyTerritoryGuild(WardMenuEconomySection economy)
        {
            if (economy == null || string.IsNullOrEmpty(economy.TerritoryGuildName))
                return CtLocalization.Get("ct.menu.value.none");

            return economy.TerritoryGuildName;
        }

        private static string FormatBiomeDominionOverview(WardMenuModel model)
        {
            if (model == null || model.BiomeDominion == null)
                return CtLocalization.Get("ct.menu.value.unavailable");

            if (!model.BiomeDominion.Claimed)
            {
                return CtLocalization.Format(
                    "ct.menu.biome.overview.free",
                    FormatBiomeName(model));
            }

            return CtLocalization.Format(
                "ct.menu.biome.overview.claimed",
                FormatBiomeName(model),
                FormatBiomeOwner(model));
        }

        private static string FormatBiomeName(WardMenuModel model)
        {
            if (model == null ||
                model.BiomeDominion == null ||
                string.IsNullOrEmpty(model.BiomeDominion.BiomeName))
            {
                return CtLocalization.Get("ct.menu.value.unknown");
            }

            return model.BiomeDominion.BiomeName;
        }

        private static string FormatBiomeDominionStatus(WardMenuModel model)
        {
            if (model == null || model.BiomeDominion == null)
                return CtLocalization.Get("ct.menu.value.unavailable");

            if (!model.BiomeDominion.Claimed)
                return CtLocalization.Get("ct.menu.biome.status.free");

            return CtLocalization.Get("ct.menu.biome.status.claimed");
        }

        private static string FormatBiomeOwner(WardMenuModel model)
        {
            if (model == null ||
                model.BiomeDominion == null ||
                string.IsNullOrEmpty(model.BiomeDominion.OwnerGuildName))
            {
                return CtLocalization.Get("ct.menu.value.none");
            }

            return model.BiomeDominion.OwnerGuildName;
        }

        private static string FormatBiomeVassal(WardMenuModel model)
        {
            if (model == null ||
                model.BiomeDominion == null ||
                !model.BiomeDominion.Claimed)
            {
                return CtLocalization.Get("ct.menu.value.none");
            }

            return model.BiomeDominion.Vassal
                ? CtLocalization.Get("ct.menu.biome.vassal.yes")
                : CtLocalization.Get("ct.menu.biome.vassal.no");
        }

        private static string FormatBiomeDoorLock(WardMenuModel model)
        {
            if (model == null ||
                model.BiomeDominion == null ||
                !model.BiomeDominion.DoorLockEnabled)
            {
                return CtLocalization.Get("ct.menu.value.unlocked");
            }

            return CtLocalization.Format(
                "ct.menu.value.locked_auto_close",
                model.BiomeDominion.DoorAutoCloseSeconds);
        }

        private static string FormatBiomeStructures(WardMenuModel model)
        {
            if (model == null || model.BiomeDominion == null)
                return CtLocalization.Get("ct.menu.value.vulnerable");

            return FormatStructures(model.BiomeDominion.StructureDamageProtectionEnabled);
        }

        private static void SetButtonText(Button button, string text)
        {
            if (button == null)
                return;

            Text buttonText = button.GetComponentInChildren<Text>();

            if (buttonText != null)
                buttonText.text = text;
        }

        private static void SetButtonActive(Button button, bool active)
        {
            if (button != null)
                button.gameObject.SetActive(active);
        }

        private static void SetTextActive(Text text, bool active)
        {
            if (text != null)
                text.gameObject.SetActive(active);
        }

        private void RequestDiplomacyAlly()
        {
            if (_diplomacyAllyAction != null)
                _diplomacyAllyAction();
        }

        private void RequestDiplomacyEnemy()
        {
            if (_diplomacyEnemyAction != null)
                _diplomacyEnemyAction();
        }

        private void RequestDiplomacyVassal()
        {
            if (_diplomacyVassalAction != null)
                _diplomacyVassalAction();
        }

        private void RequestDiplomacyNeutral()
        {
            if (_diplomacyNeutralAction != null)
                _diplomacyNeutralAction();
        }

        private void RequestShowOverview()
        {
            _showClanOverview = false;

            if (_lastModel != null)
                ApplyModel(_lastModel);

            if (_showOverviewAction != null)
                _showOverviewAction();
        }

        private void RequestToggleClanOverview()
        {
            _showClanOverview = !_showClanOverview;

            if (_lastModel != null)
                ApplyModel(_lastModel);

            if (_showOverviewAction != null)
                _showOverviewAction();
        }

        private void RequestShowWard()
        {
            if (_showWardAction != null)
                _showWardAction();
        }

        private void RequestShowTerritory()
        {
            if (_showTerritoryAction != null)
                _showTerritoryAction();
        }

        private void RequestShowTerraforming()
        {
            if (_showTerraformingAction != null)
                _showTerraformingAction();
        }

        private void RequestShowEconomy()
        {
            if (_showEconomyAction != null)
                _showEconomyAction();
        }

        private void RequestOpenTreasuryChest()
        {
            if (_openTreasuryChestAction != null)
                _openTreasuryChestAction();
        }

        private void RequestShowTerraformingPreparationChest()
        {
            if (_openTerraformingPreparationChestAction != null)
                _openTerraformingPreparationChestAction();
        }

        private void RequestCloseTerraformingPreparationChest()
        {
            if (_terraformingStoragePanel != null)
                _terraformingStoragePanel.SetActive(false);
        }

        private void RequestToggleProtection()
        {
            if (_toggleProtectionAction != null)
                _toggleProtectionAction();
        }

        private void RequestDecreaseRadius()
        {
            if (_decreaseRadiusAction != null)
                _decreaseRadiusAction();
        }

        private void RequestIncreaseRadius()
        {
            if (_increaseRadiusAction != null)
                _increaseRadiusAction();
        }

        private void RequestRenameTerritory()
        {
            if (_renameTerritoryAction != null)
                _renameTerritoryAction();
        }

        private void RequestRemovePermittedPlayer(long playerId)
        {
            if (_removePermittedPlayerAction != null)
                _removePermittedPlayerAction(playerId);
        }

        private void RequestToggleSelfPermission()
        {
            if (_toggleSelfPermissionAction != null)
                _toggleSelfPermissionAction();
        }

        private void RequestToggleDoorLock()
        {
            if (_toggleDoorLockAction != null)
                _toggleDoorLockAction();
        }

        private void RequestDecreaseDoorAutoClose()
        {
            if (_decreaseDoorAutoCloseAction != null)
                _decreaseDoorAutoCloseAction();
        }

        private void RequestIncreaseDoorAutoClose()
        {
            if (_increaseDoorAutoCloseAction != null)
                _increaseDoorAutoCloseAction();
        }

        private void RequestToggleStructureDamageProtection()
        {
            if (_toggleStructureDamageProtectionAction != null)
                _toggleStructureDamageProtectionAction();
        }

        private void RequestToggleTerraforming()
        {
            if (_toggleTerraformingAction != null)
                _toggleTerraformingAction();
        }

        private void RequestToggleTerraformingRunning()
        {
            if (_toggleTerraformingRunningAction != null)
                _toggleTerraformingRunningAction();
        }


        private void RequestDecreaseTerraformingRadius()
        {
            if (_decreaseTerraformingRadiusAction != null)
                _decreaseTerraformingRadiusAction();
        }

        private void RequestIncreaseTerraformingRadius()
        {
            if (_increaseTerraformingRadiusAction != null)
                _increaseTerraformingRadiusAction();
        }



        private void RequestStoreTerraformingHoe()
        {
            if (_storeTerraformingHoeAction != null)
                _storeTerraformingHoeAction();
        }

        private void RequestStoreTerraformingPickaxe()
        {
            if (_storeTerraformingPickaxeAction != null)
                _storeTerraformingPickaxeAction();
        }

        private void RequestAddTerraformingFuelSlot(int slotIndex)
        {
            if (_addTerraformingFuelSlotAction != null)
                _addTerraformingFuelSlotAction(slotIndex);
        }

        private void RequestAddTerraformingStoneSlot(int slotIndex)
        {
            if (_addTerraformingStoneSlotAction != null)
                _addTerraformingStoneSlotAction(slotIndex);
        }

        private void RequestShowBiomeDominion()
        {
            if (_showBiomeDominionAction != null)
                _showBiomeDominionAction();
        }

        private void RequestClaimBiomeDominion()
        {
            if (_claimBiomeDominionAction != null)
                _claimBiomeDominionAction();
        }

        private void RequestReleaseBiomeDominion()
        {
            if (_releaseBiomeDominionAction != null)
                _releaseBiomeDominionAction();
        }

        private void RequestToggleBiomeDoorLock()
        {
            if (_toggleBiomeDoorLockAction != null)
                _toggleBiomeDoorLockAction();
        }

        private void RequestDecreaseBiomeDoorAutoClose()
        {
            if (_decreaseBiomeDoorAutoCloseAction != null)
                _decreaseBiomeDoorAutoCloseAction();
        }

        private void RequestIncreaseBiomeDoorAutoClose()
        {
            if (_increaseBiomeDoorAutoCloseAction != null)
                _increaseBiomeDoorAutoCloseAction();
        }

        private void RequestToggleBiomeStructureDamageProtection()
        {
            if (_toggleBiomeStructureDamageProtectionAction != null)
                _toggleBiomeStructureDamageProtectionAction();
        }

        private void RequestEconomyDeposit()
        {
            if (_economyDepositAction != null)
                _economyDepositAction();
        }

        private void RequestEconomyWithdraw()
        {
            if (_economyWithdrawAction != null)
                _economyWithdrawAction();
        }

        private void RequestEconomyUpkeep()
        {
            if (_economyUpkeepAction != null)
                _economyUpkeepAction();
        }

        private void RequestEconomyTax()
        {
            if (_economyTaxAction != null)
                _economyTaxAction();
        }

        private void RequestEconomyTransfer()
        {
            if (_economyTransferAction != null)
                _economyTransferAction();
        }

        private void RequestCloseByInput()
        {
            if (_closeByInputAction != null)
                _closeByInputAction();
        }

        private void RequestCloseByDistance()
        {
            if (_closeByDistanceAction != null)
                _closeByDistanceAction();
        }
    }
}
