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
        // [START] HELPER: PULL MISSING INGREDIENTS FROM NEARBY CHESTS
        // ============================================================================
        public static void PullMissingIngredientsFromChests(Inventory playerInv, CostMultiple[] costMultiples)
        {
            if (playerInv == null || costMultiples == null || costMultiples.Length == 0) return;

            var player = PlayerHelper.GetLocalPlayer();
            if (player == null) return;

            var nearbyChests = GetNearbyChestsCached(player.transform.position);
            if (nearbyChests.Count == 0) return;

            foreach (var cost in costMultiples)
            {
                if (cost == null || cost.items == null || cost.items.Length == 0) continue;

                // Check how many of this ingredient the player currently has on hand
                int currentInInv = 0;
                foreach (var item in cost.items)
                {
                    if (item != null) currentInInv += playerInv.GetItemCount(item.UniqueName);
                }

                int needed = cost.amount - currentInInv;
                if (needed <= 0) continue; // Player already has enough

                // Pull needed amount from nearby chests
                foreach (var chest in nearbyChests)
                {
                    if (chest == null) continue;

                    var chestInv = chest.GetInventoryReference();
                    if (chestInv == null || chestInv.allSlots == null) continue;

                    foreach (var targetItem in cost.items)
                    {
                        if (targetItem == null) continue;

                        foreach (var cSlot in chestInv.allSlots)
                        {
                            if (cSlot == null || cSlot.IsEmpty || !cSlot.HasValidItemInstance()) continue;
                            if (cSlot.itemInstance.baseItem.UniqueIndex != targetItem.UniqueIndex) continue;

                            int toTransfer = Mathf.Min(cSlot.itemInstance.Amount, needed);
                            if (toTransfer > 0)
                            {
                                cSlot.itemInstance.Amount -= toTransfer;
                                playerInv.AddItem(targetItem.UniqueName, toTransfer);
                                needed -= toTransfer;

                                if (cSlot.itemInstance.Amount <= 0)
                                {
                                    cSlot.Reset();
                                }
                                cSlot.RefreshComponents();

                                if (needed <= 0) break;
                            }
                        }

                        if (needed <= 0) break;
                    }

                    if (needed <= 0) break;
                }
            }
        }
        // ============================================================================
        // [END] HELPER: PULL MISSING INGREDIENTS FROM NEARBY CHESTS
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

    // ============================================================================
    // [START] HARMONY PATCH: AUTO-PULL CHEST INGREDIENTS BEFORE CRAFT
    // ============================================================================
    [HarmonyPatch(typeof(Inventory), "RemoveCostMultipleIncludeSecondaryInventories")]
    public static class Patch_Inventory_RemoveCostMultiple
    {
        static void Prefix(Inventory __instance, CostMultiple[] costMultiple)
        {
            try
            {
                if (Plugin.CraftFromStorage != null && Plugin.CraftFromStorage.Value)
                {
                    CraftingFromStorage.PullMissingIngredientsFromChests(__instance, costMultiple);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Sailor's Companion] Error pulling chest ingredients: {ex.Message}");
            }
        }
    }
    // ============================================================================
    // [END] HARMONY PATCH: AUTO-PULL CHEST INGREDIENTS BEFORE CRAFT
    // ============================================================================
}
