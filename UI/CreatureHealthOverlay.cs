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
        private const float SCAN_INTERVAL = 0.5f; // re-scan entities twice a second
        private readonly List<Network_Entity> _cachedEntities = new List<Network_Entity>();

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
            if (Plugin.ShowAnimalHealthBars == null || !Plugin.ShowAnimalHealthBars.Value) return;

            if (Time.time - _lastScanTime >= SCAN_INTERVAL)
            {
                _lastScanTime = Time.time;
                ScanNearbyEntities();
            }
        }

        private void ScanNearbyEntities()
        {
            _cachedEntities.Clear();

            var player = PlayerHelper.GetLocalPlayer();
            if (player == null) return;

            var playerPos = player.transform.position;
            var entities = FindObjectsOfType<Network_Entity>();
            if (entities == null || entities.Length == 0) return;

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
            try
            {
                if (Plugin.ShowAnimalHealthBars == null || !Plugin.ShowAnimalHealthBars.Value) return;

                var player = PlayerHelper.GetLocalPlayer();
                if (player == null) return;

                var cam = Camera.main;
                if (cam == null) return;

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
                    string rawName = ent.gameObject.name.ToLower();
                    if (rawName.Contains("bear") || rawName.Contains("mama")) headHeight = 2.4f;
                    else if (rawName.Contains("shark")) headHeight = 0.8f;
                    else if (rawName.Contains("bird") || rawName.Contains("screecher") || rawName.Contains("seagull")) headHeight = 1.1f;
                    else if (rawName.Contains("puffer")) headHeight = 0.9f;

                    Vector3 worldPos = ent.transform.position + (Vector3.up * headHeight);
                    Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

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

                    string displayName = GetFriendlyCreatureName(ent.gameObject.name);

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
            string lower = goName.ToLower();
            if (lower.Contains("shark")) return "🦈 Bruce the Shark";
            if (lower.Contains("mamabear") || lower.Contains("mama_bear")) return "🐻 Mama Bear";
            if (lower.Contains("bear")) return "🐻 Bear";
            if (lower.Contains("boar") || lower.Contains("warthog")) return "🐗 Boar";
            if (lower.Contains("hyenaboss") || lower.Contains("hyena_boss")) return "🐺 Alpha Hyena";
            if (lower.Contains("hyena")) return "🐺 Hyena";
            if (lower.Contains("puffer")) return "🐡 Poison Pufferfish";
            if (lower.Contains("rat") || lower.Contains("lurker")) return "🐀 Lurker";
            if (lower.Contains("screecher") || lower.Contains("stonebird")) return "🦅 Screecher";
            if (lower.Contains("seagull") || lower.Contains("bird")) return "🕊️ Seagull";
            if (lower.Contains("llama")) return "🦙 Llama";
            if (lower.Contains("goat")) return "🐐 Goat";
            if (lower.Contains("clucker") || lower.Contains("chicken")) return "🐔 Clucker";
            if (lower.Contains("butler") || lower.Contains("bot")) return "🤖 Butler Bot";
            if (lower.Contains("dolphin")) return "🐬 Dolphin";
            if (lower.Contains("angler")) return "🐟 Anglerfish";

            // Fallback: clean up Unity clone naming
            return goName.Replace("(Clone)", "").Trim();
        }
        // ============================================================================
        // [END] IMGUI HEALTH BAR RENDERING
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: ANIMAL & ENEMY HEALTH BAR OVERLAY
    // ============================================================================
}
