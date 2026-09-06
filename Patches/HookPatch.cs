using HarmonyLib;
using UnityEngine;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: FAST HOOK PULL & GATHER SPEED
    // ============================================================================
    // [START] PATCH: FAST HOOK PULL & GATHER SPEED
    // Description: Accelerates hook cast reel speed and item gathering speed.
    // ============================================================================
    [HarmonyPatch(typeof(Hook), "Start")]
    public static class HookStartPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Hook __instance)
        {
            if (__instance != null && Plugin.HookPullSpeedMultiplier.Value > 1f)
            {
                __instance.pullSpeed *= Plugin.HookPullSpeedMultiplier.Value;
                if (Plugin.HookPullSpeedMultiplier.Value >= 2f)
                {
                    __instance.gatherTime = Mathf.Max(0.05f, __instance.gatherTime / Plugin.HookPullSpeedMultiplier.Value);
                }
            }
        }
    }
    // ============================================================================
    // [END] PATCH: FAST HOOK PULL & GATHER SPEED
    // ============================================================================
    #endregion
}
