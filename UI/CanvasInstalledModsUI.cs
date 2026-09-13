using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using SailorsCompanion.Features;
using SailorsCompanion.Patches;

namespace SailorsCompanion.UI
{
    #region [START] CANVAS INSTALLED MODS MANAGER UI (MODERN CLEAN EDITION)
    // ============================================================================
    // [START] CANVAS INSTALLED MODS MANAGER UI (MODERN CLEAN EDITION)
    // Purpose: "Modern Clean" in-game dialog listing every mod in the Konduri
    //          Modding family as a flat dark card, reachable from the main menu
    //          "MODS" button and the in-game pause menu "MOD MENU" button.
    //          Each card reflects into its own mod's settings UI to open it -
    //          this file never references the peer mods' assemblies directly.
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

        #region [START] MODERN CLEAN PALETTE (shared across the mod family - do not change these hex values)
        private static readonly Color ColBg          = new Color32(0x0F, 0x15, 0x18, 0xFF);
        private static readonly Color ColPanel       = new Color32(0x16, 0x1F, 0x24, 0xFF);
        private static readonly Color ColPanel2      = new Color32(0x1C, 0x27, 0x2D, 0xFF);
        private static readonly Color ColRow         = new Color32(0x1A, 0x24, 0x2A, 0xFF);
        private static readonly Color ColBorder      = new Color32(0x26, 0x33, 0x3B, 0xFF);
        private static readonly Color ColBorderSoft  = new Color32(0x1E, 0x29, 0x30, 0xFF);
        private static readonly Color ColText        = new Color32(0xEA, 0xF3, 0xF1, 0xFF);
        private static readonly Color ColTextMuted   = new Color32(0x8F, 0xA3, 0xA9, 0xFF);
        private static readonly Color ColTextFaint   = new Color32(0x5E, 0x73, 0x79, 0xFF);
        private static readonly Color ColAccent      = new Color32(0x2F, 0xC7, 0xB0, 0xFF);
        private static readonly Color ColAccentStrong= new Color32(0x20, 0xA7, 0x94, 0xFF);
        private static readonly Color ColAccentWash  = new Color(0x2F / 255f, 0xC7 / 255f, 0xB0 / 255f, 0.16f);
        private static readonly Color ColGold        = new Color32(0xE8, 0xB9, 0x4A, 0xFF);
        private static readonly Color ColSuccess     = new Color32(0x5F, 0xBE, 0x7A, 0xFF);
        private static readonly Color ColSuccessWash = new Color(0x5F / 255f, 0xBE / 255f, 0x7A / 255f, 0.16f);
        private static readonly Color ColDanger      = new Color32(0xE0, 0x5A, 0x5A, 0xFF);
        private static readonly Color ColOnAccentTxt = new Color32(0x06, 0x23, 0x1F, 0xFF);
        #endregion

        // Per-mod brand accents (distinct per card - not part of the shared design-system palette above).
        private static readonly Color SailorsCyan     = new Color(0.10f, 0.92f, 1.00f, 1.00f);
        private static readonly Color InventoryGold   = new Color(1.00f, 0.72f, 0.20f, 1.00f);
        private static readonly Color FarmersGreen    = new Color(0.35f, 0.95f, 0.35f, 1.00f);
        private static readonly Color CollectionPurple= new Color(0.66f, 0.48f, 1.00f, 1.00f);

        #region [START] UNITY LIFECYCLE
        private void Awake()
        {
            Instance = this;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            try
            {
                GetGameFont();
                BuildCanvasUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Sailor's Companion] CanvasInstalledModsUI.Awake() FAILED: {ex}");
            }
        }

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

            if (Instance._rootGO.activeSelf)
            {
                Close();
                return;
            }

            Instance._rootGO.SetActive(true);
            Instance.EnsureEventSystem();
            if (Instance._raycaster != null && !Instance._raycaster.enabled) Instance._raycaster.enabled = true;

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

