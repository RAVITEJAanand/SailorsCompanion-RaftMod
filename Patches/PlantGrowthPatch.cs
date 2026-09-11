using System;
using HarmonyLib;
using UnityEngine;

namespace SailorsCompanion.Patches
{
    // ============================================================================
    // [START] MODULE: FAST CROP & TREE GROWTH PATCH
    // Purpose: Speeds up growth timers for crops, flowers, bushes, and trees
    //          when enabled, eliminating tedious 30+ minute waiting times.
    // ============================================================================
    [HarmonyPatch(typeof(Plant), "IncrementGrowTimer")]
    public static class PlantGrowthPatch
    {
        // ============================================================================
        // [START] HARMONY PREFIX: MULTIPLY GROW TIMER DELTA
        // ============================================================================
        static void Prefix(ref float amount)
        {
            try
            {
                if (Plugin.EnableCropGrowthBoost != null && Plugin.EnableCropGrowthBoost.Value)
                {
                    float mult = Plugin.CropGrowthMultiplier != null ? Plugin.CropGrowthMultiplier.Value : 1.5f;
                    if (Plugin.IsSurvivalMode)
                    {
                        mult = Mathf.Clamp(mult, 1.0f, 2.0f);
                    }
                    if (mult > 0.1f)
                    {
                        amount *= mult;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Sailor's Companion] Error in PlantGrowthPatch: {ex.Message}");
            }
        }
        // ============================================================================
        // [END] HARMONY PREFIX: MULTIPLY GROW TIMER DELTA
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: FAST CROP & TREE GROWTH PATCH
    // ============================================================================
}
