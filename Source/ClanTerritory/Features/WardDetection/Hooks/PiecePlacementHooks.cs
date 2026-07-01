using HarmonyLib;
using UnityEngine;
using ClanTerritory.Core;
using ClanTerritory.Features.WardDetection.Services;

namespace ClanTerritory.Features.WardDetection.Hooks
{
    [HarmonyPatch(typeof(Player))]
    internal static class PiecePlacementHooks
    {
        private const string WardPieceName = "guard_stone";

        [HarmonyPostfix]
        [HarmonyPatch("PlacePiece")]
        private static void PlacePiecePostfix(Player __instance, Piece piece, Vector3 pos, Quaternion rot, bool doAttack)
        {
            if (__instance == null || piece == null)
                return;

            if (__instance != Player.m_localPlayer)
                return;

            if (piece.name != WardPieceName)
                return;

            IWardService wardService;

            if (ServiceContainer.TryGet<IWardService>(out wardService))
                wardService.RegisterWardAfterPlacement(__instance, pos);
        }
    }
}