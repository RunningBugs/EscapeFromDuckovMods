# ManualSaveMod — Specification

Document version: 2025-11-03  
Author: ManualSaveMod design (assistant)

## Goals (short)
- Provide a per-slot "manual continue" UX that restores a dedicated manual save copy.
- Avoid relying on game's auto-saves or indexed backups.
- Keep changes minimal and safe: per-slot manual file naming, atomic operations, and Harmony-based UI patching.

## Confirmed requirements (from user)
1. Label for the new button: `从最新手动档继续`
2. The button is shown only for the currently selected save slot (SavesSystem.CurrentSlot).
3. Placement: clone the vanilla Continue button and place the clone horizontally to the right of the original; compute template width and use margin fallback = 160 px.
4. When the user performs a manual save, create/replace a dedicated manual copy for that slot.
5. When the user clears a slot or deletes saves, also remove the manual copy for that slot.
6. No global pointer: presence of per-slot manual file indicates availability; no timestamp or global key needed.

---

## File / naming conventions
- Main game save for slot N:
  - Path: `Saves/Save_<N>.sav` (canonical via `SavesSystem.GetFilePath(N)`)
- Manual save copy (per-slot):
  - Path: `<MainSavePath>.manual` (example: `Saves/Save_1.sav.manual`)
  - That is, append `.manual` to the canonical save filename.
- Backup/temporary copy used during atomic writes:
  - `<manualPath>.tmp`

---

## High-level flow

### Manual save creation (when user clicks mod's Save button)
1. Existing mod save flow runs and completes (SavesSystem.SaveFile / CreateIndexedBackup).
2. After the save finishes (SavesSystem.IsSaving false), perform:
   - Compute `mainPath = SavesSystem.GetFilePath(slot)` (slot is current slot at save time).
   - `manualPath = mainPath + ".manual"`.
   - Copy `mainPath` -> `manualPath.tmp` (overwrite allowed).
   - Move `manualPath.tmp` -> `manualPath` (atomic replace).
3. No global registry key is needed; existence of `manualPath` is the indicator.

### Manual continue restore (when user clicks new button)
1. Button click handler will:
   - Determine `slot = SavesSystem.CurrentSlot`.
   - Compute `manualPath = GetManualFilePath(slot)`.
   - If `File.Exists(manualPath)` is false, fail gracefully (hide button / show message).
   - Copy `manualPath` -> `mainPath` (atomic: temp -> move).
   - Call `SavesSystem.UpgradeSaveFileAssemblyInfo(mainPath)` as the other restore flows do.
   - Call `SavesSystem.SetFile(slot)` so the system recognizes the file as current (this triggers `SavesSystem.OnSetFile`).
   - Set `GameManager.newBoot = true`.
   - Trigger the same scene loading flow as `ContinueButton` (e.g., `SceneLoader.Instance.LoadBaseScene()` or existing Continue routine).

### Cleanup on slot delete / clear
- Subscribe to `SavesSystem.OnSaveDeleted` or appropriate slot-clear hooks.
- When a slot's main file is deleted or user clears a slot, delete the corresponding `manualPath` (if exists).
- If user removes the manual copy file manually, the UI will hide the button next time the menu refreshes.

---

## UI: Find / Clone / Place behavior

Patch point:
- Add a Harmony postfix patch on `Duckov.UI.MainMenu.ContinueButton` initialization method (e.g., `Start`, `Awake` or `Refresh`) so we run when the menu sets up buttons.

Cloning logic:
1. Locate the Continue button GameObject instance provided by `ContinueButton`.
2. If `ManualContinueButton` already exists, reuse it.
3. If not, instantiate clone:
   - `clone = Object.Instantiate(continueButton.gameObject, parentTransform);`
   - `clone.name = "ManualContinueButton";`
   - Set `clone.transform.SetSiblingIndex(originalSiblingIndex + 1);`
4. Position:
   - Read `RectTransform templateRT = continueButton.GetComponent<RectTransform>()`.
   - Try `float width = templateRT.rect.width;` (fallback: `templateRT.sizeDelta.x`).
   - Compute offsetX = width + 160 (margin).
   - Get `Vector2 anchored = cloneRT.anchoredPosition; anchored.x = templateRT.anchoredPosition.x + offsetX; cloneRT.anchoredPosition = anchored;`
   - If horizontal layout is enforced by a LayoutGroup that ignores anchoredPosition, keep sibling index and accept Flow-based layout; the clone will likely stack vertically — fallback is to create a small horizontal container (see note).
