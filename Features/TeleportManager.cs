using System;
using UnityEngine;

namespace SailorsCompanion.Features
{
    public static class TeleportManager
    {
        public static string LastStatusMessage { get; private set; } = "";
        public static float LastStatusTime { get; private set; } = 0f;

        public static void SetNotification(string message)
        {
            LastStatusMessage = message;
            LastStatusTime = Time.unscaledTime;
            Debug.Log($"[Sailor's Companion] {message}");
        }

        /// <summary>
        /// Instantly teleports the local player back onto the Raft deck safely.
        /// </summary>
        public static bool TeleportPlayerToRaft(bool autoDropAnchor = false)
        {
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

            // 1. Disable Fly mode if active to avoid gravity conflicts
            Plugin.EnableFlyMode.Value = false;

            // 2. Determine target position on the raft deck
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

            // 3. Set Parent to Raft (GameManager.Singleton.lockedPivot)
            try
            {
                if (GameManager.Singleton != null && GameManager.Singleton.lockedPivot != null)
                {
                    player.transform.SetParentSafe(GameManager.Singleton.lockedPivot);
                }
            }
            catch { }

            // 4. Set Controller to Ground, reset fall velocity & duration
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

            // 5. Apply Position & Sync
            player.transform.position = targetPos;
            Physics.SyncTransforms();

            // 6. Optional: Drop anchor so raft doesn't drift away
            if (autoDropAnchor && !raft.IsAnchored)
            {
                try
                {
                    raft.AddAnchor(true, raft.gameObject);
                }
                catch { }
            }

            SetNotification($"⚡ Teleported back to Raft! (Traveled {prevDist:F0}m)");
            return true;
        }

        /// <summary>
        /// Teleports the Raft directly in front of the player (e.g. while on an island).
        /// </summary>
        public static bool TeleportRaftToPlayer()
        {
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

            // Position ~18 meters in front of the player at water level
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

                // Anchor the raft so it doesn't drift away
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

        /// <summary>
        /// Remotely toggles the Raft's anchor from anywhere in the world.
        /// </summary>
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
    }
}
