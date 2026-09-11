using System;
using System.Collections.Generic;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Features
{
    // ============================================================================
    // [START] MODULE: MAGNETIC DEBRIS AUTO-COLLECT SYSTEM
    // Purpose: Creates a dynamic ocean magnetic vortex pulling floating debris
    //          (wood, plastic, thatch, barrels) smoothly towards the raft or player.
    //          - Duration: 45 seconds active
    //          - Cooldown: 60 seconds (in Survival Mode)
    //          - Radius: 20 meters
    //          - Smooth physics pull: visual attraction rather than instant teleport
    // ============================================================================
    public class MagneticCollector : MonoBehaviour
    {
        public static MagneticCollector Instance { get; private set; }

        public const float MAGNET_DURATION = 45f;
        public const float MAGNET_COOLDOWN = 60f;
        public const float PULL_SPEED = 14f; // meters per second

        private static float _lastMagnetStartTime = -9999f;
        private static bool _isMagnetActive = false;
        private static float _lastPullScanTime = 0f;
        private const float PULL_SCAN_INTERVAL = 0.1f; // update physics 10 times a second

        // FindObjectsOfType<PickupItem>() scans every PickupItem in the scene, which is
        // too expensive to run 10x/second (PULL_SCAN_INTERVAL). Cache the candidate list
        // and only rescan the scene once a second; the 10Hz loop just moves items already
        // in the cache, pruning ones that were destroyed/picked up in the meantime.
        private static readonly List<PickupItem> _cachedPickups = new List<PickupItem>();
        private static float _lastPickupRescanTime = -9999f;
        private const float PICKUP_RESCAN_INTERVAL = 1.0f;

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
        public static bool IsActive => _isMagnetActive;

        public static float GetCooldownRemaining()
        {
            if (Plugin.IsCreativeMode) return 0f;
            float elapsed = Time.unscaledTime - _lastMagnetStartTime;
            float total = MAGNET_DURATION + MAGNET_COOLDOWN;
            float rem = total - elapsed;
            return rem > 0f ? rem : 0f;
        }

        public static float GetActiveTimeRemaining()
        {
            if (!_isMagnetActive) return 0f;
            float elapsed = Time.unscaledTime - _lastMagnetStartTime;
            float rem = MAGNET_DURATION - elapsed;
            return rem > 0f ? rem : 0f;
        }
        // ============================================================================
        // [END] COOLDOWN & STATUS HELPERS
        // ============================================================================

        // ============================================================================
        // [START] ACTION: TOGGLE / ACTIVATE MAGNETIC COLLECTOR
        // ============================================================================
        public static bool ToggleMagnet(bool? forceState = null, bool silent = false)
        {
            if (forceState.HasValue && !forceState.Value)
            {
                _isMagnetActive = false;
                if (!silent) TeleportManager.SetNotification("🧲 Magnetic Collector deactivated.");
                return true;
            }

            if (_isMagnetActive)
            {
                _isMagnetActive = false;
                if (!silent) TeleportManager.SetNotification("🧲 Magnetic Collector stopped.");
                return true;
            }

            float cd = GetCooldownRemaining();
            if (cd > 0f)
            {
                int sec = Mathf.CeilToInt(cd);
                if (!silent) TeleportManager.SetNotification($"⏳ Magnet recharging: {sec}s cooldown remaining.");
                return false;
            }

            var player = PlayerHelper.GetLocalPlayer();
            if (player == null)
            {
                if (!silent) TeleportManager.SetNotification("⚠️ Enter a game world to use the Magnetic Collector!");
                return false;
            }

            _lastMagnetStartTime = Time.unscaledTime;
            _isMagnetActive = true;

            if (!silent) TeleportManager.SetNotification("🧲 Magnetic Collector ACTIVATED! (45s active)");
            return true;
        }
        // ============================================================================
        // [END] ACTION: TOGGLE / ACTIVATE MAGNETIC COLLECTOR
        // ============================================================================

        // ============================================================================
        // [START] PER-FRAME DEBRIS PULL & ATTRACTION LOOP
        // ============================================================================
        private void Update()
        {
            if (!_isMagnetActive) return;

            // Check if duration expired
            if (Time.unscaledTime - _lastMagnetStartTime >= MAGNET_DURATION)
            {
                _isMagnetActive = false;
                TeleportManager.SetNotification("🧲 Magnetic Collector duration finished. Entering cooldown.");
                return;
            }

            if (Time.time - _lastPullScanTime < PULL_SCAN_INTERVAL) return;
            _lastPullScanTime = Time.time;

            PullNearbyDebris();
        }
        // ============================================================================
        // [END] PER-FRAME DEBRIS PULL & ATTRACTION LOOP
        // ============================================================================

        // ============================================================================
        // [START] ACTION: PULL NEARBY DEBRIS TOWARDS RAFT OR PLAYER
        // Purpose: Identifies floating PickupItem objects within radius and moves them.
        // ============================================================================
        private void PullNearbyDebris()
        {
            var player = PlayerHelper.GetLocalPlayer();
            if (player == null) return;

            Vector3 targetCenter = player.transform.position;
            var raft = ComponentManager<Raft>.Value ?? UnityEngine.Object.FindObjectOfType<Raft>();
            if (raft != null)
            {
                targetCenter = raft.transform.position;
            }

            float radius = Plugin.MagnetRadius != null ? Plugin.MagnetRadius.Value : 20f;

            try
            {
                if (Time.unscaledTime - _lastPickupRescanTime > PICKUP_RESCAN_INTERVAL || _cachedPickups.Count == 0)
                {
                    _lastPickupRescanTime = Time.unscaledTime;
                    _cachedPickups.Clear();
                    var pickups = UnityEngine.Object.FindObjectsOfType<PickupItem>();
                    if (pickups != null) _cachedPickups.AddRange(pickups);
                }
                else
                {
                    _cachedPickups.RemoveAll(p => p == null);
                }

                if (_cachedPickups.Count == 0) return;

                foreach (var item in _cachedPickups)
                {
                    if (item == null || !item.canBePickedUp || item.gameObject == null || !item.gameObject.activeInHierarchy) continue;

                    // CRITICAL FIX: Never pull or pick up Collection Nets, Animals, or Raft Structures!
                    if (item is ItemNet || item.pickupItemType != PickupItemType.Default) continue;
                    if (item.GetComponentInParent<Block>() != null) continue;
                    if (item.GetComponent<ItemCollector>() != null) continue;

                    // CRITICAL FIX: Never pull items intentionally dropped by the player!
                    if (item.isDropped) continue;

                    Vector3 itemPos = item.transform.position;
                    float dist = Vector3.Distance(targetCenter, itemPos);

                    if (dist <= radius)
                    {
                        // Pull smoothly towards targetCenter
                        Vector3 newPos = Vector3.MoveTowards(itemPos, targetCenter, PULL_SPEED * PULL_SCAN_INTERVAL);
                        item.transform.position = newPos;

                        // If close enough to player or collection deck, pick up automatically
                        if (dist <= 2.2f && player.PickupScript != null)
                        {
                            try
                            {
                                player.PickupScript.PickupItem(item);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Sailor's Companion] Error in magnetic pull loop: {ex.Message}");
            }
        }
        // ============================================================================
        // [END] ACTION: PULL NEARBY DEBRIS TOWARDS RAFT OR PLAYER
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: MAGNETIC DEBRIS AUTO-COLLECT SYSTEM
    // ============================================================================
}
