using System;
using UnityEngine;
using SailorsCompanion.Features;

namespace SailorsCompanion.UI
{
    // ============================================================================
    // [START] MODULE: ON-SCREEN NAVIGATION HUD OVERLAY
    // Purpose: Renders real-time compass tape, coordinates, raft tracker arrow,
    //          shark proximity radar, and in-game notification toasts.
    // ============================================================================
    public class HUDOverlay : MonoBehaviour
    {
        #region [START] CACHED REFERENCES & DATA STATE
        // ============================================================================
        // [START] CACHED REFERENCES & DATA STATE
        // ============================================================================
        private GUIStyle _hudBoxStyle = null;
        private GUIStyle _hudTextStyle = null;
        private GUIStyle _hudTitleStyle = null;
        private GUIStyle _hudSmallStyle = null;
        private GUIStyle _hudCompassStyle = null;
        private GUIStyle _hudButtonStyle = null;
        private Texture2D _bgTexture = null;
        private Texture2D _btnNormalTex = null;
        private Texture2D _btnHoverTex = null;

        // Cached component references
        private static Raft _cachedRaft = null;
        private static AI_StateMachine_Shark _cachedShark = null;
        private static WeatherManager _cachedWeather = null;
        private static UnityEngine.AzureSky.AzureSkyController _cachedSky = null;
        private static Camera _cachedCamera = null;
        private static float _lastComponentRefresh = 0f;

        // Cached live data
        private static float _lastHudCalcTime = 0f;
        private static float _cachedYaw = 0f;
        private static string _cachedCardinal = "N";
        private static Vector3 _cachedPos = Vector3.zero;
        private static float _cachedRaftDist = 0f;
        private static string _cachedRaftArrow = "•";
        private static string _cachedRaftState = "Unknown";
        private static float _cachedSharkDist = -1f;
        private static string _cachedWeatherStr = "Normal";
        private static int _cachedHour = 0;
        private static int _cachedMinute = 0;
        private static int _cachedDay = 1;
        private static string _cachedBadges = "";

        public static readonly string[] StyleNames = { "Sleek Ribbon", "Compass Bar", "Mini Pill", "Classic Box" };
        // ============================================================================
        // [END] CACHED REFERENCES & DATA STATE
        // ============================================================================
        #endregion

        #region [START] GUI STYLES & TEXTURE INITIALIZATION
        // ============================================================================
        // [START] GUI STYLES & TEXTURE INITIALIZATION
        // ============================================================================
        private Texture2D MakeColorTexture(Color col)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        private void EnsureStyles()
        {
            if (_bgTexture == null)
            {
                _bgTexture = MakeColorTexture(new Color(0.06f, 0.08f, 0.12f, 0.88f));
            }
            if (_btnNormalTex == null)
            {
                _btnNormalTex = MakeColorTexture(new Color(0.14f, 0.20f, 0.28f, 0.90f));
            }
            if (_btnHoverTex == null)
            {
                _btnHoverTex = MakeColorTexture(new Color(0.06f, 0.52f, 0.76f, 0.95f));
            }

            if (_hudBoxStyle == null)
            {
                _hudBoxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = _bgTexture },
                    padding = new RectOffset(6, 6, 4, 4),
                    border = new RectOffset(0, 0, 0, 0)
                };
            }

            if (_hudTextStyle == null)
            {
                _hudTextStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Normal,
                    normal = { textColor = new Color(0.92f, 0.94f, 0.96f) },
                    richText = true,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (_hudTitleStyle == null)
            {
                _hudTitleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.38f, 0.85f, 0.98f) },
                    richText = true,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (_hudSmallStyle == null)
            {
                _hudSmallStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Normal,
                    normal = { textColor = new Color(0.80f, 0.85f, 0.90f) },
                    richText = true,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (_hudCompassStyle == null)
            {
                _hudCompassStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white },
                    richText = true,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (_hudButtonStyle == null)
            {
                _hudButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Normal,
                    normal = { background = _btnNormalTex, textColor = new Color(0.70f, 0.85f, 0.95f) },
                    hover = { background = _btnHoverTex, textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(4, 4, 2, 2)
                };
            }
        }
        // ============================================================================
        // [END] GUI STYLES & TEXTURE INITIALIZATION
        // ============================================================================
        #endregion

