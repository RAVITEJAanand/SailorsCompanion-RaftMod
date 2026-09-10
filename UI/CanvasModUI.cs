using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SailorsCompanion.Features;

namespace SailorsCompanion.UI
{
    public class CanvasModUI : MonoBehaviour
    {
        public static CanvasModUI Instance { get; private set; }

        private GameObject _canvasGO;
        private Canvas _canvas;
        private CanvasScaler _scaler;
        private GraphicRaycaster _raycaster;

        // UI Panels
        private GameObject _modWindowGO;

        // Tab Panels
        private GameObject[] _tabPages = new GameObject[5];
        private Text[] _tabButtonTexts = new Text[5];
        private Image[] _tabButtonImages = new Image[5];
        private int _activeTab = 0;

        // Dynamic Text References
        private Text _navStatusText;
        private Text _teleHeadingText;
        private Text _teleRaftText;
        private Text _teleSharkText;
        private Text _teleCoordsText;
        private Text _teleNotifText;
        private List<GameObject> _navStyleBtns = new List<GameObject>();
        private Text _researchStatusText;

        // Item Spawner
        private InputField _itemSearchInput;
        private Transform _itemScrollContent;
        private List<Item_Base> _allItems;

        // Font
        private Font _gameFont;

        // Mode Switcher Controls
        private Transform _contentAreaTransform;
        private Image _btnModeSurvivalImg;
        private Image _btnModeCreativeImg;
        private Text _btnModeSurvivalTxt;
        private Text _btnModeCreativeTxt;
        private Text _navRecallBtnText;
        private Text _navScannerBtnText;
        private Text _navSailModeBtnText;
        private Text _qolMagnetBtnText;

        #region [START] RAFT NATIVE WOODEN PALETTE
        // ============================================================================
        // [START] RAFT NATIVE WOODEN PALETTE (Matching Raft's In-Game Settings Aesthetics)
        // ============================================================================
        private static readonly Color WoodWindowBg        = new Color(0.26f, 0.16f, 0.09f, 0.98f); // Deep Teak Plank #422917
        private static readonly Color WoodWindowBorder    = new Color(0.18f, 0.10f, 0.05f, 1.00f); // Dark Outer Timber #2E1A0D
        private static readonly Color WoodTitleBar        = new Color(0.22f, 0.13f, 0.07f, 1.00f); // Dark Wood Header #382112
        private static readonly Color WoodTrimAccent      = new Color(0.78f, 0.65f, 0.44f, 1.00f); // Parchment Golden Wood Trim #C7A670

        // Tab Colors (Parchment Wood)
        private static readonly Color TabActiveBg         = new Color(0.86f, 0.72f, 0.48f, 1.00f); // Bright Warm Birch Parchment #DDB87A
        private static readonly Color TabActiveText       = new Color(0.18f, 0.10f, 0.05f, 1.00f); // Deep Carved Wood Font #2E1A0D
        private static readonly Color TabInactiveBg       = new Color(0.20f, 0.12f, 0.06f, 0.96f); // Dark Inactive Wood #331F0F
        private static readonly Color TabInactiveText     = new Color(0.82f, 0.72f, 0.58f, 1.00f); // Parchment Beige #D1B894

        // Row Planks (Alternating Wooden Plank Strips)
        private static readonly Color WoodPlankEven       = new Color(0.30f, 0.18f, 0.11f, 0.95f); // Plank A #4D2E1C
        private static readonly Color WoodPlankOdd        = new Color(0.34f, 0.21f, 0.12f, 0.95f); // Plank B #57361F
        private static readonly Color WoodRowBorder       = new Color(0.20f, 0.11f, 0.06f, 0.90f); // Plank Gap Seam #331C0F

        // Text Colors
        private static readonly Color TextWhite           = new Color(1.00f, 1.00f, 1.00f, 1.00f); // Pure Crisp White
        private static readonly Color TextParchmentLight  = new Color(0.95f, 0.90f, 0.80f, 1.00f); // Warm Ivory / Bone #F2E6CC
        private static readonly Color TextParchmentWarm   = new Color(0.86f, 0.77f, 0.62f, 1.00f); // Warm Birch #DBC49E
        private static readonly Color TextGoldHeading     = new Color(0.96f, 0.78f, 0.38f, 1.00f); // Gold Stencil #F5C761
        private static readonly Color TextMuted           = new Color(0.68f, 0.58f, 0.45f, 1.00f); // Muted Wood #AD9473

        // Checkboxes & Buttons
        private static readonly Color CheckboxWoodBg      = new Color(0.18f, 0.10f, 0.05f, 0.98f); // Recessed Box #2E1A0D
        private static readonly Color CheckmarkGold       = new Color(0.92f, 0.78f, 0.52f, 1.00f); // Raft Golden Wood Check #EBC785
        private static readonly Color WoodButtonNormal    = new Color(0.38f, 0.23f, 0.14f, 0.96f); // Wood Plank Button #613B24
        private static readonly Color WoodButtonHover     = new Color(0.48f, 0.30f, 0.18f, 1.00f); // Lighter Wood Hover #7A4D2E
        private static readonly Color WoodButtonCrimson   = new Color(0.75f, 0.18f, 0.15f, 0.98f); // Warm Crimson Accent
        private static readonly Color ActionTileBg        = new Color(0.24f, 0.14f, 0.07f, 0.96f); // Deep Carved Timber Action Tile
        private static readonly Color ActionTileHover     = new Color(0.38f, 0.24f, 0.13f, 1.00f); // Highlighted Wood Plank
        private static readonly Color ActionTileBorder    = new Color(0.72f, 0.56f, 0.32f, 0.70f); // Parchment Gold Border Trim
        // ============================================================================
        // [END] RAFT NATIVE WOODEN PALETTE
        // ============================================================================
        #endregion

        // Preset Profile Controls & Tooltip
        private static readonly string[] ProfileKeys = { "VanillaPlus", "BalancedOP", "EasyMode", "Custom" };
        private static readonly string[] ProfileNames = { "🌿 Vanilla+", "⚖️ Balanced OP", "⚡ Easy Mode", "⚙️ Custom" };
        private static readonly string[] ProfileTooltips = {
            "🌿 <b>Vanilla+ Profile:</b> Authentic vanilla balance (1.0x weapon dmg, 40 stack, normal speeds, craft-from-storage & creature HP bars).",
            "⚖️ <b>Balanced OP Profile:</b> 1.5x weapon dmg, 100 stack, 1.5x crop/hook, 1.2x speeds, all QoL automations enabled.",
            "⚡ <b>Easy Mode Profile:</b> 2.5x weapon dmg, 200 stack, 2.0x crop/hook, 1.5x speeds for relaxed easy gameplay.",
            "⚙️ <b>Custom Profile:</b> User-defined fine-tuned configuration."
        };
        private static readonly Color ProfileActiveColor = TabActiveBg;
        private static readonly Color ProfileInactiveColor = WoodButtonNormal;
        private Image[] _profileButtonImgs = new Image[4];
        private Text[] _profileButtonTexts = new Text[4];
        private Text _qolTooltipText;
        private int _toggleItemCounter = 0;

        // UI Scaling Constant (1.3x Proportional Scale)
        public const float MenuUiScale = 1.0f;

        // Update Banner
        private GameObject _updateBannerGO;
        private Text _updateBannerText;

        #region [START] LIFECYCLE & AWAKE INITIALIZATION
        // ============================================================================
        // [START] LIFECYCLE & AWAKE INITIALIZATION
        // ============================================================================
        private void Awake()
        {
            try
            {
                Instance = this;
                gameObject.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(gameObject);
                GetGameFont();
                BuildCanvasUI();
                Debug.Log("[Sailor's Companion] CanvasModUI initialized successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Sailor's Companion] Error in CanvasModUI.Awake: " + ex);
            }
        }

        private static LayoutElement EnsureLayout(GameObject go, float prefWidth, float prefHeight, bool flexibleWidth = true)
        {
            if (go == null) return null;
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (prefWidth > 0) le.preferredWidth = prefWidth;
            if (prefHeight > 0) le.preferredHeight = prefHeight;
            le.flexibleWidth = flexibleWidth ? 1f : 0f;
            return le;
        }

        private static void FillParent(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void EnsureEventSystem()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null)
            {
                var existing = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (existing != null)
                {
                    UnityEngine.EventSystems.EventSystem.current = existing;
                }
                else
                {
                    var esGO = new GameObject("SailorsCompanion_EventSystem");
                    esGO.hideFlags = HideFlags.HideAndDontSave;
                    es = esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    DontDestroyOnLoad(esGO);
                    UnityEngine.EventSystems.EventSystem.current = es;
                }
            }
        }

        public Font GetGameFont()
        {
            if (_gameFont != null) return _gameFont;

            try
            {
                _gameFont = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI Semibold", "Segoe UI", "Arial", "Tahoma" }, 24);
            }
            catch {}
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

            var fonts = Resources.FindObjectsOfTypeAll<Font>();
            foreach (var f in fonts)
            {
                if (f != null)
                {
                    _gameFont = f;
                    return _gameFont;
                }
            }

            return null;
        }

        public void BuildCanvasUI()
        {
            GetGameFont();

            if (_canvasGO == null)
            {
                _canvasGO = new GameObject("SailorsCompanion_Canvas");
                _canvasGO.hideFlags = HideFlags.HideAndDontSave;
                _canvasGO.layer = LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5;
                DontDestroyOnLoad(_canvasGO);

                _canvas = _canvasGO.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.overrideSorting = true;
                _canvas.sortingOrder = 32000;

                _scaler = _canvasGO.AddComponent<CanvasScaler>();
                _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _scaler.referenceResolution = new Vector2(1920, 1080);
                _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                _scaler.matchWidthOrHeight = 0.5f;
                _scaler.dynamicPixelsPerUnit = MenuUiScale;

                _raycaster = _canvasGO.AddComponent<GraphicRaycaster>();
            }

            if (_modWindowGO == null)
            {
                BuildModWindow();
                _modWindowGO.SetActive(false);
            }

            Debug.Log("[Sailor's Companion] BuildCanvasUI successfully built persistent canvas & mod window!");
        }
        // ============================================================================
        // [END] LIFECYCLE & AWAKE INITIALIZATION
        // ============================================================================
        #endregion

        #region [START] MOD WINDOW FRAME & TABS CONTROLLER
        // ============================================================================
        // [START] MOD WINDOW FRAME & TABS CONTROLLER
        // ============================================================================
        private void BuildModWindow()
        {
            _modWindowGO = new GameObject("Window_ModMenu");
            _modWindowGO.transform.SetParent(_canvasGO.transform, false);

            var winRt = _modWindowGO.AddComponent<RectTransform>();
            winRt.anchorMin = new Vector2(0.5f, 0.5f);
            winRt.anchorMax = new Vector2(0.5f, 0.5f);
            winRt.pivot = new Vector2(0.5f, 0.5f);
            winRt.anchoredPosition = Vector2.zero;
            winRt.sizeDelta = new Vector2(1280, 760);
            winRt.localScale = new Vector3(MenuUiScale, MenuUiScale, 1.0f);

            var winImg = _modWindowGO.AddComponent<Image>();
            winImg.color = WoodWindowBg;

            // Outer Timber Border (recreating Raft rustic wooden frame)
            var outerBorder = CreateBox(_modWindowGO.transform, "WoodFrameBorder", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, WoodWindowBorder);
            var obRt = outerBorder.GetComponent<RectTransform>();
            obRt.offsetMin = new Vector2(-4, -4);
            obRt.offsetMax = new Vector2(4, 4);
            outerBorder.transform.SetAsFirstSibling();

            // Title Bar
            var titleBar = CreateBox(_modWindowGO.transform, "TitleBar", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 52), WoodTitleBar);
            
            // Wooden Trim Line under title
            CreateBox(titleBar.transform, "TitleAccent", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(0, 3), WoodTrimAccent);

            var titleText = CreateText(titleBar.transform, "TitleText", $"⚓ <color=#F5C761><b>SAILOR'S COMPANION</b></color> <size=13><color=#C7A670>v{PluginInfo.PLUGIN_VERSION}</color></size> — <size=13><color=#E6CEAC>Quality of Life & Survival Utilities</color></size>", 18, FontStyle.Bold, TextParchmentLight, TextAnchor.MiddleLeft);
            titleText.rectTransform.offsetMin = new Vector2(18, 0);
            titleText.rectTransform.offsetMax = new Vector2(-420, 0);

