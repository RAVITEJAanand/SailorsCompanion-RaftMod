using System;
using HarmonyLib;
using UnityEngine;
using SailorsCompanion.Features;

namespace SailorsCompanion.Patches
{
    // ============================================================================
    // [START] HARMONY PATCH: REEF & UNDERWATER MINING ACCELERATION (3.5x SPEED + SHARK WARD)
    // Purpose: Speeds up Hook reef mining (Sand, Clay, Scrap, Metal/Copper Ore, Clams)
    //          from 2.5s down to 0.7s per node, preserving tool progression while
    //          eliminating frustration, and grants a 5s shark repellent ward while mining.
    // ============================================================================

    // [START] HOOK GATHERING SPEED PATCH
    [HarmonyPatch(typeof(Hook), "HandleGathering")]
    public static class Hook_HandleGathering_Patch
    {
        // Timestamp of the last frame where the player was actively reef-mining.
        // Updated every frame while gatherTime is overridden so the shark ward
        // stays active for the full 5s after the player stops mining.
        public static float LastReefMiningTime = -9999f;

        [HarmonyPrefix]
        public static void Prefix(Hook __instance, PickupItem item)
        {
            if (__instance == null) return;

            if (Plugin.ReefFastHarvest != null && Plugin.ReefFastHarvest.Value)
            {
                // Accelerate mining from 2.5s down to 0.7s (3.5x faster!)
                __instance.gatherTime = 0.7f;

                // Refresh the ward timer every frame while actively mining,
                // not just when item is non-null (item can be null mid-gather).
                LastReefMiningTime = Time.time;
            }
        }
    }

    [HarmonyPatch(typeof(Hook), "StartCollecting")]
    public static class Hook_StartCollecting_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Hook __instance, PickupItem item)
        {
            if (__instance == null) return;

            if (Plugin.ReefFastHarvest != null && Plugin.ReefFastHarvest.Value)
            {
                __instance.gatherTime = 0.7f;
                Hook_HandleGathering_Patch.LastReefMiningTime = Time.time;
            }
        }
    }
    // [END] HOOK GATHERING SPEED PATCH

    // [START] SHARK REPEL WARD DURING MINING
    [HarmonyPatch(typeof(AI_State_Attack_Entity_Shark), "AttemptAttack")]
    public static class Shark_AttemptAttack_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(AI_State_Attack_Entity_Shark __instance)
        {
            if (Plugin.ReefFastHarvest != null && Plugin.ReefFastHarvest.Value)
            {
                if (Time.time - Hook_HandleGathering_Patch.LastReefMiningTime < 5.0f)
                {
                    // Block the attack outright during the 5s mining ward window.
                    // Do NOT call ForceDriveBy — that still triggers a swim-through.
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AI_StateMachine_Shark), "FindAndSetTargetToAttack")]
    public static class Shark_FindTarget_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(AI_StateMachine_Shark __instance)
        {
            if (Plugin.ReefFastHarvest != null && Plugin.ReefFastHarvest.Value)
            {
                if (Time.time - Hook_HandleGathering_Patch.LastReefMiningTime < 5.0f)
                {
                    // Only suppress targeting if this shark is trying to target the local player.
                    // Prevents AI errors when multiple sharks are tracking different entities.
                    var localPlayer = PlayerHelper.GetLocalPlayer();
                    if (localPlayer != null && __instance.targetToAttack == localPlayer.gameObject)
                    {
                        __instance.targetToAttack = null;
                        return false;
                    }
                }
            }
            return true;
        }
    }
    // [END] SHARK REPEL WARD DURING MINING


    // [START] CHANNELING PICKUP ACCELERATION
    [HarmonyPatch(typeof(PickupChanneling), "Awake")]
    public static class PickupChanneling_Awake_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(PickupChanneling __instance)
        {
            if (__instance == null) return;

            if (Plugin.ReefFastHarvest != null && Plugin.ReefFastHarvest.Value)
            {
                __instance.pickupTime = 0.5f;
            }
        }
    }

    [HarmonyPatch(typeof(PickupChanneling), "InitiateChannel")]
    public static class PickupChanneling_Initiate_Patch
    {
        [HarmonyPrefix]
        private static void Prefix(PickupChanneling __instance)
        {
            if (__instance == null) return;

            if (Plugin.ReefFastHarvest != null && Plugin.ReefFastHarvest.Value)
            {
                __instance.pickupTime = 0.5f;
            }
        }
    }
    // [END] CHANNELING PICKUP ACCELERATION
    // ============================================================================
    // [END] HARMONY PATCH: REEF & UNDERWATER MINING ACCELERATION
    // ============================================================================
}