        #region [START] 10HZ DATA CALCULATION & LIFECYCLE
        // ============================================================================
        // [START] 10HZ DATA CALCULATION & LIFECYCLE
        // ============================================================================
        private void Update()
        {
            // Hotkey to cycle styles: Shift + F6
            if (InputHelper.WasKeyPressed(KeyCode.F6))
            {
                if (InputHelper.IsKeyHeld(KeyCode.LeftShift) || InputHelper.IsKeyHeld(KeyCode.RightShift))
                {
                    CycleStyle();
                }
            }

            if (!Plugin.EnableHUD.Value) return;

            // Refresh component references at most once per 2 seconds
            if (Time.unscaledTime - _lastComponentRefresh > 2.0f)
            {
                _lastComponentRefresh = Time.unscaledTime;
                try
                {
                    if (_cachedCamera == null) _cachedCamera = Camera.main;
                    if (_cachedRaft == null || !_cachedRaft.gameObject.activeInHierarchy)
                        _cachedRaft = ComponentManager<Raft>.Value ?? UnityEngine.Object.FindObjectOfType<Raft>();
                    if (_cachedShark == null || !_cachedShark.gameObject.activeInHierarchy)
                        _cachedShark = UnityEngine.Object.FindObjectOfType<AI_StateMachine_Shark>();
                    if (_cachedWeather == null)
                        _cachedWeather = ComponentManager<WeatherManager>.Value ?? UnityEngine.Object.FindObjectOfType<WeatherManager>();
                    if (_cachedSky == null)
                        _cachedSky = UnityEngine.Object.FindObjectOfType<UnityEngine.AzureSky.AzureSkyController>();
                }
                catch { }
            }

            // Recalculate HUD display data at 10Hz (every 100ms) to eliminate GC spikes
            if (Time.unscaledTime - _lastHudCalcTime > 0.1f)
            {
                _lastHudCalcTime = Time.unscaledTime;
                RecalculateHudData();
            }
        }

        public static void CycleStyle()
        {
            int current = Plugin.HUDStyle != null ? Plugin.HUDStyle.Value : 0;
            int next = (current + 1) % 4;
            if (Plugin.HUDStyle != null) Plugin.HUDStyle.Value = next;
        }

        public static void SetStyle(int styleIndex)
        {
            if (Plugin.HUDStyle != null)
            {
                Plugin.HUDStyle.Value = Mathf.Clamp(styleIndex, 0, 3);
            }
        }

        private void RecalculateHudData()
        {
            var player = PlayerHelper.GetLocalPlayer();
            if (player == null) return;

            // Badges
            string badges = "";
            if (Plugin.EnableFlyMode.Value) badges += " <color=#38BDF8>[FLY]</color>";
            if (Plugin.GodMode.Value) badges += " <color=#F87171>[GOD]</color>";
            if (Plugin.FreeCrafting.Value) badges += " <color=#A78BFA>[FREE CRAFT]</color>";
            if (Plugin.InfiniteOxygen.Value) badges += " <color=#34D399>[INF O2]</color>";
            if (Plugin.InfiniteDurability.Value) badges += " <color=#FBBF24>[INF DUR]</color>";
            _cachedBadges = badges;

            var cam = _cachedCamera ?? Camera.main;
            _cachedYaw = cam != null ? cam.transform.eulerAngles.y : player.transform.eulerAngles.y;
            _cachedCardinal = GetCardinalDirection(_cachedYaw);
            _cachedPos = player.transform.position;

            // Raft distance and directional tracking
            if (_cachedRaft != null)
            {
                _cachedRaftDist = Vector3.Distance(_cachedPos, _cachedRaft.transform.position);
                _cachedRaftState = _cachedRaft.IsAnchored ? "Anchored" : $"{_cachedRaft.Velocity.magnitude * 1.94f:F1} kts";
                _cachedRaftArrow = GetDirectionArrow(cam, _cachedPos, _cachedRaft.transform.position);
            }
            else
            {
                _cachedRaftDist = 0f;
                _cachedRaftState = "Searching...";
                _cachedRaftArrow = "•";
            }

            // Shark proximity radar
            if (_cachedShark != null && _cachedShark.gameObject.activeInHierarchy)
            {
                _cachedSharkDist = Vector3.Distance(_cachedPos, _cachedShark.transform.position);
            }
            else
            {
                _cachedSharkDist = -1f;
            }

            _cachedWeatherStr = _cachedWeather != null ? _cachedWeather.GetCurrentWeatherType().ToString() : "Normal";

            // World time
            if (_cachedSky?.timeOfDay != null)
            {
                _cachedHour = Mathf.FloorToInt(_cachedSky.timeOfDay.hour);
                _cachedMinute = Mathf.FloorToInt((_cachedSky.timeOfDay.hour - _cachedHour) * 60f);
                _cachedDay = _cachedSky.timeOfDay.dayOfYear;
            }
        }
        // ============================================================================
        // [END] 10HZ DATA CALCULATION & LIFECYCLE
        // ============================================================================
        #endregion

