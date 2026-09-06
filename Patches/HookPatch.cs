using HarmonyLib;
using UnityEngine;

namespace SailorsCompanion.Patches
{
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
}
