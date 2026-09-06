using HarmonyLib;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: INFINITE TOOL & ARMOR DURABILITY
    // ============================================================================
    // [START] PATCH: INFINITE TOOL & ARMOR DURABILITY
    // Description: Prevents usage degradation on hotbar tools, weapons, and equipped armor.
    // ============================================================================
    [HarmonyPatch(typeof(Slot), nameof(Slot.IncrementUses))]
    public static class SlotDurabilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (Plugin.InfiniteDurability.Value)
            {
                // Prevent durability loss on hotbar tools / items
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Equipment_ArmorPiece), "RemoveDurability")]
    public static class ArmorDurabilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (Plugin.InfiniteDurability.Value)
            {
                // Prevent durability loss on equipped armor
                return false;
            }
            return true;
        }
        // ============================================================================
        // [END] PATCH: INFINITE TOOL & ARMOR DURABILITY
        // ============================================================================
        #endregion
    }
}