5. Text:
   - Find `TextMeshProUGUI` child in clone and set `.text = "从最新手动档继续"`.
   - Use `ModLocalization.GetText("manual_continue", LocalizationManager.CurrentLanguage, "从最新手动档继续")` for localization.
6. Visibility:
   - On patch run and on `SavesSystem.OnSetFile` / `SavesSystem.OnSaveDeleted`, check `File.Exists(GetManualFilePath(SavesSystem.CurrentSlot))`.
   - Set `clone.SetActive(true/false)` accordingly.

Note about layout groups:
- If parent uses a VerticalLayoutGroup and repositions children automatically, setting anchoredPosition may be ignored. If that happens and you require horizontal layout, we can:
  - Create a new parent `GameObject HorizontalGroup` with `HorizontalLayoutGroup` in place of current parent for the Continue button and insert both children there. This is more invasive. For now: try clone & anchored offset; use sibling index + offset fallback.

---

## Concurrency and atomicity
- Always write to `<manualPath>.tmp` then `File.Move(tmp, manualPath)` to minimize partial-file risk.
- Use try/catch around file IO and log errors via `Debug.LogError`.
- Ensure `SavesSystem.IsSaving` is false before copying.

---

## Events & subscriptions
- Subscribe to:
  - `SavesSystem.OnSetFile` — triggered when switching slots or after restoring; use to refresh button visibility.
  - `SavesSystem.OnSaveDeleted` — clean up manual file if relevant.
- On patch teardown/unpatch, unsubscribe.

---

## Helper methods (API within mod)
- `string GetManualFilePath(int slot)`  
  Returns `SavesSystem.GetFilePath(slot) + ".manual"`.
- `bool ManualFileExists(int slot)`  
  Returns `File.Exists(GetManualFilePath(slot))`.
- `void CreateManualCopyForSlot(int slot)`  
  Performs atomic copy from main save to manual file.
- `void RemoveManualCopyForSlot(int slot)`  
  Deletes manual file if exists.
- `void RestoreManualCopyForSlot(int slot)`  
  Restores manual into main and triggers load flow.

---

## Implementation notes / where to edit
- File to modify: `ManualSaveMod/ModBehaviour.cs`
  - Add helper methods above (private static).
  - In existing `SaveGameAsync`, after saves complete and `CreateIndexedBackup()` call, call `CreateManualCopyForSlot(SavesSystem.CurrentSlot)`.
  - Add a new Harmony patch on `Duckov.UI.MainMenu.ContinueButton` initialization to create/manage the cloned manual continue button.
  - Add subscription to `SavesSystem.OnSaveDeleted` and `SavesSystem.OnSetFile` to update visibility and cleanup.

---

## Testing checklist
1. Build mod and run game.
2. Create a manual save using mod's Save button on slot 1:
   - Verify `Saves/Save_1.sav.manual` file exists in persistent path.
3. Open Main Menu:
   - Confirm `从最新手动档继续` button appears next to vanilla Continue button (horizontally).
4. Click `从最新手动档继续`:
   - Confirm it restores manual file to `Saves/Save_1.sav` and loads base scene (same effect as Continue).
5. Delete/clear slot 1:
   - Confirm `Saves/Save_1.sav.manual` is deleted and button disappears on menu.
6. Repeat for other slots to verify per-slot isolation.

---

## Edge cases and future improvements
- If layout is inconsistent, consider creating a small horizontal container and placing both Continue buttons inside (more invasive).
- Optionally add a small tooltip or confirmation dialog before restoring (if desirable).
- Consider a UI to manage manual copies (list, delete) in future revisions.
- Consider marking manual copy creation with metadata in ES3 if you later want timestamps or multi-manual versions.

---

If this spec looks correct, reply "Implement" and I will apply the changes to `ManualSaveMod/ModBehaviour.cs` and create the helper methods + Harmony patches as specified. If you want any wording change (button label) or different manual filename suffix, say so now.