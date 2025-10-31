using HarmonyLib;
using UnityEngine;

namespace ReloadingFromPetBackpackMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string HarmonyId = "ReloadingFromPetBackpackMod";
        private static Harmony harmonyInstance;

        private void Awake()
        {
            Debug.Log("[ReloadingFromPetBackpackMod] Loaded");
        }

        private void OnEnable()
        {
            if (harmonyInstance != null)
            {
                Debug.Log("[ReloadingFromPetBackpackMod] Harmony already patched, skipping");
                return;
            }

            Debug.Log("[ReloadingFromPetBackpackMod] Starting to apply Harmony patches...");
            harmonyInstance = new Harmony(HarmonyId);

            try
            {
                harmonyInstance.PatchAll(typeof(ModBehaviour).Assembly);
                Debug.Log("[ReloadingFromPetBackpackMod] Harmony patches applied successfully");

                // Verify patches were applied
                var patches = harmonyInstance.GetPatchedMethods();
                int count = 0;
                foreach (var method in patches)
                {
                    count++;
                    Debug.Log($"[ReloadingFromPetBackpackMod] Patched: {method.DeclaringType?.Name}.{method.Name}");
                }
                Debug.Log($"[ReloadingFromPetBackpackMod] Total methods patched: {count}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ReloadingFromPetBackpackMod] Failed to apply patches: {ex.Message}");
                Debug.LogError($"[ReloadingFromPetBackpackMod] Stack trace: {ex.StackTrace}");
            }
        }

        private void OnDisable()
        {
            if (harmonyInstance == null)
            {
                return;
            }

            harmonyInstance.UnpatchAll(harmonyInstance.Id);
            harmonyInstance = null;
            Debug.Log("[ReloadingFromPetBackpackMod] Harmony patches removed");
        }
    }
}
