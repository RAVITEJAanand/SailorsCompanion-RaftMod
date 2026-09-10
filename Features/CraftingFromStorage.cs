using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Features
{
    // ============================================================================
    // [START] MODULE: CRAFTING FROM STORAGE & NEARBY CHESTS
    // Purpose: Enables seamless crafting using ingredients stored across nearby
    //          storage chests (22m radius) without needing to fetch them manually.
    // ============================================================================
    public static class CraftingFromStorage
    {
        public const float SCAN_RADIUS = 22f;

        private static readonly List<Storage_Small> _cachedNearbyChests = new List<Storage_Small>();
        private static float _lastChestScanTime = -10f;
        private static Vector3 _lastChestScanPos = Vector3.zero;
        private const float CHEST_SCAN_INTERVAL = 1.0f;

        // ============================================================================
        // [START] HELPER: CACHED CHEST SCANNER
        // ============================================================================
        private static List<Storage_Small> GetNearbyChestsCached(Vector3 playerPos)
        {
            if (Time.unscaledTime - _lastChestScanTime > CHEST_SCAN_INTERVAL || Vector3.Distance(playerPos, _lastChestScanPos) > 4f)
            {
                _lastChestScanTime = Time.unscaledTime;
                _lastChestScanPos = playerPos;
                _cachedNearbyChests.Clear();

                var allChests = UnityEngine.Object.FindObjectsOfType<Storage_Small>();
                if (allChests != null && allChests.Length > 0)
                {
                    foreach (var chest in allChests)
                    {
                        if (chest != null && Vector3.Distance(playerPos, chest.transform.position) <= SCAN_RADIUS)
                        {
                            _cachedNearbyChests.Add(chest);
                        }
                    }
                }
            }
            return _cachedNearbyChests;
        }
        // ============================================================================
        // [END] HELPER: CACHED CHEST SCANNER
        // ============================================================================

        // ============================================================================
        // [START] HELPER: GET ITEM COUNT FROM NEARBY CHESTS
        // ============================================================================
        public static int GetNearbyChestsItemCount(List<Item_Base> items)
        {
            if (items == null || items.Count == 0) return 0;

            var player = PlayerHelper.GetLocalPlayer();
            if (player == null) return 0;

            var nearbyChests = GetNearbyChestsCached(player.transform.position);
            if (nearbyChests.Count == 0) return 0;

            int total = 0;
            foreach (var chest in nearbyChests)
            {
                if (chest == null) continue;

                var chestInv = chest.GetInventoryReference();
                if (chestInv == null) continue;

                foreach (var item in items)
                {
                    if (item != null)
                    {
                        total += chestInv.GetItemCount(item.UniqueName);
                    }
                }
            }

            return total;
        }
        // ============================================================================
        // [END] HELPER: GET ITEM COUNT FROM NEARBY CHESTS
        // ============================================================================

        // ============================================================================
        // [START] RECURSION GUARD FOR COST CONSUMPTION
        // ============================================================================
        public static bool IsConsumingCosts { get; set; } = false;

        // ============================================================================
        // [START] HELPER: DEDUCT COSTS DIRECTLY FROM PLAYER AND NEARBY CHESTS
        // Deducts items directly from player inventory and nearby chests without
        // ever injecting items into the player's personal inventory slots.
        // ============================================================================
        public static void ConsumeCostsFromPlayerAndChests(Inventory playerInv, CostMultiple[] costMultiples)
        {
            if (playerInv == null || costMultiples == null || costMultiples.Length == 0) return;

            IsConsumingCosts = true;
            try
            {
                // Clone the cost multiples so amounts can be reduced in-place cleanly
                CostMultiple[] remaining = new CostMultiple[costMultiples.Length];
                for (int i = 0; i < costMultiples.Length; i++)
                {
                    if (costMultiples[i] != null)
                    {
                        remaining[i] = new CostMultiple(costMultiples[i].items, costMultiples[i].amount);
                    }
                }

                // 1. Deduct what the player has in personal inventory first
                playerInv.RemoveCostMultiple(remaining, true);

                // 2. If a chest/secondary inventory is currently open, deduct from it next
                if (playerInv.secondInventory != null)
                {
                    playerInv.secondInventory.RemoveCostMultiple(remaining, true);
                }

                // 3. Check if any cost is still needed
                bool anyNeeded = false;
                for (int i = 0; i < remaining.Length; i++)
                {
                    if (remaining[i] != null && remaining[i].amount > 0)
                    {
                        anyNeeded = true;
                        break;
                    }
                }
                if (!anyNeeded) return;

                // 4. Deduct remaining needed amounts directly from nearby chests
                var player = PlayerHelper.GetLocalPlayer();
                if (player == null) return;

                var nearbyChests = GetNearbyChestsCached(player.transform.position);
                foreach (var chest in nearbyChests)
                {
                    if (chest == null) continue;
                    var chestInv = chest.GetInventoryReference();
                    if (chestInv == null || chestInv == playerInv.secondInventory) continue;

                    chestInv.RemoveCostMultiple(remaining, true);

                    bool stillNeeded = false;
                    for (int i = 0; i < remaining.Length; i++)
                    {
                        if (remaining[i] != null && remaining[i].amount > 0)
                        {
                            stillNeeded = true;
                            break;
                        }
                    }
                    if (!stillNeeded) break;
                }
            }
            finally
            {
                IsConsumingCosts = false;
            }
        }
        // ============================================================================
        // [END] HELPER: DEDUCT COSTS DIRECTLY FROM PLAYER AND NEARBY CHESTS
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: CRAFTING FROM STORAGE & NEARBY CHESTS
    // ============================================================================

    // ============================================================================
    // [START] HARMONY PATCH: COST BOX INVENTORY COUNT WITH NEARBY CHESTS
    // ============================================================================
    [HarmonyPatch(typeof(BuildingUI_CostBox), "SetAmountInInventory")]
    public static class Patch_CostBox_SetAmountInInventory
    {
        static void Postfix(BuildingUI_CostBox __instance, List<Item_Base> ___items)
        {
            try
            {
                if (Plugin.CraftFromStorage != null && Plugin.CraftFromStorage.Value)
                {
                    int extraFromChests = CraftingFromStorage.GetNearbyChestsItemCount(___items);
                    if (extraFromChests > 0)
                    {
                        __instance.SetAmount(__instance.GetAmount() + extraFromChests);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Sailor's Companion] Error in CostBox patch: {ex.Message}");
            }
        }
    }
    // ============================================================================
    // [END] HARMONY PATCH: COST BOX INVENTORY COUNT WITH NEARBY CHESTS
    // ============================================================================
}
