using System;
using System.Collections.Generic;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Features
{
    // ============================================================================
    // [START] MODULE: COLLECTION NETS HELPER & AUTO-EMPTY
    // Purpose: Scans all collection nets (ItemCollector) across the raft, empties
    //          all trapped floating debris into player inventory with 1 click or
    //          automatically in the background.
    // ============================================================================
    public class NetsHelper : MonoBehaviour
    {
        public static NetsHelper Instance { get; private set; }

        private float _lastAutoEmptyTime = 0f;
        private const float AUTO_EMPTY_INTERVAL = 10f; // check every 10 seconds

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
        // [START] PER-FRAME BACKGROUND AUTO-EMPTY LOOP
        // ============================================================================
        private void Update()
        {
            if (Plugin.AutoEmptyCollectionNets != null && Plugin.AutoEmptyCollectionNets.Value)
            {
                if (Time.time - _lastAutoEmptyTime >= AUTO_EMPTY_INTERVAL)
                {
                    _lastAutoEmptyTime = Time.time;
                    EmptyAllNets(silent: true);
                }
            }
        }
        // ============================================================================
        // [END] PER-FRAME BACKGROUND AUTO-EMPTY LOOP
        // ============================================================================

        // ============================================================================
        // [START] ACTION: EMPTY ALL COLLECTION NETS
        // ============================================================================
        public static int EmptyAllNets(bool silent = false)
        {
            var player = PlayerHelper.GetLocalPlayer();
            if (player == null || player.Inventory == null)
            {
                if (!silent) TeleportManager.SetNotification("⚠️ Enter a game world to empty collection nets!");
                return 0;
            }

            var allCollectors = UnityEngine.Object.FindObjectsOfType<ItemCollector>();
            if (allCollectors == null || allCollectors.Length == 0)
            {
                if (!silent) TeleportManager.SetNotification("🕸️ No collection nets found on the raft!");
                return 0;
            }

            int totalItemsCollected = 0;
            int netsEmptied = 0;

            foreach (var collector in allCollectors)
            {
                if (collector == null || collector.collectedItems == null) continue;

                int count = collector.collectedItems.Count;
                if (count > 0)
                {
                    try
                    {
                        collector.AddCollectedItemsToPlayer(player);
                        totalItemsCollected += count;
                        netsEmptied++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[Sailor's Companion] Error emptying net: {ex.Message}");
                    }
                }
            }

            if (!silent)
            {
                if (totalItemsCollected > 0)
                {
                    TeleportManager.SetNotification($"🕸️ Collected {totalItemsCollected} items from {netsEmptied} collection nets!");
                }
                else
                {
                    TeleportManager.SetNotification("🕸️ All collection nets are currently empty!");
                }
            }

            return totalItemsCollected;
        }
        // ============================================================================
        // [END] ACTION: EMPTY ALL COLLECTION NETS
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: COLLECTION NETS HELPER & AUTO-EMPTY
    // ============================================================================
}
