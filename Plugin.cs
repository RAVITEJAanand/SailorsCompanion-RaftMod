using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using SailorsCompanion.Features;
using SailorsCompanion.UI;
using UnityEngine;

namespace SailorsCompanion
{
    // ============================================================================
    // [START] MAIN PLUGIN ENTRY POINT: SAILOR'S COMPANION
    // Description: Orchestrates BepInEx lifecycle, Harmony patches, configuration,
    //              and real-time survival stats monitoring.
    // ============================================================================
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Raft.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static GameObject ManagerGO { get; private set; }

        #region [START] CONFIGURATION DEFINITIONS
        // ============================================================================
        // [START] CONFIGURATION DEFINITIONS (General & Feature Settings)
        // ============================================================================
        public static ConfigEntry<bool> EnableHUD;
        public static ConfigEntry<int> HUDStyle;
        public static ConfigEntry<KeyCode> KeyMenu;
        public static ConfigEntry<KeyCode> KeyHUD;
        public static ConfigEntry<KeyCode> KeyFly;
        public static ConfigEntry<KeyCode> KeyTeleportToRaft;
        public static ConfigEntry<KeyCode> KeyTeleportRaftToPlayer;

        public static ConfigEntry<bool> InfiniteDurability;
        public static ConfigEntry<bool> AntiSharkRaftDamage;
        public static ConfigEntry<bool> InfiniteOxygen;
        public static ConfigEntry<bool> GodMode;
        public static ConfigEntry<bool> NoHungerThirst;
        public static ConfigEntry<int> CustomStackSize;
        public static ConfigEntry<float> SwimSpeedMultiplier;
        public static ConfigEntry<float> SprintSpeedMultiplier;
        public static ConfigEntry<float> HookPullSpeedMultiplier;
        public static ConfigEntry<bool> FreeCrafting;
        public static ConfigEntry<bool> EnableFlyMode;
        public static ConfigEntry<float> FlySpeed;
        public static ConfigEntry<bool> CheckForUpdates;
        public static ConfigEntry<bool> AutoWaterCrops;
        public static ConfigEntry<bool> CraftFromStorage;
        public static ConfigEntry<bool> AutoEmptyCollectionNets;
        public static ConfigEntry<bool> EnableCropGrowthBoost;
        public static ConfigEntry<float> CropGrowthMultiplier;
        public static ConfigEntry<bool> ShowAnimalHealthBars;
        public static ConfigEntry<float> WeaponDamageMultiplier;
        public static ConfigEntry<bool> OneHitKill;
        public static ConfigEntry<string> ModGameMode;
        public static ConfigEntry<string> ActiveProfile;

        // [START] CONFIGURATIONS: ADVANCED HOTKEYS
        public static ConfigEntry<bool> EnableHotkeys;
        public static ConfigEntry<KeyCode> KeySailToggle;
        public static ConfigEntry<KeyCode> KeyEngineToggle;
        public static ConfigEntry<KeyCode> KeyMagnetToggle;
        public static ConfigEntry<KeyCode> KeyScannerPulse;
        // [END] CONFIGURATIONS: ADVANCED HOTKEYS

        // [START] CONFIGURATIONS: BOAT & PROPULSION SYSTEMS
        public static ConfigEntry<bool> BoatEnginesOn;
        public static ConfigEntry<bool> BoatSailsOn;
        public static ConfigEntry<int> BoatSailMode;
        // [END] CONFIGURATIONS: BOAT & PROPULSION SYSTEMS

        // [START] CONFIGURATIONS: REEF & ISLAND HARVESTING
        public static ConfigEntry<bool> IslandHandPickup;
        public static ConfigEntry<bool> ReefHandHarvesting;
        public static ConfigEntry<bool> ReefFastHarvest;
        // [END] CONFIGURATIONS: REEF & ISLAND HARVESTING

        // [START] CONFIGURATIONS: OCEAN MAGNETIC PULL
        public static ConfigEntry<bool> MagnetActive;
        public static ConfigEntry<float> MagnetRadius;
        // [END] CONFIGURATIONS: OCEAN MAGNETIC PULL

        public static bool IsCreativeMode => ModGameMode != null && ModGameMode.Value == "Creative";
        public static bool IsSurvivalMode => !IsCreativeMode;

        private Harmony _harmony;
        private float _baseSwimSpeed = -1f;
        private float _baseSprintSpeed = -1f;
        // ============================================================================
        // [END] CONFIGURATION DEFINITIONS
        // ============================================================================
        #endregion

