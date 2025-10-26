using HarmonyLib;
using UnityEngine;

namespace CarrySlotShortcutMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string HarmonyId = "CarrySlotShortcutMod";

        private Harmony harmony;

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
            Debug.Log("[CarrySlotShortcutMod] Harmony patches applied");
        }

        private void OnDisable()
        {
            if (harmony == null)
            {
                return;
            }
            harmony.UnpatchAll(harmony.Id);
            harmony = null;
            Debug.Log("[CarrySlotShortcutMod] Harmony patches removed");
        }
    }
}
