using System;
using UnityEngine;

namespace SailorsCompanion.Features
{
    // ============================================================================
    // [START] MODULE: TELEPORTATION & RAFT RECOVERY SYSTEM
    // Purpose: Handles recalling players to the raft deck, summoning the raft
    //          to the player's position, and controlling the raft's anchor remotely.
    // ============================================================================
    public static class TeleportManager
    {
        #region [START] NOTIFICATION ENGINE
        // ============================================================================
        // [START] NOTIFICATION ENGINE
        // ============================================================================
        public static string LastStatusMessage { get; private set; } = "";
        public static float LastStatusTime { get; private set; } = 0f;

        public static void SetNotification(string message)
        {
            LastStatusMessage = message;
            LastStatusTime = Time.unscaledTime;
            Debug.Log($"[Sailor's Companion] {message}");
        }
        // ============================================================================
        // [END] NOTIFICATION ENGINE
        // ============================================================================
        #endregion

        #region [START] FEATURE 1: PLAYER RECALL TO RAFT (F8)
        // ============================================================================
        // [START] FEATURE 1: PLAYER RECALL TO RAFT (Hotkey: F8)
        // Description: Safely teleports the local player back onto the raft deck from
        //              anywhere in the world (deep ocean, island peaks, etc.).
        // ============================================================================
        private static float _lastRecallTime = -9999f;
        public const float SURVIVAL_RECALL_COOLDOWN = 180f; // 3 minutes in Survival Mode

        public static float GetRecallCooldownRemaining()
        {
            if (Plugin.IsCreativeMode) return 0f;
            float elapsed = Time.unscaledTime - _lastRecallTime;
            float rem = SURVIVAL_RECALL_COOLDOWN - elapsed;
            return rem > 0f ? rem : 0f;
        }

        public static bool TeleportPlayerToRaft(bool autoDropAnchor = false)
        {
            float cooldownRem = GetRecallCooldownRemaining();
            if (cooldownRem > 0f)
            {
                int sec = Mathf.CeilToInt(cooldownRem);
                int mins = sec / 60;
                int remainingSec = sec % 60;
                string timeStr = mins > 0 ? $"{mins}m {remainingSec}s" : $"{remainingSec}s";
                SetNotification($"⏳ Recall on cooldown in Survival Mode: {timeStr} remaining.");
                return false;
            }

            var player = PlayerHelper.GetLocalPlayer();
            if (player == null)
            {
                SetNotification("Cannot teleport: Player not found!");
                return false;
            }

            var raft = ComponentManager<Raft>.Value ?? UnityEngine.Object.FindObjectOfType<Raft>();
            if (raft == null)
            {
                SetNotification("Cannot teleport: Raft not found in world!");
                return false;
            }

            float prevDist = Vector3.Distance(player.transform.position, raft.transform.position);

            // Step 1: Disable Fly mode if active to avoid gravity conflicts
            Plugin.EnableFlyMode.Value = false;

            // Step 2: Determine target position on the raft deck
            Vector3 targetPos = raft.transform.position + Vector3.up * 2.2f;
            try
            {
                var blocks = BlockCreator.GetPlacedBlocks();
                if (blocks != null && blocks.Count > 0)
                {
                    // Look for a walkable block closest to raft center
                    Block bestBlock = null;
                    float bestDist = float.MaxValue;
                    foreach (var b in blocks)
                    {
                        if (b != null && b.IsWalkable())
                        {
                            float d = Vector3.Distance(b.transform.position, raft.transform.position);
                            if (d < bestDist)
                            {
                                bestDist = d;
                                bestBlock = b;
                            }
                        }
                    }

                    if (bestBlock != null)
                    {
                        targetPos = bestBlock.transform.position + Vector3.up * 1.6f;
                    }
                }
            }
            catch { }

            // Step 3: Set Parent to Raft (GameManager.Singleton.lockedPivot)
            try
            {
                if (GameManager.Singleton != null && GameManager.Singleton.lockedPivot != null)
                {
                    player.transform.SetParentSafe(GameManager.Singleton.lockedPivot);
                }
            }
            catch { }

            // Step 4: Set Controller to Ground, reset fall velocity & duration
            try
            {
                if (player.PersonController != null)
                {
                    player.PersonController.ResetExternalVelocity();
                    player.PersonController.ResetFallDuration();
                    var cc = player.PersonController.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = true;
                    player.PersonController.SwitchControllerType(ControllerType.Ground);
                }
            }
            catch { }

            // Step 5: Apply Position & Sync transforms with physics engine
            player.transform.position = targetPos;
            Physics.SyncTransforms();

            // Step 6: Optional - Drop anchor so raft doesn't drift away
            if (autoDropAnchor && !raft.IsAnchored)
            {
                try
                {
                    raft.AddAnchor(true, raft.gameObject);
                }
                catch { }
            }

            _lastRecallTime = Time.unscaledTime;
            SetNotification($"⚡ Teleported back to Raft! (Traveled {prevDist:F0}m)");
            return true;
        }
        // ============================================================================
        // [END] FEATURE 1: PLAYER RECALL TO RAFT
        // ============================================================================
        #endregion

