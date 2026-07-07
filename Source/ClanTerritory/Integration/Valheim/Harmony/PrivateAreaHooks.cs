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

        [HarmonyPostfix]
        [HarmonyPatch("Interact")]
        private static void InteractPostfix(
            PrivateArea __instance,
            Humanoid character,
            bool hold,
            bool alt,
            bool __result)
        {
            if (!__result)
                return;

            if (hold)
                return;

            Player player = character as Player;

            if (player == null)
                return;

            IWardInteractionService wardInteractionService;

            if (ServiceContainer.TryGet<IWardInteractionService>(out wardInteractionService))
                wardInteractionService.TryOpenWardMenu(__instance, player);
        }
    }
}