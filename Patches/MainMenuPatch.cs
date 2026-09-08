using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SailorsCompanion.UI;

namespace SailorsCompanion.Patches
{
    #region [START] PATCH: TITLE SCREEN BUTTON INJECTION
    // ============================================================================
    // [START] PATCH: TITLE SCREEN BUTTON INJECTION
    // Description: Injects "MODS" button into the main title menu and adds version badge.
    // ============================================================================
    [HarmonyPatch(typeof(StartMenuScreen), "Start")]
    public static class StartMenuScreenStartPatch
    {
        public static void Postfix(StartMenuScreen __instance)
        {
            Plugin.EnsureManager();
            InjectModsButton(__instance);
        }

        public static void InjectModsButton(StartMenuScreen startMenu)
        {
            if (startMenu == null) return;
            try
            {
                var trav = Traverse.Create(startMenu);

                var settingsSel = trav.Field<Selectable>("settingsButton")?.Value;
                var menuButtonsGO = trav.Field<GameObject>("menuButtons")?.Value ?? (settingsSel != null ? settingsSel.transform.parent?.gameObject : GameObject.Find("MenuButtons"));

                if (menuButtonsGO == null) return;

                if (menuButtonsGO.transform.Find("Button_SailorsCompanion_Mods") != null)
                {
                    return; // Already injected
                }

                // Use settingsButton as template, or find any button in menuButtons
                GameObject templateGO = settingsSel != null ? settingsSel.gameObject : null;
                if (templateGO == null)
                {
                    var anyBtn = menuButtonsGO.GetComponentInChildren<Button>(true);
                    if (anyBtn != null) templateGO = anyBtn.gameObject;
                }

                if (templateGO != null)
                {
                    var newBtnGO = GameObject.Instantiate(templateGO, menuButtonsGO.transform);
                    newBtnGO.name = "Button_SailorsCompanion_Mods";
                    newBtnGO.transform.SetSiblingIndex(templateGO.transform.GetSiblingIndex() + 1);

                    // Update Text (both TextMeshPro and legacy Text)
                    var tmp = newBtnGO.GetComponentInChildren<TMP_Text>(true);
                    if (tmp != null)
                    {
                        tmp.text = "MODS";
                        tmp.color = new Color(0.0f, 0.95f, 1.0f);
                    }
                    var legacyText = newBtnGO.GetComponentInChildren<Text>(true);
                    if (legacyText != null)
                    {
                        legacyText.text = "MODS";
                        legacyText.color = new Color(0.0f, 0.95f, 1.0f);
                    }

                    var btn = newBtnGO.GetComponent<Button>();
                    if (btn != null)
                    {
                        // Completely wipe all persistent Inspector listeners (which open Settings)
                        btn.onClick = new Button.ButtonClickedEvent();
                        btn.onClick.AddListener(() =>
                        {
                            Debug.Log("[Sailor's Companion] Main Menu MODS button clicked -> Opening Installed Mods Manager!");
                            CanvasInstalledModsUI.Toggle();
                        });
                    }

                    // Remove any other components that might listen to clicks
                    foreach (var comp in newBtnGO.GetComponents<MonoBehaviour>())
                    {
                        if (comp != null && comp != btn && !(comp is TMP_Text) && !(comp is Text) && !(comp is Graphic))
                        {
                            if (comp.GetType().Name.Contains("Setting") || comp is UnityEngine.EventSystems.IPointerClickHandler)
                            {
                                GameObject.Destroy(comp);
                            }
                        }
                    }

                    Debug.Log("[Sailor's Companion] Successfully injected MODS button into StartMenuScreen!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Sailor's Companion] Error injecting StartMenu MODS button: " + ex);
            }
        }
    }

    [HarmonyPatch(typeof(StartMenuScreen), "LateStart")]
    public static class StartMenuScreenLateStartPatch
    {
        public static void Postfix(StartMenuScreen __instance)
        {
            Plugin.EnsureManager();
            StartMenuScreenStartPatch.InjectModsButton(__instance);
        }
    }