        #region [START] PLUGIN INITIALIZATION & CONFIG BINDING
        // ============================================================================
        // [START] PLUGIN INITIALIZATION & CONFIG BINDING
        // ============================================================================
        private void Awake()
        {
            Instance = this;

            // Bind Hotkeys
            KeyMenu = Config.Bind("General.Hotkeys", "KeyMenu", KeyCode.F5, "Hotkey to toggle the in-game GUI menu.");
            KeyHUD = Config.Bind("General.Hotkeys", "KeyHUD", KeyCode.F6, "Hotkey to toggle the navigation HUD overlay.");
            KeyFly = Config.Bind("General.Hotkeys", "KeyFly", KeyCode.F, "Hotkey to toggle Fly / Noclip mode.");
            KeyTeleportToRaft = Config.Bind("General.Hotkeys", "KeyTeleportToRaft", KeyCode.F8, "Hotkey to instantly recall/teleport player back onto the raft.");
            KeyTeleportRaftToPlayer = Config.Bind("General.Hotkeys", "KeyTeleportRaftToPlayer", KeyCode.F9, "Hotkey to summon raft to player's current location.");

            // Bind Profile Mode & Presets
            ModGameMode = Config.Bind("General.Profile", "ModGameMode", "Survival", "Mod mode: 'Survival' for balanced QoL, 'Creative' for unrestricted sandbox cheats.");
            ActiveProfile = Config.Bind("General.Profile", "ActiveProfile", "Custom", "Active gameplay preset profile: 'VanillaPlus', 'CozyFarming', 'MasterBuilder', or 'Custom'.");

            // Bind Navigation Settings
            EnableHUD = Config.Bind("Features.Navigation", "EnableHUD", true, "Show the real-time compass, coordinates, raft tracker, and shark radar.");
            HUDStyle = Config.Bind("Features.Navigation", "HUDStyle", 1, "HUD Style: 0=Sleek Ribbon, 1=Top Compass Bar, 2=Minimalist Pill, 3=Compact Box.");
            ShowAnimalHealthBars = Config.Bind("Features.Navigation", "ShowAnimalHealthBars", true, "Render floating health bars and distance tags above animals and enemies.");

            // Bind Survival & World Settings
            InfiniteDurability = Config.Bind("Features.Survival", "InfiniteDurability", true, "Tools, weapons, hooks, and armor never lose durability.");
            AntiSharkRaftDamage = Config.Bind("Features.World", "AntiSharkRaftDamage", true, "Stops the shark from attacking or damaging raft blocks.");
            InfiniteOxygen = Config.Bind("Features.Survival", "InfiniteOxygen", true, "Allows diving freely without running out of oxygen.");
            GodMode = Config.Bind("Features.Survival", "GodMode", false, "Invulnerable to all damage.");
            NoHungerThirst = Config.Bind("Features.Survival", "NoHungerThirst", false, "Freeze hunger and thirst meters at maximum.");
            CustomStackSize = Config.Bind("Features.Inventory", "CustomStackSize", 40, "Maximum stack size for stackable resources.");

            // Bind QoL Automation & Crafting
            CraftFromStorage = Config.Bind("Features.Crafting", "CraftFromStorage", true, "Craft items directly using materials stored in nearby storage chests (22m).");
            AutoEmptyCollectionNets = Config.Bind("Features.World", "AutoEmptyCollectionNets", false, "Continuously auto-empty all raft collection nets into inventory.");
            AutoWaterCrops = Config.Bind("Features.Farming", "AutoWaterCrops", false, "Continuously auto-water all crop plots and animal grass.");
            EnableCropGrowthBoost = Config.Bind("Features.Farming", "EnableCropGrowthBoost", true, "Accelerate crop, flower, and tree growth speed.");
            CropGrowthMultiplier = Config.Bind("Features.Farming", "CropGrowthMultiplier", 1.5f, "Crop growth speed multiplier (e.g. 1.5 = 50% faster).");

            // Bind Movement, Speeds & Combat
            SwimSpeedMultiplier = Config.Bind("Features.Movement", "SwimSpeedMultiplier", 1.2f, "Multiplier for player swimming speed.");
            SprintSpeedMultiplier = Config.Bind("Features.Movement", "SprintSpeedMultiplier", 1.1f, "Multiplier for player sprinting speed.");
            HookPullSpeedMultiplier = Config.Bind("Features.World", "HookPullSpeedMultiplier", 1.5f, "Multiplier for hook debris reeling speed.");
            WeaponDamageMultiplier = Config.Bind("Features.Combat", "WeaponDamageMultiplier", 1.0f, "Multiplier for outgoing weapon & arrow damage against enemies (1.0 = normal).");
            OneHitKill = Config.Bind("Features.Combat", "OneHitKill", false, "Instantly slay any enemy or predator in one hit (Creative Mode).");
            FreeCrafting = Config.Bind("Features.World", "FreeCrafting", false, "Craft any item without consuming materials.");
            EnableFlyMode = Config.Bind("Features.Movement", "EnableFlyMode", false, "Fly / Noclip mode.");
            FlySpeed = Config.Bind("Features.Movement", "FlySpeed", 14f, "Flight speed in m/s.");
            CheckForUpdates = Config.Bind("Features.General", "CheckForUpdates", true, "Check online for mod updates on startup.");

            // Bind Advanced Hotkeys (Default: OFF, for pro users)
            EnableHotkeys = Config.Bind("General.Hotkeys", "EnableHotkeys", false, "Enable direct gameplay hotkeys for quick actions (Advanced Users).");
            KeySailToggle = Config.Bind("General.Hotkeys", "KeySailToggle", KeyCode.F4, "Hotkey to toggle all sails open or closed.");
            KeyEngineToggle = Config.Bind("General.Hotkeys", "KeyEngineToggle", KeyCode.F3, "Hotkey to toggle all engines on or off.");
            KeyMagnetToggle = Config.Bind("General.Hotkeys", "KeyMagnetToggle", KeyCode.F7, "Hotkey to activate ocean magnetic debris pull.");
            KeyScannerPulse = Config.Bind("General.Hotkeys", "KeyScannerPulse", KeyCode.F10, "Hotkey to trigger a 10s island pulse scan.");

            // Bind Boat Control Systems
            BoatEnginesOn = Config.Bind("Features.Boat", "BoatEnginesOn", false, "Raft engines power state.");
            BoatSailsOn = Config.Bind("Features.Boat", "BoatSailsOn", false, "Raft sails open state.");
            BoatSailMode = Config.Bind("Features.Boat", "BoatSailMode", 0, "Sail mode: 0=Manual, 1=AutoAlignWind, 2=FollowRaftDirection.");

            // Bind Exploration, Reef & Island Harvesting
            IslandHandPickup = Config.Bind("Features.Exploration", "IslandHandPickup", true, "Pick up surface island items by hand without hook.");
            ReefHandHarvesting = Config.Bind("Features.Exploration", "ReefHandHarvesting", true, "Harvest Sand, Clay, Scrap, and Ores underwater by hand without a hook.");
            ReefFastHarvest = Config.Bind("Features.Exploration", "ReefFastHarvest", true, "Accelerate reef channeling so players can mine safely before shark attacks.");

            // Bind Ocean Magnetic Debris Pull
            MagnetActive = Config.Bind("Features.World", "MagnetActive", false, "Magnetic debris pull active state.");
            MagnetRadius = Config.Bind("Features.World", "MagnetRadius", 20f, "Effective radius for magnetic debris pull in meters.");

            // Register Harmony Patches
            RegisterHarmonyPatches();

            // Create persistent Manager GameObject protected from Unity asset cleaning
            EnsureManager();

            Logger.LogInfo($"[{PluginInfo.PLUGIN_NAME}] v{PluginInfo.PLUGIN_VERSION} initialized successfully! Press F5 for menu, F6 for HUD.");
        }
        // ============================================================================
        // [END] PLUGIN INITIALIZATION & CONFIG BINDING
        // ============================================================================
        #endregion