        #region [START] HUD MAIN ONGUI DISPATCHER
        // ============================================================================
        // [START] HUD MAIN ONGUI DISPATCHER
        // ============================================================================
        private void OnGUI()
        {
            if (!Plugin.EnableHUD.Value) return;

            var player = PlayerHelper.GetLocalPlayer();
            if (player == null) return;

            GUI.depth = -1000;
            EnsureStyles();

            int style = Plugin.HUDStyle != null ? Plugin.HUDStyle.Value : 0;
            switch (style)
            {
                case 1:
                    DrawStyleCompassBar();
                    break;
                case 2:
                    DrawStyleMiniPill();
                    break;
                case 3:
                    DrawStyleClassicBox();
                    break;
                case 0:
                default:
                    DrawStyleRibbon();
                    break;
            }

            DrawToastNotification();
        }
        // ============================================================================
        // [END] HUD MAIN ONGUI DISPATCHER
        // ============================================================================
        #endregion

        #region [START] HUD STYLES IMPLEMENTATION
        // ============================================================================
        // [START] HUD STYLES IMPLEMENTATION
        // ============================================================================

        // Style 0: Sleek Ribbon (Compact horizontal bar in top-left)
        private void DrawStyleRibbon()
        {
            float w = 550f;
            float h = string.IsNullOrEmpty(_cachedBadges) ? 34f : 52f;
            Rect r = new Rect(14f, 14f, w, h);
            GUI.Box(r, GUIContent.none, _hudBoxStyle);

            string sharkPart = _cachedSharkDist >= 0f ? $"  •  🦈 <color={(_cachedSharkDist < 25f ? "#EF4444" : "#10B981")}><b>{_cachedSharkDist:F0}m</b></color>" : "";
            string line1 = $"🧭 <color=#38BDF8><b>{_cachedYaw:000}° {_cachedCardinal}</b></color>  •  Pos: <b>({_cachedPos.x:F0}, {_cachedPos.z:F0})</b>  •  ⚓ <b>{_cachedRaftDist:F0}m</b> {_cachedRaftArrow}{sharkPart}  •  ☀️ <b>{_cachedHour:D2}:{_cachedMinute:D2}</b>";

            GUI.Label(new Rect(r.x + 10f, r.y + 6f, w - 85f, 22f), line1, _hudTextStyle);

            if (GUI.Button(new Rect(r.x + w - 70f, r.y + 6f, 62f, 22f), "Style ⟳", _hudButtonStyle))
            {
                CycleStyle();
            }

            if (!string.IsNullOrEmpty(_cachedBadges))
            {
                GUI.Label(new Rect(r.x + 10f, r.y + 28f, w - 20f, 20f), _cachedBadges, _hudSmallStyle);
            }
        }

