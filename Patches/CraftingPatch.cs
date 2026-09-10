using System.Collections.Generic;
using HarmonyLib;
using SailorsCompanion.Features;
using SailorsCompanion.UI;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: FREE INSTANT CRAFTING
    // ============================================================================
    // [START] PATCH: FREE INSTANT CRAFTING
    // Description: Allows crafting all recipes without requiring any materials in inventory.
    // ============================================================================
    [HarmonyPatch(typeof(CostCollection), nameof(CostCollection.MeetsRequirements))]
    public static class CostCollectionMeetsRequirementsPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (Plugin.FreeCrafting.Value)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BuildingUI_CostBox), nameof(BuildingUI_CostBox.MeetsRequirements))]
    public static class BuildingUICostBoxMeetsRequirementsPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (Plugin.FreeCrafting.Value)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ItemInstance_Recipe), nameof(ItemInstance_Recipe.HasEnoughResourcesToCraft))]
    public static class RecipeHasEnoughResourcesPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (Plugin.FreeCrafting.Value)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ItemInstance_Recipe), "get_CanCraft")]
    public static class RecipeCanCraftPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (Plugin.FreeCrafting.Value || Cheat.UnlockAllCrafting)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SelectedRecipeBox), "Update")]
    public static class SelectedRecipeBoxUpdatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(SelectedRecipeBox __instance)
        {
            if (Plugin.FreeCrafting.Value && __instance != null && __instance.craftButton != null)
            {
                __instance.craftButton.interactable = true;
            }
        }
    }

    [HarmonyPatch(typeof(CostMultiple), nameof(CostMultiple.HasEnoughInInventory))]
    public static class CraftingHasEnoughPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(CostMultiple __instance, Inventory inventory, ref bool __result)
        {
            if (Plugin.FreeCrafting != null && Plugin.FreeCrafting.Value)
            {
                __result = true;
                return false;
            }

            if (Plugin.CraftFromStorage != null && Plugin.CraftFromStorage.Value && __instance != null && __instance.items != null)
            {
                int count = 0;
                if (inventory != null)
                {
                    foreach (var item in __instance.items)
                    {
                        if (item != null) count += inventory.GetItemCount(item.UniqueName);
                    }
                }
                if (count >= __instance.amount)
                {
                    __result = true;
                    return false;
                }
                count += CraftingFromStorage.GetNearbyChestsItemCount(new List<Item_Base>(__instance.items));
                __result = (count >= __instance.amount);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveCostMultiple), typeof(CostMultiple[]), typeof(bool))]
    public static class CraftingRemoveCostPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Inventory __instance, CostMultiple[] costMultiple, bool manipulateCostAmount)
        {
            if (Plugin.FreeCrafting != null && Plugin.FreeCrafting.Value)
            {
                return false; // Free crafting: never consume items
            }

            if (CraftingFromStorage.IsConsumingCosts)
            {
                return true; // Let internal chest deduction run vanilla RemoveCostMultiple
            }

            if (Plugin.CraftFromStorage != null && Plugin.CraftFromStorage.Value)
            {
                var localPlayer = PlayerHelper.GetLocalPlayer();
                if (localPlayer != null && __instance == localPlayer.Inventory)
                {
                    CraftingFromStorage.ConsumeCostsFromPlayerAndChests(__instance, costMultiple);
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveCostMultipleIncludeSecondaryInventories), typeof(CostMultiple[]))]
    public static class CraftingRemoveCostSecPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Inventory __instance, CostMultiple[] costMultiple)
        {
            if (Plugin.FreeCrafting != null && Plugin.FreeCrafting.Value)
            {
                return false; // Free crafting: never consume items
            }

            if (CraftingFromStorage.IsConsumingCosts)
            {
                return true;
            }

            if (Plugin.CraftFromStorage != null && Plugin.CraftFromStorage.Value)
            {
                CraftingFromStorage.ConsumeCostsFromPlayerAndChests(__instance, costMultiple);
                return false;
            }

            return true;
        }
        // ============================================================================
        // [END] PATCH: FREE INSTANT CRAFTING
        // ============================================================================
        #endregion
    }
}
