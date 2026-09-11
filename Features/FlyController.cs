using UnityEngine;

namespace SailorsCompanion.Features
{
    // ============================================================================
    // [START] MODULE: FLY / NOCLIP CONTROLLER
    // Purpose: Provides smooth 6-axis flight (WASD + Space/Shift + Alt Boost)
    //          by safely toggling Unity's CharacterController.
    // ============================================================================
    public class FlyController : MonoBehaviour
    {
        #region [START] STATE & FIELDS
        // ============================================================================
        // [START] STATE & FIELDS
        // ============================================================================
        private bool _isFlying = false;
        private CharacterController _characterController = null;
        private Network_Player _currentPlayer = null;
        // ============================================================================
        // [END] STATE & FIELDS
        // ============================================================================
        #endregion

        #region [START] LIFECYCLE & UPDATE LOOP
        // ============================================================================
        // [START] LIFECYCLE & UPDATE LOOP
        // ============================================================================
        private void Update()
        {
            var localPlayer = PlayerHelper.GetLocalPlayer();
            if (localPlayer == null)
            {
                if (_isFlying)
                {
                    DisableFly();
                }
                return;
            }

            if (Plugin.EnableFlyMode != null && Plugin.EnableFlyMode.Value)
            {
                if (!_isFlying || _currentPlayer != localPlayer)
                {
                    EnableFly(localPlayer);
                }

                HandleFlightMovement(localPlayer);
            }
            else
            {
                if (_isFlying)
                {
                    DisableFly();
                }
            }
        }
        // ============================================================================
        // [END] LIFECYCLE & UPDATE LOOP
        // ============================================================================
        #endregion

        #region [START] FLIGHT ENGAGEMENT (CHARACTER CONTROLLER HOOK)
        // ============================================================================
        // [START] FLIGHT ENGAGEMENT (CHARACTER CONTROLLER HOOK)
        // ============================================================================
        private void EnableFly(Network_Player player)
        {
            // If we were already flying under a different player reference (e.g. a
            // respawn/reconnect swapped the local player object), restore the old
            // player's controller first so its CharacterController is never left
            // permanently disabled.
            if (_isFlying && _currentPlayer != player)
            {
                DisableFly();
            }

            _currentPlayer = player;
            if (player.PersonController != null)
            {
                _characterController = player.PersonController.GetComponent<CharacterController>();
                if (_characterController != null)
                {
                    _characterController.enabled = false;
                }
            }
            _isFlying = true;
        }

        private void DisableFly()
        {
            if (_characterController != null)
            {
                _characterController.enabled = true;
            }

            // Clear any stale fall/external velocity accumulated by PersonController
            // while noclip was active, otherwise re-enabling the CharacterController
            // can yank the player downward or launch them on the very next frame.
            try
            {
                if (_currentPlayer != null && _currentPlayer.PersonController != null)
                {
                    _currentPlayer.PersonController.ResetExternalVelocity();
                    _currentPlayer.PersonController.ResetFallDuration();
                }
            }
            catch { }

            _isFlying = false;
            _characterController = null;
            _currentPlayer = null;
        }
        // ============================================================================
        // [END] FLIGHT ENGAGEMENT
        // ============================================================================
        #endregion

        #region [START] 6-AXIS FLIGHT MOVEMENT ENGINE
        // ============================================================================
        // [START] 6-AXIS FLIGHT MOVEMENT ENGINE
        // ============================================================================
        private void HandleFlightMovement(Network_Player player)
        {
            // Do not move player if the Mod Menu is currently open
            if (SailorsCompanion.UI.CanvasModUI.IsWindowOpen) return;

            var cam = Camera.main;
            if (cam == null) return;

            Vector3 move = Vector3.zero;

            // Horizontal & Forward/Backward (Camera-relative)
            if (InputHelper.IsKeyHeld(KeyCode.W)) move += cam.transform.forward;
            if (InputHelper.IsKeyHeld(KeyCode.S)) move -= cam.transform.forward;
            if (InputHelper.IsKeyHeld(KeyCode.D)) move += cam.transform.right;
            if (InputHelper.IsKeyHeld(KeyCode.A)) move -= cam.transform.right;

            // Vertical Ascend & Descend (LeftControl = down, LeftShift = speed boost)
            if (InputHelper.IsKeyHeld(KeyCode.Space)) move += Vector3.up;
            if (InputHelper.IsKeyHeld(KeyCode.LeftControl)) move -= Vector3.up;

            if (move.sqrMagnitude > 0.001f)
            {
                float speed = Plugin.FlySpeed != null ? Plugin.FlySpeed.Value : 14f;
                if (InputHelper.IsKeyHeld(KeyCode.LeftShift))
                {
                    speed *= 2.5f; // Turbo boost (LeftShift)
                }

                player.transform.position += move.normalized * speed * Time.deltaTime;
            }
        }

        private void OnDisable()
        {
            if (_isFlying)
            {
                DisableFly();
            }
        }
        // ============================================================================
        // [END] 6-AXIS FLIGHT MOVEMENT ENGINE
        // ============================================================================
        #endregion
    }
    // ============================================================================
    // [END] MODULE: FLY / NOCLIP CONTROLLER
    // ============================================================================
}
