using System;
using System.IO;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Saves;
using UnityEngine;
using UnityEngine.UI;

namespace ManualSaveMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string HarmonyId = "ManualSaveMod";

        private Harmony harmony;

        private void Awake()
        {
            InitializeLocalization();
        }

        private void InitializeLocalization()
        {
            try
            {
                var assemblyLocation = typeof(ModBehaviour).Assembly.Location;
                var modFolder = Path.GetDirectoryName(assemblyLocation);
                ModLocalization.Initialize(modFolder);
            }
            catch
            {
                // Localization initialization failed, will use fallback text
            }
        }

        private void OnEnable()
        {
            if (harmony != null)
            {
                return;
            }

            harmony = new Harmony(HarmonyId);
            harmony.PatchAll(typeof(ModBehaviour).Assembly);
        }

        private void OnDisable()
        {
            if (harmony == null)
            {
                return;
            }

            harmony.UnpatchAll(harmony.Id);
            harmony = null;
        }

        [HarmonyPatch(typeof(UIPanel), "Open")]
        public static class UIPanel_Open_Patch
        {
            private static GameObject saveButton;
            private static bool isSaving;
            private static string originalButtonText;

            [HarmonyPostfix]
            public static void Postfix(UIPanel __instance)
            {
                // Only process if this is a PauseMenu
                if (!(__instance is PauseMenu pauseMenu))
                {
                    return;
                }

                try
                {
                    if (pauseMenu == null)
                    {
                        return;
                    }

                    AddSaveButton(pauseMenu);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ManualSaveMod] Error adding save button: {ex}");
                }
            }

            private static void AddSaveButton(UIPanel pauseMenu)
            {
                if (pauseMenu == null)
                {
                    return;
                }

                // Find the button container (usually the first child with buttons)
                Transform contentTransform = FindButtonContainer(pauseMenu.transform);
                if (contentTransform == null)
                {
                    return;
                }

                // Check if save button already exists and clean it up
                Transform existingSaveButton = contentTransform.Find("SaveGameButton");
                if (existingSaveButton != null)
                {
                    UnityEngine.Object.Destroy(existingSaveButton.gameObject);
                }

                // Clean up old static reference if it exists
                if (saveButton != null)
                {
                    UnityEngine.Object.Destroy(saveButton);
                    saveButton = null;
                }

                // Find the first button to use as a template (skip our own button)
                Button templateButton = FindTemplateButton(contentTransform);
                if (templateButton == null)
                {
                    return;
                }

                // Clone the template button
                saveButton = UnityEngine.Object.Instantiate(templateButton.gameObject, contentTransform);
                saveButton.name = "SaveGameButton";

                // Position it right after the first button (after Resume/回到游戏)
                // This puts it in the button list, not above the title
                saveButton.transform.SetSiblingIndex(1);

                // Store the original localized text
                originalButtonText = GetLocalizedSaveText();

                // Update the button text - get fresh reference after instantiation
                var textComponents = saveButton.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                TMPro.TextMeshProUGUI buttonTextComponent = null;
                if (textComponents.Length > 0)
                {
                    buttonTextComponent = textComponents[0];
                    buttonTextComponent.text = originalButtonText;
                }

                // Set up the button click handler with captured text component
                Button button = saveButton.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnSaveButtonClicked(buttonTextComponent));
                }
            }

            private static Transform FindButtonContainer(Transform pauseMenuTransform)
            {
                // Try to find a common container name
                Transform container = pauseMenuTransform.Find("Content");
                if (container != null)
                {
                    return container;
                }

                container = pauseMenuTransform.Find("Buttons");
                if (container != null)
                {
                    return container;
                }

                // Look for any transform with buttons
                for (int i = 0; i < pauseMenuTransform.childCount; i++)
                {
                    Transform child = pauseMenuTransform.GetChild(i);
                    Button[] buttons = child.GetComponentsInChildren<Button>(true);
                    if (buttons != null && buttons.Length > 0)
                    {
                        return child;
                    }
                }

                return pauseMenuTransform;
            }

            private static Button FindTemplateButton(Transform container)
            {
                // Look for any button in the container
                Button[] buttons = container.GetComponentsInChildren<Button>(true);

                if (buttons == null || buttons.Length == 0)
                {
                    return null;
                }

                // Find first button that is NOT our save button
                foreach (Button button in buttons)
                {
                    if (button.gameObject.name != "SaveGameButton")
                    {
                        return button;
                    }
                }

                return buttons[0];
            }

            private static string GetLocalizedSaveText()
            {
                return ModLocalization.GetText("save_game", Application.systemLanguage, "Save Game");
            }

            private static void OnSaveButtonClicked(TMPro.TextMeshProUGUI buttonText)
            {
                if (isSaving)
                {
                    return;
                }

                SaveGameAsync(buttonText).Forget();
            }

            private static async UniTaskVoid SaveGameAsync(TMPro.TextMeshProUGUI buttonText)
            {
                isSaving = true;

                try
                {
                    if (buttonText != null)
                    {
                        buttonText.text = GetSavingText();
                    }

                    // Execute the save sequence (same as vanilla autosave)
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.SaveMainCharacter();
                    }
                    SavesSystem.CollectSaveData();
                    SavesSystem.SaveFile();

                    // Wait for the save to complete
                    int attempts = 0;
                    while (SavesSystem.IsSaving && attempts < 100)
                    {
                        await UniTask.Yield();
                        attempts++;
                    }

                    // Always create indexed backup for manual saves (bypass 5-minute restriction)
                    SavesSystem.CreateIndexedBackup();

                    // Show "Saved!" feedback
                    if (buttonText != null)
                    {
                        buttonText.text = GetSavedText();
                    }

                    await UniTask.WaitForSeconds(1.5f, ignoreTimeScale: true);

                    // Restore original text
                    if (buttonText != null)
                    {
                        buttonText.text = originalButtonText;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ManualSaveMod] Error during save: {ex}");

                    if (buttonText != null)
                    {
                        buttonText.text = GetErrorText();
                        await UniTask.WaitForSeconds(2f, ignoreTimeScale: true);
                        buttonText.text = originalButtonText;
                    }
                }
                finally
                {
                    isSaving = false;
                }
            }

            private static string GetSavingText()
            {
                return ModLocalization.GetText("saving", Application.systemLanguage, "Saving...");
            }

            private static string GetSavedText()
            {
                return ModLocalization.GetText("saved", Application.systemLanguage, "Saved!");
            }

            private static string GetErrorText()
            {
                return ModLocalization.GetText("error", Application.systemLanguage, "Error!");
            }
        }
    }
}
