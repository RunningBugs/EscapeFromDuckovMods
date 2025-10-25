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
                return;
            }

            harmonyInstance = new Harmony(HarmonyId);
            harmonyInstance.PatchAll(typeof(ModBehaviour).Assembly);
            Debug.Log("[ReloadingFromPetBackpackMod] Harmony patches applied");
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
