using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SailorsCompanion.Features;
using SailorsCompanion.Patches;

namespace SailorsCompanion.UI
{
    #region [START] CANVAS SAILOR'S COMPANION SETTINGS UI
    // ============================================================================
    // [START] CANVAS SAILOR'S COMPANION SETTINGS UI
    // Purpose: "Modern Clean" in-game settings canvas for Sailor's Companion - the same dark,
    //          flat, card-based design system already shared by Farmer's Companion, Inventory
    //          Master and Collection QoL. Replaces the original wooden-plank theme.
    //
    //          Structure: a left rail of 8 screens instead of the old 5 top tabs, each screen
    //          scrolling independently, so the window no longer has to fit everything in one
    //          fixed-height page. Survival Mode locks the three sandbox screens behind a lock
    //          card exactly as the wooden UI did.
    //
    //          Everything reads and writes the real ConfigEntry<T> objects on Plugin, so a
    //          change made here is saved to the .cfg and picked up by the feature managers on
    //          their next frame - there is no private copy of any setting in this file.
    // ============================================================================
    public class CanvasModUI : MonoBehaviour
    {
        public static CanvasModUI Instance { get; private set; }

        private GameObject _canvasGO;
        private Canvas _canvas;
        private CanvasScaler _scaler;
        private GraphicRaycaster _raycaster;
        private GameObject _rootGO;
        private GameObject _modWindowGO;
        private GameObject _screensHost;
        private Font _gameFont;

        // CursorPatchHelper, FlyController and the Mods Manager all read this.
        public static bool IsWindowOpen => Instance != null && Instance._rootGO != null && Instance._rootGO.activeSelf;

        #region [START] MODERN CLEAN PALETTE (shared across the mod family - do not change values)
        private static readonly Color ColBg           = new Color32(0x0F, 0x15, 0x18, 0xFF);
        private static readonly Color ColPanel        = new Color32(0x16, 0x1F, 0x24, 0xFF);
        private static readonly Color ColPanel2       = new Color32(0x1C, 0x27, 0x2D, 0xFF);
        private static readonly Color ColRow          = new Color32(0x1A, 0x24, 0x2A, 0xFF);
        private static readonly Color ColBorder       = new Color32(0x26, 0x33, 0x3B, 0xFF);
        private static readonly Color ColBorderSoft   = new Color32(0x1E, 0x29, 0x30, 0xFF);
        private static readonly Color ColText         = new Color32(0xEA, 0xF3, 0xF1, 0xFF);
        private static readonly Color ColTextMuted    = new Color32(0x8F, 0xA3, 0xA9, 0xFF);
        private static readonly Color ColTextFaint    = new Color32(0x5E, 0x73, 0x79, 0xFF);
        private static readonly Color ColAccent       = new Color32(0x2F, 0xC7, 0xB0, 0xFF);
        private static readonly Color ColAccentStrong = new Color32(0x20, 0xA7, 0x94, 0xFF);
        private static readonly Color ColAccentWash   = new Color(0x2F / 255f, 0xC7 / 255f, 0xB0 / 255f, 0.16f);
        private static readonly Color ColGold         = new Color32(0xE8, 0xB9, 0x4A, 0xFF);
        private static readonly Color ColGoldWash     = new Color(0xE8 / 255f, 0xB9 / 255f, 0x4A / 255f, 0.16f);
        private static readonly Color ColSuccess      = new Color32(0x5F, 0xBE, 0x7A, 0xFF);
        private static readonly Color ColSuccessWash  = new Color(0x5F / 255f, 0xBE / 255f, 0x7A / 255f, 0.16f);
        private static readonly Color ColDanger       = new Color32(0xE0, 0x5A, 0x5A, 0xFF);
        private static readonly Color ColDangerWash   = new Color(0xE0 / 255f, 0x5A / 255f, 0x5A / 255f, 0.16f);
        private static readonly Color ColOnAccentTxt  = new Color32(0x06, 0x23, 0x1F, 0xFF);
        #endregion [END] MODERN CLEAN PALETTE

        private const int SCREEN_COUNT = 8;
        // 0 Overview | 1 Survival & QoL | 2 Navigation | 3 Cheats | 4 Research | 5 Spawner | 6 Controls | 7 Updates
        private static readonly string[] ScreenLabels =
        {
            "Overview", "Survival & QoL", "Navigation", "Cheats & Sandbox",
            "Research", "Item Spawner", "Controls", "Updates"
        };
        private static readonly string[] ScreenMonograms = { "O", "S", "N", "C", "R", "I", "K", "U" };

        private readonly GameObject[] _screens    = new GameObject[SCREEN_COUNT];
        private readonly Button[]     _railButtons= new Button[SCREEN_COUNT];
        private readonly Image[]      _railBg     = new Image[SCREEN_COUNT];
        private readonly Text[]       _railTexts  = new Text[SCREEN_COUNT];
        private readonly Image[]      _railIcons  = new Image[SCREEN_COUNT];
        private readonly GameObject[] _railBars   = new GameObject[SCREEN_COUNT];
        private int _activeScreen = 0;

        // Screens whose content depends on Survival/Creative mode and so must be rebuilt when it flips.
        private static readonly int[] ModeDependentScreens = { 1, 3, 4, 5 };

        // ---------------- live widget references ----------------
        private Text _footerVerText;
        private Text _updateBadgeText;
        private Image _updateBadgeImg;
        private Text _railStatusText;

        private Image _modeSurvivalImg, _modeCreativeImg;
        private Text  _modeSurvivalTxt, _modeCreativeTxt;

        private static readonly string[] ProfileKeys   = { "VanillaPlus", "BalancedOP", "EasyMode", "Custom" };
        private static readonly string[] ProfileLabels = { "Vanilla+", "Balanced OP", "Easy Mode", "Custom" };
        private readonly Image[] _profileBtnImgs  = new Image[4];
        private readonly Text[]  _profileBtnTexts = new Text[4];

        private Text _ovStatusSurvival, _ovStatusNav, _ovStatusSandbox;
        private Text _ovModeText, _ovProfileText;

        private Text _teleHeadingText, _teleRaftText, _teleSharkText, _teleCoordsText;
        private Text _navSailModeBtnText, _navRecallBtnText, _navScannerBtnText, _qolMagnetBtnText;
        private readonly Image[] _navStyleImgs = new Image[4];
        private readonly Text[]  _navStyleTexts = new Text[4];

        private Text _researchStatusText;
        private Text _qolTooltipText;

        private InputField _itemSearchInput;
        private Transform _itemScrollContent;
        private List<Item_Base> _allItems;

        private float _lastTelemetryUpdate;
        private static Raft _cachedNavRaft;
        private static AI_StateMachine_Shark _cachedNavShark;
        private static Camera _cachedNavCamera;

        #region [START] UNITY LIFECYCLE
        #region [START] AWAKE
        private void Awake()
        {
            Instance = this;
            try
            {
                GetGameFont();
                BuildCanvasUI();
                StartCoroutine(PollUpdateStatus());
            }
            catch (Exception ex)
            {
                Debug.LogError("[Sailor's Companion] CanvasModUI.Awake() FAILED: " + ex);
            }
        }
        #endregion [END] AWAKE

        #region [START] GET GAME FONT
        public Font GetGameFont()
        {
            if (_gameFont != null) return _gameFont;

            try
            {
                _gameFont = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI Semibold", "Segoe UI", "Arial", "Tahoma" }, 24);
            }
            catch { }

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

            _gameFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return _gameFont;
        }
        #endregion [END] GET GAME FONT

        #region [START] ENSURE EVENT SYSTEM
        private void EnsureEventSystem()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null)
            {
                var existing = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (existing != null)
                {
                    UnityEngine.EventSystems.EventSystem.current = existing;
                    es = existing;
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

            if (es != null)
            {
                if (!es.enabled) es.enabled = true;
                if (!es.gameObject.activeInHierarchy) es.gameObject.SetActive(true);
                es.SetSelectedGameObject(null);
            }
        }
        #endregion [END] ENSURE EVENT SYSTEM

        #region [START] UPDATE
        private void Update()
        {
            try
            {
                if (_canvasGO == null || _rootGO == null)
                {
                    BuildCanvasUI();
                }

                KeyCode keyMenu = Plugin.KeyMenu != null ? Plugin.KeyMenu.Value : KeyCode.F5;
                if (InputHelper.WasKeyPressed(keyMenu) || InputHelper.WasKeyPressed(KeyCode.Insert))
                {
                    ToggleModWindow();
                }
                if (InputHelper.WasKeyPressed(KeyCode.Escape) && IsWindowOpen)
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
                    else if (Plugin.EnableHUD != null)
                    {
                        Plugin.EnableHUD.Value = !Plugin.EnableHUD.Value;
                    }
                }

                KeyCode keyFly = Plugin.KeyFly != null ? Plugin.KeyFly.Value : KeyCode.F;
                if (InputHelper.WasKeyPressed(keyFly))
                {
                    if (Plugin.IsSurvivalMode)
                    {
                        TeleportManager.SetNotification("Fly / NoClip is locked in Survival Mode. Switch to Creative Mode in the [" + keyMenu + "] menu.");
                    }
                    else if (Plugin.EnableFlyMode != null)
                    {
                        Plugin.EnableFlyMode.Value = !Plugin.EnableFlyMode.Value;
                        TeleportManager.SetNotification(Plugin.EnableFlyMode.Value ? "Fly / NoClip: ON" : "Fly / NoClip: OFF");
                    }
                }

                // Direct gameplay keys, opt-in (Plugin.EnableHotkeys defaults to false).
                if (Plugin.EnableHotkeys != null && Plugin.EnableHotkeys.Value)
                {
                    KeyCode keySails = Plugin.KeySailToggle != null ? Plugin.KeySailToggle.Value : KeyCode.F4;
                    if (InputHelper.WasKeyPressed(keySails)) BoatController.ToggleAllSails();

                    KeyCode keyEngines = Plugin.KeyEngineToggle != null ? Plugin.KeyEngineToggle.Value : KeyCode.F11;
                    if (InputHelper.WasKeyPressed(keyEngines)) BoatController.ToggleAllEngines();

                    KeyCode keyMagnet = Plugin.KeyMagnetToggle != null ? Plugin.KeyMagnetToggle.Value : KeyCode.F7;
                    if (InputHelper.WasKeyPressed(keyMagnet)) MagneticCollector.ToggleMagnet();

                    KeyCode keyScan = Plugin.KeyScannerPulse != null ? Plugin.KeyScannerPulse.Value : KeyCode.F10;
                    if (InputHelper.WasKeyPressed(keyScan)) ItemDetector.TriggerPulseScan();
                }

                KeyCode keyTeleRaft = Plugin.KeyTeleportToRaft != null ? Plugin.KeyTeleportToRaft.Value : KeyCode.F8;
                if (InputHelper.WasKeyPressed(keyTeleRaft)) TeleportManager.TeleportPlayerToRaft();

                KeyCode keySummon = Plugin.KeyTeleportRaftToPlayer != null ? Plugin.KeyTeleportRaftToPlayer.Value : KeyCode.F9;
                if (InputHelper.WasKeyPressed(keySummon)) TeleportManager.TeleportRaftToPlayer();

                if (IsWindowOpen)
                {
                    // Raft re-asserts cursor state every frame; without re-applying it here the
                    // pointer disappears mid-click while the menu is up.
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    try
                    {
                        Helper.CursorVisible = true;
                        Helper.SetCursorLockState(CursorLockMode.None);
                    }
                    catch { }

                    // Live telemetry, throttled to 5 Hz - rebuilding these strings every frame
                    // is pure allocation churn for numbers a player cannot read that fast.
                    if (_activeScreen == 2 && Time.unscaledTime - _lastTelemetryUpdate > 0.2f)
                    {
                        _lastTelemetryUpdate = Time.unscaledTime;
                        RefreshTelemetry();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Sailor's Companion] Error in CanvasModUI.Update: " + ex.Message);
            }
        }
        #endregion [END] UPDATE

        #region [START] TOGGLE MOD WINDOW
        public void ToggleModWindow()
        {
            if (_rootGO == null) BuildCanvasUI();
            if (_rootGO == null) return;

            if (_rootGO.activeSelf) CloseWindow();
            else OpenWindow();
        }
        #endregion [END] TOGGLE MOD WINDOW

        #region [START] OPEN WINDOW
        private void OpenWindow()
        {
            EnsureEventSystem();
            if (_raycaster != null && !_raycaster.enabled) _raycaster.enabled = true;
            _rootGO.SetActive(true);

            // Raft drives input through Unity's New Input System: without switching to the "UI"
            // action map the menu's buttons never reliably receive clicks.
            try
            {
                var cic = CustomInputConfig.Instance;
                if (cic != null)
                {
                    cic.EnableInput();
                    cic.SwitchCurrentActionMap("UI");
                }
            }
            catch { }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            try { Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None); }
            catch { }

            // Marks a menu as active so the game stops raycasting world interactions underneath -
            // without it you place blocks and hit things through the open menu.
            try
            {
                if (CanvasHelper.ActiveMenu == MenuType.None) CanvasHelper.ActiveMenu = MenuType.Cheat;
            }
            catch { }

            RefreshUpdateBadge();
            RefreshOverview();
            RefreshTelemetry();
            SelectScreen(_activeScreen);

            // Unity never computes layout for a hierarchy built while inactive, and does not
            // recalculate it later just because the object became active. The whole window is
            // built once behind an inactive root, so force one rebuild on every open.
            if (_modWindowGO != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_modWindowGO.GetComponent<RectTransform>());
            }
        }
        #endregion [END] OPEN WINDOW

