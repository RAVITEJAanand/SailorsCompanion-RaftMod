using System;
using UnityEngine;

namespace SailorsCompanion
{
    public static class PlayerHelper
    {
        private static Network_Player _cachedPlayer = null;
        private static float _lastSearchTime = 0f;

        public static Network_Player GetLocalPlayer()
        {
            if (_cachedPlayer != null && _cachedPlayer.gameObject != null && _cachedPlayer.gameObject.activeInHierarchy)
            {
                return _cachedPlayer;
            }

            // Throttle expensive scene scans to at most once per 500ms when player is not yet cached
            if (Time.unscaledTime - _lastSearchTime < 0.5f)
            {
                return _cachedPlayer;
            }
            _lastSearchTime = Time.unscaledTime;

            // 1. Direct ComponentManager<Network_Player>.Value
            try
            {
                var p = ComponentManager<Network_Player>.Value;
                if (p != null && p.gameObject != null && p.gameObject.activeInHierarchy)
                {
                    _cachedPlayer = p;
                    return p;
                }
            }
            catch { }

            // 2. Scan all Network_Player in the active scene
            try
            {
                var players = UnityEngine.Object.FindObjectsOfType<Network_Player>();
                if (players != null && players.Length > 0)
                {
                    foreach (var p in players)
                    {
                        if (p != null && p.gameObject != null && p.gameObject.activeInHierarchy && p.IsLocalPlayer)
                        {
                            _cachedPlayer = p;
                            return p;
                        }
                    }

                    // Fallback: in singleplayer, take any active player
                    foreach (var p in players)
                    {
                        if (p != null && p.gameObject != null && p.gameObject.activeInHierarchy)
                        {
                            _cachedPlayer = p;
                            return p;
                        }
                    }
                }
            }
            catch { }

            // 3. Fallback: via PersonController
            try
            {
                var pcs = UnityEngine.Object.FindObjectsOfType<PersonController>();
                if (pcs != null)
                {
                    foreach (var pc in pcs)
                    {
                        if (pc != null && pc.gameObject != null && pc.gameObject.activeInHierarchy)
                        {
                            var p = pc.GetComponentInParent<Network_Player>() ?? pc.GetComponent<Network_Player>();
                            if (p != null)
                            {
                                _cachedPlayer = p;
                                return p;
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
