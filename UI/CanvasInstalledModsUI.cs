using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace SailorsCompanion.UI
{
    #region [START] CANVAS INSTALLED MODS MANAGER UI (PREMIUM HIGH-RES EDITION)
    // ============================================================================
    // [START] CANVAS INSTALLED MODS MANAGER UI (PREMIUM HIGH-RES EDITION)
    // Purpose: Beautiful, high-resolution In-Game Mods Manager dialog accessible
    //          via the main menu "MODS" button. Crystal clear typography, high
    //          contrast, larger fonts (14-22px), and native Raft timber styling.
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

        // Raft Timber High-Contrast Color Palette
        private static readonly Color BgDimmer          = new Color(0.0f, 0.0f, 0.0f, 0.76f);
        private static readonly Color WoodWindowBg      = new Color(0.24f, 0.15f, 0.08f, 0.99f); // Deep Teak Timber
        private static readonly Color WoodWindowBorder  = new Color(0.14f, 0.08f, 0.04f, 1.00f); // Dark Outer Timber
        private static readonly Color WoodTitleBar      = new Color(0.19f, 0.11f, 0.05f, 1.00f); // Header Bar
        private static readonly Color WoodTrimAccent    = new Color(0.85f, 0.70f, 0.42f, 1.00f); // Golden Wood Trim
        private static readonly Color CardBg            = new Color(0.17f, 0.10f, 0.05f, 0.98f); // Recessed Plank Box
        private static readonly Color PlaqueBg          = new Color(0.11f, 0.06f, 0.03f, 0.95f); // Dark Plaque Inset

        // High-Contrast Text Colors
        private static readonly Color TextGoldHeading   = new Color(1.00f, 0.82f, 0.35f, 1.00f); // Bright Gold Stencil
        private static readonly Color TextParchment     = new Color(0.96f, 0.94f, 0.88f, 1.00f); // Clean Crisp Ivory
        private static readonly Color TextMutedGold     = new Color(0.90f, 0.78f, 0.55f, 1.00f); // Soft Gold
        private static readonly Color TextSubtle        = new Color(0.80f, 0.72f, 0.60f, 1.00f); // Parchment Muted

        // Status & Theme Accents
        private static readonly Color BadgeActiveBg     = new Color(0.06f, 0.42f, 0.18f, 1.00f); // Deep Emerald Pill
        private static readonly Color BadgeActiveText   = new Color(0.25f, 1.00f, 0.55f, 1.00f); // Neon Mint Green
        private static readonly Color SailorsCyan       = new Color(0.10f, 0.92f, 1.00f, 1.00f); // Electric Cyan
        private static readonly Color InventoryGold     = new Color(1.00f, 0.72f, 0.20f, 1.00f); // Radiant Amber

        // Button Colors
        private static readonly Color ButtonWoodPrimary = new Color(0.42f, 0.26f, 0.15f, 1.00f); // Primary Wood Button
        private static readonly Color ButtonWoodBorder  = new Color(0.85f, 0.68f, 0.35f, 0.90f); // Gold Button Border
        private static readonly Color ButtonWoodDark    = new Color(0.26f, 0.16f, 0.09f, 1.00f); // Secondary Timber Button
        private static readonly Color ButtonCloseRed    = new Color(0.65f, 0.16f, 0.14f, 0.98f); // Crimson Close Button

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
                _gameFont = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Tahoma" }, 15);
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
                _rootGO.SetActive(false); // Cleanly hidden by default!
            }
        }

        private void BuildManagerWindow()
        {
            // 1. Root Container
            _rootGO = new GameObject("Root_InstalledMods");
            _rootGO.transform.SetParent(_canvasGO.transform, false);
            var rootRt = _rootGO.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            // 2. Dimmer Background
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

            // 3. Main Window Panel
            _windowGO = new GameObject("Window_InstalledMods");
            _windowGO.transform.SetParent(_rootGO.transform, false);

            var winRt = _windowGO.AddComponent<RectTransform>();
            winRt.anchorMin = new Vector2(0.5f, 0.5f);
            winRt.anchorMax = new Vector2(0.5f, 0.5f);
            winRt.pivot = new Vector2(0.5f, 0.5f);
            winRt.sizeDelta = new Vector2(1120, 650); // Generous, wide proportions
            winRt.anchoredPosition = Vector2.zero;
            winRt.localScale = new Vector3(1.05f, 1.05f, 1.0f); // High-res scaling

            var winImg = _windowGO.AddComponent<Image>();
            winImg.color = WoodWindowBg;

            var winOutline = _windowGO.AddComponent<Outline>();
            winOutline.effectColor = WoodWindowBorder;
            winOutline.effectDistance = new Vector2(5, -5);

            // 4. Header Bar
            var headerGO = new GameObject("HeaderBar");
            headerGO.transform.SetParent(_windowGO.transform, false);
            var headRt = headerGO.AddComponent<RectTransform>();
            headRt.anchorMin = new Vector2(0, 1);
            headRt.anchorMax = new Vector2(1, 1);
            headRt.pivot = new Vector2(0.5f, 1);
            headRt.sizeDelta = new Vector2(0, 58);
            headRt.anchoredPosition = Vector2.zero;

            var headImg = headerGO.AddComponent<Image>();
            headImg.color = WoodTitleBar;

            // Golden Header Trim Accent
            var headTrimGO = new GameObject("HeadTrim");
            headTrimGO.transform.SetParent(headerGO.transform, false);
            var htRt = headTrimGO.AddComponent<RectTransform>();
            htRt.anchorMin = new Vector2(0, 0);
            htRt.anchorMax = new Vector2(1, 0);
            htRt.pivot = new Vector2(0.5f, 0);
            htRt.sizeDelta = new Vector2(0, 3);
            var htImg = headTrimGO.AddComponent<Image>();
            htImg.color = WoodTrimAccent;

            // Header Title (Size 22, Bold, High-Contrast Gold)
            var titleTxt = CreateText(headerGO, "🛠️ <color=#FFD54F><b>RAFT MODS MANAGER</b></color>  <size=15><color=#E0D0B5>(2 Active Modifications Installed)</color></size>", 22, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleLeft);
            var titleRt = titleTxt.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.offsetMin = new Vector2(24, 0);
            titleRt.offsetMax = new Vector2(-70, 0);

            // Close Button [✕]
            var closeBtnGO = new GameObject("Btn_Close");
            closeBtnGO.transform.SetParent(headerGO.transform, false);
            var closeRt = closeBtnGO.AddComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1, 0.5f);
            closeRt.anchorMax = new Vector2(1, 0.5f);
            closeRt.pivot = new Vector2(1, 0.5f);
            closeRt.sizeDelta = new Vector2(44, 38);
            closeRt.anchoredPosition = new Vector2(-12, 0);

            var closeImg = closeBtnGO.AddComponent<Image>();
            closeImg.color = ButtonCloseRed;
            var closeBtn = closeBtnGO.AddComponent<Button>();
            closeBtn.onClick.AddListener(Close);
            var closeTxt = CreateText(closeBtnGO, "✕", 20, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            FillParent(closeTxt.gameObject);

            // Subtitle Guidance Banner (Size 14)
            var subBannerGO = new GameObject("SubBanner");
            subBannerGO.transform.SetParent(_windowGO.transform, false);
            var subRt = subBannerGO.AddComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0, 1);
            subRt.anchorMax = new Vector2(1, 1);
            subRt.pivot = new Vector2(0.5f, 1);
            subRt.sizeDelta = new Vector2(0, 36);
            subRt.anchoredPosition = new Vector2(0, -60);

            var subTxt = CreateText(subBannerGO, "✨ Click <b>'Open Settings'</b> to configure either mod, or review the in-game shortcut hotkeys below:", 14, FontStyle.Normal, TextParchment, TextAnchor.MiddleCenter);
            FillParent(subTxt.gameObject);

            // Cards Container
            var cardsContainerGO = new GameObject("CardsContainer");
            cardsContainerGO.transform.SetParent(_windowGO.transform, false);
            var cardsRt = cardsContainerGO.AddComponent<RectTransform>();
            cardsRt.anchorMin = new Vector2(0, 0);
            cardsRt.anchorMax = new Vector2(1, 1);
            cardsRt.offsetMin = new Vector2(24, 62);
            cardsRt.offsetMax = new Vector2(-24, -100);

            // Card 1: Sailor's Companion (Left)
            BuildModCard(cardsContainerGO,
                title: "⚓ Sailor's Companion",
                titleColor: SailorsCyan,
                version: "v1.1.0",
                author: "KONDURI (RAVITEJAanand)",
                featureList: "• <b>Compass HUD & Hostile Shark Radar</b> [F6]\n• <b>Auto-Align Sails to Wind & Remote Anchor</b> [F4]\n• <b>Free Flight / Noclip [F], God Mode & Stamina</b>\n• <b>300+ Item Spawner & Material-Free Crafting</b> [F5]",
                hotkeysSummary: "<color=#00F5FF><b>[F5]</b></color> Mod Menu   •   <color=#00F5FF><b>[F6]</b></color> Compass HUD   •   <color=#00F5FF><b>[F]</b></color> Fly Mode\n<color=#00F5FF><b>[F4]</b></color> Toggle Sails   •   <color=#00F5FF><b>[F3]</b></color> Toggle Engines   •   <color=#00F5FF><b>[F7]</b></color> Debris Magnet",
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
                featureList: "• <b>Permanent 15-Slot Backpack Expansion Unlock</b>\n• <b>Instant Categorical Inventory & Chest Sorter</b> [Z]\n• <b>20-Slot Hotbar Row Swap & One-Click Storage Dump</b> [V/X]\n• <b>5m Vacuum Auto-Pickup, Drop Guard [Q] & Auto-Refill</b>",
                hotkeysSummary: "<color=#FFB300><b>[F2]</b></color> Settings Menu   •   <color=#FFB300><b>[Z]</b></color> Auto Sort   •   <color=#FFB300><b>[X]</b></color> Dump to Chest\n<color=#FFB300><b>[V]</b></color> Swap Hotbar Row   •   <color=#FFB300><b>[Alt+Click]</b></color> Lock Slot   •   <color=#FFB300><b>[Del]</b></color> Trash",
                openSettingsAction: () =>
                {
                    Close();
                    OpenInventoryMasterSettings();
                },
                openGithubUrl: "https://github.com/RAVITEJAanand/InventoryMaster-RaftMod",
                isLeft: false
            );

            // 5. Footer Bar
            var footerGO = new GameObject("FooterBar");
            footerGO.transform.SetParent(_windowGO.transform, false);
            var footRt = footerGO.AddComponent<RectTransform>();
            footRt.anchorMin = new Vector2(0, 0);
            footRt.anchorMax = new Vector2(1, 0);
            footRt.pivot = new Vector2(0.5f, 0);
            footRt.sizeDelta = new Vector2(0, 52);
            footRt.anchoredPosition = Vector2.zero;

            var footImg = footerGO.AddComponent<Image>();
            footImg.color = WoodTitleBar;

            // Top Trim on Footer
            var footTrimGO = new GameObject("FootTrim");
            footTrimGO.transform.SetParent(footerGO.transform, false);
            var ftRt = footTrimGO.AddComponent<RectTransform>();
            ftRt.anchorMin = new Vector2(0, 1);
            ftRt.anchorMax = new Vector2(1, 1);
            ftRt.pivot = new Vector2(0.5f, 1);
            ftRt.sizeDelta = new Vector2(0, 2);
            var ftImg = footTrimGO.AddComponent<Image>();
            ftImg.color = WoodTrimAccent;

            // Footer Text (Size 14)
            var footTxt = CreateText(footerGO, "💡 <b>Quick Tip:</b> During active gameplay, press <b>[F5]</b> for Sailor's Companion or <b>[F2]</b> for Inventory Master.", 14, FontStyle.Normal, TextGoldHeading, TextAnchor.MiddleLeft);
            var footTxtRt = footTxt.GetComponent<RectTransform>();
            footTxtRt.anchorMin = new Vector2(0, 0);
            footTxtRt.anchorMax = new Vector2(1, 1);
            footTxtRt.offsetMin = new Vector2(24, 0);
            footTxtRt.offsetMax = new Vector2(-160, 0);

            // Footer Close Button (Size 14, Bold)
            var footCloseBtnGO = new GameObject("Btn_FootClose");
            footCloseBtnGO.transform.SetParent(footerGO.transform, false);
            var footCloseRt = footCloseBtnGO.AddComponent<RectTransform>();
            footCloseRt.anchorMin = new Vector2(1, 0.5f);
            footCloseRt.anchorMax = new Vector2(1, 0.5f);
            footCloseRt.pivot = new Vector2(1, 0.5f);
            footCloseRt.sizeDelta = new Vector2(130, 36);
            footCloseRt.anchoredPosition = new Vector2(-18, 0);

            var footCloseImg = footCloseBtnGO.AddComponent<Image>();
            footCloseImg.color = ButtonCloseRed;
            var footCloseBtn = footCloseBtnGO.AddComponent<Button>();
            footCloseBtn.onClick.AddListener(Close);
            var footCloseTxt = CreateText(footCloseBtnGO, "Close (ESC)", 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            FillParent(footCloseTxt.gameObject);
        }

        private void BuildModCard(GameObject parent, string title, Color titleColor, string version, string author, string featureList, string hotkeysSummary, Action openSettingsAction, string openGithubUrl, bool isLeft)
        {
            var cardGO = new GameObject(isLeft ? "Card_Sailors" : "Card_Inventory");
            cardGO.transform.SetParent(parent.transform, false);
            var cardRt = cardGO.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(isLeft ? 0f : 0.515f, 0f);
            cardRt.anchorMax = new Vector2(isLeft ? 0.485f : 1.0f, 1f);
            cardRt.offsetMin = Vector2.zero;
            cardRt.offsetMax = Vector2.zero;

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = CardBg;

            var cardOutline = cardGO.AddComponent<Outline>();
            cardOutline.effectColor = titleColor * 0.85f;
            cardOutline.effectDistance = new Vector2(3, -3);

            // 1. Card Header (Height: 46)
            var cardHeadGO = new GameObject("CardHead");
            cardHeadGO.transform.SetParent(cardGO.transform, false);
            var headRt = cardHeadGO.AddComponent<RectTransform>();
            headRt.anchorMin = new Vector2(0, 1);
            headRt.anchorMax = new Vector2(1, 1);
            headRt.pivot = new Vector2(0.5f, 1);
            headRt.sizeDelta = new Vector2(0, 46);
            headRt.anchoredPosition = new Vector2(0, -10);

            // Mod Title (Size 22, Bold)
            var tTxt = CreateText(cardHeadGO, title, 22, FontStyle.Bold, titleColor, TextAnchor.MiddleLeft);
            var tRt = tTxt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0, 0);
            tRt.anchorMax = new Vector2(0.68f, 1);
            tRt.offsetMin = new Vector2(16, 0);
            tRt.offsetMax = Vector2.zero;

            // Status Badge (Size 12, Bold, Emerald Pill)
            var badgeGO = new GameObject("Badge");
            badgeGO.transform.SetParent(cardHeadGO.transform, false);
            var badgeRt = badgeGO.AddComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(1, 0.5f);
            badgeRt.anchorMax = new Vector2(1, 0.5f);
            badgeRt.pivot = new Vector2(1, 0.5f);
            badgeRt.sizeDelta = new Vector2(130, 28);
            badgeRt.anchoredPosition = new Vector2(-14, 0);

            var badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.color = BadgeActiveBg;
            var badgeOutline = badgeGO.AddComponent<Outline>();
            badgeOutline.effectColor = BadgeActiveText * 0.5f;
            badgeOutline.effectDistance = new Vector2(1, -1);

            var badgeTxt = CreateText(badgeGO, $"● ACTIVE ({version})", 12, FontStyle.Bold, BadgeActiveText, TextAnchor.MiddleCenter);
            FillParent(badgeTxt.gameObject);

            // 2. Author Subtitle (Size 13)
            var authGO = new GameObject("AuthorText");
            authGO.transform.SetParent(cardGO.transform, false);
            var authRt = authGO.AddComponent<RectTransform>();
            authRt.anchorMin = new Vector2(0, 1);
            authRt.anchorMax = new Vector2(1, 1);
            authRt.pivot = new Vector2(0.5f, 1);
            authRt.sizeDelta = new Vector2(0, 22);
            authRt.anchoredPosition = new Vector2(0, -56);

            var aTxt = CreateText(authGO, $"Developer: <b>{author}</b>", 13, FontStyle.Normal, TextMutedGold, TextAnchor.MiddleLeft);
            var aRt = aTxt.GetComponent<RectTransform>();
            aRt.anchorMin = Vector2.zero;
            aRt.anchorMax = Vector2.one;
            aRt.offsetMin = new Vector2(16, 0);
            aRt.offsetMax = new Vector2(-16, 0);

            // 3. Highlighted Features List (Size 14-15, High-Contrast Ivory)
            var featGO = new GameObject("FeaturesList");
            featGO.transform.SetParent(cardGO.transform, false);
            var featRt = featGO.AddComponent<RectTransform>();
            featRt.anchorMin = new Vector2(0, 1);
            featRt.anchorMax = new Vector2(1, 1);
            featRt.pivot = new Vector2(0.5f, 1);
            featRt.sizeDelta = new Vector2(0, 130);
            featRt.anchoredPosition = new Vector2(0, -82);

            var fTxt = CreateText(featGO, featureList, 15, FontStyle.Normal, TextParchment, TextAnchor.UpperLeft);
            fTxt.lineSpacing = 1.25f;
            var fRt = fTxt.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.offsetMin = new Vector2(16, 0);
            fRt.offsetMax = new Vector2(-16, 0);

            // 4. Hotkeys Plaque Box (Size 14, Dark Wood Plaque)
            var hotkeyBoxGO = new GameObject("HotkeyBox");
            hotkeyBoxGO.transform.SetParent(cardGO.transform, false);
            var hkRt = hotkeyBoxGO.AddComponent<RectTransform>();
            hkRt.anchorMin = new Vector2(0, 1);
            hkRt.anchorMax = new Vector2(1, 1);
            hkRt.pivot = new Vector2(0.5f, 1);
            hkRt.sizeDelta = new Vector2(0, 115);
            hkRt.anchoredPosition = new Vector2(0, -220);

            var hkBg = hotkeyBoxGO.AddComponent<Image>();
            hkBg.color = PlaqueBg;

            var hkOutline = hotkeyBoxGO.AddComponent<Outline>();
            hkOutline.effectColor = titleColor * 0.5f;
            hkOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var hkLabelGO = new GameObject("HkLabel");
            hkLabelGO.transform.SetParent(hotkeyBoxGO.transform, false);
            var hklRt = hkLabelGO.AddComponent<RectTransform>();
            hklRt.anchorMin = new Vector2(0, 1);
            hklRt.anchorMax = new Vector2(1, 1);
            hklRt.pivot = new Vector2(0.5f, 1);
            hklRt.sizeDelta = new Vector2(0, 24);
            hklRt.anchoredPosition = new Vector2(0, -6);
            var hklTxt = CreateText(hkLabelGO, "🎮 <b>IN-GAME CONTROLS & SHORTCUTS:</b>", 13, FontStyle.Normal, TextGoldHeading, TextAnchor.MiddleLeft);
            var hklTxtRt = hklTxt.GetComponent<RectTransform>();
            hklTxtRt.anchorMin = Vector2.zero;
            hklTxtRt.anchorMax = Vector2.one;
            hklTxtRt.offsetMin = new Vector2(14, 0);
            hklTxtRt.offsetMax = Vector2.zero;

            var hkTxt = CreateText(hotkeyBoxGO, hotkeysSummary, 14, FontStyle.Normal, TextParchment, TextAnchor.UpperLeft);
            hkTxt.lineSpacing = 1.30f;
            var hkTxtRt = hkTxt.GetComponent<RectTransform>();
            hkTxtRt.anchorMin = Vector2.zero;
            hkTxtRt.anchorMax = Vector2.one;
            hkTxtRt.offsetMin = new Vector2(14, 8);
            hkTxtRt.offsetMax = new Vector2(-14, -32);

            // 5. Action Buttons Row (Height: 52px, Prominent & High Contrast)
            var btnRowGO = new GameObject("BtnRow");
            btnRowGO.transform.SetParent(cardGO.transform, false);
            var brRt = btnRowGO.AddComponent<RectTransform>();
            brRt.anchorMin = new Vector2(0, 0);
            brRt.anchorMax = new Vector2(1, 0);
            brRt.pivot = new Vector2(0.5f, 0);
            brRt.sizeDelta = new Vector2(0, 52);
            brRt.anchoredPosition = new Vector2(0, 16);

            // Button 1: Configure / Open Settings (Size 15, Bold)
            var btnSettingsGO = new GameObject("Btn_Settings");
            btnSettingsGO.transform.SetParent(btnRowGO.transform, false);
            var bsRt = btnSettingsGO.AddComponent<RectTransform>();
            bsRt.anchorMin = new Vector2(0, 0);
            bsRt.anchorMax = new Vector2(0.64f, 1);
            bsRt.offsetMin = new Vector2(14, 0);
            bsRt.offsetMax = new Vector2(-8, 0);

            var bsImg = btnSettingsGO.AddComponent<Image>();
            bsImg.color = ButtonWoodPrimary;
            var bsOutline = btnSettingsGO.AddComponent<Outline>();
            bsOutline.effectColor = ButtonWoodBorder;
            bsOutline.effectDistance = new Vector2(2, -2);

            var bsBtn = btnSettingsGO.AddComponent<Button>();
            bsBtn.onClick.AddListener(() => openSettingsAction?.Invoke());
            var bsTxt = CreateText(btnSettingsGO, "⚙️ OPEN MOD SETTINGS", 15, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            FillParent(bsTxt.gameObject);

            // Button 2: GitHub (Size 14, Bold)
            var btnGitGO = new GameObject("Btn_GitHub");
            btnGitGO.transform.SetParent(btnRowGO.transform, false);
            var bgRt = btnGitGO.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0.66f, 0);
            bgRt.anchorMax = new Vector2(1f, 1);
            bgRt.offsetMin = new Vector2(6, 0);
            bgRt.offsetMax = new Vector2(-14, 0);

            var bgImg = btnGitGO.AddComponent<Image>();
            bgImg.color = ButtonWoodDark;
            var bgOutline = btnGitGO.AddComponent<Outline>();
            bgOutline.effectColor = TextMutedGold * 0.6f;
            bgOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var bgBtn = btnGitGO.AddComponent<Button>();
            bgBtn.onClick.AddListener(() => Application.OpenURL(openGithubUrl));
            var bgTxt = CreateText(btnGitGO, "🌐 GitHub", 14, FontStyle.Bold, TextMutedGold, TextAnchor.MiddleCenter);
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
            t.supportRichText = true;
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
