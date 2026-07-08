using ClanTerritory.Core;
using ClanTerritory.Features.Territory.Services;
using HarmonyLib;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch(typeof(WearNTear))]
    internal static class WearNTearHooks
    {
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

            if (!ruleService.IsStructureDamageProtected(__instance.transform.position))
                return true;

            __result = false;
            return false;
        }
    }
}
