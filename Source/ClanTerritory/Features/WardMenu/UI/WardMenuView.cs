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
        private TMP_Text _body;
        private Button _closeButton;
        private Action _closeAction;
        private int _hiddenFrames = 9999;

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

            _title.text = "Clan Territory";
            _body.text =
                "Ward menu\n\n" +
                "Ward: " + wardId + "\n" +
                "Position: " + runtimeWard.Position + "\n\n" +
                "This is the first UI shell.";

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

            if ((Chat.instance == null || !Chat.instance.HasFocus()) &&
                !Console.IsVisible() &&
                !Menu.IsVisible() &&
                !Minimap.IsOpen() &&
                (ZInput.GetKeyDown(KeyCode.Escape, true) ||
                 ZInput.GetButtonDown("Use") ||
                 ZInput.GetButtonDown("JoyButtonB")))
            {
                ZInput.ResetButtonStatus("Use");
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
            _body = null;
            _closeButton = null;
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
            _panel = new GameObject("Panel");
            _panel.transform.SetParent(_root.transform, false);

            Image panelImage = _panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.85f);

            RectTransform panelRect = _panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520f, 360f);
            panelRect.anchoredPosition = Vector2.zero;

            _title = CreateText(
                "Title",
                _panel.transform,
                new Vector2(0f, 125f),
                new Vector2(460f, 60f),
                28,
                TextAlignmentOptions.Center);

            _body = CreateText(
                "Body",
                _panel.transform,
                new Vector2(0f, 15f),
                new Vector2(460f, 170f),
                20,
                TextAlignmentOptions.TopLeft);

            _closeButton = CreateButton(
                "CloseButton",
                _panel.transform,
                new Vector2(0f, -125f),
                new Vector2(180f, 48f),
                "Close");

            _closeButton.onClick.AddListener(RequestClose);
        }

        private TMP_Text CreateText(
            string name,
            Transform parent,
            Vector2 position,
            Vector2 size,
            int fontSize,
            TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = "";

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
            image.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            Button button = buttonObject.AddComponent<Button>();

            TMP_Text buttonText = CreateText(
                "Text",
                buttonObject.transform,
                Vector2.zero,
                size,
                20,
                TextAlignmentOptions.Center);

            buttonText.text = label;

            return button;
        }

        private void RequestClose()
        {
            if (_closeAction != null)
                _closeAction();
        }
    }
}