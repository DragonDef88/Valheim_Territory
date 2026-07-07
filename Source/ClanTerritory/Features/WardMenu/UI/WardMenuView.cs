using System;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Runtime.Registry;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClanTerritory.Features.WardMenu.UI
{
    internal sealed class WardMenuView
    {
        private const float HideDistance = 5f;

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

        private Action _closeAction;
        private int _hiddenFrames = 9999;
        private bool _useReleasedAfterOpen;

        public bool IsVisible
        {
            get { return _root != null && _root.activeSelf && _hiddenFrames <= 1; }
        }

        public void Show(
            WardId wardId,
            RuntimeWard runtimeWard,
            PrivateArea privateArea,
            Player player,
            Action closeAction)
        {
            EnsureCreated();

            _closeAction = closeAction;
            _useReleasedAfterOpen = false;

            _title.text = "Clan Territory";
            _subtitle.text = "Ward Management";

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();
            ZDO zdo = zNetView != null && zNetView.IsValid()
                ? zNetView.GetZDO()
                : null;

            string creatorName = zdo != null
                ? zdo.GetString(ZDOVars.s_creatorName, "Unknown")
                : "Unknown";

            bool enabled = zdo != null && zdo.GetBool(ZDOVars.s_enabled);

            int permittedCount = zdo != null
                ? zdo.GetInt(ZDOVars.s_permitted)
                : 0;

            _overviewText.text =
                "Territory Overview\n\n" +
                "Ward ID:\n" + wardId + "\n\n" +
                "Owner:\n" + creatorName + "\n\n" +
                "Radius:\n" + privateArea.m_radius + "\n\n" +
                "Enabled:\n" + (enabled ? "Yes" : "No") + "\n\n" +
                "Runtime:\n" + (runtimeWard.IsActive ? "Active" : "Inactive");

            _permissionsText.text =
                "Permissions\n\n" +
                "Permitted players: " + permittedCount + "\n\n" +
                BuildPermittedPlayersText(zdo, permittedCount);

            _settingsText.text =
                "Settings\n\n" +
                "Territory name:\n" +
                "Coming next.\n\n" +
                "Clan access:\n" +
                "Coming next.";

            ShowOverview();

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
                RequestClose();
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
                (ZInput.GetKeyDown(KeyCode.Escape, true) ||
                 usePressed ||
                 ZInput.GetButtonDown("JoyButtonB")))
            {
                ZInput.ResetButtonStatus("Use");
                ZInput.ResetButtonStatus("JoyUse");
                ZInput.ResetButtonStatus("JoyButtonB");
                RequestClose();
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
            _closeAction = null;
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
                "Permissions");

            _settingsButton = CreateButton(
                "SettingsButton",
                _panel.transform,
                new Vector2(235f, 135f),
                new Vector2(190f, 42f),
                "Settings");

            _overviewPanel = CreateContentPanel("OverviewPanel");
            _permissionsPanel = CreateContentPanel("PermissionsPanel");
            _settingsPanel = CreateContentPanel("SettingsPanel");

            _overviewText = CreateText(
                "OverviewText",
                _overviewPanel.transform,
                Vector2.zero,
                new Vector2(650f, 230f),
                20,
                TextAlignmentOptions.TopLeft,
                Color.white);

            _permissionsText = CreateText(
                "PermissionsText",
                _permissionsPanel.transform,
                Vector2.zero,
                new Vector2(650f, 230f),
                20,
                TextAlignmentOptions.TopLeft,
                Color.white);

            _settingsText = CreateText(
                "SettingsText",
                _settingsPanel.transform,
                Vector2.zero,
                new Vector2(650f, 230f),
                20,
                TextAlignmentOptions.TopLeft,
                Color.white);

            _closeButton = CreateButton(
                "CloseButton",
                _panel.transform,
                new Vector2(0f, -220f),
                new Vector2(180f, 44f),
                "Close");

            _overviewButton.onClick.AddListener(ShowOverview);
            _permissionsButton.onClick.AddListener(ShowPermissions);
            _settingsButton.onClick.AddListener(ShowSettings);
            _closeButton.onClick.AddListener(RequestClose);
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
            text.enableWordWrapping = true;

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

        private void ShowOverview()
        {
            SetActiveTab(_overviewPanel);
        }

        private void ShowPermissions()
        {
            SetActiveTab(_permissionsPanel);
        }

        private void ShowSettings()
        {
            SetActiveTab(_settingsPanel);
        }

        private void SetActiveTab(GameObject activePanel)
        {
            _overviewPanel.SetActive(activePanel == _overviewPanel);
            _permissionsPanel.SetActive(activePanel == _permissionsPanel);
            _settingsPanel.SetActive(activePanel == _settingsPanel);
        }

        private static string BuildPermittedPlayersText(ZDO zdo, int permittedCount)
        {
            if (zdo == null || permittedCount <= 0)
                return "No permitted players.";

            string text = "";

            for (int i = 0; i < permittedCount; i++)
            {
                long playerId = zdo.GetLong("pu_id" + i, 0L);
                string playerName = zdo.GetString("pu_name" + i, "Unknown");

                if (playerId == 0L)
                    continue;

                text += "- " + playerName + " (" + playerId + ")\n";
            }

            if (string.IsNullOrEmpty(text))
                return "No permitted players.";

            return text;
        }

        private void RequestClose()
        {
            if (_closeAction != null)
                _closeAction();
        }
    }
}