            try { Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None); }
            catch { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

            try
            {
                if (CanvasHelper.ActiveMenu == MenuType.None)
                {
                    CanvasHelper.ActiveMenu = MenuType.Cheat;
                }
            }
            catch { }

            // The window is built once (in Awake, while _rootGO is still inactive so it starts
            // hidden) and only re-shown here. Unity's layout system never computes layout for an
            // inactive hierarchy and doesn't retroactively recalculate it just because the object
            // becomes active again, so every nested VerticalLayoutGroup/HorizontalLayoutGroup rect
            // would otherwise stay at its raw default forever. Forcing one rebuild here, every time
            // the window opens, fixes that for good (same fix used by the sibling settings menus).
            if (Instance._windowGO != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(Instance._windowGO.GetComponent<RectTransform>());
            }
        }

        public static void Close()
        {
            if (Instance == null || Instance._rootGO == null) return;
            if (!Instance._rootGO.activeSelf) return;

            Instance._rootGO.SetActive(false);

            // Restore whatever the game/other mod menus expect - same pattern as
            // CanvasModUI.ToggleModWindow(), since this window is reachable both from
            // the main menu (no player, no world) and the in-game pause menu.
            try
            {
                if (CanvasHelper.ActiveMenu == MenuType.Cheat && !CursorPatchHelper.ShouldForceCursorFree())
                {
                    CanvasHelper.ActiveMenu = MenuType.None;
                }
            }
            catch { }

            bool isOtherMenuOpen = false;
            try
            {
                if (CanvasHelper.ActiveMenu != MenuType.None && CanvasHelper.ActiveMenu != MenuType.Cheat)
                {
                    isOtherMenuOpen = true;
                }
            }
            catch { }

            if (CursorPatchHelper.ShouldForceCursorFree())
            {
                isOtherMenuOpen = true;
            }

            var localPlayer = PlayerHelper.GetLocalPlayer();

            try
            {
                var cic = CustomInputConfig.Instance;
                if (cic != null && localPlayer != null && !isOtherMenuOpen)
                {
                    cic.SwitchCurrentActionMap("Player");
                }
            }
            catch { }

            if (localPlayer != null && !isOtherMenuOpen)
            {
                try { Helper.SetCursorVisibleAndLockState(false, CursorLockMode.Locked); }
                catch { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            }
            else
            {
                try { Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None); }
                catch { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            }
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
                    es = existing;
                }
                else
                {
                    var esGO = new GameObject("SailorsCompanion_InstalledModsUI_EventSystem");
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

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }
        #endregion [END] UNITY LIFECYCLE

        #region [START] BUILD CANVAS UI
        private void BuildCanvasUI()
        {
            if (_canvasGO != null && _rootGO != null) return;

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
                _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                _scaler.matchWidthOrHeight = 0.5f;

                _raycaster = _canvasGO.AddComponent<GraphicRaycaster>();
            }

            if (_rootGO == null)
            {
                BuildManagerWindow();
                SetLayerRecursively(_canvasGO, LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5);
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

            // 2. Dimmer Background (visual backdrop only - non-blocking so clicks never accidentally
            //    close the menu, same convention as the redesigned per-mod settings canvases).
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

            // 3. Main Window Panel
            _windowGO = new GameObject("Window_InstalledMods");
            _windowGO.transform.SetParent(_rootGO.transform, false);
            var winRt = _windowGO.AddComponent<RectTransform>();
            winRt.anchorMin = new Vector2(0.5f, 0.5f);
            winRt.anchorMax = new Vector2(0.5f, 0.5f);
            winRt.pivot = new Vector2(0.5f, 0.5f);
            winRt.sizeDelta = new Vector2(1180, 760);
            winRt.anchoredPosition = Vector2.zero;

            var winImg = _windowGO.AddComponent<Image>();
            winImg.sprite = GetRoundedSprite(18);
            winImg.type = Image.Type.Sliced;
            winImg.color = ColBorder;
            AddInsetFill(_windowGO, 18, ColPanel);

            // 4. Header (brand mark + title/subtitle)
            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(_windowGO.transform, false);
            var headerRt = headerGO.AddComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.pivot = new Vector2(0.5f, 1);
            headerRt.sizeDelta = new Vector2(0, 68);
            headerRt.anchoredPosition = Vector2.zero;

            var headerLayout = headerGO.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.padding = new RectOffset(24, 64, 12, 12);
            headerLayout.spacing = 14;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            var markGO = new GameObject("Mark");
            markGO.transform.SetParent(headerGO.transform, false);
            var markLe = markGO.AddComponent<LayoutElement>();
            markLe.preferredWidth = 46; markLe.preferredHeight = 46;
            var markImg = markGO.AddComponent<Image>();
            markImg.sprite = GetRoundedSprite(11);
            markImg.type = Image.Type.Sliced;
            markImg.color = ColAccent;
            var markTxt = CreateText(markGO, "⚡", 20, FontStyle.Bold, ColOnAccentTxt, TextAnchor.MiddleCenter);
            FillParent(markTxt.gameObject);

            var titleColGO = new GameObject("TitleCol");
            titleColGO.transform.SetParent(headerGO.transform, false);
            var titleColLe = titleColGO.AddComponent<LayoutElement>();
            titleColLe.flexibleWidth = 1f;
            var titleColLayout = titleColGO.AddComponent<VerticalLayoutGroup>();
            titleColLayout.childControlWidth = true;
            titleColLayout.childControlHeight = true;
            titleColLayout.childForceExpandWidth = true;
            titleColLayout.spacing = 2;

            var titleTxt = CreateText(titleColGO, "Installed Mods", 21, FontStyle.Bold, ColText, TextAnchor.MiddleLeft);
            titleTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;
            var subTxt = CreateText(titleColGO, "4 mods in the Konduri Modding family - configure any of them from here.", 13.5f, FontStyle.Normal, ColTextFaint, TextAnchor.MiddleLeft);
            subTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 17;

            AddDivider(_windowGO.transform, 0, headerRt);
            BuildCloseButton();

            // 5. Scrolling body: a 2x2 card grid, one card per mod in the family.
            var scrollGO = new GameObject("ScrollArea");
            scrollGO.transform.SetParent(_windowGO.transform, false);
            var scrollRt = scrollGO.AddComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(24, 24);
            scrollRt.offsetMax = new Vector2(-24, -(headerRt.sizeDelta.y + 1));

            var viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var viewportRt = viewportGO.AddComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            viewportGO.AddComponent<RectMask2D>();

            var bodyGO = new GameObject("Body");
            bodyGO.transform.SetParent(viewportGO.transform, false);
            var bodyRt = bodyGO.AddComponent<RectTransform>();
            bodyRt.anchorMin = new Vector2(0, 1);
            bodyRt.anchorMax = new Vector2(1, 1);
            bodyRt.pivot = new Vector2(0.5f, 1);
            bodyRt.anchoredPosition = Vector2.zero;
            bodyRt.sizeDelta = Vector2.zero;

            var bodyLayout = bodyGO.AddComponent<VerticalLayoutGroup>();
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            bodyLayout.spacing = 16;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = false;

            var bodyFitter = bodyGO.AddComponent<ContentSizeFitter>();
            bodyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.content = bodyRt;
            scrollRect.viewport = viewportRt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 28f;

            // Caption line (flows with the body instead of a separate fixed strip - keeps the
            // window to a single scroll region, matching the reference dialog's simpler chrome).
            var captionTxt = CreateText(bodyGO, "Click Configure to open a mod's own settings menu, or view its source on GitHub.", 14, FontStyle.Normal, ColTextMuted, TextAnchor.MiddleLeft);
            captionTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 20;

            // Several features (auto-pickup, crop growth, hook speed, nets) exist in more than one
            // mod in this family. This is the one screen where all of them are visible together, so
            // it's the most useful place to warn about doubling up.
            var overlapTxt = CreateText(bodyGO, "<color=#E8B94A>Running more than one?</color>  A few features appear in several of these mods. Turn each one on in a single mod only, so it never applies twice.", 14, FontStyle.Normal, ColTextMuted, TextAnchor.MiddleLeft);
            overlapTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;

            var row1GO = new GameObject("Row1");
            row1GO.transform.SetParent(bodyGO.transform, false);
            row1GO.AddComponent<LayoutElement>().preferredHeight = 236;
            var row1Layout = row1GO.AddComponent<HorizontalLayoutGroup>();
            row1Layout.childControlWidth = true;
            row1Layout.childControlHeight = true;
            row1Layout.spacing = 16;
            row1Layout.childForceExpandWidth = true;
            row1Layout.childForceExpandHeight = true;

            var row2GO = new GameObject("Row2");
            row2GO.transform.SetParent(bodyGO.transform, false);
            row2GO.AddComponent<LayoutElement>().preferredHeight = 236;
            var row2Layout = row2GO.AddComponent<HorizontalLayoutGroup>();
            row2Layout.childControlWidth = true;
            row2Layout.childControlHeight = true;
            row2Layout.spacing = 16;
            row2Layout.childForceExpandWidth = true;
            row2Layout.childForceExpandHeight = true;

            // Card 1: Sailor's Companion (this mod itself)
            BuildModCard(row1GO.transform,
                monogram: "⚓",
                title: "Sailor's Companion",
                accent: SailorsCyan,
                installed: true,
                version: PluginInfo.PLUGIN_VERSION,
                description: "Compass HUD, shark radar, auto-sail alignment, free flight, and a 300+ item spawner.",
                openLabel: "Open Settings",
                onOpen: () =>
                {
                    Close();
                    CanvasModUI.Instance?.ToggleModWindow();
                },
                githubUrl: "https://github.com/RAVITEJAanand/SailorsCompanion-RaftMod"
            );

            // Card 2: Inventory Master
            BuildModCard(row1GO.transform,
                monogram: "\U0001F392",
                title: "Inventory Master",
                accent: InventoryGold,
                installed: PeerExists("InventoryMaster.PluginInfo"),
                version: GetPeerVersion("InventoryMaster.PluginInfo", "1.0.5"),
                description: "15-slot backpack expansion, auto-sort, hotbar row swap, and 5m auto-pickup vacuum.",
                openLabel: "Open Settings",
                onOpen: () =>
                {
                    Close();
                    OpenInventoryMasterSettings();
                },
                githubUrl: "https://github.com/RAVITEJAanand/InventoryMaster-RaftMod"
            );

            // Card 3: Farmer's Companion
            BuildModCard(row2GO.transform,
                monogram: "\U0001F33E",
                title: "Farmer's Companion",
                accent: FarmersGreen,
                installed: PeerExists("FarmersCompanion.PluginInfo"),
                version: GetPeerVersion("FarmersCompanion.PluginInfo", "1.0.10"),
                description: "Auto-watering, auto-harvest, auto-replant, and faster crop & tree growth.",
                openLabel: "Open Settings",
                onOpen: () =>
                {
                    Close();
                    OpenFarmersCompanionSettings();
                },
                githubUrl: "https://github.com/RAVITEJAanand/FarmersCompanion-RaftMod"
            );

            // Card 4: Collection QoL (4th mod in the family - pre-release, reflects defensively:
            // if its assembly/class isn't present yet, or isn't named exactly as agreed, this
            // simply shows "Not Detected" with the fallback version instead of throwing).
            BuildModCard(row2GO.transform,
                monogram: "\U0001F4E6",
                title: "Collection QoL",
                accent: CollectionPurple,
                installed: PeerExists("CollectionQoL.UI.CanvasCollectionQoLUI"),
                version: GetPeerVersion("CollectionQoL.PluginInfo", "1.0.0"),
                description: "Automated loot collection, hooks, and detection QoL.",
                openLabel: "Open Settings",
                onOpen: () =>
                {
                    Close();
                    OpenCollectionQoLSettings();
                },
                githubUrl: "https://github.com/RAVITEJAanand/CollectionQoL-RaftMod"
            );
        }

        // The redesign never got a visible close/X control on the corner of every screen - ESC still
        // closes the window (see Update above), but a panel this size reads as a standalone app and
        // players expect a corner close button regardless of knowing the hotkey. Parented directly
        // to the window root (last sibling) so it renders above the header/body.
        private void BuildCloseButton()
        {
            var go = new GameObject("CloseBtn");
            go.transform.SetParent(_windowGO.transform, false);
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
            btn.onClick.AddListener(Close);

            var txt = CreateText(go, "✕", 15, FontStyle.Bold, ColTextMuted, TextAnchor.MiddleCenter);
            txt.raycastTarget = false;
            FillParent(txt.gameObject);
        }

        // ---------------- mod card ----------------
        private void BuildModCard(Transform parent, string monogram, string title, Color accent, bool installed, string version, string description, string openLabel, Action onOpen, string githubUrl)
        {
            var cardGO = new GameObject("Card_" + title);
            cardGO.transform.SetParent(parent, false);
            var img = cardGO.AddComponent<Image>();
            img.sprite = GetRoundedSprite(14);
            img.type = Image.Type.Sliced;
            img.color = ColBorderSoft;
            AddInsetFill(cardGO, 14, ColRow);

            var layout = cardGO.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(20, 20, 18, 16);
            layout.spacing = 12;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Header row: monogram badge + title/subtitle
            var headRowGO = new GameObject("Head");
            headRowGO.transform.SetParent(cardGO.transform, false);
            headRowGO.AddComponent<LayoutElement>().preferredHeight = 46;
            var headLayout = headRowGO.AddComponent<HorizontalLayoutGroup>();
            headLayout.childControlWidth = true;
            headLayout.childControlHeight = true;
            headLayout.spacing = 14;
            headLayout.childAlignment = TextAnchor.MiddleLeft;
            headLayout.childForceExpandWidth = false;
            headLayout.childForceExpandHeight = true;

            var markGO = new GameObject("Mark");
            markGO.transform.SetParent(headRowGO.transform, false);
            var markLe = markGO.AddComponent<LayoutElement>();
            markLe.preferredWidth = 44; markLe.preferredHeight = 44;
            var markImg = markGO.AddComponent<Image>();
            markImg.sprite = GetRoundedSprite(11);
            markImg.type = Image.Type.Sliced;
            markImg.color = accent;
            var markTxt = CreateText(markGO, monogram, 19, FontStyle.Bold, ColOnAccentTxt, TextAnchor.MiddleCenter);
            FillParent(markTxt.gameObject);

            var titleColGO = new GameObject("TitleCol");
            titleColGO.transform.SetParent(headRowGO.transform, false);
            titleColGO.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var titleColLayout = titleColGO.AddComponent<VerticalLayoutGroup>();
            titleColLayout.childControlWidth = true;
            titleColLayout.childControlHeight = true;
            titleColLayout.childForceExpandWidth = true;
            titleColLayout.spacing = 2;

            var titleTxt = CreateText(titleColGO, title, 18.5f, FontStyle.Bold, accent, TextAnchor.MiddleLeft);
            titleTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;
            var byTxt = CreateText(titleColGO, "KONDURI (RAVITEJAanand)", 12.5f, FontStyle.Normal, ColTextFaint, TextAnchor.MiddleLeft);
            byTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 15;

            // Description
            var descTxt = CreateText(cardGO, description, 14, FontStyle.Normal, ColTextMuted, TextAnchor.UpperLeft);
            var descLe = descTxt.gameObject.AddComponent<LayoutElement>();
            descLe.preferredHeight = 52;
            descLe.flexibleHeight = 1f;

            // Status / version row
            var statRowGO = new GameObject("StatusRow");
            statRowGO.transform.SetParent(cardGO.transform, false);
            statRowGO.AddComponent<LayoutElement>().preferredHeight = 20;
            var statLayout = statRowGO.AddComponent<HorizontalLayoutGroup>();
            statLayout.childControlWidth = true;
            statLayout.childControlHeight = true;
            statLayout.spacing = 8;
            statLayout.childAlignment = TextAnchor.MiddleLeft;
            statLayout.childForceExpandWidth = false;
            statLayout.childForceExpandHeight = true;

            Color statusColor = installed ? ColSuccess : ColTextFaint;

            var dotGO = new GameObject("Dot");
            dotGO.transform.SetParent(statRowGO.transform, false);
            var dotLe = dotGO.AddComponent<LayoutElement>();
            dotLe.preferredWidth = 7; dotLe.preferredHeight = 7;
            var dotImg = dotGO.AddComponent<Image>();
            dotImg.sprite = GetRoundedSprite(4);
            dotImg.type = Image.Type.Sliced;
            dotImg.color = statusColor;

            var statusTxt = CreateText(statRowGO, installed ? "Installed" : "Not Detected", 13, FontStyle.Bold, statusColor, TextAnchor.MiddleLeft);
            statusTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var verTxt = CreateText(statRowGO, "v" + version, 13, FontStyle.Normal, ColTextFaint, TextAnchor.MiddleRight);
            verTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 90;

            AddDivider(cardGO.transform, 0);

            // Action buttons
            var btnRowGO = new GameObject("BtnRow");
            btnRowGO.transform.SetParent(cardGO.transform, false);
            btnRowGO.AddComponent<LayoutElement>().preferredHeight = 42;
            var btnLayout = btnRowGO.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childControlWidth = true;
            btnLayout.childControlHeight = true;
            btnLayout.spacing = 10;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = true;

            var openBtnGO = CreateGhostButton(btnRowGO.transform, openLabel, () => onOpen?.Invoke());
            openBtnGO.AddComponent<LayoutElement>().flexibleWidth = 1.3f;

            CreateLinkButton(btnRowGO.transform, "GitHub", () => { try { Application.OpenURL(githubUrl); } catch { } });
        }

        private static string GetPeerVersion(string fullTypeName, string fallback)
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var t = asm.GetType(fullTypeName);
                    if (t != null)
                    {
                        var f = t.GetField("PLUGIN_VERSION", BindingFlags.Public | BindingFlags.Static);
                        if (f != null)
                        {
                            var val = f.GetValue(null) as string;
                            if (!string.IsNullOrEmpty(val)) return val;
                        }
                    }
                }
            }
            catch { }
            return fallback;
        }

        // Used purely for the card's "Installed / Not Detected" status label - reuses the same
        // defensive assembly-scan pattern as GetPeerVersion, never throws if the peer isn't loaded.
        private static bool PeerExists(string fullTypeName)
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.GetType(fullTypeName) != null) return true;
                }
            }
            catch { }
            return false;
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

        private void OpenFarmersCompanionSettings()
        {
            try
            {
                var asmList = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in asmList)
                {
                    var t = asm.GetType("FarmersCompanion.UI.CanvasFarmersCompanionUI") ?? asm.GetType("FarmersCompanion.UI.FarmingMenuUI");
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
                Debug.LogWarning("[Mods Manager] Failed to open Farmer's Companion menu: " + ex.Message);
            }
        }

        // Collection QoL is still pre-release and being built in parallel; by agreed convention it
        // will expose CollectionQoL.UI.CanvasCollectionQoLUI.Toggle(). This mirrors the exact
        // defensive try/catch + assembly-scan pattern used for the other two peer mods above, so if
        // that type doesn't exist yet (or ends up named slightly differently) this simply no-ops
        // instead of throwing - the card itself already shows "Not Detected" via PeerExists.
        private void OpenCollectionQoLSettings()
        {
            try
            {
                var asmList = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in asmList)
                {
                    var t = asm.GetType("CollectionQoL.UI.CanvasCollectionQoLUI");
                    if (t != null)
                    {
                        var m = t.GetMethod("Toggle", BindingFlags.Public | BindingFlags.Static);
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
                Debug.LogWarning("[Mods Manager] Failed to open Collection QoL menu: " + ex.Message);
            }
        }

        // ---------------- shared chrome / small components (Modern Clean design system) ----------------
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
            FillParent(txt.gameObject);
            return go;
        }

        private void CreateLinkButton(Transform parent, string label, Action onClick)
        {
            var go = new GameObject("Link");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = GetRoundedSprite(8);
            img.type = Image.Type.Sliced;
            img.color = ColBorderSoft;
            var fillImg = AddInsetFill(go, 8, ColRow);

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandHeight = true;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = fillImg;
            var cb = btn.colors;
            cb.normalColor = ColRow;
            cb.highlightedColor = ColAccentWash;
            btn.colors = cb;
            btn.onClick.AddListener(() => onClick());

            var txt = CreateText(go, label, 14f, FontStyle.Bold, ColTextMuted, TextAnchor.MiddleCenter);
            FillParent(txt.gameObject);
        }

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

        // Outline (UnityEngine.UI.Outline) only renders an offset shadow-duplicate of a graphic - it
        // does NOT draw a stroke around a filled shape's edges, so a "bordered card" that relied on
        // it would render as a flat, undifferentiated box. This lays a slightly-inset fill Image on
        // top of the (now border-colored) parent Image, producing a real visible border ring.
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

        // ---------------- rounded-rect sprite generator (9-sliced, cached by radius) ----------------
        private static readonly Dictionary<int, Sprite> _roundedSpriteCache = new Dictionary<int, Sprite>();

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
            sprite.name = "SC_InstalledMods_Rounded_" + radius;
            _roundedSpriteCache[radius] = sprite;
            return sprite;
        }

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

        private void FillParent(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

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
        #endregion [END] BUILD CANVAS UI
    }
    // ============================================================================
    // [END] CANVAS INSTALLED MODS MANAGER UI
    // ============================================================================
    #endregion
}
