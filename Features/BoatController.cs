using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Features
{
    // ============================================================================
    // [START] ENUM: SAIL CONTROL MODE
    // Purpose: Defines how sail rotation and angle alignment are governed.
    // ============================================================================
    public enum SailControlMode
    {
        Manual = 0,             // Normal gameplay: player adjusts sail angles by hand
        AutoAlignWind = 1,      // Automatically aligns all sails with optimal wind/drift direction
        FollowRaftDirection = 2 // Automatically aligns sails with current raft movement heading
    }
    // ============================================================================
    // [END] ENUM: SAIL CONTROL MODE
    // ============================================================================

    // ============================================================================
    // [START] MODULE: BOAT CONTROLLER & SMART SAIL AUTOMATION
    // Purpose: Provides unified yet separated control over raft propulsion:
    //          - Independent toggles for Engines and Sails
    //          - Non-destructive sail toggles that preserve steering and rudder angles
    //          - Smart Sail Auto-Align for maximum sailing speed
    // ============================================================================
    public class BoatController : MonoBehaviour
    {
        public static BoatController Instance { get; private set; }

        private float _lastAutoAlignTime = 0f;
        private const float AUTO_ALIGN_INTERVAL = 0.5f; // Update alignment every 500ms

        // ============================================================================
        // [START] LIFECYCLE INITIALIZATION
        // ============================================================================
        private void Awake()
        {
            Instance = this;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
        }
        // ============================================================================
        // [END] LIFECYCLE INITIALIZATION
        // ============================================================================

        // ============================================================================
        // [START] PER-FRAME SMART SAIL ALIGNMENT LOOP
        // ============================================================================
        private void Update()
        {
            if (Plugin.BoatSailMode == null || Plugin.BoatSailMode.Value == (int)SailControlMode.Manual)
            {
                return;
            }

            if (Time.time - _lastAutoAlignTime >= AUTO_ALIGN_INTERVAL)
            {
                _lastAutoAlignTime = Time.time;
                ApplyActiveSailMode();
            }
        }
        // ============================================================================
        // [END] PER-FRAME SMART SAIL ALIGNMENT LOOP
        // ============================================================================

        // ============================================================================
        // [START] ACTION: TOGGLE ALL ENGINES
        // Purpose: Toggles all raft engine wheels (MotorWheel) and bridge controls
        //          without affecting sail states or directions.
        // ============================================================================
        public static int ToggleAllEngines(bool? targetState = null, bool silent = false)
        {
            var motors = UnityEngine.Object.FindObjectsOfType<MotorWheel>();
            if (motors == null || motors.Length == 0)
            {
                if (!silent) TeleportManager.SetNotification("⚠️ No engines (Motor Wheels) found on the raft!");
                return 0;
            }

            // Determine target state: if null, turn ON if any are OFF, else turn OFF
            bool newState;
            if (targetState.HasValue)
            {
                newState = targetState.Value;
            }
            else
            {
                bool anyOff = false;
                foreach (var m in motors)
                {
                    if (m != null && !m.engineSwitchOn)
                    {
                        anyOff = true;
                        break;
                    }
                }
                newState = anyOff;
            }

            int count = 0;
            foreach (var motor in motors)
            {
                if (motor == null) continue;
                try
                {
                    if (motor.engineSwitchOn != newState)
                    {
                        motor.ToggleEngine();
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Sailor's Companion] Error toggling engine: {ex.Message}");
                }
            }

            if (Plugin.BoatEnginesOn != null)
            {
                Plugin.BoatEnginesOn.Value = newState;
            }

            if (!silent)
            {
                string stateIcon = newState ? "🟢 ON" : "🔴 OFF";
                TeleportManager.SetNotification($"⚙️ All Engines switched {stateIcon} ({motors.Length} engines)");
            }

            return count;
        }
        // ============================================================================
        // [END] ACTION: TOGGLE ALL ENGINES
        // ============================================================================

        // ============================================================================
        // [START] ACTION: TOGGLE ALL SAILS (NON-DESTRUCTIVE)
        // Purpose: Opens or closes all sails across the raft while strictly preserving
        //          their current rotation angle to prevent steering loss.
        // ============================================================================
        public static int ToggleAllSails(bool? targetState = null, bool silent = false)
        {
            var sails = Sail.AllSails;
            if (sails == null || sails.Count == 0)
            {
                if (!silent) TeleportManager.SetNotification("⚠️ No sails found on the raft!");
                return 0;
            }

            // Determine target state: if null, open all if any closed, else close all
            bool newState;
            if (targetState.HasValue)
            {
                newState = targetState.Value;
            }
            else
            {
                bool anyClosed = false;
                foreach (var s in sails)
                {
                    if (s != null && !s.open)
                    {
                        anyClosed = true;
                        break;
                    }
                }
                newState = anyClosed;
            }

            int count = 0;
            foreach (var sail in sails)
            {
                if (sail == null) continue;
                try
                {
                    if (newState)
                    {
                        if (!sail.open)
                        {
                            Traverse.Create(sail).Method("Open").GetValue();
                            count++;
                        }
                    }
                    else
                    {
                        if (sail.open)
                        {
                            Traverse.Create(sail).Method("Close").GetValue();
                            count++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Sailor's Companion] Error toggling sail: {ex.Message}");
                }
            }

            if (Plugin.BoatSailsOn != null)
            {
                Plugin.BoatSailsOn.Value = newState;
            }

            if (!silent)
            {
                string stateIcon = newState ? "🟢 OPEN" : "🔴 CLOSED";
                TeleportManager.SetNotification($"⛵ All Sails {stateIcon} ({sails.Count} sails)");
            }

            return count;
        }
        // ============================================================================
        // [END] ACTION: TOGGLE ALL SAILS
        // ============================================================================

        // ============================================================================
        // [START] ACTION: APPLY ACTIVE SMART SAIL ALIGNMENT
        // Purpose: Rotates sails automatically to optimal angle based on selected mode.
        // ============================================================================
        public static void ApplyActiveSailMode()
        {
            if (Plugin.BoatSailMode == null) return;
            SailControlMode mode = (SailControlMode)Plugin.BoatSailMode.Value;

            if (mode == SailControlMode.AutoAlignWind)
            {
                // In Raft, world drift direction is along Vector3.forward (positive Z)
                AlignSailsToDirection(Vector3.forward);
            }
            else if (mode == SailControlMode.FollowRaftDirection)
            {
                var raft = ComponentManager<Raft>.Value ?? UnityEngine.Object.FindObjectOfType<Raft>();
                if (raft != null)
                {
                    Vector3 vel = raft.VelocityDirection;
                    if (vel.sqrMagnitude > 0.01f)
                    {
                        AlignSailsToDirection(vel.normalized);
                    }
                }
            }
        }
        // ============================================================================
        // [END] ACTION: APPLY ACTIVE SMART SAIL ALIGNMENT
        // ============================================================================

        // ============================================================================
        // [START] HELPER: ALIGN SAILS TO TARGET WORLD DIRECTION
        // Purpose: Computes local angle for each sail transform and rotates smoothly.
        // ============================================================================
        public static int AlignSailsToDirection(Vector3 targetWorldDirection)
        {
            var sails = Sail.AllSails;
            if (sails == null || sails.Count == 0) return 0;

            int updated = 0;
            foreach (var sail in sails)
            {
                if (sail == null || !sail.open) continue;

                try
                {
                    Transform rotTrans = Traverse.Create(sail).Field("rotationTransform").GetValue<Transform>();
                    if (rotTrans == null) rotTrans = sail.transform;

                    Transform parent = rotTrans.parent;
                    Vector3 localDir = parent != null 
                        ? parent.InverseTransformDirection(targetWorldDirection) 
                        : targetWorldDirection;

                    localDir.y = 0f;
                    if (localDir.sqrMagnitude > 0.001f)
                    {
                        float targetAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
                        rotTrans.localRotation = Quaternion.Euler(0f, targetAngle, 0f);
                        updated++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Sailor's Companion] Error aligning sail: {ex.Message}");
                }
            }

            return updated;
        }
        // ============================================================================
        // [END] HELPER: ALIGN SAILS TO TARGET WORLD DIRECTION
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: BOAT CONTROLLER & SMART SAIL AUTOMATION
    // ============================================================================
}
