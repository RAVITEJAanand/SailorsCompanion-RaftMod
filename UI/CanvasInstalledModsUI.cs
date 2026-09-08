using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace SailorsCompanion.UI
{
    #region [START] CANVAS INSTALLED MODS MANAGER UI
    // ============================================================================
    // [START] CANVAS INSTALLED MODS MANAGER UI
    // Purpose: Unified In-Game Mods Manager dialog accessible via the main menu
    //          "MODS" button. Displays all installed mods (Sailor's Companion &
    //          Inventory Master) with status, hotkeys, features, and settings.
    // ============================================================================
    public class CanvasInstalledModsUI : MonoBehaviour
    {
        public static CanvasInstalledModsUI Instance { get; private set; }

        private GameObject _canvasGO;
        private Canvas _canvas;
        private CanvasScaler _scaler;
        private GraphicRaycaster _raycaster;
        private GameObject _rootGO;
        private GameObject _windowGO;
        private Font _gameFont;

        public static bool IsOpen => Instance != null && Instance._rootGO != null && Instance._rootGO.activeSelf;

        // Raft Timber Color Palette
        private static readonly Color BgDimmer          = new Color(0.0f, 0.0f, 0.0f, 0.72f);
        private static readonly Color WoodWindowBg      = new Color(0.24f, 0.15f, 0.08f, 0.98f); // Deep Teak Timber
        private static readonly Color WoodWindowBorder  = new Color(0.16f, 0.09f, 0.04f, 1.00f); // Dark Outer Timber
        private static readonly Color WoodTitleBar      = new Color(0.20f, 0.12f, 0.06f, 1.00f); // Header Bar
        private static readonly Color CardBg            = new Color(0.18f, 0.11f, 0.06f, 0.95f); // Recessed Plank Box
        private static readonly Color TextGoldHeading   = new Color(0.96f, 0.78f, 0.38f, 1.00f); // Gold Stencil
        private static readonly Color TextParchment     = new Color(0.92f, 0.85f, 0.72f, 1.00f); // Warm Ivory
        private static readonly Color TextMuted         = new Color(0.68f, 0.58f, 0.45f, 1.00f); // Muted Timber
        private static readonly Color BadgeActiveBg     = new Color(0.10f, 0.45f, 0.20f, 1.00f); // Green Pill Badge
        private static readonly Color BadgeActiveText   = new Color(0.40f, 1.00f, 0.50f, 1.00f); // Vibrant Green Text
        private static readonly Color SailorsCyan       = new Color(0.00f, 0.90f, 1.00f, 1.00f); // Cyan Accent
        private static readonly Color InventoryGold     = new Color(1.00f, 0.68f, 0.20f, 1.00f); // Amber Accent
        private static readonly Color ButtonWoodNormal  = new Color(0.35f, 0.22f, 0.13f, 0.98f); // Wood Plank Button

        private void Awake()
        {
            Instance = this;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            GetGameFont();
            BuildCanvasUI();
        }

        public Font GetGameFont()
        {
            if (_gameFont != null) return _gameFont;

            var texts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (var t in texts)
            {
                if (t != null && t.font != null)
                {
                    _gameFont = t.font;
                    return _gameFont;
                }
            }

            try
            {
                _gameFont = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Segoe UI", "Tahoma" }, 14);
            }
            catch { }

            if (_gameFont == null)
            {
                _gameFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            return _gameFont;
        }

        public static void Toggle()
        {
            if (Instance == null)
            {
                var go = new GameObject("SailorsCompanion_InstalledModsUI");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<CanvasInstalledModsUI>();
            }

            if (Instance._rootGO == null)
            {
                Instance.BuildCanvasUI();
            }

            bool newState = !Instance._rootGO.activeSelf;
            Instance._rootGO.SetActive(newState);

            if (newState)
            {
                Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None);
            }
        }

        public static void Close()
        {
            if (Instance != null && Instance._rootGO != null)
            {
                Instance._rootGO.SetActive(false);
            }
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private void BuildCanvasUI()
        {
            if (_canvasGO == null)
            {
                _canvasGO = new GameObject("InstalledMods_Canvas");
                _canvasGO.hideFlags = HideFlags.HideAndDontSave;
                _canvasGO.layer = LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5;
                DontDestroyOnLoad(_canvasGO);

                _canvas = _canvasGO.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.overrideSorting = true;
                _canvas.sortingOrder = 32500; // Render above everything

                _scaler = _canvasGO.AddComponent<CanvasScaler>();
                _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _scaler.referenceResolution = new Vector2(1920, 1080);
                _scaler.matchWidthOrHeight = 0.5f;

                _raycaster = _canvasGO.AddComponent<GraphicRaycaster>();
            }

            if (_rootGO == null)
            {
                BuildManagerWindow();
                _rootGO.SetActive(false); // Entire root including dimmer is hidden by default!
            }
        }

        private void BuildManagerWindow()
        {
            // 1. Root Container (toggles Dimmer + Window together)
            _rootGO = new GameObject("Root_InstalledMods");
            _rootGO.transform.SetParent(_canvasGO.transform, false);
            var rootRt = _rootGO.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            // 2. Dimmer Background inside Root
            var dimmerGO = new GameObject("Dimmer_Background");
            dimmerGO.transform.SetParent(_rootGO.transform, false);
            var dimmerRt = dimmerGO.AddComponent<RectTransform>();
            dimmerRt.anchorMin = Vector2.zero;
            dimmerRt.anchorMax = Vector2.one;
            dimmerRt.offsetMin = Vector2.zero;
            dimmerRt.offsetMax = Vector2.zero;
            var dimmerImg = dimmerGO.AddComponent<Image>();
            dimmerImg.color = BgDimmer;
            var dimmerBtn = dimmerGO.AddComponent<Button>();
            dimmerBtn.onClick.AddListener(Close);

            // 3. Main Window Panel inside Root
            _windowGO = new GameObject("Window_InstalledMods");
            _windowGO.transform.SetParent(_rootGO.transform, false);

            var winRt = _windowGO.AddComponent<RectTransform>();
            winRt.anchorMin = new Vector2(0.5f, 0.5f);
            winRt.anchorMax = new Vector2(0.5f, 0.5f);
            winRt.pivot = new Vector2(0.5f, 0.5f);
            winRt.sizeDelta = new Vector2(980, 590);
            winRt.anchoredPosition = Vector2.zero;

            var winImg = _windowGO.AddComponent<Image>();
            winImg.color = WoodWindowBg;

            var winOutline = _windowGO.AddComponent<Outline>();
            winOutline.effectColor = WoodWindowBorder;
            winOutline.effectDistance = new Vector2(4, -4);

            // 4. Header Bar
            var headerGO = new GameObject("HeaderBar");
            headerGO.transform.SetParent(_windowGO.transform, false);
            var headRt = headerGO.AddComponent<RectTransform>();
            headRt.anchorMin = new Vector2(0, 1);
            headRt.anchorMax = new Vector2(1, 1);
            headRt.pivot = new Vector2(0.5f, 1);
            headRt.sizeDelta = new Vector2(0, 54);
            headRt.anchoredPosition = Vector2.zero;

            var headImg = headerGO.AddComponent<Image>();
            headImg.color = WoodTitleBar;

            var titleTxt = CreateText(headerGO, "🛠️ RAFT MODS MANAGER — INSTALLED MODS", 19, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleLeft);
            var titleRt = titleTxt.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.offsetMin = new Vector2(20, 0);
            titleRt.offsetMax = new Vector2(-60, 0);

            // Close button (X)
            var closeBtnGO = new GameObject("Btn_Close");
            closeBtnGO.transform.SetParent(headerGO.transform, false);
            var closeRt = closeBtnGO.AddComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1, 0.5f);
            closeRt.anchorMax = new Vector2(1, 0.5f);
            closeRt.pivot = new Vector2(1, 0.5f);
            closeRt.sizeDelta = new Vector2(40, 36);
            closeRt.anchoredPosition = new Vector2(-10, 0);

            var closeImg = closeBtnGO.AddComponent<Image>();
            closeImg.color = new Color(0.6f, 0.15f, 0.15f, 0.95f);
            var closeBtn = closeBtnGO.AddComponent<Button>();
            closeBtn.onClick.AddListener(Close);
            var closeTxt = CreateText(closeBtnGO, "✕", 18, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            FillParent(closeTxt.gameObject);

            // Subtitle Banner
            var subBannerGO = new GameObject("SubBanner");
            subBannerGO.transform.SetParent(_windowGO.transform, false);
            var subRt = subBannerGO.AddComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0, 1);
            subRt.anchorMax = new Vector2(1, 1);
            subRt.pivot = new Vector2(0.5f, 1);
            subRt.sizeDelta = new Vector2(0, 32);
            subRt.anchoredPosition = new Vector2(0, -56);

            var subTxt = CreateText(subBannerGO, "The following 2 Quality-of-Life modifications are active. In-game menus open during gameplay via their hotkeys.", 12, FontStyle.Italic, TextMuted, TextAnchor.MiddleCenter);
            FillParent(subTxt.gameObject);

            // Cards Container (Side by Side)
            var cardsContainerGO = new GameObject("CardsContainer");
            cardsContainerGO.transform.SetParent(_windowGO.transform, false);
            var cardsRt = cardsContainerGO.AddComponent<RectTransform>();
            cardsRt.anchorMin = new Vector2(0, 0);
            cardsRt.anchorMax = new Vector2(1, 1);
            cardsRt.offsetMin = new Vector2(24, 60);
            cardsRt.offsetMax = new Vector2(-24, -92);

            // Card 1: Sailor's Companion (Left)
            BuildModCard(cardsContainerGO,
                title: "⚓ Sailor's Companion",
                titleColor: SailorsCyan,
                version: "v1.1.0",
                author: "KONDURI (RAVITEJAanand)",
                description: "Complete navigation & survival suite with Real-Time Compass HUD, Auto Sail Align, Remote Anchor, Flight Mode, Shark Radar, Free Crafting, and 300+ Item Spawner.",
                hotkeysText: "🎮 In-Game Controls:\n• [F5] Mod Menu\n• [F6] Compass HUD Overlay\n• [F] Fly / Noclip Mode\n• [F4] Toggle Sails  |  [F3] Toggle Engines",
                openSettingsAction: () =>
                {
                    Close();
                    CanvasModUI.Instance?.ToggleModWindow();
                },
                openGithubUrl: "https://github.com/RAVITEJAanand/SailorsCompanion-RaftMod",
                isLeft: true
            );

            // Card 2: Inventory Master (Right)
            BuildModCard(cardsContainerGO,
                title: "🎒 Inventory Master",
                titleColor: InventoryGold,
                version: "v1.0.0",
                author: "KONDURI (RAVITEJAanand)",
                description: "Dedicated inventory management with Categorical Auto-Sort, 15-Slot Backpack Expansion, 20-Slot Hotbar Swap, 5m Auto-Pickup, Drop Guard, and Auto Water/Food Refill.",
                hotkeysText: "🎮 In-Game Controls:\n• [F2] Inventory Master Menu\n• [Z] Auto Sort Backpack / Chest\n• [X] Dump Backpack to Chest\n• [V] Hotbar Row Swap  |  [Alt+Click] Lock",
                openSettingsAction: () =>
                {
                    Close();
                    OpenInventoryMasterSettings();
                },
                openGithubUrl: "https://github.com/RAVITEJAanand/InventoryMaster-RaftMod",
                isLeft: false
            );

            // Footer Bar
            var footerGO = new GameObject("FooterBar");
            footerGO.transform.SetParent(_windowGO.transform, false);
            var footRt = footerGO.AddComponent<RectTransform>();
            footRt.anchorMin = new Vector2(0, 0);
            footRt.anchorMax = new Vector2(1, 0);
            footRt.pivot = new Vector2(0.5f, 0);
            footRt.sizeDelta = new Vector2(0, 50);
            footRt.anchoredPosition = Vector2.zero;

            var footImg = footerGO.AddComponent<Image>();
            footImg.color = WoodTitleBar;

            var footTxt = CreateText(footerGO, "💡 Hotkeys summary: [F5] Sailor's Companion  |  [F2] Inventory Master  |  [ESC] Close", 13, FontStyle.Normal, TextGoldHeading, TextAnchor.MiddleLeft);
            var footTxtRt = footTxt.GetComponent<RectTransform>();
            footTxtRt.anchorMin = new Vector2(0, 0);
            footTxtRt.anchorMax = new Vector2(1, 1);
            footTxtRt.offsetMin = new Vector2(24, 0);
            footTxtRt.offsetMax = new Vector2(-150, 0);

            var footCloseBtnGO = new GameObject("Btn_FootClose");
            footCloseBtnGO.transform.SetParent(footerGO.transform, false);
            var footCloseRt = footCloseBtnGO.AddComponent<RectTransform>();
            footCloseRt.anchorMin = new Vector2(1, 0.5f);
            footCloseRt.anchorMax = new Vector2(1, 0.5f);
            footCloseRt.pivot = new Vector2(1, 0.5f);
            footCloseRt.sizeDelta = new Vector2(120, 34);
            footCloseRt.anchoredPosition = new Vector2(-20, 0);

            var footCloseImg = footCloseBtnGO.AddComponent<Image>();
            footCloseImg.color = ButtonWoodNormal;
            var footCloseBtn = footCloseBtnGO.AddComponent<Button>();
            footCloseBtn.onClick.AddListener(Close);
            var footCloseTxt = CreateText(footCloseBtnGO, "Close (ESC)", 12, FontStyle.Bold, TextParchment, TextAnchor.MiddleCenter);
            FillParent(footCloseTxt.gameObject);
        }

        private void BuildModCard(GameObject parent, string title, Color titleColor, string version, string author, string description, string hotkeysText, Action openSettingsAction, string openGithubUrl, bool isLeft)
        {
            var cardGO = new GameObject(isLeft ? "Card_Left" : "Card_Right");
            cardGO.transform.SetParent(parent.transform, false);
            var cardRt = cardGO.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(isLeft ? 0f : 0.515f, 0f);
            cardRt.anchorMax = new Vector2(isLeft ? 0.485f : 1.0f, 1f);
            cardRt.offsetMin = Vector2.zero;
            cardRt.offsetMax = Vector2.zero;

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = CardBg;

            var cardOutline = cardGO.AddComponent<Outline>();
            cardOutline.effectColor = titleColor * 0.7f;
            cardOutline.effectDistance = new Vector2(2, -2);

            // Card Header Row (Title + Version + Active Badge)
            var cardHeadGO = new GameObject("CardHead");
            cardHeadGO.transform.SetParent(cardGO.transform, false);
            var headRt = cardHeadGO.AddComponent<RectTransform>();
            headRt.anchorMin = new Vector2(0, 1);
            headRt.anchorMax = new Vector2(1, 1);
            headRt.pivot = new Vector2(0.5f, 1);
            headRt.sizeDelta = new Vector2(0, 42);
            headRt.anchoredPosition = new Vector2(0, -10);

            var tTxt = CreateText(cardHeadGO, title, 18, FontStyle.Bold, titleColor, TextAnchor.MiddleLeft);
            var tRt = tTxt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0, 0);
            tRt.anchorMax = new Vector2(0.7f, 1);
            tRt.offsetMin = new Vector2(14, 0);
            tRt.offsetMax = Vector2.zero;

            // Status Badge
            var badgeGO = new GameObject("Badge");
            badgeGO.transform.SetParent(cardHeadGO.transform, false);
            var badgeRt = badgeGO.AddComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(1, 0.5f);
            badgeRt.anchorMax = new Vector2(1, 0.5f);
            badgeRt.pivot = new Vector2(1, 0.5f);
            badgeRt.sizeDelta = new Vector2(110, 24);
            badgeRt.anchoredPosition = new Vector2(-12, 0);

            var badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.color = BadgeActiveBg;
            var badgeTxt = CreateText(badgeGO, $"● ACTIVE ({version})", 10, FontStyle.Bold, BadgeActiveText, TextAnchor.MiddleCenter);
            FillParent(badgeTxt.gameObject);

            // Author Text
            var authGO = new GameObject("AuthorText");
            authGO.transform.SetParent(cardGO.transform, false);
            var authRt = authGO.AddComponent<RectTransform>();
            authRt.anchorMin = new Vector2(0, 1);
            authRt.anchorMax = new Vector2(1, 1);
            authRt.pivot = new Vector2(0.5f, 1);
            authRt.sizeDelta = new Vector2(0, 20);
            authRt.anchoredPosition = new Vector2(0, -50);

            var aTxt = CreateText(authGO, $"By: {author}", 11, FontStyle.Italic, TextMuted, TextAnchor.MiddleLeft);
            var aRt = aTxt.GetComponent<RectTransform>();
            aRt.anchorMin = Vector2.zero;
            aRt.anchorMax = Vector2.one;
            aRt.offsetMin = new Vector2(14, 0);
            aRt.offsetMax = new Vector2(-14, 0);

            // Description Box
            var descGO = new GameObject("DescBox");
            descGO.transform.SetParent(cardGO.transform, false);
            var descRt = descGO.AddComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0, 1);
            descRt.anchorMax = new Vector2(1, 1);
            descRt.pivot = new Vector2(0.5f, 1);
            descRt.sizeDelta = new Vector2(0, 80);
            descRt.anchoredPosition = new Vector2(0, -75);

            var dTxt = CreateText(descGO, description, 12, FontStyle.Normal, TextParchment, TextAnchor.UpperLeft);
            dTxt.lineSpacing = 1.15f;
            var dRt = dTxt.GetComponent<RectTransform>();
            dRt.anchorMin = Vector2.zero;
            dRt.anchorMax = Vector2.one;
            dRt.offsetMin = new Vector2(14, 0);
            dRt.offsetMax = new Vector2(-14, 0);

            // Hotkeys Box
            var hotkeyBoxGO = new GameObject("HotkeyBox");
            hotkeyBoxGO.transform.SetParent(cardGO.transform, false);
            var hkRt = hotkeyBoxGO.AddComponent<RectTransform>();
            hkRt.anchorMin = new Vector2(0, 1);
            hkRt.anchorMax = new Vector2(1, 1);
            hkRt.pivot = new Vector2(0.5f, 1);
            hkRt.sizeDelta = new Vector2(0, 120);
            hkRt.anchoredPosition = new Vector2(0, -165);

            var hkBg = hotkeyBoxGO.AddComponent<Image>();
            hkBg.color = new Color(0.12f, 0.07f, 0.04f, 0.90f);

            var hkTxt = CreateText(hotkeyBoxGO, hotkeysText, 12, FontStyle.Normal, TextGoldHeading, TextAnchor.UpperLeft);
            hkTxt.lineSpacing = 1.2f;
            var hkTxtRt = hkTxt.GetComponent<RectTransform>();
            hkTxtRt.anchorMin = Vector2.zero;
            hkTxtRt.anchorMax = Vector2.one;
            hkTxtRt.offsetMin = new Vector2(12, 6);
            hkTxtRt.offsetMax = new Vector2(-12, -6);

            // Action Buttons Row (Open Settings & GitHub)
            var btnRowGO = new GameObject("BtnRow");
            btnRowGO.transform.SetParent(cardGO.transform, false);
            var brRt = btnRowGO.AddComponent<RectTransform>();
            brRt.anchorMin = new Vector2(0, 0);
            brRt.anchorMax = new Vector2(1, 0);
            brRt.pivot = new Vector2(0.5f, 0);
            brRt.sizeDelta = new Vector2(0, 42);
            brRt.anchoredPosition = new Vector2(0, 14);

            // Button 1: Settings
            var btnSettingsGO = new GameObject("Btn_Settings");
            btnSettingsGO.transform.SetParent(btnRowGO.transform, false);
            var bsRt = btnSettingsGO.AddComponent<RectTransform>();
            bsRt.anchorMin = new Vector2(0, 0);
            bsRt.anchorMax = new Vector2(0.62f, 1);
            bsRt.offsetMin = new Vector2(14, 0);
            bsRt.offsetMax = new Vector2(-6, 0);

            var bsImg = btnSettingsGO.AddComponent<Image>();
            bsImg.color = ButtonWoodNormal;
            var bsBtn = btnSettingsGO.AddComponent<Button>();
            bsBtn.onClick.AddListener(() => openSettingsAction?.Invoke());
            var bsTxt = CreateText(btnSettingsGO, "⚙️ Open Settings", 13, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleCenter);
            FillParent(bsTxt.gameObject);

            // Button 2: GitHub
            var btnGitGO = new GameObject("Btn_GitHub");
            btnGitGO.transform.SetParent(btnRowGO.transform, false);
            var bgRt = btnGitGO.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0.64f, 0);
            bgRt.anchorMax = new Vector2(1f, 1);
            bgRt.offsetMin = new Vector2(6, 0);
            bgRt.offsetMax = new Vector2(-14, 0);

            var bgImg = btnGitGO.AddComponent<Image>();
            bgImg.color = new Color(0.20f, 0.14f, 0.08f, 0.95f);
            var bgBtn = btnGitGO.AddComponent<Button>();
            bgBtn.onClick.AddListener(() => Application.OpenURL(openGithubUrl));
            var bgTxt = CreateText(btnGitGO, "🌐 GitHub", 12, FontStyle.Normal, TextParchment, TextAnchor.MiddleCenter);
            FillParent(bgTxt.gameObject);
        }

        private void OpenInventoryMasterSettings()
        {
            try
            {
                var asmList = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in asmList)
                {
                    var t = asm.GetType("InventoryMaster.UI.CanvasInventoryMasterUI");
                    if (t != null)
                    {
                        var m = t.GetMethod("ToggleWindow", BindingFlags.Public | BindingFlags.Static);
                        if (m != null)
                        {
                            m.Invoke(null, null);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Mods Manager] Failed to open Inventory Master menu: " + ex.Message);
            }
        }

        private Text CreateText(GameObject parent, string text, int fontSize, FontStyle style, Color color, TextAnchor alignment)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent.transform, false);
            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = GetGameFont();
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = alignment;
            t.raycastTarget = false;
            return t;
        }

        private void FillParent(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
    // ============================================================================
    // [END] CANVAS INSTALLED MODS MANAGER UI
    // ============================================================================
    #endregion
}
