using HarmonyLib;
using UnityEngine;
using ClanTerritory.Core;
using ClanTerritory.Features.Territory.Placement;

namespace ClanTerritory.Integration.Valheim.Harmony
{
    [HarmonyPatch(typeof(Player))]
    internal static class PiecePlacementHooks
    {
        private const string WardPieceName = "guard_stone";

        [HarmonyPrefix]
        [HarmonyPatch("PlacePiece")]
        private static bool PlacePiecePrefix(
            Player __instance,
            Piece piece,
            Vector3 pos,
            Quaternion rot,
            bool doAttack)
        {
            if (__instance == null || piece == null)
                return true;

            if (__instance != Player.m_localPlayer)
                return true;

            if (piece.name != WardPieceName)
                return true;

            IWardPlacementPolicy policy;

            if (!ServiceContainer.TryGet<IWardPlacementPolicy>(out policy))
                return true;

            PlacementValidationResult result =
                policy.Validate(__instance, pos);

            if (result.IsSuccess)
                return true;

            __instance.Message(
                MessageHud.MessageType.Center,
                result.Message,
                0,
                null);

            return false;
        }
    }
}