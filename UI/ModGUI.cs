using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SailorsCompanion.UI
{
    public class ModGUI : MonoBehaviour
    {
        #region [START] MODGUI: FIELDS & THEME STATE
        // ============================================================================
        // [START] MODGUI: FIELDS & THEME STATE
        // ============================================================================
        public static bool IsOpen = false;

        private Rect _windowRect = new Rect(100, 100, 640, 530);
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "🛡️ Survival", "🦈 Raft & World", "🔬 R&D / Blueprints", "📦 Item Spawner", "🧭 Navigation" };

        private Vector2 _scrollPosSurvival = Vector2.zero;
        private Vector2 _scrollPosWorld = Vector2.zero;
        private Vector2 _scrollPosSpawner = Vector2.zero;
        private string _itemSearchText = "";

        private List<Item_Base> _cachedItems = null;
        private GUIStyle _winStyle = null;
        private GUIStyle _tabButtonStyle = null;
        private GUIStyle _tabButtonSelectedStyle = null;
        private GUIStyle _headerStyle = null;
        private GUIStyle _subHeaderStyle = null;
        private GUIStyle _cardBoxStyle = null;
        private GUIStyle _cornerBtnStyle = null;
        private Texture2D _winTex = null;
        private Texture2D _cardTex = null;
        private Texture2D _tabSelectedTex = null;
        private Texture2D _tabUnselectedTex = null;
        private Texture2D _cornerBtnTex = null;
        #endregion // [END] MODGUI: FIELDS & THEME STATE

        #region [START] MODGUI: STYLES & TEXTURE GENERATION
        // ============================================================================
        // [START] MODGUI: STYLES & TEXTURE GENERATION
        // ============================================================================
        private Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void EnsureStyles()
        {
            if (_winTex == null) _winTex = MakeTex(1, 1, new Color(0.07f, 0.09f, 0.13f, 0.97f));
            if (_cardTex == null) _cardTex = MakeTex(1, 1, new Color(0.11f, 0.14f, 0.19f, 0.88f));
            if (_tabSelectedTex == null) _tabSelectedTex = MakeTex(1, 1, new Color(0.06f, 0.52f, 0.76f, 0.95f));
            if (_tabUnselectedTex == null) _tabUnselectedTex = MakeTex(1, 1, new Color(0.14f, 0.18f, 0.24f, 0.90f));
            if (_cornerBtnTex == null) _cornerBtnTex = MakeTex(1, 1, new Color(0.08f, 0.14f, 0.22f, 0.92f));

            if (_winStyle == null)
            {
                _winStyle = new GUIStyle(GUI.skin.window)
                {
                    normal = { background = _winTex, textColor = Color.white },
                    onNormal = { background = _winTex, textColor = Color.white },
                    padding = new RectOffset(14, 14, 24, 14)
                };
            }

            if (_cardBoxStyle == null)
            {
                _cardBoxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = _cardTex },
                    padding = new RectOffset(12, 12, 10, 10),
                    margin = new RectOffset(0, 0, 6, 6)
                };
            }

            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.38f, 0.85f, 0.98f) }
                };
            }

            if (_subHeaderStyle == null)
            {
                _subHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.94f, 0.96f, 0.98f) }
                };
            }

            if (_tabButtonStyle == null)
            {
                _tabButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Normal,
                    normal = { background = _tabUnselectedTex, textColor = new Color(0.80f, 0.85f, 0.90f) },
                    hover = { background = _tabSelectedTex, textColor = Color.white },
                    fixedHeight = 34
                };
            }

            if (_tabButtonSelectedStyle == null)
            {
                _tabButtonSelectedStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    normal = { background = _tabSelectedTex, textColor = Color.white },
                    fixedHeight = 34
                };
            }

            if (_cornerBtnStyle == null)
            {
                _cornerBtnStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    normal = { background = _cornerBtnTex, textColor = new Color(0.38f, 0.85f, 0.98f) },
                    hover = { background = _tabSelectedTex, textColor = Color.white }
                };
            }
        }
        #endregion // [END] MODGUI: STYLES & TEXTURE GENERATION

        #region [START] MODGUI: TOGGLE & CURSOR CONTROL
        // ============================================================================
        // [START] MODGUI: TOGGLE & CURSOR CONTROL
        // ============================================================================
        public static void Toggle()
        {
            IsOpen = !IsOpen;
            if (IsOpen)
            {
                try { Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None); }
                catch
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            else
            {
                var p = PlayerHelper.GetLocalPlayer();
                if (p != null)
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

        private void Update()
        {
            if (IsOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        #endregion // [END] MODGUI: TOGGLE & CURSOR CONTROL

        #region [START] MODGUI: ONGUI & WINDOW DISPATCHER
        // ============================================================================
        // [START] MODGUI: ONGUI & WINDOW DISPATCHER
        // ============================================================================
        private void OnGUI()
        {
            EnsureStyles();
            GUI.depth = -2000; // Always top render depth

            // Corner toggle button in gameplay
            if (!CanvasModUI.IsWindowOpen && !IsOpen && PlayerHelper.GetLocalPlayer() != null)
            {
                Rect btnRect = new Rect(Screen.width - 180f, 14f, 166f, 32f);
                if (GUI.Button(btnRect, "⚓ Mod Menu [F5]", _cornerBtnStyle))
                {
                    CanvasModUI.Instance?.ToggleModWindow();
                }
            }

            if (!IsOpen) return;

            // Keep window on screen
            _windowRect.x = Mathf.Clamp(_windowRect.x, 10f, Screen.width - _windowRect.width - 10f);
            _windowRect.y = Mathf.Clamp(_windowRect.y, 10f, Screen.height - _windowRect.height - 10f);

            _windowRect = GUI.Window(98234, _windowRect, DrawWindow, "⚓ Sailor's Companion — Quality of Life & Utilities");
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // Tabs Row
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _tabNames.Length; i++)
            {
                var style = (i == _selectedTab) ? _tabButtonSelectedStyle : _tabButtonStyle;
                if (GUILayout.Button(_tabNames[i], style))
                {
                    _selectedTab = i;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Tab Content
            switch (_selectedTab)
            {
                case 0:
                    DrawSurvivalTab();
                    break;
                case 1:
                    DrawWorldTab();
                    break;
                case 2:
                    DrawResearchTab();
                    break;
                case 3:
                    DrawItemSpawnerTab();
                    break;
                case 4:
                    DrawNavigationTab();
                    break;
            }

            GUILayout.FlexibleSpace();

            // Footer
            GUILayout.BeginHorizontal();
            GUILayout.Label("<size=11><color=#64748B>Press F5 or ESC to close. Settings persist across sessions.</color></size>", GUI.skin.label);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("💬 Discord", GUILayout.Width(90), GUILayout.Height(26)))
            {
                Application.OpenURL("https://discord.gg/B4EMrR5Vrf");
            }
            if (GUILayout.Button("Close [ESC]", GUILayout.Width(100), GUILayout.Height(26)))
            {
                IsOpen = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 30));
        }
        #endregion // [END] MODGUI: ONGUI & WINDOW DISPATCHER

        #region [START] MODGUI: TAB 0 - SURVIVAL & VITALS
        // ============================================================================
        // [START] MODGUI: TAB 0 - SURVIVAL & VITALS
        // ============================================================================
        private void DrawSurvivalTab()
        {
            _scrollPosSurvival = GUILayout.BeginScrollView(_scrollPosSurvival);

            GUILayout.BeginVertical(_cardBoxStyle);
            GUILayout.Label("Survival & Vitals", _headerStyle);

            Plugin.GodMode.Value = GUILayout.Toggle(Plugin.GodMode.Value, " 🛡️ God Mode (Invulnerable to all damage)");
            Plugin.InfiniteOxygen.Value = GUILayout.Toggle(Plugin.InfiniteOxygen.Value, " 🤿 Infinite Oxygen (Dive freely without drowning)");
            Plugin.NoHungerThirst.Value = GUILayout.Toggle(Plugin.NoHungerThirst.Value, " 🥩 Freeze Hunger & Thirst (Never starve or dehydrate)");
            Plugin.InfiniteDurability.Value = GUILayout.Toggle(Plugin.InfiniteDurability.Value, " 🔨 Infinite Tool Durability (Tools, weapons & armor never break)");

            if (GUILayout.Button("⚡ Instant Max Vitals (Heal, Food, Water, Oxygen)", GUILayout.Height(28)))
            {
                var p = PlayerHelper.GetLocalPlayer();
                if (p?.Stats != null)
                {
                    p.Stats.stat_health?.SetToMaxValue();
                    p.Stats.stat_hunger?.Normal?.SetToMaxValue();
                    p.Stats.stat_thirst?.Normal?.SetToMaxValue();
                    p.Stats.stat_oxygen?.SetToMaxValue();
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(_cardBoxStyle);
            GUILayout.Label("Movement & Flight", _headerStyle);

            GUILayout.Label($"Swim Speed Multiplier: <b>{Plugin.SwimSpeedMultiplier.Value:F1}x</b> (Default: 1.0x)");
            Plugin.SwimSpeedMultiplier.Value = GUILayout.HorizontalSlider(Plugin.SwimSpeedMultiplier.Value, 1.0f, 4.0f);

            GUILayout.Label($"Sprint Speed Multiplier: <b>{Plugin.SprintSpeedMultiplier.Value:F1}x</b> (Default: 1.0x)");
            Plugin.SprintSpeedMultiplier.Value = GUILayout.HorizontalSlider(Plugin.SprintSpeedMultiplier.Value, 1.0f, 3.0f);

            GUILayout.Space(6);
            Plugin.EnableFlyMode.Value = GUILayout.Toggle(Plugin.EnableFlyMode.Value, " 🕊️ Fly / Noclip Mode (Hotkey: [F])");
            if (Plugin.EnableFlyMode.Value)
            {
                GUILayout.Label($"Fly Speed: <b>{Plugin.FlySpeed.Value:F0} m/s</b> (Hold LeftAlt for Turbo)");
                Plugin.FlySpeed.Value = GUILayout.HorizontalSlider(Plugin.FlySpeed.Value, 5.0f, 35.0f);
                GUILayout.Label("<size=11><color=#94A3B8>Controls: WASD to move, Space to ascend, LeftShift/Ctrl to descend.</color></size>");
            }
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }
        #endregion // [END] MODGUI: TAB 0 - SURVIVAL & VITALS

        #region [START] MODGUI: TAB 1 - RAFT & WORLD PROTECTION
        // ============================================================================
        // [START] MODGUI: TAB 1 - RAFT & WORLD PROTECTION
        // ============================================================================
        private void DrawWorldTab()
        {
            _scrollPosWorld = GUILayout.BeginScrollView(_scrollPosWorld);

            GUILayout.BeginVertical(_cardBoxStyle);
            GUILayout.Label("🔬 R&D (Research & Development)", _headerStyle);
            if (GUILayout.Button("⚡ Unlock All R&D Recipes & Blueprints", GUILayout.Height(30)))
            {
                UnlockAllResearch();
            }
            GUILayout.Label("<size=11><color=#94A3B8>Instantly learns all items and blueprints at the Research Table (R&D).</color></size>");
            GUILayout.EndVertical();

            GUILayout.BeginVertical(_cardBoxStyle);
            GUILayout.Label("Raft Protection & Gathering", _headerStyle);

            Plugin.AntiSharkRaftDamage.Value = GUILayout.Toggle(Plugin.AntiSharkRaftDamage.Value, " 🦈 Anti-Shark Raft Attacks (Stops shark from biting/destroying raft blocks)");
            Plugin.FreeCrafting.Value = GUILayout.Toggle(Plugin.FreeCrafting.Value, " 🛠️ Free Instant Crafting (Craft any recipe without materials)");

            GUILayout.Label($"Hook Pull Speed: <b>{Plugin.HookPullSpeedMultiplier.Value:F1}x</b> (Accelerates debris reeling)");
            Plugin.HookPullSpeedMultiplier.Value = GUILayout.HorizontalSlider(Plugin.HookPullSpeedMultiplier.Value, 1.0f, 5.0f);

            GUILayout.Label($"Resource Stack Size: <b>{Plugin.CustomStackSize.Value}</b> (Default: 20)");
            Plugin.CustomStackSize.Value = Mathf.RoundToInt(GUILayout.HorizontalSlider(Plugin.CustomStackSize.Value, 20f, 999f));
            GUILayout.EndVertical();

            GUILayout.BeginVertical(_cardBoxStyle);
            GUILayout.Label("Time & Sun Control", _headerStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🌅 Morning (08:00)")) SetGameTime(8f);
            if (GUILayout.Button("☀️ Noon (12:00)")) SetGameTime(12f);
            if (GUILayout.Button("🌙 Night (22:00)")) SetGameTime(22f);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label($"Time Scale (Simulation Speed): <b>{Time.timeScale:F1}x</b>");
            float newScale = GUILayout.HorizontalSlider(Time.timeScale, 0.5f, 5.0f);
            if (Math.Abs(newScale - Time.timeScale) > 0.05f)
            {
                Time.timeScale = newScale;
            }
            if (GUILayout.Button("Reset Time Scale (1.0x)", GUILayout.Width(160)))
            {
                Time.timeScale = 1.0f;
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(_cardBoxStyle);
            GUILayout.Label("Weather Control", _headerStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("☀️ Sunny/Default")) SetWeather(UniqueWeatherType.Default);
            if (GUILayout.Button("🌊 Calm")) SetWeather(UniqueWeatherType.Calm);
            if (GUILayout.Button("🌧️ Rain")) SetWeather(UniqueWeatherType.Rain);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🌫️ Fog")) SetWeather(UniqueWeatherType.Fog);
            if (GUILayout.Button("❄️ Snow")) SetWeather(UniqueWeatherType.Snow);
            if (GUILayout.Button("🌊 Big Waves")) SetWeather(UniqueWeatherType.BigWaves);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }
        #endregion // [END] MODGUI: TAB 1 - RAFT & WORLD PROTECTION

        #region [START] MODGUI: TAB 2 - R&D RECIPES RESEARCH
        // ============================================================================
        // [START] MODGUI: TAB 2 - R&D RECIPES RESEARCH
        // ============================================================================
        private void DrawResearchTab()
        {
            GUILayout.BeginVertical(_cardBoxStyle);
            GUILayout.Label("🔬 R&D / Blueprint Research", _headerStyle);
            GUILayout.Label("Instantly unlock all crafting recipes and blueprint items at the Research Table without materials.", _subHeaderStyle);
            GUILayout.Space(10);

            if (GUILayout.Button("⚡ UNLOCK ALL R&D RECIPES & BLUEPRINTS", GUILayout.Height(40)))
            {
                try
                {
                    var rt = ComponentManager<Inventory_ResearchTable>.Value ?? FindObjectOfType<Inventory_ResearchTable>();
                    if (rt != null)
                    {
                        rt.LearnAllRecipesInstantly();
                    }
                    Cheat.UnlockAllCrafting = true;
                    Debug.Log("[Sailor's Companion] Unlocked all R&D recipes and blueprints!");
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Sailor's Companion] R&D error: " + ex);
                }
            }
            GUILayout.EndVertical();
        }
        #endregion // [END] MODGUI: TAB 2 - R&D RECIPES RESEARCH

        #region [START] MODGUI: TAB 3 - ITEM SPAWNER
        // ============================================================================
        // [START] MODGUI: TAB 3 - ITEM SPAWNER
        // ============================================================================
        private void DrawItemSpawnerTab()
        {
            var player = PlayerHelper.GetLocalPlayer();
            if (player == null)
            {
                GUILayout.Label("<color=#F59E0B>Waiting for player to spawn into world...</color>", _headerStyle);
                return;
            }

            if (_cachedItems == null || _cachedItems.Count == 0)
            {
                _cachedItems = ItemManager.GetAllItems() ?? new List<Item_Base>();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search: ", GUILayout.Width(55));
            _itemSearchText = GUILayout.TextField(_itemSearchText, GUILayout.Height(24));
            if (GUILayout.Button("Clear", GUILayout.Width(60), GUILayout.Height(24)))
            {
                _itemSearchText = "";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            var filtered = _cachedItems.Where(i =>
                string.IsNullOrEmpty(_itemSearchText) ||
                (i.UniqueName != null && i.UniqueName.IndexOf(_itemSearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (i.settings_Inventory?.DisplayName != null && i.settings_Inventory.DisplayName.IndexOf(_itemSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
            ).Take(60).ToList();

            GUILayout.Label($"Showing {filtered.Count} items (filtered from {_cachedItems.Count}):", _subHeaderStyle);

            _scrollPosSpawner = GUILayout.BeginScrollView(_scrollPosSpawner);
            foreach (var item in filtered)
            {
                string dispName = item.settings_Inventory?.DisplayName;
                if (string.IsNullOrEmpty(dispName)) dispName = item.UniqueName;

                GUILayout.BeginHorizontal(_cardBoxStyle);
                GUILayout.Label($"<b>{dispName}</b> <size=11><color=#94A3B8>({item.UniqueName})</color></size>", GUILayout.Width(300));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("+1", GUILayout.Width(45), GUILayout.Height(24)))
                {
                    player.Inventory.AddItem(item.UniqueName, 1);
                }
                if (GUILayout.Button("+10", GUILayout.Width(48), GUILayout.Height(24)))
                {
                    player.Inventory.AddItem(item.UniqueName, 10);
                }
                int stack = item.settings_Inventory != null ? item.settings_Inventory.StackSize : 20;
                if (GUILayout.Button($"+{stack}", GUILayout.Width(58), GUILayout.Height(24)))
                {
                    player.Inventory.AddItem(item.UniqueName, stack);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
        #endregion // [END] MODGUI: TAB 3 - ITEM SPAWNER

        #region [START] MODGUI: TAB 4 - NAVIGATION TELEMETRY
        // ============================================================================
        // [START] MODGUI: TAB 4 - NAVIGATION TELEMETRY
        // ============================================================================
        private void DrawNavigationTab()
        {
            GUILayout.BeginVertical(_cardBoxStyle);
            GUILayout.Label("HUD & Compass Display", _headerStyle);

            Plugin.EnableHUD.Value = GUILayout.Toggle(Plugin.EnableHUD.Value, " 🧭 Show Navigation HUD Overlay (Hotkey: [F6])");

            GUILayout.Space(8);
            GUILayout.Label("Current Navigation Status:", _subHeaderStyle);

            var p = PlayerHelper.GetLocalPlayer();
            if (p != null)
            {
                var cam = Camera.main;
                float yaw = cam != null ? cam.transform.eulerAngles.y : p.transform.eulerAngles.y;
                GUILayout.Label($"• Position: <b>X: {p.transform.position.x:F1}, Y: {p.transform.position.y:F1}, Z: {p.transform.position.z:F1}</b>");
                GUILayout.Label($"• Facing: <b>{yaw:F0}°</b>");

                var raft = ComponentManager<Raft>.Value ?? UnityEngine.Object.FindObjectOfType<Raft>();
                if (raft != null)
                {
                    float dist = Vector3.Distance(p.transform.position, raft.transform.position);
                    GUILayout.Label($"• Raft Position: <b>({raft.transform.position.x:F0}, {raft.transform.position.z:F0})</b> | Distance: <b>{dist:F0}m</b>");
                    GUILayout.Label($"• Raft Status: <b>{(raft.IsAnchored ? "Anchored" : "Drifting")}</b> | Speed: <b>{raft.Velocity.magnitude * 1.94f:F1} knots</b>");
                }

                var shark = UnityEngine.Object.FindObjectOfType<AI_StateMachine_Shark>();
                if (shark != null && shark.gameObject.activeInHierarchy)
                {
                    float sharkDist = Vector3.Distance(p.transform.position, shark.transform.position);
                    GUILayout.Label($"• Bruce the Shark: <b>{sharkDist:F0}m away</b>");
                }
            }
            else
            {
                GUILayout.Label("<color=#94A3B8>Enter a game world to see live navigation data.</color>");
            }
            GUILayout.EndVertical();
        }
        #endregion // [END] MODGUI: TAB 4 - NAVIGATION TELEMETRY

        #region [START] MODGUI: WORLD & TIME HELPERS
        // ============================================================================
        // [START] MODGUI: WORLD & TIME HELPERS
        // ============================================================================
        private static void UnlockAllResearch()
        {
            try
            {
                var research = ComponentManager<Inventory_ResearchTable>.Value ?? UnityEngine.Object.FindObjectOfType<Inventory_ResearchTable>();
                if (research != null)
                {
                    research.LearnAllRecipesInstantly();
                }
                Cheat.UnlockAllCrafting = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to unlock R&D: " + ex);
            }
        }

        private static void SetGameTime(float hour)
        {
            var sky = UnityEngine.Object.FindObjectOfType<UnityEngine.AzureSky.AzureSkyController>();
            if (sky?.timeOfDay != null)
            {
                sky.timeOfDay.GotoTime(hour);
            }
        }

        private static void SetWeather(UniqueWeatherType weather)
        {
            var wm = ComponentManager<WeatherManager>.Value ?? UnityEngine.Object.FindObjectOfType<WeatherManager>();
            if (wm != null)
            {
                wm.SetWeather(weather, true);
            }
        }
        #endregion // [END] MODGUI: WORLD & TIME HELPERS

        #region [START] MODGUI: CLEANUP & DISPOSAL
        // ============================================================================
        // [START] MODGUI: CLEANUP & DISPOSAL
        // ============================================================================
        private void OnDestroy()
        {
            if (_winTex != null) Destroy(_winTex);
            if (_cardTex != null) Destroy(_cardTex);
            if (_tabSelectedTex != null) Destroy(_tabSelectedTex);
            if (_tabUnselectedTex != null) Destroy(_tabUnselectedTex);
            if (_cornerBtnTex != null) Destroy(_cornerBtnTex);
        }
        #endregion // [END] MODGUI: CLEANUP & DISPOSAL
    }
}