        // Style 1: Top Compass Bar (Subnautica / Skyrim style horizontal ribbon)
        private void DrawStyleCompassBar()
        {
            float w = 520f;
            float h = 48f;
            float x = (Screen.width - w) * 0.5f;
            Rect r = new Rect(x, 10f, w, h);
            GUI.Box(r, GUIContent.none, _hudBoxStyle);

            string tape = $"··· <color=#94A3B8>{GetCardinalAt(_cachedYaw - 45f)}</color> · [ <color=#00E5FF><b>▲ {_cachedYaw:000}° {_cachedCardinal}</b></color> ] · <color=#94A3B8>{GetCardinalAt(_cachedYaw + 45f)}</color> ···";
            GUI.Label(new Rect(r.x + 10f, r.y + 4f, w - 85f, 20f), tape, _hudCompassStyle);

            string sharkPart = _cachedSharkDist >= 0f ? $"  |  🦈 <color={(_cachedSharkDist < 25f ? "#EF4444" : "#10B981")}><b>{_cachedSharkDist:F0}m</b></color>" : "";
            string sub = $"⚓ <b>{_cachedRaftDist:F0}m</b> {_cachedRaftArrow} ({_cachedRaftState})  |  Pos: ({_cachedPos.x:F0}, {_cachedPos.z:F0}){sharkPart}  |  ☀️ {_cachedHour:D2}:{_cachedMinute:D2}{_cachedBadges}";
            GUI.Label(new Rect(r.x + 10f, r.y + 25f, w - 85f, 18f), sub, _hudSmallStyle);

            if (GUI.Button(new Rect(r.x + w - 70f, r.y + 12f, 62f, 24f), "Style ⟳", _hudButtonStyle))
            {
                CycleStyle();
            }
        }

        // Style 2: Minimalist Mini Pill (Tiny single-line badge)
        private void DrawStyleMiniPill()
        {
            float w = 440f;
            float h = 28f;
            Rect r = new Rect(14f, 14f, w, h);
            GUI.Box(r, GUIContent.none, _hudBoxStyle);

            string sharkStr = _cachedSharkDist >= 0f ? $" • 🦈 <color={(_cachedSharkDist < 25f ? "#EF4444" : "#10B981")}><b>{_cachedSharkDist:F0}m</b></color>" : "";
            string text = $"🧭 <color=#38BDF8><b>{_cachedYaw:000}° {_cachedCardinal}</b></color> • ⚓ <b>{_cachedRaftDist:F0}m</b>{sharkStr} • ☀️ <b>{_cachedHour:D2}:{_cachedMinute:D2}</b>{_cachedBadges}";

            GUI.Label(new Rect(r.x + 8f, r.y + 4f, w - 76f, 20f), text, _hudTextStyle);

            if (GUI.Button(new Rect(r.x + w - 66f, r.y + 3f, 60f, 22f), "Style ⟳", _hudButtonStyle))
            {
                CycleStyle();
            }
        }

        // Style 3: Compact Classic Box (Cleaned-up smaller version of the original box)
        private void DrawStyleClassicBox()
        {
            float w = 270f;
            float h = 92f + (!string.IsNullOrEmpty(_cachedBadges) ? 18f : 0f);
            Rect r = new Rect(14f, 14f, w, h);
            GUI.Box(r, GUIContent.none, _hudBoxStyle);

            GUI.Label(new Rect(r.x + 10f, r.y + 6f, w - 75f, 20f), "<b>🧭 Sailor's Companion</b>", _hudTitleStyle);
            if (GUI.Button(new Rect(r.x + w - 65f, r.y + 6f, 56f, 20f), "Style ⟳", _hudButtonStyle))
            {
                CycleStyle();
            }

            GUI.Label(new Rect(r.x + 10f, r.y + 26f, w - 20f, 18f), $"Heading: <color=#38BDF8><b>{_cachedYaw:000}° {_cachedCardinal}</b></color>  ({_cachedPos.x:F0}, {_cachedPos.z:F0})", _hudSmallStyle);
            GUI.Label(new Rect(r.x + 10f, r.y + 44f, w - 20f, 18f), $"Raft: <b>{_cachedRaftDist:F0}m</b> {_cachedRaftArrow} | {_cachedRaftState}", _hudSmallStyle);

            string sharkStr = _cachedSharkDist >= 0f ? $"Bruce: <color={(_cachedSharkDist < 25f ? "#EF4444" : "#10B981")}><b>{_cachedSharkDist:F0}m</b></color>" : "Bruce: Calm";
            GUI.Label(new Rect(r.x + 10f, r.y + 62f, w - 20f, 18f), $"{sharkStr}  |  Time: <b>{_cachedHour:D2}:{_cachedMinute:D2}</b>", _hudSmallStyle);

            if (!string.IsNullOrEmpty(_cachedBadges))
            {
                GUI.Label(new Rect(r.x + 10f, r.y + 80f, w - 20f, 18f), _cachedBadges, _hudSmallStyle);
            }
        }
        // ============================================================================
        // [END] HUD STYLES IMPLEMENTATION
        // ============================================================================
        #endregion

