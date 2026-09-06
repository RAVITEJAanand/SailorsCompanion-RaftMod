using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SailorsCompanion
{
    public static class InputHelper
    {
        public static bool WasKeyPressed(KeyCode legacyKey)
        {
            // 1. Try legacy Input
            try
            {
                if (Input.GetKeyDown(legacyKey)) return true;
            }
            catch { }

            // 2. Try New Input System
            try
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    switch (legacyKey)
                    {
                        case KeyCode.F5: return kb.f5Key.wasPressedThisFrame;
                        case KeyCode.F6: return kb.f6Key.wasPressedThisFrame;
                        case KeyCode.F7: return kb.f7Key.wasPressedThisFrame;
                        case KeyCode.F8: return kb.f8Key.wasPressedThisFrame;
                        case KeyCode.F9: return kb.f9Key.wasPressedThisFrame;
                        case KeyCode.F1: return kb.f1Key.wasPressedThisFrame;
                        case KeyCode.F: return kb.fKey.wasPressedThisFrame;
                        case KeyCode.Insert: return kb.insertKey.wasPressedThisFrame;
                        case KeyCode.Escape: return kb.escapeKey.wasPressedThisFrame;
                    }
                }
            }
            catch { }

            return false;
        }

        public static bool IsKeyHeld(KeyCode legacyKey)
        {
            try
            {
                if (Input.GetKey(legacyKey)) return true;
            }
            catch { }

            try
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    switch (legacyKey)
                    {
                        case KeyCode.W: return kb.wKey.isPressed;
                        case KeyCode.A: return kb.aKey.isPressed;
                        case KeyCode.S: return kb.sKey.isPressed;
                        case KeyCode.D: return kb.dKey.isPressed;
                        case KeyCode.Space: return kb.spaceKey.isPressed;
                        case KeyCode.LeftShift: return kb.leftShiftKey.isPressed;
                        case KeyCode.LeftControl: return kb.leftCtrlKey.isPressed;
                        case KeyCode.LeftAlt: return kb.leftAltKey.isPressed;
                    }
                }
            }
            catch { }

            return false;
        }
    }
}
