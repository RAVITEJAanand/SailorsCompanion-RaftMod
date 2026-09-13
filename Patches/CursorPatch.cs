using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: CURSOR UNLOCKING & VISIBILITY & CAMERA FREEZE
    // ============================================================================
    // [START] PATCH: CURSOR UNLOCKING & VISIBILITY & CAMERA FREEZE
    // Description: Ensures mouse cursor is freed and interactable when Mod UI is active,
    //              and freezes MouseLook camera rotation so moving cursor never spins the world.
    // ============================================================================
    public static class CursorPatchHelper
    {
        private static PropertyInfo _fcWindowProp;
        private static PropertyInfo _imWindowProp;
        // Collection QoL is a menu too - without it here the camera keeps turning under its open
        // window, and closing a Sailor's menu while it is up re-locks the cursor out from under it.
        private static PropertyInfo _cqWindowProp;
        private static bool _typesResolved = false;

        public static bool ShouldForceCursorFree()
        {
            // 1. Sailor's Companion Menus
            if (CanvasModUI.IsWindowOpen || CanvasInstalledModsUI.IsOpen)
            {
                return true;
            }

            // 2. Peer Mods (Farmer's Companion & Inventory Master)
            if (!_typesResolved)
            {
                ResolvePeerTypes();
            }

            if (_fcWindowProp != null)
            {
                try
                {
                    if ((bool)_fcWindowProp.GetValue(null)) return true;
                }
                catch { }
            }

            if (_imWindowProp != null)
            {
                try
                {
                    if ((bool)_imWindowProp.GetValue(null)) return true;
                }
                catch { }
            }

            if (_cqWindowProp != null)
            {
                try
                {
                    if ((bool)_cqWindowProp.GetValue(null)) return true;
                }
                catch { }
            }

            return false;
        }

        private static void ResolvePeerTypes()
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (_fcWindowProp == null)
                    {
                        var fcType = asm.GetType("FarmersCompanion.UI.CanvasFarmersCompanionUI");
                        if (fcType != null)
                        {
                            _fcWindowProp = fcType.GetProperty("IsWindowOpen", BindingFlags.Public | BindingFlags.Static);
                        }
                    }

                    if (_imWindowProp == null)
                    {
                        var imType = asm.GetType("InventoryMaster.UI.CanvasInventoryMasterUI");
                        if (imType != null)
                        {
                            _imWindowProp = imType.GetProperty("IsWindowOpen", BindingFlags.Public | BindingFlags.Static);
                        }
                    }

                    if (_cqWindowProp == null)
                    {
                        var cqType = asm.GetType("CollectionQoL.UI.CanvasCollectionQoLUI");
                        if (cqType != null)
                        {
                            _cqWindowProp = cqType.GetProperty("IsWindowOpen", BindingFlags.Public | BindingFlags.Static);
                        }
                    }
                }
            }
            catch { }
            finally
            {
                // Resolve once regardless of how many peers were found: this runs from
                // MouseLook.Update every frame, and only setting the flag once every peer resolved
                // meant an uninstalled peer caused a full AppDomain assembly scan forever.
                _typesResolved = true;
            }
        }
    }

    [HarmonyPatch(typeof(MouseLook), "Update")]
    public static class MouseLookUpdatePatch
    {
        public static bool Prefix()
        {
            if (CursorPatchHelper.ShouldForceCursorFree())
            {
                return false; // Freeze camera rotation completely while any mod UI is open!
            }
            return true;
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
    // [END] PATCH: CURSOR UNLOCKING & VISIBILITY & CAMERA FREEZE
    // ============================================================================
    #endregion
}