        #region [START] TOAST NOTIFICATION ENGINE
        // ============================================================================
        // [START] TOAST NOTIFICATION ENGINE
        // ============================================================================
        private void DrawToastNotification()
        {
            if (string.IsNullOrEmpty(TeleportManager.LastStatusMessage)) return;
            float elapsed = Time.unscaledTime - TeleportManager.LastStatusTime;
            if (elapsed > 4.5f) return;

            float alpha = elapsed < 3.5f ? 1.0f : Mathf.Clamp01((4.5f - elapsed) / 1.0f);
            Color prevColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);

            float width = 450f;
            float height = 36f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - 110f;

            Rect r = new Rect(x, y, width, height);
            GUI.Box(r, GUIContent.none, _hudBoxStyle);

            GUIStyle toastStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.38f, 0.85f, 0.98f) },
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            GUI.Label(r, TeleportManager.LastStatusMessage, toastStyle);
            GUI.color = prevColor;
        }

        private void OnDestroy()
        {
            if (_bgTexture != null) Destroy(_bgTexture);
            if (_btnNormalTex != null) Destroy(_btnNormalTex);
            if (_btnHoverTex != null) Destroy(_btnHoverTex);
        }
        // ============================================================================
        // [END] TOAST NOTIFICATION ENGINE
        // ============================================================================
        #endregion

        #region [START] NAVIGATION MATH & DIRECTION HELPERS
        // ============================================================================
        // [START] NAVIGATION MATH & DIRECTION HELPERS
        // ============================================================================
        private static string GetCardinalDirection(float yaw)
        {
            string[] cardinals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            int index = Mathf.RoundToInt(yaw / 45f) % 8;
            if (index < 0) index += 8;
            return cardinals[index];
        }

        private static string GetCardinalAt(float angle)
        {
            while (angle < 0f) angle += 360f;
            while (angle >= 360f) angle -= 360f;
            return GetCardinalDirection(angle);
        }

        private static string GetDirectionArrow(Camera cam, Vector3 from, Vector3 to)
        {
            if (cam == null) return "•";
            Vector3 dir = to - from;
            dir.y = 0;
            if (dir.sqrMagnitude < 4f) return "⚓";

            float angle = Vector3.SignedAngle(cam.transform.forward, dir, Vector3.up);

            if (angle >= -22.5f && angle <= 22.5f) return "↑";
            if (angle > 22.5f && angle <= 67.5f) return "↗";
            if (angle > 67.5f && angle <= 112.5f) return "→";
            if (angle > 112.5f && angle <= 157.5f) return "↘";
            if (angle > 157.5f || angle <= -157.5f) return "↓";
            if (angle >= -157.5f && angle < -112.5f) return "↙";
            if (angle >= -112.5f && angle < -67.5f) return "←";
            return "↖";
        }
        // ============================================================================
        // [END] NAVIGATION MATH & DIRECTION HELPERS
        // ============================================================================
        #endregion
    }
    // ============================================================================
    // [END] MODULE: ON-SCREEN NAVIGATION HUD OVERLAY
    // ============================================================================
}
