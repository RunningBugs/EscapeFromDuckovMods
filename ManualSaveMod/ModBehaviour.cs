using System;
using System.IO;
using System.Linq;

using Cysharp.Threading.Tasks;
using HarmonyLib;
using Saves;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
                // ignore
            }
        }

        private void OnEnable()
        {
            if (harmony != null) return;
            harmony = new Harmony(HarmonyId);
            harmony.PatchAll(typeof(ModBehaviour).Assembly);
        }

        private void OnDisable()
        {
            if (harmony == null) return;
            harmony.UnpatchAll(harmony.Id);
            harmony = null;
        }

        // ---------------- Manual save helpers ----------------

        private static string GetManualFilePath(int slot)
        {
            var relative = SavesSystem.GetFilePath(slot); // e.g. "Saves/Save_1.sav"
            return Path.Combine(Application.persistentDataPath, relative + ".manual");
        }

        private static bool ManualFileExists(int slot)
        {
            try { return File.Exists(GetManualFilePath(slot)); }
            catch { return false; }
        }

        private static void CreateManualCopyForSlot(int slot)
        {
            try
            {
                string mainPath = Path.Combine(Application.persistentDataPath, SavesSystem.GetFilePath(slot));
                string manualPath = GetManualFilePath(slot);
                if (!File.Exists(mainPath)) return;

                string tmp = manualPath + ".tmp";
                File.Copy(mainPath, tmp, true);
                if (File.Exists(manualPath)) File.Delete(manualPath);
                File.Move(tmp, manualPath);
                Debug.Log($"[ManualSaveMod] Created manual copy for slot {slot}: {manualPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ManualSaveMod] CreateManualCopyForSlot failed: {ex}");
            }
        }

        private static void RemoveManualCopyForSlot(int slot)
        {
            try
            {
                string manualPath = GetManualFilePath(slot);
                if (File.Exists(manualPath)) File.Delete(manualPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ManualSaveMod] RemoveManualCopyForSlot failed: {ex}");
            }
        }

        private static void RestoreManualCopyForSlot(int slot)
        {
            try
            {
                string manualPath = GetManualFilePath(slot);
                if (!File.Exists(manualPath))
                {
                    Debug.LogWarning($"[ManualSaveMod] Manual copy not found for slot {slot}");
                    return;
                }

                string mainPath = Path.Combine(Application.persistentDataPath, SavesSystem.GetFilePath(slot));
                string tmp = mainPath + ".tmp";
                File.Copy(manualPath, tmp, true);
                if (File.Exists(mainPath)) File.Delete(mainPath);
                File.Move(tmp, mainPath);

                // upgrade if necessary
                SavesSystem.UpgradeSaveFileAssemblyInfo(mainPath);

                // notify and continue
                SavesSystem.SetFile(slot);
                GameManager.newBoot = true;
                SceneLoader.Instance.LoadBaseScene().Forget();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ManualSaveMod] RestoreManualCopyForSlot failed: {ex}");
            }
        }

        // ---------------- Patch: add Save button to Pause Menu ----------------
        [HarmonyPatch(typeof(UIPanel), "Open")]
        public static class UIPanel_Open_Patch
        {
            private static GameObject saveButton;
            private static bool isSaving;
            private static string originalButtonText;

            [HarmonyPostfix]
            public static void Postfix(UIPanel __instance)
            {
                if (!(__instance is PauseMenu pauseMenu)) return;
                try { AddSaveButton(pauseMenu); }
                catch (Exception ex) { Debug.LogError($"[ManualSaveMod] Error adding save button: {ex}"); }
            }

            private static void AddSaveButton(UIPanel pauseMenu)
            {
                if (pauseMenu == null) return;

                Transform contentTransform = FindButtonContainer(pauseMenu.transform);
                if (contentTransform == null) return;

                // Remove old
                Transform existing = contentTransform.Find("SaveGameButton");
                if (existing != null) UnityEngine.Object.Destroy(existing.gameObject);
                if (saveButton != null) { UnityEngine.Object.Destroy(saveButton); saveButton = null; }

                Button templateButton = FindTemplateButton(contentTransform);
                if (templateButton == null) return;

                saveButton = UnityEngine.Object.Instantiate(templateButton.gameObject, contentTransform);
                saveButton.name = "SaveGameButton";
                saveButton.transform.SetSiblingIndex(1);

                originalButtonText = GetLocalizedSaveText();

                var textComponents = saveButton.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (textComponents.Length > 0)
                {
                    textComponents[0].text = originalButtonText;
                }

                Button btn = saveButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnSaveButtonClicked(textComponents.Length > 0 ? textComponents[0] : null));
                }
            }

            private static Transform FindButtonContainer(Transform root)
            {
                Transform c = root.Find("Content");
                if (c != null) return c;
                c = root.Find("Buttons");
                if (c != null) return c;
                for (int i = 0; i < root.childCount; i++)
                {
                    Transform child = root.GetChild(i);
                    Button[] buttons = child.GetComponentsInChildren<Button>(true);
                    if (buttons != null && buttons.Length > 0) return child;
                }
                return root;
            }

            private static Button FindTemplateButton(Transform container)
            {
                Button[] buttons = container.GetComponentsInChildren<Button>(true);
                if (buttons == null || buttons.Length == 0) return null;
                foreach (Button b in buttons) if (b.gameObject.name != "SaveGameButton") return b;
                return buttons[0];
            }

            private static string GetLocalizedSaveText()
            {
                return ModLocalization.GetText("save_game", LocalizationManager.CurrentLanguage, "Save Game");
            }

            private static void OnSaveButtonClicked(TextMeshProUGUI buttonText)
            {
                if (isSaving) return;
                SaveGameAsync(buttonText).Forget();
            }

            private static async UniTaskVoid SaveGameAsync(TextMeshProUGUI buttonText)
            {
                isSaving = true;
                try
                {
                    if (buttonText != null) buttonText.text = GetSavingText();
                    if (LevelManager.Instance != null) LevelManager.Instance.SaveMainCharacter();
                    SavesSystem.CollectSaveData();
                    SavesSystem.SaveFile();

                    int attempts = 0;
                    while (SavesSystem.IsSaving && attempts < 100)
                    {
                        await UniTask.Yield();
                        attempts++;
                    }

                    SavesSystem.CreateIndexedBackup();

                    try { CreateManualCopyForSlot(SavesSystem.CurrentSlot); }
                    catch (Exception ex) { Debug.LogError($"[ManualSaveMod] Failed creating manual copy: {ex}"); }

                    if (buttonText != null) buttonText.text = GetSavedText();
                    await UniTask.WaitForSeconds(1.5f, ignoreTimeScale: true);
                    if (buttonText != null) buttonText.text = originalButtonText;
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
                finally { isSaving = false; }
            }

            private static string GetSavingText() => ModLocalization.GetText("saving", LocalizationManager.CurrentLanguage, "Saving...");
            private static string GetSavedText() => ModLocalization.GetText("saved", LocalizationManager.CurrentLanguage, "Saved!");
            private static string GetErrorText() => ModLocalization.GetText("error", LocalizationManager.CurrentLanguage, "Error!");
        }

        // ---------------- Patch: clone Continue button and add Manual Continue ----------------
        [HarmonyPatch(typeof(Duckov.UI.MainMenu.ContinueButton), "Start")]
        public static class ContinueButton_Start_Patch
        {
            private static GameObject manualButton;
            private static GameObject continueRow; // container if created

            private static void RefreshManualButton()
            {
                try
                {
                    if (manualButton == null)
                        return;

                    // Update visibility
                    bool exists = ManualFileExists(SavesSystem.CurrentSlot);
                    manualButton.SetActive(exists);

                    // If visible, ensure the text is updated for current language/slot
                    if (exists)
                    {
                        try
                        {
                            // Reapply localized text to the manual button's TMP child that corresponds to the template
                            var manualTmps = manualButton.GetComponentsInChildren<TextMeshProUGUI>(true);
                            if (manualTmps != null && manualTmps.Length > 0)
                            {
                                // Prefer to find the first non-empty template TMP mapping stored previously;
                                // fall back to first TMP in clone.
                                // We will recompute mapping by taking the first non-empty TMP in the clone if available.
                                int useIndex = 0;
                                for (int i = 0; i < manualTmps.Length; i++)
                                {
                                    if (!string.IsNullOrWhiteSpace(manualTmps[i].text))
                                    {
                                        useIndex = i;
                                        break;
                                    }
                                }
                                manualTmps[Math.Min(useIndex, manualTmps.Length - 1)].text = ModLocalization.GetText("manual_continue", LocalizationManager.CurrentLanguage, "从最新手动档继续");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[ManualSaveMod] Failed updating manual button text on refresh: {ex}");
                        }
                    }
                }
                catch { }
            }

            [HarmonyPostfix]
            public static void Postfix(Duckov.UI.MainMenu.ContinueButton __instance)
            {
                try
                {
                    Button template = __instance.GetComponent<Button>();
                    if (template == null) return;

                    Transform parent = template.transform.parent;
                    if (parent == null) return;

                    // If parent uses a VerticalLayoutGroup, create a horizontal row and move the template into it
                    var parentLayout = parent.GetComponent<LayoutGroup>();
                    if (manualButton == null)
                    {
                        // Create a fresh GameObject for the manual button rather than cloning the entire template GameObject.
                        // This avoids copying over missing or game-specific MonoBehaviours and gives us full control over the visuals.
                        Transform containerForRow = parent;
                        bool createdRow = false;
                        GameObject rowGO = null;

                        if (parentLayout != null && parentLayout is VerticalLayoutGroup)
                        {
                            // Create a horizontal row to host the original and manual continue buttons
                            if (continueRow == null)
                            {
                                rowGO = new GameObject("ContinueRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
                                rowGO.transform.SetParent(parent, false);

                                int insertIndex = template.transform.GetSiblingIndex();
                                rowGO.transform.SetSiblingIndex(insertIndex);

                                var hl = rowGO.GetComponent<HorizontalLayoutGroup>();
                                hl.spacing = 20f;
                                hl.childForceExpandWidth = false;
                                hl.childForceExpandHeight = false;
                                hl.childAlignment = TextAnchor.MiddleCenter;

                                var csf = rowGO.GetComponent<ContentSizeFitter>();
                                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                                csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

                                // move template into row (preserve original)
                                template.transform.SetParent(rowGO.transform, false);
                                continueRow = rowGO;
                            }

                            containerForRow = continueRow.transform;
                            createdRow = true;
                        }

                        // Instantiate a clone of the template to preserve visuals and child hierarchy,
                        // then set the ContinueButton's internal text field (publicized) so the game will use our label.
                        manualButton = UnityEngine.Object.Instantiate(template.gameObject, containerForRow);
                        manualButton.name = "ManualContinueButton";

                        // If parent used a VerticalLayoutGroup we already moved the original template into the row.
                        // Place the clone directly after the template.
                        manualButton.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);

                        try
                        {
                            // If the ContinueButton type has been publicized, set its private text_Continue field via direct access.
                            // This avoids reflection and uses the publicized member created by Krafs.Publicizer.
                            try
                            {
                                var contComp = manualButton.GetComponent<Duckov.UI.MainMenu.ContinueButton>();
                                if (contComp != null)
                                {
                                    // Set the internal label field so the component will write it into the TMP child on refresh.
                                    contComp.text_Continue = ModLocalization.GetText("manual_continue", LocalizationManager.CurrentLanguage, "从最新手动档继续");
                                }
                            }
                            catch
                            {
                                // If publicized access is not available for some reason, fall back to setting TMP child directly.
                                var manualTmpsFallback = manualButton.GetComponentsInChildren<TextMeshProUGUI>(true);
                                if (manualTmpsFallback != null && manualTmpsFallback.Length > 0)
                                {
                                    manualTmpsFallback[0].text = ModLocalization.GetText("manual_continue", LocalizationManager.CurrentLanguage, "从最新手动档继续");
                                }
                            }

                            // Ensure our button uses our handler (remove existing listeners to avoid duplicate behavior)
                            var btnComp = manualButton.GetComponent<Button>();
                            if (btnComp != null)
                            {
                                btnComp.onClick.RemoveAllListeners();
                                btnComp.onClick.AddListener(() => RestoreManualCopyForSlot(SavesSystem.CurrentSlot));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[ManualSaveMod] Error configuring manual clone: {ex}");
                        }

                        // Subscribe refresh handlers and localization update
                        SavesSystem.OnSetFile -= RefreshManualButton;
                        SavesSystem.OnSaveDeleted -= RefreshManualButton;
                        SavesSystem.OnSetFile += RefreshManualButton;
                        SavesSystem.OnSaveDeleted += RefreshManualButton;

                        // Hook language change so label is re-applied if language changes later
                        LocalizationManager.OnSetLanguage -= OnManualLanguageChanged;
                        LocalizationManager.OnSetLanguage += OnManualLanguageChanged;

                        RefreshManualButton();
                    }

                    // Local language-change handler and reapply helper
                    static void OnManualLanguageChanged(SystemLanguage lang)
                    {
                        ReapplyManualTextAsync().Forget();
                    }

                    static async UniTaskVoid ReapplyManualTextAsync()
                    {
                        // give UI a couple frames to settle, then refresh text
                        await UniTask.DelayFrame(2);
                        RefreshManualButton();
                    }

                    // Subscribe refresh handlers
                    SavesSystem.OnSetFile -= RefreshManualButton;
                    SavesSystem.OnSaveDeleted -= RefreshManualButton;
                    SavesSystem.OnSetFile += RefreshManualButton;
                    SavesSystem.OnSaveDeleted += RefreshManualButton;

                    RefreshManualButton();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ManualSaveMod] ContinueButton_Start_Patch error: {ex}");
                }
            }


        }
    }
}
