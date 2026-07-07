using HarmonyLib;
using ClanTerritory.Core;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.WardDetection.Services;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch(typeof(Player))]
    internal static class PieceDestroyHooks
    {
        private const string WardPieceName = "guard_stone";

        [HarmonyPrefix]
        [HarmonyPatch("RemovePiece")]
        private static void RemovePiecePrefix(Player __instance, ref WardId? __state)
        {
            __state = null;

            if (__instance == null)
                return;

            Piece piece = AccessTools.Field(typeof(Player), "m_hoveringPiece")
                .GetValue(__instance) as Piece;

            if (piece == null)
                return;

            if (!piece.name.Contains(WardPieceName))
                return;

            PrivateArea privateArea = piece.GetComponent<PrivateArea>();

            if (privateArea == null)
                return;

            ZNetView zNetView = piece.GetComponent<ZNetView>();

            if (zNetView == null || zNetView.GetZDO() == null)
                return;

            __state = new WardId(zNetView.GetZDO().m_uid.ToString());
        }

        [HarmonyPostfix]
        [HarmonyPatch("RemovePiece")]
        private static void RemovePiecePostfix(bool __result, WardId? __state)
        {
            if (!__result)
                return;

            if (!__state.HasValue)
                return;

            IWardService wardService;

            if (ServiceContainer.TryGet<IWardService>(out wardService))
                wardService.UnregisterWard(__state.Value);
        }
    }
}