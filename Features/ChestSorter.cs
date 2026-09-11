using System;
using System.Collections.Generic;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Features
{
    // ============================================================================
    // [START] MODULE: STORAGE CHEST AUTO-SORTER & QUICK STACK
    // Purpose: Automatically scans nearby storage chests on the raft and deposits
    //          matching inventory items from the player's backpack.
    // ============================================================================
    public static class ChestSorter
    {
        public const float SCAN_RADIUS = 22f; // 22 meter radius around player

        // ============================================================================
        // [START] ACTION: QUICK STACK TO NEARBY CHESTS
        // ============================================================================
        public static void QuickStackToNearbyChests()
        {
            var player = PlayerHelper.GetLocalPlayer();
            if (player == null || player.Inventory == null)
            {
                TeleportManager.SetNotification("⚠️ Enter a game world to use Quick Stack!");
                return;
            }

            var playerInv = player.Inventory;
            var playerPos = player.transform.position;

            // Find all Storage_Small containers on the raft within scan radius
            var allChests = UnityEngine.Object.FindObjectsOfType<Storage_Small>();
            if (allChests == null || allChests.Length == 0)
            {
                TeleportManager.SetNotification("📦 No storage chests found nearby!");
                return;
            }

            int totalItemsMoved = 0;
            int chestsAffected = 0;

            // Collect valid nearby chests within SCAN_RADIUS
            var nearbyChests = new List<Storage_Small>();
            foreach (var chest in allChests)
            {
                if (chest != null && Vector3.Distance(playerPos, chest.transform.position) <= SCAN_RADIUS)
                {
                    nearbyChests.Add(chest);
                }
            }

            if (nearbyChests.Count == 0)
            {
                TeleportManager.SetNotification($"📦 No chests within {SCAN_RADIUS:F0}m of your position.");
                return;
            }

            // Iterate over each chest
            foreach (var chest in nearbyChests)
            {
                Inventory chestInv = chest.GetInventoryReference();
                if (chestInv == null || chestInv.allSlots == null) continue;

                bool chestModified = false;

                // Examine player backpack slots (avoid hotbar slots)
                var slotsToScan = playerInv.backpackSlots != null && playerInv.backpackSlots.Count > 0 
                    ? playerInv.backpackSlots 
                    : playerInv.allSlots;

                foreach (var pSlot in slotsToScan)
                {
                    if (pSlot == null || pSlot.IsEmpty || !pSlot.HasValidItemInstance()) continue;

                    var itemInstance = pSlot.itemInstance;
                    var baseItem = itemInstance?.baseItem;
                    if (baseItem == null) continue;

                    // Only quick-stack if the chest already has this item type
                    if (chestInv.GetItemCount(baseItem.UniqueName) <= 0) continue;

                    int maxStack = baseItem.settings_Inventory != null ? baseItem.settings_Inventory.StackSize : 20;

                    // Step 1: Try to merge into existing partial stacks in chest
                    foreach (var cSlot in chestInv.allSlots)
                    {
                        if (cSlot == null || cSlot.IsEmpty || !cSlot.HasValidItemInstance()) continue;
                        if (cSlot.itemInstance.baseItem.UniqueIndex != baseItem.UniqueIndex) continue;

                        int spaceInChestSlot = maxStack - cSlot.itemInstance.Amount;
                        if (spaceInChestSlot > 0)
                        {
                            int toTransfer = Mathf.Min(pSlot.itemInstance.Amount, spaceInChestSlot);
                            if (toTransfer > 0)
                            {
                                cSlot.itemInstance.Amount += toTransfer;
                                pSlot.itemInstance.Amount -= toTransfer;
                                totalItemsMoved += toTransfer;
                                chestModified = true;

                                cSlot.RefreshComponents();
                                pSlot.RefreshComponents();

                                if (pSlot.itemInstance.Amount <= 0)
                                {
                                    pSlot.Reset();
                                    pSlot.RefreshComponents();
                                    break;
                                }
                            }
                        }
                    }

                    // Step 2: If player still has items of this type, place in an empty slot in the chest
                    if (!pSlot.IsEmpty && pSlot.HasValidItemInstance() && pSlot.itemInstance.Amount > 0)
                    {
                        foreach (var cSlot in chestInv.allSlots)
                        {
                            if (cSlot != null && cSlot.IsEmpty)
                            {
                                int toTransfer = pSlot.itemInstance.Amount;
                                // SetItem(Item_Base, int) always builds a fresh full-durability ItemInstance
                                // (new ItemInstance(item, amount, item.MaxUses)), which would silently reset
                                // a partially-worn tool/weapon's durability to 100%. Clone the real instance
                                // (via the ItemInstance overload) so its actual Uses/durability is preserved.
                                cSlot.SetItem(pSlot.itemInstance);
                                pSlot.Reset();
                                totalItemsMoved += toTransfer;
                                chestModified = true;

                                cSlot.RefreshComponents();
                                pSlot.RefreshComponents();
                                break;
                            }
                        }
                    }
                }

                if (chestModified)
                {
                    chestsAffected++;
                }
            }

            if (totalItemsMoved > 0)
            {
                TeleportManager.SetNotification($"📦 Quick Stacked {totalItemsMoved} items into {chestsAffected} nearby chests!");
                Debug.Log($"[Sailor's Companion] Quick Stack transferred {totalItemsMoved} items across {chestsAffected} chests.");
            }
            else
            {
                TeleportManager.SetNotification("📦 No matching items found to quick stack.");
            }
        }
        // ============================================================================
        // [END] ACTION: QUICK STACK TO NEARBY CHESTS
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: STORAGE CHEST AUTO-SORTER & QUICK STACK
    // ============================================================================
}
