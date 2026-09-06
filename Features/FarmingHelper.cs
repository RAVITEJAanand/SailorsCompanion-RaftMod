using System;
using System.Collections.Generic;
using UnityEngine;
using SailorsCompanion.UI;

namespace SailorsCompanion.Features
{
    // ============================================================================
    // [START] MODULE: FARMING HELPER & AUTOMATIC CROP WATERING
    // Purpose: Automatically waters crops and animal grass plots across the raft,
    //          keeping all farming healthy and eliminating tedious cup-watering.
    // ============================================================================
    public class FarmingHelper : MonoBehaviour
    {
        public static FarmingHelper Instance { get; private set; }

        private float _lastAutoWaterTime = 0f;
        private const float AUTO_WATER_INTERVAL = 8f; // check every 8 seconds

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
        // [START] PERIODIC AUTO-WATER LOOP
        // ============================================================================
        private void Update()
        {
            if (Plugin.AutoWaterCrops != null && Plugin.AutoWaterCrops.Value)
            {
                if (Time.time - _lastAutoWaterTime >= AUTO_WATER_INTERVAL)
                {
                    _lastAutoWaterTime = Time.time;
                    WaterAllPlots(silent: true);
                }
            }
        }
        // ============================================================================
        // [END] PERIODIC AUTO-WATER LOOP
        // ============================================================================

        private static readonly List<Cropplot> _cachedPlots = new List<Cropplot>();
        private static float _lastPlotsScanTime = -30f;

        // ============================================================================
        // [START] ACTION: WATER ALL PLOTS
        // ============================================================================
        public static int WaterAllPlots(bool silent = false)
        {
            if (Time.unscaledTime - _lastPlotsScanTime > 20f || _cachedPlots.Count == 0)
            {
                _lastPlotsScanTime = Time.unscaledTime;
                _cachedPlots.Clear();
                var found = UnityEngine.Object.FindObjectsOfType<Cropplot>();
                if (found != null && found.Length > 0)
                {
                    _cachedPlots.AddRange(found);
                }
            }
            else
            {
                _cachedPlots.RemoveAll(p => p == null);
            }

            if (_cachedPlots.Count == 0)
            {
                if (!silent) TeleportManager.SetNotification("🌱 No crop or grass plots found on the raft!");
                return 0;
            }

            int wateredCount = 0;
            foreach (var plot in _cachedPlots)
            {
                if (plot == null) continue;

                if (plot.SlotsNeedWater())
                {
                    try
                    {
                        plot.AddWater(true);
                        wateredCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[Sailor's Companion] Failed to water plot: {ex.Message}");
                    }
                }
            }

            if (!silent)
            {
                if (wateredCount > 0)
                {
                    TeleportManager.SetNotification($"🌱 Watered {wateredCount} crop & grass plots!");
                }
                else
                {
                    TeleportManager.SetNotification("🌱 All crop and grass plots are already fully watered!");
                }
            }

            return wateredCount;
        }
        // ============================================================================
        // [END] ACTION: WATER ALL PLOTS
        // ============================================================================
    }
    // ============================================================================
    // [END] MODULE: FARMING HELPER & AUTOMATIC CROP WATERING
    // ============================================================================
}
