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
            SyncWardPins();
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetMapData")]
        private static void SetMapDataPostfix()
        {
            SyncWardPins();
        }

        private static void SyncWardPins()
        {
            WardMapIconService mapIconService;

            if (ServiceContainer.TryGet<WardMapIconService>(out mapIconService))
                mapIconService.SyncAllFromZdo();
        }
    }
}