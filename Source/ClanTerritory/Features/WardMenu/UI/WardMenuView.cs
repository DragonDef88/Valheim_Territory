using System;
using ClanTerritory.Features.WardMenu.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClanTerritory.Features.WardMenu.UI
{
    internal sealed class WardMenuView
    {
        private const float HideDistance = 5f;

        private Button _toggleProtectionButton;
        private Action _toggleProtectionAction;

        private Button _renameTerritoryButton;
        private Action _renameTerritoryAction;

        private GameObject _root;
        private GameObject _panel;

        private TMP_Text _title;
        private TMP_Text _subtitle;
        private TMP_Text _overviewText;
        private TMP_Text _permissionsText;
        private TMP_Text _settingsText;

        private Button _overviewButton;
        private Button _permissionsButton;
        private Button _settingsButton;
        private Button _closeButton;

        private GameObject _overviewPanel;
        private GameObject _permissionsPanel;
        private GameObject _settingsPanel;

        private Action _showOverviewAction;
        private Action _showWardAction;
        private Action _showTerritoryAction;
        private Action _closeByInputAction;
        private Action _closeByDistanceAction;

        private int _hiddenFrames = 9999;
        private bool _useReleasedAfterOpen;

        public bool IsVisible
        {
            get { return _root != null && _root.activeSelf && _hiddenFrames <= 1; }
        }

        public void Show(
            WardMenuModel model,
            Action showOverviewAction,
            Action showWardAction,
            Action showTerritoryAction,
            Action toggleProtectionAction,
            Action renameTerritoryAction,
            Action closeByInputAction,
            Action closeByDistanceAction)
        {
            if (model == null)
                return;

            EnsureCreated();

            _showOverviewAction = showOverviewAction;
            _showWardAction = showWardAction;
            _showTerritoryAction = showTerritoryAction;
            _toggleProtectionAction = toggleProtectionAction;
            _renameTerritoryAction = renameTerritoryAction;
            _closeByInputAction = closeByInputAction;
            _closeByDistanceAction = closeByDistanceAction;
            _useReleasedAfterOpen = false;

            _title.text = "Clan Territory";
            _subtitle.text = "Ward & Territory Management";

            _overviewText.text =
                "Overview\n\n" +
                "Territory:\n" + model.Territory.Name + "\n\n" +
                "Ward ID:\n" + model.Ward.WardId + "\n\n" +
                "Owner:\n" + model.Ward.OwnerName + "\n\n" +
                "Radius:\n" + model.Ward.Radius + "\n\n" +
                "Ward Enabled:\n" + (model.Ward.Enabled ? "Yes" : "No") + "\n\n" +
                "Territory Runtime:\n" + (model.Territory.RuntimeActive ? "Active" : "Inactive");

            _permissionsText.text =
                "Ward Access\n\n" +
                "Permitted players: " + model.Ward.PermittedPlayers.Count + "\n\n" +
                BuildPermittedPlayersText(model);

            _settingsText.text =
                "Territory Settings\n\n" +
                "Name:\n" + model.Territory.Name + "\n\n" +
                "Guild access:\n" + (model.Territory.GuildAccessEnabled ? "Enabled" : "Disabled") + "\n\n" +
                "Group access:\n" + (model.Territory.GroupAccessEnabled ? "Enabled" : "Disabled") + "\n\n" +
                "Rules:\n" + model.Territory.RulesSummary;

            _root.SetActive(true);
            _hiddenFrames = 0;
        }

        public void Hide()
        {
            if (_root == null)
                return;

            _root.SetActive(false);
            _hiddenFrames = 9999;
        }

        public void Tick(PrivateArea privateArea, Player player)
        {
            if (_root == null || !_root.activeSelf)
            {
                _hiddenFrames++;
                return;
            }

            _hiddenFrames = 0;

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
            if (_root == null)
                return;

            UnityEngine.Object.Destroy(_root);

            _root = null;
            _panel = null;
            _title = null;
            _subtitle = null;
            _overviewText = null;
            _permissionsText = null;
            _settingsText = null;
            _overviewButton = null;
            _permissionsButton = null;
            _settingsButton = null;
            _closeButton = null;
            _overviewPanel = null;
            _permissionsPanel = null;
            _settingsPanel = null;
            _showOverviewAction = null;
            _showWardAction = null;
            _showTerritoryAction = null;
            _closeByInputAction = null;
            _closeByDistanceAction = null;
            _toggleProtectionButton = null;
            _toggleProtectionAction = null;
            _renameTerritoryButton = null;
            _renameTerritoryAction = null;
        }

        public void ShowOverviewPanel()
        {
            SetActiveTab(_overviewPanel);
        }

        public void ShowWardPanel()
        {
            SetActiveTab(_permissionsPanel);
        }

        public void ShowTerritoryPanel()
        {
            SetActiveTab(_settingsPanel);
        }

        private void EnsureCreated()
        {
            if (_root != null)
                return;

            Transform parent = GetUiParent();

            _root = new GameObject("ClanTerritory_WardMenu");
            _root.transform.SetParent(parent, false);
            _root.SetActive(false);

            Canvas canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            _root.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            CreatePanel();
        }

        private Transform GetUiParent()
        {
            if (Hud.instance != null && Hud.instance.m_rootObject != null)
                return Hud.instance.m_rootObject.transform;

            return null;
        }

        private void CreatePanel()
        {
            _panel = CreatePanelObject(
                "Panel",
                _root.transform,
                new Vector2(760f, 520f),
                Vector2.zero,
                new Color(0f, 0f, 0f, 0.88f));

            _title = CreateText(
                "Title",
                _panel.transform,
                new Vector2(0f, 220f),
                new Vector2(700f, 44f),
                30,
                TextAlignmentOptions.Center,
                Color.white);

            _subtitle = CreateText(
                "Subtitle",
                _panel.transform,
                new Vector2(0f, 185f),
                new Vector2(700f, 32f),
                18,
                TextAlignmentOptions.Center,
                new Color(0.75f, 0.75f, 0.75f, 1f));

            _overviewButton = CreateButton(
                "OverviewButton",
                _panel.transform,
                new Vector2(-235f, 135f),
                new Vector2(190f, 42f),
                "Overview");

            _permissionsButton = CreateButton(
                "PermissionsButton",
                _panel.transform,
                new Vector2(0f, 135f),
                new Vector2(190f, 42f),
                "Ward");

            _settingsButton = CreateButton(
                "SettingsButton",
                _panel.transform,
                new Vector2(235f, 135f),
                new Vector2(190f, 42f),
                "Territory");

            _overviewPanel = CreateContentPanel("OverviewPanel");
            _permissionsPanel = CreateContentPanel("WardPanel");
            _settingsPanel = CreateContentPanel("TerritoryPanel");

            _overviewText = CreateText(
                "OverviewText",
                _overviewPanel.transform,
                Vector2.zero,
                new Vector2(650f, 230f),
                20,
                TextAlignmentOptions.TopLeft,
                Color.white);

            _permissionsText = CreateText(
                "WardText",
                _permissionsPanel.transform,
                new Vector2(0f, 35f),
                new Vector2(650f, 170f),
                20,
                TextAlignmentOptions.TopLeft,
                Color.white);

            _toggleProtectionButton = CreateButton(
                "ToggleProtectionButton",
                _permissionsPanel.transform,
                new Vector2(0f, -105f),
                new Vector2(240f, 38f),
                "Toggle Protection");

            _settingsText = CreateText(
                "TerritoryText",
                _settingsPanel.transform,
                new Vector2(0f, 35f),
                new Vector2(650f, 170f),
                20,
                TextAlignmentOptions.TopLeft,
                Color.white);

            _renameTerritoryButton = CreateButton(
                "RenameTerritoryButton",
                _settingsPanel.transform,
                new Vector2(0f, -105f),
                new Vector2(240f, 38f),
                "Rename Territory");

            _closeButton = CreateButton(
                "CloseButton",
                _panel.transform,
                new Vector2(0f, -220f),
                new Vector2(180f, 44f),
                "Close");

            _overviewButton.onClick.AddListener(RequestShowOverview);
            _permissionsButton.onClick.AddListener(RequestShowWard);
            _settingsButton.onClick.AddListener(RequestShowTerritory);
            _closeButton.onClick.AddListener(RequestCloseByInput);
            _toggleProtectionButton.onClick.AddListener(RequestToggleProtection);
            _renameTerritoryButton.onClick.AddListener(RequestRenameTerritory);
        }

        private GameObject CreateContentPanel(string name)
        {
            return CreatePanelObject(
                name,
                _panel.transform,
                new Vector2(680f, 260f),
                new Vector2(0f, -25f),
                new Color(0.08f, 0.08f, 0.08f, 0.9f));
        }

        private GameObject CreatePanelObject(
            string name,
            Transform parent,
            Vector2 size,
            Vector2 position,
            Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            Image image = panel.AddComponent<Image>();
            image.color = color;

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            return panel;
        }

        private TMP_Text CreateText(
            string name,
            Transform parent,
            Vector2 position,
            Vector2 size,
            int fontSize,
            TextAlignmentOptions alignment,
            Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.text = "";
            text.textWrappingMode = TextWrappingModes.Normal;

            return text;
        }

        private Button CreateButton(
            string name,
            Transform parent,
            Vector2 position,
            Vector2 size,
            string label)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.24f, 0.24f, 0.24f, 1f);

            Button button = buttonObject.AddComponent<Button>();

            TMP_Text buttonText = CreateText(
                "Text",
                buttonObject.transform,
                Vector2.zero,
                size,
                18,
                TextAlignmentOptions.Center,
                Color.white);

            buttonText.text = label;

            return button;
        }

        private void SetActiveTab(GameObject activePanel)
        {
            _overviewPanel.SetActive(activePanel == _overviewPanel);
            _permissionsPanel.SetActive(activePanel == _permissionsPanel);
            _settingsPanel.SetActive(activePanel == _settingsPanel);
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