using System;
using HarmonyLib;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: CURSOR UNLOCKING & VISIBILITY
    // ============================================================================
    // [START] PATCH: CURSOR UNLOCKING & VISIBILITY
    // Description: Ensures mouse cursor is freed and interactable when Mod UI is active or on main menus.
    // ============================================================================
    public static class CursorPatchHelper
    {
        public static bool ShouldForceCursorFree()
        {
            // Only force free when Mod Menu is open
            return CanvasModUI.IsWindowOpen || ModGUI.IsOpen;
        }
    }

    [HarmonyPatch(typeof(Helper), "SetCursorVisibleAndLockState")]
    public static class HelperSetCursorVisibleAndLockStatePatch
    {
        public static void Prefix(ref bool state, ref CursorLockMode mode)
        {
            if (CursorPatchHelper.ShouldForceCursorFree())
            {
                state = true;
                mode = CursorLockMode.None;
            }
        }
    }

    [HarmonyPatch(typeof(Helper), "SetCursorLockState")]
    public static class HelperSetCursorLockStatePatch
    {
        public static void Prefix(ref CursorLockMode mode)
        {
            if (CursorPatchHelper.ShouldForceCursorFree())
            {
                mode = CursorLockMode.None;
            }
        }
    }

    [HarmonyPatch(typeof(Helper), "SetCursorVisible")]
    public static class HelperSetCursorVisiblePatch
    {
        public static void Prefix(ref bool state)
        {
            if (CursorPatchHelper.ShouldForceCursorFree())
            {
                state = true;
            }
        }
    }
    // ============================================================================
    // [END] PATCH: CURSOR UNLOCKING & VISIBILITY
    // ============================================================================
    #endregion
}
