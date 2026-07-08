using System.Collections.Generic;
using ClanTerritory.Core;
using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Territory.Services;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.WardMenu.Actions
{
    internal sealed class WardMenuWardActions : IWardMenuWardActions
    {
        private const string ToggleEnabledRpc = "ToggleEnabled";

        public bool ToggleProtection(
            WardId wardId,
            PrivateArea privateArea,
            Player player)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. PrivateArea is null: " + wardId);
                return false;
            }

            if (player == null)
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. Player is null: " + wardId);
                return false;
            }

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece == null)
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. Piece is null: " + wardId);
                return false;
            }

            if (!piece.IsCreator())
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. Player is not ward creator: " + wardId);
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[WardMenuActions] ToggleProtection ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            long playerId = player.GetPlayerID();

            zNetView.InvokeRPC(ToggleEnabledRpc, playerId);

            ModLog.Info("[WardMenuActions] ToggleProtection invoked through Valheim RPC: " + wardId);
            return true;
        }

        public void SetRadius(
            WardId wardId,
            PrivateArea privateArea,
            float radius)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[WardMenuActions] SetRadius ignored. PrivateArea is null: " + wardId);
                return;
            }

            TerritoryWardRadiusService radiusService;

            if (!ServiceContainer.TryGet<TerritoryWardRadiusService>(out radiusService))
            {
                ModLog.Debug("[WardMenuActions] SetRadius ignored. TerritoryWardRadiusService is missing: " + wardId);
                return;
            }

            radiusService.RequestSetRadius(
                  privateArea,
                  Player.m_localPlayer,
                  radius);

            ModLog.Info("[WardMenuActions] SetRadius applied: " + wardId + ", radius: " + radius);
        }

        public bool RemovePermittedPlayer(
            WardId wardId,
            PrivateArea privateArea,
            Player player,
            long playerId)
        {
            if (privateArea == null)
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. PrivateArea is null: " + wardId);
                return false;
            }

            if (player == null)
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. Player is null: " + wardId);
                return false;
            }

            if (playerId == 0L)
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. PlayerId is empty: " + wardId);
                return false;
            }

            Piece piece = privateArea.GetComponent<Piece>();

            if (piece == null)
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. Piece is null: " + wardId);
                return false;
            }

            if (!piece.IsCreator())
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. Player is not ward creator: " + wardId);
                return false;
            }

            ZNetView zNetView = privateArea.GetComponent<ZNetView>();

            if (zNetView == null || !zNetView.IsValid())
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. ZNetView is invalid: " + wardId);
                return false;
            }

            if (!zNetView.IsOwner())
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. ZNetView is not owned locally: " + wardId);
                return false;
            }

            ZDO zdo = zNetView.GetZDO();

            if (zdo == null)
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. ZDO is null: " + wardId);
                return false;
            }

            List<KeyValuePair<long, string>> permittedPlayers = GetPermittedPlayers(zdo);

            int removedCount = permittedPlayers.RemoveAll(
                delegate (KeyValuePair<long, string> permittedPlayer)
                {
                    return permittedPlayer.Key == playerId;
                });

            if (removedCount <= 0)
            {
                ModLog.Debug("[WardMenuActions] RemovePermittedPlayer ignored. Player is not permitted: " + wardId + ", playerId: " + playerId);
                return false;
            }

            SetPermittedPlayers(
                zdo,
                permittedPlayers);

            ModLog.Info("[WardMenuActions] RemovePermittedPlayer applied: " + wardId + ", playerId: " + playerId);
            return true;
        }

        private static List<KeyValuePair<long, string>> GetPermittedPlayers(ZDO zdo)
        {
            List<KeyValuePair<long, string>> players =
                new List<KeyValuePair<long, string>>();

            if (zdo == null)
                return players;

            int permittedCount = zdo.GetInt(ZDOVars.s_permitted);

            for (int i = 0; i < permittedCount; i++)
            {
                long playerId = zdo.GetLong("pu_id" + i, 0L);
                string playerName = zdo.GetString("pu_name" + i, "Unknown");

                if (playerId == 0L)
                    continue;

                players.Add(
                    new KeyValuePair<long, string>(
                        playerId,
                        playerName));
            }

            return players;
        }

        private static void SetPermittedPlayers(
            ZDO zdo,
            List<KeyValuePair<long, string>> players)
        {
            if (zdo == null)
                return;

            if (players == null)
                players = new List<KeyValuePair<long, string>>();

            zdo.Set(
                ZDOVars.s_permitted,
                players.Count);

            for (int i = 0; i < players.Count; i++)
            {
                KeyValuePair<long, string> player = players[i];

                zdo.Set(
                    "pu_id" + i,
                    player.Key);

                zdo.Set(
                    "pu_name" + i,
                    player.Value);
            }
        }
    }
}
