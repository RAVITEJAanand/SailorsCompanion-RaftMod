using System;
using HarmonyLib;
using UnityEngine;
using SailorsCompanion.Features;

namespace SailorsCompanion.Patches
{
    // ============================================================================
    // [START] HARMONY PATCH: REEF & ISLAND HARVESTING ACCELERATION
    // Purpose: Patches PickupChanneling (Sand, Clay, Scrap, Metal Ore, Copper Ore)
    //          to enable bare hand harvesting without requiring a hook tool,
    //          and speeds up channeling time so players can collect safely.
    // ============================================================================
    [HarmonyPatch(typeof(PickupChanneling), "Awake")]
    public static class PickupChanneling_Awake_Patch
    {
        // [START] METHOD: POSTFIX AWAKE
        private static void Postfix(PickupChanneling __instance)
        {
            if (__instance == null) return;

            if (Plugin.ReefFastHarvest != null && Plugin.ReefFastHarvest.Value)
            {
                __instance.pickupTime = 0.4f;
            }
        }
        // [END] METHOD: POSTFIX AWAKE
    }

    [HarmonyPatch(typeof(PickupChanneling), "InitiateChannel")]
    public static class PickupChanneling_Initiate_Patch
    {
        // [START] METHOD: PREFIX INITIATE CHANNEL
        private static void Prefix(PickupChanneling __instance, Network_Player player)
        {
            if (__instance == null) return;

            if (Plugin.ReefFastHarvest != null && Plugin.ReefFastHarvest.Value)
            {
                __instance.pickupTime = 0.4f;
            }
        }
        // [END] METHOD: PREFIX INITIATE CHANNEL
    }
    // ============================================================================
    // [END] HARMONY PATCH: REEF & ISLAND HARVESTING ACCELERATION
    // ============================================================================
}
