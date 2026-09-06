using HarmonyLib;

namespace SailorsCompanion.Patches
{
    [HarmonyPatch(typeof(ItemInstance_Inventory), "get_StackSize")]
    public static class StackSizePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ItemInstance_Inventory __instance, ref int __result)
        {
            // Only adjust stackable items (items that normally stack > 1, like resources, food, materials)
            // Keep single-use tools/equipment at stack size 1
            if (Plugin.CustomStackSize.Value > 1 && __result > 1)
            {
                __result = Plugin.CustomStackSize.Value;
            }
        }
    }
}
