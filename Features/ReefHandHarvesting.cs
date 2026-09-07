using System;
using System.Collections.Generic;
using HarmonyLib;
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
            if (Plugin.IslandHandPickup == null || !Plugin.IslandHandPickup.Value)
            {
                return;
            }

            if (Time.time - _lastHandScanTime >= HAND_SCAN_INTERVAL)
            {
                _lastHandScanTime = Time.time;
                CheckProximityHarvest();
            }
        }
        // ============================================================================
        // [END] PER-FRAME HAND HARVESTING & PICKUP MONITOR
        // ============================================================================

        // ============================================================================
        // [START] ACTION: PROXIMITY HARVEST & LOOSE ITEM CHECK
        // Purpose: Accelerates channeling on focused reef resources and allows
        //          instant hand pickup when within close range.
        // ============================================================================
        private void CheckProximityHarvest()
        {
            var player = PlayerHelper.GetLocalPlayer();
            if (player == null || player.PickupScript == null) return;

            // Check if player is focusing a PickupChanneling resource (Sand, Clay, Scrap, Ore)
            if (Plugin.ReefHandHarvesting != null && Plugin.ReefHandHarvesting.Value)
            {
                PickupChanneling channeling = Traverse.Create(player.PickupScript).Field("pickupChanneling").GetValue<PickupChanneling>();
                if (channeling != null)
                {
                    // If fast harvest is enabled, set pickupTime to 0.3s for rapid mining
                    if (Plugin.ReefFastHarvest != null && Plugin.ReefFastHarvest.Value)
                    {
                        channeling.pickupTime = 0.3f;
                    }
                }
            }
        }
        // ============================================================================
        // [END] ACTION: PROXIMITY HARVEST & LOOSE ITEM CHECK
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: REEF & ISLAND HAND HARVESTING SYSTEM
    // ============================================================================
}
