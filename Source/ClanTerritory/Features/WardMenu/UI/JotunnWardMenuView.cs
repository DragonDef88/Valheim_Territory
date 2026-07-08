using System;
using System.Collections.Generic;
using ClanTerritory.Features.WardMenu.Models;
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

        private GameObject _root;
        private GameObject _panel;
        private GameObject _overviewPanel;
        private GameObject _wardPanel;
        private GameObject _territoryPanel;
        private GameObject _terraformingPanel;

        private Text _titleText;
        private Text _subtitleText;
        private Text _overviewText;
        private Text _wardText;
        private Text _territoryText;
        private Text _terraformingText;
        private Text _radiusValueText;
        private Text _doorAutoCloseValueText;
        private Text _terraformingRadiusValueText;
        private Text _terraformingTargetHeightValueText;

        private Button _overviewButton;
        private Button _wardButton;
        private Button _territoryButton;
        private Button _terraformingButton;
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
        private Button _toggleTerraformingButton;
        private Button _toggleTerraformingRunningButton;
        private Button _cycleTerraformingModeButton;
        private Button _decreaseTerraformingRadiusButton;
        private Button _increaseTerraformingRadiusButton;
        private Button _setTerraformingTargetHeightFromWardButton;
        private Button _setTerraformingTargetHeightFromPlayerButton;
        private Button _storeTerraformingHoeButton;
        private Button _storeTerraformingPickaxeButton;
        private Button _addTerraformingFuelButton;
        private Button _addTerraformingStoneButton;

        private Action _showOverviewAction;
        private Action _showWardAction;
        private Action _showTerritoryAction;
        private Action _showTerraformingAction;
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
        private Action _cycleTerraformingModeAction;
        private Action _decreaseTerraformingRadiusAction;
        private Action _increaseTerraformingRadiusAction;
        private Action _setTerraformingTargetHeightFromWardAction;
        private Action _setTerraformingTargetHeightFromPlayerAction;
        private Action _storeTerraformingHoeAction;
        private Action _storeTerraformingPickaxeAction;
        private Action _addTerraformingFuelAction;
        private Action _addTerraformingStoneAction;
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
            Action closeByDistanceAction)
        {
            if (model == null)
                return;

            SetActions(
                showOverviewAction,
                showWardAction,
                showTerritoryAction,
                showTerraformingAction,
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
                cycleTerraformingModeAction,
                decreaseTerraformingRadiusAction,
                increaseTerraformingRadiusAction,
                setTerraformingTargetHeightFromWardAction,
                setTerraformingTargetHeightFromPlayerAction,
                storeTerraformingHoeAction,
                storeTerraformingPickaxeAction,
                addTerraformingFuelAction,
                addTerraformingStoneAction,
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
            _terraformingPanel = null;

            _titleText = null;
            _subtitleText = null;
            _overviewText = null;
            _wardText = null;
            _territoryText = null;
            _terraformingText = null;
            _radiusValueText = null;
            _doorAutoCloseValueText = null;
            _terraformingRadiusValueText = null;
            _terraformingTargetHeightValueText = null;

            _overviewButton = null;
            _wardButton = null;
            _territoryButton = null;
            _terraformingButton = null;
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
            _toggleTerraformingButton = null;
            _toggleTerraformingRunningButton = null;
            _cycleTerraformingModeButton = null;
            _decreaseTerraformingRadiusButton = null;
            _increaseTerraformingRadiusButton = null;
            _setTerraformingTargetHeightFromWardButton = null;
            _setTerraformingTargetHeightFromPlayerButton = null;
            _storeTerraformingHoeButton = null;
            _storeTerraformingPickaxeButton = null;
            _addTerraformingFuelButton = null;
            _addTerraformingStoneButton = null;

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

        public void ShowTerraformingPanel()
        {
            SetActivePanel(_terraformingPanel);
        }

        private void SetActions(
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
            Action closeByDistanceAction)
        {
            _showOverviewAction = showOverviewAction;
            _showWardAction = showWardAction;
            _showTerritoryAction = showTerritoryAction;
            _showTerraformingAction = showTerraformingAction;
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
            _cycleTerraformingModeAction = cycleTerraformingModeAction;
            _decreaseTerraformingRadiusAction = decreaseTerraformingRadiusAction;
            _increaseTerraformingRadiusAction = increaseTerraformingRadiusAction;
            _setTerraformingTargetHeightFromWardAction = setTerraformingTargetHeightFromWardAction;
            _setTerraformingTargetHeightFromPlayerAction = setTerraformingTargetHeightFromPlayerAction;
            _storeTerraformingHoeAction = storeTerraformingHoeAction;
            _storeTerraformingPickaxeAction = storeTerraformingPickaxeAction;
            _addTerraformingFuelAction = addTerraformingFuelAction;
            _addTerraformingStoneAction = addTerraformingStoneAction;
            _closeByInputAction = closeByInputAction;
            _closeByDistanceAction = closeByDistanceAction;
        }

        private void ClearActions()
        {
            _showOverviewAction = null;
            _showWardAction = null;
            _showTerritoryAction = null;
            _showTerraformingAction = null;
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
            _cycleTerraformingModeAction = null;
            _decreaseTerraformingRadiusAction = null;
            _increaseTerraformingRadiusAction = null;
            _setTerraformingTargetHeightFromWardAction = null;
            _setTerraformingTargetHeightFromPlayerAction = null;
            _storeTerraformingHoeAction = null;
            _storeTerraformingPickaxeAction = null;
            _addTerraformingFuelAction = null;
            _addTerraformingStoneAction = null;
            _closeByInputAction = null;
            _closeByDistanceAction = null;
        }

        private void ApplyModel(WardMenuModel model)
        {
            string radiusText = FormatRadius(model.Ward.Radius);
            string protectionText = FormatProtection(model.Ward.Enabled);
            string doorText = FormatDoorLock(model);
            bool ownerMode = model.Ward.IsCurrentPlayerCreator;
            bool selfPermissionMode = !ownerMode && !model.Ward.Enabled;

            _titleText.text = "Clan Territory";
            _subtitleText.text =
                "Territory radius: " + radiusText + " m   |   Protection: " + protectionText;

            _overviewText.text =
                "Overview\n\n" +
                "Territory:\n" + model.Territory.Name + "\n\n" +
                "Ward ID:\n" + model.Ward.WardId + "\n\n" +
                "Owner:\n" + model.Ward.OwnerName + "\n\n" +
                "Territory radius:\n" + radiusText + " m\n\n" +
                "Protection:\n" + protectionText + "\n\n" +
                "Your access:\n" + FormatCurrentAccess(model) + "\n\n" +
                "Doors:\n" + doorText + "\n\n" +
                "Structures:\n" + (model.Territory.StructureDamageProtectionEnabled ? "Protected" : "Vulnerable");

            _wardText.text =
                "Ward Access\n\n" +
                "Protection: " + protectionText + "\n" +
                "Territory radius: " + radiusText + " m\n" +
                "Permitted players: " + model.Ward.PermittedPlayers.Count + "\n" +
                "Your access: " + FormatCurrentAccess(model);

            _territoryText.text =
                "Territory Settings\n\n" +
                "Name:\n" + model.Territory.Name + "\n\n" +
                "Doors: " + doorText + "\n" +
                "Structures: " + (model.Territory.StructureDamageProtectionEnabled ? "Protected" : "Vulnerable") + "\n\n" +
                "Guild access: " + (model.Territory.GuildAccessEnabled ? "Enabled" : "Disabled") + "\n" +
                "Group access: " + (model.Territory.GroupAccessEnabled ? "Enabled" : "Disabled");

            _terraformingText.text =
                "Territory Terraforming\n\n" +
                "Status: " + model.Terraforming.Status + "\n" +
                "Mode: " + model.Terraforming.Mode + "\n" +
                "Work radius: " + FormatRadius(model.Terraforming.Radius) + " m\n" +
                "Target height: " + FormatHeight(model.Terraforming.TargetHeight) + "\n" +
                "Fuel: " + FormatAmount(model.Terraforming.FuelStored) + " / 250\n" +
                "Stone: " + FormatAmount(model.Terraforming.StoneStored) + " / 500\n" +
                "Hoe slot: " + FormatSlot(model.Terraforming.HoeStored, "Hoe") + "\n" +
                "Pickaxe slot: " + FormatSlot(model.Terraforming.PickaxeStored, "Pickaxe") + "\n" +
                "Scan: " + FormatAmount(model.Terraforming.ScanProgress) + " / index " + model.Terraforming.ScanIndex;

            if (_radiusValueText != null)
                _radiusValueText.text = radiusText + " m";

            if (_doorAutoCloseValueText != null)
                _doorAutoCloseValueText.text = model.Territory.DoorAutoCloseSeconds + "s";

            if (_terraformingRadiusValueText != null)
                _terraformingRadiusValueText.text = FormatRadius(model.Terraforming.Radius) + " m";

            if (_terraformingTargetHeightValueText != null)
                _terraformingTargetHeightValueText.text = FormatHeight(model.Terraforming.TargetHeight);

            SetButtonText(
                _toggleProtectionButton,
                model.Ward.Enabled ? "Disable Protection" : "Enable Protection");

            SetButtonText(
                _toggleSelfPermissionButton,
                model.Ward.IsCurrentPlayerPermitted
                    ? "Remove Me"
                    : "Add Me");

            SetButtonText(
                _toggleDoorLockButton,
                model.Territory.DoorLockEnabled
                    ? "Unlock Doors"
                    : "Lock Doors");

            SetButtonText(
                _toggleStructureDamageProtectionButton,
                model.Territory.StructureDamageProtectionEnabled
                    ? "Disable Structure Protection"
                    : "Enable Structure Protection");

            SetButtonText(_decreaseRadiusButton, "-5");
            SetButtonText(_increaseRadiusButton, "+5");
            SetButtonText(_decreaseDoorAutoCloseButton, "-1");
            SetButtonText(_increaseDoorAutoCloseButton, "+1");
            SetButtonText(_toggleTerraformingButton, model.Terraforming.Enabled ? "Disable Terraforming" : "Enable Terraforming");
            SetButtonText(_toggleTerraformingRunningButton, model.Terraforming.Running ? "Stop" : "Start");
            SetButtonText(_cycleTerraformingModeButton, "Mode: " + model.Terraforming.Mode);
            SetButtonText(_decreaseTerraformingRadiusButton, "-2");
            SetButtonText(_increaseTerraformingRadiusButton, "+2");
            SetButtonText(_setTerraformingTargetHeightFromWardButton, "Target: Ward");
            SetButtonText(_setTerraformingTargetHeightFromPlayerButton, "Target: Player");
            SetButtonText(_storeTerraformingHoeButton, model.Terraforming.HoeStored ? "Hoe: Set" : "Place Hoe");
            SetButtonText(_storeTerraformingPickaxeButton, model.Terraforming.PickaxeStored ? "Pickaxe: Set" : "Place Pickaxe");
            SetButtonText(_addTerraformingFuelButton, "+ Fuel");
            SetButtonText(_addTerraformingStoneButton, "+ Stone");

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
            SetButtonActive(_toggleTerraformingButton, ownerMode);
            SetButtonActive(_toggleTerraformingRunningButton, ownerMode);
            SetButtonActive(_cycleTerraformingModeButton, ownerMode);
            SetButtonActive(_decreaseTerraformingRadiusButton, ownerMode);
            SetButtonActive(_increaseTerraformingRadiusButton, ownerMode);
            SetButtonActive(_setTerraformingTargetHeightFromWardButton, ownerMode);
            SetButtonActive(_setTerraformingTargetHeightFromPlayerButton, ownerMode);
            SetButtonActive(_storeTerraformingHoeButton, ownerMode);
            SetButtonActive(_storeTerraformingPickaxeButton, ownerMode);
            SetButtonActive(_addTerraformingFuelButton, ownerMode);
            SetButtonActive(_addTerraformingStoneButton, ownerMode);
            SetTextActive(_terraformingRadiusValueText, ownerMode);
            SetTextActive(_terraformingTargetHeightValueText, ownerMode);
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
                "Clan Territory",
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

            _overviewButton = CreateButton("Overview", new Vector2(-300f, 155f), 160f, 38f);
            _wardButton = CreateButton("Ward", new Vector2(-100f, 155f), 160f, 38f);
            _territoryButton = CreateButton("Territory", new Vector2(100f, 155f), 160f, 38f);
            _terraformingButton = CreateButton("Terraforming", new Vector2(300f, 155f), 160f, 38f);

            _overviewPanel = CreatePanelRoot("OverviewPanel");
            _wardPanel = CreatePanelRoot("WardPanel");
            _territoryPanel = CreatePanelRoot("TerritoryPanel");
            _terraformingPanel = CreatePanelRoot("TerraformingPanel");

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

            _toggleProtectionButton = CreateButton(_wardPanel.transform, "Toggle Protection", new Vector2(0f, -115f), 240f, 38f);
            _toggleSelfPermissionButton = CreateButton(_wardPanel.transform, "Add Me", new Vector2(0f, -115f), 240f, 38f);
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

            _toggleDoorLockButton = CreateButton(_territoryPanel.transform, "Lock Doors", new Vector2(0f, -62f), 280f, 30f);
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
            _toggleStructureDamageProtectionButton = CreateButton(_territoryPanel.transform, "Enable Structure Protection", new Vector2(0f, -132f), 280f, 30f);
            _renameTerritoryButton = CreateButton(_territoryPanel.transform, "Rename Territory", new Vector2(0f, -168f), 280f, 30f);

            _terraformingText = CreateLabel(
                "",
                new Vector2(0f, 40f),
                18,
                650f,
                168f,
                TextAnchor.UpperLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _terraformingText.transform.SetParent(_terraformingPanel.transform, false);

            _toggleTerraformingButton = CreateButton(_terraformingPanel.transform, "Enable Terraforming", new Vector2(-190f, -62f), 220f, 30f);
            _toggleTerraformingRunningButton = CreateButton(_terraformingPanel.transform, "Start", new Vector2(190f, -62f), 220f, 30f);
            _cycleTerraformingModeButton = CreateButton(_terraformingPanel.transform, "Mode: Level", new Vector2(0f, -96f), 280f, 28f);

            _decreaseTerraformingRadiusButton = CreateButton(_terraformingPanel.transform, "-2", new Vector2(-150f, -128f), 76f, 28f);

            _terraformingRadiusValueText = CreateLabel(
                "",
                new Vector2(0f, -128f),
                18,
                110f,
                28f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerifBold,
                gui.ValheimOrange);

            _terraformingRadiusValueText.transform.SetParent(_terraformingPanel.transform, false);

            _increaseTerraformingRadiusButton = CreateButton(_terraformingPanel.transform, "+2", new Vector2(150f, -128f), 76f, 28f);
            _setTerraformingTargetHeightFromWardButton = CreateButton(_terraformingPanel.transform, "Target: Ward", new Vector2(-150f, -160f), 170f, 28f);
            _setTerraformingTargetHeightFromPlayerButton = CreateButton(_terraformingPanel.transform, "Target: Player", new Vector2(150f, -160f), 170f, 28f);

            _terraformingTargetHeightValueText = CreateLabel(
                "",
                new Vector2(0f, -190f),
                18,
                220f,
                26f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerifBold,
                gui.ValheimOrange);

            _terraformingTargetHeightValueText.transform.SetParent(_terraformingPanel.transform, false);

            _storeTerraformingHoeButton = CreateButton(_terraformingPanel.transform, "Place Hoe", new Vector2(-240f, -220f), 145f, 28f);
            _storeTerraformingPickaxeButton = CreateButton(_terraformingPanel.transform, "Place Pickaxe", new Vector2(-80f, -220f), 145f, 28f);
            _addTerraformingFuelButton = CreateButton(_terraformingPanel.transform, "+ Fuel", new Vector2(80f, -220f), 145f, 28f);
            _addTerraformingStoneButton = CreateButton(_terraformingPanel.transform, "+ Stone", new Vector2(240f, -220f), 145f, 28f);

            _closeButton = CreateButton("Close", new Vector2(0f, -265f), 180f, 34f);

            _overviewButton.onClick.AddListener(RequestShowOverview);
            _wardButton.onClick.AddListener(RequestShowWard);
            _territoryButton.onClick.AddListener(RequestShowTerritory);
            _terraformingButton.onClick.AddListener(RequestShowTerraforming);
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
            _toggleTerraformingButton.onClick.AddListener(RequestToggleTerraforming);
            _toggleTerraformingRunningButton.onClick.AddListener(RequestToggleTerraformingRunning);
            _cycleTerraformingModeButton.onClick.AddListener(RequestCycleTerraformingMode);
            _decreaseTerraformingRadiusButton.onClick.AddListener(RequestDecreaseTerraformingRadius);
            _increaseTerraformingRadiusButton.onClick.AddListener(RequestIncreaseTerraformingRadius);
            _setTerraformingTargetHeightFromWardButton.onClick.AddListener(RequestSetTerraformingTargetHeightFromWard);
            _setTerraformingTargetHeightFromPlayerButton.onClick.AddListener(RequestSetTerraformingTargetHeightFromPlayer);
            _storeTerraformingHoeButton.onClick.AddListener(RequestStoreTerraformingHoe);
            _storeTerraformingPickaxeButton.onClick.AddListener(RequestStoreTerraformingPickaxe);
            _addTerraformingFuelButton.onClick.AddListener(RequestAddTerraformingFuel);
            _addTerraformingStoneButton.onClick.AddListener(RequestAddTerraformingStone);

            SetActivePanel(_overviewPanel);
            SetVisible(_visible);
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

            if (_terraformingPanel != null)
                _terraformingPanel.SetActive(activePanel == _terraformingPanel);
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

                Button removeButton = CreateButton(_wardPanel.transform, "Remove", new Vector2(235f, y), 115f, 28f);

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
                    "... and " + remainingCount + " more",
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

        private static string FormatRadius(float radius)
        {
            return Mathf.RoundToInt(radius).ToString();
        }

        private static string FormatProtection(bool enabled)
        {
            return enabled ? "Enabled" : "Disabled";
        }

        private static string FormatDoorLock(WardMenuModel model)
        {
            if (!model.Territory.DoorLockEnabled)
                return "Unlocked";

            return "Locked, auto-close " + model.Territory.DoorAutoCloseSeconds + "s";
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
            return stored ? name : "Empty";
        }

        private static string FormatCurrentAccess(WardMenuModel model)
        {
            if (model.Ward.IsCurrentPlayerCreator)
                return "Owner";

            if (model.Ward.IsCurrentPlayerPermitted)
                return "Permitted";

            return "Guest";
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

        private void RequestShowOverview()
        {
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

        private void RequestCycleTerraformingMode()
        {
            if (_cycleTerraformingModeAction != null)
                _cycleTerraformingModeAction();
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

        private void RequestSetTerraformingTargetHeightFromWard()
        {
            if (_setTerraformingTargetHeightFromWardAction != null)
                _setTerraformingTargetHeightFromWardAction();
        }

        private void RequestSetTerraformingTargetHeightFromPlayer()
        {
            if (_setTerraformingTargetHeightFromPlayerAction != null)
                _setTerraformingTargetHeightFromPlayerAction();
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

        private void RequestAddTerraformingFuel()
        {
            if (_addTerraformingFuelAction != null)
                _addTerraformingFuelAction();
        }

        private void RequestAddTerraformingStone()
        {
            if (_addTerraformingStoneAction != null)
                _addTerraformingStoneAction();
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
