using HarmonyLib;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: CUSTOM RESOURCE STACK SIZE
    // ============================================================================
    // [START] PATCH: CUSTOM RESOURCE STACK SIZE
    // Description: Dynamically overrides inventory stack limit for stackable resources (up to 999).
    // ============================================================================
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
    // ============================================================================
    // [END] PATCH: CUSTOM RESOURCE STACK SIZE
    // ============================================================================
    #endregion
}