        #region [START] CLOSE WINDOW
        private void CloseWindow()
        {
            _rootGO.SetActive(false);

            // A sibling mod's menu may still be open behind this one - re-locking the cursor then
            // would leave that menu unusable. ShouldForceCursorFree() already knows about every peer.
            bool peerOpen = false;
            try { peerOpen = CursorPatchHelper.ShouldForceCursorFree(); }
            catch { }

            try
            {
                if (CanvasHelper.ActiveMenu == MenuType.Cheat && !peerOpen)
                {
                    CanvasHelper.ActiveMenu = MenuType.None;
                }
            }
            catch { }

            if (peerOpen)
            {
                // Leave the cursor free for whichever menu is still up.
                return;
            }

            var p = PlayerHelper.GetLocalPlayer();
            bool inGame = p != null;

            // Only gameplay has a "Player" action map to return to - switching to it from the main
            // menu leaves the home screen's own buttons unable to receive clicks.
            try
            {
                var cic = CustomInputConfig.Instance;
                if (cic != null) cic.SwitchCurrentActionMap(inGame ? "Player" : "UI");
            }
            catch { }

            if (inGame)
            {
                try { Helper.SetCursorVisibleAndLockState(false, CursorLockMode.Locked); }
                catch
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            else
            {
                try { Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None); }
                catch { }
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        #endregion [END] CLOSE WINDOW
        #endregion [END] UNITY LIFECYCLE

        #region [START] CANVAS CONSTRUCTION
        #region [START] BUILD CANVAS UI
        private void BuildCanvasUI()
        {
            if (_canvasGO != null && _rootGO != null) return;

            if (_canvasGO == null)
            {
                _canvasGO = new GameObject("SailorsCompanion_Canvas");
                _canvasGO.hideFlags = HideFlags.HideAndDontSave;
                _canvasGO.layer = LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5;
                DontDestroyOnLoad(_canvasGO);

                _canvas = _canvasGO.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.overrideSorting = true;
                _canvas.sortingOrder = 33000; // top-most, matches the sibling mods' menu layer

                _scaler = _canvasGO.AddComponent<CanvasScaler>();
                _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _scaler.referenceResolution = new Vector2(1920, 1080);
                _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                _scaler.matchWidthOrHeight = 0.5f;

                _raycaster = _canvasGO.AddComponent<GraphicRaycaster>();
            }

            if (_rootGO == null)
            {
                BuildWindow();
                SetLayerRecursively(_canvasGO, LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5);
                _rootGO.SetActive(false);
            }
        }
        #endregion [END] BUILD CANVAS UI

        #region [START] BUILD WINDOW
        private void BuildWindow()
        {
            _rootGO = new GameObject("Root_SailorsCompanion");
            _rootGO.transform.SetParent(_canvasGO.transform, false);
            var rootRt = _rootGO.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            var dimmerGO = new GameObject("Dimmer");
            dimmerGO.transform.SetParent(_rootGO.transform, false);
            var dimmerRt = dimmerGO.AddComponent<RectTransform>();
            dimmerRt.anchorMin = Vector2.zero;
            dimmerRt.anchorMax = Vector2.one;
            dimmerRt.offsetMin = Vector2.zero;
            dimmerRt.offsetMax = Vector2.zero;
            var dimmerImg = dimmerGO.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.6f);
            dimmerImg.raycastTarget = false;

            _modWindowGO = new GameObject("SailorsCompanion_Window");
            _modWindowGO.transform.SetParent(_rootGO.transform, false);
            var winRt = _modWindowGO.AddComponent<RectTransform>();
            winRt.anchorMin = new Vector2(0.5f, 0.5f);
            winRt.anchorMax = new Vector2(0.5f, 0.5f);
            winRt.pivot = new Vector2(0.5f, 0.5f);
            winRt.sizeDelta = new Vector2(1320, 760);
            winRt.anchoredPosition = Vector2.zero;

            var winImg = _modWindowGO.AddComponent<Image>();
            winImg.sprite = GetRoundedSprite(18);
            winImg.type = Image.Type.Sliced;
            winImg.color = ColBorder;
            AddInsetFill(_modWindowGO, 18, ColPanel);

            var railGO = BuildRail();
            railGO.transform.SetParent(_modWindowGO.transform, false);
            var railRt = railGO.GetComponent<RectTransform>();
            railRt.anchorMin = new Vector2(0, 0);
            railRt.anchorMax = new Vector2(0, 1);
            railRt.pivot = new Vector2(0, 0.5f);
            railRt.sizeDelta = new Vector2(248, 0);
            railRt.anchoredPosition = Vector2.zero;

            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(_modWindowGO.transform, false);
            var contentRt = contentGO.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 0);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.offsetMin = new Vector2(248, 0);
            contentRt.offsetMax = Vector2.zero;

            BuildFooter(contentGO);

            _screensHost = new GameObject("Screens");
            _screensHost.transform.SetParent(contentGO.transform, false);
            var screensRt = _screensHost.AddComponent<RectTransform>();
            screensRt.anchorMin = Vector2.zero;
            screensRt.anchorMax = Vector2.one;
            screensRt.offsetMin = new Vector2(0, 52);
            screensRt.offsetMax = Vector2.zero;

            BuildAllScreens();
            SelectScreen(0);
            BuildCloseButton();
        }
        #endregion [END] BUILD WINDOW

        #region [START] BUILD ALL SCREENS
        private void BuildAllScreens()
        {
            _screens[0] = BuildScreenOverview(_screensHost);
            _screens[1] = BuildScreenSurvival(_screensHost);
            _screens[2] = BuildScreenNavigation(_screensHost);
            _screens[3] = BuildScreenCheats(_screensHost);
            _screens[4] = BuildScreenResearch(_screensHost);
            _screens[5] = BuildScreenSpawner(_screensHost);
            _screens[6] = BuildScreenControls(_screensHost);
            _screens[7] = BuildScreenUpdates(_screensHost);
        }
        #endregion [END] BUILD ALL SCREENS

        #region [START] BUILD CLOSE BUTTON
        private void BuildCloseButton()
        {
            var go = new GameObject("CloseBtn");
            go.transform.SetParent(_modWindowGO.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(32, 32);
            rt.anchoredPosition = new Vector2(-16, -16);

            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(16);
            img.type = Image.Type.Sliced;
            img.color = ColPanel2;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor = ColPanel2;
            cb.highlightedColor = ColDanger;
            cb.pressedColor = ColDanger;
            btn.colors = cb;
            btn.onClick.AddListener(CloseWindow);

            var txt = CreateText(go, "✕", 15, FontStyle.Bold, ColTextMuted, TextAnchor.MiddleCenter);
            txt.raycastTarget = false;
            FillParent(txt.gameObject);
        }
        #endregion [END] BUILD CLOSE BUTTON

        // ---------------- RAIL (left navigation) ----------------
        #region [START] BUILD RAIL
        private GameObject BuildRail()
        {
            var railGO = new GameObject("Rail");
            var railImg = railGO.AddComponent<Image>();
            railImg.color = ColPanel2;

            var railLayout = railGO.AddComponent<VerticalLayoutGroup>();
            railLayout.padding = new RectOffset(14, 14, 20, 16);
            railLayout.spacing = 2;
            railLayout.childForceExpandWidth = true;
            railLayout.childForceExpandHeight = false;
            railLayout.childControlWidth = true;
            railLayout.childControlHeight = true;

            var brandGO = new GameObject("Brand");
            brandGO.transform.SetParent(railGO.transform, false);
            var brandLe = brandGO.AddComponent<LayoutElement>();
            brandLe.preferredHeight = 66;
            // A child whose OWN inner HorizontalLayoutGroup sets childForceExpandHeight = true
            // reports flexibleHeight = 1 to its parent (the group forces every one of its children
            // to at least 1 flexible unit on the cross axis, then advertises that total upward).
            // Without pinning it to 0 here, the rail's VerticalLayoutGroup hands surplus height to
            // this row and every nav item instead of only to the dedicated Spacer, inflating them
            // far past their preferredHeight. LayoutElement outranks a LayoutGroup, so 0 wins.
            brandLe.flexibleHeight = 0;
            var brandLayout = brandGO.AddComponent<HorizontalLayoutGroup>();
            brandLayout.childControlWidth = true;
            brandLayout.childControlHeight = true;
            brandLayout.spacing = 10;
            brandLayout.childAlignment = TextAnchor.MiddleLeft;
            brandLayout.childForceExpandWidth = false;
            brandLayout.childForceExpandHeight = true;

            var markGO = new GameObject("Mark");
            markGO.transform.SetParent(brandGO.transform, false);
            var markLe = markGO.AddComponent<LayoutElement>();
            markLe.preferredWidth = 38; markLe.preferredHeight = 38;
            var markImg = markGO.AddComponent<Image>();
            markImg.sprite = GetRoundedSprite(9);
            markImg.type = Image.Type.Sliced;
            markImg.color = ColAccent;
            var markTxt = CreateText(markGO, "S", 18, FontStyle.Bold, ColOnAccentTxt, TextAnchor.MiddleCenter);
            FillParent(markTxt.gameObject);

            var brandTextGO = new GameObject("BrandText");
            brandTextGO.transform.SetParent(brandGO.transform, false);
            var btLe = brandTextGO.AddComponent<LayoutElement>();
            btLe.flexibleWidth = 1f;
            var btLayout = brandTextGO.AddComponent<VerticalLayoutGroup>();
            btLayout.childControlWidth = true;
            btLayout.childControlHeight = true;
            btLayout.childForceExpandWidth = true;
            btLayout.childForceExpandHeight = false;
            btLayout.spacing = 1;

            var nameTxt = CreateText(brandTextGO, "Sailor's Companion", 16, FontStyle.Bold, ColText, TextAnchor.MiddleLeft);
            nameTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            nameTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 17;

            var subGO = new GameObject("Sub");
            subGO.transform.SetParent(brandTextGO.transform, false);
            var subLe = subGO.AddComponent<LayoutElement>();
            subLe.preferredHeight = 15;
            var subTxt = CreateText(subGO, "Survival, Raft & Sandbox", 13, FontStyle.Normal, ColTextFaint, TextAnchor.MiddleLeft);
            subTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(subTxt.gameObject);

            AddDivider(railGO.transform, 18);

            for (int i = 0; i < SCREEN_COUNT; i++)
            {
                int idx = i;
                var itemGO = BuildRailItem(idx, ScreenLabels[i], ScreenMonograms[i], () => SelectScreen(idx));
                itemGO.transform.SetParent(railGO.transform, false);
                var itemLe = itemGO.AddComponent<LayoutElement>();
                itemLe.preferredHeight = 42;
                itemLe.flexibleHeight = 0; // see Brand above - keeps nav items at 42, not stretched
                _railButtons[i] = itemGO.GetComponent<Button>();
            }

            var spacerGO = new GameObject("Spacer");
            spacerGO.transform.SetParent(railGO.transform, false);
            var spacerLe = spacerGO.AddComponent<LayoutElement>();
            spacerLe.flexibleHeight = 1f;

            AddDivider(railGO.transform, 10);

            var statusGO = new GameObject("Status");
            statusGO.transform.SetParent(railGO.transform, false);
            var statusLe = statusGO.AddComponent<LayoutElement>();
            statusLe.preferredHeight = 26;
            statusLe.flexibleHeight = 0; // see Brand above
            var statusLayout = statusGO.AddComponent<HorizontalLayoutGroup>();
            statusLayout.childControlWidth = true;
            statusLayout.childControlHeight = true;
            statusLayout.spacing = 7;
            statusLayout.padding = new RectOffset(6, 0, 0, 0);
            statusLayout.childAlignment = TextAnchor.MiddleLeft;
            statusLayout.childForceExpandWidth = false;
            statusLayout.childForceExpandHeight = true;

            var dotGO = new GameObject("Dot");
            dotGO.transform.SetParent(statusGO.transform, false);
            var dotLe = dotGO.AddComponent<LayoutElement>();
            dotLe.preferredWidth = 6; dotLe.preferredHeight = 6;
            var dotImg = dotGO.AddComponent<Image>();
            dotImg.sprite = GetRoundedSprite(3);
            dotImg.type = Image.Type.Sliced;
            dotImg.color = ColAccent;

            _railStatusText = CreateText(statusGO, ModeName() + " Mode", 15f, FontStyle.Normal, ColTextFaint, TextAnchor.MiddleLeft);
            _railStatusText.horizontalOverflow = HorizontalWrapMode.Overflow;
            var stLe = _railStatusText.gameObject.AddComponent<LayoutElement>();
            stLe.flexibleWidth = 1f;

            return railGO;
        }
        #endregion [END] BUILD RAIL

        #region [START] BUILD RAIL ITEM
        private GameObject BuildRailItem(int index, string label, string monogram, Action onClick)
        {
            var go = new GameObject("Rail_" + label);
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(9);
            img.type = Image.Type.Sliced;
            img.color = Color.clear;
            if (index >= 0) _railBg[index] = img;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(10, 8, 0, 0);
            layout.spacing = 9;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            var barGO = new GameObject("Bar");
            barGO.transform.SetParent(go.transform, false);
            var barRt = barGO.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0, 0.5f);
            barRt.anchorMax = new Vector2(0, 0.5f);
            barRt.pivot = new Vector2(0.5f, 0.5f);
            barRt.sizeDelta = new Vector2(3, 16);
            barRt.anchoredPosition = new Vector2(-11, 0);
            var barImg = barGO.AddComponent<Image>();
            barImg.sprite = GetRoundedSprite(2);
            barImg.type = Image.Type.Sliced;
            barImg.color = ColAccent;
            // A manually anchored decorative child must opt out or the parent
            // HorizontalLayoutGroup overwrites the anchors set above.
            barGO.AddComponent<LayoutElement>().ignoreLayout = true;
            barGO.SetActive(false);
            if (index >= 0) _railBars[index] = barGO;

            var monoGO = new GameObject("Mono");
            monoGO.transform.SetParent(go.transform, false);
            var monoLe = monoGO.AddComponent<LayoutElement>();
            monoLe.preferredWidth = 20; monoLe.preferredHeight = 20;
            var monoImg = monoGO.AddComponent<Image>();
            monoImg.sprite = GetRoundedSprite(6);
            monoImg.type = Image.Type.Sliced;
            monoImg.color = ColBorderSoft;
            if (index >= 0) _railIcons[index] = monoImg;
            var monoTxt = CreateText(monoGO, monogram, 13, FontStyle.Bold, ColTextMuted, TextAnchor.MiddleCenter);
            FillParent(monoTxt.gameObject);

            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(go.transform, false);
            var lblLe = lblGO.AddComponent<LayoutElement>();
            lblLe.flexibleWidth = 1f;
            var lblTxt = CreateText(lblGO, label, 15f, FontStyle.Normal, ColTextMuted, TextAnchor.MiddleLeft);
            lblTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(lblTxt.gameObject);
            if (index >= 0) _railTexts[index] = lblTxt;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => onClick());

