using System;
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

        private void Awake()
        {
            Instance = this;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
        }

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

        public static int WaterAllPlots(bool silent = false)
        {
            var plots = UnityEngine.Object.FindObjectsOfType<Cropplot>();
            if (plots == null || plots.Length == 0)
            {
                if (!silent) TeleportManager.SetNotification("🌱 No crop or grass plots found on the raft!");
                return 0;
            }

            int wateredCount = 0;
            foreach (var plot in plots)
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
    }
    // ============================================================================
    // [END] MODULE: FARMING HELPER & AUTOMATIC CROP WATERING
    // ============================================================================
}
