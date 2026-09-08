using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Features
{
    // ============================================================================
    // [START] MODULE: ITEM DETECTOR & ISLAND 3D SCANNER
    // Purpose: Provides a balanced, timed 3D pulse scanner for islands and landmarks.
    //          - Duration: 10 seconds of active subtle in-world markers
    //          - Cooldown: 30 seconds (in Survival Mode)
    //          - Range: 15 meters around the player
    //          - Zero 24/7 screen clutter; protects natural exploration
    // ============================================================================
    public class ItemDetector : MonoBehaviour
    {
        public static ItemDetector Instance { get; private set; }

        public const float SCAN_DURATION = 10f;
        public const float SCAN_COOLDOWN = 30f;
        public const float SCAN_RADIUS = 15f;

        private static float _lastScanTime = -9999f;
        private static bool _isScanActive = false;
        private static readonly List<GameObject> _activeMarkers = new List<GameObject>();

        // ============================================================================
        // [START] LIFECYCLE INITIALIZATION
        // ============================================================================
        private void Awake()
        {
            Instance = this;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
        }
        // ============================================================================
        // [END] LIFECYCLE INITIALIZATION
        // ============================================================================

        // ============================================================================
        // [START] COOLDOWN & STATUS HELPERS
        // ============================================================================
        public static bool IsScanActive => _isScanActive;

        public static float GetCooldownRemaining()
        {
            if (Plugin.IsCreativeMode) return 0f;
            float elapsed = Time.unscaledTime - _lastScanTime;
            float total = SCAN_DURATION + SCAN_COOLDOWN;
            float rem = total - elapsed;
            return rem > 0f ? rem : 0f;
        }

        public static float GetActiveTimeRemaining()
        {
            if (!_isScanActive) return 0f;
            float elapsed = Time.unscaledTime - _lastScanTime;
            float rem = SCAN_DURATION - elapsed;
            return rem > 0f ? rem : 0f;
        }
        // ============================================================================
        // [END] COOLDOWN & STATUS HELPERS
        // ============================================================================

        // ============================================================================
        // [START] ACTION: TRIGGER ISLAND PULSE SCAN
        // Purpose: Initiates the 10-second pulse scan around the local player.
        // ============================================================================
        public static bool TriggerPulseScan(bool silent = false)
        {
            var player = PlayerHelper.GetLocalPlayer();
            if (player == null)
            {
                if (!silent) TeleportManager.SetNotification("⚠️ Enter a game world to use the Island Scanner!");
                return false;
            }

            float cd = GetCooldownRemaining();
            if (cd > 0f && !_isScanActive)
            {
                int sec = Mathf.CeilToInt(cd);
                if (!silent) TeleportManager.SetNotification($"⏳ Scanner recharging: {sec}s cooldown remaining.");
                return false;
            }

            if (Instance == null) return false;

            Instance.StopAllCoroutines();
            ClearMarkers();

            _lastScanTime = Time.unscaledTime;
            _isScanActive = true;
            Instance.StartCoroutine(Instance.ScanRoutine(player));

            if (!silent) TeleportManager.SetNotification("🔍 Island Pulse Scan activated! (10s active)");
            return true;
        }
        // ============================================================================
        // [END] ACTION: TRIGGER ISLAND PULSE SCAN
        // ============================================================================

        // ============================================================================
        // [START] COROUTINE: SCANNER LIFECYCLE ROUTINE
        // Purpose: Discovers nearby points of interest, creates billboard markers,
        //          updates distance labels, and cleans up when the 10s expire.
        // ============================================================================
        private IEnumerator ScanRoutine(Network_Player player)
        {
            Vector3 center = player.transform.position;
            int itemsFound = 0;

            // 1. Scan for Storage Chests & Crates
            try
            {
                var storages = UnityEngine.Object.FindObjectsOfType<Storage_Small>();
                if (storages != null)
                {
                    foreach (var s in storages)
                    {
                        if (s == null) continue;
                        float d = Vector3.Distance(center, s.transform.position);
                        if (d <= SCAN_RADIUS && d > 0.5f)
                        {
                            CreateMarker(s.transform, "📦 Storage", Color.yellow, s.gameObject);
                            itemsFound++;
                        }
                    }
                }
            }
            catch { }

            // 2. Scan for Underwater/Island Channeling Resources (Sand, Clay, Ore, Scrap)
            try
            {
                var channelings = UnityEngine.Object.FindObjectsOfType<PickupChanneling>();
                if (channelings != null)
                {
                    foreach (var ch in channelings)
                    {
                        if (ch == null) continue;
                        float d = Vector3.Distance(center, ch.transform.position);
                        if (d <= SCAN_RADIUS && d > 0.5f)
                        {
                            string label = "💎 Resource";
                            Color col = new Color(0.2f, 0.9f, 0.3f);
                            CreateMarker(ch.transform, label, col, ch.gameObject);
                            itemsFound++;
                        }
                    }
                }
            }
            catch { }

            // 3. Scan for loose PickupItems
            try
            {
                var pickups = UnityEngine.Object.FindObjectsOfType<PickupItem>();
                if (pickups != null)
                {
                    foreach (var p in pickups)
                    {
                        if (p == null || !p.canBePickedUp || p.gameObject == null || !p.gameObject.activeInHierarchy) continue;
                        if (p is ItemNet || p.pickupItemType != PickupItemType.Default) continue;
                        if (p.GetComponentInParent<Block>() != null) continue;
                        if (p.GetComponent<ItemCollector>() != null) continue;

                        float d = Vector3.Distance(center, p.transform.position);
                        if (d <= SCAN_RADIUS && d > 0.5f)
                        {
                            string name = !string.IsNullOrEmpty(p.PickupName) ? p.PickupName : "Item";
                            CreateMarker(p.transform, $"✨ {name}", Color.cyan, p.gameObject);
                            itemsFound++;
                        }
                    }
                }
            }
            catch { }

            if (itemsFound > 0)
            {
                TeleportManager.SetNotification($"🔍 Scan detected {itemsFound} nearby points of interest!");
            }
            else
            {
                TeleportManager.SetNotification("🔍 Scan complete: No points of interest found in 15m radius.");
            }

            // Keep markers alive for SCAN_DURATION
            float timer = SCAN_DURATION;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            ClearMarkers();
            _isScanActive = false;
        }
        // ============================================================================
        // [END] COROUTINE: SCANNER LIFECYCLE ROUTINE
        // ============================================================================

        // ============================================================================
        // [START] HELPER: CREATE WORLD BILLBOARD 3D MARKER
        // ============================================================================
        private static void CreateMarker(Transform target, string title, Color color, GameObject owner)
        {
            if (target == null) return;

            var markerGO = new GameObject($"ItemMarker_{title}");
            markerGO.transform.position = target.position + Vector3.up * 0.8f;
            markerGO.transform.SetParent(target, true);

            var textMesh = markerGO.AddComponent<TextMesh>();
            textMesh.text = title;
            textMesh.fontSize = 28;
            textMesh.characterSize = 0.07f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;

            var bill = markerGO.AddComponent<ScannerBillboard>();
            bill.TargetCamera = Camera.main;

            _activeMarkers.Add(markerGO);
        }

        private static void ClearMarkers()
        {
            foreach (var m in _activeMarkers)
            {
                if (m != null)
                {
                    UnityEngine.Object.Destroy(m);
                }
            }
            _activeMarkers.Clear();
        }
        // ============================================================================
        // [END] HELPER: CREATE WORLD BILLBOARD 3D MARKER
        // ============================================================================
    }

    // ============================================================================
    // [START] HELPER COMPONENT: SCANNER BILLBOARD
    // Purpose: Rotates 3D marker text to smoothly face the player's active camera.
    // ============================================================================
    public class ScannerBillboard : MonoBehaviour
    {
        public Camera TargetCamera;

        private void LateUpdate()
        {
            if (TargetCamera == null) TargetCamera = Camera.main;
            if (TargetCamera != null)
            {
                transform.LookAt(transform.position + TargetCamera.transform.rotation * Vector3.forward,
                                 TargetCamera.transform.rotation * Vector3.up);
            }
        }
    }
    // ============================================================================
    // [END] HELPER COMPONENT: SCANNER BILLBOARD
    // ============================================================================
}
