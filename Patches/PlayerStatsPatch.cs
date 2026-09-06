using HarmonyLib;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: GOD MODE DAMAGE IMMUNITY
    // ============================================================================
    // [START] PATCH: GOD MODE DAMAGE IMMUNITY
    // Description: Intercepts PlayerStats.Damage to nullify all incoming damage to the local player.
    // ============================================================================
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Damage))]
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
    #endregion // [END] PATCH: GOD MODE DAMAGE IMMUNITY
}