        #region [START] HARMONY PATCH REGISTRATION
        // ============================================================================
        // [START] HARMONY PATCH REGISTRATION
        // ============================================================================
        private void RegisterHarmonyPatches()
        {
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            try
            {
                var types = Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    if (type.GetCustomAttributes(typeof(HarmonyPatch), true).Length > 0)
                    {
                        try
                        {
                            _harmony.CreateClassProcessor(type).Patch();
                            Logger.LogInfo($"[{PluginInfo.PLUGIN_NAME}] Applied patch: {type.Name}");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"[{PluginInfo.PLUGIN_NAME}] Failed patch {type.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[{PluginInfo.PLUGIN_NAME}] Critical error discovering Harmony patches: {ex.Message}");
            }
        }
        // ============================================================================
        // [END] HARMONY PATCH REGISTRATION
        // ============================================================================
        #endregion

        #region [START] PERSISTENT MANAGER LIFECYCLE
        // ============================================================================
        // [START] PERSISTENT MANAGER LIFECYCLE
        // ============================================================================
        public static void EnsureManager()
        {
            if (ManagerGO == null)
            {
                ManagerGO = new GameObject("SailorsCompanion_Manager");
                ManagerGO.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(ManagerGO);
                ManagerGO.AddComponent<CanvasModUI>();
                ManagerGO.AddComponent<CanvasInstalledModsUI>();
                ManagerGO.AddComponent<ModGUI>();
                ManagerGO.AddComponent<HUDOverlay>();
                ManagerGO.AddComponent<CreatureHealthOverlay>();
                ManagerGO.AddComponent<FlyController>();
                ManagerGO.AddComponent<FarmingHelper>();
                ManagerGO.AddComponent<NetsHelper>();
                ManagerGO.AddComponent<BoatController>();
                ManagerGO.AddComponent<ItemDetector>();
                ManagerGO.AddComponent<ReefHandHarvesting>();
                ManagerGO.AddComponent<MagneticCollector>();
                ManagerGO.AddComponent<UpdateChecker>();
                Debug.Log("[Sailor's Companion] Initialized persistent SailorsCompanion_Manager with HideAndDontSave protection.");
            }
        }
        // ============================================================================
        // [END] PERSISTENT MANAGER LIFECYCLE
        // ============================================================================
        #endregion

        #region [START] PER-FRAME ENGINE UPDATES
        // ============================================================================
        // [START] PER-FRAME ENGINE UPDATES
        // ============================================================================
        private static int _lastUpdateFrame = -1;

        public void OnGameManagerUpdate()
        {
            if (Time.frameCount == _lastUpdateFrame) return;
            _lastUpdateFrame = Time.frameCount;

            // Hotkey fallback if CanvasModUI has not initialized yet
            if (CanvasModUI.Instance == null)
            {
                if (InputHelper.WasKeyPressed(KeyMenu.Value) || InputHelper.WasKeyPressed(KeyCode.Insert))
                {
                    CanvasModUI.Instance?.ToggleModWindow();
                }

                if (EnableHotkeys != null && EnableHotkeys.Value)
                {
                    if (KeySailToggle != null && InputHelper.WasKeyPressed(KeySailToggle.Value))
                    {
                        BoatController.ToggleAllSails();
                    }
                    if (KeyEngineToggle != null && InputHelper.WasKeyPressed(KeyEngineToggle.Value))
                    {
                        BoatController.ToggleAllEngines();
                    }
                    if (KeyMagnetToggle != null && InputHelper.WasKeyPressed(KeyMagnetToggle.Value))
                    {
                        MagneticCollector.ToggleMagnet();
                    }
                    if (KeyScannerPulse != null && InputHelper.WasKeyPressed(KeyScannerPulse.Value))
                    {
                        ItemDetector.TriggerPulseScan();
                    }
                }
            }

            // Continuous updates for local player stats and speeds
            var player = PlayerHelper.GetLocalPlayer();
            if (player != null)
            {
                UpdatePlayerStats(player);
                UpdatePlayerSpeed(player);
            }
        }
        // ============================================================================
        // [END] PER-FRAME ENGINE UPDATES
        // ============================================================================
        #endregion

        #region [START] SURVIVAL STATS & MOVEMENT MULTIPLIERS
        // ============================================================================
        // [START] SURVIVAL STATS & MOVEMENT MULTIPLIERS
        // ============================================================================
        private void UpdatePlayerStats(Network_Player player)
        {
            if (player.Stats == null) return;

            if (NoHungerThirst.Value)
            {
                player.Stats.stat_hunger?.Normal?.SetToMaxValue();
                player.Stats.stat_thirst?.Normal?.SetToMaxValue();
            }

            if (InfiniteOxygen.Value)
            {
                player.Stats.stat_oxygen?.SetToMaxValue();
            }

            if (GodMode.Value)
            {
                player.Stats.stat_health?.SetToMaxValue();
            }
        }

        private void UpdatePlayerSpeed(Network_Player player)
        {
            var pc = player.PersonController;
            if (pc == null) return;

            // Capture initial base speeds
            if (_baseSwimSpeed <= 0f && pc.swimSpeed > 0.1f)
            {
                _baseSwimSpeed = pc.swimSpeed;
            }
            if (_baseSprintSpeed <= 0f && pc.sprintSpeed > 0.1f)
            {
                _baseSprintSpeed = pc.sprintSpeed;
            }

            // Apply multipliers (strictly clamped to balanced survival ranges in Survival Mode)
            float swimMult = SwimSpeedMultiplier != null ? SwimSpeedMultiplier.Value : 1.0f;
            float sprintMult = SprintSpeedMultiplier != null ? SprintSpeedMultiplier.Value : 1.0f;
            if (IsSurvivalMode)
            {
                swimMult = UnityEngine.Mathf.Clamp(swimMult, 1.0f, 1.5f);
                sprintMult = UnityEngine.Mathf.Clamp(sprintMult, 1.0f, 1.5f);
            }

            if (_baseSwimSpeed > 0f)
            {
                pc.swimSpeed = _baseSwimSpeed * swimMult;
            }
            if (_baseSprintSpeed > 0f)
            {
                pc.sprintSpeed = _baseSprintSpeed * sprintMult;
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
        // ============================================================================
        // [END] SURVIVAL STATS & MOVEMENT MULTIPLIERS
        // ============================================================================
        #endregion
    }
    // ============================================================================
    // [END] MAIN PLUGIN ENTRY POINT
    // ============================================================================
}
