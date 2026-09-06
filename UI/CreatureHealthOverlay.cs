using System;
using System.Collections.Generic;
using UnityEngine;
using SailorsCompanion.Features;

namespace SailorsCompanion.UI
{
    // ============================================================================
    // [START] MODULE: ANIMAL & ENEMY HEALTH BAR OVERLAY
    // Purpose: Renders world-to-screen health bars and distance tags over animals,
    //          sharks, predators, and domestic livestock within 35 meters.
    // ============================================================================
    public class CreatureHealthOverlay : MonoBehaviour
    {
        public static CreatureHealthOverlay Instance { get; private set; }

        private GUIStyle _nameStyle;
        private Texture2D _bgTexture;
        private Texture2D _fillTexture;
        private Texture2D _borderTexture;

        // Cached search list to avoid heavy GC allocation per frame
        private float _lastScanTime = 0f;
        private const float SCAN_INTERVAL = 1.5f; // re-scan entities every 1.5 seconds
        private readonly List<Network_Entity> _cachedEntities = new List<Network_Entity>();
        private static Camera _cachedCamera = null;
        private static readonly Dictionary<string, string> _nameCache = new Dictionary<string, string>();

        // ============================================================================
        // [START] LIFECYCLE INITIALIZATION
        // ============================================================================
        private void Awake()
        {
            Instance = this;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            InitTextures();
        }

        private void InitTextures()
        {
            _bgTexture = MakeSolidTexture(new Color(0.06f, 0.06f, 0.08f, 0.85f));
            _fillTexture = MakeSolidTexture(Color.white);
            _borderTexture = MakeSolidTexture(new Color(0.18f, 0.18f, 0.22f, 0.95f));
        }

        private Texture2D MakeSolidTexture(Color col)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        private void OnDestroy()
        {
            if (_bgTexture != null) Destroy(_bgTexture);
            if (_fillTexture != null) Destroy(_fillTexture);
            if (_borderTexture != null) Destroy(_borderTexture);
        }
        // ============================================================================
        // [END] LIFECYCLE INITIALIZATION
        // ============================================================================

        // ============================================================================
        // [START] PERIODIC ENTITY SCANNER
        // ============================================================================
        private void Update()
        {
            if (Plugin.ShowAnimalHealthBars == null || !Plugin.ShowAnimalHealthBars.Value)
            {
                if (_cachedEntities.Count > 0) _cachedEntities.Clear();
                return;
            }

            // Clean up any destroyed or dead entities continuously
            if (_cachedEntities.Count > 0)
            {
                _cachedEntities.RemoveAll(e => e == null || e.gameObject == null || e.stat_health == null || e.stat_health.IsZero || e.stat_health.Value <= 0f);
            }

            if (Time.unscaledTime - _lastScanTime >= SCAN_INTERVAL)
            {
                _lastScanTime = Time.unscaledTime;
                ScanNearbyEntities();
            }
        }

        private void ScanNearbyEntities()
        {
            var player = PlayerHelper.GetLocalPlayer();
            if (player == null)
            {
                _cachedEntities.Clear();
                return;
            }

            var playerPos = player.transform.position;
            var entities = FindObjectsOfType<Network_Entity>();
            if (entities == null || entities.Length == 0) return;

            _cachedEntities.Clear();
            foreach (var ent in entities)
            {
                if (ent == null || ent.gameObject == null) continue;
                if (ent.entityType == EntityType.Player) continue; // Skip human players
                if (ent.stat_health == null || ent.stat_health.IsZero || ent.stat_health.Value <= 0f) continue; // Skip dead

                float dist = Vector3.Distance(playerPos, ent.transform.position);
                if (dist <= 35f)
                {
                    _cachedEntities.Add(ent);
                }
            }
        }
        // ============================================================================
        // [END] PERIODIC ENTITY SCANNER
        // ============================================================================

