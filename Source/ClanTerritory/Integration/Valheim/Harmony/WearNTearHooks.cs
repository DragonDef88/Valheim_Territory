using ClanTerritory.Core;
using ClanTerritory.Features.Territory.Services;
using HarmonyLib;
using UnityEngine;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch(typeof(WearNTear))]
    internal static class WearNTearHooks
    {
        [HarmonyPrefix]
        [HarmonyPatch("RPC_Damage")]
        private static bool RPCDamagePrefix(
            WearNTear __instance,
            long sender,
            HitData hit)
        {
            if (__instance == null)
                return true;

            if (__instance.GetComponent<Piece>() == null)
                return true;

            TerritoryRuleService ruleService;

            if (!ServiceContainer.TryGet<TerritoryRuleService>(out ruleService))
                return true;

            Vector3 feedbackPosition = __instance.transform.position;

            if (hit != null)
                feedbackPosition = hit.m_point;

            if (!ruleService.TryBlockStructureDamage(
                    __instance.transform.position,
                    feedbackPosition))
            {
                return true;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("ApplyDamage")]
        private static bool ApplyDamagePrefix(
            WearNTear __instance,
            ref bool __result)
        {
            if (__instance == null)
                return true;

            if (__instance.GetComponent<Piece>() == null)
                return true;

            TerritoryRuleService ruleService;

            if (!ServiceContainer.TryGet<TerritoryRuleService>(out ruleService))
                return true;

            if (!ruleService.TryBlockStructureDamage(__instance.transform.position))
                return true;

            __result = false;
            return false;
        }
    }
}