        #region [START] FEATURE 2: SUMMON RAFT TO PLAYER (F9)
        // ============================================================================
        // [START] FEATURE 2: SUMMON RAFT TO PLAYER (Hotkey: F9)
        // Description: Pulls the raft directly in front of the player on the ocean
        //              water level (~18m away) and automatically anchors it.
        // ============================================================================
        public static bool TeleportRaftToPlayer()
        {
            if (Plugin.IsSurvivalMode)
            {
                SetNotification("🔒 Summoning the Raft is restricted to Creative Mode.");
                return false;
            }

            var player = PlayerHelper.GetLocalPlayer();
            if (player == null)
            {
                SetNotification("Cannot summon raft: Player not found!");
                return false;
            }

            var raft = ComponentManager<Raft>.Value ?? UnityEngine.Object.FindObjectOfType<Raft>();
            if (raft == null)
            {
                SetNotification("Cannot summon raft: Raft not found!");
                return false;
            }

            var cam = Camera.main;
            Vector3 forward = cam != null ? cam.transform.forward : player.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f) forward = player.transform.forward;
            forward.Normalize();

            // Position ~18 meters in front of the player at water level (y = 0)
            Vector3 targetPos = player.transform.position + forward * 18f;
            targetPos.y = 0f;

            try
            {
                raft.transform.position = targetPos;
                var rb = raft.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.position = targetPos;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                var netPosField = typeof(Raft).GetField("networkPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                netPosField?.SetValue(raft, targetPos);

                var anchorPosField = typeof(Raft).GetField("anchorPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                anchorPosField?.SetValue(raft, targetPos);

                Physics.SyncTransforms();

                // Automatically anchor the raft so it stays put
                if (!raft.IsAnchored)
                {
                    raft.AddAnchor(true, raft.gameObject);
                }
            }
            catch (Exception ex)
            {
                SetNotification("Error summoning raft: " + ex.Message);
                return false;
            }

            SetNotification("⛵ Raft summoned in front of you and anchored!");
            return true;
        }
        // ============================================================================
        // [END] FEATURE 2: SUMMON RAFT TO PLAYER
        // ============================================================================
        #endregion

        #region [START] FEATURE 3: REMOTE RAFT ANCHOR CONTROL
        // ============================================================================
        // [START] FEATURE 3: REMOTE RAFT ANCHOR CONTROL
        // Description: Allows dropping or raising the raft's anchor from anywhere.
        // ============================================================================
        public static bool ToggleRaftAnchor()
        {
            var raft = ComponentManager<Raft>.Value ?? UnityEngine.Object.FindObjectOfType<Raft>();
            if (raft == null)
            {
                SetNotification("Raft not found!");
                return false;
            }

            try
            {
                if (raft.IsAnchored)
                {
                    raft.RemoveAnchor(raft.AnchorCount > 0 ? raft.AnchorCount : 1);
                    SetNotification("⚓ Raft Unanchored (Drifting)");
                }
                else
                {
                    raft.AddAnchor(true, raft.gameObject);
                    SetNotification("⚓ Raft Anchored Remotely!");
                }
                return true;
            }
            catch (Exception ex)
            {
                SetNotification("Error toggling anchor: " + ex.Message);
                return false;
            }
        }
        // ============================================================================
        // [END] FEATURE 3: REMOTE RAFT ANCHOR CONTROL
        // ============================================================================
        #endregion
    }
    // ============================================================================
    // [END] MODULE: TELEPORTATION & RAFT RECOVERY SYSTEM
    // ============================================================================
}
