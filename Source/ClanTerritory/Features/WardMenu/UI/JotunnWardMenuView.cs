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

        private Text _titleText;
        private Text _subtitleText;
        private Text _overviewText;
        private Text _wardText;
        private Text _territoryText;
        private Text _radiusValueText;

        private Button _overviewButton;
        private Button _wardButton;
        private Button _territoryButton;
        private Button _closeButton;
        private Button _toggleProtectionButton;
        private Button _decreaseRadiusButton;
        private Button _increaseRadiusButton;
        private Button _renameTerritoryButton;

        private Action _showOverviewAction;
        private Action _showWardAction;
        private Action _showTerritoryAction;
        private Action _toggleProtectionAction;
        private Action _decreaseRadiusAction;
        private Action _increaseRadiusAction;
        private Action _renameTerritoryAction;
        private Action<long> _removePermittedPlayerAction;
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
            Action toggleProtectionAction,
            Action decreaseRadiusAction,
            Action increaseRadiusAction,
            Action renameTerritoryAction,
            Action<long> removePermittedPlayerAction,
            Action closeByInputAction,
            Action closeByDistanceAction)
        {
            if (model == null)
                return;

            SetActions(
                showOverviewAction,
                showWardAction,
                showTerritoryAction,
                toggleProtectionAction,
                decreaseRadiusAction,
                increaseRadiusAction,
                renameTerritoryAction,
                removePermittedPlayerAction,
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

            _titleText = null;
            _subtitleText = null;
            _overviewText = null;
            _wardText = null;
            _territoryText = null;
            _radiusValueText = null;

            _overviewButton = null;
            _wardButton = null;
            _territoryButton = null;
            _closeButton = null;
            _toggleProtectionButton = null;
            _decreaseRadiusButton = null;
            _increaseRadiusButton = null;
            _renameTerritoryButton = null;

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

        private void SetActions(
            Action showOverviewAction,
            Action showWardAction,
            Action showTerritoryAction,
            Action toggleProtectionAction,
            Action decreaseRadiusAction,
            Action increaseRadiusAction,
            Action renameTerritoryAction,
            Action<long> removePermittedPlayerAction,
            Action closeByInputAction,
            Action closeByDistanceAction)
        {
            _showOverviewAction = showOverviewAction;
            _showWardAction = showWardAction;
            _showTerritoryAction = showTerritoryAction;
            _toggleProtectionAction = toggleProtectionAction;
            _decreaseRadiusAction = decreaseRadiusAction;
            _increaseRadiusAction = increaseRadiusAction;
            _renameTerritoryAction = renameTerritoryAction;
            _removePermittedPlayerAction = removePermittedPlayerAction;
            _closeByInputAction = closeByInputAction;
            _closeByDistanceAction = closeByDistanceAction;
        }

        private void ClearActions()
        {
            _showOverviewAction = null;
            _showWardAction = null;
            _showTerritoryAction = null;
            _toggleProtectionAction = null;
            _decreaseRadiusAction = null;
            _increaseRadiusAction = null;
            _renameTerritoryAction = null;
            _removePermittedPlayerAction = null;
            _closeByInputAction = null;
            _closeByDistanceAction = null;
        }

        private void ApplyModel(WardMenuModel model)
        {
            string radiusText = FormatRadius(model.Ward.Radius);
            string protectionText = FormatProtection(model.Ward.Enabled);

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
                "Territory Runtime:\n" + (model.Territory.RuntimeActive ? "Active" : "Inactive");

            _wardText.text =
                "Ward Access\n\n" +
                "Protection: " + protectionText + "\n" +
                "Territory radius: " + radiusText + " m\n" +
                "Permitted players: " + model.Ward.PermittedPlayers.Count;

            _territoryText.text =
                "Territory Settings\n\n" +
                "Name:\n" + model.Territory.Name + "\n\n" +
                "Territory radius:\n" + radiusText + " m\n\n" +
                "Protection:\n" + protectionText + "\n\n" +
                "Guild access:\n" + (model.Territory.GuildAccessEnabled ? "Enabled" : "Disabled") + "\n\n" +
                "Group access:\n" + (model.Territory.GroupAccessEnabled ? "Enabled" : "Disabled") + "\n\n" +
                "Rules:\n" + model.Territory.RulesSummary;

            if (_radiusValueText != null)
                _radiusValueText.text = radiusText + " m";

            BuildPermittedPlayerRows(model);

            SetButtonText(
                _toggleProtectionButton,
                model.Ward.Enabled ? "Disable Protection" : "Enable Protection");

            SetButtonText(
                _decreaseRadiusButton,
                "-5");

            SetButtonText(
                _increaseRadiusButton,
                "+5");
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
                760f,
                540f,
                false);

            _panel.name = "ClanTerritory_JotunnWardPanel";

            _titleText = CreateLabel(
                "Clan Territory",
                new Vector2(0f, 220f),
                30,
                700f,
                44f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerifBold,
                gui.ValheimOrange);

            _subtitleText = CreateLabel(
                "",
                new Vector2(0f, 185f),
                18,
                700f,
                32f,
                TextAnchor.MiddleCenter,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _overviewButton = CreateButton(
                "Overview",
                new Vector2(-235f, 135f),
                190f,
                42f);

            _wardButton = CreateButton(
                "Ward",
                new Vector2(0f, 135f),
                190f,
                42f);

            _territoryButton = CreateButton(
                "Territory",
                new Vector2(235f, 135f),
                190f,
                42f);

            _overviewPanel = CreatePanelRoot("OverviewPanel");
            _wardPanel = CreatePanelRoot("WardPanel");
            _territoryPanel = CreatePanelRoot("TerritoryPanel");

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

            _toggleProtectionButton = CreateButton(
                _wardPanel.transform,
                "Toggle Protection",
                new Vector2(0f, -115f),
                240f,
                38f);

            _decreaseRadiusButton = CreateButton(
                _wardPanel.transform,
                "-5",
                new Vector2(-130f, -160f),
                110f,
                38f);

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

            _increaseRadiusButton = CreateButton(
                _wardPanel.transform,
                "+5",
                new Vector2(130f, -160f),
                110f,
                38f);

            _territoryText = CreateLabel(
                "",
                new Vector2(0f, 35f),
                20,
                650f,
                170f,
                TextAnchor.UpperLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _territoryText.transform.SetParent(_territoryPanel.transform, false);

            _renameTerritoryButton = CreateButton(
                _territoryPanel.transform,
                "Rename Territory",
                new Vector2(0f, -105f),
                240f,
                38f);

            _closeButton = CreateButton(
                "Close",
                new Vector2(0f, -220f),
                180f,
                44f);

            _overviewButton.onClick.AddListener(RequestShowOverview);
            _wardButton.onClick.AddListener(RequestShowWard);
            _territoryButton.onClick.AddListener(RequestShowTerritory);
            _closeButton.onClick.AddListener(RequestCloseByInput);
            _toggleProtectionButton.onClick.AddListener(RequestToggleProtection);
            _decreaseRadiusButton.onClick.AddListener(RequestDecreaseRadius);
            _increaseRadiusButton.onClick.AddListener(RequestIncreaseRadius);
            _renameTerritoryButton.onClick.AddListener(RequestRenameTerritory);

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

        private Button CreateButton(
            string text,
            Vector2 position,
            float width,
            float height)
        {
            return CreateButton(
                _panel.transform,
                text,
                position,
                width,
                height);
        }

        private Button CreateButton(
            Transform parent,
            string text,
            Vector2 position,
            float width,
            float height)
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
        }

        private void BuildPermittedPlayerRows(WardMenuModel model)
        {
            ClearPermittedPlayerRows();

            if (_wardPanel == null)
                return;

            if (model == null || model.Ward.PermittedPlayers.Count <= 0)
                return;

            GUIManager gui = GUIManager.Instance;

            int visibleCount = Mathf.Min(
                model.Ward.PermittedPlayers.Count,
                MaxVisiblePermittedPlayers);

            const float startY = 8f;
            const float rowSpacing = 27f;

            for (int i = 0; i < visibleCount; i++)
            {
                WardMenuPlayerModel player = model.Ward.PermittedPlayers[i];
                float y = startY - i * rowSpacing;

                Text playerText = CreateLabel(
                    player.PlayerName + " (" + player.PlayerId + ")",
                    new Vector2(-115f, y),
                    16,
                    390f,
                    24f,
                    TextAnchor.MiddleLeft,
                    gui.AveriaSerif,
                    gui.ValheimBeige);

                playerText.transform.SetParent(_wardPanel.transform, false);
                _permittedPlayerRowObjects.Add(playerText.gameObject);

                Button removeButton = CreateButton(
                    _wardPanel.transform,
                    "Remove",
                    new Vector2(235f, y),
                    115f,
                    28f);

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
                    new Vector2(-115f, y),
                    16,
                    390f,
                    24f,
                    TextAnchor.MiddleLeft,
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

        private static void SetButtonText(Button button, string text)
        {
            if (button == null)
                return;

            Text buttonText = button.GetComponentInChildren<Text>();

            if (buttonText != null)
                buttonText.text = text;
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
