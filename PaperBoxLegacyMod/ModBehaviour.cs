using Duckov.Buffs;
using Duckov.Modding;
using HarmonyLib;
using UnityEngine;

namespace PaperBoxLegacyMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private Harmony harmony;

        private const int PaperBoxBuffId = 1202;
        private const int PaperBoxMeleeBuffId = 1204;

        private void Awake()
        {
            Debug.Log("[PaperBoxLegacyMod] Loaded");
        }

        private void OnEnable()
        {
            harmony = new Harmony("PaperBoxLegacyMod");
            harmony.PatchAll(typeof(ModBehaviour).Assembly);
            Debug.Log("[PaperBoxLegacyMod] Harmony patches applied");
        }

        private void OnDisable()
        {
            if (harmony != null)
            {
                harmony.UnpatchAll(harmony.Id);
                harmony = null;
                Debug.Log("[PaperBoxLegacyMod] Harmony patches removed");
            }
        }

        [HarmonyPatch(typeof(ItemStatsSystem.OnShootAttackTrigger), "OnShootAttack")]
        private static class OnShootAttackTrigger_Trigger_Patch
        {
            private static bool Prefix(ItemStatsSystem.OnShootAttackTrigger __instance, DuckovItemAgent agent)
            {
                // Check if this is a paper box buff (1202 or 1204)
                Buff buff = __instance.GetComponentInParent<Buff>();
                if (buff == null)
                {
                    return true;
                }

                if (buff.ID == PaperBoxMeleeBuffId)
                {
                    Debug.Log($"[PaperBoxLegacyMod] Skipping trigger for PaperBoxMeleeBuffId ({PaperBoxMeleeBuffId})");
                    return false;
                }

                if (buff.ID != PaperBoxBuffId)
                {
                    return true;
                }

                // Attack events come from melee weapon agents; allow ranged shoot events to keep legacy behaviour.
                bool isMeleeAttack = agent is ItemAgent_MeleeWeapon;
                if (isMeleeAttack)
                {
                    Debug.Log($"[PaperBoxLegacyMod] Blocking melee attack trigger for PaperBoxBuffId ({PaperBoxBuffId})");
                    return false;
                }

                return true; // Allow shoot triggers for PaperBox legacy behaviour
            }
        }

        // [HarmonyPatch(typeof(RemoveBuffAction), "OnTriggered")]
        // private static class RemoveBuffAction_OnTriggered_Patch
        // {
        //     private static void Prefix(RemoveBuffAction __instance)
        //     {
        //         if (__instance.buffID == PaperBoxBuffId)
        //         {
        //             string context = __instance.transform?.parent?.name ?? __instance.gameObject.name;
        //             Debug.Log($"[PaperBoxLegacyMod] RemoveBuffAction.OnTriggered for PaperBox (context='{context}', removeOneLayer={__instance.removeOneLayer})");
        //         }
        //     }
        // }
    }
}
