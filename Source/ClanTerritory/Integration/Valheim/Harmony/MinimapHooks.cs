using ClanTerritory.Core;
using ClanTerritory.Features.Map.Services;
using HarmonyLib;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch(typeof(Minimap))]
    internal static class MinimapHooks
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void StartPostfix()
        {
            WardMapIconService mapIconService;

            if (ServiceContainer.TryGet<WardMapIconService>(out mapIconService))
                mapIconService.SyncAllFromZdo();
        }
    }
}