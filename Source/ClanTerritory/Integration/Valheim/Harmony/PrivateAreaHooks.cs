using HarmonyLib;
using ClanTerritory.Core;
using ClanTerritory.Features.Territory.WorldDiscovery.Scanners;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Features.WardDetection.Services;
using ClanTerritory.Features.WardInteraction.Services;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch(typeof(PrivateArea))]
    internal static class PrivateAreaHooks
    {
        private static readonly PrivateAreaScanner Scanner =
            new PrivateAreaScanner();

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void AwakePostfix(PrivateArea __instance)
        {
            if (!Scanner.TryCreateWardModel(
                    __instance,
                    out WardModel model))
                return;

            IWardService wardService;

            if (ServiceContainer.TryGet<IWardService>(out wardService))
                wardService.RegisterWard(model);
        }

        [HarmonyPrefix]
        [HarmonyPatch("Interact")]
        private static bool InteractPrefix(
            PrivateArea __instance,
            Humanoid human,
            bool hold,
            bool alt,
            ref bool __result)
        {
            if (hold)
            {
                __result = false;
                return false;
            }

            Player player = human as Player;

            if (player == null)
                return true;

            IWardInteractionService wardInteractionService;

            if (!ServiceContainer.TryGet<IWardInteractionService>(out wardInteractionService))
                return true;

            if (!wardInteractionService.TryOpenWardMenu(__instance, player))
                return true;

            __result = true;
            return false;
        }
    }
}