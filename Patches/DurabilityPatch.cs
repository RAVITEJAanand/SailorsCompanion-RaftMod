using HarmonyLib;

namespace SailorsCompanion.Patches
{
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
    }
}
