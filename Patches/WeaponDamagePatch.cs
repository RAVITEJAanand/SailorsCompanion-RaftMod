using HarmonyLib;
using UnityEngine;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: WEAPON DAMAGE MULTIPLIER & 1-HIT KILL
    // ============================================================================
    // [START] PATCH: WEAPON DAMAGE MULTIPLIER & 1-HIT KILL
    // Description: Scales melee and projectile weapon damage (spears, arrows, machete)
    //              against hostile creatures and predators, with 1-Hit Kill in Creative.
    // ============================================================================
    [HarmonyPatch(typeof(Network_Host), nameof(Network_Host.DamageEntity))]
    public static class WeaponDamagePatch
    {
        [HarmonyPrefix]
        public static void Prefix(Network_Entity entity, ref float damage, EntityType damageInflictorEntityType)
        {
            // Only amplify damage originating from player attacks against non-player targets (enemies/animals)
            if (damageInflictorEntityType == EntityType.Player && entity != null && entity.entityType != EntityType.Player)
            {
                // Creative Mode 1-Hit Kill: Instantly defeat any predator or creature
                if (Plugin.OneHitKill != null && Plugin.OneHitKill.Value)
                {
                    damage = 9999f;
                    return;
                }

                // Weapon Damage Multiplier (Survival QoL & Presets)
                if (Plugin.WeaponDamageMultiplier != null)
                {
                    float mult = Plugin.WeaponDamageMultiplier.Value;
                    if (mult > 1.0f)
                    {
                        damage *= mult;
                    }
                }
            }
        }
    }
    // ============================================================================
    // [END] PATCH: WEAPON DAMAGE MULTIPLIER & 1-HIT KILL
    // ============================================================================
    #endregion
}