    [HarmonyPatch(typeof(StartMenuScreen), "Update")]
    public static class StartMenuScreenUpdatePatch
    {
        private static float _lastCheck = 0f;
        public static void Postfix(StartMenuScreen __instance)
        {
            Plugin.EnsureManager();
            if (Time.unscaledTime - _lastCheck > 0.5f)
            {
                _lastCheck = Time.unscaledTime;
                StartMenuScreenStartPatch.InjectModsButton(__instance);
            }
        }
    }
    #endregion // [END] PATCH: TITLE SCREEN BUTTON INJECTION

    #region [START] PATCH: PAUSE MENU BUTTON INJECTION
    // ============================================================================
    // [START] PATCH: PAUSE MENU BUTTON INJECTION
    // Description: Injects "MOD MENU" button into the in-game ESC Pause Menu.
    // ============================================================================
    [HarmonyPatch(typeof(PauseMenu), "Start")]
    public static class PauseMenuStartPatch
    {
        public static void Postfix(PauseMenu __instance)
        {
            InjectPauseModsButton(__instance);
        }

        public static void InjectPauseModsButton(PauseMenu pauseMenu)
        {
            if (pauseMenu == null) return;
            try
            {
                var holder = pauseMenu.buttonHolderPanel ?? (pauseMenu.transform.Find("ButtonHolder")?.gameObject);
                if (holder == null) return;

                var parent = holder.transform;
                if (parent.Find("Button_SailorsCompanion_PauseMods") != null) return;

                var targetBtn = parent.GetComponentInChildren<Button>(true);
                if (targetBtn == null) return;

                var newBtnGO = GameObject.Instantiate(targetBtn.gameObject, parent);
                newBtnGO.name = "Button_SailorsCompanion_PauseMods";
                newBtnGO.transform.SetSiblingIndex(targetBtn.transform.GetSiblingIndex() + 1);

                // Update text (both TextMeshPro and legacy Text)
                var tmp = newBtnGO.GetComponentInChildren<TMP_Text>(true);
                if (tmp != null)
                {
                    tmp.text = "MOD MENU";
                    tmp.color = new Color(0.0f, 0.95f, 1.0f);
                }
                var txt = newBtnGO.GetComponentInChildren<Text>(true);
                if (txt != null)
                {
                    txt.text = "MOD MENU";
                    txt.color = new Color(0.0f, 0.95f, 1.0f);
                }

                var btn = newBtnGO.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(() =>
                    {
                        Debug.Log("[Sailor's Companion] Pause Menu MOD MENU button clicked -> Opening Installed Mods Manager!");
                        CanvasInstalledModsUI.Toggle();
                    });
                }

                foreach (var comp in newBtnGO.GetComponents<MonoBehaviour>())
                {
                    if (comp != null && comp != btn && !(comp is TMP_Text) && !(comp is Text) && !(comp is Graphic))
                    {
                        if (comp.GetType().Name.Contains("Setting") || comp is UnityEngine.EventSystems.IPointerClickHandler)
                        {
                            GameObject.Destroy(comp);
                        }
                    }
                }

                Debug.Log("[Sailor's Companion] Successfully injected MOD MENU button into PauseMenu!");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Sailor's Companion] Error injecting PauseMenu MOD MENU button: " + ex);
            }
        }
    }

    [HarmonyPatch(typeof(PauseMenu), "Update")]
    public static class PauseMenuUpdatePatch
    {
        private static float _lastPauseCheck = 0f;
        public static void Postfix(PauseMenu __instance)
        {
            if (Time.unscaledTime - _lastPauseCheck > 0.5f)
            {
                _lastPauseCheck = Time.unscaledTime;
                PauseMenuStartPatch.InjectPauseModsButton(__instance);
            }
        }
    }
    // ============================================================================
    // [END] PATCH: PAUSE MENU BUTTON INJECTION
    // ============================================================================
    #endregion
}
