using System;
using ClanTerritory.Features.WardMenu.Models;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ClanTerritory.Features.WardMenu.UI
{
    internal sealed class JotunnWardMenuView : IWardMenuView
    {
        private const float HideDistance = 5f;

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
            _closeByInputAction = null;
            _closeByDistanceAction = null;
        }

        private void ApplyModel(WardMenuModel model)
        {
            _titleText.text = "Clan Territory";
            _subtitleText.text = "Ward & Territory Management";

            _overviewText.text =
                "Overview\n\n" +
                "Territory:\n" + model.Territory.Name + "\n\n" +
                "Ward ID:\n" + model.Ward.WardId + "\n\n" +
                "Owner:\n" + model.Ward.OwnerName + "\n\n" +
                "Radius:\n" + model.Ward.Radius + "\n\n" +
                "Ward Enabled:\n" + (model.Ward.Enabled ? "Yes" : "No") + "\n\n" +
                "Territory Runtime:\n" + (model.Territory.RuntimeActive ? "Active" : "Inactive");

            _wardText.text =
                "Ward Access\n\n" +
                "Permitted players: " + model.Ward.PermittedPlayers.Count + "\n\n" +
                BuildPermittedPlayersText(model);

            _territoryText.text =
                "Territory Settings\n\n" +
                "Name:\n" + model.Territory.Name + "\n\n" +
                "Guild access:\n" + (model.Territory.GuildAccessEnabled ? "Enabled" : "Disabled") + "\n\n" +
                "Group access:\n" + (model.Territory.GroupAccessEnabled ? "Enabled" : "Disabled") + "\n\n" +
                "Rules:\n" + model.Territory.RulesSummary;
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
                "Ward & Territory Management",
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
                new Vector2(0f, 35f),
                20,
                650f,
                170f,
                TextAnchor.UpperLeft,
                gui.AveriaSerif,
                gui.ValheimBeige);

            _wardText.transform.SetParent(_wardPanel.transform, false);

            _toggleProtectionButton = CreateButton(
                _wardPanel.transform,
                "Toggle Protection",
                new Vector2(0f, -105f),
                240f,
                38f);

            _decreaseRadiusButton = CreateButton(
                _wardPanel.transform,
                "Radius -5",
                new Vector2(-130f, -150f),
                110f,
                38f);

            _increaseRadiusButton = CreateButton(
                _wardPanel.transform,
                "Radius +5",
                new Vector2(130f, -150f),
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

        private static string BuildPermittedPlayersText(WardMenuModel model)
        {
            if (model.Ward.PermittedPlayers.Count <= 0)
                return "No permitted players.";

            string text = "";

            for (int i = 0; i < model.Ward.PermittedPlayers.Count; i++)
            {
                WardMenuPlayerModel player = model.Ward.PermittedPlayers[i];

                text += "- " + player.PlayerName + " (" + player.PlayerId + ")\n";
            }

            return text;
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