            // Mode Switcher in title bar: [ 🟢 Survival Mode ] [ ⚡ Creative ]
            var btnSurvGO = CreateButton(titleBar.transform, "Btn_Mode_Survival", "🟢 Survival Mode", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-280, 0), new Vector2(128, 30), () => SetModMode("Survival"), new Color(0.14f, 0.50f, 0.25f, 0.95f), TextParchmentLight, 12);
            _btnModeSurvivalImg = btnSurvGO.GetComponent<Image>();
            _btnModeSurvivalTxt = btnSurvGO.GetComponentInChildren<Text>();

            var btnCreatGO = CreateButton(titleBar.transform, "Btn_Mode_Creative", "⚡ Creative", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-155, 0), new Vector2(115, 30), () => SetModMode("Creative"), WoodButtonNormal, TabInactiveText, 12);
            _btnModeCreativeImg = btnCreatGO.GetComponent<Image>();
            _btnModeCreativeTxt = btnCreatGO.GetComponentInChildren<Text>();

            // Discord button in title bar
            CreateButton(titleBar.transform, "Btn_Discord", "💬 Discord", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-55, 0), new Vector2(90, 30), () => Application.OpenURL("https://discord.gg/B4EMrR5Vrf"), WoodButtonNormal, TextParchmentLight, 12);

            // Close button in title bar: authentic Raft wooden [X] button
            CreateButton(titleBar.transform, "Btn_Close", "✕", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-12, 0), new Vector2(30, 30), () => ToggleModWindow(), CheckboxWoodBg, TextParchmentLight, 16);

            // Tabs Row
            var tabRow = CreateBox(_modWindowGO.transform, "TabRow", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -58), new Vector2(-32, 42), Color.clear);
            var tabLayout = tabRow.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 8;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;

            string cheatsTabName = Plugin.IsSurvivalMode ? "🔒 CHEATS" : "⚡ CHEATS";
            string spawnerTabName = Plugin.IsSurvivalMode ? "🔒 ITEM SPAWNER" : "📦 ITEM SPAWNER";
            string[] tabNames = { "🎒 SURVIVAL QOL", cheatsTabName, "🧭 NAVIGATION", "🔬 R&D / BLUEPRINTS", spawnerTabName };
            for (int i = 0; i < tabNames.Length; i++)
            {
                int index = i;
                var tabBtn = CreateButton(tabRow.transform, $"TabBtn_{i}", tabNames[i], Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () => SelectTab(index), TabInactiveBg, TabInactiveText, 14);
                _tabButtonImages[i] = tabBtn.GetComponent<Image>();
                _tabButtonTexts[i] = tabBtn.GetComponentInChildren<Text>();
            }

            // Wooden Trim line separating tabs from content
            var tabTrim = CreateBox(_modWindowGO.transform, "TabTrimLine", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -104), new Vector2(-32, 3), WoodTrimAccent);

            // Tab Content Area (precisely bounded between Tabs and Footer)
            var contentArea = CreateBox(_modWindowGO.transform, "ContentArea", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var cRt = contentArea.GetComponent<RectTransform>();
            cRt.offsetMin = new Vector2(20, 42);
            cRt.offsetMax = new Vector2(-20, -114);
            _contentAreaTransform = contentArea.transform;

            UpdateModeButtonsVisuals();

            // Build individual tab pages
            _tabPages[0] = BuildSurvivalQoLTab(contentArea.transform);
            _tabPages[1] = BuildCheatsTab(contentArea.transform);
            _tabPages[2] = BuildNavTab(contentArea.transform);
            _tabPages[3] = BuildResearchTab(contentArea.transform);
            _tabPages[4] = BuildSpawnerTab(contentArea.transform);

            // Fixed Hotkeys Footer Bar
            var footerBar = CreateBox(_modWindowGO.transform, "FooterBar", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(0, 36), WoodTitleBar);
            CreateBox(footerBar.transform, "FooterAccent", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 2), WoodTrimAccent);
            CreateText(footerBar.transform, "FooterText", "<color=#C7A670>Hotkeys:</color> <color=#F5C761>[F5]</color> Menu  |  <color=#F5C761>[F6]</color> HUD  |  <color=#F5C761>[F4]</color> Sails  |  <color=#F5C761>[F3]</color> Engines  |  <color=#F5C761>[F7]</color> Magnet  |  <color=#F5C761>[F10]</color> Scan  |  <color=#F5C761>[F8]</color> Recall  |  <color=#F5C761>[ESC]</color> Close", 12, FontStyle.Bold, TextParchmentLight, TextAnchor.MiddleCenter);

            // Update Banner (shown when a newer version is available online)
            _updateBannerGO = CreateBox(_modWindowGO.transform, "UpdateBanner", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(-36, 40), WoodButtonCrimson);
            var bannerLayout = _updateBannerGO.AddComponent<HorizontalLayoutGroup>();
            bannerLayout.spacing = 10;
            bannerLayout.padding = new RectOffset(16, 12, 4, 4);
            bannerLayout.childForceExpandHeight = true;
            bannerLayout.childForceExpandWidth = false;

            _updateBannerText = CreateText(_updateBannerGO.transform, "UpdateTxt", "✨ <b>New Update Available!</b>", 14, FontStyle.Bold, TextParchmentLight, TextAnchor.MiddleLeft);
            EnsureLayout(_updateBannerText.gameObject, -1, 32, true);

            CreateButton(_updateBannerGO.transform, "Btn_UpdateDownload", "⬇️ Download Update", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(165, 30), () =>
            {
                Application.OpenURL(UpdateChecker.DownloadUrl);
            }, CheckboxWoodBg, TextParchmentLight, 13);

            CreateButton(_updateBannerGO.transform, "Btn_UpdateDismiss", "✕", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(30, 30), () =>
            {
                UpdateChecker.Dismissed = true;
                _updateBannerGO?.SetActive(false);
            }, CheckboxWoodBg, TextParchmentLight, 15);

            _updateBannerGO.SetActive(false);

            SelectTab(0);
        }

        private void SetModMode(string newMode)
        {
            if (Plugin.ModGameMode != null && Plugin.ModGameMode.Value == newMode) return;
            if (Plugin.ModGameMode != null) Plugin.ModGameMode.Value = newMode;
            UpdateModeButtonsVisuals();

            // Update Tab Titles
            if (_tabButtonTexts[1] != null)
                _tabButtonTexts[1].text = Plugin.IsSurvivalMode ? "🔒 CHEATS" : "⚡ CHEATS";
            if (_tabButtonTexts[4] != null)
                _tabButtonTexts[4].text = Plugin.IsSurvivalMode ? "🔒 ITEM SPAWNER" : "📦 ITEM SPAWNER";

            // Rebuild affected tab pages
            if (_contentAreaTransform != null)
            {
                if (_tabPages[0] != null) Destroy(_tabPages[0]);
                if (_tabPages[1] != null) Destroy(_tabPages[1]);
                if (_tabPages[3] != null) Destroy(_tabPages[3]);
                if (_tabPages[4] != null) Destroy(_tabPages[4]);

                _tabPages[0] = BuildSurvivalQoLTab(_contentAreaTransform);
                _tabPages[1] = BuildCheatsTab(_contentAreaTransform);
                _tabPages[3] = BuildResearchTab(_contentAreaTransform);
                _tabPages[4] = BuildSpawnerTab(_contentAreaTransform);

                SelectTab(_activeTab);
            }

            TeleportManager.SetNotification(Plugin.IsCreativeMode
                ? "⚡ Creative Mode Active: Unrestricted cheats & spawner unlocked!"
                : "🟢 Survival Mode Active: Balanced QoL active, cheats & spawner locked.");
        }

        private void UpdateModeButtonsVisuals()
        {
            bool isCreative = Plugin.IsCreativeMode;
            Color survColor = !isCreative ? new Color(0.14f, 0.50f, 0.25f, 0.95f) : WoodButtonNormal;
            Color creatColor = isCreative ? WoodButtonCrimson : WoodButtonNormal;

            if (_btnModeSurvivalImg != null)
            {
                _btnModeSurvivalImg.color = survColor;
                var btn = _btnModeSurvivalImg.GetComponent<Button>();
                if (btn != null)
                {
                    var cb = btn.colors;
                    cb.normalColor = survColor;
                    cb.selectedColor = survColor;
                    btn.colors = cb;
                }
            }
            if (_btnModeCreativeImg != null)
            {
                _btnModeCreativeImg.color = creatColor;
                var btn = _btnModeCreativeImg.GetComponent<Button>();
                if (btn != null)
                {
                    var cb = btn.colors;
                    cb.normalColor = creatColor;
                    cb.selectedColor = creatColor;
                    btn.colors = cb;
                }
            }

            if (_btnModeSurvivalTxt != null)
                _btnModeSurvivalTxt.color = !isCreative ? TextParchmentLight : TextMuted;
            if (_btnModeCreativeTxt != null)
                _btnModeCreativeTxt.color = isCreative ? TextParchmentLight : TextMuted;
        }

        private void ApplyProfile(string profileName)
        {
            if (string.IsNullOrEmpty(profileName)) return;

            if (Plugin.ActiveProfile != null)
                Plugin.ActiveProfile.Value = profileName;

            string tooltipText = "💡 <b>Hint:</b> Choose a preset profile above or toggle individual survival options.";

            if (profileName == "VanillaPlus")
            {
                SetProfileSettings(
                    stackSize: 40,
                    weaponDamage: 1.0f,
                    enableGrowthBoost: false,
                    growthMultiplier: 1.0f,
                    hookSpeed: 1.0f,
                    swimSpeed: 1.0f,
                    sprintSpeed: 1.0f,
                    autoWater: false,
                    autoNets: false,
                    craftFromStorage: true,
                    antiShark: false,
                    infiniteDurability: false,
                    animalHealthBars: true
                );
                tooltipText = "🌿 <b>Vanilla+ Profile:</b> Authentic vanilla balance (1.0x weapon dmg, 40 stack, normal speeds, craft-from-storage & creature HP bars).";
                TeleportManager.SetNotification("🌿 Activated 'Vanilla+' Preset Profile");
            }
            else if (profileName == "BalancedOP" || profileName == "CozyFarming")
            {
                SetProfileSettings(
                    stackSize: 100,
                    weaponDamage: 1.5f,
                    enableGrowthBoost: true,
                    growthMultiplier: 1.5f,
                    hookSpeed: 1.5f,
                    swimSpeed: 1.2f,
                    sprintSpeed: 1.2f,
                    autoWater: true,
                    autoNets: true,
                    craftFromStorage: true,
                    antiShark: true,
                    infiniteDurability: true,
                    animalHealthBars: true
                );
                tooltipText = "⚖️ <b>Balanced OP Profile:</b> 1.5x weapon dmg, 100 stack, 1.5x crop/hook, 1.2x speeds, all QoL automations enabled.";
                TeleportManager.SetNotification("⚖️ Activated 'Balanced OP' Preset Profile");
            }
            else if (profileName == "EasyMode" || profileName == "MasterBuilder")
            {
                SetProfileSettings(
                    stackSize: 200,
                    weaponDamage: 2.5f,
                    enableGrowthBoost: true,
                    growthMultiplier: 2.0f,
                    hookSpeed: 2.0f,
                    swimSpeed: 1.5f,
                    sprintSpeed: 1.5f,
                    autoWater: true,
                    autoNets: true,
                    craftFromStorage: true,
                    antiShark: true,
                    infiniteDurability: true,
                    animalHealthBars: true
                );
                tooltipText = "⚡ <b>Easy Mode Profile:</b> 2.5x weapon dmg, 200 stack, 2.0x crop/hook, 1.5x speeds for relaxed easy gameplay.";
                TeleportManager.SetNotification("⚡ Activated 'Easy Mode' Preset Profile");
            }

            try
            {
                Plugin.Instance?.Config?.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Sailor's Companion] Error saving profile configuration: " + ex.Message);
            }

            // Rebuild Tab 0 so all UI controls visually reflect the new profile values
            if (_contentAreaTransform != null)
            {
                if (_tabPages[0] != null)
                {
                    Destroy(_tabPages[0]);
                }
                _tabPages[0] = BuildSurvivalQoLTab(_contentAreaTransform, tooltipText);
                SelectTab(_activeTab);
            }

            UpdateProfileButtonsVisuals();
        }

        private static void SetProfileSettings(
            int stackSize,
            float weaponDamage,
            bool enableGrowthBoost,
            float growthMultiplier,
            float hookSpeed,
            float swimSpeed,
            float sprintSpeed,
            bool autoWater,
            bool autoNets,
            bool craftFromStorage,
            bool antiShark,
            bool infiniteDurability,
            bool animalHealthBars)
        {
            if (Plugin.CustomStackSize != null) Plugin.CustomStackSize.Value = stackSize;
            if (Plugin.WeaponDamageMultiplier != null) Plugin.WeaponDamageMultiplier.Value = weaponDamage;
            if (Plugin.EnableCropGrowthBoost != null) Plugin.EnableCropGrowthBoost.Value = enableGrowthBoost;
            if (Plugin.CropGrowthMultiplier != null) Plugin.CropGrowthMultiplier.Value = growthMultiplier;
            if (Plugin.HookPullSpeedMultiplier != null) Plugin.HookPullSpeedMultiplier.Value = hookSpeed;
            if (Plugin.SwimSpeedMultiplier != null) Plugin.SwimSpeedMultiplier.Value = swimSpeed;
            if (Plugin.SprintSpeedMultiplier != null) Plugin.SprintSpeedMultiplier.Value = sprintSpeed;
            if (Plugin.AutoWaterCrops != null) Plugin.AutoWaterCrops.Value = autoWater;
            if (Plugin.AutoEmptyCollectionNets != null) Plugin.AutoEmptyCollectionNets.Value = autoNets;
            if (Plugin.CraftFromStorage != null) Plugin.CraftFromStorage.Value = craftFromStorage;
            if (Plugin.AntiSharkRaftDamage != null) Plugin.AntiSharkRaftDamage.Value = antiShark;
            if (Plugin.InfiniteDurability != null) Plugin.InfiniteDurability.Value = infiniteDurability;
            if (Plugin.ShowAnimalHealthBars != null) Plugin.ShowAnimalHealthBars.Value = animalHealthBars;
        }

        private void UpdateProfileButtonsVisuals()
        {
            string active = Plugin.ActiveProfile != null ? Plugin.ActiveProfile.Value : "Custom";

            for (int i = 0; i < _profileButtonImgs.Length; i++)
            {
                if (_profileButtonImgs[i] == null) continue;
                bool isSel = (ProfileKeys[i] == active
                    || (ProfileKeys[i] == "BalancedOP" && active == "CozyFarming")
                    || (ProfileKeys[i] == "EasyMode" && active == "MasterBuilder"));

                Color targetBg = isSel ? ProfileActiveColor : ProfileInactiveColor;
                _profileButtonImgs[i].color = targetBg;
                var btn = _profileButtonImgs[i].GetComponent<Button>();
                if (btn != null)
                {
                    var cb = btn.colors;
                    cb.normalColor = targetBg;
                    cb.highlightedColor = isSel ? targetBg : WoodButtonHover;
                    cb.pressedColor = WoodWindowBorder;
                    cb.selectedColor = targetBg;
                    btn.colors = cb;
                }

                if (_profileButtonTexts[i] != null)
                {
                    _profileButtonTexts[i].color = isSel ? TabActiveText : TextParchmentLight;
                    _profileButtonTexts[i].fontStyle = isSel ? FontStyle.Bold : FontStyle.Normal;
                }
            }
        }

        private void MarkProfileCustom()
        {
            if (Plugin.ActiveProfile != null && Plugin.ActiveProfile.Value != "Custom")
            {
                Plugin.ActiveProfile.Value = "Custom";
                UpdateProfileButtonsVisuals();
            }
        }

        public void SetQoLTooltip(string text)
        {
            if (_qolTooltipText != null)
            {
                _qolTooltipText.text = text;
            }
        }

        private GameObject CreateCategoryHeader(Transform parent, string title, float height = 28f)
        {
            var headerGO = CreateBox(parent, "Header_" + title, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, height), WoodTitleBar);
            EnsureLayout(headerGO, -1, height);
            
            // Subtle golden wood trim bottom edge
            CreateBox(headerGO.transform, "Trim", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 2), WoodTrimAccent);

            var txt = CreateText(headerGO.transform, "Txt", title, 14, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleLeft);
            txt.rectTransform.offsetMin = new Vector2(12, 0);
            return headerGO;
        }

        private GameObject CreateLockCard(Transform parent, string title, string description, Action onUnlock)
        {
            var page = CreateBox(parent, "Page_Locked", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var pageLayout = page.AddComponent<VerticalLayoutGroup>();
            pageLayout.padding = new RectOffset(40, 40, 30, 30);
            pageLayout.childAlignment = TextAnchor.MiddleCenter;
            pageLayout.childForceExpandWidth = false;
            pageLayout.childForceExpandHeight = false;

            var card = CreateBox(page.transform, "LockPlaque", Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920, 430), WoodPlankEven);
            EnsureLayout(card, 920, 430, false);

            // Double border: Dark Timber Frame + Gold Trim
            var cardBorder = CreateBox(card.transform, "CardBorder", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, WoodWindowBorder);
            var cbRt = cardBorder.GetComponent<RectTransform>();
            cbRt.offsetMin = new Vector2(-4, -4);
            cbRt.offsetMax = new Vector2(4, 4);
            var cbLe = cardBorder.AddComponent<LayoutElement>();
            cbLe.ignoreLayout = true;
            cardBorder.transform.SetAsFirstSibling();

            var goldTrim = CreateBox(card.transform, "GoldTrim", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var gtOutline = goldTrim.AddComponent<Outline>();
            gtOutline.effectColor = WoodTrimAccent;
            gtOutline.effectDistance = new Vector2(2, -2);
            var gtLe = goldTrim.AddComponent<LayoutElement>();
            gtLe.ignoreLayout = true;

            var cardLayout = card.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(36, 36, 26, 26);
            cardLayout.spacing = 14;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;

            // Plaque Header Bar
            var plaqueHead = CreateBox(card.transform, "PlaqueHead", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 42), WoodTitleBar);
            EnsureLayout(plaqueHead, -1, 42);
            CreateBox(plaqueHead.transform, "TopTrim", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 2), WoodTrimAccent);
            CreateBox(plaqueHead.transform, "BotTrim", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 2), WoodTrimAccent);
            CreateText(plaqueHead.transform, "HeadTxt", "🔒  <b>SURVIVAL MODE RESTRICTION</b>  🔒", 16, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleCenter);

            var titleTxt = CreateText(card.transform, "LockTitle", $"<size=22><color=#FFFFFF><b>{title}</b></color></size>\n<size=14><color=#EBB861>Temporarily Disabled to Preserve Survival Immersion</color></size>", 18, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleCenter);
            EnsureLayout(titleTxt.gameObject, -1, 56);

            var descBox = CreateBox(card.transform, "DescBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 110), new Color(0.14f, 0.08f, 0.04f, 0.95f));
            EnsureLayout(descBox, -1, 110);
            var dbOutline = descBox.AddComponent<Outline>();
            dbOutline.effectColor = WoodRowBorder;
            dbOutline.effectDistance = new Vector2(1, -1);
            var descTxt = CreateText(descBox.transform, "LockDesc", description, 15, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleCenter);
            descTxt.lineSpacing = 1.35f;
            FillParent(descTxt.gameObject);

            // Dual Action Buttons Row
            var btnRow = CreateBox(card.transform, "BtnRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 48), Color.clear);
            EnsureLayout(btnRow, -1, 48);
            var btnLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 16;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = true;

            var unlockBtn = CreateButton(btnRow.transform, "Btn_UnlockCreative", "⚡ Switch to Creative Mode to Unlock", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, onUnlock, WoodButtonCrimson, TextWhite, 15);
            var uOutline = unlockBtn.AddComponent<Outline>();
            uOutline.effectColor = WoodTrimAccent;
            uOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var backBtn = CreateButton(btnRow.transform, "Btn_BackToQoL", "🎒 Return to Survival QoL Settings", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () => SelectTab(0), WoodButtonNormal, TextParchmentLight, 15);
            var bOutline = backBtn.AddComponent<Outline>();
            bOutline.effectColor = WoodTrimAccent * 0.7f;
            bOutline.effectDistance = new Vector2(1.5f, -1.5f);

            return page;
        }
        // ============================================================================
        // [END] MOD WINDOW FRAME & TABS CONTROLLER
        // ============================================================================
        #endregion


        #region [START] TAB 0: SURVIVAL QOL
        // ============================================================================
        // [START] TAB 0: SURVIVAL QUALITY OF LIFE (Preset Profiles, Categorized Groups & Clamped Balances)
        // ============================================================================
        private GameObject BuildSurvivalQoLTab(Transform parent, string initialTooltip = null)
        {
            var page = CreateBox(parent, "Page_SurvivalQoL", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6;
            layout.padding = new RectOffset(6, 6, 4, 4);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // 0. Preset Profiles Selector Row (Height: 36)
            var profileRow = CreateBox(page.transform, "ProfileSelectorRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 36), Color.clear);
            EnsureLayout(profileRow, -1, 36);
            var profLayout = profileRow.AddComponent<HorizontalLayoutGroup>();
            profLayout.spacing = 8;
            profLayout.childForceExpandWidth = true;
            profLayout.childForceExpandHeight = true;

            for (int i = 0; i < 4; i++)
            {
                int pIdx = i;
                var btn = CreateButton(profileRow.transform, $"Btn_Profile_{i}", ProfileNames[i], Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
                {
                    if (ProfileKeys[pIdx] == "Custom")
                    {
                        if (Plugin.ActiveProfile != null) Plugin.ActiveProfile.Value = "Custom";
                        UpdateProfileButtonsVisuals();
                        SetQoLTooltip(ProfileTooltips[pIdx]);
                    }
                    else
                    {
                        ApplyProfile(ProfileKeys[pIdx]);
                    }
                }, ProfileInactiveColor, TextParchmentLight, 15);
                _profileButtonImgs[i] = btn.GetComponent<Image>();
                _profileButtonTexts[i] = btn.GetComponentInChildren<Text>();
            }
            UpdateProfileButtonsVisuals();

            // 1. Quick Action Bar: 4 Primary Utility Action Tiles with Hotkey Badges (Height: 40)
            var actionRow = CreateBox(page.transform, "QuickActionBar", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 40), Color.clear);
            EnsureLayout(actionRow, -1, 40);
            var actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 8;
            actionLayout.childForceExpandWidth = true;

            CreateActionTile(actionRow.transform, "Btn_QuickStack", "📦 Quick Stack", "[STACK]", () =>
            {
                ChestSorter.QuickStackToNearbyChests();
                SetQoLTooltip("📦 <b>Quick Stack:</b> Deposited backpack items into matching nearby chests.");
            }, 40f, 76f);

            CreateActionTile(actionRow.transform, "Btn_EmptyNets", "🕸️ Empty Nets", "[SWEEP]", () =>
            {
                NetsHelper.EmptyAllNets(silent: false);
                SetQoLTooltip("🕸️ <b>Empty Nets:</b> Scooped all trapped flotsam from collection nets into your inventory.");
            }, 40f, 76f);

            CreateActionTile(actionRow.transform, "Btn_WaterPlots", "🌱 Water Crops", "[AUTO]", () =>
            {
                FarmingHelper.WaterAllPlots(silent: false);
                SetQoLTooltip("🌱 <b>Water Plots:</b> Hydrated all crop plots, grass plots, and tree planters.");
            }, 40f, 76f);

            CreateActionTile(actionRow.transform, "Btn_Magnet", "🧲 Ocean Magnet", "[F7] KEY", () =>
            {
                MagneticCollector.ToggleMagnet();
                SetQoLTooltip("🧲 <b>Ocean Magnet:</b> Smoothly pulls floating flotsam and debris towards your raft.");
            }, out _qolMagnetBtnText, 40f, 78f);

            // 2. Main Two-Column Content Area (Height: ~450)
            var twoColGO = CreateBox(page.transform, "TwoColumnsArea", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 450), Color.clear);
            EnsureLayout(twoColGO, -1, 450);
            var twoColLayout = twoColGO.AddComponent<HorizontalLayoutGroup>();
            twoColLayout.spacing = 14;
            twoColLayout.childForceExpandWidth = true;
            twoColLayout.childForceExpandHeight = true;

            // === LEFT COLUMN ===
            var leftCol = CreateBox(twoColGO.transform, "LeftColumn", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Color.clear);
            var leftLayout = leftCol.AddComponent<VerticalLayoutGroup>();
            leftLayout.spacing = 4;
            leftLayout.childForceExpandWidth = true;
            leftLayout.childForceExpandHeight = false;

            // CATEGORY 1: INVENTORY & STORAGE AUTOMATION
            CreateCategoryHeader(leftCol.transform, "📦 INVENTORY & STORAGE AUTOMATION", 28f);

            CreateToggleItem(leftCol.transform, "🛠️ Craft from Storage (Auto-pulls within 22m)", Plugin.CraftFromStorage?.Value ?? true, v =>
            {
                if (Plugin.CraftFromStorage != null) Plugin.CraftFromStorage.Value = v;
                MarkProfileCustom();
                TeleportManager.SetNotification(v ? "🛠️ Craft from Storage: ENABLED" : "🛠️ Craft from Storage: DISABLED");
            }, 38f, 14, "🛠️ <b>Craft from Storage:</b> Automatically pulls needed ingredients from nearby storage containers when crafting.");

            CreateToggleItem(leftCol.transform, "🕸️ Auto-Empty Nets (Auto-gathers trapped flotsam)", Plugin.AutoEmptyCollectionNets?.Value ?? false, v =>
            {
                if (Plugin.AutoEmptyCollectionNets != null) Plugin.AutoEmptyCollectionNets.Value = v;
                MarkProfileCustom();
                TeleportManager.SetNotification(v ? "🕸️ Auto-Empty Nets: ENABLED" : "🕸️ Auto-Empty Nets: DISABLED");
            }, 38f, 14, "🕸️ <b>Auto-Empty Nets:</b> Periodically sweeps collection nets so they never get clogged.");

            // CATEGORY 2: ISLAND & REEF HARVESTING
            CreateCategoryHeader(leftCol.transform, "🏝️ ISLAND & REEF HARVESTING", 28f);

            CreateToggleItem(leftCol.transform, "🏝️ Island Hand Pickup (Collect flowers/fruits barehanded)", Plugin.IslandHandPickup?.Value ?? true, v =>
            {
                if (Plugin.IslandHandPickup != null) Plugin.IslandHandPickup.Value = v;
                MarkProfileCustom();
                TeleportManager.SetNotification(v ? "🏝️ Island Hand Pickup: ENABLED" : "🏝️ Island Hand Pickup: DISABLED");
            }, 38f, 14, "🏝️ <b>Island Hand Pickup:</b> Pick up flowers, fruits, and surface items on islands without needing a hook.");

            CreateToggleItem(leftCol.transform, "⚡ Fast Reef Mining (3.5x Hook Speed + Shark Ward)", Plugin.ReefFastHarvest?.Value ?? true, v =>
            {
                if (Plugin.ReefFastHarvest != null) Plugin.ReefFastHarvest.Value = v;
                MarkProfileCustom();
                TeleportManager.SetNotification(v ? "⚡ Fast Reef Mining: ENABLED (3.5x Speed + Shark Ward)" : "⚡ Fast Reef Mining: DISABLED");
            }, 38f, 14, "⚡ <b>Fast Reef Mining:</b> Mines underwater Sand, Clay, Scrap, and Ores 3.5x faster (~0.7s) with your Hook, and temporarily wards off Bruce the shark while mining.");

            // CATEGORY 3: FARMING & SUSTENANCE
            bool hasFarmersCompanion = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.konduri.farmerscompanion");
            CreateCategoryHeader(leftCol.transform, hasFarmersCompanion ? "🌱 FARMING (MANAGED BY FARMER'S COMPANION [F1])" : "🌱 FARMING & SUSTENANCE", 28f);

            if (hasFarmersCompanion)
            {
                CreateText(leftCol.transform, "FC_Notice", "🌾 <color=#66FF66><b>Farmer's Companion Active:</b></color> Auto-water, smart usage, growth boosts, and livestock collection are actively handled by Farmer's Companion. Press <b>[F1]</b> to open menu.", 13, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            }
            else
            {
                CreateToggleItem(leftCol.transform, "🌱 Auto-Water Crops (Never let crop plots dry out)", Plugin.AutoWaterCrops?.Value ?? false, v =>
                {
                    if (Plugin.AutoWaterCrops != null) Plugin.AutoWaterCrops.Value = v;
                    MarkProfileCustom();
                    TeleportManager.SetNotification(v ? "🌱 Auto-Watering: ENABLED" : "🌱 Auto-Watering: DISABLED");
                }, 38f, 14, "🌱 <b>Auto-Water:</b> Continuously maintains full hydration on crop plots and livestock grass.");

                CreateToggleItem(leftCol.transform, "🌾 Crop & Tree Growth Boost (Accelerate growth cycles)", Plugin.EnableCropGrowthBoost?.Value ?? false, v =>
                {
                    if (Plugin.EnableCropGrowthBoost != null) Plugin.EnableCropGrowthBoost.Value = v;
                    MarkProfileCustom();
                    TeleportManager.SetNotification(v ? "🌾 Crop Growth Boost: ENABLED" : "🌾 Crop Growth Boost: DISABLED");
                }, 38f, 14, "🌾 <b>Crop Growth Boost:</b> Toggles custom growth multiplier for farming plots and tree planters.");
            }


            // === RIGHT COLUMN ===
            var rightCol = CreateBox(twoColGO.transform, "RightColumn", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Color.clear);
            var rightLayout = rightCol.AddComponent<VerticalLayoutGroup>();
            rightLayout.spacing = 4;
            rightLayout.childForceExpandWidth = true;
            rightLayout.childForceExpandHeight = false;

            // CATEGORY 4: RAFT & CREATURE DEFENSE
            CreateCategoryHeader(rightCol.transform, "🦈 RAFT & CREATURE DEFENSE", 28f);

            CreateToggleItem(rightCol.transform, "🐾 Animal & Enemy Health Bars (Floating HP & distance)", Plugin.ShowAnimalHealthBars?.Value ?? true, v =>
            {
                if (Plugin.ShowAnimalHealthBars != null) Plugin.ShowAnimalHealthBars.Value = v;
                MarkProfileCustom();
                TeleportManager.SetNotification(v ? "🐾 Animal Health Bars: ENABLED" : "🐾 Animal Health Bars: DISABLED");
            }, 38f, 14, "🐾 <b>Creature Health Bars:</b> Displays overhead health bars and distance meters on animals and predators.");

            CreateToggleItem(rightCol.transform, "🦈 Anti-Shark Raft Protection (Bruce won't attack raft)", Plugin.AntiSharkRaftDamage?.Value ?? false, v =>
            {
                if (Plugin.AntiSharkRaftDamage != null) Plugin.AntiSharkRaftDamage.Value = v;
                MarkProfileCustom();
                TeleportManager.SetNotification(v ? "🦈 Anti-Shark: ENABLED" : "🦈 Anti-Shark: DISABLED");
            }, 38f, 14, "🦈 <b>Anti-Shark Protection:</b> Bruce the shark will ignore raft foundations and focus only on players in water.");

            CreateToggleItem(rightCol.transform, "🔨 Infinite Tool Durability (Tools, weapons & armor never break)", Plugin.InfiniteDurability?.Value ?? false, v =>
            {
                if (Plugin.InfiniteDurability != null) Plugin.InfiniteDurability.Value = v;
                MarkProfileCustom();
                TeleportManager.SetNotification(v ? "🔨 Infinite Durability: ENABLED" : "🔨 Infinite Durability: DISABLED");
            }, 38f, 14, "🔨 <b>Infinite Durability:</b> Prevents hooks, weapons, tools, and armor from breaking from use.");

            // CATEGORY 5: BALANCED MULTIPLIERS & SPEEDS
            CreateCategoryHeader(rightCol.transform, "🏃 BALANCED MULTIPLIERS & SPEEDS", 28f);

            float maxGrowth = Plugin.IsCreativeMode ? 10.0f : 2.0f;
            float maxStack = Plugin.IsCreativeMode ? 999f : 200f;
            float maxWeapon = Plugin.IsCreativeMode ? 10.0f : 3.0f;
            float maxReel = Plugin.IsCreativeMode ? 5.0f : 2.0f;
            float maxSwim = Plugin.IsCreativeMode ? 4.0f : 1.5f;
            float maxSprint = Plugin.IsCreativeMode ? 3.0f : 1.5f;

            float curWeapon = Plugin.WeaponDamageMultiplier?.Value ?? 1.0f;
            float curGrowth = Plugin.CropGrowthMultiplier?.Value ?? 1.0f;
            float curStack = Plugin.CustomStackSize?.Value ?? 40;
            float curReel = Plugin.HookPullSpeedMultiplier?.Value ?? 1.0f;
            float curSwim = Plugin.SwimSpeedMultiplier?.Value ?? 1.0f;
            float curSprint = Plugin.SprintSpeedMultiplier?.Value ?? 1.0f;

            CreateDualStepperRow(rightCol.transform,
                "⚔️ Weapon Dmg", 1.0f, maxWeapon, 0.5f, curWeapon, "x", v =>
                {
                    if (Plugin.WeaponDamageMultiplier != null) Plugin.WeaponDamageMultiplier.Value = v;
                    MarkProfileCustom();
                }, "⚔️ <b>Weapon Damage:</b> Multiplies damage dealt by spears, arrows, and machete against creatures (1.0x–2.0x recommended).",
                "🌾 Crop Growth", 1.0f, maxGrowth, 0.5f, curGrowth, "x", v =>
                {
                    if (Plugin.CropGrowthMultiplier != null) Plugin.CropGrowthMultiplier.Value = v;
                    MarkProfileCustom();
                }, "🌾 <b>Crop Growth:</b> Multiplies crop and tree growth speed (1.0x–2.0x recommended).",
                38f);

            CreateDualStepperRow(rightCol.transform,
                "📦 Stack Limit", 20f, maxStack, 20f, curStack, "", v =>
                {
                    if (Plugin.CustomStackSize != null) Plugin.CustomStackSize.Value = Mathf.RoundToInt(v);
                    MarkProfileCustom();
                }, "📦 <b>Stack Limit:</b> Maximum item capacity per inventory slot (20-200 recommended).",
                "🎣 Hook Reel", 1.0f, maxReel, 0.5f, curReel, "x", v =>
                {
                    if (Plugin.HookPullSpeedMultiplier != null) Plugin.HookPullSpeedMultiplier.Value = v;
                    MarkProfileCustom();
                }, "🎣 <b>Reel Speed:</b> Accelerates pulling hooks from the water (1.0x–2.0x recommended).",
                38f);

            CreateDualStepperRow(rightCol.transform,
                "🏊 Swim Speed", 1.0f, maxSwim, 0.1f, curSwim, "x", v =>
                {
                    if (Plugin.SwimSpeedMultiplier != null) Plugin.SwimSpeedMultiplier.Value = v;
                    MarkProfileCustom();
                }, "🏊 <b>Swim Speed:</b> Enhances water mobility without glitching collisions (1.0x–1.5x recommended).",
                "🏃 Sprint Speed", 1.0f, maxSprint, 0.1f, curSprint, "x", v =>
                {
                    if (Plugin.SprintSpeedMultiplier != null) Plugin.SprintSpeedMultiplier.Value = v;
                    MarkProfileCustom();
                }, "🏃 <b>Sprint Speed:</b> Subtle movement speed increase across raft and land (1.0x–1.5x recommended).",
                38f);

            // CATEGORY 6: ADVANCED PRO HOTKEYS
            CreateCategoryHeader(rightCol.transform, "⌨️ ADVANCED PRO HOTKEYS", 28f);

            CreateToggleItem(rightCol.transform, "⌨️ Enable Quick Hotkeys ([F4] Sails, [F3] Engines, [F7] Magnet, [F10] Scan)", Plugin.EnableHotkeys?.Value ?? false, v =>
            {
                if (Plugin.EnableHotkeys != null) Plugin.EnableHotkeys.Value = v;
                TeleportManager.SetNotification(v ? "⌨️ Quick Hotkeys: ENABLED ([F4] Sails, [F3] Engines, [F7] Magnet, [F10] Scan)" : "⌨️ Quick Hotkeys: DISABLED (UI Buttons only)");
            }, 38f, 13, "⌨️ <b>Quick Hotkeys:</b> Enables direct gameplay keys for speed actions without opening menus ([F4] Sails, [F3] Engines, [F7] Magnet, [F10] Scanner).");

            // 3. Tooltip / Hint Box (Height: 32)
            var hintBox = CreateBox(page.transform, "QoLHintBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 32), WoodTitleBar);
            EnsureLayout(hintBox, -1, 32);
            CreateBox(hintBox.transform, "HintAccent", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 2), WoodTrimAccent);
            string defaultHint = string.IsNullOrEmpty(initialTooltip) ? "💡 <b>Hotkeys:</b> [F7] Magnet | [F8] Recall | [F9] Summon | [F10] Radar | [F4] Sails | [F3] Engines | [F] Fly" : initialTooltip;
            _qolTooltipText = CreateText(hintBox.transform, "HintText", defaultHint, 14, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleLeft);
            _qolTooltipText.rectTransform.offsetMin = new Vector2(12, 0);
            _qolTooltipText.rectTransform.offsetMax = new Vector2(-12, 0);

            return page;
        }
        // ============================================================================
        // [END] TAB 0: SURVIVAL QOL
        // ============================================================================
        #endregion

        #region [START] TAB 1: CHEATS & SANDBOX
        // ============================================================================
        // [START] TAB 1: CHEATS & SANDBOX (God Mode, Oxygen, Hunger/Thirst, Fly, Free Craft, Weather)
        // ============================================================================
        private GameObject BuildCheatsTab(Transform parent)
        {
            if (Plugin.IsSurvivalMode)
            {
                return CreateLockCard(parent, "Cheats & God Mode",
                    "Survival Mode preserves authentic game balance, hunger/thirst tension, and progression immersion.\n\nGod Mode, Infinite Oxygen, Fly / Noclip, Free Instant Crafting, and Weather controls are reserved for Creative Sandbox.\n\nSwitch to Creative / Sandbox Mode below to unlock all god powers immediately.",
                    () => SetModMode("Creative"));
            }

            var page = CreateBox(parent, "Page_Cheats", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var pageLayout = page.AddComponent<VerticalLayoutGroup>();
            pageLayout.spacing = 6;
            pageLayout.padding = new RectOffset(6, 6, 4, 4);
            pageLayout.childForceExpandWidth = true;
            pageLayout.childForceExpandHeight = false;

            // Two-Column Creative Sandbox Area (Height: 490)
            var twoColGO = CreateBox(page.transform, "CheatsTwoColumns", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 490), Color.clear);
            EnsureLayout(twoColGO, -1, 490);
            var twoColLayout = twoColGO.AddComponent<HorizontalLayoutGroup>();
            twoColLayout.spacing = 14;
            twoColLayout.childForceExpandWidth = true;
            twoColLayout.childForceExpandHeight = true;

            // === LEFT COLUMN: PLAYER GOD CHEATS & VITALS ===
            var leftCol = CreateBox(twoColGO.transform, "LeftCheatsCol", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Color.clear);
            var lLayout = leftCol.AddComponent<VerticalLayoutGroup>();
            lLayout.spacing = 6;
            lLayout.childForceExpandWidth = true;

            CreateCategoryHeader(leftCol.transform, "⚡ PLAYER GOD CHEATS & INVULNERABILITY", 28f);
            CreateToggleItem(leftCol.transform, "🛡️ God Mode (Invulnerable to damage & sharks)", Plugin.GodMode.Value, v => Plugin.GodMode.Value = v, 36f, 14);
            CreateToggleItem(leftCol.transform, "⚔️ 1-Hit Kill / Infinite Damage (Slay creatures in 1 hit)", Plugin.OneHitKill?.Value ?? false, v => { if (Plugin.OneHitKill != null) Plugin.OneHitKill.Value = v; }, 36f, 14);
            CreateToggleItem(leftCol.transform, "🤿 Infinite Oxygen (Dive freely without running out of air)", Plugin.InfiniteOxygen.Value, v => Plugin.InfiniteOxygen.Value = v, 36f, 14);
            CreateToggleItem(leftCol.transform, "🥩 Freeze Hunger & Thirst (Never starve or dehydrate)", Plugin.NoHungerThirst.Value, v => Plugin.NoHungerThirst.Value = v, 36f, 14);
            CreateToggleItem(leftCol.transform, "🕊️ Fly / Noclip Mode (Hotkey: [F] | Space/Shift fly)", Plugin.EnableFlyMode.Value, v => Plugin.EnableFlyMode.Value = v, 36f, 14);
            CreateToggleItem(leftCol.transform, "🛠️ Free Instant Crafting (Craft any recipe with 0 materials)", Plugin.FreeCrafting.Value, v => Plugin.FreeCrafting.Value = v, 36f, 14);

            CreateCategoryHeader(leftCol.transform, "💖 INSTANT VITALS RECOVERY", 28f);
            var vitalsBtn = CreateActionTile(leftCol.transform, "Btn_MaxVitals", "⚡ Replenish All Vitals (Health, O2, Food)", "[RESTORE]", () =>
            {
                var p = PlayerHelper.GetLocalPlayer();
                if (p?.Stats != null)
                {
                    p.Stats.stat_health?.SetToMaxValue();
                    p.Stats.stat_hunger?.Normal?.SetToMaxValue();
                    p.Stats.stat_thirst?.Normal?.SetToMaxValue();
                    p.Stats.stat_oxygen?.SetToMaxValue();
                    TeleportManager.SetNotification("⚡ Vitals fully replenished to 100%!");
                }
            }, 40f, 85f);
            EnsureLayout(vitalsBtn, -1, 40);

            // === RIGHT COLUMN: RAFT, TIME & WEATHER CONTROLS ===
            var rightCol = CreateBox(twoColGO.transform, "RightCheatsCol", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Color.clear);
            var rLayout = rightCol.AddComponent<VerticalLayoutGroup>();
            rLayout.spacing = 6;
            rLayout.childForceExpandWidth = true;

            CreateCategoryHeader(rightCol.transform, "⛵ RAFT TELEPORTATION & CONTROL", 28f);
            var teleRow = CreateBox(rightCol.transform, "TeleRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 38), Color.clear);
            EnsureLayout(teleRow, -1, 38);
            var teleLayout = teleRow.AddComponent<HorizontalLayoutGroup>();
            teleLayout.spacing = 8;
            teleLayout.childForceExpandWidth = true;
            CreateActionTile(teleRow.transform, "Btn_Recall", "⚡ Recall to Raft", "[F8] KEY", () => TeleportManager.TeleportPlayerToRaft(), 38f, 78f);
            CreateActionTile(teleRow.transform, "Btn_Summon", "⛵ Summon Raft", "[F9] KEY", () => TeleportManager.TeleportRaftToPlayer(), 38f, 78f);
            CreateActionTile(teleRow.transform, "Btn_Anchor", "⚓ Toggle Anchor", "[ANCHOR]", () => TeleportManager.ToggleRaftAnchor(), 38f, 80f);

            CreateCategoryHeader(rightCol.transform, "☀️ WORLD TIME CONTROLLER", 28f);
            var timeRow = CreateBox(rightCol.transform, "TimeRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 38), Color.clear);
            EnsureLayout(timeRow, -1, 38);
            var timeLayout = timeRow.AddComponent<HorizontalLayoutGroup>();
            timeLayout.spacing = 8;
            timeLayout.childForceExpandWidth = true;
            CreateActionTile(timeRow.transform, "Btn_Morning", "🌅 Morning", "[08:00]", () => SetTime(8f), 38f, 70f);
            CreateActionTile(timeRow.transform, "Btn_Noon", "☀️ Noon", "[12:00]", () => SetTime(12f), 38f, 70f);
            CreateActionTile(timeRow.transform, "Btn_Night", "🌙 Night", "[22:00]", () => SetTime(22f), 38f, 70f);

            CreateCategoryHeader(rightCol.transform, "🌧️ DYNAMIC WEATHER CONTROLLER", 28f);
            var weatherRow = CreateBox(rightCol.transform, "WeatherRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 38), Color.clear);
            EnsureLayout(weatherRow, -1, 38);
            var weatherLayout = weatherRow.AddComponent<HorizontalLayoutGroup>();
            weatherLayout.spacing = 8;
            weatherLayout.childForceExpandWidth = true;
            CreateActionTile(weatherRow.transform, "Btn_Sunny", "☀️ Sunny", "[CLEAR]", () => SetWeather(UniqueWeatherType.Default), 38f, 65f);
            CreateActionTile(weatherRow.transform, "Btn_Calm", "🌊 Calm", "[CALM]", () => SetWeather(UniqueWeatherType.Calm), 38f, 65f);
            CreateActionTile(weatherRow.transform, "Btn_Rain", "🌧️ Rain", "[RAIN]", () => SetWeather(UniqueWeatherType.Rain), 38f, 65f);
            CreateActionTile(weatherRow.transform, "Btn_Fog", "🌫️ Fog", "[FOG]", () => SetWeather(UniqueWeatherType.Fog), 38f, 65f);

            // Flight Instructions Box
            var flyBox = CreateBox(rightCol.transform, "FlyBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 100), WoodPlankEven);
            EnsureLayout(flyBox, -1, 100);
            var fbOutline = flyBox.AddComponent<Outline>();
            fbOutline.effectColor = WoodRowBorder;
            fbOutline.effectDistance = new Vector2(1, -1);
            var fbLayout = flyBox.AddComponent<VerticalLayoutGroup>();
            fbLayout.padding = new RectOffset(16, 16, 8, 8);
            fbLayout.spacing = 4;
            var fbTitle = CreateText(flyBox.transform, "T", "🕊️ <b>Free Flight & Noclip Controls [F]</b>", 14, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleLeft);
            EnsureLayout(fbTitle.gameObject, -1, 20);
            var fbDesc = CreateText(flyBox.transform, "D", "• <b>[F]</b>: Toggle Flight mode  |  <b>[WASD]</b>: Fly in any direction.\n• <b>[Space]</b>: Ascend  |  <b>[Left Shift]</b>: Descend  |  <b>[Left Ctrl]</b>: Turbo Speed boost.", 13, FontStyle.Normal, TextParchmentLight, TextAnchor.UpperLeft);
            fbDesc.lineSpacing = 1.25f;
            EnsureLayout(fbDesc.gameObject, -1, 60);

            return page;
        }
        // ============================================================================
        // [END] TAB 1: CHEATS & SANDBOX
        // ============================================================================
        #endregion

        #region [START] TAB 2: NAVIGATION HUD & SHARK RADAR
        // ============================================================================
        // [START] TAB 2: NAVIGATION HUD & SHARK RADAR
        // ============================================================================
        private GameObject BuildNavTab(Transform parent)
        {
            var page = CreateBox(parent, "Page_Nav", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateToggleItem(page.transform, "🧭 Show On-Screen HUD Overlay (Hotkey: [F6])", Plugin.EnableHUD.Value, v =>
            {
                Plugin.EnableHUD.Value = v;
            }, 40f, 15);

            // HUD Style Selection Row
            var styleRow = CreateBox(page.transform, "HUDStyleRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 40), WoodTitleBar);
            EnsureLayout(styleRow, -1, 40);
            var styleLayout = styleRow.AddComponent<HorizontalLayoutGroup>();
            styleLayout.spacing = 10;
            styleLayout.padding = new RectOffset(14, 14, 3, 3);
            styleLayout.childForceExpandHeight = true;

            var styleLabel = CreateText(styleRow.transform, "StyleLabel", "HUD Style:", 15, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleLeft);
            EnsureLayout(styleLabel.gameObject, 90, 34, false);

            _navStyleBtns.Clear();
            string[] styleNames = HUDOverlay.StyleNames;
            for (int s = 0; s < styleNames.Length; s++)
            {
                int styleIdx = s;
                bool isSelected = (Plugin.HUDStyle != null && Plugin.HUDStyle.Value == s);
                var sBtn = CreateButton(styleRow.transform, $"Btn_Style_{s}", styleNames[s], Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(120, 34), () =>
                {
                    HUDOverlay.SetStyle(styleIdx);
                    UpdateNavStyleButtonVisuals();
                }, isSelected ? TabActiveBg : WoodButtonNormal, isSelected ? TabActiveText : TextParchmentLight, 14);
                EnsureLayout(sBtn, 120, 34, false);
                _navStyleBtns.Add(sBtn);
            }

            // Quick Teleport & Island Scan Row: Action Tiles with Hotkey Badges (Height: 40)
            var navTeleRow = CreateBox(page.transform, "NavTeleRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 40), Color.clear);
            EnsureLayout(navTeleRow, -1, 40);
            var navTeleLayout = navTeleRow.AddComponent<HorizontalLayoutGroup>();
            navTeleLayout.spacing = 8;
            navTeleLayout.childForceExpandWidth = true;

            CreateActionTile(navTeleRow.transform, "Btn_NavTeleToRaft", "⚡ Recall to Raft", "[F8] KEY", () =>
            {
                TeleportManager.TeleportPlayerToRaft();
            }, out _navRecallBtnText, 40f, 78f);

            CreateActionTile(navTeleRow.transform, "Btn_NavSummonRaft", "⛵ Summon Raft", "[F9] KEY", () =>
            {
                TeleportManager.TeleportRaftToPlayer();
            }, 40f, 78f);

            CreateActionTile(navTeleRow.transform, "Btn_NavAnchor", "⚓ Toggle Anchor", "[ANCHOR]", () =>
            {
                TeleportManager.ToggleRaftAnchor();
            }, 40f, 80f);

            CreateActionTile(navTeleRow.transform, "Btn_NavScanIsland", "🔍 Island Radar", "[F10] KEY", () =>
            {
                ItemDetector.TriggerPulseScan();
            }, out _navScannerBtnText, 40f, 82f);

            // Raft Propulsion & Smart Boat Control Row: Action Tiles with Hotkey Badges (Height: 40)
            var boatRow = CreateBox(page.transform, "BoatControlRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 40), Color.clear);
            EnsureLayout(boatRow, -1, 40);
            var boatLayout = boatRow.AddComponent<HorizontalLayoutGroup>();
            boatLayout.spacing = 8;
            boatLayout.childForceExpandWidth = true;

            CreateActionTile(boatRow.transform, "Btn_ToggleSails", "⛵ Toggle All Sails", "[F4] KEY", () =>
            {
                BoatController.ToggleAllSails();
            }, 40f, 78f);

            CreateActionTile(boatRow.transform, "Btn_ToggleEngines", "⚙️ Toggle Engines", "[F3] KEY", () =>
            {
                BoatController.ToggleAllEngines();
            }, 40f, 78f);

            string initialModeName = GetSailModeDisplayName();
            CreateActionTile(boatRow.transform, "Btn_SailMode", initialModeName, "[CYCLE]", () =>
            {
                CycleSailMode();
            }, out _navSailModeBtnText, 40f, 78f);

            // Live Navigation Telemetry Console (Height: 185)
            var statusBox = CreateBox(page.transform, "StatusBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 185), WoodPlankEven);
            EnsureLayout(statusBox, -1, 185);
            var sbOutline = statusBox.AddComponent<Outline>();
            sbOutline.effectColor = WoodRowBorder;
            sbOutline.effectDistance = new Vector2(1, -1);
            var boxLayout = statusBox.AddComponent<VerticalLayoutGroup>();
            boxLayout.padding = new RectOffset(12, 12, 10, 10);
            boxLayout.spacing = 8;
            boxLayout.childForceExpandWidth = true;

            var title = CreateText(statusBox.transform, "NavTitle", "<b>🧭 <color=#F5C761>Live Nautical Telemetry</color> & Sensor Console</b>", 16, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleLeft);
            EnsureLayout(title.gameObject, -1, 24);

            // 4-Tile Instrument Row
            var gaugeRow = CreateBox(statusBox.transform, "GaugeRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 95), Color.clear);
            EnsureLayout(gaugeRow, -1, 95);
            var gLayout = gaugeRow.AddComponent<HorizontalLayoutGroup>();
            gLayout.spacing = 8;
            gLayout.childForceExpandWidth = true;
            gLayout.childForceExpandHeight = true;

            _teleHeadingText = BuildTelemetryTile(gaugeRow.transform, "Tile_Heading", "🧭 COMPASS HEADING", "---° (Standby)", "Awaiting World Load");
            _teleRaftText = BuildTelemetryTile(gaugeRow.transform, "Tile_Raft", "⛵ RAFT POSITION", "Standby", "Velocity: 0.0 kts");
            _teleSharkText = BuildTelemetryTile(gaugeRow.transform, "Tile_Shark", "🦈 BRUCE SONAR", "Sonar Clear", "Threat: Normal");
            _teleCoordsText = BuildTelemetryTile(gaugeRow.transform, "Tile_Coords", "📍 WORLD GPS", "X: 0.0  Y: 0.0  Z: 0.0", "Sea Level (Y=0.0)");

            // Notification / Quick Ticker Bar
            var notifRow = CreateBox(statusBox.transform, "NotifRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 32), CheckboxWoodBg);
            EnsureLayout(notifRow, -1, 32);
            var nrOutline = notifRow.AddComponent<Outline>();
            nrOutline.effectColor = WoodTrimAccent * 0.7f;
            nrOutline.effectDistance = new Vector2(1, -1);
            _teleNotifText = CreateText(notifRow.transform, "TickerTxt", "💡 <color=#E0D0B5>Hotkeys:</color> <color=#F5C761>[F5]</color> Menu  |  <color=#F5C761>[F6]</color> HUD  |  <color=#F5C761>[Shift+F6]</color> Style  |  <color=#F5C761>[F4]</color> Sails  |  <color=#F5C761>[F3]</color> Engines  |  <color=#F5C761>[F8]</color> Recall  |  <color=#F5C761>[F9]</color> Summon", 13, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleCenter);

            // Hidden fallback for any legacy code
            var dummyGO = new GameObject("NavStatusDummy");
            dummyGO.transform.SetParent(statusBox.transform, false);
            dummyGO.SetActive(false);
            _navStatusText = dummyGO.AddComponent<Text>();
            _navStatusText.font = GetGameFont();

            // Version & Update Check Row
            var verRow = CreateBox(page.transform, "VerRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 40), WoodTitleBar);
            EnsureLayout(verRow, -1, 40);
            var verLayout = verRow.AddComponent<HorizontalLayoutGroup>();
            verLayout.padding = new RectOffset(16, 16, 3, 3);
            verLayout.spacing = 12;
            verLayout.childForceExpandHeight = true;

            CreateText(verRow.transform, "VerLabel", $"⚓ Sailor's Companion <b>v{PluginInfo.PLUGIN_VERSION}</b>", 14, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleLeft);

            CreateButton(verRow.transform, "Btn_CheckUpdates", "🔄 Check for Updates", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(180, 34), () =>
            {
                UpdateChecker.Dismissed = false;
                UpdateChecker.Instance?.TriggerCheck();
                TeleportManager.SetNotification("Checking GitHub for mod updates...");
            }, WoodButtonNormal, TextParchmentLight, 14);

            return page;
        }

        private Text BuildTelemetryTile(Transform parent, string name, string header, string initialVal, string initialSub)
        {
            var tile = CreateBox(parent, name, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.14f, 0.08f, 0.04f, 0.95f));
            var outline = tile.AddComponent<Outline>();
            outline.effectColor = WoodTrimAccent * 0.75f;
            outline.effectDistance = new Vector2(1, -1);

            var layout = tile.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 3;
            layout.childForceExpandWidth = true;

            var head = CreateText(tile.transform, "H", header, 12, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleCenter);
            EnsureLayout(head.gameObject, -1, 16);

            var valTxt = CreateText(tile.transform, "V", $"<b><color=#FFFFFF>{initialVal}</color></b>\n<size=12><color=#DBC49E>{initialSub}</color></size>", 15, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleCenter);
            valTxt.lineSpacing = 1.2f;
            EnsureLayout(valTxt.gameObject, -1, 48);

            return valTxt;
        }
        // ============================================================================
        // [END] TAB 2: NAVIGATION HUD & SHARK RADAR
        // ============================================================================
        #endregion

        #region [START] TAB 3: RESEARCH & R&D BLUEPRINTS
        // ============================================================================
        // [START] TAB 3: RESEARCH & R&D BLUEPRINTS (Progressive Chapters & Creative Instant Learn)
        // ============================================================================
        private GameObject BuildResearchTab(Transform parent)
        {
            var page = CreateBox(parent, "Page_Research", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            if (Plugin.IsSurvivalMode)
            {
                var infoBox = CreateBox(page.transform, "InfoBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 80), WoodPlankEven);
                EnsureLayout(infoBox, -1, 80);
                var ibOutline = infoBox.AddComponent<Outline>();
                ibOutline.effectColor = WoodRowBorder;
                ibOutline.effectDistance = new Vector2(1, -1);
                var boxLayout = infoBox.AddComponent<VerticalLayoutGroup>();
                boxLayout.padding = new RectOffset(18, 18, 10, 10);
                boxLayout.spacing = 4;
                boxLayout.childForceExpandWidth = true;

                var title = CreateText(infoBox.transform, "Title", "🔬 <b><color=#F5C761>Progressive Story Research</color> & Chapter Blueprints</b>", 17, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleLeft);
                EnsureLayout(title.gameObject, -1, 24);

                var desc = CreateText(infoBox.transform, "Desc", "In Survival Mode, recipes and story discoveries are unlocked chapter-by-chapter to protect the rewarding story journey of Raft.", 14, FontStyle.Normal, TextParchmentLight, TextAnchor.UpperLeft);
                EnsureLayout(desc.gameObject, -1, 38);

                // Progressive Tech Cards
                var btnBase = CreateButton(page.transform, "Btn_BaseTech", "🔬 <b><color=#F5C761>1. Research Base Table Materials</color></b> <color=#F2E6CC>(Wood, Plastic, Metal, Scrap, Clay, Bricks, Goo)</color>", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 50), () =>
                {
                    ResearchBaseMaterials();
                }, WoodButtonNormal, TextParchmentLight, 14);
                var bOutline = btnBase.AddComponent<Outline>();
                bOutline.effectColor = WoodTrimAccent * 0.7f;
                bOutline.effectDistance = new Vector2(1, -1);
                EnsureLayout(btnBase, -1, 50);

                var btnCh1 = CreateButton(page.transform, "Btn_Chapter1", "📻 <b><color=#F5C761>2. Unlock Chapter 1 Blueprints</color></b> <color=#F2E6CC>(Radio Tower & Vasagatan — Receiver, Antenna, Engine)</color>", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 50), () =>
                {
                    UnlockChapterBlueprints(1, "Radio Tower & Vasagatan", new[] { "antenna", "receiver", "headlight", "machete", "steering", "engine" });
                }, WoodButtonNormal, TextParchmentLight, 14);
                var c1Outline = btnCh1.AddComponent<Outline>();
                c1Outline.effectColor = WoodTrimAccent * 0.7f;
                c1Outline.effectDistance = new Vector2(1, -1);
                EnsureLayout(btnCh1, -1, 50);

                var btnCh2 = CreateButton(page.transform, "Btn_Chapter2", "🐻 <b><color=#F5C761>3. Unlock Chapter 2 Blueprints</color></b> <color=#F2E6CC>(Balboa, Caravan Island, Tangaroa — Biofuel, Charger, Pipes)</color>", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 50), () =>
                {
                    UnlockChapterBlueprints(2, "Balboa / Caravan / Tangaroa", new[] { "biofuel", "storage", "charger", "grill", "pipe", "firework" });
                }, WoodButtonNormal, TextParchmentLight, 14);
                var c2Outline = btnCh2.AddComponent<Outline>();
                c2Outline.effectColor = WoodTrimAccent * 0.7f;
                c2Outline.effectDistance = new Vector2(1, -1);
                EnsureLayout(btnCh2, -1, 50);

                var btnCh3 = CreateButton(page.transform, "Btn_Chapter3", "🏙️ <b><color=#F5C761>4. Unlock Chapter 3 Blueprints</color></b> <color=#F2E6CC>(Varuna Point, Temperance, Utopia — Adv Battery, Windmill, Titanium)</color>", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 50), () =>
                {
                    UnlockChapterBlueprints(3, "Varuna / Temperance / Utopia", new[] { "batteryadvanced", "anchorstationaryadvanced", "backpackadvanced", "smelter", "windmill", "titanium", "biofuelextractoradvanced" });
                }, WoodButtonNormal, TextParchmentLight, 14);
                var c3Outline = btnCh3.AddComponent<Outline>();
                c3Outline.effectColor = WoodTrimAccent * 0.7f;
                c3Outline.effectDistance = new Vector2(1, -1);
                EnsureLayout(btnCh3, -1, 50);

                var statusBox = CreateBox(page.transform, "StatusBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 36), WoodTitleBar);
                EnsureLayout(statusBox, -1, 36);
                CreateBox(statusBox.transform, "Trim", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 1.5f), WoodTrimAccent);
                _researchStatusText = CreateText(statusBox.transform, "Status", "💡 <color=#E0D0B5>Status: Ready. Click any chapter above to learn blueprints into your research station.</color>", 14, FontStyle.Italic, TextParchmentLight, TextAnchor.MiddleCenter);
            }
            else
            {
                var infoBox = CreateBox(page.transform, "InfoBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 80), WoodPlankEven);
                EnsureLayout(infoBox, -1, 80);
                var ibOutline = infoBox.AddComponent<Outline>();
                ibOutline.effectColor = WoodRowBorder;
                ibOutline.effectDistance = new Vector2(1, -1);
                var boxLayout = infoBox.AddComponent<VerticalLayoutGroup>();
                boxLayout.padding = new RectOffset(18, 18, 10, 10);
                boxLayout.spacing = 4;
                boxLayout.childForceExpandWidth = true;

                var title = CreateText(infoBox.transform, "Title", "⚡ <b><color=#F5C761>Creative Sandbox</color> Blueprint Master Station</b>", 17, FontStyle.Bold, TextGoldHeading, TextAnchor.MiddleLeft);
                EnsureLayout(title.gameObject, -1, 24);

                var desc = CreateText(infoBox.transform, "Desc", "Creative Mode allows instant learning of every item, engine, weapon, tool, furniture, and story blueprint without visiting islands.", 14, FontStyle.Normal, TextParchmentLight, TextAnchor.UpperLeft);
                EnsureLayout(desc.gameObject, -1, 38);

                // Master Unlock Banner Button
                var unlockBtn = CreateButton(page.transform, "Btn_UnlockAllRD", "⚡ UNLOCK ALL 300+ R&D RECIPES & STORY BLUEPRINTS NOW", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 56), () =>
                {
                    try
                    {
                        var rt = ComponentManager<Inventory_ResearchTable>.Value ?? FindObjectOfType<Inventory_ResearchTable>();
                        if (rt != null)
                        {
                            rt.LearnAllRecipesInstantly();
                        }
                        Cheat.UnlockAllCrafting = true;
                        if (_researchStatusText != null)
                        {
                            _researchStatusText.text = "<b><color=#34D399>✅ SUCCESS: All 300+ R&D recipes and blueprints learned permanently!</color></b>";
                        }
                        Debug.Log("[Sailor's Companion] Unlocked all R&D recipes and blueprints!");
                    }
                    catch (Exception ex)
                    {
                        if (_researchStatusText != null)
                        {
                            _researchStatusText.text = $"<color=#F87171>Notice: {ex.Message} (Load into world first)</color>";
                        }
                        Debug.LogError("[Sailor's Companion] R&D error: " + ex);
                    }
                }, WoodButtonCrimson, TextWhite, 16);
                var uOutline = unlockBtn.AddComponent<Outline>();
                uOutline.effectColor = WoodTrimAccent;
                uOutline.effectDistance = new Vector2(2, -2);
                EnsureLayout(unlockBtn, -1, 56);

                CreateCategoryHeader(page.transform, "📖 TARGETED STORY CHAPTER UNLOCKS", 28f);

                var btnCh1 = CreateButton(page.transform, "Btn_Chapter1", "📻 <b><color=#F5C761>Chapter 1 Blueprints</color></b> (Radio Tower & Vasagatan)", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 44), () =>
                {
                    UnlockChapterBlueprints(1, "Radio Tower & Vasagatan", new[] { "antenna", "receiver", "headlight", "machete", "steering", "engine" });
                }, WoodButtonNormal, TextParchmentLight, 14);
                EnsureLayout(btnCh1, -1, 44);

                var btnCh2 = CreateButton(page.transform, "Btn_Chapter2", "🐻 <b><color=#F5C761>Chapter 2 Blueprints</color></b> (Balboa, Caravan Island, Tangaroa)", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 44), () =>
                {
                    UnlockChapterBlueprints(2, "Balboa / Caravan / Tangaroa", new[] { "biofuel", "storage", "charger", "grill", "pipe", "firework" });
                }, WoodButtonNormal, TextParchmentLight, 14);
                EnsureLayout(btnCh2, -1, 44);

                var btnCh3 = CreateButton(page.transform, "Btn_Chapter3", "🏙️ <b><color=#F5C761>Chapter 3 Blueprints</color></b> (Varuna Point, Temperance, Utopia)", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 44), () =>
                {
                    UnlockChapterBlueprints(3, "Varuna / Temperance / Utopia", new[] { "batteryadvanced", "anchorstationaryadvanced", "backpackadvanced", "smelter", "windmill", "titanium", "biofuelextractoradvanced" });
                }, WoodButtonNormal, TextParchmentLight, 14);
                EnsureLayout(btnCh3, -1, 44);

                var statusBox = CreateBox(page.transform, "StatusBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 36), WoodTitleBar);
                EnsureLayout(statusBox, -1, 36);
                CreateBox(statusBox.transform, "Trim", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 1.5f), WoodTrimAccent);
                _researchStatusText = CreateText(statusBox.transform, "Status", "💡 <color=#E0D0B5>Status: Ready. Click Master Unlock to learn everything, or choose specific chapters above.</color>", 14, FontStyle.Italic, TextParchmentLight, TextAnchor.MiddleCenter);
            }

            return page;
        }

        private void ResearchBaseMaterials()
        {
            try
            {
                var rt = ComponentManager<Inventory_ResearchTable>.Value ?? FindObjectOfType<Inventory_ResearchTable>();
                var all = ItemManager.GetAllItems();
                if (all == null || all.Count == 0)
                {
                    if (_researchStatusText != null)
                        _researchStatusText.text = "<color=#F87171>Load into a game world to research items!</color>";
                    return;
                }

                string[] baseKeywords = { "plank", "plastic", "scrap", "metal", "copper", "stone", "rope", "brick", "goo", "glass", "hinge", "bolt", "feather", "clay", "sand", "dirt", "leather", "wool" };
                int count = 0;
                foreach (var item in all)
                {
                    if (item == null || string.IsNullOrEmpty(item.UniqueName)) continue;
                    string name = item.UniqueName.ToLower();
                    if (name.StartsWith("blueprint_")) continue;
                    if (baseKeywords.Any(k => name.Contains(k)))
                    {
                        if (rt != null)
                        {
                            try { rt.Research(item, true); count++; } catch { }
                            var p = PlayerHelper.GetLocalPlayer();
                            if (p != null)
                            {
                                try { rt.LearnItem(item, p.steamID); } catch { }
                            }
                        }
                    }
                }

                if (_researchStatusText != null)
                    _researchStatusText.text = $"<color=#34D399><b>✅ Researched {count} base materials at the Research Table!</b></color>";
                TeleportManager.SetNotification($"🔬 Researched {count} base crafting materials!");
            }
            catch (Exception ex)
            {
                if (_researchStatusText != null)
                    _researchStatusText.text = $"<color=#F87171>Error: {ex.Message}</color>";
            }
        }

        private void UnlockChapterBlueprints(int chapter, string chapterName, string[] keywords)
        {
            try
            {
                var rt = ComponentManager<Inventory_ResearchTable>.Value ?? FindObjectOfType<Inventory_ResearchTable>();
                var all = ItemManager.GetAllItems();
                if (all == null || all.Count == 0)
                {
                    if (_researchStatusText != null)
                        _researchStatusText.text = "<color=#F87171>Load into a game world to unlock blueprints!</color>";
                    return;
                }

                int count = 0;
                foreach (var item in all)
                {
                    if (item == null || string.IsNullOrEmpty(item.UniqueName)) continue;
                    string name = item.UniqueName.ToLower();
                    if (keywords.Any(k => name.Contains(k)))
                    {
                        if (rt != null)
                        {
                            try { rt.ResearchBlueprint(item); count++; } catch { }
                        }
                    }
                }

                if (_researchStatusText != null)
                    _researchStatusText.text = $"<color=#34D399><b>✅ SUCCESS: Chapter {chapter} ({chapterName}) blueprints unlocked!</b></color>";
                TeleportManager.SetNotification($"📻 Chapter {chapter} Blueprints Unlocked!");
            }
            catch (Exception ex)
            {
                if (_researchStatusText != null)
                    _researchStatusText.text = $"<color=#F87171>Error: {ex.Message}</color>";
            }
        }
        // ============================================================================
        // [END] TAB 3: RESEARCH & R&D BLUEPRINTS
        // ============================================================================
        #endregion

        #region [START] TAB 4: 300+ ITEM SPAWNER ENGINE
        // ============================================================================
        // [START] TAB 4: 300+ ITEM SPAWNER ENGINE (Real-Time Search & Instant Spawning)
        // ============================================================================
        private GameObject BuildSpawnerTab(Transform parent)
        {
            if (Plugin.IsSurvivalMode)
            {
                return CreateLockCard(parent, "Item Spawner",
                    "Gathering resources, fishing for food, and diving for scrap form the core progression loop of Raft.\n\nItem Spawning is disabled in Survival Mode to preserve authentic accomplishment.\n\nSwitch to Creative / Sandbox Mode at the top or below to browse and spawn any of the 300+ items.",
                    () => SetModMode("Creative"));
            }

            var page = CreateBox(parent, "Page_Spawner", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(14, 14, 8, 8);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var banner = CreateBox(page.transform, "Banner", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 38), WoodTitleBar);
            EnsureLayout(banner, -1, 38);
            var bannerTxt = CreateText(banner.transform, "Txt", "📦 <b>Item Spawner:</b> Search any item in Raft and add stacks directly into your inventory.", 15, FontStyle.Normal, TextGoldHeading, TextAnchor.MiddleCenter);

            // Category Quick-Filter Chips Row
            var catRow = CreateBox(page.transform, "CatFilterRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 32), Color.clear);
            EnsureLayout(catRow, -1, 32);
            var catLayout = catRow.AddComponent<HorizontalLayoutGroup>();
            catLayout.spacing = 6;
            catLayout.childForceExpandWidth = true;

            string[] catLabels = { "🌐 All", "🪵 Resources", "🔨 Tools", "🍲 Food", "🏠 Decor & Base", "📻 Story" };
            string[] catFilters = { "", "plank plastic scrap metal titanium copper stone", "hook axe spear bow arrow machete headlight", "fish meat beet potato mango melon water soup", "foundation wall door window table chair bed paint", "blueprint receiver antenna key cassette note" };

            for (int c = 0; c < catLabels.Length; c++)
            {
                int cIdx = c;
                CreateButton(catRow.transform, $"Btn_Cat_{c}", catLabels[c], Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
                {
                    if (_itemSearchInput != null) _itemSearchInput.text = catFilters[cIdx];
                    RefreshItemSpawnerList(catFilters[cIdx]);
                }, WoodButtonNormal, TextParchmentLight, 13);
            }

            // Search row
            var searchRow = CreateBox(page.transform, "SearchRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 38), Color.clear);
            EnsureLayout(searchRow, -1, 38);
            var searchLayout = searchRow.AddComponent<HorizontalLayoutGroup>();
            searchLayout.spacing = 8;

            var inputGO = CreateBox(searchRow.transform, "InputSearch", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(620, 36), CheckboxWoodBg);
            EnsureLayout(inputGO, 620, 36, false);
            var inputTxt = CreateText(inputGO.transform, "Text", "", 15, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleLeft);
            inputTxt.rectTransform.offsetMin = new Vector2(12, 0);
            var placeholderTxt = CreateText(inputGO.transform, "Placeholder", "🔍 Type to search items... (e.g. plank, titanium, shark)", 14, FontStyle.Italic, TextMuted, TextAnchor.MiddleLeft);
            placeholderTxt.rectTransform.offsetMin = new Vector2(12, 0);
            _itemSearchInput = inputGO.AddComponent<InputField>();
            _itemSearchInput.textComponent = inputTxt;
            _itemSearchInput.placeholder = placeholderTxt;
            _itemSearchInput.onValueChanged.AddListener(s => RefreshItemSpawnerList(s));

            var clearBtn = CreateButton(searchRow.transform, "Btn_Clear", "✕ Clear", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(100, 36), () =>
            {
                if (_itemSearchInput != null) _itemSearchInput.text = "";
                RefreshItemSpawnerList("");
            }, WoodButtonNormal, TextParchmentLight, 14);
            EnsureLayout(clearBtn, 100, 36, false);

            var refreshBtn = CreateButton(searchRow.transform, "Btn_Refresh", "🔄 Refresh", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(120, 36), () => RefreshItemSpawnerList(_itemSearchInput?.text ?? ""), WoodButtonNormal, TextParchmentLight, 14);
            EnsureLayout(refreshBtn, 120, 36, false);

            // Scroll View with RectMask2D (Reliable 2D clipping without stencil mask alpha bug)
            var scrollGO = CreateBox(page.transform, "ScrollView", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 400), WoodWindowBg);
            EnsureLayout(scrollGO, -1, 400);
            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 25f;

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollGO.transform, false);
            var vpRt = viewport.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.pivot = new Vector2(0.5f, 0.5f);
            vpRt.sizeDelta = Vector2.zero;
            viewport.AddComponent<RectMask2D>();
            scrollRect.viewport = vpRt;

            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewport.transform, false);
            var cRt = contentGO.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0, 1);
            cRt.anchorMax = new Vector2(1, 1);
            cRt.pivot = new Vector2(0.5f, 1);
            cRt.sizeDelta = new Vector2(0, 400);
            var contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4;
            contentLayout.padding = new RectOffset(4, 4, 4, 4);
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            var csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = cRt;

            _itemScrollContent = contentGO.transform;
            RefreshItemSpawnerList("");

            return page;
        }


        private void RefreshItemSpawnerList(string filter)
        {
            if (_itemScrollContent == null) return;

            foreach (Transform child in _itemScrollContent)
            {
                Destroy(child.gameObject);
            }

            if (_allItems == null || _allItems.Count == 0)
            {
                var list = ItemManager.GetAllItems();
                if (list != null && list.Count > 0)
                {
                    _allItems = list.Where(i => i != null && !string.IsNullOrEmpty(i.UniqueName)).ToList();
                }
                else
                {
                    var found = Resources.FindObjectsOfTypeAll<Item_Base>();
                    if (found != null && found.Length > 0)
                    {
                        _allItems = found.Where(i => i != null && !string.IsNullOrEmpty(i.UniqueName)).Distinct().ToList();
                    }
                }
            }

            if (_allItems == null || _allItems.Count == 0)
            {
                var row = CreateBox(_itemScrollContent, "NoticeRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 70), WoodPlankEven);
                EnsureLayout(row, -1, 70);
                CreateText(row.transform, "NoticeTxt", "💡 <b>Items load when you load into a game world.</b>\nEnter a game world to browse and spawn all 300+ items directly into your inventory!", 15, FontStyle.Normal, TextParchmentWarm, TextAnchor.MiddleCenter);
                Canvas.ForceUpdateCanvases();
                return;
            }

            string[] tokens = string.IsNullOrEmpty(filter) ? new string[0] : filter.Trim().ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var filtered = _allItems.Where(i =>
            {
                if (tokens.Length == 0) return true;
                string uName = i.UniqueName?.ToLower() ?? "";
                string dName = i.settings_Inventory?.DisplayName?.ToLower() ?? "";
                // Match if all or any tokens match
                return tokens.Any(t => uName.Contains(t) || dName.Contains(t));
            }).Take(80).ToList();

            if (filtered.Count == 0)
            {
                var emptyRow = CreateBox(_itemScrollContent, "EmptyRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 80), WoodPlankEven);
                EnsureLayout(emptyRow, -1, 80);
                CreateText(emptyRow.transform, "EmptyTxt", $"🔍 <b>No items found matching \"{filter}\"</b>\nTry searching: <b>Hammer</b>, <b>Plank</b>, <b>Plastic</b>, <b>Scrap</b>, <b>Titanium</b>, or click <b>Clear</b>.", 15, FontStyle.Normal, TextParchmentWarm, TextAnchor.MiddleCenter);
                Canvas.ForceUpdateCanvases();
                return;
            }

            int itemIdx = 0;
            foreach (var item in filtered)
            {
                Color itemRowColor = (itemIdx++ % 2 == 0) ? WoodPlankEven : WoodPlankOdd;
                var row = CreateBox(_itemScrollContent, $"Item_{item.UniqueIndex}", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 42), itemRowColor);
                EnsureLayout(row, -1, 42);

                // Plank seam
                CreateBox(row.transform, "Seam", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 1), WoodRowBorder);

                var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
                rowLayout.spacing = 8;
                rowLayout.padding = new RectOffset(14, 14, 0, 0);
                rowLayout.childForceExpandHeight = false;

                string disp = item.settings_Inventory?.DisplayName;
                if (string.IsNullOrEmpty(disp)) disp = item.UniqueName;

                var nameTxt = CreateText(row.transform, "Name", $"<b>{disp}</b> <size=13><color=#AD9473>({item.UniqueName})</color></size>", 15, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleLeft);
                EnsureLayout(nameTxt.gameObject, 640, 34, true);

                string uniqueName = item.UniqueName;
                int stack = item.settings_Inventory != null ? item.settings_Inventory.StackSize : 20;

                var btn1 = CreateButton(row.transform, "Btn_1", "+1", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(56, 32), () => GiveItem(uniqueName, 1), WoodButtonNormal, TextParchmentLight, 14);
                EnsureLayout(btn1, 56, 32, false);

                var btn10 = CreateButton(row.transform, "Btn_10", "+10", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(56, 32), () => GiveItem(uniqueName, 10), WoodButtonNormal, TextParchmentLight, 14);
                EnsureLayout(btn10, 56, 32, false);

                string stackLabel = stack > 1 ? $"+{stack}" : "+Max";
                var btnStack = CreateButton(row.transform, "Btn_Stack", stackLabel, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(76, 32), () => GiveItem(uniqueName, stack), TabActiveBg, TabActiveText, 14);
                EnsureLayout(btnStack, 76, 32, false);
            }

            Canvas.ForceUpdateCanvases();
        }

        private void GiveItem(string uniqueName, int amount)
        {
            var p = PlayerHelper.GetLocalPlayer();
            if (p?.Inventory != null)
            {
                p.Inventory.AddItem(uniqueName, amount);
                TeleportManager.SetNotification($"📦 Received {amount}x {uniqueName}");
            }
            else
            {
                TeleportManager.SetNotification("⚠️ Enter a game world to spawn items!");
            }
        }
        // ============================================================================
        // [END] TAB 3: 300+ ITEM SPAWNER ENGINE
        // ============================================================================
        #endregion

        private void SelectTab(int tabIndex)
        {
            _activeTab = tabIndex;
            for (int i = 0; i < _tabPages.Length; i++)
            {
                if (_tabPages[i] != null)
                {
                    _tabPages[i].SetActive(i == tabIndex);
                }
                if (_tabButtonImages[i] != null)
                {
                    bool isSelected = (i == tabIndex);
                    Color targetBg = isSelected ? TabActiveBg : TabInactiveBg;
                    Color targetText = isSelected ? TabActiveText : TabInactiveText;

                    _tabButtonImages[i].color = targetBg;
                    var btn = _tabButtonImages[i].GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.transition = Selectable.Transition.None;
                        var cb = btn.colors;
                        cb.normalColor = targetBg;
                        cb.highlightedColor = isSelected ? targetBg : WoodButtonHover;
                        cb.pressedColor = WoodWindowBorder;
                        cb.selectedColor = targetBg;
                        btn.colors = cb;
                    }

                    if (_tabButtonTexts[i] != null)
                    {
                        _tabButtonTexts[i].color = targetText;
                        _tabButtonTexts[i].fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
                    }
                }
            }

            if (tabIndex == 4 && _itemScrollContent != null)
            {
                RefreshItemSpawnerList(_itemSearchInput?.text ?? "");
            }
        }


        #region [START] UI COMPONENT BUILDERS (Buttons, Toggles, Sliders)
        // ============================================================================
        // [START] UI COMPONENT BUILDERS (Buttons, Toggles, Sliders)
        // ============================================================================
        private GameObject CreateBox(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, FontStyle style, Color color, TextAnchor alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            var t = go.AddComponent<Text>();
            t.font = GetGameFont();
            t.text = content;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = alignment;
            t.supportRichText = true;
            return t;
        }

        private GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Action onClick, Color bgColor, Color textColor, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = WoodButtonHover;
            cb.pressedColor = WoodWindowBorder;
            cb.selectedColor = bgColor;
            btn.colors = cb;

            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var textRt = textGO.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            var t = textGO.AddComponent<Text>();
            t.font = GetGameFont();
            t.text = label;
            t.fontSize = fontSize;
            t.fontStyle = FontStyle.Bold;
            t.color = textColor;
            t.alignment = TextAnchor.MiddleCenter;
            t.supportRichText = true;

            return go;
        }

        private GameObject CreateActionTile(Transform parent, string name, string title, string hotkey, Action onClick, out Text titleTextOut, float preferredHeight = 38f, float chipWidth = 78f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            EnsureLayout(go, -1, preferredHeight, true);

            var img = go.AddComponent<Image>();
            img.color = ActionTileBg;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = ActionTileBorder;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor = ActionTileBg;
            cb.highlightedColor = ActionTileHover;
            cb.pressedColor = new Color(0.18f, 0.10f, 0.05f, 1.0f);
            cb.selectedColor = ActionTileBg;
            btn.colors = cb;

            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var innerLayout = go.AddComponent<HorizontalLayoutGroup>();
            innerLayout.padding = new RectOffset(12, 8, 3, 3);
            innerLayout.spacing = 8;
            innerLayout.childForceExpandWidth = false;
            innerLayout.childForceExpandHeight = true;
            innerLayout.childControlWidth = true;
            innerLayout.childControlHeight = true;

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(go.transform, false);
            var titleTxt = titleGO.AddComponent<Text>();
            titleTxt.font = GetGameFont();
            titleTxt.text = title;
            titleTxt.fontSize = 14;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = TextWhite;
            titleTxt.alignment = TextAnchor.MiddleLeft;
            titleTxt.supportRichText = true;
            titleTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleTxt.verticalOverflow = VerticalWrapMode.Truncate;

            var titleLe = titleGO.AddComponent<LayoutElement>();
            titleLe.flexibleWidth = 1f;
            titleTextOut = titleTxt;

            if (!string.IsNullOrEmpty(hotkey))
            {
                var chipGO = new GameObject("HotkeyChip");
                chipGO.transform.SetParent(go.transform, false);
                var chipLe = chipGO.AddComponent<LayoutElement>();
                chipLe.preferredWidth = chipWidth;
                chipLe.preferredHeight = Mathf.Max(24f, preferredHeight - 10f);
                chipLe.flexibleWidth = 0f;

                var chipImg = chipGO.AddComponent<Image>();
                chipImg.color = new Color(0.12f, 0.07f, 0.03f, 0.95f);
                var chipOutline = chipGO.AddComponent<Outline>();
                chipOutline.effectColor = new Color(1.0f, 0.82f, 0.35f, 0.85f);
                chipOutline.effectDistance = new Vector2(1, -1);

                var chipTxtGO = new GameObject("ChipText");
                chipTxtGO.transform.SetParent(chipGO.transform, false);
                var chipTxtRt = chipTxtGO.AddComponent<RectTransform>();
                chipTxtRt.anchorMin = Vector2.zero;
                chipTxtRt.anchorMax = Vector2.one;
                chipTxtRt.offsetMin = Vector2.zero;
                chipTxtRt.offsetMax = Vector2.zero;

                var chipTxt = chipTxtGO.AddComponent<Text>();
                chipTxt.font = GetGameFont();
                chipTxt.text = hotkey;
                chipTxt.fontSize = 12;
                chipTxt.fontStyle = FontStyle.Bold;
                chipTxt.color = TextGoldHeading;
                chipTxt.alignment = TextAnchor.MiddleCenter;
            }

            return go;
        }

        private GameObject CreateActionTile(Transform parent, string name, string title, string hotkey, Action onClick, float preferredHeight = 38f, float chipWidth = 78f)
        {
            return CreateActionTile(parent, name, title, hotkey, onClick, out _, preferredHeight, chipWidth);
        }

        #region [START] UI TOGGLE ITEM WITH RAFT RECESSED CHECKBOX
        // ============================================================================
        // [START] UI TOGGLE ITEM WITH RAFT RECESSED CHECKBOX (Authentic In-Game Settings Style)
        // ============================================================================
        private void CreateToggleItem(Transform parent, string label, bool initialValue, Action<bool> onToggle, float rowHeight = 36f, int fontSize = 13, string tooltip = null)
        {
            Color plankColor = (_toggleItemCounter++ % 2 == 0) ? WoodPlankEven : WoodPlankOdd;
            var row = CreateBox(parent, "ToggleRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, rowHeight), plankColor);
            EnsureLayout(row, -1, rowHeight);

            // Subtle wood plank seam
            CreateBox(row.transform, "PlankSeam", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 1), WoodRowBorder);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(12, 10, 2, 2);
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            // Feature Label
            var t = CreateText(row.transform, "Label", label, fontSize, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleLeft);
            var tLe = t.gameObject.AddComponent<LayoutElement>();
            tLe.flexibleWidth = 1f;
            tLe.preferredHeight = rowHeight - 4;

            // Raft Recessed Square Wooden Checkbox (26x26)
            var checkContainer = CreateBox(row.transform, "CheckContainer", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(26, 26), CheckboxWoodBg);
            var cbLe = checkContainer.AddComponent<LayoutElement>();
            cbLe.preferredWidth = 26;
            cbLe.preferredHeight = 26;
            cbLe.flexibleWidth = 0f;

            // Checkbox timber border
            var cbBorder = CreateBox(checkContainer.transform, "Border", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, WoodWindowBorder);
            var cbRt = cbBorder.GetComponent<RectTransform>();
            cbRt.offsetMin = new Vector2(-1, -1);
            cbRt.offsetMax = new Vector2(1, 1);
            cbBorder.transform.SetAsFirstSibling();

            var checkTxt = CreateText(checkContainer.transform, "Checkmark", initialValue ? "✔" : "", 17, FontStyle.Bold, CheckmarkGold, TextAnchor.MiddleCenter);

            bool state = initialValue;

            void UpdateVisuals(bool isOn)
            {
                checkTxt.text = isOn ? "✔" : "";
            }

            // Click button on checkbox container
            var btn = checkContainer.AddComponent<Button>();
            btn.targetGraphic = checkContainer.GetComponent<Image>();
            var cb = btn.colors;
            cb.normalColor = CheckboxWoodBg;
            cb.highlightedColor = WoodButtonHover;
            cb.pressedColor = WoodWindowBorder;
            cb.selectedColor = CheckboxWoodBg;
            btn.colors = cb;

            void Toggle()
            {
                state = !state;
                UpdateVisuals(state);
                MarkProfileCustom();
                if (!string.IsNullOrEmpty(tooltip)) SetQoLTooltip(tooltip);
                onToggle?.Invoke(state);
            }

            btn.onClick.AddListener(Toggle);

            // Also allow clicking anywhere on the plank row to toggle
            var rowBtn = row.AddComponent<Button>();
            rowBtn.targetGraphic = row.GetComponent<Image>();
            var rcb = rowBtn.colors;
            rcb.normalColor = plankColor;
            rcb.highlightedColor = WoodPlankOdd;
            rcb.pressedColor = WoodWindowBg;
            rcb.selectedColor = plankColor;
            rowBtn.colors = rcb;
            rowBtn.onClick.AddListener(Toggle);
        }
        // ============================================================================
        // [END] UI TOGGLE ITEM WITH RAFT RECESSED CHECKBOX
        // ============================================================================
        #endregion

        private void CreateButtonItem(Transform parent, string label, Action onClick)
        {
            var btn = CreateButton(parent, "ButtonItem", label, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 44), onClick, WoodButtonNormal, TextParchmentLight, 15);
            EnsureLayout(btn, -1, 44);
        }

        private void CreateStepperItem(Transform parent, string label, float min, float max, float step, float initialValue, string unit, Action<float> onChange)
        {
            var row = CreateBox(parent, "StepperRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 42), WoodPlankEven);
            EnsureLayout(row, -1, 42);
            CreateBox(row.transform, "Seam", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 1), WoodRowBorder);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.padding = new RectOffset(14, 14, 0, 0);
            layout.childForceExpandHeight = false;

            float currentVal = initialValue;

            var labelTxt = CreateText(row.transform, "Label", $"{label}: <b>{currentVal:F1}{unit}</b>", 16, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleLeft);
            EnsureLayout(labelTxt.gameObject, 520, 34, true);

            var minusBtn = CreateButton(row.transform, "Btn_Minus", "  -  ", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(54, 32), () =>
            {
                currentVal = Mathf.Max(min, currentVal - step);
                labelTxt.text = $"{label}: <b>{currentVal:F1}{unit}</b>";
                onChange?.Invoke(currentVal);
            }, WoodButtonNormal, TextParchmentLight, 18);
            EnsureLayout(minusBtn, 54, 32, false);

            var plusBtn = CreateButton(row.transform, "Btn_Plus", "  +  ", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(54, 32), () =>
            {
                currentVal = Mathf.Min(max, currentVal + step);
                labelTxt.text = $"{label}: <b>{currentVal:F1}{unit}</b>";
                onChange?.Invoke(currentVal);
            }, WoodButtonNormal, TextParchmentLight, 18);
            EnsureLayout(plusBtn, 54, 32, false);
        }

        private string FormatMultiplierBadge(string label, float val, string format, string unit)
        {
            string valStr = val.ToString(format);
            return $"{label}: <color=#FFD54F><b>{valStr}{unit}</b></color>";
        }

        private void CreateDualStepperRow(Transform parent,
            string label1, float min1, float max1, float step1, float initial1, string unit1, Action<float> cb1, string tooltip1,
            string label2, float min2, float max2, float step2, float initial2, string unit2, Action<float> cb2, string tooltip2,
            float rowHeight = 38f)
        {
            var row = CreateBox(parent, "DualStepperRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, rowHeight), Color.clear);
            EnsureLayout(row, -1, rowHeight);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateHalfStepper(row.transform, label1, min1, max1, step1, initial1, unit1, cb1, rowHeight, tooltip1);
            CreateHalfStepper(row.transform, label2, min2, max2, step2, initial2, unit2, cb2, rowHeight, tooltip2);
        }

        private void CreateHalfStepper(Transform parent, string label, float min, float max, float step, float initialVal, string unit, Action<float> onChange, float height, string tooltip = null)
        {
            var box = CreateBox(parent, "StepperBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, height), WoodPlankEven);
            EnsureLayout(box, -1, height);
            CreateBox(box.transform, "Seam", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 1), WoodRowBorder);

            var layout = box.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.padding = new RectOffset(12, 8, 2, 2);
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            float currentVal = initialVal;
            string format = (step < 1f) ? "F1" : "F0";

            var labelTxt = CreateText(box.transform, "Label", FormatMultiplierBadge(label, currentVal, format, unit), 15, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleLeft);
            var lLe = labelTxt.gameObject.AddComponent<LayoutElement>();
            lLe.flexibleWidth = 1f;

            var minusBtn = CreateButton(box.transform, "Btn_Minus", "－", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(34, height - 6), () =>
            {
                currentVal = Mathf.Max(min, currentVal - step);
                labelTxt.text = FormatMultiplierBadge(label, currentVal, format, unit);
                MarkProfileCustom();
                if (!string.IsNullOrEmpty(tooltip)) SetQoLTooltip(tooltip);
                onChange?.Invoke(currentVal);
            }, WoodButtonNormal, TextParchmentLight, 18);
            var mLe = minusBtn.AddComponent<LayoutElement>();
            mLe.preferredWidth = 34;
            mLe.preferredHeight = height - 6;
            mLe.flexibleWidth = 0f;

            var plusBtn = CreateButton(box.transform, "Btn_Plus", "＋", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(34, height - 6), () =>
            {
                currentVal = Mathf.Min(max, currentVal + step);
                labelTxt.text = FormatMultiplierBadge(label, currentVal, format, unit);
                MarkProfileCustom();
                if (!string.IsNullOrEmpty(tooltip)) SetQoLTooltip(tooltip);
                onChange?.Invoke(currentVal);
            }, WoodButtonNormal, TextParchmentLight, 18);
            var pLe = plusBtn.AddComponent<LayoutElement>();
            pLe.preferredWidth = 34;
            pLe.preferredHeight = height - 6;
            pLe.flexibleWidth = 0f;
        }

        private void CreateTripleStepperRow(Transform parent,
            string label1, float min1, float max1, float step1, float initial1, string unit1, Action<float> cb1, string tooltip1,
            string label2, float min2, float max2, float step2, float initial2, string unit2, Action<float> cb2, string tooltip2,
            string label3, float min3, float max3, float step3, float initial3, string unit3, Action<float> cb3, string tooltip3,
            float rowHeight = 32f)
        {
            var row = CreateBox(parent, "TripleStepperRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, rowHeight), Color.clear);
            EnsureLayout(row, -1, rowHeight);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateThirdStepper(row.transform, label1, min1, max1, step1, initial1, unit1, cb1, rowHeight, tooltip1);
            CreateThirdStepper(row.transform, label2, min2, max2, step2, initial2, unit2, cb2, rowHeight, tooltip2);
            CreateThirdStepper(row.transform, label3, min3, max3, step3, initial3, unit3, cb3, rowHeight, tooltip3);
        }

        private void CreateThirdStepper(Transform parent, string label, float min, float max, float step, float initialVal, string unit, Action<float> onChange, float height, string tooltip = null)
        {
            var box = CreateBox(parent, "StepperBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, height), WoodPlankEven);
            EnsureLayout(box, -1, height);
            CreateBox(box.transform, "Seam", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 1), WoodRowBorder);

            var layout = box.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4;
            layout.padding = new RectOffset(8, 6, 2, 2);
            layout.childForceExpandHeight = false;

            float currentVal = initialVal;
            string format = (step < 1f) ? "F1" : "F0";

            var labelTxt = CreateText(box.transform, "Label", FormatMultiplierBadge(label, currentVal, format, unit), 12, FontStyle.Normal, TextParchmentLight, TextAnchor.MiddleLeft);
            EnsureLayout(labelTxt.gameObject, 195, height - 4, true);

            var minusBtn = CreateButton(box.transform, "Btn_Minus", " - ", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(34, height - 6), () =>
            {
                currentVal = Mathf.Max(min, currentVal - step);
                labelTxt.text = FormatMultiplierBadge(label, currentVal, format, unit);
                MarkProfileCustom();
                if (!string.IsNullOrEmpty(tooltip)) SetQoLTooltip(tooltip);
                onChange?.Invoke(currentVal);
            }, WoodButtonNormal, TextParchmentLight, 15);
            EnsureLayout(minusBtn, 34, height - 6, false);

            var plusBtn = CreateButton(box.transform, "Btn_Plus", " + ", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(34, height - 6), () =>
            {
                currentVal = Mathf.Min(max, currentVal + step);
                labelTxt.text = FormatMultiplierBadge(label, currentVal, format, unit);
                MarkProfileCustom();
                if (!string.IsNullOrEmpty(tooltip)) SetQoLTooltip(tooltip);
                onChange?.Invoke(currentVal);
            }, WoodButtonNormal, TextParchmentLight, 15);
            EnsureLayout(plusBtn, 34, height - 6, false);
        }
        // ============================================================================
        // [END] UI COMPONENT BUILDERS
        // ============================================================================
        #endregion

        #region [START] UPDATE LOOP & HOTKEY HANDLER
        // ============================================================================
        // [START] UPDATE LOOP & HOTKEY HANDLER (F5, F6, F/F7, F8, F9, ESC)
        // ============================================================================
        private void Update()
        {
            try
            {
                if (_canvasGO == null || _modWindowGO == null)
                {
                    BuildCanvasUI();
                }

                // Check hotkeys (F5 or Insert for mod window, F6 for HUD, F/F7 for Fly, F8 for Teleport, F9 for Summon)
                KeyCode keyMenu = Plugin.KeyMenu != null ? Plugin.KeyMenu.Value : KeyCode.F5;
                if (InputHelper.WasKeyPressed(keyMenu) || InputHelper.WasKeyPressed(KeyCode.Insert))
                {
                    ToggleModWindow();
                }
                if (InputHelper.WasKeyPressed(KeyCode.Escape) && _modWindowGO != null && _modWindowGO.activeSelf)
                {
                    ToggleModWindow();
                }
                KeyCode keyHud = Plugin.KeyHUD != null ? Plugin.KeyHUD.Value : KeyCode.F6;
                if (InputHelper.WasKeyPressed(keyHud))
                {
                    if (InputHelper.IsKeyHeld(KeyCode.LeftShift) || InputHelper.IsKeyHeld(KeyCode.RightShift))
                    {
                        HUDOverlay.CycleStyle();
                        UpdateNavStyleButtonVisuals();
                    }
                    else
                    {
                        Plugin.EnableHUD.Value = !Plugin.EnableHUD.Value;
                    }
                }
                // Fly / NoClip Hotkey (Strictly single key, safely locked in Survival mode)
                KeyCode keyFly = Plugin.KeyFly != null ? Plugin.KeyFly.Value : KeyCode.F;
                if (InputHelper.WasKeyPressed(keyFly))
                {
                    if (Plugin.IsSurvivalMode)
                    {
                        TeleportManager.SetNotification("🔒 Fly / NoClip is locked in Survival Mode. Switch to Creative Mode in [F5] menu.");
                    }
                    else
                    {
                        Plugin.EnableFlyMode.Value = !Plugin.EnableFlyMode.Value;
                        TeleportManager.SetNotification(Plugin.EnableFlyMode.Value ? "🕊️ Fly / NoClip: ON" : "🕊️ Fly / NoClip: OFF");
                    }
                }

                // Quick Gameplay Hotkeys ([F4] Sails, [F3] Engines, [F7] Magnet, [F10] Radar)
                if (Plugin.EnableHotkeys != null && Plugin.EnableHotkeys.Value)
                {
                    KeyCode keySails = Plugin.KeySailToggle != null ? Plugin.KeySailToggle.Value : KeyCode.F4;
                    if (InputHelper.WasKeyPressed(keySails))
                    {
                        BoatController.ToggleAllSails();
                    }

                    KeyCode keyEngines = Plugin.KeyEngineToggle != null ? Plugin.KeyEngineToggle.Value : KeyCode.F3;
                    if (InputHelper.WasKeyPressed(keyEngines))
                    {
                        BoatController.ToggleAllEngines();
                    }

                    KeyCode keyMagnet = Plugin.KeyMagnetToggle != null ? Plugin.KeyMagnetToggle.Value : KeyCode.F7;
                    if (InputHelper.WasKeyPressed(keyMagnet))
                    {
                        MagneticCollector.ToggleMagnet();
                    }

                    KeyCode keyScan = Plugin.KeyScannerPulse != null ? Plugin.KeyScannerPulse.Value : KeyCode.F10;
                    if (InputHelper.WasKeyPressed(keyScan))
                    {
                        ItemDetector.TriggerPulseScan();
                    }
                }

                KeyCode keyTeleRaft = Plugin.KeyTeleportToRaft != null ? Plugin.KeyTeleportToRaft.Value : KeyCode.F8;
                if (InputHelper.WasKeyPressed(keyTeleRaft))
                {
                    TeleportManager.TeleportPlayerToRaft();
                }
                KeyCode keySummon = Plugin.KeyTeleportRaftToPlayer != null ? Plugin.KeyTeleportRaftToPlayer.Value : KeyCode.F9;
                if (InputHelper.WasKeyPressed(keySummon))
                {
                    TeleportManager.TeleportRaftToPlayer();
                }

                // Free and unlock cursor only when mod window is open
                if (IsWindowOpen)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    try
                    {
                        Helper.CursorVisible = true;
                        Helper.SetCursorLockState(CursorLockMode.None);
                    }
                    catch {}
                }

                // Update live Navigation tab & Survival QoL data if visible (throttled to 5Hz to prevent frame lag)
                if ((_activeTab == 0 || _activeTab == 2) && Time.unscaledTime - _lastNavTabUpdate > 0.2f)
                {
                    _lastNavTabUpdate = Time.unscaledTime;
                    UpdateNavTabText();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Sailor's Companion] Error in CanvasModUI.Update: " + ex.Message);
            }
        }

        private float _lastNavTabUpdate = 0f;
        private static Raft _cachedNavRaft = null;
        private static AI_StateMachine_Shark _cachedNavShark = null;
        private static Camera _cachedNavCamera = null;

        private static string GetSailModeDisplayName()
        {
            int mode = Plugin.BoatSailMode != null ? Plugin.BoatSailMode.Value : 0;
            switch (mode)
            {
                case 1: return "🧭 Sail: Auto-Wind";
                case 2: return "🧭 Sail: Follow Heading";
                default: return "🧭 Sail: Manual Control";
            }
        }

        private void CycleSailMode()
        {
            if (Plugin.BoatSailMode == null) return;
            int next = (Plugin.BoatSailMode.Value + 1) % 3;
            Plugin.BoatSailMode.Value = next;
            if (_navSailModeBtnText != null)
            {
                _navSailModeBtnText.text = GetSailModeDisplayName();
            }
            BoatController.ApplyActiveSailMode();
            TeleportManager.SetNotification($"🧭 Smart Sail Mode: {GetSailModeDisplayName()}");
        }

        private void UpdateNavTabText()
        {
            if (_navRecallBtnText != null)
            {
                float cd = TeleportManager.GetRecallCooldownRemaining();
                if (cd > 0f)
                {
                    int sec = Mathf.CeilToInt(cd);
                    int mins = sec / 60;
                    int s = sec % 60;
                    string cdStr = mins > 0 ? $"{mins}m {s}s" : $"{s}s";
                    _navRecallBtnText.text = $"⏳ Recall ({cdStr})";
                }
                else
                {
                    _navRecallBtnText.text = "⚡ Recall to Raft";
                }
            }

            if (_navScannerBtnText != null)
            {
                if (ItemDetector.IsScanActive)
                {
                    int sec = Mathf.CeilToInt(ItemDetector.GetActiveTimeRemaining());
                    _navScannerBtnText.text = $"🔍 Active ({sec}s)";
                }
                else
                {
                    float cd = ItemDetector.GetCooldownRemaining();
                    if (cd > 0f)
                    {
                        int sec = Mathf.CeilToInt(cd);
                        _navScannerBtnText.text = $"⏳ Scan ({sec}s)";
                    }
                    else
                    {
                        _navScannerBtnText.text = "🔍 Island Radar";
                    }
                }
            }

            if (_qolMagnetBtnText != null)
            {
                if (MagneticCollector.IsActive)
                {
                    int sec = Mathf.CeilToInt(MagneticCollector.GetActiveTimeRemaining());
                    _qolMagnetBtnText.text = $"🧲 Active ({sec}s)";
                }
                else
                {
                    float cd = MagneticCollector.GetCooldownRemaining();
                    if (cd > 0f)
                    {
                        int sec = Mathf.CeilToInt(cd);
                        _qolMagnetBtnText.text = $"⏳ Magnet ({sec}s)";
                    }
                    else
                    {
                        _qolMagnetBtnText.text = "🧲 Ocean Magnet";
                    }
                }
            }

            if (_navSailModeBtnText != null)
            {
                _navSailModeBtnText.text = GetSailModeDisplayName();
            }

            var p = PlayerHelper.GetLocalPlayer();
            if (p == null)
            {
                if (_teleHeadingText != null) _teleHeadingText.text = "<b><color=#AD9473>Standby</color></b>\n<size=12><color=#7A6A55>Enter world to read compass</color></size>";
                if (_teleRaftText != null) _teleRaftText.text = "<b><color=#AD9473>Standby</color></b>\n<size=12><color=#7A6A55>Waiting for save file</color></size>";
                if (_teleSharkText != null) _teleSharkText.text = "<b><color=#34D399>Sonar Clear</color></b>\n<size=12><color=#7A6A55>No hostile predator detected</color></size>";
                if (_teleCoordsText != null) _teleCoordsText.text = "<b><color=#AD9473>X: 0.0  Y: 0.0  Z: 0.0</color></b>\n<size=12><color=#7A6A55>Waiting for world telemetry</color></size>";
                if (_navStatusText != null) _navStatusText.text = "<color=#94A3B8>Enter a game world to see live navigation, raft tracking, and shark distance data.</color>";
                return;
            }

            if (_cachedNavCamera == null) _cachedNavCamera = Camera.main;
            float yaw = _cachedNavCamera != null ? _cachedNavCamera.transform.eulerAngles.y : p.transform.eulerAngles.y;
            string[] cardinals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            int cIndex = Mathf.RoundToInt(yaw / 45f) % 8;
            if (cIndex < 0) cIndex += 8;

            if (_teleHeadingText != null)
            {
                _teleHeadingText.text = $"<b><color=#FFD54F><size=17>{yaw:000}° ({cardinals[cIndex]})</size></color></b>\n<size=12><color=#E6CEAC>Facing {cardinals[cIndex]} Direction</color></size>";
            }

            string raftStr = "Raft: Not detected";
            if (_cachedNavRaft == null || !_cachedNavRaft.gameObject.activeInHierarchy)
            {
                _cachedNavRaft = ComponentManager<Raft>.Value ?? FindObjectOfType<Raft>();
            }
            if (_cachedNavRaft != null)
            {
                float dist = Vector3.Distance(p.transform.position, _cachedNavRaft.transform.position);
                string stateStr = _cachedNavRaft.IsAnchored ? "<color=#F87171>Anchored</color>" : "<color=#34D399>Drifting</color>";
                float knots = _cachedNavRaft.Velocity.magnitude * 1.94f;
                raftStr = $"Raft: <b>{dist:F0}m</b> away  |  State: <b>{(_cachedNavRaft.IsAnchored ? "Anchored" : "Drifting")}</b>  |  Speed: <b>{knots:F1} knots</b>";
                if (_teleRaftText != null)
                {
                    _teleRaftText.text = $"<b><color=#FFFFFF>{dist:F0}m Away</color></b> | {stateStr}\n<size=12><color=#E6CEAC>Velocity: <b>{knots:F1} knots</b></color></size>";
                }
            }
            else if (_teleRaftText != null)
            {
                _teleRaftText.text = "<b><color=#F87171>Not Detected</color></b>\n<size=12><color=#E6CEAC>Raft reference missing</color></size>";
            }

            string sharkStr = "Bruce: Peaceful";
            if (_cachedNavShark == null || !_cachedNavShark.gameObject.activeInHierarchy)
            {
                _cachedNavShark = FindObjectOfType<AI_StateMachine_Shark>();
            }
            if (_cachedNavShark != null && _cachedNavShark.gameObject.activeInHierarchy)
            {
                float sDist = Vector3.Distance(p.transform.position, _cachedNavShark.transform.position);
                sharkStr = $"Bruce the Shark: <b>{sDist:F0}m</b> away";
                if (_teleSharkText != null)
                {
                    string alertColor = sDist < 25f ? "#EF4444" : (sDist < 50f ? "#F59E0B" : "#34D399");
                    string threat = sDist < 25f ? "DANGER: Close!" : (sDist < 50f ? "Prowling nearby" : "Far away / Calm");
                    _teleSharkText.text = $"<b><color={alertColor}>{sDist:F0}m Away</color></b>\n<size=12><color=#E6CEAC>{threat}</color></size>";
                }
            }
            else if (_teleSharkText != null)
            {
                _teleSharkText.text = "<b><color=#34D399>No Shark Detected</color></b>\n<size=12><color=#E6CEAC>Ocean waters are clear</color></size>";
            }

            if (_teleCoordsText != null)
            {
                _teleCoordsText.text = $"<b><color=#FFD54F>X: {p.transform.position.x:F1}  Z: {p.transform.position.z:F1}</color></b>\n<size=12><color=#E6CEAC>Altitude: <b>Y: {p.transform.position.y:F1}m</b></color></size>";
            }

            string notifStr = "";
            if (!string.IsNullOrEmpty(TeleportManager.LastStatusMessage) && (Time.unscaledTime - TeleportManager.LastStatusTime < 8.0f))
            {
                notifStr = $"\n<color=#EF4444><b>Notification:</b> {TeleportManager.LastStatusMessage}</color>";
                if (_teleNotifText != null)
                {
                    _teleNotifText.text = $"📢 <color=#EF4444><b>Notification:</b> {TeleportManager.LastStatusMessage}</color>";
                }
            }
            else if (_teleNotifText != null)
            {
                _teleNotifText.text = "💡 <color=#E0D0B5>Hotkeys:</color> <color=#F5C761>[F5]</color> Menu  |  <color=#F5C761>[F6]</color> HUD  |  <color=#F5C761>[Shift+F6]</color> Style  |  <color=#F5C761>[F4]</color> Sails  |  <color=#F5C761>[F3]</color> Engines  |  <color=#F5C761>[F8]</color> Recall  |  <color=#F5C761>[F9]</color> Summon";
            }

            if (_navStatusText != null)
            {
                _navStatusText.text = $"• Player Position: <b>X: {p.transform.position.x:F1}, Y: {p.transform.position.y:F1}, Z: {p.transform.position.z:F1}</b>\n" +
                                      $"• Facing Direction: <b>{yaw:000}° ({cardinals[cIndex]})</b>\n" +
                                      $"• {raftStr}\n" +
                                      $"• {sharkStr}{notifStr}\n\n" +
                                      $"<size=13><color=#CBD5E1>Hotkeys: [F5] Menu  |  [F6] HUD  |  [Shift+F6] Cycle Style  |  [F] Fly  |  [F8] Recall  |  [F9] Summon</color></size>";
            }

            UpdateNavStyleButtonVisuals();
        }

        private void UpdateNavStyleButtonVisuals()
        {
            if (_navStyleBtns == null || _navStyleBtns.Count == 0) return;
            int current = Plugin.HUDStyle != null ? Plugin.HUDStyle.Value : 0;
            for (int i = 0; i < _navStyleBtns.Count; i++)
            {
                if (_navStyleBtns[i] != null)
                {
                    bool isSel = (i == current);
                    var img = _navStyleBtns[i].GetComponent<Image>();
                    if (img != null)
                        img.color = isSel ? TabActiveBg : WoodButtonNormal;
                    var txt = _navStyleBtns[i].GetComponentInChildren<Text>();
                    if (txt != null)
                    {
                        txt.color = isSel ? TabActiveText : TextParchmentLight;
                        txt.fontStyle = isSel ? FontStyle.Bold : FontStyle.Normal;
                    }
                }
            }
        }

        public void RefreshUpdateBanner()
        {
            if (_updateBannerGO == null) return;
            bool show = UpdateChecker.IsUpdateAvailable && !UpdateChecker.Dismissed;
            _updateBannerGO.SetActive(show);
            if (show && _updateBannerText != null)
            {
                string notes = !string.IsNullOrEmpty(UpdateChecker.ReleaseNotes) ? $" — <i>\"{UpdateChecker.ReleaseNotes}\"</i>" : "";
                _updateBannerText.text = $"✨ <b>New Update v{UpdateChecker.LatestVersion} Available!</b> (Current: v{PluginInfo.PLUGIN_VERSION}){notes}";
            }
        }

        public static bool IsWindowOpen => Instance != null && Instance._modWindowGO != null && Instance._modWindowGO.activeSelf;

        public void ToggleModWindow()
        {
            if (_modWindowGO == null)
            {
                BuildCanvasUI();
            }
            if (_modWindowGO == null) return;
            bool open = !_modWindowGO.activeSelf;
            _modWindowGO.SetActive(open);
            ModGUI.IsOpen = false; // Keep single window

            if (open)
            {
                EnsureEventSystem();
                RefreshUpdateBanner();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                try
                {
                    Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None);
                }
                catch {}

                try
                {
                    if (CanvasHelper.ActiveMenu == MenuType.None)
                    {
                        CanvasHelper.ActiveMenu = MenuType.Cheat;
                    }
                }
                catch {}
            }
            else
            {
                try
                {
                    if (CanvasHelper.ActiveMenu == MenuType.Cheat)
                    {
                        CanvasHelper.ActiveMenu = MenuType.None;
                    }
                }
                catch {}

                var p = PlayerHelper.GetLocalPlayer();
                if (p != null)
                {
                    bool isOtherMenuOpen = false;
                    try
                    {
                        if (CanvasHelper.ActiveMenu != MenuType.None)
                        {
                            isOtherMenuOpen = true;
                        }
                    }
                    catch {}

                    if (!isOtherMenuOpen)
                    {
                        try
                        {
                            Helper.SetCursorVisibleAndLockState(false, CursorLockMode.Locked);
                        }
                        catch
                        {
                            Cursor.lockState = CursorLockMode.Locked;
                            Cursor.visible = false;
                        }
                    }
                    else
                    {
                        try
                        {
                            Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None);
                        }
                        catch {}
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                }
                else
                {
                    try
                    {
                        Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None);
                        Helper.CursorVisible = true;
                        Helper.SetCursorLockState(CursorLockMode.None);
                    }
                    catch {}
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
        // ============================================================================
        // [END] UPDATE LOOP & HOTKEY HANDLER
        // ============================================================================
        #endregion

        #region [START] WORLD TIME & WEATHER HELPERS
        // ============================================================================
        // [START] WORLD TIME & WEATHER HELPERS
        // ============================================================================
        public void SetHUDVisible(bool visible)
        {
            Plugin.EnableHUD.Value = visible;
        }

        private static void SetTime(float hour)
        {
            var sky = FindObjectOfType<UnityEngine.AzureSky.AzureSkyController>();
            if (sky?.timeOfDay != null)
            {
                sky.timeOfDay.GotoTime(hour);
            }
        }

        private static void SetWeather(UniqueWeatherType weather)
        {
            var wm = ComponentManager<WeatherManager>.Value ?? FindObjectOfType<WeatherManager>();
            if (wm != null)
            {
                wm.SetWeather(weather, true);
            }
        }
        // ============================================================================
        // [END] WORLD TIME & WEATHER HELPERS
        // ============================================================================
        #endregion
    }
}
