using System;
using HarmonyLib;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: GAMEMANAGER TICK HOOK
    // ============================================================================
    // [START] PATCH: GAMEMANAGER TICK HOOK
    // Description: Hooks GameManager.Update to guarantee plugin tick execution in all scenes.
    // ============================================================================
    [HarmonyPatch(typeof(GameManager), "Update")]
    public static class GameManagerUpdatePatch
    {
        public static void Postfix()
        {
            try
            {
                Plugin.EnsureManager();
                Plugin.Instance?.OnGameManagerUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Sailor's Companion] Error in GameManagerUpdatePatch: " + ex.Message);
            }
        }
    }
    // ============================================================================
    // [END] PATCH: GAMEMANAGER TICK HOOK
    // ============================================================================
    #endregion
}
