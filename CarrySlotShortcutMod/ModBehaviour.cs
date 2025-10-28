using Duckov;
using HarmonyLib;
using Saves;
using UnityEngine;

namespace CarrySlotShortcutMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string HarmonyId = "CarrySlotShortcutMod";

        private Harmony harmony;
        private bool waitingForInventoryEvent;

        private void Awake()
        {
            Debug.Log("[CarrySlotShortcutMod] Loaded");
        }

        private void OnEnable()
        {
            if (harmony != null)
            {
                return;
            }

            harmony = new Harmony(HarmonyId);
            harmony.PatchAll(typeof(ModBehaviour).Assembly);

            SavesSystem.OnSetFile += OnSetSaveFile;
            LevelManager.OnLevelInitialized += OnLevelInitialized;
            CharacterMainControl.OnMainCharacterInventoryChangedEvent += OnMainCharacterInventoryChanged;

            waitingForInventoryEvent = false;

            Debug.Log("[CarrySlotShortcutMod] Harmony patches applied");
        }

        private void OnDisable()
        {
            SavesSystem.OnSetFile -= OnSetSaveFile;
            LevelManager.OnLevelInitialized -= OnLevelInitialized;
            CharacterMainControl.OnMainCharacterInventoryChangedEvent -= OnMainCharacterInventoryChanged;

            waitingForInventoryEvent = false;

            if (harmony == null)
            {
                return;
            }

            harmony.UnpatchAll(harmony.Id);
            harmony = null;

            Debug.Log("[CarrySlotShortcutMod] Harmony patches removed");
        }

        private void OnSetSaveFile()
        {
            waitingForInventoryEvent = false;
            ShortcutPersistence.ClearAll();
            Debug.Log("[CarrySlotShortcutMod][AutoRefresh] Save file switched; cleared cached bindings.");
        }

        private void OnLevelInitialized()
        {
            waitingForInventoryEvent = true;
            Debug.Log("[CarrySlotShortcutMod][AutoRefresh] Level initialized; attempting immediate shortcut refresh.");
            if (TryRefreshShortcuts())
            {
                waitingForInventoryEvent = false;
                Debug.Log("[CarrySlotShortcutMod][AutoRefresh] Immediate refresh succeeded.");
            }
            else
            {
                Debug.Log("[CarrySlotShortcutMod][AutoRefresh] Immediate refresh deferred until inventory change.");
            }
        }

        private void OnMainCharacterInventoryChanged(CharacterMainControl control, ItemStatsSystem.Inventory inventory, int index)
        {
            if (!waitingForInventoryEvent)
            {
                return;
            }

            CharacterMainControl main = CharacterMainControl.Main;
            if (main == null || control != main)
            {
                Debug.Log("[CarrySlotShortcutMod][AutoRefresh] Inventory change ignored (not main character or main missing).");
                return;
            }

            Debug.Log($"[CarrySlotShortcutMod][AutoRefresh] Inventory change detected (index {index}); retrying shortcut refresh.");
            if (TryRefreshShortcuts())
            {
                waitingForInventoryEvent = false;
                Debug.Log("[CarrySlotShortcutMod][AutoRefresh] Deferred refresh succeeded after inventory change.");
            }
            else
            {
                Debug.Log("[CarrySlotShortcutMod][AutoRefresh] Deferred refresh still pending.");
            }
        }

        private static bool TryRefreshShortcuts()
        {
            ItemShortcut shortcut = ItemShortcut.Instance;
            if (shortcut == null)
            {
                Debug.Log("[CarrySlotShortcutMod][AutoRefresh] Shortcut instance unavailable; refresh aborted.");
                return false;
            }

            Debug.Log("[CarrySlotShortcutMod][AutoRefresh] RefreshAll invoked.");
            ShortcutPersistence.RefreshAll(shortcut);
            return true;
        }
    }
}
