using UnityEngine;

namespace SailorsCompanion.Features
{
    public class FlyController : MonoBehaviour
    {
        private bool _isFlying = false;
        private CharacterController _characterController = null;
        private Network_Player _currentPlayer = null;

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

            if (Plugin.EnableFlyMode.Value)
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

        private void EnableFly(Network_Player player)
        {
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
            _isFlying = false;
            _characterController = null;
            _currentPlayer = null;
        }

        private void HandleFlightMovement(Network_Player player)
        {
            if (SailorsCompanion.UI.CanvasModUI.IsWindowOpen) return;

            var cam = Camera.main;
            if (cam == null) return;

            Vector3 move = Vector3.zero;

            // Camera-relative directions
            if (InputHelper.IsKeyHeld(KeyCode.W)) move += cam.transform.forward;
            if (InputHelper.IsKeyHeld(KeyCode.S)) move -= cam.transform.forward;
            if (InputHelper.IsKeyHeld(KeyCode.D)) move += cam.transform.right;
            if (InputHelper.IsKeyHeld(KeyCode.A)) move -= cam.transform.right;

            // Up / Down
            if (InputHelper.IsKeyHeld(KeyCode.Space)) move += Vector3.up;
            if (InputHelper.IsKeyHeld(KeyCode.LeftShift) || InputHelper.IsKeyHeld(KeyCode.LeftControl)) move -= Vector3.up;

            if (move.sqrMagnitude > 0.001f)
            {
                float speed = Plugin.FlySpeed.Value;
                if (InputHelper.IsKeyHeld(KeyCode.LeftAlt))
                {
                    speed *= 2.5f; // Turbo boost
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
    }
}
