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
            // 1. Always free cursor when Mod Menu or IMGUI is open
            if (CanvasModUI.IsWindowOpen || ModGUI.IsOpen) return true;

            // 2. Always free cursor when in Main Menu, Title Screen, or Scene Loading (no local player)
            if (PlayerHelper.GetLocalPlayer() == null) return true;

            return false;
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
