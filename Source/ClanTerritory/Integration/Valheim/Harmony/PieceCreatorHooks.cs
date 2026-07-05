using HarmonyLib;
using ClanTerritory.Core;
using ClanTerritory.Features.Territory.WorldDiscovery.Scanners;
using ClanTerritory.Features.WardDetection.Models;
using ClanTerritory.Features.WardDetection.Services;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch(typeof(Piece))]
    internal static class PieceCreatorHooks
    {
        private const string WardPieceName = "guard_stone";

        private static readonly PrivateAreaScanner Scanner =
            new PrivateAreaScanner();

        [HarmonyPostfix]
        [HarmonyPatch("SetCreator")]
        private static void SetCreatorPostfix(Piece __instance)
        {
            if (__instance == null)
                return;

            if (!__instance.name.Contains(WardPieceName))
                return;

            PrivateArea privateArea =
                __instance.GetComponent<PrivateArea>();

            if (privateArea == null)
                return;

            if (!Scanner.TryCreateWardModel(
                    privateArea,
                    out WardModel model))
                return;

            IWardService wardService;

            if (ServiceContainer.TryGet<IWardService>(out wardService))
                wardService.RegisterWard(model);
        }
    }
}