using HarmonyLib;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: ANTI-SHARK RAFT PROTECTION
    // ============================================================================
    // [START] PATCH: ANTI-SHARK RAFT PROTECTION
    // Description: Blocks Bruce the Shark from targeting, biting, or damaging raft foundations.
    // ============================================================================
    [HarmonyPatch(typeof(AI_State_Attack_Block_Shark), "FindBlockToAttack")]
    public static class SharkFindBlockPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Block __result)
        {
            if (Plugin.AntiSharkRaftDamage.Value)
            {
                // Prevent shark from finding blocks to attack on the raft
                __result = null;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AI_State_Attack_Block_Shark), "DealDamageToBlock")]
    public static class SharkDealDamagePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Block p_targetBlock)
        {
            if (Plugin.AntiSharkRaftDamage.Value)
            {
                // Prevent any damage to raft foundations
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AI_State_Attack_Block_Shark), "InitiateAttackOnBlock")]
    public static class SharkInitiateAttackPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Block p_targetBlock)
        {
            if (Plugin.AntiSharkRaftDamage.Value)
            {
                // Prevent initiating attack on raft blocks
                return false;
            }
            return true;
        }
    }
    #endregion // [END] PATCH: ANTI-SHARK RAFT PROTECTION
}
