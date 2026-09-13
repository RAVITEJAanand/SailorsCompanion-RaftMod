using System;
using HarmonyLib;
using UnityEngine;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: GOD MODE DAMAGE IMMUNITY
    // ============================================================================
    // [START] PATCH: GOD MODE DAMAGE IMMUNITY
    // Description: Intercepts PlayerStats.Damage to nullify all incoming damage to the local player.
    // Argument types are pinned explicitly (verified against the decompiled PlayerStats.Damage
    // signature) rather than resolved by name alone, so this patch can never silently fail to
    // bind against an ambiguous or future-changed overload.
    // ============================================================================
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Damage), new Type[] { typeof(float), typeof(Vector3), typeof(Vector3), typeof(EntityType), typeof(bool), typeof(SO_Buff) })]
    public static class PlayerStatsDamagePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerStats __instance, float damage)
        {
            if (Plugin.GodMode.Value)
            {
                var localPlayer = PlayerHelper.GetLocalPlayer();
                if (localPlayer != null && __instance == localPlayer.Stats)
                {
                    // Block all damage to local player
                    return false;
                }
            }
            return true;
        }
    }
    // ============================================================================
    // [END] PATCH: GOD MODE DAMAGE IMMUNITY
    // ============================================================================
    #endregion
}
