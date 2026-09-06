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
            if (__instance != null && Plugin.HookPullSpeedMultiplier != null)
            {
                float mult = Plugin.IsSurvivalMode
                    ? Mathf.Clamp(Plugin.HookPullSpeedMultiplier.Value, 1.0f, 2.0f)
                    : Plugin.HookPullSpeedMultiplier.Value;

                if (mult > 1f)
                {
                    __instance.pullSpeed *= mult;
                    if (mult >= 1.5f)
                    {
                        __instance.gatherTime = Mathf.Max(0.05f, __instance.gatherTime / mult);
                    }
                }
            }
        }
    }
    // ============================================================================
    // [END] PATCH: FAST HOOK PULL & GATHER SPEED
    // ============================================================================
    #endregion
}
