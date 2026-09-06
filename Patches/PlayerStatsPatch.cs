using HarmonyLib;

namespace SailorsCompanion.Patches
{
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
}
