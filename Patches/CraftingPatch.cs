using HarmonyLib;

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

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveCostMultiple), typeof(CostMultiple[]), typeof(bool))]
    public static class CraftingRemoveCostPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (Plugin.FreeCrafting.Value)
            {
                return false; // Do not consume items
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveCostMultipleIncludeSecondaryInventories), typeof(CostMultiple[]))]
    public static class CraftingRemoveCostSecPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (Plugin.FreeCrafting.Value)
            {
                return false; // Do not consume items
            }
            return true;
        }
        // ============================================================================
        // [END] PATCH: FREE INSTANT CRAFTING
        // ============================================================================
        #endregion
    }
}
