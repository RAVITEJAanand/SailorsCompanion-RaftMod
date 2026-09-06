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

            // Bind Navigation Settings
            EnableHUD = Config.Bind("Features.Navigation", "EnableHUD", true, "Show the real-time compass, coordinates, raft tracker, and shark radar.");
            HUDStyle = Config.Bind("Features.Navigation", "HUDStyle", 0, "HUD Style: 0=Sleek Ribbon, 1=Top Compass Bar, 2=Minimalist Pill, 3=Compact Box.");

            // Bind Survival & World Settings
            InfiniteDurability = Config.Bind("Features.Survival", "InfiniteDurability", true, "Tools, weapons, hooks, and armor never lose durability.");
            AntiSharkRaftDamage = Config.Bind("Features.World", "AntiSharkRaftDamage", true, "Stops the shark from attacking or damaging raft blocks.");
            InfiniteOxygen = Config.Bind("Features.Survival", "InfiniteOxygen", true, "Allows diving freely without running out of oxygen.");
            GodMode = Config.Bind("Features.Survival", "GodMode", false, "Invulnerable to all damage.");
            NoHungerThirst = Config.Bind("Features.Survival", "NoHungerThirst", false, "Freeze hunger and thirst meters at maximum.");
            CustomStackSize = Config.Bind("Features.Inventory", "CustomStackSize", 99, "Maximum stack size for stackable resources.");

            // Bind Movement & Speeds
            SwimSpeedMultiplier = Config.Bind("Features.Movement", "SwimSpeedMultiplier", 1.8f, "Multiplier for player swimming speed.");
            SprintSpeedMultiplier = Config.Bind("Features.Movement", "SprintSpeedMultiplier", 1.4f, "Multiplier for player sprinting speed.");
            HookPullSpeedMultiplier = Config.Bind("Features.World", "HookPullSpeedMultiplier", 2.2f, "Multiplier for hook debris reeling speed.");
            FreeCrafting = Config.Bind("Features.World", "FreeCrafting", false, "Craft any item without consuming materials.");
            EnableFlyMode = Config.Bind("Features.Movement", "EnableFlyMode", false, "Fly / Noclip mode.");
            FlySpeed = Config.Bind("Features.Movement", "FlySpeed", 14f, "Flight speed in m/s.");

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
            Type[] patchClasses = new[]
            {
                typeof(Patches.SlotDurabilityPatch),
                typeof(Patches.ArmorDurabilityPatch),
                typeof(Patches.SharkFindBlockPatch),
                typeof(Patches.SharkDealDamagePatch),
                typeof(Patches.SharkInitiateAttackPatch),
                typeof(Patches.StackSizePatch),
                typeof(Patches.CraftingHasEnoughPatch),
                typeof(Patches.CraftingRemoveCostPatch),
                typeof(Patches.CraftingRemoveCostSecPatch),
                typeof(Patches.PlayerStatsDamagePatch),
                typeof(Patches.HookStartPatch),
                typeof(Patches.StartMenuScreenStartPatch),
                typeof(Patches.StartMenuScreenLateStartPatch),
                typeof(Patches.StartMenuScreenUpdatePatch),
                typeof(Patches.PauseMenuStartPatch),
                typeof(Patches.PauseMenuUpdatePatch),
                typeof(Patches.GameManagerUpdatePatch),
                typeof(Patches.HelperSetCursorVisibleAndLockStatePatch),
                typeof(Patches.HelperSetCursorLockStatePatch),
                typeof(Patches.HelperSetCursorVisiblePatch),
            };

            foreach (var patchClass in patchClasses)
            {
                try
                {
                    _harmony.CreateClassProcessor(patchClass).Patch();
                    Logger.LogInfo($"[{PluginInfo.PLUGIN_NAME}] Applied patch: {patchClass.Name}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[{PluginInfo.PLUGIN_NAME}] Failed patch {patchClass.Name}: {ex.Message}");
                }
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
                ManagerGO.AddComponent<ModGUI>();
                ManagerGO.AddComponent<HUDOverlay>();
                ManagerGO.AddComponent<FlyController>();
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

            // Apply multipliers
            if (_baseSwimSpeed > 0f)
            {
                pc.swimSpeed = _baseSwimSpeed * SwimSpeedMultiplier.Value;
            }
            if (_baseSprintSpeed > 0f)
            {
                pc.sprintSpeed = _baseSprintSpeed * SprintSpeedMultiplier.Value;
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