        // ============================================================================
        // [START] IMGUI HEALTH BAR RENDERING
        // ============================================================================
        private void OnGUI()
        {
            // CRITICAL: Only render during Repaint event to eliminate 75% redundant CPU work and garbage allocation
            if (Event.current.type != EventType.Repaint) return;

            try
            {
                if (Plugin.ShowAnimalHealthBars == null || !Plugin.ShowAnimalHealthBars.Value) return;
                if (!PlayerHelper.IsInGameWorld()) return;
                if (_cachedEntities.Count == 0) return;

                var player = PlayerHelper.GetLocalPlayer();
                if (player == null) return;

                if (_cachedCamera == null) _cachedCamera = Camera.main;
                if (_cachedCamera == null) return;

                if (_nameStyle == null)
                {
                    _nameStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.LowerCenter,
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                        richText = true
                    };
                }

                foreach (var ent in _cachedEntities)
                {
                    if (ent == null || ent.gameObject == null || ent.stat_health == null) continue;
                    if (ent.stat_health.Value <= 0f || ent.stat_health.IsZero) continue;

                    // Compute height offset according to creature height
                    float headHeight = 1.4f;
                    string rawName = ent.gameObject.name;
                    if (rawName.IndexOf("bear", StringComparison.OrdinalIgnoreCase) >= 0 || rawName.IndexOf("mama", StringComparison.OrdinalIgnoreCase) >= 0) headHeight = 2.4f;
                    else if (rawName.IndexOf("shark", StringComparison.OrdinalIgnoreCase) >= 0) headHeight = 0.8f;
                    else if (rawName.IndexOf("bird", StringComparison.OrdinalIgnoreCase) >= 0 || rawName.IndexOf("screecher", StringComparison.OrdinalIgnoreCase) >= 0 || rawName.IndexOf("seagull", StringComparison.OrdinalIgnoreCase) >= 0) headHeight = 1.1f;
                    else if (rawName.IndexOf("puffer", StringComparison.OrdinalIgnoreCase) >= 0) headHeight = 0.9f;

                    Vector3 worldPos = ent.transform.position + (Vector3.up * headHeight);
                    Vector3 screenPos = _cachedCamera.WorldToScreenPoint(worldPos);

                    // Only render if in front of camera
                    if (screenPos.z <= 0.5f || screenPos.z > 35f) continue;

                    float screenY = Screen.height - screenPos.y;
                    float screenX = screenPos.x;

                    float curHp = ent.stat_health.Value;
                    float maxHp = Mathf.Max(1f, ent.stat_health.Max);
                    float pct = Mathf.Clamp01(curHp / maxHp);

                    // Dynamic bar width based on distance
                    float barWidth = Mathf.Clamp(130f - (screenPos.z * 1.2f), 80f, 120f);
                    float barHeight = 10f;
                    float barX = screenX - (barWidth / 2f);
                    float barY = screenY;

                    string displayName = GetFriendlyCreatureName(rawName);

                    // Draw Name and HP label
                    string label = $"{displayName}  <color=#F8FAFC>[{curHp:F0}/{maxHp:F0}]</color>  <size=10><color=#94A3B8>{screenPos.z:F0}m</color></size>";
                    Rect labelRect = new Rect(barX - 60f, barY - 22f, barWidth + 120f, 20f);

                    // Label Shadow
                    _nameStyle.normal.textColor = Color.black;
                    GUI.Label(new Rect(labelRect.x + 1, labelRect.y + 1, labelRect.width, labelRect.height), label, _nameStyle);
                    // Label Foreground
                    _nameStyle.normal.textColor = Color.white;
                    GUI.Label(labelRect, label, _nameStyle);

                    // Draw Outer Border (1px)
                    GUI.color = new Color(0.18f, 0.18f, 0.22f, 0.95f);
                    GUI.DrawTexture(new Rect(barX - 1, barY - 1, barWidth + 2, barHeight + 2), _borderTexture);

                    // Draw Background
                    GUI.color = new Color(0.06f, 0.06f, 0.08f, 0.90f);
                    GUI.DrawTexture(new Rect(barX, barY, barWidth, barHeight), _bgTexture);

                    // Draw Health Fill
                    Color healthColor;
                    if (pct > 0.50f)
                    {
                        healthColor = Color.Lerp(new Color(0.95f, 0.75f, 0.15f), new Color(0.20f, 0.85f, 0.35f), (pct - 0.5f) * 2f);
                    }
                    else
                    {
                        healthColor = Color.Lerp(new Color(0.92f, 0.18f, 0.20f), new Color(0.95f, 0.75f, 0.15f), pct * 2f);
                    }

                    GUI.color = healthColor;
                    GUI.DrawTexture(new Rect(barX, barY, barWidth * pct, barHeight), _fillTexture);
                }

                GUI.color = Color.white; // Reset GUI tint
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Sailor's Companion] Error rendering Creature Health Bar: {ex.Message}");
            }
        }

        private static string GetFriendlyCreatureName(string goName)
        {
            if (string.IsNullOrEmpty(goName)) return "Creature";
            if (_nameCache.TryGetValue(goName, out var cached)) return cached;

            string lower = goName.ToLower();
            string result;
            if (lower.Contains("shark")) result = "🦈 Bruce the Shark";
            else if (lower.Contains("mamabear") || lower.Contains("mama_bear")) result = "🐻 Mama Bear";
            else if (lower.Contains("bear")) result = "🐻 Bear";
            else if (lower.Contains("boar") || lower.Contains("warthog")) result = "🐗 Boar";
            else if (lower.Contains("hyenaboss") || lower.Contains("hyena_boss")) result = "🐺 Alpha Hyena";
            else if (lower.Contains("hyena")) result = "🐺 Hyena";
            else if (lower.Contains("puffer")) result = "🐡 Poison Pufferfish";
            else if (lower.Contains("rat") || lower.Contains("lurker")) result = "🐀 Lurker";
            else if (lower.Contains("screecher") || lower.Contains("stonebird")) result = "🦅 Screecher";
            else if (lower.Contains("seagull") || lower.Contains("bird")) result = "🕊️ Seagull";
            else if (lower.Contains("llama")) result = "🦙 Llama";
            else if (lower.Contains("goat")) result = "🐐 Goat";
            else if (lower.Contains("clucker") || lower.Contains("chicken")) result = "🐔 Clucker";
            else if (lower.Contains("butler") || lower.Contains("bot")) result = "🤖 Butler Bot";
            else if (lower.Contains("dolphin")) result = "🐬 Dolphin";
            else if (lower.Contains("angler")) result = "🐟 Anglerfish";
            else result = goName.Replace("(Clone)", "").Trim();

            _nameCache[goName] = result;
            return result;
        }
        // ============================================================================
        // [END] IMGUI HEALTH BAR RENDERING
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: ANIMAL & ENEMY HEALTH BAR OVERLAY
    // ============================================================================
}
