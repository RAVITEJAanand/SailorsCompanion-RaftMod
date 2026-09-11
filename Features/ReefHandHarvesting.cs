using System;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Features
{
    // ============================================================================
    // [START] MODULE: REEF & ISLAND HAND HARVESTING SYSTEM
    // Purpose: Enables hand-based harvesting on islands and surrounding shallow reefs:
    //          - Surface Land Pickup: Collect ground items by hand without hook tool.
    //          - Reef Underwater Harvesting: Mine Sand, Clay, Scrap, and Ores with
    //            bare hands without requiring a hook in hand.
    //          - Fast Reef Harvest: Reduces channeling time from 3s to 0.4s to evade sharks.
    // ============================================================================
    public class ReefHandHarvesting : MonoBehaviour
    {
        public static ReefHandHarvesting Instance { get; private set; }

        private float _lastHandScanTime = 0f;
        private const float HAND_SCAN_INTERVAL = 0.2f; // check 5 times a second
        private const float HAND_PICKUP_RANGE = 2.2f;

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
        // [START] PER-FRAME HAND HARVESTING & PICKUP MONITOR
        // ============================================================================
        private void Update()
        {
            if (Time.time - _lastHandScanTime < HAND_SCAN_INTERVAL) return;
            _lastHandScanTime = Time.time;

            // Island Hand Pickup: collect surface flowers, fruits, etc. without hook
            if (Plugin.IslandHandPickup != null && Plugin.IslandHandPickup.Value)
            {
                CheckIslandHandPickup();
            }

            // Reef channeling acceleration is handled by HarvestingPatch.cs's Harmony
            // patches on PickupChanneling.Awake/InitiateChannel, gated on
            // Plugin.ReefFastHarvest - polling it here too (previously gated on the
            // unrelated Plugin.ReefHandHarvesting flag) fought over pickupTime every
            // frame and produced inconsistent mining speed.
        }
        // ============================================================================
        // [END] PER-FRAME HAND HARVESTING & PICKUP MONITOR
        // ============================================================================

        // ============================================================================
        // [START] ACTION: ISLAND SURFACE ITEM AUTO-COLLECT
        // Purpose: Scans for nearby PickupItems on island terrain (not on raft blocks,
        //          not ocean floating debris) and silently collects them into inventory
        //          without requiring a hook to be equipped.
        // ============================================================================
        private void CheckIslandHandPickup()
        {
            var player = PlayerHelper.GetLocalPlayer();
            if (player == null) return;

            // Only apply on islands — skip if the player is underwater (oxygen stat is depleting)
            if (player.Stats?.stat_oxygen != null && player.Stats.stat_oxygen.NormalValue < 1f) return;

            var playerPos = player.transform.position;
            var playerInv = player.Inventory;
            if (playerInv == null) return;

            var allPickups = UnityEngine.Object.FindObjectsOfType<PickupItem>();
            if (allPickups == null) return;

            foreach (var pickup in allPickups)
            {
                if (pickup == null || !pickup.canBePickedUp) continue;
                if (!pickup.gameObject.activeInHierarchy) continue;

                // Skip ocean floating debris (ItemNet, flotsam on ocean)
                if (pickup is ItemNet) continue;
                if (pickup.pickupItemType != PickupItemType.Default) continue;

                // Skip items attached to raft blocks (storage, machines, etc.)
                if (pickup.GetComponentInParent<Block>() != null) continue;

                // Skip items already being collected by a collection net
                if (pickup.GetComponent<ItemCollector>() != null) continue;

                // Never vacuum up items the player just intentionally dropped nearby -
                // otherwise there is no way to actually put something down on an island.
                // (Matches the same isDropped guard used by AutoPickupManager/MagneticCollector.)
                if (pickup.isDropped) continue;

                float dist = Vector3.Distance(playerPos, pickup.transform.position);
                if (dist > HAND_PICKUP_RANGE) continue;

                // Collect: go through the game's own Pickup.PickupItem() so the FULL stack
                // amount is added correctly (and networked pickups are removed/synced
                // properly). The previous code called AddItem(itemName, 1) - always
                // exactly 1 item regardless of the pickup's real stack Amount - and then
                // unconditionally destroyed the whole PickupItem GameObject, silently
                // deleting the rest of any stack larger than 1 (e.g. a pile of coconuts).
                try
                {
                    if (pickup.itemInstance == null || !pickup.itemInstance.Valid) continue;
                    if (player.PickupScript == null) continue;

                    player.PickupScript.PickupItem(pickup, forcePickup: true, triggerHandAnimation: false);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Sailor's Companion] IslandHandPickup: failed to collect {pickup.name}: {ex.Message}");
                }
            }
        }
        // ============================================================================
        // [END] ACTION: ISLAND SURFACE ITEM AUTO-COLLECT
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: REEF & ISLAND HAND HARVESTING SYSTEM
    // ============================================================================
}
