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
        private Text _researchStatusText;

        // Item Spawner
        private InputField _itemSearchInput;
        private Transform _itemScrollContent;
        private List<Item_Base> _allItems;

        // Font
        private Font _gameFont;

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
                EnsureEventSystem();
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
            catch {}
            if (_gameFont != null) return _gameFont;

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
            EnsureEventSystem();
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
            winRt.sizeDelta = new Vector2(960, 590);

            var winImg = _modWindowGO.AddComponent<Image>();
            winImg.color = new Color(0.06f, 0.06f, 0.08f, 0.98f);

            // Title Bar
            var titleBar = CreateBox(_modWindowGO.transform, "TitleBar", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 52), new Color(0.09f, 0.09f, 0.12f, 1f));
            CreateBox(titleBar.transform, "TitleAccent", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(0, 2), new Color(0.85f, 0.15f, 0.20f, 1f));

            var titleText = CreateText(titleBar.transform, "TitleText", $"⚓ <color=#EF4444>Sailor's Companion</color> <size=14><color=#FFFFFF>v{PluginInfo.PLUGIN_VERSION}</color></size> — <color=#E2E8F0>Quality of Life & Utilities</color>", 19, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            titleText.rectTransform.offsetMin = new Vector2(20, 0);

            // Discord button in title bar
            CreateButton(titleBar.transform, "Btn_Discord", "💬 Discord", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-60, 0), new Vector2(100, 32), () => Application.OpenURL("https://discord.gg/B4EMrR5Vrf"), new Color(0.80f, 0.16f, 0.20f, 0.95f), Color.white, 13);

            // Close button in title bar
            CreateButton(titleBar.transform, "Btn_Close", "✕", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-12, 0), new Vector2(34, 32), () => ToggleModWindow(), new Color(0.18f, 0.18f, 0.22f, 0.95f), Color.white, 16);

            // Tabs Row
            var tabRow = CreateBox(_modWindowGO.transform, "TabRow", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -58), new Vector2(-28, 44), Color.clear);
            var tabLayout = tabRow.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 8;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;

            string[] tabNames = { "🎒 Survival QoL", "⚡ Cheats", "🧭 Navigation", "🔬 R&D / Blueprints", "📦 Item Spawner" };
            for (int i = 0; i < tabNames.Length; i++)
            {
                int index = i;
                var tabBtn = CreateButton(tabRow.transform, $"TabBtn_{i}", tabNames[i], Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () => SelectTab(index), new Color(0.13f, 0.13f, 0.17f, 0.92f), Color.white, 14);
                _tabButtonImages[i] = tabBtn.GetComponent<Image>();
                _tabButtonTexts[i] = tabBtn.GetComponentInChildren<Text>();
            }

            // Tab Content Area (precisely bounded between Tabs and Footer)
            var contentArea = CreateBox(_modWindowGO.transform, "ContentArea", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var cRt = contentArea.GetComponent<RectTransform>();
            cRt.offsetMin = new Vector2(18, 38);
            cRt.offsetMax = new Vector2(-18, -108);

            // Build individual tab pages
            _tabPages[0] = BuildSurvivalQoLTab(contentArea.transform);
            _tabPages[1] = BuildCheatsTab(contentArea.transform);
            _tabPages[2] = BuildNavTab(contentArea.transform);
            _tabPages[3] = BuildResearchTab(contentArea.transform);
            _tabPages[4] = BuildSpawnerTab(contentArea.transform);

            // Fixed Hotkeys Footer Bar
            var footerBar = CreateBox(_modWindowGO.transform, "FooterBar", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(0, 34), new Color(0.08f, 0.08f, 0.10f, 1f));
            CreateBox(footerBar.transform, "FooterAccent", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 1), new Color(0.20f, 0.20f, 0.25f, 0.8f));
            CreateText(footerBar.transform, "FooterText", "<color=#94A3B8>Hotkeys:</color> <color=#EF4444>[F5]</color> Menu  |  <color=#EF4444>[F6]</color> HUD  |  <color=#EF4444>[F]</color> Fly  |  <color=#EF4444>[F8]</color> Recall to Raft  |  <color=#EF4444>[F9]</color> Summon Raft  |  <color=#EF4444>[ESC]</color> Close", 13, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter);

            // Update Banner (shown when a newer version is available online)
            _updateBannerGO = CreateBox(_modWindowGO.transform, "UpdateBanner", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(-36, 40), new Color(0.72f, 0.12f, 0.16f, 0.98f));
            var bannerLayout = _updateBannerGO.AddComponent<HorizontalLayoutGroup>();
            bannerLayout.spacing = 10;
            bannerLayout.padding = new RectOffset(16, 12, 4, 4);
            bannerLayout.childForceExpandHeight = true;
            bannerLayout.childForceExpandWidth = false;

            _updateBannerText = CreateText(_updateBannerGO.transform, "UpdateTxt", "✨ <b>New Update Available!</b>", 14, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            EnsureLayout(_updateBannerText.gameObject, -1, 32, true);

            CreateButton(_updateBannerGO.transform, "Btn_UpdateDownload", "⬇️ Download Update", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(165, 32), () =>
            {
                Application.OpenURL(UpdateChecker.DownloadUrl);
            }, new Color(0.10f, 0.10f, 0.14f, 1f), Color.white, 14);

            CreateButton(_updateBannerGO.transform, "Btn_UpdateDismiss", "✕", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(32, 32), () =>
            {
                UpdateChecker.Dismissed = true;
                _updateBannerGO?.SetActive(false);
            }, new Color(0.18f, 0.18f, 0.22f, 0.95f), Color.white, 15);

            _updateBannerGO.SetActive(false);

            SelectTab(0);
        }
        // ============================================================================
        // [END] MOD WINDOW FRAME & TABS CONTROLLER
        // ============================================================================
        #endregion

        #region [START] TAB 0: SURVIVAL QOL
        // ============================================================================
        // [START] TAB 0: SURVIVAL QUALITY OF LIFE (Scrollable Page with Full ON/OFF Controls)
        // ============================================================================
        private GameObject BuildSurvivalQoLTab(Transform parent)
        {
            var page = CreateBox(parent, "Page_SurvivalQoL", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);

            // Scroll View Container for smooth navigation through all QoL options
            var scrollGO = CreateBox(page.transform, "ScrollView", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 25f;

            var viewport = CreateBox(scrollGO.transform, "Viewport", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            var contentGO = CreateBox(viewport.transform, "Content", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero, Color.clear);
            var contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8;
            contentLayout.padding = new RectOffset(4, 12, 4, 12);
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentGO.GetComponent<RectTransform>();

            // 1. Quick Stack Button Row
            var qsRow = CreateBox(contentGO.transform, "QuickStackRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 42), Color.clear);
            EnsureLayout(qsRow, -1, 42);
            var qsLayout = qsRow.AddComponent<HorizontalLayoutGroup>();
            qsLayout.spacing = 10;
            qsLayout.childForceExpandWidth = true;

            CreateButton(qsRow.transform, "Btn_QuickStack", "📦 Quick Stack Items into Nearby Chests (22m)", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
            {
                ChestSorter.QuickStackToNearbyChests();
            }, new Color(0.85f, 0.15f, 0.20f, 1f), Color.white, 15);

            // 2. Craft from Nearby Storage / Chests Toggle
            CreateToggleItem(contentGO.transform, "🛠️ Craft from Nearby Storage (Auto-pulls crafting materials from chests within 22m)", Plugin.CraftFromStorage.Value, v =>
            {
                Plugin.CraftFromStorage.Value = v;
                TeleportManager.SetNotification(v ? "🛠️ Craft from Storage: ENABLED" : "🛠️ Craft from Storage: DISABLED");
            });

            // 3. Collection Nets: Button + Auto-Empty Toggle
            var netsRow = CreateBox(contentGO.transform, "NetsRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 42), Color.clear);
            EnsureLayout(netsRow, -1, 42);
            var netsLayout = netsRow.AddComponent<HorizontalLayoutGroup>();
            netsLayout.spacing = 10;
            netsLayout.childForceExpandWidth = true;

            CreateButton(netsRow.transform, "Btn_EmptyNets", "🕸️ Empty All Raft Collection Nets Now", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
            {
                NetsHelper.EmptyAllNets(silent: false);
            }, new Color(0.18f, 0.18f, 0.23f), Color.white, 14);

            CreateToggleItem(contentGO.transform, "🕸️ Auto-Empty Collection Nets (Continuously gathers trapped items into inventory)", Plugin.AutoEmptyCollectionNets.Value, v =>
            {
                Plugin.AutoEmptyCollectionNets.Value = v;
                TeleportManager.SetNotification(v ? "🕸️ Auto-Empty Nets: ENABLED" : "🕸️ Auto-Empty Nets: DISABLED");
            });

            // 4. Farming Helper: Button + Auto-Water Toggle
            var farmRow = CreateBox(contentGO.transform, "FarmRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 42), Color.clear);
            EnsureLayout(farmRow, -1, 42);
            var farmLayout = farmRow.AddComponent<HorizontalLayoutGroup>();
            farmLayout.spacing = 10;
            farmLayout.childForceExpandWidth = true;

            CreateButton(farmRow.transform, "Btn_WaterPlots", "🌱 Water All Crops & Animal Grass Now", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
            {
                FarmingHelper.WaterAllPlots(silent: false);
            }, new Color(0.18f, 0.18f, 0.23f), Color.white, 14);

            CreateToggleItem(contentGO.transform, "🌱 Auto-Water Crops Continually (Never let crop plots or livestock grass dry out)", Plugin.AutoWaterCrops.Value, v =>
            {
                Plugin.AutoWaterCrops.Value = v;
                TeleportManager.SetNotification(v ? "🌱 Auto-Watering: ENABLED" : "🌱 Auto-Watering: DISABLED");
            });

            // 5. Crop & Tree Growth Boost: Toggle + Stepper
            CreateToggleItem(contentGO.transform, "🌾 Accelerate Crop & Tree Growth (Speeds up farming & tree growth cycles)", Plugin.EnableCropGrowthBoost.Value, v =>
            {
                Plugin.EnableCropGrowthBoost.Value = v;
                TeleportManager.SetNotification(v ? "🌾 Crop Growth Boost: ENABLED" : "🌾 Crop Growth Boost: DISABLED");
            });

            CreateStepperItem(contentGO.transform, "🌾 Crop Growth Speed Multiplier", 1.0f, 5.0f, 0.5f, Plugin.CropGrowthMultiplier.Value, "x", v =>
            {
                Plugin.CropGrowthMultiplier.Value = v;
            });

            // 6. Animal & Enemy Health Bars Toggle
            CreateToggleItem(contentGO.transform, "🐾 Animal & Enemy Health Bars (Floating HP bars and distance meters over creatures)", Plugin.ShowAnimalHealthBars.Value, v =>
            {
                Plugin.ShowAnimalHealthBars.Value = v;
                TeleportManager.SetNotification(v ? "🐾 Animal Health Bars: ENABLED" : "🐾 Animal Health Bars: DISABLED");
            });

            // 7. Anti-Shark Protection Toggle
            CreateToggleItem(contentGO.transform, "🦈 Anti-Shark Raft Protection (Bruce will not attack or destroy raft foundations)", Plugin.AntiSharkRaftDamage.Value, v =>
            {
                Plugin.AntiSharkRaftDamage.Value = v;
                TeleportManager.SetNotification(v ? "🦈 Anti-Shark: ENABLED" : "🦈 Anti-Shark: DISABLED");
            });

            // 8. Infinite Tool Durability Toggle
            CreateToggleItem(contentGO.transform, "🔨 Infinite Tool Durability (Hooks, weapons, tools, gear & armor never break)", Plugin.InfiniteDurability.Value, v =>
            {
                Plugin.InfiniteDurability.Value = v;
                TeleportManager.SetNotification(v ? "🔨 Infinite Durability: ENABLED" : "🔨 Infinite Durability: DISABLED");
            });

            // 9. Steppers: Hook speed, Stack size, Swim speed, Sprint speed
            CreateStepperItem(contentGO.transform, "🎣 Hook Reel-In Speed Multiplier", 1.0f, 5.0f, 0.5f, Plugin.HookPullSpeedMultiplier.Value, "x", v => Plugin.HookPullSpeedMultiplier.Value = v);
            CreateStepperItem(contentGO.transform, "📦 Resource Stack Size Limit", 20f, 999f, 50f, Plugin.CustomStackSize.Value, "", v => Plugin.CustomStackSize.Value = Mathf.RoundToInt(v));
            CreateStepperItem(contentGO.transform, "🏊 Swimming Speed Multiplier", 1.0f, 4.0f, 0.2f, Plugin.SwimSpeedMultiplier.Value, "x", v => Plugin.SwimSpeedMultiplier.Value = v);
            CreateStepperItem(contentGO.transform, "🏃 Sprinting Speed Multiplier", 1.0f, 3.0f, 0.2f, Plugin.SprintSpeedMultiplier.Value, "x", v => Plugin.SprintSpeedMultiplier.Value = v);

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
            var page = CreateBox(parent, "Page_Cheats", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateToggleItem(page.transform, "🛡️ God Mode (Invulnerable to all damage & shark bites)", Plugin.GodMode.Value, v => Plugin.GodMode.Value = v);
            CreateToggleItem(page.transform, "🤿 Infinite Oxygen (Dive freely without running out of air)", Plugin.InfiniteOxygen.Value, v => Plugin.InfiniteOxygen.Value = v);
            CreateToggleItem(page.transform, "🥩 Freeze Hunger & Thirst (Never starve or dehydrate)", Plugin.NoHungerThirst.Value, v => Plugin.NoHungerThirst.Value = v);
            CreateToggleItem(page.transform, "🕊️ Fly / Noclip Mode (Hotkey: [F] | WASD + Space/Shift)", Plugin.EnableFlyMode.Value, v => Plugin.EnableFlyMode.Value = v);
            CreateToggleItem(page.transform, "🛠️ Free Instant Crafting (Craft any recipe without materials)", Plugin.FreeCrafting.Value, v => Plugin.FreeCrafting.Value = v);

            CreateButtonItem(page.transform, "⚡ Instant Max Vitals (Full Health, Food, Water, Oxygen)", () =>
            {
                var p = PlayerHelper.GetLocalPlayer();
                if (p?.Stats != null)
                {
                    p.Stats.stat_health?.SetToMaxValue();
                    p.Stats.stat_hunger?.Normal?.SetToMaxValue();
                    p.Stats.stat_thirst?.Normal?.SetToMaxValue();
                    p.Stats.stat_oxygen?.SetToMaxValue();
                    TeleportManager.SetNotification("⚡ Vitals replenished to 100%!");
                }
            });

            // Raft Teleportation & Recovery Row
            var teleRow = CreateBox(page.transform, "TeleportRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 36), Color.clear);
            EnsureLayout(teleRow, -1, 36);
            var teleLayout = teleRow.AddComponent<HorizontalLayoutGroup>();
            teleLayout.spacing = 8;
            teleLayout.childForceExpandWidth = true;

            CreateButton(teleRow.transform, "Btn_TeleToRaft", "⚡ Recall to Raft [F8]", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
            {
                TeleportManager.TeleportPlayerToRaft();
            }, new Color(0.80f, 0.15f, 0.18f, 0.95f), Color.white, 14);

            CreateButton(teleRow.transform, "Btn_SummonRaft", "⛵ Summon Raft Here [F9]", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
            {
                TeleportManager.TeleportRaftToPlayer();
            }, new Color(0.18f, 0.18f, 0.23f), Color.white, 14);

            CreateButton(teleRow.transform, "Btn_ToggleAnchor", "⚓ Toggle Anchor", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
            {
                TeleportManager.ToggleRaftAnchor();
            }, new Color(0.18f, 0.18f, 0.23f), Color.white, 14);

            // Time buttons row
            var timeRow = CreateBox(page.transform, "TimeRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 34), Color.clear);
            EnsureLayout(timeRow, -1, 34);
            var timeLayout = timeRow.AddComponent<HorizontalLayoutGroup>();
            timeLayout.spacing = 8;
            timeLayout.childForceExpandWidth = true;

            CreateButton(timeRow.transform, "Btn_Morning", "🌅 Morning (08:00)", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () => SetTime(8f), new Color(0.16f, 0.16f, 0.20f), Color.white, 13);
            CreateButton(timeRow.transform, "Btn_Noon", "☀️ Noon (12:00)", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () => SetTime(12f), new Color(0.16f, 0.16f, 0.20f), Color.white, 13);
            CreateButton(timeRow.transform, "Btn_Night", "🌙 Night (22:00)", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () => SetTime(22f), new Color(0.16f, 0.16f, 0.20f), Color.white, 13);

            // Weather buttons row
            var weatherRow = CreateBox(page.transform, "WeatherRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 34), Color.clear);
            EnsureLayout(weatherRow, -1, 34);
            var weatherLayout = weatherRow.AddComponent<HorizontalLayoutGroup>();
            weatherLayout.spacing = 8;
            weatherLayout.childForceExpandWidth = true;

            CreateButton(weatherRow.transform, "Btn_Sunny", "☀️ Sunny", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () => SetWeather(UniqueWeatherType.Default), new Color(0.16f, 0.16f, 0.20f), Color.white, 13);
            CreateButton(weatherRow.transform, "Btn_Calm", "🌊 Calm", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () => SetWeather(UniqueWeatherType.Calm), new Color(0.16f, 0.16f, 0.20f), Color.white, 13);
            CreateButton(weatherRow.transform, "Btn_Rain", "🌧️ Rain", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () => SetWeather(UniqueWeatherType.Rain), new Color(0.16f, 0.16f, 0.20f), Color.white, 13);
            CreateButton(weatherRow.transform, "Btn_Fog", "🌫️ Fog", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () => SetWeather(UniqueWeatherType.Fog), new Color(0.16f, 0.16f, 0.20f), Color.white, 13);

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
            layout.padding = new RectOffset(14, 14, 10, 10);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateToggleItem(page.transform, "🧭 Show On-Screen HUD Overlay (Hotkey: [F6])", Plugin.EnableHUD.Value, v =>
            {
                Plugin.EnableHUD.Value = v;
            });

            // HUD Style Selection Row
            var styleRow = CreateBox(page.transform, "HUDStyleRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 38), new Color(0.10f, 0.10f, 0.13f, 0.95f));
            EnsureLayout(styleRow, -1, 38);
            var styleLayout = styleRow.AddComponent<HorizontalLayoutGroup>();
            styleLayout.spacing = 8;
            styleLayout.padding = new RectOffset(12, 12, 3, 3);
            styleLayout.childForceExpandHeight = true;

            var styleLabel = CreateText(styleRow.transform, "StyleLabel", "HUD Style:", 14, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            EnsureLayout(styleLabel.gameObject, 80, 32, false);

            string[] styleNames = { "Sleek Ribbon", "Compass Bar", "Mini Pill", "Classic Box" };
            for (int s = 0; s < styleNames.Length; s++)
            {
                int styleIdx = s;
                var sBtn = CreateButton(styleRow.transform, $"Btn_Style_{s}", styleNames[s], Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(105, 32), () =>
                {
                    HUDOverlay.SetStyle(styleIdx);
                }, (Plugin.HUDStyle != null && Plugin.HUDStyle.Value == s) ? new Color(0.85f, 0.15f, 0.20f, 1f) : new Color(0.14f, 0.14f, 0.18f, 0.92f), Color.white, 13);
                EnsureLayout(sBtn, 105, 32, false);
            }

            // Quick Teleport Row
            var navTeleRow = CreateBox(page.transform, "NavTeleRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 36), Color.clear);
            EnsureLayout(navTeleRow, -1, 36);
            var navTeleLayout = navTeleRow.AddComponent<HorizontalLayoutGroup>();
            navTeleLayout.spacing = 8;
            navTeleLayout.childForceExpandWidth = true;

            CreateButton(navTeleRow.transform, "Btn_NavTeleToRaft", "⚡ Recall to Raft [F8]", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
            {
                TeleportManager.TeleportPlayerToRaft();
            }, new Color(0.80f, 0.15f, 0.18f, 0.95f), Color.white, 14);

            CreateButton(navTeleRow.transform, "Btn_NavSummonRaft", "⛵ Summon Raft Here [F9]", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
            {
                TeleportManager.TeleportRaftToPlayer();
            }, new Color(0.18f, 0.18f, 0.23f), Color.white, 14);

            CreateButton(navTeleRow.transform, "Btn_NavAnchor", "⚓ Toggle Anchor", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, () =>
            {
                TeleportManager.ToggleRaftAnchor();
            }, new Color(0.18f, 0.18f, 0.23f), Color.white, 14);

            // Live Navigation Data Box
            var statusBox = CreateBox(page.transform, "StatusBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 150), new Color(0.10f, 0.10f, 0.13f, 0.95f));
            EnsureLayout(statusBox, -1, 150);
            var boxLayout = statusBox.AddComponent<VerticalLayoutGroup>();
            boxLayout.padding = new RectOffset(16, 16, 10, 10);
            boxLayout.spacing = 6;
            boxLayout.childForceExpandWidth = true;

            var title = CreateText(statusBox.transform, "NavTitle", "<b>🧭 <color=#EF4444>Live Navigation</color> & Shark Radar Data</b>", 16, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            EnsureLayout(title.gameObject, -1, 24);

            _navStatusText = CreateText(statusBox.transform, "NavStatus", "Loading live navigation data...", 14, FontStyle.Normal, Color.white, TextAnchor.UpperLeft);
            EnsureLayout(_navStatusText.gameObject, -1, 100);

            // Version & Update Check Row
            var verRow = CreateBox(page.transform, "VerRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 36), new Color(0.09f, 0.09f, 0.12f, 0.95f));
            EnsureLayout(verRow, -1, 36);
            var verLayout = verRow.AddComponent<HorizontalLayoutGroup>();
            verLayout.padding = new RectOffset(14, 14, 2, 2);
            verLayout.spacing = 10;
            verLayout.childForceExpandHeight = true;

            CreateText(verRow.transform, "VerLabel", $"⚓ Sailor's Companion <b>v{PluginInfo.PLUGIN_VERSION}</b>", 13, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);

            CreateButton(verRow.transform, "Btn_CheckUpdates", "🔄 Check for Updates", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(170, 30), () =>
            {
                UpdateChecker.Dismissed = false;
                UpdateChecker.Instance?.TriggerCheck();
                TeleportManager.SetNotification("Checking GitHub for mod updates...");
            }, new Color(0.80f, 0.15f, 0.18f, 0.95f), Color.white, 13);

            return page;
        }
        // ============================================================================
        // [END] TAB 2: NAVIGATION HUD & SHARK RADAR
        // ============================================================================
        #endregion

        #region [START] TAB 3: RESEARCH & R&D BLUEPRINTS
        // ============================================================================
        // [START] TAB 3: RESEARCH & R&D BLUEPRINTS (Instant Learn All Recipes)
        // ============================================================================
        private GameObject BuildResearchTab(Transform parent)
        {
            var page = CreateBox(parent, "Page_Research", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14;
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var infoBox = CreateBox(page.transform, "InfoBox", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 130), new Color(0.10f, 0.10f, 0.13f, 0.95f));
            EnsureLayout(infoBox, -1, 130);
            var boxLayout = infoBox.AddComponent<VerticalLayoutGroup>();
            boxLayout.padding = new RectOffset(18, 18, 12, 12);
            boxLayout.spacing = 8;
            boxLayout.childForceExpandWidth = true;

            var title = CreateText(infoBox.transform, "Title", "🔬 <b><color=#EF4444>Research Table</color> & Blueprint Automation (R&D)</b>", 17, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            EnsureLayout(title.gameObject, -1, 26);

            var desc = CreateText(infoBox.transform, "Desc", "Instantly researches and learns all crafting recipes, tools, weapons, furniture, engines, and blueprint items in the game without requiring you to find or sacrifice materials at the Research Table.", 14, FontStyle.Normal, new Color(0.88f, 0.90f, 0.94f), TextAnchor.UpperLeft);
            EnsureLayout(desc.gameObject, -1, 60);

            _researchStatusText = CreateText(page.transform, "Status", "<color=#CBD5E1>Status: Ready. Click below to unlock all items.</color>", 14, FontStyle.Italic, Color.white, TextAnchor.MiddleCenter);
            EnsureLayout(_researchStatusText.gameObject, -1, 26);

            var unlockBtn = CreateButton(page.transform, "Btn_UnlockAllRD", "⚡ UNLOCK ALL R&D RECIPES & BLUEPRINTS NOW", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 50), () =>
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
                        _researchStatusText.text = "<color=#34D399><b>✅ SUCCESS: All R&D recipes and blueprints unlocked!</b></color>";
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
            }, new Color(0.85f, 0.15f, 0.20f, 1f), Color.white, 15);
            EnsureLayout(unlockBtn, -1, 50);

            return page;
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
            var page = CreateBox(parent, "Page_Spawner", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(14, 14, 10, 10);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var banner = CreateBox(page.transform, "Banner", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 38), new Color(0.10f, 0.10f, 0.13f, 0.95f));
            EnsureLayout(banner, -1, 38);
            var bannerTxt = CreateText(banner.transform, "Txt", "📦 <b>Item Spawner:</b> Search any item in Raft and add stacks directly into your inventory.", 14, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter);

            // Search row
            var searchRow = CreateBox(page.transform, "SearchRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 38), Color.clear);
            EnsureLayout(searchRow, -1, 38);
            var searchLayout = searchRow.AddComponent<HorizontalLayoutGroup>();
            searchLayout.spacing = 8;

            var inputGO = CreateBox(searchRow.transform, "InputSearch", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(480, 36), new Color(0.12f, 0.12f, 0.16f));
            EnsureLayout(inputGO, 480, 36, false);
            var inputTxt = CreateText(inputGO.transform, "Text", "", 15, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            inputTxt.rectTransform.offsetMin = new Vector2(12, 0);
            _itemSearchInput = inputGO.AddComponent<InputField>();
            _itemSearchInput.textComponent = inputTxt;
            _itemSearchInput.onValueChanged.AddListener(s => RefreshItemSpawnerList(s));

            var clearBtn = CreateButton(searchRow.transform, "Btn_Clear", "✕ Clear", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(90, 36), () =>
            {
                if (_itemSearchInput != null) _itemSearchInput.text = "";
                RefreshItemSpawnerList("");
            }, new Color(0.80f, 0.15f, 0.18f, 0.95f), Color.white, 14);
            EnsureLayout(clearBtn, 90, 36, false);

            var refreshBtn = CreateButton(searchRow.transform, "Btn_Refresh", "🔄 Refresh", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(110, 36), () => RefreshItemSpawnerList(_itemSearchInput?.text ?? ""), new Color(0.18f, 0.18f, 0.23f), Color.white, 14);
            EnsureLayout(refreshBtn, 110, 36, false);

            // Scroll View
            var scrollGO = CreateBox(page.transform, "ScrollView", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 310), new Color(0.06f, 0.06f, 0.08f, 0.75f));
            EnsureLayout(scrollGO, -1, 310);
            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewport = CreateBox(scrollGO.transform, "Viewport", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.clear);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            var contentGO = CreateBox(viewport.transform, "Content", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero, Color.clear);
            var contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentGO.GetComponent<RectTransform>();

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
                _allItems = ItemManager.GetAllItems();
                if (_allItems == null || _allItems.Count == 0)
                {
                    var found = Resources.FindObjectsOfTypeAll<Item_Base>();
                    if (found != null && found.Length > 0)
                    {
                        _allItems = found.Distinct().ToList();
                    }
                }
            }

            if (_allItems == null || _allItems.Count == 0)
            {
                var row = CreateBox(_itemScrollContent, "NoticeRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 70), new Color(0.11f, 0.11f, 0.15f, 0.92f));
                EnsureLayout(row, -1, 70);
                CreateText(row.transform, "NoticeTxt", "💡 <b>Items load when you load into a game world.</b>\nEnter a game world to browse and spawn all 300+ items directly into your inventory!", 15, FontStyle.Normal, new Color(0.9f, 0.94f, 0.98f), TextAnchor.MiddleCenter);
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
                var emptyRow = CreateBox(_itemScrollContent, "EmptyRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 80), new Color(0.11f, 0.11f, 0.15f, 0.92f));
                EnsureLayout(emptyRow, -1, 80);
                CreateText(emptyRow.transform, "EmptyTxt", $"🔍 <b>No items found matching \"{filter}\"</b>\nTry searching: <b>Hammer</b>, <b>Plank</b>, <b>Plastic</b>, <b>Scrap</b>, <b>Titanium</b>, or click <b>Clear</b>.", 15, FontStyle.Normal, new Color(0.9f, 0.94f, 0.98f), TextAnchor.MiddleCenter);
                return;
            }

            foreach (var item in filtered)
            {
                var row = CreateBox(_itemScrollContent, $"Item_{item.UniqueIndex}", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 40), new Color(0.11f, 0.11f, 0.15f, 0.92f));
                EnsureLayout(row, -1, 40);
                var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
                rowLayout.spacing = 8;
                rowLayout.padding = new RectOffset(12, 12, 0, 0);
                rowLayout.childForceExpandHeight = false;

                string disp = item.settings_Inventory?.DisplayName;
                if (string.IsNullOrEmpty(disp)) disp = item.UniqueName;

                var nameTxt = CreateText(row.transform, "Name", $"<b>{disp}</b> <size=13><color=#94A3B8>({item.UniqueName})</color></size>", 15, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
                EnsureLayout(nameTxt.gameObject, 460, 32, true);

                string uniqueName = item.UniqueName;
                int stack = item.settings_Inventory != null ? item.settings_Inventory.StackSize : 20;

                var btn1 = CreateButton(row.transform, "Btn_1", "+1", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(52, 30), () => GiveItem(uniqueName, 1), new Color(0.18f, 0.18f, 0.23f), Color.white, 14);
                EnsureLayout(btn1, 52, 30, false);

                var btn10 = CreateButton(row.transform, "Btn_10", "+10", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(52, 30), () => GiveItem(uniqueName, 10), new Color(0.18f, 0.18f, 0.23f), Color.white, 14);
                EnsureLayout(btn10, 52, 30, false);

                var btnStack = CreateButton(row.transform, "Btn_Stack", $"+{stack}", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(70, 30), () => GiveItem(uniqueName, stack), new Color(0.85f, 0.15f, 0.20f, 1f), Color.white, 14);
                EnsureLayout(btnStack, 70, 30, false);
            }
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
                    _tabButtonImages[i].color = (i == tabIndex) ? new Color(0.85f, 0.15f, 0.20f, 1f) : new Color(0.13f, 0.13f, 0.17f, 0.92f);
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

            return go;
        }

        #region [START] UI TOGGLE ITEM WITH DUAL ON/OFF BUTTONS
        // ============================================================================
        // [START] UI TOGGLE ITEM WITH DUAL ON/OFF BUTTONS
        // ============================================================================
        private void CreateToggleItem(Transform parent, string label, bool initialValue, Action<bool> onToggle)
        {
            var row = CreateBox(parent, "ToggleRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 44), new Color(0.11f, 0.11f, 0.15f, 0.90f));
            EnsureLayout(row, -1, 44);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(14, 14, 4, 4);
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            // Feature Label
            var t = CreateText(row.transform, "Label", label, 15, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            EnsureLayout(t.gameObject, 680, 36, true);

            // Container for ON / OFF Buttons
            var btnGroup = CreateBox(row.transform, "BtnGroup", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(140, 34), Color.clear);
            EnsureLayout(btnGroup, 140, 34, false);
            var bgLayout = btnGroup.AddComponent<HorizontalLayoutGroup>();
            bgLayout.spacing = 6;
            bgLayout.childForceExpandWidth = true;
            bgLayout.childForceExpandHeight = true;

            bool state = initialValue;

            Color activeOnColor = new Color(0.85f, 0.15f, 0.20f, 1f); // Vibrant Crimson Red
            Color inactiveColor = new Color(0.16f, 0.16f, 0.20f, 0.95f); // Dark Slate Charcoal
            Color activeOffColor = new Color(0.35f, 0.12f, 0.15f, 0.95f); // Muted Dark Burgundy

            var onBtn = CreateButton(btnGroup.transform, "Btn_ON", "ON", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, null, state ? activeOnColor : inactiveColor, state ? Color.white : new Color(0.6f, 0.6f, 0.7f), 13);
            var offBtn = CreateButton(btnGroup.transform, "Btn_OFF", "OFF", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, null, !state ? activeOffColor : inactiveColor, !state ? Color.white : new Color(0.6f, 0.6f, 0.7f), 13);

            var onImg = onBtn.GetComponent<Image>();
            var onTxt = onBtn.GetComponentInChildren<Text>();
            var offImg = offBtn.GetComponent<Image>();
            var offTxt = offBtn.GetComponentInChildren<Text>();

            void UpdateVisuals(bool isOn)
            {
                onImg.color = isOn ? activeOnColor : inactiveColor;
                onTxt.color = isOn ? Color.white : new Color(0.6f, 0.6f, 0.7f);
                onTxt.fontStyle = isOn ? FontStyle.Bold : FontStyle.Normal;

                offImg.color = !isOn ? activeOffColor : inactiveColor;
                offTxt.color = !isOn ? Color.white : new Color(0.6f, 0.6f, 0.7f);
                offTxt.fontStyle = !isOn ? FontStyle.Bold : FontStyle.Normal;
            }

            UpdateVisuals(state);

            onBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!state)
                {
                    state = true;
                    UpdateVisuals(state);
                    onToggle?.Invoke(true);
                }
            });

            offBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (state)
                {
                    state = false;
                    UpdateVisuals(state);
                    onToggle?.Invoke(false);
                }
            });
        }
        // ============================================================================
        // [END] UI TOGGLE ITEM WITH DUAL ON/OFF BUTTONS
        // ============================================================================
        #endregion

        private void CreateButtonItem(Transform parent, string label, Action onClick)
        {
            var btn = CreateButton(parent, "ButtonItem", label, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 44), onClick, new Color(0.80f, 0.16f, 0.20f, 0.95f), Color.white, 15);
            EnsureLayout(btn, -1, 44);
        }

        private void CreateStepperItem(Transform parent, string label, float min, float max, float step, float initialValue, string unit, Action<float> onChange)
        {
            var row = CreateBox(parent, "StepperRow", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(0, 42), new Color(0.11f, 0.11f, 0.15f, 0.90f));
            EnsureLayout(row, -1, 42);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.padding = new RectOffset(14, 14, 0, 0);
            layout.childForceExpandHeight = false;

            float currentVal = initialValue;

            var labelTxt = CreateText(row.transform, "Label", $"{label}: <b>{currentVal:F1}{unit}</b>", 16, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
            EnsureLayout(labelTxt.gameObject, 520, 34, true);

            var minusBtn = CreateButton(row.transform, "Btn_Minus", "  -  ", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(54, 32), () =>
            {
                currentVal = Mathf.Max(min, currentVal - step);
                labelTxt.text = $"{label}: <b>{currentVal:F1}{unit}</b>";
                onChange?.Invoke(currentVal);
            }, new Color(0.18f, 0.18f, 0.22f), Color.white, 18);
            EnsureLayout(minusBtn, 54, 32, false);

            var plusBtn = CreateButton(row.transform, "Btn_Plus", "  +  ", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(54, 32), () =>
            {
                currentVal = Mathf.Min(max, currentVal + step);
                labelTxt.text = $"{label}: <b>{currentVal:F1}{unit}</b>";
                onChange?.Invoke(currentVal);
            }, new Color(0.85f, 0.15f, 0.20f), Color.white, 18);
            EnsureLayout(plusBtn, 54, 32, false);
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
                    Plugin.EnableHUD.Value = !Plugin.EnableHUD.Value;
                }
                KeyCode keyFly = Plugin.KeyFly != null ? Plugin.KeyFly.Value : KeyCode.F;
                if (InputHelper.WasKeyPressed(keyFly) || InputHelper.WasKeyPressed(KeyCode.F7))
                {
                    Plugin.EnableFlyMode.Value = !Plugin.EnableFlyMode.Value;
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

                // Update live Navigation tab data if visible (throttled to 5Hz to prevent frame lag)
                if (_activeTab == 2 && _navStatusText != null && Time.unscaledTime - _lastNavTabUpdate > 0.2f)
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

        private void UpdateNavTabText()
        {
            var p = PlayerHelper.GetLocalPlayer();
            if (p == null)
            {
                _navStatusText.text = "<color=#94A3B8>Enter a game world to see live navigation, raft tracking, and shark distance data.</color>";
                return;
            }

            if (_cachedNavCamera == null) _cachedNavCamera = Camera.main;
            float yaw = _cachedNavCamera != null ? _cachedNavCamera.transform.eulerAngles.y : p.transform.eulerAngles.y;
            string[] cardinals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            int cIndex = Mathf.RoundToInt(yaw / 45f) % 8;
            if (cIndex < 0) cIndex += 8;

            string raftStr = "Raft: Not detected";
            if (_cachedNavRaft == null || !_cachedNavRaft.gameObject.activeInHierarchy)
            {
                _cachedNavRaft = ComponentManager<Raft>.Value ?? FindObjectOfType<Raft>();
            }
            if (_cachedNavRaft != null)
            {
                float dist = Vector3.Distance(p.transform.position, _cachedNavRaft.transform.position);
                raftStr = $"Raft: <b>{dist:F0}m</b> away  |  State: <b>{(_cachedNavRaft.IsAnchored ? "Anchored" : "Drifting")}</b>  |  Speed: <b>{_cachedNavRaft.Velocity.magnitude * 1.94f:F1} knots</b>";
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
            }

            string notifStr = "";
            if (!string.IsNullOrEmpty(TeleportManager.LastStatusMessage) && (Time.unscaledTime - TeleportManager.LastStatusTime < 8.0f))
            {
                notifStr = $"\n<color=#EF4444><b>Notification:</b> {TeleportManager.LastStatusMessage}</color>";
            }

            _navStatusText.text = $"• Player Position: <b>X: {p.transform.position.x:F1}, Y: {p.transform.position.y:F1}, Z: {p.transform.position.z:F1}</b>\n" +
                                  $"• Facing Direction: <b>{yaw:000}° ({cardinals[cIndex]})</b>\n" +
                                  $"• {raftStr}\n" +
                                  $"• {sharkStr}{notifStr}\n\n" +
                                  $"<size=13><color=#CBD5E1>Hotkeys: [F5] Menu  |  [F6] HUD  |  [F] Fly  |  [F8] Recall to Raft  |  [F9] Summon Raft</color></size>";
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