            return go;
        }
        #endregion [END] BUILD RAIL ITEM

        #region [START] SELECT SCREEN
        public void SelectScreen(int index)
        {
            _activeScreen = index;
            for (int i = 0; i < SCREEN_COUNT; i++)
            {
                bool active = (i == index);
                if (_screens[i] != null) _screens[i].SetActive(active);
                if (_railBg[i] != null) _railBg[i].color = active ? ColAccentWash : Color.clear;
                if (_railTexts[i] != null) _railTexts[i].color = active ? ColText : ColTextMuted;
                if (_railIcons[i] != null) _railIcons[i].color = active ? ColAccent : ColBorderSoft;
                if (_railBars[i] != null) _railBars[i].SetActive(active);
            }

            if (index == 0) RefreshOverview();
            if (index == 2) RefreshTelemetry();
            if (index == 7) RefreshUpdateBadge();

            if (_screens[index] != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_screens[index].GetComponent<RectTransform>());
            }
        }
        #endregion [END] SELECT SCREEN
        #endregion [END] CANVAS CONSTRUCTION

        #region [START] GAME MODE & PROFILES
        #region [START] MODE NAME
        private static string ModeName()
        {
            return Plugin.IsCreativeMode ? "Creative" : "Survival";
        }
        #endregion [END] MODE NAME

        #region [START] SET MOD MODE
        private void SetModMode(string newMode)
        {
            if (Plugin.ModGameMode != null && Plugin.ModGameMode.Value == newMode) return;
            if (Plugin.ModGameMode != null) Plugin.ModGameMode.Value = newMode;

            // Switching back to Survival must actually disable the Creative-only cheats, not just
            // re-lock their screen. Otherwise a cheat enabled while in Creative keeps running.
            if (newMode == "Survival")
            {
                if (Plugin.GodMode != null) Plugin.GodMode.Value = false;
                if (Plugin.OneHitKill != null) Plugin.OneHitKill.Value = false;
                if (Plugin.InfiniteOxygen != null) Plugin.InfiniteOxygen.Value = false;
                if (Plugin.NoHungerThirst != null) Plugin.NoHungerThirst.Value = false;
                if (Plugin.EnableFlyMode != null) Plugin.EnableFlyMode.Value = false;
                if (Plugin.FreeCrafting != null) Plugin.FreeCrafting.Value = false;
            }

            RebuildModeDependentScreens();
            UpdateModeButtonVisuals();
            RefreshOverview();

            TeleportManager.SetNotification(Plugin.IsCreativeMode
                ? "Creative Mode active: cheats, research and the item spawner are unlocked."
                : "Survival Mode active: balanced QoL only, sandbox screens locked.");
        }
        #endregion [END] SET MOD MODE

        #region [START] REBUILD MODE DEPENDENT SCREENS
        private void RebuildModeDependentScreens()
        {
            if (_screensHost == null) return;

            foreach (int i in ModeDependentScreens)
            {
                if (_screens[i] != null) Destroy(_screens[i]);
            }

            _screens[1] = BuildScreenSurvival(_screensHost);
            _screens[3] = BuildScreenCheats(_screensHost);
            _screens[4] = BuildScreenResearch(_screensHost);
            _screens[5] = BuildScreenSpawner(_screensHost);

            SelectScreen(_activeScreen);
        }
        #endregion [END] REBUILD MODE DEPENDENT SCREENS

        #region [START] UPDATE MODE BUTTON VISUALS
        private void UpdateModeButtonVisuals()
        {
            bool creative = Plugin.IsCreativeMode;

            if (_modeSurvivalImg != null) _modeSurvivalImg.color = creative ? ColPanel2 : ColSuccessWash;
            if (_modeCreativeImg != null) _modeCreativeImg.color = creative ? ColGoldWash : ColPanel2;
            if (_modeSurvivalTxt != null) _modeSurvivalTxt.color = creative ? ColTextMuted : ColSuccess;
            if (_modeCreativeTxt != null) _modeCreativeTxt.color = creative ? ColGold : ColTextMuted;
            if (_railStatusText != null) _railStatusText.text = ModeName() + " Mode";
            if (_ovModeText != null)
            {
                _ovModeText.text = creative ? "Creative" : "Survival";
                _ovModeText.color = creative ? ColGold : ColSuccess;
            }
        }
        #endregion [END] UPDATE MODE BUTTON VISUALS

        #region [START] APPLY PROFILE
        private void ApplyProfile(string profileName)
        {
            if (string.IsNullOrEmpty(profileName)) return;
            if (Plugin.ActiveProfile != null) Plugin.ActiveProfile.Value = profileName;

            if (profileName == "VanillaPlus")
            {
                SetProfileSettings(40, 1.0f, 1.0f, 1.0f, 1.0f, false, true, false, false, true);
                TeleportManager.SetNotification("Vanilla+ profile applied: authentic balance, QoL only.");
            }
            else if (profileName == "BalancedOP" || profileName == "CozyFarming")
            {
                SetProfileSettings(100, 1.5f, 1.5f, 1.2f, 1.2f, true, true, true, true, true);
                TeleportManager.SetNotification("Balanced OP profile applied: stronger multipliers, all automations on.");
            }
            else if (profileName == "EasyMode" || profileName == "MasterBuilder")
            {
                SetProfileSettings(200, 2.5f, 2.0f, 1.5f, 1.5f, true, true, true, true, true);
                TeleportManager.SetNotification("Easy Mode profile applied: maximum multipliers for relaxed play.");
            }
            // "Custom" intentionally changes nothing - it is the state you land in after
            // touching any individual setting, not a preset of its own.

            try { Plugin.Instance?.Config?.Save(); }
            catch (Exception ex) { Debug.LogWarning("[Sailor's Companion] Error saving profile: " + ex.Message); }

            // Rebuild the Survival screen so every control shows the new values.
            if (_screensHost != null)
            {
                if (_screens[1] != null) Destroy(_screens[1]);
                _screens[1] = BuildScreenSurvival(_screensHost);
                SelectScreen(_activeScreen);
            }

            UpdateProfileButtonVisuals();
            RefreshOverview();
        }
        #endregion [END] APPLY PROFILE

        #region [START] SET PROFILE SETTINGS
        private static void SetProfileSettings(int stackSize, float weaponDamage, float hookSpeed,
                                               float swimSpeed, float sprintSpeed, bool autoNets,
                                               bool craftFromStorage, bool antiShark,
                                               bool infiniteDurability, bool animalHealthBars)
        {
            if (Plugin.CustomStackSize != null) Plugin.CustomStackSize.Value = stackSize;
            if (Plugin.WeaponDamageMultiplier != null) Plugin.WeaponDamageMultiplier.Value = weaponDamage;
            if (Plugin.HookPullSpeedMultiplier != null) Plugin.HookPullSpeedMultiplier.Value = hookSpeed;
            if (Plugin.SwimSpeedMultiplier != null) Plugin.SwimSpeedMultiplier.Value = swimSpeed;
            if (Plugin.SprintSpeedMultiplier != null) Plugin.SprintSpeedMultiplier.Value = sprintSpeed;
            if (Plugin.AutoEmptyCollectionNets != null) Plugin.AutoEmptyCollectionNets.Value = autoNets;
            if (Plugin.CraftFromStorage != null) Plugin.CraftFromStorage.Value = craftFromStorage;
            if (Plugin.AntiSharkRaftDamage != null) Plugin.AntiSharkRaftDamage.Value = antiShark;
            if (Plugin.InfiniteDurability != null) Plugin.InfiniteDurability.Value = infiniteDurability;
            if (Plugin.ShowAnimalHealthBars != null) Plugin.ShowAnimalHealthBars.Value = animalHealthBars;
        }
        #endregion [END] SET PROFILE SETTINGS

        #region [START] UPDATE PROFILE BUTTON VISUALS
        private void UpdateProfileButtonVisuals()
        {
            string active = Plugin.ActiveProfile != null ? Plugin.ActiveProfile.Value : "Custom";

            for (int i = 0; i < _profileBtnImgs.Length; i++)
            {
                if (_profileBtnImgs[i] == null) continue;
                bool sel = ProfileKeys[i] == active
                        || (ProfileKeys[i] == "BalancedOP" && active == "CozyFarming")
                        || (ProfileKeys[i] == "EasyMode" && active == "MasterBuilder");

                _profileBtnImgs[i].color = sel ? ColAccentWash : ColPanel2;
                if (_profileBtnTexts[i] != null)
                {
                    _profileBtnTexts[i].color = sel ? ColAccent : ColTextMuted;
                    _profileBtnTexts[i].fontStyle = sel ? FontStyle.Bold : FontStyle.Normal;
                }
            }

            if (_ovProfileText != null) _ovProfileText.text = ProfileDisplayName(active);
        }
        #endregion [END] UPDATE PROFILE BUTTON VISUALS

        #region [START] PROFILE DISPLAY NAME
        private static string ProfileDisplayName(string key)
        {
            for (int i = 0; i < ProfileKeys.Length; i++)
            {
                if (ProfileKeys[i] == key) return ProfileLabels[i];
            }
            if (key == "CozyFarming") return "Balanced OP";
            if (key == "MasterBuilder") return "Easy Mode";
            return "Custom";
        }
        #endregion [END] PROFILE DISPLAY NAME

        #region [START] MARK PROFILE CUSTOM
        // Any individual setting the player changes by hand takes them off the preset.
        private void MarkProfileCustom()
        {
            if (Plugin.ActiveProfile != null && Plugin.ActiveProfile.Value != "Custom")
            {
                Plugin.ActiveProfile.Value = "Custom";
                UpdateProfileButtonVisuals();
            }
            RefreshOverview();
        }
        #endregion [END] MARK PROFILE CUSTOM

        #region [START] SET QOL TOOLTIP
        public void SetQoLTooltip(string text)
        {
            if (_qolTooltipText != null) _qolTooltipText.text = text;
        }
        #endregion [END] SET QOL TOOLTIP
        #endregion [END] GAME MODE & PROFILES

        #region [START] SCREEN: OVERVIEW
        #region [START] BUILD SCREEN OVERVIEW
        private GameObject BuildScreenOverview(GameObject parent)
        {
            var screen = CreateScreenShell(parent, "Overview", "Game mode, preset profile, and everything Sailor's Companion is doing right now.", null, out var body);

            AddGroupLabel(body, "Game Mode");
            var modeCard = CreateCard(body);
            BuildModeRow(modeCard);

            AddGroupLabel(body, "Preset Profile");
            var profCard = CreateCard(body);
            BuildProfileRow(profCard);

            AddGroupLabel(body, "At a Glance");
            var statRowGO = new GameObject("StatGrid");
            statRowGO.transform.SetParent(body.transform, false);
            statRowGO.AddComponent<LayoutElement>().preferredHeight = 86;
            var statLayout = statRowGO.AddComponent<HorizontalLayoutGroup>();
            statLayout.childControlWidth = true;
            statLayout.childControlHeight = true;
            statLayout.spacing = 12;
            statLayout.childForceExpandWidth = true;
            statLayout.childForceExpandHeight = true;

            _ovModeText = BuildStatTile(statRowGO.transform, "Mode");
            _ovProfileText = BuildStatTile(statRowGO.transform, "Profile");

            AddGroupLabel(body, "Categories");
            var catRowGO = new GameObject("CatGrid");
            catRowGO.transform.SetParent(body.transform, false);
            catRowGO.AddComponent<LayoutElement>().preferredHeight = 132;
            var catLayout = catRowGO.AddComponent<HorizontalLayoutGroup>();
            catLayout.childControlWidth = true;
            catLayout.childControlHeight = true;
            catLayout.spacing = 12;
            catLayout.childForceExpandWidth = true;
            catLayout.childForceExpandHeight = true;

            BuildCategoryCard(catRowGO.transform, "Survival & QoL",
                "Craft from storage, nets, island and reef harvesting, raft defense, multipliers.",
                out _ovStatusSurvival, () => SelectScreen(1));
            BuildCategoryCard(catRowGO.transform, "Navigation",
                "Compass HUD, shark sonar, raft telemetry, sails, engines and teleports.",
                out _ovStatusNav, () => SelectScreen(2));
            BuildCategoryCard(catRowGO.transform, "Sandbox",
                "God mode, fly, free crafting, research unlocks and the 300+ item spawner.",
                out _ovStatusSandbox, () => SelectScreen(3));

            AddCallout(body, "Using our other mods?",
                "Farmer's Companion, Inventory Master and Collection QoL share some features with this mod. Check the settings in each one so a feature is only turned on in a single place.");

            return screen;
        }
        #endregion [END] BUILD SCREEN OVERVIEW

        #region [START] BUILD MODE ROW
        private void BuildModeRow(GameObject card)
        {
            var rowGO = CreateRowShell(card, "M", "Game Mode",
                "Survival keeps the mod to balanced quality-of-life. Creative unlocks cheats, research unlocks and the item spawner.",
                false, out _);

            var segGO = new GameObject("Segmented");
            segGO.transform.SetParent(rowGO.transform, false);
            segGO.AddComponent<LayoutElement>().preferredWidth = 230;
            var segLayout = segGO.AddComponent<HorizontalLayoutGroup>();
            segLayout.childControlWidth = true;
            segLayout.childControlHeight = true;
            segLayout.spacing = 8;
            segLayout.childAlignment = TextAnchor.MiddleRight;
            segLayout.childForceExpandWidth = true;
            segLayout.childForceExpandHeight = false;

            _modeSurvivalImg = BuildSegmentButton(segGO.transform, "Survival", out _modeSurvivalTxt, () => SetModMode("Survival"));
            _modeCreativeImg = BuildSegmentButton(segGO.transform, "Creative", out _modeCreativeTxt, () => SetModMode("Creative"));

            UpdateModeButtonVisuals();
        }
        #endregion [END] BUILD MODE ROW

        #region [START] BUILD SEGMENT BUTTON
        private Image BuildSegmentButton(Transform parent, string label, out Text labelText, Action onClick)
        {
            var go = new GameObject("Seg_" + label);
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 34;
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(8);
            img.type = Image.Type.Sliced;
            img.color = ColPanel2;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => onClick());

            labelText = CreateText(go, label, 15f, FontStyle.Bold, ColTextMuted, TextAnchor.MiddleCenter);
            labelText.raycastTarget = false;
            FillParent(labelText.gameObject);
            return img;
        }
        #endregion [END] BUILD SEGMENT BUTTON

        #region [START] BUILD PROFILE ROW
        private void BuildProfileRow(GameObject card)
        {
            var rowGO = CreateRowShell(card, "P", "Preset Profile",
                "One click sets stack size, weapon damage, hook and movement speeds, and the QoL automations. Changing any single setting afterwards switches you to Custom.",
                true, out _);

            var segGO = new GameObject("Profiles");
            segGO.transform.SetParent(rowGO.transform, false);
            segGO.AddComponent<LayoutElement>().preferredWidth = 400;
            var segLayout = segGO.AddComponent<HorizontalLayoutGroup>();
            segLayout.childControlWidth = true;
            segLayout.childControlHeight = true;
            segLayout.spacing = 6;
            segLayout.childAlignment = TextAnchor.MiddleRight;
            segLayout.childForceExpandWidth = true;
            segLayout.childForceExpandHeight = false;

            for (int i = 0; i < ProfileKeys.Length; i++)
            {
                int idx = i;
                string key = ProfileKeys[i];
                Text txt;
                // "Custom" is a state, not a preset: clicking it must not re-apply anything,
                // it only marks that the values no longer match a named profile.
                _profileBtnImgs[i] = BuildSegmentButton(segGO.transform, ProfileLabels[i], out txt, () =>
                {
                    if (key == "Custom")
                    {
                        if (Plugin.ActiveProfile != null) Plugin.ActiveProfile.Value = "Custom";
                        UpdateProfileButtonVisuals();
                    }
                    else
                    {
                        ApplyProfile(key);
                    }
                });
                _profileBtnTexts[i] = txt;
            }

            UpdateProfileButtonVisuals();
        }
        #endregion [END] BUILD PROFILE ROW

        #region [START] BUILD CATEGORY CARD
        private GameObject BuildCategoryCard(Transform parent, string title, string desc, out Text statusText, Action onClick)
        {
            var go = new GameObject("Cat_" + title);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(11);
            img.type = Image.Type.Sliced;
            img.color = ColBorderSoft;
            var fillImg = AddInsetFill(go, 11, ColRow);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = 6;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var nameTxt = CreateText(go, title, 16, FontStyle.Bold, ColText, TextAnchor.MiddleLeft);
            nameTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;

            var descTxt = CreateText(go, desc, 13.5f, FontStyle.Normal, ColTextMuted, TextAnchor.UpperLeft);
            var descLe = descTxt.gameObject.AddComponent<LayoutElement>();
            descLe.preferredHeight = 44;
            descLe.flexibleHeight = 1f;

            statusText = CreateText(go, "-- active", 16f, FontStyle.Bold, ColAccent, TextAnchor.MiddleLeft);
            statusText.gameObject.AddComponent<LayoutElement>().preferredHeight = 16;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = fillImg;
            var cb = btn.colors;
            cb.normalColor = ColRow;
            cb.highlightedColor = new Color(ColRow.r + 0.03f, ColRow.g + 0.03f, ColRow.b + 0.03f, 1f);
            cb.pressedColor = ColPanel;
            btn.colors = cb;
            btn.onClick.AddListener(() => onClick());

            return go;
        }
        #endregion [END] BUILD CATEGORY CARD

        #region [START] BUILD STAT TILE
        private Text BuildStatTile(Transform parent, string label)
        {
            var go = new GameObject("Stat_" + label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(11);
            img.type = Image.Type.Sliced;
            img.color = ColBorderSoft;
            AddInsetFill(go, 11, ColRow);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(16, 12, 10, 10);
            layout.childForceExpandWidth = true;

            var numTxt = CreateText(go, "-", 24, FontStyle.Bold, ColText, TextAnchor.MiddleLeft);
            numTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            numTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;

            var lblTxt = CreateText(go, label, 14, FontStyle.Normal, ColTextMuted, TextAnchor.MiddleLeft);
            lblTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 16;

            return numTxt;
        }
        #endregion [END] BUILD STAT TILE

        #region [START] REFRESH OVERVIEW
        private void RefreshOverview()
        {
            try
            {
                int survival = Cfg(Plugin.CraftFromStorage) + Cfg(Plugin.AutoEmptyCollectionNets)
                             + Cfg(Plugin.IslandHandPickup) + Cfg(Plugin.ReefHandHarvesting)
                             + Cfg(Plugin.ReefFastHarvest) + Cfg(Plugin.ShowAnimalHealthBars)
                             + Cfg(Plugin.AntiSharkRaftDamage) + Cfg(Plugin.InfiniteDurability);
                SetCategoryStatus(_ovStatusSurvival, survival, 8);

                int nav = Cfg(Plugin.EnableHUD) + Cfg(Plugin.ShowAnimalHealthBars) + Cfg(Plugin.EnableHotkeys);
                SetCategoryStatus(_ovStatusNav, nav, 3);

                if (_ovStatusSandbox != null)
                {
                    if (Plugin.IsSurvivalMode)
                    {
                        _ovStatusSandbox.text = "Locked in Survival";
                        _ovStatusSandbox.color = ColTextFaint;
                    }
                    else
                    {
                        int sandbox = Cfg(Plugin.GodMode) + Cfg(Plugin.OneHitKill) + Cfg(Plugin.InfiniteOxygen)
                                    + Cfg(Plugin.NoHungerThirst) + Cfg(Plugin.EnableFlyMode) + Cfg(Plugin.FreeCrafting);
                        SetCategoryStatus(_ovStatusSandbox, sandbox, 6);
                    }
                }

                UpdateModeButtonVisuals();
                UpdateProfileButtonVisuals();
            }
            catch { }
        }
        #endregion [END] REFRESH OVERVIEW

        #region [START] CFG
        // A missing ConfigEntry counts as off rather than throwing - the managers use the same
        // null-tolerant pattern, because binding order is not guaranteed during early startup.
        private static int Cfg(BepInEx.Configuration.ConfigEntry<bool> e)
        {
            return (e != null && e.Value) ? 1 : 0;
        }
        #endregion [END] CFG

        #region [START] SET CATEGORY STATUS
        private void SetCategoryStatus(Text t, int on, int total)
        {
            if (t == null) return;
            t.text = on + " of " + total + " active";
            t.color = on == 0 ? ColTextFaint : (on == total ? ColSuccess : ColAccent);
        }
        #endregion [END] SET CATEGORY STATUS
        #endregion [END] SCREEN: OVERVIEW

        #region [START] SCREEN: SURVIVAL & QOL
        #region [START] BUILD SCREEN SURVIVAL
        private GameObject BuildScreenSurvival(GameObject parent)
        {
            var screen = CreateScreenShell(parent, "Survival & QoL",
                "Quality-of-life that keeps vanilla progression intact. Everything here is safe to leave on in Survival Mode.",
                MenuKeyLabel(), out var body);

            AddGroupLabel(body, "Quick Actions");
            var actCard = CreateCard(body);
            AddButtonRow(actCard, "Q", "Quick Stack",
                "Deposits everything in your backpack into matching chests nearby.", "Run", () =>
                {
                    ChestSorter.QuickStackToNearbyChests();
                    SetQoLTooltip("Quick Stack: deposited backpack items into matching nearby chests.");
                });
            AddButtonRow(actCard, "N", "Empty All Nets",
                "Scoops every collection net on the raft into your inventory in one go.", "Run", () =>
                {
                    NetsHelper.EmptyAllNets(silent: false);
                    SetQoLTooltip("Empty Nets: collected all trapped flotsam into your inventory.");
                });
            AddButtonRow(actCard, "W", "Water All Plots",
                "Hydrates every crop plot, grass plot and tree planter on the raft.", "Run", () =>
                {
                    FarmingHelper.WaterAllPlots(silent: false);
                    SetQoLTooltip("Water Plots: hydrated all crop plots, grass plots and planters.");
                });
            AddButtonRow(actCard, "G", "Ocean Magnet",
                "Pulls floating debris toward the raft for a while, then goes on cooldown.", "Toggle", () =>
                {
                    MagneticCollector.ToggleMagnet();
                    SetQoLTooltip("Ocean Magnet: pulling floating flotsam toward your raft.");
                });

            var tipGO = new GameObject("Tip");
            tipGO.transform.SetParent(body.transform, false);
            tipGO.AddComponent<LayoutElement>().preferredHeight = 20;
            _qolTooltipText = CreateText(tipGO, "Pick an action above, or adjust the settings below.", 14, FontStyle.Normal, ColTextFaint, TextAnchor.MiddleLeft);
            _qolTooltipText.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(_qolTooltipText.gameObject);

            AddGroupLabel(body, "Inventory & Storage");
            var storeCard = CreateCard(body);
            AddToggleRow(storeCard, "C", "Craft from Storage",
                "Crafting pulls materials straight out of chests within 22 metres.",
                () => Plugin.CraftFromStorage != null && Plugin.CraftFromStorage.Value,
                v => { if (Plugin.CraftFromStorage != null) Plugin.CraftFromStorage.Value = v; MarkProfileCustom(); });
            AddToggleRow(storeCard, "N", "Auto-Empty Collection Nets",
                "Keeps emptying the raft's nets on their own. Collection QoL has this too - use one or the other.",
                () => Plugin.AutoEmptyCollectionNets != null && Plugin.AutoEmptyCollectionNets.Value,
                v => { if (Plugin.AutoEmptyCollectionNets != null) Plugin.AutoEmptyCollectionNets.Value = v; MarkProfileCustom(); }, true);
            AddStepperRow(storeCard, "S", "Stack Limit",
                "Maximum items per inventory slot.",
                20f, Plugin.IsCreativeMode ? 200f : 100f, 20f, "",
                () => Plugin.CustomStackSize != null ? Plugin.CustomStackSize.Value : 40,
                v => { if (Plugin.CustomStackSize != null) Plugin.CustomStackSize.Value = Mathf.RoundToInt(v); MarkProfileCustom(); }, true);

            AddGroupLabel(body, "Island & Reef Harvesting");
            var harvCard = CreateCard(body);
            AddToggleRow(harvCard, "I", "Island Hand Pickup",
                "Collect surface items on islands by hand, no hook needed. Collection QoL has this too - use one or the other.",
                () => Plugin.IslandHandPickup != null && Plugin.IslandHandPickup.Value,
                v => { if (Plugin.IslandHandPickup != null) Plugin.IslandHandPickup.Value = v; MarkProfileCustom(); });
            AddToggleRow(harvCard, "R", "Reef Hand Harvesting",
                "Mine sand, clay, scrap and ore underwater by hand without a hook.",
                () => Plugin.ReefHandHarvesting != null && Plugin.ReefHandHarvesting.Value,
                v => { if (Plugin.ReefHandHarvesting != null) Plugin.ReefHandHarvesting.Value = v; MarkProfileCustom(); }, true);
            AddToggleRow(harvCard, "F", "Fast Reef Mining",
                "Speeds up underwater channeling so you can surface before the shark arrives.",
                () => Plugin.ReefFastHarvest != null && Plugin.ReefFastHarvest.Value,
                v => { if (Plugin.ReefFastHarvest != null) Plugin.ReefFastHarvest.Value = v; MarkProfileCustom(); }, true);

            AddGroupLabel(body, "Raft & Creature Defense");
            var defCard = CreateCard(body);
            AddToggleRow(defCard, "H", "Animal & Enemy Health Bars",
                "Floating health bars and distance tags above animals and enemies.",
                () => Plugin.ShowAnimalHealthBars != null && Plugin.ShowAnimalHealthBars.Value,
                v => { if (Plugin.ShowAnimalHealthBars != null) Plugin.ShowAnimalHealthBars.Value = v; MarkProfileCustom(); });
            AddToggleRow(defCard, "B", "Anti-Shark Raft Protection",
                "Bruce stops biting raft blocks. He will still come after you in the water.",
                () => Plugin.AntiSharkRaftDamage != null && Plugin.AntiSharkRaftDamage.Value,
                v => { if (Plugin.AntiSharkRaftDamage != null) Plugin.AntiSharkRaftDamage.Value = v; MarkProfileCustom(); }, true);
            AddToggleRow(defCard, "D", "Infinite Tool Durability",
                "Tools, weapons, hooks and armor never wear out.",
                () => Plugin.InfiniteDurability != null && Plugin.InfiniteDurability.Value,
                v => { if (Plugin.InfiniteDurability != null) Plugin.InfiniteDurability.Value = v; MarkProfileCustom(); }, true);

            AddGroupLabel(body, "Multipliers & Speeds");
            var multCard = CreateCard(body);
            bool creative = Plugin.IsCreativeMode;
            AddStepperRow(multCard, "W", "Weapon Damage",
                "Damage dealt by spears, arrows and machete against creatures.",
                1.0f, creative ? 5.0f : 2.0f, 0.5f, "x",
                () => Plugin.WeaponDamageMultiplier != null ? Plugin.WeaponDamageMultiplier.Value : 1.0f,
                v => { if (Plugin.WeaponDamageMultiplier != null) Plugin.WeaponDamageMultiplier.Value = v; MarkProfileCustom(); });
            AddStepperRow(multCard, "K", "Hook Reel Speed",
                "How fast hooks pull debris out of the water. Collection QoL boosts the hook too - the two multiply together.",
                1.0f, creative ? 5.0f : 2.0f, 0.5f, "x",
                () => Plugin.HookPullSpeedMultiplier != null ? Plugin.HookPullSpeedMultiplier.Value : 1.0f,
                v => { if (Plugin.HookPullSpeedMultiplier != null) Plugin.HookPullSpeedMultiplier.Value = v; MarkProfileCustom(); }, true);
            AddStepperRow(multCard, "V", "Swim Speed",
                "Player swimming speed.",
                1.0f, creative ? 4.0f : 1.5f, 0.1f, "x",
                () => Plugin.SwimSpeedMultiplier != null ? Plugin.SwimSpeedMultiplier.Value : 1.0f,
                v => { if (Plugin.SwimSpeedMultiplier != null) Plugin.SwimSpeedMultiplier.Value = v; MarkProfileCustom(); }, true);
            AddStepperRow(multCard, "P", "Sprint Speed",
                "Player sprinting speed on land and raft.",
                1.0f, creative ? 3.0f : 1.5f, 0.1f, "x",
                () => Plugin.SprintSpeedMultiplier != null ? Plugin.SprintSpeedMultiplier.Value : 1.0f,
                v => { if (Plugin.SprintSpeedMultiplier != null) Plugin.SprintSpeedMultiplier.Value = v; MarkProfileCustom(); }, true);

            return screen;
        }
        #endregion [END] BUILD SCREEN SURVIVAL
        #endregion [END] SCREEN: SURVIVAL & QOL

        #region [START] SCREEN: NAVIGATION
        #region [START] BUILD SCREEN NAVIGATION
        private GameObject BuildScreenNavigation(GameObject parent)
        {
            var screen = CreateScreenShell(parent, "Navigation",
                "Compass HUD, live raft and shark telemetry, and one-press raft controls.",
                MenuKeyLabel(), out var body);

            AddGroupLabel(body, "Live Telemetry");
            var teleRowGO = new GameObject("TeleGrid");
            teleRowGO.transform.SetParent(body.transform, false);
            teleRowGO.AddComponent<LayoutElement>().preferredHeight = 92;
            var teleLayout = teleRowGO.AddComponent<HorizontalLayoutGroup>();
            teleLayout.childControlWidth = true;
            teleLayout.childControlHeight = true;
            teleLayout.spacing = 12;
            teleLayout.childForceExpandWidth = true;
            teleLayout.childForceExpandHeight = true;

            _teleHeadingText = BuildTelemetryTile(teleRowGO.transform, "Compass Heading");
            _teleRaftText    = BuildTelemetryTile(teleRowGO.transform, "Raft");
            _teleSharkText   = BuildTelemetryTile(teleRowGO.transform, "Shark Sonar");
            _teleCoordsText  = BuildTelemetryTile(teleRowGO.transform, "World Position");

            AddGroupLabel(body, "On-Screen HUD");
            var hudCard = CreateCard(body);
            AddToggleRow(hudCard, "H", "Show HUD Overlay",
                "Compass, coordinates, raft tracker and shark radar drawn on screen. Press " + HudKeyLabel() + " to toggle, Shift+" + HudKeyLabel() + " to cycle style.",
                () => Plugin.EnableHUD != null && Plugin.EnableHUD.Value,
                v => { if (Plugin.EnableHUD != null) Plugin.EnableHUD.Value = v; });

            var styleRowGO = new GameObject("StyleRow");
            styleRowGO.transform.SetParent(body.transform, false);
            styleRowGO.AddComponent<LayoutElement>().preferredHeight = 38;
            var styleLayout = styleRowGO.AddComponent<HorizontalLayoutGroup>();
            styleLayout.childControlWidth = true;
            styleLayout.childControlHeight = true;
            styleLayout.spacing = 8;
            styleLayout.childForceExpandWidth = true;
            styleLayout.childForceExpandHeight = true;

            for (int s = 0; s < HUDOverlay.StyleNames.Length && s < 4; s++)
            {
                int idx = s;
                Text t;
                _navStyleImgs[s] = BuildSegmentButton(styleRowGO.transform, HUDOverlay.StyleNames[s], out t, () =>
                {
                    HUDOverlay.SetStyle(idx);
                    UpdateNavStyleButtonVisuals();
                });
                _navStyleTexts[s] = t;
            }
            UpdateNavStyleButtonVisuals();

            AddGroupLabel(body, "Raft Controls");
            var raftCard = CreateCard(body);
            _navRecallBtnText = AddActionRow(raftCard, "T", "Recall to Raft",
                "Teleports you back onto the raft. Bound to " + KeyLabel(Plugin.KeyTeleportToRaft, KeyCode.F8) + ".",
                "Recall", () => TeleportManager.TeleportPlayerToRaft());
            AddButtonRow(raftCard, "S", "Summon Raft",
                "Brings the raft to where you are standing. Bound to " + KeyLabel(Plugin.KeyTeleportRaftToPlayer, KeyCode.F9) + ".",
                "Summon", () => TeleportManager.TeleportRaftToPlayer());
            AddButtonRow(raftCard, "A", "Toggle Anchor",
                "Drops or raises the raft anchor.",
                "Toggle", () => TeleportManager.ToggleRaftAnchor());
            _navScannerBtnText = AddActionRow(raftCard, "R", "Island Radar",
                "A 10 second pulse scan that marks nearby islands and points of interest.",
                "Scan", () => ItemDetector.TriggerPulseScan());

            AddGroupLabel(body, "Sails & Engines");
            var boatCard = CreateCard(body);
            AddButtonRow(boatCard, "L", "Toggle All Sails",
                "Opens or closes every sail on the raft at once.",
                "Toggle", () => BoatController.ToggleAllSails());
            AddButtonRow(boatCard, "E", "Toggle All Engines",
                "Starts or stops every engine on the raft at once.",
                "Toggle", () => BoatController.ToggleAllEngines());
            _navSailModeBtnText = AddActionRow(boatCard, "M", "Smart Sail Mode",
                "Manual leaves sails alone, Auto-Wind keeps them aligned with the wind, Follow Heading keeps them aligned with the raft.",
                GetSailModeDisplayName(), () => CycleSailMode());

            return screen;
        }
        #endregion [END] BUILD SCREEN NAVIGATION

        #region [START] BUILD TELEMETRY TILE
        private Text BuildTelemetryTile(Transform parent, string label)
        {
            var go = new GameObject("Tele_" + label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(11);
            img.type = Image.Type.Sliced;
            img.color = ColBorderSoft;
            AddInsetFill(go, 11, ColRow);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(14, 12, 10, 10);
            layout.spacing = 3;
            layout.childForceExpandWidth = true;

            var lblTxt = CreateText(go, label.ToUpperInvariant(), 12, FontStyle.Bold, ColTextFaint, TextAnchor.MiddleLeft);
            lblTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            lblTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 14;

            var valTxt = CreateText(go, "Standby", 17, FontStyle.Bold, ColText, TextAnchor.UpperLeft);
            valTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 44;

            return valTxt;
        }
        #endregion [END] BUILD TELEMETRY TILE

        #region [START] UPDATE NAV STYLE BUTTON VISUALS
        private void UpdateNavStyleButtonVisuals()
        {
            int active = Plugin.HUDStyle != null ? Plugin.HUDStyle.Value : 1;
            for (int i = 0; i < _navStyleImgs.Length; i++)
            {
                if (_navStyleImgs[i] == null) continue;
                bool sel = i == active;
                _navStyleImgs[i].color = sel ? ColAccentWash : ColPanel2;
                if (_navStyleTexts[i] != null)
                {
                    _navStyleTexts[i].color = sel ? ColAccent : ColTextMuted;
                    _navStyleTexts[i].fontStyle = sel ? FontStyle.Bold : FontStyle.Normal;
                }
            }
        }
        #endregion [END] UPDATE NAV STYLE BUTTON VISUALS

        #region [START] GET SAIL MODE DISPLAY NAME
        private static string GetSailModeDisplayName()
        {
            int mode = Plugin.BoatSailMode != null ? Plugin.BoatSailMode.Value : 0;
            switch (mode)
            {
                case 1:  return "Auto-Wind";
                case 2:  return "Follow Heading";
                default: return "Manual";
            }
        }
        #endregion [END] GET SAIL MODE DISPLAY NAME

        #region [START] CYCLE SAIL MODE
        private void CycleSailMode()
        {
            if (Plugin.BoatSailMode == null) return;
            Plugin.BoatSailMode.Value = (Plugin.BoatSailMode.Value + 1) % 3;
            if (_navSailModeBtnText != null) _navSailModeBtnText.text = GetSailModeDisplayName();
            BoatController.ApplyActiveSailMode();
            TeleportManager.SetNotification("Smart Sail Mode: " + GetSailModeDisplayName());
        }
        #endregion [END] CYCLE SAIL MODE

        #region [START] REFRESH TELEMETRY
        private void RefreshTelemetry()
        {
            // Recall and scanner share their button label with a cooldown readout, so they are
            // refreshed here alongside the tiles rather than only when the screen is built.
            if (_navRecallBtnText != null)
            {
                float cd = TeleportManager.GetRecallCooldownRemaining();
                _navRecallBtnText.text = cd > 0f ? Mathf.CeilToInt(cd) + "s" : "Recall";
            }

            if (_navScannerBtnText != null)
            {
                if (ItemDetector.IsScanActive)
                {
                    _navScannerBtnText.text = Mathf.CeilToInt(ItemDetector.GetActiveTimeRemaining()) + "s";
                }
                else
                {
                    float cd = ItemDetector.GetCooldownRemaining();
                    _navScannerBtnText.text = cd > 0f ? Mathf.CeilToInt(cd) + "s" : "Scan";
                }
            }

            if (_navSailModeBtnText != null) _navSailModeBtnText.text = GetSailModeDisplayName();

            var p = PlayerHelper.GetLocalPlayer();
            if (p == null)
            {
                SetTelemetry(_teleHeadingText, "Standby", "Enter a world to read the compass");
                SetTelemetry(_teleRaftText, "Standby", "Waiting for the save file");
                SetTelemetry(_teleSharkText, "Clear", "No predator detected");
                SetTelemetry(_teleCoordsText, "Standby", "Waiting for world telemetry");
                return;
            }

            if (_cachedNavCamera == null) _cachedNavCamera = Camera.main;
            float yaw = _cachedNavCamera != null ? _cachedNavCamera.transform.eulerAngles.y : p.transform.eulerAngles.y;
            string[] cardinals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            int cIndex = Mathf.RoundToInt(yaw / 45f) % 8;
            if (cIndex < 0) cIndex += 8;
            SetTelemetry(_teleHeadingText,
                yaw.ToString("000", CultureInfo.InvariantCulture) + "° " + cardinals[cIndex],
                "Facing " + cardinals[cIndex]);

            if (_cachedNavRaft == null || !_cachedNavRaft.gameObject.activeInHierarchy)
            {
                _cachedNavRaft = ComponentManager<Raft>.Value ?? FindObjectOfType<Raft>();
            }
            if (_cachedNavRaft != null)
            {
                float dist = Vector3.Distance(p.transform.position, _cachedNavRaft.transform.position);
                float knots = _cachedNavRaft.Velocity.magnitude * 1.94f;
                SetTelemetry(_teleRaftText,
                    dist.ToString("F0", CultureInfo.InvariantCulture) + "m away",
                    (_cachedNavRaft.IsAnchored ? "Anchored" : "Drifting") + " · " + knots.ToString("F1", CultureInfo.InvariantCulture) + " kn");
            }
            else
            {
                SetTelemetry(_teleRaftText, "Not found", "Raft reference missing");
            }

            if (_cachedNavShark == null || !_cachedNavShark.gameObject.activeInHierarchy)
            {
                _cachedNavShark = FindObjectOfType<AI_StateMachine_Shark>();
            }
            if (_cachedNavShark != null && _cachedNavShark.gameObject.activeInHierarchy)
            {
                float sDist = Vector3.Distance(p.transform.position, _cachedNavShark.transform.position);
                string threat = sDist < 25f ? "Close - watch out" : (sDist < 50f ? "Prowling nearby" : "Far away");
                SetTelemetry(_teleSharkText, sDist.ToString("F0", CultureInfo.InvariantCulture) + "m away", threat,
                    sDist < 25f ? ColDanger : (sDist < 50f ? ColGold : ColSuccess));
            }
            else
            {
                SetTelemetry(_teleSharkText, "Clear", "Ocean waters are calm", ColSuccess);
            }

            var pos = p.transform.position;
            SetTelemetry(_teleCoordsText,
                "X " + pos.x.ToString("F0", CultureInfo.InvariantCulture) + "  Z " + pos.z.ToString("F0", CultureInfo.InvariantCulture),
                "Altitude Y " + pos.y.ToString("F1", CultureInfo.InvariantCulture) + "m");
        }
        #endregion [END] REFRESH TELEMETRY

        #region [START] SET TELEMETRY
        private void SetTelemetry(Text t, string value, string sub, Color? valueColor = null)
        {
            if (t == null) return;
            t.text = value + "\n<size=12><color=#8FA3A9>" + sub + "</color></size>";
            t.color = valueColor ?? ColText;
        }
        #endregion [END] SET TELEMETRY
        #endregion [END] SCREEN: NAVIGATION

        #region [START] SCREEN: CHEATS & SANDBOX
        #region [START] BUILD SCREEN CHEATS
        private GameObject BuildScreenCheats(GameObject parent)
        {
            var screen = CreateScreenShell(parent, "Cheats & Sandbox",
                "Invulnerability, flight, free crafting, and direct control over the time of day and the weather.",
                MenuKeyLabel(), out var body);

            if (Plugin.IsSurvivalMode)
            {
                AddLockCard(body, "Cheats are locked in Survival Mode",
                    "Survival Mode keeps Sailor's Companion to quality-of-life only, so nothing here can undo the progression you earned. Switch to Creative Mode to unlock god mode, flight, free crafting and the world controls.");
                return screen;
            }

            AddGroupLabel(body, "Player");
            var playerCard = CreateCard(body);
            AddToggleRow(playerCard, "G", "God Mode",
                "Invulnerable to all damage, including the shark.",
                () => Plugin.GodMode != null && Plugin.GodMode.Value,
                v => { if (Plugin.GodMode != null) Plugin.GodMode.Value = v; RefreshOverview(); });
            AddToggleRow(playerCard, "K", "One-Hit Kill",
                "Any enemy or predator dies in a single hit.",
                () => Plugin.OneHitKill != null && Plugin.OneHitKill.Value,
                v => { if (Plugin.OneHitKill != null) Plugin.OneHitKill.Value = v; RefreshOverview(); }, true);
            AddToggleRow(playerCard, "O", "Infinite Oxygen",
                "Dive as long as you like without running out of air.",
                () => Plugin.InfiniteOxygen != null && Plugin.InfiniteOxygen.Value,
                v => { if (Plugin.InfiniteOxygen != null) Plugin.InfiniteOxygen.Value = v; RefreshOverview(); }, true);
            AddToggleRow(playerCard, "H", "Freeze Hunger & Thirst",
                "Both meters stay full.",
                () => Plugin.NoHungerThirst != null && Plugin.NoHungerThirst.Value,
                v => { if (Plugin.NoHungerThirst != null) Plugin.NoHungerThirst.Value = v; RefreshOverview(); }, true);
            AddButtonRow(playerCard, "V", "Replenish All Vitals",
                "Refills health, oxygen, hunger and thirst right now.", "Restore", () =>
                {
                    var p = PlayerHelper.GetLocalPlayer();
                    if (p?.Stats != null)
                    {
                        p.Stats.stat_health?.SetToMaxValue();
                        p.Stats.stat_hunger?.Normal?.SetToMaxValue();
                        p.Stats.stat_thirst?.Normal?.SetToMaxValue();
                        p.Stats.stat_oxygen?.SetToMaxValue();
                        TeleportManager.SetNotification("Vitals fully replenished.");
                    }
                    else
                    {
                        TeleportManager.SetNotification("Enter a game world first.");
                    }
                });

            AddGroupLabel(body, "Movement & Crafting");
            var moveCard = CreateCard(body);
            AddToggleRow(moveCard, "F", "Fly / Noclip",
                "Free flight with Space and Shift. Bound to " + KeyLabel(Plugin.KeyFly, KeyCode.F) + ".",
                () => Plugin.EnableFlyMode != null && Plugin.EnableFlyMode.Value,
                v => { if (Plugin.EnableFlyMode != null) Plugin.EnableFlyMode.Value = v; RefreshOverview(); });
            AddStepperRow(moveCard, "S", "Fly Speed",
                "How fast flight moves you, in metres per second.",
                4f, 40f, 2f, " m/s",
                () => Plugin.FlySpeed != null ? Plugin.FlySpeed.Value : 14f,
                v => { if (Plugin.FlySpeed != null) Plugin.FlySpeed.Value = v; }, true);
            AddToggleRow(moveCard, "C", "Free Instant Crafting",
                "Craft any recipe without spending materials.",
                () => Plugin.FreeCrafting != null && Plugin.FreeCrafting.Value,
                v => { if (Plugin.FreeCrafting != null) Plugin.FreeCrafting.Value = v; RefreshOverview(); }, true);

            AddGroupLabel(body, "Time of Day");
            var timeCard = CreateCard(body);
            AddTripleButtonRow(timeCard, "T", "Set Time",
                "Jumps the world clock straight to morning, noon or night.",
                "Morning", () => SetTime(8f), "Noon", () => SetTime(12f), "Night", () => SetTime(22f));

            AddGroupLabel(body, "Weather");
            var weatherCard = CreateCard(body);
            AddTripleButtonRow(weatherCard, "W", "Set Weather",
                "Forces the weather system to a specific state.",
                "Clear", () => SetWeather(UniqueWeatherType.Default),
                "Calm", () => SetWeather(UniqueWeatherType.Calm),
                "Rain", () => SetWeather(UniqueWeatherType.Rain));
            AddButtonRow(weatherCard, "F", "Fog",
                "Rolls in a thick fog bank.", "Apply", () => SetWeather(UniqueWeatherType.Fog));

            return screen;
        }
        #endregion [END] BUILD SCREEN CHEATS

        #region [START] SET TIME
        private static void SetTime(float hour)
        {
            try
            {
                var sky = FindObjectOfType<UnityEngine.AzureSky.AzureSkyController>();
                if (sky?.timeOfDay != null)
                {
                    sky.timeOfDay.GotoTime(hour);
                    TeleportManager.SetNotification("Time set to " + Mathf.RoundToInt(hour) + ":00.");
                    return;
                }
            }
            catch { }
            TeleportManager.SetNotification("Enter a game world to change the time.");
        }
        #endregion [END] SET TIME

        #region [START] SET WEATHER
        private static void SetWeather(UniqueWeatherType weather)
        {
            try
            {
                var wm = ComponentManager<WeatherManager>.Value ?? FindObjectOfType<WeatherManager>();
                if (wm != null)
                {
                    wm.SetWeather(weather, true);
                    TeleportManager.SetNotification("Weather set to " + weather + ".");
                    return;
                }
            }
            catch { }
            TeleportManager.SetNotification("Enter a game world to change the weather.");
        }
        #endregion [END] SET WEATHER
        #endregion [END] SCREEN: CHEATS & SANDBOX

        #region [START] SCREEN: RESEARCH
        #region [START] BUILD SCREEN RESEARCH
        private GameObject BuildScreenResearch(GameObject parent)
        {
            var screen = CreateScreenShell(parent, "Research",
                "Learn recipes at the research table without hunting down every material first.",
                MenuKeyLabel(), out var body);

            if (Plugin.IsSurvivalMode)
            {
                AddLockCard(body, "Research unlocks are locked in Survival Mode",
                    "Discovering recipes at the research table is a core part of Raft's progression, so these shortcuts stay off in Survival Mode. Switch to Creative Mode to unlock them.");
                return screen;
            }

            AddGroupLabel(body, "Status");
            var statusGO = new GameObject("ResearchStatus");
            statusGO.transform.SetParent(body.transform, false);
            statusGO.AddComponent<LayoutElement>().preferredHeight = 22;
            _researchStatusText = CreateText(statusGO, "Load into a game world, then pick an unlock below.", 14, FontStyle.Normal, ColTextFaint, TextAnchor.MiddleLeft);
            _researchStatusText.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(_researchStatusText.gameObject);

            AddGroupLabel(body, "Materials");
            var matCard = CreateCard(body);
            AddButtonRow(matCard, "B", "Research Base Materials",
                "Researches planks, plastic, scrap, metal, stone, rope and the rest of the common crafting inputs.",
                "Research", ResearchBaseMaterials);

            AddGroupLabel(body, "Story Chapters");
            var chapCard = CreateCard(body);
            AddButtonRow(chapCard, "1", "Chapter 1 Blueprints",
                "Radio Tower and Vasagatan: antenna, receiver, headlight, machete, steering wheel, engine.",
                "Unlock", () => UnlockChapterBlueprints(1, "Radio Tower & Vasagatan",
                    new[] { "antenna", "receiver", "headlight", "machete", "steering", "engine" }));
            AddButtonRow(chapCard, "2", "Chapter 2 Blueprints",
                "Balboa, Caravan Island and Tangaroa: biofuel, storage, charger, grill, pipes, fireworks.",
                "Unlock", () => UnlockChapterBlueprints(2, "Balboa / Caravan / Tangaroa",
                    new[] { "biofuel", "storage", "charger", "grill", "pipe", "firework" }));
            AddButtonRow(chapCard, "3", "Chapter 3 Blueprints",
                "Varuna Point, Temperance and Utopia: advanced battery, advanced anchor, advanced backpack, smelter.",
                "Unlock", () => UnlockChapterBlueprints(3, "Varuna / Temperance / Utopia",
                    new[] { "batteryadvanced", "anchorstationaryadvanced", "backpackadvanced", "smelter" }));

            AddGroupLabel(body, "Everything");
            var allCard = CreateCard(body);
            AddButtonRow(allCard, "A", "Unlock All Recipes",
                "Learns every research-table recipe and story blueprint at once. This cannot be undone on this save.",
                "Unlock All", UnlockAllResearch);

            return screen;
        }
        #endregion [END] BUILD SCREEN RESEARCH

        #region [START] RESEARCH BASE MATERIALS
        private void ResearchBaseMaterials()
        {
            try
            {
                var rt = ComponentManager<Inventory_ResearchTable>.Value ?? FindObjectOfType<Inventory_ResearchTable>();
                var all = ItemManager.GetAllItems();
                if (all == null || all.Count == 0 || rt == null)
                {
                    SetResearchStatus("Load into a game world to research items.", ColDanger);
                    return;
                }

                string[] baseKeywords = { "plank", "plastic", "scrap", "metal", "copper", "stone", "rope",
                                          "brick", "goo", "glass", "hinge", "bolt", "feather", "clay",
                                          "sand", "dirt", "leather", "wool" };
                var player = PlayerHelper.GetLocalPlayer();
                int count = 0;
                foreach (var item in all)
                {
                    if (item == null || string.IsNullOrEmpty(item.UniqueName)) continue;
                    string name = item.UniqueName.ToLower();
                    if (name.StartsWith("blueprint_")) continue;
                    if (!baseKeywords.Any(k => name.Contains(k))) continue;

                    try { rt.Research(item, true); count++; } catch { }
                    if (player != null)
                    {
                        try { rt.LearnItem(item, player.steamID); } catch { }
                    }
                }

                SetResearchStatus("Researched " + count + " base materials at the research table.", ColSuccess);
                TeleportManager.SetNotification("Researched " + count + " base crafting materials.");
            }
            catch (Exception ex)
            {
                SetResearchStatus("Error: " + ex.Message, ColDanger);
            }
        }
        #endregion [END] RESEARCH BASE MATERIALS

        #region [START] UNLOCK CHAPTER BLUEPRINTS
        private void UnlockChapterBlueprints(int chapter, string chapterName, string[] keywords)
        {
            try
            {
                var rt = ComponentManager<Inventory_ResearchTable>.Value ?? FindObjectOfType<Inventory_ResearchTable>();
                var all = ItemManager.GetAllItems();
                if (all == null || all.Count == 0 || rt == null)
                {
                    SetResearchStatus("Load into a game world to unlock blueprints.", ColDanger);
                    return;
                }

                int count = 0;
                foreach (var item in all)
                {
                    if (item == null || string.IsNullOrEmpty(item.UniqueName)) continue;
                    string name = item.UniqueName.ToLower();
                    if (!keywords.Any(k => name.Contains(k))) continue;
                    try { rt.ResearchBlueprint(item); count++; } catch { }
                }

                SetResearchStatus("Chapter " + chapter + " (" + chapterName + ") unlocked - " + count + " blueprints.", ColSuccess);
                TeleportManager.SetNotification("Chapter " + chapter + " blueprints unlocked.");
            }
            catch (Exception ex)
            {
                SetResearchStatus("Error: " + ex.Message, ColDanger);
            }
        }
        #endregion [END] UNLOCK CHAPTER BLUEPRINTS

        #region [START] UNLOCK ALL RESEARCH
        private void UnlockAllResearch()
        {
            try
            {
                var rt = ComponentManager<Inventory_ResearchTable>.Value ?? FindObjectOfType<Inventory_ResearchTable>();
                if (rt == null)
                {
                    SetResearchStatus("Load into a game world to unlock recipes.", ColDanger);
                    return;
                }
                rt.LearnAllRecipesInstantly();
                Cheat.UnlockAllCrafting = true;
                SetResearchStatus("Every research-table recipe and blueprint is now learned.", ColSuccess);
                TeleportManager.SetNotification("All recipes and blueprints unlocked.");
            }
            catch (Exception ex)
            {
                SetResearchStatus("Notice: " + ex.Message + " (load into a world first)", ColDanger);
            }
        }
        #endregion [END] UNLOCK ALL RESEARCH

        #region [START] SET RESEARCH STATUS
        private void SetResearchStatus(string text, Color color)
        {
            if (_researchStatusText == null) return;
            _researchStatusText.text = text;
            _researchStatusText.color = color;
        }
        #endregion [END] SET RESEARCH STATUS
        #endregion [END] SCREEN: RESEARCH

        #region [START] SCREEN: ITEM SPAWNER
        #region [START] BUILD SCREEN SPAWNER
        private GameObject BuildScreenSpawner(GameObject parent)
        {
            var screen = CreateScreenShell(parent, "Item Spawner",
                "Search Raft's full item list and place any of it straight into your inventory.",
                MenuKeyLabel(), out var body);

            if (Plugin.IsSurvivalMode)
            {
                _itemSearchInput = null;
                _itemScrollContent = null;
                AddLockCard(body, "The item spawner is locked in Survival Mode",
                    "Gathering, fishing and diving for scrap are the heart of Raft's progression, so spawning items stays off in Survival Mode. Switch to Creative Mode to browse and spawn any item.");
                return screen;
            }

            AddGroupLabel(body, "Search");
            var searchCard = CreateCard(body);
            BuildSearchRow(searchCard);

            AddGroupLabel(body, "Results");
            var listCard = CreateCard(body);
            var listHostGO = new GameObject("ListHost");
            listHostGO.transform.SetParent(listCard.transform, false);
            listHostGO.AddComponent<LayoutElement>().preferredHeight = 360;

            var listLayout = listHostGO.AddComponent<VerticalLayoutGroup>();
            listLayout.childControlWidth = true;
            listLayout.childControlHeight = true;
            listLayout.padding = new RectOffset(10, 10, 10, 10);
            listLayout.spacing = 4;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;
            _itemScrollContent = listHostGO.transform;

            RefreshItemSpawnerList("");

            return screen;
        }
        #endregion [END] BUILD SCREEN SPAWNER

        #region [START] BUILD SEARCH ROW
        private void BuildSearchRow(GameObject card)
        {
            var rowGO = CreateRowShell(card, "F", "Find an Item",
                "Type part of a name - for example plank, scrap, titanium - then press Enter.",
                false, out _);

            var fieldGO = new GameObject("SearchField");
            fieldGO.transform.SetParent(rowGO.transform, false);
            fieldGO.AddComponent<LayoutElement>().preferredWidth = 260;
            var fieldImg = fieldGO.AddComponent<Image>();
            fieldImg.sprite = GetRoundedSprite(8);
            fieldImg.type = Image.Type.Sliced;
            fieldImg.color = ColBorder;
            AddInsetFill(fieldGO, 8, ColPanel2);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(fieldGO.transform, false);
            var inputTxt = textGO.AddComponent<Text>();
            inputTxt.font = GetGameFont();
            inputTxt.fontSize = 15;
            inputTxt.color = ColText;
            inputTxt.alignment = TextAnchor.MiddleLeft;
            inputTxt.supportRichText = false;
            var textRt = textGO.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(12, 0);
            textRt.offsetMax = new Vector2(-12, 0);

            var phGO = new GameObject("Placeholder");
            phGO.transform.SetParent(fieldGO.transform, false);
            var phTxt = phGO.AddComponent<Text>();
            phTxt.font = GetGameFont();
            phTxt.fontSize = 15;
            phTxt.color = ColTextFaint;
            phTxt.alignment = TextAnchor.MiddleLeft;
            phTxt.text = "Search items...";
            var phRt = phGO.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(12, 0);
            phRt.offsetMax = new Vector2(-12, 0);

            _itemSearchInput = fieldGO.AddComponent<InputField>();
            _itemSearchInput.textComponent = inputTxt;
            _itemSearchInput.placeholder = phTxt;
            _itemSearchInput.targetGraphic = fieldImg;
            _itemSearchInput.lineType = InputField.LineType.SingleLine;
            _itemSearchInput.onValueChanged.AddListener(RefreshItemSpawnerList);

            var clearGO = CreateGhostButton(rowGO.transform, "Clear", () =>
            {
                if (_itemSearchInput != null) _itemSearchInput.text = "";
                RefreshItemSpawnerList("");
            });
            clearGO.AddComponent<LayoutElement>().preferredWidth = 86;
        }
        #endregion [END] BUILD SEARCH ROW

        #region [START] REFRESH ITEM SPAWNER LIST
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
                    // ItemManager is empty until a world is loaded; the asset scan is the fallback.
                    var found = Resources.FindObjectsOfTypeAll<Item_Base>();
                    if (found != null && found.Length > 0)
                    {
                        _allItems = found.Where(i => i != null && !string.IsNullOrEmpty(i.UniqueName)).Distinct().ToList();
                    }
                }
            }

            if (_allItems == null || _allItems.Count == 0)
            {
                AddSpawnerNotice("Items load with the world. Enter a game world to browse and spawn them.");
                return;
            }

            string[] tokens = string.IsNullOrEmpty(filter)
                ? new string[0]
                : filter.Trim().ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var filtered = _allItems.Where(i =>
            {
                if (tokens.Length == 0) return true;
                string uName = i.UniqueName?.ToLower() ?? "";
                string dName = i.settings_Inventory?.DisplayName?.ToLower() ?? "";
                return tokens.Any(t => uName.Contains(t) || dName.Contains(t));
            }).Take(60).ToList();

            if (filtered.Count == 0)
            {
                AddSpawnerNotice("Nothing matches \"" + filter + "\". Try plank, scrap, titanium, or clear the search.");
                return;
            }

            foreach (var item in filtered)
            {
                AddSpawnerRow(item);
            }
        }
        #endregion [END] REFRESH ITEM SPAWNER LIST

        #region [START] ADD SPAWNER NOTICE
        private void AddSpawnerNotice(string message)
        {
            var go = new GameObject("Notice");
            go.transform.SetParent(_itemScrollContent, false);
            go.AddComponent<LayoutElement>().preferredHeight = 48;
            var txt = CreateText(go, message, 14, FontStyle.Normal, ColTextFaint, TextAnchor.MiddleCenter);
            FillParent(txt.gameObject);
        }
        #endregion [END] ADD SPAWNER NOTICE

        #region [START] ADD SPAWNER ROW
        private void AddSpawnerRow(Item_Base item)
        {
            string uniqueName = item.UniqueName;
            string disp = item.settings_Inventory?.DisplayName;
            if (string.IsNullOrEmpty(disp)) disp = uniqueName;
            int stack = item.settings_Inventory != null ? item.settings_Inventory.StackSize : 20;

            var rowGO = new GameObject("ItemRow");
            rowGO.transform.SetParent(_itemScrollContent, false);
            rowGO.AddComponent<LayoutElement>().preferredHeight = 38;
            var img = rowGO.AddComponent<Image>();
            img.sprite = GetRoundedSprite(7);
            img.type = Image.Type.Sliced;
            img.color = ColPanel2;

            var layout = rowGO.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(12, 8, 4, 4);
            layout.spacing = 6;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            var nameGO = new GameObject("Name");
            nameGO.transform.SetParent(rowGO.transform, false);
            nameGO.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var nameTxt = CreateText(nameGO, disp + "  <color=#5E7379>" + uniqueName + "</color>", 14, FontStyle.Normal, ColText, TextAnchor.MiddleLeft);
            nameTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(nameTxt.gameObject);

            var b1 = CreateGhostButton(rowGO.transform, "+1", () => GiveItem(uniqueName, 1));
            b1.AddComponent<LayoutElement>().preferredWidth = 52;

            var b10 = CreateGhostButton(rowGO.transform, "+10", () => GiveItem(uniqueName, 10));
            b10.AddComponent<LayoutElement>().preferredWidth = 58;

            var bStack = CreateGhostButton(rowGO.transform, stack > 1 ? "+" + stack : "+Max", () => GiveItem(uniqueName, stack));
            bStack.AddComponent<LayoutElement>().preferredWidth = 72;
        }
        #endregion [END] ADD SPAWNER ROW

        #region [START] GIVE ITEM
        private void GiveItem(string uniqueName, int amount)
        {
            var p = PlayerHelper.GetLocalPlayer();
            if (p?.Inventory != null)
            {
                p.Inventory.AddItem(uniqueName, amount);
                TeleportManager.SetNotification("Received " + amount + "x " + uniqueName + ".");
            }
            else
            {
                TeleportManager.SetNotification("Enter a game world to spawn items.");
            }
        }
        #endregion [END] GIVE ITEM
        #endregion [END] SCREEN: ITEM SPAWNER

        #region [START] SCREEN: CONTROLS
        #region [START] BUILD SCREEN CONTROLS
        private GameObject BuildScreenControls(GameObject parent)
        {
            var screen = CreateScreenShell(parent, "Controls",
                "Every key Sailor's Companion listens for. Change any of them in the .cfg file.",
                MenuKeyLabel(), out var body);

            AddGroupLabel(body, "Always Active");
            var mainCard = CreateCard(body);
            AddHotkeyRow(mainCard, MenuKeyLabel(), "Open / close this settings menu (Insert also works).");
            AddHotkeyRow(mainCard, "Esc", "Close this menu.");
            AddHotkeyRow(mainCard, HudKeyLabel(), "Show / hide the navigation HUD overlay.");
            AddHotkeyRow(mainCard, "Shift + " + HudKeyLabel(), "Cycle through the four HUD styles.");
            AddHotkeyRow(mainCard, KeyLabel(Plugin.KeyFly, KeyCode.F), "Toggle fly / noclip (Creative Mode only).");
            AddHotkeyRow(mainCard, KeyLabel(Plugin.KeyTeleportToRaft, KeyCode.F8), "Recall yourself to the raft.");
            AddHotkeyRow(mainCard, KeyLabel(Plugin.KeyTeleportRaftToPlayer, KeyCode.F9), "Summon the raft to you.");

            AddGroupLabel(body, "Quick Gameplay Keys");
            var quickCard = CreateCard(body);
            AddToggleRow(quickCard, "Q", "Enable Quick Hotkeys",
                "Off by default so these keys never fight with another mod. Turn on to drive sails, engines, the magnet and the scanner straight from the keyboard.",
                () => Plugin.EnableHotkeys != null && Plugin.EnableHotkeys.Value,
                v =>
                {
                    if (Plugin.EnableHotkeys != null) Plugin.EnableHotkeys.Value = v;
                    TeleportManager.SetNotification(v ? "Quick hotkeys enabled." : "Quick hotkeys disabled - use the menu buttons.");
                    RefreshOverview();
                });
            AddHotkeyRow(quickCard, KeyLabel(Plugin.KeySailToggle, KeyCode.F4), "Toggle all sails open or closed.");
            AddHotkeyRow(quickCard, KeyLabel(Plugin.KeyEngineToggle, KeyCode.F11), "Toggle all engines on or off.");
            AddHotkeyRow(quickCard, KeyLabel(Plugin.KeyMagnetToggle, KeyCode.F7), "Activate the ocean magnetic debris pull.");
            AddHotkeyRow(quickCard, KeyLabel(Plugin.KeyScannerPulse, KeyCode.F10), "Trigger a 10 second island pulse scan.");

            AddCallout(body, "Sharing keys with our other mods",
                "Farmer's Companion uses F1, Inventory Master uses F2 and Collection QoL uses F3 for their own menus, plus Alt + F5 to F8 for its actions. Those are kept clear of everything above.");

            return screen;
        }
        #endregion [END] BUILD SCREEN CONTROLS

        #region [START] KEY LABEL
        private static string KeyLabel(BepInEx.Configuration.ConfigEntry<KeyCode> entry, KeyCode fallback)
        {
            return (entry != null ? entry.Value : fallback).ToString();
        }
        #endregion [END] KEY LABEL

        #region [START] MENU KEY LABEL
        private static string MenuKeyLabel()
        {
            return KeyLabel(Plugin.KeyMenu, KeyCode.F5);
        }
        #endregion [END] MENU KEY LABEL

        #region [START] HUD KEY LABEL
        private static string HudKeyLabel()
        {
            return KeyLabel(Plugin.KeyHUD, KeyCode.F6);
        }
        #endregion [END] HUD KEY LABEL
        #endregion [END] SCREEN: CONTROLS

        #region [START] SCREEN: UPDATES
        #region [START] BUILD SCREEN UPDATES
        private GameObject BuildScreenUpdates(GameObject parent)
        {
            var screen = CreateScreenShell(parent, "Updates",
                "Check whether a newer build of Sailor's Companion is available.",
                MenuKeyLabel(), out var body);

            AddGroupLabel(body, "This Install");
            var verCard = CreateCard(body);
            var verRow = CreateRowShell(verCard, "V", "Installed Version",
                "Sailor's Companion " + PluginInfo.PLUGIN_VERSION + ".", false, out _);

            var badgeGO = new GameObject("Badge");
            badgeGO.transform.SetParent(verRow.transform, false);
            badgeGO.AddComponent<LayoutElement>().preferredWidth = 170;
            _updateBadgeImg = badgeGO.AddComponent<Image>();
            _updateBadgeImg.sprite = GetRoundedSprite(8);
            _updateBadgeImg.type = Image.Type.Sliced;
            _updateBadgeImg.color = ColPanel2;
            _updateBadgeText = CreateText(badgeGO, "● Not checked yet", 14f, FontStyle.Bold, ColTextMuted, TextAnchor.MiddleCenter);
            FillParent(_updateBadgeText.gameObject);

            AddToggleRow(verCard, "A", "Check on Startup",
                "Looks for a newer version once each time the game launches.",
                () => Plugin.CheckForUpdates != null && Plugin.CheckForUpdates.Value,
                v => { if (Plugin.CheckForUpdates != null) Plugin.CheckForUpdates.Value = v; }, true);
            AddButtonRow(verCard, "C", "Check Now",
                "Asks GitHub for the latest published version right away.", "Check", () =>
                {
                    UpdateChecker.Dismissed = false;
                    UpdateChecker.Instance?.TriggerCheck();
                    TeleportManager.SetNotification("Checking GitHub for mod updates...");
                    RefreshUpdateBadge();
                });
            AddButtonRow(verCard, "D", "Open Download Page",
                "Opens the mod page in your browser.", "Open", () =>
                {
                    try { Application.OpenURL(UpdateChecker.DownloadUrl); } catch { }
                });

            AddGroupLabel(body, "Our Other Mods");
            var famCard = CreateCard(body);
            AddHotkeyRow(famCard, "F1", "Farmer's Companion - crops, watering, harvesting and livestock.");
            AddHotkeyRow(famCard, "F2", "Inventory Master - backpack slots, sorting, quick transfer and drop protection.");
            AddHotkeyRow(famCard, "F3", "Collection QoL - nets, hooks, magnet, loot detection and priority pickup.");

            AddCallout(body, "Before you report a bug",
                "If you run more than one of our mods, open each one's settings and make sure a shared feature is only enabled in one of them. Two copies of the same feature is the most common cause of odd behaviour.");

            return screen;
        }
        #endregion [END] BUILD SCREEN UPDATES

        #region [START] REFRESH UPDATE BADGE
        private void RefreshUpdateBadge()
        {
            string suffix, badgeLabel;
            Color badgeBg, badgeFg;

            if (UpdateChecker.IsChecking)
            {
                suffix = "checking...";
                badgeLabel = "● Checking...";
                badgeBg = ColPanel2; badgeFg = ColTextMuted;
            }
            else if (!UpdateChecker.HasChecked)
            {
                suffix = "not checked yet";
                badgeLabel = "● Not checked yet";
                badgeBg = ColPanel2; badgeFg = ColTextMuted;
            }
            else if (UpdateChecker.IsUpdateAvailable)
            {
                suffix = "v" + UpdateChecker.LatestVersion + " available";
                badgeLabel = "● Update available";
                badgeBg = ColGoldWash; badgeFg = ColGold;
            }
            else
            {
                suffix = "up to date";
                badgeLabel = "● Up to date";
                badgeBg = ColSuccessWash; badgeFg = ColSuccess;
            }

            if (_footerVerText != null)
                _footerVerText.text = "Sailor's Companion  <color=#8FA3A9>v" + PluginInfo.PLUGIN_VERSION + " · " + suffix + "</color>";
            if (_updateBadgeText != null) { _updateBadgeText.text = badgeLabel; _updateBadgeText.color = badgeFg; }
            if (_updateBadgeImg != null) _updateBadgeImg.color = badgeBg;
        }
        #endregion [END] REFRESH UPDATE BADGE

        #region [START] POLL UPDATE STATUS
        // UpdateChecker finishes on a web request, not on a frame we control, so the badge is
        // polled once a second rather than being pushed to from the checker.
        private IEnumerator PollUpdateStatus()
        {
            while (true)
            {
                RefreshUpdateBadge();
                yield return new WaitForSeconds(1f);
            }
        }
        #endregion [END] POLL UPDATE STATUS

        #region [START] REFRESH UPDATE BANNER
        // Kept for callers outside this file; the banner is now the footer badge.
        public void RefreshUpdateBanner()
        {
            RefreshUpdateBadge();
        }
        #endregion [END] REFRESH UPDATE BANNER

        #region [START] SET HUD VISIBLE
        public void SetHUDVisible(bool visible)
        {
            if (Plugin.EnableHUD != null) Plugin.EnableHUD.Value = visible;
        }
        #endregion [END] SET HUD VISIBLE
        #endregion [END] SCREEN: UPDATES

        #region [START] SHARED SHELL & COMPONENTS
        #region [START] CREATE SCREEN SHELL
        private GameObject CreateScreenShell(GameObject parent, string title, string desc, string hotkey, out GameObject body)
        {
            var screen = new GameObject("Screen_" + title);
            screen.transform.SetParent(parent.transform, false);
            var screenRt = screen.AddComponent<RectTransform>();
            screenRt.anchorMin = Vector2.zero;
            screenRt.anchorMax = Vector2.one;
            screenRt.offsetMin = Vector2.zero;
            screenRt.offsetMax = Vector2.zero;

            var headGO = new GameObject("Head");
            headGO.transform.SetParent(screen.transform, false);
            var headRt = headGO.AddComponent<RectTransform>();
            headRt.anchorMin = new Vector2(0, 1);
            headRt.anchorMax = new Vector2(1, 1);
            headRt.pivot = new Vector2(0.5f, 1);
            headRt.sizeDelta = new Vector2(0, 112);
            headRt.anchoredPosition = Vector2.zero;

            var headLayout = headGO.AddComponent<HorizontalLayoutGroup>();
            headLayout.childControlWidth = true;
            headLayout.childControlHeight = true;
            // The right padding keeps the title column clear of the floating close button.
            headLayout.padding = new RectOffset(28, 60, 16, 12);
            headLayout.childForceExpandWidth = false;
            headLayout.childForceExpandHeight = true;

            var titleColGO = new GameObject("TitleCol");
            titleColGO.transform.SetParent(headGO.transform, false);
            var titleColLe = titleColGO.AddComponent<LayoutElement>();
            titleColLe.flexibleWidth = 1f;
            var titleColLayout = titleColGO.AddComponent<VerticalLayoutGroup>();
            titleColLayout.childControlWidth = true;
            titleColLayout.childControlHeight = true;
            titleColLayout.spacing = 3;
            titleColLayout.childForceExpandWidth = true;

            var titleTxt = CreateText(titleColGO, title, 24, FontStyle.Bold, ColText, TextAnchor.MiddleLeft);
            titleTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
            var descTxt = CreateText(titleColGO, desc, 15, FontStyle.Normal, ColTextMuted, TextAnchor.UpperLeft);
            var descLe = descTxt.gameObject.AddComponent<LayoutElement>();
            descLe.preferredHeight = 44;
            descLe.flexibleHeight = 1f;

            if (!string.IsNullOrEmpty(hotkey))
            {
                var hkGO = new GameObject("Hotkey");
                hkGO.transform.SetParent(headGO.transform, false);
                var hkLe = hkGO.AddComponent<LayoutElement>();
                hkLe.preferredWidth = 100;
                var hkLayout = hkGO.AddComponent<HorizontalLayoutGroup>();
                hkLayout.childControlWidth = true;
                hkLayout.childControlHeight = true;
                hkLayout.childAlignment = TextAnchor.MiddleRight;
                hkLayout.spacing = 6;
                hkLayout.childForceExpandWidth = false;
                hkLayout.childForceExpandHeight = true;

                var hkLbl = CreateText(hkGO, "Menu", 13, FontStyle.Normal, ColTextFaint, TextAnchor.MiddleRight);
                hkLbl.gameObject.AddComponent<LayoutElement>().preferredWidth = 34;
                CreateKeyChip(hkGO.transform, hotkey);
            }

            AddDivider(screen.transform, 0, headRt);

            var scrollGO = new GameObject("ScrollArea");
            scrollGO.transform.SetParent(screen.transform, false);
            var scrollRt = scrollGO.AddComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(28, 20);
            scrollRt.offsetMax = new Vector2(-26, -112);

            var viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var viewportRt = viewportGO.AddComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            viewportGO.AddComponent<RectMask2D>();

            body = new GameObject("Body");
            body.transform.SetParent(viewportGO.transform, false);
            var bodyRt = body.AddComponent<RectTransform>();
            bodyRt.anchorMin = new Vector2(0, 1);
            bodyRt.anchorMax = new Vector2(1, 1);
            bodyRt.pivot = new Vector2(0.5f, 1);
            bodyRt.anchoredPosition = Vector2.zero;
            // RectTransform defaults sizeDelta to (100,100). With stretch anchors that adds 100px
            // of width the RectMask2D then clips off both sides of every row.
            bodyRt.sizeDelta = Vector2.zero;

            var bodyLayout = body.AddComponent<VerticalLayoutGroup>();
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            bodyLayout.spacing = 14;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = false;

            var bodyFitter = body.AddComponent<ContentSizeFitter>();
            bodyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.content = bodyRt;
            scrollRect.viewport = viewportRt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 28f;

            return screen;
        }
        #endregion [END] CREATE SCREEN SHELL

        #region [START] ADD GROUP LABEL
        private void AddGroupLabel(GameObject parent, string text)
        {
            var go = new GameObject("GroupLabel");
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<LayoutElement>().preferredHeight = 16;
            var t = CreateText(go, text.ToUpperInvariant(), 13, FontStyle.Bold, ColTextFaint, TextAnchor.MiddleLeft);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(t.gameObject);
        }
        #endregion [END] ADD GROUP LABEL

        #region [START] ADD CALLOUT
        private void AddCallout(GameObject parent, string title, string desc)
        {
            var go = new GameObject("Callout");
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<LayoutElement>().preferredHeight = 88;
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(11);
            img.type = Image.Type.Sliced;
            img.color = new Color(ColGold.r, ColGold.g, ColGold.b, 0.12f);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.spacing = 2;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var titleTxt = CreateText(go, title, 15, FontStyle.Bold, ColGold, TextAnchor.MiddleLeft);
            titleTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
            // Sized for two lines - Unity truncates a wrapped line that doesn't fit its box.
            var descTxt = CreateText(go, desc, 14, FontStyle.Normal, ColTextMuted, TextAnchor.UpperLeft);
            descTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 46;
        }
        #endregion [END] ADD CALLOUT

        #region [START] ADD LOCK CARD
        // Shown in place of a screen's contents while Survival Mode is on.
        private void AddLockCard(GameObject parent, string title, string desc)
        {
            var go = new GameObject("LockCard");
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<LayoutElement>().preferredHeight = 190;
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(11);
            img.type = Image.Type.Sliced;
            img.color = ColBorderSoft;
            AddInsetFill(go, 11, ColRow);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(24, 24, 20, 20);
            layout.spacing = 8;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var titleTxt = CreateText(go, title, 18, FontStyle.Bold, ColGold, TextAnchor.MiddleLeft);
            titleTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

            var descTxt = CreateText(go, desc, 15, FontStyle.Normal, ColTextMuted, TextAnchor.UpperLeft);
            var descLe = descTxt.gameObject.AddComponent<LayoutElement>();
            descLe.preferredHeight = 72;

            var btnGO = CreateGhostButton(go.transform, "Switch to Creative Mode", () => SetModMode("Creative"));
            var btnLe = btnGO.AddComponent<LayoutElement>();
            btnLe.preferredHeight = 38;
            btnLe.preferredWidth = 240;
        }
        #endregion [END] ADD LOCK CARD

        #region [START] CREATE CARD
        private GameObject CreateCard(GameObject parent)
        {
            var go = new GameObject("Card");
            go.transform.SetParent(parent.transform, false);
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(11);
            img.type = Image.Type.Sliced;
            img.color = ColBorderSoft;
            AddInsetFill(go, 11, ColRow);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var le = go.AddComponent<LayoutElement>();
            le.flexibleHeight = 0f;

            return go;
        }
        #endregion [END] CREATE CARD

        #region [START] ADD ROW SEPARATOR
        private void AddRowSeparator(GameObject row)
        {
            var sepGO = new GameObject("Sep");
            sepGO.transform.SetParent(row.transform, false);
            sepGO.transform.SetAsFirstSibling();
            var sepRt = sepGO.AddComponent<RectTransform>();
            sepRt.anchorMin = new Vector2(0, 1);
            sepRt.anchorMax = new Vector2(1, 1);
            sepRt.pivot = new Vector2(0.5f, 1);
            sepRt.sizeDelta = new Vector2(0, 1);
            sepRt.anchoredPosition = Vector2.zero;
            var img = sepGO.AddComponent<Image>();
            img.color = ColBorderSoft;
            sepGO.AddComponent<LayoutElement>().ignoreLayout = true;
        }
        #endregion [END] ADD ROW SEPARATOR

        #region [START] CREATE ROW SHELL
        private GameObject CreateRowShell(GameObject card, string monogram, string title, string desc, bool secondary, out RectTransform rt)
        {
            var rowGO = new GameObject("Row");
            rowGO.transform.SetParent(card.transform, false);
            rt = rowGO.AddComponent<RectTransform>();
            rowGO.AddComponent<LayoutElement>().preferredHeight = 72;

            var layout = rowGO.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(18, 18, 12, 12);
            layout.spacing = 16;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // The separator goes above every row but the first. Count only real rows - the
            // inset Fill child would otherwise make the first row look like the second.
            int existingRows = 0;
            foreach (Transform t in card.transform) { if (t.name == "Row") existingRows++; }
            if (existingRows > 1) AddRowSeparator(rowGO);

            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(rowGO.transform, false);
            iconGO.AddComponent<LayoutElement>().preferredWidth = 40;
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = GetRoundedSprite(11);
            iconImg.type = Image.Type.Sliced;
            iconImg.color = ColPanel2;
            var iconTxt = CreateText(iconGO, monogram, 14, FontStyle.Bold, secondary ? ColGold : ColAccent, TextAnchor.MiddleCenter);
            FillParent(iconTxt.gameObject);

            var textColGO = new GameObject("Text");
            textColGO.transform.SetParent(rowGO.transform, false);
            var textColLe = textColGO.AddComponent<LayoutElement>();
            textColLe.flexibleWidth = 1f;
            var textColLayout = textColGO.AddComponent<VerticalLayoutGroup>();
            textColLayout.childControlWidth = true;
            textColLayout.childControlHeight = true;
            textColLayout.childForceExpandWidth = true;
            textColLayout.spacing = 2;
            textColLayout.childAlignment = TextAnchor.MiddleLeft;

            var titleTxt = CreateText(textColGO, title, 18, FontStyle.Bold, ColText, TextAnchor.MiddleLeft);
            titleTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;
            var descTxt = CreateText(textColGO, desc, 14, FontStyle.Normal, ColTextMuted, TextAnchor.UpperLeft);
            descTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 34;

            return rowGO;
        }
        #endregion [END] CREATE ROW SHELL

        #region [START] ADD TOGGLE ROW
        private GameObject AddToggleRow(GameObject card, string monogram, string title, string desc, Func<bool> getter, Action<bool> setter, bool secondary = false)
        {
            var rowGO = CreateRowShell(card, monogram, title, desc, secondary, out _);
            var sw = CreateToggleSwitch(rowGO.transform, getter(), setter);
            sw.AddComponent<LayoutElement>();
            return rowGO;
        }
        #endregion [END] ADD TOGGLE ROW

        #region [START] ADD BUTTON ROW
        private GameObject AddButtonRow(GameObject card, string monogram, string title, string desc, string buttonLabel, Action onClick)
        {
            var rowGO = CreateRowShell(card, monogram, title, desc, false, out _);
            var btnGO = CreateGhostButton(rowGO.transform, buttonLabel, onClick);
            btnGO.AddComponent<LayoutElement>().preferredWidth = 120;
            return rowGO;
        }
        #endregion [END] ADD BUTTON ROW

        #region [START] ADD ACTION ROW
        // A button row whose label is live (cooldown seconds, current sail mode), so the caller
        // gets the Text back to keep updating.
        private Text AddActionRow(GameObject card, string monogram, string title, string desc, string buttonLabel, Action onClick)
        {
            var rowGO = CreateRowShell(card, monogram, title, desc, false, out _);
            var btnGO = CreateGhostButton(rowGO.transform, buttonLabel, onClick);
            btnGO.AddComponent<LayoutElement>().preferredWidth = 150;
            return btnGO.GetComponentInChildren<Text>();
        }
        #endregion [END] ADD ACTION ROW

        #region [START] ADD TRIPLE BUTTON ROW
        private GameObject AddTripleButtonRow(GameObject card, string monogram, string title, string desc,
                                              string l1, Action a1, string l2, Action a2, string l3, Action a3)
        {
            var rowGO = CreateRowShell(card, monogram, title, desc, true, out _);

            var groupGO = new GameObject("BtnGroup");
            groupGO.transform.SetParent(rowGO.transform, false);
            groupGO.AddComponent<LayoutElement>().preferredWidth = 300;
            var layout = groupGO.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.spacing = 6;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var b1 = CreateGhostButton(groupGO.transform, l1, a1);
            b1.AddComponent<LayoutElement>().preferredHeight = 34;
            var b2 = CreateGhostButton(groupGO.transform, l2, a2);
            b2.AddComponent<LayoutElement>().preferredHeight = 34;
            var b3 = CreateGhostButton(groupGO.transform, l3, a3);
            b3.AddComponent<LayoutElement>().preferredHeight = 34;

            return rowGO;
        }
        #endregion [END] ADD TRIPLE BUTTON ROW

        #region [START] ADD HOTKEY ROW
        private void AddHotkeyRow(GameObject card, string key, string desc)
        {
            var rowGO = new GameObject("HkRow");
            rowGO.transform.SetParent(card.transform, false);
            rowGO.AddComponent<LayoutElement>().preferredHeight = 44;
            var layout = rowGO.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(18, 18, 8, 8);
            layout.spacing = 14;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            int existingRows = 0;
            foreach (Transform t in card.transform) { if (t.name == "HkRow") existingRows++; }
            if (existingRows > 1) AddRowSeparator(rowGO);

            var chipGO = new GameObject("Chip");
            chipGO.transform.SetParent(rowGO.transform, false);
            chipGO.AddComponent<LayoutElement>().preferredWidth = 96;
            var chipImg = chipGO.AddComponent<Image>();
            chipImg.sprite = GetRoundedSprite(6);
            chipImg.type = Image.Type.Sliced;
            chipImg.color = ColBorder;
            AddInsetFill(chipGO, 6, ColRow);
            var chipTxt = CreateText(chipGO, key, 14f, FontStyle.Bold, ColTextMuted, TextAnchor.MiddleCenter);
            chipTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(chipTxt.gameObject);

            var descTxt = CreateText(rowGO, desc, 14, FontStyle.Normal, ColTextMuted, TextAnchor.MiddleLeft);
            descTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        }
        #endregion [END] ADD HOTKEY ROW

        #region [START] ADD STEPPER ROW
        private GameObject AddStepperRow(GameObject card, string monogram, string title, string desc, float min, float max, float step, string suffix, Func<float> getter, Action<float> setter, bool secondary = false)
        {
            var rowGO = CreateRowShell(card, monogram, title, desc, secondary, out _);

            var stepperGO = new GameObject("Stepper");
            stepperGO.transform.SetParent(rowGO.transform, false);
            stepperGO.AddComponent<LayoutElement>().preferredWidth = 185;
            var stepperLayout = stepperGO.AddComponent<HorizontalLayoutGroup>();
            stepperLayout.childControlWidth = true;
            stepperLayout.childControlHeight = true;
            stepperLayout.spacing = 8;
            stepperLayout.childAlignment = TextAnchor.MiddleRight;
            stepperLayout.childForceExpandWidth = false;
            stepperLayout.childForceExpandHeight = true;

            var trackGO = new GameObject("Track");
            trackGO.transform.SetParent(stepperGO.transform, false);
            trackGO.AddComponent<LayoutElement>().preferredWidth = 100;
            var trackImg = trackGO.AddComponent<Image>();
            trackImg.sprite = GetRoundedSprite(2);
            trackImg.type = Image.Type.Sliced;
            trackImg.color = ColBorder;

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(trackGO.transform, false);
            var fillRt = fillGO.AddComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0, 0.5f);
            fillRt.anchorMax = new Vector2(0, 0.5f);
            fillRt.pivot = new Vector2(0, 0.5f);
            fillRt.sizeDelta = new Vector2(10, 5);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.sprite = GetRoundedSprite(2);
            fillImg.type = Image.Type.Sliced;
            fillImg.color = ColAccent;

            var thumbGO = new GameObject("Thumb");
            thumbGO.transform.SetParent(trackGO.transform, false);
            var thumbRt = thumbGO.AddComponent<RectTransform>();
            thumbRt.anchorMin = new Vector2(0, 0.5f);
            thumbRt.anchorMax = new Vector2(0, 0.5f);
            thumbRt.pivot = new Vector2(0.5f, 0.5f);
            thumbRt.sizeDelta = new Vector2(12, 12);
            var thumbImg = thumbGO.AddComponent<Image>();
            thumbImg.sprite = GetRoundedSprite(6);
            thumbImg.type = Image.Type.Sliced;
            thumbImg.color = ColText;

            var valGO = new GameObject("Val");
            valGO.transform.SetParent(stepperGO.transform, false);
            valGO.AddComponent<LayoutElement>().preferredWidth = 66;
            var valTxt = CreateText(valGO, "", 14, FontStyle.Normal, ColText, TextAnchor.MiddleRight);
            valTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(valTxt.gameObject);

            var slider = trackGO.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.fillRect = fillRt;
            slider.handleRect = thumbRt;
            slider.targetGraphic = thumbImg;

            string fmt = step < 1f ? "F1" : "F0";
            Action<float> refresh = v => { valTxt.text = v.ToString(fmt, CultureInfo.InvariantCulture) + suffix; };

            float initial = Mathf.Clamp(getter(), min, max);
            slider.value = initial;
            refresh(initial);
            slider.onValueChanged.AddListener(v =>
            {
                setter(v);
                refresh(v);
            });

            return rowGO;
        }
        #endregion [END] ADD STEPPER ROW

        // ---------------- animated toggle switch ----------------
        private const float SwitchW = 48, SwitchH = 27, KnobSize = 21, KnobMargin = 3;

        #region [START] CREATE TOGGLE SWITCH
        private GameObject CreateToggleSwitch(Transform parent, bool initial, Action<bool> onChange)
        {
            var go = new GameObject("Switch");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(SwitchW, SwitchH);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = SwitchW; le.preferredHeight = SwitchH;

            var trackImg = go.AddComponent<Image>();
            trackImg.sprite = GetRoundedSprite((int)(SwitchH / 2));
            trackImg.type = Image.Type.Sliced;
            trackImg.color = initial ? ColAccent : ColBorder;

            var knobGO = new GameObject("Knob");
            knobGO.transform.SetParent(go.transform, false);
            var knobRt = knobGO.AddComponent<RectTransform>();
            knobRt.anchorMin = new Vector2(0, 0.5f);
            knobRt.anchorMax = new Vector2(0, 0.5f);
            knobRt.pivot = new Vector2(0, 0.5f);
            knobRt.sizeDelta = new Vector2(KnobSize, KnobSize);
            knobRt.anchoredPosition = new Vector2(initial ? SwitchW - KnobSize - KnobMargin : KnobMargin, 0);
            var knobImg = knobGO.AddComponent<Image>();
            knobImg.sprite = GetRoundedSprite((int)(KnobSize / 2));
            knobImg.type = Image.Type.Sliced;
            knobImg.color = ColBg;

            bool state = initial;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = trackImg;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() =>
            {
                state = !state;
                onChange(state);
                StartCoroutine(AnimateSwitch(knobRt, trackImg, state));
            });

            return go;
        }
        #endregion [END] CREATE TOGGLE SWITCH

        #region [START] ANIMATE SWITCH
        private IEnumerator AnimateSwitch(RectTransform knob, Image track, bool on)
        {
            float duration = 0.12f;
            float t = 0f;
            float fromX = knob.anchoredPosition.x;
            float toX = on ? SwitchW - KnobSize - KnobMargin : KnobMargin;
            Color fromC = track.color;
            Color toC = on ? ColAccent : ColBorder;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                knob.anchoredPosition = new Vector2(Mathf.Lerp(fromX, toX, k), 0);
                track.color = Color.Lerp(fromC, toC, k);
                yield return null;
            }
            knob.anchoredPosition = new Vector2(toX, 0);
            track.color = toC;
        }
        #endregion [END] ANIMATE SWITCH

        #region [START] CREATE GHOST BUTTON
        private GameObject CreateGhostButton(Transform parent, string label, Action onClick)
        {
            var go = new GameObject("GhostBtn");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(8);
            img.type = Image.Type.Sliced;
            img.color = ColBorder;
            var fillImg = AddInsetFill(go, 8, ColPanel2);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = fillImg;
            var cb = btn.colors;
            cb.normalColor = ColPanel2;
            cb.highlightedColor = ColAccentWash;
            cb.pressedColor = ColRow;
            btn.colors = cb;
            btn.onClick.AddListener(() => onClick());

            var txt = CreateText(go, label, 15f, FontStyle.Bold, ColText, TextAnchor.MiddleCenter);
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(txt.gameObject);
            return go;
        }
        #endregion [END] CREATE GHOST BUTTON

        #region [START] CREATE KEY CHIP
        private GameObject CreateKeyChip(Transform parent, string text)
        {
            var go = new GameObject("Chip");
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredWidth = 42;
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(5);
            img.type = Image.Type.Sliced;
            img.color = ColBorder;
            AddInsetFill(go, 5, ColRow);
            var txt = CreateText(go, text, 16f, FontStyle.Bold, ColTextMuted, TextAnchor.MiddleCenter);
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(txt.gameObject);
            return go;
        }
        #endregion [END] CREATE KEY CHIP

        #region [START] ADD DIVIDER
        private void AddDivider(Transform parent, float marginBottom, RectTransform anchorBelow = null)
        {
            if (anchorBelow != null)
            {
                var dGO = new GameObject("Divider");
                dGO.transform.SetParent(parent, false);
                var dRt = dGO.AddComponent<RectTransform>();
                dRt.anchorMin = new Vector2(0, 1);
                dRt.anchorMax = new Vector2(1, 1);
                dRt.pivot = new Vector2(0.5f, 1);
                dRt.sizeDelta = new Vector2(0, 1);
                dRt.anchoredPosition = new Vector2(0, -anchorBelow.sizeDelta.y);
                var img = dGO.AddComponent<Image>();
                img.color = ColBorderSoft;
                return;
            }

            var go = new GameObject("Divider");
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredHeight = 1;
            var im = go.AddComponent<Image>();
            im.color = ColBorderSoft;
        }
        #endregion [END] ADD DIVIDER

        #region [START] BUILD FOOTER
        private void BuildFooter(GameObject parent)
        {
            var footGO = new GameObject("Footer");
            footGO.transform.SetParent(parent.transform, false);
            var fRt = footGO.AddComponent<RectTransform>();
            fRt.anchorMin = new Vector2(0, 0);
            fRt.anchorMax = new Vector2(1, 0);
            fRt.pivot = new Vector2(0.5f, 0);
            fRt.sizeDelta = new Vector2(0, 52);
            fRt.anchoredPosition = Vector2.zero;

            AddDivider(footGO.transform, 0);
            var topLine = footGO.transform.Find("Divider");
            if (topLine != null)
            {
                var tlRt = topLine.GetComponent<RectTransform>();
                tlRt.anchorMin = new Vector2(0, 1);
                tlRt.anchorMax = new Vector2(1, 1);
                tlRt.pivot = new Vector2(0.5f, 1);
                tlRt.sizeDelta = new Vector2(0, 1);
                tlRt.anchoredPosition = Vector2.zero;

                // footGO's own HorizontalLayoutGroup treats every child as a row item unless told
                // otherwise - without this, the next layout rebuild collapses this divider's
                // full-width top-border anchors into "just another item in the row", rendering as
                // an unexplained grey box instead of a thin border line.
                var tlLe = topLine.GetComponent<LayoutElement>();
                if (tlLe == null) tlLe = topLine.gameObject.AddComponent<LayoutElement>();
                tlLe.ignoreLayout = true;
            }

            var layout = footGO.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(28, 26, 8, 8);
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleLeft;

            var verGO = new GameObject("Ver");
            verGO.transform.SetParent(footGO.transform, false);
            verGO.AddComponent<LayoutElement>().flexibleWidth = 1f;
            _footerVerText = CreateText(verGO, "Sailor's Companion  <color=#8FA3A9>v" + PluginInfo.PLUGIN_VERSION + " · not checked yet</color>", 15f, FontStyle.Normal, ColTextMuted, TextAnchor.MiddleLeft);
            // This line sits in a single-line-tall strip. CreateText defaults to Wrap and Unity's
            // Text defaults verticalOverflow to Truncate, so a wrapped second line is silently
            // clipped and the sentence looks cut off mid-word. Force single-line instead.
            _footerVerText.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(_footerVerText.gameObject);

            var btnGO = new GameObject("Btn_Check");
            btnGO.transform.SetParent(footGO.transform, false);
            btnGO.AddComponent<LayoutElement>().preferredWidth = 175;
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.sprite = GetRoundedSprite(8);
            btnImg.type = Image.Type.Sliced;
            btnImg.color = ColAccent;
            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var cb = btn.colors;
            cb.normalColor = ColAccent;
            cb.highlightedColor = ColAccentStrong;
            cb.pressedColor = ColAccentStrong;
            btn.colors = cb;
            btn.onClick.AddListener(() =>
            {
                UpdateChecker.Dismissed = false;
                UpdateChecker.Instance?.TriggerCheck();
                TeleportManager.SetNotification("Checking GitHub for mod updates...");
                RefreshUpdateBadge();
            });
            var btnTxt = CreateText(btnGO, "Check for Updates", 17f, FontStyle.Bold, ColOnAccentTxt, TextAnchor.MiddleCenter);
            btnTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            FillParent(btnTxt.gameObject);
        }
        #endregion [END] BUILD FOOTER

        // ---------------- rounded-rect sprite generator (9-sliced, cached by radius) ----------------
        private static readonly Dictionary<int, Sprite> _roundedSpriteCache = new Dictionary<int, Sprite>();

        #region [START] GET ROUNDED SPRITE
        private static Sprite GetRoundedSprite(int radius)
        {
            radius = Mathf.Max(2, radius);
            if (_roundedSpriteCache.TryGetValue(radius, out var cached) && cached != null) return cached;

            int size = radius * 2 + 4;
            var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inCornerX = x < radius || x >= size - radius;
                    bool inCornerY = y < radius || y >= size - radius;
                    float alpha = 1f;
                    if (inCornerX && inCornerY)
                    {
                        float cx = x < radius ? radius : size - radius - 1;
                        float cy = y < radius ? radius : size - radius - 1;
                        float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                        alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            sprite.name = "SC_Rounded_" + radius;
            _roundedSpriteCache[radius] = sprite;
            return sprite;
        }
        #endregion [END] GET ROUNDED SPRITE

        #region [START] ADD INSET FILL
        // UnityEngine.UI.Outline draws an offset duplicate of the graphic, not a border stroke, so
        // on a filled shape it reads as a shadow. A border is instead the parent image showing
        // through around a slightly smaller fill child.
        private Image AddInsetFill(GameObject go, int radius, Color fillColor, float inset = 1.5f)
        {
            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(go.transform, false);
            fillGO.transform.SetAsFirstSibling();
            var rt = fillGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
            var img = fillGO.AddComponent<Image>();
            img.sprite = GetRoundedSprite(radius);
            img.type = Image.Type.Sliced;
            img.color = fillColor;
            img.raycastTarget = false;
            fillGO.AddComponent<LayoutElement>().ignoreLayout = true;
            return img;
        }
        #endregion [END] ADD INSET FILL

        #region [START] CREATE TEXT
        private Text CreateText(GameObject parent, string text, float fontSize, FontStyle style, Color color, TextAnchor alignment)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent.transform, false);
            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = GetGameFont();
            t.fontSize = Mathf.RoundToInt(fontSize);
            t.fontStyle = style;
            t.color = color;
            t.alignment = alignment;
            t.raycastTarget = false;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            return t;
        }
        #endregion [END] CREATE TEXT

        #region [START] FILL PARENT
        private void FillParent(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        #endregion [END] FILL PARENT

        #region [START] SET LAYER RECURSIVELY
        private static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                var child = obj.transform.GetChild(i);
                if (child != null) SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        #endregion [END] SET LAYER RECURSIVELY
        #endregion [END] SHARED SHELL & COMPONENTS
    }
    // ============================================================================
    // [END] CANVAS SAILOR'S COMPANION SETTINGS UI
    // ============================================================================
    #endregion [END] CANVAS SAILOR'S COMPANION SETTINGS UI
}
