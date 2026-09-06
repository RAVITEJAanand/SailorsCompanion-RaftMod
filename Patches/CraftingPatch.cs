using HarmonyLib;

namespace SailorsCompanion.Patches
{
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
    }
}
