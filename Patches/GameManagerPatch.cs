using System;
using HarmonyLib;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Patches
{
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
}
